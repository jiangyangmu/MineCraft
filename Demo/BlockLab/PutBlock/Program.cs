using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using SharpDX.Direct2D1;
using D3DBuffer = SharpDX.Direct3D11.Buffer;
using D3DDevice = SharpDX.Direct3D11.Device;

namespace PutBlock
{
    static class Program
    {
        // Stats, Commands
        class GameState
        {
            public int FPS { get => (int)(totalFrame * 1000.0f / totalTimeMS); }

            public void BindControlInput(Control control)
            {
                control.MouseDown += (sender, args) =>
                {
                    switch (args.Button)
                    {
                        case MouseButtons.Left: Mine = true; break;
                        case MouseButtons.Right: Put = true; break;
                    }
                };
                control.KeyDown += (sender, args) =>
                {
                    switch (args.KeyCode)
                    {
                        case Keys.R: Shoot = true; break;
                        default: break;
                    }
                };
            }

            public bool Shoot { get; set; }
            public bool Mine { get; set; }
            public bool Put { get; set; }

            public void Update(float elapsedTimeMS)
            {
                if (totalTimeMS > 3000.0f)
                {
                    totalFrame /= 2;
                    totalTimeMS *= 0.5f;
                }
                ++totalFrame;
                totalTimeMS += elapsedTimeMS;
            }
            
            private float totalFrame = 0;
            private float totalTimeMS = 0.0f;
        }

        static void Main()
        {
            var world = new World();
            // world.AddBlockPlane(world.Origin, Block.Type.GRASS, 1, 1, 1, 1);
            world.AddBlockPlane(world.Origin + world.Up * 2, Block.Type.GRASS, 6, 6, 6, 6);
            world.AddBlockPlane(world.Origin + world.Up, Block.Type.GRASS, 8, 8, 8, 8);
            world.AddBlockPlane(world.Origin, Block.Type.GRASS, 10, 10, 10, 10);
            world.AddRay(Vector3.Zero, Vector3.UnitX, Vector3.UnitX);
            world.AddRay(Vector3.Zero, Vector3.UnitY, Vector3.UnitY);
            world.AddRay(Vector3.Zero, Vector3.UnitZ, Vector3.UnitZ);

            var camera = new Camera(
                pos: world.OriginF,
                up: world.UpF
                );

            var game = new Game();
            game.Initialize(world, camera);

            camera.AspectRatio = game.MainWindow.ClientSize.Width / (float)game.MainWindow.ClientSize.Height;

            var player = new PlayerController(world.OriginF + 5.0f * world.UpF);

            // Toolbar
            var _2dBlackBrush = new SolidColorBrush(game.D2DDeviceContext, Color.Black);
            var _2dWhiteBrush = new SolidColorBrush(game.D2DDeviceContext, Color.White);
            var _imageSource = D3DTextureLoader.LoadBitmap(new SharpDX.WIC.ImagingFactory2(), "GUI/ToolBarBox.jpg");
            var _2dImage = Bitmap.FromWicBitmap(
                        game.D2DDeviceContext,
                        _imageSource,
                        new BitmapProperties(
                            new PixelFormat(SharpDX.DXGI.Format.R8G8B8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied)
                            ));
            var _imageSource2 = D3DTextureLoader.LoadBitmap(new SharpDX.WIC.ImagingFactory2(), "GUI/ToolBarSelect.png");
            var _2dImage2 = Bitmap.FromWicBitmap(
                        game.D2DDeviceContext,
                        _imageSource2,
                        new BitmapProperties(
                            new PixelFormat(SharpDX.DXGI.Format.R8G8B8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied)
                            ));
            var _selected = 0;
            var _counter = 0;

            var _textFactory = new SharpDX.DirectWrite.Factory();
            var _textFormat = new SharpDX.DirectWrite.TextFormat(_textFactory, "Consolas", 30.0f);
            bool _resized = false;

            var gameState = new GameState();
            game.Start(
                ControlLogic: (Control mainWnd) =>
                {
                    gameState.BindControlInput(mainWnd);
                    camera.OrientationController.BindControlInput(mainWnd);
                    player.BindControlInput(mainWnd);
                    player.BindCamera(camera);

                    mainWnd.Resize += (sender, e) =>
                    {
                        camera.AspectRatio = mainWnd.ClientSize.Width / (float)mainWnd.ClientSize.Height;

                        _resized = true;
                    };
                },
                GameLogic: (float elapsedTimeMS) =>
                {
                    int ms = (int)(1000.0f * 1.7 / 120.0f - elapsedTimeMS);
                    Thread.Sleep(ms > 0 ? ms : 0);

                    player.Update(
                        elapsedTimeMS,
                        world.Ground(new Int3((int)player.Position.X, (int)player.Position.Y, (int)(player.Position.Z))));

                    gameState.Update(elapsedTimeMS);


                    if (gameState.Shoot)
                    {
                        gameState.Shoot = false;
                        world.AddRay(camera.Pos, camera.Orientation, Vector3.One);
                    }
                    else if (gameState.Mine)
                    {
                        gameState.Mine = false;
                        if (world.DoMineBlock())
                            ++_counter;
                    }
                    else if (gameState.Put)
                    {
                        gameState.Put = false;
                        if (_counter > 0 && world.DoPutBlock())
                            --_counter;
                    }
                    // Mark picked block as red.
                    world.PickTest(new Ray(camera.Pos, camera.Orientation), 5);

                    if (_resized)
                    {
                        _2dBlackBrush?.Dispose();
                        _2dWhiteBrush?.Dispose();
                        _2dImage?.Dispose();
                        _2dImage2?.Dispose();
                        _textFormat?.Dispose();

                        _2dBlackBrush = new SolidColorBrush(game.D2DDeviceContext, Color.Black);
                        _2dWhiteBrush = new SolidColorBrush(game.D2DDeviceContext, Color.White);
                        _2dImage = Bitmap.FromWicBitmap(
                            game.D2DDeviceContext,
                            _imageSource,
                            new BitmapProperties(
                                new PixelFormat(SharpDX.DXGI.Format.R8G8B8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied)
                                ));
                        _2dImage2 = Bitmap.FromWicBitmap(
                            game.D2DDeviceContext,
                            _imageSource2,
                            new BitmapProperties(
                                new PixelFormat(SharpDX.DXGI.Format.R8G8B8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied)
                                ));
                        _textFormat = new SharpDX.DirectWrite.TextFormat(_textFactory, "Consolas", 30.0f);
                    }
                },
                RenderLogic: (out string debugText) =>
                {
                    debugText =
                    "FPS: " + gameState.FPS + "\r\n" +
                    "Mouse Delta: " + (game.MainWindow as MainForm).AbsMousePosition + "\r\n" +
                    camera.DebugString +
                    world.DebugString +
                    player.DebugString +
                    "";

                    var matViewProj = camera.ViewMatrix * camera.ProjMatrix;
                    matViewProj.Transpose();

                    game.Context.UpdateSubresource(ref matViewProj, game.BufferManager.GetCB().Buffer);

                    world.UpdateVertexBuffer(game.BufferManager.GetVB());
                    world.CallDraws(game.Context);

                    var X = game.D2DDeviceContext.Size.Width;
                    var Y = game.D2DDeviceContext.Size.Height;

                    game.D2DDeviceContext.BeginDraw();

                    // Aim cross
                    game.D2DDeviceContext.FillRectangle(
                        new SharpDX.Mathematics.Interop.RawRectangleF(
                            X / 2 - 20, Y / 2 - 5,
                            X / 2 + 20, Y / 2 + 5),
                        _2dWhiteBrush);
                    game.D2DDeviceContext.FillRectangle(
                       new SharpDX.Mathematics.Interop.RawRectangleF(
                           X / 2 - 5, Y / 2 - 20,
                           X / 2 + 5, Y / 2 + 20),
                       _2dWhiteBrush);
                    // Toolbar
                    var x = _2dImage.Size.Width;
                    var y = _2dImage.Size.Height;
                    for (int i = 0; i <= 11; ++i)
                    {
                        float shift = i - 5.5f;
                        game.D2DDeviceContext.DrawImage(
                            _2dImage,
                            new SharpDX.Mathematics.Interop.RawVector2(
                                X / 2 - shift * x, Y - y)
                            );
                    }
                    var x2 = _2dImage2.Size.Width;
                    var y2 = _2dImage2.Size.Height;
                    game.D2DDeviceContext.DrawImage(
                        _2dImage2,
                        new SharpDX.Mathematics.Interop.RawVector2(
                            X / 2 - (_selected + 5.55f) * x, Y - y2)
                        );
                    // Toolbar items
                    var _counterStr = _counter.ToString();
                    game.D2DDeviceContext.DrawText(
                        _counterStr,
                        _textFormat,
                        new SharpDX.Mathematics.Interop.RawRectangleF(
                            X / 2 - (5.5f - 0.5f) * x - 0.3f * _textFormat.FontSize * _counterStr.Length, Y - 0.85f * y,
                            X / 2 - (4.5f - 0.5f) * x - 0.3f * _textFormat.FontSize * _counterStr.Length, Y
                            ),
                        _2dWhiteBrush
                        );

                    game.D2DDeviceContext.EndDraw();
                }
                );
        }
    }
}
