using RO.Common;
using RO.Databases;
using RO.LocalPlayer;
using RO.Network;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    using Skill = SkillTree.Skill;
    using SkillTreeEntry = SkillTreeDb.SkillEntry;

    public partial class SkillPanelController : UIController.Panel, IScrollHandler
        , IPointerDownHandler
    {
#pragma warning disable 0649
        [Serializable]
        private struct Tabs
        {
            public Image firstJobBackground;
            public TabButton firstJob;
            public TabButton secondJob;
            public TabButton misc;
        }

        [Serializable]
        private struct FullSlotSprites
        {
            public Sprite noSkill;
            public Sprite redSkill;
            public Sprite blueSkill;
        }
#pragma warning restore 0649

        [SerializeField] private FullSlotSprites _fullSlotSprites = default;
        [SerializeField] private Sprite[] _skillSprites = new Sprite[(int)SkillIds.Last];

        [SerializeField] private UI.SkillFullSlotController[] _fullSlots = new UI.SkillFullSlotController[42];

        [SerializeField] private ListScrollView _miniBodyScrollView = default;
        [SerializeField] private MultiColumnListScrollView _fullBodyScrollView = default;

        [SerializeField] private Vector2 miniBodyDimentions = default;
        [SerializeField] private Vector2 fullBodyDimentions = default;

        [SerializeField] private Button minimizeButton = default;
        [SerializeField] private Tabs tabs = default;
        [SerializeField] private Toggle viewSkillInfoToggle = default;
        [SerializeField] private Button applyButton = default;
        [SerializeField] private Button resetButton = default;
        [SerializeField] private Button _closeButton = default;
        [SerializeField] private Text _skillPointsText = default;

        private enum TabSelectedType
        {
            FirstJob,
            SecondJob,
            Misc
        }

        private class SkillInfo
        {
            public SkillInfo Reset()
            {
                selectedLvl = 0;
                levelUpCache = 0;
                treeEntry = default;
                skill = null;

                return this;
            }

            public int selectedLvl; // for casting the skills
            public int levelUpCache; // for cashing skill level up requests
            public SkillTreeEntry treeEntry; // corresponding dbtree entry for this skill
            public Skill skill; // this is the gamelogic skill component
        }

        // miniScrollView indexes to this directly, fullScrollView hits _fullToMiniIndexTable first
        private SkillInfo[] _skillInfo = new SkillInfo[SkillTree.MaxCapacity];
        private FullSlotController[] _treeIndexToFullSlot = new FullSlotController[SkillTree.MaxCapacity]; // to search for requirements
        private int[] _fullToMiniIndexTable = new int[SkillTreeDb.MaxSlotIndex + 1 + Constants.MAX_PLAYER_EXTRA_SKILLS]; //allow spaces in the fullList

        private GameObject resizeButton;
        private TabSelectedType selectedTabType = TabSelectedType.FirstJob;
        private TabSelectedType selectedTabTypePreviousView = TabSelectedType.FirstJob;

        private int _miniBodyVisibleRows, _miniBodyDefaultRows;
        private int _fullBodyVisibleColumns, _fullBodyDefaultColumns;
        private int _fullBodyVisibleRows, _fullBodyDefaultRows;

        private int _secondJobIndex = 0;
        private int _miscIndex = 0;
        private int _skillCount = 0;

        private int _skillPoints = 0;
        private int _usedSkillPoints = 0;

        private bool _showSkillInfo = true;
        private bool _isMinimized = true;
        private bool _dirtyPreviousView = true;
        private bool _pendingLoad = false;
        private bool _pendingExtraSkillUpdate = false;

        private bool _levelUpLocked = false;

        SkillPanelController()
        {
            for (int i = 0; i < _skillInfo.Length; i++)
                _skillInfo[i] = new SkillInfo().Reset();
        }

        void Awake()
        {
            IO.KeyBinder.RegisterAction(IO.KeyBinder.Shortcut.Skills,
                () => gameObject.SetActive(!gameObject.activeSelf));

            var resize = GetComponentInChildren<ResizePanel>();
            resize.OnResize = OnResize;
            resizeButton = resize.gameObject;

            minimizeButton.OnClick = onClickMinimize;

            _closeButton.OnClick = onClose;
            _closeButton.gameObject.GetComponent<LabelArea>().OnEnter
                = () => LabelController.ShowLabel(_closeButton.transform.position,
                IO.KeyBinder.GetShortcutAsString(IO.KeyBinder.Shortcut.Skills), default);

            tabs.firstJob.OnClick = onFirstJobTabClick;
            tabs.secondJob.OnClick = onSecondJobTabClick;
            tabs.misc.OnClick = onMiscTabClick;

            _miniBodyScrollView.Init();

            _miniBodyDefaultRows = _miniBodyVisibleRows = _miniBodyScrollView.VisibleRows;
            _fullBodyDefaultColumns = _fullBodyVisibleColumns = _fullBodyScrollView.VisibleColumns;
            _fullBodyDefaultRows = _fullBodyVisibleRows = _fullBodyScrollView.VisibleRows;

            viewSkillInfoToggle.OnValueChanged = (v) => _showSkillInfo = v;
            resetButton.OnClick = onReset;
            applyButton.OnClick = onApply;

            PacketDistributer.RegisterCallback(PacketIds.RCV_PlayerSkillTreeLevelUpReply, OnSkillTreeLevelUpReply);
        }

        private void OnSkillTreeLevelUpReply(RCV_Packet rawPacket)
        {
            var packet = (RCV_PlayerSkillTreeLevelUpReply)rawPacket;

            if (!_levelUpLocked)
            {
                Debug.LogError("Ignored skill tree level up reply with result " + packet.success.ToString());
                return;
            }

            if (packet.success)
            {
                Debug.Log("Received skill tree level up reply with success result");
            }
            else
            {
                Debug.LogWarning("Received skill tree level up reply with failure result");

                //show message box of error and put panel back to unlocked state
            }

            _usedSkillPoints = 0;
            for (int i = 0; i < _miscIndex; i++)
            {
                _skillInfo[i].skill.IncrementSkillLevel(_skillInfo[i].levelUpCache);
                if (_skillInfo[i].selectedLvl == 0)
                    _skillInfo[i].selectedLvl = _skillInfo[i].skill.SkillLevel;
                _skillInfo[i].levelUpCache = 0;
            }

            _levelUpLocked = false;

            //can also reactivate the levelupfunctionality now
            applyButton.enabled = true;
            resetButton.enabled = true;

            redraw();
            _dirtyPreviousView = true;
        }

        private new void OnEnable()
        {
            base.OnEnable();

            if (_pendingLoad || (_pendingExtraSkillUpdate && selectedTabType == TabSelectedType.Misc))
            {
                if (_isMinimized)
                    loadMiniBody();
                else
                    loadFullBody();

                return;
            }
        }

        public static SkillPanelController Instantiate(UIController uiController, Transform parent)
        {
            return Instantiate<SkillPanelController>(uiController, parent, "SkillPanel");
        }

        public void AddExtraSkill(Skill skill)
        {
            //TODO missing ading our index
            var i = _skillCount++;
            var fullIndex = i - _miscIndex;

            _skillInfo[i].skill = skill;
            _skillInfo[i].treeEntry = new SkillTreeEntry(skill.Id, fullIndex);
            _skillInfo[i].selectedLvl = skill.SkillLevel;
            _skillInfo[i].levelUpCache = 0;

            if (selectedTabTypePreviousView == TabSelectedType.Misc)
                _dirtyPreviousView = true;

            if (selectedTabType == TabSelectedType.Misc)
            {
                if (gameObject.activeInHierarchy)
                {
                    _fullToMiniIndexTable[fullIndex] = i;
                    if (_isMinimized)
                        _miniBodyScrollView.AddEntry(i);
                    else
                        _fullBodyScrollView.AddEntry(i);
                }
                else
                    _pendingExtraSkillUpdate = true;
            }
        }

        public void RemoveExtraSkill(Skill skill)
        {
            int i = _miscIndex;
            bool found = false;

            for (; i < _skillCount; i++)
            {
                if (_skillInfo[i].skill.Id == skill.Id &&
                    _skillInfo[i].skill.SkillLevel == skill.SkillLevel)
                {
                    found = true;
                    break;
                }
            }

            Debug.Assert(found);
            --_skillCount;
            _skillInfo[i].skill = null;

            if (selectedTabTypePreviousView == TabSelectedType.Misc)
                _dirtyPreviousView = true;

            if (selectedTabType == TabSelectedType.Misc)
            {
                if (gameObject.activeInHierarchy)
                {
                    if (_isMinimized)
                        _miniBodyScrollView.RemoveEntry(i);
                    else
                        _fullBodyScrollView.RemoveEntry(i);
                }
                else
                    _pendingExtraSkillUpdate = true;
            }
        }

        public void Load(Skill[] skills, Jobs job)
        {
            // clear arrays
            Array.ForEach(_skillInfo, x => x.Reset());

            // just in case it was locked
            _levelUpLocked = false;
            applyButton.enabled = true;
            resetButton.enabled = true;

            //clear index so we can recalculate in recursion
            _secondJobIndex = 0;

            // load all permanent job skills
            int i = 0;
            loadJob(skills, job, ref i);
            _secondJobIndex = _secondJobIndex == 0 ? i : _secondJobIndex;
            _miscIndex = _skillCount = i;

            // load misc skills
            for (; i < skills.Length; i++)
            {
                if (skills[i] != null)
                {
                    _skillInfo[i].treeEntry = default;
                    _skillInfo[i].skill = skills[i];
                    _skillInfo[i].selectedLvl = skills[i].SkillLevel;
                    _skillInfo[i].levelUpCache = 0;

                    _skillCount++;
                }
            }

            lazyLoad();
        }

        public void SetSkillPoints(int skillPoints)
        {
            if (_levelUpLocked) // a set skillpoints wiil come afterwards anyways
                return;

            _skillPoints = skillPoints;

            if (skillPoints < _usedSkillPoints)
            {
                _usedSkillPoints = 0;

                for (int i = 0; i < _miscIndex; i++)
                    _skillInfo[i].levelUpCache = 0;
            }

            updateSkillPoints();
        }

        private void updateSkillPoints()
        {
            _skillPointsText.text = _usedSkillPoints > 0 ? $"Skill Point: <color=blue>{_usedSkillPoints}</color> / {_skillPoints}"
                                                         : $"Skill Point: {_skillPoints}";
            if (_skillPoints > _usedSkillPoints)
                return;

            redraw();
        }

        // We load recursively so we don't hardcode job dependents relations and depth
        private void loadJob(Skill[] skills, Jobs job, ref int i)
        {
            var dependentJob = JobDependents.table[(int)job];
            if (dependentJob != job)
                loadJob(skills, dependentJob, ref i);

            if (_secondJobIndex == 0 && dependentJob < Jobs.SecondJobStart && job >= Jobs.SecondJobStart)
                _secondJobIndex = i;

            foreach (var entry in SkillTreeDb.GetSkillTree(job).Skills)
            {
                _skillInfo[i].treeEntry = entry;
                _skillInfo[i].skill = skills[i];
                _skillInfo[i].selectedLvl = skills[i].SkillLevel;
                _skillInfo[i].levelUpCache = 0;

                i++;
            }
        }

        private void loadMiniSkillView(int begin, int end)
        {
            _miniBodyScrollView.Clear(begin);
            for (int i = begin; i < end; i++)
                _miniBodyScrollView.AddEntry(i);
        }

        private void loadFullSkillView(int begin, int end)
        {
            _fullBodyScrollView.Clear(begin);
            for (int i = begin; i < end; i++)
            {
                _fullToMiniIndexTable[_skillInfo[i].treeEntry.SlotIndex] = i - begin;
                _fullBodyScrollView.AddEntry(i);
            }
        }

        private void onClose()
        {
            gameObject.SetActive(false);
        }

        private void onClickMinimize()
        {
            if (_isMinimized)
            {
                resizeButton.SetActive(false);
                _miniBodyScrollView.gameObject.SetActive(false);

                (transform as RectTransform).sizeDelta = fullBodyDimentions;

                if (_dirtyPreviousView)
                {
                    _fullBodyScrollView.Clear();
                    loadFullBody();
                }

                // if player doesnt touch anything after we wont rebuild elements next tab click
                _dirtyPreviousView = false;
                selectedTabTypePreviousView = selectedTabType;
                _fullBodyScrollView.gameObject.SetActive(true);
            }
            else
            {
                resizeButton.SetActive(true);
                _fullBodyScrollView.gameObject.SetActive(false);

                (transform as RectTransform).sizeDelta = miniBodyDimentions;

                if (_dirtyPreviousView || _miniBodyScrollView.VisibleRows != _miniBodyDefaultRows)
                {
                    _miniBodyScrollView.Clear();
                    _miniBodyScrollView.VisibleRows = _miniBodyDefaultRows;
                    loadMiniBody();
                }

                // if player doesnt touch anything we wont rebuild elements next tab click
                _dirtyPreviousView = false;
                selectedTabTypePreviousView = selectedTabType;
                _miniBodyScrollView.gameObject.SetActive(true);
            }

            _isMinimized = !_isMinimized;
        }

        private void OnResize(int stepX, int stepY)
        {
            _miniBodyVisibleRows = _miniBodyDefaultRows - stepY;
            _miniBodyScrollView.VisibleRows = _miniBodyVisibleRows;
        }

        private void onApply()
        {
            if (_usedSkillPoints == 0)
                return;

            applyButton.enabled = false;
            resetButton.enabled = false;
            _levelUpLocked = true;

            int cachedLvlUpsCount = 0;
            int lastlevelUpCacheIndex = 0;

            // Calculate correct packet to send
            for (int i = 0; i < _miscIndex; i++)
            {
                if (_skillInfo[i].levelUpCache > 0)
                {
                    lastlevelUpCacheIndex = i;
                    cachedLvlUpsCount++;
                }
            }

            // Single skill level up so send it directly
            if (cachedLvlUpsCount == 1)
            {
                var packet = new SND_LevelUpSingleSkill();
                packet.SkillIndex = (byte)lastlevelUpCacheIndex;
                packet.SkillLvl = (byte)_skillInfo[lastlevelUpCacheIndex].levelUpCache;

                NetworkController.SendPacket(packet);
                return;
            }

            // More than half of max ln better to send full tree
            if (cachedLvlUpsCount > Constants.MAX_PLAYER_SKILLS / 2)
            {
                var packet = new SND_LevelUpAllTreeSkills();

                packet.SkillIncrements = new byte[_miscIndex];

                for (int i = 0; i < _miscIndex; i++)
                {
                    packet.SkillIncrements[i] = (byte)_skillInfo[i].levelUpCache;
                }
                NetworkController.SendPacket(packet);
            }
            else
            {
                // Send a multi skill level up request
                var packet = new SND_LevelUpMultipleSkills();

                packet.Skills = new SND_LevelUpMultipleSkills.SkillInfo[cachedLvlUpsCount];
                int remaining = cachedLvlUpsCount;

                for (int i = 0; remaining > 0; i++)
                {
                    if (_skillInfo[i].levelUpCache > 0)
                    {
                        packet.Skills[cachedLvlUpsCount - remaining].SkillIndex = (byte)i;
                        packet.Skills[cachedLvlUpsCount - remaining].SkillLvl = (byte)_skillInfo[i].levelUpCache;
                        remaining--;
                    }
                }
                NetworkController.SendPacket(packet);
            }
        }

        private void onReset()
        {
            if (_levelUpLocked || _usedSkillPoints == 0)
                return;

            _usedSkillPoints = 0;

            for (int i = 0; i < _miscIndex; i++)
            {
                _skillInfo[i].levelUpCache = 0;
            }

            updateSkillPoints();
            redraw();
        }

        private void onFirstJobTabClick()
        {
            if (selectedTabType == TabSelectedType.FirstJob)
                return;

            selectedTabType = TabSelectedType.FirstJob;
            if (_isMinimized)
                loadMiniSkillView(0, _secondJobIndex);
            else
                loadFullSkillView(0, _secondJobIndex);
        }

        private void onSecondJobTabClick()
        {
            if (selectedTabType == TabSelectedType.SecondJob)
                return;

            selectedTabType = TabSelectedType.SecondJob;
            if (_isMinimized)
                loadMiniSkillView(_secondJobIndex, _miscIndex);
            else
                loadFullSkillView(_secondJobIndex, _miscIndex);
        }

        private void onMiscTabClick()
        {
            if (selectedTabType == TabSelectedType.Misc)
                return;

            selectedTabType = TabSelectedType.Misc;
            if (_isMinimized)
                loadMiniSkillView(_miscIndex, _skillCount);
            else
                loadFullSkillView(_miscIndex, _skillCount);
        }

        public void OnScroll(PointerEventData data)
        {
            if (_isMinimized)
            {
                if (_miniBodyScrollView.gameObject.activeSelf)
                    _miniBodyScrollView.OnScroll(data);
            }
            else
            {
                if (_fullBodyScrollView.gameObject.activeSelf)
                    _fullBodyScrollView.OnScroll(data);
            }
        }

        private void lazyLoad()
        {
            if (gameObject.activeInHierarchy)
            {
                if (_isMinimized)
                    loadMiniBody();
                else
                    loadFullBody();
            }
            else
                _pendingLoad = true;
        }

        private void redraw()
        {
            if (gameObject.activeInHierarchy)
            {
                if (_isMinimized)
                    _miniBodyScrollView.Redraw();
                else
                    _fullBodyScrollView.Redraw();
            }
        }

        private void loadMiniBody()
        {
            switch (selectedTabType)
            {
                case TabSelectedType.FirstJob:
                    loadMiniSkillView(0, _secondJobIndex);
                    break;
                case TabSelectedType.SecondJob:
                    loadMiniSkillView(_secondJobIndex, _miscIndex);
                    break;
                case TabSelectedType.Misc:
                    loadMiniSkillView(_miscIndex, _skillCount);
                    break;
            }

            _pendingLoad = false;
            _pendingExtraSkillUpdate = false;

            if (selectedTabType == selectedTabTypePreviousView)
                _dirtyPreviousView = true;
        }

        private void loadFullBody()
        {
            switch (selectedTabType)
            {
                case TabSelectedType.FirstJob:
                    loadFullSkillView(0, _secondJobIndex);
                    break;
                case TabSelectedType.SecondJob:
                    loadFullSkillView(_secondJobIndex, _miscIndex);
                    break;
                case TabSelectedType.Misc:
                    loadFullSkillView(_miscIndex, _skillCount);
                    break;
            }

            _pendingLoad = false;
            _pendingExtraSkillUpdate = false;

            if (selectedTabType == selectedTabTypePreviousView)
                _dirtyPreviousView = true;
        }

        private void toggleFirstJobTabColor(bool highlighted)
        {
            tabs.firstJobBackground.color = highlighted ? new Color(1, 231 / 255f, 231 / 255f, 1) : Color.white;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            BringToFront();
        }
    }
}