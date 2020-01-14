using System;
using System.Collections.Generic;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using D3DBuffer = SharpDX.Direct3D11.Buffer;
using D3DDevice = SharpDX.Direct3D11.Device;
using D2DRenderTarget = SharpDX.Direct2D1.RenderTarget;
using D2DDeviceContext = SharpDX.Direct2D1.DeviceContext;

namespace BlockMarket
{
    public enum RasterizerStateType
    {
        Default,
        NoCulling,
    }

    class D3DDynamicVertexBuffer : IDisposable
    {
        public D3DDynamicVertexBuffer(D3DDevice device)
        {
            this.device = device;
            this.context = device.ImmediateContext;
            bufferSizeInBytes = 0;
        }

        public D3DBuffer Buffer { get => buffer; }

        public void Reset<T>(T[] vertexData) where T : struct
        {
            var vertexSize = Utilities.SizeOf<T>();
            if (bufferSizeInBytes < (vertexData.Length * vertexSize))
            {
                buffer?.Dispose();

                bufferSizeInBytes = vertexData.Length * vertexSize;
                buffer = CreateDynamicVertexBuffer(device, vertexData, bufferSizeInBytes);

                context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(
                    buffer,
                    vertexSize,
                    0));
            }
            else
            {
                context.MapSubresource(buffer, 0, MapMode.WriteDiscard, 0, out DataStream stream);
                stream.WriteRange(vertexData);
                stream.Dispose();
                context.UnmapSubresource(buffer, 0);
            }
        }
        public void Dispose()
        {
            buffer?.Dispose();
        }

        private D3DDevice device;
        private DeviceContext context;
        private D3DBuffer buffer;
        private int bufferSizeInBytes;

        private static D3DBuffer CreateDynamicVertexBuffer<T>(
            D3DDevice device,
            T[] vertexData,
            int bufferSizeInBytes) where T : struct
        {
            var vertexDataSize = vertexData.Length * Utilities.SizeOf<T>();
            if (vertexDataSize % 16 != 0)
            {
                throw new ArgumentException("Vertex data size must be multiply of 16.");
            }
            if (bufferSizeInBytes < vertexDataSize)
            {
                throw new ArgumentException("Buffer size must be greater than vertex data size.");
            }

            var vertexBufferDesc = new BufferDescription()
            {
                /* Usage */
                Usage = ResourceUsage.Dynamic,
                /* ByteWidth */
                SizeInBytes = bufferSizeInBytes,
                /* BindFlags */
                BindFlags = BindFlags.VertexBuffer,
                /* CPUAccessFlags */
                CpuAccessFlags = CpuAccessFlags.Write,
                /* MiscFlags */
                OptionFlags = ResourceOptionFlags.None,
                /* StructureByteStride */
                StructureByteStride = 0,
            };

            var vertexBuffer = D3DBuffer.Create(device, vertexData, vertexBufferDesc);
            return vertexBuffer;
        }
    }
    class D3DConstantBuffer : IDisposable
    {
        public D3DConstantBuffer(D3DDevice device)
        {
            this.device = device;
            this.context = device.ImmediateContext;
            this.bufferSizeInBytes = 0;
        }

        public D3DBuffer Buffer { get => buffer; }

        public void Reset<T>(T[] bufferData) where T : struct
        {
            if (bufferSizeInBytes < (bufferData.Length * Utilities.SizeOf<T>()))
            {
                buffer?.Dispose();

                bufferSizeInBytes = bufferData.Length * Utilities.SizeOf<T>();
                buffer = CreateConstantBuffer(device, bufferData, bufferSizeInBytes);
            }
            else
            {
                context.UpdateSubresource(bufferData, buffer);
            }
        }
        public void Dispose()
        {
            buffer?.Dispose();
        }

        private D3DDevice device;
        private DeviceContext context;
        private D3DBuffer buffer;
        private int bufferSizeInBytes;

        private static D3DBuffer CreateConstantBuffer<T>(
            D3DDevice device,
            T[] constData,
            int bufferSizeInBytes) where T : struct
        {
            var constDataSize = constData.Length * Utilities.SizeOf<T>();
            if (bufferSizeInBytes < constDataSize)
            {
                throw new ArgumentException("Buffer size must be greater than data size.");
            }

            var constBufferDesc = new BufferDescription()
            {
                /* Usage */
                Usage = ResourceUsage.Default,
                /* ByteWidth */
                SizeInBytes = bufferSizeInBytes,
                /* BindFlags */
                BindFlags = BindFlags.ConstantBuffer,
                /* CPUAccessFlags */
                CpuAccessFlags = CpuAccessFlags.None,
                /* MiscFlags */
                OptionFlags = ResourceOptionFlags.None,
                /* StructureByteStride */
                StructureByteStride = 0,
            };

            var constBuffer = D3DBuffer.Create(device, constData, constBufferDesc);
            return constBuffer;
        }
    }

    // DO
    //  - Manage Buffer/Texture/Shader/Pipeline-State resources.
    class D3DResourceManager : IDisposable
    {
        public D3DResourceManager(D3DDevice device)
        {
            this.device = device;
        }

        // ---- Operations ----

        // Buffer accessors.
        public D3DDynamicVertexBuffer GetVB()
        {
            if (vb == null)
            {
                vb = new D3DDynamicVertexBuffer(device);
            }
            return vb;
        }
        public D3DConstantBuffer GetCB()
        {
            if (cb == null)
            {
                cb = new D3DConstantBuffer(device);
            }
            return cb;
        }
        // Shader accessors.
        public VertexShader GetDefaultVSShader()
        {
            return vertexShaderDict["VS"].vertexShader;
        }
        public ShaderBytecode GetDefaultVSShaderByteCode()
        {
            return vertexShaderDict["VS"].vertexShaderByteCode;
        }
        public PixelShader GetDefaultPSShader()
        {
            return pixelShaderDict["PS_Texture_Block"];
        }
        public PixelShader GetPSShader(string name)
        {
            return pixelShaderDict[name];
        }
        // Texture accessors.
        public ShaderResourceView GetDefaultTextureSRV()
        {
            return textureSRVDict["Grass"];
        }
        public ShaderResourceView GetTextureSRV(string name)
        {
            return textureSRVDict[name];
        }
        // Pipeline state accessors.
        public BlendState GetDefaultBlendState()
        {
            return blendStateDict["Default"];
        }
        public BlendState GetBlendState(string name)
        {
            return blendStateDict[name];
        }
        public RasterizerState GetRasterizerState(RasterizerStateType type)
        {
            return rasterizerStateDict[type];
        }
        public DepthStencilState GetDefaultDepthStencilState()
        {
            return depthStencilState;
        }

        public void LoadResources()
        {
            // Shaders

            var vertexShaderNames = new[]
            {
                "VS",
            };
            foreach (var shaderName in vertexShaderNames)
            {
                var vertexShaderByteCode = ShaderBytecode.CompileFromFile("Shader.fx", shaderName, "vs_4_0");
                vertexShaderDict.Add(shaderName, new VertexShaderInfo(vertexShaderByteCode, new VertexShader(device, vertexShaderByteCode)));
            }

            var pixelShaderNames = new[]
            {
                "PS_Texture_Block",
                "PS_Transparent_Block",
                "PS_Liquid_Block",
                "PS_Line_Block",
            };
            foreach (var shaderName in pixelShaderNames)
            {
                var pixelShaderByteCode = ShaderBytecode.CompileFromFile("Shader.fx", shaderName, "ps_4_0");
                pixelShaderDict.Add(shaderName, new PixelShader(device, pixelShaderByteCode));
            }

            // Textures

            var textureNameAndImages = new string[][]
            {
                new string[]{ "Grass", "Resources/Texture/Grass.png" },
                new string[]{ "Sand", "Resources/Texture/Sand.jpg" },
                new string[]{ "Stone", "Resources/Texture/Stone.jpg" },
                new string[]{ "OakWood", "Resources/Texture/OakWood.jpg" },
                new string[]{ "OakLeaf", "Resources/Texture/OakLeaf.png" },
                new string[]{ "Glass", "Resources/Texture/Glass.png" },
                new string[]{ "Water", "Resources/Texture/Water.jpg" },
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

            // States

            var alphaBSDesc = new BlendStateDescription()
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false,
            };
            alphaBSDesc.RenderTarget[0] = new RenderTargetBlendDescription()
            {
                IsBlendEnabled = true,
                SourceBlend = BlendOption.SourceAlpha,
                DestinationBlend = BlendOption.InverseSourceAlpha,
                BlendOperation = BlendOperation.Add,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                AlphaBlendOperation = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            };
            var alphaBS = new BlendState(device, alphaBSDesc);
            var defaultBS = new BlendState(device, BlendStateDescription.Default());

            blendStateDict.Add("Transparent", alphaBS);
            blendStateDict.Add("Default", defaultBS);

            var defaultRSDesc = RasterizerStateDescription.Default();
            var noCullingRSDesc = new RasterizerStateDescription()
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.None,
                IsFrontCounterClockwise = defaultRSDesc.IsFrontCounterClockwise,
                DepthBias = defaultRSDesc.DepthBias,
                SlopeScaledDepthBias = defaultRSDesc.SlopeScaledDepthBias,
                IsDepthClipEnabled = defaultRSDesc.IsDepthClipEnabled,
                IsScissorEnabled = defaultRSDesc.IsScissorEnabled,
                IsMultisampleEnabled = defaultRSDesc.IsMultisampleEnabled,
                IsAntialiasedLineEnabled = defaultRSDesc.IsAntialiasedLineEnabled,
            };
            var noCullingRS = new RasterizerState(device, noCullingRSDesc);

            rasterizerStateDict.Add(RasterizerStateType.Default, device.ImmediateContext.Rasterizer.State);
            rasterizerStateDict.Add(RasterizerStateType.NoCulling, noCullingRS);

            var depthStencilStateDesc = new DepthStencilStateDescription()
            {
                IsDepthEnabled = true,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.Less,
                IsStencilEnabled = false,
                StencilReadMask = 0xff,
                StencilWriteMask = 0xff,
                FrontFace = new DepthStencilOperationDescription()
                {
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Increment,
                    PassOperation = StencilOperation.Keep,
                    Comparison = Comparison.Always,
                },
                BackFace = new DepthStencilOperationDescription()
                {
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Decrement,
                    PassOperation = StencilOperation.Keep,
                    Comparison = Comparison.Always,
                }
            };
            depthStencilState = new DepthStencilState(device, depthStencilStateDesc);
        }
        public void Dispose()
        {
            vb?.Dispose();
            cb?.Dispose();
        }

        // ---- Implementation ----

        private class VertexShaderInfo
        {
            public ShaderBytecode vertexShaderByteCode;
            public VertexShader vertexShader;

            public VertexShaderInfo(CompilationResult vertexShaderByteCode, VertexShader vertexShader)
            {
                this.vertexShaderByteCode = vertexShaderByteCode;
                this.vertexShader = vertexShader;
            }
        }

        private D3DDevice device;

        // Buffers.
        private D3DDynamicVertexBuffer vb;
        private D3DConstantBuffer cb;
        // Textures.
        private Dictionary<string, ShaderResourceView> textureSRVDict = new Dictionary<string, ShaderResourceView>();
        // Shaders.
        private Dictionary<string, VertexShaderInfo> vertexShaderDict = new Dictionary<string, VertexShaderInfo>();
        private Dictionary<string, PixelShader> pixelShaderDict = new Dictionary<string, PixelShader>();
        // Pipeline states.
        private Dictionary<string, BlendState> blendStateDict = new Dictionary<string, BlendState>();
        private Dictionary<RasterizerStateType, RasterizerState> rasterizerStateDict = new Dictionary<RasterizerStateType, RasterizerState>();
        private DepthStencilState depthStencilState;
    }

}
