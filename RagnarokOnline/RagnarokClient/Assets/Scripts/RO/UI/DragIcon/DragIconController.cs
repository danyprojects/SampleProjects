using System;
using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    public sealed class DragIconController : MonoBehaviour
    {
        private void Awake()
        {
            _image = GetComponent<Image>();
            Canvas canvas = GetComponentInParent<Canvas>();
            _canvasRectTransform = canvas.rootCanvas.transform as RectTransform;
            _image.gameObject.SetActive(false);

            IconType = IconType.None;
            _context = null;
        }

        public void OnBeginDrag<T>(PointerEventData eventData, IconType type, short id, Sprite sprite, T context, int count = 1, Action onDroppedCb = null)
        {
            Debug.Assert(type != IconType.None);

            IconType = type;
            Id = id;
            Count = count;
            _context = context;
            _image.sprite = sprite;
            _onIconDropped = onDroppedCb;

            UIController.Panel.SetCanvasFilterFlags(IconType == IconType.Item ?
                CanvasFilter.ItemDrag : CanvasFilter.SkillDrag);

            RO.Media.CursorAnimator.CursorMode = RO.Media.CursorAnimator.CursorModes.Software;
            _image.transform.SetAsLastSibling();
            _image.rectTransform.anchoredPosition = eventData.position;
            _image.gameObject.SetActive(true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector3[] canvasCorners = new Vector3[4];
            _canvasRectTransform.GetWorldCorners(canvasCorners);

            float clampedX = Mathf.Clamp(eventData.position.x, canvasCorners[0].x, canvasCorners[2].x);
            float clampedY = Mathf.Clamp(eventData.position.y, canvasCorners[0].y, canvasCorners[2].y);

            _image.rectTransform.anchoredPosition = new Vector2(clampedX, clampedY);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            RO.Media.CursorAnimator.CursorMode = RO.Media.CursorAnimator.CursorModes.Hardware;
            _image.gameObject.SetActive(false);

            UIController.Panel.ClearCanvasFilterFlags(IconType == IconType.Item ?
                CanvasFilter.ItemDrag : CanvasFilter.SkillDrag);
        }

        public T OnDrop<T>(PointerEventData eventData)
        {
            Debug.Assert(_context is T);

            object context = _context;
            _context = null;
            IconType = IconType.None;
            _onIconDropped?.Invoke();

            return (T)context;
        }

        public static DragIconController Instantiate(UIController uiController, Transform parent)
        {
            var panel = AssetBundleProvider.LoadUiBundleAsset<GameObject>("DragIcon");
            panel = Instantiate(panel, parent, false);

            var controller = panel.GetComponentInChildren<DragIconController>();
            controller._uiController = uiController;

            return controller;
        }

        public IconType IconType { get; private set; }
        public short Id { get; private set; }
        public int Count { get; private set; }
        public Sprite Sprite => _image.sprite;

        private UIController _uiController;
        private object _context;
        private Action _onIconDropped;
        private Image _image;
        private RectTransform _canvasRectTransform;
    }
}
