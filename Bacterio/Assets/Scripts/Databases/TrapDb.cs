
using UnityEngine;
using System;

namespace Bacterio.Databases
{
    public enum TrapDbId
    {
        ExplodingTrap = 0,

        Last = ExplodingTrap,

        None = -1
    }

    public sealed class TrapDb
    {
        [Serializable]
        private struct TrapDataDeserialize
        {
            public TrapData[] Traps;
        }

        [Serializable]
        public struct TrapData
        {
            public enum TrapType
            {
                Offensive = 0,
                Defensive
            }

            public TrapType trapType;
            public int multiplier;
            public float radius;
        }

        public static readonly string DB_FILE_NAME = "trapDb";
        private readonly TrapData[] _traps;

        public ref TrapData GetTrapData(TrapDbId id)
        {
            WDebug.Assert(id <= TrapDbId.Last, "Invalid ID");

            return ref _traps[(int)id];
        }

        public TrapDb(AssetBundleProvider provider)
        {
            TextAsset jsonFile = provider.LoadMiscAsset<TextAsset>(DB_FILE_NAME);
            var data = JsonUtility.FromJson<TrapDataDeserialize>(jsonFile.text);
            _traps = data.Traps;

            WDebug.LogCWarn(_traps.Length != (int)TrapDbId.Last + 1, "Loaded different number of traps than in the enum");
        }
    }
}
