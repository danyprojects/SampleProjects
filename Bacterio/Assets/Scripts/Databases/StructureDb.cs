using System;
using UnityEngine;

namespace Bacterio.Databases
{
    public enum StructureDbId
    {
        Heart = 0,
        Wound,

        Last = Wound,

        None = -1
    }

    public sealed class StructureDb
    {
        [Serializable]
        private struct StructureDataDeserialize
        {
            public StructureData[] Structures;
            public string[] Meshes;
        }

        [Serializable]
        public struct StructureData
        {
            public Common.Pathfinder.PathNode[] _pathNodes;
            public bool _hasTerritory;
            public bool _isSpawner;
            public string _spriteName;
        }

        public static readonly string DB_FILE_NAME = "structureDb";
        private readonly StructureData[] _structures;
        private readonly string[] _territoryMeshNames;

        public ref StructureData GetStructureData(StructureDbId id)
        {
            WDebug.Assert(id <= StructureDbId.Last, "Invalid ID");

            return ref _structures[(int)id];
        }

        public int TerritoryNameCount { get { return _territoryMeshNames.Length; } }
        public string GetTerritoryMeshName(int index)
        {
            WDebug.Assert(index < _territoryMeshNames.Length, "Got request mesh name with index greater than amount we have");
            return _territoryMeshNames[index];
        }

        public string GetTerritoryMeshName(Common.MTRandom random)
        {
            return _territoryMeshNames[random.Range(0, _territoryMeshNames.Length - 1)];
        }

        public StructureDb(AssetBundleProvider provider)
        {
            TextAsset jsonFile = provider.LoadMiscAsset<TextAsset>(DB_FILE_NAME);
            var data = JsonUtility.FromJson<StructureDataDeserialize>(jsonFile.text);
            _structures = data.Structures;
            _territoryMeshNames = data.Meshes;

            WDebug.LogCWarn(_structures.Length != (int)StructureDbId.Last + 1, "Loaded different number of structures than in the enum");
        }
    }
}
