#pragma once

#include <server/common/Direction.h>
#include <server/common/CommonTypes.h>
#include <server/common/NpcId.h>
#include <server/map_objects/Block.h>

class Npc
	: public Block
{
public:
	constexpr static int16_t INVALID_NPC_ID = static_cast<int16_t>(-1);
	constexpr static int16_t WARP_PORTAL_DB_ID = INT16_MAX;

	Npc(NpcId npcId, uint8_t index)
		: Block(BlockType::Npc)
		, _npcId(npcId)
		, _mapDataIndex(index)
		, _enabled(true)
	{}

	bool isWarp() const
	{
		return _npcId == WARP_PORTAL_DB_ID;
	}

	struct
	{
		uint8_t _enabled : 1,
			_reserved : 7;
	};

	Direction _direction = Direction::Down;
	const NpcId _npcId;

private:
	friend class NpcController;
	uint8_t _mapDataIndex;
};