#pragma once

#include <server/common/CommonTypes.h>
#include <server/common/DamageTypes.h>

class Map;
class Unit;

class BattleController final
{
public:
	BattleController(Map& map);

	DmgInitial calcInitialMagicDmg(Unit& src, Unit& target, Element element, DmgRaw dmg);
	DmgInitial calcInitialMagicDmg(Unit& src, Unit& target, Element element, DmgMatkRate rate, bool swapMdefWithDef = false);

	DmgInitial calcInitialMiscDmg(Unit& src, Unit& target, Element element, DmgRaw dmg);

	DmgInitial calcInitialPhysicalDmg(Unit& src, Unit& target, Element element, DmgRaw dmg, int targeted, DmgFlag flags);
	DmgInitial calcInitialPhysicalDmg(Unit& src, Unit& target, Element element, DmgAtkRate rate, DmgAtkFlat flat, int targeted, DmgFlag flags);

	int calcFinalMagicDmg(Unit& src, Unit& target, Element element, DmgInitial dmg, DmgFlag flags);
	int calcFinalMiscDmg(Unit& src, Unit& target, Element element, DmgInitial dmg, DmgFlag flags);
	int calcFinalPhysicalDmg(Unit& src, Unit& target, Element element, DmgInitial dmg, DmgFlag flags);

	bool canPhysicalAttack(Unit& src, Unit& target, bool isRanged, bool ignorePneumaAndSw);

	bool calcLuckyDodge(Unit& target);
	bool calcCriticalHit(Unit& src, Unit& target);
	bool calcHit(Unit& src, Unit& target, int targeted, int hitRate);

private:
	DmgInitial charCalcInitialPhysicalDmg(Unit& src, Unit& target, Element element, DmgAtkRate rate, DmgAtkFlat flat, int targeted, DmgFlag flags);
	DmgInitial otherCalcInitialPhysicalDmg(Unit& src, Unit& target, Element element, DmgAtkRate rate, DmgAtkFlat flat, int targeted, DmgFlag flags);

	Map& _map;
};