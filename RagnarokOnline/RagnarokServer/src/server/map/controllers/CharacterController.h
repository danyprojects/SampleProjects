#pragma once

#include <server/common/PacketEnums.h>
#include <server/common/Job.h>
#include <server/common/MapId.h>
#include <server/common/AutoTrigger.h>
#include <server/map/IMapLoop.hpp>
#include <server/map/DynamicBlockArray.hpp>
#include <server/network/PacketHandlerResult.h>

#include <sdk/array/ArrayView.hpp>
#include <sdk/IntrusiveList.hpp>

class Map;
class Character;

class CharacterController final
	: public IMapLoop<Map, CharacterController>
{
	typedef AutoTrigger::AutoTriggerHandler AutoTriggerHandler;
	typedef std::array<AutoTrigger::AutoTriggerData, MAX_CHARACTER_AUTO_TRIGGERS> TriggerArray;

public:
	enum class WarpType : uint8_t
	{
		MapPortal,
		Teleport,

		None
	};

	CharacterController(Map& map);

	void addCharacter(Character& character);
	void removeCharacter(Character& character);

	void maxAllSkills(Character& src);
	void changeJobLvl(Character& src, int increment);
	void jobUpgrade(Character& src, Job newJob);
	void resetJob(Character& src, Job newJob);

	//All 3 types of auto triggers
	void registerOnAttackTrigger(Character& src, const Item& item, AutoTriggerHandler handler, uint16_t chance, uint32_t duration, TriggerConditions conditions = TriggerConditions::Default);
	void removeOnAttackTrigger(Character& src, const Item& item, AutoTriggerHandler handler);
	void runOnAttackTriggers(Character& src, Unit& target, TriggerConditions conditions);

	void registerOnAttackedTrigger(Character& src, const Item& item, AutoTriggerHandler handler, uint16_t chance, uint32_t duration, TriggerConditions conditions = TriggerConditions::Default);
	void removeOnAttackedTrigger(Character& src, const Item& item, AutoTriggerHandler handler);
	void runOnAttackedTriggers(Character& src, Unit& target, TriggerConditions conditions);

	void registerOnSkillTrigger(Character& src, const Item& item, AutoTriggerHandler handler, uint16_t chance, uint32_t duration, TriggerConditions conditions = TriggerConditions::Default);
	void removeOnSkillTrigger(Character& src, const Item& item, AutoTriggerHandler handler);
	void runOnSkillTriggers(Character& src, Unit& target, TriggerConditions conditions);

	void checkCharacterAutoTriggers(Character& src);

	PacketHandlerResult onRCV_LevelUpSingleSkill(const packet::Packet& rawPacket, Character& src);
	PacketHandlerResult onRCV_LevelUpMultipleSkills(const packet::Packet& rawPacket, Character& src);
	PacketHandlerResult onRCV_LevelUpAllTreeSkills(const packet::Packet& rawPacket, Character& src);

	void warpToCell(Character& character, const Point position, const WarpType warpType);

	void notifyJobOrLevelChange(const Character& character, uint8_t baseLevel, Job job, uint8_t jobLevel) const;
	void notifyJobOrLevelChange(const Character& character, uint8_t baseLevel) const;
	void notifyJobOrLevelChange(const Character& character, Job job, uint8_t jobLevel) const;

	void notifyWarpOut(const Character& character, const WarpType warpType) const;
	void notifyWarpIn(const Character& character) const;

	void notifyStatusChange(const Character& src, OtherPlayerChangeType type, uint32_t value) const;
	void notifyStatusChange(const Character& src, LocalPlayerChangeType type, uint32_t value) const;

	PacketHandlerResult onRCV_CastAoESkill(const packet::Packet& rawPacket, Character& src);
	PacketHandlerResult onRCV_CastTargetSkill(const packet::Packet& rawPacket, Character& src);
	PacketHandlerResult onRCV_CastSelfTargetSkill(const packet::Packet& rawPacket, Character& src);
private:
	friend class IMapLoop<Map, CharacterController>;

	void registerTrigger(const Item& item, TriggerArray& triggerArray, AutoTriggerHandler handler, uint16_t chance, uint32_t duration, TriggerConditions conditions);
	void removeTrigger(Character& src, const Item& item, TriggerArray& triggerArray, AutoTriggerType triggerType, const AutoTriggerHandler handler);
	void runTriggers(Character& src, Unit& target, TriggerArray& triggerArray, TriggerConditions conditions, AutoTriggerType triggerType);
	void revertTriggerBuff(Character& src, AutoTriggerType triggerType, uint8_t index);

	void addAutoTriggerBuff(Character& src, AutoTriggerType triggerType, uint8_t triggerIndex, Tick deadline);
	void removeAutoTriggerBuff(Character& src, AutoTriggerType triggerType, uint8_t triggerIndex);
	void onAutoTriggerBuffEnd(Character& src, uint8_t index);

	// ***** IMapLoop
	void onBeginMapLoop();

	void sendSkillTreeUpdate(Character& src);

	Map& _map;
	IntrusiveList<Character> _characterList;
	DynamicBlockArray<Character> _charBlockArray;
};