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

namespace BlockMarket
{
    class ResourceManager
    {
        public PixelShader GetDefaultPSShader()
        {
            return pixelShaderDict["PS_Texture_Block"];
        }
        public ShaderResourceView GetDefaultTextureSRV()
        {
            return textureSRVDict["Grass"];
        }
        public PixelShader GetPSShader(string name)
        {
            return pixelShaderDict[name];
        }
        public ShaderResourceView GetTextureSRV(string name)
        {
            return textureSRVDict[name];
        }

        public void LoadResources(D3DDevice device)
        {
            var shaderNames = new[]
            {
                "PS_Texture_Block",
                "PS_Line_Block",
            };
            foreach (var shaderName in shaderNames)
            {
                var pixelShaderByteCode = ShaderBytecode.CompileFromFile("Shader.fx", shaderName, "ps_4_0");
                pixelShaderDict.Add(shaderName, new PixelShader(device, pixelShaderByteCode));
            }

            var textureNameAndImages = new string[][]
            {
                new string[]{ "Grass", "Texture/Grass.png" },
                new string[]{ "OakWood", "Texture/OakWood.jpg" },
            };
            var imageFactory = new SharpDX.WIC.ImagingFactory2();
            foreach (var nameAndImage in textureNameAndImages)
            {
                var texture = D3DTextureLoader.CreateTexture2DFromBitmap(device,
                    D3DTextureLoader.LoadBitmap(imageFactory, nameAndImage[1]));
                textureSRVDict.Add(nameAndImage[0], new ShaderResourceView(device, texture));
                texture.Dispose();
            }
            imageFactory.Dispose();
        }

        private Dictionary<string, PixelShader> pixelShaderDict = new Dictionary<string, PixelShader>();
        private Dictionary<string, ShaderResourceView> textureSRVDict = new Dictionary<string, ShaderResourceView>();
    }
    class Game
    {
        public Game()
        {
        }

        public MainForm MainWindow { get => mainWnd; }
        // D3D Objects
        public D3DDevice Device { get => device; }
        public DeviceContext Context { get => device.ImmediateContext; }
        public SwapChain SwapChain { get => swapChain; }
        public ResourceManager ResourceManager { get => resourceManager; }
        public D3DBufferManager BufferManager { get => bufferManager; }
        // D2D Objects
        public D2DDeviceContext D2DDeviceContext { get => d2dDeviceContext; }

        public void Initialize(World world)
        {
            try
            {
                mainWnd = new MainForm();

                // Create device and swapChain.
                InitializeD3D();

                // Load external resources.
                LoadResources();

                // Initialize pipeline data & state.
                // * shaders
                // * buffers
                // * textures
                // * set stage state
                InitializeD3DPipeline(world);

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
            RenderLoop.Run(mainWnd, () =>
            {
                // Before enter this function, process user input from mainWnd.

                if (windowEvent != WindowEvent.None)
                {
                    ResetRenderTargets(windowEvent);
                    windowEvent = WindowEvent.None;
                }

                var elapsedTimeMS = timer.ElapsedMilliseconds;
                timer.Restart();
                
                // Change game state.

                GameLogic(elapsedTimeMS);

                // Render.

                ClearScreen();
                RenderLogic(out string debugText);
                mainWnd.debugText.Text =
                    debugText +
                    "Mouse Captured: " + mainWnd.Capture + "\r\n" +
                    "";
                PresentNextFrame();
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
            bufferManager = new D3DBufferManager(device);

            adapter.Dispose();
            factory.Dispose();
        }
        private void LoadResources()
        {
            var vertexShaderByteCode = ShaderBytecode.CompileFromFile("Shader.fx", "VS", "vs_4_0");
            vertexShader = new VertexShader(device, vertexShaderByteCode);

            var signature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
            inputLayout = new InputLayout(device, signature, D3DVertex.InputFormat);

            resourceManager.LoadResources(device);
        }
        private void InitializeD3DPipeline(World world)
        {
            // Setup graphics pipeline

            var context = device.ImmediateContext;

            // 0. Prepare buffers

            world.UpdateVertexBuffer(bufferManager.GetVB());
            bufferManager.GetCB().Reset(new[] { Matrix.Identity });

            var vertexBuffer = bufferManager.GetVB().Buffer;
            var constantBuffer = bufferManager.GetCB().Buffer;

            // 1. IA, VS, PS stage: Prepare shaders, bind buffers

            context.InputAssembler.InputLayout = inputLayout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.PointList;
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(
                vertexBuffer,
                Utilities.SizeOf<D3DVertex>(),
                0));

            context.VertexShader.SetConstantBuffer(0, constantBuffer);
            context.VertexShader.Set(vertexShader);

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
        // Shaders.
        private InputLayout inputLayout;
        private VertexShader vertexShader;
        // Buffers.
        private D3DBufferManager bufferManager;
        // Shaders, textures.
        private ResourceManager resourceManager = new ResourceManager();
        // Views.
        private RenderTargetView renderTargetView;
        private DepthStencilView depthStencilView;

        // -------- DX 2D State --------

        private D2DDeviceContext d2dDeviceContext;
    }
}
