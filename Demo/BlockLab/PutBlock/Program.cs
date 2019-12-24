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
            world.AddBlockPlane(Block.Type.GRASS, world.Origin + world.Up * 2, 6, 6, 6, 6);
            world.AddBlockPlane(Block.Type.GRASS, world.Origin + world.Up, 8, 8, 8, 8);
            world.AddBlockPlane(Block.Type.GRASS, world.Origin, 10, 10, 10, 10);
            world.AddRay(Vector3.Zero, Vector3.UnitX, Vector3.UnitX);
            world.AddRay(Vector3.Zero, Vector3.UnitY, Vector3.UnitY);
            world.AddRay(Vector3.Zero, Vector3.UnitZ, Vector3.UnitZ);

            var camera = new Camera(
                pos: world.OriginF,
                up: world.UpF
                );

            var game = new Game();
            game.Initialize(world);

            camera.AspectRatio = game.MainWindow.ClientSize.Width / (float)game.MainWindow.ClientSize.Height;

            var player = new PlayerController(world.OriginF + 5.0f * world.UpF);

            bool windowResized = false;
            var gui = new GUI();
            gui.LoadResources();
            gui.Reset(game.D2DDeviceContext);

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

                        windowResized = true;
                    };
                },
                GameLogic: (float elapsedTimeMS) =>
                {
                    int ms = (int)(1000.0f * 1.7 / 120.0f - elapsedTimeMS);
                    Thread.Sleep(ms > 0 ? ms : 0);

                    // Update player state

                    player.Update(
                        elapsedTimeMS,
                        world.Ground(new Int3((int)player.Position.X, (int)player.Position.Y, (int)(player.Position.Z))));

                    // Update general game state

                    gameState.Update(elapsedTimeMS);

                    // Process user command

                    if (gameState.Shoot)
                    {
                        gameState.Shoot = false;
                        world.AddRay(camera.Pos, camera.Orientation, Vector3.One);
                    }
                    else if (gameState.Mine)
                    {
                        gameState.Mine = false;
                        if (world.DoMineBlock())
                            ++gui.InventoryBlockCount;
                    }
                    else if (gameState.Put)
                    {
                        gameState.Put = false;
                        if (gui.InventoryBlockCount > 0 && world.DoPutBlock())
                            --gui.InventoryBlockCount;
                    }

                    // Mark picked block as red.
                    world.PickTest(new Ray(camera.Pos, camera.Orientation), 5);
                },
                RenderLogic: (out string debugText) =>
                {
                    debugText =
                    "FPS: " + gameState.FPS + "\r\n" +
                    "Mouse Delta: " + game.MainWindow.AbsMousePosition + "\r\n" +
                    camera.DebugString +
                    world.DebugString +
                    player.DebugString +
                    "";

                    // D3D Render

                    var matViewProj = camera.ViewMatrix * camera.ProjMatrix;
                    matViewProj.Transpose();

                    game.Context.UpdateSubresource(ref matViewProj, game.BufferManager.GetCB().Buffer);

                    world.UpdateVertexBuffer(game.BufferManager.GetVB());
                    world.CallDraws(game.Context);

                    // D2D Render

                    if (windowResized)
                    {
                        gui.Reset(game.D2DDeviceContext);
                    }
                    game.D2DDeviceContext.BeginDraw();

                    gui.DrawAimCross();
                    gui.DrawToolBar(11, 0, gui.InventoryBlockCount);

                    game.D2DDeviceContext.EndDraw();
                }
                );
        }
    }
}
