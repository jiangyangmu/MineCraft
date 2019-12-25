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

namespace BlockMarket
{
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

    class D3DBufferManager : IDisposable
    {
        public D3DBufferManager(D3DDevice device)
        {
            this.device = device;
        }

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
        public void Dispose()
        {
            vb?.Dispose();
            cb?.Dispose();
        }

        private D3DDevice device;
        private D3DDynamicVertexBuffer vb;
        private D3DConstantBuffer cb;
    }
}
