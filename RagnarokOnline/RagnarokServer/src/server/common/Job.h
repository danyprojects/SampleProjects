#pragma once

#include <cstdint>

struct Job final
{
	enum Value : uint8_t
	{
		Novice,

		FirstJobStart,
		Swordsman = FirstJobStart,
		Archer,
		Acolyte,
		Mage,
		Thief,
		Merchant,

		ExtraFirstJobStart,
		SuperNovice = ExtraFirstJobStart,
		Taekwon,
		Gunslinger,
		Ninja,

		SecondJobStart,
		Knight = SecondJobStart,
		Crusader,
		Hunter,
		Bard,
		Dancer,
		Priest,
		Monk,
		Wizard,
		Sage,
		Assassin,
		Rogue,
		Blacksmith,
		Alchemist,

		ExtraSecondJobStart,
		SoulLinker = ExtraSecondJobStart,
		StarGladiator,

		TransJobStart,
		LordKnight = TransJobStart,
		Paladin,
		Sniper,
		Clown,
		Gypsy,
		HighPriest,
		Champion,
		HighWizard,
		Professor,
		AssassinCross,
		Stalker,
		Whitesmith,
		Creator,

		Last = Creator,		
		All //This is a special job only for trigger effects, it should be ignored for everything else
	};

	auto constexpr toInt() const { return _value; }

	auto constexpr operator==(Job job) const { return _value == job._value; }
	auto constexpr operator!=(Job job) const { return _value != job._value; }

	constexpr Job getDependent() const
	{
		return dependents[_value];
	}

	constexpr Job()
		:_value(Novice)
	{ }

	constexpr Job(Value value)
		: _value(value)
	{}

private:
	static constexpr Value dependents[Job::Last + 1] =
	{
		Job::Novice, //novice
		Job::Novice, //swordsman
		Job::Novice, //Archer
		Job::Novice, //Acolyte
		Job::Novice, //Mage
		Job::Novice, //Thief
		Job::Novice, //Merchant
		Job::Novice, //SuperNovice
		Job::Novice, //Taekwon
		Job::Novice, //Gunslinger
		Job::Novice, //Ninja
		Job::Swordsman, //Knight
		Job::Swordsman, //Crusader
		Job::Archer, //Hunter
		Job::Archer, //Bard
		Job::Archer, //Dancer
		Job::Acolyte, //Priest
		Job::Acolyte, //Monk
		Job::Mage, //Wizard
		Job::Mage, //Sage
		Job::Thief, //Assassin
		Job::Thief, //Rogue
		Job::Merchant, //Blacksmith
		Job::Merchant, //Alchemist
		Job::Taekwon, //SoulLinker
		Job::Taekwon, //StarGladiator
		Job::Knight, //LordKnight
		Job::Crusader, //Paladin
		Job::Hunter, //Sniper
		Job::Bard, //Clown
		Job::Dancer, //Gypsy
		Job::Priest, //HighPriest
		Job::Monk, //Champion
		Job::Wizard, //HighWizard
		Job::Sage, //Professor
		Job::Assassin, //AssassinCross
		Job::Rogue, //Stalker
		Job::Blacksmith, //Whitesmith
		Job::Alchemist, //Creator
	};

	Value _value;
};