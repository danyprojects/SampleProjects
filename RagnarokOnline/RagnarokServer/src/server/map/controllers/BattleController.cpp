#include "BattleController.h"

#include <server/map/Map.h>
#include <server/map/data/MapData.h>
#include <server/map_objects/Unit.h>
#include <server/map_objects/Monster.h>
#include <server/map_objects/Character.h>
#include <server/lobby/Account.h>

namespace
{
	constexpr int elementDmgRate[enum_cast(ElementLvl::Last)][enum_cast(Element::Last) + 1][enum_cast(Element::Last) + 1] =
	{
		{// lvl1
			{100, 100, 100, 100, 100, 100, 100, 100,  25, 100},
			{100,  25, 100, 150,  50, 100,  75, 100, 100, 100},
			{100, 100, 100,  50, 150, 100,  75, 100, 100, 100},
			{100,  50, 150,  25, 100, 100,  75, 100, 100, 125},
			{100, 175,  50, 100,  25, 100,  75, 100, 100, 100},
			{100, 100, 125, 125, 125,   0,  75,  50, 100, -25},
			{100, 100, 100, 100, 100, 100,   0, 125, 100, 150},
			{100, 100, 100, 100, 100,  50, 125,   0, 100, -25},
			{25 , 100, 100, 100, 100, 100,  75,  75, 125, 100},
			{100, 100, 100, 100, 100,  50, 100,   0, 100,   0},
		},
		{// lvl2
			{100, 100, 100, 100, 100, 100, 100, 100,  25, 100},
			{100,   0, 100, 175,  25, 100,  50,  75, 100, 100},
			{100, 100,  50,  25, 175, 100,  50,  75, 100, 100},
			{100,  25, 175,   0, 100, 100,  50,  75, 100, 150},
			{100, 175,  25, 100,   0, 100,  50,  75, 100, 100},
			{100,  75, 125, 125, 125,   0,  50,  25,  75, -50},
			{100, 100, 100, 100, 100, 100, -25, 150, 100, 175},
			{100, 100, 100, 100, 100,  25, 150, -25, 100, -50},
			{0  ,  75,  75,  75,  75,  75,  50,  50, 150, 125},
			{100,  75,  75,  75,  75,  25, 125,   0, 100,   0},
		},
		{// lvl3
			{100, 100, 100, 100, 100, 100, 100, 100,   0, 100},
			{100, -25, 100, 200,   0, 100,  25,  50, 100, 125},
			{100, 100,   0,   0, 200, 100,  25,  50, 100,  75},
			{100,   0, 200, -25, 100, 100,  25,  50, 100, 175},
			{100, 200,   0, 100, -25, 100,  25,  50, 100, 100},
			{100,  50, 100, 100, 100,   0,  25,   0,  50, -75},
			{100, 100, 100, 100, 100, 125, -50, 175, 100, 200},
			{100, 100, 100, 100, 100,   0, 175, -50, 100, -75},
			{0  ,  50,  50,  50,  50,  50,  25,  25, 175, 150},
			{100,  50,  50,  50,  50,   0, 150,   0, 100,   0},
		},
		{// lvl4
			{100, 100, 100, 100, 100, 100, 100, 100,   0, 100},
			{100, -50, 100, 200,   0,  75,   0,  25, 100, 150},
			{100, 100, -25,   0, 200,  75,   0,  25, 100,  50},
			{100,   0, 200, -50, 100,  75,   0,  25, 100, 200},
			{100, 200,   0, 100, -50,  75,   0,  25, 100, 100},
			{100,  25,  75,  75,  75,   0,   0, -25,  25,-100},
			{100,  75,  75,  75,  75, 125,-100, 200, 100, 200},
			{100,  75,  75,  75,  75, -25, 200,-100, 100,-100},
			{0  ,  25,  25,  25,  25,  25,   0,   0, 200, 175},
			{100,  25,  25,  25,  25, -25, 175,   0, 100,   0},
		}
	};

	constexpr int sizeWeaponDmgRate[enum_cast(Size::Last) + 1][enum_cast(WeaponType::Last) + 1]
	{
		{100,100, 75, 75, 75, 75, 50, 50, 75,100,100,100,100, 75, 75,100, 75,100,100,100,100,100,100},
		{100, 75,100, 75, 75, 75, 75, 75,100,100,100,100, 75,100,100,100,100,100,100,100,100,100,100},
		{100, 50, 75,100,100,100,100,100,100,100,100, 75, 50, 75, 50, 50, 75,100,100,100,100,100,100}
	};

	int getElementDmgRate(Element atkEle, Unit& target)
	{
		auto lvl = enum_cast(target._defElementLvl);
		auto srcElem = enum_cast(atkEle);
		auto targetElem = enum_cast(target._defElement);

		return elementDmgRate[lvl-1][srcElem][targetElem];
	}

	int getSizeWeaponDmgRate(WeaponType weaponType, const Unit& target)
	{
		return sizeWeaponDmgRate[enum_cast(target._size)][enum_cast(weaponType)];
	}

	auto isSpear = [](const Item* item)
	{
		if (item)
		{
			switch (item->db()._weaponType)
			{
			case WeaponType::Spear:
			case WeaponType::TwoHandSpear:
				return true;
			};
		}
		return false;
	};
}

BattleController::BattleController(Map& map)
	:_map(map)
{
	/*
	// Test code with debuger delete later keeeping now for further testing
	auto charInfo = uid::SharedPtr<CharacterInfo>(new CharacterInfo(*((uid::Name*)(nullptr))), nullptr);
	charInfo->lvl = 1;
	charInfo->offline.str = 1;
	charInfo->offline.agi = 1;
	charInfo->offline.vit = 1;
	charInfo->offline.int_ = 1;
	charInfo->offline.dex = 99;
	charInfo->offline.luck = 1;

	auto* account = new Account(*((uid::Name*)(nullptr)));
	auto* character = new Character(*account, charInfo);

	auto* mob = new Monster(MonsterId::RaydricArcher, 1, 0);
	mob->spawn();

	auto initial = calcInitialPhysicalDmg(*character, *mob, Element::Neutral, DmgAtkRate(100), DmgAtkFlat(0), 1, DmgFlag::None);
	auto fin = calcFinalPhysicalDmg(*character, *mob, Element::Neutral, initial, DmgFlag::None);
	*/
}

DmgInitial BattleController::calcInitialMagicDmg(Unit& src, Unit& target, Element element, DmgRaw dmg)
{
	assert(element != Element::None);

	int damage = dmg;

	if (src._type == BlockType::Character)
	{
		int cardfix = 100;
		auto& character = reinterpret_cast<Character&>(src);

		cardfix = cardfix * (100 + character._magicAddPercentRace[enum_cast(target._race)]) / 100;
		cardfix = cardfix * (100 + character._magicAddPercentElem[enum_cast(target._defElement)]) / 100;

		if (target._type == BlockType::Monster)
		{
			auto& mob = reinterpret_cast<Monster&>(target);
			if (mob.db().flag.statusImmune)
				cardfix = cardfix * (100 + character._magicAddPercentRace[enum_cast(Race::Boss)]) / 100;
			else
				cardfix = cardfix * (100 + character._magicAddPercentRace[enum_cast(Race::NonBoss)]) / 100;

			cardfix = cardfix * (100 + character._magicAddPercentTribe[enum_cast(mob._tribe)]) / 100;
		}

		damage = damage * cardfix / 100;
	}

	if (target._type == BlockType::Character)
	{
		int cardfix = 100;
		auto& character = reinterpret_cast<Character&>(target);

		cardfix = cardfix * (100 - character._magicSubPercentRace[enum_cast(target._race)]) / 100;
		cardfix = cardfix * (100 - character._magicSubPercentElem[enum_cast(target._defElement)]) / 100;

		if (target._type == BlockType::Monster)
		{
			auto& mob = reinterpret_cast<Monster&>(target);
			if (mob.db().flag.statusImmune)
				cardfix = cardfix * (100 - character._magicSubPercentRace[enum_cast(Race::Boss)]) / 100;
			else
				cardfix = cardfix * (100 - character._magicSubPercentRace[enum_cast(Race::NonBoss)]) / 100;

			cardfix = cardfix * (100 - character._magicSubPercentTribe[enum_cast(mob._tribe)]) / 100;
		}
		damage = damage * cardfix / 100;

		if (damage <= 0 || character._ignoreMagicDmg)
			return 0;
	}

	// We also apply resistance pierce here since elemtRate can be 0%, and would cause dmg to hit 0 
	auto elemRate = getElementDmgRate(element, target) + src._resistancePierce[enum_cast(target._defElement)];
	return damage = damage * std::max(elemRate, 0) / 100;
}

DmgInitial BattleController::calcInitialMagicDmg(
	Unit& src, Unit& target, Element element, DmgMatkRate rate, bool swapMdefWithDef)
{
	assert(element != Element::None);

	int minMatk = src.getMinMatk() * rate / 100;
	int maxMatk = src.getMaxMatk() * rate / 100;

	int damage = minMatk + _map._rand() % (maxMatk - minMatk + 1);

	int mdefRate = 100;
	if (src._type == BlockType::Character)
	{
		auto& character = reinterpret_cast<Character&>(src);
		mdefRate -= character._ignoreMdefRace[enum_cast(target._race)];

		if (target._type == BlockType::Monster)
		{
			auto& mob = reinterpret_cast<Monster&>(target);

			if (mob.db().flag.statusImmune)
				mdefRate -= character._ignoreMdefRace[enum_cast(Race::Boss)];
			else
				mdefRate -= character._ignoreMdefRace[enum_cast(Race::NonBoss)];
		}
	}

	if (mdefRate > 0)
	{
		int mdef1 = swapMdefWithDef ? target.getDef() : target.getMdef();
		int mdef2 = swapMdefWithDef ? target.getDef2() : target.getMdef2();

		mdef1 -= mdef1 * (100 - mdefRate) / 100;

		damage = std::max(1, damage * (100 - mdef1) / 100 - mdef2);
	}

	return calcInitialMagicDmg(src, target, element, DmgRaw(damage));
}

DmgInitial BattleController::calcInitialMiscDmg(
	Unit& src, Unit& target, Element element, DmgRaw dmg)
{
	assert(element != Element::None);

	int damage = dmg;

	if (target._type == BlockType::Character)
	{
		int cardfix = 100;
		auto& character = reinterpret_cast<Character&>(target);

		cardfix = cardfix * (100 - character._attackSubPercentRace[enum_cast(target._race)]) / 100;
		cardfix = cardfix * (100 - character._attackSubPercentElem[enum_cast(target._defElement)]) / 100;
		cardfix = cardfix * (100 - character._miscDefRate) / 100;

		damage = damage * cardfix / 100;

		if (damage < 0)
			damage = 0;
	}

	// We also apply resistance pierce here since elemtRate can be 0%, and would cause dmg to hit 0 
	auto elemRate = getElementDmgRate(element, target) + src._resistancePierce[enum_cast(target._defElement)];
	return damage = damage * std::max(elemRate, 0) / 100;
}

DmgInitial BattleController::calcInitialPhysicalDmg(
	Unit& src, Unit& target, Element element, DmgRaw dmg, int targeted, DmgFlag flags)
{
	int damage = dmg;
	int defRate = 100;

	if (src._type == BlockType::Character)
	{
		const auto& character = reinterpret_cast<Character&>(src);
		const auto raceBit = 1 << enum_cast(target._race);
		const auto elementBit = 1 << enum_cast(target._defElement);
		bool scaleDmg = false;

		defRate -= character._ignoreDefRace[enum_cast(target._race)];

		if (character._scaleAtkByDefElementMask & elementBit || character._scaleAtkByDefRaceMask & raceBit)
			scaleDmg = true;

		int cardfix = 100;
		cardfix = cardfix * (100 + character._attackAddPercentRace[enum_cast(target._race)]) / 100;
		cardfix = cardfix * (100 + character._attackAddPercentElem[enum_cast(target._defElement)]) / 100;

		if (target._type == BlockType::Monster)
		{
			auto& mob = reinterpret_cast<Monster&>(target);
			if (mob.db().flag.statusImmune)
			{
				cardfix = cardfix * (100 + character._attackAddPercentRace[enum_cast(Race::Boss)]) / 100;
				defRate -= character._ignoreDefRace[enum_cast(Race::Boss)];

				if (character._scaleAtkByDefRaceMask & (1 << enum_cast(Race::Boss)))
					scaleDmg = true;
			}
			else
			{
				cardfix = cardfix * (100 + character._attackAddPercentRace[enum_cast(Race::NonBoss)]) / 100;
				defRate -= character._ignoreDefRace[enum_cast(Race::NonBoss)];

				if (character._scaleAtkByDefRaceMask & (1 << enum_cast(Race::NonBoss)))
					scaleDmg = true;
			}

			cardfix = cardfix * (100 + character._attackAddPercentTribe[enum_cast(mob._tribe)]) / 100;
		}

		damage = damage * cardfix / 100;

		if (scaleDmg)
		{
			damage = (damage * (target.getDef() + target.getDef2())) / 100;
			defRate = 0;
		}
	}

	if (!(flags & DmgFlag::IgnoreDef || flags & DmgFlag::CriticalAttack))
	{
		auto vit = target.getVit();
		auto def1 = target.getDef();
		auto def2 = target.getDef2();

		def1 -= def1 * (100 - std::max(defRate,0)) / 100;

		if (targeted > 2)
		{
			def1 = (def1 * (100 - (targeted - 2) * 5)) / 100;
			def2 = (def2 * (100 - (targeted - 2) * 5)) / 100;
			vit = (vit * (100 - (targeted - 2) * 5)) / 100;
		}

		// TODO:: usually DemonBane modifier for players should enter here rethink it later

		int vitbonusmax = (vit / 20) * (vit / 20) - 1;
		if (vitbonusmax >= 1)
			def2 += rand() % (vitbonusmax + 1);

		damage = damage * (100 - def1) / 100 - def2;
	}

	if (target._type == BlockType::Character)
	{
		int cardfix = 100;
		auto& character = reinterpret_cast<Character&>(target);

		cardfix = cardfix * (100 - character._attackSubPercentRace[enum_cast(target._race)]) / 100;
		cardfix = cardfix * (100 - character._attackSubPercentElem[enum_cast(target._defElement)]) / 100;

		if (target._type == BlockType::Monster)
		{
			auto& mob = reinterpret_cast<Monster&>(target);
			if (mob.db().flag.statusImmune)
				cardfix = cardfix * (100 - character._attackSubPercentRace[enum_cast(Race::Boss)]) / 100;
			else
				cardfix = cardfix * (100 - character._attackSubPercentRace[enum_cast(Race::NonBoss)]) / 100;

			cardfix = cardfix * (100 - character._attackSubPercentTribe[enum_cast(mob._tribe)]) / 100;
		}

		damage = damage * cardfix / 100;

		auto atkDefRate = flags & DmgFlag::RangedAttack ? character._rangeAttackDefRate : character._attackDefRate;
		cardfix = cardfix * (100 - atkDefRate) / 100;

		damage = damage * cardfix / 100;

		if (character._ignorePhysicalDmg)
			damage = 0;
	}

	if (element == Element::None)
		element = src._atkElement;

	auto elemRate = getElementDmgRate(element, target) + src._resistancePierce[enum_cast(target._defElement)];
	return damage = damage * std::max(elemRate, 0) / 100;
}

DmgInitial BattleController::calcInitialPhysicalDmg(
	Unit& src, Unit& target, Element element, DmgAtkRate rate, DmgAtkFlat flat, int targeted, DmgFlag flags)
{
	switch (src._type)
	{
	case BlockType::Character:
		return charCalcInitialPhysicalDmg(src, target, element, rate, flat, targeted, flags);
	default:
		return otherCalcInitialPhysicalDmg(src, target, element, rate, flat, targeted, flags);
	}
}

int BattleController::calcFinalMagicDmg(
	Unit& src, Unit& target, Element element, DmgInitial dmg, DmgFlag flags)
{
	if (!dmg)
		return 0;

	// TODO check status change count to allow ealy return if zero

	if (target._statusChange.contains(BuffId::MagicRod) && flags & DmgFlag::IsAoe)
	{
		if (target._type == BlockType::Character)
		{
			// modify sp, healsp, change can act tick
			// we may want to call the code on skilcontroller to actual process magic rod from here
		}

		return 0;
	}

	if (target._statusChange.contains(BuffId::LexAeterna))
	{
		dmg *= 2;
		//to implement skill_status_change_end(bl, SC_AETERNA, -1);
	}

	if (target._statusChange.contains(BuffId::Deluge) && element == Element::Water)
	{
		// to implement damage += damage * sc_data[SC_DELUGE].val4 / 100;
	}
	else if (target._statusChange.contains(BuffId::Volcano) && element == Element::Fire)
	{
		// to implement damage += damage * sc_data[SC_VOLCANO].val4 / 100;
	}
	else if (target._statusChange.contains(BuffId::ViolentGale) && element == Element::Wind)
	{
		// to implement damage += damage * sc_data[SC_VIOLENTGALE].val4 / 100;
	}

	if (target._statusChange.contains(BuffId::Assumptio))
	{
		dmg /= _map._mapData._isPvpMap ? 2 : 3;
	}
	else if (target._statusChange.contains(BuffId::KyrieEleison))
	{
		// todo blocks holy light not sure if anythig else
	}

	if (target._type == BlockType::Character)
	{
		//TODO::trigger plagiarism
	}

	return dmg;
}

int BattleController::calcFinalMiscDmg(
	Unit& src, Unit& target, Element element, DmgInitial dmg, DmgFlag flags)
{
	if (!dmg)
		return 0;

	// TODO check status change count to allow ealy return if zero

	if (target._statusChange.contains(BuffId::LexAeterna))
	{
		dmg *= 2;
		//to implement skill_status_change_end(bl, SC_AETERNA, -1);
	}

	if (target._statusChange.contains(BuffId::Assumptio))
	{
		dmg /= _map._mapData._isPvpMap ? 2 : 3;
	}
	else if (target._statusChange.contains(BuffId::KyrieEleison))
	{
		// todo process kyrie
	}

	if (target._type == BlockType::Character)
	{
		//TODO::trigger plagiarism
	}

	return dmg;
}

int BattleController::calcFinalPhysicalDmg(
	Unit& src, Unit& target, Element element, DmgInitial dmg, DmgFlag flags)
{
	if (!dmg)
		return 0;

	if (target._statusChange.contains(BuffId::LexAeterna))
	{
		dmg *= 2;
		//to implement skill_status_change_end(bl, SC_AETERNA, -1);
	}

	if (target._statusChange.contains(BuffId::Deluge) && element == Element::Water)
	{
		// to implement damage += damage * sc_data[SC_DELUGE].val4 / 100;
	}
	else if (target._statusChange.contains(BuffId::Volcano) && element == Element::Fire)
	{
		// to implement damage += damage * sc_data[SC_VOLCANO].val4 / 100;
	}
	else if (target._statusChange.contains(BuffId::ViolentGale) && element == Element::Wind)
	{
		// to implement damage += damage * sc_data[SC_VIOLENTGALE].val4 / 100;
	}

	if (target._statusChange.contains(BuffId::Assumptio))
	{
		dmg /= _map._mapData._isPvpMap ? 2 : 3;
	}
	else if (target._statusChange.contains(BuffId::KyrieEleison))
	{
		// todo process kyrie
	}

	if (target._statusChange.contains(BuffId::EnergyCoat))
	{
		// TOOO:: apply energy coat dmg reduction
	}

	if (target._statusChange.contains(BuffId::RejectSword))
	{
		// TODO dmg -= processSwordReject
	}

	if (target._statusChange.contains(BuffId::ReflectShield))
	{
		// process shield reflect
	}

	// TODO:: trigger weapon sp hp drain base on dmg

	if (target._type == BlockType::Character)
	{
		//TTODO:: relfect _meleeAtkReflectRate; uint8_t _rangeAtkReflectRate;

		//TODO::trigger plagiarism
	}

	return dmg;
}

bool BattleController::calcLuckyDodge(Unit& target)
{
	if (target._type == BlockType::Character)
		return _map._rand() % 1000 < reinterpret_cast<Character&>(target).getFlee2();
	return false;
}

bool BattleController::calcCriticalHit(Unit& src, Unit& target)
{
	int crit = src.getCrit();

	if (target._type == BlockType::Character)
		crit = crit * (100 - reinterpret_cast<Character&>(target)._critDefRate) / 100;

	return (_map._rand() % 1000) < crit;
}

bool BattleController::calcHit(Unit& src, Unit& target, int targeted, int hitRate)
{
	if (target._statusChange.contains(BuffId::Sleep)  ||
		target._statusChange.contains(BuffId::Stun)   ||
		target._statusChange.contains(BuffId::Freeze) ||
		target._statusChange.contains(BuffId::StoneCurse)) // TODO:: differentiate between stone curse states
		return false;

	auto flee = target.getFlee();

	if (targeted > 2)
		flee = flee * (100 - (targeted - 2) * 10) / 100;

	auto hit = src.getHit() - flee + 80;

	hit = std::clamp(hit * hitRate, 5, 95);

	return _map._rand() % 100 >= hit;
}

DmgInitial BattleController::charCalcInitialPhysicalDmg(
	Unit& src, Unit& target, Element element, DmgAtkRate rate, DmgAtkFlat flat, int targeted, DmgFlag flags)
{
	auto& character = reinterpret_cast<Character&>(src);

	int atkMin1 = src.getDex();
	int atkMin2 = atkMin1;
	int atkMax1 = 0;
	int atkMax2 = 0;
	WeaponType weaponType1 = WeaponType::Barehand;
	WeaponType weaponType2 = WeaponType::Barehand;

	const auto weapon1 = character._inventory.getEquip(EquipSlot::Weapon);

	if (weapon1)
	{
		atkMin1 = atkMin1 * (80 + weapon1->db()._weaponLvl * 20) / 100;
		atkMax1 = weapon1->db()._atk;

		weaponType1 = weapon1->db()._weaponType;

		//RO didnt do it like this so there will be a diff in calculator
		if (auto ammo = character._inventory.getEquip(EquipSlot::Ammunition))
			atkMax1 += ammo->db()._atk;
	}

	if (auto weapon2 = character._inventory.getEquip(EquipSlot::Shield); weapon2 && weapon2->db()._type == ItemType::Weapon)
	{
		weaponType2 = weapon2->db()._weaponType;

		atkMin2 = atkMin2 * (80 + weapon2->db()._weaponLvl * 20) / 100;
		atkMax2 = weapon2->db()._atk;
	}
	
	const bool isBowAttack = weapon1 && weapon1->db()._weaponType == WeaponType::Bow;

	if (isBowAttack)
		atkMin1 = atkMax1 * std::min(atkMin1, atkMax1) / 100;
	else
		atkMin1 = std::min(atkMin1, atkMax1);

	atkMin2 = std::min(atkMin2, atkMax2);

	if (src._statusChange.contains(BuffId::MaximizePower) || (flags & DmgFlag::CriticalAttack))
	{
		atkMin1 = atkMax1;
		atkMin2 = atkMax2;
	}

	if (!character._ignoreSizeReductionCount && !(character.isMounted() && isSpear(weapon1)))
	{
		atkMax1 = atkMax1 * getSizeWeaponDmgRate(weaponType1, target) / 100;
		atkMin1 = atkMin1 * getSizeWeaponDmgRate(weaponType1, target) / 100;

		atkMax2 = atkMax2 * getSizeWeaponDmgRate(weaponType2, target) / 100;
		atkMin2 = atkMin2 * getSizeWeaponDmgRate(weaponType2, target) / 100;
	}

	int dmg1 = atkMin1 + src.getBaseAtk(isBowAttack) + flat;
	int dmg2 = enum_cast(weaponType2) ? dmg1 - atkMin1 + atkMin2 : 0; // no dmg2 if no secondary weapon

	if (atkMax1 > atkMin1)
		dmg1 += rand() % (atkMax1 - atkMin1 + 1);
	if (atkMax2 > atkMin2)
		dmg2 += rand() % (atkMax2 - atkMin2 + 1);

	DmgRaw total((dmg1 + dmg2) * (rate + src._atkRate) / 100);

	element = element != Element::None ? element : character._weaponElement;
	return calcInitialPhysicalDmg(src, target, element, total, targeted, flags);
}

DmgInitial BattleController::otherCalcInitialPhysicalDmg(
	Unit& src, Unit& target, Element element, DmgAtkRate rate, DmgAtkFlat flat, int targeted, DmgFlag flags)
{
	int atkMin = 0;
	int atkMax = 0;

	switch (src._type)
	{
	case BlockType::Monster:
		atkMin = reinterpret_cast<Monster&>(src).db().minAttack;
		atkMax = reinterpret_cast<Monster&>(src).db().maxAttack;
		break;
	// TODO:: homunculus and so on
	}

	atkMin = std::min(atkMin, atkMax);

	if (src._statusChange.contains(BuffId::MaximizePower) || (flags & DmgFlag::CriticalAttack))
		atkMin = atkMax;

	int damage = atkMin + src.getBaseAtk() + flat;
	if (atkMax > atkMin)
		damage += rand() % (atkMax - atkMin + 1);

	damage += damage * (rate + src._atkRate) / 100;

	return calcInitialPhysicalDmg(src, target, element, DmgRaw(damage), targeted, flags);
}

bool BattleController::canPhysicalAttack(Unit& src, Unit& target, bool isRanged, bool ignorePneumaAndSw)
{

	if (target._statusChange.contains(BuffId::SafetyWall))
	{
		// TODO:: consume safety wall
		return false;
	}

	if (isRanged && target._statusChange.contains(BuffId::Pneuma))
		return 0;

	if (target._statusChange.contains(BuffId::AutoCounter))
	{
		//todo process autocounter
		return 0;
	}

	if (target._statusChange.contains(BuffId::AutoGuard))
	{
		// todo rand() % 100 < sc_data[SC_PARRYING].val2)
		return 0;
	}

	if (target._statusChange.contains(BuffId::Parry))
	{
		// todo rand() % 100 < sc_data[SC_PARRYING].val2)
		return 0;
	}
	return true;
}
