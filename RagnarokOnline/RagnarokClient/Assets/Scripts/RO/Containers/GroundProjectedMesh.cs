using System;
using UnityEngine;

namespace RO.Containers
{
    [Serializable]
    public sealed class GroundProjectedMesh : ScriptableObject
    {
        public Mesh mesh;
        public Vector3[] vertices;
        public int min, max;
    }
}
