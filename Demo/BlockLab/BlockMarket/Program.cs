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

namespace BlockMarket
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

        static void HugeRandomCube(World world, BlockType type = BlockType.PUT, int size = 256, int maxBlockCount = 1000000)
        {
            var rand = new Random();
            var half = size / 2;
            for (int i = 0; i < maxBlockCount; ++i)
            {
                var x = (int)(rand.NextLong() % size - half);
                var y = (int)(rand.NextLong() % size - half);
                var z = (int)(rand.NextLong() % size - half);
                world.BlockManager.AddBlock(new Block(type, new Int3(x, y, z)));
            }
        }
        static void HugeBox(World world, BlockType type = BlockType.PUT, int size = 256, int maxBlockCount = 1000000)
        {
            int half = size / 2;
            for (int x = -half; x <= half; ++x)
            {
                for (int y = -half; y <= half; ++y)
                {
                    for (int z = -half; z <= half; ++z)
                    {
                        if (Math.Abs(x) >= half ||
                            Math.Abs(y) >= half ||
                            Math.Abs(z) >= half)
                        {
                            world.BlockManager.AddBlock(new Block(type, new Int3(x, y, z)));
                            --maxBlockCount;
                        }
                        if (maxBlockCount <= 0) break;
                    }
                    if (maxBlockCount <= 0) break;
                }
                if (maxBlockCount <= 0) break;
            }
        }
        static void BlockShow(World world)
        {
            world.BlockManager.AddBlockPlane(Block.Grass(world.Origin), 10, 10, 10, 10);
            world.BlockManager.AddBlockPlane(Block.OakWood(world.Origin + world.Up + world.Right * 0), 0, 0, 5, 5);
            world.BlockManager.AddBlockPlane(Block.OakLeaf(world.Origin + world.Up + world.Right * 1), 0, 0, 5, 5);
            world.BlockManager.AddBlockPlane(Block.Glass(world.Origin + world.Up + world.Right * 2), 0, 0, 5, 5);
            world.BlockManager.AddBlockPlane(Block.Sand(world.Origin + world.Up + world.Right * 3), 0, 0, 5, 5);
            world.BlockManager.AddBlockPlane(Block.Stone(world.Origin + world.Up + world.Right * 4), 0, 0, 5, 5);
            world.BlockManager.AddTree(world.Origin + world.Right * 8, 7);
            world.BlockManager.AddWaterPool(world.Origin + world.Right * -8 + world.Forward * -3 + world.Up, world.Origin + world.Right * -5 + world.Forward * 3 + world.Up * 4);
        }
        static void Main()
        {
            var world = new World();
            HugeRandomCube(world, BlockType.GRASS, 128, 1000000);
            //HugeBox(world, BlockType.GRASS, 256, 1000000);
            //BlockShow(world);
            world.AddRay(Vector3.Zero, Vector3.UnitX, Vector3.UnitX);
            world.AddRay(Vector3.Zero, Vector3.UnitY, Vector3.UnitY);
            world.AddRay(Vector3.Zero, Vector3.UnitZ, Vector3.UnitZ);

            var camera = new Camera(
                pos: world.OriginF + 5.0f * world.UpF + 15.0f * world.ForwardF * 15.0f * world.RightF,
                up: world.UpF
                );

            var game = new Game();
            game.Initialize(world);

            camera.AspectRatio = game.MainWindow.ClientSize.Width / (float)game.MainWindow.ClientSize.Height;

            var player = new PlayerController(camera.Pos);
            var inventory = new PlayerInventory();

            bool windowResized = false;
            var gui = new GUI();
            gui.LoadResources();
            gui.Reset(game.D2DDeviceContext);

            var gameState = new GameState();
            
            var deltaMS = 0.0f;
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
                    mainWnd.KeyDown += (sender, e) =>
                    {
                        switch (e.KeyCode)
                        {
                            case Keys.D1: inventory.SelectedItemIndex = 0; break;
                            case Keys.D2: inventory.SelectedItemIndex = 1; break;
                            case Keys.D3: inventory.SelectedItemIndex = 2; break;
                            case Keys.D4: inventory.SelectedItemIndex = 3; break;
                            case Keys.D5: inventory.SelectedItemIndex = 4; break;
                            case Keys.D6: inventory.SelectedItemIndex = 5; break;
                            case Keys.D7: inventory.SelectedItemIndex = 6; break;
                            case Keys.D8: inventory.SelectedItemIndex = 7; break;
                            case Keys.D9: inventory.SelectedItemIndex = 8; break;
                            default: break;
                        }
                    };
                },
                GameLogic: (float elapsedTimeMS) =>
                {
                    //int ms = (int)(1000.0f * 1.7 / 60.0f - elapsedTimeMS);
                    //Thread.Sleep(ms > 0 ? ms : 0);

                    // Update player state

                    float ground = world.Ground(new Int3((int)player.Position.X, (int)player.Position.Y, (int)(player.Position.Z)));
                    float buoyancy = 0.0f;
                    if (world.InLiquid(new Int3((int)player.Position.X, (int)player.Position.Y, (int)(camera.Pos.Z - 2.0f))))
                    {
                        buoyancy = 8.8f;
                    }
                    player.Update(
                        elapsedTimeMS,
                        ground,
                        buoyancy);

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
                        if (world.DoMineBlock(out ItemType type))
                            inventory.AddItem(type);
                    }
                    else if (gameState.Put)
                    {
                        gameState.Put = false;
                        if (inventory.SelectedCount > 0 && world.DoPutBlock(inventory.SelectedItem))
                            inventory.RemoveItem(inventory.SelectedItem);
                    }

                    // Mark picked block as red.
                    // world.PickTest(new Ray(camera.Pos, camera.Orientation), 5);

                    deltaMS += elapsedTimeMS;
                    if (deltaMS > 100.0f)
                    {
                        world.UpdateLiquidTexture();
                        deltaMS -= 100.0f;
                    }
                },
                RenderLogic: (out string debugText) =>
                {
                    debugText =
                    "FPS: " + gameState.FPS + "\r\n" +
                    //"Mouse Delta: " + game.MainWindow.AbsMousePosition + "\r\n" +
                    //camera.DebugString +
                    world.DebugString +
                    //player.DebugString +
                    "";

                    // D3D Render

                    var matViewProj = camera.ViewMatrix * camera.ProjMatrix;
                    matViewProj.Transpose();

                    game.Context.UpdateSubresource(ref matViewProj, game.BufferManager.GetCB().Buffer);
                    // world.UpdateVertexBuffer(game.BufferManager.GetVB());
                    world.Render(game.Context, game.ResourceManager);

                    // D2D Render

                    if (windowResized)
                    {
                        gui.Reset(game.D2DDeviceContext);
                    }
                    game.D2DDeviceContext.BeginDraw();

                    gui.DrawAimCross();
                    gui.DrawToolBar(
                        11,
                        inventory.SelectedItemIndex,
                        inventory.GetItemNameList(),
                        inventory.GetItemCountList());

                    game.D2DDeviceContext.EndDraw();
                }
                );
        }
    }
}
