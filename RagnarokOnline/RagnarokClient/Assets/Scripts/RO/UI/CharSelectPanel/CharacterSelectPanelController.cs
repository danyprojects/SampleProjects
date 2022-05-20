using RO.Network;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    public sealed class CharacterSelectPanelController : UIController.Panel
        , IKeyboardHandler
    {
#pragma warning disable 0649
        [Serializable]
        private struct Stats
        {
            public Text _str;
            public Text _agi;
            public Text _vit;
            public Text _int;
            public Text _dex;
            public Text _luk;
        }

        [Serializable]
        private struct Info
        {
            public Text _name;
            public Text _job;
            public Text _lvl;
            public Text _exp;
            public Text _hp;
            public Text _sp;
            public Text _map;
        }
#pragma warning restore 0649

        [SerializeField]
        private RectTransform _selector = default;

        [SerializeField]
        private SimpleCursorButton _leftButton = default;

        [SerializeField]
        private SimpleCursorButton _rightButton = default;

        [SerializeField]
        private Stats _stats = default;

        [SerializeField]
        private Info _info = default;

        [SerializeField]
        private Text _usedSlotsLabel = default;

        [SerializeField]
        private Text _usedSlotsOutline = default;

        [SerializeField]
        private Text _currentIndex = default;

        [SerializeField]
        private Button _makeButton = default;

        [SerializeField]
        private Button _cancelButton = default;

        [SerializeField]
        private Button _okButton = default;

        /* [SerializeField]
         private Button _deleteButton = default;

         [SerializeField]
         private Button _cancelDeleteButton = default;

         [SerializeField]
         private Button _renameButton = default;*/

        [SerializeField]
        private Media.UIPlayerAnimatorController[] _uiPlayerAnimationController = default;

        [SerializeField]
        private SimpleButton _slot1Button = default;

        [SerializeField]
        private SimpleButton _slot2Button = default;

        [SerializeField]
        private SimpleButton _slot3Button = default;

        public const int MAX_CHARACTER_SLOTS = 9;

        private int _selectedCharIndex = 0;

        private const int _selectorStep = 162;
        private Vector2[] _selectorPosition;

        private UIController.CharacterInfo[] _availableCharacters;

        public CharacterSelectPanelController()
        {
            PacketDistributer.RegisterCallback(PacketIds.RCV_EnterCharacterSelect, OnEnterCharacterSelect);
        }

        private void Awake()
        {
            _selectorPosition = new Vector2[3];
            _selectorPosition[0] = _selector.anchoredPosition;
            _selectorPosition[1] = _selector.anchoredPosition;
            _selectorPosition[1].x += _selectorStep;
            _selectorPosition[2] = _selectorPosition[1];
            _selectorPosition[2].x += _selectorStep;

            _leftButton.OnClick = OnClickLeft;
            _rightButton.OnClick = OnClickRight;
            _okButton.OnClick = OnClickOk;
            _makeButton.OnClick = OnClickMake;
            _cancelButton.OnClick = OnClickCancel;

            _slot1Button.OnClick = OnSlot1Click;
            _slot2Button.OnClick = OnSlot2Click;
            _slot3Button.OnClick = OnSlot3Click;

            ResetStats();
        }

        public new void OnEnable()
        {
            base.OnEnable();
            EventSystem.CurrentKeyboardHandler = this;

            _availableCharacters = GetCharInfos();
            var selectedCharacter = UiController.GetSelectedCharacter();

            for (int i = 0; i < _availableCharacters.Length; i++)
            {
                if (ReferenceEquals(_availableCharacters[i], selectedCharacter))
                {
                    _selectedCharIndex = i;
                    break;
                }
            }

            UpdateSelectedCharacter();
        }

        private new void OnDisable()
        {
            if (ReferenceEquals(EventSystem.CurrentKeyboardHandler, this))
                EventSystem.CurrentKeyboardHandler = null;
            RO.Media.CursorAnimator.UnsetAnimation(RO.Media.CursorAnimator.Animations.Click);
        }

        public void OnKeyDown(Event evt)
        {
            switch (evt.keyCode)
            {
                case KeyCode.LeftArrow:
                    OnClickLeft();
                    break;
                case KeyCode.RightArrow:
                    OnClickRight();
                    break;
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    if (_availableCharacters[_selectedCharIndex].name == null)
                        OnClickMake();
                    else
                        OnClickOk();
                    break;
            }
        }

        public void OnKeyboardFocus(bool hasFocus) { } // we deal with it on enable disable

        public void OnEnterCharacterSelect(RCV_Packet packet)
        {
            Debug.Log("Received EnterCharSelect");

            LoginPanelSetActive(false); // in case we came from lobby
            gameObject.SetActive(true);

            RCV_EnterCharacterSelect rcvPacket = (RCV_EnterCharacterSelect)packet;

            //reset every character
            for (int i = 0; i < _availableCharacters.Length; i++)
                _availableCharacters[i].name = null;

            foreach (var info in rcvPacket.CharInfos)
            {
                var character = _availableCharacters[info.charSlot];

                character.name = info.name;
                character.job = info.job;
                character.hairstyle = info.hairstyle;
                character.baseLevel = info.lvl;
                character.currentHp = info.hp;
                character.currentSp = info.sp;
                character.str = info.str;
                character.agi = info.agi;
                character.vit = info.vit;
                character.int_ = info.int_;
                character.dex = info.dex;
                character.luk = info.luk;
                character.totalExp = info.exp;
                character.gender = (RO.Common.Gender)info.gender;

                //Missing these
                /*public short upperHeadgear;
                public short midHeadgear;
                public short lowerHeadgear;
                public short mapId;*/
            }

            _selectedCharIndex = 0;
            UpdateSelectedCharacter();
        }

        public void OnSlot1Click()
        {
            int index = _selectedCharIndex % 3;
            if (index == 0)
                return;

            _selectedCharIndex -= index == 1 ? 1 : 2;

            UpdateSelectedCharacter();
        }

        public void OnSlot2Click()
        {
            int index = _selectedCharIndex % 3;
            if (index == 1)
                return;

            _selectedCharIndex += index == 2 ? -1 : 1;

            UpdateSelectedCharacter();
        }

        public void OnSlot3Click()
        {
            int index = _selectedCharIndex % 3;
            if (index == 2)
                return;

            _selectedCharIndex += index == 1 ? 1 : 2;

            UpdateSelectedCharacter();
        }

        private void OnClickOk()
        {
            SetSelectedCharacter(_availableCharacters[_selectedCharIndex]);

            SND_SelectCharacter packet = new SND_SelectCharacter();
            packet.Index = (byte)_selectedCharIndex;

            NetworkController.SendPacket(packet);
        }

        private void OnClickCancel()
        {
            gameObject.SetActive(false);
            TriggerDisconnect();
            LoginPanelSetActive(true);
        }

        private void OnClickMake()
        {
            SetSelectedCharacter(_availableCharacters[_selectedCharIndex]);
            gameObject.SetActive(false);

            CharacterMakePanelSetActive(true);
        }

        private void OnClickLeft()
        {
            if (_selectedCharIndex > 0)
                _selectedCharIndex--;
            else
                _selectedCharIndex = MAX_CHARACTER_SLOTS - 1;

            UpdateSelectedCharacter();
        }

        private void OnClickRight()
        {
            _selectedCharIndex++;
            _selectedCharIndex %= MAX_CHARACTER_SLOTS;
            UpdateSelectedCharacter();
        }

        private void UpdateSelectedCharacter()
        {
            if (_availableCharacters[_selectedCharIndex].name != null)
            {
                _okButton.gameObject.SetActive(true);
                _makeButton.gameObject.SetActive(false);
            }
            else
            {
                _okButton.gameObject.SetActive(false);
                _makeButton.gameObject.SetActive(true);
            }

            UpdateSelectorPosition();
            DisplayCharacters();
            DisplaySelectedCharacterStats();
        }

        private void DisplaySelectedCharacterStats()
        {
            var character = _availableCharacters[_selectedCharIndex];

            if (character.name != null)
            {
                _info._name.text = character.name;
                _info._job.text = character.job.ToString();
                _info._lvl.text = character.baseLevel.ToString();
                _info._exp.text = character.totalExp.ToString();
                _info._hp.text = character.currentHp.ToString();
                _info._sp.text = character.currentSp.ToString();
                _info._map.text = character.currentMap;

                _stats._str.text = character.str.ToString();
                _stats._agi.text = character.agi.ToString();
                _stats._vit.text = character.vit.ToString();
                _stats._int.text = character.int_.ToString();
                _stats._luk.text = character.luk.ToString();
                _stats._dex.text = character.dex.ToString();
            }
            else
                ResetStats();
        }

        private void ResetStats()
        {
            _info._name.text = null;
            _info._job.text = null;
            _info._lvl.text = null;
            _info._exp.text = null;
            _info._hp.text = null;
            _info._sp.text = null;
            _info._map.text = null;

            _stats._str.text = null;
            _stats._agi.text = null;
            _stats._vit.text = null;
            _stats._int.text = null;
            _stats._luk.text = null;
            _stats._dex.text = null;
        }

        private void DisplayCharacters()
        {
            int leftCharacterIndex = 0;

            if (_selectedCharIndex % 3 == 0)
                leftCharacterIndex = _selectedCharIndex;
            else if ((_selectedCharIndex - 1) % 3 == 0)
                leftCharacterIndex = _selectedCharIndex - 1;
            else
                leftCharacterIndex = _selectedCharIndex - 2;

            for (int i = 0; i < 3; i++)
            {
                var info = _availableCharacters[leftCharacterIndex + i];

                if (info.name != null) //just use the name to detect empty slots
                {
                    _uiPlayerAnimationController[i].gameObject.SetActive(true);
                    _uiPlayerAnimationController[i].AnimatePlayer(info.gender, info.job,
                        new RO.MapObjects.Character.Direction(0, 0), info.hairstyle, info.isMounted);
                }
                else
                {
                    _uiPlayerAnimationController[i].gameObject.SetActive(false);
                }
            }

            int count = 0;
            foreach (var character in _availableCharacters)
                if (character.name != null)
                    count++;



            _usedSlotsLabel.text = count.ToString();
            _usedSlotsOutline.text = _usedSlotsLabel.text;
        }

        private void UpdateSelectorPosition()
        {
            int index = _selectedCharIndex % 3;

            _currentIndex.text = (_selectedCharIndex < 3 ? 1 :
                                  _selectedCharIndex < 6 ? 2 : 3).ToString();

            _selector.anchoredPosition = _selectorPosition[index];
        }

        public static CharacterSelectPanelController Instantiate(UIController uiController, Transform parent)
        {
            return Instantiate<CharacterSelectPanelController>(uiController, parent, "CharacterSelectPanel");
        }
    }
}
