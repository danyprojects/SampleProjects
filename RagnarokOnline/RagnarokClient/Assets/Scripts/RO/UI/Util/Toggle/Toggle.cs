using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace RO.UI
{
    public class Toggle : ToggleGroup.ToggleBase, IPointerClickHandler
        , IPointerDownHandler, IPointerUpHandler
        , IPointerEnterHandler, IPointerExitHandler
        , ICanvasRaycastFilter
    {
        [SerializeField]
        [FormerlySerializedAs("Target Graphic")]
        private Image _graphic = default;

        [SerializeField]
        [FormerlySerializedAs("On Sprite")]
        private Sprite _onSprite = default;

        [SerializeField]
        [FormerlySerializedAs("Off Sprite")]
        private Sprite _offSprite = default;

        [SerializeField]
        [FormerlySerializedAs("Highligthed Sprite")]
        private Sprite _highligthedSprite = default;

        [SerializeField]
        [FormerlySerializedAs("Default Value")]
        private bool _isOn = true;

        [SerializeField]
        [FormerlySerializedAs("Highligthed Tint")]
        private Color _highligthedTint = Color.white;

        public bool IsOn => _isOn;

        public Action<bool> OnValueChanged;

        private UIController.Panel _panel;

        void Awake()
        {
            _panel = GetComponentInParent<UIController.Panel>();

            if (_isOn && _group != null)
            {
                if (_group.SelectedToggle != null)
                    _isOn = false; // if we are being cloned with instantiate for example
                else
                    SetAsInitialToogle();
            }

            _graphic.sprite = _isOn ? _onSprite : _offSprite;
        }

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            const CanvasFilter filter = ~(CanvasFilter.NpcDialog
                | CanvasFilter.ModalMsgDialog | CanvasFilter.DisconnectDialog);

            return (filter & UIController.Panel.CanvasFilter) == 0 && enabled;
        }

        public override void SetValue(bool value)
        {
            if (value == _isOn)
                return;

            // block turning active toogle off if we have group
            if (!value && _group?.SelectedToggle == this)
                return;

            _isOn = value;
            _graphic.sprite = _isOn ? _onSprite : _offSprite;

            OnValueChanged?.Invoke(_isOn);

            if (value && _group != null)
                SetAsSelectedToogle();
        }

        // Usefull if we want to manipulate color manually
        public void SetColor(Color color)
        {
            _graphic.color = color;
        }

        public void AddToGroup(ToggleGroup toggleGroup)
        {
            Debug.Assert(_group == false);

            _group = toggleGroup;
            _isOn = false;

            SetValue(true);
        }

        public void RemoveFromGroup(Toggle newSelectedToggle)
        {
            Debug.Assert(newSelectedToggle != this);
            _group = null;

            if (_isOn)
            {
                newSelectedToggle.SetValue(true); // will put toggle to false;
                _isOn = true;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            _panel.BringToFront();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            SetValue(!_isOn);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            RO.Media.CursorAnimator.SetAnimation(RO.Media.CursorAnimator.Animations.Click);

            if (_highligthedSprite != null)
                _graphic.sprite = _highligthedSprite;

            _graphic.color = _highligthedTint;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            RO.Media.CursorAnimator.UnsetAnimation(RO.Media.CursorAnimator.Animations.Click);

            _graphic.sprite = _isOn ? _onSprite : _offSprite;
            _graphic.color = Color.white;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // prevent from bubbling up
        }
    }
}
