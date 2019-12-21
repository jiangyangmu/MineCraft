using System;
using System.Windows.Forms;

using SharpDX.DXGI;
using D3DBuffer = SharpDX.Direct3D11.Buffer;
using D3DDevice = SharpDX.Direct3D11.Device;

namespace PickBlock
{
    class GPUMemoryTest
    {
        public static void Run()
        {
            var factory = new Factory1();
            // 0: Intel 1: Nvidia 2: CPU
            var adapter = factory.Adapters1[1];
            var device = new D3DDevice(adapter);

            var gpuName = adapter.Description.Description;

            var _64MB = new byte[64 * 1024 * 1024];
            var buffer = new DXDynamicVertexBuffer(device);
            for (int i = 64; i <= 2048; i += 64)
            {
                buffer.Reset(_64MB);
                var result = MessageBox.Show(i + " MB! Flush?", "Memory Test - " + gpuName, MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Yes) device.ImmediateContext.Flush();
                else if (result == DialogResult.No) continue;
                else break;
            }

            buffer.Dispose();
            device.Dispose();
            adapter.Dispose();
            factory.Dispose();
        }
    }
}
