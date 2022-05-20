#include "SkillTree.h"

#include <server/player_extensions/SkillTreeDb.h>

#include <array>
#include <cassert>


void SkillTree::refreshSkillPermanentLvls(uint8_t* levelUpCache)
{
	std::memcpy(_skillPermamentLvl.data(), levelUpCache, _skillPermamentLvl.size());
	for (int i = 0; i < _permanentSkillCount; i++)
	{
		if (_skills[i]._lvl < levelUpCache[i])
			_skills[i]._lvl = levelUpCache[i];
	}
}

// it will update levelUpCache if successfull
bool SkillTree::tryLevelUpPermanentSkill(uint8_t* levelUpCache, uint8_t index, uint8_t increment)
{
	// no point checking further
	if (increment > 10 || levelUpCache[index] + increment > _skills[index].db()._maxLevel)
		return false;

	// if skill is at level 0 check its requirements
	if (levelUpCache[index] == 0)
	{
		for (const auto& req : _skills[index].db().skillTreeEntry(_job).requirements())
		{
			if (levelUpCache[req._dependencyIndex] < req._lvl)
				return false;
		}
	}

	// if we make it here skill was leveled up
	levelUpCache[index] += increment;
	return true;
}

bool SkillTree::levelUpPermanentSkills(const ArrayView<uint8_t, uint8_t> array)
{
	if (array.size() != _permanentSkillCount)
		return false;

	uint8_t levelUpCache[MAX_SKILL_TREE_PERMANENT_SKILLS];
	std::memcpy(levelUpCache, _skillPermamentLvl.data(), sizeof(levelUpCache));

	for (int i = 0; i < array.size(); i++)
	{
		if (!tryLevelUpPermanentSkill(levelUpCache, i, array[i]))
			return false;
	}

	// We made it so copy permanent skills back and update usable level
	refreshSkillPermanentLvls(levelUpCache);
	return true;
}

bool SkillTree::levelUpPermanentSkills(const ArrayView<std::pair<uint8_t, uint8_t>, uint8_t> array)
{
	uint8_t levelUpCache[MAX_SKILL_TREE_PERMANENT_SKILLS];
	std::memcpy(levelUpCache, _skillPermamentLvl.data(), sizeof(levelUpCache));

	for (int i = 0; i < array.size(); i++)
	{
		auto index = array[i].first;

		if (index >= _permanentSkillCount
			|| !tryLevelUpPermanentSkill(levelUpCache, index, array[i].second))
			return false;
	}

	// We made it so copy permanent skills back and update usable level
	refreshSkillPermanentLvls(levelUpCache);
	return true;
}

void SkillTree::levelUpAllPermanentSkills()
{
	for (int i = 0; i < _permanentSkillCount; i++)
	{
		_skillPermamentLvl[i] = _skills[i].db()._maxLevel;
		_skills[i]._lvl = _skillPermamentLvl[i];
	}
}

SkillTree::SkillTree(Job job)
	:_unindexedSkillReader(*this)
	,_job(job)
{
	loadTree(job);
	_permanentSkillCount = _totalCount;
}

void SkillTree::reload(Job job)
{
	_job = job;
	_totalCount = 0;
	loadTree(job);
	_permanentSkillCount = _totalCount;

	if (_extraCount)
	{
		//search for an already existing entry, update level if necessary
		for (int i = 0; i < _extraCount; i++)
		{
			bool found = false;
			for (int j = 0; j < _totalCount; j++)
			{
				if (_skills[j]._id == _extraSkillId[i])
				{
					if (_skills[j]._lvl < _extraSkillLvl[i])
					{
						_skills[j]._lvl = _extraSkillLvl[i];
					}

					found = true;
					break;
				}
			}

			if (!found) //create entry
				_skills[_totalCount++] = Skill(_extraSkillId[i], _extraSkillLvl[i], Skill::Flag::Temporary);
		}
	}
}

void SkillTree::append(Job newJob)
{
	_job = newJob;

	if (!_extraCount)
	{
		loadJob(newJob);
		return;
	}

	int temporarySkillsIndex = _totalCount - 1;
	while (_skills[temporarySkillsIndex]._flag == Skill::Flag::Temporary)
		temporarySkillsIndex--;

	_totalCount = temporarySkillsIndex + 1; //trash temporary entries
	loadJob(newJob);
	_permanentSkillCount = _totalCount;

	//search for an already existing entry, update level if necessary
	//we only need to search the new job skills
	for (int i = 0; i < _extraCount; i++)
	{
		bool found = false;
		for (int j = temporarySkillsIndex + 1; j < _totalCount; j++)
		{
			if (_skills[j]._id == _extraSkillId[i])
			{
				if (_skills[j]._lvl < _extraSkillLvl[i])
				{
					_skills[j]._lvl = _extraSkillLvl[i];
				}

				found = true;
				break;
			}
		}

		if (!found) //create entry
			_skills[_totalCount++] = Skill(_extraSkillId[i], _extraSkillLvl[i], Skill::Flag::Temporary);
	}
}

uint8_t SkillTree::pushExtraSkill(SkillId skillId, int level)
{
	assert(_extraCount < _extraSkillId.size());

	_extraSkillId[_extraCount]  = skillId;
	_extraSkillLvl[_extraCount] = level;
	_extraCount++;

	bool found = false;
	int i = 0;
	for (; i < _totalCount; i++) //search for an already existing entry, update level if necessary
	{
		if (_skills[i]._id == skillId)
		{
			if (_skills[i]._lvl < level)
			{
				_skills[i]._lvl = level;
			}

			found = true;
			break;
		}
	}

	if (!found)
		_skills[_totalCount++] = Skill(skillId, level, Skill::Flag::Temporary);

	return static_cast<uint8_t>(i);
}

uint8_t SkillTree::popExtraSkill(SkillId skillId, int level)
{
	//remove from extra arrays
	for (int i = 0; i < _extraCount; i++)
	{
		if (_extraSkillId[i] == skillId && _extraSkillLvl[i] == level)
		{
			for (; i < _extraCount; i++)
			{
				_extraSkillId[i] = _extraSkillId[i + 1];
				_extraSkillLvl[i] = _extraSkillLvl[i + 1];
			}

			_extraCount--;
			break;
		}
	}

	//calc next maxlvl
	int maxLvl = 0;
	for (int i = 0; i < _extraCount; i++)
	{
		if (_extraSkillId[i] == skillId && _extraSkillLvl[i] > maxLvl)
			maxLvl = _extraSkillLvl[i];
	}

	//search for entry in permanent entries and see if we must decrease the level
	for (int i = 0; i < _permanentSkillCount; i++)
	{
		Skill& skill = _skills[i];

		if (skill._id == skillId)
		{
			if (_skillPermamentLvl[i] > maxLvl)
				maxLvl = _skillPermamentLvl[i];

			skill._lvl = maxLvl;

			return INVALID_INDEX;
		}
	}

	//there is no permanent entry for this skill and this is last extra skill for this id, so search and remove it
	if (maxLvl == 0)
	{
		for (int i = _permanentSkillCount; i < _totalCount; i++)
		{
			Skill& skill = _skills[i];

			if (skill._id != skillId)
				continue;

			for (int k = i; k < _totalCount; k++)
				_skills[i] = _skills[i + 1];

			_totalCount--;
			return static_cast<uint8_t>(i);
		}
	}

	assert(false); //we should never allow try to remove non existing item
}

void SkillTree::loadTree(Job job)
{
	auto dependentJob = job.getDependent();
	if (dependentJob != job)
		loadTree(dependentJob);

	loadJob(job);
}

void SkillTree::loadJob(Job job)
{
	for (const auto& skill : SkillTreeDb::getSkillTree(job))
	{
		_skillPermamentLvl[_totalCount] = 0;
		_skills[_totalCount++] = Skill(skill._id, 0, Skill::Flag::Permanent);
	}
}

bool SkillTree::UnindexedExtraSkillReader::next(uint8_t& lvl, SkillId& id)
{
	if (_available > 0)
	{
		while (!_dirty[--_available])
		{
			lvl = _tree._extraSkillLvl[_available];
			id = _tree._extraSkillId[_available];

			return true;
		}
	}

	return false;
}

void SkillTree::UnindexedExtraSkillReader::refresh()
{
	std::memset(_dirty.data(), false, sizeof(_dirty));
	_available = _tree._extraCount;

	auto& tree = _tree;
	// iterate all extra skills on main skilltree
	for (int i = tree._permanentSkillCount; i < tree._totalCount; i++)
	{
		for (int j = 0; j < tree._extraCount; j++)
		{
			// if we found extra skill in skill tree and it has a match on main skilltree,
			// then its considered an indexed skill and we remove it from list of extra skills
			if (tree._skills[i]._id == tree._extraSkillId[j] && tree._skills[i]._lvl == tree._extraSkillLvl[j])
			{
				_dirty[j] = true;
				_available--;
				break;
			}
		}
	}
}