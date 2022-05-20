using UnityEngine;
using Mirror;
using Bacterio.Network;
using Bacterio.MapObjects;

namespace Bacterio.NetworkEvents.StructureEvents
{
    public struct RequestStructureFullSyncEvent : NetworkMessage
    { }

    public struct ReplyStructureFullSyncEvent : NetworkMessage
    {
        public Block.UniqueId[] structureIds;
        public Vector2[] positions;
        public Databases.StructureDbId[] dbIds;
        public int[] seeds;
        public short[] deforms;

        public static void Send(NetworkConnection connection, ref ReplyStructureFullSyncEvent _event)
        {
            connection.Send(_event);
        }
    }

    public struct StructureSpawnedEvent : NetworkMessage
    {
        public static StructureSpawnedEvent _event;

        public Block.UniqueId structureId;
        public Databases.StructureDbId dbId;
        public Vector2 position;

        public static void Send(Structure structure)
        {
            _event.structureId = structure._uniqueId;
            _event.dbId = structure._dbId;
            _event.position = structure.transform.position;

            WDebug.Assert(NetworkInfo.IsHost, "Attempted to send structure spawned event from a client");
            NetworkServer.SendToReady(_event);
        }
    }

    public struct StructureWithTerritorySpawnedEvent : NetworkMessage
    {
        public static StructureWithTerritorySpawnedEvent _event;

        public Block.UniqueId structureId;
        public Databases.StructureDbId dbId;
        public Vector2 position;
        public int seed;

        public static void Send(Structure structure, Territory territory)
        {
            _event.structureId = structure._uniqueId;
            _event.dbId = structure._dbId;
            _event.position = structure.transform.position;
            _event.seed = territory._random.Seed;

            WDebug.Assert(NetworkInfo.IsHost, "Attempted to send structure spawned event from a client");
            NetworkServer.SendToReady(_event);
        }
    }

    public struct StructureDespawnedEvent : NetworkMessage
    {
        public static StructureDespawnedEvent _event;

        public Block.UniqueId structureId;

        public static void Send(Structure structure)
        {
            _event.structureId = structure._uniqueId;

            WDebug.Assert(NetworkInfo.IsHost, "Attempted to send structure despawned event from a client");
            NetworkServer.SendToReady(_event);
        }
    }

    public struct TerritoryDeformed : NetworkMessage
    {
        public static TerritoryDeformed _event;

        public Block.UniqueId structureId;
        public bool isOutwards;

        public static void Send(Structure structure, bool isOutwards)
        {
            _event.structureId = structure._uniqueId;
            _event.isOutwards = isOutwards;

            WDebug.Assert(NetworkInfo.IsHost, "Attempted to send territory deformed event from a client");
            NetworkServer.SendToReady(_event);
        }
    }

    public struct RequestWoundHealEvent : NetworkMessage
    {
        public static RequestWoundHealEvent _event;

        public Block.UniqueId woundId;
        public int healPower;


        public static void Send(Structure wound, int healPower)
        {
            _event.woundId = wound._uniqueId;
            _event.healPower = healPower;

            WDebug.Assert(!NetworkInfo.IsHost, "Attempted to send request wound heal from host");
            NetworkClient.Send(_event);
        }
    }
}
