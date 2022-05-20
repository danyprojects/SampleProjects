using System;
using UnityEngine;

using Bacterio.Game;
using Bacterio.UI.Screens;
using Bacterio.UI.Menus;
using Bacterio.UI.Panels;

namespace Bacterio.UI
{
    public sealed class GameUIController
    {
        private readonly  Canvas _gameCanvas = null;
        private readonly BackgroundCanvas _backgroundCanvas = null;

        private OptionsRibbonPanel _optionsRibbonPanel = null;
        private GameOptionsMenu _gameOptionsMenu = null;
        private CellBasicInfoPanel _cellBasicInfoPanel = null;
        private PlayerDeathScreen _playerDeathScreen = null;
        private DisconnectedScreen _disconnectedScreen = null;
        private CountdownPanel _countdownPanel = null;
        private GameEndScreen _gameEndScreen = null;
        private UpgradeShopPanel _upgradeShopPanel = null;

        public GameUIController(Canvas gameCanvas, BackgroundCanvas backgroundCanvas)
        {
            _gameCanvas = gameCanvas;
            _backgroundCanvas = backgroundCanvas;
        }

        public void StartCellPlayerUI(Action gameQuitCb)
        {
            if(_optionsRibbonPanel == null)
            {
                //Instantiate options panel (top right)
                var optionsObj = GlobalContext.assetBundleProvider.LoadUIScreenAsset("OptionsRibbonPanel");
                _optionsRibbonPanel = GameObject.Instantiate(optionsObj.GetComponent<OptionsRibbonPanel>(), _gameCanvas.transform);
                _optionsRibbonPanel.OnGameOptionsCb = ShowGameOptionsMenu;
                _optionsRibbonPanel.OnUpgradeShopCb = ToggleUpgradeShopPanel;
                _optionsRibbonPanel.gameObject.SetActive(true);

                //instantiate player info panel (top left)
                var cellInfoObj = GlobalContext.assetBundleProvider.LoadUIScreenAsset("CellBasicInfoPanel");
                _cellBasicInfoPanel = GameObject.Instantiate(cellInfoObj.GetComponent<CellBasicInfoPanel>(), _gameCanvas.transform);
                _cellBasicInfoPanel.gameObject.SetActive(true);
                _cellBasicInfoPanel.SetActive(true);

                //instantiate the upgrade shop. Will be needed in most games
                var panelObj = GlobalContext.assetBundleProvider.LoadUIScreenAsset("UpgradeShopPanel");
                _upgradeShopPanel = GameObject.Instantiate(panelObj.GetComponent<UpgradeShopPanel>(), _gameCanvas.transform);
                _upgradeShopPanel.gameObject.SetActive(true);

                //Instantiate Game options menu too, will be needed in most cases
                var menuObj = GlobalContext.assetBundleProvider.LoadUIScreenAsset("GameOptionsMenu");
                _gameOptionsMenu = GameObject.Instantiate(menuObj.GetComponent<GameOptionsMenu>(), _gameCanvas.transform);
                _gameOptionsMenu.OnCloseButtonCb = () =>
                {
                    GameContext.inputHandler.IsPaused = false;
                    _backgroundCanvas.SetActive(false);
                    _gameOptionsMenu.SetActive(false);
                };
                _gameOptionsMenu.OnReturnToMainMenuCb = gameQuitCb;
                _gameOptionsMenu.gameObject.SetActive(true);
            }

            _optionsRibbonPanel.SetActive(true);
        }

        public void ShowPlayerDeathScreen(int respawnTime, Action onRespawnTimeEnd)
        {
            Clear();

            if (_playerDeathScreen == null)
            {
                var screenObj = GlobalContext.assetBundleProvider.LoadUIScreenAsset("PlayerDeathScreen");
                _playerDeathScreen = GameObject.Instantiate(screenObj.GetComponent<PlayerDeathScreen>(), _gameCanvas.transform);
                _playerDeathScreen.SetBackgroundCanvas(_backgroundCanvas);
                _playerDeathScreen.gameObject.SetActive(true);
            }

            _playerDeathScreen.SetRemainingTime(respawnTime, onRespawnTimeEnd);
            _playerDeathScreen.SetActive(true);
        }
        
        public void ConfigureUpgradeShopPanel(MapObjects.Cell localCell, Action<MapObjects.Cell, Databases.AuraDbId> auraAttachCb, Action<MapObjects.Cell, Databases.BuffDbId> applyBuffCb)
        {
            _upgradeShopPanel.LocalCell = localCell;
            _upgradeShopPanel.AuraAttachCb = auraAttachCb;
            _upgradeShopPanel.ApplyBuffCb = applyBuffCb;
        }

        public void SetDisconnectedScreen(bool enabled, Action gameQuitCb)
        {
            Clear();

            if (_disconnectedScreen == null)
            {
                var screenObj = GlobalContext.assetBundleProvider.LoadUIScreenAsset("DisconnectedScreen");
                screenObj = GameObject.Instantiate(screenObj, _gameCanvas.transform);
                _disconnectedScreen = screenObj.GetComponent<DisconnectedScreen>();
                _disconnectedScreen.OnQuitClickCb = gameQuitCb;
                _disconnectedScreen.gameObject.SetActive(true);
            }

            _backgroundCanvas.SetDimmedBackground();
            _backgroundCanvas.SetActive(enabled);
            _disconnectedScreen.SetActive(enabled);
        }

        public void ShowGameEndResultsScreen(Action gameQuitCb)
        {
            Clear();

            if (_gameEndScreen == null)
            {
                var screenObj = GlobalContext.assetBundleProvider.LoadUIScreenAsset("GameEndScreen");
                _gameEndScreen = GameObject.Instantiate(screenObj.GetComponent<GameEndScreen>(), _gameCanvas.transform);
                _gameEndScreen.OnQuitClickCb = gameQuitCb;
                _gameEndScreen.gameObject.SetActive(true);
            }

            _backgroundCanvas.SetDimmedBackground();
            _backgroundCanvas.SetActive(true);
            _gameEndScreen.ShowResults();
            _gameEndScreen.SetActive(true);
        }

        public void ShowCountdownPanel(long currentTimeMs, int durationMs)
        {
            //instantiate panel if needed
            if (_countdownPanel == null)
            {
                var panelObj = GlobalContext.assetBundleProvider.LoadUIScreenAsset("CountdownPanel");
                panelObj = GameObject.Instantiate(panelObj, _gameCanvas.transform);
                _countdownPanel = panelObj.GetComponent<CountdownPanel>();
            }

            _countdownPanel.SetDurationTime(currentTimeMs, durationMs);
            _countdownPanel.gameObject.SetActive(true);
            _countdownPanel.SetActive(true);
        }

        public void Clear()
        {
            _playerDeathScreen?.SetActive(false);
            _disconnectedScreen?.SetActive(false);
            _gameOptionsMenu?.SetActive(false);
            _gameEndScreen?.SetActive(false);
            _upgradeShopPanel?.SetActive(false);
        }

        public void Dispose()
        {
            GameObject.Destroy(_optionsRibbonPanel?.gameObject);
            GameObject.Destroy(_gameOptionsMenu?.gameObject);
            GameObject.Destroy(_playerDeathScreen?.gameObject);
            GameObject.Destroy(_disconnectedScreen?.gameObject);
            GameObject.Destroy(_cellBasicInfoPanel?.gameObject);
            GameObject.Destroy(_countdownPanel?.gameObject);
            GameObject.Destroy(_gameEndScreen?.gameObject);
            GameObject.Destroy(_upgradeShopPanel?.gameObject);
        }

        //This is private since it should only appear as a result of another UI click.
        private void ShowGameOptionsMenu()
        {
            WDebug.Assert(_gameOptionsMenu != null, "Game options menu wasnt instantiated yet");

            GameContext.inputHandler.IsPaused = true;
            _gameOptionsMenu.SetActive(true);
        }

        public void ToggleUpgradeShopPanel()
        {
            if (_upgradeShopPanel.enabled)
            {
                _upgradeShopPanel.SetActive(false);
            }
            else
            {
                Clear();
                _upgradeShopPanel.SetActive(true);
            }
        }

    }
}
