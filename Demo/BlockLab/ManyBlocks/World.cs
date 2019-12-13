using System;
using System.Collections.Generic;

using SharpDX;
using SharpDX.Mathematics;

namespace WorldOfBlocks
{
    public struct Block
    {
        public Vector4 front_pos_0; public Vector4 front_color_0; // Front
        public Vector4 front_pos_1; public Vector4 front_color_1;
        public Vector4 front_pos_2; public Vector4 front_color_2;
        public Vector4 front_pos_3; public Vector4 front_color_3;
        public Vector4 front_pos_4; public Vector4 front_color_4;
        public Vector4 front_pos_5; public Vector4 front_color_5;

        public Vector4 back_pos_0; public Vector4 back_color_0; // BACK 
        public Vector4 back_pos_1; public Vector4 back_color_1;
        public Vector4 back_pos_2; public Vector4 back_color_2;
        public Vector4 back_pos_3; public Vector4 back_color_3;
        public Vector4 back_pos_4; public Vector4 back_color_4;
        public Vector4 back_pos_5; public Vector4 back_color_5;

        public Vector4 top_pos_0; public Vector4 top_color_0; // Top
        public Vector4 top_pos_1; public Vector4 top_color_1;
        public Vector4 top_pos_2; public Vector4 top_color_2;
        public Vector4 top_pos_3; public Vector4 top_color_3;
        public Vector4 top_pos_4; public Vector4 top_color_4;
        public Vector4 top_pos_5; public Vector4 top_color_5;

        public Vector4 bottom_pos_0; public Vector4 bottom_color_0; // Bottom
        public Vector4 bottom_pos_1; public Vector4 bottom_color_1;
        public Vector4 bottom_pos_2; public Vector4 bottom_color_2;
        public Vector4 bottom_pos_3; public Vector4 bottom_color_3;
        public Vector4 bottom_pos_4; public Vector4 bottom_color_4;
        public Vector4 bottom_pos_5; public Vector4 bottom_color_5;

        public Vector4 left_pos_0; public Vector4 left_color_0; // Left
        public Vector4 left_pos_1; public Vector4 left_color_1;
        public Vector4 left_pos_2; public Vector4 left_color_2;
        public Vector4 left_pos_3; public Vector4 left_color_3;
        public Vector4 left_pos_4; public Vector4 left_color_4;
        public Vector4 left_pos_5; public Vector4 left_color_5;

        public Vector4 right_pos_0; public Vector4 right_color_0; // Right
        public Vector4 right_pos_1; public Vector4 right_color_1;
        public Vector4 right_pos_2; public Vector4 right_color_2;
        public Vector4 right_pos_3; public Vector4 right_color_3;
        public Vector4 right_pos_4; public Vector4 right_color_4;
        public Vector4 right_pos_5; public Vector4 right_color_5;

        public static Block Create(Vector3 pos, Vector3 color)
        {
            Block block;

            block.front_pos_0 = new Vector4(pos.X + -1.0f, pos.Y + -1.0f, pos.Z + -1.0f, 1.0f); block.front_color_0 = new Vector4(color.X, color.Y, color.Z, 1.0f); // Front
            block.front_pos_1 = new Vector4(pos.X + -1.0f, pos.Y + 1.0f, pos.Z + -1.0f, 1.0f); block.front_color_1 = new Vector4(color.X, color.Y, color.Z, 1.0f);
            block.front_pos_2 = new Vector4(pos.X + 1.0f, pos.Y + 1.0f, pos.Z + -1.0f, 1.0f); block.front_color_2 = new Vector4(color.X, color.Y, color.Z, 1.0f);
            block.front_pos_3 = new Vector4(pos.X + -1.0f, pos.Y + -1.0f, pos.Z + -1.0f, 1.0f); block.front_color_3 = new Vector4(color.X, color.Y, color.Z, 1.0f);
            block.front_pos_4 = new Vector4(pos.X + 1.0f, pos.Y + 1.0f, pos.Z + -1.0f, 1.0f); block.front_color_4 = new Vector4(color.X, color.Y, color.Z, 1.0f);
            block.front_pos_5 = new Vector4(pos.X + 1.0f, pos.Y + -1.0f, pos.Z + -1.0f, 1.0f); block.front_color_5 = new Vector4(color.X, color.Y, color.Z, 1.0f);

            block.back_pos_0 = new Vector4(pos.X + -1.0f, pos.Y + -1.0f, pos.Z + 1.0f, 1.0f); block.back_color_0 = new Vector4(color.X, color.Y, color.Z, 1.0f); // BACK 
            block.back_pos_1 = new Vector4(pos.X + 1.0f, pos.Y + 1.0f, pos.Z + 1.0f, 1.0f); block.back_color_1 = new Vector4(color.X, color.Y, color.Z, 1.0f);
            block.back_pos_2 = new Vector4(pos.X + -1.0f, pos.Y + 1.0f, pos.Z + 1.0f, 1.0f); block.back_color_2 = new Vector4(color.X, color.Y, color.Z, 1.0f);
            block.back_pos_3 = new Vector4(pos.X + -1.0f, pos.Y + -1.0f, pos.Z + 1.0f, 1.0f); block.back_color_3 = new Vector4(color.X, color.Y, color.Z, 1.0f);
            block.back_pos_4 = new Vector4(pos.X + 1.0f, pos.Y + -1.0f, pos.Z + 1.0f, 1.0f); block.back_color_4 = new Vector4(color.X, color.Y, color.Z, 1.0f);
            block.back_pos_5 = new Vector4(pos.X + 1.0f, pos.Y + 1.0f, pos.Z + 1.0f, 1.0f); block.back_color_5 = new Vector4(color.X, color.Y, color.Z, 1.0f);

            block.top_pos_0 = new Vector4(pos.X + -1.0f, pos.Y + 1.0f, pos.Z + -1.0f, 1.0f); block.top_color_0 = new Vector4(color.X, color.Y, color.Z, 1.0f); // Top
            block.top_pos_1 = new Vector4(pos.X + -1.0f, pos.Y + 1.0f, pos.Z + 1.0f, 1.0f); block.top_color_1 = new Vector4(color.X, color.Y, color.Z, 1.0f);
            block.top_pos_2 = new Vector4(pos.X + 1.0f, pos.Y + 1.0f, pos.Z + 1.0f, 1.0f); block.top_color_2 = new Vector4(color.X, color.Y, color.Z, 1.0f);
            block.top_pos_3 = new Vector4(pos.X + -1.0f, pos.Y + 1.0f, pos.Z + -1.0f, 1.0f); block.top_color_3 = new Vector4(color.X, color.Y, color.Z, 1.0f);
            block.top_pos_4 = new Vector4(pos.X + 1.0f, pos.Y + 1.0f, pos.Z + 1.0f, 1.0f); block.top_color_4 = new Vector4(color.X, color.Y, color.Z, 1.0f);
            block.top_pos_5 = new Vector4(pos.X + 1.0f, pos.Y + 1.0f, pos.Z + -1.0f, 1.0f); block.top_color_5 = new Vector4(color.X, color.Y, color.Z, 1.0f);

            block.bottom_pos_0 = new Vector4(pos.X + -1.0f, pos.Y + -1.0f, pos.Z + -1.0f, 1.0f); block.bottom_color_0 = new Vector4(color.X, color.Y, color.Z, 1.0f); // Bottom
            block.bottom_pos_1 = new Vector4(pos.X + 1.0f, pos.Y + -1.0f, pos.Z + 1.0f, 1.0f); block.bottom_color_1 = new Vector4(color.X, color.Y, color.Z, 1.0f);
            block.bottom_pos_2 = new Vector4(pos.X + -1.0f, pos.Y + -1.0f, pos.Z + 1.0f, 1.0f); block.bottom_color_2 = new Vector4(color.X, color.Y, color.Z, 1.0f);
            block.bottom_pos_3 = new Vector4(pos.X + -1.0f, pos.Y + -1.0f, pos.Z + -1.0f, 1.0f); block.bottom_color_3 = new Vector4(color.X, color.Y, color.Z, 1.0f);
            block.bottom_pos_4 = new Vector4(pos.X + 1.0f, pos.Y + -1.0f, pos.Z + -1.0f, 1.0f); block.bottom_color_4 = new Vector4(color.X, color.Y, color.Z, 1.0f);
            block.bottom_pos_5 = new Vector4(pos.X + 1.0f, pos.Y + -1.0f, pos.Z + 1.0f, 1.0f); block.bottom_color_5 = new Vector4(color.X, color.Y, color.Z, 1.0f);

            block.left_pos_0 = new Vector4(pos.X + -1.0f, pos.Y + -1.0f, pos.Z + -1.0f, 1.0f); block.left_color_0 = new Vector4(color.X, color.Y, color.Z, 1.0f); // Left
            block.left_pos_1 = new Vector4(pos.X + -1.0f, pos.Y + -1.0f, pos.Z + 1.0f, 1.0f); block.left_color_1 = new Vector4(color.X, color.Y, color.Z, 1.0f);
            block.left_pos_2 = new Vector4(pos.X + -1.0f, pos.Y + 1.0f, pos.Z + 1.0f, 1.0f); block.left_color_2 = new Vector4(color.X, color.Y, color.Z, 1.0f);
            block.left_pos_3 = new Vector4(pos.X + -1.0f, pos.Y + -1.0f, pos.Z + -1.0f, 1.0f); block.left_color_3 = new Vector4(color.X, color.Y, color.Z, 1.0f);
            block.left_pos_4 = new Vector4(pos.X + -1.0f, pos.Y + 1.0f, pos.Z + 1.0f, 1.0f); block.left_color_4 = new Vector4(color.X, color.Y, color.Z, 1.0f);
            block.left_pos_5 = new Vector4(pos.X + -1.0f, pos.Y + 1.0f, pos.Z + -1.0f, 1.0f); block.left_color_5 = new Vector4(color.X, color.Y, color.Z, 1.0f);

            block.right_pos_0 = new Vector4(pos.X + 1.0f, pos.Y + -1.0f, pos.Z + -1.0f, 1.0f); block.right_color_0 = new Vector4(color.X, color.Y, color.Z, 1.0f); // Right
            block.right_pos_1 = new Vector4(pos.X + 1.0f, pos.Y + 1.0f, pos.Z + 1.0f, 1.0f); block.right_color_1 = new Vector4(color.X, color.Y, color.Z, 1.0f);
            block.right_pos_2 = new Vector4(pos.X + 1.0f, pos.Y + -1.0f, pos.Z + 1.0f, 1.0f); block.right_color_2 = new Vector4(color.X, color.Y, color.Z, 1.0f);
            block.right_pos_3 = new Vector4(pos.X + 1.0f, pos.Y + -1.0f, pos.Z + -1.0f, 1.0f); block.right_color_3 = new Vector4(color.X, color.Y, color.Z, 1.0f);
            block.right_pos_4 = new Vector4(pos.X + 1.0f, pos.Y + 1.0f, pos.Z + -1.0f, 1.0f); block.right_color_4 = new Vector4(color.X, color.Y, color.Z, 1.0f);
            block.right_pos_5 = new Vector4(pos.X + 1.0f, pos.Y + 1.0f, pos.Z + 1.0f, 1.0f); block.right_color_5 = new Vector4(color.X, color.Y, color.Z, 1.0f);

            return block;
        }
    }
    public class World
    {
        public World()
        {
            Name = "Empty World";
            Blocks = new Block[0];
        }
        public World(string name, int length, int width, int height)
        {
            Name = name;
            Length = length;
            Width = width;
            Height = height;
            Blocks = new Block[length * width * height];
            int i = 0;
            var GREY = new Vector3(0.3f, 0.3f, 0.3f);
            var WHITE = new Vector3(1.0f, 1.0f, 1.0f);
            for (int h = 0; h < height; ++h)
            {
                for (int w = 0; w < width; ++w)
                {
                    for (int l = 0; l < length; ++l)
                    {
                        Blocks[i] = Block.Create(
                            new Vector3(l * 2.0f - length + 1.0f, w * 2.0f - width + 1.0f, h * 2.0f),
                            (l + w + h) % 2 == 0 ? GREY : WHITE);
                        ++i;
                    }
                }
            }
        }

        public int Length { get;  }
        public int Width { get; }
        public int Height { get; }
        public string Name { get; set; }
        public Block[] Blocks { get; set; }
    }
}
