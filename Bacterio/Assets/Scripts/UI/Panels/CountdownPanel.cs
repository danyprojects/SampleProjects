using UnityEngine;
using TMPro;

namespace Bacterio.UI.Panels
{
    public sealed class CountdownPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _activeWoundsText;

        private long _durationMs;

        private long _prevSec;
        private long _prevMin;

        public void SetActive(bool isActive)
        {
            GetComponent<Canvas>().enabled = isActive;
            this.enabled = isActive;

            if (isActive)
            {
                Game.GameStatus.ActiveWoundCountChanged += SetWoundCount;
                Game.GameStatus.ElapsedTimeChanged += UpdateTime;
            }
            else 
            {
                Game.GameStatus.ActiveWoundCountChanged -= SetWoundCount;
                Game.GameStatus.ElapsedTimeChanged -= UpdateTime;
            }
        }

        private void Awake()
        {
        }

        public void SetDurationTime(long currentTimeMs, long durationMs)
        {
            _durationMs = durationMs;

            _prevSec = -1;
            _prevMin = -1;

            UpdateTime(currentTimeMs);
        }

        private void UpdateTime(long elapsedTime)
        {
            long remainingMs = _durationMs - elapsedTime;

            if (remainingMs < 0)
                return;

            long secs = (remainingMs % Constants.ONE_MINUTE_MS) / Constants.ONE_SECOND_MS;
            long mins = remainingMs / Constants.ONE_MINUTE_MS;

            if (secs == _prevSec && mins == _prevMin)
                return;

            _timerText.text = (mins < 10 ? "0" + mins.ToString() : mins.ToString()) + " : " + (secs < 10 ? "0" + secs.ToString() : secs.ToString());
            _prevSec = secs;
            _prevMin = mins;
        }

        public void SetWoundCount(int woundCount)
        {
            _activeWoundsText.text = woundCount.ToString();
        }

        private void OnDestroy()
        {
            Game.GameStatus.ActiveWoundCountChanged -= SetWoundCount;
            Game.GameStatus.ElapsedTimeChanged -= UpdateTime;
        }
    }
}
