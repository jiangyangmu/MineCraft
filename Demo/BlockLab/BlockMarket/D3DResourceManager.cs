using System;
using System.Collections.Generic;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using D3DBuffer = SharpDX.Direct3D11.Buffer;
using D3DDevice = SharpDX.Direct3D11.Device;
using D2DRenderTarget = SharpDX.Direct2D1.RenderTarget;
using D2DDeviceContext = SharpDX.Direct2D1.DeviceContext;

namespace BlockMarket
{
    class D3DResourceManager
    {
        public D3DResourceManager(D3DDevice device)
        {
            this.device = device;
        }

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
        public ShaderResourceView GetDefaultTextureSRV()
        {
            return textureSRVDict["Grass"];
        }
        public BlendState GetDefaultBlendState()
        {
            return blendStateDict["Default"];
        }

        public PixelShader GetPSShader(string name)
        {
            return pixelShaderDict[name];
        }
        public ShaderResourceView GetTextureSRV(string name)
        {
            return textureSRVDict[name];
        }

        public BlendState GetBlendState(string name)
        {
            return blendStateDict[name];
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
                "PS_Texture_Alpha_Block",
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
                new string[]{ "Grass", "Texture/Grass.png" },
                new string[]{ "Sand", "Texture/Sand.jpg" },
                new string[]{ "Stone", "Texture/Stone.jpg" },
                new string[]{ "OakWood", "Texture/OakWood.jpg" },
                new string[]{ "OakLeaf", "Texture/OakLeaf.png" },
                new string[]{ "Glass", "Texture/Glass.png" },
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
        }

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
        private Dictionary<string, VertexShaderInfo> vertexShaderDict = new Dictionary<string, VertexShaderInfo>();
        private Dictionary<string, PixelShader> pixelShaderDict = new Dictionary<string, PixelShader>();
        private Dictionary<string, ShaderResourceView> textureSRVDict = new Dictionary<string, ShaderResourceView>();
        private Dictionary<string, BlendState> blendStateDict = new Dictionary<string, BlendState>();
    }
}
