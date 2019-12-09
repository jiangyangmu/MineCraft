using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpDX;
using SharpDX.D3DCompiler;

namespace HLSLDebug
{
    class Program
    {
        static void Main(string[] args)
        {
            var vertexShaderByteCode = ShaderBytecode.CompileFromFile("Shader.hlsl", "VS", "vs_4_0");
            var vertexShaderDisassembly = new ShaderBytecode(vertexShaderByteCode).Disassemble();
            Console.Write("---- Vertex Shader Disassembly ----\r\n" + vertexShaderDisassembly);
        }
    }
}
