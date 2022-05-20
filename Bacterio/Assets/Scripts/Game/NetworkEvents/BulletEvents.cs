using UnityEngine;
using Mirror;
using Bacterio.Network;
using Bacterio.MapObjects;

namespace Bacterio.NetworkEvents.BulletEvents
{
    public struct CellBulletSyncData
    {
        public Block.UniqueId bulletId;
        public Vector2 position;
        public Vector2 direction;
        public int remainingLifetime;
    }

    public struct BacteriaBulletSyncData
    {
        public NetworkArrayObject.UniqueId _bacteriaId;
        public Block.UniqueId bulletId;
        public Vector2 position;
        public Vector2 direction;
        public int remainingLifetime;
    }
}
