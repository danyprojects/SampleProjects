#pragma once

#include <cstdint>

enum class EnterRangeType : uint8_t
{
	Default = 0x1,
	Teleport = 0x2,
	Unhide = 0x4,
	Uncloak = 0x8,
};

enum class LeaveRangeType : uint8_t
{
	Default = 0x1,
	Teleport = 0x2,
	Hide = 0x4,
	Cloak = 0x8,
};

enum class LocalPlayerChangeType : uint8_t
{
	MaxHp = 0,
	CurrentHp,
	MaxSp,
	CurrentSp,
	Str,
	Int,
	Agi,
	Vit,
	Dex,
	Luk,
	Atk,
	BonusAtk,
	Matk,
	Def,
	BonusDef,
	Flee,
	Hit
};

enum class OtherPlayerChangeType : uint8_t
{
	Hairstyle = 0,
	Job,
	EquipItem,
	UnequipItem,
	MoveSpd,
	AtkSpeed
};

enum class MonsterChangeType : uint8_t
{
	SpriteId = 0,
	MoveSpd,
	AtkSpeed,
	HpPercent
};

enum class DamageType : uint8_t
{
	PhysicalClose = 0,
	PhysicalRanged,
	Magic
};

enum class DamageHitType : uint8_t
{
	SingleHit = 0,
	MultiHit
};

enum class NpcAction : uint8_t
{
	Talk = 0
};