using System;
using UnityEngine;
using UnityEngine.UI;

namespace Bacterio.UI.Panels
{
    public sealed class OptionsRibbonPanel : MonoBehaviour
    {
        [SerializeField] private Button _openGameOptionsButton;
        [SerializeField] private Button _openUpgradeShopButton;

        public Action OnGameOptionsCb { get; set; }
        public Action OnUpgradeShopCb { get; set; }

        public void SetActive(bool isActive)
        {
            GetComponent<Canvas>().enabled = isActive;
            this.enabled = isActive;
        }

        private void Awake()
        {
            _openGameOptionsButton.onClick.AddListener(() => { OnGameOptionsCb?.Invoke(); });
            _openUpgradeShopButton.onClick.AddListener(() => { OnUpgradeShopCb?.Invoke(); });
        }
    }
}
