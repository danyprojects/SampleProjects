using System;
using UnityEngine;
using UnityEngine.UI;

namespace Bacterio.UI.Screens
{
    public sealed class ModeSelectScreen : MonoBehaviour
    {
        [SerializeField] private Button _survivalModeButton = null;
        public Action OnSurvivalModeClickCb { get; set; }


        public void SetActive(bool isActive)
        {
            GetComponent<Canvas>().enabled = isActive;
            this.enabled = isActive;
        }

        private void Awake()
        {
            _survivalModeButton.onClick.AddListener(() => { OnSurvivalModeClickCb?.Invoke(); });
        }
    }
}