using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bacterio.Databases
{
    public enum AuraDbId
    {
        CellWoundDetection = 0,
        Test,

        Last = Test,

        None = -1
    }

    public sealed class AuraDb
    {
        [Serializable]
        private struct AuraDataDeserialize
        {
            public AuraData[] Auras;
        }

        [Serializable]
        public struct AuraData
        {
            private enum Flags : byte
            {
                IsVisible = 1 << 0,
                IsInteractable = 1 << 1,

                All = 0xFF,
                None = 0
            }

            public int mask;
            public float radius;
            private byte _flags;

            public bool IsVisible { get { return (_flags & (byte)Flags.IsVisible) > 0; } }
            public bool IsInteractable { get { return (_flags & (byte)Flags.IsInteractable) > 0; } }
        }

        public static readonly string DB_FILE_NAME = "auraDb";
        private readonly AuraData[] _auras;

        public ref AuraData GetAuraData(AuraDbId id)
        {
            WDebug.Assert(id <= AuraDbId.Last, "Invalid ID");

            return ref _auras[(int)id];
        }

        public AuraDb(AssetBundleProvider provider)
        {
            TextAsset jsonFile = provider.LoadMiscAsset<TextAsset>(DB_FILE_NAME);
            var data = JsonUtility.FromJson<AuraDataDeserialize>(jsonFile.text);
            _auras = data.Auras;

            WDebug.LogCWarn(_auras.Length != (int)AuraDbId.Last + 1, "Loaded different number of auras than in the enum");
        }
    }
}
