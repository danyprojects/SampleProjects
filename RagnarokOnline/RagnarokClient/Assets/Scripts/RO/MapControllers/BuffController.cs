using RO.Databases;
using RO.MapObjects;
using RO.Network;
using System;
using UnityEngine;

namespace RO
{
    public partial class GameController : MonoBehaviour
    {
        private partial class BuffController
        {
            private GameController _gameCtrl = null;

            private Action<Unit>[] _buffApplyHandlers = new Action<Unit>[(int)BuffIDs.Last];
            private Action<Unit>[] _buffRemoveHandlers = new Action<Unit>[(int)BuffIDs.Last];

            public BuffController(GameController gameController)
            {
                _gameCtrl = gameController;
                RegisterPacketHandlers();
                RegisterBuffHandlers();
            }

            // ********* Buff handlers
            private void OnEnergyCoatApply(Unit target)
            {
                //Do nothing if target already has energy coat
                if (target.status.buffs[(int)BuffIDs.EnergyCoat])
                    return;

                //Color taken from playing with unity
                Color color = new Color(0.591f, 0.63f, 0.915f);
                if (target.BlockType == BlockTypes.Character)
                    ((Character)target).PlayerAnimator.SetColor(ref color);
            }

            private void OnEnergyCoatRemove(Unit target)
            {
                //Do nothing if target doesnt have energy coat
                if (!target.status.buffs[(int)BuffIDs.EnergyCoat])
                    return;

                Color color = new Color(0.591f, 0.63f, 0.915f);
                if (target.BlockType == BlockTypes.Character)
                    ((Character)target).PlayerAnimator.UnsetColor(ref color);
            }

            // ******** Packet handlers
            private void OnPlayerBuffApply(RCV_Packet packet)
            {
                RCV_PlayerBuffApply buffPacket = (RCV_PlayerBuffApply)packet;
                _buffApplyHandlers[(int)buffPacket.buffId](_gameCtrl._localPlayer);
                _gameCtrl._localPlayer.status.buffs[(int)buffPacket.buffId] = true;

                //Don't show a buff icon if buff doesn't have one
                if (BuffDb.Buffs[(int)buffPacket.buffId].iconId == BuffIconIDs.None)
                    return;

                if (buffPacket.buffDuration == uint.MaxValue)
                    _gameCtrl._uiController.AddPermanentBuff(buffPacket.buffId);
                else
                    _gameCtrl._uiController.AddBuff(buffPacket.buffId, buffPacket.buffDuration / 1000.0f); //convert duration to ms 
            }

            private void OnPlayerBuffRemove(RCV_Packet packet)
            {
                RCV_PlayerBuffRemove buffPacket = (RCV_PlayerBuffRemove)packet;
                _buffRemoveHandlers[(int)buffPacket.buffId](_gameCtrl._localPlayer);
                _gameCtrl._localPlayer.status.buffs[(int)buffPacket.buffId] = false;

                //Only local 
                if (BuffDb.Buffs[(int)buffPacket.buffId].iconId != BuffIconIDs.None)
                    _gameCtrl._uiController.RemoveBuff(buffPacket.buffId);
            }

            private void RegisterPacketHandlers()
            {
                PacketDistributer.RegisterCallback(PacketIds.RCV_PlayerBuffApply, OnPlayerBuffApply);
                PacketDistributer.RegisterCallback(PacketIds.RCV_PlayerBuffRemove, OnPlayerBuffRemove);
            }

            private void RegisterBuffHandlers()
            {
                _buffApplyHandlers[(int)BuffIDs.EnergyCoat] = OnEnergyCoatApply;
                _buffRemoveHandlers[(int)BuffIDs.EnergyCoat] = OnEnergyCoatRemove;
            }
        }
    }
}
