using System;
using UnityEngine;

namespace RO.Containers
{
    [Serializable]
    public sealed class CursorAnimationData : ScriptableObject
    {
        [Serializable]
        public struct CursorFrame
        {
            public int textureId;
            public Vector2 hotspot;
        }

        [Serializable]
        public struct CursorAnimation
        {
            public Texture2D[] textures;
            public CursorFrame[] cursorFrames;
        }

        public CursorAnimation[] _cursorAnimations = null;
    }
}
