using System;
using System.Collections.Generic;

using SharpDX;
using SharpDX.Mathematics;

namespace PickBlock
{
    public class Block
    {
        public enum Type
        {
            GRASS,
        };
        public static readonly float SIZE = 1.0f;
        public static readonly float HALF_SIZE = 0.5f;
        public static readonly int VERTEX_COUNT = 72;

        public Block(Type type, Int3 pos)
        {
            this.type = type;
            this.pos = pos;
            alive = true;
        }

        public bool Alive { get => alive; set => alive = value; }

        // For DirectX
        public void FillVertice(ref Vector4[] v, ref int offset)
        {
            // Position, Color
            var fpos = new Vector3(pos.X, pos.Y, pos.Z);
            var fcolor = (pos.X + pos.Y + pos.Z) % 2 == 0 ? new Vector3(0.3f, 0.3f, 0.3f) : new Vector3(1.0f, 1.0f, 1.0f);

            v[offset++] = new Vector4(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f); // Front
            v[offset++] = new Vector4(fpos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f);
            v[offset++] = new Vector4(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f);
            v[offset++] = new Vector4(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f);
            v[offset++] = new Vector4(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f);
            v[offset++] = new Vector4(fpos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f);

            v[offset++] = new Vector4(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f); // BACK 
            v[offset++] = new Vector4(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f);
            v[offset++] = new Vector4(fpos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f);
            v[offset++] = new Vector4(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f);
            v[offset++] = new Vector4(fpos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f);
            v[offset++] = new Vector4(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f);

            v[offset++] = new Vector4(fpos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f); // Top
            v[offset++] = new Vector4(fpos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f);
            v[offset++] = new Vector4(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f);
            v[offset++] = new Vector4(fpos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f);
            v[offset++] = new Vector4(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f);
            v[offset++] = new Vector4(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f);

            v[offset++] = new Vector4(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f); // Bottom
            v[offset++] = new Vector4(fpos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f);
            v[offset++] = new Vector4(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f);
            v[offset++] = new Vector4(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f);
            v[offset++] = new Vector4(fpos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f);
            v[offset++] = new Vector4(fpos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f);

            v[offset++] = new Vector4(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f); // Left
            v[offset++] = new Vector4(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f);
            v[offset++] = new Vector4(fpos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f);
            v[offset++] = new Vector4(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f);
            v[offset++] = new Vector4(fpos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f);
            v[offset++] = new Vector4(fpos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f);

            v[offset++] = new Vector4(fpos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f); // Right
            v[offset++] = new Vector4(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f);
            v[offset++] = new Vector4(fpos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f);
            v[offset++] = new Vector4(fpos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f);
            v[offset++] = new Vector4(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f);
            v[offset++] = new Vector4(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE, 1.0f); v[offset++] = new Vector4(fcolor.X, fcolor.Y, fcolor.Z, 1.0f);
        }

        private Type type;
        private Int3 pos;
        private bool alive;
    }
    public class World
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

        public World()
        {
            Right = new Int3(1, 0, 0);
            Forward = new Int3(0, 1, 0);
            Up = new Int3(0, 0, -1);
            Origin = new Int3(0, 0, 0);

            RightF = new Vector3(1, 0, 0);
            ForwardF = new Vector3(0, 1, 0);
            UpF = new Vector3(0, 0, -1);
            OriginF = new Vector3(0, 0, 0);

            blockMap = new Dictionary<string, Block>();
            blockCount = 0;
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
            blockCount = 0;
        }

        public bool AddBlock(Int3 pos, Block.Type type)
        {
            var key = pos.ToString();

            Block oldBlock;
            if (!blockMap.TryGetValue(key, out oldBlock))
            {
                blockMap.Add(key, new Block(type, pos));
                ++blockCount;
                IsDirty = true;
                return true;
            }
            else if (!oldBlock.Alive)
            {
                blockMap[key] = new Block(type, pos);
                ++blockCount;
                IsDirty = true;
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
                // blockMap[key].Alive = false;
                --blockCount;
                IsDirty = true;
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
                // blockMap[key].Alive = false;
                --blockCount;
                IsDirty = true;
                return true;
            }
            else
            {
                return false;
            }
        }
        
        // For DirectX
        public bool IsDirty { get; set; }
        public int BlockCount { get => blockCount; }
        public void FillVertice(ref Vector4[] v, ref int offset)
        {
            foreach (Block block in blockMap.Values)
            {
                if (block.Alive)
                    block.FillVertice(ref v, ref offset);
            }
        }
        public Vector4[] GetVertices()
        {
            var vertices = new Vector4[blockCount * Block.VERTEX_COUNT];
            int offset = 0;
            foreach (Block block in blockMap.Values)
            {
                if (block.Alive)
                    block.FillVertice(ref vertices, ref offset);
            }
            return vertices;
        }

        private Dictionary<string, Block> blockMap;
        private int blockCount;
        
        // Representation
        // Block as Tri Vertex
    }
}
