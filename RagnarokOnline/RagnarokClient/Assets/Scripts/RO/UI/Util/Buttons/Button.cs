using System;
using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    class Button : MonoBehaviour, IPointerClickHandler
        , IPointerDownHandler, IPointerUpHandler
        , IPointerExitHandler, IPointerEnterHandler
        , ICanvasRaycastFilter
    {
        [SerializeField] private Sprite HighligthedSprite = default;
        [SerializeField] private Sprite PressedSprite = default;
        [SerializeField] private Sprite DisabledSprite = default;

        public Action OnClick;

        protected Image _image;
        [SerializeField]
        private UIController.Panel _panel = default;
        private Sprite _default;
        private bool _isHightligthed = false;
        private bool _isPressed = false;

        void Awake()
        {
            _image = GetComponent<Image>();
            _default = _image.sprite;
            if (_panel == null)
                _panel = GetComponentInParent<UIController.Panel>();

            if (!enabled && DisabledSprite != null)
                _image.sprite = DisabledSprite;
        }

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            const CanvasFilter filter = ~(CanvasFilter.NpcDialog
                | CanvasFilter.ModalMsgDialog | CanvasFilter.DisconnectDialog);

            return (filter & UIController.Panel.CanvasFilter) == 0 && enabled;
        }

        private void OnEnable()
        {
            _image.sprite = _default;
        }

        private void OnDisable()
        {
            _image.sprite = DisabledSprite != null ? DisabledSprite : _default;
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
            if (PressedSprite != null)
                _image.sprite = PressedSprite;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
            _image.sprite = _isHightligthed && HighligthedSprite != null
                ? HighligthedSprite : _default;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            RO.Media.CursorAnimator.SetAnimation(RO.Media.CursorAnimator.Animations.Click);

            _isHightligthed = true;
            if (!_isPressed)
                _image.sprite = HighligthedSprite != null ? HighligthedSprite : _default;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            RO.Media.CursorAnimator.UnsetAnimation(RO.Media.CursorAnimator.Animations.Click);

            _isHightligthed = false;
            if (!_isPressed)
                _image.sprite = _default;
        }
    }
}
