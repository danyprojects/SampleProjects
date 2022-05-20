using System;
using UnityEngine;
using UnityEngine.UI;

namespace Bacterio.UI.Screens
{
    public sealed class TitleScreen : MonoBehaviour
    {
        [SerializeField] private Button _gameStartButton = null;
        public Action OnStartGameClickCb { get; set; }

        public void SetActive(bool isActive)
        {
            GetComponent<Canvas>().enabled = isActive;
            this.enabled = isActive;
        }

        private void Awake()
        {
            _gameStartButton.onClick.AddListener(() => { OnStartGameClickCb?.Invoke(); });
        }
    }
}
