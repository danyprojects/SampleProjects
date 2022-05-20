#pragma once

#include <server/common/ServerLimits.h>
#include <server/common/MonsterId.h>
#include <server/common/MonsterDb.h>
#include <server/map_objects/Unit.h>

#include <sdk/Tick.h>
#include <sdk/SingletonUniquePtr.hpp>

#include <array>

class Monster final
	: public Unit
{
public:
	static constexpr Type BLOCK_TYPE = Type::Monster;

	Monster(MonsterId id, uint8_t _level, uint16_t spawnDataIndex);

	const auto& db() const { return MonsterDb::getMob(_monsterId); }

	const MonsterId _monsterId;
	Tribe _tribe;
	Tick _lastThinkTime;
	Tick _nextWalkTime;
	uint16_t _moveFailCount = 0;
	const uint16_t _spawnDataIndex;

	void spawn();
	bool isSpotted() const { return _spotted; }
	void markAsSpotted() { _spotted = true; }
private:
	bool _spotted = false;
	std::array<RawBuff, MAX_MONSTER_BUFFS> _buffSlots; // buffer injected to buff controller
};