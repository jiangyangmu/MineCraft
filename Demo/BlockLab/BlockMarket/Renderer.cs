using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace BlockMarket
{
    class BlockRenderer
    {
        public enum RenderType
        {
            NULL,
            TEXTURE,
            TEXTURE_ALPHA,
            LIQUID,
            LINE,
        }

        public static BlockRenderer[] RenderList = new[]
        {
            new BlockRenderer(RenderType.NULL, PrimitiveTopology.Undefined, "", ""),
            new BlockRenderer(RenderType.LINE, PrimitiveTopology.LineList, "PS_Line_Block", "Grass", Vector3.One),
            new BlockRenderer(RenderType.TEXTURE, PrimitiveTopology.TriangleList, "PS_Texture_Block", "Grass"),
            new BlockRenderer(RenderType.TEXTURE, PrimitiveTopology.TriangleList, "PS_Texture_Block", "Sand"),
            new BlockRenderer(RenderType.TEXTURE, PrimitiveTopology.TriangleList, "PS_Texture_Block", "Stone"),
            new BlockRenderer(RenderType.TEXTURE, PrimitiveTopology.TriangleList, "PS_Texture_Block", "OakWood"),
            new BlockRenderer(RenderType.TEXTURE_ALPHA, PrimitiveTopology.TriangleList, "PS_Transparent_Block", "OakLeaf"),
            new BlockRenderer(RenderType.TEXTURE_ALPHA, PrimitiveTopology.TriangleList, "PS_Transparent_Block", "Glass"),
            new BlockRenderer(RenderType.LIQUID, PrimitiveTopology.TriangleList, "PS_Liquid_Block", "Water"),
        };

        public static BlockRenderer NullRender { get => RenderList[0]; }
        public static BlockRenderer PutBlockRender { get => RenderList[1]; }
        public static BlockRenderer GrassBlockRender { get => RenderList[2]; }
        public static BlockRenderer SandBlockRender { get => RenderList[3]; }
        public static BlockRenderer StoneBlockRender { get => RenderList[4]; }
        public static BlockRenderer OakWoodBlockRender { get => RenderList[5]; }
        public static BlockRenderer OakLeafBlockRender { get => RenderList[6]; }
        public static BlockRenderer GlassBlockRender { get => RenderList[7]; }
        public static BlockRenderer WaterBlockRender { get => RenderList[8]; }
        public static bool DelayRendering(RenderType type)
        {
            return type == RenderType.TEXTURE_ALPHA || type == RenderType.LIQUID;
        }

        public RenderType Type { get; }
        public PrimitiveTopology Topology { get; }
        public string PSShaderName { get; }
        public string TextureName { get; }
        public Vector3 Color { get; private set; }

        public int GetBlockVertexCount()
        {
            return D3DVertex.GetBlockVertexCount(Topology);
        }
        public void FillBlockVertices(ref D3DVertex[] v, ref int offset, BlockBody block)
        {
            var blockVertices = D3DVertex.GetBlockVertices(
                Topology,
                Vector3.UnitX,
                Vector3.UnitY,
                Vector3.UnitZ,
                (Vector3)block.pos,
                Color,
                BlockBody.size
                );
            foreach (var vertex in blockVertices)
            {
                v[offset++] = vertex;
            }
        }
        public void FillBlockVertices(ref D3DVertex[] v, ref int offset, BlockBody block, Vector3 color)
        {
            var blockVertices = D3DVertex.GetBlockVertices(
                Topology,
                Vector3.UnitX,
                Vector3.UnitY,
                Vector3.UnitZ,
                (Vector3)block.pos,
                color,
                BlockBody.size
                );
            foreach (var vertex in blockVertices)
            {
                v[offset++] = vertex;
            }
        }

        public void ChangeColor(Vector3 color)
        {
            Color = color;
        }
        public static ItemType ToItemType(BlockRenderer render)
        {
            if (render.TextureName == "Grass")
                return ItemType.GRASS_BLOCK;
            else if (render.TextureName == "Sand")
                return ItemType.SAND_BLOCK;
            else if (render.TextureName == "Stone")
                return ItemType.STONE_BLOCK;
            else if (render.TextureName == "OakWood")
                return ItemType.OAK_WOOD_BLOCK;
            else if (render.TextureName == "OakLeaf")
                return ItemType.OAK_LEAF_BLOCK;
            else if (render.TextureName == "Glass")
                return ItemType.GLASS_BLOCK;
            else
                return ItemType.GRASS_BLOCK;
        }

        private BlockRenderer(RenderType type, PrimitiveTopology topology, string shaderName, string textureName)
        {
            Type = type;
            Topology = topology;
            PSShaderName = shaderName;
            TextureName = textureName;
            Color = Vector3.One * 0.3f;
        }
        private BlockRenderer(RenderType type, PrimitiveTopology topology, string shaderName, string textureName, Vector3 color)
        {
            Type = type;
            Topology = topology;
            PSShaderName = shaderName;
            TextureName = textureName;
            Color = color;
        }
    }
}
