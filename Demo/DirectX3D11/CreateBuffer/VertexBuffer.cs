using System;
using System.Collections.Generic;

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
    class DynamicVertexBuffer : IDisposable
    {
        public DynamicVertexBuffer(ref D3DDevice device)
        {
            Device = device;
            Context = device.ImmediateContext;
        }

        public void Init<T>(T[] data) where T : struct
        {
            if (Buffer != null)
            {
                Buffer.Dispose();
            }
            Buffer = CreateVertexBuffer(Device, data, ResourceUsage.Dynamic, CpuAccessFlags.Write);
        }
        public void Update<T>(T[] data) where T : struct
        {
            Context.MapSubresource(Buffer, 0, MapMode.WriteDiscard, 0, out DataStream stream);
            stream.WriteRange(data);
            stream.Dispose();
            Context.UnmapSubresource(Buffer, 0);
        }

        public D3DDevice Device { get; }
        public DeviceContext Context { get; }
        public D3DBuffer Buffer { set; get; }

        private static D3DBuffer CreateVertexBuffer<T>(D3DDevice device, T[] vertexData, ResourceUsage resourceUsage, CpuAccessFlags cpuAccessFlags) where T : struct
        {
            var vertexDataSize = vertexData.Length * Utilities.SizeOf<T>();
            if (vertexDataSize % 16 != 0)
            {
                throw new ArgumentException("vertex data size is not multiply of 16.");
            }

            var vertexBufferDesc = new BufferDescription()
            {
                /* Usage */
                Usage = resourceUsage,
                /* ByteWidth */
                SizeInBytes = vertexDataSize,
                /* BindFlags */
                BindFlags = BindFlags.VertexBuffer,
                /* CPUAccessFlags */
                CpuAccessFlags = cpuAccessFlags,
                /* MiscFlags */
                OptionFlags = ResourceOptionFlags.None,
                /* StructureByteStride */
                StructureByteStride = 0,
            };

            var vertexBuffer = D3DBuffer.Create(device, vertexData, vertexBufferDesc);
            return vertexBuffer;
        }

        public void Dispose()
        {
            Buffer.Dispose();
        }
    }
}
