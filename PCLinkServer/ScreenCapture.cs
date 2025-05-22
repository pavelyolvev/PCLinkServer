using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;

namespace PCLinkServer;

public class ScreenCapture
{
    private ID3D11Device _device;
    private ID3D11DeviceContext _context;
    private IDXGIOutputDuplication _duplication;
    private int _width;
    private int _height;
    
    private UdpClient udpClient;
    private IPEndPoint targetEndPoint;
    private System.Threading.Timer captureTimer;
    private bool isStreaming = false;
    private object lockObject = new object(); // Для синхронизации доступа к isStreaming
    
    //Параметры
    private int frameRate = 60; 
    private long jpegQuality = 60L; // Качество JPEG (0-100)
    public ScreenCapture()
    {
        Initialize();
    }

    private void Initialize()
    {
        // Создаем устройство D3D11
        D3D11.D3D11CreateDevice(null, DriverType.Hardware, DeviceCreationFlags.BgraSupport, null,
            out _device, out _context);

        using var dxgiDevice = _device.QueryInterface<IDXGIDevice>();
        using var adapter = dxgiDevice.GetAdapter();

        if (adapter.EnumOutputs(0, out IDXGIOutput output).Failure || output == null)
            throw new InvalidOperationException("DXGI: Не удалось получить выходной источник (output)");

        using var output1 = output.QueryInterface<IDXGIOutput1>();
        if (output1 == null)
            throw new InvalidOperationException("DXGI: Не удалось получить IDXGIOutput1");

        _duplication = output1.DuplicateOutput(_device);
        if (_duplication == null)
            throw new InvalidOperationException("Не удалось инициализировать дублирование экрана.");

        var desc = output.Description;
        _width = desc.DesktopCoordinates.Right - desc.DesktopCoordinates.Left;
        _height = desc.DesktopCoordinates.Bottom - desc.DesktopCoordinates.Top;

        Thread.Sleep(100); // Дать системе время на установку дублирования
    }

    public void StartStreaming(String targetIpAddress, int targetPort)
    {
        lock (lockObject)
        {
            if (isStreaming) return;
            isStreaming = true;
            udpClient = new UdpClient();
            targetEndPoint = new IPEndPoint(IPAddress.Parse(targetIpAddress), targetPort);
        }
        int interval = 1000 / frameRate;
        // // Инициализация
        // var encoder = new H264Streamer(width: 1920, height: 1080, fps: 30, bitrate: 2000000);
        // encoder.Initialize();
        //
        // // Кодирование кадров
        // while (true)
        // {
        //     Bitmap frame = CaptureFrame();
        //     byte[] encodedData = encoder.EncodeFrame(frame);
        //
        //     // Отправка данных клиенту
        //     udpClient.Send(encodedData); // Ваш метод отправки
        //
        //     frame.Dispose();
        //
        //     // Контроль частоты кадров
        //     Thread.Sleep(interval);
        // }

        // Освобождение ресурсов
        // encoder.Dispose();
        new Thread(() =>
        {
            captureTimer = new System.Threading.Timer(CaptureAndSendFrame, null, 0, interval);
        }).Start();
    }

    public void StopStreaming()
    {
        lock (lockObject)
        {
            if (!isStreaming) return;
            isStreaming = false;
        }
        captureTimer?.Dispose();
        udpClient?.Close();
    }
    public Bitmap CaptureFrame()
{
    if (_duplication == null)
        throw new InvalidOperationException("Duplication is not initialized");

    try
    {
        // Пытаемся получить следующий кадр с таймаутом
        var result = _duplication.AcquireNextFrame(5000, out var frameInfo, out var desktopResource);
        
        if (result.Failure || desktopResource == null)
        {
            // Если не удалось получить кадр, освобождаем ресурсы и пытаемся восстановить соединение
            _duplication.ReleaseFrame();
            ReinitializeDuplication();
            return null;
        }

        using (desktopResource)
        {
            using var texture = desktopResource.QueryInterface<ID3D11Texture2D>();
            var textureDesc = texture.Description;
            
            // Создание текстуры для копирования
            textureDesc.Usage = ResourceUsage.Staging;
            textureDesc.BindFlags = BindFlags.None;
            textureDesc.CPUAccessFlags = CpuAccessFlags.Read;
            textureDesc.MiscFlags = ResourceOptionFlags.None;
            textureDesc.MipLevels = 1;
            textureDesc.ArraySize = 1;
            textureDesc.SampleDescription = new SampleDescription(1, 0);

            using var stagingTexture = _device.CreateTexture2D(textureDesc);
            _context.CopyResource(stagingTexture, texture);

            // Получение данных из текстуры
            var dataBox = _context.Map(stagingTexture, 0, MapMode.Read, Vortice.Direct3D11.MapFlags.None);

            try
            {
                // Создание Bitmap из данных
                var bitmap = new Bitmap(_width, _height, PixelFormat.Format32bppArgb);
                var bitmapData = bitmap.LockBits(new Rectangle(0, 0, _width, _height), 
                    ImageLockMode.WriteOnly, bitmap.PixelFormat);

                if (dataBox.DataPointer == IntPtr.Zero || bitmapData.Scan0 == IntPtr.Zero)
                    throw new InvalidOperationException("Invalid pointer for memory copy.");

                long rowBytes = Math.Min(bitmapData.Stride, dataBox.RowPitch);

                unsafe
                {
                    for (int y = 0; y < _height; y++)
                    {
                        void* src = (byte*)dataBox.DataPointer + y * dataBox.RowPitch;
                        void* dst = (byte*)bitmapData.Scan0 + y * bitmapData.Stride;
                        Buffer.MemoryCopy(src, dst, rowBytes, rowBytes);
                    }
                }

                bitmap.UnlockBits(bitmapData);
                return bitmap;
            }
            finally
            {
                _context.Unmap(stagingTexture, 0);
            }
        }
    }
    finally
    {
        _duplication.ReleaseFrame();
    }
}

private void ReinitializeDuplication()
{
    // Освобождаем старые ресурсы
    _duplication?.Dispose();
    _duplication = null;

    // Повторная инициализация
    using var dxgiDevice = _device.QueryInterface<IDXGIDevice>();
    using var adapter = dxgiDevice.GetAdapter();
    
    if (adapter.EnumOutputs(0, out IDXGIOutput output).Failure || output == null)
        throw new InvalidOperationException("Failed to get output");

    using var output1 = output.QueryInterface<IDXGIOutput1>();
    _duplication = output1.DuplicateOutput(_device);
    
    if (_duplication == null)
        throw new InvalidOperationException("Failed to initialize duplication");
}

    public void CaptureAndSendFrame(object state)
    {
        lock (lockObject)
        {
            long millisecondsStart = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            if (!isStreaming) return;
        
            try
            {
                Bitmap bitmapToSend = CaptureFrame();
                if (bitmapToSend == null) return;
            
                using (bitmapToSend)
                using (MemoryStream ms = new MemoryStream())
                {
                    // Сжатие изображения в JPEG
                    ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                    EncoderParameters encoderParams = new EncoderParameters(1);
                    encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, jpegQuality);

                    bitmapToSend.Save(ms, jpgEncoder, encoderParams);
                    byte[] jpegBytes = ms.ToArray();

                    const int fragmentSize = 16384;
                    if (jpegBytes.Length == 0) return;

                    // Генерация идентификатора кадра
                    int frameId = Environment.TickCount; // Можно заменить на другой уникальный источник

                    int totalFragments = (int)Math.Ceiling(jpegBytes.Length / (double)fragmentSize);

                    byte[] packetBuffer = new byte[fragmentSize + 8];

                    for (int i = 0; i < totalFragments; i++)
                    {
                        int offset = i * fragmentSize;
                        int length = Math.Min(fragmentSize, jpegBytes.Length - offset);

                        // Заголовок
                        Buffer.BlockCopy(BitConverter.GetBytes(frameId), 0, packetBuffer, 0, 4);
                        Buffer.BlockCopy(BitConverter.GetBytes((ushort)totalFragments), 0, packetBuffer, 4, 2);
                        Buffer.BlockCopy(BitConverter.GetBytes((ushort)i), 0, packetBuffer, 6, 2);

                        // Данные
                        Buffer.BlockCopy(jpegBytes, offset, packetBuffer, 8, length);

                        udpClient.Send(packetBuffer, length + 8, targetEndPoint);
                    }


                    // Console.WriteLine($"Sent frame {frameId} as {totalFragments} packets");
                }
                Console.WriteLine($"frametime: {(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - millisecondsStart} ms");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error capturing frame: {ex.Message}");
                // Можно добавить задержку перед повторной попыткой
                Thread.Sleep(100);
            }
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