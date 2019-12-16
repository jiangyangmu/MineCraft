using System;
using System.Windows.Forms;
using System.Diagnostics;

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
    static class Program
    {
        
        static void Main()
        {
            string info = "";
            try
            {
                InitializeDirectX(out Form mainWnd, out D3DDevice device, out SwapChain swapChain);

                // Possible combination
                // Create
                //  {Immutable, Default, Dynamic, Staging} x {none, read, write, read_write}
                // CPU side operations
                //  {read, write, read_write, write_discard, write_no_override}
                Tuple<string, ResourceUsage>[] usageList =
                {
                    new Tuple<string, ResourceUsage>("Default", ResourceUsage.Default),
                    new Tuple<string, ResourceUsage>("Dynamic", ResourceUsage.Dynamic),
                    new Tuple<string, ResourceUsage>("Immutable", ResourceUsage.Immutable),
                    new Tuple<string, ResourceUsage>("Staging", ResourceUsage.Staging),
                };
                Tuple<string, CpuAccessFlags>[] cpuFlagList =
                {
                    new Tuple<string, CpuAccessFlags>("None", CpuAccessFlags.None),
                    new Tuple<string, CpuAccessFlags>("Read", CpuAccessFlags.Read),
                    new Tuple<string, CpuAccessFlags>("Write", CpuAccessFlags.Write),
                    new Tuple<string, CpuAccessFlags>("RW", CpuAccessFlags.Read | CpuAccessFlags.Write),
                };

                // Correct combination
                // Create
                //  1. Default + none
                //  2. Dynamic + write
                //  3. Immutable + none
                // CPU side operations
                //  2. write_discard
                foreach (var usage in usageList)
                {
                    foreach (var cpuFlag in cpuFlagList)
                    {
                        info += "----------- " + usage.Item1 + " + " + cpuFlag.Item1 + " -----------\r\n";
                        try
                        {
                            var vertexData = new[] { new Vector4(1, 2, 3, 4) };
                            var vertexData2 = new[] { new Vector4(11, 22, 33, 44) };
                            info += "Create:\r\n";
                            var vertexBuffer = CreateVertexBuffer(ref device, vertexData, usage.Item2, cpuFlag.Item2);
                            info += "  Success.\r\n";
                            var context = device.ImmediateContext;
                            info += ProfileBuffer(ref context, ref vertexBuffer, vertexData2);
                            vertexBuffer.Dispose();
                        }
                        catch (Exception e)
                        {
                            info += e.Message;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                info += "----------- Global Exception -----------\r\n";
                info += e.ToString();
            }
            finally
            {
                var mb = new ScrollableMessageBox();
                mb.showText.Text = info;
                mb.Show();
                Application.Run(mb);
            }
        }

        static D3DBuffer CreateVertexBuffer<T>(ref D3DDevice device, T[] vertexData, ResourceUsage resourceUsage, CpuAccessFlags cpuAccessFlags) where T : struct
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
        static D3DBuffer CreateConstantBuffer<T>(ref D3DDevice device, T[] constantData) where T : struct
        {
            var constantDataSize = constantData.Length * Utilities.SizeOf<T>();

            var constantBufferDesc = new BufferDescription()
            {
                /* Usage */
                Usage = ResourceUsage.Default,
                /* ByteWidth */
                SizeInBytes = constantDataSize,
                /* BindFlags */
                BindFlags = BindFlags.ConstantBuffer,
                /* CPUAccessFlags */
                CpuAccessFlags = CpuAccessFlags.None,
                /* MiscFlags */
                OptionFlags = ResourceOptionFlags.None,
                /* StructureByteStride */
                StructureByteStride = 0,
            };

            var constantBuffer = D3DBuffer.Create(device, constantData, constantBufferDesc);
            return constantBuffer;
        }
        static string ProfileBuffer<T>(ref DeviceContext context, ref D3DBuffer buffer, T[] writeData) where T : struct
        {
            string info = "";

            Tuple<string, MapMode> modeList;
            
            info += "Map-Read Test:\r\n";
            try
            {
                context.MapSubresource(buffer, MapMode.Read, /* wait if busy */0, out DataStream readStream);
                info += "  length: " + readStream.Length + "\r\n";
                info += "  read: " + readStream.CanRead + "\r\n";
                info += "  write: " + readStream.CanWrite + "\r\n";
                info += "  seek: " + readStream.CanSeek + "\r\n";
                if (readStream.CanRead)
                {
                    info += "  Content:\r\n";
                    while (readStream.RemainingLength > 0)
                        info += "  " + (readStream.Length - readStream.RemainingLength) + ":" + readStream.Read<T>().ToString() + "\r\n";
                }
                context.UnmapSubresource(buffer, 0);
                readStream.Dispose();
            }
            catch (Exception e)
            {
                info += "  exception: " + e.Message + "\r\n";
            }
            
            info += "Map-Write Test:\r\n";
            try
            {
                bool readBack = false;
                context.MapSubresource(buffer, MapMode.Write, /* wait if busy */0, out DataStream writeStream);
                info += "  length: " + writeStream.Length + "\r\n";
                info += "  read: " + writeStream.CanRead + "\r\n";
                info += "  write: " + writeStream.CanWrite + "\r\n";
                info += "  seek: " + writeStream.CanSeek + "\r\n";
                if (writeStream.CanWrite)
                {
                    writeStream.WriteRange<T>(writeData);
                    readBack = true;
                }
                context.UnmapSubresource(buffer, 0);
                writeStream.Dispose();
                if (readBack)
                {
                    context.MapSubresource(buffer, MapMode.Read, 0, out DataStream readStream);
                    if (readStream.CanRead)
                    {
                        info += "  Content After Write:\r\n";
                        while (readStream.RemainingLength > 0)
                            info += "  " + (readStream.Length - readStream.RemainingLength) + ":" + readStream.Read<T>().ToString() + "\r\n";
                    }
                    context.UnmapSubresource(buffer, 0);
                    readStream.Dispose();
                }
            }
            catch (Exception e)
            {
                info += "  exception: " + e.Message + "\r\n";
            }

            info += "Map-RW Test:\r\n";
            try
            {
                bool readBack = false;
                context.MapSubresource(buffer, MapMode.ReadWrite, /* wait if busy */0, out DataStream writeStream);
                info += "  length: " + writeStream.Length + "\r\n";
                info += "  read: " + writeStream.CanRead + "\r\n";
                info += "  write: " + writeStream.CanWrite + "\r\n";
                info += "  seek: " + writeStream.CanSeek + "\r\n";
                if (writeStream.CanRead)
                {
                    info += "  Read Content:\r\n";
                    while (writeStream.RemainingLength > 0)
                        info += "  " + (writeStream.Length - writeStream.RemainingLength) + ":" + writeStream.Read<T>().ToString() + "\r\n";
                }
                if (writeStream.CanWrite)
                {
                    writeStream.WriteRange<T>(writeData);
                    readBack = true;
                }
                context.UnmapSubresource(buffer, 0);
                writeStream.Dispose();
                if (readBack)
                {
                    context.MapSubresource(buffer, MapMode.Read, 0, out DataStream readStream);
                    if (readStream.CanRead)
                    {
                        info += "  Content After Write:\r\n";
                        while (readStream.RemainingLength > 0)
                            info += "  " + (readStream.Length - readStream.RemainingLength) + ":" + readStream.Read<T>().ToString() + "\r\n";
                    }
                    context.UnmapSubresource(buffer, 0);
                    readStream.Dispose();
                }
            }
            catch (Exception e)
            {
                info += "  exception: " + e.Message + "\r\n";
            }

            info += "Map-Write-Discard Test:\r\n";
            try
            {
                bool readBack = false;
                context.MapSubresource(buffer, MapMode.WriteDiscard, /* wait if busy */0, out DataStream writeStream);
                info += "  length: " + writeStream.Length + "\r\n";
                info += "  read: " + writeStream.CanRead + "\r\n";
                info += "  write: " + writeStream.CanWrite + "\r\n";
                info += "  seek: " + writeStream.CanSeek + "\r\n";
                if (writeStream.CanWrite)
                {
                    writeStream.WriteRange<T>(writeData);
                    readBack = true;
                }
                context.UnmapSubresource(buffer, 0);
                writeStream.Dispose();
                if (readBack)
                {
                    context.MapSubresource(buffer, MapMode.Read, 0, out DataStream readStream);
                    if (readStream.CanRead)
                    {
                        info += "  Content After Write:\r\n";
                        while (readStream.RemainingLength > 0)
                            info += "  " + (readStream.Length - readStream.RemainingLength) + ":" + readStream.Read<T>().ToString() + "\r\n";
                    }
                    context.UnmapSubresource(buffer, 0);
                    readStream.Dispose();
                }
            }
            catch (Exception e)
            {
                info += "  exception: " + e.Message + "\r\n";
            }
            return info;
        }

        static void InitializeDirectX(out Form mainWnd, out D3DDevice device, out SwapChain swapChain)
        {
            mainWnd = new Form1();

            var bufferDesc = new ModeDescription(Format.Unknown)
            {
                Width = mainWnd.ClientSize.Width,
                Height = mainWnd.ClientSize.Height,
                RefreshRate = new Rational(60, 1),
                Format = Format.R8G8B8A8_UNorm,
                ScanlineOrdering = DisplayModeScanlineOrder.Unspecified,
                Scaling = DisplayModeScaling.Unspecified
            };
            var sampleDesc = new SampleDescription(0, 0)
            {
                Count = 1,
                Quality = 0
            };
            var bufferUsage = Usage.RenderTargetOutput;
            int bufferCount = 1;
            var swapEffect = SwapEffect.Discard;

            var swapChainDesc = new SwapChainDescription()
            {
                ModeDescription = bufferDesc,
                SampleDescription = sampleDesc,
                Usage = bufferUsage,
                BufferCount = bufferCount,
                OutputHandle = mainWnd.Handle,
                IsWindowed = true,
                SwapEffect = swapEffect,
            };

            var factory = new Factory1();
            // 0: Intel 1: Nvidia 2: CPU
            var adapter = factory.Adapters1[1];
            device = new D3DDevice(adapter);
            swapChain = new SwapChain(factory, device, swapChainDesc);
            MessageBox.Show(adapter.Description.Description);
            //D3DDevice.CreateWithSwapChain(
            //    DriverType.Hardware,
            //    DeviceCreationFlags.None,
            //    swapChainDesc,
            //    out device,
            //    out swapChain);
        }
    }
}
