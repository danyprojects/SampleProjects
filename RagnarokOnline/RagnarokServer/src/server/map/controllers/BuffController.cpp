#include "BuffController.h"

#include <server/map/Map.h>
#include <server/map_objects/Unit.h>
#include <server/network/Packets.h>

#include <cassert>

using Packet = packet::Packet;

bool BuffController::removeBuff(Unit& target, BuffId buffId)
{
	return removeBuff(target, buffId, Unit::StatusChange::INVALID_INDEX);
}

void BuffController::checkUnitBuffTicks(Unit& target)
{
	auto& statusChange = target._statusChange;
	auto& buffSlots = statusChange._buffSlots;
	//_smallestTick will always have a valid ID and will be properly assigned after a removeUnitBuff
	while (buffSlots[statusChange._smallestTick].deadline <= _map._currentTick)
		removeBuff(target, buffSlots[statusChange._smallestTick].buffId, statusChange._smallestTick);
}

const Buff& BuffController::getUnitBuff(Unit& target, BuffId buffId)
{
	//This is supposed to be called from a context that already checks if buff is active
	assert(target._statusChange.buffs[enum_cast(buffId)] == true);

	auto& statusChange = target._statusChange;
	auto& buffSlots = target._statusChange._buffSlots;

	//The rescheduleBuff list will not change as it's not sorted so don't make this a reference
	auto firstBuff = statusChange._categoryLookups[BuffDb::getBuff(buffId)._category];

	//iterate through all buffs in the rescheduleBuff sublist to look for buff id
	while (buffSlots[firstBuff].buffId != buffId)
		firstBuff = buffSlots[firstBuff].nextBuff;

	return buffSlots[firstBuff];
}

//	******* General methods
void BuffController::notifyUnitBuffApply(const Unit& unit, const Buff& buff)
{
	if (unit._type == BlockType::Character)
	{
		uint32_t duration = buff.deadline == Tick::MAX() ? Tick::MAX().count() : (buff.deadline - _map._currentTick);
		_map.writePacket(unit._id, packet::SND_PlayerBuffApply(buff.buffId, duration));
	}
}

void BuffController::notifyUnitBuffRemove(const Unit& unit, BuffId buffId)
{
	if (unit._type == BlockType::Character)		
		_map.writePacket(unit._id, packet::SND_PlayerBuffRemove(buffId));
}

//Add unit buff should only be called when the buff is NOT active 
Buff& BuffController::addUnitBuff(Unit& target, BuffId buffId, int buffLvl, Tick deadline)
{
	//The buff must be inactive if the reschedule was called 
	assert(target._statusChange.buffs[enum_cast(buffId)] == false);

	auto& statusChange = target._statusChange;
	auto& buffSlots = target._statusChange._buffSlots;

	statusChange.buffs[enum_cast(buffId)] = true;

	//Get a free buff slot. Always assume there are enough buff slots
	int newBuffSlot = statusChange._nextFreeBuff;
	assert(newBuffSlot != Unit::StatusChange::INVALID_INDEX);

	statusChange._nextFreeBuff = buffSlots[newBuffSlot].nextBuff;

	//Fill the buff
	Buff& buff = buffSlots[newBuffSlot];
	buff.buffId = buffId;
	buff.buffLvl = buffLvl;
	buff.deadline = deadline;

	//Buff needs to be inserted in the tick list sorted by ascending order of ticks. 
	//If the new deadline is greater than last inserted tick, which should be common, then start searching at last inserted tick
	//Otherwise start searching at smallest tick
	int index = deadline >= buffSlots[statusChange._lastInsertedTick].deadline ? 
								statusChange._lastInsertedTick : statusChange._smallestTick;
	assert(index != Unit::StatusChange::INVALID_INDEX);

	//Last inserted can now point to new buff slot
	statusChange._lastInsertedTick = newBuffSlot;

	//1 or more buffs, most common use case
	if (statusChange._buffCount > 0)
	{
		//Find the current active buff with a deadline greater or equal than the new buff
		//If no buff with larger deadline is found then index will be pointing to the tail and we'll want to insert after it
		while(buffSlots[index].nextTick != Unit::StatusChange::INVALID_INDEX)
		{
			if (deadline <= buffSlots[index].deadline)
				break;
			index = buffSlots[index].nextTick;
		}

		//Check if we exited the for due to deadline condition being true
		//If so, we want to insert right before the buff found	so both next and previous are valid to use
		if(deadline <= buffSlots[index].deadline) 		
		{
			buff.nextTick = index;
			buff.previousTick = buffSlots[index].previousTick;
		}
		else //We want to insert as the tail
		{
			buff.nextTick = Unit::StatusChange::INVALID_INDEX;
			buff.previousTick = index;
		}

		//The old next and previous should now point to new buff if they are valid
		if(buff.nextTick != Unit::StatusChange::INVALID_INDEX)
			buffSlots[buff.nextTick].previousTick = newBuffSlot;
		//If previous tick is invalid then new buff is the smallest tick in the list, otherwise we inserted somewhere in the array
		//If not invalid, make the previous point to new buff. If it's the smallest, make the smallest tick point to new buff
		if(buff.previousTick != Unit::StatusChange::INVALID_INDEX)
			buffSlots[buff.previousTick].nextTick = newBuffSlot;
		else
			statusChange._smallestTick = newBuffSlot;
	}
	else // no buffs
	{
		//new buff shouldn't point to anything since it's the only buff in the queue
		buff.nextTick = Unit::StatusChange::INVALID_INDEX;
		buff.previousTick = Unit::StatusChange::INVALID_INDEX;

		//the new tick head
		statusChange._smallestTick = newBuffSlot;
	}

	//Insert buff in it's category sublist for optimized lookups in the future
	auto& lookup = statusChange._categoryLookups[buff.db()._category];

	if (lookup != Unit::StatusChange::INVALID_INDEX) //There are buffs in the category, should be most common case
	{
		//Make new buff point to old head and make head point to new buff
		buff.nextBuff = lookup;
		buffSlots[lookup].previousBuff = newBuffSlot;
	}
	else //No buffs in the category
	{
		//Make new buff point to nowhere in next and previous
		buff.nextBuff = Unit::StatusChange::INVALID_INDEX;
		buff.previousBuff = Unit::StatusChange::INVALID_INDEX;
	}

	//Make head of the category point to new buff 
	lookup = newBuffSlot;
	statusChange._buffCount++;

	return buff;
}

//Reschedule unit buff should only be called when the buff IS ACTIVE 
Buff& BuffController::rescheduleUnitBuff(Unit& target, BuffId buffId, int buffLvl, Tick newDeadline, BuffOverwriteType overwriteType)
{
	//The buff must be active if the reschedule was called 
	assert(target._statusChange.buffs[enum_cast(buffId)] == true);

	auto& statusChange = target._statusChange;
	auto& buffSlots = target._statusChange._buffSlots;

	//The rescheduleBuff list will not change as it's not sorted so don't make this a reference
	auto rescheduleBuff = statusChange._categoryLookups[BuffDb::getBuff(buffId)._category];
	assert(rescheduleBuff != Unit::StatusChange::INVALID_INDEX); //A reschedule should never be called without a buff in the category
		
	//iterate through all buffs in the rescheduleBuff sublist to look for buff id
	while(buffSlots[rescheduleBuff].buffId != buffId)
		rescheduleBuff = buffSlots[rescheduleBuff].nextBuff;

	auto& buff = buffSlots[rescheduleBuff];

	//shouldn't happen but if it does there is nothing to do
	if (buff.deadline == newDeadline)
		return buff;

	//Still check the never here so in the end we still return the buff
	//Check other overwrite conditions as well
	if(overwriteType == BuffOverwriteType::Never
		|| (overwriteType == BuffOverwriteType::OnGreaterEqualLevel && buff.buffLvl < buffLvl) 
		|| (overwriteType == BuffOverwriteType::OnLesserEqualLevel && buff.buffLvl > buffLvl))
		return buff;

	//Now we need to re-sort its tick
	//Most of the time the new deadline should be greater than old deadline
	int index = buff.deadline <= newDeadline ? rescheduleBuff : statusChange._smallestTick;
		
	//Find the current active buff with a deadline greater or equal than the new deadline
	while (buffSlots[index].nextTick != Unit::StatusChange::INVALID_INDEX)
	{
		if (newDeadline <= buffSlots[index].deadline)
			break;
		index = buffSlots[index].nextTick;
	}

	buff.deadline = newDeadline;

	//If the found index is the same as the buff we're rescheduling, nothing needs to be done since buff is already on right position
	if (index == rescheduleBuff)
		return buff;

	//If smallest tick was poiting to rescheduleBuff and we know we're sending it to a different position. Make it point to "old" next
	if (statusChange._smallestTick == rescheduleBuff)
		statusChange._smallestTick = buff.nextTick;

	//Remove the buff from it's old position in the tick list
	if (buff.previousTick != Unit::StatusChange::INVALID_INDEX) //buff is not head
		buffSlots[buff.previousTick].nextTick = buff.nextTick;
	if (buff.nextTick != Unit::StatusChange::INVALID_INDEX) // buff is not tail
		buffSlots[buff.nextTick].previousTick = buff.previousTick;

	//This part is the same as the add new buff part.
	//Point the buff to the new next and previous
	if (buff.deadline <= buffSlots[index].deadline)
	{
		buff.nextTick = index;
		buff.previousTick = buffSlots[index].previousTick;
	}
	else //We want to insert as the tail
	{
		buff.nextTick = Unit::StatusChange::INVALID_INDEX;
		buff.previousTick = index;
	}

	//New next and previous should point to rescheduled buff and also assign smallest tick if rescheduled is head
	if (buff.nextTick != Unit::StatusChange::INVALID_INDEX)
		buffSlots[buff.nextTick].previousTick = rescheduleBuff;
	if (buff.previousTick != Unit::StatusChange::INVALID_INDEX)
		buffSlots[buff.previousTick].nextTick = rescheduleBuff;
	else
		statusChange._smallestTick = rescheduleBuff;

	return buff;
}

void BuffController::removeUnitBuff(Unit& target, BuffId buffId)
{
	//The buff must be active if the reschedule was called 
	assert(target._statusChange.buffs[enum_cast(buffId)] == true);

	//Buff flag will be changed in other removeUnitBuff overload

	//The rescheduleBuff list will not change as it's not sorted so don't make this a reference
	auto removeBuffIndex = target._statusChange._categoryLookups[BuffDb::getBuff(buffId)._category];
	assert(removeBuffIndex != Unit::StatusChange::INVALID_INDEX); //A Remove should never be called without a buff in the category

	//iterate through all buffs in the removeBuff sublist to look for buff id
	while (target._statusChange._buffSlots[removeBuffIndex].buffId != buffId)
		removeBuffIndex = target._statusChange._buffSlots[removeBuffIndex].nextBuff;
		
	//then call the overloaded remove which takes the index
	removeUnitBuff(target, removeBuffIndex);
}

void BuffController::removeUnitBuff(Unit& target, uint8_t buffIndex)
{
	//The buff must be active if the reschedule was called 
	auto& buff = target._statusChange._buffSlots[buffIndex];
	auto& statusChange = target._statusChange;

	assert(statusChange.buffs[enum_cast(buff.buffId)] == true);
	statusChange.buffs[enum_cast(buff.buffId)] = false;

	auto& buffSlots = statusChange._buffSlots;

	//TODO: find a way to not repeat the check buff.nextTick != Unit::StatusChange::INVALID_INDEX and still do the same operation
	//Remove the buff from it's old position in the tick list
	if (buff.previousTick != Unit::StatusChange::INVALID_INDEX) //buff is not head in tick list
		buffSlots[buff.previousTick].nextTick = buff.nextTick;
	else //Buff did not have a previous so it's the smallest tick
	{
		//If it has a next, make smallest tick point to it, otherwise invalidate smallest tick
		if (buff.nextTick != Unit::StatusChange::INVALID_INDEX)
			statusChange._smallestTick = buff.nextTick;
		else
			statusChange._smallestTick = statusChange._nextFreeBuff; //Next free buff will always have a tick of MAX()
	}
	if (buff.nextTick != Unit::StatusChange::INVALID_INDEX) // buff is not tail in tick list
		buffSlots[buff.nextTick].previousTick = buff.previousTick;

	//If last inserted tick was pointing to buff being removed, invalidate it by setting to smallest tick
	if (statusChange._lastInsertedTick == buffIndex)
		statusChange._lastInsertedTick = statusChange._smallestTick;

	//We dont care about the values of next tick and previous tick now as the buff will not be in use

	//Remove buff from the category list
	auto& lookup = statusChange._categoryLookups[buff.db()._category];

	if (buff.previousBuff != Unit::StatusChange::INVALID_INDEX) //if buff has a valid previous make it point to buffnext
		buffSlots[buff.previousBuff].nextBuff = buff.nextBuff;
	if (buff.nextBuff != Unit::StatusChange::INVALID_INDEX)	//if buff has a valid next make it point to buff previous and 
		buffSlots[buff.nextBuff].previousBuff = buff.previousBuff;
	if (lookup == buffIndex) //If buff was the head of the category
		lookup = buff.nextBuff;

	//Add the buff to the free buff list by making it the new head
	buff.previousBuff = Unit::StatusChange::INVALID_INDEX;
	buff.nextBuff = statusChange._nextFreeBuff;
	statusChange._nextFreeBuff = buffIndex;

	buff.deadline = Tick::MAX();

	statusChange._buffCount--;
}

// ********* Buff handlers
bool BuffController::applyEnergyCoat(Unit& target, int buffLvl, Tick deadline, BuffOverwriteType overwriteType)
{
	
	const Buff *buff;
	if (target._statusChange.buffs[enum_cast(BuffId::EnergyCoat)])
		buff = &rescheduleUnitBuff(target, BuffId::EnergyCoat, buffLvl, deadline, overwriteType);
	else
		buff = &addUnitBuff(target, BuffId::EnergyCoat, buffLvl, deadline);
		
	notifyUnitBuffApply(target, *buff);
	return true;
}

bool BuffController::removeEnergyCoat(Unit& target, const uint8_t buffIndex)
{
	assert(target._statusChange.buffs[enum_cast(BuffId::EnergyCoat)]);

	removeUnitBuff(target, BuffId::EnergyCoat, buffIndex);

	notifyUnitBuffRemove(target, BuffId::EnergyCoat);
	return true;
}


//Dispatch methods
bool BuffController::applyBuff(Unit& target, BuffId buffId, int buffLvl, BuffOverwriteType overwriteType)
{
	Tick deadline = _map._currentTick + BuffDb::getBuff(buffId).getDuration(buffLvl);
	return applyBuff(target, buffId, buffLvl, deadline, overwriteType);
}

bool BuffController::applyBuff(Unit& target, BuffId buffId, int buffLvl, Tick deadline, BuffOverwriteType overwriteType)
{
	using ArrayType = std::array<BuffController::BuffApplyHandler, enum_cast(BuffId::Last) + 1>;

	static constexpr ArrayType _table = []() constexpr
	{
		ArrayType table = {};

		//Mage Skills
		table[enum_cast(BuffId::EnergyCoat)] = &BuffController::applyEnergyCoat;
		return table;
	}();

	static_assert([]() constexpr
		{
#ifdef NDEBUG
			for (auto&& v : _table)
				if (v == nullptr)
					return false;
#endif
			return true;
		}());

	return (this->*_table[enum_cast(buffId)])(target, buffLvl, deadline, overwriteType);
}

bool BuffController::removeBuff(Unit& target, BuffId buffId, uint8_t buffIndex)
{
	using ArrayType = std::array<BuffController::BuffRemoveHandler, enum_cast(BuffId::Last) + 1>;

	static constexpr ArrayType _table = []() constexpr
	{
		ArrayType table = {};

		//Mage Skills
		table[enum_cast(BuffId::EnergyCoat)] = &BuffController::removeEnergyCoat;
		return table;
	}();

	static_assert([]() constexpr
		{
#ifdef NDEBUG
			for (auto&& v : _table)
				if (v == nullptr)
					return false;
#endif
			return true;
		}());

	return (this->*_table[enum_cast(buffId)])(target, buffIndex);
}