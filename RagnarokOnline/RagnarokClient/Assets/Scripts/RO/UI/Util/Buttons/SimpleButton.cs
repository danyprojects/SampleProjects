using System;
using UnityEngine;

namespace RO.UI
{
    public class SimpleButton : MonoBehaviour
        , IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
        , ICanvasRaycastFilter
    {
        public Action OnClick;

        private UIController.Panel _panel;

        private void Awake()
        {
            _panel = GetComponentInParent<UIController.Panel>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _panel.BringToFront();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // prevent it from bubbling up
        }

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            return enabled;
        }
    }
}