#pragma once

#include <server/common/Point.h>
#include <server/common/MapId.h>
#include <server/common/Direction.h>
#include <server/common/MonsterId.h>
#include <server/common/NpcId.h>
#include <server/map/data/Tile.h>

#include <sdk/array/FixedSizeArray.hpp>
#include <sdk/array/FixedSizeArray2D.hpp>

class MapData final
{
public:
	struct Monster 
	{
		MonsterId _monsterId;
		uint16_t _spawnMinSeconds;
		uint16_t _spawnVariance;
		uint8_t _amount;
		uint8_t _level;
		Point _spawnPoint;
		Point _spawnRadius;
	};

	struct WarpPortal
	{
		Point _position;
		Point _destinationPos;
		MapId _destinationMap;
		uint8_t _spanX;
		uint8_t _spanY;
		Direction _facing;
	};

	struct Npc
	{
		NpcId _id;
		Point _position;
		uint8_t _spanX;
		uint8_t _spanY;
		Direction _direction;
	};

	MapId _mapId;
	MapGroupId _groupId;
	FixedSizeArray2D<Tile, uint16_t> _tiles;
	FixedSizeArray<WarpPortal, uint8_t> _warpPortals;
	FixedSizeArray<Monster, uint16_t> _mobs;
	FixedSizeArray<Npc, uint16_t> _npcs;
	uint16_t _numOfMonsters;
	bool _isPvpMap = false;
};