using RO.Common;
using RO.Containers;
using RO.Databases;
using RO.Media;
using UnityEngine;

namespace RO.MapObjects
{
    public sealed class Item : Block
    {
        public override bool IsEnabled 
        {
            get 
            {
                return ItemAnimator.enabled;
            } 
            set
            {
                ItemAnimator.enabled = value;
            }
        }

        public ItemData ItemData = null;

        public Item _nextItem = null, _previousItem = null;
        public ItemAnimator ItemAnimator { get; set; } = null;

        public Item(int sessionId, ItemIDs itemDbId)
            : base(sessionId, BlockTypes.Item)
        {
            //Item has a parent empty object for handling position and rotations
            ItemAnimator = ObjectPoll.ItemAnimatorsPool.GetComponent<ItemAnimator>();
            SetGameObject(ItemAnimator.gameObject);

            ItemData = AssetBundleProvider.LoadItemDataAsset(itemDbId);
            ItemAnimator.AnimateItem(ItemData, this);
            ItemAnimator.enabled = true;

        }

        public void MoveTo(Vector2Int destination)
        {
            position = destination;
            Utility.GameToWorldCoordinatesCenter(destination, out Vector3 pos);

            //Calculate a random position within the cell
            pos.x += Random.Range(-Constants.HALF_CELL_UNIT_SIZE, Constants.HALF_CELL_UNIT_SIZE);
            pos.z += Random.Range(-Constants.HALF_CELL_UNIT_SIZE, Constants.HALF_CELL_UNIT_SIZE);

            Physics.Raycast(pos, Vector3.down, out RaycastHit hit, Utility.RAYCAST_DISTANCE, LayerMasks.Map);
            transform.localPosition = new Vector3(hit.point.x, hit.point.y, hit.point.z);            
        }

        public void Cleanup()
        {
            IsEnabled = false;
            ItemAnimator.enabled = false;
            ObjectPoll.ItemAnimatorsPool = ItemAnimator.gameObject;
            ItemAnimator = null;
            ItemData = null;
        }

    }
}
