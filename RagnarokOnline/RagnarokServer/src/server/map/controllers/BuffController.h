#pragma once

#include <server/common/BuffId.h>
#include <server/map_objects/Unit.h>

#include <sdk/Tick.h>
#include <sdk/enum_cast.hpp>

class Map;

class BuffController final
{
public:
	BuffController::BuffController(Map &map)
		:_map(map)
	{ }

	bool applyBuff(Unit& target, BuffId buffId, int buffLvl, Tick deadline, BuffOverwriteType overwriteType = BuffOverwriteType::OnGreaterEqualLevel);

	bool applyBuff(Unit& target, BuffId buffId, int buffLvl, BuffOverwriteType overwriteType = BuffOverwriteType::OnGreaterEqualLevel);

	bool removeBuff(Unit& target, BuffId buffId);

	const Buff& getUnitBuff(Unit& target, BuffId buffId);

	void checkUnitBuffTicks(Unit& target);

	//Add methods for removing all on death and all on dispell

private:
	typedef bool(BuffController::* BuffApplyHandler)(Unit& target, int buffLevel, Tick deadline, BuffOverwriteType overwriteType);
	typedef bool(BuffController::* BuffRemoveHandler)(Unit& target, const uint8_t buffIndex);

	// *********** General methods
	//Perhaps these will change when there is proper status change packets
	void notifyUnitBuffApply(const Unit& unit, const Buff& buff);

	void notifyUnitBuffRemove(const Unit& unit, BuffId buffId);

	//This will return the reference to the buff so that if the is a buff that wants to deal with stacks,
	//the other buffs do not pay for it
	Buff& addUnitBuff(Unit& target, BuffId buffId, int buffLvl, Tick deadline);

	//Same as add unit in terms of return type
	Buff& rescheduleUnitBuff(Unit& target, BuffId buffId, int buffLvl, Tick newDeadline, BuffOverwriteType overwriteType);

	void removeUnitBuff(Unit& target, BuffId buffId);

	void removeUnitBuff(Unit& target, uint8_t buffIndex);

	//Implemented here to guarantee inlining
	void removeUnitBuff(Unit& target, BuffId buffId, uint8_t buffIndex)
	{
		if (buffIndex == Unit::StatusChange::INVALID_INDEX)
			removeUnitBuff(target, buffId);
		else
			removeUnitBuff(target, buffIndex);
	}

	bool removeBuff(Unit& target, BuffId buffId, uint8_t buffIndex);

	// *********** Buff handlers
	bool applyEnergyCoat(Unit& target, int buffLvl, Tick deadline, BuffOverwriteType overwriteType);

	bool removeEnergyCoat(Unit& target, const uint8_t buffIndex);

	Map& _map;
};