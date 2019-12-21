using System;
using System.Collections.Generic;

using SharpDX;
using SharpDX.Mathematics;

namespace PickBlock
{
    class Camera
    {
        public Camera(Vector3 pos, Vector3 up)
        {
            Up = up;
            Pos = pos;
            hBase = Vector3.UnitX;
            vBase = Vector3.Cross(up, hBase);
        }

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
        public Vector3 Pos { get => pos; set => pos = value; }
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
                hradian = value / 180.0f * (float)Math.PI;
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
                if (value < -90.0f) vradian = -90.0f / 180.0f * (float)Math.PI;
                else if (value > 90.0f) vradian = 90.0f / 180.0f * (float)Math.PI;
                else vradian = value / 180.0f * (float)Math.PI;
            }
        }

        public string DebugString
        {
            get =>
                "Cam Pos: " + Pos + "\r\n" +
                "Cam Ori: " + Orientation + "\r\n" +
                "Cam Forward: " + Forward + "\r\n" +
                "Cam Right: " + Right + "\r\n" +
                "Cam HAng: " + (hradian * 180.0f / (float)Math.PI) + "\r\n" +
                "Cam VAng: " + (vradian * 180.0f / (float)Math.PI) + "\r\n" +
                "";
        }

        // public void LookAt(Vector3 target);
        // public void LookDirection(Vector3 dir);
        public void MoveForward(float distance) { pos += Forward * distance; }
        public void MoveBackward(float distance) { pos -= Forward * distance; }
        public void MoveLeft(float distance) { pos -= Right * distance; }
        public void MoveRight(float distance) { pos += Right * distance; }
        public void MoveUp(float distance) { pos += Up * distance; }
        public void MoveDown(float distance) { pos -= Up * distance; }

        private Vector3 pos;
        private Vector3 hBase;
        private Vector3 vBase;
        private float hradian;
        private float vradian;

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

        
    }
}
