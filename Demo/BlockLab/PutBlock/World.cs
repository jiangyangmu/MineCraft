using System;
using System.Collections.Generic;

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Mathematics;

namespace PutBlock
{
    class Block
    {
        public enum Type
        {
            GRASS,
            VIRTUAL,
        };
        public static readonly float SIZE = 1.0f;
        public static readonly float HALF_SIZE = 0.5f;

        public Block(Type type, Int3 pos)
        {
            this.type = type;
            this.pos = pos;
        }

        public BoundingBox Box
        {
            get => new BoundingBox(
                new Vector3(pos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE),
                new Vector3(pos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE));
        }
        public Int3 Pos { get => pos; }

        public static int VertexCount(Type type)
        {
            return (type == Type.GRASS) ? 36 : 24;
        }
        public static BoundingBox BoxFromPos(Int3 pos)
        {
            return new BoundingBox(
                new Vector3(pos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE),
                new Vector3(pos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE));
        }

        public Int3? GetPutSlot(Ray ray)
        {
            var v = new[]
            {
                // Face to +X
                new Vector3(pos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE),
                new Vector3(pos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE),
                new Vector3(pos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE),
                new Vector3(pos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE),
                new Vector3(pos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE),
                new Vector3(pos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE),

                // Face to -X
                new Vector3(pos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE),
                new Vector3(pos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE),
                new Vector3(pos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE),
                new Vector3(pos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE),
                new Vector3(pos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE),
                new Vector3(pos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE),

                // Face to +Y
                new Vector3(pos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE),
                new Vector3(pos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE),
                new Vector3(pos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE),
                new Vector3(pos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE),
                new Vector3(pos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE),
                new Vector3(pos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE),

                // Face to -Y
                new Vector3(pos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE),
                new Vector3(pos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE),
                new Vector3(pos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE),
                new Vector3(pos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE),
                new Vector3(pos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE),
                new Vector3(pos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE),

                // Face to +Z
                new Vector3(pos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE),
                new Vector3(pos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE),
                new Vector3(pos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE),
                new Vector3(pos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE),
                new Vector3(pos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE),
                new Vector3(pos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE),

                // Face to -Z
                new Vector3(pos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE),
                new Vector3(pos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE),
                new Vector3(pos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE),
                new Vector3(pos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE),
                new Vector3(pos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE),
                new Vector3(pos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE),
            };
            var norms = new[]
            {
                Vector3.UnitX,
                -Vector3.UnitX,
                Vector3.UnitY,
                -Vector3.UnitY,
                Vector3.UnitZ,
                -Vector3.UnitZ,
            };
            var slots = new[]
            {
                new Int3(pos.X + 1, pos.Y, pos.Z),
                new Int3(pos.X - 1, pos.Y, pos.Z),
                new Int3(pos.X, pos.Y + 1, pos.Z),
                new Int3(pos.X, pos.Y - 1, pos.Z),
                new Int3(pos.X, pos.Y, pos.Z + 1),
                new Int3(pos.X, pos.Y, pos.Z - 1),
            };
            for (int i = 0; i < 36; i += 3)
            {
                if (ray.Intersects(ref v[i], ref v[i + 1], ref v[i + 2]) &&
                    Vector3.Dot(ray.Direction, norms[i / 6]) <= 0.0f)
                {
                    return slots[i / 6];
                }
            }
            return null;
        }

        // For DirectX
        public void FillVertice(ref D3DVertex[] v, ref int offset, Vector3 overrideColor)
        {
            // Position, Color
            var fpos = new Vector3(pos.X, pos.Y, pos.Z);
            var fcolor = !overrideColor.IsZero
                ? overrideColor
                : new Vector3(0.3f, 0.3f, 0.3f);

            if (type == Type.GRASS)
            {
                // Down
                v[offset++] = new D3DVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 1));
                v[offset++] = new D3DVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 0));
                v[offset++] = new D3DVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(1, 0));
                v[offset++] = new D3DVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 1));
                v[offset++] = new D3DVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(1, 0));
                v[offset++] = new D3DVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(1, 1));

                // Up
                v[offset++] = new D3DVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 1));
                v[offset++] = new D3DVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(1, 0));
                v[offset++] = new D3DVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 0));
                v[offset++] = new D3DVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 1));
                v[offset++] = new D3DVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(1, 1));
                v[offset++] = new D3DVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(1, 0));

                // Face to +Y
                v[offset++] = new D3DVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0, 1));
                v[offset++] = new D3DVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0, 0));
                v[offset++] = new D3DVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 0));
                v[offset++] = new D3DVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0, 1));
                v[offset++] = new D3DVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 0));
                v[offset++] = new D3DVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 1));

                // Face to -Y
                v[offset++] = new D3DVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0, 1));
                v[offset++] = new D3DVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 0));
                v[offset++] = new D3DVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0, 0));
                v[offset++] = new D3DVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0, 1));
                v[offset++] = new D3DVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 1));
                v[offset++] = new D3DVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 0));

                // Face to -X
                v[offset++] = new D3DVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0, 1));
                v[offset++] = new D3DVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0, 0));
                v[offset++] = new D3DVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 0));
                v[offset++] = new D3DVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0, 1));
                v[offset++] = new D3DVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 0));
                v[offset++] = new D3DVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 1));

                // Face to +X
                v[offset++] = new D3DVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0, 1));
                v[offset++] = new D3DVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 0));
                v[offset++] = new D3DVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0, 0));
                v[offset++] = new D3DVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0, 1));
                v[offset++] = new D3DVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 1));
                v[offset++] = new D3DVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z), new Vector2(0.5f, 0));
            }
            else if (type == Type.VIRTUAL)
            {
                // Top
                v[offset++] = new D3DVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z));
                v[offset++] = new D3DVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z));
                v[offset++] = new D3DVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z));
                v[offset++] = new D3DVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z));
                v[offset++] = new D3DVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z));
                v[offset++] = new D3DVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z));
                v[offset++] = new D3DVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z));
                v[offset++] = new D3DVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z));

                // Bottom
                v[offset++] = new D3DVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z));
                v[offset++] = new D3DVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z));
                v[offset++] = new D3DVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z));
                v[offset++] = new D3DVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z));
                v[offset++] = new D3DVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z));
                v[offset++] = new D3DVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z));
                v[offset++] = new D3DVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z));
                v[offset++] = new D3DVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z));

                // Vertical
                v[offset++] = new D3DVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z));
                v[offset++] = new D3DVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z));
                v[offset++] = new D3DVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z));
                v[offset++] = new D3DVertex(new Vector3(fpos.X - HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z));
                v[offset++] = new D3DVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z));
                v[offset++] = new D3DVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y - HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z));
                v[offset++] = new D3DVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z - HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z));
                v[offset++] = new D3DVertex(new Vector3(fpos.X + HALF_SIZE, pos.Y + HALF_SIZE, pos.Z + HALF_SIZE), new Vector3(fcolor.X, fcolor.Y, fcolor.Z));
            }
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

        public string DebugString
        {
            get =>
                "======== World ========\r\n" +
                "Dirty frame: " + NumDirtyFrame + "\r\n" +
                "Block: " + NumBlock + "\r\n" +
                "Mine: " + (mineBlock == null ? 0 : 1) + "\r\n" +
                "Put: " + (putBlock == null ? 0 : 1) + "\r\n" +
                "Ray: " + NumRay + "\r\n" +
                "";
        }

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
        // block picking, mine, put
        public bool DoMineBlock()
        {
            if (mineBlock != null)
            {
                RemoveBlock(mineBlock.Pos);
                mineBlock = null;
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool DoPutBlock()
        {
            if (putBlock != null)
            {
                AddBlock(putBlock.Pos, Block.Type.GRASS);
                putBlock = null;
                return true;
            }
            else
            {
                return false;
            }
        }
        public void PickTest(Ray ray, int range)
        {
            Block pickedBlock = null;
            float pickedDistance = float.MaxValue;

            int rx = (int)ray.Position.X;
            int ry = (int)ray.Position.Y;
            int rz = (int)ray.Position.Z;
            range = Math.Max(0, range - 1);
            for (int x = rx - range; x <= rx + range; ++x)
            {
                for (int y = ry - range; y <= ry + range; ++y)
                {
                    for (int z = rz - range; z <= rz + range; ++z)
                    {
                        var pos = new Int3(x, y, z).ToString();
                        if (blockMap.TryGetValue(pos, out Block block))
                        {
                            BoundingBox box = block.Box;
                            if (ray.Intersects(ref box, out float distance) &&
                                distance < pickedDistance)
                            {
                                pickedBlock = block;
                                pickedDistance = distance;
                            }
                        }
                    }
                }
            }

            if (pickedBlock != null)
            {
                // Enter mine & put
                if (mineBlock != pickedBlock)
                {
                    mineBlock = pickedBlock;
                    isDirty = true;
                }
                var putPos = mineBlock.GetPutSlot(ray);
                if (putPos.HasValue)
                {
                    if (putBlock == null || putBlock.Pos != putPos)
                    {
                        putBlock = new Block(Block.Type.VIRTUAL, putPos.Value);
                        isDirty = true;
                    }
                }
            }
            else
            {
                // Exit mine & put
                if (mineBlock != null || putBlock != null)
                {
                    mineBlock = null;
                    putBlock = null;
                    isDirty = true;
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
        public float Ground(Int3 pos)
        {
            pos.Z = 10;
            while (!HasBlock(pos) && pos.Z >= 0)
            {
                --pos.Z;
            }
            do
            {
                ++pos.Z;
            } while (HasBlock(pos));
            --pos.Z;
            return pos.Z + Block.HALF_SIZE;
        }

        // For DirectX
        public void UpdateVertexBuffer(D3DDynamicVertexBuffer vertexBuffer)
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

            int blockRegion = blockMap.Count * Block.VertexCount(Block.Type.GRASS);
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.Draw(blockRegion, offset);
            offset += blockRegion;

            if (putBlock != null)
            {
                int virtualBlockRegion = 1 * Block.VertexCount(Block.Type.VIRTUAL);
                context.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
                context.Draw(virtualBlockRegion, offset);
                offset += virtualBlockRegion;
            }
        }

        private D3DVertex[] GetVertices()
        {
            var vertices = new D3DVertex[
                rayCollection.Count * 2 + // ray
                blockMap.Count * Block.VertexCount(Block.Type.GRASS) + // grass block + mine block
                (putBlock != null ? 1 : 0) * Block.VertexCount(Block.Type.VIRTUAL) + // put block
                0];

            int offset = 0;
            foreach (Tuple<Ray, Vector3> entry in rayCollection)
            {
                var ray = entry.Item1;
                var color = entry.Item2;
                vertices[offset++] = new D3DVertex(ray.Position, color);
                vertices[offset++] = new D3DVertex(ray.Position + ray.Direction * 100.0f, color);
            }
            foreach (Block block in blockMap.Values)
            {
                block.FillVertice(ref vertices, ref offset, (mineBlock == block) ? Vector3.UnitX : Vector3.Zero);
            }
            putBlock?.FillVertice(ref vertices, ref offset, Vector3.One);

            vertexCount = vertices.Length;

            return vertices;
        }

        private bool isDirty = false;
        private int dirtyFrame = 0;
        private Dictionary<string, Block> blockMap;
        private Block mineBlock;
        private Block putBlock;
        private List<Tuple<Ray, Vector3>> rayCollection;
        private int vertexCount;
    }
}
