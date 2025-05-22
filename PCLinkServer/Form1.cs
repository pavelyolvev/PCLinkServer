using System.Drawing.Imaging;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using PCLink;

namespace PCLinkServer;

public partial class Form1 : Form
{
    private bool isRunning = true;
    private bool serverIsRunning = false;
    private InputControl mc = new InputControl();

    private const int COMMAND_PORT = 12312;

    private Preference preference;
    private CancellationTokenSource _cancellationTokenSource;
    private Task serverTask;
    private ScreenCapture screenCapture;

    public Form1()
    {
        InitializeComponent();
        preference = Preferences.GetPreference();
        StatusSet("Выключен");
        loadPreferences();

        screenCapture = new ScreenCapture();
        // Thread appRunningThread = new Thread(StartUdpCommandServer);
        // appRunningThread.IsBackground = true;
        // appRunningThread.Start();
    }

    private void OnOpen(object sender, EventArgs e)
    {
        this.Show();
        this.ShowInTaskbar = true;
        this.WindowState = FormWindowState.Normal;
    }

    private void OnExit(object sender, EventArgs e)
    {
        notifyIcon1.Visible = false;
        Application.Exit();
    }

    private void OnTrayIconDoubleClick(object sender, EventArgs e)
    {
        this.Show();
        this.ShowInTaskbar = true;
        this.WindowState = FormWindowState.Normal;
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (this.WindowState == FormWindowState.Minimized)
        {
            this.Hide(); // Скрываем окно, чтобы оно не появлялось на панели задач
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // notifyIcon1.Visible = false; // Убираем иконку из трея при завершении
        // base.OnFormClosing(e);
        this.WindowState = FormWindowState.Minimized;
        this.Hide();
    }
    private void button1_Click(object sender, EventArgs e)
    {
        // Thread commandThread = new Thread(StartUdpCommandServer);
        // commandThread.IsBackground = true;
        if (!isRunning)
        {
            isRunning = true;
            
            // commandThread.Start();
            // Log("UDP сервер команд запущен...");
            
            Console.WriteLine("Starting Desktop Streamer...");


            String ip = "192.168.31.183";
            int port = 12313;
            screenCapture.StartStreaming(ip, port);
        }
        else
        {
            isRunning = false;
            // commandThread.Interrupt();
            screenCapture.StopStreaming();
            Console.WriteLine("Streaming stopped.");
        }
    }

    IPEndPoint lastClientEP = null;
    UdpClient udpServer = null; // Объявляем udpServer как поле класса


    public class UdpMessage
    {
        public int id { get; set; }
        public string message { get; set; }
        public string[] parameters { get; set; }
    }
    
    private UdpClient _udpClient;

    public async Task StartAsync(int port, CancellationToken token)
    {
        StatusSet("Ожидает подключения");
        _udpClient = new UdpClient(port);
        Console.WriteLine($"UDP-сервер запущен на порту {port}");
        NotifyThroughIcon("PCLink", "Сервер запущен", 3000);
        try
        {
            while (!token.IsCancellationRequested)
            {
                var resultTask = _udpClient.ReceiveAsync();
                var completedTask = await Task.WhenAny(resultTask, Task.Delay(-1, token));

                if (completedTask == resultTask)
                {
                    var result = resultTask.Result;
                    var jsonString = Encoding.UTF8.GetString(result.Buffer);

                    try
                    {
                        Console.WriteLine($"Получено string: msg={jsonString}");
                        var message = JsonSerializer.Deserialize<UdpMessage>(jsonString);
                        Console.WriteLine($"Получено: id={message.id}, msg={message.message}");

                        string respMessage = ResponseSelector(message, result.RemoteEndPoint);
                        string[] parStrings = [];

                        if (respMessage.Equals("AUTH_SUCCEED"))
                        {
                            string macAdress = GetMacAddress();
                            parStrings = new[] { macAdress };
                        }

                        mc.HandleCmd(message.message, message.parameters);

                        var response = new UdpMessage
                        {
                            id = message.id,
                            message = respMessage,
                            parameters = parStrings
                        };

                        var responseJson = JsonSerializer.Serialize(response);
                        var responseBytes = Encoding.UTF8.GetBytes(responseJson);

                        await _udpClient.SendAsync(responseBytes, responseBytes.Length, result.RemoteEndPoint);
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine("Ошибка при разборе JSON: " + ex.Message);
                    }
                }
                else
                {
                    // Отмена сработала
                    NotifyThroughIcon("PCLink", "Сервер выключен", 3000);
                    StatusSet("Выключен");
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Сервер остановлен.");
        }
        finally
        {
            _udpClient?.Close();
        }
    }

    Authentification _auth = new Authentification();
    string ResponseSelector(UdpMessage message, IPEndPoint sendPoint)
    {
        switch (message.message)
        {
            case "REQUEST_ACCESS":
                string response = "ACCESS_DENIED";
                AccessRecord? record = _auth.GetRecordByIp(sendPoint.Address.ToString());
                if (record.HasValue && record.Value.Ip.Equals(sendPoint.Address.ToString()))
                {
                    if (record.Value.AuthCode.ToString().Equals(message.parameters[0]))
                    {
                        StatusSet("Подключено");
                        response = "ACCESS_GRANTED";   
                    }
                }
                return response;
            case "AUTH_START":
                int code = _auth.GenerateCode();
                new CodeForm(code.ToString()).Show();
                // MessageBox.Show($@"Новый клиент. Код: {code}");

                return "CODE_GENERATED";
            case "END":
                StatusSet("Ожидает подключения");
                return "END";
            case "AUTH":
                if (_auth.ReadCode(message.parameters[0]))
                {
                    _auth.SaveNewRecord(sendPoint.Address.ToString(), message.parameters[1], _auth.Code);
                    // MessageBox.Show(@"Новый клиент сохранен. Авторизация успешна");
                    NotifyThroughIcon("Авторизация успешна", "клиент сохранен", 5000);
                    return "AUTH_SUCCEED";
                }
                return "AUTH_FAILED";
            case "PC_SLEEP":
                if (preference.isAllowedSleep)
                {
                    ExtraFucntions.SleepWindows();
                    return "PC_SLEPT";    
                }
                else
                {
                    return "NOT_ALLOWED";
                }
            case "PC_SHUTDOWN":
                if (preference.isAllowedShutdown)
                {
                    ExtraFucntions.ShutdownWindows();
                    return "PC_SHUTDOWN";    
                }
                else
                {
                    return "NOT_ALLOWED";
                }
            case "PC_RESTART":
                if (preference.isAllowedRestart)
                {
                    ExtraFucntions.RestartWindows();
                    return "PC_RESTARTED";    
                }
                else
                {
                    return "NOT_ALLOWED";
                }
        }

        return "UNKNOWN_REQUEST";
    }
    string GetMacAddress()
    {
        foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (nic.OperationalStatus == OperationalStatus.Up &&
                nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            {
                var macBytes = nic.GetPhysicalAddress().GetAddressBytes();
                return string.Join(":", macBytes.Select(b => b.ToString("X2")));
            }
        }
        return "MAC_NOT_FOUND";
    }
    // private void StartUdpCommandServer()
    // {
    //     udpServer = new UdpClient(COMMAND_PORT);
    //     IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
    //
    //     while (isRunning)
    //     {
    //         try
    //         {
    //             byte[] data = udpServer.Receive(ref remoteEP);
    //             string cmd = Encoding.UTF8.GetString(data);
    //
    //             Log($"Received command: {cmd} from {remoteEP.Address}");
    //             if (!trustedIps.Contains(remoteEP.Address.ToString()))
    //             {
    //                 if (cmd.StartsWith("AUTH:"))
    //                 {
    //                     if (int.TryParse(cmd.Substring("AUTH:".Length), out int receivedCode) &&
    //                         pendingAuthCodes.TryGetValue(remoteEP.Address.ToString(), out int correctCode) &&
    //                         receivedCode == correctCode)
    //                     {
    //                         trustedIps.Add(remoteEP.Address.ToString());
    //                         pendingAuthCodes.Remove(remoteEP.Address.ToString());
    //                         Log($"Клиент {remoteEP.Address} успешно аутентифицирован.");
    //                     }
    //                     else
    //                     {
    //                         Log($"Неверный код от {remoteEP.Address}");
    //                     }
    //                 }
    //                 else
    //                 {
    //                     // Сгенерировать и отправить код подтверждения
    //                     if (!pendingAuthCodes.ContainsKey(remoteEP.Address.ToString()))
    //                     {
    //                         int code = new Random().Next(1000, 10000);
    //                         // int code = 1234;
    //                         pendingAuthCodes[remoteEP.Address.ToString()] = code;
    //                         Log($"Новый клиент: {remoteEP.Address}\nКод: {code}");
    //                     }
    //                     return; // Необработанный клиент — отклоняем
    //                 }
    //             }
    //
    //             // --- Обработка команды CLIENT_IP ---
    //             if (cmd.StartsWith("CLIENT_IP:"))
    //             {
    //                 string clientIpString = cmd.Substring("CLIENT_IP:".Length);
    //                 try
    //                 {
    //                     IPAddress clientIpAddress = IPAddress.Parse(clientIpString);
    //                     // Обновляем целевую точку для стрима
    //                     lock (targetEndPointLock) // Используем лок для безопасного обновления
    //                     {
    //                          targetEndPoint = new IPEndPoint(clientIpAddress, targetPort); // targetPort = 9050
    //                     }
    //                     Log($"Updated stream target IP to: {clientIpAddress} on port {targetPort}");
    //
    //                     // Опционально: Сохраняем этот endpoint как lastClientEP, если он нужен для других команд
    //                      lastClientEP = remoteEP; // Сохраняем endpoint клиента команд
    //                 }
    //                 catch (FormatException)
    //                 {
    //                     Log($"Invalid IP format received: {clientIpString}");
    //                 }
    //                 continue; // Обработали команду IP, переходим к следующему пакету
    //             }
    //             // ------------------------------------
    //
    //
    //             // Сохраняем IP клиента команд (для MOVE/CLICK), если это не команда IP
    //             lastClientEP = remoteEP;
    //
    //             // Обработка других команд (MOVE, CLICK, MODE)
    //             // this.Invoke(() => mc.HandleCmd(cmd));
    //         }
    //         catch (SocketException sex)
    //         {
    //              // Логируем ошибку, если сервер еще должен работать
    //              if (isRunning) Log($"Socket Error in command server: {sex.Message}");
    //              else Log("Command server socket closed."); // Нормальное завершение
    //              if (!isRunning) break; // Выходим из цикла при остановке
    //         }
    //         catch (Exception ex)
    //         {
    //             Log($"Ошибка приема команды: {ex.Message}");
    //         }
    //     }
    //     udpServer?.Close(); // Закрываем сокет при выходе из цикла
    //     Log("UDP Command server stopped.");
    // }
    
    private void Log(string message)
    {
        // Invoke(() =>
        // {
        //     LogList.Items.Add($"[{DateTime.Now:T}] {message}");
        // });
    }


    private void loadPreferences()
    {
        checkBox1.Checked = preference.isAllowedShutdown;
        checkBox2.Checked = preference.isAllowedRestart;
        checkBox3.Checked = preference.isAllowedSleep;
        checkBox4.Checked = preference.startOnSystem;
        checkBox5.Checked = preference.startMinimized;
        checkBox6.Checked = preference.isAllowedVideo;
        checkBox7.Checked = preference.autoLaunch;

        if (preference.startMinimized)
        {
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Hide();
        }

        if (preference.autoLaunch)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            serverTask = StartAsync(COMMAND_PORT, _cancellationTokenSource.Token);
            onOffToolStripMenuItem.Text = "Выключить";
        }
    }

    private void StatusSet(String message)
    {
        statusLbl.Text = message;
        statusToolStripMenuItem.Text = message;
        switch (message)
        {
            case "Выключен":
                statusLbl.ForeColor = Color.Brown;
                statusToolStripMenuItem.ForeColor = Color.Brown;
                break;
            case "Ожидает подключения":
                statusLbl.ForeColor = Color.DarkOrange;
                statusToolStripMenuItem.ForeColor = Color.DarkOrange;
                break;
            case "Подключено":
                statusLbl.ForeColor = Color.ForestGreen;
                statusToolStripMenuItem.ForeColor = Color.ForestGreen;
                break;
        }
    }

    private void NotifyThroughIcon(String title, String message, int delay = 3000)
    {
        
        notifyIcon1.BalloonTipTitle = title;
        notifyIcon1.BalloonTipText = message;
        notifyIcon1.ShowBalloonTip(delay);
    }

    private void checkBox1_CheckedChanged(object sender, EventArgs e)
    {
        Preferences.UpdateShutdown(checkBox1.Checked);
        preference = Preferences.GetPreference();
    }

    private void checkBox2_CheckedChanged(object sender, EventArgs e)
    {
        Preferences.UpdateRestart(checkBox2.Checked);
        preference = Preferences.GetPreference();
    }

    private void checkBox3_CheckedChanged(object sender, EventArgs e)
    {
        Preferences.UpdateSleep(checkBox3.Checked);
        preference = Preferences.GetPreference();
    }

    private void checkBox4_CheckedChanged(object sender, EventArgs e)
    {
        Preferences.UpdateStartOnSystem(checkBox4.Checked);
        preference = Preferences.GetPreference();
    }
    private void checkBox5_CheckedChanged(object sender, EventArgs e)
    {
        Preferences.UpdateStartMinimized(checkBox5.Checked);
        preference = Preferences.GetPreference();
    }

    private async void onOffToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (serverTask != null && !serverTask.IsCompleted)
        {
            _cancellationTokenSource?.Cancel();
            await serverTask;
            _udpClient?.Close();

            serverTask = null;
            _cancellationTokenSource = null;

            onOffToolStripMenuItem.Text = "Включить";
        }
        else
        {
            _cancellationTokenSource = new CancellationTokenSource();
            serverTask = StartAsync(COMMAND_PORT, _cancellationTokenSource.Token);
            onOffToolStripMenuItem.Text = "Выключить";
        }
    }


    private void checkBox6_CheckedChanged(object sender, EventArgs e)
    {
        Preferences.UpdateVideo(checkBox6.Checked);
        preference = Preferences.GetPreference();
    }

    private void checkBox7_CheckedChanged(object sender, EventArgs e)
    {
        Preferences.UpdateAutoLaunch(checkBox7.Checked);
        preference = Preferences.GetPreference();
    }

    private void button2_Click(object sender, EventArgs e)
    {
        string sharedPath = @"E:\FTP";
        Task ftpServer = FtpServer.RunFtpServerAsync(sharedPath, 2121);
        NotifyThroughIcon("FTP сервер", "Успешно запущен", 4000);
    }
}