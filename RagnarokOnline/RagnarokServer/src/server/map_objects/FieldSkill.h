#pragma once

#include <server/common/SkillId.h>
#include <server/common/CommonTypes.h>
#include <server/map_objects/Block.h>

#include <sdk/Tick.h>

#include <cstdint>

class Timer;
class Unit;

class FieldSkill final
	: public Block
{
public:
	static constexpr Type BLOCK_TYPE = Type::Skill;

	FieldSkill()
		: Block(Block::Type::Skill)
	{}

	uint8_t _skillLvl;
	SkillId _skillId;
	uint16_t _dmgRate;
	Tick _endTick;
	Timer* _timer;
	Unit* _src;
	FieldSkill* _nextSkill = nullptr;
};