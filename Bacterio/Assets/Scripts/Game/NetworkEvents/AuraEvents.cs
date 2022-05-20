using UnityEngine;
using Mirror;
using Bacterio.Network;
using Bacterio.MapObjects;

namespace Bacterio.NetworkEvents.AuraEvents
{
    public struct CellAuraSyncData
    {
        public Block.UniqueId auraId;
        public Databases.AuraDbId dbId;
    }

    public struct BacteriaAuraSyncData
    {
        public NetworkArrayObject.UniqueId _bacteriaId;
        public Block.UniqueId auraId;
        public Databases.AuraDbId dbId;
    }


    public struct CellAuraAttachedEvent : NetworkMessage
    {
        public static CellAuraAttachedEvent _event;

        public Block.UniqueId auraId;
        public int cellIndex;
        public Databases.AuraDbId dbId;

        public static void Send(Cell cell, Aura aura)
        {
            _event.cellIndex = cell._uniqueId._index;
            _event.auraId = aura._uniqueId;
            _event.dbId = aura._dbId;

            if (NetworkInfo.IsHost)
                NetworkServer.SendToReady(_event);
            else
                NetworkClient.Send(_event);
        }
    }
}
