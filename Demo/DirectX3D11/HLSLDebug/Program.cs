using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

namespace HLSLDebug
{
    class Program
    {
        static void Main(string[] args)
        {
            var vertexShaderByteCode = ShaderBytecode.CompileFromFile("Shader.hlsl", "VS", "vs_4_0");
            var vertexShaderDisassembly = new ShaderBytecode(vertexShaderByteCode).Disassemble();
            Console.Write("\r\n\r\n########## Vertex Shader Disassembly ##########\r\n" + vertexShaderDisassembly);

            var pixelShaderByteCode = ShaderBytecode.CompileFromFile("Shader.hlsl", "PS", "ps_4_0");
            var pixelShaderDisassembly = new ShaderBytecode(pixelShaderByteCode).Disassemble();
            Console.Write("\r\n\r\n########## Pixel Shader Disassembly ##########\r\n" + pixelShaderDisassembly);

            var shaderByteCode = ShaderBytecode.CompileFromFile("Shader.hlsl", "fx_4_0").Bytecode;
            // var shaderReflection = new ShaderReflection(shaderByteCode.Data);
            //var variable = shaderReflection.GetVariable("gTextureMap");
            Console.Write("\r\n\r\n########## Shader Disassembly ##########\r\n" + shaderByteCode.Disassemble());
        }
    }
}
