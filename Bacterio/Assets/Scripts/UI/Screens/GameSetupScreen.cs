using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Bacterio.Lobby;

namespace Bacterio.UI.Screens
{
    public sealed class GameSetupScreen : MonoBehaviour
    {
        //Ui fields
        [SerializeField] private Button _gameStartButton = null;
        [SerializeField] private TextMeshProUGUI _gameStartButtonText = null;
        [SerializeField] private Button _backButton = null;
        [SerializeField] private Button _testButton = null;

        //internal vars
        private Action _onBackClickCb = null;
        private RoomController _roomController = null;
        private GameSetupData _gameSetupData = null;
        private bool _localReadyStatus = false;


        public void SetActive(bool isActive)
        {
            GetComponent<Canvas>().enabled = isActive;
            this.enabled = isActive;

            //Unregister events
            if(!isActive)
            {
                if(_gameSetupData != null)
                    _gameSetupData.DataUpdated -= OnGameSetupDataUpdated;
                if(_roomController != null)
                {
                    _roomController.PlayerJoinedRoom -= OnPlayerJoinRoom;
                    _roomController.PlayerLeftRoom -= OnPlayerLeftRoom;
                    _roomController.PlayerReadyStatusChanged -= OnPlayerReadyStatusChanged;
                    _roomController.CountdownUpdate -= OnCountdownUpdated;
                }
                _gameSetupData = null;
                _roomController = null;
                _onBackClickCb = null;                
            }
        }

        public void Configure(GameSetupData gameSetupData, RoomController roomController, Action backClickCb)
        {
            _onBackClickCb = backClickCb;
            _roomController = roomController;
            _gameSetupData = gameSetupData;
            _localReadyStatus = false;

            //register to events
            _roomController.PlayerJoinedRoom += OnPlayerJoinRoom;
            _roomController.PlayerLeftRoom += OnPlayerLeftRoom;
            _roomController.PlayerReadyStatusChanged += OnPlayerReadyStatusChanged;
            _roomController.CountdownUpdate += OnCountdownUpdated;
            _gameSetupData.DataUpdated += OnGameSetupDataUpdated;

            if (Network.NetworkInfo.IsHost)
            {
                _gameStartButton.interactable = false;
                _gameStartButtonText.text = "Start";
            }
            else
            {
                _gameStartButtonText.text = "Ready";
            }

            //Clear old listeners and setup new ones
            ClearListeners();
            SetupListeners();
        }


        private void Awake()
        {
            _backButton.onClick.AddListener(() => { _onBackClickCb?.Invoke(); });
        }

        //*********************************************** Utility
        private void ClearListeners()
        {
            _gameStartButton.onClick.RemoveAllListeners();
            _testButton.onClick.RemoveAllListeners();
        }

        public void SetupListeners()
        {
            if (Network.NetworkInfo.IsHost)
            {
                _gameStartButton.onClick.AddListener(() => { _roomController?.StartCountdown(); });

                _testButton.onClick.AddListener(() => { _gameSetupData.ChangeTestValue(5); });
            }
            else
            {
                _gameStartButton.onClick.AddListener(() => { _roomController?.SetLocalPlayerReady(!_localReadyStatus); });
            }
        }

        //*********************************************** Event callbacks
        private void OnPlayerJoinRoom(Network.NetworkPlayerObject playerObj)
        {
            playerObj.transform.position = Vector3.right * playerObj._uniqueId._index;
        }

        private void OnPlayerLeftRoom(Network.NetworkPlayerObject playerObj)
        {
        }

        private void OnPlayerReadyStatusChanged(Network.NetworkPlayerObject playerObj, bool isReady)
        {
            //If local player ready status changed, we need to update the start button and his position 
            if (playerObj.hasAuthority)
            {
                var pos = Vector3.right * playerObj._uniqueId._index;
                pos.y = isReady ? 1 : 0;
                playerObj.transform.position = pos;

                _localReadyStatus = isReady;

                //Don't update text if it's not host
                if (!Network.NetworkInfo.IsHost)                
                    _gameStartButtonText.text = isReady ? "Cancel" : "Ready";                
            }

            if (Network.NetworkInfo.IsHost)            
                _gameStartButton.interactable = _roomController.AllPlayersReady;            
        }

        private void OnGameSetupDataUpdated()
        {
            _testButton.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = _gameSetupData.TestValue.ToString();
        }

        private void OnCountdownUpdated(int remaining)
        {
            var seconds = (remaining / Constants.ONE_SECOND_MS + 1).ToString();

            if(seconds != _gameStartButtonText.text)
                _gameStartButtonText.text = seconds;
        }

        private void OnDestroy()
        {
            //unregister events
            if (_gameSetupData != null)
                _gameSetupData.DataUpdated -= OnGameSetupDataUpdated;
            if (_roomController != null)
            {
                _roomController.PlayerJoinedRoom -= OnPlayerJoinRoom;
                _roomController.PlayerLeftRoom -= OnPlayerLeftRoom;
                _roomController.PlayerReadyStatusChanged -= OnPlayerReadyStatusChanged;
                _roomController.CountdownUpdate -= OnCountdownUpdated;
            }

            _gameSetupData = null;
            _roomController = null;
        }
    }
}
