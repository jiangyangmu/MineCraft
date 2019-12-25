using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace BlockMarket
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

        public static int GetBlockVertexCount(PrimitiveTopology topology)
        {
            if (topology == PrimitiveTopology.TriangleList)
                return 36;
            else if (topology == PrimitiveTopology.LineList)
                return 24;
            else
                throw new ArgumentException("Unsupported topology: " + topology);
        }
        public static D3DVertex[] GetBlockVertices(
            PrimitiveTopology topology,
            Vector3 unitFront, // X
            Vector3 unitRight, // Y
            Vector3 unitUp, // Z
            Vector3 pos, Vector3 color, float size)
        {
            D3DVertex[] v = new D3DVertex[0];

            float half_size = size * 0.5f;
            if (topology == PrimitiveTopology.TriangleList)
            {
                v = new[]
                {   
                    // Up
                    new D3DVertex( pos + half_size * (- unitFront - unitRight + unitUp), color, new Vector2(0.5f, 1)),
                    new D3DVertex( pos + half_size * (+ unitFront + unitRight + unitUp), color, new Vector2(1, 0)),
                    new D3DVertex( pos + half_size * (- unitFront + unitRight + unitUp), color, new Vector2(0.5f, 0)),
                    new D3DVertex( pos + half_size * (- unitFront - unitRight + unitUp), color, new Vector2(0.5f, 1)),
                    new D3DVertex( pos + half_size * (+ unitFront - unitRight + unitUp), color, new Vector2(1, 1)),
                    new D3DVertex( pos + half_size * (+ unitFront + unitRight + unitUp), color, new Vector2(1, 0)),

                    // Down
                    new D3DVertex( pos + half_size * (- unitFront - unitRight - unitUp), color, new Vector2(0.5f, 1)),
                    new D3DVertex( pos + half_size * (- unitFront + unitRight - unitUp), color, new Vector2(0.5f, 0)),
                    new D3DVertex( pos + half_size * (+ unitFront + unitRight - unitUp), color, new Vector2(1, 0)),
                    new D3DVertex( pos + half_size * (- unitFront - unitRight - unitUp), color, new Vector2(0.5f, 1)),
                    new D3DVertex( pos + half_size * (+ unitFront + unitRight - unitUp), color, new Vector2(1, 0)),
                    new D3DVertex( pos + half_size * (+ unitFront - unitRight - unitUp), color, new Vector2(1, 1)),

                    // Front
                    new D3DVertex( pos + half_size * (+ unitFront - unitRight - unitUp), color, new Vector2(0, 1)),
                    new D3DVertex( pos + half_size * (+ unitFront + unitRight + unitUp), color, new Vector2(0.5f, 0)),
                    new D3DVertex( pos + half_size * (+ unitFront - unitRight + unitUp), color, new Vector2(0, 0)),
                    new D3DVertex( pos + half_size * (+ unitFront - unitRight - unitUp), color, new Vector2(0, 1)),
                    new D3DVertex( pos + half_size * (+ unitFront + unitRight - unitUp), color, new Vector2(0.5f, 1)),
                    new D3DVertex( pos + half_size * (+ unitFront + unitRight + unitUp), color, new Vector2(0.5f, 0)),

                    // Back
                    new D3DVertex( pos + half_size * (- unitFront - unitRight - unitUp), color, new Vector2(0, 1)),
                    new D3DVertex( pos + half_size * (- unitFront - unitRight + unitUp), color, new Vector2(0, 0)),
                    new D3DVertex( pos + half_size * (- unitFront + unitRight + unitUp), color, new Vector2(0.5f, 0)),
                    new D3DVertex( pos + half_size * (- unitFront - unitRight - unitUp), color, new Vector2(0, 1)),
                    new D3DVertex( pos + half_size * (- unitFront + unitRight + unitUp), color, new Vector2(0.5f, 0)),
                    new D3DVertex( pos + half_size * (- unitFront + unitRight - unitUp), color, new Vector2(0.5f, 1)),

                    // Right
                    new D3DVertex( pos + half_size * (- unitFront + unitRight - unitUp), color, new Vector2(0, 1)),
                    new D3DVertex( pos + half_size * (- unitFront + unitRight + unitUp), color, new Vector2(0, 0)),
                    new D3DVertex( pos + half_size * (+ unitFront + unitRight + unitUp), color, new Vector2(0.5f, 0)),
                    new D3DVertex( pos + half_size * (- unitFront + unitRight - unitUp), color, new Vector2(0, 1)),
                    new D3DVertex( pos + half_size * (+ unitFront + unitRight + unitUp), color, new Vector2(0.5f, 0)),
                    new D3DVertex( pos + half_size * (+ unitFront + unitRight - unitUp), color, new Vector2(0.5f, 1)),

                    // Left
                    new D3DVertex( pos + half_size * (- unitFront - unitRight - unitUp), color, new Vector2(0, 1)),
                    new D3DVertex( pos + half_size * (+ unitFront - unitRight + unitUp), color, new Vector2(0.5f, 0)),
                    new D3DVertex( pos + half_size * (- unitFront - unitRight + unitUp), color, new Vector2(0, 0)),
                    new D3DVertex( pos + half_size * (- unitFront - unitRight - unitUp), color, new Vector2(0, 1)),
                    new D3DVertex( pos + half_size * (+ unitFront - unitRight - unitUp), color, new Vector2(0.5f, 1)),
                    new D3DVertex( pos + half_size * (+ unitFront - unitRight + unitUp), color, new Vector2(0.5f, 0)),
                };
            }
            else if (topology == PrimitiveTopology.LineList)
            {
                v = new[]
                {
                   // Up plane
                    new D3DVertex( pos + half_size * (- unitFront - unitRight + unitUp), color),
                    new D3DVertex( pos + half_size * (- unitFront + unitRight + unitUp), color),
                    new D3DVertex( pos + half_size * (- unitFront + unitRight + unitUp), color),
                    new D3DVertex( pos + half_size * (+ unitFront + unitRight + unitUp), color),
                    new D3DVertex( pos + half_size * (+ unitFront + unitRight + unitUp), color),
                    new D3DVertex( pos + half_size * (+ unitFront - unitRight + unitUp), color),
                    new D3DVertex( pos + half_size * (+ unitFront - unitRight + unitUp), color),
                    new D3DVertex( pos + half_size * (- unitFront - unitRight + unitUp), color),

                    // Bottom plane
                    new D3DVertex( pos + half_size * (- unitFront - unitRight - unitUp), color),
                    new D3DVertex( pos + half_size * (- unitFront + unitRight - unitUp), color),
                    new D3DVertex( pos + half_size * (- unitFront + unitRight - unitUp), color),
                    new D3DVertex( pos + half_size * (+ unitFront + unitRight - unitUp), color),
                    new D3DVertex( pos + half_size * (+ unitFront + unitRight - unitUp), color),
                    new D3DVertex( pos + half_size * (+ unitFront - unitRight - unitUp), color),
                    new D3DVertex( pos + half_size * (+ unitFront - unitRight - unitUp), color),
                    new D3DVertex( pos + half_size * (- unitFront - unitRight - unitUp), color),

                    // Side planes
                    new D3DVertex( pos + half_size * (- unitFront - unitRight - unitUp), color),
                    new D3DVertex( pos + half_size * (- unitFront - unitRight + unitUp), color),
                    new D3DVertex( pos + half_size * (- unitFront + unitRight - unitUp), color),
                    new D3DVertex( pos + half_size * (- unitFront + unitRight + unitUp), color),
                    new D3DVertex( pos + half_size * (+ unitFront - unitRight - unitUp), color),
                    new D3DVertex( pos + half_size * (+ unitFront - unitRight + unitUp), color),
                    new D3DVertex( pos + half_size * (+ unitFront + unitRight - unitUp), color),
                    new D3DVertex( pos + half_size * (+ unitFront + unitRight + unitUp), color),
                };
            }
            else
            {
                throw new ArgumentException("Unsupported topology: " + topology);
            }

            return v;
        }
    }

    // struct D3DRenderParams
    // struct D3DVertexInput
}
