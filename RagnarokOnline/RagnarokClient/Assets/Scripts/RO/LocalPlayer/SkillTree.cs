using RO.Common;
using RO.Databases;
using RO.IO;
using RO.Network;
using System.Collections.Generic;

namespace RO.LocalPlayer
{
    using LocalSkillInfo = RCV_PlayerSkillTreeReload.SkillInfo;

    public sealed partial class SkillTree
    {
        private const int INVALID_INDEX = 255;
        private InputHandler _inputHandler;
        private LocalCharacter _localCharacter;
        private Skill[] _skills = new Skill[MaxCapacity];
        private IRecycler _recycler = null;
        private Stack<Skill> _skillStack = new Stack<Skill>(Constants.MAX_PLAYER_EXTRA_SKILLS);

        public const int MaxCapacity = Constants.MAX_PLAYER_SKILLS + Constants.MAX_PLAYER_EXTRA_SKILLS;
        public int UnindexedSkillsCount { get; private set; }
        public int IndexedSkillsCount { get; private set; }
        public int PermanentSkillCount { get; private set; }

        private interface IRecycler
        {
            Skill initSkill(Skill skill, int lvl, SkillIds skillId, int skillIndex);
        }

        public SkillTree(InputHandler inputHandler, LocalCharacter localCharacter)
        {
            _inputHandler = inputHandler;
            _localCharacter = localCharacter;

            int i = 0;
            while (i++ < Constants.MAX_PLAYER_EXTRA_SKILLS)
                _skillStack.Push(new Skill(this));
        }

        // ONLY call due to a server request, returns inserted skill
        // server will already provide pre-calculated index so its never invalid
        public Skill AddExtraSkill(SkillIds skillId, int lvl, int index)
        {
            // its a new indexed skill
            if (index >= IndexedSkillsCount)
                return _skills[IndexedSkillsCount++] = _recycler.initSkill(_skillStack.Pop(), lvl, skillId, index);
            // points to existing skill so insert on end
            else
                return _skills[_skills.Length - ++UnindexedSkillsCount] = _recycler.initSkill(_skillStack.Pop(), lvl, skillId, index);
        }

        // ONLY call due to a server request, returns removed skill
        // server will pass index 255 if skill has no indexing on server
        public Skill RemoveExtraSkill(SkillIds skillId, int lvl, int index)
        {
            Skill skill = null;

            // just look for one remove it and shift array
            if (index == INVALID_INDEX)
            {
                for (int i = _skills.Length - 1; i >= _skills.Length - UnindexedSkillsCount; i--)
                {
                    if (skill != null)
                        _skills[i] = _skills[i - 1];
                    else if (_skills[i].Id == skillId && _skills[i].SkillLevel == lvl)
                    {
                        skill = _skills[i];
                        _skills[i] = _skills[i - 1];
                    }
                }
                UnindexedSkillsCount--;
            }
            else
            {
                // if it is an indexed skill that got removed we have guarantee it was
                // the only one so server pulled all entries after that one backwards

                skill = _skills[index];
                for (int i = index; i < IndexedSkillsCount; i++)
                {
                    _skills[i] = _skills[i + 1];
                    _skills[i]._skillIndex = i;
                }
            }

            _skillStack.Push(skill);
            return skill;
        }

        // Returns internal filled in tree after the reload
        public Skill[] Reload(LocalSkillInfo[] skills, int permanentSkillCount, int unindexedExtraSkillCount)
        {
            // Free extra skills if any
            for (int i = PermanentSkillCount; i < _skills.Length; i++)
            {
                if (_skills[i] != null)
                    _skillStack.Push(_skills[i]);
            }

            IndexedSkillsCount = skills.Length - unindexedExtraSkillCount;
            UnindexedSkillsCount = unindexedExtraSkillCount;
            PermanentSkillCount = permanentSkillCount;

            // Fill in indexed skills
            {
                int i = 0;
                for (; i < permanentSkillCount; i++)
                {
                    _skills[i] = _recycler.initSkill(new Skill(this), skills[i].level, skills[i].skillId, i);
                }

                // Fill in indexed extra skills
                for (; IndexedSkillsCount < permanentSkillCount; i++)
                {
                    _skills[i] = _recycler.initSkill(_skillStack.Pop(), skills[i].level, skills[i].skillId, i);
                }
            }

            // Fill in unindexed extra skills, and insert them at END of array
            {
                int i = _skills.Length - 1;
                for (; i >= _skills.Length - unindexedExtraSkillCount; i--)
                {
                    for (int k = 0; k < IndexedSkillsCount; k++)
                    {
                        // we have a match so stop search and save that index
                        if (skills[k].skillId == skills[i].skillId)
                        {
                            _skills[i] = _recycler.initSkill(_skillStack.Pop(), skills[i].level, skills[i].skillId, k);
                            break;
                        }
                    }
                }

                // clean old entries
                for (; i >= IndexedSkillsCount; i--)
                {
                    _skills[i] = null;
                }
            }

            return _skills;
        }
    }
}