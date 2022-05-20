using System;
using UnityEngine;

using Bacterio.Game;
using Bacterio.Network;

namespace Bacterio
{
    public sealed class LobbyController : MonoBehaviour
    {
        //Objects for game logic
        private GlobalContext _globalContext;
        private UI.ScreenController _screenController;
        private MirrorWrapper _mirrorWrapper;
        private Lobby.RoomController _roomController;
        private Lobby.RoomFinder _roomFinder;
        private NetworkPlayerController _networkPlayerController;

        //Game state vars
        private GameState _gameState = GameState.Invalid;
        private Lobby.GameSetupData _gameSetupData = null;
        private Lobby.Room _selectedRoom = null;

        //Objects for the game controller and UI
        private UI.BackgroundCanvas _backgroundCanvas = null;
        private GameController _gameController = null; //The instance of the running game controller
        private Canvas _gameCanvas = null;

        private void Awake()
        {
            Physics.autoSimulation = false;
            Physics2D.simulationMode = SimulationMode2D.FixedUpdate;

            //Find objects in the scene
            _backgroundCanvas = GameObject.Find("BackgroundCanvas").GetComponent<UI.BackgroundCanvas>();
            _gameController = GameObject.Find("GameController").AddComponent<GameController>(); //Add so we don't have to worry about initialization order
            _gameCanvas = GameObject.Find("GameCanvas").GetComponent<Canvas>();
            _mirrorWrapper = GameObject.Find("NetworkManager").GetComponent<MirrorWrapper>();

            WDebug.Assert(_gameController != null, "No mode controller game object");
            WDebug.Assert(_gameCanvas != null, "No mode canvas game object");
            WDebug.Assert(_backgroundCanvas != null, "No background canvas game object");
            WDebug.Assert(_mirrorWrapper != null, "No network manager game object");

            //Create objects that need to be manually started
            _globalContext = new GlobalContext();
            _roomFinder = new Lobby.RoomFinder(OnCreateRoom, OnJoinRoom);
            _screenController = new UI.ScreenController(_roomFinder);

            //Set callbacks
            NetworkInfo.ClientStartedEvent += OnClientStarted;
            NetworkInfo.ClientReconnectedEvent += OnClientReconnected;
            NetworkInfo.ClientStoppedEvent += OnClientStopped;
            NetworkInfo.HostStartedEvent += OnHostStarted;
            NetworkInfo.HostStoppedEvent += OnHostStopped;
            NetworkInfo.LocalClientDisconnectedEvent += OnLocalClientDisconnected;
            NetworkInfo.RemoteClientDisconnectedEvent += OnRemoteClientDisconnected;

            //Start showing UI 
            _backgroundCanvas.ReloadBackgrounds();
            _screenController.LoadTitleScreen(OnMainMenuStarted);
        }

        public void Update()
        {
            _globalContext.Update();

            if (_gameState == GameState.CountdownForStart)
            {
                WDebug.Assert(_roomController != null, "No room controller but we're in countdown");
                _roomController.UpdateCountdown();
            }
        }

        private void OnDestroy()
        {
            NetworkInfo.ClientStartedEvent -= OnClientStarted;
            NetworkInfo.ClientReconnectedEvent -= OnClientReconnected;
            NetworkInfo.ClientStoppedEvent -= OnClientStopped;
            NetworkInfo.HostStartedEvent -= OnHostStarted;
            NetworkInfo.HostStoppedEvent -= OnHostStopped;
            NetworkInfo.LocalClientDisconnectedEvent -= OnLocalClientDisconnected;
            NetworkInfo.RemoteClientDisconnectedEvent -= OnRemoteClientDisconnected;

            _networkPlayerController = null;
            if (_roomController != null)
            {
                _roomController.CountdownStart -= OnRoomCountdownStart;
                _roomController.Dispose();  //Also disposes of networkPlayerController
            }
            _roomController = null;

            _globalContext.Dispose();
            _globalContext = null;
        }

        //******************************** Callbacks for the UI  ****************************************
        private void OnMainMenuStarted()
        {
            _screenController.StartMainMenuScreen(StartSinglePlayer);
            _gameState = GameState.MainMenu;
        }

        //**************************************** Mirror wrapper callbacks ****************************************
        //*************** Called on host only
        private void OnHostStarted()
        {
            StartRoom();
        }

        private void OnHostStopped()
        {
            _screenController.StartMainMenuScreen(StartSinglePlayer);
            _gameState = GameState.MainMenu;
        }

        //*************** Called on clients including host
        private void OnClientStarted()
        {
            if(NetworkInfo.IsHost)
                return;

            if(_gameState == GameState.InGame) //resume game
            {
                WDebug.Assert(_gameController.IsPaused == true, "ClientStarted in state InGame but game was not paused!");

                _gameController.IsPaused = false;
                //Reanable network player object
                Mirror.NetworkClient.localPlayer.GetComponent<MirrorClient>()._networkPlayerObj.gameObject.SetActive(true);
            }
            else //Must be joining / creating a room
                StartRoom();
        }

        private void OnClientReconnected(long serverGameStartTime)
        {
            WDebug.Assert(_selectedRoom != null, "Reconnected without a room");

            _roomController = new Lobby.RoomController(GlobalContext.assetBundleProvider.LoadObjectAsset("Cell"), _selectedRoom, out _networkPlayerController);
            _gameSetupData = new Lobby.GameSetupData(_roomController._room.RoomGameMode);
            OnRoomGameReconnected(serverGameStartTime);

            //setup the callback so we can send an event right after sending ready
            NetworkInfo.ClientReadyEvent += OnClientReady;
        }

        private void OnLocalClientDisconnected() //When this is called, game should get paused until client decides to quit. If game isn't running then we stop client
        {
            WDebug.Log("Local client disconnected. Current state: " + _gameState);
            switch(_gameState)
            {
                case GameState.JoiningRoom: //Should always be a timeout
                {
                    UI.ScreenController.ShowMessagePopup("Connection timed out", "Ok", () =>
                    {
                        if (NetworkInfo.IsHost)
                            _mirrorWrapper.StopHost();
                        else
                            _mirrorWrapper.StopClient();
                        UI.ScreenController.HideMessagePopup();
                    });
                }break;
                case GameState.AwaitingGameStart: //Should be connection lost to host / host closed room / client left room
                {
                    StopNetwork();
                    _gameState = GameState.MainMenu;
                    _screenController.StartMainMenuScreen(StartSinglePlayer);
                    UI.ScreenController.ShowMessagePopup("Connection lost", "Close", UI.ScreenController.HideMessagePopup);
                }
                break;
                case GameState.InGame: //We lost connection to host or host exited, in either case pause the game and let player decide what to do
                {
                    WDebug.Assert(_gameController != null, "Received local client disconnect during inGame state without a mode controller");
                    //Disable network player object so that it doesn't try to keep sending position updates
                    Mirror.NetworkClient.localPlayer.GetComponent<MirrorClient>()._networkPlayerObj.gameObject.SetActive(false);
                    _gameController.IsPaused = true;
                }break;
                case GameState.GameModeQuit: //The disconnect we will get when we quit out of a game
                {
                    _gameState = GameState.MainMenu;
                    _screenController.StartMainMenuScreen(StartSinglePlayer);
                }
                break;
                default: WDebug.Log("Local client disconnect during unexpected game state: " + _gameState); break;
            }
        }

        private void OnRemoteClientDisconnected(uint connectionId)
        {
            WDebug.Log("Remote client ID: " + connectionId + " disconnected");

            if (_roomController == null)
                return;

            var client = _roomController.GetClientById(connectionId);
            if (client == null)
            {
                WDebug.Log("No client in local room for ID: " + connectionId);
                return;
            }

            //TODO: start a timer before actually cleaning up stuff
            //if (_modeController != null)
            //    _modeController.CleanupRemotePlayer(client._networkPlayerObj);
        }

        private void OnClientStopped()
        {

        }

        private void OnClientReady()
        {
            NetworkInfo.ClientReadyEvent -= OnClientReady;
            _gameController.SynchronizeOnReconnect();
        }

        //**************************************** Internal Utility  ****************************************
        private void StartRoom()
        {
            WDebug.Assert(_selectedRoom != null, "Started roomController without a room");

            _roomController = new Lobby.RoomController(GlobalContext.assetBundleProvider.LoadObjectAsset("Cell"), _selectedRoom, out _networkPlayerController);
            _roomController.CountdownStart += OnRoomCountdownStart;
            _roomController.GameStarted += OnRoomGameStarted;
            _gameSetupData = new Lobby.GameSetupData(_roomController._room.RoomGameMode);

            switch (_gameSetupData.SelectedGameMode)
            {
                case GameMode.Survival:
                {
                    _screenController.StartModeSetupScreen(_roomController, _gameSetupData, StopNetwork);
                    _gameState = GameState.AwaitingGameStart;
                }
                break;
            }
        }

        private void StartSinglePlayer()
        {
            _selectedRoom = new Lobby.Room(false, "MyRoom");
            if (!NetworkInfo.StartHost())
                UI.ScreenController.ShowMessagePopup("Error: " + (byte)ErrorCode.FAILED_TO_START_HOST, "Close", UI.ScreenController.HideMessagePopup);
        }

        //**************************************** Room callbacks  ****************************************
        private void OnRoomCountdownStart()
        {
            _gameState = GameState.CountdownForStart;
        }

        private void OnRoomGameReconnected(long serverGameStartTime)
        {
            WDebug.Assert(_gameSetupData.SelectedGameMode != GameMode.None, "OnRoomGameReconnected got called but there was no selected game mode");
            WDebug.Assert(_gameState == GameState.JoiningRoom, "OnRoomGameReconnected got called but we were not joining a room");
            WDebug.Assert(_gameController != null, "Attempted to restart game without a controller");

            //TODO: Change the discovery request params to notify game ongoing

            _gameState = GameState.InGame;

            //TODO: start other modes too
            WDebug.Log("Starting survival mode game");

            //Clear UI
            _backgroundCanvas.SetActive(false);
            _screenController.ClearScreen();

            //Start mode controller
            _gameController.Initialize(_gameCanvas, _backgroundCanvas, _gameSetupData, _networkPlayerController, OnGameModeQuit, serverGameStartTime);
        }

        private void OnRoomGameStarted()
        {
            WDebug.Assert(_gameSetupData.SelectedGameMode != GameMode.None, "OnRoomGameStarted got called but there was no selected game mode");
            WDebug.Assert(_gameState == GameState.CountdownForStart, "OnRoomGameStarted got called but we were not awaiting it");
            WDebug.Assert(_gameController != null, "Attempted to start game without a controller");

            //TODO: Change the discovery request params to notify game ongoing


            _gameState = GameState.InGame;

            //TODO: start other modes too
            WDebug.Log("Starting survival mode game");

            //Clear UI
            _backgroundCanvas.SetActive(false);
            _screenController.ClearScreen();

            //Start mode controller
            _gameController.Initialize(_gameCanvas, _backgroundCanvas, _gameSetupData, _networkPlayerController, OnGameModeQuit);
        }

        private void StopNetwork()
        {
            //Deactivate room finder (if it was active)
            _roomFinder.SetDiscoveryStatus(false);

            //Deactivate the room
            _networkPlayerController = null;
            _roomController?.Dispose(); //Also disposes of networkPlayerController
            _roomController = null;
            _selectedRoom = null;

            if (NetworkInfo.IsHost)
                _mirrorWrapper.StopHost();
            else
                _mirrorWrapper.StopClient();
        }

        private void OnGameModeQuit()
        {
            WDebug.Assert(_gameController != null, "Game quit but there was no controller");
            _gameState = GameState.GameModeQuit;
            GlobalContext.cameraController.ResetPosition();

            //If has already been disconnected, then StopNetwork won't generate another Disconnected event, so we handle it here
            if (!NetworkInfo.IsLocalConnected)
            {
                _gameState = GameState.MainMenu;
                _screenController.StartMainMenuScreen(StartSinglePlayer);
            }

            //Stop network
            StopNetwork();
        }

        private bool OnCreateRoom(ushort port, Lobby.Room room)
        {
            _selectedRoom = room;

            NetworkInfo.Transport.Port = port;
            if (!NetworkInfo.StartHost())
            {
                UI.ScreenController.ShowMessagePopup("Error: " + (byte)ErrorCode.FAILED_TO_START_HOST, "Close", UI.ScreenController.HideMessagePopup);
                _selectedRoom = null;
                return false;
            }
            _gameState = GameState.AwaitingGameStart;
            return true;
        }

        private bool OnJoinRoom(Lobby.Room room)
        {
            _selectedRoom = room;

            UI.ScreenController.ShowMessagePopup("Connecting....");
            _gameState = GameState.JoiningRoom;

            if (!NetworkInfo.StartClient(room.Endpoint.Address.ToString()))
            {
                UI.ScreenController.ShowMessagePopup("Error: " + (byte)ErrorCode.FAILED_TO_START_CLIENT, "Close", UI.ScreenController.HideMessagePopup);
                _selectedRoom = null;
                return false;
            }
            return true;
        }
    }
}
