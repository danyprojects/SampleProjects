using UnityEngine;
using Mirror;
using Bacterio.Network;
using Bacterio.MapObjects;

namespace Bacterio.NetworkEvents.PillEvents
{
    public struct RequestPillFullSyncEvent : NetworkMessage
    { }

    public struct ReplyPillFullSyncEvent : NetworkMessage
    {
        public Block.UniqueId[] pillIds;
        public Databases.PillDbId[] dbIds;
        public Vector2[] positions;

        public void Send(NetworkConnection connection)
        {
            connection.Send(this);
        }
    }

    public struct PillDespawnedEvent : NetworkMessage
    {
        public static PillDespawnedEvent _event;

        public Block.UniqueId pillId;

        public static void Send(Pill pill)
        {
            WDebug.Assert(NetworkInfo.IsHost, "Client tried to despawn pill");

            _event.pillId = pill._uniqueId;

            NetworkServer.SendToReady(_event);
        }
    }

    public struct PillSpawnedEvent : NetworkMessage
    {
        public static PillSpawnedEvent _event;

        public Block.UniqueId pillId;
        public Databases.PillDbId dbId;
        public Vector2 position;

        public static void Send(Pill pill, Vector2 position)
        {
            WDebug.Assert(NetworkInfo.IsHost, "Client tried to spawn pill");

            _event.pillId = pill._uniqueId;
            _event.dbId = pill._dbId;
            _event.position = position;

            if (NetworkInfo.IsHost)
                NetworkServer.SendToReady(_event);
            else
                NetworkClient.Send(_event);
        }
    }

    public struct RequestCellConsumePill : NetworkMessage
    {
        public static RequestCellConsumePill _event;

        public Block.UniqueId pillId;

        public static void Send(Pill pill)
        {
            WDebug.Assert(!NetworkInfo.IsHost, "Host requested cell consume pill");

            _event.pillId = pill._uniqueId;

            NetworkClient.Send(_event);
        }
    }

    public struct NotifyCellTriggeredPill : NetworkMessage
    {
        public static NotifyCellTriggeredPill _event;

        public byte cellIndex;
        public Block.UniqueId pillId;

        public static void Send(Cell cell, Pill pill)
        {
            WDebug.Assert(NetworkInfo.IsHost, "Client notified cell consumed pill");

            _event.cellIndex = (byte)cell._uniqueId._index;
            _event.pillId = pill._uniqueId;

            NetworkServer.SendToReady(_event);
        }
    }
}
