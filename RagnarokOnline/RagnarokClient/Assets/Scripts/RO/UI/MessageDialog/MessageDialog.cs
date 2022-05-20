using System;
using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    public sealed class MessageDialog : UIController.Panel
        , IPointerDownHandler, ICanvasRaycastFilter
    {
        [SerializeField]
        private Text _message = default;

        [SerializeField]
        private Button _ok = default;

        [SerializeField]
        private Button _cancel = default;

        private RectTransform _okTransform;

        private Vector3 _okPos;
        private Vector3 _cancelPos;
        private CanvasFilter _filter;
        private void Awake()
        {
            _okTransform = _ok.GetComponent<RectTransform>();
            _okPos = _okTransform.localPosition;
            _cancelPos = _cancel.GetComponent<RectTransform>().localPosition;
        }

        public void Fill(string message, CanvasFilter filter, Action ok, Action cancel = null)
        {
            SetCanvasFilterFlags(filter);

            _message.text = message;
            _filter = filter;
            _ok.OnClick = ok;
            _cancel.OnClick = cancel;
            _okTransform.localPosition = cancel == null ? _cancelPos : _okPos;
        }

        public new bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            return (~_filter & CanvasFilter) == 0;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            BringToFront();
        }

        public static MessageDialog Instantiate(UIController uiController, Transform parent)
        {
            return Instantiate<MessageDialog>(uiController, parent, "MessageDialog");
        }
    }
}