using UnityEngine;

namespace RO.Containers
{
    [System.Serializable]
    public sealed class Str : ScriptableObject
    {
        [System.Serializable]
        public struct Layer
        {
            [System.Serializable]
            public struct Frame
            {
                public int type;
                public int frameIndex;
                public int textureId;
                public int animationType;
                public float delay;
                public float rotation;
                public float[] color;
                public Vector2 offset;
                public Material[] materials;
                public Vector3[] vertices;
            }
            public Frame[] Frames;
            public Texture2D[] Textures;
        }

        public Layer[] Layers;
        public int TotalFrames;
    }
}
