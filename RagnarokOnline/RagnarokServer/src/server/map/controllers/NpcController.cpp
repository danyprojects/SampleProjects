#include "NpcController.h"

#include <server/network/Packets.h>
#include <server/map/Map.h>
#include <server/map/data/MapData.h>
#include <server/common/ServerLimits.h>


namespace
{
	FixedSizeArray<Npc>::Initializer initializeWarps(const MapData& mapData)
	{
		FixedSizeArray<Npc>::Initializer initializer(mapData._warpPortals.size());

		for (int16_t i = 0; i < mapData._warpPortals.size(); i++)
			initializer.emplaceBack(Npc::WARP_PORTAL_DB_ID, i);

		return initializer;
	}

	FixedSizeArray<Npc>::Initializer initializeNpcs(const MapData& mapData)
	{
		FixedSizeArray<Npc>::Initializer initializer(mapData._npcs.size());

		for (int16_t i = 0; i < mapData._npcs.size(); i++)
			initializer.emplaceBack(mapData._npcs[i]._id, i);
		
		return initializer;
	}
}

NpcController::NpcController(Map &map)
	: _map(map)
	, _fixedWarps(_map._blockArray, initializeWarps(map._mapData))
	, _fixedNpcs(_map._blockArray, initializeNpcs(map._mapData))
{ 
	//Initialize position 
	for (auto& warp : _fixedWarps)
	{
		//Disable warps whose destination map is not existent
		warp._enabled = static_cast<uint16_t>(_map._mapData._warpPortals[warp._mapDataIndex]._destinationMap) != UINT16_MAX;
		warp._position = _map._mapData._warpPortals[warp._mapDataIndex]._position;
		_map._blockGrid.push(warp);
	}

	for (auto& npc : _fixedNpcs)
	{
		npc._position = _map._mapData._npcs[npc._mapDataIndex]._position;
		npc._direction = _map._mapData._npcs[npc._mapDataIndex]._direction;
		_map._blockGrid.push(npc);
	}
}

PacketHandlerResult NpcController::onRCV_NpcAction(const packet::Packet& rawPacket, Character& src)
{
	const auto& packet = reinterpret_cast<const packet::RCV_NpcAction&>(rawPacket);

	auto* block = _map._blockArray.get(packet.npcBlockId);

	if(block == nullptr || block->_type != BlockType::Npc)
		return PacketHandlerResult::CONSUMED(packet);

	auto& npc = reinterpret_cast<Npc&>(*block);

	if(!npc._enabled)
		return PacketHandlerResult::CONSUMED(packet);

	//TODO: make use of packet action

	auto& npcData = NpcDb::getNpc(npc._npcId);

	if (npcData._isShop)
		runShop(src, npcData._subId);
	else
		_map._npcScripts.runScript(src, npcData._subId);

	return PacketHandlerResult::CONSUMED(packet);
}

void NpcController::executePortal(Npc& warp, Character& character)
{
	auto& warpData = _map._mapData._warpPortals[warp._mapDataIndex];
		
	character._direction = warpData._facing;
	character._headDirection = warpData._facing;

	//Do not update position, it will get updated by the methods we're calling once it notifies leave ranges to current position
	if (warpData._destinationMap == _map._mapData._mapId)		
		_map._characterController.warpToCell(character, warpData._destinationPos, CharacterController::WarpType::MapPortal);
	else
	{
		//Notify warp out before changing character position
		_map._characterController.notifyWarpOut(character, CharacterController::WarpType::MapPortal);

		character._position = warpData._destinationPos;
		_map.swapToMap(character, warpData._destinationMap);
	}
}

bool NpcController::isInRangeOfPortal(const Npc& warp, const Point& position) const
{
	auto& warpData = _map._mapData._warpPortals[warp._mapDataIndex];

	return Pathfinder::isInRectangularDistance(warp._position, position, warpData._spanX, warpData._spanY);
}

void NpcController::runShop(Character& src, NpcSubId npcSubId)
{
	auto& shop = NpcDb::getShop(npcSubId);

	using Packet = packet::SND_OpenNpcShop;
	std::aligned_storage<Packet::MAX_SIZE(), alignof(Packet)>::type raw;

	auto& packet = *reinterpret_cast<packet::SND_OpenNpcShop*>(&raw);

	packet.writeHeader(shop._currency, 0, shop._items.size());

	// write the indexed skills first
	for (int i = 0; i < shop._items.size(); i++)
	{
		packet._itemInfos[i].dbId = shop._items[i]._itemId;
		packet._itemInfos[i].price = shop._items[i]._price;
	}

	_map.writePacket(src, { (std::byte*)&packet, static_cast<uint8_t>(packet._totalSize) });
}