using System;
using System.Collections.Generic;

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Mathematics;

namespace MineBlock
{
    class Block
    {
        public enum Type
        {
            GRASS,
        };
        public static readonly float SIZE = 1.0f;
        public static readonly float HALF_SIZE = 0.5f;
        public static readonly int VERTEX_COUNT = 36;

        public Block(Type type, Int3 pos)
        {
            this.type = type;
            this.pos = pos;
        }

        public Int3 Pos { get => pos; }
        public BoundingBox Box
        {
            get => new BoundingBox(
                new Vector3(pos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE),
                new Vector3(pos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE));
        }

        // For DirectX
        public void FillVertice(ref DXVertex[] v, ref int offset, Vector3 overrideColor)
        {
            // Position, Color
            var fpos = new Vector3(pos.X, pos.Y, pos.Z);
            var fcolor = !overrideColor.IsZero
                ? overrideColor
                : new Vector3(0.3f, 0.3f, 0.3f);

            // Down
            v[offset++] = new DXVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 1));
            v[offset++] = new DXVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 0));
            v[offset++] = new DXVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(1, 0));
            v[offset++] = new DXVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 1));
            v[offset++] = new DXVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(1, 0));
            v[offset++] = new DXVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(1, 1));

            // Up
            v[offset++] = new DXVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 1));
            v[offset++] = new DXVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(1, 0));
            v[offset++] = new DXVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 0));
            v[offset++] = new DXVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 1));
            v[offset++] = new DXVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(1, 1));
            v[offset++] = new DXVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(1, 0));

            // Face to +Y
            v[offset++] = new DXVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0, 1));
            v[offset++] = new DXVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0, 0));
            v[offset++] = new DXVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 0));
            v[offset++] = new DXVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0, 1));
            v[offset++] = new DXVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 0));
            v[offset++] = new DXVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 1));

            // Face to -Y
            v[offset++] = new DXVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0, 1));
            v[offset++] = new DXVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 0));
            v[offset++] = new DXVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0, 0));
            v[offset++] = new DXVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0, 1));
            v[offset++] = new DXVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 1));
            v[offset++] = new DXVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 0));

            // Face to -X
            v[offset++] = new DXVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0, 1));
            v[offset++] = new DXVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0, 0));
            v[offset++] = new DXVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 0));
            v[offset++] = new DXVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0, 1));
            v[offset++] = new DXVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 0));
            v[offset++] = new DXVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 1));

            // Face to +X
            v[offset++] = new DXVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0, 1));
            v[offset++] = new DXVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 0));
            v[offset++] = new DXVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0, 0));
            v[offset++] = new DXVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0, 1));
            v[offset++] = new DXVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 1));
            v[offset++] = new DXVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 0));
        }

        private Type type;
        private Int3 pos;
    }
    class World
    {
        // Left-handed, X => Right, Y => Forward, -Z => Up
        public Int3 Right { get; }
        public Int3 Up { get; }
        public Int3 Forward { get; }
        public Int3 Origin { get; }

        public Vector3 RightF { get; }
        public Vector3 UpF { get; }
        public Vector3 ForwardF { get; }
        public Vector3 OriginF { get; }

        public int NumDirtyFrame { get => dirtyFrame; set => dirtyFrame = value; }
        public int NumBlock { get => blockMap.Count; }
        public int NumRay { get => rayCollection.Count; }

        public World()
        {
            Right = new Int3(1, 0, 0);
            Forward = new Int3(0, 1, 0);
            Up = new Int3(0, 0, 1);
            Origin = new Int3(0, 0, 0);

            RightF = new Vector3(1, 0, 0);
            ForwardF = new Vector3(0, 1, 0);
            UpF = new Vector3(0, 0, 1);
            OriginF = new Vector3(0, 0, 0);

            blockMap = new Dictionary<string, Block>();
            rayCollection = new List<Tuple<Ray, Vector3>>();

            pickedBlock = Int3.Zero;
        }
        public World(Int3 right, Int3 forward, Int3 up)
        {
            Right = right;
            Forward = forward;
            Up = up;
            Origin = new Int3(0, 0, 0);

            RightF = new Vector3(Right.X, Right.Y, Right.Z);
            ForwardF = new Vector3(Forward.X, Forward.Y,Forward.Z);
            UpF = new Vector3(Up.X, Up.Y, Up.Z);
            OriginF = new Vector3(Origin.X, Origin.Y,Origin.Z);

            blockMap = new Dictionary<string, Block>();
            rayCollection = new List<Tuple<Ray, Vector3>>();

            pickedBlock = Int3.Zero;
        }

        public bool AddBlock(Int3 pos, Block.Type type)
        {
            var key = pos.ToString();

            Block oldBlock;
            if (!blockMap.TryGetValue(key, out oldBlock))
            {
                blockMap.Add(key, new Block(type, pos));
                isDirty = true;
                return true;
            }
            else
            {
                return false;
            }
        }
        // left, right, forward, backward >= 0
        public void AddBlockPlane(Int3 pos, Block.Type type, int left, int right, int forward, int backward)
        {
            for (int r = -left; r <= right; ++r)
            {
                for (int c = -backward; c <= forward; ++c)
                {
                    AddBlock(new Int3(pos.X + r, pos.Y + c, pos.Z), type);
                }
            }
        }
        public bool RemoveBlock(Int3 pos)
        {
            var key = pos.ToString();

            if (blockMap.ContainsKey(key))
            {
                blockMap.Remove(key);
                isDirty = true;
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool RemoveRandomBlock()
        {
            var e = blockMap.GetEnumerator();
            if (!e.MoveNext())
            {
                return false;
            }
            var key = e.Current.Key;

            if (blockMap.ContainsKey(key))
            {
                blockMap.Remove(key);
                isDirty = true;
                return true;
            }
            else
            {
                return false;
            }
        }
        // block picking
        public void PickBlock(Int3 pos)
        {
            if (pickedBlock != pos)
            {
                pickedBlock = pos;
                isDirty = true;
            }
        }
        public void RemovePickedBlock()
        {
            RemoveBlock(pickedBlock);
        }
        public void PickTest(Ray ray)
        {
            foreach (Block block in blockMap.Values)
            {
                if (ray.Intersects(block.Box))
                {
                    PickBlock(block.Pos);
                    return;
                }
            }
        }
        public void AddRay(Vector3 pos, Vector3 ori, Vector3 color)
        {
            rayCollection.Add(new Tuple<Ray, Vector3>(new Ray(pos, ori), color));
            isDirty = true;
        }
        // collision
        public bool HasBlock(Int3 pos)
        {   
            return blockMap.ContainsKey(pos.ToString());
        }

        // For DirectX
        public void UpdateVertexBuffer(DXDynamicVertexBuffer vertexBuffer)
        {
            if (isDirty)
            {
                isDirty = false;
                ++NumDirtyFrame;
                vertexBuffer.Reset(GetVertices());
                
            }
        }
        public void CallDraws(DeviceContext context)
        {
            int offset = 0;

            int rayRegion = rayCollection.Count * 2;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
            context.Draw(rayRegion, offset);
            offset += rayRegion;

            int blockRegion = blockMap.Count * Block.VERTEX_COUNT;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.Draw(blockRegion, offset);
            offset += blockRegion;
        }

        private DXVertex[] GetVertices()
        {
            var vertices = new DXVertex[
                blockMap.Count * Block.VERTEX_COUNT + // block
                rayCollection.Count * 2 // ray
                ];

            int offset = 0;
            foreach (Tuple<Ray, Vector3> entry in rayCollection)
            {
                var ray = entry.Item1;
                var color = entry.Item2;
                vertices[offset++] = new DXVertex(ray.Position, color);
                vertices[offset++] = new DXVertex(ray.Position + ray.Direction * 100.0f, color);
            }
            foreach (Block block in blockMap.Values)
            {
                block.FillVertice(ref vertices, ref offset, pickedBlock == block.Pos ? Vector3.UnitX : Vector3.Zero);
            }

            vertexCount = vertices.Length;

            return vertices;
        }

        private bool isDirty = false;
        private int dirtyFrame = 0;
        private Dictionary<string, Block> blockMap;
        private Int3 pickedBlock;
        private List<Tuple<Ray, Vector3>> rayCollection;
        private int vertexCount;
    }
}
