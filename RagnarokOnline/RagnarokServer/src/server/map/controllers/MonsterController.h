#pragma once

#include <server/common/PacketEnums.h>
#include <server/map_objects/Monster.h>
#include <server/map/FixedBlockArray.hpp>
#include <server/map/DynamicBlockArray.hpp>
#include <server/map/IMapLoop.hpp>

#include <sdk/pool/CachedPool.hpp>
#include <sdk/timer/Reschedule.h>

#include <cstdint>

class Map;

class MonsterController final
	: public IMapLoop<Map, MonsterController>
{
public:
	MonsterController(Map& map, ConcurrentPool<Monster>& pool);
	void processMobs();

	void killMonster(Monster& mob);

	void notifyKillMonster(const Monster& mob);
	void notifyStatusChange(const Monster& src, MonsterChangeType type, uint32_t value) const;

private:
	friend IMapLoop<Map, MonsterController>;

	//Called by Map once per loop
	void onEndMapLoop();

	Reschedule runLazyAiTimer(int32_t, Tick);
	void runLazyAi(Monster& mob);

	void randomWalk(Monster& mob);
	void spawnFixedMob(Monster& mob);

	//TODO:: move this elsewhere later
	void startMove(Monster& mob, Point targetPos);

	Map& _map;
	FixedBlockArray<Monster> _fixedMonsters;
	DynamicBlockArray<Monster> _dynamicMonsters;
	CachedPool<Monster> _monsterPool;
};