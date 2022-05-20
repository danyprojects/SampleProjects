#pragma once

#include <sdk/Tick.h>

class Map;

template<typename Friend, typename T>
class IMapLoop;

template<typename Controller>
class IMapLoop<Map, Controller>
{
	friend Map;

	void I_beginMapLoop()
	{
		static_cast<Controller*>(this)->onBeginMapLoop();
	}

	void I_endMapLoop()
	{
		static_cast<Controller*>(this)->onEndMapLoop();
	}
};

template<typename Friend>
class IMapLoop<Friend, Map>
{
	friend Friend;

	bool I_mapLoop(Tick tick)
	{
		return static_cast<Map*>(this)->onMapLoop(tick);
	}
};