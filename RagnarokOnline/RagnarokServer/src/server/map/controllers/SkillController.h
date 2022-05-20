#pragma once

#include <server/common/Skill.h>
#include <server/common/SkillId.h>
#include <server/map/IMapLoop.hpp>
#include <server/map_objects/Unit.h>
#include <server/map_objects/BlockId.h>
#include <server/map_objects/BlockType.h>
#include <server/map_objects/FieldSkill.h>
#include <server/common/DamageTypes.h>
#include <server/map/DynamicBlockArray.hpp>

#include <sdk/enum_cast.hpp>
#include <sdk/pool/CachedPool.hpp>

#include <array>

class Map;
class Skill;
	
class SkillController final
	: public IMapLoop<Map, SkillController>
{
public:
	SkillController::SkillController(Map& map, ConcurrentPool<FieldSkill>& pool);

	//This one should check inventory. Make another one for other things
	bool checkCastRequirements(const Unit& unit, const Skill& skill, uint8_t skillLevel);

	void applyCastRequirements(Unit& unit, Skill& skill, uint8_t skillLevel);
		
	//Methods to dispatch skills
	void dispatchOnCastTimeFinished(Unit& src);

	void dispatchAtSourceUnit(Unit& src, Skill::Id skillId, uint8_t skillLevel);

	void dispatchAtTargetUnit(Unit& src, Skill::Id skillId, uint8_t skillLevel, BlockId target);

	void dispatchAtTargetPosition(Unit& src, Skill::Id skillId, uint8_t skillLevel, Point targetPosition);

	//Packet notification methods
	template<typename T>
	void notifyFromCenter(BlockId sourceId, const Block& center, const T& packet)
	{
		_map._blockGrid.runActionInRange(center, DEFAULT_VIEW_RANGE,
			[&](Block& block)
		{
			if (block._type == BlockType::Character && block._id != sourceId)
				_map.writePacket(block._id, packet);
		});
	}

	void notifyAoESkillCastStart(const Unit& src, SkillId skillId, CastTime castTime, const Point& center);

	void notifyTargetSkillCastStart(const Unit& src, SkillId skillId, CastTime castTime, const Unit& target);

	void notifySelfSkillCastStart(const Unit& src, SkillId skillId, CastTime castTime);

	void notififyTargetSkillDispatch(const Unit& src, SkillId skillId, const Unit& target);

	void notifySelfSkillDispatch(const Unit& src, SkillId skillId);

private:
	friend IMapLoop<Map, SkillController>;

	typedef void(SkillController::* SkillDispatchHandler)(Unit& src, int skillLevel, Point position, BlockId target);
	void dispatch(SkillId id, Unit& src, int skillLevel, Point position, BlockId target);

	void notififyAoESkillDispatch(const Unit& src, SkillId skillId, const FieldSkill& fieldSkill);

	void onEndMapLoop();

	// skill dispatch methods
	void dispatchStormGust(Unit& src, int skillLevel, Point position, BlockId /*targetId*/);

	void dispatchMammomite(Unit& src, int skillLevel, Point /*position*/, BlockId targetId);

	void dispatchEnergyCoat(Unit& src, int skillLevel, Point /*position*/, BlockId /*targetId*/);

	void dispatchJupitelThunder(Unit& src, int skillLevel, Point /*position*/, BlockId targetId);

	// pushes a field skill to the unit's field skill list
	void pushFieldSkillToUnit(Unit& src, FieldSkill& fieldSkill);

	// removes a field skill from the unit's field skill list
	void popFieldSkillFromUnit(FieldSkill& fieldSkill);

	FieldSkill& createFieldSkill(Unit& src, SkillId id, int32_t fieldSkillId, Point position,
		Tick endTick, DmgMatkRate rate = DmgMatkRate(0));

	void removeFieldSkill(int32_t fieldSkillId);

	Map& _map;
	DynamicBlockArray<FieldSkill> _blockArray;
	CachedPool<FieldSkill> _fieldSkillPool;
};