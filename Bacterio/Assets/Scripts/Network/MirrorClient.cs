using System;
using UnityEngine;
using Mirror;

namespace Bacterio.Network
{
    public sealed class MirrorClient : NetworkBehaviour
    {
        public static event Action<MirrorClient> ClientSpawned;
        public static event Action<MirrorClient> ClientDespawned;
        public event Action<MirrorClient> PlayerInfoUpdated;

        [SyncVar]
        public int _clientToken = 0; //This is required to support reconnection accross all clients

        public Common.PlayerInfo _playerInfo = null;
        public NetworkPlayerObject _networkPlayerObj = null;

        public override void OnStartClient()
        {
            base.OnStartClient();

            ClientSpawned?.Invoke(this);
        }

        public override void OnStopClient()
        {
            ClientDespawned?.Invoke(this);

            base.OnStopClient();
        }

        public void InitializePublicClientInfo(Common.PlayerInfo playerInfo)
        {
            WDebug.Assert(_playerInfo == null, "Initializing player info twice");

            //Send info to server, who will then send it to all clients
            UpdatePlayerInfoServerRpc(_playerInfo);
        }

        [Command]
        private void UpdatePlayerInfoServerRpc(Common.PlayerInfo playerInfo)
        {
            //Update info on all clients
            UpdatePlayerInfoClientRpc(playerInfo);
        }

        [ClientRpc]
        private void UpdatePlayerInfoClientRpc(Common.PlayerInfo playerInfo)
        {
            _playerInfo = playerInfo;

            PlayerInfoUpdated?.Invoke(this);
        }
    }
}
