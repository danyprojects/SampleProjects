using System;
using UnityEngine;

namespace RO.UI
{
    public class ResizePanel : MonoBehaviour, ICanvasRaycastFilter
        , IBeginDragHandler, IEndDragHandler, IDragHandler
        , IPointerEnterHandler, IPointerExitHandler
        , IPointerDownHandler, IPointerUpHandler
    {
        public Action<int/*x*/, int /*y*/> OnResize; // current resize steps 

        [SerializeField]
        private Vector2Int _minSize = default;
        public Vector2Int MinSize => _minSize;

        [SerializeField]
        private Vector2Int _maxSize = default;
        public Vector2Int MaxSize => _maxSize;

        [SerializeField]
        private Vector2Int _step = default;
        public Vector2Int Step => _step;

        [SerializeField]
        private RectTransform _rectTransform = default;

        private bool _pressed = false;
        private Vector2 _currentPointerPosition;
        private Vector2 _initialPosition;
        private Vector2 _initialSize;
        private Vector2 _defaultSize;

        void Awake()
        {
            if (_rectTransform == null)
                _rectTransform = transform.parent.GetComponent<RectTransform>();
        }

        void Start()
        {
            _defaultSize = _rectTransform.sizeDelta;
        }

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            const CanvasFilter filter = ~(CanvasFilter.NpcDialog);

            return (filter & UIController.Panel.CanvasFilter) == 0 && enabled;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // we can reuse this flag for resize since its basicly like a drag panel
            UIController.Panel.SetCanvasFilterFlags(CanvasFilter.DragPanel);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // we can reuse this flag for resize since its basicly like a drag panel
            UIController.Panel.ClearCanvasFilterFlags(CanvasFilter.DragPanel);
        }

        public void OnDrag(PointerEventData data)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, data.position, null, out _currentPointerPosition);
            Vector2 resizeValue = _currentPointerPosition - _initialPosition;

            var x = _step.x != 0 ? (int)resizeValue.x / _step.x * Math.Abs(_step.x) : 0;
            var y = _step.y != 0 ? (int)resizeValue.y / _step.y * Math.Abs(_step.y) : 0;

            resizeValue.x = Mathf.Clamp(_initialSize.x + x, _minSize.x, _maxSize.x);
            resizeValue.y = Mathf.Clamp(_initialSize.y - y, _minSize.y, _maxSize.y);

            _rectTransform.sizeDelta = resizeValue;
            resizeValue = _defaultSize - resizeValue;

            x = _step.x != 0 ? (int)resizeValue.x / _step.x : 0;
            y = _step.y != 0 ? (int)resizeValue.y / _step.y : 0;

            OnResize?.Invoke(Math.Abs(x), Math.Abs(y));
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            RO.Media.CursorAnimator.SetAnimation(RO.Media.CursorAnimator.Animations.Click);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_pressed)
                RO.Media.CursorAnimator.UnsetAnimation(RO.Media.CursorAnimator.Animations.Click);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                _pressed = true;
                RO.Media.CursorAnimator.CursorMode = RO.Media.CursorAnimator.CursorModes.Software;

                _initialSize = _rectTransform.sizeDelta;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, eventData.position, null, out _initialPosition);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                _pressed = false;
                RO.Media.CursorAnimator.CursorMode = RO.Media.CursorAnimator.CursorModes.Hardware;
                RO.Media.CursorAnimator.UnsetAnimation(RO.Media.CursorAnimator.Animations.Click);
            }
        }
    }
}