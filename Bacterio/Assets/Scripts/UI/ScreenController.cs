using System;
using UnityEngine;

using Bacterio.UI.Screens;
using Bacterio.UI.Other;

namespace Bacterio.UI
{
    public sealed class ScreenController
    {
        private Canvas _rootCanvas = null;
        
        //Objects needed by controller and other UIs
        private readonly Lobby.RoomFinder _roomFinder = null;

        //Screens
        private TitleScreen _titleScreen = null;
        private MainMenuScreen _mainMenuScreen = null;
        private RoomFinderScreen _roomFinderScreen = null;
        private GameSetupScreen _modeSetupScreen = null;

        //Screens that will be accessible from anywhere
        private static PopupMessage _popupMessage = null;
        private static PopupInputField _popupInputField = null;

        public ScreenController(Lobby.RoomFinder roomFinder)
        {
            _roomFinder = roomFinder;

            _rootCanvas = GameObject.Find("ScreenCanvas").GetComponent<Canvas>();

            WDebug.Assert(_rootCanvas != null, "No screen canvas game object");

            //Instantiate popups, we'll need them in 99% of the cases
            var popupObj = GlobalContext.assetBundleProvider.LoadUIScreenAsset("PopupMessage");
            _popupMessage = GameObject.Instantiate(popupObj.GetComponent<PopupMessage>(), _rootCanvas.transform);
            _popupMessage.gameObject.SetActive(true); //UI objects should start disabled so that they don't run awake on instantiate
            WDebug.Assert(_popupMessage != null, "Could not create a popup message object");


            popupObj = GlobalContext.assetBundleProvider.LoadUIScreenAsset("PopupInputField");
            _popupInputField = GameObject.Instantiate(popupObj.GetComponent<PopupInputField>(), _rootCanvas.transform);
            _popupInputField.gameObject.SetActive(true); //UI objects should start disabled so that they don't run awake on instantiate
            WDebug.Assert(_popupMessage != null, "Could not create a popup input object");
        }

        public void LoadTitleScreen(Action startMainMenuCb)
        {
            if (_titleScreen == null)
            {
                var titleScreenObj = GlobalContext.assetBundleProvider.LoadUIScreenAsset("TitleScreen");
                _titleScreen = GameObject.Instantiate(titleScreenObj.GetComponent<TitleScreen>(), _rootCanvas.transform);
                _titleScreen.OnStartGameClickCb = startMainMenuCb;
                _titleScreen.gameObject.SetActive(true); //UI objects should start disabled so that they don't run awake on instantiate
            }

            _titleScreen.SetActive(true);
        }

        public void StartMainMenuScreen(Action startSinglePlayerCb)
        {
            ClearScreen();

            if (_mainMenuScreen == null)
            {
                var mainMenuScreenObj = GlobalContext.assetBundleProvider.LoadUIScreenAsset("MainMenuScreen");
                _mainMenuScreen = GameObject.Instantiate(mainMenuScreenObj.GetComponent<MainMenuScreen>(), _rootCanvas.transform);
                _mainMenuScreen.gameObject.SetActive(true); //UI objects should start disabled so that they don't run awake on instantiate
            }

            _mainMenuScreen.OnSinglePlayerClickCb = startSinglePlayerCb;
            _mainMenuScreen.OnMultiPlayerClickCb = ShowRoomFinderScreen;
            _mainMenuScreen.SetActive(true);
        }

        public void StartModeSetupScreen(Lobby.RoomController roomController, Lobby.GameSetupData gameSetupData, Action gameSetupAbortCb)
        {
            ClearScreen();

            if (_modeSetupScreen == null)
            {
                var modeSetupObj = GlobalContext.assetBundleProvider.LoadUIScreenAsset("GameSetupScreen");
                _modeSetupScreen = GameObject.Instantiate(modeSetupObj.GetComponent<GameSetupScreen>(), _rootCanvas.transform);
                _modeSetupScreen.gameObject.SetActive(true); //UI objects should start disabled so that they don't run awake on instantiate
            }

            _modeSetupScreen.Configure(gameSetupData, roomController, gameSetupAbortCb);
            _modeSetupScreen.SetActive(true);
        }

        public void ClearScreen()
        {
            _titleScreen?.SetActive(false);
            _mainMenuScreen?.SetActive(false);
            _modeSetupScreen?.SetActive(false);
            _roomFinderScreen?.SetActive(false);
            _popupMessage?.SetActive(false);
        }

        //************************************************* Static methods for showing generic uis ************************
        public static void ShowMessagePopup(string message, string title = "")
        {
            _popupMessage.Configure(title, message);
            _popupMessage.transform.SetAsLastSibling();
            _popupMessage.SetActive(true);
        }

        public static void ShowMessagePopup(string message, string leftButtonText, Action leftButtonClick, string rightButtonText, Action rightButtonClick, string title = "")
        {
            _popupMessage.Configure(title, message, leftButtonText, leftButtonClick, rightButtonText, rightButtonClick);
            _popupMessage.transform.SetAsLastSibling();
            _popupMessage.SetActive(true);
        }

        public static void ShowMessagePopup(string message, string centerButtonText, Action centerButtonClick, string title = "")
        {
            _popupMessage.Configure(title, message, centerButtonText, centerButtonClick);
            _popupMessage.transform.SetAsLastSibling();
            _popupMessage.SetActive(true);
        }

        public static void HideMessagePopup()
        {
            _popupMessage?.SetActive(false);
        }

        public static void ShowInputPopup(string hint, string buttonText, Action<string> inputSubmited, string title = "")
        {
            _popupInputField.Configure(hint, buttonText, inputSubmited, title);
            _popupInputField.transform.SetAsLastSibling();
            _popupInputField.SetActive(true);            
        }

        public static void HideInputPopup()
        {
            _popupInputField?.SetActive(false);
        }

        //************************************************* Internal utility ************************
        private void ShowRoomFinderScreen()
        {
            ClearScreen();

            if (_roomFinderScreen == null)
            {
                var screenObj = GlobalContext.assetBundleProvider.LoadUIScreenAsset("RoomFinderScreen");
                _roomFinderScreen = GameObject.Instantiate(screenObj.GetComponent<RoomFinderScreen>(), _rootCanvas.transform);
                _roomFinderScreen.RoomFinder = _roomFinder;
                _roomFinderScreen.gameObject.SetActive(true); //UI objects should start disabled so that they don't run awake on instantiate
            }

            _roomFinderScreen.SetActive(true);
        }
    }
}
