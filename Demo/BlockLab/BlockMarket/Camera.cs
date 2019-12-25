using System;
using System.Collections.Generic;
using System.Windows.Forms;

using SharpDX;
using SharpDX.Mathematics;

namespace BlockMarket
{
    class CameraEventArgs
    {
        public CameraEventArgs(Vector3 ori)
        {
            Orientation = ori;
        }

        public Vector3 Orientation { get; }
    }

    class CameraOrientationController
    {
        public CameraOrientationController(Camera camera)
        {
            this.camera = camera;
        }

        public void BindControlInput(Control newControl)
        {
            if (control != null)
            {
                control.MouseMove -= OnMouseMove;
            }
            control = newControl;
            control.MouseMove += OnMouseMove;
        }

        private void OnMouseMove(object sender, MouseEventArgs args)
        {
            var wnd = sender as MainForm;

            if (!pt.HasValue)
            {
                pt = wnd.AbsMousePosition;
            }
            else
            {
                hANg += 0.2f * (wnd.AbsMousePosition.X - pt.Value.X);
                vAng -= 0.2f * (wnd.AbsMousePosition.Y - pt.Value.Y);
                vAng = Math.Max(-90.0f, Math.Min(90.0f, vAng));
                pt = wnd.AbsMousePosition;
            }

            camera.HorizontalAngle = hANg;
            camera.VerticalAngle = vAng;
        }

        private Control control;
        private Camera camera;
        public System.Drawing.Point? pt;
        public float hANg = 0;
        public float vAng = -30.0f;
    }

    class Camera
    {
        public Camera(Vector3 pos, Vector3 up)
        {
            Up = up;
            this.pos = pos;
            hBase = Vector3.UnitX;
            vBase = Vector3.Cross(up, hBase);
            orientationController = new CameraOrientationController(this);
        }

        // Properties

        public Vector3 Up { get; }
        public Vector3 Forward
        {
            get
            {
                var m = Matrix.RotationAxis(Up, hradian);
                m.Transpose();
                var ori = new Vector3(
                   m.M11 * hBase.X + m.M12 * hBase.Y + m.M13 * hBase.Z,
                   m.M21 * hBase.X + m.M22 * hBase.Y + m.M23 * hBase.Z,
                   0);
                ori.Normalize();
                return ori;
            }
        }
        public Vector3 Right { get => Vector3.Cross(Up, Forward); }
        public Vector3 Pos { get => pos; }
        public Vector3 Orientation
        {
            get
            {
                var m = Matrix.RotationAxis(Up, hradian) * Matrix.RotationAxis(Right, -vradian);
                m.Transpose();
                var ori = new Vector3(
                   m.M11 * hBase.X + m.M12 * hBase.Y + m.M13 * hBase.Z,
                   m.M21 * hBase.X + m.M22 * hBase.Y + m.M23 * hBase.Z,
                   m.M31 * hBase.X + m.M32 * hBase.Y + m.M33 * hBase.Z);
                ori.Normalize();
                return ori;
            }
        }

        public float AspectRatio { get; set; }
        // Left: 0.0f, Right: 360.0f
        public float HorizontalAngle
        {
            get
            {
                return hradian;
            }
            set
            {
                //if (value < -180.0f) hradian = -(float)Math.PI;
                //else if (value > 180.0f) hradian = (float)Math.PI;
                //else hradian = value / 180.0f * (float)Math.PI;
                var hradian2 = value / 180.0f * (float)Math.PI;
                if (hradian2 != hradian)
                {
                    hradian = hradian2;
                    DispatchEvents();
                }
            }
        }
        // Down: -90.0f, Up: 90.0f
        public float VerticalAngle
        {
            get
            {
                return vradian;
            }
            set
            {
                float vradian2;
                if (value < -90.0f) vradian2 = -90.0f / 180.0f * (float)Math.PI;
                else if (value > 90.0f) vradian2 = 90.0f / 180.0f * (float)Math.PI;
                else vradian2 = value / 180.0f * (float)Math.PI;
                if (vradian2 != vradian)
                {
                    vradian = vradian2;
                    DispatchEvents();
                }
            }
        }

        public CameraOrientationController OrientationController { get => orientationController; }

        public string DebugString
        {
            get =>
                "======== Camera ========\r\n" +
                "DX: " + orientationController.hANg + "\r\n" +
                "DY: " + orientationController.vAng + "\r\n" +
                "Pos: " + Pos + "\r\n" +
                "Ori: " + Orientation + "\r\n" +
                "Forward: " + Forward + "\r\n" +
                "Right: " + Right + "\r\n" +
                "HAng: " + (hradian * 180.0f / (float)Math.PI) + "\r\n" +
                "VAng: " + (vradian * 180.0f / (float)Math.PI) + "\r\n" +
                "";
        }

        // Events

        public delegate void CameraEventHandler(object sender, CameraEventArgs e);
        public event CameraEventHandler OrientationChange;
        private void DispatchEvents()
        {
            OrientationChange?.Invoke(this, new CameraEventArgs(Orientation));
        }

        // Operations

        public void SetPosition(Vector3 pos)
        {
            this.pos = pos;
        }

        // Implementation

        private Vector3 pos;
        private readonly Vector3 hBase;
        private readonly Vector3 vBase;
        private float hradian;
        private float vradian;
        private CameraOrientationController orientationController;

        public Matrix ViewMatrix
        {
            get
            {
                return Matrix.Identity
                    * Matrix.Translation(-Pos)
                    * Matrix.RotationAxis(Up, -hradian)
                    * Matrix.Translation(Pos)
                    * Matrix.LookAtLH(Pos, Pos + hBase, Up)
                    * Matrix.RotationAxis(-hBase, -vradian)
                    ;
            }
        }

        public Matrix ProjMatrix
        {
            // Matrix.OrthoLH(camera.AspectRatio * 10, 10, 0.1f, 10.0f);
            get => Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, AspectRatio, 0.1f, 100.0f);
        }
    }

}
