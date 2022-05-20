using RO.Network;
using UnityEngine;

namespace RO.UI
{
    public sealed class GameOptionPanelController : UIController.Panel
        , IPointerDownHandler
    {
        [SerializeField] private Button _returnLastSavePoint = default;
        [SerializeField] private Button _charSelect = default;
        [SerializeField] private Button _settings = default;
        [SerializeField] private Button _sound = default;
        [SerializeField] private Button _shortcut = default;
        [SerializeField] private Button _existWindows = default;
        [SerializeField] private Button _returnToGame = default;

        private RectTransform _panelTransform;
        private bool _isCharacterDead = false;
        private bool _pendingReturnToLobby = false;

        private void Awake()
        {
            _panelTransform = GetComponent<RectTransform>();

            _returnLastSavePoint.OnClick = onReturnLastSavePoint;
            _charSelect.OnClick = onReturnCharSelect;
            _settings.OnClick = onSettings;
            _sound.OnClick = onSound;
            _shortcut.OnClick = onShortcuts;
            _existWindows.OnClick = onExitWindows;
            _returnToGame.OnClick = onReturnToGame;

            gameObject.SetActive(false);
        }

        public void SetCharacterDead(bool isDead)
        {
            if (_isCharacterDead == isDead)
                return;

            _isCharacterDead = isDead;
            refreshPanel();
        }

        public void EnteredLobby()
        {
            _pendingReturnToLobby = false;
        }

        private new void OnEnable()
        {
            base.OnEnable();
            refreshPanel();
        }

        private void refreshPanel()
        {
            _returnLastSavePoint.gameObject.SetActive(_isCharacterDead);
            _charSelect.gameObject.SetActive(!_isCharacterDead);
            _settings.gameObject.SetActive(!_isCharacterDead);
            _sound.gameObject.SetActive(!_isCharacterDead);
            _shortcut.gameObject.SetActive(!_isCharacterDead);
            _existWindows.gameObject.SetActive(!_isCharacterDead);

            _panelTransform.sizeDelta = new Vector2(280, _isCharacterDead ? 70 : 162);
        }

        private void onReturnLastSavePoint()
        {
            gameObject.SetActive(false);
        }

        private void onReturnCharSelect()
        {
            gameObject.SetActive(false);
            if (!_pendingReturnToLobby)
                NetworkController.SendPacket(new SND_ReturnToCharacterSelect());
            _pendingReturnToLobby = true;
        }

        private void onSettings()
        {
            gameObject.SetActive(false);
        }

        private void onSound()
        {
            SoundPanelSetActive(true);
            gameObject.SetActive(false);
        }

        private void onShortcuts()
        {
            ShortcutPanelSetActive(true);
            gameObject.SetActive(false);
        }

        private void onExitWindows()
        {
            Application.Quit();
        }

        private void onReturnToGame()
        {
            gameObject.SetActive(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            BringToFront();
        }

        public static GameOptionPanelController Instantiate(UIController uiController, Transform parent)
        {
            var controller = Instantiate<GameOptionPanelController>(uiController, parent, "GameOptionPanel");
            controller.gameObject.SetActive(true);
            return controller;
        }
    }
}