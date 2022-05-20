using System;
using System.Collections.Generic;
using UnityEngine;
using Bacterio.Network;

namespace Bacterio.Lobby
{

    public sealed class RoomFinder
    {
        public event Action<Room> RoomAdded;
        public event Action<Room> RoomUpdated;

        public Dictionary<int, Room> Rooms { get; private set; }

        private Func<ushort, Room, bool> _createRoomCb;
        private Func<Room, bool> _joinRoomCb;

        public RoomFinder(Func<ushort, Room, bool> createRoomCb, Func<Room, bool> joinRoomCb)
        {
            _createRoomCb = createRoomCb;
            _joinRoomCb = joinRoomCb;
            
            Rooms = new Dictionary<int, Room>();

            NetworkInfo.Discovery.LANRoomFoundEvent += OnLANRoomDiscoveredEvent;
        }

        public void SetDiscoveryStatus(bool isActive)
        {
            if (isActive)
                NetworkInfo.Discovery.StartDiscovery();
            else
                NetworkInfo.Discovery.StopDiscovery();
        }

        public void JoinRoom(Room room)
        {
            WDebug.Log("Joining room name: " + room.RoomName);
            NetworkInfo.Authenticator.RequestMessage.password = "1234";
            _joinRoomCb?.Invoke(room);
        }

        public void CreateLANRoom(string name, ushort port = Constants.DEFAULT_PORT)
        {
            WDebug.Log("Creating room name: " + name + " at port: " + port);

            Room room = new Room(true, name);

            //If we failed to create room don't do anything else
            if (!_createRoomCb(port, room))
                return;

            //Set a response that we will send to those viewing this room
            NetworkInfo.Discovery.Response = new DiscoveryResponse() { PlayerId = 0, RoomName = name };
            NetworkInfo.Discovery.AdvertiseServer();
        }

        public void RefreshRooms()
        {
            Rooms.Clear();
            NetworkInfo.Discovery.BroadcastDiscoveryRequest();
        }

        public void ClearRooms()
        {
            Rooms.Clear();
        }

        private void OnLANRoomDiscoveredEvent(DiscoveryResponse roomResponse, System.Net.IPEndPoint endpoint)
        {
            WDebug.Log("Found LAN room: " + roomResponse.RoomName);

            //Room already exists
            if (Rooms.ContainsKey(roomResponse.PlayerId))
            {
                if (Rooms[roomResponse.PlayerId].UpdateRoom(roomResponse))
                    RoomUpdated(Rooms[roomResponse.PlayerId]);
            }
            else
            {
                var room = new Room(false, roomResponse, endpoint);
                Rooms.Add(roomResponse.PlayerId, room);
                RoomAdded?.Invoke(Rooms[roomResponse.PlayerId]);
            }
        }

        public void Dispose()
        {
            NetworkInfo.Discovery.LANRoomFoundEvent -= OnLANRoomDiscoveredEvent;
        }
    }
}
