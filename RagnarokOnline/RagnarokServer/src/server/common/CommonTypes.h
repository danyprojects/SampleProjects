#pragma once

#include <cstdint>

typedef uint32_t Zeny;
typedef uint16_t Weight;
typedef uint32_t Hp;
typedef uint32_t Sp;
typedef uint16_t Str;
typedef uint16_t Agi;
typedef uint16_t Vit;
typedef uint16_t Int;
typedef uint16_t Dex;
typedef uint16_t Luck;
typedef uint16_t Atk;
typedef uint16_t Matk;
typedef uint16_t Def;
typedef uint16_t Mdef;
typedef uint16_t Hit;
typedef uint16_t Flee;
typedef uint16_t Crit;
typedef uint16_t AtkSpd;
typedef uint16_t WalkSpd;
typedef uint8_t HairStyle;
typedef uint16_t CastTime;

enum class ErrorCode : uint8_t
{
	Ok,
	GenericFail,
	DuplicateName,
	InternalError,
	FailedBellowLevel,
	Ownership,
	Overweight,
	InventoryFull
};

enum class MoveAction : uint8_t
{
	None = 0, //So we can use this as an if condition directly
	CastAoE,
	CastTarget,
	Attack,
	PickUpItem
};

enum class PathType : bool
{
	Hard = 0,
	Weak
};

enum class ElevatedAtCommand : uint8_t
{
	Size,
	Dex,
	Int,
	Agi,
	Vit,
	Luk,
	Str,
	Go,
	Warp,
	Zeny,
	Item,
	JobChange,
	JobLvl,
	BaseLvl,
	GuildLvl,
	AllSkills,
	Skill,

	None,
};


enum class Currency : uint8_t
{
	Zeny = 0,
	Cash
};

enum class Gender : uint8_t
{
	Any,
	Male,
	Female
};

enum class Size : uint8_t
{
	Small,
	Medium,
	Large,
	Last = Large
};

enum class Element : uint8_t
{
	Neutral = 0,
	Water,
	Earth,
	Fire,
	Wind,
	Poison,
	Holy,
	Dark,
	Ghost,
	Undead,
	Last = Undead,

	None = static_cast<uint8_t>(-1)
};

enum class ElementLvl : uint8_t
{
	_1 = 1,
	_2,
	_3,
	_4,
	Last = _4
};

enum class Race : uint8_t
{
	Formless = 0,
	Undead,
	Brute,
	Plant,
	Insect,
	Fish,
	Demon,
	DemiHuman,
	Angel,
	Dragon,
	Boss,
	NonBoss,
	Last = NonBoss
};

enum class ItemType : uint8_t
{
	Healing = 0,
	Usable,
	Etc,
	Armor,
	Weapon,
	Card,
	PetEgg,
	PetArmor,
	Ammo,
	DelayConsume,
	Cash,
};

enum class EquipSlot : uint8_t
{
	TopHeadgear = 0,
	MidHeadgear,
	LowHeadgear,
	Armor,
	Weapon,
	Shield,
	Garment,
	Shoes,
	Accessory_L,
	Accessory_R,

	Ammunition,

	Last = Ammunition,

	None = static_cast<uint8_t>(-1)
};

// these are used by some items to give bonus against a specific tribe of monsters
// todo: Load these into mob db from file db/pre-re/mob_race2_db.conf in master tool
enum class Tribe : uint8_t
{
	None = 0,

	Goblin,
	Kobold,
	Orc,
	Golem,
	Guardian,
	Ninja,
	Scaraba,
	Turtle,
	Last = Turtle
};

enum class GroupChat
{
	Party,
	Guild,
	Map,
	World
};

enum class ChatType
{
	Party,
	Guild,
	Map,
	World,
	Whisper
};

enum class WeaponType : uint8_t
{
	Barehand = 0,
	ShortSword,
	Sword,
	TwoHandSword,
	Spear,
	TwoHandSpear,
	Axe,
	TwoHandAxe,
	Mace,
	Rod,
	TwoHandRod,
	Bow,
	Knuckle,
	Instrument,
	Whip,
	Book,
	Katar,
	Handgun,
	Rifle,
	Gatling,
	Shotgun,
	GrenadeLauncher,
	Shuriken,
	Last = Shuriken
	// Removed extra processing only entries, server doesnt use it 
};