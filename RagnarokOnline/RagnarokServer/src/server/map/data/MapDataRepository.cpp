#include "MapDataRepository.h"

#include <server/common/GlobalConfig.h>

#include <cassert>
#include <fstream>
#include <stdexcept>
#include <filesystem>

namespace fs = std::filesystem;

MapDataRepository::MapDataRepository()
{
	const auto dataDir = fs::path(GlobalConfig::dataPath()).append("fields");

	for (auto& entry : fs::directory_iterator(dataDir))
	{
		std::ifstream file;
		file.open(entry, std::ios::in | std::ios::binary); //let it throw on failure
		
		const auto readByte = [&]() 
		{
			char value;
			file.read(&value, sizeof(value));
			return static_cast<uint8_t>(value);
		};

		const auto readShort = [&]()
		{
			uint16_t value;
			file.read(reinterpret_cast<char*>(&value), sizeof(value));
			return value;
		};
		
		// Magic
		if (readShort() != 0xCAFE)
			throw std::runtime_error(entry.path().string() + ": Invalid map file format");

		// MapId and fetch MapData
		const uint16_t mapId  = readShort();
		MapData& mapData	  = _mapData[mapId];
		mapData._mapId		  = static_cast<MapId>(mapId);

		// GroupId
		mapData._groupId = static_cast<MapGroupId>(readShort());

		// TileFlags
		{
			uint16_t mapWidth  = readShort();
			uint16_t mapHeight = readShort();

			//array was empty so call contructor with placement new
			assert(mapData._tiles.empty());
			new (&mapData._tiles) decltype(mapData._tiles)(mapWidth, mapHeight);

			file.read(reinterpret_cast<char*>(mapData._tiles.data()), mapWidth * mapHeight);
		}

		// Warps
		if(uint8_t warpCount = readByte())
		{
			//array was empty so call contructor with placement new
			assert(mapData._warpPortals.empty());
			new (&mapData._warpPortals) decltype(mapData._warpPortals)(warpCount);

			file.read(reinterpret_cast<char*>(mapData._warpPortals.data()), warpCount * sizeof(MapData::WarpPortal));
		}

		// Mobs
		if(uint8_t mobCount = readByte())
		{
			//array was empty so call contructor with placement new
			assert(mapData._mobs.empty());
			new (&mapData._mobs) decltype(mapData._mobs)(mobCount);
			
			file.read(reinterpret_cast<char*>(mapData._mobs.data()), mobCount * sizeof(MapData::Monster));
		}

		//Count the amount of mobs
		mapData._numOfMonsters = 0;
		for (int i = 0; i < mapData._mobs.size(); i++)
			mapData._numOfMonsters += mapData._mobs[i]._amount;

		// Npcs
		if (uint8_t npcCount = readByte())
		{
			assert(mapData._npcs.empty());
			new (&mapData._npcs) decltype(mapData._npcs)(npcCount);

			file.read(reinterpret_cast<char*>(mapData._npcs.data()), npcCount * sizeof(MapData::Npc));
		}
	}

	for (auto& map : _mapData)
		if(map._tiles.empty())
			throw std::runtime_error("Found uninitialized map");
}