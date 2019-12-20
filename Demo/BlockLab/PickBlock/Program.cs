using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;

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
            public float FPS { get => (totalFrame * 1000.0f / totalTimeMS); }
            public Vector3 POS { get => new Vector3(movePos.X, movePos.Y, altitude); }

            public void OnMouseMove(int x, int y, int screenWidth, int screenHeight)
            {
                mousePosX = (float)x / screenWidth;
                mousePosY = (float)y / screenHeight;
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
                    case Keys.R: removeOneBlock = true; break;
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
                if (totalTimeMS > 1000.0f)
                {
                    totalFrame = 0;
                    totalTimeMS = 0.0f;
                }
                ++totalFrame;
                totalTimeMS += elapsedTimeMS;
            }
            public void UpdatePosition(float elapsedTimeMS, Vector3 forward, Vector3 right)
            {
                float moveFactor = (elapsedTimeMS / 1000.0f) * 10.0f;
                float jumpFactor = (elapsedTimeMS / 1000.0f) * 4.0f;

                // horizontal
                if (keyMap['w']) movePos += forward * moveFactor;
                if (keyMap['s']) movePos -= forward * moveFactor;
                if (keyMap['a']) movePos -= right * moveFactor;
                if (keyMap['d']) movePos += right * moveFactor;

                // vertical
                if (altitude > 0.0f || (altitude == 0.0f && upVelocity != 0.0f))
                {
                    altitude += upVelocity * jumpFactor;
                    upVelocity += -9.8f * jumpFactor;
                }
                else
                {
                    altitude = upVelocity = 0.0f;
                }
            }
            public void UpdateEye(World world, ref Camera camera)
            {
                // update WVP matrix
                var proj = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, camera.AspectRatio, 0.1f, 100.0f);
                var view = camera.ViewMatrix;
                var worldMat = Matrix.Translation(world.UpF * -altitude) * Matrix.Translation(movePos);
                worldViewProj = worldMat * view * proj;
                worldViewProj.Transpose();

                // update camera
                camera.HorizontalAngle = (mousePosX - 0.5f) * 360.0f;
                camera.VerticalAngle = (0.5f - mousePosY) * 180.0f;
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
            public Vector3 movePos = new Vector3(0, 0, 0);
            public bool removeOneBlock = false;
            public float mousePosX = 0.0f;
            public float mousePosY = 0.0f;
            public float altitude = 0.0f;
            public float upVelocity = 0.0f;
            public float totalFrame = 0;
            public float totalTimeMS = 0.0f;
        }
        static void Main()
        {
            var world = new World();
            world.AddBlockPlane(world.Origin, Block.Type.GRASS, 10, 10, 10, 10);

            var camera = new Camera();
            camera.Eye = world.OriginF + 5.0f * world.UpF;
            camera.Up = world.UpF;
            camera.Forward = new Vector3(0.0f, 1.0f, 0.0f);
            camera.HorizontalAngle = 0.0f;
            camera.VerticalAngle = 0.0f;

            var gameState = new GameState();
            var game = new Game();
            game.Initialize(world, camera);

            camera.AspectRatio = game.MainWindow.Width / (float)game.MainWindow.Height;

            game.Start(
                ControlLogic: (Control mainWnd) =>
                {
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
                    gameState.UpdateFPS(elapsedTimeMS);
                    gameState.UpdatePosition(elapsedTimeMS, camera.Orientation, camera.OrientationRight);
                    gameState.UpdateEye(world, ref camera);
                },
                RenderLogic: (out string debugText) =>
                {
                    debugText =
                    "FPS: " + gameState.FPS + "\r\n" +
                    "Pos: " + gameState.POS + "\r\n" +
                    "Ori: " + camera.Orientation.ToString() + "\r\n";

                    game.Context.UpdateSubresource(ref gameState.worldViewProj, game.BufferManager.GetCB(DXBufferType.CONSTANT).Buffer);

                    if (gameState.removeOneBlock)
                    {
                        gameState.removeOneBlock = false;
                        if (world.RemoveRandomBlock())
                        {
                            game.BufferManager.GetVB(DXBufferType.TRIANGLES).Update(world.GetVertices());
                        }
                    }
                    var vertexCount = world.BlockCount * Block.VERTEX_COUNT;

                    game.Context.Draw(vertexCount, 0);
                }
                );
        }
    }
}
