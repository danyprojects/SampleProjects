using System;
using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    class TintButton : MonoBehaviour, IPointerClickHandler
        , IPointerDownHandler, IPointerUpHandler
        , IPointerExitHandler, IPointerEnterHandler
        , ICanvasRaycastFilter
    {
        [SerializeField] private Color HighligthedColor = Color.white;
        [SerializeField] private Color PressedColor = Color.white;
        [SerializeField] private Color DisabledColor = Color.white;

        public Action OnClick = default;

        protected Image _image;
        private UIController.Panel _panel;
        private Color _default;
        private bool _isHightligthed = false;
        private bool _isPressed = false;

        void Awake()
        {
            _image = GetComponent<Image>();
            _default = _image.color;
            _panel = GetComponentInParent<UIController.Panel>();

            if (!enabled)
                _image.color = DisabledColor;
        }

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            const CanvasFilter filter = ~(CanvasFilter.NpcDialog
                | CanvasFilter.ModalMsgDialog | CanvasFilter.DisconnectDialog);

            return (filter & UIController.Panel.CanvasFilter) == 0 && enabled;
        }

        private void OnEnable()
        {
            _image.color = _default;
        }

        private void OnDisable()
        {
            _image.color = DisabledColor;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            OnClick?.Invoke();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            _panel.BringToFront();

            _isPressed = true;
            _image.color = PressedColor;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
            _image.color = _isHightligthed ? HighligthedColor : _default;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            RO.Media.CursorAnimator.SetAnimation(RO.Media.CursorAnimator.Animations.Click);

            _isHightligthed = true;
            if (!_isPressed)
                _image.color = HighligthedColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            RO.Media.CursorAnimator.UnsetAnimation(RO.Media.CursorAnimator.Animations.Click);

            _isHightligthed = false;
            if (!_isPressed)
                _image.color = _default;
        }
    }
}
