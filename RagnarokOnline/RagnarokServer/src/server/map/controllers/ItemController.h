#pragma once

#include <server/map_objects/FieldItem.h>
#include <server/map_objects/Character.h>
#include <server/map/DynamicBlockArray.hpp>
#include <server/map/IMapLoop.hpp>
#include <server/network/PacketHandlerResult.h>

#include <sdk/timer/Reschedule.h>
#include <sdk/pool/CachedPool.hpp>

namespace packet {
	struct Packet;
}

class Map;
class Unit;
class Character;

class ItemController final
	: public IMapLoop<Map, ItemController>
{
public:
	ItemController(Map& map, ConcurrentPool<FieldItem>& pool);

	//utility
	void spawnItemAroundPosition(ItemDbId itemDbId, const Point& position);
	bool tryPickUpItem(Unit& unit, FieldItem& item);
	bool canUnitPickUpItem(const Unit& unit, const FieldItem& item) const;
	bool stripEquipSlots(Character& src, EquipMask slots);

	//other
	void onMoveActionPickUpItemReached(Unit& unit, uint32_t moveTick);

	//Packet handlers
	PacketHandlerResult onRCV_PickUpItem(const packet::Packet& rawPacket, Character& src);
	PacketHandlerResult onRCV_DropItem(const packet::Packet& rawPacket, Character& src);
	PacketHandlerResult onRCV_UseItem(const packet::Packet& rawPacket, Character& src);
	PacketHandlerResult onRCV_UnequipItem(const packet::Packet& rawPacket, Character& src);

private:
	friend IMapLoop<Map, ItemController>;

	void onEndMapLoop();

	//Packet notifications
	void notifyItemSpawn(const FieldItem& item) const;
	void notifyItemDespawn(const FieldItem& item)  const;
	void notifyItemPickUp(const Unit& unit, const FieldItem& item) const;
	void notifyFailToPickUpItem(const Unit& unit) const;

	//Utility methods
	Point getFreeCellAroundPosition(const Point& position) const;
	void spawnItemAtPosition(FieldItem& item, const Point& position);
	Reschedule despawnItem(int32_t itemIndex, Tick);
	void doPickUpItem(Unit& unit, FieldItem& item);

	bool equipItem(const Item& item, uint8_t index, Character& src);  //Replaces item in all slots it ocupies
	bool unequipItem(const Item& item, Character& src); //removes item from all slots it occupies
	bool unequipSlot(EquipSlot slot, Character& src); //Removes a single slot and calls deEquip script
	void equipSlot(EquipSlot slot, Character& src, uint8_t index); //Adds a single slot and calls equip and use
	bool useItem(const Item& item, Character& src); //Calls on use for item

	//Methods for timers
	Reschedule runUnitMoveActionItemPickUp(int32_t blockId, Tick);

	// create/destroy field item
	FieldItem& createFieldItem(ItemDbId id, uint16_t amount);
	void removeFieldItem(FieldItem& itemBlock);

	Map& _map;
	DynamicBlockArray<FieldItem> _blockArray;
	CachedPool<FieldItem> _fieldItemPool;
};