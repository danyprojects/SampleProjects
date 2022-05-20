#include "CharacterController.h"

#include <server/map/Map.h>
#include <server/network/Packets.h>
#include <server/map/data/MapData.h>
#include <server/common/ServerLimits.h>

#include <server/lobby/Account.h>
#include <server/common/Item.h>
#include <sdk/uid/Name.h>
#include <sdk/uid/ObjectPool.hpp>

CharacterController::CharacterController(Map& map)
	:_map(map)
	,_charBlockArray(map._blockArray)
{}

void CharacterController::removeCharacter(Character& character)
{
	_map._blockGrid.remove(character);
	_charBlockArray.release(character);
	_characterList.remove(&character);
}

void CharacterController::onBeginMapLoop()
{
	for (auto&& character : _characterList)
	{
		if (character._isCasting) //can be moving and casting
			_map._unitController.onUnitCastTimeFinished(character);

		_map._buffController.checkUnitBuffTicks(character);
		checkCharacterAutoTriggers(character);

		// do moving last in case character ends up getting invalid
		// TODO:: change this to timer, this also might invalidate iterator atm
		if (character._isMoving)
			_map._unitController.doUnitMove(character);
	}
}

void CharacterController::addCharacter(Character& character)
{
	// block has no grid position yet so we don't have to remove
	_map._blockGrid.push(character);

	_charBlockArray.insert(character);
	_characterList.insert(&character);

	_map.writePacket(character,
		packet::SND_EnterMap(_map._mapData._mapId, character._position, character._direction));

	notifyWarpIn(character);
}

void CharacterController::warpToCell(Character& character, const Point position, const WarpType warpType)
{
	//Send the leave range to players
	notifyWarpOut(character, warpType);

	//Apply the position
	character._position = position;
	_map._blockGrid.updateBlock(character); //Always update block after changing position

	//Send warp to cell packet to own player
	{
		const auto warpPacket = packet::SND_PlayerWarpToCell(character._position, character._direction, character._headDirection);
		_map.writePacket(character, warpPacket);
	}

	//Send the enter range to players and also notify character of new things on screen
	notifyWarpIn(character);
}

void CharacterController::notifyWarpOut(const Character& character, const WarpType warpType) const
{
	const auto leaveRange = warpType == WarpType::MapPortal ? LeaveRangeType::Default : LeaveRangeType::Teleport;

	//get packet ready for other players
	const auto packet = packet::SND_PlayerLeaveRange(character._id, leaveRange);

	//Only send leave range to other players
	_map._blockGrid.runActionInRange(character, DEFAULT_VIEW_RANGE,
		[&](Block& block)
	{
		if (block._type == BlockType::Character)
			_map.writePacket(block._id, packet);
	});
}

void CharacterController::notifyWarpIn(const Character& character) const
{
	const auto& inv = character._inventory;

	//get packet ready for other players
	const auto ownEnterRange = packet::SND_PlayerEnterRange(
		character._id, character._position, character._direction, character._headDirection,
		character.getGender(), character.getJob(), character.getHairStyle(), 
		inv.getEquipDbId(EquipSlot::TopHeadgear), inv.getEquipDbId(EquipSlot::MidHeadgear), inv.getEquipDbId(EquipSlot::LowHeadgear),
		inv.getEquipDbId(EquipSlot::Weapon), inv.getEquipDbId(EquipSlot::Shield),
		character._walkSpd, character._atkSpd, EnterRangeType::Teleport); //warp in should always be a teleport

	//Send enter range to other players
	//Enter enter ranges to own player
	_map._blockGrid.runActionInRange(character, DEFAULT_VIEW_RANGE,
		[&](Block& block)
	{
		if (block._type == BlockType::Character)
		{
			const auto& otherCharacter = reinterpret_cast<Character&>(block);
			const auto& inv = otherCharacter._inventory;

			const auto otherEnterRange = packet::SND_PlayerEnterRange(
				otherCharacter._id, otherCharacter._position, otherCharacter._direction, otherCharacter._headDirection,
				otherCharacter.getGender(), otherCharacter.getJob(), otherCharacter.getHairStyle(),
				inv.getEquipDbId(EquipSlot::TopHeadgear), inv.getEquipDbId(EquipSlot::MidHeadgear), inv.getEquipDbId(EquipSlot::LowHeadgear),
				inv.getEquipDbId(EquipSlot::Weapon), inv.getEquipDbId(EquipSlot::Shield),
				otherCharacter._walkSpd, otherCharacter._atkSpd, EnterRangeType::Default);

			//Send enter range to other player
			_map.writePacket(otherCharacter, ownEnterRange);
			//Send enter range and movement start (if aplicable) to own player
			_map.writePacket(character, otherEnterRange);

			if (otherCharacter._isMoving)
			{
				short startDelay = otherCharacter._movementData.nextMoveTick - _map._currentTick;
				Point destination;
				destination = otherCharacter._movementData.path[otherCharacter._movementData.pathLength - 1];
				packet::SND_PlayerOtherMove otherPlayerMove(otherCharacter._id, otherCharacter._position, destination, startDelay);
				_map.writePacket(character, otherPlayerMove);
			}
		}
		else if (block._type == BlockType::Monster)
		{
			auto& monster = reinterpret_cast<Monster&>(block);

			if (monster.isDead()) //Skip if monster is dead
				return;

			packet::SND_MonsterEnterRange monsterEnterRange(
				monster._id, monster._position, monster._direction, monster._monsterId, monster._walkSpd,
				monster._atkSpd, EnterRangeType::Default);

			_map.writePacket(character, monsterEnterRange);

			//We also need to check if the player that entered the screen is moving to notify the player moving
			if (monster._isMoving)
			{
				short startDelay = monster._movementData.nextMoveTick - _map._currentTick;
				Point destination;
				destination = monster._movementData.path[monster._movementData.pathLength - 1];
				packet::SND_MonsterMove monsterMove(monster._id, monster._position, destination, startDelay);
				_map.writePacket(character, monsterMove);
			}
		}
		else if (block._type == BlockType::Npc)
		{
			auto& npc = reinterpret_cast<Npc&>(block);
			packet::SND_OtherEnterRange npcEnterRange(
				BlockType::Npc, static_cast<uint16_t>(npc._npcId), npc._id, npc._position, EnterRangeType::Default);

			_map.writePacket(character, npcEnterRange);
		}
		else if (block._type == BlockType::Item)
		{
			auto& item = reinterpret_cast<FieldItem&>(block);
			packet::SND_ItemEnterRange itemEnterRange(
				static_cast<uint16_t>(item._itemData._dbId), item._id, item._position, item._itemData._amount, false, true);

			_map.writePacket(character, itemEnterRange);
		}
	});
}

void CharacterController::notifyJobOrLevelChange(const Character& character, uint8_t baseLevel, Job job, uint8_t jobLevel) const
{
	//send update base job or lvl change to local player (id 0)
	_map.writePacket(character._id, packet::SND_PlayerJobOrLevelChanged(BlockId::LOCAL_SESSION_ID(), job, baseLevel, jobLevel));

	//get packet ready for other players
	const auto packet = packet::SND_PlayerJobOrLevelChanged(character._id, job, baseLevel, jobLevel);

	_map._blockGrid.runActionInRange(character, DEFAULT_VIEW_RANGE,
		[&](Block& block)
	{
		if (block._type == BlockType::Character)
			_map.writePacket(block._id, packet);
	});
}

void CharacterController::notifyJobOrLevelChange(const Character& character, uint8_t baseLevel) const
{
	notifyJobOrLevelChange(character, baseLevel, character.getJob(), 0); // job lvl 0 for no change in cliente
}

void CharacterController::notifyJobOrLevelChange(const Character& character, Job job, uint8_t jobLevel) const
{
	notifyJobOrLevelChange(character, 0, job, jobLevel); // base level 0 for no change in client
}

void CharacterController::notifyStatusChange(const Character& src, OtherPlayerChangeType type, uint32_t value) const
{	
	auto packet = packet::SND_OtherPlayerStatusChange(src._id, type, value);

	//This is the packet that will make items disappear and OPTIONALLY make player show pickup animation. So run with item as center
	_map._blockGrid.runActionInRange(src, DEFAULT_VIEW_RANGE,
		[&](Block& block)
		{
			if (block._type == BlockType::Character)
				_map.writePacket(block._id, packet);
		});
}

void CharacterController::notifyStatusChange(const Character& src, LocalPlayerChangeType type, uint32_t value) const
{
	auto packet = packet::SND_LocalPlayerStatusChange(type, value);

	//This is the packet that will make items disappear and OPTIONALLY make player show pickup animation. So run with item as center
	_map._blockGrid.runActionInRange(src, DEFAULT_VIEW_RANGE,
		[&](Block& block)
		{
			if (block._type == BlockType::Character)
				_map.writePacket(block._id, packet);
		});
}

void CharacterController::changeJobLvl(Character& src, int increment)
{
	increment = increment < -255 ? -255 : increment > 255 ? 255 : increment;

	int tmpJbLvl = static_cast<int>(src.getJobLvl()) + increment;
	int tmpSkillPoint = static_cast<int>(src._skillPoints) + increment;

	src.setJobLvl(static_cast<uint8_t>(tmpJbLvl < 1 ? 1 : tmpJbLvl > 255 ? 255 : tmpJbLvl));
	src._skillPoints = static_cast<uint8_t>(tmpSkillPoint < 0 ? 0 : tmpSkillPoint > 255 ? 255 : tmpSkillPoint);

	notifyJobOrLevelChange(src, src.getJob(), src.getJobLvl());
	_map.writePacket(src, packet::SND_PlayerSkillPointsChanged(src._skillPoints));
}

void CharacterController::sendSkillTreeUpdate(Character& src)
{
	auto& skillTree = src._skillTree;
	auto& unindexedSkillReader = skillTree.getUnindexedExtraSkillReader();
	const auto unindexedSkillCount = unindexedSkillReader.available();
	const auto permanentSkillCount = skillTree.getPermanentSkillCount();

	using Packet = packet::SND_PlayerSkillTreeReload;
	std::aligned_storage<Packet::MAX_SIZE(), alignof(Packet)>::type raw;

	auto& packet = *reinterpret_cast<packet::SND_PlayerSkillTreeReload*>(&raw);

	packet.writeHeader(skillTree.size() + unindexedSkillCount, permanentSkillCount, unindexedSkillCount, src.getJob());

	// write the indexed skills first
	int i = 0;
	for (; i < skillTree.size(); i++)
	{
		auto& skill = skillTree.getSkillUnsafe(i);
		packet.skillInfos[i].id = skill._id;
		packet.skillInfos[i].level = i < permanentSkillCount ? skillTree.getPermanentSkillLevelUnsafe(i) : skill._lvl;
	}

	// write the unindexed skills
	while (unindexedSkillReader.next(packet.skillInfos[i].level, packet.skillInfos[i].id))
	{
		i++;
	};

	_map.writePacket(src, { (std::byte*) & packet, static_cast<uint8_t>(packet._totalSize) });
}

//ONLY call this when upgrading a job otherwise use resetJob
void CharacterController::jobUpgrade(Character& src, Job newJob)
{
	src.setJob(newJob);
	src.setJobLvl(1);
	auto skillTree = src._skillTree;
	skillTree.append(newJob);

	notifyJobOrLevelChange(src, src.getJob(), src.getJobLvl());
	sendSkillTreeUpdate(src);
}

void CharacterController::resetJob(Character& src, Job newJob)
{
	src.setJob(newJob);
	src.setJobLvl(1);
	src._skillPoints = 0;
	src._skillTree.reload(newJob);

	notifyJobOrLevelChange(src, src.getJob(), src.getJobLvl());
	_map.writePacket(src, packet::SND_PlayerSkillPointsChanged(0));

	sendSkillTreeUpdate(src);
}

void CharacterController::maxAllSkills(Character& src)
{
	src._skillTree.levelUpAllPermanentSkills();
	sendSkillTreeUpdate(src);
}

PacketHandlerResult CharacterController::onRCV_LevelUpSingleSkill(const packet::Packet& rawPacket, Character& src)
{
	const auto& packet = reinterpret_cast<const packet::RCV_LevelUpSingleSkill&>(rawPacket);

	bool success = false;
	if (src._skillPoints >= packet.skillIncrement)
	{
		success = src._skillTree.levelUpPermanentSkill(packet.skillIndex, packet.skillIncrement);
		if (success)
			src._skillPoints -= packet.skillIncrement;
	}

	_map.writePacket(src, packet::SND_PlayerSkillTreeLevelUpReply(success));
	_map.writePacket(src, packet::SND_PlayerSkillTreeLevelUpReply(success));
	_map.writePacket(src, packet::SND_PlayerSkillPointsChanged(src._skillPoints));

	return PacketHandlerResult::CONSUMED(packet);
}

PacketHandlerResult CharacterController::onRCV_LevelUpMultipleSkills(const packet::Packet& rawPacket, Character& src)
{
	const auto& packet = reinterpret_cast<const packet::RCV_LevelUpMultipleSkills&>(rawPacket);

	unsigned int totalIncrements = 0;
	for (int i = 0; i < packet.skillInfoSize(); i++)
	{
		totalIncrements += packet.skillsInfo[i].skillIncrement;
	}

	bool success = false;
	if (totalIncrements <= src._skillPoints)
	{
		success = src._skillTree.levelUpPermanentSkills(
			{ (std::pair<uint8_t, uint8_t>*)(packet.skillsInfo), packet.skillInfoSize() });

		if (success)
			src._skillPoints -= static_cast<uint8_t>(totalIncrements);
	}

	_map.writePacket(src, packet::SND_PlayerSkillTreeLevelUpReply(success));
	_map.writePacket(src, packet::SND_PlayerSkillPointsChanged(src._skillPoints));

	return PacketHandlerResult::CONSUMED(packet);
}

PacketHandlerResult CharacterController::onRCV_LevelUpAllTreeSkills(const packet::Packet& rawPacket, Character& src)
{
	const auto& packet = reinterpret_cast<const packet::RCV_LevelUpAllTreeSkills&>(rawPacket);

	unsigned int totalIncrements = 0;
	for (int i = 0; i < packet.skillIncrementSize(); i++)
	{
		totalIncrements += packet.skillIncrement[i];
	}

	bool success = false;
	if (totalIncrements <= src._skillPoints)
	{
		success = src._skillTree.levelUpPermanentSkills(
			{ const_cast<uint8_t*>(packet.skillIncrement), packet.skillIncrementSize() });

		if (success)
			src._skillPoints -= static_cast<uint8_t>(totalIncrements);
	}

	_map.writePacket(src, packet::SND_PlayerSkillTreeLevelUpReply(success));
	_map.writePacket(src, packet::SND_PlayerSkillPointsChanged(src._skillPoints));

	return PacketHandlerResult::CONSUMED(packet);
}

// Packet handlers for skill input
PacketHandlerResult CharacterController::onRCV_CastAoESkill(const packet::Packet& rawPacket, Character& src)
{
	auto& packet = reinterpret_cast<const packet::RCV_CastAreaOfEffectSkill&>(rawPacket);
	auto& skillTree = src._skillTree;

	if (packet.skillIndex >= skillTree.size())
		return PacketHandlerResult::INVALID();

	Skill& skill = skillTree.getSkillUnsafe(packet.skillIndex);

	_map._unitController.tryCastSkillAtPosition(src, packet.position, skill, packet.skilllevel);

	return PacketHandlerResult(PacketHandlerResult::Status::Consumed, sizeof(packet::RCV_CastAreaOfEffectSkill));
}

PacketHandlerResult CharacterController::onRCV_CastTargetSkill(const packet::Packet& rawPacket, Character& src)
{
	auto& packet = reinterpret_cast<const packet::RCV_CastSingleTargetSkill&>(rawPacket);
	auto& skillTree = src._skillTree;

	if (packet.skillIndex >= skillTree.size())
		return PacketHandlerResult::INVALID();

	Block* target = packet.targetId.isValid() ? _map._blockArray.get(packet.targetId) : &src;

	if (target == nullptr || target->_type > BlockType::LastUnit)
		return PacketHandlerResult(PacketHandlerResult::Status::Consumed, sizeof(packet::RCV_CastSingleTargetSkill));

	Skill& skill = skillTree.getSkillUnsafe(packet.skillIndex);

	_map._unitController.tryCastSkillAtTarget(src, reinterpret_cast<Unit&>(*target), skill, packet.skilllevel);

	return PacketHandlerResult(PacketHandlerResult::Status::Consumed, sizeof(packet::RCV_CastSingleTargetSkill));
}

PacketHandlerResult CharacterController::onRCV_CastSelfTargetSkill(const packet::Packet& rawPacket, Character& src)
{
	auto& packet = reinterpret_cast<const packet::RCV_CastSelfTargetSkill&>(rawPacket);
	auto& skillTree = src._skillTree;

	if (packet.skillIndex >= skillTree.size())
		return PacketHandlerResult::INVALID();

	Skill& skill = skillTree.getSkillUnsafe(packet.skillIndex);

	CastTime castTime = skill.db()._castTime[packet.skilllevel - 1];

	if (castTime > INSTANT_CASTIME)
	{
		//Store data for casting after cast time has elapsed
		src._isCasting = true;
		src._castData.deadline = _map._currentTick + castTime;
		src._castData.skill = &skill;
		src._castData.skillLevel = packet.skilllevel;

		//Then notify the cast
		_map._skillController.notifySelfSkillCastStart(src, skill._id, castTime);
	}
	else //instant cast, dispatch skill right away
	{
		_map._skillController.dispatchAtSourceUnit(src, skill._id, packet.skilllevel);
		_map._skillController.applyCastRequirements(src, skill, packet.skilllevel);
	}
	return PacketHandlerResult(PacketHandlerResult::Status::Consumed, sizeof(packet::RCV_CastSelfTargetSkill));
}

//***********	Methods for auto triggers
void CharacterController::checkCharacterAutoTriggers(Character& src)
{
	auto& autoTrigger = src._autoTrigger;
	auto& slots = autoTrigger._autoTriggerBuffSlots;
	while (autoTrigger.firstUsedIndex != AutoTrigger::INVALID_INDEX && slots[autoTrigger.firstUsedIndex].deadline <= _map._currentTick)
		onAutoTriggerBuffEnd(src, autoTrigger.firstUsedIndex);
}

void CharacterController::registerOnAttackTrigger(Character& src, const Item& item, AutoTriggerHandler handler, uint16_t chance, uint32_t duration, TriggerConditions conditions)
{
	registerTrigger(item, src._autoTrigger._onAttackTriggers, handler, chance, duration, conditions);
}

void CharacterController::removeOnAttackTrigger(Character& src, const Item& item, AutoTriggerHandler handler)
{
	removeTrigger(src, item, src._autoTrigger._onAttackTriggers, AutoTriggerType::OnAttack, handler);
}

void CharacterController::runOnAttackTriggers(Character& src, Unit& target, TriggerConditions conditions)
{
	runTriggers(src, target, src._autoTrigger._onAttackTriggers, conditions, AutoTriggerType::OnAttack);
}

void CharacterController::registerOnAttackedTrigger(Character& src, const Item& item, AutoTriggerHandler handler, uint16_t chance, uint32_t duration, TriggerConditions conditions)
{
	registerTrigger(item, src._autoTrigger._onAttackedTriggers, handler, chance, duration, conditions);
}

void CharacterController::removeOnAttackedTrigger(Character& src, const Item& item, AutoTriggerHandler handler)
{
	removeTrigger(src, item, src._autoTrigger._onAttackedTriggers, AutoTriggerType::OnAttacked, handler);
}

void CharacterController::runOnAttackedTriggers(Character& src, Unit& target, TriggerConditions conditions)
{
	runTriggers(src, target, src._autoTrigger._onAttackedTriggers, conditions, AutoTriggerType::OnAttacked);
}

void CharacterController::registerOnSkillTrigger(Character& src, const Item& item, AutoTriggerHandler handler, uint16_t chance, uint32_t duration, TriggerConditions conditions)
{
	registerTrigger(item, src._autoTrigger._onSkillTriggers, handler, chance, duration, conditions);
}

void CharacterController::removeOnSkillTrigger(Character& src, const Item& item, AutoTriggerHandler handler)
{
	removeTrigger(src, item, src._autoTrigger._onSkillTriggers, AutoTriggerType::OnSkill, handler);
}

void CharacterController::runOnSkillTriggers(Character& src, Unit& target, TriggerConditions conditions)
{
	runTriggers(src, target, src._autoTrigger._onSkillTriggers, conditions, AutoTriggerType::OnSkill);
}

void CharacterController::registerTrigger(const Item& item, TriggerArray& triggerArray, AutoTriggerHandler handler, uint16_t chance, uint32_t duration, TriggerConditions conditions)
{
	//RO also inserted by iterating and looking for a free slot
	for (int i = 0; i < MAX_CHARACTER_AUTO_TRIGGERS; i++)
	{
		if (triggerArray[i]._handler != nullptr)
			continue;

		triggerArray[i]._handler = handler;
		triggerArray[i]._chance = chance;
		triggerArray[i]._duration = duration;
		triggerArray[i]._conditions = conditions;
		triggerArray[i]._equipSlot = item._equipSlot;
		return;
	}
}

void CharacterController::removeTrigger(Character& src, const Item& item, TriggerArray& triggerArray, AutoTriggerType triggerType, const AutoTriggerHandler handler)
{
	for (int i = 0; i < MAX_CHARACTER_AUTO_TRIGGERS; i++)
	{
		//We need to compare handler too because the trigger might be from a card, which then has itemSlot of the gear it's carded into
		//If it's multiple of the same card in the same gear, it'll remove the first handle that matches, since that would mean there are 2 equal handlers
		if (triggerArray[i]._equipSlot != item._equipSlot || triggerArray[i]._handler != handler)
			continue;

		//Remove will automatically call revert on the handler
		if (triggerArray[i]._isActive)
			removeAutoTriggerBuff(src, triggerType, i);

		triggerArray[i]._handler = nullptr;
		triggerArray[i]._equipSlot = EquipSlot::None;

		return;
	}
}

void CharacterController::runTriggers(Character& src, Unit& target, TriggerArray& triggerArray, TriggerConditions conditions, AutoTriggerType triggerType)
{
	auto rnd = _map._rand();

	for (int i = 0; i < MAX_CHARACTER_AUTO_TRIGGERS; i++)
	{
		if (triggerArray[i]._handler == nullptr)
			continue;
		
		//TODO: check if condition check is correct
		//If conditions arent right or chance fails, skip 
		if ((triggerArray[i]._conditions & conditions) != triggerArray[i]._conditions || 
			 triggerArray[i]._chance < static_cast<uint32_t>(rnd % 10000))
			continue;

		//Run the trigger
		triggerArray[i]._handler(_map._itemScripts, src, *src._inventory.getEquip(triggerArray[i]._equipSlot), OperationType::Apply, &target);

		//If trigger has duration, add it to the list of active triggers for tick tracking
		if (triggerArray[i]._duration > 0)
		{
			addAutoTriggerBuff(src, triggerType, i, _map._currentTick + triggerArray[i]._duration);
			triggerArray[i]._isActive = true;
		}
	}
}

void CharacterController::revertTriggerBuff(Character& src, AutoTriggerType triggerType, uint8_t index)
{
	AutoTrigger::AutoTriggerData* trigger = nullptr;
	switch (triggerType)
	{
		case AutoTriggerType::OnAttack: trigger = &src._autoTrigger._onAttackTriggers[index]; break;
		case AutoTriggerType::OnAttacked: trigger = &src._autoTrigger._onAttackedTriggers[index]; break;
		case AutoTriggerType::OnSkill: trigger = &src._autoTrigger._onSkillTriggers[index]; break;
	}

	assert(trigger != nullptr);
	assert(trigger->_isActive);
	assert(src._inventory.getEquip(trigger->_equipSlot) != nullptr);

	trigger->_handler(_map._itemScripts, src, *src._inventory.getEquip(trigger->_equipSlot), OperationType::Revert, nullptr);
	trigger->_isActive = false;
}

void CharacterController::addAutoTriggerBuff(Character& src, AutoTriggerType triggerType, uint8_t triggerIndex, Tick deadline)
{
	auto& autoTrigger = src._autoTrigger;
	auto& slots = autoTrigger._autoTriggerBuffSlots;

	//We should never run out of slots since we have 1 buff slot per auto trigger.
	assert(autoTrigger.firstFreeIndex != AutoTrigger::INVALID_INDEX);

	//Get the free index and set the next free for next triggers
	auto freeIndex = autoTrigger.firstFreeIndex;
	auto& newSlot = slots[freeIndex];
	autoTrigger.firstFreeIndex = newSlot.nextTick;

	//Set the trigger data
	newSlot.autoTriggerType = triggerType;
	newSlot.autoTriggerIndex = triggerIndex;
	newSlot.deadline = deadline;

	//If we dont have any autotriggers yet then insert at head
	if (autoTrigger.firstUsedIndex == AutoTrigger::INVALID_INDEX)
	{
		autoTrigger.firstUsedIndex = freeIndex;
		newSlot.previousTick = AutoTrigger::INVALID_INDEX;
		newSlot.nextTick = AutoTrigger::INVALID_INDEX;
	}
	else //If we have at least 1 autotrigger search where to insert
	{
		//Find the current active autotrigger with a deadline greater or equal than the new autotrigger
		//If no autotrigger with larger deadline is found then index will be pointing to the tail and we'll want to insert after it
		auto foundIndex = autoTrigger.firstUsedIndex;
		while (slots[foundIndex].nextTick != AutoTrigger::INVALID_INDEX)
		{
			if (deadline <= slots[foundIndex].deadline)
				break;
			foundIndex = slots[foundIndex].nextTick;
		}

		auto& foundSlot = slots[foundIndex];
		//Check if we exited the loop due to deadline condition being true
		//If so, we want to insert right before the buff found	so both next and previous are valid to use
		if (deadline <= foundSlot.deadline)
		{
			//Point next to found index, and previous to the previous of found index
			newSlot.nextTick = foundIndex;
			newSlot.previousTick = foundSlot.previousTick;

			//If we found a value in the middle
			if (foundSlot.previousTick != AutoTrigger::INVALID_INDEX)
				slots[foundSlot.previousTick].nextTick = freeIndex;
			else //Otherwise we're inserting at head
				autoTrigger.firstUsedIndex = freeIndex;

			foundSlot.previousTick = freeIndex;
		}
		else //We want to insert as the tail
		{
			newSlot.nextTick = AutoTrigger::INVALID_INDEX;
			newSlot.previousTick = foundIndex;
			foundSlot.nextTick = freeIndex;
		}
	}

}

void CharacterController::removeAutoTriggerBuff(Character& src, AutoTriggerType triggerType, uint8_t triggerIndex)
{
	auto& autoTrigger = src._autoTrigger;
	auto& slots = autoTrigger._autoTriggerBuffSlots;
	
	//Find the trigger
	auto nextIndex = autoTrigger.firstUsedIndex;
	while (nextIndex != AutoTrigger::INVALID_INDEX)
	{
		if (slots[nextIndex].autoTriggerType == triggerType && slots[nextIndex].autoTriggerIndex == triggerIndex)
		{
			onAutoTriggerBuffEnd(src, nextIndex);
			return;
		}
		nextIndex = slots[nextIndex].nextTick;
	}

	//If we didn't find a trigger then we somehow tried to remove a trigger that was not active. Must not happen
	assert(false);
}

void CharacterController::onAutoTriggerBuffEnd(Character& src, uint8_t index)
{
	auto& autoTrigger = src._autoTrigger;
	auto& slots = autoTrigger._autoTriggerBuffSlots;
	auto& triggerSlot = slots[index];

	revertTriggerBuff(src, triggerSlot.autoTriggerType, triggerSlot.autoTriggerIndex);

	//Fix pointers
	//if it's head
	if (autoTrigger.firstUsedIndex == index)
	{
		//if there's a next
		if(triggerSlot.nextTick != AutoTrigger::INVALID_INDEX)
			slots[triggerSlot.nextTick].previousTick = AutoTrigger::INVALID_INDEX;
		autoTrigger.firstUsedIndex = triggerSlot.nextTick;
	}
	else //there will always be a previous
	{
		//if there's a next make it point to previous
		if (triggerSlot.nextTick != AutoTrigger::INVALID_INDEX)
			slots[triggerSlot.nextTick].previousTick = triggerSlot.previousTick;
		slots[triggerSlot.previousTick].nextTick = triggerSlot.nextTick;

		//don't update head here
	}

	//Add removed to free stack
	triggerSlot.nextTick = autoTrigger.firstFreeIndex;
	autoTrigger.firstFreeIndex = index;
	return;
}