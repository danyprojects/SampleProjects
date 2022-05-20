#include "TriggerEffects.h"

#include <server/map/Map.h>


TriggerEffects::TriggerEffects(Map& map)
	: _map(map)
{ }

//****************** start of effect handlers
void TriggerEffects::bStr(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bAgi(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bVit(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bInt(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bDex(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bLuk(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bAgiVit(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bAgiDexStr(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bAllStats(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bMaxHP(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bMaxHPRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bMaxSP(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bMaxSPRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bHPrecovRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bSPrecovRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bUseSPRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bNoRegen(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bHPDrainValue(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bSPDrainValue(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bHPDrainRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bSPDrainRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bHPGainValue(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bSPGainValue(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bMagicHPGainValue(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bMagicSPGainValue(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bAtk(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bAtk2(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bAtkRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bBaseAtk(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bDef(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bDef2(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bDefRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bDef2Rate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bNearAtkDef(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bLongAtkDef(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bMagicAtkDef(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bMiscAtkDef(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bCriticalDef(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bLongAtkRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bCritAtkRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bNoWeaponDamage(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bNoMagicDamage(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bNoMiscDamage(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bAtkEle(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bDefEle(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bDefRatioAtkEle(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bDefRatioAtkRace(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bDefRatioAtkClass(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bMatk(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bMatkRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bMdef(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bMdef2(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bMdefRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bMdef2Rate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bHealPower(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bHealPower2(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bAddItemHealRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bCastRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bFixedCastRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bFixedCast(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bVariableCastRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bVariableCast(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bNoCastCancel(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bNoCastCancel2(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bDelayRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bHit(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bHitRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bCritical(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bCriticalRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bCriticalLong(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bFlee(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bFleeRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bFlee2(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bFlee2Rate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bPerfectHitRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bPerfectHitAddRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bSpeedRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bSpeedAddRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bAspd(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bAspdRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bAtkRange(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bAddMaxWeight(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bIgnoreDefRace(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bIgnoreMdefRace(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bIgnoreDefEle(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bIgnoreMdefEle(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bIgnoreMdefRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bIgnoreDefClass(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bIgnoreMdefClass(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bMagicDamageReturn(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bShortWeaponDamageReturn(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bLongWeaponDamageReturn(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bUnstripable(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bUnstripableWeapon(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bUnstripableArmor(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bUnstripableHelm(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bUnstripableShield(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bUnbreakable(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bUnbreakableGarment(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bUnbreakableWeapon(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bUnbreakableArmor(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bUnbreakableHelm(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bUnbreakableShield(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bUnbreakableShoes(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bBreakWeaponRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bBreakArmorRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bAddMonsterDropChainItem(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bDoubleRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bDoubleAddRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bSplashRange(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bSplashAddRange(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bClassChange(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bAddStealRate(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bRestartFullRecover(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bNoSizeFix(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bNoGemStone(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bIntravision(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bNoKnockback(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bPerfectHide(Character& src, OperationType opType, int32_t arg1)
{}
void TriggerEffects::bHPRegenRate(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bHPLossRate(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bSPRegenRate(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bSPLossRate(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bSkillUseSP(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bSkillUseSPRate(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bHPDrainValue(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bHPDrainRate(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bSPDrainValue(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bSPDrainRate(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bHPDrainValueRace(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bSPDrainValueRace(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bHPVanishRate(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bSPVanishRate(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bHPGainRaceAttack(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bSPGainRaceAttack(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bSPGainRace(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bSkillAtk(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bWeaponAtk(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bWeaponAtkRate(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bSkillHeal(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bSkillHeal2(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bAddItemHealRate(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bAddItemGroupHealRate(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bCastRate(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bFixedCastRate(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bSkillFixedCast(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bVariableCastRate(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bSkillVariableCast(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bSkillCooldown(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bAddSize(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bMagicAddSize(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bSubSize(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bAddRaceTolerance(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bAddRace(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bMagicAddRace(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bSubRace(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bCriticalAddRace(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bAddRace2(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bSubRace2(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bAddEle(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bSubEle(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bMagicAddEle(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bMagicAtkEle(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bAddDamageMonster(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bAddMagicDamageMonster(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bAddDefMonster(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bAddMdefMonster(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bSubClass(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bAddClass(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bIgnoreDefRaceRate(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bIgnoreMdefRaceRate(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bIgnoreDefClassRate(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bIgnoreMdefClassRate(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bExpAddRace(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bResEff(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bAddEff(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bAddEff2(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bAddEffWhenHit(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bComaRace(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bComaEle(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bAddRaceDropItem(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bAddRaceDropChainItem(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bAddMonsterDropItemGroup(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bGetZenyNum(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bAddGetZenyNum(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bAddSkillBlow(Character& src, OperationType opType, int32_t arg1, int32_t arg2)
{}
void TriggerEffects::bHPVanishRate(Character& src, OperationType opType, int32_t arg1, int32_t arg2, int32_t arg3)
{}
void TriggerEffects::bSPVanishRate(Character& src, OperationType opType, int32_t arg1, int32_t arg2, int32_t arg3)
{}
void TriggerEffects::bAddEle(Character& src, OperationType opType, int32_t arg1, int32_t arg2, int32_t arg3)
{}
void TriggerEffects::bSubEle(Character& src, OperationType opType, int32_t arg1, int32_t arg2, int32_t arg3)
{}
void TriggerEffects::bAddEff(Character& src, OperationType opType, int32_t arg1, int32_t arg2, int32_t arg3)
{}
void TriggerEffects::bAddEffOnSkill(Character& src, OperationType opType, int32_t arg1, int32_t arg2, int32_t arg3)
{}
void TriggerEffects::bAddEffWhenHit(Character& src, OperationType opType, int32_t arg1, int32_t arg2, int32_t arg3)
{}
void TriggerEffects::bAutoSpell(Character& src, OperationType opType, int32_t arg1, int32_t arg2, int32_t arg3)
{}
void TriggerEffects::bAutoSpellWhenHit(Character& src, OperationType opType, int32_t arg1, int32_t arg2, int32_t arg3)
{}
void TriggerEffects::bSPDrainRate(Character& src, OperationType opType, int32_t arg1, int32_t arg2, int32_t arg3)
{}
void TriggerEffects::bHPDrainValueRace(Character& src, OperationType opType, int32_t arg1, int32_t arg2, int32_t arg3)
{}
void TriggerEffects::bSPDrainValueRace(Character& src, OperationType opType, int32_t arg1, int32_t arg2, int32_t arg3)
{}
void TriggerEffects::bAddMonsterDropItem(Character& src, OperationType opType, int32_t arg1, int32_t arg2, int32_t arg3)
{}
void TriggerEffects::bAddRaceDropItem(Character& src, OperationType opType, int32_t arg1, int32_t arg2, int32_t arg3)
{}
void TriggerEffects::bSetDefRace(Character& src, OperationType opType, int32_t arg1, int32_t arg2, int32_t arg3, int32_t arg4)
{}
void TriggerEffects::bSetMDefRace(Character& src, OperationType opType, int32_t arg1, int32_t arg2, int32_t arg3, int32_t arg4)
{}
void TriggerEffects::bAddEff(Character& src, OperationType opType, int32_t arg1, int32_t arg2, int32_t arg3, int32_t arg4)
{}
void TriggerEffects::bAddEffOnSkill(Character& src, OperationType opType, int32_t arg1, int32_t arg2, int32_t arg3, int32_t arg4)
{}
void TriggerEffects::bAutoSpellOnSkill(Character& src, OperationType opType, int32_t arg1, int32_t arg2, int32_t arg3, int32_t arg4)
{}
void TriggerEffects::bAutoSpell(Character& src, OperationType opType, int32_t arg1, int32_t arg2, int32_t arg3, int32_t arg4)
{}
void TriggerEffects::bAutoSpellWhenHit(Character& src, OperationType opType, int32_t arg1, int32_t arg2, int32_t arg3, int32_t arg4)
{}
void TriggerEffects::bAutoSpellOnSkill(Character& src, OperationType opType, int32_t arg1, int32_t arg2, int32_t arg3, int32_t arg4, int32_t arg5)
{}
void TriggerEffects::bAutoSpell(Character& src, OperationType opType, int32_t arg1, int32_t arg2, int32_t arg3, int32_t arg4, int32_t arg5)
{}
void TriggerEffects::bAutoSpellWhenHit(Character& src, OperationType opType, int32_t arg1, int32_t arg2, int32_t arg3, int32_t arg4, int32_t arg5)
{}

// 1 arguments handlers
void TriggerEffects::triggerEffect(uint8_t index, Character& character,
	OperationType operation, int32_t val1)
{
	using ArrayType = std::array<TriggerEffects::TriggerEffectHandler1, enum_cast(TriggerEffect1ArgId::Last) + 1>;

	static constexpr ArrayType _table = []() constexpr
	{
		ArrayType table = {};

		table[enum_cast(TriggerEffect1ArgId::bStr)] = &TriggerEffects::bStr;
		table[enum_cast(TriggerEffect1ArgId::bAgi)] = &TriggerEffects::bAgi;
		table[enum_cast(TriggerEffect1ArgId::bVit)] = &TriggerEffects::bVit;
		table[enum_cast(TriggerEffect1ArgId::bInt)] = &TriggerEffects::bInt;
		table[enum_cast(TriggerEffect1ArgId::bDex)] = &TriggerEffects::bDex;
		table[enum_cast(TriggerEffect1ArgId::bLuk)] = &TriggerEffects::bLuk;
		table[enum_cast(TriggerEffect1ArgId::bAgiVit)] = &TriggerEffects::bAgiVit;
		table[enum_cast(TriggerEffect1ArgId::bAgiDexStr)] = &TriggerEffects::bAgiDexStr;
		table[enum_cast(TriggerEffect1ArgId::bAllStats)] = &TriggerEffects::bAllStats;
		table[enum_cast(TriggerEffect1ArgId::bMaxHP)] = &TriggerEffects::bMaxHP;
		table[enum_cast(TriggerEffect1ArgId::bMaxHPRate)] = &TriggerEffects::bMaxHPRate;
		table[enum_cast(TriggerEffect1ArgId::bMaxSP)] = &TriggerEffects::bMaxSP;
		table[enum_cast(TriggerEffect1ArgId::bMaxSPRate)] = &TriggerEffects::bMaxSPRate;
		table[enum_cast(TriggerEffect1ArgId::bHPrecovRate)] = &TriggerEffects::bHPrecovRate;
		table[enum_cast(TriggerEffect1ArgId::bSPrecovRate)] = &TriggerEffects::bSPrecovRate;
		table[enum_cast(TriggerEffect1ArgId::bUseSPRate)] = &TriggerEffects::bUseSPRate;
		table[enum_cast(TriggerEffect1ArgId::bNoRegen)] = &TriggerEffects::bNoRegen;
		table[enum_cast(TriggerEffect1ArgId::bHPDrainValue)] = &TriggerEffects::bHPDrainValue;
		table[enum_cast(TriggerEffect1ArgId::bSPDrainValue)] = &TriggerEffects::bSPDrainValue;
		table[enum_cast(TriggerEffect1ArgId::bHPDrainRate)] = &TriggerEffects::bHPDrainRate;
		table[enum_cast(TriggerEffect1ArgId::bSPDrainRate)] = &TriggerEffects::bSPDrainRate;
		table[enum_cast(TriggerEffect1ArgId::bHPGainValue)] = &TriggerEffects::bHPGainValue;
		table[enum_cast(TriggerEffect1ArgId::bSPGainValue)] = &TriggerEffects::bSPGainValue;
		table[enum_cast(TriggerEffect1ArgId::bMagicHPGainValue)] = &TriggerEffects::bMagicHPGainValue;
		table[enum_cast(TriggerEffect1ArgId::bMagicSPGainValue)] = &TriggerEffects::bMagicSPGainValue;
		table[enum_cast(TriggerEffect1ArgId::bAtk)] = &TriggerEffects::bAtk;
		table[enum_cast(TriggerEffect1ArgId::bAtk2)] = &TriggerEffects::bAtk2;
		table[enum_cast(TriggerEffect1ArgId::bAtkRate)] = &TriggerEffects::bAtkRate;
		table[enum_cast(TriggerEffect1ArgId::bBaseAtk)] = &TriggerEffects::bBaseAtk;
		table[enum_cast(TriggerEffect1ArgId::bDef)] = &TriggerEffects::bDef;
		table[enum_cast(TriggerEffect1ArgId::bDef2)] = &TriggerEffects::bDef2;
		table[enum_cast(TriggerEffect1ArgId::bDefRate)] = &TriggerEffects::bDefRate;
		table[enum_cast(TriggerEffect1ArgId::bDef2Rate)] = &TriggerEffects::bDef2Rate;
		table[enum_cast(TriggerEffect1ArgId::bNearAtkDef)] = &TriggerEffects::bNearAtkDef;
		table[enum_cast(TriggerEffect1ArgId::bLongAtkDef)] = &TriggerEffects::bLongAtkDef;
		table[enum_cast(TriggerEffect1ArgId::bMagicAtkDef)] = &TriggerEffects::bMagicAtkDef;
		table[enum_cast(TriggerEffect1ArgId::bMiscAtkDef)] = &TriggerEffects::bMiscAtkDef;
		table[enum_cast(TriggerEffect1ArgId::bCriticalDef)] = &TriggerEffects::bCriticalDef;
		table[enum_cast(TriggerEffect1ArgId::bLongAtkRate)] = &TriggerEffects::bLongAtkRate;
		table[enum_cast(TriggerEffect1ArgId::bCritAtkRate)] = &TriggerEffects::bCritAtkRate;
		table[enum_cast(TriggerEffect1ArgId::bNoWeaponDamage)] = &TriggerEffects::bNoWeaponDamage;
		table[enum_cast(TriggerEffect1ArgId::bNoMagicDamage)] = &TriggerEffects::bNoMagicDamage;
		table[enum_cast(TriggerEffect1ArgId::bNoMiscDamage)] = &TriggerEffects::bNoMiscDamage;
		table[enum_cast(TriggerEffect1ArgId::bAtkEle)] = &TriggerEffects::bAtkEle;
		table[enum_cast(TriggerEffect1ArgId::bDefEle)] = &TriggerEffects::bDefEle;
		table[enum_cast(TriggerEffect1ArgId::bDefRatioAtkEle)] = &TriggerEffects::bDefRatioAtkEle;
		table[enum_cast(TriggerEffect1ArgId::bDefRatioAtkRace)] = &TriggerEffects::bDefRatioAtkRace;
		table[enum_cast(TriggerEffect1ArgId::bDefRatioAtkClass)] = &TriggerEffects::bDefRatioAtkClass;
		table[enum_cast(TriggerEffect1ArgId::bMatk)] = &TriggerEffects::bMatk;
		table[enum_cast(TriggerEffect1ArgId::bMatkRate)] = &TriggerEffects::bMatkRate;
		table[enum_cast(TriggerEffect1ArgId::bMdef)] = &TriggerEffects::bMdef;
		table[enum_cast(TriggerEffect1ArgId::bMdef2)] = &TriggerEffects::bMdef2;
		table[enum_cast(TriggerEffect1ArgId::bMdefRate)] = &TriggerEffects::bMdefRate;
		table[enum_cast(TriggerEffect1ArgId::bMdef2Rate)] = &TriggerEffects::bMdef2Rate;
		table[enum_cast(TriggerEffect1ArgId::bHealPower)] = &TriggerEffects::bHealPower;
		table[enum_cast(TriggerEffect1ArgId::bHealPower2)] = &TriggerEffects::bHealPower2;
		table[enum_cast(TriggerEffect1ArgId::bAddItemHealRate)] = &TriggerEffects::bAddItemHealRate;
		table[enum_cast(TriggerEffect1ArgId::bCastRate)] = &TriggerEffects::bCastRate;
		table[enum_cast(TriggerEffect1ArgId::bFixedCastRate)] = &TriggerEffects::bFixedCastRate;
		table[enum_cast(TriggerEffect1ArgId::bFixedCast)] = &TriggerEffects::bFixedCast;
		table[enum_cast(TriggerEffect1ArgId::bVariableCastRate)] = &TriggerEffects::bVariableCastRate;
		table[enum_cast(TriggerEffect1ArgId::bVariableCast)] = &TriggerEffects::bVariableCast;
		table[enum_cast(TriggerEffect1ArgId::bNoCastCancel)] = &TriggerEffects::bNoCastCancel;
		table[enum_cast(TriggerEffect1ArgId::bNoCastCancel2)] = &TriggerEffects::bNoCastCancel2;
		table[enum_cast(TriggerEffect1ArgId::bDelayRate)] = &TriggerEffects::bDelayRate;
		table[enum_cast(TriggerEffect1ArgId::bHit)] = &TriggerEffects::bHit;
		table[enum_cast(TriggerEffect1ArgId::bHitRate)] = &TriggerEffects::bHitRate;
		table[enum_cast(TriggerEffect1ArgId::bCritical)] = &TriggerEffects::bCritical;
		table[enum_cast(TriggerEffect1ArgId::bCriticalRate)] = &TriggerEffects::bCriticalRate;
		table[enum_cast(TriggerEffect1ArgId::bCriticalLong)] = &TriggerEffects::bCriticalLong;
		table[enum_cast(TriggerEffect1ArgId::bFlee)] = &TriggerEffects::bFlee;
		table[enum_cast(TriggerEffect1ArgId::bFleeRate)] = &TriggerEffects::bFleeRate;
		table[enum_cast(TriggerEffect1ArgId::bFlee2)] = &TriggerEffects::bFlee2;
		table[enum_cast(TriggerEffect1ArgId::bFlee2Rate)] = &TriggerEffects::bFlee2Rate;
		table[enum_cast(TriggerEffect1ArgId::bPerfectHitRate)] = &TriggerEffects::bPerfectHitRate;
		table[enum_cast(TriggerEffect1ArgId::bPerfectHitAddRate)] = &TriggerEffects::bPerfectHitAddRate;
		table[enum_cast(TriggerEffect1ArgId::bSpeedRate)] = &TriggerEffects::bSpeedRate;
		table[enum_cast(TriggerEffect1ArgId::bSpeedAddRate)] = &TriggerEffects::bSpeedAddRate;
		table[enum_cast(TriggerEffect1ArgId::bAspd)] = &TriggerEffects::bAspd;
		table[enum_cast(TriggerEffect1ArgId::bAspdRate)] = &TriggerEffects::bAspdRate;
		table[enum_cast(TriggerEffect1ArgId::bAtkRange)] = &TriggerEffects::bAtkRange;
		table[enum_cast(TriggerEffect1ArgId::bAddMaxWeight)] = &TriggerEffects::bAddMaxWeight;
		table[enum_cast(TriggerEffect1ArgId::bIgnoreDefRace)] = &TriggerEffects::bIgnoreDefRace;
		table[enum_cast(TriggerEffect1ArgId::bIgnoreMdefRace)] = &TriggerEffects::bIgnoreMdefRace;
		table[enum_cast(TriggerEffect1ArgId::bIgnoreDefEle)] = &TriggerEffects::bIgnoreDefEle;
		table[enum_cast(TriggerEffect1ArgId::bIgnoreMdefEle)] = &TriggerEffects::bIgnoreMdefEle;
		table[enum_cast(TriggerEffect1ArgId::bIgnoreMdefRate)] = &TriggerEffects::bIgnoreMdefRate;
		table[enum_cast(TriggerEffect1ArgId::bIgnoreDefClass)] = &TriggerEffects::bIgnoreDefClass;
		table[enum_cast(TriggerEffect1ArgId::bIgnoreMdefClass)] = &TriggerEffects::bIgnoreMdefClass;
		table[enum_cast(TriggerEffect1ArgId::bMagicDamageReturn)] = &TriggerEffects::bMagicDamageReturn;
		table[enum_cast(TriggerEffect1ArgId::bShortWeaponDamageReturn)] = &TriggerEffects::bShortWeaponDamageReturn;
		table[enum_cast(TriggerEffect1ArgId::bLongWeaponDamageReturn)] = &TriggerEffects::bLongWeaponDamageReturn;
		table[enum_cast(TriggerEffect1ArgId::bUnstripable)] = &TriggerEffects::bUnstripable;
		table[enum_cast(TriggerEffect1ArgId::bUnstripableWeapon)] = &TriggerEffects::bUnstripableWeapon;
		table[enum_cast(TriggerEffect1ArgId::bUnstripableArmor)] = &TriggerEffects::bUnstripableArmor;
		table[enum_cast(TriggerEffect1ArgId::bUnstripableHelm)] = &TriggerEffects::bUnstripableHelm;
		table[enum_cast(TriggerEffect1ArgId::bUnstripableShield)] = &TriggerEffects::bUnstripableShield;
		table[enum_cast(TriggerEffect1ArgId::bUnbreakable)] = &TriggerEffects::bUnbreakable;
		table[enum_cast(TriggerEffect1ArgId::bUnbreakableGarment)] = &TriggerEffects::bUnbreakableGarment;
		table[enum_cast(TriggerEffect1ArgId::bUnbreakableWeapon)] = &TriggerEffects::bUnbreakableWeapon;
		table[enum_cast(TriggerEffect1ArgId::bUnbreakableArmor)] = &TriggerEffects::bUnbreakableArmor;
		table[enum_cast(TriggerEffect1ArgId::bUnbreakableHelm)] = &TriggerEffects::bUnbreakableHelm;
		table[enum_cast(TriggerEffect1ArgId::bUnbreakableShield)] = &TriggerEffects::bUnbreakableShield;
		table[enum_cast(TriggerEffect1ArgId::bUnbreakableShoes)] = &TriggerEffects::bUnbreakableShoes;
		table[enum_cast(TriggerEffect1ArgId::bBreakWeaponRate)] = &TriggerEffects::bBreakWeaponRate;
		table[enum_cast(TriggerEffect1ArgId::bBreakArmorRate)] = &TriggerEffects::bBreakArmorRate;
		table[enum_cast(TriggerEffect1ArgId::bAddMonsterDropChainItem)] = &TriggerEffects::bAddMonsterDropChainItem;
		table[enum_cast(TriggerEffect1ArgId::bDoubleRate)] = &TriggerEffects::bDoubleRate;
		table[enum_cast(TriggerEffect1ArgId::bDoubleAddRate)] = &TriggerEffects::bDoubleAddRate;
		table[enum_cast(TriggerEffect1ArgId::bSplashRange)] = &TriggerEffects::bSplashRange;
		table[enum_cast(TriggerEffect1ArgId::bSplashAddRange)] = &TriggerEffects::bSplashAddRange;
		table[enum_cast(TriggerEffect1ArgId::bClassChange)] = &TriggerEffects::bClassChange;
		table[enum_cast(TriggerEffect1ArgId::bAddStealRate)] = &TriggerEffects::bAddStealRate;
		table[enum_cast(TriggerEffect1ArgId::bRestartFullRecover)] = &TriggerEffects::bRestartFullRecover;
		table[enum_cast(TriggerEffect1ArgId::bNoSizeFix)] = &TriggerEffects::bNoSizeFix;
		table[enum_cast(TriggerEffect1ArgId::bNoGemStone)] = &TriggerEffects::bNoGemStone;
		table[enum_cast(TriggerEffect1ArgId::bIntravision)] = &TriggerEffects::bIntravision;
		table[enum_cast(TriggerEffect1ArgId::bNoKnockback)] = &TriggerEffects::bNoKnockback;
		table[enum_cast(TriggerEffect1ArgId::bPerfectHide)] = &TriggerEffects::bPerfectHide;

		return table;
	}();

	static_assert([]() constexpr
	{
#ifdef NDEBUG
		for (auto&& v : _table)
			if (v == nullptr)
				return false;
#endif
		return true;
	}());

	(this->*_table[index])(character, operation, val1);
}

// 2 arguments handlers
void TriggerEffects::triggerEffect(uint8_t index, Character& character,
	OperationType operation, int32_t val1, int32_t val2)
{
	using ArrayType = std::array<TriggerEffects::TriggerEffectHandler2, enum_cast(TriggerEffect2ArgId::Last) + 1>;

	static constexpr ArrayType _table = []() constexpr
	{
		ArrayType table = {};

		table[enum_cast(TriggerEffect2ArgId::bHPRegenRate)] = &TriggerEffects::bHPRegenRate;
		table[enum_cast(TriggerEffect2ArgId::bHPLossRate)] = &TriggerEffects::bHPLossRate;
		table[enum_cast(TriggerEffect2ArgId::bSPRegenRate)] = &TriggerEffects::bSPRegenRate;
		table[enum_cast(TriggerEffect2ArgId::bSPLossRate)] = &TriggerEffects::bSPLossRate;
		table[enum_cast(TriggerEffect2ArgId::bSkillUseSP)] = &TriggerEffects::bSkillUseSP;
		table[enum_cast(TriggerEffect2ArgId::bSkillUseSPRate)] = &TriggerEffects::bSkillUseSPRate;
		table[enum_cast(TriggerEffect2ArgId::bHPDrainValue)] = &TriggerEffects::bHPDrainValue;
		table[enum_cast(TriggerEffect2ArgId::bHPDrainRate)] = &TriggerEffects::bHPDrainRate;
		table[enum_cast(TriggerEffect2ArgId::bSPDrainValue)] = &TriggerEffects::bSPDrainValue;
		table[enum_cast(TriggerEffect2ArgId::bSPDrainRate)] = &TriggerEffects::bSPDrainRate;
		table[enum_cast(TriggerEffect2ArgId::bHPDrainValueRace)] = &TriggerEffects::bHPDrainValueRace;
		table[enum_cast(TriggerEffect2ArgId::bSPDrainValueRace)] = &TriggerEffects::bSPDrainValueRace;
		table[enum_cast(TriggerEffect2ArgId::bHPVanishRate)] = &TriggerEffects::bHPVanishRate;
		table[enum_cast(TriggerEffect2ArgId::bSPVanishRate)] = &TriggerEffects::bSPVanishRate;
		table[enum_cast(TriggerEffect2ArgId::bHPGainRaceAttack)] = &TriggerEffects::bHPGainRaceAttack;
		table[enum_cast(TriggerEffect2ArgId::bSPGainRaceAttack)] = &TriggerEffects::bSPGainRaceAttack;
		table[enum_cast(TriggerEffect2ArgId::bSPGainRace)] = &TriggerEffects::bSPGainRace;
		table[enum_cast(TriggerEffect2ArgId::bSkillAtk)] = &TriggerEffects::bSkillAtk;
		table[enum_cast(TriggerEffect2ArgId::bWeaponAtk)] = &TriggerEffects::bWeaponAtk;
		table[enum_cast(TriggerEffect2ArgId::bWeaponAtkRate)] = &TriggerEffects::bWeaponAtkRate;
		table[enum_cast(TriggerEffect2ArgId::bSkillHeal)] = &TriggerEffects::bSkillHeal;
		table[enum_cast(TriggerEffect2ArgId::bSkillHeal2)] = &TriggerEffects::bSkillHeal2;
		table[enum_cast(TriggerEffect2ArgId::bAddItemHealRate)] = &TriggerEffects::bAddItemHealRate;
		table[enum_cast(TriggerEffect2ArgId::bAddItemGroupHealRate)] = &TriggerEffects::bAddItemGroupHealRate;
		table[enum_cast(TriggerEffect2ArgId::bCastRate)] = &TriggerEffects::bCastRate;
		table[enum_cast(TriggerEffect2ArgId::bFixedCastRate)] = &TriggerEffects::bFixedCastRate;
		table[enum_cast(TriggerEffect2ArgId::bSkillFixedCast)] = &TriggerEffects::bSkillFixedCast;
		table[enum_cast(TriggerEffect2ArgId::bVariableCastRate)] = &TriggerEffects::bVariableCastRate;
		table[enum_cast(TriggerEffect2ArgId::bSkillVariableCast)] = &TriggerEffects::bSkillVariableCast;
		table[enum_cast(TriggerEffect2ArgId::bSkillCooldown)] = &TriggerEffects::bSkillCooldown;
		table[enum_cast(TriggerEffect2ArgId::bAddSize)] = &TriggerEffects::bAddSize;
		table[enum_cast(TriggerEffect2ArgId::bMagicAddSize)] = &TriggerEffects::bMagicAddSize;
		table[enum_cast(TriggerEffect2ArgId::bSubSize)] = &TriggerEffects::bSubSize;
		table[enum_cast(TriggerEffect2ArgId::bAddRaceTolerance)] = &TriggerEffects::bAddRaceTolerance;
		table[enum_cast(TriggerEffect2ArgId::bAddRace)] = &TriggerEffects::bAddRace;
		table[enum_cast(TriggerEffect2ArgId::bMagicAddRace)] = &TriggerEffects::bMagicAddRace;
		table[enum_cast(TriggerEffect2ArgId::bSubRace)] = &TriggerEffects::bSubRace;
		table[enum_cast(TriggerEffect2ArgId::bCriticalAddRace)] = &TriggerEffects::bCriticalAddRace;
		table[enum_cast(TriggerEffect2ArgId::bAddRace2)] = &TriggerEffects::bAddRace2;
		table[enum_cast(TriggerEffect2ArgId::bSubRace2)] = &TriggerEffects::bSubRace2;
		table[enum_cast(TriggerEffect2ArgId::bAddEle)] = &TriggerEffects::bAddEle;
		table[enum_cast(TriggerEffect2ArgId::bSubEle)] = &TriggerEffects::bSubEle;
		table[enum_cast(TriggerEffect2ArgId::bMagicAddEle)] = &TriggerEffects::bMagicAddEle;
		table[enum_cast(TriggerEffect2ArgId::bMagicAtkEle)] = &TriggerEffects::bMagicAtkEle;
		table[enum_cast(TriggerEffect2ArgId::bAddDamageMonster)] = &TriggerEffects::bAddDamageMonster;
		table[enum_cast(TriggerEffect2ArgId::bAddMagicDamageMonster)] = &TriggerEffects::bAddMagicDamageMonster;
		table[enum_cast(TriggerEffect2ArgId::bAddDefMonster)] = &TriggerEffects::bAddDefMonster;
		table[enum_cast(TriggerEffect2ArgId::bAddMdefMonster)] = &TriggerEffects::bAddMdefMonster;
		table[enum_cast(TriggerEffect2ArgId::bSubClass)] = &TriggerEffects::bSubClass;
		table[enum_cast(TriggerEffect2ArgId::bAddClass)] = &TriggerEffects::bAddClass;
		table[enum_cast(TriggerEffect2ArgId::bIgnoreDefRaceRate)] = &TriggerEffects::bIgnoreDefRaceRate;
		table[enum_cast(TriggerEffect2ArgId::bIgnoreMdefRaceRate)] = &TriggerEffects::bIgnoreMdefRaceRate;
		table[enum_cast(TriggerEffect2ArgId::bIgnoreDefClassRate)] = &TriggerEffects::bIgnoreDefClassRate;
		table[enum_cast(TriggerEffect2ArgId::bIgnoreMdefClassRate)] = &TriggerEffects::bIgnoreMdefClassRate;
		table[enum_cast(TriggerEffect2ArgId::bExpAddRace)] = &TriggerEffects::bExpAddRace;
		table[enum_cast(TriggerEffect2ArgId::bResEff)] = &TriggerEffects::bResEff;
		table[enum_cast(TriggerEffect2ArgId::bAddEff)] = &TriggerEffects::bAddEff;
		table[enum_cast(TriggerEffect2ArgId::bAddEff2)] = &TriggerEffects::bAddEff2;
		table[enum_cast(TriggerEffect2ArgId::bAddEffWhenHit)] = &TriggerEffects::bAddEffWhenHit;
		table[enum_cast(TriggerEffect2ArgId::bComaRace)] = &TriggerEffects::bComaRace;
		table[enum_cast(TriggerEffect2ArgId::bComaEle)] = &TriggerEffects::bComaEle;
		table[enum_cast(TriggerEffect2ArgId::bAddRaceDropItem)] = &TriggerEffects::bAddRaceDropItem;
		table[enum_cast(TriggerEffect2ArgId::bAddRaceDropChainItem)] = &TriggerEffects::bAddRaceDropChainItem;
		table[enum_cast(TriggerEffect2ArgId::bAddMonsterDropItemGroup)] = &TriggerEffects::bAddMonsterDropItemGroup;
		table[enum_cast(TriggerEffect2ArgId::bGetZenyNum)] = &TriggerEffects::bGetZenyNum;
		table[enum_cast(TriggerEffect2ArgId::bAddGetZenyNum)] = &TriggerEffects::bAddGetZenyNum;
		table[enum_cast(TriggerEffect2ArgId::bAddSkillBlow)] = &TriggerEffects::bAddSkillBlow;

		return table;
	}();

	static_assert([]() constexpr
	{
#ifdef NDEBUG
		for (auto&& v : _table)
			if (v == nullptr)
				return false;
#endif
		return true;
	}());

	(this->*_table[index])(character, operation, val1, val2);
}

// 3 arguments handlers
void TriggerEffects::triggerEffect(uint8_t index, Character& character,
	OperationType operation, int32_t val1, int32_t val2, int32_t val3)
{
	using ArrayType = std::array<TriggerEffects::TriggerEffectHandler3, enum_cast(TriggerEffect3ArgId::Last) + 1>;

	static constexpr ArrayType _table = []() constexpr
	{
		ArrayType table = {};

		table[enum_cast(TriggerEffect3ArgId::bHPVanishRate)] = &TriggerEffects::bHPVanishRate;
		table[enum_cast(TriggerEffect3ArgId::bSPVanishRate)] = &TriggerEffects::bSPVanishRate;
		table[enum_cast(TriggerEffect3ArgId::bAddEle)] = &TriggerEffects::bAddEle;
		table[enum_cast(TriggerEffect3ArgId::bSubEle)] = &TriggerEffects::bSubEle;
		table[enum_cast(TriggerEffect3ArgId::bAddEff)] = &TriggerEffects::bAddEff;
		table[enum_cast(TriggerEffect3ArgId::bAddEffOnSkill)] = &TriggerEffects::bAddEffOnSkill;
		table[enum_cast(TriggerEffect3ArgId::bAddEffWhenHit)] = &TriggerEffects::bAddEffWhenHit;
		table[enum_cast(TriggerEffect3ArgId::bAutoSpell)] = &TriggerEffects::bAutoSpell;
		table[enum_cast(TriggerEffect3ArgId::bAutoSpellWhenHit)] = &TriggerEffects::bAutoSpellWhenHit;
		table[enum_cast(TriggerEffect3ArgId::bSPDrainRate)] = &TriggerEffects::bSPDrainRate;
		table[enum_cast(TriggerEffect3ArgId::bHPDrainValueRace)] = &TriggerEffects::bHPDrainValueRace;
		table[enum_cast(TriggerEffect3ArgId::bSPDrainValueRace)] = &TriggerEffects::bSPDrainValueRace;
		table[enum_cast(TriggerEffect3ArgId::bAddMonsterDropItem)] = &TriggerEffects::bAddMonsterDropItem;
		table[enum_cast(TriggerEffect3ArgId::bAddRaceDropItem)] = &TriggerEffects::bAddRaceDropItem;

		return table;
	}();

	static_assert([]() constexpr
	{
#ifdef NDEBUG
		for (auto&& v : _table)
			if (v == nullptr)
				return false;
#endif
		return true;
	}());

	(this->*_table[index])(character, operation, val1, val2, val3);
}

// 4 arguments handlers
void TriggerEffects::triggerEffect(uint8_t index, Character& character,
	OperationType operation, int32_t val1, int32_t val2, int32_t val3, int32_t val4)
{
	using ArrayType = std::array<TriggerEffects::TriggerEffectHandler4, enum_cast(TriggerEffect4ArgId::Last) + 1>;

	static constexpr ArrayType _table = []() constexpr
	{
		ArrayType table = {};

		table[enum_cast(TriggerEffect4ArgId::bSetDefRace)] = &TriggerEffects::bSetDefRace;
		table[enum_cast(TriggerEffect4ArgId::bSetMDefRace)] = &TriggerEffects::bSetMDefRace;
		table[enum_cast(TriggerEffect4ArgId::bAddEff)] = &TriggerEffects::bAddEff;
		table[enum_cast(TriggerEffect4ArgId::bAddEffOnSkill)] = &TriggerEffects::bAddEffOnSkill;
		table[enum_cast(TriggerEffect4ArgId::bAutoSpellOnSkill)] = &TriggerEffects::bAutoSpellOnSkill;
		table[enum_cast(TriggerEffect4ArgId::bAutoSpell)] = &TriggerEffects::bAutoSpell;
		table[enum_cast(TriggerEffect4ArgId::bAutoSpellWhenHit)] = &TriggerEffects::bAutoSpellWhenHit;

		return table;
	}();

	static_assert([]() constexpr
	{
#ifdef NDEBUG
		for (auto&& v : _table)
			if (v == nullptr)
				return false;
#endif
		return true;
	}());

	(this->*_table[index])(character, operation, val1, val2, val3, val4);
}

// 5 arguments handlers
void TriggerEffects::triggerEffect(uint8_t index, Character& character,
	OperationType operation, int32_t val1, int32_t val2, int32_t val3, int32_t val4, int32_t val5)
{
	using ArrayType = std::array<TriggerEffects::TriggerEffectHandler5, enum_cast(TriggerEffect5ArgId::Last) + 1>;

	static constexpr ArrayType _table = []() constexpr
	{
		ArrayType table = {};

		table[enum_cast(TriggerEffect5ArgId::bAutoSpellOnSkill)] = &TriggerEffects::bAutoSpellOnSkill;
		table[enum_cast(TriggerEffect5ArgId::bAutoSpell)] = &TriggerEffects::bAutoSpell;
		table[enum_cast(TriggerEffect5ArgId::bAutoSpellWhenHit)] = &TriggerEffects::bAutoSpellWhenHit;

		return table;
	}();

	static_assert([]() constexpr
	{
#ifdef NDEBUG
		for (auto&& v : _table)
			if (v == nullptr)
				return false;
#endif
		return true;
	}());

	(this->*_table[index])(character, operation, val1, val2, val3, val4, val5);
}