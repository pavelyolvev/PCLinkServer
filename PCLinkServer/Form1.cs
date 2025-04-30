using System.Drawing.Imaging;
using System.Net;
using System.Net.Sockets;
using System.Text;
using PCLink;

namespace PCLinkServer;

public partial class Form1 : Form
{
    private bool isRunning = true;
    private MouseControl mc = new MouseControl();
// --- Настройки ---
    private static string targetIpAddress = "192.168.31.183"; // !!! ЗАМЕНИТЕ НА IP ВАШЕГО ТЕЛЕФОНА !!!
    private static int targetPort = 9050;
    private static int frameRate = 30; // Кадров в секунду
    private static long jpegQuality = 20L; // Качество JPEG (0-100)

    // --- Новые настройки для уменьшения размера ---
    // Установите желаемое разрешение. 0 - использовать разрешение монитора.
    private static int captureWidth = 1280; // Например, 1280 пикселей в ширину
    private static int captureHeight = 720; // Например, 720 пикселей в высоту
    // Захватывать только основной монитор? true - да, false - захватывать все экраны как один большой (может не работать корректно)
    private static bool usePrimaryMonitorOnly = true;
    // --------------------------------------------

    private static UdpClient udpClient;
    private static IPEndPoint targetEndPoint;
    private static System.Threading.Timer captureTimer;
    private static bool isStreaming = false;
    private static object lockObject = new object(); // Для синхронизации доступа к isStreaming
    private const int COMMAND_PORT = 12312;
    private const int APP_PORT = 12311;
    public Form1()
    {
        InitializeComponent();
        Thread appRunningThread = new Thread(StartUdpCommandServer);
        appRunningThread.IsBackground = true;
        appRunningThread.Start();
    }

    // void StartAppServer()
    // {
    //     udpServer = new UdpClient(APP_PORT);
    //     IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
    //     while (true)
    //     {
    //         
    //     }
    // }
    private void button1_Click(object sender, EventArgs e)
    {
        // Thread commandThread = new Thread(StartUdpCommandServer);
        // commandThread.IsBackground = true;
        if (!isRunning)
        {
            isRunning = true;
            
            // commandThread.Start();
            Log("UDP сервер команд запущен...");
            
            Console.WriteLine("Starting Desktop Streamer...");
            udpClient = new UdpClient();
            targetEndPoint = new IPEndPoint(IPAddress.Parse(targetIpAddress), targetPort);

            string resolutionInfo = (captureWidth > 0 && captureHeight > 0)
                ? $"Target Resolution: {captureWidth}x{captureHeight}"
                : "Target Resolution: Monitor Native";
            string monitorInfo = usePrimaryMonitorOnly ? "Primary Monitor Only" : "All Monitors (Virtual Screen)";

            Console.WriteLine($"Streaming to {targetIpAddress}:{targetPort} at ~{frameRate} FPS, JPEG Quality: {jpegQuality}");
            Console.WriteLine($"{resolutionInfo}, {monitorInfo}");


            StartStreaming();
        }
        else
        {
            isRunning = false;
            // commandThread.Interrupt();
            StopStreaming();
            Console.WriteLine("Streaming stopped.");
        }
    }

    IPEndPoint lastClientEP = null;
    UdpClient udpServer = null; // Объявляем udpServer как поле класса

    private static object targetEndPointLock = new object();

    private void StartUdpCommandServer()
    {
        udpServer = new UdpClient(COMMAND_PORT);
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

        while (isRunning)
        {
            try
            {
                byte[] data = udpServer.Receive(ref remoteEP);
                string cmd = Encoding.UTF8.GetString(data);

                Log($"Received command: {cmd} from {remoteEP.Address}");

                // --- Обработка команды CLIENT_IP ---
                if (cmd.StartsWith("CLIENT_IP:"))
                {
                    string clientIpString = cmd.Substring("CLIENT_IP:".Length);
                    try
                    {
                        IPAddress clientIpAddress = IPAddress.Parse(clientIpString);
                        // Обновляем целевую точку для стрима
                        lock (targetEndPointLock) // Используем лок для безопасного обновления
                        {
                             targetEndPoint = new IPEndPoint(clientIpAddress, targetPort); // targetPort = 9050
                        }
                        Log($"Updated stream target IP to: {clientIpAddress} on port {targetPort}");

                        // Опционально: Сохраняем этот endpoint как lastClientEP, если он нужен для других команд
                         lastClientEP = remoteEP; // Сохраняем endpoint клиента команд
                    }
                    catch (FormatException)
                    {
                        Log($"Invalid IP format received: {clientIpString}");
                    }
                    continue; // Обработали команду IP, переходим к следующему пакету
                }
                // ------------------------------------


                // Сохраняем IP клиента команд (для MOVE/CLICK), если это не команда IP
                lastClientEP = remoteEP;

                // Обработка других команд (MOVE, CLICK, MODE)
                this.Invoke(() => mc.HandleCmd(cmd));
            }
            catch (SocketException sex)
            {
                 // Логируем ошибку, если сервер еще должен работать
                 if (isRunning) Log($"Socket Error in command server: {sex.Message}");
                 else Log("Command server socket closed."); // Нормальное завершение
                 if (!isRunning) break; // Выходим из цикла при остановке
            }
            catch (Exception ex)
            {
                Log($"Ошибка приема команды: {ex.Message}");
            }
        }
        udpServer?.Close(); // Закрываем сокет при выходе из цикла
        Log("UDP Command server stopped.");
    }


    static void StartStreaming()
    {
        lock (lockObject)
        {
            if (isStreaming) return;
            isStreaming = true;
        }
        int interval = 1000 / frameRate;
        captureTimer = new System.Threading.Timer(CaptureAndSendFrame, null, 0, interval);
    }

    static void StopStreaming()
    {
        lock (lockObject)
        {
            if (!isStreaming) return;
            isStreaming = false;
        }
        captureTimer?.Dispose();
        udpClient?.Close();
    }

    static void CaptureAndSendFrame(object state)
    {
        lock (lockObject)
        {
            if (!isStreaming) return;
        }

        IPEndPoint currentTarget = null;
        lock(targetEndPointLock) // Безопасно читаем целевую точку
        {
            currentTarget = targetEndPoint;
        }

        // Отправляем кадр только если целевая точка для стрима установлена (получена команда CLIENT_IP)
        if (currentTarget == null)
        {
            // Console.WriteLine("Stream target not set yet. Dropping frame.");
            return; // Не отправляем, если IP клиента еще не известен
        }

        try
        {
            // 1. Определяем область захвата
            Rectangle screenBounds;
            if (usePrimaryMonitorOnly)
            {
                // Используем только основной монитор
                screenBounds = Screen.PrimaryScreen.Bounds;
            }
            else
            {
                // Используем весь виртуальный экран (объединение всех мониторов)
                // Внимание: Это может не всегда корректно работать или давать очень большое изображение
                screenBounds = SystemInformation.VirtualScreen;
            }

            // 2. Захват экрана (в исходном разрешении выбранной области)
            using (Bitmap originalBitmap = new Bitmap(screenBounds.Width, screenBounds.Height, PixelFormat.Format32bppArgb))
            {
                using (Graphics graphics = Graphics.FromImage(originalBitmap))
                {
                    graphics.CopyFromScreen(screenBounds.X, screenBounds.Y, 0, 0, screenBounds.Size, CopyPixelOperation.SourceCopy);
                }

                // 3. Масштабирование (если заданы captureWidth/Height)
                Bitmap bitmapToSend = null;
                bool createdNewBitmap = false; // Флаг, чтобы знать, нужно ли удалять bitmapToSend

                if (captureWidth > 0 && captureHeight > 0 && (originalBitmap.Width != captureWidth || originalBitmap.Height != captureHeight))
                {
                    // Создаем новый Bitmap с нужным размером
                    bitmapToSend = new Bitmap(captureWidth, captureHeight);
                    using (Graphics scaledGraphics = Graphics.FromImage(bitmapToSend))
                    {
                        // Настраиваем качество интерполяции (опционально)
                        scaledGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic; // Или Low для скорости
                        scaledGraphics.DrawImage(originalBitmap, 0, 0, captureWidth, captureHeight);
                    }
                    createdNewBitmap = true;
                     //Console.WriteLine($"Scaled from {originalBitmap.Width}x{originalBitmap.Height} to {bitmapToSend.Width}x{bitmapToSend.Height}"); // Отладка
                }
                else
                {
                    // Используем оригинальный Bitmap без масштабирования
                    bitmapToSend = originalBitmap;
                     // Console.WriteLine($"Using original resolution: {bitmapToSend.Width}x{bitmapToSend.Height}"); // Отладка
                }

                try // Обернем работу с bitmapToSend в try-finally для гарантированного Dispose
                {
                    // 4. Кодирование в JPEG (масштабированного или оригинального)
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                        EncoderParameters encoderParams = new EncoderParameters(1);
                        encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, jpegQuality);

                        bitmapToSend.Save(ms, jpgEncoder, encoderParams);
                        byte[] jpegBytes = ms.ToArray();

                        // 5. Отправка по UDP
                        if (jpegBytes.Length > 0 && jpegBytes.Length < 65507)
                        {
                            udpClient.Send(jpegBytes, jpegBytes.Length, targetEndPoint);
                            // Console.WriteLine($"Sent frame: {jpegBytes.Length} bytes");
                        }
                        else if (jpegBytes.Length >= 65507)
                        {
                            Console.WriteLine($"Warning: Frame size ({jpegBytes.Length} bytes at {bitmapToSend.Width}x{bitmapToSend.Height} Q:{jpegQuality}) too large for single UDP packet. Frame dropped.");
                            // Попробуйте еще уменьшить captureWidth/Height или jpegQuality
                        }
                    }
                }
                finally
                {
                    // Если мы создавали новый масштабированный Bitmap, его нужно удалить
                    if (createdNewBitmap && bitmapToSend != null)
                    {
                        bitmapToSend.Dispose();
                    }
                    // originalBitmap удаляется внешним using
                }
            }
        }
        catch (SocketException se)
        {
            Console.WriteLine($"Socket Error: {se.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error capturing or sending frame: {ex.Message}");
             // StopStreaming(); // Раскомментируйте, если хотите останавливать при ошибках
        }
    }

    private static ImageCodecInfo GetEncoder(ImageFormat format)
    {
        ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
        foreach (ImageCodecInfo codec in codecs)
        {
            if (codec.FormatID == format.Guid)
            {
                return codec;
            }
        }
        return null;
    }

    private void Log(string message)
    {
        Invoke(() =>
        {
            LogList.Items.Add($"[{DateTime.Now:T}] {message}");
        });
    }
}