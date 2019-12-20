using System;
using System.Collections.Generic;

using SharpDX;
using SharpDX.Mathematics;

namespace PickBlock
{
    class Camera
    {
        public Vector3 Eye { get; set; }
        public Vector3 Up { get; set; }
        public Vector3 Forward { get; set; }

        public float AspectRatio { get; set; }

        public Vector3 Orientation
        {
            get
            {
                var oriMatrix = Matrix.RotationAxis(Up, -hradian);
                var ori = new Vector3(
                    oriMatrix.M11 * Forward.X + oriMatrix.M12 * Forward.Y,
                    oriMatrix.M21 * Forward.X + oriMatrix.M22 * Forward.Y,
                    0);
                ori.Normalize();
                return -ori;
            }
        }
        public Vector3 OrientationRight
        {
            get
            {
                var orir = Vector3.Cross(Up, Orientation);
                orir.Normalize();
                return orir;
            }
        }

        // Left: -180.0f, Right: 180.0f
        public float HorizontalAngle
        {
            get
            {
                return hradian;
            }
            set
            {
                if (value < -180.0f)        hradian = -(float)Math.PI;
                else if (value > 180.0f)    hradian = (float)Math.PI;
                else                        hradian = value / 180.0f * (float)Math.PI;
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
                if (value < -90.0f)         vradian = -90.0f / 180.0f * (float)Math.PI;
                else if (value > 90.0f)     vradian = 90.0f / 180.0f * (float)Math.PI;
                else                        vradian = value / 180.0f * (float)Math.PI;
            }
        }

        private float hradian;
        private float vradian;

        public Matrix ViewMatrix
        {
            get
            {
                var right = Vector3.Cross(Up, Forward);
                return Matrix.RotationAxis(Up, -hradian) * Matrix.RotationAxis(right, vradian) * Matrix.LookAtLH(Eye, Eye + Forward, Up);
            }
        }
    }
}
