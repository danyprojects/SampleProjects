using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Bacterio.UI.Screens
{
    public class GameEndScreen : MonoBehaviour
    {
        [SerializeField] private Button _quitButton = null;
        [SerializeField] private TextMeshProUGUI _resultText = null;
        public Action OnQuitClickCb { get; set; }

        public void SetActive(bool isActive)
        {
            GetComponent<Canvas>().enabled = isActive;
            this.enabled = isActive;
        }

        public void ShowResults()
        {
            _resultText.text = Game.GameContext.gameStatus.EndResult.ToString();
        }

        private void Awake()
        {
            _quitButton.onClick.AddListener(() => { OnQuitClickCb?.Invoke(); });
        }
    }
}
