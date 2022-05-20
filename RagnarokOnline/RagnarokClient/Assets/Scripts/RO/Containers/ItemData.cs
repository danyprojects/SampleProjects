using UnityEngine;

namespace RO.Containers
{
    [System.Serializable]
    public sealed class ItemData : ScriptableObject
    {
        public int id;
        public new string name;
        public string description;
        public Texture2D icon;
        public Texture2D palette;
        public Texture2D viewImage;
    }
}
