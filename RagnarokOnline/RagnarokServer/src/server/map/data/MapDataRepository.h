#pragma once

#include <server/common/ServerLimits.h>
#include <server/map/data/MapData.h>

#include <sdk/enum_cast.hpp>

#include <array>

class MapDataRepository
{
public:
	MapDataRepository();

	const MapData& get(MapId _id) const
	{
		return _mapData[enum_cast(_id)];
	}

private:
	std::array<MapData, MAX_MAP_ID> _mapData;
};

