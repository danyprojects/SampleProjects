using RO.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    public sealed partial class BattleModePanelController : UIController.Panel
    {
        public class SlotController : MonoBehaviour, IPointerClickHandler
        , IPointerEnterHandler, IPointerExitHandler, IDropHandler
        , IBeginDragHandler, IDragHandler, IEndDragHandler
        , IPointerDownHandler, IPointerUpHandler
        {
            [SerializeField]
            private Text _count = default;
            private BattleModePanelController _bmPanel;

            // may be quickslot panel
            private UIController.Panel _parentPanel;

            private Image _icon = null;
            private SlotInfo _slotInfo = null;
            private bool _isDragging = false;
            private string _cachedHotkeyAsString;
            private KeyBinder.Hotkey hotkey = KeyBinder.Hotkey.Last;
            private StringBuilder _stringBuilder = new StringBuilder();


            private void Awake()
            {
                _icon = GetComponent<Image>();
                _bmPanel = GetComponentInParent<BattleModePanelController>();
                _parentPanel = GetComponentInParent<UIController.Panel>();

                GetComponent<LabelArea>().OnEnter = ShowLabel;

                for (int i = 0; i < _bmPanel._slots.Length; i++)
                {
                    if (ReferenceEquals(this, _bmPanel._slots[i]))
                    {
                        _slotInfo = _bmPanel._slotInfo[i];
                        if (i < (int)KeyBinder.Hotkey.Last)
                        {
                            hotkey = (KeyBinder.Hotkey)i;
                            _slotInfo._description = _cachedHotkeyAsString = KeyBinder.GetHotkeyAsString(hotkey);
                        }
                        break;
                    }
                }
            }

            public void ShowLabel()
            {
                if (_slotInfo._description != null)
                {
                    // key might have changed meanwhile so check it
                    if (!ReferenceEquals(_cachedHotkeyAsString, KeyBinder.GetHotkeyAsString(hotkey)))
                    {
                        switch (_slotInfo._iconType)
                        {
                            case IconType.Skill:
                                UpdateSkillDescription();
                                break;
                            case IconType.Item:
                                UpdateItemDescription();
                                break;
                            case IconType.None:
                                _slotInfo._description = _cachedHotkeyAsString = KeyBinder.GetHotkeyAsString(hotkey);
                                break;
                        }
                    }

                    _bmPanel.LabelController.ShowLabel(transform.position, _slotInfo._description, new Vector2(-1, 2));
                }
            }

            public void OnBeginDrag(PointerEventData eventData)
            {
                if (_slotInfo._isEmpty)
                    return;

                _isDragging = true;

                switch (_slotInfo._iconType)
                {
                    case IconType.Skill:
                        _bmPanel.DragIconController.OnBeginDrag(eventData,
                        IconType.Skill, _slotInfo._id, _icon.sprite, _slotInfo._context, _slotInfo._count, OnDropped);
                        break;
                    case IconType.Item:
                        break;
                }
            }

            public void OnDrag(PointerEventData eventData)
            {
                if (!_isDragging)
                    return;

                _bmPanel.DragIconController.OnDrag(eventData);
            }

            public void OnEndDrag(PointerEventData eventData)
            {
                if (!_isDragging)
                    return;
                _isDragging = false;

                _bmPanel.DragIconController.OnEndDrag(eventData);
            }

            public void OnDrop(PointerEventData eventData)
            {
                var dragIcon = _bmPanel.DragIconController;

                if (dragIcon.IconType == IconType.None)
                    return;

                _slotInfo._isEmpty = false;
                _slotInfo._count = dragIcon.Count;
                _slotInfo._iconType = dragIcon.IconType;
                _slotInfo._id = dragIcon.Id;
                _icon.sprite = dragIcon.Sprite;
                _slotInfo._context = _bmPanel.DragIconController.OnDrop<object>(eventData);

                _count.text = _slotInfo._count.ToString();
                _count.gameObject.SetActive(true);

                switch (_slotInfo._iconType)
                {
                    case IconType.Skill:
                        UpdateSkillDescription();
                        KeyBinder.RegisterAction(hotkey, RunSkillAction);
                        break;
                    case IconType.Item:
                        UpdateItemDescription();
                        KeyBinder.RegisterAction(hotkey, RunItemAction);
                        break;
                }
            }

            private void OnDropped()
            {
                _slotInfo.Clear();
                _icon.sprite = _bmPanel._emptySlotSprite;
                _count.gameObject.SetActive(false);

                if (hotkey != KeyBinder.Hotkey.Last)
                    KeyBinder.RegisterAction(hotkey, null);
            }

            private void RunSkillAction(bool quickCast)
            {
                ((LocalPlayer.SkillTree.Skill)_slotInfo._context).OnSkillSelect(_slotInfo._count, quickCast);
            }

            private void RunItemAction(bool quickCast)
            {

            }

            private void UpdateSkillDescription()
            {
                var skillDb = ((LocalPlayer.SkillTree.Skill)_slotInfo._context).Db;
                var count = _bmPanel.DragIconController.Count;

                _stringBuilder.Clear();
                _stringBuilder.Append(_cachedHotkeyAsString = KeyBinder.GetHotkeyAsString(hotkey))
                    .Append(' ')
                    .Append(skillDb.Name)
                    .Append(" Lv ")
                    .Append(count - 1)
                    .Append(" (Sp : ")
                    .Append(skillDb.Sp[count - 1].Value)
                    .Append(")");

                _slotInfo._description = _stringBuilder.ToString();
            }

            private void UpdateItemDescription()
            {

            }

            public void OnPointerClick(PointerEventData eventData)
            {
                if (_slotInfo._isEmpty)
                    return;

                switch (eventData.button)
                {
                    case PointerEventData.InputButton.Right:
                        //open details box
                        break;
                    case PointerEventData.InputButton.Left:
                        switch (_slotInfo._iconType) //there should be a callback associated, invoke that
                        {
                            case IconType.Skill:
                                RunSkillAction(false); //if it's from user click on bm then never use quickcast
                                break;
                            case IconType.Item:
                                RunItemAction(false);
                                break;
                        }
                        break;
                }
            }

            public void OnPointerEnter(PointerEventData eventData)
            {
                _icon.color = _bmPanel._hightlightColor;
            }

            public void OnPointerExit(PointerEventData eventData)
            {
                _icon.color = Color.white;
            }

            public void OnPointerDown(PointerEventData eventData)
            {
                _parentPanel.BringToFront();
            }

            public void OnPointerUp(PointerEventData eventData)
            {
                // prevent bubble up
            }
        }
    }

    public class BattleModeSlotController : BattleModePanelController.SlotController { }
}