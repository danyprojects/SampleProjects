using UnityEngine;
using Mirror;
using Bacterio.MapObjects;
using Bacterio.Network;

namespace Bacterio.NetworkEvents.CellEvents
{
    public struct CellSyncData
    {
        public NetworkPlayerObject.UniqueId _cellId;
        public Cell.Status _status;
        public long _remainingRespawnTimeMs;
        public BulletEvents.CellBulletSyncData[] _bullets;
        public AuraEvents.CellAuraSyncData[] _auras;
        public TrapEvents.CellTrapSyncData[] _traps;
    }

    public struct RequestCellSyncEvent : NetworkMessage
    { }

    public struct ReplyCellSyncEvent : NetworkMessage
    {
        public CellSyncData[] _cellSyncDatas;

        public static void Send(NetworkConnection connection, ref ReplyCellSyncEvent _event)
        {
            connection.Send(_event);
        }
    }

    public struct RequestCellDamageEvent : NetworkMessage
    {
        public static RequestCellDamageEvent _event;

        public int damage;
        public Vector2 knockForce; //Direction + distance in 1. contains 0,0 if no knockback

        public static void Send(NetworkConnection connection, int damage)
        {
            _event.damage = damage;
            _event.knockForce = Vector2.zero;
            connection.Send(_event);
        }
        public static void Send(NetworkConnection connection, int damage, Vector2 knockForce)
        {
            _event.damage = damage;
            _event.knockForce = knockForce;
            connection.Send(_event);
        }
    }

    public struct NotifyShootEvent : NetworkMessage
    {
        public static NotifyShootEvent _event;

        public byte cellIndex;
        public bool isShooting;
        public Block.UniqueId bulletUniqueId;
        public Vector2 direction;

        public static void SendShootStop(byte cellIndex)
        {
            _event.cellIndex = cellIndex;
            _event.isShooting = false;

            if (NetworkInfo.IsHost)
                NetworkServer.SendToReady(_event);
            else
                NetworkClient.Send(_event);
        }

        public static void SendShotBullet(byte cellIndex, Vector2 direction, Block.UniqueId bulletUniqueId)
        {
            _event.cellIndex = cellIndex;
            _event.isShooting = true;
            _event.direction = direction;
            _event.bulletUniqueId = bulletUniqueId;

            if (NetworkInfo.IsHost)
                NetworkServer.SendToReady(_event);
            else
                NetworkClient.Send(_event);
        }
    }

    public struct NotifyAddUpgradePoints : NetworkMessage
    {
        public static NotifyAddUpgradePoints _event;

        public int additionalPoints;

        public static void Send(int additionalPoints)
        {
            _event.additionalPoints = additionalPoints;

            NetworkServer.SendToReady(_event);
        }
    }
}
