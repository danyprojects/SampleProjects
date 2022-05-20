using System;
using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    public sealed class InventoryPanelController : UIController.Panel
        , IPointerDownHandler
    {
#pragma warning disable 0649
        [Serializable]
        private class Slot //: MultiColumnListScrollView.ISlot
        {
            public Image icon;
            public Text count;
            public Text countOutline;
        }

        [Serializable]
        private class Tabs
        {
            public TabButton item;
            public TabButton equip;
            public TabButton etc;
        }
#pragma warning restore 0649

        [SerializeField]
        private Tabs tabs = default;
        [SerializeField]
        private Button minimize = default;
        [SerializeField]
        private RectTransform body = default;
        [SerializeField]
        private MultiColumnListScrollView listView = default;
        [SerializeField]
        private Slot[] slots = new Slot[49];

        private int _defaultVisibleColumns;
        private int _defaultVisibleRows;
        private int _visibleColumns;
        private int _visibleRows;
        private bool _isMinimized = false;

        //TODO add actual array for Item instances here, can also use 3 individual arrays,
        //or split array by the different tabs

        private void Awake()
        {
            //IO.KeyBinder.RegisterAction(IO.KeyBinder.Shortcut.Inventory, 
            //  () => gameObject.SetActive(!gameObject.activeSelf));

            /* _closeButton.OnClick = onPressClose;
             _closeButton.gameObject.GetComponent<LabelArea>().OnEnter
                 = () => LabelController.ShowLabel(_closeButton.transform.position,
                 IO.KeyBinder.GetShortcutAsString(IO.KeyBinder.Shortcut.Inventory), default);*/

            GetComponentInChildren<ResizePanel>().OnResize = OnResize;
            minimize.OnClick = OnClickMinimize;
            // listView.Init(slots, fillEntry);

            _visibleRows = _defaultVisibleRows = listView.VisibleRows;
            _visibleColumns = _defaultVisibleColumns = listView.VisibleColumns;

            for (int i = 0; i < slots.Length; ++i)
                subscribeSlotEvents(i);

            tabs.item.OnClick = OnItemTabClick;
            tabs.equip.OnClick = OnEquipTabClick;
            tabs.etc.OnClick = OnEtcTabClick;
        }

        private void OnResize(int stepX, int stepY)
        {
            _visibleRows = _defaultVisibleRows - stepY;
            _visibleColumns = _defaultVisibleColumns + stepX;

            listView.SetDimentions(_visibleRows, _visibleColumns);
        }

        private void OnClickMinimize()
        {
            body.gameObject.SetActive(_isMinimized);
            _isMinimized = !_isMinimized;
        }

        //Tabs
        private void OnItemTabClick()
        {

        }

        private void OnEquipTabClick()
        {

        }

        private void OnEtcTabClick()
        {

        }

        //Slot events
        private void slotOnBeginDrag(PointerEventData eventData, int index)
        {
            //create the copy to be draged
            throw new System.NotImplementedException();
        }

        private void slotOnEndDrag(PointerEventData eventData, int index)
        {
            //drop copy to respective panel
            throw new System.NotImplementedException();
        }

        private void slotOnPointerClick(PointerEventData eventData, int index)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Right:
                    {
                        //open details box
                        break;
                    }
                case PointerEventData.InputButton.Left:
                    {
                        if (eventData.button == PointerEventData.InputButton.Right)
                        {
                            //use skill/item
                        }
                        break;
                    }
                default:
                    break;
            }
        }

        private void slotOnPointerEnter(PointerEventData eventData, int index)
        {
            //TODO:: show label
        }

        private void slotOnPointerExit(PointerEventData eventData, int index)
        {
            //TODO:: hide label
        }

        private void subscribeSlotEvents(int index)
        {
            /*var slot = slots[index].transform.GetComponent<InventoryPanelSlot>();

            slot.onBeginDragAction = eventData => slotOnBeginDrag(eventData, index);
            slot.onEndDragAction = eventData => slotOnEndDrag(eventData, index);
            slot.onPointerClickAction = eventData => slotOnPointerClick(eventData, index);
            slot.onPointerEnterAction = eventData => slotOnPointerEnter(eventData, index);
            slot.onPointerExitAction = eventData => slotOnPointerExit(eventData, index);*/
        }

        private void fillEntry(int slotIndex, int entryIndex)
        {
            slots[slotIndex].count.text = entryIndex.ToString();
            slots[slotIndex].countOutline.text = entryIndex.ToString();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            BringToFront();
        }
    }
}