using System;
using UnityEngine;
using TMPro;

namespace Bacterio.UI.Screens
{
    public class PlayerDeathScreen : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _respawnTimeText = null;
        private BackgroundCanvas _backgroundCanvas;
        private Action _onRespawnTimeEnd;
        private int _remainingTime;

        public void SetActive(bool isActive)
        {
            GetComponent<Canvas>().enabled = isActive;
            this.enabled = isActive;
        }

        public void SetBackgroundCanvas(BackgroundCanvas backgroundCanvas)
        {
            _backgroundCanvas = backgroundCanvas;
        }

        public void SetRemainingTime(int remainingTime, Action onRespawnTimeEnd)
        {
            _backgroundCanvas.SetDimmedBackground();
            _backgroundCanvas.SetActive(true);

            //Only show the UI with no countdown if time is invalid
            WDebug.Log("Remaining time: " + remainingTime);
            if (remainingTime == Constants.INVALID_CELL_RESPAWN_TIME)
            {
                _respawnTimeText.text = "";
                return;
            }

            _onRespawnTimeEnd = onRespawnTimeEnd;
            _remainingTime = remainingTime / Constants.ONE_SECOND_MS;
            _respawnTimeText.text = _remainingTime.ToString();

            //Start the timer for the countdown
            Game.GameContext.timerController.Add(Constants.ONE_SECOND_MS, DecrementRemainingTime);
        }

        public void DecrementRemainingTime()
        {
            if (!enabled)
                return;

            _remainingTime--;
            
            //If we reach 0, respawn
            if(_remainingTime <= 0)
            {
                SetActive(false);
                _backgroundCanvas.SetActive(false);
                _onRespawnTimeEnd();
                return;
            }

            //Else update text and queue up for another second
            _respawnTimeText.text = _remainingTime.ToString();

            //Will stop requeing once death screen is disabled
            Game.GameContext.timerController.Add(Constants.ONE_SECOND_MS, DecrementRemainingTime);
        }
    }
}
