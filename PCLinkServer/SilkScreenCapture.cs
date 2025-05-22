namespace PCLinkServer;

using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Core.Native;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

public unsafe class SilkScreenCapture
{
    // private ID3D11Device* _device;
    // private ID3D11DeviceContext* _context;
    // private IDXGIOutputDuplication* _duplication;
    // private IDXGIOutput1* _output1;
    //
    // private DXGI DXGI = DXGI.GetApi();
    // private D3D11 D3D11 = D3D11.GetApi();
    //
    // private int _width;
    // private int _height;
    //
    // public void Initialize()
    // {
    //     // Create device
    //     ID3D11Device* device;
    //     ID3D11DeviceContext* context;
    //
    //     D3D_FEATURE_LEVEL* featureLevels = stackalloc D3D_FEATURE_LEVEL[1] { D3D_FEATURE_LEVEL.D3DFeatureLevel110 };
    //     D3D_FEATURE_LEVEL actualLevel;
    //
    //     if (D3D11.D3D11CreateDevice(null, D3D_DRIVER_TYPE.D3DDriverTypeHardware, 0, (uint)CreateDeviceFlag.BgraSupport,
    //             featureLevels, 1, D3D11.SdkVersion, &device, &actualLevel, &context) < 0)
    //     {
    //         throw new Exception("Failed to create D3D11 device");
    //     }
    //
    //     _device = device;
    //     _context = context;
    //
    //     // Get DXGI Device
    //     IDXGIDevice* dxgiDevice;
    //     if (*(IntPtr*)device->LpVtbl->QueryInterface(device, SilkMarshal.GuidPtrOf<IDXGIDevice>(), (void**)&dxgiDevice) < 0)
    //     {
    //         throw new Exception("Failed to get IDXGIDevice");
    //     }
    //
    //     IDXGIAdapter* adapter;
    //     dxgiDevice->GetAdapter(&adapter);
    //
    //     // Get output
    //     IDXGIOutput* output;
    //     adapter->EnumOutputs(0, &output);
    //
    //     // Get output1
    //     if (*(IntPtr*)output->LpVtbl->QueryInterface(output, SilkMarshal.GuidPtrOf<IDXGIOutput1>(), (void**)&_output1) < 0)
    //     {
    //         throw new Exception("Failed to get IDXGIOutput1");
    //     }
    //
    //     // Duplicate output
    //     if (_output1->DuplicateOutput((IUnknown*)device, &_duplication) < 0)
    //     {
    //         throw new Exception("Failed to duplicate output");
    //     }
    //
    //     // Get output description (to retrieve width/height)
    //     OutputDesc desc;
    //     output->GetDesc(&desc);
    //     _width = desc.DesktopCoordinates.Right - desc.DesktopCoordinates.Left;
    //     _height = desc.DesktopCoordinates.Bottom - desc.DesktopCoordinates.Top;
    // }
    //
    // public Bitmap CaptureFrame()
    // {
    //     IDXGIResource* desktopResource;
    //     OutduplFrameInfo frameInfo;
    //
    //     if (_duplication->AcquireNextFrame(1000, &frameInfo, &desktopResource) < 0)
    //     {
    //         throw new Exception("Failed to acquire next frame");
    //     }
    //
    //     ID3D11Texture2D* acquiredTexture;
    //     if (desktopResource->QueryInterface(SilkMarshal.GuidPtrOf<ID3D11Texture2D>(), (void**)&acquiredTexture) < 0)
    //     {
    //         _duplication->ReleaseFrame();
    //         throw new Exception("Failed to get ID3D11Texture2D");
    //     }
    //
    //     // Create staging texture
    //     Texture2DDesc desc;
    //     acquiredTexture->GetDesc(&desc);
    //
    //     desc.Usage = Usage.UsageStaging;
    //     desc.BindFlags = 0;
    //     desc.CPUAccessFlags = (uint)CpuAccessFlag.CpuAccessRead;
    //     desc.MiscFlags = 0;
    //
    //     ID3D11Texture2D* stagingTex;
    //     _device->CreateTexture2D(&desc, null, &stagingTex);
    //
    //     // Copy resource
    //     _context->CopyResource((ID3D11Resource*)stagingTex, (ID3D11Resource*)acquiredTexture);
    //
    //     // Map resource
    //     MappedSubresource mapped;
    //     _context->Map((ID3D11Resource*)stagingTex, 0, (uint)Map.MapRead, 0, &mapped);
    //
    //     // Copy data to Bitmap
    //     Bitmap bmp = new Bitmap(_width, _height, PixelFormat.Format32bppArgb);
    //     var bmpData = bmp.LockBits(new Rectangle(0, 0, _width, _height), ImageLockMode.WriteOnly, bmp.PixelFormat);
    //
    //     for (int y = 0; y < _height; y++)
    //     {
    //         Buffer.MemoryCopy(
    //             (byte*)mapped.PData + y * mapped.RowPitch,
    //             (byte*)bmpData.Scan0 + y * bmpData.Stride,
    //             bmpData.Stride,
    //             bmpData.Stride);
    //     }
    //
    //     bmp.UnlockBits(bmpData);
    //
    //     // Cleanup
    //     _context->Unmap((ID3D11Resource*)stagingTex, 0);
    //     _duplication->ReleaseFrame();
    //
    //     stagingTex->Release();
    //     acquiredTexture->Release();
    //     desktopResource->Release();
    //
    //     return bmp;
    // }
}
