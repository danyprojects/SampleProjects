using System;
using System.Collections.Generic;


namespace Bacterio.Network
{
    //This array can have holes. 
    public sealed class NetworkPlayerController
    {
        public event Action<NetworkPlayerObject> PlayerSpawned;
        public event Action<NetworkPlayerObject> PlayerDespawned;
        public event Action<NetworkPlayerObject> PlayerGotAuthority;

        private readonly NetworkPlayerObject[] _players = null;
        public bool IgnoreSpanws = false;
        public int LastIndex { get; private set; } = -1;
        public int Count { get; private set; } = 0;

        public NetworkPlayerController()
        {
            _players = new NetworkPlayerObject[Constants.MAX_PLAYERS];

            NetworkPlayerObject.PlayerObjectSpawned += OnNetworkPlayerObjectSpawned;
            NetworkPlayerObject.PlayerObjectDespawned += OnNetworkPlayerObjectDespawned;
            NetworkPlayerObject.PlayerObjectGotAuthority += OnNetworkPlayerObjectGotAuthority;
        }

        public NetworkPlayerObject GetFromClientToken(int token)
        {
            for (int i = 0; i <= LastIndex; i++)
            {
                if (_players[i] != null && _players[i]._uniqueId._clientToken == token)
                    return _players[i];
            }

            return null;
        }

        public NetworkPlayerObject GetFromConnection(Mirror.NetworkConnection networkConnection)
        {
            WDebug.Assert(NetworkInfo.IsHost, "Tried to get networkPlayerObject by connection from a client");
            for (int i = 0; i <= LastIndex; i++)
            {
                if (_players[i] != null && _players[i].connectionToClient.connectionId == networkConnection.connectionId)
                    return _players[i];
            }

            return null;
        }

        public NetworkPlayerObject[] GetAll()
        {
            return _players;
        }

        //Methods to deal with the playable object
        private void OnNetworkPlayerObjectSpawned(NetworkPlayerObject playerObject)
        {
            WDebug.Assert(playerObject._uniqueId._clientToken != Constants.INVALID_CLIENT_TOKEN, "Player object spawned with invalid token: " + playerObject._uniqueId._clientToken);
            WDebug.Assert(playerObject._uniqueId._index < Constants.MAX_PLAYERS, "Player " + playerObject._uniqueId._clientToken + " object spawned with invalid index: " + playerObject._uniqueId._index);

            WDebug.Log("Player spawned. Index: " + playerObject._uniqueId._index + " Token: " + playerObject._uniqueId._clientToken + " Ignoring: " + (_players[playerObject._uniqueId._index] != null));

            //Always ignore if player already exists
            if (IgnoreSpanws || _players[playerObject._uniqueId._index] != null)
                return;

            _players[playerObject._uniqueId._index] = playerObject;

            //Update count and last index
            Count++;
            if (playerObject._uniqueId._index > LastIndex)
                LastIndex = playerObject._uniqueId._index;

            PlayerSpawned?.Invoke(playerObject);

            //If object already spawned with authority
            if (playerObject.hasAuthority)
            {
                PlayerGotAuthority?.Invoke(playerObject);
            }
        }

        private void OnNetworkPlayerObjectDespawned(NetworkPlayerObject playerObject)
        {
            WDebug.Log("Player despawned. Index: " + playerObject._uniqueId._index + " Token: " + playerObject._uniqueId._clientToken);

            WDebug.Assert(playerObject._uniqueId._index < Constants.MAX_PLAYERS, "Player object spawned with invalid index: " + playerObject._uniqueId._index);
            WDebug.Assert(_players[playerObject._uniqueId._index] != null, "Player object despawned but it didn't exist");

            if (IgnoreSpanws)
                return;

            _players[playerObject._uniqueId._index] = null;

            //Update count and last index
            Count--;       
            for (; LastIndex >= 0; LastIndex--)                
                if (_players[LastIndex] != null)
                    break;

            PlayerDespawned?.Invoke(playerObject);
        }

        private void OnNetworkPlayerObjectGotAuthority(NetworkPlayerObject playerObject)
        {
            WDebug.Log("Player authority. Index: " + playerObject._uniqueId._index + " Token: " + playerObject._uniqueId._clientToken);

            //If object doesn't exist yet, create it
            if (_players[playerObject._uniqueId._index] == null)
            {
                OnNetworkPlayerObjectSpawned(playerObject);
                return;
            }

            PlayerGotAuthority?.Invoke(playerObject);
        }

        public void Dispose()
        {
            NetworkPlayerObject.PlayerObjectSpawned -= OnNetworkPlayerObjectSpawned;
            NetworkPlayerObject.PlayerObjectDespawned -= OnNetworkPlayerObjectDespawned;
            NetworkPlayerObject.PlayerObjectGotAuthority -= OnNetworkPlayerObjectGotAuthority;

            //Objects that were despawned but kept in memory for re-use should be deleted.
            for (int i = 0; i < _players.Length; i++)
            {
                if (_players[i] == null)
                    continue;

                UnityEngine.Object.Destroy(_players[i].gameObject);
            }
        }
    }
}
