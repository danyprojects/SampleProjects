using System;
using System.Collections.Generic;
using UnityEngine;
using Bacterio.Network;

namespace Bacterio.Lobby
{
    public sealed class Room
    {
        public string RoomName { get; private set; }
        public string Password { get; private set; }
        public GameMode RoomGameMode { get; private set; }

        public readonly bool IsOnline;
        public readonly System.Net.IPEndPoint Endpoint;

        public Room(bool isOnline, DiscoveryResponse response, System.Net.IPEndPoint endpoint)
        {
            IsOnline = isOnline;
            RoomName = response.RoomName;
            Endpoint = endpoint;
            RoomGameMode = GameMode.Survival;
        }

        public Room(bool isOnline, string name)
        {
            IsOnline = isOnline;
            RoomName = name;
            Password = "1234"; //for now
            RoomGameMode = GameMode.Survival;
        }

        public bool UpdateRoom(DiscoveryResponse response)
        {
            bool changed = false;

            if(RoomName != response.RoomName)
            {
                changed |= true;
                RoomName = response.RoomName;
            }

            return changed;
        }
    }
}
