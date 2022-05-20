#pragma once

#include <server/common/TriggerEffectsCommon.h>
#include <server/common/CommonTypes.h>
#include <server/common/ServerLimits.h>

#include <sdk/Tick.h>

#include <array>

class Unit;
class Character;
class Item;
class ItemScripts;

struct AutoTrigger final
{

public:
	AutoTrigger() 
		: firstFreeIndex(0)
		, firstUsedIndex(INVALID_INDEX)
	{
		for (uint8_t i = 0; i < _autoTriggerBuffSlots.size(); i++)
			_autoTriggerBuffSlots[i].nextTick = i + 1;

		_autoTriggerBuffSlots[_autoTriggerBuffSlots.size() - 1].nextTick = INVALID_INDEX;
	}

private:
	friend class CharacterController;
	static constexpr uint8_t INVALID_INDEX = UINT8_MAX;
	typedef void(*AutoTriggerHandler)(ItemScripts& itemScripts, Character& src, const Item& item, OperationType useType, Unit* target);

	//Represents a registered auto triggered
	struct AutoTriggerData final
	{
		constexpr AutoTriggerData()
			: _handler(nullptr)
			, _chance(0)
			, _duration(0)
			, _equipSlot(EquipSlot::None)
			, _conditions(TriggerConditions::None)
			, _isActive(false)
			, _reserved(0)
		{ }
		AutoTriggerHandler _handler;
		uint16_t _chance;
		uint16_t _duration;
		EquipSlot _equipSlot;
		TriggerConditions _conditions;

		struct
		{
			uint8_t _isActive : 1,
					_reserved : 7;
		};
	};

	//Represents the buff the autotrigger gives when it is triggered
	struct AutoTriggerBuff final
	{
		Tick deadline = Tick::MAX();
		uint8_t autoTriggerIndex = AutoTrigger::INVALID_INDEX;
		AutoTriggerType autoTriggerType = AutoTriggerType::OnAttack;
		uint8_t nextTick = AutoTrigger::INVALID_INDEX;
		uint8_t previousTick = AutoTrigger::INVALID_INDEX;
	};


	//TODO: we can make these arrays into a categories array so we can have the same space shared by all the autotriggers
	std::array<AutoTriggerData, MAX_CHARACTER_AUTO_TRIGGERS> _onAttackTriggers;
	std::array<AutoTriggerData, MAX_CHARACTER_AUTO_TRIGGERS> _onAttackedTriggers;
	std::array<AutoTriggerData, MAX_CHARACTER_AUTO_TRIGGERS> _onSkillTriggers;

	std::array<AutoTriggerBuff, MAX_CHARACTER_AUTO_TRIGGERS* (enum_cast(AutoTriggerType::Last) + 1)> _autoTriggerBuffSlots;
	uint8_t firstFreeIndex;
	uint8_t firstUsedIndex;
};


