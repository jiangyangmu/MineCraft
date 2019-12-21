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
using D3DBuffer = SharpDX.Direct3D11.Buffer;
using D3DDevice = SharpDX.Direct3D11.Device;

namespace PickBlock
{
    static class Program
    {
        class GameState
        {
            public int FPS { get => (int)(totalFrame * 1000.0f / totalTimeMS); }

            public void OnMouseMove(int x, int y, int screenWidth, int screenHeight)
            {
                mousePos.X = (float)x / screenWidth;
                mousePos.Y = (float)y / screenHeight;
            }
            public void OnMouseDown(MouseButtons button)
            {
                if (button == MouseButtons.Left)
                    command["mine!"] = true;
            }
            public void OnKeyDown(Keys keyCode)
            {
                switch (keyCode)
                {
                    case Keys.W: keyMap['w'] = true; break;
                    case Keys.S: keyMap['s'] = true; break;
                    case Keys.A: keyMap['a'] = true; break;
                    case Keys.D: keyMap['d'] = true; break;
                    case Keys.Q: keyMap['q'] = true; break;
                    case Keys.E: keyMap['e'] = true; break;
                    case Keys.R: command["shoot!"] = true; break;
                    case Keys.Space: upVelocity = 8.0f; break;
                    default: break;
                }
            }
            public void OnKeyUp(Keys keyCode)
            {
                switch (keyCode)
                {
                    case Keys.W: keyMap['w'] = false; break;
                    case Keys.S: keyMap['s'] = false; break;
                    case Keys.A: keyMap['a'] = false; break;
                    case Keys.D: keyMap['d'] = false; break;
                    case Keys.Q: keyMap['q'] = false; break;
                    case Keys.E: keyMap['e'] = false; break;
                    default: break;
                }
            }

            public void UpdateFPS(float elapsedTimeMS)
            {
                if (totalTimeMS > 3000.0f)
                {
                    totalFrame /= 2;
                    totalTimeMS *= 0.5f;
                }
                ++totalFrame;
                totalTimeMS += elapsedTimeMS;
            }
            public void UpdatePosition(float elapsedTimeMS, Camera camera)
            {
                float moveFactor = (elapsedTimeMS / 1000.0f) * 10.0f;
                float jumpFactor = (elapsedTimeMS / 1000.0f) * 4.0f;

                // horizontal
                if (keyMap['w']) camera.MoveForward(moveFactor);
                if (keyMap['s']) camera.MoveBackward(moveFactor);
                if (keyMap['a']) camera.MoveLeft(moveFactor);
                if (keyMap['d']) camera.MoveRight(moveFactor);

                // vertical
                // var ground = world.Has
                if (altitude > 0.0f || (altitude == 0.0f && upVelocity != 0.0f))
                {
                    camera.MoveUp(upVelocity * jumpFactor);
                    altitude += upVelocity * jumpFactor;
                    upVelocity += -9.8f * jumpFactor;
                }
                else
                {
                    camera.MoveUp(-altitude);
                    altitude = upVelocity = 0.0f;
                }
            }
            public void UpdateEye(World world, ref Camera camera)
            {
                // update WVP matrix
                // var proj = Matrix.OrthoLH(camera.AspectRatio * 10, 10, 0.1f, 10.0f);
                var proj = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, camera.AspectRatio, 0.1f, 100.0f);
                var view = camera.ViewMatrix;

                var worldMat = Matrix.Identity;// Matrix.Translation(world.UpF * -altitude);
                worldViewProj = worldMat * view * proj;
                worldViewProj.Transpose();

                // update camera
                camera.HorizontalAngle = (mousePos.X) * 360.0f * 2.0f;
                camera.VerticalAngle = (0.5f - mousePos.Y) * 180.0f;
            }
            
            public Matrix worldViewProj = Matrix.Identity;
            public Dictionary<char, bool> keyMap = new Dictionary<char, bool>()
            {
                { 'w' , false },
                { 's' , false },
                { 'a' , false },
                { 'd' , false },
                { 'q' , false },
                { 'e' , false },
            };
            public Dictionary<string, bool> command = new Dictionary<string, bool>()
            {
                { "shoot!", false },
                { "mine!", false },
            };
            public Vector2 mousePos = Vector2.Zero;
            public float altitude = 0.0f;
            public float upVelocity = 0.0f;

            public float totalFrame = 0;
            public float totalTimeMS = 0.0f;
        }

        static void Main()
        {
            var world = new World();
            world.AddBlockPlane(world.Origin + world.Up * 2, Block.Type.GRASS, 6, 6, 6, 6);
            world.AddBlockPlane(world.Origin + world.Up, Block.Type.GRASS, 8, 8, 8, 8);
            world.AddBlockPlane(world.Origin, Block.Type.GRASS, 10, 10, 10, 10);
            world.AddRay(Vector3.Zero, Vector3.UnitX, Vector3.UnitX);
            world.AddRay(Vector3.Zero, Vector3.UnitY, Vector3.UnitY);
            world.AddRay(Vector3.Zero, Vector3.UnitZ, Vector3.UnitZ);

            var camera = new Camera(
                pos: world.OriginF + 5.0f * world.UpF,
                up: world.UpF
                );

            var gameState = new GameState();
            var game = new Game();
            game.Initialize(world, camera);

            camera.AspectRatio = game.MainWindow.Width / (float)game.MainWindow.Height;

            game.Start(
                ControlLogic: (Control mainWnd) =>
                {
                    mainWnd.MouseDown += (sender, e) =>
                    {
                        gameState.OnMouseDown(e.Button);
                    };
                    mainWnd.MouseMove += (sender, e) =>
                    {
                        gameState.OnMouseMove(e.X, e.Y, mainWnd.ClientSize.Width, mainWnd.ClientSize.Height);
                    };
                    mainWnd.KeyDown += (sender, e) =>
                    {
                        gameState.OnKeyDown(e.KeyCode);
                    };
                    mainWnd.KeyUp += (sender, e) =>
                    {
                        gameState.OnKeyUp(e.KeyCode);
                    };
                },
                GameLogic: (float elapsedTimeMS) =>
                {
                    int ms = (int)(1000.0f * 1.7 / 120.0f - elapsedTimeMS);
                    Thread.Sleep(ms > 0 ? ms : 0);

                    gameState.UpdateFPS(elapsedTimeMS);
                    gameState.UpdatePosition(elapsedTimeMS, camera);
                    gameState.UpdateEye(world, ref camera);
                },
                RenderLogic: (out string debugText) =>
                {
                    debugText =
                    "FPS: " + gameState.FPS + "\r\n" +
                    "Mouse: " + gameState.mousePos + "\r\n" + 
                    camera.DebugString +
                    "Dirty frame: " + world.NumDirtyFrame + "\r\n" +
                    "Block: " + world.NumBlock + "\r\n" +
                    "Ray: " + world.NumRay + "\r\n";

                    game.Context.UpdateSubresource(ref gameState.worldViewProj, game.BufferManager.GetCB(DXBufferType.CONSTANT).Buffer);

                    if (gameState.command["shoot!"])
                    {
                        gameState.command["shoot!"] = false;
                        world.AddRay(camera.Pos, camera.Orientation, Vector3.One);
                    }
                    else if (gameState.command["mine!"])
                    {
                        gameState.command["mine!"] = false;
                        world.RemovePickedBlock();
                    }
                    world.PickTest(new Ray(camera.Pos, camera.Orientation));

                    world.UpdateVertexBuffer(game.BufferManager.GetVB(DXBufferType.TRIANGLES));
                    world.CallDraws(game.Context);
                }
                );
        }
    }
}
