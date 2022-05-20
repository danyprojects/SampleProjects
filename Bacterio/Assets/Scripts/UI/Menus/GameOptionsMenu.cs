using System;
using UnityEngine;
using UnityEngine.UI;

namespace Bacterio.UI.Menus
{
    public class GameOptionsMenu : MonoBehaviour
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _returnToGameButton;
        [SerializeField] private Button _returnToMainMenuButton;

        public Action OnCloseButtonCb { get; set; }
        public Action OnReturnToMainMenuCb { get; set; }

        public void SetActive(bool isActive)
        {
            GetComponent<Canvas>().enabled = isActive;
            this.enabled = isActive;
        }

        private void Awake()
        {
            _closeButton.onClick.AddListener(() => { OnCloseButtonCb?.Invoke(); });
            _returnToGameButton.onClick.AddListener(() => { OnCloseButtonCb?.Invoke(); });
            _returnToMainMenuButton.onClick.AddListener(() => { OnReturnToMainMenuCb?.Invoke(); });
        }
    }
}
