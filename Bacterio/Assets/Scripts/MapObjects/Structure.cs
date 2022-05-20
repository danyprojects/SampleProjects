using UnityEngine;
using Bacterio.Databases;

namespace Bacterio.MapObjects
{
    public class Structure : Block
    {
        public struct WoundData 
        {
            public int hp;
            public int nextHealByLocalCellMs;
        }

        public struct SpawnerData
        {
            public int nextBacteriaSpawnMs;
        }

        public StructureDbId _dbId { get; private set; }
        public Animators.StructureAnimator _animator;
        public Territory _territory;
        public bool _isSpawner;        


        public WoundData _woundData;
        public SpawnerData _spawnerData;

        public bool ContainsPoint(Vector2 point)
        {
            return Physics2D.Raycast(point, Vector2.one, 0.0001f, Constants.STRUCTURES_MASK).collider != null;
        }

        public void Configure(StructureDbId dbId, Vector2 position)
        {
            ref var structureData = ref GlobalContext.structureDb.GetStructureData(dbId);

            transform.position = position;
            _dbId = dbId;
            _isSpawner = structureData._isSpawner;

            //Init special datas according to dbId
            if (dbId == StructureDbId.Wound)
            {
                _woundData.hp = Constants.DEFAULT_WOUND_HP;
                _woundData.nextHealByLocalCellMs = 0;
            }

            if (structureData._isSpawner)
            {
                _spawnerData.nextBacteriaSpawnMs = GlobalContext.localTimeMs + Constants.DEFAULT_BACTERIA_SPAWN_INTERVAL;
            }
        }
    }
}
