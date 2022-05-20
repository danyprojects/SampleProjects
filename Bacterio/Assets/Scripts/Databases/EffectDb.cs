using System;
using UnityEngine;

namespace Bacterio.Databases
{
    public enum TrailEffectId
    {
        MoveSpeedUp = 0,

        Last = MoveSpeedUp,

        Invalid = -1
    }

    public enum ParticleEffectId
    {
        ExplodeTrap = 0,

        Last = ExplodeTrap,

        Invalid = -1
    }

    public sealed class EffectDb
    {
        [Serializable]
        private struct EffectsDataDeserialize
        {
            public TrailEffectData[] TrailEffects;
            public ParticleEffectData[] ParticleEffects;
        }

        [Serializable]
        public struct TrailEffectData
        {
            public TrailEffectId effectId;
            public int durationMs;
            public Gradient gradient;
        }

        [Serializable]
        public struct ParticleEffectData
        {
            public ParticleEffectId effectId;

            //generator
            public int durationMs;
            public bool looping;
            public ParticleSystem.MinMaxGradient startColor;
            public ParticleSystem.MinMaxCurve startLifetime;
            public ParticleSystem.MinMaxCurve startSpeed;

            //emission
            public ParticleSystem.MinMaxCurve rateOverTime;

            //velocity over lifetime
            public ParticleSystem.MinMaxCurve speedModifier;

            //Color over lifetime
            public ParticleSystem.MinMaxGradient colorOverLifetime;
        }

        public static readonly string DB_FILE_NAME = "effectDb";
        public readonly TrailEffectData[] _trailDatas;
        public readonly ParticleEffectData[] _particleDatas;

        public ref TrailEffectData GetTrailData(TrailEffectId id)
        {
            WDebug.Assert(id <= TrailEffectId.Last, "Invalid ID");

            return ref _trailDatas[(int)id];
        }

        public ref ParticleEffectData GetParticleData(ParticleEffectId id)
        {
            WDebug.Assert(id <= ParticleEffectId.Last, "Invalid ID");

            return ref _particleDatas[(int)id];
        }

        public EffectDb(AssetBundleProvider provider)
        {
            TextAsset jsonFile = provider.LoadMiscAsset<TextAsset>(DB_FILE_NAME);
            var data = JsonUtility.FromJson<EffectsDataDeserialize>(jsonFile.text);
            _trailDatas = data.TrailEffects;
            _particleDatas = data.ParticleEffects;

            WDebug.LogCWarn(_trailDatas.Length != (int)TrailEffectId.Last + 1, "Loaded different number of trail effects than in the enum");
            WDebug.LogCWarn(_particleDatas.Length != (int)ParticleEffectId.Last + 1, "Loaded different number of particle effects than in the enum");
        }
    }
}
