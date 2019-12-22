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

namespace MineBlock
{
    struct DXVertex
    {
        public Vector3 pos;
        public Vector3 col;
        public Vector2 tex;

        public DXVertex(Vector3 pos, Vector3 col)
        {
            this.pos = pos;
            this.col = col;
            this.tex = Vector2.Zero;
        }
        public DXVertex(Vector3 pos, Vector3 col, Vector2 tex)
        {
            this.pos = pos;
            this.col = col;
            this.tex = tex;
        }

        public static InputElement[] InputFormat
        {
            get => new InputElement[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                new InputElement("COLOR", 0, Format.R32G32B32_Float, 12, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, 24, 0),
            };
        }
    }
    class Game
    {
        public Game()
        {
        }

        public Form MainWindow { get => mainWnd; }
        public D3DDevice Device { get => device; }
        public DeviceContext Context { get => device.ImmediateContext; }
        public SwapChain SwpChain { get => swapChain; }
        public DXBufferManager BufferManager { get => bufferManager; }

        public void Initialize(World world, Camera camera)
        {
            try
            {
                mainWnd = new MainForm();

                // Create device and swapChain.
                InitializeDX();

                // Load external resources.
                LoadResources();

                // Initialize pipeline data & state
                // * shaders
                // * buffers
                // * textures
                // * set stage state
                InitializeDXPipeline(world, camera);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                throw e;
            }
        }
        public void Start(ControlLogicMethod ControlLogic, GameLogicMethod GameLogic, RenderLogicMethod RenderLogic)
        {
            // Setup window events & user input handling.
            {
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
                    else if (e.KeyCode == Keys.Escape)
                    {
                        mainWnd.Close();
                    }
                };
            }
            ControlLogic(mainWnd);

            // Start game loop
            // 1. process user inputs
            // 2. update world
            // 3. depends on 2, maybe update DX input buffers
            // 4. set IA topology, offset, length, draw, repeat
            var timer = new Stopwatch();
            timer.Start();
            RenderLoop.Run(mainWnd, () =>
            {
                if (windowEvent != WindowEvent.None)
                {
                    ResizeDXViews(windowEvent);
                    windowEvent = WindowEvent.None;
                }

                var elapsedTimeMS = timer.ElapsedMilliseconds;
                timer.Restart();
                
                GameLogic(elapsedTimeMS);
                ClearScreen();
                RenderLogic(out string debugText); (mainWnd as MainForm).debugText.Text = debugText;
                PresentNextFrame();
            });
        }

        // ---- Extension Interface ----

        public delegate void ControlLogicMethod(Control mainWnd);
        public delegate void GameLogicMethod(float elapsedTimeMS);
        public delegate void RenderLogicMethod(out string debugText);

        // ---- Internal Methods ----

        // Initialization.
        private void InitializeDX()
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
                    Format = Format.R8G8B8A8_UNorm,
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

            device = new D3DDevice(adapter);
            swapChain = new SwapChain(factory, device, swapChainDesc);
            bufferManager = new DXBufferManager(device);

            adapter.Dispose();
            factory.Dispose();
        }
        private void LoadResources()
        {
            var vertexShaderByteCode = ShaderBytecode.CompileFromFile("Shader.fx", "VS", "vs_4_0");
            vertexShader = new VertexShader(device, vertexShaderByteCode);

            var pixelShaderByteCode = ShaderBytecode.CompileFromFile("Shader.fx", "PS", "ps_4_0");
            pixelShader = new PixelShader(device, pixelShaderByteCode);

            var signature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
            inputLayout = new InputLayout(device, signature, DXVertex.InputFormat);

            var texture = DXTextureLoader.CreateTexture2DFromBitmap(device,
                DXTextureLoader.LoadBitmap(new SharpDX.WIC.ImagingFactory2(), "Texture/Grass.png"));
            textureSRV = new ShaderResourceView(device, texture);
        }
        private void InitializeDXPipeline(World world, Camera camera)
        {
            // Setup graphics pipeline

            var context = device.ImmediateContext;

            // 0. Prepare buffers

            world.UpdateVertexBuffer(bufferManager.GetVB(DXBufferType.TRIANGLES));
            bufferManager.GetCB(DXBufferType.CONSTANT).Reset(new[] { Matrix.Identity });

            var vertexBuffer = bufferManager.GetVB(DXBufferType.TRIANGLES).Buffer;
            var constantBuffer = bufferManager.GetCB(DXBufferType.CONSTANT).Buffer;

            // 1. IA, VS, PS stage: Prepare shaders, bind buffers

            context.InputAssembler.InputLayout = inputLayout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(
                vertexBuffer,
                Utilities.SizeOf<Vector4>() * 2, // vertex size
                0));

            context.VertexShader.SetConstantBuffer(0, constantBuffer);
            context.VertexShader.Set(vertexShader);

            context.PixelShader.SetShaderResource(0, textureSRV);
            context.PixelShader.Set(pixelShader);

            ResizeDXViews(WindowEvent.Resize);
        }
        private void ResizeDXViews(WindowEvent e)
        {
            // swapChain, backBuffer, renderTargetView, depthBuffer, depthStencilView

            if (renderTargetView != null) renderTargetView.Dispose();
            if (depthBuffer != null) depthBuffer.Dispose();
            if (depthStencilView != null) depthStencilView.Dispose();

            if (e == WindowEvent.EnterFullScreen) swapChain.SetFullscreenState(true, null);
            else if (e == WindowEvent.ExitFullScreen) swapChain.SetFullscreenState(false, null);
            swapChain.ResizeBuffers(SWAPCHAIN_BUFFER_COUNT, MainWindow.ClientSize.Width, MainWindow.ClientSize.Height, Format.Unknown, SwapChainFlags.None);

            viewport = new Viewport(0, 0, MainWindow.ClientSize.Width, MainWindow.ClientSize.Height, 0.0f, 1.0f);
            device.ImmediateContext.Rasterizer.SetViewport(viewport);

            backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
            renderTargetView = new RenderTargetView(Device, backBuffer);
            backBuffer.Dispose();

            depthBuffer = new Texture2D(device, new Texture2DDescription()
            {
                Format = Format.D32_Float_S8X24_UInt,
                ArraySize = 1,
                MipLevels = 1,
                Width = MainWindow.ClientSize.Width,
                Height = MainWindow.ClientSize.Height,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });
            depthStencilView = new DepthStencilView(Device, depthBuffer);

            device.ImmediateContext.OutputMerger.SetTargets(depthStencilView, renderTargetView);
            // MessageBox.Show("Size: " + MainWindow.ClientSize.ToString());
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

        private Form mainWnd;
        private enum WindowEvent { None, Resize, EnterFullScreen, ExitFullScreen };
        private WindowEvent windowEvent = WindowEvent.None;

        // -------- DX State --------

        private D3DDevice device;
        private SwapChain swapChain;
        private readonly int SWAPCHAIN_BUFFER_COUNT = 1;
        // Shaders.
        private InputLayout inputLayout;
        private VertexShader vertexShader;
        private PixelShader pixelShader;
        // Buffers.
        private DXBufferManager bufferManager;
        // Textures
        private ShaderResourceView textureSRV;

        private Texture2D backBuffer;
        private Texture2D depthBuffer;
        // Views.
        private Viewport viewport;
        private RenderTargetView renderTargetView;
        private DepthStencilView depthStencilView;
    }
}
