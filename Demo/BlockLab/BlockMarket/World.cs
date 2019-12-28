using System;
using System.Collections;
using System.Collections.Generic;

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Mathematics;

namespace BlockMarket
{
    enum BlockType
    {
        PUT = 1,
        GRASS = 2,
        SAND = 3,
        STONE = 4,
        OAK_WOOD = 5,
        OAK_LEAF = 6,
        GLASS = 7,
        WATER = 8,
    }

    class BlockTypeUtil
    {
        public static BlockType FromItemType(ItemType itemType)
        {
            BlockType type;
            switch (itemType)
            {
                case ItemType.GRASS_BLOCK: type = BlockType.GLASS; break;
                case ItemType.SAND_BLOCK: type = BlockType.SAND; break;
                case ItemType.STONE_BLOCK: type = BlockType.STONE; break;
                case ItemType.OAK_WOOD_BLOCK: type = BlockType.OAK_WOOD; break;
                case ItemType.OAK_LEAF_BLOCK: type = BlockType.OAK_LEAF; break;
                case ItemType.GLASS_BLOCK: type = BlockType.GLASS; break;
                default: throw new ArgumentException("Can't convert ItemType " + itemType + " to BlockType.");
            }
            return type;
        }
    }

    // Block pos, size, transform, physics
    struct BlockBody : IEquatable<BlockBody>
    {
        public static readonly int size = 1;
        public Int3 pos;

        public BlockBody(Int3 pos)
        {
            this.pos = pos;
        }

        public static bool operator ==(BlockBody x, BlockBody y)
        {
            return x.Equals(y);
        }
        public static bool operator !=(BlockBody x, BlockBody y)
        {
            return !x.Equals(y);
        }
        public override bool Equals(object body)
        {
            return body is BlockBody && Equals((BlockBody)body);
        }
        public bool Equals(BlockBody other)
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
        public BlockBody? GetPutBlock(Ray ray)
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
                    return new BlockBody() { pos = slots[i / 6] };
                }
            }
            return null;
        }
    }

    struct Block
    {
        public BlockType type; // rendering
        public Int3 pos; // physics

        public Block(BlockType type, Int3 pos)
        {
            this.type = type;
            this.pos = pos;
        }

        public BlockRenderer Render { get => BlockRenderer.RenderList[(int)type]; }
        public BlockBody Body { get => new BlockBody(pos); }

        public static Block Put(Int3 pos)
        {
            return new Block(BlockType.PUT, pos);
        }
        public static Block Grass(Int3 pos)
        {
            return new Block(BlockType.GRASS, pos);
        }
        public static Block Sand(Int3 pos)
        {
            return new Block(BlockType.SAND, pos);
        }
        public static Block Stone(Int3 pos)
        {
            return new Block(BlockType.STONE, pos);
        }
        public static Block OakWood(Int3 pos)
        {
            return new Block(BlockType.OAK_WOOD, pos);
        }
        public static Block OakLeaf(Int3 pos)
        {
            return new Block(BlockType.OAK_LEAF, pos);
        }
        public static Block Glass(Int3 pos)
        {
            return new Block(BlockType.GLASS, (pos));
        }
        public static Block Water(Int3 pos)
        {
            return new Block(BlockType.WATER, (pos));
        }
    }

    // Tree, Water
    class GenerativeBlock
    {
        enum Type
        {
            TREE,
            WATER,
        }
    }
    // TNT
    class DestructiveBlock
    {

    }

    class BlockManager
    {
        public bool IsDirty { get => isDirty; }
        public int BlockCount { get => blockCount; }

        // Position based
        public void AddBlock(Block info)
        {
            if (HasBlock(info.pos))
                return;

            var index = blockRenderList.IndexOf(info.Render);
            if (index == -1)
            {
                blockRenderList.Add(info.Render);
                blockObjectsList.Add(new Dictionary<Int3, BlockBody>());
                index = blockRenderList.Count - 1;
            }

            blockObjectsList[index].Add(info.pos, info.Body);

            ++blockCount;
            isDirty = true;
        }
        public void AddBlockPlane(Block info, int forward, int backward, int left, int right)
        {
            var pos = info.pos;
            for (int r = -left; r <= right; ++r)
            {
                for (int c = -backward; c <= forward; ++c)
                {
                    info.pos = pos + new Int3(r, c, 0);
                    AddBlock(info);
                }
            }
        }
        private static Random randTreeLeafPos = new Random();
        public void AddTree(Int3 pos, int height)
        {
            var level = pos;
            for (int h = 0; h < 3; ++h)
            {
                AddBlockPlane(Block.OakWood(level), 0, 0, 0, 0);
                level += Int3.UnitZ;
            }
            for (int h = 0; h < (height - 3); ++h)
            {
                if (h < 2)
                    AddBlockPlane(Block.OakWood(level), 0, 0, 0, 0);

                var half = Math.Max(0, (height - h) / 6);
                if (height < 15)
                    half = Math.Max(half, (height - h) / 2);
                for (int c = 0; c < half * half * 3; ++c)
                {
                    var xy = randTreeLeafPos.NextPoint(new Point(-half, -half), new Point(half, half));
                    var shift = new Int3(xy.X, xy.Y, 0);
                    AddBlockPlane(Block.OakLeaf(level + shift), 0, 0, 0, 0);
                }
                level += Int3.UnitZ;
            }
        }
        public void AddWaterPool(Int3 min, Int3 max)
        {
            for (int x = min.X; x <= max.X; ++x)
            {
                for (int y = min.Y; y <= max.Y; ++y)
                {
                    for (int z = min.Z; z <= max.Z; ++z)
                    {
                        AddBlock(Block.Water(new Int3(x, y, z)));
                    }
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
        public bool HasBlock(BlockBody block)
        {
            return HasBlock(block.pos);
        }
        public bool HasBlockWithType(Int3 pos, BlockRenderer render)
        {
            int index = blockRenderList.IndexOf(render);
            return index != -1 && blockObjectsList[index].ContainsKey(pos);
        }
        public bool TryGetBlock(Int3 pos, out BlockBody block)
        {
            foreach (var blockDict in blockObjectsList)
            {
                if (blockDict.TryGetValue(pos, out block))
                    return true;
            }
            block = new BlockBody(Int3.Zero);
            return false;
        }
        public bool TryGetBlockType(Int3 pos, out BlockRenderer render)
        {
            for (int i = 0; i < blockObjectsList.Count; ++i)
            {
                if (blockObjectsList[i].ContainsKey(pos))
                {
                    render = blockRenderList[i];
                    return true;
                }
            }
            render = BlockRenderer.NullRender;
            return false;
        }
        public bool RemoveBlock(BlockBody block)
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
        public void FillVertices(ref D3DVertex[] v, ref int offset, Func<BlockBody, Vector3, Vector3> colorMutator)
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
        public void Render(DeviceContext context, D3DResourceManager resMgr, ref int offset)
        {
            context.OutputMerger.BlendState = resMgr.GetDefaultBlendState();

            var countAndOffset = new List<Tuple<int, int>>(blockRenderList.Count);
            for (int i = 0; i < blockRenderList.Count; ++i)
            {
                var blockRender = blockRenderList[i];
                var blockCount = blockObjectsList[i].Count;
                int vertexCount = blockCount * blockRender.GetBlockVertexCount();

                countAndOffset.Add(new Tuple<int, int>(vertexCount, offset));
                offset += vertexCount;
            }

            for (int i = 0; i < blockRenderList.Count; ++i)
            {
                var blockRender = blockRenderList[i];

                if (BlockRenderer.DelayRendering(blockRender.Type))
                {
                    continue;
                }

                context.InputAssembler.PrimitiveTopology = blockRender.Topology;
                context.PixelShader.Set(resMgr.GetPSShader(blockRender.PSShaderName));
                context.PixelShader.SetShaderResource(0, resMgr.GetTextureSRV(blockRender.TextureName));
                context.Draw(countAndOffset[i].Item1, countAndOffset[i].Item2);
            }
            context.OutputMerger.BlendState = resMgr.GetBlendState("Transparent");
            for (int i = 0; i < blockRenderList.Count; ++i)
            {
                var blockRender = blockRenderList[i];

                if (!BlockRenderer.DelayRendering(blockRender.Type))
                {
                    continue;
                }

                context.InputAssembler.PrimitiveTopology = blockRender.Topology;
                context.PixelShader.Set(resMgr.GetPSShader(blockRender.PSShaderName));
                context.PixelShader.SetShaderResource(0, resMgr.GetTextureSRV(blockRender.TextureName));
                context.Draw(countAndOffset[i].Item1, countAndOffset[i].Item2);
            }
        }

        private List<BlockRenderer> blockRenderList = new List<BlockRenderer>();
        private List<Dictionary<Int3, BlockBody>> blockObjectsList = new List<Dictionary<Int3, BlockBody>>();

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
                "Put: " + (putBlock.HasValue ? putBlock.Value.pos.ToString() : "null") + "\r\n" +
                "Ray: " + NumRay + "\r\n" +
                "Water: " + (BlockRenderer.WaterBlockRender.Color) + "\r\n" +
                "";
        }

        // Operations
        
        // block operation -> BlockManager
        public BlockManager BlockManager { get => blockManager; }
        // block picking, mine, put
        public bool DoMineBlock(out ItemType type)
        {
            if (mineBlock.HasValue)
            {
                blockManager.TryGetBlockType(mineBlock.Value.pos, out BlockRenderer render);
                type = BlockRenderer.ToItemType(render);
                blockManager.RemoveBlock(mineBlock.Value);
                mineBlock = null;
                return true;
            }
            else
            {
                type = ItemType.GRASS_BLOCK;
                return false;
            }
        }
        public bool DoPutBlock(ItemType type)
        {
            if (putBlock.HasValue)
            {
                blockManager.AddBlock(new Block(BlockTypeUtil.FromItemType(type), putBlock.Value.pos));
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
            BlockBody? pickedBlock = null;
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
                        var pos = new Int3(x, y, z);
                        if (blockManager.TryGetBlock(pos, out BlockBody block))
                        {
                            // Skip liquid block
                            if (blockManager.HasBlockWithType(pos, BlockRenderer.WaterBlockRender))
                            {
                                continue;
                            }
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
                    if (putBlock == null || putBlock.Value.Body != put)
                    {
                        putBlock = new Block(BlockType.PUT, put.Value.pos);
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
            while ((!blockManager.HasBlock(pos) || blockManager.HasBlockWithType(pos, BlockRenderer.WaterBlockRender)) && pos.Z >= 0)
            {
                --pos.Z;
            }
            do
            {
                ++pos.Z;
                if (blockManager.HasBlockWithType(pos, BlockRenderer.WaterBlockRender))
                    break;
            } while (blockManager.HasBlock(pos));
            --pos.Z;
            return pos.Z + BlockBody.size * 0.5f;
        }
        public bool InLiquid(Int3 pos)
        {
            return blockManager.HasBlockWithType(pos, BlockRenderer.WaterBlockRender);
        }

        // Implementation

        // For DirectX
        // liquid moving texture
        private float debugCounter = 0.0f;
        public void UpdateLiquidTexture()
        {
            debugCounter += 1.0f / 64.0f;
            if (debugCounter > 1.0f) debugCounter -= 1.0f;
            BlockRenderer.WaterBlockRender.ChangeColor(Vector3.One * debugCounter);
            isDirty = true;
        }
        public void UpdateVertexBuffer(D3DDynamicVertexBuffer vertexBuffer)
        {
            if (isDirty)
            {
                isDirty = false;
                ++NumDirtyFrame;
                vertexBuffer.Reset(GetVertices());
            }
        }
        public void Render(DeviceContext context, D3DResourceManager resMgr)
        {
            int offset = 0;

            int rayRegion = rayCollection.Count * 2;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;
            context.Draw(rayRegion, offset);
            offset += rayRegion;

            blockManager.Render(context, resMgr, ref offset);
            if (putBlock.HasValue)
            {
                putBlockMgr.RemoveBlocksByType(putBlock.Value.Render);
                putBlockMgr.AddBlock(putBlock.Value);
                putBlockMgr.Render(context, resMgr, ref offset);
            }
        }

        private D3DVertex[] GetVertices()
        {
            var vertices = new D3DVertex[
                rayCollection.Count * 2 + // ray
                blockManager.GetTotalVertexCount() + // block + mine block
                (putBlock.HasValue ? putBlock.Value.Render.GetBlockVertexCount() : 0) + // put block
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
                putBlock.Value.Render.FillBlockVertices(ref vertices, ref offset, putBlock.Value.Body);

            vertexCount = vertices.Length;

            return vertices;
        }

        private bool isDirty = false;
        private int dirtyFrame = 0;

        private BlockManager blockManager;
        private BlockBody? mineBlock;
        private BlockManager putBlockMgr; // render put block
        private Block? putBlock;

        private List<Tuple<Ray, Vector3>> rayCollection;
        private int vertexCount;
    }
}
