using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics;
using SharpDX.Windows;
using D3DBuffer = SharpDX.Direct3D11.Buffer;
using D3DDevice = SharpDX.Direct3D11.Device;
using D2DRenderTarget = SharpDX.Direct2D1.RenderTarget;
using D2DDeviceContext = SharpDX.Direct2D1.DeviceContext;

namespace BlockMarket.Render
{
    // DO
    //  - Initialize DirectX.
    //  - Create a new window.
    //  - Start message loop.
    // SUPPORT
    //  - Window resizing, mouse capturing.
    // API
    //  - Resource management: export manager objects.
    //  - Pipeline configuration: export DeviceContext object.
    //  - Main loop: provide three extension delegates.
    class D3DApplication
    {
        public D3DApplication()
        {
        }

        public MainForm MainWindow { get => mainWnd; }
        // D3D Objects
        public D3DDevice Device { get => device; }
        public DeviceContext Context { get => device.ImmediateContext; }
        public SwapChain SwapChain { get => swapChain; }
        public D3DResourceManager ResourceManager { get => resourceManager; }
        // D2D Objects
        public D2DDeviceContext D2DDeviceContext { get => d2dDeviceContext; }

        public void Initialize()
        {
            try
            {
                mainWnd = new MainForm();

                // Create device and swapChain.
                InitializeD3D();

                // Create resource manager, load external resources.
                PrepareResources();

                // Initialize pipeline data & state.
                InitializeD3DPipeline();

                // Initialize render targets.
                ResetRenderTargets(WindowEvent.Resize);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                throw e;
            }
        }
        public void Start(ControlLogicMethod ControlLogic, GameLogicMethod GameLogic, RenderLogicMethod RenderLogic)
        {
            var perfReset = false;

            // Setup window events & user input handling.
            {
                mainWnd.MouseDown += (object sender, MouseEventArgs e) =>
                {
                    mainWnd.LockMouse();
                };
                mainWnd.Resize += (object sender, EventArgs e) =>
                {
                    if (windowEvent < WindowEvent.Resize)
                        windowEvent = WindowEvent.Resize;
                };
                mainWnd.KeyUp += (object sender, KeyEventArgs e) =>
                {
                    if (e.KeyCode == Keys.F5)
                    {
                        if (windowEvent < WindowEvent.EnterFullScreen)
                            windowEvent = WindowEvent.EnterFullScreen;
                        else if (windowEvent < WindowEvent.ExitFullScreen)
                            windowEvent = WindowEvent.ExitFullScreen;
                    }
                    else if (e.KeyCode == Keys.F)
                    {
                        perfReset = true;
                    }
                    else if (e.KeyCode == Keys.Escape)
                    {
                        if (mainWnd.IsMouseLocked)
                            mainWnd.UnlockMouse();
                        else
                            mainWnd.Close();
                    }
                };
            }
            ControlLogic(mainWnd);

            // Start game loop
            var timer = new Stopwatch();
            timer.Start();
            var perfTimer = new Stopwatch();
            long perfPrepareCounter = 0;
            long perfLogicCounter = 0;
            long perfRenderCounter = 0;
            long perfFrameCounter = 1;
            perfTimer.Restart();
            RenderLoop.Run(mainWnd, () =>
            {
                // Before enter this function, process user input from mainWnd.

                if (windowEvent != WindowEvent.None)
                {
                    ResetRenderTargets(windowEvent);
                    windowEvent = WindowEvent.None;
                }

                perfPrepareCounter += perfTimer.ElapsedTicks;

                var elapsedTimeMS = timer.ElapsedMilliseconds;
                timer.Restart();

                // Change game state.

                perfTimer.Restart();

                GameLogic(elapsedTimeMS);

                perfLogicCounter += perfTimer.ElapsedTicks;

                // Render.

                perfTimer.Restart();

                ClearScreen();
                RenderLogic(out string debugText);
                PresentNextFrame();

                perfRenderCounter += perfTimer.ElapsedTicks;
                ++perfFrameCounter;

                mainWnd.debugText.Text =
                    debugText +
                    //"========= Misc =========" + "\r\n" +
                    //"Mouse Captured: " + mainWnd.Capture + "\r\n" +
                    "========= Performance =========" + "\r\n" +
                    //"Frame:" + perfFrameCounter + "\r\n" +
                    //"Prep   Tick:" + perfPrepareCounter + "\r\n" +
                    //"Logic  Tick:" + perfLogicCounter + "\r\n" +
                    //"Render Tick:" + perfRenderCounter + "\r\n" +
                    "Prep   Time (ms):" + (perfPrepareCounter * 1000.0f / Stopwatch.Frequency / perfFrameCounter).ToString("0.00") + "\r\n" +
                    "Logic  Time (ms):" + (perfLogicCounter * 1000.0f / Stopwatch.Frequency / perfFrameCounter).ToString("0.00") + "\r\n" +
                    "Render Time (ms):" + (perfRenderCounter * 1000.0f / Stopwatch.Frequency / perfFrameCounter).ToString("0.00") + "\r\n" +
                    "";
                if (perfReset)
                {
                    perfReset = false;
                    perfFrameCounter = 0;
                    perfPrepareCounter = 0;
                    perfRenderCounter = 0;
                    perfLogicCounter = 0;
                }
                perfTimer.Restart();
            });
        }

        // ---- Extension Interface ----

        public delegate void ControlLogicMethod(Control mainWnd);
        public delegate void GameLogicMethod(float elapsedTimeMS);
        public delegate void RenderLogicMethod(out string debugText);

        // ---- Internal Methods ----

        // Initialization.
        private void InitializeD3D()
        {
            // Swapchain defines
            // * display mode (resolution, refresh rate, format, scanline, scale)
            // * surface format
            var swapChainDesc = new SwapChainDescription()
            {
                ModeDescription = new ModeDescription(Format.Unknown)
                {
                    Width = mainWnd.ClientSize.Width,
                    Height = mainWnd.ClientSize.Height,
                    RefreshRate = new Rational(60, 1),
                    Format = Format.B8G8R8A8_UNorm, // Required to work with DirectX 2D
                    ScanlineOrdering = DisplayModeScanlineOrder.Unspecified,
                    Scaling = DisplayModeScaling.Unspecified
                },
                SampleDescription = new SampleDescription(0, 0)
                {
                    Count = 1,
                    Quality = 0
                },
                Usage = Usage.RenderTargetOutput,
                // Set to 1: In windowed mode, the desktop is the front buffer.
                // Set to 2: In full-screen mode, there is a dedicated front buffer.
                BufferCount = SWAPCHAIN_BUFFER_COUNT,
                OutputHandle = mainWnd.Handle,
                IsWindowed = true,
                SwapEffect = SwapEffect.Discard,
            };

            var factory = new Factory1();
            // 0: Intel 1: Nvidia 2: CPU
            var adapter = factory.Adapters1[0];

            device = new D3DDevice(
                adapter,
                DeviceCreationFlags.BgraSupport // Required to work with DirectX 2D
                );
            swapChain = new SwapChain(factory, device, swapChainDesc);

            adapter.Dispose();
            factory.Dispose();
        }
        private void PrepareResources()
        {
            resourceManager = new D3DResourceManager(device);
            resourceManager.LoadResources();
        }
        private void InitializeD3DPipeline()
        {
            // Setup graphics pipeline

            var context = device.ImmediateContext;

            // 0. Prepare buffers

            resourceManager.GetCB().Reset(new[] { Matrix.Identity });

            var constantBuffer = resourceManager.GetCB().Buffer;

            // 1. IA, VS, PS stage: Prepare shaders, bind buffers

            context.InputAssembler.InputLayout = new InputLayout(
                device,
                ShaderSignature.GetInputSignature(resourceManager.GetDefaultVSShaderByteCode()),
                D3DVertex.InputFormat);
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;

            context.VertexShader.SetConstantBuffer(0, constantBuffer);
            context.VertexShader.Set(resourceManager.GetDefaultVSShader());

            context.PixelShader.SetShaderResource(0, resourceManager.GetDefaultTextureSRV());
            context.PixelShader.Set(resourceManager.GetDefaultPSShader());
        }
        private void ResetRenderTargets(WindowEvent e)
        {
            var X = MainWindow.ClientSize.Width;
            var Y = MainWindow.ClientSize.Height;

            // Dispose D3D & D2D render target objects.

            renderTargetView?.Dispose();
            depthStencilView?.Dispose();
            d2dDeviceContext?.Dispose();

            // D3D: swapChain, backBuffer, renderTargetView, depthBuffer, depthStencilView

            if (e == WindowEvent.EnterFullScreen) swapChain.SetFullscreenState(true, null);
            else if (e == WindowEvent.ExitFullScreen) swapChain.SetFullscreenState(false, null);
            swapChain.ResizeBuffers(SWAPCHAIN_BUFFER_COUNT, X, Y, Format.Unknown, SwapChainFlags.None);

            var viewport = new Viewport(0, 0, X, Y, 0.0f, 1.0f);
            device.ImmediateContext.Rasterizer.SetViewport(viewport);

            var backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
            renderTargetView = new RenderTargetView(Device, backBuffer);
            backBuffer.Dispose();

            var depthBuffer = new Texture2D(device, new Texture2DDescription()
            {
                Format = Format.D32_Float_S8X24_UInt,
                ArraySize = 1,
                MipLevels = 1,
                Width = X,
                Height = Y,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });
            depthStencilView = new DepthStencilView(Device, depthBuffer);

            device.ImmediateContext.OutputMerger.SetDepthStencilState(resourceManager.GetDefaultDepthStencilState());
            device.ImmediateContext.OutputMerger.SetBlendState(resourceManager.GetDefaultBlendState());
            device.ImmediateContext.OutputMerger.SetTargets(depthStencilView, renderTargetView);

            // D2D: deviceContext

            var dxgiSurface = Surface.FromSwapChain(SwapChain, 0);
            d2dDeviceContext = new D2DDeviceContext(dxgiSurface);
            dxgiSurface.Dispose();
        }
        // Per frame.
        private void ClearScreen()
        {
            var context = device.ImmediateContext;

            context.ClearDepthStencilView(depthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
            context.ClearRenderTargetView(renderTargetView, Color.Black);
        }
        private void PresentNextFrame()
        {
            swapChain.Present(0, PresentFlags.None);
        }

        // ------- Internal State -------

        private MainForm mainWnd;
        private enum WindowEvent { None, Resize, EnterFullScreen, ExitFullScreen };
        private WindowEvent windowEvent = WindowEvent.None;

        // -------- DX 3D State --------

        private D3DDevice device;
        private SwapChain swapChain;
        private readonly int SWAPCHAIN_BUFFER_COUNT = 1;
        // Buffers, textures, shaders, states.
        private D3DResourceManager resourceManager;
        // Views.
        private RenderTargetView renderTargetView;
        private DepthStencilView depthStencilView;

        // -------- DX 2D State --------

        private D2DDeviceContext d2dDeviceContext;
    }
}
