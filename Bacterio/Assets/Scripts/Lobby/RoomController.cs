using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Bacterio.Network;

namespace Bacterio.Lobby
{
    public sealed class RoomController
    {        
        //This is what the server will attach to the connection to read it in other places
        private class PlayerAuthenticationData
        {
            public int _clientToken;
        }

        private struct RoomEventClient : NetworkMessage
        {
            public enum Event
            {
                StartGame = 0,
                PlayerReady,
            }

            public Event _event;
            public int _int1;
            public int _int2;
            public long _long1;
        }

        private struct RoomEventServer : NetworkMessage
        {
            public enum Event
            {
                PlayerReady = 0
            }

            public Event _event;
            public int _data1;
        }

        //Room event structs to prevent allocs
        private RoomEventClient _roomEventClient;
        private RoomEventServer _roomEventServer;

        //events
        public event Action CountdownStart;
        public event Action GameStarted;
        public event Action<int> CountdownUpdate;
        public event Action<NetworkPlayerObject> PlayerJoinedRoom;
        public event Action<NetworkPlayerObject> PlayerLeftRoom;
        public event Action<NetworkPlayerObject, bool> PlayerReadyStatusChanged;

        public bool AllPlayersReady 
        { 
            get //If there's at least 1 player not ready, return false. Otherwise true
            {
                foreach (var value in _clientsReady.Values)
                    if (!value)
                        return false;
                return true;
            }
        }

        //Fields
        public readonly Room _room = null;
        private readonly Dictionary<int, MirrorClient> _clients = null;
        private readonly Dictionary<int, bool> _clientsReady = null;
        private readonly NetworkPlayerController _networkPlayerCtrl = null;
        private readonly bool[] _freeIndexes = null;
        private GameObject _playerObjForClient = null;
        private int _localClientToken = Constants.INVALID_CLIENT_TOKEN;
        private long _serverGameStartTime = 0;

        //Flags
        private bool _roomIsOpen = true;
        private bool _gameIsRunning = false;
        private bool _acceptingNewPlayers = true;


        public RoomController(GameObject objForNewPlayers, Room room, out NetworkPlayerController networkPlayerController, int maxAllowedPlayers = Constants.MAX_PLAYERS)
        {
            WDebug.Assert(objForNewPlayers != null, "Attempted to start roomController with a null playerObj");
            WDebug.Assert(room != null, "Attempted to start roomController without a room");

            _playerObjForClient = objForNewPlayers;
            _room = room;
            _clients = new Dictionary<int, MirrorClient>(maxAllowedPlayers);
            _clientsReady = new Dictionary<int, bool>(maxAllowedPlayers);
            _networkPlayerCtrl = new NetworkPlayerController();
            networkPlayerController = _networkPlayerCtrl;
            _freeIndexes = new bool[maxAllowedPlayers]; //start with all free
            for (int i = 0; i < maxAllowedPlayers; i++)
                _freeIndexes[i] = true;

            //Setup the authentication
            if (NetworkInfo.IsHost)
                NetworkInfo.Authenticator.RequireAuthentication = OnRequireAuthentication;

            //Register callback to catch game start on clients
            if (NetworkInfo.IsHost)
                NetworkServer.RegisterHandler<RoomEventServer>(OnRoomEventServer);
            NetworkClient.RegisterHandler<RoomEventClient>(OnRoomEventClientAndHost);

            //Register to events of network behaviours
            MirrorClient.ClientSpawned += OnClientSpawned;
            MirrorClient.ClientDespawned += OnClientDespawned;

            //Register events on networkPlayerController
            _networkPlayerCtrl.PlayerSpawned += OnNetworkPlayerObjectSpawned;
            _networkPlayerCtrl.PlayerDespawned += OnNetworkPlayerObjectDespawned;
        }

        public void UpdateCountdown()
        {
            var currentServerTime = (long)(NetworkTime.time * Constants.ONE_SECOND_MS);
            var remainingTime = (int)(_serverGameStartTime - currentServerTime);

            CountdownUpdate?.Invoke(remainingTime);

            if (remainingTime <= 0)
                GameStarted?.Invoke();
        }

        public void StartCountdown()
        {
            WDebug.Assert(NetworkInfo.IsHost, "Only host can start the game");
            WDebug.Assert(!_gameIsRunning, "Attempted to start game, but game is already running");

            //Don't allow start game before all clients report ready
            if (!AllPlayersReady)
                return;

            _acceptingNewPlayers = false;

            //From now on game will start. So we will not re-create any new MirrorClients.
            //So we set the hook on serverAddPlayer in the case a player reconnects and needs to be re-attached to the old MirrorClient
            if (NetworkInfo.IsHost)
                MirrorWrapper.OnPlayerReconnectServer = HandlePlayerReconnectingServer;

            //store the time, will need it for reconnects
            _serverGameStartTime = (long)(NetworkTime.time * Constants.ONE_SECOND_MS) + Constants.GAME_START_COUTDOWN_TIME;

            //Call start game for all clients, including the host
            var startEvent = new RoomEventClient();
            startEvent._long1 = _serverGameStartTime;
            NetworkServer.SendToReady(startEvent);
        }

        public void SetLocalPlayerReady(bool isReady)
        {
            //Notify other clients
            if (NetworkInfo.IsHost)
            {                
                NotifyClientsPlayerReadyChange(_localClientToken, isReady);
            }
            else //Request host to change our status
            {
                _roomEventServer._event = RoomEventServer.Event.PlayerReady;
                _roomEventServer._data1 = isReady ? 1 : 0;
                NetworkClient.Send(_roomEventServer);
            }
        }

        public MirrorClient GetClientById(uint netId)
        {
            foreach(var client in _clients.Values)
                if (client.netId == netId)
                    return client;
            return null;
        }

        //******************************** Room event handlers **********************************
        private void OnRoomEventClientAndHost(RoomEventClient eventClient)
        {
            switch(eventClient._event)
            {
                case RoomEventClient.Event.StartGame: CountdownStartClientAndHost(eventClient._long1); break;
                case RoomEventClient.Event.PlayerReady:
                {
                    var token = (int)eventClient._int1;
                    var isReady = eventClient._int2 != 0;

                    _clientsReady[token] = isReady;

                    PlayerReadyStatusChanged?.Invoke(_networkPlayerCtrl.GetFromClientToken(token), isReady);
                }
                break;
            }
        }

        private void OnRoomEventServer(NetworkConnection connection, RoomEventServer eventServer)
        {
            switch(eventServer._event)
            {
                case RoomEventServer.Event.PlayerReady:
                {
                    WDebug.Assert(connection.identity.GetComponent<MirrorClient>() != null && _clients.ContainsKey(connection.identity.GetComponent<MirrorClient>()._clientToken), "Got player ready for a non existent client");

                    var token = connection.identity.GetComponent<MirrorClient>()._clientToken;
                    WDebug.Assert(_clientsReady.ContainsKey(token), "Got player ready for and it had no default");

                    //TODO: validate if player can change status

                    //Notify clients
                    NotifyClientsPlayerReadyChange(token, eventServer._data1 != 0);
                }
                break;
            }
        }

        private void CountdownStartClientAndHost(long serverStartTime)
        {
            //This can be called again while game is running whenever a player reconnects
            if (_gameIsRunning)
                return;

            _gameIsRunning = true;
            _networkPlayerCtrl.IgnoreSpanws = true;

            _serverGameStartTime = serverStartTime;
            CountdownStart?.Invoke();
            UpdateCountdown(); //this will generate Countdown updated event as well.
        }

        //******************************** Mirror event handlers **********************************

        private void HandlePlayerReconnectingServer(NetworkConnection connection, MirrorClient newClient)
        {
            WDebug.Assert(NetworkInfo.IsHost, "Called handlePlayerReconnect from a client");

            var data = (PlayerAuthenticationData)connection.authenticationData;
            WDebug.Assert(data._clientToken != Constants.INVALID_CLIENT_TOKEN, "Player reconnect was called with an invalid token index");

            //Check if there's a player object for the connection token.
            var playerObj = _networkPlayerCtrl.GetFromClientToken(data._clientToken);
            if (playerObj == null)
                return;

            //And if so, overwrite the token that will be used when the MirrorClient after spawns in the network
            newClient._clientToken = data._clientToken;
        }
       
        private void OnClientSpawned(MirrorClient client)
        {
            WDebug.Log("Client spawned. Token: " + client._clientToken);

            WDebug.Assert(_clients.Count < Constants.MAX_PLAYERS, "A player spawned but room was already full!");
            WDebug.Assert(!_clients.ContainsKey(client._clientToken), "Client spawned but it was already in the room");

            //Store our own token for next connection if we disconnect. We store it from the mirrorClient because when it spawns it is guaranteed that each client already has authority due to it being the Mirror player object
            if (client.hasAuthority)
            {
                _localClientToken = client._clientToken;
                NetworkInfo.Authenticator.RequestMessage.clientToken = client._clientToken;
            }

            //Add client to the room
            _clients.Add(client._clientToken, client);        

            //If this is a reconnect, then existing clients already have the corresponding networkPlayerObject. The actual reconnecting client will never enter here
            var playerObj = _networkPlayerCtrl.GetFromClientToken(client._clientToken);
            if (playerObj != null)
            {
                //Match the player object to the new client
                client._networkPlayerObj = playerObj;

                //If we're the host, also respawn the old player object in the network
                if (NetworkInfo.IsHost)
                {
                    //This is a hack to force the host to clear all buffers of this network transform so that it can move as soon as client connects
                    playerObj.GetComponent<NetworkTransform>().enabled = false;
                    playerObj.GetComponent<NetworkTransform>().enabled = true;

                    playerObj.netIdentity.RemoveClientAuthority();
                    playerObj.netIdentity.AssignClientAuthority(client.connectionToClient);
                    //NetworkServer.Spawn(playerObj.gameObject, client.gameObject); // We don't unspawn it 
                }
            }
            //Otherwise it is a new connection. If we're on host, we need to spawn a playable object with the client as the owner
            else if (NetworkInfo.IsHost)
            {
                SpawnPlayerObjectForClient(client);
            }
        }

        private void OnClientDespawned(MirrorClient client)
        {
            WDebug.Log("Client despawned. Token: " + client._clientToken);

            WDebug.Assert(client._networkPlayerObj != null && _clients.ContainsKey(client._networkPlayerObj._uniqueId._clientToken), "Client despawned but it was not in the room");            
            _clients.Remove(client._clientToken);

            //No need to manually despawn the client. Mirror already destroys all objects owned by the disconnecting player

            //Host has extra logic due to the networkPlayerObject
            if (NetworkInfo.IsHost)
            {
                //If we have a networkplayer object, need to do the logic to unspawn that
                if (client._networkPlayerObj == null)
                    return;

                //If game is running we want to unspawn and keep the object. Otherwise it'll be destroyed automatically by mirror since we don't remove authority from object
                //if (_gameIsRunning)
                //    NetworkServer.UnSpawn(client._networkPlayerObj.gameObject);
            }
        }

        //******************************** Network player object handlers **********************************
        private void OnNetworkPlayerObjectSpawned(NetworkPlayerObject obj)
        {
            if (!_clients.ContainsKey(obj._uniqueId._clientToken))
                return;

            WDebug.Assert(_clients.ContainsKey(obj._uniqueId._clientToken), "No client for the spawned network object!");

            //Match the object to the corresponding mirror client
            _clients[obj._uniqueId._clientToken]._networkPlayerObj = obj;

            //Always set ready status to start false and notify if host always count as ready as soon as it has the player object
            _clientsReady.Add(obj._uniqueId._clientToken, false);
            if (NetworkInfo.IsHost)
                SetLocalPlayerReady(true);

            PlayerJoinedRoom?.Invoke(obj);
        }

        private void OnNetworkPlayerObjectDespawned(NetworkPlayerObject obj)
        {
            //Don't assert that client exists. During room, if a player leaves, first the client will disconnect and then the networkObject. So the assert would fail

            //Remove the object from the corresponding mirror client if it still exists
            if (_clients.ContainsKey(obj._uniqueId._clientToken))
                _clients[obj._uniqueId._clientToken]._networkPlayerObj = null;

            _clientsReady.Remove(obj._uniqueId._clientToken);

            PlayerLeftRoom?.Invoke(obj);
        }

        //******************************** Authentication handlers **********************************
        private void OnRequireAuthentication(NetworkConnection connection, BacterioAuthenticator.AuthRequestMessage request, out BacterioAuthenticator.AuthResponseMessage response)
        {
            response = new BacterioAuthenticator.AuthResponseMessage();
            response.isAccepted = false;

            //Always accept host
            if (connection.connectionId == NetworkServer.localConnection.connectionId)
            {
                response.isAccepted = true;
                return;
            }

            if(request.password != _room.Password)
            {
                WDebug.LogVerb("Rejected due to Wrong password");
                return;
            }

            if (!_roomIsOpen)
            {
                WDebug.LogVerb("Rejected due to room not open");
                return;
            }

            //Check if the connection is from an old player
            bool connectionIsFromOldPlayer = _networkPlayerCtrl.GetFromClientToken(request.clientToken) != null ? true : false;

            //Accept connections from old player
            if (!_acceptingNewPlayers && !connectionIsFromOldPlayer)
            {
                WDebug.LogVerb("Rejected due to: Game is running, not accepting new players");
                return;
            }

            //Attach reconnect data to connection in the server
            PlayerAuthenticationData data = new PlayerAuthenticationData();
            data._clientToken = request.clientToken;
            connection.authenticationData = data;

            //If everything else passing, accept
            response.gameIsRunning = _gameIsRunning;
            response.isAccepted = true;
            response.serverGameStartTime = _serverGameStartTime;
        }

        //******************************** Utility **********************************
        private int GetFreeIndex()
        {
            for (int i = 0; i < _freeIndexes.Length; i++)
                if (_freeIndexes[i])
                {
                    _freeIndexes[i] = false;
                    return i;
                }

            return -1;
        }

        private void SpawnPlayerObjectForClient(MirrorClient client)
        {
            var obj = GameObject.Instantiate(_playerObjForClient);
            WDebug.Assert(obj.GetComponent<NetworkPlayerObject>() != null, "Configured room player object does not have clientOwnerIdentity");

            //Fill the player network ID
            var networkPlayer = obj.GetComponent<NetworkPlayerObject>();
            networkPlayer._uniqueId._clientToken = client._clientToken;
            networkPlayer._uniqueId._index = GetFreeIndex();

            //Spawn position, temporary
            networkPlayer.transform.position = new Vector3(client._clientToken % 4, 0, 0);

            NetworkServer.Spawn(obj, client.gameObject);
        }

        private void NotifyClientsPlayerReadyChange(int clientToken, bool isReady)
        {
            _roomEventClient._event = RoomEventClient.Event.PlayerReady;
            _roomEventClient._int1 = clientToken;
            _roomEventClient._int2 = isReady ? 1 : 0;

            NetworkServer.SendToReady(_roomEventClient);
        }

        public void Dispose()
        {
            MirrorWrapper.OnPlayerReconnectServer = null;

            //Deregister events to prevent leaks
            if (NetworkInfo.IsHost)
                NetworkServer.UnregisterHandler<RoomEventServer>();
            NetworkClient.UnregisterHandler<RoomEventClient>();

            MirrorClient.ClientSpawned -= OnClientSpawned;
            MirrorClient.ClientDespawned -= OnClientDespawned;

            _networkPlayerCtrl.PlayerSpawned -= OnNetworkPlayerObjectSpawned;
            _networkPlayerCtrl.PlayerDespawned -= OnNetworkPlayerObjectDespawned;

            _networkPlayerCtrl.Dispose();
        }
    }
}
