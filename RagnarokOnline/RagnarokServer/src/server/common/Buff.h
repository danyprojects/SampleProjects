#pragma once

#include <server/common/BuffDb.h>
#include <server/common/BuffId.h>

#include <sdk/Tick.h>

struct Buff final
{
	typedef BuffId Id;
	static constexpr uint8_t INVALID_INDEX = UINT8_MAX;

	Tick deadline = Tick::MAX();
	Id buffId;
	uint8_t buffLvl;
	uint8_t stacks;

private:
	friend class BuffController;
	friend class Unit;
	uint8_t nextBuff = INVALID_INDEX;
	uint8_t previousBuff = INVALID_INDEX;
	uint8_t nextTick = INVALID_INDEX;
	uint8_t previousTick = INVALID_INDEX;

	// If buff limit becomes a problem discuss either add more size or add another 2 bytes
	// to track history of buffs to do a circular array

public:
	const BuffDb& db() const
	{
		return BuffDb::getBuff(buffId);
	}
};

enum class BuffOverwriteType : uint8_t
{
	Always = 0,
	OnGreaterEqualLevel,
	OnLesserEqualLevel,
	Never,
};