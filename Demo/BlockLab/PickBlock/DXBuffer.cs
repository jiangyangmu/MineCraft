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

namespace PickBlock
{
    enum DXBufferType
    {
        TRIANGLES,
        CONSTANT,
    }

    class DXDynamicVertexBuffer : IDisposable
    {
        public DXDynamicVertexBuffer(D3DDevice device)
        {
            this.device = device;
            this.context = device.ImmediateContext;
            bufferSizeInBytes = 0;
        }

        public D3DBuffer Buffer { get => buffer; }

        public void Create<T>(T[] bufferData) where T : struct
        {
            if (buffer != null)
            {
                throw new InvalidOperationException("Can't create twice.");
            }
            bufferSizeInBytes = bufferData.Length * Utilities.SizeOf<T>();
            buffer = CreateDynamicVertexBuffer(device, bufferData, bufferSizeInBytes);
        }
        public void Update<T>(T[] bufferData) where T : struct
        {
            if (buffer == null)
            {
                throw new InvalidOperationException("Call Create() first.");
            }
            if (bufferSizeInBytes < (bufferData.Length * Utilities.SizeOf<T>()))
            {
                throw new InvalidOperationException("Data size mismatch.");
            }

            context.MapSubresource(buffer, 0, MapMode.WriteDiscard, 0, out DataStream stream);
            stream.WriteRange(bufferData);
            stream.Dispose();
            context.UnmapSubresource(buffer, 0);
        }
        public void Dispose()
        {
            buffer.Dispose();
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
    class DXConstantBuffer : IDisposable
    {
        public DXConstantBuffer(D3DDevice device)
        {
            this.device = device;
            this.context = device.ImmediateContext;
            this.bufferSizeInBytes = 0;
        }

        public D3DBuffer Buffer { get => buffer; }

        public void Create<T>(T[] bufferData) where T : struct
        {
            if (buffer != null)
            {
                throw new InvalidOperationException("Can't create twice.");
            }
            bufferSizeInBytes = bufferData.Length * Utilities.SizeOf<T>();
            buffer = CreateConstantBuffer(device, bufferData, bufferSizeInBytes);
        }
        public void Update<T>(T[] bufferData) where T : struct
        {
            if (buffer == null)
            {
                throw new InvalidOperationException("Call Create() first.");
            }
            if (bufferSizeInBytes != (bufferData.Length * Utilities.SizeOf<T>()))
            {
                throw new InvalidOperationException("Data size mismatch.");
            }

            context.UpdateSubresource(bufferData, buffer);
        }
        public void Dispose()
        {
            buffer.Dispose();
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

    class DXBufferManager : IDisposable
    {
        public DXBufferManager(D3DDevice device)
        {
            this.device = device;
            this.context = device.ImmediateContext;
        }

        public DXDynamicVertexBuffer GetVB(DXBufferType type)
        {
            if (vb == null)
            {
                vb = new DXDynamicVertexBuffer(device);
            }
            return vb;
        }
        public DXConstantBuffer GetCB(DXBufferType type)
        {
            if (cb == null)
            {
                cb = new DXConstantBuffer(device);
            }
            return cb;
        }
        public void Dispose()
        {
            if (vb != null)
            {
                vb.Dispose();
            }
            if (cb != null)
            {
                cb.Dispose();
            }
        }

        private D3DDevice device;
        private DeviceContext context;
        private DXDynamicVertexBuffer vb;
        private DXConstantBuffer cb;
        // private Dictionary<DXBufferType, DXDynamicVertexBuffer> dynamicVBs;
    }
}
