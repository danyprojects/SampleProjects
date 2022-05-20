#include "MonsterController.h"

#include <server/map/Map.h>
#include <server/map/data/MapData.h>
#include <server/common/ServerLimits.h>
#include <server/network/Packets.h>

#include <sdk/log/Log.h>

namespace
{
	#define TIMER_CB(Callback) TimerCb<MonsterController, &MonsterController::Callback>()

	constexpr char* const TAG = "MobCtrl";

	// milliseconds
	constexpr uint32_t MOB_HARD_AI_THINK_TIME = 100;
	// milliseconds
	constexpr uint32_t MOB_LAZY_AI_THINK_TIME = MOB_HARD_AI_THINK_TIME * 10;
	// milliseconds
	constexpr uint32_t MIN_RANDOM_WALK_TIME   = 4000;

	FixedSizeArray<Monster>::Initializer initializeMonsters(const MapData& mapData)
	{
		FixedSizeArray<Monster>::Initializer initializer(mapData._numOfMonsters);

		int16_t i = 0;
		for (auto& mob : mapData._mobs)
		{
			for (int k = 0; k < mob._amount; k++)
				initializer.emplaceBack(mob._monsterId, mob._level, i);
			i++;
		}

		return initializer;
	}
}

MonsterController::MonsterController(Map &map, ConcurrentPool<Monster>& pool)
	:_map(map)
	,_fixedMonsters(_map._blockArray, initializeMonsters(map._mapData))
	,_dynamicMonsters(_map._blockArray)
	,_monsterPool(pool)
{	
	for (auto& monster : _fixedMonsters)
		spawnFixedMob(monster);

	_map._timerController.add(this, _map._currentTick + MOB_LAZY_AI_THINK_TIME, TIMER_CB(runLazyAiTimer));
}

void MonsterController::onEndMapLoop()
{
	_monsterPool.releaseCache();
}

void MonsterController::processMobs()
{
	for (auto& mob : _fixedMonsters)
	{
		if (mob.isDead())
			continue;

		if (mob._isMoving)
			_map._unitController.doUnitMove(mob);
	}
}

//Packet notifications
void MonsterController::notifyKillMonster(const Monster& mob)
{
	const auto packet = packet::SND_BlockDied(mob._id);

	_map._blockGrid.runActionInRange(mob, DEFAULT_VIEW_RANGE,
		[&](Block& block)
	{
		if (block._type == BlockType::Character)
		{
			auto blockSessionId = block._id;
			_map.writePacket(blockSessionId, packet);
		}
	});
}

void MonsterController::notifyStatusChange(const Monster& src, MonsterChangeType type, uint32_t value) const
{
	auto packet = packet::SND_MonsterStatusChange(src._id, type, value);

	//This is the packet that will make items disappear and OPTIONALLY make player show pickup animation. So run with item as center
	_map._blockGrid.runActionInRange(src, DEFAULT_VIEW_RANGE,
		[&](Block& block)
		{
			if (block._type == BlockType::Character)
				_map.writePacket(block._id, packet);
		});
}

Reschedule MonsterController::runLazyAiTimer(int32_t, Tick)
{
	for (auto& mob : _fixedMonsters)
		runLazyAi(mob);

	// TODO:: loop dynamic mobs also here if available 

	return Reschedule::YES(_map._currentTick + MOB_LAZY_AI_THINK_TIME);
}

void MonsterController::runLazyAi(Monster& mob)
{
	if (mob.isDead())
		return;

	const auto tick = _map._currentTick;
	if (tick - mob._lastThinkTime < MOB_LAZY_AI_THINK_TIME)
		return;

	mob._lastThinkTime = tick;

	// TODO:: check if this mob is slave and if so run slave ai instead, and then return

	if (mob.db().flag.canMove && tick >= mob._nextWalkTime && _map._unitController.canMove(mob))
	{		
		if (mob.isSpotted() || true) // TODO:: Hack until we implement hard AI
			randomWalk(mob);
	}
	else if (!mob._isMoving)
	{
		// TODO:: here we will have chance to cast idle skill check hercules
	}
}

void MonsterController::randomWalk(Monster& mob)
{
	auto distance = mob._moveFailCount;
	distance = distance < 5 ? 5 : distance > 7 ? 7 : distance;

	int i;
	for (i = 20; i > 0; i--) // 20 retries
	{
		// search for a movable place
		auto rnd = _map._rand();
		int16_t x = rnd % (distance * 2 + 1) - distance;
		int16_t y = rnd / (distance * 2 + 1) % (distance * 2 + 1) - distance;

		if (x == 0 && y == 0) // don't allow current cell
			continue;

		x += mob._position.x;
		y += mob._position.y;

		auto tile = _map.tryGetTile(x, y);
		if (tile && tile->walkable && _map._unitController.trySetUnitMoveDestination(mob, { x, y }, PathType::Hard))
		{
			startMove(mob, { x, y });
			break;
		}
	}

	if (i < 0) 
	{
		mob._moveFailCount++;

		if (mob._moveFailCount > 1000) 
		{
			Log::warning("MOB can't move. spawn id=%d, db=%d, mapDb=%s (%d,%d)\n", TAG, mob._id, mob.db().id, _map._mapData._mapId, mob._position.x, mob._position.y);
			mob._moveFailCount = 0;
				
			//TODO:: respawn
		}
		return;
	}

	//Should we actually move this calculation to the unit controller?
	auto speed = mob._walkSpd;
	auto diagonalSpeed = static_cast<uint32_t>(DIAGONAL_TO_UNIT_SIZE * speed / CELL_TO_UNIT_SIZE);

	uint32_t walkTime = 0;
	auto prev = mob._position;

	for (i = 0; i < mob._movementData.pathLength; i++)
	{
		// The next walk start time is calculated.
		Point next = mob._movementData.path[i];

		if (next.x != prev.x && next.y != prev.y)
			walkTime += diagonalSpeed;
		else
			walkTime += speed;

		prev = next;
	}

	mob._moveFailCount = 0;
	mob._nextWalkTime = _map._currentTick + _map._rand() % 1000 + MIN_RANDOM_WALK_TIME + walkTime;
}

void MonsterController::spawnFixedMob(Monster& monster)
{
	auto& mobData = _map._mapData._mobs[monster._spawnDataIndex];
	auto& rad = mobData._spawnRadius;
	auto& pos = mobData._spawnPoint;

	do
	{
		monster._position = 
		{ 
			pos.x ? Map::_rand() % (rad.x*2 + 1) - rad.x + pos.x
					: Map::_rand() % (_map._tiles.width() - 2) + 1,
			pos.y ? Map::_rand() % (rad.y * 2 + 1) - rad.y + pos.y
					: Map::_rand() % (_map._tiles.height() - 2) + 1,
		};
		assert(monster._position.x < _map._tiles.width() && monster._position.y < _map._tiles.height());
	} while (!_map._tiles(monster._position.x, monster._position.y).walkable);

	monster.spawn();
	monster._direction = Direction::Down;
	_map._blockGrid.push(monster);
}

void MonsterController::startMove(Monster& mob, Point targetPos)
{
	const short startDelay = mob._movementData.nextMoveTick - _map._currentTick;

	const auto packet = packet::SND_MonsterMove(mob._id, mob._position, targetPos, startDelay);

	_map._blockGrid.runActionInRange(mob, DEFAULT_VIEW_RANGE,
		[&](Block& block)
	{
		if (block._type == BlockType::Character)
		{
			auto blockSessionId = block._id;
			_map.writePacket(blockSessionId, packet);
		}
	});
}

void MonsterController::killMonster(Monster& mob)
{
	mob._hp = 0;

	//TODO: need to check if need any clears
	mob._isCasting = false; 
	mob._isMoving = false;

	auto rnd = _map._rand();

	//spawn drops
	for (const auto& drop : mob.db().drops)
	{
		if (drop.dropChance * BASE_DROP_RATE < static_cast<uint32_t>(rnd % 10000))
			continue;

		_map._itemController.spawnItemAroundPosition(drop.id, mob._position);
	}

	for (const auto& drop : mob.db().mvpDrops)
	{
		if (drop.dropChance * BASE_DROP_RATE < static_cast<uint32_t>(rnd % 10000))
			continue;

		_map._itemController.spawnItemAroundPosition(drop.id, mob._position);
	}
}