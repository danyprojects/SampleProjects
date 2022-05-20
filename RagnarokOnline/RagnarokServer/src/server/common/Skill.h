#pragma once

#include <server/common/SkillId.h>
#include <server/common/SkillDb.h>

#include <sdk/Tick.h>

#include <cstdint>

class Skill final
{
public:
	typedef SkillId Id;

	enum class Flag : uint8_t
	{
		Permanent,
		Temporary,
		Plagiarized,
		Unused,     //stuff like novice trick dead
		Granted
	};

	Skill() = default;

	Skill(Id id)
		: _id(id)
	{ }

	Skill(Id id, int level, Flag flag)
		:_id(id)
		, _lvl(level)
		, _flag(flag)
	{ }

	const SkillDb& db() const
	{
		return SkillDb::getSkill(_id);
	}

	uint16_t dbCooldown(int level) const
	{
		return db()._cooldown[level - 1];
	}

	Tick _cooldown;
	uint8_t _lvl = 0;
	Flag _flag = Flag::Unused;
	Id _id = Id::None;
};