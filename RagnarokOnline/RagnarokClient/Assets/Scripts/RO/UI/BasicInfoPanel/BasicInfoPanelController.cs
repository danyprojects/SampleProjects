using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    public sealed class BasicInfoPanelController : UIController.Panel
        , IPointerDownHandler
    {
        private readonly int hpSpRedPercentage = 50;
        private readonly int weightRedPercentage = 20;
        private readonly string baseLabelText = "Basic Information";

        private RectTransform _basicInfoTransform;

        [SerializeField]
        private RectTransform fullBodyTransform = default;
        [SerializeField]
        private RectTransform miniBodyTransform = default;
        [SerializeField]
        private Text menuLabelParentText = default;
        [SerializeField]
        private Text menuLabelChildText = default;

        [SerializeField]
        private Button _minimizeButton = default;

        [SerializeField]
        private Text _nameText = default;
        [SerializeField]
        private Text _jobText = default;

        [SerializeField]
        private Text _maxHpText = default;
        [SerializeField]
        private Text _currentHpText = default;
        [SerializeField]
        private Text _percentageHpText = default;
        [SerializeField]
        private Slider _hpSlider = default;
        [SerializeField]
        private Image _hpRedSliderImage = default;

        [SerializeField]
        private Text _maxSpText = default;
        [SerializeField]
        private Text _currentSpText = default;
        [SerializeField]
        private Text _percentageSpText = default;
        [SerializeField]
        private Slider _spSlider = default;
        [SerializeField]
        private Image _spRedSliderImage = default;

        [SerializeField]
        private Text _baseLvlText = default;
        [SerializeField]
        private Text _jobLvlText = default;
        [SerializeField]
        private Slider _baseExpSlider = default;
        [SerializeField]
        private Slider _jobExpSlider = default;

        [SerializeField]
        private Text _zenyText = default;

        [SerializeField]
        private Text _weightText = default;

        [SerializeField]
        private Text _miniBodyClassExpLvlText = default;
        [SerializeField]
        private Text _miniBodyHpText = default;
        [SerializeField]
        private Text _miniBodySpText = default;

        [SerializeField]
        private BasicInfoPanelExtraButton _infoButton = default;
        [SerializeField]
        private BasicInfoPanelExtraButton _itemsButton = default;
        [SerializeField]
        private BasicInfoPanelExtraButton _skillsButton = default;

        [SerializeField]
        private Toggle _extrasToggle = default;

        private Transform _baseExpBackground;
        private Transform _jobExpBackground;

        private bool _isWeightRed;
        private int _currentWeight;
        private int _maxWeight;
        private int _weightPercentage;

        private bool _isHpRed;
        private int _currentHp;
        private int _maxHp;
        private int _percentageHp;

        private bool _isSpRed;
        private int _currentSp;
        private int _maxSp;
        private int _percentageSp;

        private int _currentJobLevel;
        private int _currentJobExp;
        private int _nextJobLvlExp = 1; // todo fix to table values

        private int _currentBaseLevel;
        private int _currentBaseExp;
        private int _nextBaseLvlExp = 1; // todo fix to table values

        private bool _isMinimized = false;
        private Vector2 _minimizeOffset;

        public static BasicInfoPanelController Instantiate(UIController uiController, Transform parent)
        {
            return Instantiate<BasicInfoPanelController>(uiController, parent, "BasicInfoPanel");
        }

        private void Awake()
        {
            IO.KeyBinder.RegisterAction(IO.KeyBinder.Shortcut.BasicInfo, () => onPressMinimize());

            _minimizeButton.OnClick = onPressMinimize;
            _minimizeButton.gameObject.GetComponent<LabelArea>().OnEnter
                = () => LabelController.ShowLabel(_minimizeButton.transform.position,
                IO.KeyBinder.GetShortcutAsString(IO.KeyBinder.Shortcut.BasicInfo), default);

            _infoButton.OnClick = onClickInfo;
            _infoButton.PointerEnter = onEnterExtraButton;
            _infoButton.PointerExit = onExitExtraButton;

            _skillsButton.OnClick = onClickSkill;
            _skillsButton.PointerEnter = onEnterExtraButton;
            _skillsButton.PointerExit = onExitExtraButton;

            _itemsButton.OnClick = onClickItem;
            _itemsButton.PointerEnter = onEnterExtraButton;
            _itemsButton.PointerExit = onExitExtraButton;

            _extrasToggle.OnValueChanged = OnExtrasValueChanged;

            var handler = _baseExpSlider.GetComponentInChildren<LabelArea>();
            _baseExpBackground = handler.transform;
            handler.OnEnter = OnBaseExpEnter;

            handler = _jobExpSlider.GetComponentInChildren<LabelArea>();
            _jobExpBackground = handler.transform;
            handler.OnEnter = OnJobExpEnter;

            _basicInfoTransform = GetComponent<RectTransform>();

            _minimizeOffset = new Vector2(0,
                    fullBodyTransform.sizeDelta.y - miniBodyTransform.sizeDelta.y);
        }

        private void onClickInfo()
        {

        }

        private void onClickSkill()
        {
            SkillPanelSetActive(true);
        }

        private void onClickItem()
        {

        }

        private void onEnterExtraButton()
        {
            _infoButton.setAlpha(1f);
            _skillsButton.setAlpha(1f);
            _itemsButton.setAlpha(1f);
        }

        private void onExitExtraButton()
        {
            _infoButton.setAlpha(0.5f);
            _skillsButton.setAlpha(0.5f);
            _itemsButton.setAlpha(0.5f);
        }

        private void OnExtrasValueChanged(bool value)
        {
            _infoButton.gameObject.SetActive(value);
            _skillsButton.gameObject.SetActive(value);
            _itemsButton.gameObject.SetActive(value);
        }

        public Vector3 GetLocalPosition()
        {
            return _basicInfoTransform.localPosition;
        }

        public Vector3 SetLocalPosition()
        {
            return _basicInfoTransform.localPosition;
        }

        public void SetMaxHP(int hp)
        {
            if (_maxHp == hp)
                return;

            _maxHp = hp;
            _maxHpText.text = hp.ToString();
            _hpSlider.maxValue = hp;

            hpUpdated();
        }

        public void SetHP(int hp)
        {
            if (_currentHp == hp)
                return;

            _currentHp = hp;
            _currentHpText.text = hp.ToString();
            _hpSlider.value = hp;

            hpUpdated();
        }

        public void SetMaxSP(int sp)
        {
            if (_maxSp == sp)
                return;

            _maxSp = sp;
            _maxSpText.text = sp.ToString();
            _spSlider.maxValue = sp;

            spUpdated();
        }

        public void SetSP(int sp)
        {
            if (_currentSp == sp)
                return;

            _currentSp = sp;
            _currentSpText.text = sp.ToString();
            _spSlider.value = sp;

            spUpdated();
        }

        public void SetName(string name)
        {
            _nameText.text = name;

            if (_isMinimized)
            {
                menuLabelParentText.text = name;
                menuLabelChildText.text = name;
            }
        }

        public void SetJob(string job)
        {
            _jobText.text = job;

            updateMiniBodyMainText();
        }

        public void SetBaseLvl(int lvl)
        {
            _currentBaseLevel = lvl;
            _baseLvlText.text = lvl.ToString("Base Lv\\. ###");

            updateMiniBodyMainText();
        }

        public void SetJobLvl(int lvl)
        {
            _currentJobLevel = lvl;
            _jobLvlText.text = lvl.ToString("Job Lv\\. ###");

            updateMiniBodyMainText();
        }

        public void SetZeny(int zeny)
        {
            _zenyText.text = zeny.ToString("Zeny : ##,#");
        }

        public void SetWeight(int weight)
        {
            if (weight == _currentWeight)
                return;

            _currentWeight = weight;

            weightUpdated();
        }

        public void SetMaxWeight(int weight)
        {
            if (weight == _maxWeight)
                return;

            _maxWeight = weight;

            weightUpdated();
        }

        public void SetJobExp(int exp)
        {
            if (exp == _currentJobExp)
                return;

            if (_currentJobExp == _nextJobLvlExp && !_jobExpSlider.gameObject.activeSelf)
            {
                _jobExpSlider.gameObject.SetActive(true);
            }
            else if (exp == _nextJobLvlExp)
            {
                _jobExpSlider.value = exp;
                _jobExpSlider.gameObject.SetActive(false);
            }

            _currentJobExp = exp;
            _jobExpSlider.value = exp;
        }

        public void SetJobNextLvlExp(int exp)
        {
            if (exp == _nextJobLvlExp)
                return;

            _nextJobLvlExp = exp;
            _jobExpSlider.maxValue = exp;
        }

        public void SetBaseExp(int exp)
        {
            if (exp == _currentBaseExp)
                return;

            if (_currentBaseExp == _nextBaseLvlExp && !_baseExpSlider.gameObject.activeSelf)
            {
                _baseExpSlider.gameObject.SetActive(true);
            }
            else if (exp == _nextBaseLvlExp)
            {
                _baseExpSlider.value = exp;
                _baseExpSlider.gameObject.SetActive(false);
            }

            _baseExpSlider.value = exp;
            _currentBaseExp = exp;

            updateMiniBodyMainText();
        }

        public void SetBaseNextLvlExp(int exp)
        {
            if (exp == _nextBaseLvlExp)
                return;

            _nextBaseLvlExp = exp;
            _baseExpSlider.maxValue = exp;

            updateMiniBodyMainText();
        }

        private void OnBaseExpEnter()
        {
            LabelController.ShowLabel(_baseExpBackground.position,
                (_currentBaseExp * 100 / (float)_nextBaseLvlExp).ToString("#0.0%"), default);
        }

        private void OnJobExpEnter()
        {
            LabelController.ShowLabel(_jobExpBackground.position,
                (_currentJobExp * 100 / (float)_nextJobLvlExp).ToString("#0.0%"), default);
        }

        private void hpUpdated()
        {
            _miniBodyHpText.text = string.Format("HP. {0} / {1}", _currentHp, _maxHp);

            int percentage = _currentHp * 100 / _maxHp;

            if (_percentageHp == percentage)
                return;

            _percentageHp = percentage;
            _percentageHpText.text = percentage.ToString("#####\\%");

            if (percentage <= hpSpRedPercentage)
            {
                if (!_isHpRed)
                {
                    _isHpRed = true;
                    _hpRedSliderImage.enabled = true;
                }
            }
            else
            {
                if (_isHpRed)
                {
                    _isHpRed = false;
                    _hpRedSliderImage.enabled = false;
                }
            }
        }

        private void spUpdated()
        {
            _miniBodySpText.text = string.Format("SP. {0} / {1}", _currentSp, _maxSp);

            int percentage = _currentSp * 100 / _maxSp;

            if (_percentageSp == percentage)
                return;

            _percentageSp = percentage;
            _percentageSpText.text = percentage.ToString("#####\\%");

            if (percentage <= hpSpRedPercentage)
            {
                if (!_isSpRed)
                {
                    _isSpRed = true;
                    _spRedSliderImage.enabled = true;
                }
            }
            else
            {
                if (_isSpRed)
                {
                    _isSpRed = false;
                    _spRedSliderImage.enabled = false;
                }
            }
        }

        private void weightUpdated()
        {
            _weightText.text = string.Format("Weight:{0}/{1}", _currentWeight, _maxWeight);

            int percentage = _currentWeight * 100 / _maxWeight;

            if (percentage == _weightPercentage)
                return;

            _weightPercentage = percentage;

            if (percentage <= weightRedPercentage)
            {
                if (!_isWeightRed)
                {
                    _isWeightRed = true;
                    _weightText.color = Color.red;
                }
            }
            else
            {
                if (_isWeightRed)
                {
                    _isWeightRed = false;
                    _weightText.color = Color.black;
                }
            }
        }

        private void updateMiniBodyMainText()
        {
            _miniBodyClassExpLvlText.text = string.Format("Lv. {0} / {1} / Lv. {2} / Exp. {3} %",
               _currentBaseLevel, _jobText.text, _currentJobLevel,
               (_currentBaseExp * 100 / (float)_nextBaseLvlExp).ToString("##.#"));
        }

        private void onPressMinimize()
        {
            if (!fullBodyTransform.gameObject.activeSelf)
            {
                _isMinimized = false;
                fullBodyTransform.gameObject.SetActive(true);
                miniBodyTransform.gameObject.SetActive(false);

                ((RectTransform)transform).sizeDelta += _minimizeOffset;

                menuLabelChildText.text = baseLabelText;
                menuLabelParentText.gameObject.SetActive(true);
            }
            else
            {
                _isMinimized = true;
                fullBodyTransform.gameObject.SetActive(false);
                miniBodyTransform.gameObject.SetActive(true);

                ((RectTransform)transform).sizeDelta -= _minimizeOffset;

                menuLabelChildText.text = _nameText.text;
                menuLabelParentText.gameObject.SetActive(false);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            BringToFront();
        }
    }
}
