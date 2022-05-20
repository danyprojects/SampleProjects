#include "ItemController.h"

#include <sdk/log/Log.h>
#include <server/map/Map.h>
#include <server/network/Packets.h>

namespace
{
	constexpr char* const TAG = "ItemController";

	#define TIMER_CB(Callback) TimerCb<ItemController, &ItemController::Callback>()
}

ItemController::ItemController(Map& map, ConcurrentPool<FieldItem>& pool)
	:_map(map)
	,_blockArray(map._blockArray)
	,_fieldItemPool(pool)
{ }

void ItemController::onEndMapLoop()
{
	_fieldItemPool.releaseCache();
}

//********	Public Utility
void ItemController::spawnItemAroundPosition(ItemDbId itemDbId, const Point& position)
{
	Point freeCell = getFreeCellAroundPosition(position);

	//Check if it was a valid cell
	if (freeCell.x == 0) //only need to check 0 since borders are never walkable
		return;

	//Get an item block and fill it
	auto& itemBlock = createFieldItem(itemDbId, 1);
	//TODO: fill other fields here

	//Spawn item
	spawnItemAtPosition(itemBlock, freeCell);
}

bool ItemController::tryPickUpItem(Unit& unit, FieldItem& item)
{
	//get distance to item
	int distance = Pathfinder::CalculateDiagonalDistance(unit._position, item._position);

	//if within distance pick up item immediatly
	if (distance <= MAX_PICK_UP_RANGE)
	{
		if (canUnitPickUpItem(unit, item))
			doPickUpItem(unit, item);
		else
			notifyFailToPickUpItem(unit);
	}
	else if (distance < MAX_WALK_DISTANCE) //otherwise try to walk towards item cell if distance is within walking range
	{
		if (!_map._unitController.canMove(unit) || !_map._unitController.trySetUnitMoveDestination(unit, item._position, PathType::Weak))
			return false;

		_map._unitController.notifyUnitStartMove(unit, item._position);
		unit._moveAction = MoveAction::PickUpItem;
		unit._targetData.targetBlock = item._id;
		unit._targetData.targetPosition = item._position;
	}
	else //invalid distances should be 0
		return false;

	return true;
}

bool ItemController::canUnitPickUpItem(const Unit& unit, const FieldItem& item) const
{
	//Monsters can always pick up. //TODO: maybe they have a limit of items?
	if (unit._type == BlockType::Monster)
		return true;

	//TODO: check for ownership, such as player who didn't kill the mob can't pick up

	//TODO: notify player if he could not pick up the item

	return true;
}

bool ItemController::stripEquipSlots(Character& src, EquipMask slots)
{
	//TODO:
	return true;
}

//********	Other
void ItemController::onMoveActionPickUpItemReached(Unit& unit, uint32_t moveTick)
{
	_map._timerController.add(this, _map._currentTick + moveTick, unit._id._index, TIMER_CB(runUnitMoveActionItemPickUp));
	unit._animDelayEnd = _map._currentTick + moveTick; //So we can't start a new move until we actually pick up the item
}

//********	Packet handlers
PacketHandlerResult ItemController::onRCV_PickUpItem(const packet::Packet& rawPacket, Character& src)
{
	auto& rcvPacket = reinterpret_cast<const packet::RCV_PickUpItem&>(rawPacket);

	Block* block = _map._blockArray.get(rcvPacket.itemBlockId);
	if(block->_type != BlockType::Item)
		return PacketHandlerResult(PacketHandlerResult::Status::Consumed, sizeof(packet::RCV_PickUpItem));

	FieldItem& item = *reinterpret_cast<FieldItem*>(block);

	tryPickUpItem(src, item);

	return PacketHandlerResult(PacketHandlerResult::Status::Consumed, sizeof(packet::RCV_PickUpItem));
}

PacketHandlerResult ItemController::onRCV_DropItem(const packet::Packet& rawPacket, Character& src)
{
	auto& rcvPacket = reinterpret_cast<const packet::RCV_DropItem&>(rawPacket);

	auto* item = src._inventory.getItem(rcvPacket.index);

	//Check if it's a valid item and amount
	if (item == nullptr || item->_amount < rcvPacket.amount || src.isDead() ||
		item->_accountBound || item->_equipSlot != EquipSlot::None)
		//TODO: add other checks if player can drop item
	{
		return PacketHandlerResult(PacketHandlerResult::Status::Consumed, sizeof(packet::RCV_DropItem));
	}

	//try to get a free cell around player position
	Point freeCell = getFreeCellAroundPosition(src._position);

	//Check if it was a valid cell. only need to check 0 since borders are never walkable
	if (freeCell.x == 0) 
		return PacketHandlerResult(PacketHandlerResult::Status::Consumed, sizeof(packet::RCV_DropItem));

	// Get an item block and fill it
	auto & itemBlock = createFieldItem(item->_dbId, rcvPacket.amount);
	//TODO: fill other fields here
	

	//Spawn item
	spawnItemAtPosition(itemBlock, freeCell);

	//then delete that amount of the item from the player inventory. Should never fail
	auto error = src._inventory.deleteItem(rcvPacket.index, rcvPacket.amount);
	assert(error == Inventory::ErrorCode::Success);

	const auto packet = packet::SND_DeleteItem(rcvPacket.index, rcvPacket.amount);
	_map.writePacket(src, packet);	

	return PacketHandlerResult(PacketHandlerResult::Status::Consumed, sizeof(packet::RCV_DropItem));
}

PacketHandlerResult ItemController::onRCV_UseItem(const packet::Packet& rawPacket, Character& src)
{
	auto& rcvPacket = reinterpret_cast<const packet::RCV_UseItem&>(rawPacket);

	if(src.isDead() || rcvPacket.index < 0 || rcvPacket.index >= INVENTORY_SIZE)
		return PacketHandlerResult(PacketHandlerResult::Status::Consumed, sizeof(packet::RCV_UseItem));

	auto* item = src._inventory.getItem(rcvPacket.index);

	if(item == nullptr)
		return PacketHandlerResult(PacketHandlerResult::Status::Consumed, sizeof(packet::RCV_UseItem));

	//If it's equippable then try to equip
	if (item->db().isEquipable())
	{
		//If could equip, then notify equip and also check if it's visible to notify other players
		if (equipItem(*item, rcvPacket.index, src))
		{
			_map.writePacket(src, packet::SND_EquipItem(rcvPacket.index, ErrorCode::Ok));
			if((item->db()._equipMask & EquipMask::Visible) > 0)
				_map._characterController.notifyStatusChange(src, OtherPlayerChangeType::EquipItem, static_cast<uint32_t>(item->_dbId));
		}
		else
			_map.writePacket(src, packet::SND_EquipItem(rcvPacket.index, ErrorCode::GenericFail));
	}
	else if (item->db().isUsable())
	{
		if (useItem(*item, src))
		{
			src._inventory.deleteItem(rcvPacket.index, 1);
			_map.writePacket(src, packet::SND_DeleteItem(rcvPacket.index, 1));

			//TODO: maybe send use success / fail item
		}
	}

	return PacketHandlerResult(PacketHandlerResult::Status::Consumed, sizeof(packet::RCV_UseItem));
}

PacketHandlerResult ItemController::onRCV_UnequipItem(const packet::Packet& rawPacket, Character& src)
{
	auto& rcvPacket = reinterpret_cast<const packet::RCV_UnequipItem&>(rawPacket);

	if (src.isDead() || rcvPacket.index < 0 || rcvPacket.index >= INVENTORY_SIZE)
		return PacketHandlerResult(PacketHandlerResult::Status::Consumed, sizeof(packet::RCV_UnequipItem));

	auto* item = src._inventory.getItem(rcvPacket.index);

	//Check if item is valid and equipped
	if (item == nullptr || item->_equipSlot == EquipSlot::None)
		return PacketHandlerResult(PacketHandlerResult::Status::Consumed, sizeof(packet::RCV_UnequipItem));

	//If could unequip, check if it's visible to notify other players
	if (unequipItem(*item, src))
	{
		_map.writePacket(src, packet::SND_UnequipItem(rcvPacket.index, ErrorCode::Ok));
		if ((item->db()._equipMask & EquipMask::Visible) > 0)
			_map._characterController.notifyStatusChange(src, OtherPlayerChangeType::UnequipItem, static_cast<uint32_t>(item->_dbId));
	}
	else
		_map.writePacket(src, packet::SND_UnequipItem(rcvPacket.index, ErrorCode::GenericFail));

	return PacketHandlerResult(PacketHandlerResult::Status::Consumed, sizeof(packet::RCV_UnequipItem));
}

//Notifies
void ItemController::notifyItemSpawn(const FieldItem& item) const
	{		
		const auto packet = packet::SND_ItemEnterRange(static_cast<uint16_t>(item._itemData._dbId), item._id, item._position, item._itemData._amount, true, true);

		_map._blockGrid.runActionInRange(item, DEFAULT_VIEW_RANGE,
		[&](Block& block)
		{
			if (block._type == BlockType::Character)
				_map.writePacket(block._id, packet);
		});
}

void ItemController::notifyItemDespawn(const FieldItem& item) const
{
	const auto packet = packet::SND_OtherLeaveRange(BlockType::Item, static_cast<uint16_t>(item._itemData._dbId), item._id, LeaveRangeType::Default);

	_map._blockGrid.runActionInRange(item, DEFAULT_VIEW_RANGE,
		[&](Block& block)
		{
			if (block._type == BlockType::Character)
				_map.writePacket(block._id, packet);
		});
}

void ItemController::notifyItemPickUp(const Unit& unit, const FieldItem& item) const
{
	//Monsters don't show a pick up animation
	if (unit._type == BlockType::Monster)
	{
		notifyItemDespawn(item);
		return;
	}
		
	auto packet = packet::SND_PlayerPickUpItem(unit._id, item._id);

	//This is the packet that will make items disappear and OPTIONALLY make player show pickup animation. So run with item as center
	_map._blockGrid.runActionInRange(item, DEFAULT_VIEW_RANGE,
		[&](Block& block)
		{
			if (block._type == BlockType::Character && block._id != unit._id)
				_map.writePacket(block._id, packet);
		});

	packet._playerId = BlockId::LOCAL_SESSION_ID();
	_map.writePacket(unit._id, packet);
}

void ItemController::notifyFailToPickUpItem(const Unit& unit) const
{
	//Only characters need to be notified on fail to pick up
	if (unit._type == BlockType::Character)
	{
		//TODO: notify fail to pick up to player only, no broadcast. Should this be done with a specific packet?
	}
}

//******* Methods for timers
Reschedule ItemController::runUnitMoveActionItemPickUp(int32_t blockId, Tick)
{
	Block* block = _map._blockArray.get(BlockId(blockId));
	if (block == nullptr || block->_type > BlockType::LastUnit) //only units can pick up item
		return Reschedule::NO();

	Unit& unit = *reinterpret_cast<Unit*>(block);

	block = _map._blockArray.get(BlockId(unit._targetData.targetBlock));
	if(block == nullptr || block->_type != BlockType::Item) //Make sure item still exists. Only items can be picked up
		return Reschedule::NO();

	FieldItem& item = *reinterpret_cast<FieldItem*>(block);

	//Now that we reached item, check if we're able to pick it up or not
	if (canUnitPickUpItem(unit, item))
		doPickUpItem(unit, item);
	else
		notifyFailToPickUpItem(unit);

	return Reschedule::NO();
}

//******* Utility
Point ItemController::getFreeCellAroundPosition(const Point& position) const
{
	int itemsPerCell[3][3] = { 0 };
	//Get the count of all items around position
	_map._blockGrid.runActionInRange(position, 1, [&](Block& block)
		{
			if (block._type == Block::Type::Item)
				itemsPerCell[position.x - block._position.x + 1][position.y - block._position.y + 1]++;
		});

	//Get free cells. Aka cells that haven't reached the maximum items per cell
	Point freeCells[9];
	int freeCell = 0;

	for (int16_t y = 0; y < 3; y++)
		for (int16_t x = 0; x < 3; x++)
			if (itemsPerCell[x][y] < MAX_ITEMS_PER_CELL && _map._tiles(position.x + (x - 1), position.y + (y - 1)).isWalkable())
				freeCells[freeCell++] = Point{ x - 1, y - 1 };

	//No free cells for item
	if (freeCell == 0)
		return Point{ 0,0 };

	//Pick a random free cell and add it to position
	return position + freeCells[_map._rand(freeCell - 1)];
}

void ItemController::spawnItemAtPosition(FieldItem& itemBlock, const Point& position)
{
	//Set item position
	itemBlock._position = position;
	_map._blockGrid.push(itemBlock);

	//Notify drop with animation
	notifyItemSpawn(itemBlock);

	//add the timer to despawn
	_map._timerController.add(this, _map._currentTick + DEFAULT_DESPAWN_TIME, itemBlock._id._index, TIMER_CB(despawnItem));
}

Reschedule ItemController::despawnItem(int32_t itemIndex, Tick)
{
	Block* block = _map._blockArray.get(BlockId(itemIndex));

	if(block == nullptr)
		return Reschedule::NO();

	FieldItem& itemBlock = *reinterpret_cast<FieldItem*>(block);
	notifyItemDespawn(itemBlock);

	removeFieldItem(itemBlock);

	return Reschedule::NO();
}

void ItemController::doPickUpItem(Unit& unit, FieldItem& item)
{
	//Monsters can pick up items too but don't have an animation or inventory
	if (unit._type == BlockType::Character)
	{
		Character& character = reinterpret_cast<Character&>(unit);
		
		auto[error, index] = character._inventory.addItem(item._itemData);

		//If add succeeds, notify player about itemchange in inventory
		if (error == Inventory::ErrorCode::Success)
		{
			unit._animDelayEnd = _map._currentTick + ACTION_DELAY_BASE_TIME * 3; // pick up should be 3 frames

			auto packet = packet::SND_GetItem(item._itemData._dbId, index, item._itemData._amount);
			_map.writePacket(unit._id, packet);
		}
		else //otherwise notify the pick up error respectively
		{
			if (error == Inventory::ErrorCode::Overweight)
				_map.writePacket(unit._id, packet::SND_PlayerPickUpItemFail(ErrorCode::Overweight));
			if (error == Inventory::ErrorCode::InventoryFull)
				_map.writePacket(unit._id, packet::SND_PlayerPickUpItemFail(ErrorCode::InventoryFull));
			else
				_map.writePacket(unit._id, packet::SND_PlayerPickUpItemFail(ErrorCode::GenericFail));

			//if pickup fails we don't notify others or remove item from field
			return;
		}
	}

	notifyItemPickUp(unit, item);
	removeFieldItem(item);
}

bool ItemController::equipItem(const Item& item, uint8_t index, Character& src)
{
	//Add fail checks here
	//if (src._level < item.db()._requiredLvl)
	//	return false;

	switch (item.db()._equipMask.getValue())
	{
		//********Equips that always involve two or more slots
		//***** Headgears
		case EquipMask::HeadUpper:
		{
			// Always dequip top
			unequipSlot(EquipSlot::TopHeadgear, src);
			equipSlot(EquipSlot::TopHeadgear, src, index);
		}break;
		case EquipMask::HeadMiddle:
		{
			// Always dequip middle. And top if upper is not single slot
			auto* top = src._inventory.getEquip(EquipSlot::TopHeadgear);

			if (top != nullptr && top->db()._equipMask != EquipMask::HeadUpper)
				unequipSlot(EquipSlot::TopHeadgear, src);

			unequipSlot(EquipSlot::MidHeadgear, src);
			equipSlot(EquipSlot::MidHeadgear, src, index);
		}break;
		case EquipMask::HeadLower:
		{
			// Always dequip lower. And mid / top if upper mid lower or mid lower
			auto* top = src._inventory.getEquip(EquipSlot::TopHeadgear);
			auto* mid = src._inventory.getEquip(EquipSlot::MidHeadgear);

			if (top != nullptr && top->db()._equipMask == EquipMask::HeadUpperMiddleLower)
				unequipSlot(EquipSlot::TopHeadgear, src);
			else if(mid != nullptr && mid->db()._equipMask == EquipMask::HeadMiddleLower)
				unequipSlot(EquipSlot::MidHeadgear, src);

			unequipSlot(EquipSlot::LowHeadgear, src);
			equipSlot(EquipSlot::LowHeadgear, src, index);
		}break;
		case EquipMask::HeadUpperMiddle: 
		{
			// Always dequip top and middle and equip top
			unequipSlot(EquipSlot::TopHeadgear, src);
			unequipSlot(EquipSlot::MidHeadgear, src);
			equipSlot(EquipSlot::TopHeadgear, src, index);
		}break;
		case EquipMask::HeadMiddleLower:
		{
			// Always dequip mid and low and upper if is multislot
			auto* top = src._inventory.getEquip(EquipSlot::TopHeadgear);
			if(top != nullptr && top->db()._equipMask != EquipMask::HeadUpper)
				unequipSlot(EquipSlot::TopHeadgear, src);

			unequipSlot(EquipSlot::MidHeadgear, src);
			unequipSlot(EquipSlot::LowHeadgear, src);
			equipSlot(EquipSlot::MidHeadgear, src, index);

		}break;
		case EquipMask::HeadUpperMiddleLower: 
		{
			// De-equip all and equip at top
			unequipSlot(EquipSlot::TopHeadgear, src);
			unequipSlot(EquipSlot::MidHeadgear, src);
			unequipSlot(EquipSlot::LowHeadgear, src);
			equipSlot(EquipSlot::TopHeadgear, src, index);	
		}break;

		//**Weapons
		case EquipMask::Shield: 
		{
			//If we're equipping a shield but we had a 2 hand waepon equipped, de-equip weapon
			auto* weap = src._inventory.getEquip(EquipSlot::Weapon);
			if (weap && weap->db()._equipMask == EquipMask::TwoHanded)
				unequipSlot(EquipSlot::Weapon, src);

			unequipSlot(EquipSlot::Shield, src);
			equipSlot(EquipSlot::Shield, src, index);
		}break;
		case EquipMask::Weapon:
		{
			//TODO: dual wield

			unequipSlot(EquipSlot::Weapon, src);
			equipSlot(EquipSlot::Weapon, src, index);
		}break;
		case EquipMask::TwoHanded:
		{
			// Always unequip weapon and shield
			unequipSlot(EquipSlot::Weapon, src);
			unequipSlot(EquipSlot::Shield, src);

			// Equip weapon and also set the index on shield
			equipSlot(EquipSlot::Weapon, src, index);
		}break;

		// accessories will always equip at left if both slots are empty, and overwrite the one at left if both are full
		case EquipMask::Accessory:
		{
			// Check if left is free
			auto* accLeft = src._inventory.getEquip(EquipSlot::Accessory_L);
			if (accLeft == nullptr)
			{
				equipSlot(EquipSlot::Accessory_L, src, index);
				break;
			}
			
			// Left was occupied, check if right is
			auto* accRight = src._inventory.getEquip(EquipSlot::Accessory_R);
			if (accRight == nullptr)
			{
				equipSlot(EquipSlot::Accessory_R, src, index);
				break;
			}

			// Both are full, swap with left accessory
			unequipSlot(EquipSlot::Accessory_L, src);
			equipSlot(EquipSlot::Accessory_L, src, index);
		}break;

		//*********Single slot equips
		case EquipMask::AccessoryLeft: //Should not be in use
		{
			unequipSlot(EquipSlot::Accessory_L, src);
			equipSlot(EquipSlot::Accessory_L, src, index);
		}break;
		case EquipMask::AccessoryRight:  //Should not be in use
		{
			unequipSlot(EquipSlot::Accessory_R, src);
			equipSlot(EquipSlot::Accessory_R, src, index);
		}break;
		case EquipMask::Garment:
		{
			unequipSlot(EquipSlot::Garment, src);
			equipSlot(EquipSlot::Garment, src, index);
		}break;
		case EquipMask::Armor:
		{
			unequipSlot(EquipSlot::Armor, src);
			equipSlot(EquipSlot::Armor, src, index);
		}break;
		case EquipMask::Shoes:
		{
			unequipSlot(EquipSlot::Shoes, src);
			equipSlot(EquipSlot::Shoes, src, index);
		}break;
		case EquipMask::Ammunition: 
		{
			unequipSlot(EquipSlot::Ammunition, src);
			equipSlot(EquipSlot::Ammunition, src, index);
		}break;
		default:
		{
			Log::debug("Item mask not supported: %d", TAG, item.db()._equipMask.getValue());
		}break;
	}
	
	return true;
}

bool ItemController::unequipItem(const Item& item, Character& src)
{
	// Check if we can de-equip the item

	unequipSlot(item._equipSlot, src);
	return true;
}

bool ItemController::unequipSlot(EquipSlot slot, Character& src)
{
	auto* item = static_cast<IEquip&>(src._inventory).unsetEquip(slot);

	if (item == nullptr) //Had nothing to remove
		return false;
	
	_map._itemScripts.useItem(*item, src, OperationType::Revert);
	_map._itemScripts.unequipItem(*item, src, OperationType::Apply); //Dequips should not have reverts

	//TODO: remove defense and attack stats

	return true;
}

void ItemController::equipSlot(EquipSlot slot, Character& src, uint8_t index)
{
	//Set equip to slot
	auto* item = static_cast<IEquip&>(src._inventory).setEquip(slot, index);
	assert(item != nullptr);

	//Call on equip item
	_map._itemScripts.useItem(*item, src, OperationType::Apply);

	//TODO: apply defense and attack stats as those don't come in the script
}

bool ItemController::useItem(const Item& item, Character& src)
{
	//TODO: check if we can use item

	_map._itemScripts.useItem(item, src, OperationType::Apply);
	return true;
}

void ItemController::removeFieldItem(FieldItem& item)
{
	// remove it from block grid
	_map._blockGrid.remove(item);

	_blockArray.release(item);
	_fieldItemPool.delete_(&item);
}

FieldItem& ItemController::createFieldItem(ItemDbId id, uint16_t amount)
{
	auto* item = _fieldItemPool.new_(id, amount);
	_blockArray.insert(*item);

	return *item;
}