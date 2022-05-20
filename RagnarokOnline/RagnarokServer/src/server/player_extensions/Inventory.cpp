#include "Inventory.h"
#include <server/common/ItemDb.h>

#include <tuple>


Inventory::Inventory()
	: _weight(0)
	, _itemCount(0)
	, _maxWeight(DEFAULT_CHAR_WEIGHT + 20000)
	, _inventory({})
{
	_equips.fill(INVALID_INDEX);

	for (int i = 0; i < (int)ItemDbId::Last + 1; i++)
		addItem((ItemDbId)i, 1);
}

Inventory::~Inventory()
{
}

//Gear methods
Item* IEquip::getEquip(EquipSlot equip)
{
	//If slot has no gear it'll map to invalid slot, return nullptr there. Otherwise return what it points to
	Inventory* inv = static_cast<Inventory*>(this);
	auto index = inv->_equips[enum_cast(equip)];

	if (index >= INVENTORY_SIZE)
		return nullptr;

	//Should never have a valid gear index but with amount 0
	assert(inv->_inventory[index]._amount != 0);
	return &inv->_inventory[index];
}

Item* IEquip::unsetEquip(EquipSlot equip)
{
	//If slot doesn't point to anything, there is already nothing in that slot
	Inventory* inv = static_cast<Inventory*>(this);
	auto index = inv->_equips[enum_cast(equip)];

	if (index >= INVENTORY_SIZE)
		return nullptr;

	//Otherwise, unset the slot and return the item that was there
	inv->_equips[enum_cast(equip)] = Inventory::INVALID_INDEX;
	inv->_inventory[index]._equipSlot = EquipSlot::None;

	return &inv->_inventory[index];
}

Item* IEquip::setEquip(EquipSlot equip, uint8_t index)
{
	//Assign the given index to the equip slot
	Inventory* inv = static_cast<Inventory*>(this);

	//if we're overwritting a valid index then we have a bug since we didn't call de-equip script
	assert(inv->_equips[enum_cast(equip)] == Inventory::INVALID_INDEX); 

	//Write the index and return item
	inv->_equips[enum_cast(equip)] = index;
	inv->_inventory[index]._equipSlot = equip;
	return &inv->_inventory[index];
}

const Item* Inventory::getEquip(EquipSlot equip) const
{
	return getItem(_equips[enum_cast(equip)]);
}

uint8_t Inventory::getEquipIndex(EquipSlot equip) const
{
	return _equips[enum_cast(equip)];
}

ItemDbId Inventory::getEquipDbId(EquipSlot equip) const
{
	auto* item = getEquip(equip);
	return item != nullptr ? item->_dbId : ItemDbId::None;
}

//Item methods
// Methods for add
std::tuple<Inventory::ErrorCode, uint8_t> Inventory::addItem(const Item& item)
{
	// check if we have enough weight
	if ((_weight + item._amount * item.db()._weight) > _maxWeight)
		return std::make_tuple(ErrorCode::Overweight, INVALID_INDEX);

	// check if we already have the item in the inventory
	uint8_t index = searchOrGetNewSlot(item._dbId);

	if (index == INVALID_INDEX) // inventory full
		return std::make_tuple(ErrorCode::InventoryFull, INVALID_INDEX);

	Item* slot = &_inventory[index];

	// update weight
	_weight += ItemDb::getItem(item._dbId)._weight * item._amount;

	// we add it as a new item if old amount was 0
	if (slot->_amount == 0)
	{
		slot->_dbId = item._dbId;
		slot->_expireTime = item._expireTime;
		slot->_cardDbId[0] = item._cardDbId[0];
		slot->_cardDbId[1] = item._cardDbId[1];
		slot->_cardDbId[2] = item._cardDbId[2];
		slot->_cardDbId[3] = item._cardDbId[3];
		slot->_affixesReserved = item._affixesReserved; //todo
		slot->_equipSlot = item._equipSlot;
		slot->_affixLevelUp = item._affixLevelUp;
		slot->_refine = item._refine;
		slot->_startAfixes = item._startAfixes;
		slot->_accountBound = item._accountBound;
		slot->_worldQuest = item._worldQuest;

		_itemCount++;
	}

	// in both cases we increment the amount
	slot->_amount += item._amount;

	return std::make_tuple(ErrorCode::Success, index);
}

std::tuple<Inventory::ErrorCode, uint8_t> Inventory::addItem(ItemDbId id, uint16_t amount)
{	
	// check if we have enough weight
	if ((_weight + ItemDb::getItem(id)._weight * amount) > _maxWeight)
		return std::make_tuple(ErrorCode::Overweight, INVALID_INDEX);

	// check if we already have the item in the inventory
	uint8_t index = searchOrGetNewSlot(id);

	if (index == INVALID_INDEX) // inventory full
		return std::make_tuple(ErrorCode::InventoryFull, INVALID_INDEX);

	Item* slot = &_inventory[index];

	// update weight
	_weight += ItemDb::getItem(id)._weight * amount;

	// we add it as a new item if old item had amount 0
	if (slot->_amount == 0)
	{
		slot->_dbId = id;
		slot->_expireTime = Tick::MAX();
		slot->_cardDbId[0] = ItemDbId::None;
		slot->_cardDbId[1] = ItemDbId::None;
		slot->_cardDbId[2] = ItemDbId::None;
		slot->_cardDbId[3] = ItemDbId::None;
		slot->_affixesReserved = 0; //todo
		slot->_equipSlot = EquipSlot::None;
		slot->_affixLevelUp = 0;
		slot->_refine = 0;
		slot->_startAfixes = 0;
		slot->_accountBound = false;
		slot->_worldQuest = 0;

		_itemCount++;
	}

	// in both cases we increment the amount
	slot->_amount._value += amount;

	return std::make_tuple(ErrorCode::Success, index);
}

//Methods for get
const Item& Inventory::getItemUnsafe(uint8_t index)  const
{
	assert(index < INVENTORY_SIZE && _inventory[index]._amount != 0);
	return _inventory[index];
}

const Item* Inventory::getItem(uint8_t index)  const
{
	if (index >= INVENTORY_SIZE)
		return nullptr;

	auto& slot = _inventory[index];

	return slot._amount == 0 ? nullptr : &slot;
}

const Item* Inventory::getItem(ItemDbId id)  const
{
	uint8_t index = searchItemSlot(id);

	return index == INVALID_INDEX ? nullptr : &_inventory[index];
}

//Methods for delete
Inventory::ErrorCode Inventory::deleteItem(uint8_t index, uint16_t amount)
{
	auto& slot = _inventory[index];

	//Needs to have at least the required amount
	if (slot._amount < amount)
		return ErrorCode::AmountNotEnough;

	//update amount and weight
	slot._amount -= amount;
	_weight -= slot._amount * slot.db()._weight;

	_itemCount--;
	
	return ErrorCode::Success;
}

std::tuple<Inventory::ErrorCode, uint8_t> Inventory::deleteItem(ItemDbId id, uint16_t amount)
{
	uint8_t index = searchItemSlot(id);

	if (index == INVALID_INDEX)
		return std::make_tuple(ErrorCode::ItemDoesntExist, INVALID_INDEX);

	auto* item = &_inventory[index];

	if(item->_amount < amount)
		return std::make_tuple(ErrorCode::AmountNotEnough, INVALID_INDEX);

	//update amount and weight
	item->_amount -= amount;
	_weight -= item->_amount * item->db()._weight;

	_itemCount--;

	return std::make_tuple(ErrorCode::Success, index);
}

//***** privates
uint8_t Inventory::searchOrGetNewSlot(ItemDbId id)  const
{
	uint8_t firstFree = INVALID_INDEX;
	for (int i = 0; i < INVENTORY_SIZE; i++)
	{
		if (_inventory[i]._amount == 0)
		{
			if (firstFree == INVALID_INDEX) // store the first free
				firstFree = i;

			continue;
		}

		if (_inventory[i]._dbId == id && _inventory[i]._equipSlot == EquipSlot::None)
			return i;
	}

	return firstFree;
}

uint8_t Inventory::searchItemSlot(ItemDbId id)  const
{
	for (uint8_t i = 0; i < INVENTORY_SIZE; i++)
	{
		if (_inventory[i]._dbId == id && _inventory[i]._equipSlot == EquipSlot::None)
			return i;
	}

	return INVALID_INDEX;
}