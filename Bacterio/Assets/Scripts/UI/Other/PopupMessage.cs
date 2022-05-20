using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Bacterio.UI.Other
{
    public sealed class PopupMessage : MonoBehaviour
    {
        [SerializeField] private Button _leftButton;
        [SerializeField] private TextMeshProUGUI _leftText;
        [SerializeField] private Button _rightButton;
        [SerializeField] private TextMeshProUGUI _rightText;
        [SerializeField] private Button _centerButton;
        [SerializeField] private TextMeshProUGUI _centerText;

        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _messageText;

        public void SetActive(bool isActive)
        {
            GetComponent<Canvas>().enabled = isActive;
            this.enabled = isActive;
        }


        public void Configure(string title, string message)
        {
            _leftButton.gameObject.SetActive(false);
            _rightButton.gameObject.SetActive(false);
            _centerButton.gameObject.SetActive(false);

            _titleText.text = title;
            _messageText.text = message;
        }

        public void Configure(string title, string message, string centerButtonText, Action centerButtonClick)
        {
            //Configure buttons
            _leftButton.gameObject.SetActive(false);
            _rightButton.gameObject.SetActive(false);
            _centerButton.gameObject.SetActive(true);

            //Remove old listener and add new
            _centerButton.onClick.RemoveAllListeners();
            _centerButton.onClick.AddListener(() => centerButtonClick());
            _centerText.text = centerButtonText;

            _titleText.text = title;
            _messageText.text = message;
        }

        public void Configure(string title, string message, string leftButtonText, Action leftButtonClick, string rightButtonText, Action rightButtonClick)
        {
            //Configure buttons
            _leftButton.gameObject.SetActive(true);
            _rightButton.gameObject.SetActive(true);
            _centerButton.gameObject.SetActive(false);

            //Remove old listener and add new
            _leftButton.onClick.RemoveAllListeners();
            _rightButton.onClick.RemoveAllListeners();
            _leftButton.onClick.AddListener(() => leftButtonClick());
            _rightButton.onClick.AddListener(() => rightButtonClick());
            _leftText.text = leftButtonText;
            _rightText.text = rightButtonText;

            _titleText.text = title;
            _messageText.text = message;
        }
    }
}
