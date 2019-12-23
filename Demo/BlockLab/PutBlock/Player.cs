using System;
using System.Windows.Forms;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using D3DBuffer = SharpDX.Direct3D11.Buffer;
using D3DDevice = SharpDX.Direct3D11.Device;

namespace PutBlock
{
    class PlayerEventArgs
    {
        public PlayerEventArgs(Vector3 position)
        {
            Position = position;
        }

        public Vector3 Position { get; }
    }

    // Player + Controller binding
    class PlayerController
    {
        // Initialization
        public PlayerController(Vector3 position)
        {
            this.position = position;
        }

        public void BindControlInput(Control control)
        {
            // mouse, keyboard events => commands
            control.KeyDown += (sender, args) =>
            {
                switch (args.KeyCode)
                {
                    case Keys.W: moveWeight.X = 1.0f; break;
                    case Keys.S: moveWeight.X = -1.0f; break;
                    case Keys.A: moveWeight.Y = 1.0f; break;
                    case Keys.D: moveWeight.Y = -1.0f; break;
                    case Keys.Space: jumpVelocity = 15.0f; break;
                    default: break;
                }
            };
            control.KeyUp += (sender, args) =>
            {
                switch (args.KeyCode)
                {
                    case Keys.W: if (moveWeight.X == 1.0f) moveWeight.X = 0.0f; break;
                    case Keys.S: if (moveWeight.X == -1.0f) moveWeight.X = 0.0f; break;
                    case Keys.A: if (moveWeight.Y == 1.0f) moveWeight.Y = 0.0f; break;
                    case Keys.D: if (moveWeight.Y == -1.0f) moveWeight.Y = 0.0f; break;
                    default: break;
                }
            };
        }
        public void BindCamera(Camera camera)
        {
            // camera.orientationChanged => move direction
            // position changed => camera.UpdatePosition
            camera.OrientationChange += (sender, args) =>
            {
                var dir = args.Orientation;
                dir.Z = 0;
                dir.Normalize();
                forwardDir = dir;
                leftDir = Vector3.Cross(forwardDir, Vector3.UnitZ);
            };
            PositionChange += (sender, args) =>
            {
                var eye = args.Position;
                eye.Z += HEIGHT;
                camera.SetPosition(eye);
            };
        }

        // Events

        public delegate void PlayerEventHandler(object sender, PlayerEventArgs e);
        public event PlayerEventHandler PositionChange;
        private void DispatchEvents()
        {
            PositionChange?.Invoke(this, new PlayerEventArgs(Position));
        }

        // Properties

        public readonly float HEIGHT = 3.0f;
        private readonly float MOVE_SPEED = 10.0f;
        private readonly float JUMP_SPEED = 5.0f;
        public Vector3 Position { get => position; }

        public string DebugString
        {
            get
            {
                return
                    "======== Player ========\r\n" +
                    "Pos: " + position + "\r\n" +
                    "MoveX: " + moveWeight.X + "\r\n" +
                    "MoveY: " + moveWeight.Y + "\r\n" +
                    "Fwd: " + forwardDir + "\r\n" +
                    "Left: " + leftDir + "\r\n" +
                    "JumpV: " + jumpVelocity + "\r\n" +
                    "";
            }
        }

        // Operations

        // Per frame
        public void Update(float elapsedMS, float ground)
        {
            var delta = Vector3.Zero;
            delta += forwardDir * moveWeight.X * MOVE_SPEED * elapsedMS / 1000.0f;
            delta += leftDir * moveWeight.Y * MOVE_SPEED * elapsedMS / 1000.0f;
            delta += upDir * jumpVelocity * elapsedMS / 1000.0f;

            if (position.Z != ground)
            {
                jumpVelocity += -9.8f * JUMP_SPEED * elapsedMS / 1000.0f; // gravity
            }

            if (!delta.IsZero)
            {
                position += delta;
                position.Z = (position.Z < ground) ? ground : position.Z;
                DispatchEvents();
            }
        }

        private Vector3 position;
        private Vector2 moveWeight = Vector2.Zero; // (forward/backward, left/right)
        private float   jumpVelocity = 0.0f;
        private Vector3 forwardDir = Vector3.UnitX;
        private Vector3 leftDir = -Vector3.UnitY;
        private Vector3 upDir = Vector3.UnitZ;
    }
}
