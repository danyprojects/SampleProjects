#pragma once

#include <server/common/Job.h>
#include <server/common/Skill.h>

#include <sdk/array/ArrayView.hpp>

#include <utility>
#include <cassert>
#include <functional>

class SkillTree
{
public:
	static constexpr uint8_t INVALID_INDEX = 255;

	// use to extract unindexed skills on a reload
	class UnindexedExtraSkillReader
	{
	public:
		// returns true if values successfully read, false if no more values available
		bool next(uint8_t& lvl, Skill::Id& id);

		// returns the number of entries still available for read
		uint8_t available() const { return static_cast<uint8_t>(_available); }
	private:
		friend class SkillTree;

		UnindexedExtraSkillReader(SkillTree& tree) :_tree(tree) {};
		void refresh();

		SkillTree& _tree;
		int _available = 0;
		std::array<bool, MAX_SKILL_TREE_EXTRA_SKILLS> _dirty = { false };
	};

	int size() const { return _totalCount; }

	uint8_t getPermanentSkillCount() { return _permanentSkillCount; }

	const Skill* getSkill(uint8_t index) const
	{
		return index < _totalCount ? &_skills[index] : nullptr;
	}

	Skill* getSkill(uint8_t index)
	{
		return index < _totalCount ? &_skills[index] : nullptr;
	}

	const Skill& getSkillUnsafe(uint8_t index) const
	{
		assert(index < _totalCount);
		return _skills[index];
	}

	Skill& getSkillUnsafe(uint8_t index)
	{
		assert(index < _totalCount);
		return _skills[index];
	}

	uint8_t getSkillLevel(Skill::Id skillId) const
	{
		for (int i = 0; i < _permanentSkillCount; i++)
			if (_skills[i]._id == skillId)
				return _skillPermamentLvl[i];
		return 0;
	}

	uint8_t getPermanentSkillLevel(uint8_t index) const
	{
		return index < _permanentSkillCount ? _skillPermamentLvl[index] : 0;
	}

	uint8_t getPermanentSkillLevelUnsafe(uint8_t index) const
	{
		assert(index < _permanentSkillCount);

		return _skillPermamentLvl[index];
	}

	// return true on levelup success false otherwise
	bool levelUpPermanentSkill(uint8_t index, uint8_t increment)
	{
		if (index >= _permanentSkillCount)
			return false;

		return tryLevelUpPermanentSkill(_skillPermamentLvl.data(), index, increment);
	}

	// level up is transactional so it's an all or none levelup
	// return true on transaction success false otherwise
	bool levelUpPermanentSkills(const ArrayView<std::pair<uint8_t/*index*/, uint8_t/*increment*/>, uint8_t> array);

	// array MUST have same size has permanent skills count
	// level up is transactional so it's an all or none levelup
	// return true on transaction success false otherwise
	bool levelUpPermanentSkills(const ArrayView<uint8_t/*increment*/, uint8_t> array);

	// levels up all permanent skills to the max level
	void levelUpAllPermanentSkills();

	SkillTree(Job job);

	// clears all PERMANENT entries and reloads them
	// ALL cooldowns are reset, extraskills are maintained
	void reload(Job job);

	// appends skills of ONLY the specified job (does not recurse down dependents table)
	// ALL cooldowns are reset, extraskills are maintained
	void append(Job newJob);

	// adds entry even if its a duplicate (must add support for plagiarized skill later)
	// returns the index for the newly inserted skill
	uint8_t pushExtraSkill(SkillId skillId, int level = 1);

	// removes only first instance that matches (must add support for plagiarized skill later)
	// returns INVALID_INDEX if we removed unindexed skill or a valid index otherwise
	uint8_t popExtraSkill(SkillId skillId, int level = 1);

	// fetches all the data for the currently unindexed extra skills, usefull when reloading tree
	UnindexedExtraSkillReader& getUnindexedExtraSkillReader()
	{
		_unindexedSkillReader.refresh();
		return _unindexedSkillReader;
	}

private:
	void loadTree(Job job);
	void loadJob(Job job);
	bool tryLevelUpPermanentSkill(uint8_t* levelUpCache, uint8_t index, uint8_t increment);
	void refreshSkillPermanentLvls(uint8_t* levelUpCache);

	UnindexedExtraSkillReader _unindexedSkillReader;
	std::array<Skill, MAX_SKILL_TREE_SKILLS> _skills; // extra skills always at end so iterate on reverse
	std::array<SkillId, MAX_SKILL_TREE_EXTRA_SKILLS> _extraSkillId;
	std::array<uint8_t, MAX_SKILL_TREE_EXTRA_SKILLS> _extraSkillLvl;
	std::array<uint8_t, MAX_SKILL_TREE_PERMANENT_SKILLS> _skillPermamentLvl;
	uint8_t _totalCount = 0;
	uint8_t _extraCount = 0;
	uint8_t _permanentSkillCount = 0;
	Job _job;
};