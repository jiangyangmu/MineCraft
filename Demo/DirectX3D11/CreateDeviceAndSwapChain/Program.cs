using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace CreateDeviceAndSwapChain
{
    static class Program
    {
        static void Main()
        {
            var mainWnd = new Form1();

            // 1. Describe SwapChain

            // BufferDesc - Describes a display mode
            //
            // display resolution
            //      600 x 480
            // FPS
            //      60
            // display pixel format
            //      32-bit unsigned-normalized-integer format that supports 8 bits per channel including alpha.
            // scanline ordering
            //      Unspecified
            //      Progressive      - The image is created from the first scanline to the last without skipping any.
            //      UpperFieldFirst  - The image is created beginning with the upper field.
            //      LowerFieldFirst  - The image is created beginning with the lower field.
            // scaling
            //      Unspecified
            //      Centered    - Specifies no scaling. The image is centered on the display.
            //      Stretched   - Specifies stretched scaling.
            var bufferDesc = new ModeDescription(Format.Unknown)
            {
                Width = 600,
                Height = 480,
                RefreshRate = new Rational(60, 1),
                Format = Format.R8G8B8A8_UNorm,
                ScanlineOrdering = DisplayModeScanlineOrder.Unspecified,
                Scaling = DisplayModeScaling.Unspecified
            };
            mainWnd.ShowObjectWithFields("BufferDesc", bufferDesc);

            // SampleDesc - describes multi-sampling parameters
            //
            // # of multisamples per pixel
            //      1
            // quality level - the higher, the better quality
            //      0
            var sampleDesc = new SampleDescription(0, 0)
            {
                Count = 1,
                Quality = 0
            };

            // BufferUsage - describes the surface usage and CPU access options for the back buffer
            //
            //      BackBuffer          - The surface or resource is used as a back buffer.
            //      ReadOnly            - Use the surface or resource for reading only.
            //      RenderTargetOutput  - Use the surface or resource as an output render target.
            //      ShaderInput         - Use the surface or resource as an input to a shader.
            //      Shared              - Share the surface or resource.
            //      UnorderedAccess     - Use the surface or resource for unordered access.
            var bufferUsage = Usage.RenderTargetOutput;

            // BufferCount - describes the number of buffers in the swap chain.
            int bufferCount = 1;

            // SwapEffect - describes options for handling pixels in a display surface after calling SwapChain.Present
            //      Discard
            //      Sequential
            //      FlipDiscard
            //      FlipSequential
            var swapEffect = SwapEffect.Discard;

            // Flags

            var swapChainDesc = new SwapChainDescription()
            {
                /* BUfferDesc */ ModeDescription = bufferDesc,
                /* SampleDesc */ SampleDescription = sampleDesc,
                /* BufferUsage */ Usage = bufferUsage,
                /* BufferCount */ BufferCount = bufferCount,
                /* OutputWindow */ OutputHandle = mainWnd.Handle,
                /* Windowed */ IsWindowed = true,
                /* SwapEffect */ SwapEffect = swapEffect,
                /* Flags */
            };
            mainWnd.ShowObjectWithFields("SwapChainDescription", swapChainDesc);

            // 2. Create Device and SwapChain

            SharpDX.Direct3D11.Device.CreateWithSwapChain(
                DriverType.Hardware,
                DeviceCreationFlags.None,
                swapChainDesc,
                out SharpDX.Direct3D11.Device device,
                out SwapChain swapChain);

            Application.Run(mainWnd);
        }
    }
}
