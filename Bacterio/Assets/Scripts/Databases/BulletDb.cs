using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bacterio.Databases
{
    public enum BulletDbId
    {
        Default = 0,
        BurstWaveRicochetRotative,
        BurstStraightPierceRotative,

        Last = BurstStraightPierceRotative,

        None = -1
    }

    //Attributes defined in https://danyprojects.atlassian.net/wiki/spaces/BAC/pages/1769483/Bullet+Design
    public enum ShootAttribute : byte
    {
        Single       = 1 << 0,
        Cluster      = 1 << 1,
        Burst        = 1 << 2,
        Charge       = 1 << 3,

        Invalid = 0
    }

    public enum MovementAttribute : byte
    {
        Straight    = 1 << 0,
        Wave        = 1 << 1,
        Boomerang   = 1 << 2,
        Homing      = 1 << 3,
        Immovable   = 1 << 4,
        Rotative    = 1 << 5,

        Invalid = 0
    }

    public enum HitAttribute : byte
    {
        Once        = 1 << 0,
        Pierce      = 1 << 1,
        Ricochet    = 1 << 2,
        AreaCircle  = 1 << 3,
        AreaCone    = 1 << 4,
        NoDamage    = 1 << 5,

        Area        = AreaCircle | AreaCone,

        Invalid = 0
    }

    public enum ClusterShape : byte 
    {
        Line = 0,
        Circle,
        Triangle,
        Square,

        Invalid = byte.MaxValue
    }

    public sealed class BulletDb
    {
        [Serializable]
        private struct BulletDataDeserialize
        {
            public BulletData[] Bullets;
        }

        [Serializable]
        public struct BurstConfiguration
        {
            public byte burstMax;
            public int burstCooldown;
        }

        [Serializable]
        public struct ClusterConfiguration
        {
            public ClusterShape clusterShape;
            public byte clusterSize;
        }

        [Serializable]
        public struct RotationConfiguration
        {
            public float rotationSpeed;
            public float waveSpeed;
            public int maxAngle;
        }

        [Serializable]
        public struct BulletData
        {
            //Attributes
            public ShootAttribute shootAttribute;
            public MovementAttribute movementAttribute;
            public HitAttribute hitAttribute;

            //General data
            public short moveSpeed;
            public int reloadTime;
            public short startingAmmunition;
            public int regenTime;
            public short maxHits;
            public int hitDelay;

            //Specialized data
            public BurstConfiguration burstConfig;
            public ClusterConfiguration clusterConfig;
            public RotationConfiguration rotationConfig;
        }

        public static readonly string DB_FILE_NAME = "bulletDb";
        private readonly BulletData[] _bullets;

        public ref BulletData GetBulletData(BulletDbId id)
        {
            WDebug.Assert(id <= BulletDbId.Last, "Invalid ID");

            return ref _bullets[(int)id];
        }

        public BulletDb(AssetBundleProvider provider)
        {
            TextAsset jsonFile = provider.LoadMiscAsset<TextAsset>(DB_FILE_NAME);
            var data = JsonUtility.FromJson<BulletDataDeserialize>(jsonFile.text);
            _bullets = data.Bullets;

            WDebug.LogCWarn(_bullets.Length != (int)BulletDbId.Last + 1, "Loaded different number of bullets than in the enum");
        }
    }
}
