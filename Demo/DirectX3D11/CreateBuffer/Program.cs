using System;
using System.Windows.Forms;
using System.Diagnostics;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics;
using SharpDX.Windows;
using D3DBuffer = SharpDX.Direct3D11.Buffer;
using D3DDevice = SharpDX.Direct3D11.Device;

namespace CreateBuffer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            InitializeDirectX(
                out Form mainWnd,
                out D3DDevice device,
                out SwapChain swapChain);

            // Create vertex buffer

            // Vertex Elements: Position, Color, Normal
            var vertexData = new[]
            {
                // Vertex = (Position, Color)
                new Vector4(-1.0f,  0.0f, 0.0f, 1.0f), new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                new Vector4( 0.0f,  1.732f, 0.0f, 1.0f), new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                new Vector4( 1.0f,  0.0f, 0.0f, 1.0f), new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
            };
            var vertexDataSize = vertexData.Length * Utilities.SizeOf<Vector4>();

            var vertexBufferDesc = new BufferDescription()
            {
                /* Usage */ Usage = ResourceUsage.Default,
                /* ByteWidth */ SizeInBytes = vertexDataSize,
                /* BindFlags */ BindFlags = BindFlags.VertexBuffer,
                /* CPUAccessFlags */ CpuAccessFlags = CpuAccessFlags.None,
                /* MiscFlags */ OptionFlags = ResourceOptionFlags.None,
                /* StructureByteStride */ StructureByteStride = 0,
            };

            var vertexBuffer = D3DBuffer.Create(device, vertexData, vertexBufferDesc);

            // TODO: Create index buffer

            // Create constant buffer

            // Example: Transformation Matrix
            var constantData = new[]
            {
                // WorldViewProjectMatrix
                Matrix.Identity
            };
            var constantDataSize = constantData.Length * Utilities.SizeOf<Matrix>();

            var constantBufferDesc = new BufferDescription()
            {
                /* Usage */
                Usage = ResourceUsage.Default,
                /* ByteWidth */
                SizeInBytes = constantDataSize,
                /* BindFlags */
                BindFlags = BindFlags.ConstantBuffer,
                /* CPUAccessFlags */
                CpuAccessFlags = CpuAccessFlags.None,
                /* MiscFlags */
                OptionFlags = ResourceOptionFlags.None,
                /* StructureByteStride */
                StructureByteStride = 0,
            };

            var constantBuffer = D3DBuffer.Create(device, constantData, constantBufferDesc);

            RunDirectX(
                    mainWnd,
                    ref device,
                    swapChain,
                    ref vertexBuffer,
                    constantBuffer);
        }

        static void InitializeDirectX(out Form mainWnd, out D3DDevice device, out SwapChain swapChain)
        {
            mainWnd = new Form1();

            var bufferDesc = new ModeDescription(Format.Unknown)
            {
                Width = mainWnd.ClientSize.Width,
                Height = mainWnd.ClientSize.Height,
                RefreshRate = new Rational(60, 1),
                Format = Format.R8G8B8A8_UNorm,
                ScanlineOrdering = DisplayModeScanlineOrder.Unspecified,
                Scaling = DisplayModeScaling.Unspecified
            };
            var sampleDesc = new SampleDescription(0, 0)
            {
                Count = 1,
                Quality = 0
            };
            var bufferUsage = Usage.RenderTargetOutput;
            int bufferCount = 1;
            var swapEffect = SwapEffect.Discard;

            var swapChainDesc = new SwapChainDescription()
            {
                ModeDescription = bufferDesc,
                SampleDescription = sampleDesc,
                Usage = bufferUsage,
                BufferCount = bufferCount,
                OutputHandle = mainWnd.Handle,
                IsWindowed = true,
                SwapEffect = swapEffect,
            };
            D3DDevice.CreateWithSwapChain(
                DriverType.Hardware,
                DeviceCreationFlags.None,
                swapChainDesc,
                out device,
                out swapChain);
        }
        static void RunDirectX(Form mainWnd, ref D3DDevice device, SwapChain swapChain, ref D3DBuffer vertexBuffer, D3DBuffer constantBuffer)
        {
            // Setup graphics pipeline

            var context = device.ImmediateContext;

            // 1. IA, VS, PS stage: Prepare shaders, bind buffers

            var vertexSize = Utilities.SizeOf<Vector4>() * 2;
            var vertexBuffers = new[] { vertexBuffer };
            var vertexBufferStrides = new[] { vertexSize };
            var vertexBufferOffsets = new[] { 0 };

            var vertexShaderByteCode = ShaderBytecode.CompileFromFile("Shader.fx", "VS", "vs_4_0");
            var vertexShader = new VertexShader(device, vertexShaderByteCode);
            var pixelShaderByteCode = ShaderBytecode.CompileFromFile("Shader.fx", "PS", "ps_4_0");
            var pixelShader = new PixelShader(device, pixelShaderByteCode);
            var signature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
            var inputLayout = new InputLayout(device, signature, new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0)
            });

            context.InputAssembler.InputLayout = inputLayout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.InputAssembler.SetVertexBuffers(0, vertexBuffers, vertexBufferStrides, vertexBufferOffsets);
            context.VertexShader.SetConstantBuffer(0, constantBuffer);
            context.VertexShader.Set(vertexShader);
            context.PixelShader.Set(pixelShader);

            // 2. Rasterizer, OM stage: viewport, render targets & depth-stencil test.

            var viewport = new Viewport(0, 0, mainWnd.ClientSize.Width, mainWnd.ClientSize.Height, 0.0f, 1.0f);

            var backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
            var depthBuffer = new Texture2D(device, new Texture2DDescription()
            {
                Format = Format.D32_Float_S8X24_UInt,
                ArraySize = 1,
                MipLevels = 1,
                Width = mainWnd.ClientSize.Width,
                Height = mainWnd.ClientSize.Height,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });
            var renderView = new RenderTargetView(device, backBuffer);
            var depthView = new DepthStencilView(device, depthBuffer);

            context.Rasterizer.SetViewport(viewport);
            context.OutputMerger.SetTargets(depthView, renderView);

            // Setup transformations.

            var view = Matrix.LookAtLH(new Vector3(0, 0, -5), new Vector3(0, 0, 0), Vector3.UnitY);
            var proj = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, mainWnd.ClientSize.Width / (float)mainWnd.ClientSize.Height, 0.1f, 100.0f);
            var world = Matrix.Identity;

            // Render.

            mainWnd.KeyUp += (sender, args) =>
            {
                if (args.KeyCode == Keys.Escape)
                    mainWnd.Close();
            };
            var clock = new Stopwatch();
            clock.Start();
            var vertexCount = vertexBuffer.Description.SizeInBytes / vertexSize;
            RenderLoop.Run(mainWnd, () => {
                // Clear views.
                context.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1.0f, 0);
                context.ClearRenderTargetView(renderView, Color.Black);

                // Update world data.
                var time = clock.ElapsedMilliseconds / 1000.0f;
                world = Matrix.Translation(new Vector3(0, -1.732f / 2.0f, 0)) * Matrix.RotationZ(time);

                var matrix = world * view * proj;
                matrix.Transpose();

                context.UpdateSubresource(ref matrix, constantBuffer);

                // Draw.
                context.Draw(vertexCount, 0);

                swapChain.Present(0, PresentFlags.None);
            });
        }
    }
}
