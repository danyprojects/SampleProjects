using RO.IO;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    public partial class ShortcutPanelController : UIController.Panel
        , IPointerDownHandler
    {
#pragma warning disable 0649
        [Serializable]
        private struct Tabs
        {
            public TabButton skillBar;
            public TabButton _interface;
            public TabButton macros;
        }
#pragma warning restore 0649

        [SerializeField]
        private Sprite rowHighligthedSprite = default;
        [SerializeField]
        private Label label = default;
        [SerializeField]
        private Button resetButton = default;
        [SerializeField]
        private Button applyButton = default;
        [SerializeField]
        private Button cancelButton = default;
        [SerializeField]
        private Button closeButton = default;

        [SerializeField]
        private SimpleButton enableBMButton = default;
        [SerializeField]
        private Toggle enableBMToggle = default;

        [SerializeField]
        private Tabs tabs = default;

        [SerializeField]
        private Slot[] _slots = new Slot[36];

        private int interfaceTabNumber = 0; // we don't use more for now so it's hardcoded
        private bool isCollisionDialogOpen = false;

        private enum Tab
        {
            Skillbar,
            Interface,
            Macro
        }

        private Tab activeTab = Tab.Skillbar;
        private Slot _selectedSlot = null;
        private Sprite _rowDefaultSprite;
        private RectTransform _labelRectTransform;
        private float _defaultRowWidth;
        private float _skillBarRowWidth;
        private KeyCode[] _allowedKeys = null;

        private void Awake()
        {
            closeButton.OnClick = OnCloseClick;
            resetButton.OnClick = OnResetClick;
            applyButton.OnClick = OnApplyClick;
            cancelButton.OnClick = OnCancelClick;

            tabs.skillBar.OnClick = () => OnTabClick(Tab.Skillbar);
            tabs._interface.OnClick = () => OnTabClick(Tab.Interface);
            tabs.macros.OnClick = () => OnTabClick(Tab.Macro);

            enableBMToggle.OnValueChanged = OnEnableBMValueChanged;
            enableBMButton.OnClick = () => enableBMToggle.SetValue(!enableBMToggle.IsOn);

            var image = _slots.First().GetComponent<Image>();
            _rowDefaultSprite = image.sprite;

            _skillBarRowWidth = image.rectTransform.sizeDelta.x;
            _defaultRowWidth = _skillBarRowWidth + 38;

            _labelRectTransform = (RectTransform)label.transform.parent;
        }

        private void Update()
        {
            // anyKeyDown will only return true during the frame a key is pressed
            if (_selectedSlot == null || isCollisionDialogOpen || !Input.anyKeyDown)
                return;

            Slot.Keys keyBind = default;

            // Check for special keys                
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                _selectedSlot.ProcessInputKeys(keyBind);
                return;
            }

            // TODO: should tab be handled?

            // If press wasn't a special key, search for the pressed key. If multiple are pressed, stops at first one
            for (int i = 0; i < _allowedKeys.Length; i++)
            {
                if (Input.GetKeyDown(_allowedKeys[i]))
                {
                    keyBind.keyCode = _allowedKeys[i];
                    break;
                }
            }

            //If no key was found just quit
            if (keyBind.keyCode == KeyCode.None)
                return;

            // If we found a valid keypress then get the command keys and update cached
            keyBind.commandKeys = (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) ? KeyBinder.CommandKeys.Alt : KeyBinder.CommandKeys.None;
            keyBind.commandKeys |= (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? KeyBinder.CommandKeys.Shift : KeyBinder.CommandKeys.None;
            keyBind.commandKeys |= (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) ? KeyBinder.CommandKeys.Ctrl : KeyBinder.CommandKeys.None;

            _selectedSlot.ProcessInputKeys(keyBind);
        }

        private new void OnEnable()
        {
            base.OnEnable();

            InitializeAllowedKeys();
            KeyBinder.IsBlockingAllKeys = true;
            EscController.Disable();
        }

        private new void OnDisable()
        {
            base.OnDisable();

            _allowedKeys = null;
            KeyBinder.IsBlockingAllKeys = false;
            EscController.Enable();
        }

        private void InitializeAllowedKeys()
        {
            _allowedKeys = new KeyCode[]
            {
                //Number row on top of alphanumeric keyboard
                KeyCode.Alpha0, KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4,
                KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9,

                //Letter keys by rows, starting from the top
                KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T, KeyCode.Y, KeyCode.U, KeyCode.I, KeyCode.O, KeyCode.P,
                KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L,
                KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V, KeyCode.B, KeyCode.N, KeyCode.M,

                //F1-15 keys                
                KeyCode.F1, KeyCode.F2, KeyCode.F3, KeyCode.F4, KeyCode.F5, KeyCode.F6, KeyCode.F7, KeyCode.F8,
                KeyCode.F9, KeyCode.F10, KeyCode.F11, KeyCode.F12, KeyCode.F13, KeyCode.F14, KeyCode.F15,
                
                //Extra mouse buttons (2 is scrollwheel click)
                KeyCode.Mouse2, KeyCode.Mouse3, KeyCode.Mouse4, KeyCode.Mouse5, KeyCode.Mouse6,

                //Numpad keys
                KeyCode.Keypad0, KeyCode.Keypad1, KeyCode.Keypad2, KeyCode.Keypad3, KeyCode.Keypad4,
                KeyCode.Keypad5, KeyCode.Keypad6, KeyCode.Keypad7, KeyCode.Keypad8, KeyCode.Keypad9,
                KeyCode.KeypadPeriod, KeyCode.KeypadDivide, KeyCode.KeypadMultiply, KeyCode.KeypadMinus,
                KeyCode.KeypadPlus, KeyCode.KeypadEquals, KeyCode.Numlock,

                //Alphanumeric operator keys
                KeyCode.Exclaim, KeyCode.DoubleQuote, KeyCode.Hash, KeyCode.Dollar, KeyCode.Percent, KeyCode.Ampersand,
                KeyCode.Quote, KeyCode.LeftParen, KeyCode.RightParen, KeyCode.Asterisk, KeyCode.Plus, KeyCode.Comma,
                KeyCode.Minus, KeyCode.Period, KeyCode.Slash, KeyCode.Backslash, KeyCode.Colon, KeyCode.Semicolon, KeyCode.Less,
                KeyCode.Caret, KeyCode.Equals, KeyCode.Greater, KeyCode.Question, KeyCode.At, KeyCode.LeftBracket, KeyCode.RightBracket,
                KeyCode.Underscore, KeyCode.BackQuote, KeyCode.LeftCurlyBracket, KeyCode.RightCurlyBracket,  KeyCode.Pipe, KeyCode.Tilde, 
                
                //Misc keys
                KeyCode.Space, KeyCode.Backspace, KeyCode.Delete, KeyCode.Clear, KeyCode.Pause,
                KeyCode.Insert, KeyCode.End, KeyCode.Home, KeyCode.PageDown, KeyCode.PageUp,
                KeyCode.AltGr, KeyCode.Menu, KeyCode.CapsLock, KeyCode.ScrollLock
            };
        }

        private void OnCloseClick()
        {
            OnCancelClick();
            gameObject.SetActive(false);
        }

        private void OnResetClick()
        {
            if (_selectedSlot != null)
                _selectedSlot.DeselectSlot();

            foreach (var slot in _slots)
                slot.OnReset();
        }

        private void OnApplyClick()
        {
            if (_selectedSlot != null)
                _selectedSlot.DeselectSlot();

            foreach (var slot in _slots)
                slot.OnApply();
        }

        private void OnCancelClick()
        {
            if (_selectedSlot != null)
                _selectedSlot.DeselectSlot();

            foreach (var slot in _slots)
                slot.OnCancel();
        }

        private void OnTabClick(Tab tab)
        {
            if (activeTab == tab)
                return;
            activeTab = tab;

            if (_selectedSlot != null)
                _selectedSlot.DeselectSlot();

            foreach (var slot in _slots)
                slot.OnTabChanged();
        }

        private void OnEnableBMValueChanged(bool value)
        {
            // TODO:: do we even want to support BM on/off ?
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            BringToFront();
        }

        public static ShortcutPanelController Instantiate(UIController uiController, Transform parent)
        {
            return Instantiate<ShortcutPanelController>(uiController, parent, "ShortcutPanel");
        }
    }
}