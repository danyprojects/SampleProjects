#include "UnitController.h"

#include <server/map/Map.h>
#include <server/network/Packets.h>
#include <server/map_objects/Unit.h>
#include <server/map_objects/FieldSkill.h>
#include <server/map_objects/Character.h>
#include <server/map_objects/Monster.h>

using Packet = packet::Packet;

namespace
{
	#define TIMER_CB(Callback) TimerCb<UnitController, &UnitController::Callback>()

	// To use as index for delays in the array below
	enum class HitlockDelayLevel
	{
		None = 0,
		VerySmall,
		Small,
		Medium,
		High,
		VeryHigh,
		Huge
	};

	// Delays are in the form of base delay * number of frames
	constexpr uint32_t HITLOCK_DELAYS[] = { ACTION_DELAY_BASE_TIME * 0,
											ACTION_DELAY_BASE_TIME * 1,
											ACTION_DELAY_BASE_TIME * 3,
											ACTION_DELAY_BASE_TIME * 5,
											ACTION_DELAY_BASE_TIME * 7,
											ACTION_DELAY_BASE_TIME * 9,
											ACTION_DELAY_BASE_TIME * 20 }; // this is half a second
}

//************ Utility methods
bool UnitController::canMove(Unit& unit) const
{
	//todo:  free cast skill from sage later. Status checks for immobility
	if (unit._isCasting || unit._animDelayEnd > _map._currentTick)
		return false;

	//TODO check status changes like freeze curset etc

	return true;
}

bool UnitController::canCast(Unit& unit) const
{
	//TODO: check status change like silence and stone curse
	if (unit._isCasting)
		return false;

	return true;
}

void UnitController::setUnitDirection(Unit& unit, Direction direction)
{
	unit._direction = direction;

	if (unit._type == BlockType::Character)
		reinterpret_cast<Character&>(unit)._headDirection = unit._direction;
}

bool UnitController::trySetUnitMoveDestination(Unit& unit, Point destination, const PathType pathType)
{
	//Get path
	unit._movementData.pathLength = _map._pathfinder.findPath(unit._position, destination, pathType, unit._movementData.path);

	if (unit._movementData.pathLength == 0)
	{
		unit._isMoving = false;
		return false;
	}

	//Got valid path
	//If we are moving do not update move time so we wait for previous movement to finish 
	if (!unit._isMoving)
		unit._movementData.nextMoveTick = _map._currentTick;

	unit._movementData.moveIndex = 0;
	unit._isMoving = true;

	return true;
}

bool UnitController::tryCastSkillAtPosition(Unit& source, Point position, Skill& skill, const uint8_t skillLevel)
{
	int distance = Pathfinder::CalculateDiagonalDistance(source._position, position);

	if (distance > MAX_WALK_DISTANCE)
		return false;

	//Clear previous move and action in case of early return
	source._moveAction = MoveAction::None;
	source._isMoving = false;

	CastTime castTime = calculateCastTime(source, skill, skillLevel);

	//If it's not in LoS or not in range of cast, start a movement action without checking cooldown and requirements (same as RO)
	if (distance > skill.db()._range[skillLevel - 1] || !_map._pathfinder.isInLineOfSight(source._position, position))
	{
		//Start a move to target position
		if (!canMove(source) || !trySetUnitMoveDestination(source, position, PathType::Weak))
			return false;

		notifyUnitStartMove(source, position);

		//Set the data to trigger skill cast once in range
		source._moveAction = MoveAction::CastAoE;

		//Store the cast data 
		source._isCasting = false;
		source._castData.skill = &skill;
		source._castData.skillLevel = skillLevel;
		source._targetData.targetPosition = position;
	}
	//If we're already in LoS and range of cast, check cast requirements before we start casting
	else if (canCast(source) && _map._skillController.checkCastRequirements(source, skill, skillLevel))
	{
		//Turn source to direction of cast
		updateUnitDirection(source, position.x, position.y);

		if (castTime > INSTANT_CASTIME)
		{
			//Store data for casting after cast time has elapsed
			source._isCasting = true;
			source._castData.deadline = _map._currentTick + castTime;
			source._castData.skill = &skill;
			source._castData.skillLevel = skillLevel;
			source._targetData.targetPosition = position;

			//Then notify the cast
			_map._skillController.notifyAoESkillCastStart(source, skill._id, castTime, position);
		}
		else //instant cast, dispatch skill right away
		{
			_map._skillController.dispatchAtTargetPosition(source, skill._id, skillLevel, position);	
			_map._skillController.applyCastRequirements(source, *source._castData.skill, source._castData.skillLevel);
		}
	}
	else //Return false if we didn't meet the cast requirements
		return false;

	//If we got here then we either casted, are casting or are moving, return true
	return true;
}

bool UnitController::tryCastSkillAtTarget(Unit& source, Unit& target, Skill& skill, const uint8_t skillLevel)
{
	int distance = Pathfinder::CalculateDiagonalDistance(source._position, target._position);

	if (distance > MAX_WALK_DISTANCE)
		return false;

	//Clear previous move and action in case of early return
	source._moveAction = MoveAction::None;
	source._isMoving = false;

	CastTime castTime = calculateCastTime(source, skill, skillLevel);

	//If it's not in LoS or not in range of cast, start a movement action without checking cooldown and requirements (same as RO)
	if (distance > skill.db()._range[skillLevel - 1] || !_map._pathfinder.isInLineOfSight(source._position, target._position))
	{
		//Start a move to target position
		if (!canMove(source) || !trySetUnitMoveDestination(source, target._position, PathType::Weak))
			return false;

		notifyUnitStartMove(source, target._position);

		//Set the data to trigger skill cast once in range
		source._moveAction = MoveAction::CastTarget;

		//Store the cast data 
		source._isCasting = false;
		source._castData.skill = &skill;
		source._castData.skillLevel = skillLevel;
		source._targetData.targetPosition = target._position;
		source._targetData.targetBlock = target._id;
	}
	//If we're already in LoS and range of cast, check cast requirements before we start casting
	else if (canCast(source) && _map._skillController.checkCastRequirements(source, skill, skillLevel))
	{
		//Turn source to direction of cast
		updateUnitDirection(source, target._position.x, target._position.y);

		if (castTime > INSTANT_CASTIME)
		{
			//Store data for casting after cast time has elapsed
			source._isCasting = true;
			source._castData.deadline = _map._currentTick + castTime;
			source._castData.skill = &skill;
			source._castData.skillLevel = skillLevel;
			source._targetData.targetBlock = target._id;

			//Then notify the cast
			_map._skillController.notifyTargetSkillCastStart(source, skill._id, castTime, target);
		}
		else //instant cast, dispatch skill right away
		{
			_map._skillController.dispatchAtTargetUnit(source, skill._id, skillLevel, target._id);
			_map._skillController.applyCastRequirements(source, *source._castData.skill, source._castData.skillLevel);
		}
	}
	else //Return false if we didn't meet the cast requirements
		return false;

	//If we got here then we either casted, are casting or are moving, return true
	return true;
}

CastTime UnitController::calculateCastTime(const Unit& src, const Skill& skill, const uint8_t skillLevel)
{
	if (src._type == BlockType::Character)
	{
		//Apply reduction according to different buffs and gears
		return skill.db()._castTime[skillLevel - 1];
	}
	else if(src._type == BlockType::Monster)
	{
		//Get the cast time directly from the monster skill as fixed cast time
		return 0;
	}
	else //homunculus?
	{
		return 0;
	}
}

void UnitController::applyMagicDamage(Unit& src, Unit& target, const uint32_t damage, DamageHitType dmgHitType)
{
	if (damage >= target._hp)
	{
		target._hp = 0;
		if (target._type == BlockType::Monster)
			_map._monsterController.killMonster(reinterpret_cast<Monster&>(target));
	}
	else
		target._hp -= damage;

	//don't bother with animation and cancel movement if target died
	if (!target.isDead())
	{
		//Add animation delay here			
		src._animDelayEnd = _map._currentTick + HITLOCK_DELAYS[enum_cast(HitlockDelayLevel::Medium)]; //For now always use medium delay		

		//do this before receive just for packet ordering in the client. Order has no impact on server
		checkCancelMovement(target);
	}

	//notify that unit has taken damage
	notifyUnitReceiveDamage(target, damage, dmgHitType);

	//Send packet after receive dmg to still trigger dmg animation
	if (target.isDead() && target._type == BlockType::Monster)
		_map._monsterController.notifyKillMonster(reinterpret_cast<Monster&>(target));
}

bool UnitController::knockbackUnit(Unit& target, Direction direction, int distance)
{
	//TODO: Skip knockback during WoE / BG

	//TODO: What about endure? If there are any exceptions that would not make it stop movement, then we cannot reuse the stop move packet for this
	target._isMoving = false;
	target._moveAction = MoveAction::None;

	//Calculate destination cell
	Point destination = target._position;
	int knockedCells = 0;
	Point increments = direction.getIncrements();
	for (; knockedCells < distance; knockedCells++)
	{
		Point nextCell = destination + increments;
		//Stop if we find a non walkable cell before reaching distance
		if (!_map._tiles(nextCell.x, nextCell.y).isWalkable())
			break;

		destination = nextCell;
	}

	//Do nothing if knockback didn't move cell. Player still valid
	if (knockedCells == 0)
		return true;

	//Notify snap first
	notifyUnitSnapToCell(target, destination);

	//Send leave and enter ranges
	for (int i = 0; i < knockedCells; i++)
	{
		notifyUnitLeaveRangeBorder(target, DEFAULT_VIEW_RANGE - i, direction.getInverse());
		notifyUnitEnterRangeBorder(target, DEFAULT_VIEW_RANGE + i + 1, direction);
	}

	//Update target position
	target._position = destination;
	_map._blockGrid.updateBlock(target);

	return executeStepOnActions(target);
}

bool UnitController::executeStepOnActions(Unit& unit)
{
	//TODO: this method needs to be changed so that every action is stored temporarily before executing
	//If there was a warp then it should execute the warp and nothing else

	//TODO: only players need to take npcs into account

	//This is needed so we don't need to stop the run action and just execute warps after
	Block* warp = nullptr;

	_map._blockGrid.runActionInRange(unit, MAX_AURA_RANGE, [&](Block& block)
		{
			//TODO: Check for players / monsters / field skills that might have a step-on configured
			if (unit._type == BlockType::Character && block._type == BlockType::Npc)
			{
				auto& npc = reinterpret_cast<Npc&>(block);
				if (npc._enabled && npc.isWarp())
				{
					if (_map._npcController.isInRangeOfPortal(npc, unit._position))
						warp = &npc;
				}
			}
		});

	//If we stepped on a warp
	//TODO: warped by priest skill
	if (warp != nullptr && unit._type == BlockType::Character)
	{
		unit._isMoving = false; //can't be moving when warping
		if (warp->_type == BlockType::Npc)
		{
			_map._npcController.executePortal(reinterpret_cast<Npc&>(*warp), reinterpret_cast<Character&>(unit));
			return false;
		}
	}

	return true;
}

//************ Packet notification methods

void UnitController::notifyUnitReceiveDamage(const Unit& target, const uint32_t damage, DamageHitType dmgHitType)
{
	auto& targetId = target._id;

	if (target._type == BlockType::Character)
	{
		//notify target player	
		_map.writePacket(targetId, packet::SND_PlayerLocalReceiveDamage(damage, dmgHitType));

		//notify other players
		const auto packet = packet::SND_PlayerOtherReceiveDamage(targetId, damage, dmgHitType);

		_map._blockGrid.runActionInRange(target, DEFAULT_VIEW_RANGE,
			[&](Block& block)
		{
			if (block._type == BlockType::Character)
				_map.writePacket(block._id, packet);
		});
	}
	else if(target._type == BlockType::Monster)
	{
		const auto packet = packet::SND_MonsterReceiveDamage(targetId, damage, dmgHitType);

		_map._blockGrid.runActionInRange(target, DEFAULT_VIEW_RANGE,
			[&](Block& block)
		{
			if (block._type == BlockType::Character)
				_map.writePacket(block._id, packet);
		});
	}
	else // homun or merc
	{
		const auto packet = packet::SND_OtherReceiveDamage(targetId, target._type, damage, dmgHitType);

		_map._blockGrid.runActionInRange(target, DEFAULT_VIEW_RANGE,
			[&](Block& block)
		{
			if (block._type == BlockType::Character)
				_map.writePacket(block._id, packet);
		});
	}
}

void UnitController::notifyUnitStopMove(const Unit& unit)
{
	notifyUnitSnapToCell(unit, unit._position);
}

void UnitController::notifyUnitStartMove(const Unit& unit, const Point position)
{
	const short startDelay = unit._movementData.nextMoveTick - _map._currentTick;

	if (unit._type == BlockType::Character)
	{
		_map.writePacket(unit._id,
			packet::SND_PlayerLocalMove(unit._position, position, startDelay));

		const auto otherPacket = packet::SND_PlayerOtherMove(unit._id, unit._position, position, startDelay);

		_map._blockGrid.runActionInRange(unit, DEFAULT_VIEW_RANGE,
			[&](Block& block)
			{
				if (block._type == BlockType::Character)
					_map.writePacket(block._id, otherPacket);
			});
	}
	else if (unit._type == BlockType::Monster)
	{
		const auto otherPacket = packet::SND_MonsterMove(unit._id, unit._position, position, startDelay);

		_map._blockGrid.runActionInRange(unit, DEFAULT_VIEW_RANGE,
			[&](Block& block)
			{
				if (block._type == BlockType::Character)					
					_map.writePacket(block._id, otherPacket);
			});
	}
	else
	{
		const auto otherPacket = packet::SND_OtherMove(unit._type, unit._id, unit._position, position, startDelay);

		_map._blockGrid.runActionInRange(unit, DEFAULT_VIEW_RANGE,
			[&](Block& block)
			{
				if (block._type == BlockType::Character)
					_map.writePacket(block._id, otherPacket);
			});
	}
}

void UnitController::notifyUnitLeaveRangeBorder(const Unit& unit, int range, Direction direction)
{
	// Check which entities will be leaving the screen. Characters need to check for all block types that left the screen
	// Any other types only need to check if any characters left the screen
	if (unit._type == BlockType::Character)
	{
		const auto ownPlayerLeaveRange = packet::SND_PlayerLeaveRange(unit._id, LeaveRangeType::Default);

		//Run a range check to see what entities left the range of the player
		_map._blockGrid.runActionInRangeBorder(unit, range, direction,
			[&](Block& block)
			{
				if (block._type == BlockType::Character)
				{
					auto blockSessionId = block._id;

					//We notify the unit in processing about who left the screen
					_map.writePacket(unit._id, packet::SND_PlayerLeaveRange(blockSessionId, LeaveRangeType::Default));

					//And also notify the one who left the screen that the unit has left
					_map.writePacket(blockSessionId, ownPlayerLeaveRange);
				}
				else if (block._type == BlockType::Monster)
				{
					auto& monster = reinterpret_cast<Monster&>(block);
					if (monster.isDead()) //Skip if monster is dead
						return;

					packet::SND_MonsterLeaveRange monsterLeaveRange(monster._id, LeaveRangeType::Default);

					_map.writePacket(unit._id, monsterLeaveRange);
				}
				else if (block._type == BlockType::Npc)
				{
					auto& npc = reinterpret_cast<Npc&>(block);
					packet::SND_OtherLeaveRange npcLeaveRange(
						BlockType::Npc, static_cast<uint16_t>(npc._npcId), npc._id, LeaveRangeType::Default);

					_map.writePacket(unit._id, npcLeaveRange);
				}
				else if (block._type == BlockType::Item)
				{
					auto& itemBlock = reinterpret_cast<FieldItem&>(block);
					packet::SND_OtherLeaveRange itemLeaveRange(
						BlockType::Item, static_cast<uint16_t>(itemBlock._itemData._dbId), itemBlock._id, LeaveRangeType::Default);

					_map.writePacket(unit._id, itemLeaveRange);
				}
			});
	}
	else
	{
		//Run a range check to see what entities left the range of the monster
		_map._blockGrid.runActionInRangeBorder(unit, range, direction,
			[&](Block& block)
			{
				if (block._type == BlockType::Character)
				{
					//Notify the one who left the screen that the unit has left
					_map.writePacket(block._id, packet::SND_MonsterLeaveRange(unit._id, LeaveRangeType::Default));
				}
			});
	}
}

void UnitController::notifyUnitEnterRangeBorder(const Unit& unit, int range, Direction direction)
{
	//Send enter ranges (at range DEFAULT_VIEW_RANGE + i)
	if (unit._type == BlockType::Character) // TODO:: should we forward this part to Character controller
	{
		const auto& character = reinterpret_cast<const Character&>(unit);
		const auto& inv = character._inventory;

		const auto ownPlayerEnterRange = packet::SND_PlayerEnterRange(
			character._id, character._position, character._direction, character._headDirection,
			character.getGender(), character.getJob(), character.getHairStyle(),
			inv.getEquipDbId(EquipSlot::TopHeadgear), inv.getEquipDbId(EquipSlot::MidHeadgear), inv.getEquipDbId(EquipSlot::LowHeadgear),
			inv.getEquipDbId(EquipSlot::Weapon), inv.getEquipDbId(EquipSlot::Shield),
			character._walkSpd, character._atkSpd, EnterRangeType::Default);

		//Run a range check in players to see what entities entered the range of the player
		_map._blockGrid.runActionInRangeBorder(unit, range, direction,
			[&](Block& block)
			{
				if (block._type == BlockType::Character)
				{
					auto& otherCharacter = reinterpret_cast<Character&>(block);
					const auto& inv = otherCharacter._inventory;

					packet::SND_PlayerEnterRange otherPlayerEnterRange(
						block._id, otherCharacter._position, otherCharacter._direction, otherCharacter._headDirection,
						otherCharacter.getGender(), otherCharacter.getJob(), otherCharacter.getHairStyle(),
						inv.getEquipDbId(EquipSlot::TopHeadgear), inv.getEquipDbId(EquipSlot::MidHeadgear), inv.getEquipDbId(EquipSlot::LowHeadgear),
						inv.getEquipDbId(EquipSlot::Weapon), inv.getEquipDbId(EquipSlot::Shield),
						otherCharacter._walkSpd, otherCharacter._atkSpd, EnterRangeType::Default);

					//We notify the player who moved when something enters the screen
					_map.writePacket(unit._id, otherPlayerEnterRange);

					//We also need to check if the player that entered the screen is moving to notify the player moving
					if (otherCharacter._isMoving)
					{
						short startDelay = otherCharacter._movementData.nextMoveTick - _map._currentTick;
						Point destination;
						destination = otherCharacter._movementData.path[otherCharacter._movementData.pathLength - 1];
						packet::SND_PlayerOtherMove otherPlayerMove(otherCharacter._id, otherCharacter._position, destination, startDelay);
						_map.writePacket(unit._id, otherPlayerMove);
					}

					//Then we notify the player who entered the screen that the moving player has entered and is moving to destination
					_map.writePacket(block._id, ownPlayerEnterRange);
				}
				else if (block._type == BlockType::Monster)
				{
					auto& monster = reinterpret_cast<Monster&>(block);

					if (monster.isDead()) //Skip if monster is dead
						return;

					packet::SND_MonsterEnterRange monsterEnterRange(
						monster._id, monster._position, monster._direction, monster._monsterId, monster._walkSpd,
						monster._atkSpd, EnterRangeType::Default);

					_map.writePacket(unit._id, monsterEnterRange);

					//We also need to check if the player that entered the screen is moving to notify the player moving
					if (monster._isMoving)
					{
						short startDelay = monster._movementData.nextMoveTick - _map._currentTick;
						Point destination;
						destination = monster._movementData.path[monster._movementData.pathLength - 1];
						packet::SND_MonsterMove monsterMove(monster._id, monster._position, destination, startDelay);
						_map.writePacket(unit._id, monsterMove);
					}
				}
				else if (block._type == BlockType::Npc)
				{
					auto& npc = reinterpret_cast<Npc&>(block);
					packet::SND_OtherEnterRange npcEnterRange(
						BlockType::Npc, static_cast<uint16_t>(npc._npcId), npc._id, npc._position, EnterRangeType::Default);

					_map.writePacket(unit._id, npcEnterRange);
				}
				else if (block._type == BlockType::Item)
				{
					auto& itemBlock = reinterpret_cast<FieldItem&>(block);
					packet::SND_ItemEnterRange itemEnterRange(
						static_cast<uint16_t>(itemBlock._itemData._dbId), itemBlock._id, itemBlock._position, itemBlock._itemData._amount, false, true);

					_map.writePacket(unit._id, itemEnterRange);
				}
			});
	}
	else
	{
		const auto& mob = reinterpret_cast<const Monster&>(unit);
		packet::SND_MonsterEnterRange monsterEnterRange(
			mob._id, mob._position, mob._direction, mob._monsterId, mob._walkSpd, mob._atkSpd, EnterRangeType::Default);

		//Run a range check in players to see what entities entered the range of the player
		_map._blockGrid.runActionInRangeBorder(unit, range, direction,
			[&](Block& block)
			{
				if (block._type == BlockType::Character)
					_map.writePacket(block._id, monsterEnterRange);					
			});
	}
}

//Only sends stop moves to fix position. Enter / leave ranges are responsibility of the caller
void UnitController::notifyUnitSnapToCell(const Unit& unit, const Point destination)
{
	switch (unit._type)
	{
		case BlockType::Character:
		{
			//send update position to local player
			_map.writePacket(unit._id, packet::SND_PlayerLocalStopMove(destination));

			//get packet ready for other players
			const auto packet = packet::SND_PlayerOtherStopMove(unit._id, destination);

			//Only send update position to other players
			_map._blockGrid.runActionInRange(unit, DEFAULT_VIEW_RANGE,
				[&](Block& block)
				{
					if (block._type == BlockType::Character)
						_map.writePacket(block._id, packet);
				});
		}break;
		case BlockType::Monster:
		{
			const auto packet = packet::SND_MonsterStopMove(unit._id, destination);

			_map._blockGrid.runActionInRange(unit, DEFAULT_VIEW_RANGE,
				[&](Block& block)
				{
					if (block._type == BlockType::Character)
						_map.writePacket(block._id, packet);
				});
		}break;
		default: //any other block goes here
		{
			const auto packet = packet::SND_OtherStopMove(unit._type, unit._id, destination);

			_map._blockGrid.runActionInRange(unit, DEFAULT_VIEW_RANGE,
				[&](Block& block)
				{
					if (block._type == BlockType::Character)
						_map.writePacket(block._id, packet);
				});
		}
	}
}

//***************** Private internal methods
void UnitController::checkCancelMovement(Unit& unit)
{
	//check for berserk and endure later
	if (!unit._isMoving)
		return;

	unit._isMoving = false; // stop moving

	//notify players around that unit has stopped moving
	notifyUnitStopMove(unit);
}

void UnitController::doUnitMove(Unit& unit)
{
	//Triggers when we finished walking to a cell
	if (_map._currentTick < unit._movementData.nextMoveTick)
		return;

	//Check if unit reached last cell in path. If so, stop moving
	if (unit._movementData.moveIndex >= unit._movementData.pathLength)
	{
		unit._isMoving = false;
		return;
	}		

	//Otherwise move another cell
	Point next = unit._movementData.path[unit._movementData.moveIndex];

	//Update unit direction here rather than using the method so we can use the unsafe approach for efficiency
	//Head direction will be updated below once we do the check for character. Head direction is not used for rest of operations so it's ok
	unit._direction = Direction::lookUpUnsafe(unit._position, next);

	//Send leave ranges before updating position
	notifyUnitLeaveRangeBorder(unit, DEFAULT_VIEW_RANGE, unit._direction.getInverse());

	//Check if we're moving diagonal because distances differ
	uint32_t moveTick = unit._walkSpd;
	if (next.x != unit._position.x && next.y != unit._position.y)
		moveTick = static_cast<uint32_t>(DIAGONAL_TO_UNIT_SIZE * unit._walkSpd / CELL_TO_UNIT_SIZE);

	unit._movementData.nextMoveTick += moveTick;
	
	//Change position and notify enter range
	unit._position = next;	
	_map._blockGrid.updateBlock(unit); //Always update block after changing position

	{
		Point destination;
		destination = unit._movementData.path[unit._movementData.pathLength - 1];

		//Since we only notify the move start for players who entered the range, start delay at this point is when they will start moving
		//ignoring the cell where they just appeared that
		short startDelay = unit._movementData.nextMoveTick - _map._currentTick;

		//Characters need to check for every block that entered range. Monsters and other entities only need to check for characters.
		if (unit._type == BlockType::Character)
		{
			//Update head direction here now that we know it's a character
			reinterpret_cast<Character&>(unit)._headDirection = unit._direction;

			const auto ownPlayerOtherMove = packet::SND_PlayerOtherMove(
				unit._id, unit._position, destination, startDelay);

			auto& character = reinterpret_cast<Character&>(unit);
			const auto& inv = character._inventory;

			const auto ownPlayerEnterRange = packet::SND_PlayerEnterRange(
				character._id, character._position, character._direction, character._headDirection,
				character.getGender(), character.getJob(), character.getHairStyle(),
				inv.getEquipDbId(EquipSlot::TopHeadgear), inv.getEquipDbId(EquipSlot::MidHeadgear), inv.getEquipDbId(EquipSlot::LowHeadgear),
				inv.getEquipDbId(EquipSlot::Weapon), inv.getEquipDbId(EquipSlot::Shield),
				character._walkSpd, character._atkSpd, EnterRangeType::Default);

			//Run a range check in players to see what entities entered the range of the player
			_map._blockGrid.runActionInRangeBorder(unit, DEFAULT_VIEW_RANGE, unit._direction,
				[&](Block& block)
			{
				if (block._type == BlockType::Character)
				{
					auto& blockSessionId = block._id;
					auto& otherCharacter = reinterpret_cast<Character&>(block);
					const auto& inv = otherCharacter._inventory;

					packet::SND_PlayerEnterRange otherPlayerEnterRange(
						blockSessionId, otherCharacter._position, otherCharacter._direction, otherCharacter._headDirection,
						otherCharacter.getGender(), otherCharacter.getJob(), otherCharacter.getHairStyle(),
						inv.getEquipDbId(EquipSlot::TopHeadgear), inv.getEquipDbId(EquipSlot::MidHeadgear), inv.getEquipDbId(EquipSlot::LowHeadgear),
						inv.getEquipDbId(EquipSlot::Weapon), inv.getEquipDbId(EquipSlot::Shield),
						otherCharacter._walkSpd, otherCharacter._atkSpd, EnterRangeType::Default);

					//We notify the player who moved when something enters the screen
					_map.writePacket(unit._id, otherPlayerEnterRange);

					//We also need to check if the player that entered the screen is moving to notify the player moving
					if (otherCharacter._isMoving)
					{
						short startDelay = otherCharacter._movementData.nextMoveTick - _map._currentTick;
						Point destination;
						destination = otherCharacter._movementData.path[otherCharacter._movementData.pathLength - 1];
						packet::SND_PlayerOtherMove otherPlayerMove(otherCharacter._id, otherCharacter._position, destination, startDelay);
						_map.writePacket(unit._id, otherPlayerMove);
					}

					//Then we notify the player who entered the screen that the moving player has entered and is moving to destination
					_map.writePacket(blockSessionId, ownPlayerEnterRange);
					_map.writePacket(blockSessionId, ownPlayerOtherMove);
				}
				else if (block._type == BlockType::Monster)
				{
					auto& monster = reinterpret_cast<Monster&>(block);

					if (monster.isDead()) //Skip if monster is dead
						return;

					packet::SND_MonsterEnterRange monsterEnterRange(
						monster._id, monster._position, monster._direction, monster._monsterId, monster._walkSpd, 
						monster._atkSpd, EnterRangeType::Default);

					_map.writePacket(unit._id, monsterEnterRange);

					//We also need to check if the monster that entered the screen is moving to notify the player moving
					if (monster._isMoving)
					{
						short startDelay = monster._movementData.nextMoveTick - _map._currentTick;
						Point destination;
						destination = monster._movementData.path[monster._movementData.pathLength - 1];
						packet::SND_MonsterMove monsterMove(monster._id, monster._position, destination, startDelay);
						_map.writePacket(unit._id, monsterMove);
					}
				}
				else if (block._type == BlockType::Npc)
				{
					auto& npc = reinterpret_cast<Npc&>(block);
					packet::SND_OtherEnterRange npcEnterRange(
						BlockType::Npc, static_cast<uint16_t>(npc._npcId), npc._id, npc._position, EnterRangeType::Default);

					_map.writePacket(unit._id, npcEnterRange);
				}
				else if (block._type == BlockType::Item)
				{
					auto& item = reinterpret_cast<FieldItem&>(block);
					packet::SND_ItemEnterRange itemEnterRange(
						static_cast<uint16_t>(item._itemData._dbId), item._id, item._position, item._itemData._amount, false, true);

					_map.writePacket(character._id, itemEnterRange);
				}
			});
		}
		else
		{
			auto& mob = reinterpret_cast<Monster&>(unit);
			packet::SND_MonsterEnterRange monsterEnterRange(
				mob._id, mob._position, mob._direction, mob._monsterId, mob._walkSpd, mob._atkSpd, EnterRangeType::Default);

			const auto monsterMovePacket = packet::SND_MonsterMove(
				mob._id, mob._position, destination, startDelay);

			//Run a range check  to see what entities entered the range of the mob
			_map._blockGrid.runActionInRangeBorder(unit, DEFAULT_VIEW_RANGE, unit._direction,
				[&](Block& block)
			{
				if (block._type == BlockType::Character)
				{
					//Send the enter range first and then the movement
					_map.writePacket(block._id, monsterEnterRange);
					_map.writePacket(block._id, monsterMovePacket);
				}
			});
		}
	}

	unit._movementData.moveIndex++;

	//Finally, check if we stepped on the area of any block that requires a trigger		
	//Check unit move if player is valid after executing step on actions
	if(executeStepOnActions(unit))
		checkUnitMoveAction(unit, moveTick);
}

void UnitController::updateUnitDirection(Unit& unit, int nextX, int nextY)
{	
	unit._direction = Direction::lookUpSafe(unit._position, { static_cast<int16_t>(nextX), static_cast<int16_t>(nextY) });
			
	//Characters should also have the head direction match the body
	if (unit._type == BlockType::Character)
		reinterpret_cast<Character&>(unit)._headDirection = unit._direction;		
}

void UnitController::onUnitCastTimeFinished(Unit& unit)
{
	//TODO: the cast needs to somehow have a source state as it can come through a consumable or gear cast. 
	//And in this cast _castData.skill will be null as it doesnt exist in the skill tree

	//check if it hit cast time yet
	if (unit._castData.deadline > _map._currentTick)
		return;

	_map._skillController.dispatchOnCastTimeFinished(unit);	

	//apply cooldown reductions here
	int cooldown = unit._castData.skill->dbCooldown(unit._castData.skillLevel);

	unit._castData.skill->_cooldown = _map._currentTick + cooldown;

	//Apply cast requirements
	_map._skillController.applyCastRequirements(unit, *unit._castData.skill, unit._castData.skillLevel);

	unit._isCasting = false;
}

void UnitController::checkUnitMoveAction(Unit& unit, uint32_t moveTick)
{
	//If there's no move action then quit
	if(!static_cast<uint8_t>(unit._moveAction))
		return;
	else if (unit._moveAction == MoveAction::CastAoE)
	{
		int distance = Pathfinder::CalculateDiagonalDistance(unit._position, unit._targetData.targetPosition);

		//If we're still not in LoS or in range of target position then keep walking
		if (distance > unit._castData.skill->db()._range[unit._castData.skillLevel - 1]
			|| !_map._pathfinder.isInLineOfSight(unit._position, unit._targetData.targetPosition))
			return;

		//Start the cast in  movement time to give time to walk to the center of the cell visually
		_map._timerController.add(this, _map._currentTick + unit._walkSpd, unit._id._index, TIMER_CB(runUnitMoveActionCastAoE));
		unit._animDelayEnd = _map._currentTick + moveTick; //So we can't start a new move until we actually cast

		//Stop the movement as time interval for the action is for visually fixing the position
		unit._moveAction = MoveAction::None;
		unit._isMoving = false;
	}
	else if (unit._moveAction == MoveAction::CastTarget)
	{
		Unit* target = reinterpret_cast<Unit*>(_map._blockArray.get(unit._targetData.targetBlock));

		//Target is gone, clear action and return
		if (target == nullptr)
		{
			unit._moveAction = MoveAction::None;
			unit._isMoving = false;
			return;
		}

		int distance = Pathfinder::CalculateDiagonalDistance(unit._position, target->_position);

		//If target moved it's possible the walk distance is greater than allowed. Cancel action
		if (distance > MAX_WALK_DISTANCE)
		{
			unit._moveAction = MoveAction::None;
			unit._isMoving = false;
		}

		//Target might have moved in between checks
		bool targetMoved = unit._targetData.targetPosition != target->_position;

		//If we're still not in LoS or in range of target position then either keep walking or check new movement
		if (distance > unit._castData.skill->db()._range[unit._castData.skillLevel - 1]
			|| !_map._pathfinder.isInLineOfSight(unit._position, target->_position))
		{
			//Try to start a new move if target moved
			if (targetMoved)
			{
				//If move fails then cancel action. Otherwise notify new move
				if (!canMove(unit) || !trySetUnitMoveDestination(unit, target->_position, PathType::Weak))
				{
					unit._moveAction = MoveAction::None;
					unit._isMoving = false;
				}
				else
					notifyUnitStartMove(unit, target->_position);
			}
			return;
		}

		//Start the cast in movement time to give time to walk to the center of the cell visually
		_map._timerController.add(this, _map._currentTick + moveTick, unit._id._index, TIMER_CB(runUnitMoveActionCastTarget));
		unit._animDelayEnd = _map._currentTick + moveTick; //So we can't start a new move until we actually cast

		//Stop the movement as time interval for the action is for visually fixing the position
		unit._moveAction = MoveAction::None;
		unit._isMoving = false;
	}
	else if(unit._moveAction == MoveAction::PickUpItem)
	{
		int distance = Pathfinder::CalculateDiagonalDistance(unit._position, unit._targetData.targetPosition);

		//keep walking if we're still not in range
		if(distance > MAX_PICK_UP_RANGE)
			return;

		_map._itemController.onMoveActionPickUpItemReached(unit, moveTick);

		//Stop the movement
		unit._moveAction = MoveAction::None;
		unit._isMoving = false;
	}
	else //Do we need an if for moveactiontype::attack? or will there only be these 4
	{

	}
}

Reschedule UnitController::runUnitMoveActionCastAoE(int32_t blockId, Tick)
{
	Unit* unit = reinterpret_cast<Unit*>(_map._blockArray.get(BlockId(blockId)));

	//unit is gone, do nothing
	if (unit == nullptr)
		return Reschedule::NO();

	if (!canCast(*unit) || !_map._skillController.checkCastRequirements(*unit, *unit->_castData.skill, unit->_castData.skillLevel))
		return Reschedule::NO();

	//Turn source to direction of cast
	updateUnitDirection(*unit, unit->_targetData.targetPosition.x, unit->_targetData.targetPosition.y);

	//If we meet the casting requirements then start casting
	//Recalculate cast time since reductions might be different after moving
	CastTime castTime = calculateCastTime(*unit, *unit->_castData.skill, unit->_castData.skillLevel); 

	//Deadline contains only the cast time when set as a unit action
	if (castTime > INSTANT_CASTIME)
	{
		//Start casting and set new deadline. Rest of the data was already set before
		unit->_isCasting = true;
		unit->_castData.deadline = _map._currentTick + castTime;

		//Then notify the cast
		_map._skillController.notifyAoESkillCastStart(*unit, unit->_castData.skill->_id, castTime, unit->_targetData.targetPosition);
	}
	else //instant cast, dispatch skill right away
	{
		_map._skillController.dispatchAtTargetPosition(*unit, unit->_castData.skill->_id, unit->_castData.skillLevel, unit->_targetData.targetPosition);
		//Only apply cast requirements on cast dispatch
		_map._skillController.applyCastRequirements(*unit, *unit->_castData.skill, unit->_castData.skillLevel);
	}

	return Reschedule::NO();
}

Reschedule UnitController::runUnitMoveActionCastTarget(int32_t blockId, Tick)
{
	Unit* unit = reinterpret_cast<Unit*>(_map._blockArray.get(BlockId(blockId)));

	//unit is gone, do nothing
	if (unit == nullptr)
		return Reschedule::NO();

	Unit* target = reinterpret_cast<Unit*>(_map._blockArray.get(unit->_targetData.targetBlock));

	//Target is gone, do nothing
	if (target == nullptr || !canCast(*unit) || !_map._skillController.checkCastRequirements(*unit, *unit->_castData.skill, unit->_castData.skillLevel))
		return Reschedule::NO();

	//Turn unit to direction of target
	updateUnitDirection(*unit, target->_position.x, target->_position.y);

	//We do not range check again, player was already in range of target when queueing the timer. 
	//It does not make sense to punish player with out of range because of a delay to fix graphical bug

	//If we meet the casting requirements then start casting
	//Recalculate cast time since reductions might be different after moving
	CastTime castTime = calculateCastTime(*unit, *unit->_castData.skill, unit->_castData.skillLevel);

	//Deadline contains only the cast time when set as a unit action
	if (castTime > INSTANT_CASTIME)
	{
		//Start casting and set new deadline. Rest of the data was already set before
		unit->_isCasting = true;
		unit->_castData.deadline = _map._currentTick + castTime;

		//Then notify the cast
		_map._skillController.notifyTargetSkillCastStart(*unit, unit->_castData.skill->_id, castTime, *target);
	}
	else //instant cast, dispatch skill right away
	{
		_map._skillController.dispatchAtTargetUnit(*unit, unit->_castData.skill->_id, unit->_castData.skillLevel, target->_id);
		//Only apply cast requirements on cast dispatch
		_map._skillController.applyCastRequirements(*unit, *unit->_castData.skill, unit->_castData.skillLevel);
	}

	return Reschedule::NO();
}

//************ Packet handlers
PacketHandlerResult UnitController::onRCV_PlayerMove(const packet::Packet& rawPacket, Unit& src)
{
	auto& rcvPacket = reinterpret_cast<const packet::RCV_PlayerMove&>(rawPacket);

	if (!_map._tiles(rcvPacket.position.x, rcvPacket.position.y).walkable ||
		rcvPacket.position.x <= 0 || rcvPacket.position.x >= _map._tiles.width() ||
		rcvPacket.position.y <= 0 || rcvPacket.position.y >= _map._tiles.height())
	{
		return PacketHandlerResult(PacketHandlerResult::Status::Consumed, sizeof(packet::RCV_PlayerMove));
	}

	if (!canMove(src) || !trySetUnitMoveDestination(src, rcvPacket.position, PathType::Hard))
		return PacketHandlerResult(PacketHandlerResult::Status::Consumed, sizeof(packet::RCV_PlayerMove));

	//Cancel any move action type if we had one
	src._moveAction = MoveAction::None;
	notifyUnitStartMove(src, rcvPacket.position);

	return PacketHandlerResult(PacketHandlerResult::Status::Consumed, sizeof(packet::RCV_PlayerMove));
}