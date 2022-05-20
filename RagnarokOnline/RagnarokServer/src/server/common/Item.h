#pragma once

#include <server/common/CommonTypes.h>
#include <server/common/ItemDbId.h>
#include <server/common/ItemDb.h>
#include <server/common/EquipMask.h>

#include <sdk/Tick.h>

class Inventory;
class FieldItem;

class Item
{
public:
	// we only want it to be modifiable by inventory and storage due to weight impact
	struct Amount
	{
		friend class Item;
		friend class Inventory;
		friend class Storage;

		Amount() = default;
		constexpr operator uint16_t() const { return _value; }
	private:
		constexpr Amount(uint16_t amount) : _value(amount){}

		constexpr auto operator +=(uint16_t amount) { return  _value += amount; }
		constexpr auto operator -=(uint16_t amount) { return  _value -= amount; }
		constexpr auto operator++() { return ++_value; }
		constexpr auto operator--() { return --_value; }
		constexpr auto operator++(int) { return _value++; }
		constexpr auto operator--(int) { return _value--; }

		uint16_t _value = 0;
	};

private:
	friend class Inventory;
	friend class FieldItem;

	Item() = default;

	Item(ItemDbId id, uint16_t amount)
		: _dbId(id)
		, _amount(amount)
	{}

public:
	const auto& db() const { return ItemDb::getItem(_dbId); }

	ItemDbId _dbId;
	Amount _amount;
	Tick _expireTime;
	ItemDbId _cardDbId[4];
	uint32_t _affixesReserved;
	EquipSlot _equipSlot;
	uint8_t _affixLevelUp;
	struct
	{
		uint8_t _refine : 5,
				_startAfixes : 3;
	};
	struct
	{
		uint8_t _accountBound : 1,
				_worldQuest : 1,
				_reserved : 6;
	};
	uint8_t _reserved2;
	uint16_t _reserved3;
	//uint32_t _reserved4; // This one is only in the case we want to align to 8 bytes
};