#pragma once

#include <server/common/ServerLimits.h>
#include <server/common/Item.h>

#include <array>

class ItemController;

//This class is to make it so that only ItemController is able to get a mutable Item*, and only by equip slot
class IEquip
{
private:
	friend class ItemController;

	Item* getEquip(EquipSlot equip);
	Item* unsetEquip(EquipSlot equip);
	Item* setEquip(EquipSlot equip, uint8_t index);
};

class Inventory final : public IEquip
{
public:
	static constexpr uint8_t INVALID_INDEX = UINT8_MAX;

	enum class ErrorCode
	{
		Success,
		Overweight,
		InventoryFull,
		ItemDoesntExist,
		AmountNotEnough
	};

	Inventory();
	~Inventory();

	uint16_t getWeight() const { return _weight; }
	uint8_t getItemCount() const { return _itemCount; }

	//Equip methods
	const Item* getEquip(EquipSlot equip) const;
	uint8_t getEquipIndex(EquipSlot equip) const;
	ItemDbId getEquipDbId(EquipSlot equip) const;

	//Item methods
	//Methods for adding. These can always fail
	std::tuple<ErrorCode, uint8_t> addItem(const Item& item);
	std::tuple<ErrorCode, uint8_t> addItem(ItemDbId id, uint16_t amount);

	//Methods for get
	const Item& getItemUnsafe(uint8_t index) const;
	const Item* getItem(uint8_t index) const;
	const Item* getItem(ItemDbId id) const;

	//Methods for delete. Delete is always safe since we need to check the amount anyway
	ErrorCode deleteItem(uint8_t index, uint16_t amount);
	std::tuple<ErrorCode, uint8_t> deleteItem(ItemDbId id, uint16_t amount);

private:
	friend class IEquip;

	//Searchs for item ID. If found, returns itemPtr pointing to item. If not found returns itemPtr to a free slot
	uint8_t searchOrGetNewSlot(ItemDbId id)  const;

	//searches for item ID. Returns null if not found
	uint8_t searchItemSlot(ItemDbId id)  const;

	std::array<int, enum_cast(EquipSlot::Last) + 1> _equips;
	std::array<Item, INVENTORY_SIZE> _inventory;
	uint16_t _weight;
	uint16_t _maxWeight;
	uint8_t _itemCount;
};
