#include "SkillController.h"

#include <server/map/Map.h>
#include <server/network/Packets.h>
#include <server/map_objects/Unit.h>


SkillController::SkillController(Map& map, ConcurrentPool<FieldSkill>& pool)
	:_map(map)
	, _blockArray(map._blockArray)
	, _fieldSkillPool(pool)
{}

void SkillController::onEndMapLoop()
{
	_fieldSkillPool.releaseCache();
}

//************ Dispatchers
void SkillController::dispatchOnCastTimeFinished(Unit& src)
{
	dispatch(src._castData.skill->_id, src, src._castData.skillLevel, src._targetData.targetPosition, src._targetData.targetBlock);
}

void SkillController::dispatchAtSourceUnit(Unit& src, Skill::Id skillId, uint8_t skillLevel)
{
	dispatch(skillId, src, skillLevel, {}, {});
}

void SkillController::dispatchAtTargetUnit(Unit& src, Skill::Id skillId, uint8_t skillLevel, BlockId target)
{
	dispatch(skillId, src, skillLevel, {}, target);
}

void SkillController::dispatchAtTargetPosition(Unit& src, Skill::Id skillId, uint8_t skillLevel, Point targetPosition)
{
	dispatch(skillId, src, skillLevel, targetPosition, {});
}


//************ Utility
bool SkillController::checkCastRequirements(const Unit& unit, const Skill& skill, uint8_t skillLevel)
{		
	if (unit._isCasting || _map._currentTick < skill._cooldown)
		return false;

	//TODO check lokis veil here

	if (unit._type == BlockType::Character)
	{
		//Todo: check items and mp
	}

	return true;
}

void SkillController::applyCastRequirements(Unit& unit, Skill& skill, uint8_t skillLevel)
{
	if(unit._type == BlockType::Character)
	{
		//TODO: calc cooldown reduction
		skill._cooldown = _map._currentTick + skill.dbCooldown(skillLevel);
	}
}

//************ Packet notifications

void SkillController::notifyAoESkillCastStart(const Unit& src, SkillId skillId, CastTime castTime, const Point& center)
{
	auto srcId = src._id;

	//Notify local player that cast has finished
	switch (src._type)
	{
		case BlockType::Character:
			_map.writePacket(srcId, packet::SND_PlayerLocalBeginAoECast(skillId, castTime, center));
			notifyFromCenter(srcId, src, packet::SND_PlayerOtherBeginAoECast(srcId, skillId, castTime, center));
			break;
		case BlockType::Monster:
			notifyFromCenter(srcId, src, packet::SND_MonsterBeginAoECast(srcId, skillId, castTime, src._position));
			break;
		case BlockType::Homunculus:
		case BlockType::Mercenary:
			notifyFromCenter(srcId, src, packet::SND_OtherBeginAoECast(src._type, srcId, skillId, castTime, src._position));
			break;
	}		
}

void SkillController::notifyTargetSkillCastStart(const Unit& src, SkillId skillId, CastTime castTime, const Unit& target)
{
	auto srcId = src._id;

	//Notify local player that cast has finished
	switch (src._type)
	{
		case BlockType::Character:
			_map.writePacket(srcId, packet::SND_PlayerLocalBeginTargetCast(skillId, castTime, target));
			notifyFromCenter(srcId, src, packet::SND_PlayerOtherBeginTargetCast(srcId, skillId, castTime, target));
			break;
		case BlockType::Monster:
			notifyFromCenter(srcId, src, packet::SND_MonsterBeginTargetCast(srcId, skillId, castTime, target));
			break;
		case BlockType::Homunculus:
		case BlockType::Mercenary:
			notifyFromCenter(srcId, src, packet::SND_OtherBeginTargetCast(src._type, srcId, skillId, castTime, target));
			break;
	}
}

void SkillController::notifySelfSkillCastStart(const Unit& src, SkillId skillId, CastTime castTime)
{
	auto srcId = src._id;

	//Notify local player that cast has finished
	switch (src._type)
	{
		case BlockType::Character:
			_map.writePacket(srcId, packet::SND_PlayerLocalBeginSelfCast(skillId, castTime));
			notifyFromCenter(srcId, src, packet::SND_PlayerOtherBeginSelfCast(srcId, skillId, castTime));
			break;
		case BlockType::Monster:
			notifyFromCenter(srcId, src, packet::SND_MonsterBeginSelfCast(srcId, skillId, castTime));
			break;
		case BlockType::Homunculus:
		case BlockType::Mercenary:
			notifyFromCenter(srcId, src, packet::SND_OtherBeginSelfCast(src._type, srcId, skillId, castTime));
			break;
	}
}

void SkillController::notififyAoESkillDispatch(const Unit& src, SkillId skillId, const FieldSkill& fieldSkill)
{
	auto& srcId = src._id;
			
	//Notify local player that cast has finished
	switch (src._type)
	{
		case BlockType::Character: 
			_map.writePacket(srcId, packet::SND_PlayerLocalFinishedAoECast(skillId, fieldSkill._position));
			notifyFromCenter(srcId, src, packet::SND_PlayerOtherFinishedAoECast(srcId, skillId, fieldSkill._position));
			break;
		case BlockType::Monster:
			notifyFromCenter(srcId, src, packet::SND_MonsterFinishedAoECast(srcId, skillId, fieldSkill._position));
			break;
		case BlockType::Homunculus:
		case BlockType::Mercenary:
			notifyFromCenter(srcId, src, packet::SND_OtherFinishedAoECast(src._type, srcId, skillId, fieldSkill._position));
			break;
	}
}

void SkillController::notififyTargetSkillDispatch(const Unit& src, SkillId skillId, const Unit& target)
{
	auto& srcId = src._id;

	//Notify local player that cast has finished
	switch (src._type)
	{
		case BlockType::Character:
			_map.writePacket(srcId, packet::SND_PlayerLocalFinishedTargetCast(skillId, target));
			notifyFromCenter(srcId, src, packet::SND_PlayerOtherFinishedTargetCast(srcId, skillId, target));
			break;
		case BlockType::Monster:
			notifyFromCenter(srcId, src, packet::SND_MonsterFinishedTargetCast(srcId, skillId, target));
			break;
		case BlockType::Homunculus:
		case BlockType::Mercenary:
			notifyFromCenter(srcId, src, packet::SND_OtherFinishedTargetCast(src._type, srcId, skillId, target));
			break;
	}
}

void SkillController::notifySelfSkillDispatch(const Unit& src, SkillId skillId)
{
	auto& srcId = src._id;

	//Notify local player that cast has finished
	switch (src._type)
	{
		case BlockType::Character:
			_map.writePacket(srcId, packet::SND_PlayerLocalFinishedSelfCast(skillId));
			notifyFromCenter(srcId, src, packet::SND_PlayerOtherFinishedSelfCast(srcId, skillId));
			break;
		case BlockType::Monster:
			notifyFromCenter(srcId, src, packet::SND_MonsterFinishedSelfCast(srcId, skillId));
			break;
		case BlockType::Homunculus:
		case BlockType::Mercenary:
			notifyFromCenter(srcId, src, packet::SND_OtherFinishedSelfCast(src._type, srcId, skillId));
			break;
	}
}

//************ Skill handlers

void SkillController::dispatchStormGust(Unit& src, int skillLevel, Point position, BlockId /*targetId*/)
{
	static constexpr auto handler = 
	[](SkillController& skillController, int32_t fieldSkillId, Tick tick)
	{
		constexpr int SG_RANGE = 4;
		constexpr int UNIT_HIT_DELAY = 1000 / 2; // 2x per second
		constexpr int ACTION_INTERVAL = 1000 / 60; // lets say it executes 60x a second

		auto& skill = skillController._blockArray.unsafeGet(fieldSkillId);
		auto& unitController = skillController._map._unitController;
		auto& battle = skillController._map._batteController;

		skillController._map._blockGrid.runActionInRange(skill, SG_RANGE, [&](Block& block)
		{
			auto& target = reinterpret_cast<Unit&>(block);

			//skip non units, own caster, dead, LP cells and units with next SG tick not up yet				
			if (block._type > BlockType::LastUnit || &target == skill._src || target.isDead() || 
				skillController._map._tiles(target._position.x, target._position.y).hasLandProtectorOrBasilica() ||
				skillController._map._currentTick < target._statusChange.nextStormGustHit)
				return;

			//TODO: check if target is friend of source

			auto initialDmg = battle.calcInitialMagicDmg(*skill._src, target, Element::Water, DmgMatkRate(skill._dmgRate));
			auto finalDmg = battle.calcFinalMagicDmg(*skill._src, target, Element::Water, initialDmg, DmgFlag::None);

			if (finalDmg)
			{
				unitController.applyMagicDamage(*skill._src, target, finalDmg, DamageHitType::SingleHit);

				target._statusChange.nextStormGustHit = skillController._map._currentTick + UNIT_HIT_DELAY;
			}
		});

		//Only enqueue for another execution if the next execute time is before the end tick			
		Tick nextAction = skillController._map._currentTick + ACTION_INTERVAL;

		return nextAction >= skill._endTick ? Reschedule::NO() : Reschedule::YES(nextAction);
	};

	const Tick endTick = _map._currentTick + 4600;

	auto& skill = createFieldSkill(src, Skill::Id::StormGust, skillLevel, position, endTick, DmgMatkRate(100 + 40 * skillLevel));
	notififyAoESkillDispatch(src, Skill::Id::StormGust, skill);

	//Call the first execute manually to save time from enqueing an instant. It will give us the next execute time too
	Tick nextAction = handler(*this, skill._id, endTick).get();

	_map._timerController.add(this, nextAction, skill._id, handler);

	//Enqueue the stop timer
	_map._timerController.add(this, endTick, skill._id,
	[](SkillController& skillController, int32_t fieldSkillId, Tick tick)
	{
		skillController.removeFieldSkill(fieldSkillId);

		return Reschedule::NO();
	});
}

void SkillController::dispatchMammomite(Unit& src, int skillLevel, Point /*position*/, BlockId targetId)
{
	//mammomite should put a timer for animation delay
}

void SkillController::dispatchEnergyCoat(Unit& src, int skillLevel, Point /*position*/, BlockId /*targetId*/)
{
	//Energy coat should never fail the cast
	notifySelfSkillDispatch(src, SkillId::EnergyCoat);
	_map._buffController.applyBuff(src, BuffId::EnergyCoat, skillLevel);
}

void SkillController::dispatchJupitelThunder(Unit& src, int skillLvl, Point /*position*/, BlockId targetId)
{
	Unit* target = reinterpret_cast<Unit*>(_map._blockArray.get(BlockId(targetId)));

	//unit is gone, do nothing
	if (target == nullptr)
		return;

	notififyTargetSkillDispatch(src, SkillId::JupitelThunder, *target);

	//set unit direction to look at target
	auto dir = Direction::lookUpSafe(src._position, target->_position);
	_map._unitController.setUnitDirection(src, dir);

	const auto hits = 2 + skillLvl;
	constexpr auto maxKnockBack = 8;

	auto initialDmg = _map._batteController.calcInitialMagicDmg(src, *target, Element::Wind, DmgMatkRate(100));
	initialDmg *= hits;
	auto finalDmg = _map._batteController.calcFinalMagicDmg(src, *target, Element::Wind, initialDmg, DmgFlag::None);

	//Apply knock back (with leave and enter ranges of target)
	if (finalDmg && _map._unitController.knockbackUnit(*target, dir, std::min(hits, maxKnockBack)))
	{
		_map._unitController.applyMagicDamage(src, *target, finalDmg, DamageHitType::MultiHit);
	}
}

//*************** General methods

void SkillController::popFieldSkillFromUnit(FieldSkill& fieldSkill)
{
	Unit& src = *fieldSkill._src;

	//head is what we're looking for. Just remove it
	if (src._activeFieldSkill == &fieldSkill)
	{
		src._activeFieldSkill = fieldSkill._nextSkill;
		return;
	}

	//field skill must exist and have a parent so this should be safe
	FieldSkill* head = src._activeFieldSkill;
	while (head->_nextSkill != &fieldSkill)
		head = head->_nextSkill;

	head->_nextSkill = head->_nextSkill->_nextSkill;

	fieldSkill._src = nullptr;
}

FieldSkill& SkillController::createFieldSkill(Unit& src, SkillId id, int32_t lvl, Point position, Tick endTick, DmgMatkRate rate)
{
	auto& skill = *_fieldSkillPool.new_();
	skill._skillId = id;
	skill._skillLvl = lvl;
	skill._position = position;
	skill._src = &src;
	skill._dmgRate = rate;
	skill._endTick = endTick;

	if (rate && src._statusChange.contains(BuffId::MysticAmplification))
	{
		//consume amplify
		//skill..matkRate = xxx;
	}

	_blockArray.insert(skill);
	_map._blockGrid.push(skill);

	//make the new field skill into the head
	skill._nextSkill = src._activeFieldSkill;
	src._activeFieldSkill = &skill;

	return skill;
}

void SkillController::removeFieldSkill(int32_t fieldSkillId)
{
	auto& skill = _blockArray.unsafeGet(fieldSkillId);

	// remove field skill from unit list
	popFieldSkillFromUnit(skill);

	// remove it from block grid
	_map._blockGrid.remove(skill);

	_blockArray.release(skill);
	_fieldSkillPool.delete_(&skill);
}

void SkillController::dispatch(SkillId id, Unit& src, int skillLevel, Point position, BlockId target)
{
	using ArrayType = std::array<SkillController::SkillDispatchHandler, enum_cast(SkillId::Last) + 1>;

	static constexpr ArrayType _table = []() constexpr
	{
		ArrayType table = {};

		//Mage Skills
		table[enum_cast(SkillId::EnergyCoat)] = &SkillController::dispatchEnergyCoat;

		//Wizard Skills
		table[enum_cast(SkillId::StormGust)] = &SkillController::dispatchStormGust;
		table[enum_cast(SkillId::JupitelThunder)] = &SkillController::dispatchJupitelThunder;

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

	(this->*_table[enum_cast(id)])(src, skillLevel, position, target);
}