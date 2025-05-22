using System.Drawing.Imaging;
using System.Net;
using System.Net.Sockets;

namespace PCLinkServer;

public class VideoStream
{
        // --- Настройки ---
    private static int frameRate = 30; // Кадров в секунду
    private static long jpegQuality = 20L; // Качество JPEG (0-100)

    // --- Новые настройки для уменьшения размера ---
    // Установите желаемое разрешение. 0 - использовать разрешение монитора.
    private static int captureWidth = 1280; // Например, 1280 пикселей в ширину
    private static int captureHeight = 720; // Например, 720 пикселей в высоту
    // Захватывать только основной монитор? true - да, false - захватывать все экраны как один большой (может не работать корректно)
    private static bool usePrimaryMonitorOnly = true;
    // --------------------------------------------

    private static object targetEndPointLock = new object();
    private static System.Threading.Timer captureTimer;
    private static bool isStreaming = false;
    private static object lockObject = new object(); // Для синхронизации доступа к isStreaming
    
    private static UdpClient udpClient;
    private static IPEndPoint targetEndPoint;
    public static void StartStreaming(String targetIpAddress, int targetPort)
    {
        lock (lockObject)
        {
            if (isStreaming) return;
            isStreaming = true;
            udpClient = new UdpClient();
            targetEndPoint = new IPEndPoint(IPAddress.Parse(targetIpAddress), targetPort);
        }
        int interval = 1000 / frameRate;
        new Thread(() =>
        {
            captureTimer = new System.Threading.Timer(CaptureAndSendFrame, null, 0, interval);
        }).Start();
    }

    public static void StopStreaming()
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

            long millisToCatchScreenStart = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            // 2. Захват экрана (в исходном разрешении выбранной области)
            using (Bitmap originalBitmap = new Bitmap(screenBounds.Width, screenBounds.Height, PixelFormat.Format24bppRgb))
            {
                using (Graphics graphics = Graphics.FromImage(originalBitmap))
                {
                    graphics.CopyFromScreen(screenBounds.X, screenBounds.Y, 0, 0, screenBounds.Size, CopyPixelOperation.SourceCopy);
                }
                long millisToScaleScreenStart = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
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
                    long millisToCodeJPGStart = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    // 4. Кодирование в JPEG (масштабированного или оригинального)
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                        EncoderParameters encoderParams = new EncoderParameters(1);
                        encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, jpegQuality);

                        bitmapToSend.Save(ms, jpgEncoder, encoderParams);
                        byte[] jpegBytes = ms.ToArray();
                        long millisToSendStart = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
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
                        long millisEnd = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                        Console.WriteLine($"screenRecording: {millisToScaleScreenStart-millisToCatchScreenStart} ms, Scaling:  {millisToCodeJPGStart - millisToScaleScreenStart}, Coding: {millisToSendStart-millisToCodeJPGStart}, Sending: {millisEnd-millisToSendStart}");
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
}