using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpDX;
using SharpDX.Direct2D1;
using BitmapSource = SharpDX.WIC.BitmapSource;
using D2DBitmap = SharpDX.Direct2D1.Bitmap;
using D2DDeviceContext = SharpDX.Direct2D1.DeviceContext;

namespace PutBlock
{

    class GUI
    {
        public GUI()
        {
        }

        public void LoadResources()
        {
            bsToolBarBox = D3DTextureLoader.LoadBitmap(new SharpDX.WIC.ImagingFactory2(), "GUI/ToolBarBox.jpg");
            bsToolBarSelect = D3DTextureLoader.LoadBitmap(new SharpDX.WIC.ImagingFactory2(), "GUI/ToolBarSelect.png");

            var textFactory = new SharpDX.DirectWrite.Factory();
            textFormat = new SharpDX.DirectWrite.TextFormat(textFactory, "Consolas", 30.0f);
            textFactory.Dispose();
        }

        public void Reset(D2DDeviceContext d2dContext)
        {
            blackBrush?.Dispose();
            whiteBrush?.Dispose();
            bmToolBarBox?.Dispose();
            bmToolBarSelect?.Dispose();
            // textFormat?.Dispose();

            d2dDeviceContext = d2dContext;

            blackBrush = new SolidColorBrush(d2dContext, Color.Black);
            whiteBrush = new SolidColorBrush(d2dContext, Color.White);

            bmToolBarBox = D2DBitmap.FromWicBitmap(
                       d2dContext,
                       bsToolBarBox,
                       new BitmapProperties(
                           new PixelFormat(SharpDX.DXGI.Format.R8G8B8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied)
                           ));
            bmToolBarSelect = D2DBitmap.FromWicBitmap(
                       d2dContext,
                       bsToolBarSelect,
                       new BitmapProperties(
                           new PixelFormat(SharpDX.DXGI.Format.R8G8B8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied)
                           ));
        }

        public void DrawAimCross()
        {
            int X = d2dDeviceContext.PixelSize.Width;
            int Y = d2dDeviceContext.PixelSize.Height;

            // Aim cross
            d2dDeviceContext.FillRectangle(
                new SharpDX.Mathematics.Interop.RawRectangleF(
                    X / 2 - 20, Y / 2 - 5,
                    X / 2 + 20, Y / 2 + 5),
                whiteBrush);
            d2dDeviceContext.FillRectangle(
               new SharpDX.Mathematics.Interop.RawRectangleF(
                   X / 2 - 5, Y / 2 - 20,
                   X / 2 + 5, Y / 2 + 20),
               whiteBrush);
        }
        public void DrawToolBar(int totalSlot, int selectedSlot, int itemCount)
        {
            int X = d2dDeviceContext.PixelSize.Width;
            int Y = d2dDeviceContext.PixelSize.Height;

            // Toolbar
            var x = bmToolBarBox.Size.Width;
            var y = bmToolBarBox.Size.Height;
            for (int i = 0; i < totalSlot; ++i)
            {
                float shift = i - (totalSlot * 0.5f - 1.0f);
                d2dDeviceContext.DrawImage(
                    bmToolBarBox,
                    new SharpDX.Mathematics.Interop.RawVector2(
                        X / 2 - shift * x, Y - y)
                    );
            }

            var x2 = bmToolBarSelect.Size.Width;
            var y2 = bmToolBarSelect.Size.Height;
            d2dDeviceContext.DrawImage(
                bmToolBarSelect,
                new SharpDX.Mathematics.Interop.RawVector2(
                    X / 2 - (totalSlot * 0.5f - selectedSlot + 0.05f) * x, Y - y2)
                );

            // Toolbar items
            var itemCountStr = itemCount.ToString();
            d2dDeviceContext.DrawText(
                itemCountStr,
                textFormat,
                new SharpDX.Mathematics.Interop.RawRectangleF(
                    X / 2 - (totalSlot * 0.5f - 0.5f) * x - 0.3f * textFormat.FontSize * itemCountStr.Length, Y - 0.85f * y,
                    X / 2 - (totalSlot * 0.5f - 1.0f - 0.5f) * x - 0.3f * textFormat.FontSize * itemCountStr.Length, Y
                    ),
                whiteBrush
                );
        }

        private D2DDeviceContext d2dDeviceContext;

        private SharpDX.DirectWrite.TextFormat textFormat;
        private BitmapSource bsToolBarBox;
        private BitmapSource bsToolBarSelect;
        // Change with D2DDeviceContext
        private Brush blackBrush;
        private Brush whiteBrush;
        private D2DBitmap bmToolBarBox;
        private D2DBitmap bmToolBarSelect;

        // Inventory
        public int InventoryBlockCount { get; set; }
    }

}
