using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Bacterio.Network
{
    public static class NetworkInfo
    {
        public static event Action HostStartedEvent;
        public static event Action HostStoppedEvent;
        public static event Action ClientStartedEvent;
        public static event Action<long> ClientReconnectedEvent;
        public static event Action ClientStoppedEvent;
        public static event Action ClientReadyEvent;
        public static event Action<uint> RemoteClientDisconnectedEvent;
        public static event Action LocalClientDisconnectedEvent;

        //Info getters
        public static BacterioAuthenticator Authenticator { get; private set; } = null;
        public static BacterioNetworkDiscovery Discovery { get; private set; } = null;
        public static kcp2k.KcpTransport Transport { get; private set; } = null;
        public static bool IsHost { get; private set; } = false;
        public static bool IsLocalConnected { get; private set; } = false;


        public static bool StartHost()
        {
            try
            {
                NetworkManager.singleton.StartHost();
                return true;
            }catch(Exception e)
            {
                WDebug.Log("Failed to start host: " + e.Message);
                return false;
            }
        }

        public static bool StartClient(string address = "localhost")
        {
            try
            {
                NetworkManager.singleton.networkAddress = address;
                NetworkManager.singleton.StartClient();
                return true;
            }
            catch (Exception e)
            {
                WDebug.Log("Failed to start client: " + e.Message);
                return false;
            }
        }

        //This is so that we hide the network manager stuff in a different scope to prevent polution. Most of the time other modules will only need NetworkInfo
        public abstract class MirrorWrapperBase : NetworkManager
        {
            public static Action<NetworkConnection, MirrorClient> OnPlayerReconnectServer = null;
            private bool GameStarted { get { return OnPlayerReconnectServer != null; } } //The callback reconnected is only set when game has started

            private int _nextToken = Constants.INVALID_CLIENT_TOKEN + 1;

            //This is so that whenever a client disconnects, the host can notify the other clients so they can cleanup
            private struct RemoteClientDisconnectionEvent : NetworkMessage
            {
                public uint netId;
            }

            public override void Awake()
            {
                base.Awake();
                Authenticator = GetComponent<BacterioAuthenticator>();
                Discovery = GetComponent<BacterioNetworkDiscovery>();
                Transport = GetComponent<kcp2k.KcpTransport>();

                WDebug.Assert(Authenticator != null, "No authenticator in networkmanager");
                WDebug.Assert(Discovery != null, "No discovery in networkmanager");
                WDebug.Assert(Transport != null, "No transport in networkmanager");

                Transport.Timeout = Constants.TIMEOUT_MS;
            }

            //************************************************ Callbacks
            public override void OnStartHost()
            {
                WDebug.Log("Host Started");

                NetworkClient.RegisterHandler<RemoteClientDisconnectionEvent>(OnNetworkConnectionStateEventClient);
                IsHost = true;
                HostStartedEvent?.Invoke();
            }

            public override void OnStopHost()
            {
                WDebug.Log("Host Stopped");
                NetworkClient.UnregisterHandler<RemoteClientDisconnectionEvent>();
                IsHost = false;
                HostStoppedEvent?.Invoke();
            }

            public override void OnServerError(NetworkConnection conn, Exception exception)
            {
                WDebug.Log("Server error: " + exception.Message);
                base.OnServerError(conn, exception);
            }

            public override void OnClientError(Exception exception)
            {
                WDebug.Log("Client error: " + exception.Message);
                base.OnClientError(exception);
            }

            //Called on both host and other clients
            public override void OnStartClient()
            {
                //Host doesn't need to run this
                if (IsHost)
                    return;

                WDebug.Log("Client Starting: " + NetworkClient.isConnecting);
                IsHost = false;

                NetworkClient.OnConnectedEvent += () => OnClientConnectionChanged(true);
                NetworkClient.OnDisconnectedEvent += () => OnClientConnectionChanged(false);
            }

            //Called on both host and other clients
            public override void OnStopClient()
            {
                if(IsHost)
                {
                    IsHost = false;
                    return;
                }

                WDebug.Log("Stopped client");
                ClientStoppedEvent?.Invoke();
            }

            //******* Client state callbacks
            //Called on other clients when client transitions to "connected". Not called on HOST since host doesn't register to these events
            private void OnClientConnectionChanged(bool isConnected)
            {
                if (isConnected)
                {
                    WDebug.Log("Local Client connected");
                    NetworkClient.RegisterHandler<RemoteClientDisconnectionEvent>(OnNetworkConnectionStateEventClient);
                    IsLocalConnected = true;

                    //If client is not authenticated then it shouldn't count as client started. But connection state will be sent first. 
                    //TODO: This needs to be tested if a temporary connection loss will generate a new connection or not
                    if(NetworkClient.connection.isAuthenticated)
                        ClientStartedEvent?.Invoke();
                }
                else
                {
                    WDebug.Log("Local Client disconnected");
                    NetworkClient.OnConnectedEvent -= () => OnClientConnectionChanged(true);
                    NetworkClient.OnDisconnectedEvent -= () => OnClientConnectionChanged(false);

                    NetworkClient.UnregisterHandler<RemoteClientDisconnectionEvent>();
                    IsLocalConnected = false;

                    LocalClientDisconnectedEvent?.Invoke();
                }
            }

            //************************************************ Connections
            //Called on server when client connections are accepted (including the host client)
            public override void OnServerConnect(NetworkConnection conn)
            {
                WDebug.Log("Server ->  Client id " + conn.connectionId + " connected");
            }
           
            //Called on server when a client disconnects
            public override void OnServerDisconnect(NetworkConnection conn)
            {
                //Null will happen in case connection is rejected
                if (conn.identity == null)
                    return;

                //If game has already started then we have to deal with authority and cleanup
                //For remote players, invoke the callback before the base call. Base will destroy the objects, so we need to cleanup before that. During host quits we don't have a connection, just skip this. 
                if (GameStarted && !conn.identity.isLocalPlayer && NetworkServer.localConnection != null)
                {
                    //Removing authority from player object will prevent it from getting automatically destroyed when we destroy the connection
                    //This way the player object itself will remain in the network, under the authority of the server.
                    var networkPlayerObj = conn.identity.GetComponent<MirrorClient>()._networkPlayerObj;
                    if (networkPlayerObj != null)
                    {
                        networkPlayerObj.netIdentity.RemoveClientAuthority();
                        networkPlayerObj.netIdentity.AssignClientAuthority(NetworkServer.localConnection);
                    }                    

                    RemoteClientDisconnectedEvent?.Invoke(conn.identity.netId);
                    //Notify clients of this disconnection before they get the despawn, so they can cleanup properly
                    NetworkServer.SendToAll(new RemoteClientDisconnectionEvent() { netId = conn.identity.netId});
                }

                base.OnServerDisconnect(conn);
            }

            //Called on server when "AddPlayer" message arrives
            public override void OnServerAddPlayer(NetworkConnection conn)
            {
                WDebug.Log("Server -> Add player for connection ID: " + conn.connectionId);

                //Manually spawn the player object since we have special handling on the player object to allow reconnection
                //If we have the playerReconnect action set, then game already started and we never want to add new players. So forward it to the callback
                var clientObj = GameObject.Instantiate(playerPrefab);
                var client = clientObj.GetComponent<MirrorClient>();
                client._clientToken = _nextToken++;
                OnPlayerReconnectServer?.Invoke(conn, client); //Might overwrite the token
                NetworkServer.AddPlayerForConnection(conn, clientObj);
            }

            //************************************** Methods that run on client
            private void OnNetworkConnectionStateEventClient(RemoteClientDisconnectionEvent connectionEvent)
            {
                //Host doesn't need to deal with this since he was the one that generated it to begin with
                if (IsHost)
                    return;

                if (connectionEvent.netId != NetworkClient.connection.connectionId)                
                    RemoteClientDisconnectedEvent?.Invoke(connectionEvent.netId);
            }

            //Called on host and client when connection is accepted
            public override void OnClientConnect(NetworkConnection conn)
            {
                WDebug.Log("Client id " + conn.connectionId + " -> connected");

                //if is a reconnect, start game controller first. Otherwise do the normal behaviour.
                if (Authenticator.ResponseMessage.gameIsRunning) //Is a reconnect
                    ClientReconnectedEvent?.Invoke(Authenticator.ResponseMessage.serverGameStartTime);
                else
                    ClientStartedEvent?.Invoke();

                //Request server to add player.
                NetworkClient.Ready();
                NetworkClient.AddPlayer();

                ClientReadyEvent?.Invoke();
            }

            //Called on client when it disconnects
            public override void OnClientDisconnect(NetworkConnection conn)
            {
               //WDebug.Log(IsHost ? "Client disconnect on host" : "Client disconnect on client");
            }

            public override void OnDestroy()
            {
                base.OnDestroy();
            }
        }
    }

    //This is so that Unity still finds this object to put into a gameobject as a component
    public sealed class MirrorWrapper : NetworkInfo.MirrorWrapperBase { }
}


       