#include "Monster.h"

Monster::Monster(MonsterId id, uint8_t level, uint16_t spawnDataIndex)
	:Unit(Type::Monster, _buffSlots)
	,_monsterId(id)
	,_spawnDataIndex(spawnDataIndex)
{
	//starting flags
	_isMoving = false;
	_isCasting = false;

	//starting ticks
	_statusChange.nextStormGustHit = Tick::ZERO();
	_animDelayEnd = Tick::ZERO();
}

void Monster::spawn()
{
	auto& db = Monster::db();

	//TODO: still missing a few
	_hp = db.hp;
	_sp = db.sp;
	_lvl = db.lvl;
	_maxHpFlat = db.hp;
	_maxSpFlat = db.sp;
	_strBase = db.str;
	_agiBase = db.agi;
	_vitBase = db.vit;
	_intBase = db.int_;
	_dexBase = db.dex;
	_lukBase = db.luk;
	_defFlat = db.def;
	_mdefFlat = db.mdef;
	_race = db.race;
	_size = db.size;
	_atkSpd = db.attackMotion;
	_walkSpd = db.moveSpeed;
	_defElement = db.element;
	_defElementLvl = db.elementLvl;
}