using System;
using UnityEngine;

namespace RO.UI
{
    public sealed class InventoryPanelSlot : MonoBehaviour, IPointerClickHandler
    , IPointerEnterHandler, IPointerExitHandler
    , IPointerDownHandler, IPointerUpHandler
    , IBeginDragHandler, IEndDragHandler
    {
        public Action<PointerEventData> onBeginDragAction;
        public Action<PointerEventData> onEndDragAction;
        public Action<PointerEventData> onPointerClickAction;
        public Action<PointerEventData> onPointerEnterAction;
        public Action<PointerEventData> onPointerExitAction;

        private UIController.Panel _panel;

        private void Awake()
        {
            _panel = GetComponentInParent<UIController.Panel>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            onBeginDragAction(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            onEndDragAction(eventData);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onPointerClickAction(eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            onPointerEnterAction(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            onPointerExitAction(eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _panel.BringToFront();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // block bubble up
        }
    }
}