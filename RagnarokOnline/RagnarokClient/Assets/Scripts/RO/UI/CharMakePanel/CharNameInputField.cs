using UnityEngine;
using UnityEngine.UI;

namespace RO.UI
{
    public class CharNameInputField : InputField
        , IDeselectHandler, ISelectHandler, IPointerDownHandler
    {
        [SerializeField]
        private Image _background = default;

        public new void OnPointerDown(PointerEventData eventData)
        {
            EventSystem.SetSelectedGameObject(gameObject);
            base.OnPointerDown(eventData);
        }

        public void OnSelect()
        {
            _background.canvasRenderer.SetAlpha(1);
        }

        public void OnDeselect()
        {
            _background.canvasRenderer.SetAlpha(0);
            EventSystem.CurrentKeyboardHandler = null;
        }
    }
}