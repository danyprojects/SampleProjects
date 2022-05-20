using System;
using UnityEngine;
using UnityEngine.UI;

namespace Bacterio.UI.Screens
{
    public class DisconnectedScreen : MonoBehaviour
    {
        [SerializeField] private Button _quitButton = null;
        public Action OnQuitClickCb { get; set; }

        public void SetActive(bool isActive)
        {
            GetComponent<Canvas>().enabled = isActive;
            this.enabled = isActive;
        }

        private void Awake()
        {
            _quitButton.onClick.AddListener(() => { OnQuitClickCb?.Invoke(); });
        }
    }
}
