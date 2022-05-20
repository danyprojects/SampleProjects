using RO.Common;
using RO.MapObjects;
using RO.Network;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    public sealed class CharacterMakePanelController : UIController.Panel
    {
#pragma warning disable 0649
        [Serializable]
        private struct Stat
        {
            public Button _button;
            public Text _text;
        }

        [Serializable]
        private struct CharRotate
        {
            public SimpleCursorButton _left;
            public SimpleCursorButton _right;
        }
#pragma warning restore 0649

        [SerializeField]
        private Stat _str = default;

        [SerializeField]
        private Stat _agi = default;

        [SerializeField]
        private Stat _vit = default;

        [SerializeField]
        private Stat _int = default;

        [SerializeField]
        private Stat _dex = default;

        [SerializeField]
        private Stat _luk = default;

        [SerializeField]
        private Text _currentUsedText = default;

        [SerializeField]
        private Text _currentUsedOutline = default;

        [SerializeField]
        private CharNameInputField _name = default;

        [SerializeField]
        private Toggle _maleToggle = default;

        [SerializeField]
        private Toggle _femaleToggle = default;

        [SerializeField]
        private CharRotate _charRotate = default;

        [SerializeField]
        private SimpleCursorButton _airStyleButton = default;

        [SerializeField]
        private Button _okButton = default;

        [SerializeField]
        private Button _cancelButton = default;

        [SerializeField]
        private Button _resetButton = default;

        [SerializeField]
        private HexagonRadarChart _chart = default;

        [SerializeField]
        private Media.UIPlayerAnimatorController _uiMaleAnimationController = default;

        private UIController.CharacterInfo _newCharacter;
        private Media.UIPlayerAnimatorController _uiFemaleAnimationController;

        private int currentUsed;
        private bool _ignoreRequest = false;

        private const int MAX_STAT_POINTS = 24;
        private const int MAX_STAT_VALUE = 9;

        private readonly Color _blueColor = new Color(90 / 255f, 107 / 255f, 157 / 255f);
        private readonly Color _redColor = new Color(255 / 255f, 57 / 255f, 123 / 255f);

        private Character.Direction _chardirection = new Character.Direction(0, 0);

        private Regex nameRegex = new Regex(@"[a-zA-Z](\s?[0-9a-zA-Z]){2,}", RegexOptions.Compiled);

        public CharacterMakePanelController()
        {
            PacketDistributer.RegisterCallback(PacketIds.RCV_ReplyCreateCharacter, OnCharacterCreateReply);
        }

        private void Awake()
        {
            _okButton.OnClick = OnClickOk;
            _cancelButton.OnClick = OnClickCancel;
            _charRotate._left.OnClick = OnClickRotateLeft;
            _charRotate._right.OnClick = OnClickRotateRight;

            _maleToggle.OnValueChanged = OnMaleValueChanged;
            _femaleToggle.OnValueChanged = OnFemaleValueChanged;

            _airStyleButton.OnClick = OnClickHairButton;

            _str._button.OnClick = OnClickStr;
            _agi._button.OnClick = OnClickAgi;
            _vit._button.OnClick = OnClickVit;
            _int._button.OnClick = OnClickInt;
            _dex._button.OnClick = OnClickDex;
            _luk._button.OnClick = OnClickLuk;

            _resetButton.OnClick = OnClickReset;

            _uiFemaleAnimationController = _uiMaleAnimationController.gameObject.AddComponent<Media.UIPlayerAnimatorController>();

            _uiMaleAnimationController.AnimatePlayer(Gender.Male, Jobs.Novice, _chardirection, 1, false);
            _uiFemaleAnimationController.AnimatePlayer(Gender.Female, Jobs.Novice, _chardirection, 1, false);
            _uiFemaleAnimationController.enabled = false;
        }

        public new void OnEnable()
        {
            base.OnEnable();
            EventSystem.SetSelectedGameObject(_name.gameObject);
            EventSystem.CurrentKeyboardHandler = _name;

            _ignoreRequest = false;
            _newCharacter = UiController.GetSelectedCharacter();


#if UNITY_EDITOR
            if (_newCharacter == null)
            {
                _newCharacter = new UIController.CharacterInfo();
            }
#endif

            ResetStats();
            UpdateUsedIndex();

            _chardirection.HeadCamera = 0;
            _chardirection.BodyCamera = 0;

            _newCharacter.hairstyle = 1;

            _uiMaleAnimationController.ChangedDirection();
            _uiFemaleAnimationController.ChangedDirection();
            _uiMaleAnimationController.ChangeHairstyle(_newCharacter.hairstyle);
            _uiFemaleAnimationController.ChangeHairstyle(_newCharacter.hairstyle);

            _maleToggle.SetValue(true);
            _name.Clear();
        }

        public new void OnDisable()
        {
            base.OnDisable();

            EventSystem.CurrentKeyboardHandler = null;
            RO.Media.CursorAnimator.UnsetAnimation(RO.Media.CursorAnimator.Animations.Click);
        }

        private void OnClickReset()
        {
            ResetStats();
#if DEBUG
            for (int i = 0; i < 8; i++)
            {
                OnClickVit();
                OnClickDex();
                OnClickStr();
            }
            _name.Text = "asdfasdf";
#endif
            UpdateUsedIndex();
        }

        private void OnClickHairButton()
        {
            _newCharacter.hairstyle++;
            _newCharacter.hairstyle %= Constants.MAX_HAIR_ID;

            _uiMaleAnimationController.ChangeHairstyle(_newCharacter.hairstyle);
            _uiFemaleAnimationController.ChangeHairstyle(_newCharacter.hairstyle);
        }

        private void UpdateUsedIndex()
        {
            _currentUsedText.text = currentUsed.ToString();
            _currentUsedOutline.text = _currentUsedText.text;

            if (currentUsed == MAX_STAT_POINTS)
            {
                _currentUsedText.color = _blueColor;
                _okButton.enabled = true;
            }
            else
            {
                _currentUsedText.color = _redColor;
                _okButton.enabled = false;
            }
        }

        private void OnMaleValueChanged(bool value)
        {
            _uiMaleAnimationController.enabled = value;
            if (value)
                _uiMaleAnimationController.PlayIdleAnimation();
        }

        private void OnFemaleValueChanged(bool value)
        {
            _uiFemaleAnimationController.enabled = value;
            if (value)
                _uiFemaleAnimationController.PlayIdleAnimation();
        }

        private void ResetStats()
        {
            currentUsed = 0;

            _newCharacter.str = 1;
            _newCharacter.luk = 1;
            _newCharacter.vit = 1;
            _newCharacter.int_ = 1;
            _newCharacter.dex = 1;
            _newCharacter.agi = 1;

            _chart.setRadarValue(HexagonRadarChart.Indexes.Bottom, 1f);
            _chart.setRadarValue(HexagonRadarChart.Indexes.BottomLeft, 1f);
            _chart.setRadarValue(HexagonRadarChart.Indexes.BottomRight, 1f);
            _chart.setRadarValue(HexagonRadarChart.Indexes.Top, 1f);
            _chart.setRadarValue(HexagonRadarChart.Indexes.TopLeft, 1f);
            _chart.setRadarValue(HexagonRadarChart.Indexes.TopRight, 1f);

            _vit._text.text = "1";
            _str._text.text = "1";
            _luk._text.text = "1";
            _dex._text.text = "1";
            _int._text.text = "1";
            _agi._text.text = "1";

            _chart.redrawRadarChart();
        }

        private void OnClickVit()
        {
            if (currentUsed < MAX_STAT_POINTS && _newCharacter.vit < MAX_STAT_VALUE)
            {
                currentUsed++;
                _newCharacter.vit++;
                _chart.setRadarValue(HexagonRadarChart.Indexes.TopRight, _newCharacter.vit);
                UpdateUsedIndex();
                _vit._text.text = _newCharacter.vit.ToString();
                _chart.redrawRadarChart();
            }
        }

        private void OnClickAgi()
        {
            if (currentUsed < MAX_STAT_POINTS && _newCharacter.agi < MAX_STAT_VALUE)
            {
                currentUsed++;
                _newCharacter.agi++;
                _chart.setRadarValue(HexagonRadarChart.Indexes.TopLeft, _newCharacter.agi);
                UpdateUsedIndex();
                _agi._text.text = _newCharacter.agi.ToString();
                _chart.redrawRadarChart();
            }
        }

        private void OnClickDex()
        {
            if (currentUsed < MAX_STAT_POINTS && _newCharacter.dex < MAX_STAT_VALUE)
            {
                currentUsed++;
                _newCharacter.dex++;
                _chart.setRadarValue(HexagonRadarChart.Indexes.BottomLeft, _newCharacter.dex);
                UpdateUsedIndex();
                _dex._text.text = _newCharacter.dex.ToString();
                _chart.redrawRadarChart();
            }
        }

        private void OnClickLuk()
        {
            if (currentUsed < MAX_STAT_POINTS && _newCharacter.luk < MAX_STAT_VALUE)
            {
                currentUsed++;
                _newCharacter.luk++;
                _chart.setRadarValue(HexagonRadarChart.Indexes.BottomRight, _newCharacter.luk);
                UpdateUsedIndex();
                _luk._text.text = _newCharacter.luk.ToString();
                _chart.redrawRadarChart();
            }
        }

        private void OnClickInt()
        {
            if (currentUsed < MAX_STAT_POINTS && _newCharacter.int_ < MAX_STAT_VALUE)
            {
                currentUsed++;
                _newCharacter.int_++;
                _chart.setRadarValue(HexagonRadarChart.Indexes.Bottom, _newCharacter.int_);
                UpdateUsedIndex();
                _int._text.text = _newCharacter.int_.ToString();
                _chart.redrawRadarChart();
            }
        }

        private void OnClickStr()
        {
            if (currentUsed < MAX_STAT_POINTS && _newCharacter.str < MAX_STAT_VALUE)
            {
                currentUsed++;
                _newCharacter.str++;
                _chart.setRadarValue(HexagonRadarChart.Indexes.Top, _newCharacter.str);
                UpdateUsedIndex();
                _str._text.text = _newCharacter.str.ToString();
                _chart.redrawRadarChart();
            }
        }

        private void ShowDialog(string message)
        {
            gameObject.SetActive(false);
            MessageDialogController.ShowDialog(message, () => gameObject.SetActive(true));
        }

        private bool ValidateName()
        {
            if (_name.Text == null || _name.Text.Length < 2)
            {
                ShowDialog("Name must have at least 2 characters");
                return false;
            }

            if (Char.IsDigit(_name.Text.First()))
            {
                ShowDialog("Name cannot start with a number");
                return false;
            }

            if (!nameRegex.IsMatch(_name.Text))
            {
                ShowDialog("Invalid name, use only alphanumeric characters and a single space between words");
                return false;
            }

            return true;
        }

        private void OnClickOk()
        {
            if (_ignoreRequest || !ValidateName())
                return;

            _newCharacter.name = _name.Text;
            _newCharacter.gender = _uiFemaleAnimationController.enabled ? Gender.Female : Gender.Male;

            SND_CreateCharacter _sndCreateChar = new SND_CreateCharacter();
            _sndCreateChar.Index = (byte)_newCharacter.index;
            _sndCreateChar.name = _name.Text;
            _sndCreateChar.str = (byte)_newCharacter.str;
            _sndCreateChar.agi = (byte)_newCharacter.agi;
            _sndCreateChar._int = (byte)_newCharacter.int_;
            _sndCreateChar.vit = (byte)_newCharacter.vit;
            _sndCreateChar.dex = (byte)_newCharacter.dex;
            _sndCreateChar.luk = (byte)_newCharacter.luk;
            _sndCreateChar.gender = (byte)_newCharacter.gender;
            _sndCreateChar.hairstyle = (byte)_newCharacter.hairstyle;

            NetworkController.SendPacket(_sndCreateChar);
        }

        private void OnClickCancel()
        {
            if (_ignoreRequest)
                return;

            _newCharacter.name = null; //so it won't show up in charselect

            gameObject.SetActive(false);
            CharacterSelectPanelSetActive(true);
        }

        private void OnClickRotateLeft()
        {
            _chardirection.HeadCamera = (_chardirection.HeadCamera + 1) % 8;
            _chardirection.BodyCamera = (_chardirection.BodyCamera + 1) % 8;

            _uiMaleAnimationController.ChangedDirection();
            _uiFemaleAnimationController.ChangedDirection();
        }

        private void OnClickRotateRight()
        {
            _chardirection.HeadCamera = (_chardirection.HeadCamera + 8 - 1) % 8;
            _chardirection.BodyCamera = (_chardirection.BodyCamera + 8 - 1) % 8;

            _uiMaleAnimationController.ChangedDirection();
            _uiFemaleAnimationController.ChangedDirection();
        }

        private void OnCharacterCreateReply(RCV_Packet packet)
        {
            _ignoreRequest = false;

            var replyPacket = (RCV_ReplyCreateCharacter)packet;
            Debug.Log("Received character create reply");

            if (replyPacket.replyStatus == RCV_ReplyCreateCharacter.ReplyStatus.Ok)
            {
                gameObject.SetActive(false);
                CharacterSelectPanelSetActive(true);
            }
            else
            {
                ShowDialog("Character name is already taken");
            }
        }

        public static CharacterMakePanelController Instantiate(UIController uiController, Transform parent)
        {
            return Instantiate<CharacterMakePanelController>(uiController, parent, "CharacterMakePanel");
        }
    }
}