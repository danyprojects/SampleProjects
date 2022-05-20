
using System;
using UnityEngine;

namespace Bacterio.Databases
{
    public enum ShopItemId
    {
        Heal = 0,
        Revive,
        AttackUp,

        Last = AttackUp,

        Invalid = -1
    }

    public class ShopDb
    {
        [Serializable]
        private struct ShopItemDataDeserialize
        {
            public ShopItemData[] ShopItems;
        }

        [Serializable]
        public struct ShopItemData
        {
            public string name;
            public int price;
            public short multiplier;
            public short limit;
        }

        public static readonly string DB_FILE_NAME = "shopDb";
        private readonly ShopItemData[] _shopItems;

        public ref ShopItemData GetShopData(ShopItemId id)
        {
            WDebug.Assert(id <= ShopItemId.Last, "Invalid ID");

            return ref _shopItems[(int)id];
        }

        public ShopDb(AssetBundleProvider provider)
        {
            TextAsset jsonFile = provider.LoadMiscAsset<TextAsset>(DB_FILE_NAME);
            var data = JsonUtility.FromJson<ShopItemDataDeserialize>(jsonFile.text);
            _shopItems = data.ShopItems;

            WDebug.LogCWarn(_shopItems.Length != (int)ShopItemId.Last + 1, "Loaded different number of traps itens than in the enum");
        }
    }
}
