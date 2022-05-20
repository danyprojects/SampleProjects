#pragma once

#include <cstdint>
#include <sdk/enum_cast.hpp>


// unique unchangable Map ids, all instance Maps must go after LastNonInstance
// the enumeration MUST, be contiguous so don't assign values directly
enum class MapId : uint16_t
{
	GlKnt01,
	Glast01,
	Izlude,
	Valkyrie,
	Gonryun,
	LastNonInstance = Gonryun,
	Max
};

enum class MapGroupId : uint16_t
{
	NonInstance,
	Max
};


constexpr auto MAX_MAP_ID = enum_cast(MapId::Max);
constexpr auto LAST_NON_INSTANCE_MAP_ID = enum_cast(MapId::LastNonInstance);

typedef uint16_t MapInstanceIndex;

struct MapGroupTableEntry
{
	constexpr MapGroupTableEntry(MapGroupId _id, MapInstanceIndex max)
		:_max(max)
	{}

	MapInstanceIndex _max;
};


constexpr MapGroupTableEntry MAP_GROUP_TABLE[enum_cast(MapGroupId::Max)] =
{
	{ MapGroupId::NonInstance, 0 }/*,
	{ MapGroupId::EndlessTower, 4 },
	{ MapGroupId::OrcDungeon, 4},
	{ MapGroupId::Nidhogg, 4},
	{ MapGroupId::BattleGround1, 4}*/
};

constexpr auto BIGGEST_MAP_INSTANCE_COUNT =
[]() constexpr
{
	MapInstanceIndex size = 0;
	for (auto && value : MAP_GROUP_TABLE)
		size = value._max > size ? value._max : size;

	return size;
}();

constexpr auto TOTAL_MAP_INSTANCES =
[]() constexpr
{
	MapInstanceIndex size = 0;
	for (auto && value : MAP_GROUP_TABLE)
		size += value._max;

	return size;
}();
