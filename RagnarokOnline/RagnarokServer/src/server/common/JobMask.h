#pragma once

#include <server/common/Job.h>

#include <sdk/enum_cast.hpp>

#include <cstdint>

struct JobMask final
{
	enum Value : uint64_t
	{
		Novice = 1 << Job::Novice,

		Swordsman = 1 << Job::Swordsman,
		Archer = 1 <<  Job::Archer,
		Acolyte = 1 << Job::Acolyte,
		Mage = 1 << Job::Mage,
		Thief = 1 << Job::Thief,
		Merchant = 1 << Job::Merchant,

		SuperNovice = 1 << Job::SuperNovice,
		Taekwon = 1 << Job::Taekwon,
		Gunslinger = 1 << Job::Gunslinger,
		Ninja = 1 << Job::Ninja,

		Knight = 1 << Job::Knight,
		Crusader = 1 << Job::Crusader,
		Hunter = 1 << Job::Hunter,
		Bard = 1 << Job::Bard,
		Dancer = 1 << Job::Dancer,
		Priest = 1 << Job::Priest,
		Monk = 1 << Job::Monk,
		Wizard = 1 << Job::Wizard,
		Sage = 1 << Job::Sage,
		Assassin = 1 << Job::Assassin,
		Rogue = 1 << Job::Rogue,
		Blacksmith = 1 << Job::Blacksmith,
		Alchemist = 1 << Job::Alchemist,

		SoulLinker = 1 << Job::SoulLinker,
		StarGladiator = 1 << Job::StarGladiator,

		LordKnight = 1 << Job::LordKnight,
		Paladin = 1 << Job::Paladin,
		Sniper = 1 << Job::Sniper,
		Clown = 1 << Job::Clown,
		Gypsy = 1 << Job::Gypsy,
		HighPriest = 1 << Job::HighPriest,
		Champion = 1ull << Job::Champion,
		HighWizard = 1ull << Job::HighWizard,
		Professor = 1ull << Job::Professor,
		AssassinCross = 1ull << Job::AssassinCross,
		Stalker = 1ull << Job::Stalker,
		Whitesmith = 1ull << Job::Whitesmith,
		Creator = 1ull << Job::Creator,
		All = -1
	};

	constexpr Value operator&(Value values) const
	{
		return static_cast<Value>(_value & values);
	}

	constexpr explicit JobMask(Value value)
		:_value(value)
	{}
	constexpr JobMask() : _value(Value::All) {}
private:
	union
	{
		struct
		{
			bool
				novice : 1,

				swordsman : 1,
				archer : 1,
				acolyte : 1,
				mage : 1,
				thief : 1,
				merchant : 1,

				superNovice : 1,
				taekwon : 1,
				gunslinger : 1,
				ninja : 1,

				knight : 1,
				crusader : 1,
				hunter : 1,
				bard : 1,
				dancer : 1,
				priest : 1,
				monk : 1,
				wizard : 1,
				sage : 1,
				assassin : 1,
				rogue : 1,
				blacksmith : 1,
				alchemist : 1,

				soulLinker : 1,
				starGladiator : 1,

				lordKnight : 1,
				paladin : 1,
				sniper : 1,
				clown : 1,
				gypsy : 1,
				highPriest : 1,
				champion : 1,
				highWizard : 1,
				professor : 1,
				assassinCross : 1,
				stalker : 1,
				whitesmith : 1,
				creator : 1;
		};
		Value _value;
	};
};

inline JobMask::Value operator|(JobMask::Value left, JobMask::Value right)
{
	return left | right;
}