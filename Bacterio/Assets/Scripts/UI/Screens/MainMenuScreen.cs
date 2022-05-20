using System;
using UnityEngine;
using UnityEngine.UI;

namespace Bacterio.UI.Screens
{
    public sealed class MainMenuScreen : MonoBehaviour
    {
        [SerializeField] private Button _singlePlayerButton = null;
        [SerializeField] private Button _multiplayerButton = null;

        public Action OnSinglePlayerClickCb { get; set; }
        public Action OnMultiPlayerClickCb { get; set; }

        public void SetActive(bool isActive)
        {
            GetComponent<Canvas>().enabled = isActive;
            this.enabled = isActive;
        }

        private void Awake()
        {
            _singlePlayerButton.onClick.AddListener(() => { OnSinglePlayerClickCb?.Invoke(); });
            _multiplayerButton.onClick.AddListener(() => { OnMultiPlayerClickCb?.Invoke(); });
        }
    }
}