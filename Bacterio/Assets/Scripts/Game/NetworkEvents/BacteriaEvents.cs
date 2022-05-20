using UnityEngine;
using Mirror;
using Bacterio.Network;
using Bacterio.MapObjects;

namespace Bacterio.NetworkEvents.BacteriaEvents
{
    public struct BacteriaSyncData
    {
        public BulletEvents.BacteriaBulletSyncData[] _bullets;
        public AuraEvents.BacteriaAuraSyncData[] _auras;
        public TrapEvents.BacteriaTrapSyncData[] _traps;
    }

    public struct RequestBacteriaSyncEvent : NetworkMessage
    { }

    public struct ReplyBacteriaSyncEvent : NetworkMessage
    {
        public BacteriaSyncData[] _bacteriaSyncDatas;

        public static void Send(NetworkConnection connection, ref ReplyBacteriaSyncEvent _event)
        {
            connection.Send(_event);
        }
    }

    public struct RequestBacteriaDamageEvent : NetworkMessage
    {
        public static RequestBacteriaDamageEvent _event;

        public NetworkArrayObject.UniqueId bacteriaId;
        public int damage;
        public Vector2 knockForce; //Direction + distance in 1. contains 0,0 if no knockback

        public static void Send(Bacteria bacteria, int damage)
        {
            WDebug.Assert(!NetworkInfo.IsHost, "Host tried to request bacteria damage event");

            _event.bacteriaId = bacteria._uniqueId;
            _event.damage = damage;
            _event.knockForce = Vector2.zero;
            NetworkClient.Send(_event);
        }
        public static void Send(Bacteria bacteria, int damage, Vector2 knockForce)
        {
            WDebug.Assert(!NetworkInfo.IsHost, "Host tried to request bacteria damage event");

            _event.bacteriaId = bacteria._uniqueId;
            _event.damage = damage;
            _event.knockForce = knockForce;
            NetworkClient.Send(_event);
        }
    }
}
