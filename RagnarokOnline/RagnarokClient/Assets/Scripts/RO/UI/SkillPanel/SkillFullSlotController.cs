using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    public partial class SkillPanelController : UIController.Panel
    {
        public class FullSlotController : MonoBehaviour, IBeginDragHandler, IPointerExitHandler, IEndDragHandler,
            IDragHandler, IPointerClickHandler, IPointerEnterHandler, MultiColumnListScrollView.ISlot
        {
            [SerializeField] private Image _icon = default;
            [SerializeField] private Text _skillName = default;
            [SerializeField] private Text _requiredLvl = default;
            [SerializeField] private Text _lvl = default;
            [SerializeField] private SimpleCursorButton _rightArrow = default;
            [SerializeField] private SimpleCursorButton _leftArrow = default;

            private Image _background;
            private SkillPanelController _panel;
            private SkillInfo _skillInfo;

            public RectTransform rectTransform { get { return (RectTransform)transform; } }

            private void Awake()
            {
                _background = GetComponent<Image>();
                _panel = GetComponentInParent<SkillPanelController>();
                gameObject.SetActive(false);
            }

            private void OnDisable()
            {

            }

            public void fill(int skillIndex)
            {
                // translate index first
                skillIndex = _panel._fullToMiniIndexTable[skillIndex];

                if (skillIndex < 0)
                {
                    _background.sprite = _panel._fullSlotSprites.noSkill;
                    setActive(false);
                    _background.raycastTarget = false;
                    return;
                }
                else
                {
                    _panel._treeIndexToFullSlot[skillIndex] = this;
                    _skillInfo = _panel._skillInfo[skillIndex];

                    setActive(true);
                    _background.raycastTarget = true;
                }

                _skillInfo = _panel._skillInfo[_panel._fullToMiniIndexTable[skillIndex]];
            }

            private void setActive(bool value)
            {
                _icon.gameObject.SetActive(value);
                _skillName.gameObject.SetActive(value);
                _requiredLvl.gameObject.SetActive(value);
                _lvl.gameObject.SetActive(value);
                _rightArrow.gameObject.SetActive(value);
                _leftArrow.gameObject.SetActive(value);
            }

            public void OnBeginDrag(PointerEventData eventData)
            {
                throw new System.NotImplementedException();
            }

            public void OnDrag(PointerEventData eventData)
            {
                throw new System.NotImplementedException();
            }

            public void OnEndDrag(PointerEventData eventData)
            {
                throw new System.NotImplementedException();
            }

            public void OnPointerClick(PointerEventData eventData)
            {/*
                if (eventData.clickCount == 2)
                {
                    _controller._skills[SkillIndex].OnSkillSelect(_controller._skillSelectedLevel[SkillIndex]);
                }
                else if (eventData.clickCount == 1)
                {
                    if (_controller._skillPoints > 0)
                    {
                        _controller._skillPoints--;
                        int lvl = ++_controller._skillLevelUpCache[SkillIndex];
                        lvl += _controller._skills[SkillIndex].SkillLevel;

                        //TODO:: update skillpoint text
                        _controller.fullSlots[SlotIndex].maxLvl.text = lvl.ToString();
                        _controller.fullSlots[SlotIndex].fixedLvl.text = lvl.ToString();

                        _controller._skillLevelUpCache[SkillIndex]++;
                    }
                }*/
            }

            public void OnPointerEnter(PointerEventData eventData)
            {
                throw new System.NotImplementedException();
            }

            public void OnPointerExit(PointerEventData eventData)
            {
                // _background.color = Color.black;
                // _controller.tabs.firstJobBackground.color = Color.white;
            }
            /*
            public void OnClickLeftArrow(PointerEventData eventData)
            {
                var skill = _controller._skills[SkillIndex];
                int selected = _controller._skillSelectedLevel[SkillIndex];

                if (selected > 1)
                {
                    selected--;
                    _selectableLevel.text = "Lv : " + selected + "/ " + skill.SkillLevel;
                    _controller._skillSelectedLevel[SkillIndex] = selected;
                }
            }

            public void OnClickRightArrow(PointerEventData eventData)
            {
                var skill = _controller._skills[SkillIndex];
                int selected = _controller._skillSelectedLevel[SkillIndex];

                if (selected < skill.SkillLevel)
                {
                    selected++;
                    _selectableLevel.text = "Lv : " + selected + "/ " + skill.SkillLevel;
                    _controller._skillSelectedLevel[SkillIndex] = selected;
                }
            }*/
        }
    }

    public sealed class SkillFullSlotController : SkillPanelController.FullSlotController { }
}