#pragma once

#include <server/common/SkillId.h>
#include <server/player_extensions/SkillTreeDb.h>

#include <sdk/enum_cast.hpp>
#include <sdk/array/ArrayView.hpp>

#include <array>
#include <cstdint>
#include <initializer_list>

class SkillDb final
{
public:
	static constexpr const SkillDb& getSkill(SkillId id);

	constexpr const SkillTreeDb::SkillEntry& skillTreeEntry(Job job) const
	{
		return _skillTreeEntryCb(job);
	}

	uint8_t _maxLevel;
	std::array<uint16_t, 10> _sp;
	std::array<uint8_t, 10> _range;
	std::array<uint16_t, 10> _cooldown;
	std::array<uint16_t, 10> _castTime;

private:
	struct Private;
	typedef const SkillTreeDb::SkillEntry&(*SkillTreeEntryCb)(Job job);

	template<typename T>
	struct ValueArray
	{
		constexpr ValueArray(std::initializer_list<T> list, T multiplier)
		{
			int i = 0;
			for (const auto& val : list)
				_array[i++] = val * multiplier;
		}
		constexpr operator std::array<T, 10>() const{ return _array; }

		std::array<T, 10> _array = {};
	};

	struct Sp : public ValueArray<uint16_t> { 
		constexpr Sp(std::initializer_list<uint16_t> list) : ValueArray(list, 1) {}
	};
	struct Range : public ValueArray<uint8_t> {
		constexpr Range(std::initializer_list<uint8_t> list) : ValueArray(list, 1) {}
	};
	struct Cooldown : public ValueArray<uint16_t> {
		constexpr Cooldown(std::initializer_list<uint16_t> list) : ValueArray(list, 1000) {}
	};
	struct CastTime : public ValueArray<uint16_t> {
		constexpr CastTime(std::initializer_list<uint16_t> list) : ValueArray(list, 1000) {}
	};
	struct MaxLvl
	{
		constexpr explicit MaxLvl(int value) : _value(value) {}
		constexpr operator int() const { return _value; }

		int _value;
	};

	constexpr SkillDb()
		: _maxLevel(0)
		, _sp({})
		, _range({})
		, _cooldown({})
		, _castTime({})
		, _skillTreeEntryCb(nullptr)
	{ }

	constexpr SkillDb(MaxLvl lvl, Sp sp, Range range, Cooldown cooldown, CastTime castTime, SkillTreeEntryCb cb)
		:_maxLevel(lvl)
		,_sp(_sp)
		,_range(range)
		,_cooldown(cooldown)
		,_castTime(castTime)
		,_skillTreeEntryCb(cb)
	{}

	SkillTreeEntryCb _skillTreeEntryCb;

	static constexpr const SkillTreeDb::SkillEntry& search(Job job, SkillId id)
	{
		for (const auto& entry : SkillTreeDb::getSkillTree(job))
		{
			if (entry._id == id)
				return entry;
		}

		throw;
	}

	//********************* SKILL DATA INITIALIZATION
	static constexpr std::array<SkillDb, enum_cast(SkillId::Last) + 1> initSkillDb()
	{
		std::array<SkillDb, enum_cast(SkillId::Last) + 1> table = {};

		table[enum_cast(SkillId::StormGust)] = SkillDb
		{
			MaxLvl	 {10},
			Sp		 {78, 78, 78, 78, 78, 78, 78, 78, 78, 78},
			Range	 {9, 9, 9, 9, 9, 9, 9, 9, 9, 9},
			Cooldown {5,5,5,5,5,5,5,5,5,5},
			CastTime {1,1,1,1,1,1,1,1,1,1},
			[](Job job)->const auto&
			{
				return search(Job::Wizard, SkillId::StormGust);
			}
		};
		table[enum_cast(SkillId::MeteorStorm)] = SkillDb
		{
			MaxLvl	 {10},
			Sp		 {20, 24, 30, 34, 40, 44, 50, 54, 60, 64},
			Range	 {9, 9, 9, 9, 9, 9, 9, 9, 9, 9},
			Cooldown {5,5,5,5,5,5,5,5,5,5},
			CastTime {5,5,5,5,5,5,5,5,5,5},
			[](Job job)->const auto&
			{
				return search(Job::Wizard, SkillId::MeteorStorm);
			}
		};
		table[enum_cast(SkillId::MagnumBreak)] = SkillDb
		{
			MaxLvl	 {10},
			Sp		 {30, 30, 30, 30, 30, 30, 30, 30, 30, 30},
			Range	 {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
			Cooldown {5,5,5,5,5,5,5,5,5,5},
			CastTime {5,5,5,5,5,5,5,5,5,5},
			[](Job job)->const auto&
			{
				return job == Job::SuperNovice ? search(Job::SuperNovice, SkillId::MagnumBreak)
											   : search(Job::Swordsman, SkillId::MagnumBreak);
			}
		};
		table[enum_cast(SkillId::FireWall)] = SkillDb
		{
			MaxLvl	 {10},
			Sp		 {40, 40, 40, 40, 40, 40, 40, 40, 40, 40},
			Range	 {9, 9, 9, 9, 9, 9, 9, 9, 9, 9},
			Cooldown {5,5,5,5,5,5,5,5,5,5},
			CastTime {5,5,5,5,5,5,5,5,5,5},
			[](Job job)->const auto&
			{
				return job == Job::SuperNovice ? search(Job::SuperNovice, SkillId::FireWall)
											   : search(Job::Mage, SkillId::FireWall);
			}
		};
		table[enum_cast(SkillId::SafetyWall)] = SkillDb
		{
			MaxLvl	 {10},
			Sp		 {30, 30, 30, 35, 35, 35, 40, 40, 40, 40},
			Range	 {9, 9, 9, 9, 9, 9, 9, 9, 9, 9},
			Cooldown {5,5,5,5,5,5,5,5,5,5},
			CastTime {5,5,5,5,5,5,5,5,5,5},
			[](Job job)->const auto&
			{
				return job == Job::Priest || job == Job::HighPriest ? search(Job::Priest, SkillId::SafetyWall)
					: job == Job::SuperNovice ? search(Job::SuperNovice, SkillId::SafetyWall)
					: search(Job::Mage, SkillId::SafetyWall);
			}
		};
		table[enum_cast(SkillId::ThunderStorm)] = SkillDb
		{
			MaxLvl	 {10},
			Sp		 {29, 34, 39, 44, 49, 54, 59, 64, 69, 74},
			Range	 {9, 9, 9, 9, 9, 9, 9, 9, 9, 9},
			Cooldown {5,5,5,5,5,5,5,5,5,5},
			CastTime {5,5,5,5,5,5,5,5,5,5},
			[](Job job)->const auto&
			{
				return job == Job::SuperNovice ? search(Job::SuperNovice, SkillId::ThunderStorm)
											   : search(Job::Mage, SkillId::ThunderStorm);
			}
		};
		table[enum_cast(SkillId::IceWall)] = SkillDb
		{
			MaxLvl	 {10},
			Sp		 {20, 20, 20, 20, 20, 20, 20, 20, 20, 20},
			Range	 {9, 9, 9, 9, 9, 9, 9, 9, 9, 9},
			Cooldown {5,5,5,5,5,5,5,5,5,5},
			CastTime {5,5,5,5,5,5,5,5,5,5},
			[](Job job)->const auto&
			{
				return search(Job::Wizard, SkillId::IceWall);
			}
		};
		table[enum_cast(SkillId::SightTrasher)] = SkillDb
		{
			MaxLvl	 {10},
			Sp		 {35, 37, 39, 41, 43, 45, 47, 49, 51, 53},
			Range	 {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
			Cooldown {5,5,5,5,5,5,5,5,5,5},
			CastTime {5,5,5,5,5,5,5,5,5,5},
			[](Job job)->const auto&
			{
				return search(Job::Wizard, SkillId::SightTrasher);
			}
		};
		table[enum_cast(SkillId::FirePillar)] = SkillDb
		{
			MaxLvl	 {10},
			Sp		 {75, 75, 75, 75, 75, 75, 75, 75, 75, 75},
			Range	 {9, 9, 9, 9, 9, 9, 9, 9, 9, 9},
			Cooldown {5,5,5,5,5,5,5,5,5,5},
			CastTime {5,5,5,5,5,5,5,5,5,5},
			[](Job job)->const auto&
			{
				return search(Job::Wizard, SkillId::FirePillar);
			}
		};
		table[enum_cast(SkillId::LordOfVermilion)] = SkillDb
		{
			MaxLvl	 {10},
			Sp		 {60, 64, 68, 72, 76, 80, 84, 88, 92, 96},
			Range	 {9, 9, 9, 9, 9, 9, 9, 9, 9, 9},
			Cooldown {5,5,5,5,5,5,5,5,5,5},
			CastTime {5,5,5,5,5,5,5,5,5,5},
			[](Job job)->const auto&
			{
				return search(Job::Wizard, SkillId::LordOfVermilion);
			}
		};
		table[enum_cast(SkillId::HeavensDrive)] = SkillDb
		{
			MaxLvl	 {5},
			Sp		 {28, 32, 36, 40, 44},
			Range	 {9, 9, 9, 9, 9},
			Cooldown {5,5,5,5,5},
			CastTime {5,5,5,5,5},
			[](Job job)->const auto&
			{
				return search(Job::Wizard, SkillId::HeavensDrive);
			}
		};
		table[enum_cast(SkillId::Quagmire)] = SkillDb
		{
			MaxLvl	 {5},
			Sp		 {5, 10, 15, 20, 25},
			Range	 {9, 9, 9, 9, 9},
			Cooldown {5,5,5,5,5},
			CastTime {5,5,5,5,5},
			[](Job job)->const auto&
			{
				return search(Job::Wizard, SkillId::Quagmire);
			}
		};
		table[enum_cast(SkillId::Ganbantein)] = SkillDb
		{
			MaxLvl	 {1},
			Sp		 {40},
			Range	 {18},
			Cooldown {5},
			CastTime {5},
			[](Job job)->const auto&
			{
				return search(Job::HighWizard, SkillId::Ganbantein);
			}
		};
		table[enum_cast(SkillId::GravitationalField)] = SkillDb
		{
			MaxLvl	 {5},
			Sp		 {20, 40, 60, 80, 100},
			Range	 {18, 18, 18, 18, 18},
			Cooldown {5,5,5,5,5},
			CastTime {5,5,5,5,5},
			[](Job job)->const auto&
			{
				return search(Job::HighWizard, SkillId::GravitationalField);
			}
		};
		table[enum_cast(SkillId::Devotion)] = SkillDb
		{
			MaxLvl	 {5},
			Sp		 {25, 25, 25, 25, 25},
			Range	 {7, 8, 9, 10, 11},
			Cooldown {5,5,5,5,5},
			CastTime {5,5,5,5,5},
			[](Job job)->const auto&
			{
				return search(Job::Crusader, SkillId::Devotion);
			}
		};
		table[enum_cast(SkillId::Mammomite)] = SkillDb
		{
			MaxLvl	 {10},
			Sp		 {5, 5, 5, 5, 5, 5, 5, 5, 5, 5},
			Range	 {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
			Cooldown {5,5,5,5,5,5,5,5,5,5},
			CastTime {5,5,5,5,5,5,5,5,5,5},
			[](Job job)->const auto&
			{
				return job == Job::SuperNovice ? search(Job::SuperNovice, SkillId::Mammomite)
											   : search(Job::Merchant, SkillId::Mammomite);
			}
		};
		table[enum_cast(SkillId::Bash)] = SkillDb
		{
			MaxLvl	 {10},
			Sp		 {8, 8, 8, 8, 8, 15, 15, 15, 15, 15},
			Range	 {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
			Cooldown {5,5,5,5,5,5,5,5,5,5},
			CastTime {5,5,5,5,5,5,5,5,5,5},
			[](Job job)->const auto&
			{
				return job == Job::SuperNovice ? search(Job::SuperNovice, SkillId::Bash)
											   : search(Job::Swordsman, SkillId::Bash);
			}
		};
		table[enum_cast(SkillId::Provoke)] = SkillDb
		{
			MaxLvl	 {10},
			Sp		 {4, 5, 6, 7, 8, 9, 10, 11, 12, 13},
			Range	 {9, 9, 9, 9, 9, 9, 9, 9, 9, 9},
			Cooldown {5,5,5,5,5,5,5,5,5,5},
			CastTime {5,5,5,5,5,5,5,5,5,5},
			[](Job job)->const auto&
			{
				return job == Job::SuperNovice ? search(Job::SuperNovice, SkillId::Provoke)
											   : search(Job::Swordsman, SkillId::Provoke);
			}
		};
		table[enum_cast(SkillId::StoneCurse)] = SkillDb
		{
			MaxLvl	 {10},
			Sp		 {25, 24, 23, 22, 21, 20, 19, 18, 17, 16},
			Range	 {2, 2, 2, 2, 2, 2, 2, 2, 2, 2},
			Cooldown {5,5,5,5,5,5,5,5,5,5},
			CastTime {5,5,5,5,5,5,5,5,5,5},
			[](Job job)->const auto&
			{
				return job == Job::SuperNovice ? search(Job::SuperNovice, SkillId::StoneCurse)
											   : search(Job::Mage, SkillId::StoneCurse);
			}
		};
		table[enum_cast(SkillId::ColdBolt)] = SkillDb
		{
			MaxLvl	 {10},
			Sp		 {12, 14, 16, 18, 20, 22, 24, 26, 28, 30},
			Range	 {9, 9, 9, 9, 9, 9, 9, 9, 9, 9},
			Cooldown {5,5,5,5,5,5,5,5,5,5},
			CastTime {5,5,5,5,5,5,5,5,5,5},
			[](Job job)->const auto&
			{
				return job == Job::SuperNovice ? search(Job::SuperNovice, SkillId::ColdBolt)
											   : search(Job::Mage, SkillId::ColdBolt);
			}
		};
		table[enum_cast(SkillId::LightningBolt)] = SkillDb
		{
			MaxLvl	 {10},
			Sp		 {12, 14, 16, 18, 20, 22, 24, 26, 28, 30},
			Range	 {9, 9, 9, 9, 9, 9, 9, 9, 9, 9},
			Cooldown {5,5,5,5,5,5,5,5,5,5},
			CastTime {5,5,5,5,5,5,5,5,5,5},
			[](Job job)->const auto&
			{
				return job == Job::SuperNovice ? search(Job::SuperNovice, SkillId::LightningBolt)
											   : search(Job::Mage, SkillId::LightningBolt);
			}
		};
		table[enum_cast(SkillId::NapalmBeat)] = SkillDb
		{
			MaxLvl	 {10},
			Sp		 {9, 9, 9, 12, 12, 12, 15, 15, 15, 18},
			Range	 {9, 9, 9, 9, 9, 9, 9, 9, 9, 9},
			Cooldown {5,5,5,5,5,5,5,5,5,5},
			CastTime {5,5,5,5,5,5,5,5,5,5},
			[](Job job)->const auto&
			{
				return job == Job::SuperNovice ? search(Job::SuperNovice, SkillId::NapalmBeat)
											   : search(Job::Mage, SkillId::NapalmBeat);
			}
		};
		table[enum_cast(SkillId::FireBolt)] = SkillDb
		{
			MaxLvl	 {10},
			Sp		 {12, 14, 16, 18, 20, 22, 24, 26, 28, 30},
			Range	 {9, 9, 9, 9, 9, 9, 9, 9, 9, 9},
			Cooldown {5,5,5,5,5,5,5,5,5,5},
			CastTime {5,5,5,5,5,5,5,5,5,5},
			[](Job job)->const auto&
			{
				return job == Job::SuperNovice ? search(Job::SuperNovice, SkillId::FireBolt)
											   : search(Job::Mage, SkillId::FireBolt);
			}
		};
		table[enum_cast(SkillId::FrostDiver)] = SkillDb
		{
			MaxLvl	 {10},
			Sp		 {25, 24, 23, 22, 21, 20, 19, 18, 17, 16},
			Range	 {9, 9, 9, 9, 9, 9, 9, 9, 9, 9},
			Cooldown {5,5,5,5,5,5,5,5,5,5},
			CastTime {5,5,5,5,5,5,5,5,5,5},
			[](Job job)->const auto&
			{
				return job == Job::SuperNovice ? search(Job::SuperNovice, SkillId::FrostDiver)
											   : search(Job::Mage, SkillId::FrostDiver);
			}
		};
		table[enum_cast(SkillId::FireBall)] = SkillDb
		{
			MaxLvl	 {10},
			Sp		 {25, 25, 25, 25, 25, 25, 25, 25, 25, 25},
			Range	 {9, 9, 9, 9, 9, 9, 9, 9, 9, 9},
			Cooldown {5,5,5,5,5,5,5,5,5,5},
			CastTime {5,5,5,5,5,5,5,5,5,5},
			[](Job job)->const auto&
			{
				return search(Job::Mage, SkillId::FireBall);
			}
		};
		table[enum_cast(SkillId::SoulStrike)] = SkillDb
		{
			MaxLvl	 {10},
			Sp		 {18, 14, 24, 20, 30, 26, 36, 32, 42, 38},
			Range	 {9, 9, 9, 9, 9, 9, 9, 9, 9, 9},
			Cooldown {5,5,5,5,5,5,5,5,5,5},
			CastTime {5,5,5,5,5,5,5,5,5,5},
			[](Job job)->const auto&
			{
				return job == Job::SuperNovice ? search(Job::SuperNovice, SkillId::SoulStrike)
											   : search(Job::Mage, SkillId::SoulStrike);
			}
		};
		table[enum_cast(SkillId::Sense)] = SkillDb
		{
			MaxLvl	 {1},
			Sp		 {10},
			Range	 {9},
			Cooldown {5},
			CastTime {5},
			[](Job job)->const auto&
			{
				return search(Job::Wizard, SkillId::Sense);
			}
		};
		table[enum_cast(SkillId::JupitelThunder)] = SkillDb
		{
			MaxLvl	 {10},
			Sp		 {20, 23, 26, 29, 32, 35, 38, 41, 44, 47},
			Range	 {9, 9, 9, 9, 9, 9, 9, 9, 9, 9},
			Cooldown {0,0,0,0,0,0,0,0,0,0},
			CastTime {1,1,1,1,1,1,1,1,1,1},
			[](Job job)->const auto&
			{
				return search(Job::Wizard, SkillId::JupitelThunder);
			}
		};
		table[enum_cast(SkillId::EarthSpike)] = SkillDb
		{
			MaxLvl	 {5},
			Sp		 {12, 14, 16, 18, 20},
			Range	 {9, 9, 9, 9, 9},
			Cooldown {5,5,5,5,5},
			CastTime {5,5,5,5,5},
			[](Job job)->const auto&
			{
				return search(Job::Wizard, SkillId::EarthSpike);
			}
		};
		table[enum_cast(SkillId::WaterBall)] = SkillDb
		{
			MaxLvl	 {5},
			Sp		 {15, 20, 20, 25, 25},
			Range	 {9, 9, 9, 9, 9},
			Cooldown {5,5,5,5,5},
			CastTime {5,5,5,5,5},
			[](Job job)->const auto&
			{
				return search(Job::Wizard, SkillId::WaterBall);
			}
		};
		table[enum_cast(SkillId::MagicCrasher)] = SkillDb
		{
			MaxLvl	 {1},
			Sp		 {8},
			Range	 {9},
			Cooldown {5},
			CastTime {5},
			[](Job job)->const auto&
			{
				return search(Job::HighWizard, SkillId::MagicCrasher);
			}
		};
		table[enum_cast(SkillId::NapalmVulcan)] = SkillDb
		{
			MaxLvl	 {5},
			Sp		 {10, 25, 40, 55, 70},
			Range	 {9, 9, 9, 9, 9},
			Cooldown {5,5,5,5,5},
			CastTime {5,5,5,5,5},
			[](Job job)->const auto&
			{
				return search(Job::HighWizard, SkillId::NapalmVulcan);
			}
		};
		table[enum_cast(SkillId::FirstAid)] = SkillDb
		{
			MaxLvl	 {1},
			Sp		 {3},
			Range	 {1},
			Cooldown {5},
			CastTime {5},
			[](Job job)->const auto&
			{
				return search(Job::Novice, SkillId::FirstAid);
			}
		};
		table[enum_cast(SkillId::TrickDead)] = SkillDb
		{
			MaxLvl	 {1},
			Sp		 {5},
			Range	 {1},
			Cooldown {5},
			CastTime {5},
			[](Job job)->const auto&
			{
				return search(Job::Novice, SkillId::TrickDead);
			}
		};
		table[enum_cast(SkillId::BasicSkill)] = SkillDb
		{
			MaxLvl	 {9},
			Sp		 {0, 0, 0, 0, 0, 0, 0, 0, 0},
			Range	 {1, 1, 1, 1, 1, 1, 1, 1, 1},
			Cooldown {5,5,5,5,5,5,5,5,5},
			CastTime {5,5,5,5,5,5,5,5,5},
			[](Job job)->const auto&
			{
				return search(Job::Novice, SkillId::BasicSkill);
			}
		};
		table[enum_cast(SkillId::SwordMastery)] = SkillDb
		{
			MaxLvl	 {10},
			Sp		 {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Range	 {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
			Cooldown {5,5,5,5,5,5,5,5,5,5},
			CastTime {5,5,5,5,5,5,5,5,5,5},
			[](Job job)->const auto&
			{
				return job == Job::SuperNovice ? search(Job::SuperNovice, SkillId::SwordMastery)
											   : search(Job::Swordsman, SkillId::SwordMastery);
			}
		};
		table[enum_cast(SkillId::TwoHandSwordMastery)] = SkillDb
		{
			MaxLvl	 {10},
			Sp		 {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Range	 {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
			Cooldown {5,5,5,5,5,5,5,5,5,5},
			CastTime {5,5,5,5,5,5,5,5,5,5},
			[](Job job)->const auto&
			{
				return search(Job::Swordsman, SkillId::TwoHandSwordMastery);
			}
		};
		table[enum_cast(SkillId::HpRecovery)] = SkillDb
		{
			MaxLvl	 {10},
			Sp		 {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Range	 {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
			Cooldown {5,5,5,5,5,5,5,5,5,5},
			CastTime {5,5,5,5,5,5,5,5,5,5},
			[](Job job)->const auto&
			{
				return job == Job::SuperNovice ? search(Job::SuperNovice, SkillId::HpRecovery)
											   : search(Job::Swordsman, SkillId::HpRecovery);
			}
		};
		table[enum_cast(SkillId::SpRecovery)] = SkillDb
		{
			MaxLvl	 {10},
			Sp		 {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Range	 {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
			Cooldown {5,5,5,5,5,5,5,5,5,5},
			CastTime {5,5,5,5,5,5,5,5,5,5},
			[](Job job)->const auto&
			{
				return job == Job::SuperNovice ? search(Job::SuperNovice, SkillId::SpRecovery)
											   : search(Job::Mage, SkillId::SpRecovery);
			}
		};
		table[enum_cast(SkillId::Endure)] = SkillDb
		{
			MaxLvl	 {10},
			Sp		 {10, 10, 10, 10, 10, 10, 10, 10, 10, 10},
			Range	 {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
			Cooldown {5,5,5,5,5,5,5,5,5,5},
			CastTime {5,5,5,5,5,5,5,5,5,5},
			[](Job job)->const auto&
			{
				return job == Job::SuperNovice ? search(Job::SuperNovice, SkillId::Endure)
											   : search(Job::Swordsman, SkillId::Endure);
			}
		};
		table[enum_cast(SkillId::FatalBlow)] = SkillDb
		{
			MaxLvl	 {1},
			Sp		 {0},
			Range	 {1},
			Cooldown {5},
			CastTime {5},
			[](Job job)->const auto&
			{
				return search(Job::Swordsman, SkillId::FatalBlow);
			}
		};
		table[enum_cast(SkillId::MovingHpRecovery)] = SkillDb
		{
			MaxLvl	 {1},
			Sp		 {0},
			Range	 {1},
			Cooldown {5},
			CastTime {5},
			[](Job job)->const auto&
			{
				return search(Job::Swordsman, SkillId::MovingHpRecovery);
			}
		};
		table[enum_cast(SkillId::AutoBerserk)] = SkillDb
		{
			MaxLvl	 {1},
			Sp		 {1},
			Range	 {1},
			Cooldown {5},
			CastTime {5},
			[](Job job)->const auto&
			{
				return search(Job::Swordsman, SkillId::AutoBerserk);
			}
		};
		table[enum_cast(SkillId::Sight)] = SkillDb
		{
			MaxLvl	 {1},
			Sp		 {10},
			Range	 {1},
			Cooldown {5},
			CastTime {5},
			[](Job job)->const auto&
			{
				return job == Job::SuperNovice ? search(Job::SuperNovice, SkillId::Sight)
											   : search(Job::Mage, SkillId::Sight);
			}
		};
		table[enum_cast(SkillId::EnergyCoat)] = SkillDb
		{
			MaxLvl	 {1},
			Sp		 {30},
			Range	 {1},
			Cooldown {1},
			CastTime {2},
			[](Job job)->const auto&
			{
				return search(Job::Mage, SkillId::EnergyCoat);
			}
		};
		table[enum_cast(SkillId::SightBlaster)] = SkillDb
		{
			MaxLvl	 {1},
			Sp		 {40},
			Range	 {1},
			Cooldown {5},
			CastTime {5},
			[](Job job)->const auto&
			{
				return search(Job::Wizard, SkillId::SightBlaster);
			}
		};
		table[enum_cast(SkillId::FrostNova)] = SkillDb
		{
			MaxLvl	 {10},
			Sp		 {45, 43, 41, 39, 37, 35, 33, 31, 29, 2},
			Range	 {9, 9, 9, 9, 9, 9, 9, 9, 9, 9},
			Cooldown {5,5,5,5,5,5,5,5,5,5},
			CastTime {5,5,5,5,5,5,5,5,5,5},
			[](Job job)->const auto&
			{
				return search(Job::Wizard, SkillId::FrostNova);
			}
		};
		table[enum_cast(SkillId::SoulDrain)] = SkillDb
		{
			MaxLvl	 {10},
			Sp		 {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Range	 {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
			Cooldown {5,5,5,5,5,5,5,5,5,5},
			CastTime {5,5,5,5,5,5,5,5,5,5},
			[](Job job)->const auto&
			{
				return search(Job::HighWizard, SkillId::SoulDrain);
			}
		};
		table[enum_cast(SkillId::MysticAmplification)] = SkillDb
		{
			MaxLvl	 {10},
			Sp		 {14, 18, 22, 26, 30, 34, 38, 42, 46, 50},
			Range	 {1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
			Cooldown {5,5,5,5,5,5,5,5,5,5},
			CastTime {5,5,5,5,5,5,5,5,5,5},
			[](Job job)->const auto&
			{
				return search(Job::HighWizard, SkillId::MysticAmplification);
			}
		};

		return table;
	}
};

struct SkillDb::Private
{
	static constexpr std::array<SkillDb, enum_cast(SkillId::Last) + 1> _skill = SkillDb::initSkillDb();
};

inline constexpr const SkillDb& SkillDb::getSkill(SkillId id)
{
	return Private::_skill[enum_cast(id)];
}
