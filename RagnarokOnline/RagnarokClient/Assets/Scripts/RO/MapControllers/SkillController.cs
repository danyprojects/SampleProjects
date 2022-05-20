using RO.Common;
using RO.Databases;
using RO.MapObjects;
using RO.Media;
using RO.Network;
using System;
using UnityEngine;

namespace RO
{
    public partial class GameController : MonoBehaviour
    {
        private sealed partial class SkillController
        {
            private struct TargetData
            {
                public Block target;
                public Vector2Int position;
            }

            private GameController _gameCtrl = null;
            private Action<Unit>[] _skillFinishedHandlers = new Action<Unit>[(int)SkillIds.Last];
            private Action<int, Unit>[] _skillBeginHandlers = new Action<int, Unit>[(int)SkillIds.Last];

            private TargetData _targetData = new TargetData();

            public SkillController(GameController gameController)
            {
                _gameCtrl = gameController;
                RegisterSkillHandlers();
                RegisterPacketHandlers();
            }

            // *************************** Skill Handlers 

            //Generic method that will simply show a cast bar, skill name, default cast animation at unit and cast aura
            private void OnGenericSelfCastBegin(int castTime, Unit src, CylinderEffectIDs castAuraId)
            {
                //Create the cast circle and push the callback to disable it after castTime seconds
                //todo: for latency, cast time should be shifted forward
                float castTimeF = castTime / 1000f;

                if (src.BlockType == BlockTypes.Character)
                {
                    var character = (Character)src;
                    character.flags.IsMoving = false; // stop movement. TODO: what about free cast?

                    //TODO: If we want to take into account latency, then we should play the cast animation for longer so we can manually resume it
                    character.PlayerAnimator.PlayCastingAnimation(castTimeF);
                }

                src.castInfo.castAuraToken = EffectsAnimatorController.PlayEffect(castAuraId, src.transform, castTimeF, src.castInfo.RemoveAura);
                src.castInfo.fillBarToken = FillBarController.StartCastBar(src.transform, castTimeF, src.castInfo.RemoveFillBar);
            }

            //Generic method that will show cast bar, cast circle, skill name, default cast animation, update unit direction and cast aura
            private void OnGenericAoeCastBegin(int castTime, Unit src, int castCircleSize, CylinderEffectIDs castAuraId)
            {
                //Create the cast circle and push the callback to disable it after castTime seconds
                //todo: for latency, cast time should be shifted forward
                float castTimeF = castTime / 1000f;
                src.castInfo.castCircleToken = CastCircleController.CreateCastCircle(ref _targetData.position, castCircleSize, castTimeF, src.castInfo.RemoveCastCircle);

                //Don't do anythign else if unit is null
                if (src == null)
                    return;

                //get the cast direction
                int direction = Utility.LookUpDirectionSafe(src.position, _targetData.position);

                if (src.BlockType == BlockTypes.Character)
                {
                    var character = (Character)src;
                    character.flags.IsMoving = false; // stop movement. TODO: what about free cast?

                    //make character face direction
                    character.UpdateDirection(direction, direction);
                    character.PlayerAnimator.PlayCastingAnimation(castTimeF);
                }

                src.castInfo.castAuraToken = EffectsAnimatorController.PlayEffect(castAuraId, src.transform, castTimeF, src.castInfo.RemoveAura);
                src.castInfo.fillBarToken = FillBarController.StartCastBar(src.transform, castTimeF, src.castInfo.RemoveFillBar);
            }

            private void OnGenericTargetCastBegin(int castTime, Unit src, CylinderEffectIDs castAuraId)
            {
                if (src == null)
                    return;

                float castTimeF = castTime / 1000f;
                int direction = -1;

                //get the cast direction if target exists
                if (_targetData.target != null)
                {
                    direction = Utility.LookUpDirectionSafe(src.position, _targetData.target.position);

                    //Draw lock on at target
                    src.castInfo.castLockOnToken = CastLockOnController.CreateCastLockOn(_targetData.target.transform, castTimeF, src.castInfo.RemoveLockOn);
                }

                if (src.BlockType == BlockTypes.Character)
                {
                    var character = (Character)src;
                    character.flags.IsMoving = false; // stop movement. TODO: what about free cast?

                    //make character face direction
                    if (direction != -1)
                        character.UpdateDirection(direction, direction);
                    character.PlayerAnimator.PlayCastingAnimation(castTime / 1000f);
                }

                src.castInfo.castAuraToken = EffectsAnimatorController.PlayEffect(castAuraId, src.transform, castTimeF, src.castInfo.RemoveAura);
                src.castInfo.fillBarToken = FillBarController.StartCastBar(src.transform, castTimeF, src.castInfo.RemoveFillBar);
            }

            private void OnStormGustBegin(int castTime, Unit src)
            {
                const int STORM_GUST_SIZE = 9;

                OnGenericAoeCastBegin(castTime, src, STORM_GUST_SIZE, CylinderEffectIDs.MagicPillarBlue);
            }

            private void OnStormGustFinished(Unit src)
            {
                //if unit is valid then play cast finished animation
                if (src != null && src.IsEnabled)
                {
                    if (src.BlockType == BlockTypes.Character)
                        ((Character)src).PlayerAnimator.PlayCastingAnimation(0);
                }

                Utility.GameToWorldCoordinatesCenter(_targetData.position, out Vector3 coordinates);

                Physics.Raycast(coordinates, Vector3.down, out RaycastHit hit, Utility.RAYCAST_DISTANCE, LayerMasks.Map);
                coordinates = hit.point;

                EffectsAnimatorController.PlayEffect(EffectIDs.Stormgust, ref coordinates, Color.white, null);
            }

            private void OnEnergyCoatBegin(int castTime, Unit src)
            {
                OnGenericSelfCastBegin(castTime, src, CylinderEffectIDs.MagicPillarYellow);
            }

            private void OnEnergyCoatFinished(Unit src)
            {
                if (src == null || !src.IsEnabled)
                    return;

                Transform transform = null;
                if (src.BlockType == BlockTypes.Character)
                {
                    var character = (Character)src;
                    character.PlayerAnimator.PlayCastingAnimation(0);
                    transform = character.PlayerAnimator.transform;
                }

                src.effects.Add(EffectsAnimatorController.PlayEffect(EffectIDs.EnergyCoat, transform, src.RemoveEffect));
            }

            private void OnJupitelThunderBegin(int castTime, Unit src)
            {
                OnGenericTargetCastBegin(castTime, src, CylinderEffectIDs.MagicPillarYellow);
            }

            private void OnJupitelThunderFinished(Unit src)
            {
                Debug.Log("Received jupitel thunder finished");

                if (!src.IsEnabled)
                    return;

                if (src.BlockType == BlockTypes.Character)
                    ((Character)src).PlayerAnimator.PlayCastingAnimation(0);

                //Only check for unit. Already did null check before
                if (_targetData.target.BlockType > BlockTypes.LastUnit)
                    return;

                Unit target = (Unit)_targetData.target;
                target.effects.Add(EffectsAnimatorController.PlayJupitelThunder(src.transform, _targetData.target.transform, target.RemoveEffect));
                //Start the effect attached to target (so it disappears if character is disabled)
            }

            private void RegisterSkillHandlers()
            {
                _skillBeginHandlers[(int)SkillIds.StormGust] = OnStormGustBegin;
                _skillBeginHandlers[(int)SkillIds.EnergyCoat] = OnEnergyCoatBegin;
                _skillBeginHandlers[(int)SkillIds.JupitelThunder] = OnJupitelThunderBegin;
                _skillFinishedHandlers[(int)SkillIds.StormGust] = OnStormGustFinished;
                _skillFinishedHandlers[(int)SkillIds.EnergyCoat] = OnEnergyCoatFinished;
                _skillFinishedHandlers[(int)SkillIds.JupitelThunder] = OnJupitelThunderFinished;
            }

            // *************************** Packet handlers

            private void OnPlayerLocalBeginAoECast(RCV_Packet packet)
            {
                RCV_PlayerLocalBeginAoECast castPacket = (RCV_PlayerLocalBeginAoECast)packet;

                _targetData.position.x = castPacket.positionX;
                _targetData.position.y = castPacket.positionY;

                _skillBeginHandlers[castPacket.skillId](castPacket.castTime, _gameCtrl._localPlayer);
            }

            private void OnPlayerOtherBeginAoECast(RCV_Packet packet)
            {
                RCV_PlayerOtherBeginAoECast castPacket = (RCV_PlayerOtherBeginAoECast)packet;

                _targetData.position.x = castPacket.positionX;
                _targetData.position.y = castPacket.positionY;

                _skillBeginHandlers[castPacket.skillId](castPacket.castTime, _gameCtrl._blocks.GetChacter(castPacket.playerId));
            }

            private void OnPlayerLocalBeginTargetCast(RCV_Packet packet)
            {
                RCV_PlayerLocalBeginTargetCast castPacket = (RCV_PlayerLocalBeginTargetCast)packet;

                _targetData.target = _gameCtrl._blocks.GetBlock(castPacket.targetId);

                _skillBeginHandlers[castPacket.skillId](castPacket.castTime, _gameCtrl._localPlayer);
            }

            private void OnPlayerOtherBeginTargetCast(RCV_Packet packet)
            {
                RCV_PlayerOtherBeginTargetCast castPacket = (RCV_PlayerOtherBeginTargetCast)packet;

                //Todo: get the target depending on the type from the block
                _targetData.target = _gameCtrl._blocks.GetBlock(castPacket.targetId); ;

                _skillBeginHandlers[castPacket.skillId](castPacket.castTime, _gameCtrl._blocks.GetChacter(castPacket.playerId));
            }

            private void OnPlayerLocalBeginSelfCast(RCV_Packet packet)
            {
                RCV_PlayerLocalBeginSelfCast castPacket = (RCV_PlayerLocalBeginSelfCast)packet;
                _skillBeginHandlers[castPacket.skillId](castPacket.castTime, _gameCtrl._localPlayer);
            }

            private void OnPlayerOtherBeginSelfCast(RCV_Packet packet)
            {
                RCV_PlayerOtherBeginSelfCast castPacket = (RCV_PlayerOtherBeginSelfCast)packet;

                _skillBeginHandlers[castPacket.skillId](castPacket.castTime, _gameCtrl._blocks.GetChacter(castPacket.playerId));
            }

            private void OnPlayerLocalFinishedAoECast(RCV_Packet packet)
            {
                RCV_PlayerLocalFinishedAoECast castPacket = (RCV_PlayerLocalFinishedAoECast)packet;

                _targetData.position.x = castPacket.positionX;
                _targetData.position.y = castPacket.positionY;

                _skillFinishedHandlers[castPacket.skillId](_gameCtrl._localPlayer);
            }

            private void OnPlayerOtherFinishedAoECast(RCV_Packet packet)
            {
                RCV_PlayerOtherFinishedAoECast castPacket = (RCV_PlayerOtherFinishedAoECast)packet;

                _targetData.position.x = castPacket.positionX;
                _targetData.position.y = castPacket.positionY;

                _skillFinishedHandlers[castPacket.skillId](_gameCtrl._blocks.GetChacter(castPacket.playerId));
            }

            private void OnPlayerLocalFinishedTargetCast(RCV_Packet packet)
            {
                RCV_PlayerLocalFinishedTargetCast castPacket = (RCV_PlayerLocalFinishedTargetCast)packet;

                //Todo: get the target depending on the type from the block
                _targetData.target = _gameCtrl._blocks.GetBlock(castPacket.targetId);

                if (_targetData.target == null)
                {
                    _gameCtrl._localPlayer.PlayerAnimator.PlayStandbyAnimation();
                    return;
                }

                int direction = Utility.LookUpDirectionSafe(_gameCtrl._localPlayer.position, _targetData.target.position);
                _gameCtrl._localPlayer.UpdateDirection(direction, direction);
                _skillFinishedHandlers[castPacket.skillId](_gameCtrl._localPlayer);
            }

            private void OnPlayerOtherFinishedTargetCast(RCV_Packet packet)
            {
                RCV_PlayerOtherFinishedTargetCast castPacket = (RCV_PlayerOtherFinishedTargetCast)packet;

                Character src = _gameCtrl._blocks.GetChacter(castPacket.playerId);

                if (src == null)
                    return;

                //Todo: get the target depending on the type from the block
                _targetData.target = _gameCtrl._blocks.GetBlock(castPacket.targetId);

                if (_targetData.target == null)
                {
                    src.PlayerAnimator.PlayStandbyAnimation();
                    return;
                }

                int direction = Utility.LookUpDirectionSafe(src.position, _targetData.target.position);
                src.UpdateDirection(direction, direction);
                _skillFinishedHandlers[castPacket.skillId](src);
            }

            private void OnPlayerLocalFinishedSelfCast(RCV_Packet packet)
            {
                RCV_PlayerLocalFinishedSelfCast castPacket = (RCV_PlayerLocalFinishedSelfCast)packet;

                _skillFinishedHandlers[castPacket.skillId](_gameCtrl._localPlayer);
            }

            private void OnPlayerOtherFinishedSelfCast(RCV_Packet packet)
            {
                RCV_PlayerOtherFinishedSelfCast castPacket = (RCV_PlayerOtherFinishedSelfCast)packet;

                _skillFinishedHandlers[castPacket.skillId](_gameCtrl._blocks.GetChacter(castPacket.playerId));
            }

            private void RegisterPacketHandlers()
            {
                PacketDistributer.RegisterCallback(PacketIds.RCV_PlayerLocalBeginAoECast, OnPlayerLocalBeginAoECast);
                PacketDistributer.RegisterCallback(PacketIds.RCV_PlayerOtherBeginAoECast, OnPlayerOtherBeginAoECast);
                PacketDistributer.RegisterCallback(PacketIds.RCV_PlayerLocalBeginTargetCast, OnPlayerLocalBeginTargetCast);
                PacketDistributer.RegisterCallback(PacketIds.RCV_PlayerOtherBeginTargetCast, OnPlayerOtherBeginTargetCast);
                PacketDistributer.RegisterCallback(PacketIds.RCV_PlayerLocalBeginSelfCast, OnPlayerLocalBeginSelfCast);
                PacketDistributer.RegisterCallback(PacketIds.RCV_PlayerOtherBeginSelfCast, OnPlayerOtherBeginSelfCast);
                PacketDistributer.RegisterCallback(PacketIds.RCV_PlayerLocalFinishedAoECast, OnPlayerLocalFinishedAoECast);
                PacketDistributer.RegisterCallback(PacketIds.RCV_PlayerOtherFinishedAoECast, OnPlayerOtherFinishedAoECast);
                PacketDistributer.RegisterCallback(PacketIds.RCV_PlayerLocalFinishedTargetCast, OnPlayerLocalFinishedTargetCast);
                PacketDistributer.RegisterCallback(PacketIds.RCV_PlayerOtherFinishedTargetCast, OnPlayerOtherFinishedTargetCast);
                PacketDistributer.RegisterCallback(PacketIds.RCV_PlayerLocalFinishedSelfCast, OnPlayerLocalFinishedSelfCast);
                PacketDistributer.RegisterCallback(PacketIds.RCV_PlayerOtherFinishedSelfCast, OnPlayerOtherFinishedSelfCast);
            }
        }
    }
}
