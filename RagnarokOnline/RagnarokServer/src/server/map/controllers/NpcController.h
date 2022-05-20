#pragma once

#include <server/common/NpcDb.h>
#include <server/map_objects/BlockId.h>
#include <server/map_objects/Npc.h>
#include <server/map/FixedBlockArray.hpp>
#include <server/network/PacketHandlerResult.h>

#include <cstdint>

class Map;
class Character;

class NpcController final
{
public:

	NpcController(Map& map);

	bool isInRangeOfPortal(const Npc& npc, const Point& position) const;

	void executePortal(Npc& warp, Character& character); //warp isnt const as we might want to disable it after use

	//packet handlers
	PacketHandlerResult onRCV_NpcAction(const packet::Packet& rawPacket, Character& src);

private:
	void runShop(Character& src, NpcSubId npcSubId);

	Map& _map;
	FixedBlockArray<Npc> _fixedWarps;
	FixedBlockArray<Npc> _fixedNpcs;
};