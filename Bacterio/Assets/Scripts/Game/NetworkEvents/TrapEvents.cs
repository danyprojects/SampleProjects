using UnityEngine;
using Mirror;
using Bacterio.Network;
using Bacterio.MapObjects;

namespace Bacterio.NetworkEvents.TrapEvents
{
    public struct CellTrapSyncData
    {
        public Block.UniqueId trapId;
        public Databases.TrapDbId dbId;
        public byte cellOwnerIndex;
        public Vector2 position;
        public int attackPower;
    }

    public struct BacteriaTrapSyncData
    {
        public NetworkArrayObject.UniqueId ownerBacteriaId;
        public Block.UniqueId trapId;
        public Databases.TrapDbId dbId;
        public Vector2 position;
        public int attackPower;
    }

    public struct TrapSpawnedEvent : NetworkMessage
    {
        public static TrapSpawnedEvent _event;

        public Block.UniqueId trapId;
        public Databases.TrapDbId dbId;
        public Vector2 position;
        public byte cellOwnerIndex;
        public NetworkArrayObject.UniqueId bacteriaOwnerId;
        public int attackPower;

        public static void Send(Cell cell, Trap trap)
        {
            _event.trapId = trap._uniqueId;
            _event.dbId = trap._dbId;
            _event.cellOwnerIndex = (byte)cell._uniqueId._index;
            _event.bacteriaOwnerId = NetworkArrayObject.UniqueId.invalid;
            _event.position = trap.transform.position;
            _event.attackPower = trap._attackPower;

            if (NetworkInfo.IsHost)
                NetworkServer.SendToReady(_event);
            else
                NetworkClient.Send(_event);
        }

        public static void Send(Bacteria bacteria, Trap trap)
        {
            WDebug.Assert(NetworkInfo.IsHost, "Client attempted to send trap spawned event from bacteria");

            _event.trapId = trap._uniqueId;
            _event.dbId = trap._dbId;
            _event.cellOwnerIndex = Constants.INVALID_CELL_INDEX;
            _event.bacteriaOwnerId = bacteria._uniqueId;
            _event.position = trap.transform.position;
            _event.attackPower = trap._attackPower;

            NetworkServer.SendToReady(_event);
        }
    }

    public struct TrapDespawnedEvent : NetworkMessage
    {
        public static TrapDespawnedEvent _event;

        public Block.UniqueId trapId;
        public byte cellOwnerIndex; //Invalid if it's a bacteria owner

        public static void Send(Trap trap)
        {
            _event.trapId = trap._uniqueId;
            _event.cellOwnerIndex = (byte)trap._ownerCellIndex;

            NetworkServer.SendToReady(_event);
        }
    }

    public struct RequestCellStepOnTrapEvent : NetworkMessage
    {
        public static RequestCellStepOnTrapEvent _event;

        public Block.UniqueId trapId;
        public byte cellOwnerIndex; //Invalid if it's a bacteria owner

        public static void Send(Trap trap)
        {
            _event.trapId = trap._uniqueId;
            _event.cellOwnerIndex = (byte)trap._ownerCellIndex;

            NetworkClient.Send(_event);
        }
    }

    public struct NotifyTrapTriggerEvent : NetworkMessage
    {
        public static NotifyTrapTriggerEvent _event;

        public Block.UniqueId trapId;
        public byte cellOwnerIndex; //Invalid if it's a bacteria owner
        public byte triggerCellIndex; //Invalid if it's a bacteria trigger
        public NetworkArrayObject.UniqueId triggerBacteriaId;

        public static void Send(Trap trap, Cell cell)
        {
            _event.trapId = trap._uniqueId;
            _event.cellOwnerIndex = (byte)trap._ownerCellIndex;
            _event.triggerCellIndex = (byte)cell._uniqueId._index;
            _event.triggerBacteriaId = NetworkArrayObject.UniqueId.invalid;

            NetworkServer.SendToReady(_event);
        }

        public static void Send(Trap trap, Bacteria bacteria)
        {
            _event.trapId = trap._uniqueId;
            _event.cellOwnerIndex = (byte)trap._ownerCellIndex;
            _event.triggerCellIndex = Constants.INVALID_CELL_INDEX;
            _event.triggerBacteriaId = bacteria._uniqueId;

            NetworkServer.SendToReady(_event);
        }
    }
}
