using System;
using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    public class TabButton : MonoBehaviour
        , IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
        , ICanvasRaycastFilter
    {
        public Action OnClick;

        [SerializeField]
        private Image tab = default;
        [SerializeField]
        private Sprite sprite = default;

        private UIController.Panel _panel;

        private void Awake()
        {
            _panel = GetComponentInParent<UIController.Panel>();
        }

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            const CanvasFilter filter = ~(CanvasFilter.NpcDialog
                | CanvasFilter.ModalMsgDialog | CanvasFilter.DisconnectDialog);

            return (filter & UIController.Panel.CanvasFilter) == 0 && enabled;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            tab.sprite = sprite;
            OnClick?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            _panel.BringToFront();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // prevent it from bubbling up
        }
    }
}