#pragma once

#include <server/common/Buff.h>
#include <server/common/AutoTrigger.h>
#include <server/common/Direction.h>
#include <server/map_objects/Unit.h>
#include <server/map/MapInstanceId.h>
#include <server/common/ServerLimits.h>
#include <server/common/MapId.h>
#include <server/common/Item.h>
#include <server/common/CharacterId.h>
#include <server/common/CharacterInfo.h>
#include <server/common/TriggerEffectsCommon.h>
#include <server/player_extensions/SkillTree.h>
#include <server/player_extensions/Inventory.h>
#include <server/map/SessionDescriptor.h>

#include <sdk/IntrusiveDoublyLinkedNode.h>
#include <sdk/uid/Intrusive.hpp>

#include <array>

class Account;
class Storage;
class Party;
class Guild;
class ItemScripts;

class Character final
	: public Unit
	, public SessionDescriptor
	, public IntrusiveDoublyLinkedNode<Character>
	, public uid::Intrusive<Character>
{
public:
	static constexpr Type BLOCK_TYPE = Type::Character;

	Character(Account& account, uid::SharedPtr<CharacterInfo> info);
	~Character();

	CharacterId uid() const { return CharacterId(Intrusive::uid()); }

	Storage& getStorage();
	const Storage& getStorage() const;

	bool isMounted() const;

	auto& getName() const { return _info.name; }

	auto getJob() const { return _info.job; }
	void setJob(Job job) { _info.job = job; }

	auto getGender() const { return _info.gender; }
	void setGender(Gender gender) { _info.gender = gender; }

	auto getMapId() const { return _info.mapId; }
	void setMapId(MapId mapId) { _info.mapId = mapId; }

	auto getMapInstanceId() const { return _info.mapInstanceId; }
	void setMapInstanceId(MapInstanceId id) { _info.mapInstanceId = id; }

	auto getExpNextLvl() const { return _info.expNextLvl; }
	void setExpNextLvl(uint32_t exp) { _info.expNextLvl = exp; }

	auto getHairStyle() const { return _info.hairStyle; }
	void setHairStyle(uint8_t hairStyle) { _info.hairStyle = hairStyle; }

	auto getJobLvl() const { return _info.jobLvl; }
	void setJobLvl(uint8_t lvl) { _info.jobLvl = lvl; }
	
	// Get provided by unit
	void setLvl(uint8_t lvl)
	{
		_info.lvl = _lvl = lvl;
	}

	void updateCharInfo();

	bool _ignoreMagicDmg = false;
	bool _ignorePhysicalDmg = false;
	uint16_t _scaleAtkByDefRaceMask = 0;
	uint16_t _scaleAtkByDefElementMask = 0;
	uint8_t _ignoreMdefRace[enum_cast(Race::Last) + 1] = {};
	uint8_t _ignoreDefRace[enum_cast(Race::Last) + 1] = {};
	uint8_t _magicAddPercentRace[enum_cast(Race::Last) + 1] = {};
	uint8_t _magicAddPercentElem[enum_cast(Element::Last) + 1] = {};
	uint8_t _magicAddPercentTribe[enum_cast(Tribe::Last) + 1] = {};
	uint8_t _magicSubPercentRace[enum_cast(Race::Last) + 1] = {};
	uint8_t _magicSubPercentElem[enum_cast(Element::Last) + 1] = {};
	uint8_t _magicSubPercentTribe[enum_cast(Tribe::Last) + 1] = {};
	uint8_t _attackAddPercentRace[enum_cast(Race::Last) + 1] = {};
	uint8_t _attackAddPercentElem[enum_cast(Element::Last) + 1] = {};
	uint8_t _attackAddPercentTribe[enum_cast(Tribe::Last) + 1] = {};
	uint8_t _attackSubPercentRace[enum_cast(Race::Last) + 1] = {};
	uint8_t _attackSubPercentElem[enum_cast(Element::Last) + 1] = {};
	uint8_t _attackSubPercentTribe[enum_cast(Tribe::Last) + 1] = {};
	uint8_t _miscDefRate = 0;
	uint8_t _critDefRate = 0;
	uint8_t _attackDefRate = 0;
	uint8_t _rangeAttackDefRate = 0;
	uint8_t _meleeAtkReflectRate = 0;
	uint8_t _rangeAtkReflectRate = 0;
	uint8_t _ignoreSizeReductionCount = 0;
	uint8_t _skillPoints;
	Direction _headDirection;
	Element _weaponElement = Element::None;
	uid::SharedPtr<Party> _party;
	uid::SharedPtr<Guild> _guild;
	AutoTrigger _autoTrigger;
	SkillTree _skillTree;
	Inventory _inventory;
private:
	CharacterInfo& _info;
	std::array<RawBuff, MAX_CHARACTER_BUFFS> _buffSlots; // buffer injected to buff controller
	uid::SharedPtr<CharacterInfo> _charInfo;
	Account& _account;
};