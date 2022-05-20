using System;
using UnityEngine;

namespace EditorTools
{
    [Serializable]
    public sealed class TransformFixes : ScriptableObject
    {
        [Serializable]
        public struct Fix
        {
            public int objectIndex;
            public Vector3 positionFix;
            public Vector3 rotationFix;

            public Fix[] childFixes;
        }

        public Fix[] fixes;
    }
}