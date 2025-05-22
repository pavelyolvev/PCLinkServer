
namespace PCLinkServer;

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

public class H264Streamer : IDisposable
{
    private readonly int _width;
    private readonly int _height;
    private readonly int _fps;
    private readonly int _bitrate;
    private readonly string _preset;
    
    private IntPtr _avCodecContextPtr;
    private IntPtr _avFramePtr;
    private IntPtr _swsContextPtr;
    private bool _initialized;
    private long _frameNumber = 0;
    
    private readonly object _lock = new object();
    
    public H264Streamer(int width, int height, int fps = 30, int bitrate = 2000000, string preset = "fast")
    {
        _width = width;
        _height = height;
        _fps = fps;
        _bitrate = bitrate;
        _preset = preset;
        
        // Инициализация FFmpeg
        ffmpeg.avformat_network_init();
    }
    
    public void Initialize()
    {
        lock (_lock)
        {
            if (_initialized) return;
            
            // Находим кодировщик H.264
            var codec = ffmpeg.avcodec_find_encoder(AVCodecID.AV_CODEC_ID_H264);
            if (codec == IntPtr.Zero)
                throw new InvalidOperationException("H.264 codec not found");
            
            // Создаем контекст кодека
            _avCodecContextPtr = ffmpeg.avcodec_alloc_context3(codec);
            if (_avCodecContextPtr == IntPtr.Zero)
                throw new InvalidOperationException("Could not allocate codec context");
            
            var codecContext = new AVCodecContext
            {
                bit_rate = _bitrate,
                width = _width,
                height = _height,
                time_base = new AVRational { num = 1, den = _fps },
                framerate = new AVRational { num = _fps, den = 1 },
                gop_size = 10,
                max_b_frames = 1,
                pix_fmt = AVPixelFormat.AV_PIX_FMT_YUV420P
            };
            
            Marshal.StructureToPtr(codecContext, _avCodecContextPtr, false);
            
            // Устанавливаем параметры кодирования
            ffmpeg.av_opt_set(ffmpeg.avcodec_get_priv_data(_avCodecContextPtr), "preset", _preset, 0);
            ffmpeg.av_opt_set(ffmpeg.avcodec_get_priv_data(_avCodecContextPtr), "tune", "zerolatency", 0);
            
            // Открываем кодека
            if (ffmpeg.avcodec_open2(_avCodecContextPtr, codec, IntPtr.Zero) < 0)
                throw new InvalidOperationException("Could not open codec");
            
            // Создаем фрейм для кодирования
            _avFramePtr = ffmpeg.av_frame_alloc();
            if (_avFramePtr == IntPtr.Zero)
                throw new InvalidOperationException("Could not allocate frame");
            
            var frame = new AVFrame
            {
                format = (int)AVPixelFormat.AV_PIX_FMT_YUV420P,
                width = _width,
                height = _height
            };
            
            Marshal.StructureToPtr(frame, _avFramePtr, false);
            
            if (ffmpeg.av_frame_get_buffer(_avFramePtr, 32) < 0)
                throw new InvalidOperationException("Could not allocate frame buffer");
            
            // Инициализируем конвертер цветового пространства (RGB -> YUV420P)
            _swsContextPtr = ffmpeg.sws_getContext(
                _width, _height, AVPixelFormat.AV_PIX_FMT_BGR24,
                _width, _height, AVPixelFormat.AV_PIX_FMT_YUV420P,
                (int)ffmpeg.SwsFlags.SWS_BILINEAR, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            
            if (_swsContextPtr == IntPtr.Zero)
                throw new InvalidOperationException("Could not initialize sws context");
            
            _initialized = true;
        }
    }
    
    public byte[] EncodeFrame(Bitmap bitmap)
    {
        if (!_initialized)
            throw new InvalidOperationException("Encoder not initialized");
    
        lock (_lock)
        {
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, _width, _height), 
                ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        
            try
            {
                // Source data
                var srcData = new[] { bitmapData.Scan0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero };
                var srcLinesize = new[] { bitmapData.Stride, 0, 0, 0 };
            
                // Get frame and prepare destination data
                var frame = Marshal.PtrToStructure<AVFrame>(_avFramePtr);
                var dstData = new[] { frame.data[0], frame.data[1], frame.data[2], IntPtr.Zero };
                var dstLinesize = new[] { frame.linesize[0], frame.linesize[1], frame.linesize[2], 0 };
            
                // Convert RGB to YUV420P
                ffmpeg.sws_scale(_swsContextPtr, srcData, srcLinesize, 0, _height, 
                    dstData, dstLinesize);
                
                frame.pts = ffmpeg.av_rescale_q(_frameNumber++, new AVRational { num = 1, den = _fps }, 
                    Marshal.PtrToStructure<AVRational>(ffmpeg.avcodec_get_time_base(_avCodecContextPtr)));
                Marshal.StructureToPtr(frame, _avFramePtr, false);
                
                // Кодируем фрейм
                var pkt = new AVPacket();
                ffmpeg.av_init_packet(ref pkt);
                pkt.data = IntPtr.Zero;
                pkt.size = 0;
                
                int ret;
                if ((ret = ffmpeg.avcodec_send_frame(_avCodecContextPtr, _avFramePtr)) < 0)
                    throw new InvalidOperationException($"Error sending frame: {ret}");
                
                using (var ms = new MemoryStream())
                {
                    while (ret >= 0)
                    {
                        ret = ffmpeg.avcodec_receive_packet(_avCodecContextPtr, ref pkt);
                        if (ret == ffmpeg.AVERROR(ffmpeg.EAGAIN) || ret == ffmpeg.AVERROR_EOF)
                            break;
                        if (ret < 0)
                            throw new InvalidOperationException("Error during encoding");
                        
                        // Копируем пакет в MemoryStream
                        var buffer = new byte[pkt.size];
                        Marshal.Copy(pkt.data, buffer, 0, pkt.size);
                        ms.Write(buffer, 0, buffer.Length);
                        
                        ffmpeg.av_packet_unref(ref pkt);
                    }
                    
                    return ms.ToArray();
                }
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }
    }
    
    public void Dispose()
    {
        lock (_lock)
        {
            if (_swsContextPtr != IntPtr.Zero)
            {
                ffmpeg.sws_freeContext(_swsContextPtr);
                _swsContextPtr = IntPtr.Zero;
            }
            
            if (_avFramePtr != IntPtr.Zero)
            {
                ffmpeg.av_frame_free(ref _avFramePtr);
                _avFramePtr = IntPtr.Zero;
            }
            
            if (_avCodecContextPtr != IntPtr.Zero)
            {
                ffmpeg.avcodec_free_context(ref _avCodecContextPtr);
                _avCodecContextPtr = IntPtr.Zero;
            }
            
            _initialized = false;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AVCodecContext
    {
        public IntPtr av_class;
        public int log_level_offset;
        public int codec_type;
        public IntPtr codec;
        public IntPtr codec_name;
        public AVCodecID codec_id;
        public int codec_tag;
        public int bit_rate;
        public int bit_rate_tolerance;
        public int global_quality;
        public int compression_level;
        public int flags;
        public int flags2;
        public IntPtr extradata;
        public int extradata_size;
        public AVRational time_base;
        public int width;
        public int height;
        public int gop_size;
        public AVPixelFormat pix_fmt;
        public int max_b_frames;
        public AVRational framerate;
        public long reordered_opaque;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AVFrame
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public IntPtr[] data;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public int[] linesize;
        public int width;
        public int height;
        public int format;
        public long pts;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AVPacket
    {
        public IntPtr buf;
        public long pts;
        public long dts;
        public IntPtr data;
        public int size;
        public int stream_index;
        public int flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct AVRational
    {
        public int num;
        public int den;
    }

    private enum AVPixelFormat
    {
        AV_PIX_FMT_NONE = -1,
        AV_PIX_FMT_YUV420P,
        AV_PIX_FMT_YUYV422,
        AV_PIX_FMT_RGB24,
        AV_PIX_FMT_BGR24,
        AV_PIX_FMT_NB
    }

    private enum AVCodecID
    {
        AV_CODEC_ID_NONE,
        AV_CODEC_ID_H264,
        AV_CODEC_ID_MPEG4,
        AV_CODEC_ID_NB
    }

    private static class ffmpeg
    {
        [DllImport("avcodec", CallingConvention = CallingConvention.Cdecl)]
        public static extern void avcodec_register_all();

        [DllImport("avformat", CallingConvention = CallingConvention.Cdecl)]
        public static extern void avformat_network_init();

        [DllImport("avcodec", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr avcodec_find_encoder(AVCodecID id);

        [DllImport("avcodec", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr avcodec_alloc_context3(IntPtr codec);

        [DllImport("avcodec", CallingConvention = CallingConvention.Cdecl)]
        public static extern int avcodec_open2(IntPtr avctx, IntPtr codec, IntPtr options);

        [DllImport("avcodec", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr av_frame_alloc();

        [DllImport("avcodec", CallingConvention = CallingConvention.Cdecl)]
        public static extern void av_frame_free(ref IntPtr frame);

        [DllImport("avcodec", CallingConvention = CallingConvention.Cdecl)]
        public static extern int av_frame_get_buffer(IntPtr frame, int align);

        [DllImport("avcodec", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr avcodec_get_priv_data(IntPtr avctx);

        [DllImport("avcodec", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr avcodec_get_time_base(IntPtr avctx);

        [DllImport("avcodec", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr av_frame_get_data(IntPtr frame);

        [DllImport("avcodec", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr av_frame_get_linesize(IntPtr frame);

        [DllImport("avcodec", CallingConvention = CallingConvention.Cdecl)]
        public static extern int avcodec_send_frame(IntPtr avctx, IntPtr frame);

        [DllImport("avcodec", CallingConvention = CallingConvention.Cdecl)]
        public static extern int avcodec_receive_packet(IntPtr avctx, ref AVPacket avpkt);

        [DllImport("avcodec", CallingConvention = CallingConvention.Cdecl)]
        public static extern void av_packet_unref(ref AVPacket avpkt);

        [DllImport("avcodec", CallingConvention = CallingConvention.Cdecl)]
        public static extern void av_init_packet(ref AVPacket avpkt);

        [DllImport("avcodec", CallingConvention = CallingConvention.Cdecl)]
        public static extern void avcodec_free_context(ref IntPtr avctx);

        [DllImport("swscale", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr sws_getContext(
            int srcW, int srcH, AVPixelFormat srcFormat,
            int dstW, int dstH, AVPixelFormat dstFormat,
            int flags, IntPtr srcFilter, IntPtr dstFilter, IntPtr param);

        [DllImport("swscale", CallingConvention = CallingConvention.Cdecl)]
        public static extern int sws_scale(IntPtr sws_ctx, 
            IntPtr[] srcSlice, int[] srcStride,
            int srcSliceY, int srcSliceH, 
            IntPtr[] dstSlice, int[] dstStride);

        [DllImport("swscale", CallingConvention = CallingConvention.Cdecl)]
        public static extern void sws_freeContext(IntPtr sws_ctx);

        [DllImport("avutil", CallingConvention = CallingConvention.Cdecl)]
        public static extern int AVERROR(int e);

        [DllImport("avutil", CallingConvention = CallingConvention.Cdecl)]
        public static extern long av_rescale_q(long a, AVRational bq, AVRational cq);

        [DllImport("avutil", CallingConvention = CallingConvention.Cdecl)]
        public static extern int av_opt_set(IntPtr obj, string name, string val, int search_flags);

        public const int EAGAIN = -11;
        public const int AVERROR_EOF = -541478725;

        public enum SwsFlags
        {
            SWS_BILINEAR = 2
        }
    }
}