using System;
using UnityEngine;
using Bacterio.Common;
using Bacterio.Network;

namespace Bacterio.Game
{
    public sealed class GameController : MonoBehaviour
    {
        private GameContext _gameContext = null;

        private MapController _mapController = null;
        private Action _modeQuitCb = null;

        private bool _isPaused = false;

        public bool IsPaused
        {
            get
            {
                return _isPaused;
            }
            set
            {
                _isPaused = value;
                GameContext.uiController.SetDisconnectedScreen(value, QuitMode);
                enabled = !value;
            }
        }

        public void CleanupRemotePlayer(NetworkPlayerObject playerObject)
        {
            _mapController.CleanupRemotePlayer(playerObject);
        }

        public void Initialize(Canvas gameCanvas, UI.BackgroundCanvas backgroundCanvas, Lobby.GameSetupData gameSetupData, NetworkPlayerController networkPlayerController, Action modeQuitCb, long serverGameStartTime = 0)
        {
            _modeQuitCb = modeQuitCb;

            //Until game starts, make it so that we don't have null checks and less ifs by assigning camera this transform
            WDebug.Assert(transform.position == Vector3.zero, "Mode controller isn't centered");
            GlobalContext.cameraController.AssignTarget(transform);

            //Start the game context
            _gameContext = new GameContext(new UI.GameUIController(gameCanvas, backgroundCanvas));

            //Until game is ready keep input paused.
            GameContext.inputHandler.IsPaused = true;

            //Update elapsed if it's a reconnect.
            if (serverGameStartTime != 0)
                GameContext.gameStatus.ElapsedTimeMs = GameContext.serverTimeMs - serverGameStartTime;

            //Register to game win condition events, only host needs it
            if (NetworkInfo.IsHost)            
                GameStatus.GameEndResultChanged += OnGameEndResultChanged;            

            //Register to network events
            Mirror.NetworkClient.RegisterHandler<NetworkEvents.GameEndEvent>(OnGameEndEventClient);

            //Start UI. Should be done before starting the controllers so the UI can register to the static events
            GameContext.uiController.StartCellPlayerUI(QuitMode);

            //TODO: set actual game background. For now it just enables it
            GlobalContext.cameraController.SetBackground();

            //TODO: only start timed game if it's a timed game
            StartTimedGame((int)(Constants.DEFAULT_GAME_DURATION_MS - GameContext.gameStatus.ElapsedTimeMs));

            //TODO: show loading screen. Loading screen show overlay on top of the game / cell ui

            //Start the controllers
            _mapController = new MapController(gameSetupData, networkPlayerController);

            //Start receiving updates
            enabled = true;
        }

        public void SynchronizeOnReconnect()
        {
            _mapController.SynchronizeOnReconnect();
        }

        public void QuitMode()
        {
            //Cb first. This will disable the network things
            _modeQuitCb();

            //Dispose of the base game
            _mapController.Dispose();
            _mapController = null;

            //Dispose of the other controllers in the game context
            _gameContext.Dispose();
            _gameContext = null;

            GameContext.inputHandler.IsPaused = true;

            //Unregister game win condition events
            if (NetworkInfo.IsHost)            
                GameStatus.GameEndResultChanged -= OnGameEndResultChanged;            

            //Unregister network events
            Mirror.NetworkClient.UnregisterHandler<NetworkEvents.GameEndEvent>();

            //stop accepting updates
            enabled = false;
        }

        private void Awake()
        {
            enabled = false;
        }

        private void Update()
        {
            _gameContext.Update();

            if (NetworkInfo.IsHost)
                _mapController.UpdateHost();
            else
                _mapController.UpdateClient();

            GlobalContext.cameraController.RunOnce();

            GameContext.gameStatus.ElapsedTimeMs += GameContext.serverDeltaTimeMs;
        }

        private void OnDestroy()
        {
            //Unregister game win condition events
            if (NetworkInfo.IsHost)            
                GameStatus.GameEndResultChanged -= OnGameEndResultChanged;            

            //Unregister network events
            Mirror.NetworkClient.UnregisterHandler<NetworkEvents.GameEndEvent>();
        }

        private void StartTimedGame(int duration)
        {
            GameContext.uiController.ShowCountdownPanel(GameContext.serverTimeMs, duration);

            if(NetworkInfo.IsHost)
                GameContext.timerController.Add(duration, () =>
                {
                    GameContext.gameStatus.EndResult = GameEndResult.Lose;
                });
        }

        private void EndGameServer(GameEndResult result)
        {
            WDebug.Assert(NetworkInfo.IsHost, "Called EndGameServer from a client");

            //Stop the game 
            GameContext.inputHandler.IsPaused = true;
            enabled = false;

            //Notify network
            NetworkEvents.GameEndEvent.Send(result);

            //Clear all timers so we don't get extra things still running
            _gameContext.ClearTimers();

            //Show the UI
            GameContext.uiController.ShowGameEndResultsScreen(QuitMode);
        }

        //Network event to get the game result
        private void OnGameEndEventClient(NetworkEvents.GameEndEvent _endEvent)
        {
            if (NetworkInfo.IsHost)
                return;

            GameContext.gameStatus.EndResult = _endEvent.endResult;

            //Stop the game 
            GameContext.inputHandler.IsPaused = true;
            enabled = false;

            //Clear all timers so we don't get extra things still running
            _gameContext.ClearTimers();

            //Show the UI
            GameContext.uiController.ShowGameEndResultsScreen(QuitMode);
        }

        //Callbacks to decide on win conditions
        private void OnGameEndResultChanged(GameEndResult gameEndResult)
        {
            if (!NetworkInfo.IsHost)
                return;

            if (gameEndResult != GameEndResult.Invalid)
                EndGameServer(gameEndResult);
        }
    }
}
