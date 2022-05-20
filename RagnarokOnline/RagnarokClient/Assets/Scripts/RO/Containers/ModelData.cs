using UnityEngine;

namespace RO.Containers
{
    [System.Serializable]
    public class ModelData : ScriptableObject
    {
        [System.Serializable]
        public struct ModelAnimation
        {
            [System.Serializable]
            public struct PositionAnimation
            {
                public float updateTime;
                public Vector3 position;
            }

            [System.Serializable]
            public struct RotationAnimation
            {
                public float updateTime;
                public Quaternion quaternion;
            }

            public PositionAnimation[] posAnim;
            public RotationAnimation[] rotAnim;
            public int targetMesh;
        }

        [System.Serializable]
        public struct MeshData
        {
            //Unity can't serialize multidemnsional arrays
            [System.Serializable]
            public struct Submesh
            {
                public int[] triangles;
            }

            public Vector3[] vertices;
            public Submesh[] submeshes;
            public Vector2[] uvs;

            public int parent;
        }

        public MeshData[] meshDatas;
        public ModelAnimation[] modelAnimations;
    }
}
