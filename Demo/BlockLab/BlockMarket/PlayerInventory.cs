using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlockMarket
{
    enum ItemType
    {
        GRASS_BLOCK,
        SAND_BLOCK,
        STONE_BLOCK,
        OAK_WOOD_BLOCK,
        OAK_LEAF_BLOCK,
        GLASS_BLOCK,
    }

    class PlayerInventory
    {
        public PlayerInventory()
        {
        }

        public ItemType SelectedItem { get => inventory[selectedIndex].Item1; }
        public int SelectedItemIndex { get => selectedIndex; set => selectedIndex = Math.Max(0, Math.Min(inventory.Count - 1, value)); }
        public int SelectedCount { get => inventory[selectedIndex].Item2; }

        public void AddItem(ItemType type)
        {
            int index = inventory.FindIndex(item => item.Item1 == type);
            var pair = inventory[index];
            inventory[index] = new Tuple<ItemType, int>(pair.Item1, pair.Item2 + 1);
        }
        public void RemoveItem(ItemType type)
        {
            int index = inventory.FindIndex(item => item.Item1 == type);
            var pair = inventory[index];
            if (pair.Item2 > 0)
            {
                inventory[index] = new Tuple<ItemType, int>(pair.Item1, pair.Item2 - 1);
            }
        }

        public string[] GetItemNameList()
        {
            return new[]
            {
                "Grass",
                "Sand",
                "Stone",
                "Oak Wood",
                "Oak Leaf",
                "Glass",
            };
        }
        public int[] GetItemCountList()
        {
            var result = from t in inventory select t.Item2;
            return result.ToArray();
        }

        private int selectedIndex = 0;
        private List<Tuple<ItemType, int>> inventory = new List<Tuple<ItemType, int>>()
        {
            new Tuple<ItemType, int>( ItemType.GRASS_BLOCK, 0 ),
            new Tuple<ItemType, int>( ItemType.SAND_BLOCK, 0 ),
            new Tuple<ItemType, int>( ItemType.STONE_BLOCK, 0 ),
            new Tuple<ItemType, int>( ItemType.OAK_WOOD_BLOCK, 0 ),
            new Tuple<ItemType, int>( ItemType.OAK_LEAF_BLOCK, 0 ),
            new Tuple<ItemType, int>( ItemType.GLASS_BLOCK, 0 ),
        };
    }
}
