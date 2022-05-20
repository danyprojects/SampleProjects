#pragma once

#include <cstdint>

enum class SkillId : uint16_t
{
	First = 0,

	AreaOfEffectStart = First,

	StormGust = AreaOfEffectStart,
	MeteorStorm,
	MagnumBreak,
	FireWall,
	SafetyWall,
	ThunderStorm,
	IceWall,
	SightTrasher,
	FirePillar,
	LordOfVermilion,
	HeavensDrive,
	Quagmire,
	Ganbantein,
	GravitationalField,

	SingleTargetStart,
	Devotion = SingleTargetStart,
	Mammomite,
	Bash,
	Provoke,
	StoneCurse,
	ColdBolt,
	LightningBolt,
	NapalmBeat,
	FireBolt,
	FrostDiver,
	FireBall,
	SoulStrike,
	Sense,
	JupitelThunder,
	EarthSpike,
	WaterBall,
	MagicCrasher,
	NapalmVulcan,

	NoTargetStart,
	FirstAid = NoTargetStart,
	TrickDead,
	BasicSkill,
	SwordMastery,
	TwoHandSwordMastery,
	HpRecovery,
	SpRecovery,
	Endure,
	FatalBlow,
	MovingHpRecovery,
	AutoBerserk,
	Sight,
	EnergyCoat,
	SightBlaster,
	FrostNova,
	SoulDrain,
	MysticAmplification,

	Last = MysticAmplification,

	None = static_cast<uint16_t>(-1)
};