using UnityEngine;

namespace RO.Containers
{
    [System.Serializable]
    public sealed class Act : ScriptableObject
    {
        [System.Serializable]
        public struct Action
        {
            [System.Serializable]
            public struct Frame
            {
                [System.Serializable]
                public struct FrameData
                {
                    public float[] IsMirrored;
                    public Color32[] Color;
                    public Vector2[] Scale;
                    public Vector2[] PositionOffset;
                    public int[] SpriteId;
                    public int[] Rotation;

                    public FrameData(int nSprites)
                    {
                        IsMirrored = new float[nSprites];
                        Color = new Color32[nSprites];
                        Scale = new Vector2[nSprites];
                        PositionOffset = new Vector2[nSprites];
                        SpriteId = new int[nSprites];
                        Rotation = new int[nSprites];
                    }
                }
                public Vector2 attachPoint;
                public FrameData frameData;
                public int eventId;
            }
            public Frame[] Frames;
            public float delay;
        }
        public Action[] Actions;
        public Sprite[] Sprites;
        public string[] Events;
        public Texture Palette;
        public float OrderInLayer;
        public int MaxSprites;
    }
}
