#pragma once
#include <server/common/ItemDbId.h>
#include <server/common/TriggerEffectsCommon.h>

#include <array>
#include <cstdint>

class Character;
class Map;
class Item;

class ItemScripts final
{
public:
	ItemScripts(Map& map)
		:_map(map)
	{}

	void useItem(const Item& item, Character& src, OperationType useType);
	void unequipItem(const Item& item, Character& src, OperationType useType);

private:
	typedef void(ItemScripts::* ItemUseHandler)(const Item&, Character&, OperationType);
	typedef void(ItemScripts::* ItemDeEquipHandler)(const Item&, Character&);

	//Empty methods so we never have nullptrs in the item table.
	constexpr void defaultOnUse(const Item&, Character&, OperationType) {}
	constexpr void defaultOnDeEquip(const Item&, Character&, OperationType) {}

	//Item script declaration goes here
	void dummy(const Item& item, Character& src, OperationType useType);

	Map& _map;
};