using UnityEngine;
using Mirror;
using Bacterio.Network;
using Bacterio.MapObjects;

namespace Bacterio.NetworkEvents
{
    public struct EmptyEvent
    {
        public static void Send<T>() where T : struct, NetworkMessage
        {
            T empty = new T();

            if (NetworkInfo.IsHost)
                NetworkServer.SendToReady(empty);
            else
                NetworkClient.Send(empty);
        
        }
    }

    public struct GameEndEvent : NetworkMessage
    {
        public static GameEndEvent _event;
        public GameEndResult endResult;

        public static void Send(GameEndResult result)
        {
            _event.endResult = result;

            WDebug.Assert(NetworkInfo.IsHost, "Client attempted to send game end event");
            NetworkServer.SendToReady(_event);
        }
    }
}
