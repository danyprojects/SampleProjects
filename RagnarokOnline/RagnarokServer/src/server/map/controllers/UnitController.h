#pragma once

#include <server/common/Point.h>
#include <server/common/Direction.h>
#include <server/common/CommonTypes.h>
#include <server/common/PacketEnums.h>
#include <server/network/PacketHandlerResult.h>

#include <sdk/timer/Reschedule.h>

namespace packet {
	struct Packet;
}

class Skill;
class Map;
class Unit;

class UnitController final
{
public:
	UnitController::UnitController(Map &map)
		:_map(map)
	{}

	//************** Utility methods
	bool canMove(Unit& unit) const;

	bool canCast(Unit& unit) const;

	// This will check if it's a character to also set head direction
	void setUnitDirection(Unit& unit, Direction direction);

	bool trySetUnitMoveDestination(Unit& unit, Point destination, const PathType pathType);

	// Returns true if casted, casting or moving to cast, false otherwise
	bool tryCastSkillAtPosition(Unit& source, Point position, Skill& skill, const uint8_t skillLevel);

	// Returns true if casted, casting or moving to cast, false otherwise
	bool tryCastSkillAtTarget(Unit& source, Unit& target, Skill& skill, const uint8_t skillLevel);

	CastTime calculateCastTime(const Unit& src, const Skill& skill, const uint8_t skillLevel);

	void applyMagicDamage(Unit& src, Unit& target, const uint32_t damage, DamageHitType dmgHitType);

	// Returns if player is still valid (maybe it stepped on a warp)
	[[nodiscard]] bool knockbackUnit(Unit& target, Direction direction, int distance);

	// Returns if player is still valid (maybe it stepped on a warp)	
	[[nodiscard]] bool executeStepOnActions(Unit& unit);

	//************** Packet notification methods
	void notifyUnitReceiveDamage(const Unit& target, const uint32_t damage, DamageHitType dmgHitType);
		
	void notifyUnitStopMove(const Unit& unit);

	void notifyUnitStartMove(const Unit& unit, const Point position); 

	void notifyUnitLeaveRangeBorder(const Unit& unit, int range, Direction direction);

	void notifyUnitEnterRangeBorder(const Unit& unit, int range, Direction direction);

	void notifyUnitSnapToCell(const Unit& unit, const Point destination);

	// Public for now change it if this is done via timers
	void doUnitMove(Unit& unit);

	// Called when cast time finishes only. For instant skill dispatches use skill controller dispatches
	void onUnitCastTimeFinished(Unit& unit);

	//************* Packet handlers
	PacketHandlerResult onRCV_PlayerMove(const packet::Packet& rawPacket, Unit& src);
private:
	void checkCancelMovement(Unit& unit);
	void updateUnitDirection(Unit& unit, int nextX, int nextY);

	// Called everytime after a doUnitMove to check for move actions
	void checkUnitMoveAction(Unit& unit, uint32_t moveTick);

	//******** Methods for timers
	Reschedule runUnitMoveActionCastAoE(int32_t blockId, Tick);
	Reschedule runUnitMoveActionCastTarget(int32_t blockId, Tick);

	Map& _map;
};