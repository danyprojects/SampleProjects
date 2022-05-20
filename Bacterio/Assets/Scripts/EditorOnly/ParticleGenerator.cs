using UnityEngine;

namespace BacterioEditor
{
#if UNITY_EDITOR
    public sealed class ParticleGenerator : MonoBehaviour
    {
        public ParticleSystem _particleSystem;

        public Bacterio.Databases.ParticleEffectId _particleId;
    }
#endif
}
