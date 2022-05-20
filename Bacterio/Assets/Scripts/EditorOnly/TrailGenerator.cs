using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BacterioEditor
{
#if UNITY_EDITOR
    public sealed class TrailGenerator : MonoBehaviour
    {
        public TrailRenderer _trailRenderer;

        public Bacterio.Databases.TrailEffectId _trailId;
        public int _durationMs;
    }
#endif
}
