using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Bacterio.UI.Other
{
    public sealed class PopupInputField : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _hintText;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _buttonText;

        private readonly Color32 HINT_COLOR = new Color32(183, 196, 203, 255); //light gray

        public void SetActive(bool isActive)
        {
            GetComponent<Canvas>().enabled = isActive;
            this.enabled = isActive;
        }

        public void Configure(string hint, string buttonText, Action<string> onButtonPress, string title = null)
        {
            WDebug.Assert(hint != null && hint != "", "Must specify a hint");
            WDebug.Assert(buttonText != null && buttonText != "", "Must specify a button text");
            WDebug.Assert(onButtonPress != null, "Must have a valid callback");

            _hintText.text = hint;
            _titleText.text = title ?? "";
            _inputField.text = "";
            _buttonText.text = buttonText;

            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(() => onButtonPress(_inputField.text));
        }
    }
}
