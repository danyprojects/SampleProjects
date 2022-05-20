using RO.Databases;
using RO.MapObjects;
using RO.Network;
using System;
using UnityEngine;

namespace RO
{
    public partial class GameController : MonoBehaviour
    {
        private partial class ItemController
        {
            private GameController _gameCtrl = null;

            public ItemController(GameController gameController)
            {
                _gameCtrl = gameController;
            }

            public void UpdateItems()
            {
                if(_gameCtrl._cameraFollow.RotationChanged)
                {
                    Item item = _gameCtrl._blocks.FirstItem;
                    while (item != null)
                    {
                        if (item.IsEnabled)
                            item.transform.localEulerAngles = _gameCtrl._cameraFollow.Target.localEulerAngles;
                        item = item._nextItem;
                    }
                }
            }

            public void OnItemEnterRange(RCV_OtherEnterRange packet)
            {
                //Maybe this wont be necessary?
                if(_gameCtrl._blocks.GetItem(packet.blockId) != null)
                    _gameCtrl._blocks.DeleteItem(_gameCtrl._blocks.GetItem(packet.blockId));

                Item item = _gameCtrl._blocks.CreateItem(packet.blockId, (ItemIDs)packet.dbId);
                
                //Update item position and rotation
                item.MoveTo(new Vector2Int(packet.posX, packet.posY));
                item.transform.localEulerAngles = _gameCtrl._cameraFollow.Target.localEulerAngles;
            }

            public void OnItemLeaveRange(RCV_OtherLeaveRange packet)
            {
                _gameCtrl._blocks.DeleteItem(_gameCtrl._blocks.GetItem(packet.blockId));
            }
        }
    }
}
