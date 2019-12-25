using System;
using System.Collections;
using System.Collections.Generic;

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Mathematics;

namespace BlockMarket
{
    struct BlockObject : IEquatable<BlockObject>
    {
        public static readonly int size = 1;
        public Int3 pos;

        public BlockObject(Int3 pos)
        {
            this.pos = pos;
        }

        public static bool operator ==(BlockObject x, BlockObject y)
        {
            return x.Equals(y);
        }
        public static bool operator !=(BlockObject x, BlockObject y)
        {
            return !x.Equals(y);
        }
        public override bool Equals(object obj)
        {
            return obj is BlockObject && Equals((BlockObject)obj);
        }
        public bool Equals(BlockObject other)
        {
            return pos.Equals(other.pos);
        }
        public override int GetHashCode()
        {
            return 991532785 + EqualityComparer<Int3>.Default.GetHashCode(pos);
        }

        public BoundingBox GetBoundingBox()
        {
            float half_size = size * 0.5f;
            return new BoundingBox(
               new Vector3(pos.X - half_size, pos.Y - half_size, pos.Z - half_size),
               new Vector3(pos.X + half_size, pos.Y + half_size, pos.Z + half_size));
        }
        public BlockObject? GetPutBlock(Ray ray)
        {
            float half_size = size * 0.5f;
            var v = new[]
            {
                // Face to +X
                new Vector3(pos.X + half_size, pos.Y - half_size, pos.Z - half_size),
                new Vector3(pos.X + half_size, pos.Y + half_size, pos.Z + half_size),
                new Vector3(pos.X + half_size, pos.Y - half_size, pos.Z + half_size),
                new Vector3(pos.X + half_size, pos.Y - half_size, pos.Z - half_size),
                new Vector3(pos.X + half_size, pos.Y + half_size, pos.Z - half_size),
                new Vector3(pos.X + half_size, pos.Y + half_size, pos.Z + half_size),

                // Face to -X
                new Vector3(pos.X - half_size, pos.Y - half_size, pos.Z - half_size),
                new Vector3(pos.X - half_size, pos.Y - half_size, pos.Z + half_size),
                new Vector3(pos.X - half_size, pos.Y + half_size, pos.Z + half_size),
                new Vector3(pos.X - half_size, pos.Y - half_size, pos.Z - half_size),
                new Vector3(pos.X - half_size, pos.Y + half_size, pos.Z + half_size),
                new Vector3(pos.X - half_size, pos.Y + half_size, pos.Z - half_size),

                // Face to +Y
                new Vector3(pos.X - half_size, pos.Y + half_size, pos.Z - half_size),
                new Vector3(pos.X - half_size, pos.Y + half_size, pos.Z + half_size),
                new Vector3(pos.X + half_size, pos.Y + half_size, pos.Z + half_size),
                new Vector3(pos.X - half_size, pos.Y + half_size, pos.Z - half_size),
                new Vector3(pos.X + half_size, pos.Y + half_size, pos.Z + half_size),
                new Vector3(pos.X + half_size, pos.Y + half_size, pos.Z - half_size),

                // Face to -Y
                new Vector3(pos.X - half_size, pos.Y - half_size, pos.Z - half_size),
                new Vector3(pos.X + half_size, pos.Y - half_size, pos.Z + half_size),
                new Vector3(pos.X - half_size, pos.Y - half_size, pos.Z + half_size),
                new Vector3(pos.X - half_size, pos.Y - half_size, pos.Z - half_size),
                new Vector3(pos.X + half_size, pos.Y - half_size, pos.Z - half_size),
                new Vector3(pos.X + half_size, pos.Y - half_size, pos.Z + half_size),

                // Face to +Z
                new Vector3(pos.X - half_size, pos.Y - half_size, pos.Z + half_size),
                new Vector3(pos.X + half_size, pos.Y + half_size, pos.Z + half_size),
                new Vector3(pos.X - half_size, pos.Y + half_size, pos.Z + half_size),
                new Vector3(pos.X - half_size, pos.Y - half_size, pos.Z + half_size),
                new Vector3(pos.X + half_size, pos.Y - half_size, pos.Z + half_size),
                new Vector3(pos.X + half_size, pos.Y + half_size, pos.Z + half_size),

                // Face to -Z
                new Vector3(pos.X - half_size, pos.Y - half_size, pos.Z - half_size),
                new Vector3(pos.X - half_size, pos.Y + half_size, pos.Z - half_size),
                new Vector3(pos.X + half_size, pos.Y + half_size, pos.Z - half_size),
                new Vector3(pos.X - half_size, pos.Y - half_size, pos.Z - half_size),
                new Vector3(pos.X + half_size, pos.Y + half_size, pos.Z - half_size),
                new Vector3(pos.X + half_size, pos.Y - half_size, pos.Z - half_size),
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
                    return new BlockObject() { pos = slots[i / 6] };
                }
            }
            return null;
        }
    }

    struct BlockRenderer
    {
        public static BlockRenderer GrassBlockRender { get => new BlockRenderer(PrimitiveTopology.TriangleList, "PS_Texture_Block", "Grass"); }
        public static BlockRenderer OakWoodBlockRender { get => new BlockRenderer(PrimitiveTopology.TriangleList, "PS_Texture_Block", "OakWood"); }
        public static BlockRenderer PutBlockRender { get => new BlockRenderer(PrimitiveTopology.LineList, "PS_Line_Block", "Grass", Vector3.One); }

        public PrimitiveTopology Topology { get; }
        public string PSShaderName { get; }
        public string TextureName { get; }
        public Vector3 Color { get; }

        public int GetBlockVertexCount()
        {
            return D3DVertex.GetBlockVertexCount(Topology);
        }
        public void FillBlockVertices(ref D3DVertex[] v, ref int offset, BlockObject block)
        {
            var blockVertices = D3DVertex.GetBlockVertices(
                Topology,
                Vector3.UnitX,
                Vector3.UnitY,
                Vector3.UnitZ,
                (Vector3)block.pos,
                Color,
                BlockObject.size
                );
            foreach (var vertex in blockVertices)
            {
                v[offset++] = vertex;
            }
        }
        public void FillBlockVertices(ref D3DVertex[] v, ref int offset, BlockObject block, Vector3 color)
        {
            var blockVertices = D3DVertex.GetBlockVertices(
                Topology,
                Vector3.UnitX,
                Vector3.UnitY,
                Vector3.UnitZ,
                (Vector3)block.pos,
                color,
                BlockObject.size
                );
            foreach (var vertex in blockVertices)
            {
                v[offset++] = vertex;
            }
        }

        private BlockRenderer(PrimitiveTopology topology, string shaderName, string textureName)
        {
            Topology = topology;
            PSShaderName = shaderName;
            TextureName = textureName;
            Color = Vector3.One * 0.3f;
        }
        private BlockRenderer(PrimitiveTopology topology, string shaderName, string textureName, Vector3 color)
        {
            Topology = topology;
            PSShaderName = shaderName;
            TextureName = textureName;
            Color = color;
        }
    }

    struct BlockInfo
    {
        public BlockObject obj;
        public BlockRenderer render;

        public BlockInfo(BlockRenderer render, BlockObject obj)
        {
            this.obj = obj;
            this.render = render;
        }
    }


    class BlockManager
    {
        public bool IsDirty { get => isDirty; }
        public int BlockCount { get => blockCount; }

        // Position based
        public void AddBlock(BlockRenderer render, BlockObject block)
        {
            if (HasBlock(block))
                return;

            var index = blockRenderList.IndexOf(render);
            if (index == -1)
            {
                blockRenderList.Add(render);
                blockObjectsList.Add(new Dictionary<Int3, BlockObject>());
                index = blockRenderList.Count - 1;
            }

            blockObjectsList[index].Add(block.pos, block);

            ++blockCount;
            isDirty = true;
        }
        public void AddBlockPlane(BlockRenderer render, BlockObject centerBlock, int left, int right, int forward, int backward)
        {
            for (int r = -left; r <= right; ++r)
            {
                for (int c = -backward; c <= forward; ++c)
                {
                    AddBlock(render, new BlockObject(centerBlock.pos + new Int3(r, c, 0)));
                }
            }
        }
        public bool HasBlock(Int3 pos)
        {
            foreach (var blockDict in blockObjectsList)
            {
                if (blockDict.ContainsKey(pos))
                    return true;
            }
            return false;
        }
        public bool HasBlock(BlockObject block)
        {
            return HasBlock(block.pos);
        }
        public bool HasBlockWithType(Int3 pos, BlockRenderer render)
        {
            int index = blockRenderList.IndexOf(render);
            return index != -1 && blockObjectsList[index].ContainsKey(pos);
        }
        public bool TryGetBlock(Int3 pos, out BlockObject block)
        {
            foreach (var blockDict in blockObjectsList)
            {
                if (blockDict.TryGetValue(pos, out block))
                    return true;
            }
            block = new BlockObject(Int3.Zero);
            return false;
        }
        public bool RemoveBlock(BlockObject block)
        {
            foreach (var blockDict in blockObjectsList)
            {
                if (blockDict.Remove(block.pos))
                {
                    --blockCount;
                    isDirty = true;
                    return true;
                }
            }
            return false;
        }
        public int RemoveBlocksByType(BlockRenderer render)
        {
            int index = blockRenderList.IndexOf(render);
            if (index != -1)
            {
                int count = blockObjectsList[index].Count;
                blockObjectsList[index].Clear();
                return count;
            }
            else
            {
                return 0;
            }
        }

        public int GetTotalVertexCount()
        {
            int count = 0;
            for (int i = 0; i < blockRenderList.Count; ++i)
            {
                count += blockRenderList[i].GetBlockVertexCount() * blockObjectsList[i].Count;
            }
            return count;
        }
        public void FillVertices(ref D3DVertex[] v, ref int offset)
        {
            for (int i = 0; i < blockRenderList.Count; ++i)
            {
                var render = blockRenderList[i];
                var blockObjects = blockObjectsList[i];

                foreach (var block in blockObjects.Values)
                {
                    render.FillBlockVertices(ref v, ref offset, block);
                }
            }
            isDirty = false;
        }
        public void FillVertices(ref D3DVertex[] v, ref int offset, Func<BlockObject, Vector3, Vector3> colorMutator)
        {
            for (int i = 0; i < blockRenderList.Count; ++i)
            {
                var render = blockRenderList[i];
                var blockObjects = blockObjectsList[i];

                foreach (var block in blockObjects.Values)
                {
                    render.FillBlockVertices(ref v, ref offset, block, colorMutator.Invoke(block, render.Color));
                }
            }
            isDirty = false;
        }
        public void Render(DeviceContext context, ResourceManager resMgr, ref int offset)
        {
            for (int i = 0; i < blockRenderList.Count; ++i)
            {
                var blockRender = blockRenderList[i];
                var blockCount = blockObjectsList[i].Count;
                int vertexCount = blockCount * blockRender.GetBlockVertexCount();

                context.InputAssembler.PrimitiveTopology = blockRender.Topology;
                context.PixelShader.Set(resMgr.GetPSShader(blockRender.PSShaderName));
                context.PixelShader.SetShaderResource(0, resMgr.GetTextureSRV(blockRender.TextureName));
                context.Draw(vertexCount, offset);
                offset += vertexCount;
            }
        }

        private List<BlockRenderer> blockRenderList = new List<BlockRenderer>()
        {
            BlockRenderer.GrassBlockRender,
            BlockRenderer.OakWoodBlockRender,
            BlockRenderer.PutBlockRender,
        };
        private List<Dictionary<Int3, BlockObject>> blockObjectsList = new List<Dictionary<Int3, BlockObject>>()
        {
            new Dictionary<Int3, BlockObject>(),
            new Dictionary<Int3, BlockObject>(),
            new Dictionary<Int3, BlockObject>(),
        };

        private int blockCount = 0;
        private bool isDirty = true;
    }

    class World
    {
        public World()
        {
            Forward = new Int3(1, 0, 0);
            Right = new Int3(0, 1, 0);
            Up = new Int3(0, 0, 1);
            Origin = new Int3(0, 0, 0);

            RightF = new Vector3(Right.X, Right.Y, Right.Z);
            ForwardF = new Vector3(Forward.X, Forward.Y, Forward.Z);
            UpF = new Vector3(Up.X, Up.Y, Up.Z);
            OriginF = new Vector3(Origin.X, Origin.Y, Origin.Z);

            blockManager = new BlockManager();
            putBlockMgr = new BlockManager();
            rayCollection = new List<Tuple<Ray, Vector3>>();
        }

        // Properties

        // Coordination: left-handed, X => Forward, Y => Right, Z => Up
        public Int3 Right { get; }
        public Int3 Up { get; }
        public Int3 Forward { get; }
        public Int3 Origin { get; }

        public Vector3 RightF { get; }
        public Vector3 UpF { get; }
        public Vector3 ForwardF { get; }
        public Vector3 OriginF { get; }

        public int NumDirtyFrame { get => dirtyFrame; set => dirtyFrame = value; }
        public int NumBlock { get => blockManager.BlockCount; }
        public int NumRay { get => rayCollection.Count; }

        public string DebugString
        {
            get =>
                "======== World ========\r\n" +
                "Dirty frame: " + NumDirtyFrame + "\r\n" +
                "Vertex: " + blockManager.GetTotalVertexCount() + "\r\n" +
                "Block: " + NumBlock + "\r\n" +
                "Mine: " + (mineBlock.HasValue ? mineBlock.Value.pos.ToString() : "null") + "\r\n" +
                "Put: " + (putBlock.HasValue ? putBlock.Value.obj.pos.ToString() : "null") + "\r\n" +
                "Ray: " + NumRay + "\r\n" +
                "";
        }

        // Operations
        
        // block operation -> BlockManager
        public BlockManager BlockManager { get => blockManager; }
        // block picking, mine, put
        public bool DoMineBlock()
        {
            if (mineBlock.HasValue)
            {
                blockManager.RemoveBlock(mineBlock.Value);
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
            if (putBlock.HasValue)
            {
                blockManager.AddBlock(BlockRenderer.GrassBlockRender, putBlock.Value.obj);
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
            BlockObject? pickedBlock = null;
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
                        if (blockManager.TryGetBlock(new Int3(x, y, z), out BlockObject block))
                        {
                            BoundingBox box = block.GetBoundingBox();
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
                var put = mineBlock.Value.GetPutBlock(ray);
                if (put.HasValue)
                {
                    if (putBlock == null || putBlock.Value.obj != put)
                    {
                        putBlock = new BlockInfo(BlockRenderer.PutBlockRender, put.Value);
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
        public float Ground(Int3 pos)
        {
            pos.Z = 10;
            while (!blockManager.HasBlock(pos) && pos.Z >= 0)
            {
                --pos.Z;
            }
            do
            {
                ++pos.Z;
            } while (blockManager.HasBlock(pos));
            --pos.Z;
            return pos.Z + BlockObject.size * 0.5f;
        }

        // Implementation

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
        public void Render(DeviceContext context, ResourceManager resMgr)
        {
            int offset = 0;

            int rayRegion = rayCollection.Count * 2;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
            context.Draw(rayRegion, offset);
            offset += rayRegion;

            blockManager.Render(context, resMgr, ref offset);
            if (putBlock.HasValue)
            {
                putBlockMgr.RemoveBlocksByType(putBlock.Value.render);
                putBlockMgr.AddBlock(putBlock.Value.render, putBlock.Value.obj);
                putBlockMgr.Render(context, resMgr, ref offset);
            }
        }

        private D3DVertex[] GetVertices()
        {
            var vertices = new D3DVertex[
                rayCollection.Count * 2 + // ray
                blockManager.GetTotalVertexCount() + // block + mine block
                (putBlock.HasValue ? putBlock.Value.render.GetBlockVertexCount() : 0) + // put block
                0];

            int offset = 0;
            foreach (Tuple<Ray, Vector3> entry in rayCollection)
            {
                var ray = entry.Item1;
                var color = entry.Item2;
                vertices[offset++] = new D3DVertex(ray.Position, color);
                vertices[offset++] = new D3DVertex(ray.Position + ray.Direction * 100.0f, color);
            }

            if (mineBlock.HasValue)
            {
                blockManager.FillVertices(ref vertices, ref offset,
                    (block, color) =>
                    {
                        return block.pos == mineBlock.Value.pos ? Vector3.UnitX : color;
                    });
            }
            else
            {
                blockManager.FillVertices(ref vertices, ref offset);
            }
            if (putBlock.HasValue)
                putBlock.Value.render.FillBlockVertices(ref vertices, ref offset, putBlock.Value.obj);

            vertexCount = vertices.Length;

            return vertices;
        }

        private bool isDirty = false;
        private int dirtyFrame = 0;

        private BlockManager blockManager;
        private BlockObject? mineBlock;
        private BlockManager putBlockMgr; // render put block
        private BlockInfo? putBlock;

        private List<Tuple<Ray, Vector3>> rayCollection;
        private int vertexCount;
    }
}
