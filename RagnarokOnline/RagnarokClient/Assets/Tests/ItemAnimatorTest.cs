using RO;
using RO.Common;
using RO.Databases;
using RO.Media;
using UnityEngine;

namespace Tests
{
    public class ItemAnimatorTest : MonoBehaviour
    {
        public ItemIDs Item = ItemIDs.None;
        GameObject obj;
        ItemAnimator item = null;

        void Start()
        {
            Globals.Time = Time.time;
        }

        // Update is called once per frame
        void Update()
        {
            Globals.Time += Time.deltaTime;
            Globals.TimeSinceLevelLoad = Time.timeSinceLevelLoad;

            if (Item != ItemIDs.None)
            {
                var itemData = AssetBundleProvider.LoadItemDataAsset(Item);
                Item = ItemIDs.None;

                if (item == null)
                    item = ObjectPoll.ItemAnimatorsPool.GetComponent<ItemAnimator>();

                item.AnimateItem(itemData, null);
            }
        }
    }
}
