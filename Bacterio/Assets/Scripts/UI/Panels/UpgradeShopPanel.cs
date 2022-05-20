using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Bacterio.UI.Entries;
using Bacterio.Databases;

namespace Bacterio.UI.Panels
{
    public sealed class UpgradeShopPanel : MonoBehaviour
    {
        [SerializeField] private UpgradeShopEntry _upgradeShopEntry = null;
        [SerializeField] private Button _exitButton = null;
        [SerializeField] private TextMeshProUGUI _currencyAmountText = null;

        [NonSerialized] public MapObjects.Cell LocalCell = null;
        [NonSerialized] public Action<MapObjects.Cell, AuraDbId> AuraAttachCb = null;
        [NonSerialized] public Action<MapObjects.Cell, BuffDbId> ApplyBuffCb = null;

        private bool _isInitialized = false;
        private Common.ObjectPool<UpgradeShopEntry> _entriesPool = null;
        private List<UpgradeShopEntry> _entries = null;

        public void SetActive(bool isActive)
        {
            GetComponent<Canvas>().enabled = isActive;
            this.enabled = isActive;

            if (isActive)
            {
                UpdateFields();
            }
            else
            {
            }
        }

        public void Awake()
        {
            _entriesPool = new Common.ObjectPool<UpgradeShopEntry>(_upgradeShopEntry, 1, 1, _upgradeShopEntry.transform.position, Quaternion.identity, _upgradeShopEntry.transform.parent);
            _entries = new List<UpgradeShopEntry>();
            _exitButton.onClick.AddListener(() => { SetActive(false); });
        }

        private void UpdateFields()
        {
            WDebug.Assert(LocalCell != null, "Panel was shown but we didn't have a local cell");

            //Fill the general fields
            _currencyAmountText.text = LocalCell.UpgradePoints.ToString();

            //We only do this once and as we go since it's relatively heavy
            if (!_isInitialized)
            {
                InitializeEntries();
                _isInitialized = true;
            }
        }
        
        private void InitializeEntries()
        {
            var entryOffset = ((RectTransform)_upgradeShopEntry.transform).rect.height + Constants.SHOP_ENTRY_SPACING;

            for (int i = 0; i <= (int)ShopItemId.Last; i++)
            {
                var id = (ShopItemId)i;
                ref var itemData = ref GlobalContext.shopDb.GetShopData(id);

                //Get a new entry
                var entry = _entriesPool.Pop();

                //configure it
                //Divide by 100 at the end so we don't need to convert anything to float
                var price = itemData.price + itemData.price * LocalCell._upgrades[i] * itemData.multiplier / 100;
                entry.Configure(OnEntryClick, id, itemData.name, price, price <= LocalCell.UpgradePoints);

                //position it
                entry.transform.localPosition += new Vector3(0, -i * entryOffset, 0);

                //Activate the entry and track it
                entry.gameObject.SetActive(true);
                _entries.Add(entry);
            }
        }

        private void OnEntryClick(UpgradeShopEntry entry)
        {
            ref var itemData = ref GlobalContext.shopDb.GetShopData(entry._shopItemId);

            //Don't allow purchasing over the limit
            if (LocalCell._upgrades[(int)entry._shopItemId] >= itemData.limit)
                return;

            //Divide by 100 at the end so we don't need to convert anything to float
            var price = itemData.price + itemData.price * LocalCell._upgrades[(int)entry._shopItemId] * itemData.multiplier / 100;

            //Dont allow purchase if not enough points
            if (LocalCell.UpgradePoints < price)
                return;

            //Run the item effect
            switch (entry._shopItemId)
            {
                case ShopItemId.Heal: LocalCell.CurrentHp = LocalCell.GetMaxHp(); break;
                case ShopItemId.Revive: LocalCell.Lives++; break;
                case ShopItemId.AttackUp: LocalCell.AttackMultiplier += 20; break;

                default: WDebug.LogWarn("Invalid shop item id: " + entry._shopItemId);break;
            }

            //Mark and purchase
            LocalCell._upgrades[(int)entry._shopItemId]++;
            LocalCell.UpgradePoints -= price;

            //If we can't buy any more, remove it from the shop
            if (LocalCell._upgrades[(int)entry._shopItemId] >= itemData.limit)
            {
                entry.Disable();
            }
            else //update price
            {
                price = itemData.price + itemData.price * LocalCell._upgrades[(int)entry._shopItemId] * itemData.multiplier / 100;
                entry.UpdatePrice(price, price <= LocalCell.UpgradePoints);
            }
        }
    }
}
