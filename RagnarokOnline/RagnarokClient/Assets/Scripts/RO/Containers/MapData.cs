using UnityEngine;

namespace RO.Containers
{
    [System.Serializable]
    public sealed class MapData : ScriptableObject
    {
        [System.Serializable]
        public struct Tile
        {
            private enum TileFlags : int
            {
                Walkable = 1 << 0,
                Snipable = 1 << 1,
                Water = 1 << 2,
                NoVending = 1 << 3,
                NoIcewall = 1 << 4,
                Basilica = 1 << 5,
                LandProtect = 1 << 6,
                Icewall = 1 << 7
            }

            public byte flags;

            public bool IsWalkable
            {
                get
                {
                    return (flags & (int)TileFlags.Walkable) > 0;
                }
                set
                {
                    flags = (byte)(value ? (flags | (int)TileFlags.Walkable) : (flags & ~(int)TileFlags.Walkable));
                }
            }

            public bool IsSnipable
            {
                get
                {
                    return (flags & (int)TileFlags.Snipable) > 0;
                }
                set
                {
                    flags = (byte)(value ? (flags | (int)TileFlags.Snipable) : (flags & ~(int)TileFlags.Snipable));
                }
            }

            public bool HasWater
            {
                get
                {
                    return (flags & (int)TileFlags.Water) > 0;
                }
                set
                {
                    flags = (byte)(value ? (flags | (int)TileFlags.Water) : (flags & ~(int)TileFlags.Water));
                }
            }

            public bool IsNoVending
            {
                get
                {
                    return (flags & (int)TileFlags.NoVending) > 0;
                }
                set
                {
                    flags = (byte)(value ? (flags | (int)TileFlags.NoVending) : (flags & ~(int)TileFlags.NoVending));
                }
            }

            public bool IsNoIcewall
            {
                get
                {
                    return (flags & (int)TileFlags.NoIcewall) > 0;
                }
                set
                {
                    flags = (byte)(value ? (flags | (int)TileFlags.NoIcewall) : (flags & ~(int)TileFlags.NoIcewall));
                }
            }

            public bool HasBasilica
            {
                get
                {
                    return (flags & (int)TileFlags.Basilica) > 0;
                }
                set
                {
                    flags = (byte)(value ? (flags | (int)TileFlags.Basilica) : (flags & ~(int)TileFlags.Basilica));
                }
            }

            public bool HasLandProtect
            {
                get
                {
                    return (flags & (int)TileFlags.LandProtect) > 0;
                }
                set
                {
                    flags = (byte)(value ? (flags | (int)TileFlags.LandProtect) : (flags & ~(int)TileFlags.LandProtect));
                }
            }

            public bool HasIcewall
            {
                get
                {
                    return (flags & (int)TileFlags.Icewall) > 0;
                }
                set
                {
                    flags = (byte)(value ? (flags | (int)TileFlags.Icewall) : (flags & ~(int)TileFlags.Icewall));
                }
            }

            public bool IsSeeThrough
            {
                get { return (flags & (int)(TileFlags.Snipable | TileFlags.Walkable)) > 0; }
            }
        }

        [System.Serializable]
        public struct Water
        {
            public int type;
            public float height;
            public float speed;
            public float pitch;
            public int animationSpeed;
        }

        [System.Serializable]
        public struct Lighting
        {
            public int longitude;
            public int latitude;
            public Color ambient;
            public Color diffuse;
            public float ambientIntensity;
        }

        //Unity doesnt support readonly serialization so we need to duplicate these to keep them unsettable but readable
        public int Id { get { return _id; } }
        public Tile[] Tiles { get { return _tiles; } }
        public int Width { get { return _width; } }
        public int Height { get { return _height; } }
        public Water WaterInfo { get { return _waterInfo; } }
        public Lighting LightingInfo { get { return _lightingInfo; } }
        public Texture2D Lightmap { get { return _lightmap; } }

        //Need the private versions for serialization while keeping them "readonly". They will be set through reflection
        [SerializeField] private int _id = default;
        [SerializeField] private Tile[] _tiles = default;
        [SerializeField] private int _width = default, _height = default;
        [SerializeField] private Water _waterInfo = default;
        [SerializeField] private Lighting _lightingInfo = default;
        [SerializeField] private Texture2D _lightmap = default;

        public Tile GetTile(int x, int y)
        {
            return _tiles[x + y * Width];
        }

        public Tile GetTile(in Vector2Int cell)
        {
            return _tiles[cell.x + cell.y * Width];
        }
    }
}