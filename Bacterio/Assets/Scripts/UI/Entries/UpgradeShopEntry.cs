using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Bacterio.UI.Entries
{
    public sealed class UpgradeShopEntry : MonoBehaviour
    {
        [SerializeField] private Button _entryButton;
        [SerializeField] private TextMeshProUGUI _entryNameText;
        [SerializeField] private TextMeshProUGUI _entryPriceText;

        public Databases.ShopItemId _shopItemId;

        private void Awake()
        {
        }

        public void Configure(Action<UpgradeShopEntry> clickCb, Databases.ShopItemId shopItemId, string name, int price, bool canBuy)
        {
            _shopItemId = shopItemId;
            _entryNameText.text = name;
            _entryPriceText.text = price.ToString();
            _entryPriceText.color = canBuy ? Constants.SHOP_TEXT_NORMAL : Constants.SHOP_NOT_ENOUGH_CURRENCY;
            _entryButton.onClick.RemoveAllListeners();
            _entryButton.onClick.AddListener(() => clickCb(this));
        }

        public void UpdatePrice(int price, bool canBuy)
        {
            _entryPriceText.text = price.ToString();
            _entryPriceText.color = canBuy ? Constants.SHOP_TEXT_NORMAL : Constants.SHOP_NOT_ENOUGH_CURRENCY;
        }

        public void Disable()
        {
            _entryButton.onClick.RemoveAllListeners();
            _entryNameText.color = Constants.SHOP_GRAYED_OUT_TEXT;
            _entryNameText.text = "Sold out";
        }
    }
}
