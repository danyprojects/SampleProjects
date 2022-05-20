using System;
using UnityEngine;

namespace Bacterio.Databases
{
    public enum CellDbId
    {
        Default = 0,

        Last = Default,

        None = -1
    }

    public sealed class CellDb
    {
        [Serializable]
        private struct CellDataDeserialize
        {
            public CellData[] Cells;
        }

        [Serializable]
        public struct CellData
        {
            public BulletDbId bulletDbId;
            public int maxHp;
            public short moveSpeed;
            public short atk;
            public short def;
        }

        public static readonly string DB_FILE_NAME = "cellDb";
        private readonly CellData[] _cells;

        public ref CellData GetCellData(CellDbId id)
        {
            WDebug.Assert(id <= CellDbId.Last, "Invalid ID");

            return ref _cells[(int)id];
        }

        public CellDb(AssetBundleProvider provider)
        {
            TextAsset jsonFile = provider.LoadMiscAsset<TextAsset>(DB_FILE_NAME);
            var data = JsonUtility.FromJson<CellDataDeserialize>(jsonFile.text);
            _cells = data.Cells;

            WDebug.LogCWarn(_cells.Length != (int)CellDbId.Last + 1, "Loaded different number of cells than in the enum");
        }
    }
}
