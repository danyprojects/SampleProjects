using RO.Common;
using RO.Databases;
using RO.MapObjects;
using RO.Media;
using RO.Network;
using UnityEngine;

namespace RO
{

    public partial class GameController : MonoBehaviour
    {
        private sealed partial class UnitController
        {
            GameController _gameCtrl = null;

            public UnitController(GameController gameController)
            {
                _gameCtrl = gameController;
                RegisterPacketHandlers();
            }

            public void UpdateCharacters()
            {
                //Update local player independently
                UpdateLocalCharacter();

                //Update other players
                Character character = _gameCtrl._blocks.FirstCharacter;
                while (character != null)
                {
                    if (character.IsEnabled)
                        UpdateCharacter(character);
                    character = character._nextCharacter;
                }
            }

            public void UpdateMonsters()
            {
                Monster monster = _gameCtrl._blocks.FirstMonster;
                while (monster != null)
                {
                    if (monster.IsEnabled)
                        UpdateMonster(monster);
                    monster = monster._nextMonster;
                }
            }

            //************** Internal methods
            private void UpdateLocalCharacter()
            {
                Character localCharacter = _gameCtrl.LocalPlayer;

                //For a small interval, it's possible to be moving and fixing position at same time, although only one of them will update the position
                if (localCharacter.flags.IsMoving)
                    localCharacter.Move();
                if (localCharacter.flags.IsFixingPosition)
                    localCharacter.FixPosition();

                localCharacter.PlayerAnimator.UpdateAnimations();

                if (_gameCtrl._cameraFollow.DirectionChanged)
                    localCharacter.UpdateDirection(_gameCtrl._cameraFollow.CameraDirection);
                if (_gameCtrl._cameraFollow.RotationChanged)
                    if (_gameCtrl.LocalPlayer.transform == _gameCtrl._cameraFollow.Target)
                        localCharacter.UpdateRotationAsCenter(_gameCtrl._cameraFollow.transform);
                    else
                        localCharacter.UpdateRotationFromCenter(_gameCtrl._cameraFollow.Target);
            }

            private void UpdateCharacter(Character character)
            {
                //For a small interval, it's possible to be moving and fixing position at same time, although only one of them will update the position
                if (character.flags.IsMoving)
                    character.Move();
                if (character.flags.IsFixingPosition)
                    character.FixPosition();

                character.PlayerAnimator.UpdateAnimations();

                if (_gameCtrl._cameraFollow.DirectionChanged)
                    character.UpdateDirection(_gameCtrl._cameraFollow.CameraDirection);
                if (_gameCtrl._cameraFollow.RotationChanged)
                    character.UpdateRotationFromCenter(_gameCtrl._cameraFollow.Target);
            }

            private void UpdateMonster(Monster monster)
            {
                //For a small interval, it's possible to be moving and fixing position at same time, although only one of them will update the position
                if (monster.flags.IsMoving)
                    monster.Move();
                if (monster.flags.IsFixingPosition)
                    monster.FixPosition();

                monster.MonsterAnimator.UpdateAnimations();

                if (_gameCtrl._cameraFollow.DirectionChanged)
                    monster.UpdateCameraDirection(_gameCtrl._cameraFollow.CameraDirection);
                if (_gameCtrl._cameraFollow.RotationChanged)
                    monster.UpdateRotationFromCenter(_gameCtrl._cameraFollow.Target);
            }

            //************** Packet handlers
            private void OnPlayerJobOrLevelChange(RCV_Packet packet)
            {
                Debug.Log("Received player job or level change packet");
                var rcv = (RCV_PlayerJobOrLevelChanged)packet;

                //Check if it's local player to ignore job change (should be followed by a skill tree change)
                if (rcv.playerId == Constants.LOCAL_SESSION_ID)
                {
                    //ignore job change for local player

                    if (rcv.jobLevel > 0 && _gameCtrl._localPlayer.JobLvl != rcv.jobLevel)
                    {
                        //play job lvl up animation
                        Character localChar = _gameCtrl._localPlayer;
                        localChar.effects.Add(EffectsAnimatorController.PlayEffect(EffectIDs.LevelUpJob, localChar.transform, localChar.RemoveEffect));
                        _gameCtrl._localPlayer.JobLvl = rcv.jobLevel;
                    }
                    if (rcv.baseLevel > 0 && _gameCtrl._localPlayer.BaseLvl != rcv.baseLevel)
                    {
                        //play base lvl up animation
                        Character localChar = _gameCtrl._localPlayer;
                        localChar.effects.Add(EffectsAnimatorController.PlayEffect(EffectIDs.LevelUpBase, localChar.transform, localChar.RemoveEffect));
                        _gameCtrl._localPlayer.BaseLvl = rcv.baseLevel;
                    }
                    return;
                }

                //Otherwise it's another character
                Character character = _gameCtrl._blocks.GetChacter(rcv.playerId);

                if (character == null)
                    return;

                //Other character can change job
                if (character._charInfo.job != rcv.job)
                {
                    character._charInfo.job = rcv.job;
                    character.PlayerAnimator.ChangeJob(rcv.job);
                    //No animation since job changes should always result in a job lvl change > 0
                }
                if (rcv.jobLevel > 0)
                    character.effects.Add(EffectsAnimatorController.PlayEffect(EffectIDs.LevelUpJob, character.transform, character.RemoveEffect));
                if (rcv.baseLevel > 0)
                    character.effects.Add(EffectsAnimatorController.PlayEffect(EffectIDs.LevelUpBase, character.transform, character.RemoveEffect));
            }

            private void OnPlayerLocalMove(RCV_Packet packet)
            {
                RCV_PlayerLocalMove mvPacket = (RCV_PlayerLocalMove)packet;
                _gameCtrl._localPlayer.SetMoveDestination(mvPacket.posX, mvPacket.posY, mvPacket.destX, mvPacket.destY, mvPacket.startDelay);
            }

            private void OnPlayerOtherMove(RCV_Packet packet)
            {
                RCV_PlayerOtherMove mvPacket = (RCV_PlayerOtherMove)packet;
                _gameCtrl._blocks.GetChacter(mvPacket.playerId).SetMoveDestination(mvPacket.posX, mvPacket.posY, mvPacket.destX, mvPacket.destY, mvPacket.startDelay);
            }

            private void OnMonsterMove(RCV_Packet packet)
            {
                RCV_MonsterMove mvPacket = (RCV_MonsterMove)packet;

                //Debug.Log("Received monster move ID: " + mvPacket.monsterId);

                _gameCtrl._blocks.GetMonster(mvPacket.monsterId).SetMoveDestination(mvPacket.posX, mvPacket.posY, mvPacket.destX, mvPacket.destY, mvPacket.startDelay);
            }

            private void OnPlayerLocalStopMove(RCV_Packet packet)
            {
                var mvPacket = (RCV_PlayerLocalStopMove)packet;

                _gameCtrl._localPlayer.flags.IsMoving = false;
                _gameCtrl._localPlayer.PlayerAnimator.PlayIdleAnimation();

                //Start a lerp from current graphical position to cell posX, posY
                _gameCtrl._localPlayer.LerpToCell(mvPacket.posX, mvPacket.posY);
            }

            private void OnPlayerOtherStopMove(RCV_Packet packet)
            {
                var mvPacket = (RCV_PlayerOtherStopMove)packet;

                Character character = _gameCtrl._blocks.GetChacter(mvPacket.playerId);
                character.flags.IsMoving = false;
                character.PlayerAnimator.PlayIdleAnimation();

                //Start a lerp from current graphical position to cell posX, posY
                character.LerpToCell(mvPacket.posX, mvPacket.posY);
            }

            private void OnMonsterStopMove(RCV_Packet packet)
            {
                RCV_MonsterStopMove mvPacket = (RCV_MonsterStopMove)packet;
                Monster monster = _gameCtrl._blocks.GetMonster(mvPacket.monsterId);

                monster.flags.IsMoving = false;
                monster.MonsterAnimator.PlayIdleAnimation();

                monster.LerpToCell(mvPacket.posX, mvPacket.posY);
            }

            private void OnPlayerWarpToCell(RCV_Packet packet)
            {
                RCV_PlayerWarpToCell warpPacket = (RCV_PlayerWarpToCell)packet;

                Debug.Log("Received player warp to cell");

                _gameCtrl._uiController.FadeOutScreen();

                _gameCtrl._localPlayer.flags.IsFixingPosition = false;
                _gameCtrl._localPlayer.flags.IsMoving = false;
                _gameCtrl._localPlayer.PlayerAnimator.PlayIdleAnimation();

                //Disable all enabled characters and monsters
                Character character = _gameCtrl._blocks.FirstCharacter;
                while (character != null)
                {
                    character.flags.IsMoving = false;
                    character.flags.IsFixingPosition = false;
                    character.IsEnabled = false;
                    character = character._nextCharacter;
                }

                Monster monster = _gameCtrl._blocks.FirstMonster;
                while (monster != null)
                {
                    monster.flags.IsMoving = false;
                    monster.flags.IsFixingPosition = false;
                    monster.IsEnabled = false;
                    monster = monster._nextMonster;
                }

                //disable warp portals
                _gameCtrl._npcController.ClearWarpPortals();

                //Only update player position once fade finishes due to camera snapping
                TimerController.PushNonPersistent(MediaConstants.BLACK_FADE_TIME, () =>
                {
                    _gameCtrl._localPlayer.InstantAppearAtCell(warpPacket.posX, warpPacket.posY, false);
                    _gameCtrl._localPlayer.UpdateDirection(warpPacket.direction);
                    _gameCtrl._cameraFollow.SnapToTarget();

                    _gameCtrl._uiController.FadeInScreen();
                });
            }

            private void OnPlayerEnterRange(RCV_Packet packet)
            {
                Debug.Log("Received player enter range");
                RCV_PlayerEnterRange rcvPacket = (RCV_PlayerEnterRange)packet;

                //only create character if it's null
                Character character = _gameCtrl._blocks.GetChacter(rcvPacket.playerId);
                if (character == null)
                {
                    character = _gameCtrl._blocks.CreateCharacter(rcvPacket.playerId);

                    character._charInfo.gender = rcvPacket.gender;
                    character._charInfo.job = rcvPacket.job;
                    character._charInfo.hairstyle = rcvPacket.hairstyle;
                    //character._charInfo. = rcvPacket.topHeadgear;
                    // character._charInfo. = rcvPacket.midHeadgear;
                    // character._charInfo. = rcvPacket.lowHeadgear;

                    character.Animate();
                }
                else //otherwise update it's fields
                {
                    //Gender cannot be updated 
                    character.PlayerAnimator.ChangeJob(rcvPacket.job);
                    character.PlayerAnimator.ChangeHairstyle(rcvPacket.hairstyle);
                }

                character.status.moveSpd = rcvPacket.movementSpeed;
                character.isFriendly = true; //TODO: Can client calculate this itself or does it need to come from server?
                character.UpdateDirections(rcvPacket.direction, _gameCtrl._cameraFollow.CameraDirection);
                character.UpdateRotationFromCenter(_gameCtrl._cameraFollow.Target);
                character.SmoothAppearAtCell(rcvPacket.posX, rcvPacket.posY, rcvPacket.enterType);

                character.IsEnabled = true;
            }

            private void OnMonsterEnterRange(RCV_Packet packet)
            {
                RCV_MonsterEnterRange rcvPacket = (RCV_MonsterEnterRange)packet;

                Debug.Log("Received monster enter range ID: " + rcvPacket.instanceId);

                //only create character if it's null
                Monster monster = _gameCtrl._blocks.GetMonster(rcvPacket.instanceId);
                if (monster == null)
                {
                    monster = _gameCtrl._blocks.CreateMonster(rcvPacket.instanceId, (MonsterIDs)rcvPacket.dbId);
                    monster.Animate();
                }
                else //otherwise update it's fields
                {
                }

                monster.status.moveSpd = rcvPacket.movementSpeed;
                monster.isFriendly = false; //TOD: same as player isFriendly
                monster.UpdateDirections(rcvPacket.direction, _gameCtrl._cameraFollow.CameraDirection);
                monster.UpdateRotationFromCenter(_gameCtrl._cameraFollow.Target);

                monster.AppearAtCell(rcvPacket.posX, rcvPacket.posY, rcvPacket.enterType);

                monster.IsEnabled = true;
            }

            private void OnOtherEnterRange(RCV_Packet packet)
            {
                RCV_OtherEnterRange rcvPacket = (RCV_OtherEnterRange)packet;

                //redirect packets to it's controllers
                if (rcvPacket.blockType == BlockTypes.Npc)
                    _gameCtrl._npcController.OnNpcEnterRange(rcvPacket);
                else if (rcvPacket.blockType == BlockTypes.Item)
                    _gameCtrl._itemController.OnItemEnterRange(rcvPacket);
            }

            private void OnPlayerLeaveRange(RCV_Packet packet)
            {
                Debug.Log("Received player leave range");
                RCV_PlayerLeaveRange rcvPacket = (RCV_PlayerLeaveRange)packet;

                Character character = _gameCtrl._blocks.GetChacter(rcvPacket.playerId);

                int fadeTag = character.PlayerAnimator.Fade(Media.FadeDirection.Out);
                character.flags.IsMoving = false;
                character.flags.IsFixingPosition = false;
                character.PlayerAnimator.PlayIdleAnimation();

                if (rcvPacket.leaveType == LeaveRangeType.Teleport)
                {
                    Vector3 pos = character.transform.position;
                    EffectsAnimatorController.PlayEffect(CylinderEffectIDs.WarpOut, ref pos, null);
                }

                TimerController.PushNonPersistent(Media.MediaConstants.UNIT_FADE_TIME, () =>
                {
                    if (character.PlayerAnimator.FadeTag == fadeTag)
                        character.IsEnabled = false;
                });
            }

            private void OnMonsterLeaveRange(RCV_Packet packet)
            {
                RCV_MonsterLeaveRange pckt = (RCV_MonsterLeaveRange)packet;

                Debug.Log("Receved monster leave range ID: " + pckt.instanceId);

                Monster monster = _gameCtrl._blocks.GetMonster(pckt.instanceId);

                int fadeTag = monster.MonsterAnimator.Fade(Media.FadeDirection.Out);
                monster.flags.IsMoving = false;
                monster.flags.IsFixingPosition = false;

                if (pckt.leaveType == LeaveRangeType.Teleport)
                {
                    Vector3 pos = monster.transform.position;
                    EffectsAnimatorController.PlayEffect(CylinderEffectIDs.WarpOut, ref pos, null);
                }

                TimerController.PushNonPersistent(Media.MediaConstants.UNIT_FADE_TIME, () =>
                {
                    if (monster.MonsterAnimator.FadeTag == fadeTag)
                        monster.IsEnabled = false;
                });
            }

            private void OnOtherLeaveRange(RCV_Packet packet)
            {
                RCV_OtherLeaveRange rcvPacket = (RCV_OtherLeaveRange)packet;

                //redirect packet to it's controller
                if (rcvPacket.blockType == BlockTypes.Npc)
                    _gameCtrl._npcController.OnNpcLeaveRange(rcvPacket);
                if (rcvPacket.blockType == BlockTypes.Item)
                    _gameCtrl._itemController.OnItemLeaveRange(rcvPacket);
            }

            private void OnBlockDied(RCV_Packet packet)
            {
                RCV_BlockDied blockDied = (RCV_BlockDied)packet;

                Debug.Log("Recieved block died id: " + blockDied.blockId);

                Block block = _gameCtrl._blocks.GetBlock(blockDied.blockId);

                if (block == null) //Shouldn't happen
                    return;

                Unit unit = null;
                //server should not be sending a block died by something other than a unit
                //Characters do not fade out and are still targetable when they die
                //Everything else should fade out, free the session ID and become untargettable while playing dead animation
                if (block.BlockType == BlockTypes.Character)
                {
                    Character character = (Character)block;
                    character.PlayerAnimator.PlayDeadAnimation();
                }
                else if (block.BlockType == BlockTypes.Monster)
                {
                    Monster monster = (Monster)block;
                    unit = monster;
                    _gameCtrl._blocks.FreeMonsterId(monster); //free the session ID
                    float animDuration = monster.MonsterAnimator.PlayDeadAnimation(true); //Play dead animation and get length
                    monster.MonsterAnimator.SetEnableRaycast(false); // Become untargetable
                    TimerController.PushNonPersistent(animDuration, () => _gameCtrl._blocks.CleanupMonster(monster)); // Cleanup monster at the end of fade
                }
                else //TODO: add other unit types
                {
                }

                unit.status.currentHp = 0;
                unit.flags.IsMoving = false;
                unit.flags.IsFixingPosition = false;
            }

            private void OnPlayerLocalReceiveDamage(RCV_Packet packet)
            {
                var dmg = (RCV_PlayerLocalReceiveDamage)packet;

                Media.FloatingTextController.PlayRegularDamage(_gameCtrl._localPlayer.transform, (uint)dmg.damage, Media.FloatingTextColor.Red);

                //Apply damage and logic to stop movement if applicable

                _gameCtrl._localPlayer.PlayerAnimator.PlayReceivedDmgAnimation();
            }

            private void OnPlayerOtherReceiveDamage(RCV_Packet packet)
            {
                var dmgPacket = (RCV_PlayerOtherReceiveDamage)packet;

                Character character = _gameCtrl._blocks.GetChacter(dmgPacket.playerId);
                Media.FloatingTextController.PlayRegularDamage(character.transform, (uint)dmgPacket.damage);

                //Apply damage and logic to stop movement if applicable

                character.PlayerAnimator.PlayReceivedDmgAnimation();
            }

            private void OnMonsterReceiveDamage(RCV_Packet packet)
            {
                var dmgPacket = (RCV_MonsterReceiveDamage)packet;

                Monster monster = _gameCtrl._blocks.GetMonster(dmgPacket.monsterId);
                Media.FloatingTextController.PlayRegularDamage(monster.transform, (uint)dmgPacket.damage);

                //Apply damage and logic to stop movement if applicable

                //Skip dmg animation if monster is dead (could just be showing dead)
                if (!monster.IsDead)
                    monster.MonsterAnimator.PlayReceiveDamageAnimation();
            }

            private void RegisterPacketHandlers()
            {
                PacketDistributer.RegisterCallback(PacketIds.RCV_PlayerJobOrLevelChanged, OnPlayerJobOrLevelChange);
                PacketDistributer.RegisterCallback(PacketIds.RCV_PlayerLocalMove, OnPlayerLocalMove);
                PacketDistributer.RegisterCallback(PacketIds.RCV_PlayerOtherMove, OnPlayerOtherMove);
                PacketDistributer.RegisterCallback(PacketIds.RCV_MonsterMove, OnMonsterMove);
                PacketDistributer.RegisterCallback(PacketIds.RCV_PlayerEnterRange, OnPlayerEnterRange);
                PacketDistributer.RegisterCallback(PacketIds.RCV_MonsterEnterRange, OnMonsterEnterRange);
                PacketDistributer.RegisterCallback(PacketIds.RCV_OtherEnterRange, OnOtherEnterRange);
                PacketDistributer.RegisterCallback(PacketIds.RCV_PlayerLeaveRange, OnPlayerLeaveRange);
                PacketDistributer.RegisterCallback(PacketIds.RCV_MonsterLeaveRange, OnMonsterLeaveRange);
                PacketDistributer.RegisterCallback(PacketIds.RCV_OtherLeaveRange, OnOtherLeaveRange);
                PacketDistributer.RegisterCallback(PacketIds.RCV_BlockDied, OnBlockDied);
                PacketDistributer.RegisterCallback(PacketIds.RCV_PlayerLocalReceiveDamage, OnPlayerLocalReceiveDamage);
                PacketDistributer.RegisterCallback(PacketIds.RCV_PlayerOtherReceiveDamage, OnPlayerOtherReceiveDamage);
                PacketDistributer.RegisterCallback(PacketIds.RCV_MonsterReceiveDamage, OnMonsterReceiveDamage);
                PacketDistributer.RegisterCallback(PacketIds.RCV_PlayerLocalStopMove, OnPlayerLocalStopMove);
                PacketDistributer.RegisterCallback(PacketIds.RCV_PlayerOtherStopMove, OnPlayerOtherStopMove);
                PacketDistributer.RegisterCallback(PacketIds.RCV_MonsterStopMove, OnMonsterStopMove);
                PacketDistributer.RegisterCallback(PacketIds.RCV_PlayerWarpToCell, OnPlayerWarpToCell);
            }
        }
    }
}
