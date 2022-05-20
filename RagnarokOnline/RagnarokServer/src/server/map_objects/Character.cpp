#include "Character.h"

#include <server/Guild.h>
#include <server/Party.h>
#include <server/lobby/Account.h>

#include <sdk/uid/Name.h>
#include <sdk/log/Log.h>


namespace
{
	constexpr char* const TAG = "Character";
}

Character::Character(Account& account, uid::SharedPtr<CharacterInfo> charInfo)
	:Unit(Type::Character, _buffSlots)
	,_account(account)
	,_skillTree(charInfo->job)
	,_info(*charInfo)
	,_charInfo(std::move(charInfo))
{
	//These 4 are always fixed for new player characters
	_race = Race::DemiHuman;
	_size = Size::Medium;
	_defElement = Element::Neutral;
	_defElementLvl = ElementLvl::_1;

	//TODO: get the defaults from a table later
	_maxSpFlat = 10;
	_maxHpFlat = 100;
	_sp = _maxSpFlat;
	_hp = _maxHpFlat;
	_atkSpd = 130;
	_walkSpd = 150;

	//starting flags
	_isMoving = false;
	_isCasting = false;

	//starting ticks
	_statusChange.nextStormGustHit = Tick::ZERO();
	_animDelayEnd = Tick::ZERO();

	_info.lvl = 1;
	_info.expNextLvl = 0;

	_strBase = _info.offline.str;
	_agiBase = _info.offline.agi;
	_vitBase = _info.offline.vit;
	_intBase = _info.offline.int_;
	_dexBase = _info.offline.dex;
	_lukBase = _info.offline.luck;
}

Character::~Character()
{
	updateCharInfo();

	Log::debug("Destroying character, charId=%d, for accId=%d", TAG, uid(), _account.uid());
}

Storage& Character::getStorage()
{
	return _account.getStorage();
}

const Storage& Character::getStorage() const
{
	return _account.getStorage();
}

bool Character::isMounted() const
{
	return false;
}

void Character::updateCharInfo()
{
	_info.upperHeadgear = _inventory.getEquipDbId(EquipSlot::TopHeadgear);
	_info.middleHeadgear = _inventory.getEquipDbId(EquipSlot::MidHeadgear);
	_info.lowerHeadgear = _inventory.getEquipDbId(EquipSlot::LowHeadgear);

	_info.offline.str = _strBase;
	_info.offline.agi = _agiBase;
	_info.offline.vit = _vitBase;
	_info.offline.int_ = _intBase;
	_info.offline.dex = _dexBase;
	_info.offline.luck = _lukBase;
	_info.offline.hp = _hp;
	_info.offline.sp = _sp;
}
