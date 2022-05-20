using RO.Databases;
using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    using SkillTreeEntry = SkillTreeDb.SkillEntry;

    public partial class SkillPanelController : UIController.Panel
    {
        public class MiniSlotController : MonoBehaviour, IBeginDragHandler, IPointerExitHandler,
            IEndDragHandler, IDragHandler, IPointerClickHandler, IPointerEnterHandler, ListScrollView.ISlot
        {
            [SerializeField] private Image _icon = default;
            [SerializeField] private Button _levelUp = default;
            [SerializeField] private Text _skillName = default;
            [SerializeField] private Text _sp = default;
            [SerializeField] private Text _lvl = default;
            [SerializeField] private SimpleCursorButton _leftArrow = default;
            [SerializeField] private SimpleCursorButton _rightArrow = default;

            private Image _background;
            private SkillPanelController _panel;
            private SkillInfo _skillInfo;

            private static readonly Color _hightlightedColor = new Color(206 / 255f, 0, 0, 1);
            private static readonly Color _disabledColorText = new Color(123 / 255f, 123 / 255f, 123 / 255f, 1);

            public RectTransform RectTransform { get { return (RectTransform)transform; } }

            // To get around initialization order issues we don't use awake
            public void Init()
            {
                _background = GetComponent<Image>();
                _panel = GetComponentInParent<SkillPanelController>();

                _leftArrow.OnClick = OnClickLeftArrow;
                _rightArrow.OnClick = OnClickRightArrow;
                _levelUp.OnClick = OnClickLevelUp;
            }

            private void updateSkill()
            {
                var skill = _skillInfo.skill;
                int visibleMaxLvl = skill.SkillLevel + _skillInfo.levelUpCache;

                bool isMaxLevel = visibleMaxLvl >= skill.MaxLevel;
                bool isSelectable = skill.IsSelectableLvl;
                bool isSkillEnabled = visibleMaxLvl > 0;
                bool isPassive = skill.IsPassive;

                // update icon color
                _icon.color = isSkillEnabled ? GrayscaleShader.Original(_icon.color.a)
                                             : GrayscaleShader.Grey(_icon.color.a);
                // update text color
                _skillName.color = isSkillEnabled ? Color.black : _disabledColorText;

                // update level up
                _levelUp.gameObject.SetActive(_panel._skillPoints - _panel._usedSkillPoints > 0
                    && !skill.IsQuestSkill && !isMaxLevel && hasAllRequirements(_skillInfo.treeEntry)
                    && !_panel._levelUpLocked);

                // update arrows
                _leftArrow.gameObject.SetActive(isSelectable && isSkillEnabled);
                _rightArrow.gameObject.SetActive(isSelectable && isSkillEnabled);

                var visibleSelectedLvl = _skillInfo.selectedLvl > 0 ? _skillInfo.selectedLvl : visibleMaxLvl;
                var maxLvlColor = _skillInfo.levelUpCache > 0 ? "blue" : "black";

                // update sp
                _sp.text = !isSkillEnabled ? "" : isPassive ? "Passive" : $"Sp : {skill.Db.Sp[visibleSelectedLvl - 1].Value}";

                if (isSelectable)
                {
                    //is selectes is zero the in case we are showing it it must have a cached lvlup
                    if (_skillInfo.selectedLvl > 0 && _skillInfo.selectedLvl < visibleMaxLvl)
                    {
                        _lvl.text = $"Lv :  <color=red>{_skillInfo.selectedLvl}</color>{slash(_skillInfo.selectedLvl, visibleMaxLvl)}<color={maxLvlColor}>{visibleMaxLvl}</color>";
                    }
                    else
                        _lvl.text = $"Lv :  <color=black>{visibleMaxLvl}</color>{slash(visibleMaxLvl, visibleMaxLvl)}<color={maxLvlColor}>{visibleMaxLvl}</color>";
                }
                else
                {
                    _lvl.text = $"Lv :  <color={maxLvlColor}>{visibleMaxLvl}</color>";
                }

                _lvl.gameObject.SetActive(isSkillEnabled);
            }

            public void Fill(int skillIndex)
            {
                _skillInfo = _panel._skillInfo[skillIndex];
                var skill = _skillInfo.skill;

                _skillName.text = skill.Db.Name;
                _icon.sprite = _panel._skillSprites[(int)skill.Id];

                updateSkill();
            }

            public void OnBeginDrag(PointerEventData eventData)
            {
                if (_skillInfo.skill.IsPassive || _skillInfo.skill.SkillLevel == 0)
                    return;

                _panel.DragIconController.OnBeginDrag(eventData, IconType.Skill, (short)_skillInfo.skill.Id,
                    _icon.sprite, _skillInfo.skill, _skillInfo.selectedLvl);
            }

            public void OnDrag(PointerEventData eventData)
            {
                _panel.DragIconController.OnDrag(eventData);
            }

            public void OnEndDrag(PointerEventData eventData)
            {
                _panel.DragIconController.OnEndDrag(eventData);
            }

            public void OnPointerClick(PointerEventData eventData)
            {
                if (eventData.clickCount == 2 && _skillInfo.selectedLvl <= _skillInfo.skill.SkillLevel)
                    _skillInfo.skill.OnSkillSelect(_skillInfo.selectedLvl, false); //if user is manually double clicking then never use quick cast
            }

            public void DetailsBoxOnPointerEnter(PointerEventData eventData)
            {
                // TODO show skill details
                OnPointerEnter(eventData);
            }

            public void DetailsBoxOnPointerExit(PointerEventData eventData)
            {
                // TODO hide skill details
                OnPointerExit(eventData);
            }

            public void OnPointerEnter(PointerEventData eventData)
            {
                var skill = _skillInfo.skill;

                _background.color = skill.SkillLevel + _skillInfo.levelUpCache == 0
                                        ? new Color(181 / 255f, 181 / 255f, 181 / 255f, 1)
                                        : skill.IsPassive ? new Color(115 / 255f, 214 / 255f, 239 / 255f, 1)
                                                          : new Color(115 / 255f, 156 / 255f, 239 / 255f, 1);

                _panel.toggleFirstJobTabColor(_panel.selectedTabType == TabSelectedType.SecondJob &&
                                                hasFirstJobRequirement(_skillInfo.treeEntry));
            }

            public void OnPointerExit(PointerEventData eventData)
            {
                _background.color = Color.white;
                _panel.toggleFirstJobTabColor(false);
            }

            public void OnClickLevelUp()
            {
                if (_panel._levelUpLocked)
                    return;

                _panel._usedSkillPoints++;
                _skillInfo.levelUpCache++;

                // we kinda don't need to update everything but will do for now
                updateSkill();
                _panel.updateSkillPoints();
            }

            public void OnClickLeftArrow()
            {
                if (_skillInfo.selectedLvl > 1 && _skillInfo.levelUpCache == 0)
                {
                    _skillInfo.selectedLvl--;
                    updateSelectedLvl();
                }
            }

            public void OnClickRightArrow()
            {
                if (_skillInfo.selectedLvl < _skillInfo.skill.SkillLevel && _skillInfo.levelUpCache == 0)
                {
                    _skillInfo.selectedLvl++;
                    updateSelectedLvl();
                }
            }

            private string slash(int selected, int max)
            {
                return selected == 10 && max == 10 ? " / " : max == 10 ? "  / " : "  /  ";
            }

            private void updateSelectedLvl()
            {
                _sp.text = $"Sp : {_skillInfo.skill.Db.Sp[_skillInfo.selectedLvl - 1].Value}";
                var color = _skillInfo.selectedLvl < _skillInfo.skill.SkillLevel ? "red" : "black";

                var sel = _skillInfo.selectedLvl;
                var max = _skillInfo.skill.SkillLevel;

                _lvl.text = $"Lv :  <color={color}>{sel}</color>{slash(sel, max)}<color=black>{max}</color>";
            }

            private bool hasAllRequirements(SkillTreeEntry entry)
            {
                foreach (var req in entry.Requirements)
                {
                    var skillInfo = _panel._skillInfo[req.DependencyIndex];

                    if (skillInfo.skill.SkillLevel + skillInfo.levelUpCache < req.Lvl)
                        return false;
                }
                return true;
            }

            private bool hasFirstJobRequirement(SkillTreeEntry entry)
            {
                foreach (var req in entry.Requirements)
                {
                    var skillInfo = _panel._skillInfo[req.DependencyIndex];

                    //ignore skills we already have
                    if (skillInfo.skill.SkillLevel + skillInfo.levelUpCache >= req.Lvl)
                        continue;

                    // is it first job skill and we dont have its required level ?
                    if (req.DependencyIndex < _panel._secondJobIndex)
                        return true;

                    // recurse down skill dependency
                    if (hasFirstJobRequirement(_panel._skillInfo[req.DependencyIndex].treeEntry))
                        return true;

                }
                return false;
            }
        }
    }

    public sealed class SkillMiniSlotController : SkillPanelController.MiniSlotController { }
}