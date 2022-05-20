using RO.Databases;
using RO.Network;
using UnityEngine;

namespace RO
{
    public partial class GameController : MonoBehaviour
    {
        private sealed partial class NpcController
        {
            private GameController _gameCtrl = null;

            public NpcController(GameController gameController)
            {
                _gameCtrl = gameController;
                RegisterPacketHandlers();
            }

            public void OnNpcEnterRange(RCV_OtherEnterRange packet)
            {
                Debug.Log("Received npc enter range");
                //special behavior for warp portal
                if ((NpcDb.NpcIds)packet.dbId == NpcDb.NpcIds.MapPortal)
                {
                    //Create new warp portal if it hasn't been instantiated yet
                    if (_gameCtrl._mapPortals[packet.blockId] == null)
                        _gameCtrl._mapPortals[packet.blockId] =
                            new MapObjects.MapPortal(packet.blockId, new Vector2Int(packet.posX, packet.posY), _gameCtrl.transform);
                    else
                    {
                        //Assign it's position too in case we saved it across scenes. It's faster to move a position than to reinstantiate
                        _gameCtrl._mapPortals[packet.blockId].Move(new Vector2Int(packet.posX, packet.posY));
                        _gameCtrl._mapPortals[packet.blockId].IsEnabled = true;
                    }
                    _gameCtrl._mapPortals[packet.blockId].Fade(Media.FadeDirection.In);
                }
            }

            public void OnNpcLeaveRange(RCV_OtherLeaveRange packet)
            {
                Debug.Log("Received npc leave range");

                //special behavior for warp portal
                if ((NpcDb.NpcIds)packet.dbId == NpcDb.NpcIds.MapPortal)
                {
                    //should always be created by now. Should not receive a leave range without an enter range
                    int fadeTag = _gameCtrl._mapPortals[packet.blockId].Fade(Media.FadeDirection.Out);
                    TimerController.PushNonPersistent(Media.MediaConstants.UNIT_FADE_TIME, () =>
                    {
                        if (_gameCtrl._mapPortals[packet.blockId].FadeTag == fadeTag)
                            _gameCtrl._mapPortals[packet.blockId].IsEnabled = false;
                    });
                }
            }

            public void ClearWarpPortals()
            {
                for (int i = 0; i < _gameCtrl._mapPortals.Length; i++)
                    if (_gameCtrl._mapPortals[i] != null)
                        _gameCtrl._mapPortals[i].IsEnabled = false;
            }

            private void RegisterPacketHandlers()
            {

            }
        }
    }
}
