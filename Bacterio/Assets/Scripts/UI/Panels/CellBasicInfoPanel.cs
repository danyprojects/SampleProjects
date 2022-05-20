using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Bacterio.MapObjects;

namespace Bacterio.UI.Panels
{
    public sealed class CellBasicInfoPanel : MonoBehaviour
    {
        [SerializeField] private Slider _hpSlider;
        [SerializeField] private TextMeshProUGUI _hpText;
        [SerializeField] private TextMeshProUGUI _upgradePointsText;
        [SerializeField] private TextMeshProUGUI _bulletText;
        [SerializeField] private TextMeshProUGUI _livesText;

        public void SetActive(bool isActive)
        {
            GetComponent<Canvas>().enabled = isActive;
            this.enabled = isActive;

            if(isActive)
            {
                Cell.CurrentHpChanged += OnCurrentHpChanged;
                Cell.MaxHpChanged += OnMaxHpChanged;
                Cell.AmmunitionChanged += OnAmmunitionChanged;
                Cell.UpgradePointsChanged += OnUpgradePointsChanged;
                Cell.LivesChanged += OnLivesChanged;
            }
            else
            {
                Cell.CurrentHpChanged -= OnCurrentHpChanged;
                Cell.MaxHpChanged -= OnMaxHpChanged;
                Cell.AmmunitionChanged -= OnAmmunitionChanged;
                Cell.UpgradePointsChanged -= OnUpgradePointsChanged;
                Cell.LivesChanged -= OnLivesChanged;
            }
        }

        private void Awake()
        {
        }

        private void OnCurrentHpChanged(Cell cell, int newHp)
        {
            if (!cell.hasAuthority)
                return;

            _hpSlider.value = newHp;
            _hpText.text = newHp.ToString() + " / " + _hpSlider.maxValue;
        }

        private void OnMaxHpChanged(Cell cell, int newHp)
        {
            if (!cell.hasAuthority)
                return;

            _hpSlider.maxValue = newHp;
            _hpText.text = _hpSlider.value + " / " + newHp;
        }

        private void OnLivesChanged(Cell cell, int newLives)
        {
            if (!cell.hasAuthority)
                return;

            if (newLives <= -1)
                newLives = 0;

            var newLivesStr = newLives.ToString();
            if (newLivesStr == _livesText.text)
                return;

            _livesText.text = newLivesStr;
        }

        private void OnAmmunitionChanged(Cell cell, int newAmmunition)
        {
            if (!cell.hasAuthority)
                return;

            _bulletText.text = newAmmunition.ToString();
        }

        private void OnUpgradePointsChanged(Cell cell, int newPoints)
        {

            if (!cell.hasAuthority)
                return;

            _upgradePointsText.text = newPoints.ToString();
        }

        private void OnDestroy()
        {
            Cell.CurrentHpChanged -= OnCurrentHpChanged;
            Cell.MaxHpChanged -= OnMaxHpChanged;
            Cell.AmmunitionChanged -= OnAmmunitionChanged;
            Cell.UpgradePointsChanged -= OnUpgradePointsChanged;
            Cell.LivesChanged -= OnLivesChanged;
        }
    }
}
