using UnityEngine;

namespace RO.UI
{
    public class LabelController
    {
        private Label _label;
        private RectTransform _rootTransform;

        public LabelController(Transform rootParent)
        {
            _rootTransform = (RectTransform)rootParent;
            _label = Label.Instantiate(rootParent);
        }

        public void ShowLabel(Vector3 worldPosition, string text, Vector2 offset)
        {
            ShowLabel(worldPosition, text, offset, Label.MaxWidthBeforeOverflow);
        }

        public void ShowLabel(Vector3 worldPosition, string text, Vector2 offset, float maxWidthBeforeOverflow)
        {
            if (text == null)
                return;

            _label.transform.parent.position = worldPosition;
            _label.SetText(text, maxWidthBeforeOverflow);
            offset.y += Label.DefaultHeight;

            var rectTranform = (RectTransform)_label.transform.parent;
            rectTranform.anchoredPosition += offset;

            var clampedPosition = new Vector2
            {
                x = Mathf.Clamp(rectTranform.anchoredPosition.x, 0, _rootTransform.sizeDelta.x - rectTranform.sizeDelta.x),
                y = Mathf.Clamp(rectTranform.anchoredPosition.y, Label.DefaultHeight, _rootTransform.sizeDelta.y)
            };

            rectTranform.anchoredPosition = clampedPosition;
            rectTranform.SetAsLastSibling();

            rectTranform.gameObject.SetActive(true);
        }

        public void HideLabel()
        {
            _label.transform.parent.gameObject.SetActive(false);
        }
    }
}