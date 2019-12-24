using System;
using System.Collections.Generic;
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

namespace WorldOfBlocks
{
    static class Program
    {
        static void Main()
        {
            var world = new World("Many Blocks", 40, 40, 1);

            InitializeDirectX(out MainForm mainWnd,out D3DDevice device, out SwapChain swapChain);
            RunDirectX(mainWnd, ref device, swapChain, ref world);
        }

        static void InitializeDirectX(out MainForm mainWnd, out D3DDevice device, out SwapChain swapChain)
        {
            mainWnd = new MainForm();

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

            var factory = new Factory1();
            // 0: Intel 1: Nvidia 2: CPU
            var adapter = factory.Adapters1[0];
            device = new D3DDevice(adapter);
            swapChain = new SwapChain(factory, device, swapChainDesc);

            //D3DDevice.CreateWithSwapChain(
            //    DriverType.Hardware,
            //    DeviceCreationFlags.None,
            //    swapChainDesc,
            //    out device,
            //    out swapChain);
        }
        static void RunDirectX(MainForm mainWnd, ref D3DDevice device, SwapChain swapChain, ref World blockWorld)
        {
            // Setup graphics pipeline

            // 0. Prepare buffers

            // Create vertex buffer
            var vertexData = blockWorld.Blocks;
            var vertexDataSize = Utilities.SizeOf(vertexData);
            var vertexBufferDesc = new BufferDescription()
            {
                /* Usage */
                Usage = ResourceUsage.Default,
                /* ByteWidth */
                SizeInBytes = vertexDataSize,
                /* BindFlags */
                BindFlags = BindFlags.VertexBuffer,
                /* CPUAccessFlags */
                CpuAccessFlags = CpuAccessFlags.None,
                /* MiscFlags */
                OptionFlags = ResourceOptionFlags.None,
                /* StructureByteStride */
                StructureByteStride = 0,
            };
            var vertexBuffer = D3DBuffer.Create(device, vertexData, vertexBufferDesc);

            // Create constant buffer
            var constantData = new[] { Matrix.Identity };
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

            // Setup transformations & control.

            var eye = new Vector3(0, 0, -10);

            var rotateWorld = 0.0f;

            var keyMap = new Dictionary<char, bool>();
            keyMap['w'] = false; keyMap['s'] = false;
            keyMap['a'] = false; keyMap['d'] = false;
            keyMap['q'] = false; keyMap['e'] = false;

            var mouseBaseX = 0;
            var mouseBaseY = 0;
            var mousePosX = 0.0f;
            var mousePosY = 0.0f;
            var jump = 0.0f;
            var jumpVec = 0.0f;
            mainWnd.MouseMove += (sender, e) =>
            {
                mousePosX = ((e.X + mainWnd.ClientSize.Width - mouseBaseX) % mainWnd.ClientSize.Width) * 1.0f / mainWnd.ClientSize.Width;
                mousePosY = ((e.Y + mainWnd.ClientSize.Height - mouseBaseY) % mainWnd.ClientSize.Height) * 1.0f / mainWnd.ClientSize.Height;
                mainWnd.debugText.Text = "X: " + e.X + "\tY: " + e.Y + "\r\nPX: " + mousePosX + "\tPY: " + mousePosY + "\r\nJump: " + jump + "\tVec: " + jumpVec;
            };
            mainWnd.KeyDown += (sender, args) =>
            {
                if (args.KeyCode == Keys.W) keyMap['w'] = true;
                else if (args.KeyCode == Keys.S) keyMap['s'] = true;
                else if (args.KeyCode == Keys.A) keyMap['a'] = true;
                else if (args.KeyCode == Keys.D) keyMap['d'] = true;
                else if (args.KeyCode == Keys.Q) keyMap['q'] = true;
                else if (args.KeyCode == Keys.E) keyMap['e'] = true;
                else if (args.KeyCode == Keys.Space)
                {
                    jumpVec = 10.0f;
                }
            };
            mainWnd.KeyUp += (sender, args) =>
            {
                if (args.KeyCode == Keys.W) keyMap['w'] = false;
                else if (args.KeyCode == Keys.S) keyMap['s'] = false;
                else if (args.KeyCode == Keys.A) keyMap['a'] = false;
                else if (args.KeyCode == Keys.D) keyMap['d'] = false;
                else if (args.KeyCode == Keys.Q) keyMap['q'] = false;
                else if (args.KeyCode == Keys.E) keyMap['e'] = false;
                else if (args.KeyCode == Keys.Escape)
                    mainWnd.Close();
            };

            // Render.

            var vertexCount = vertexBuffer.Description.SizeInBytes / vertexSize;
            RenderLoop.Run(mainWnd, () =>
            {
                if (keyMap['w']) eye.X += 1.5f / 60.0f;
                if (keyMap['s']) eye.X -= 1.5f / 60.0f;
                if (keyMap['a']) eye.Y += 1.5f / 60.0f;
                if (keyMap['d']) eye.Y -= 1.5f / 60.0f;
                if (keyMap['q']) rotateWorld += 0.1f / 60.0f;
                if (keyMap['e']) rotateWorld -= 0.1f / 60.0f;

                // Clear views.
                context.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1.0f, 0);
                context.ClearRenderTargetView(renderView, Color.Black);

                // Update world data.
                if (jump > 0.0f || (jump == 0.0f && jumpVec != 0.0f))
                {
                    jump += jumpVec / 60.0f / 3.0f;
                    jumpVec += -9.8f / 60.0f / 3.0f;
                }
                else
                {
                    jump = jumpVec = 0.0f;
                }

                var ray = new Vector3(eye.X, eye.Y + 1.0f, eye.Z - jump) - new Vector3(eye.X, eye.Y, eye.Z - jump);

                var proj = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, mainWnd.ClientSize.Width / (float)mainWnd.ClientSize.Height, 0.1f, 100.0f);
                var view = Matrix.LookAtLH(new Vector3(eye.X, eye.Y, eye.Z - jump), new Vector3(eye.X, eye.Y + 1.0f, eye.Z - jump), new Vector3(0, 0, 1));
                var world = Matrix.RotationZ(rotateWorld);

                var matrix = world * (view * Matrix.RotationY(mousePosX * 2.0f * (float)Math.PI) * Matrix.RotationX(-mousePosY * 2.0f * (float)Math.PI)) * proj;
                matrix.Transpose();

                context.UpdateSubresource(ref matrix, constantBuffer);

                // Draw.
                context.Draw(vertexCount, 0);

                swapChain.Present(0, PresentFlags.None);
            });
        }
    }
}
