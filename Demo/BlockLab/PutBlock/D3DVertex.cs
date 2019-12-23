using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace PutBlock
{
    struct D3DVertex
    {
        public Vector3 pos;
        public Vector3 col;
        public Vector2 tex;

        public D3DVertex(Vector3 pos, Vector3 col)
        {
            this.pos = pos;
            this.col = col;
            this.tex = Vector2.Zero;
        }
        public D3DVertex(Vector3 pos, Vector3 col, Vector2 tex)
        {
            this.pos = pos;
            this.col = col;
            this.tex = tex;
        }

        public static InputElement[] InputFormat
        {
            get => new InputElement[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
                new InputElement("COLOR", 0, Format.R32G32B32_Float, 12, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, 24, 0),
            };
        }
    }

}
