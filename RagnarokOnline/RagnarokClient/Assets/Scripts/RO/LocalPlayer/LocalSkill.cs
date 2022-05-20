using Algorithms;
using RO.Common;
using RO.Databases;
using RO.MapObjects;
using RO.Media;
using System;
using UnityEngine;

namespace RO.LocalPlayer
{
    using SkillFlag = SkillDb.SkillFlag;

    public sealed partial class SkillTree
    {
        //Only SkillTree create skills. This helps us guarantee that the OnSkillSelect callback is correctely set

        public sealed class Skill
        {
            public SkillIds Id { get; private set; }
            public Action<int/*level*/, bool /*quickcast*/> OnSkillSelect { get; private set; }
            public int SkillLevel { get; private set; }
            public double Cooldown
            {
                get { return Cooldown; }
                set { _tree._skills[_skillIndex].Cooldown = Cooldown = value; } // propagate cooldown update if needed
            }
            public SkillDb Db { get; private set; }

            public int MaxLevel { get { return Db.MaxLevel; } }

            public bool IsPassive { get { return (Db.SkillFlags & SkillFlag.Passive) == SkillFlag.Passive; } }

            public bool IsSelectableLvl { get { return (Db.SkillFlags & SkillFlag.SelectLvl) == SkillFlag.SelectLvl; } }

            public bool IsQuestSkill { get { return (Db.SkillFlags & SkillFlag.QuestSkill) == SkillFlag.QuestSkill; } }

            public bool HasFlags(SkillFlag flags)
            {
                return (Db.SkillFlags & flags) == flags;
            }

            //Only meant to be updated by SkillTree
            public int _skillIndex { private get; set; }
            private readonly SkillTree _tree;

            private struct Recycler : IRecycler
            {
                public Skill initSkill(Skill skill, int lvl, SkillIds skillId, int skillIndex)
                {
                    skill.Init(lvl, skillId, skillIndex);
                    return skill;
                }
            }

            private void Init(int lvl, SkillIds skillId, int skillIndex)
            {
                _skillIndex = skillIndex;
                Id = skillId;
                SkillLevel = lvl;
                Db = SkillDb.GetSkill(Id);

                //Check the skill type because the callback on select changes accordingly
                if (skillId < SkillIds.SingleTargetStart)
                    OnSkillSelect = OnRequestCoordinatesFromMouse;
                else if (skillId < SkillIds.NoTargetStart)
                    OnSkillSelect = OnRequestTargetFromMouse;
                else
                    OnSkillSelect = SendSelfTargetSkill;
            }

            public Skill(SkillTree tree)
            {
                if (tree._recycler == null)
                    tree._recycler = new Recycler();

                _tree = tree;
            }

            // Should ONLY be changed by via a server request
            public void IncrementSkillLevel(int increment)
            {
                SkillLevel += increment;
            }

            private void OnRequestTargetFromMouse(int lvl, bool quickCast)
            {
                CursorAnimator.SetAnimation(CursorAnimator.Animations.Cast);
                CursorAnimator.Level = (CursorAnimator.Levels)lvl;

                int layers = LayerMasks.Monster | LayerMasks.Player;
                _tree._inputHandler.RequestMouseScroll((int delta) => OnMouseScroll(delta)); //This needs to be first in case request is canceled immediatly
                _tree._inputHandler.RequestClickTarget((target) => SendSingleTargetSkill(target), quickCast, layers, OnRequestCanceled);
            }

            private void OnRequestCoordinatesFromMouse(int lvl, bool quickCast)
            {
                CursorAnimator.SetAnimation(CursorAnimator.Animations.Cast);
                CursorAnimator.Level = (CursorAnimator.Levels)lvl;

                _tree._inputHandler.RequestMouseScroll((int delta) => OnMouseScroll(delta)); //This needs to be first in case request is canceled immediatly
                _tree._inputHandler.RequestClickCoordinates((coordinates) => SendAreaOfEffectSkill(coordinates), quickCast, OnRequestCanceled);
            }

            //Callbacks triggered through input handler
            private void SendSingleTargetSkill(Block target)
            {
                CursorAnimator.UnsetAnimation(CursorAnimator.Animations.Cast);
                _tree._inputHandler.RequestMouseScroll(null);

                if (target == null || !Pathfinder.IsInLineOfSight(ref _tree._localCharacter.position, ref target.position))
                {
                    CursorAnimator.Level = CursorAnimator.Levels.Zero;
                    return;
                }

                Network.SND_CastSingleTargetSkill packet = new Network.SND_CastSingleTargetSkill
                {
                    skillIndex = (byte)_skillIndex,
                    skillLevel = (byte)CursorAnimator.Level,
                    instanceId = (short)target.SessionId
                };

                CursorAnimator.Level = CursorAnimator.Levels.Zero;
                Network.NetworkController.SendPacket(packet);
            }

            private void SendAreaOfEffectSkill(Vector2Int coordinates)
            {
                CursorAnimator.UnsetAnimation(CursorAnimator.Animations.Cast);
                _tree._inputHandler.RequestMouseScroll(null);

                //Don't shoot if it's not in line of sight
                if (!Pathfinder.IsInLineOfSight(ref _tree._localCharacter.position, ref coordinates))
                {
                    CursorAnimator.Level = CursorAnimator.Levels.Zero;
                    return;
                }

                Network.SND_CastAreaOfEffectSkill packet = new Network.SND_CastAreaOfEffectSkill
                {
                    skillIndex = (byte)_skillIndex,
                    skillLevel = (byte)CursorAnimator.Level,
                    destinationX = (short)coordinates.x,
                    destinationY = (short)coordinates.y
                };

                CursorAnimator.Level = CursorAnimator.Levels.Zero;
                Network.NetworkController.SendPacket(packet);
            }

            private void SendSelfTargetSkill(int lvl, bool quickCast)
            {
                Network.SND_CastSelfTargetSkill packet = new Network.SND_CastSelfTargetSkill
                {
                    skillIndex = (byte)_skillIndex,
                    skillLevel = (byte)lvl
                };

                Network.NetworkController.SendPacket(packet);
            }

            private void OnMouseScroll(int delta)
            {
                //This will change the number on the mouse cursor to be according to the skill level that is being used
                int level = (int)CursorAnimator.Level + delta;
                level = level > SkillLevel ? SkillLevel : level < Constants.MIN_SKILL_LEVEL ? Constants.MIN_SKILL_LEVEL : level;
                CursorAnimator.Level = (CursorAnimator.Levels)level;
            }

            private void OnRequestCanceled()
            {
                CursorAnimator.UnsetAnimation(CursorAnimator.Animations.Cast);
                CursorAnimator.Level = CursorAnimator.Levels.Zero;
                _tree._inputHandler.RequestMouseScroll(null);
            }
        }
    }
}