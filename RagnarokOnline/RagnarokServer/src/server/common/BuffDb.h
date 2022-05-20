#pragma once

#include <server/common/BuffId.h>

#include <sdk/enum_cast.hpp>
#include <sdk/array/ArrayView.hpp>

#include <array>
#include <cstdint>
#include <cassert>
#include <initializer_list>

class BuffDb final
{
public:
	//The more categories we have the faster the lookups of buffs will be due to more spreads
	enum Category : uint8_t
	{
		First = 0,
		Buff = First,

		Debuff,

		Last = Debuff
	};

	static constexpr const BuffDb& getBuff(BuffId id);

	uint32_t getDuration(uint8_t buffLvl) const
	{
		buffLvl--;
		assert(buffLvl < 10); //index starts at 0 and negatives on uints wrap around
		return _duration[buffLvl];
	}

	uint8_t _maxLevel;
	Category _category;

	struct
	{
		uint8_t _dispelsOnDeath : 1,
			_dispellsOnSkill : 1,
			_isStackable : 1,
			_reserved : 5;
	};
private:
	std::array<uint32_t, 10> _duration;

	struct Private;

	template<typename T>
	struct ValueArray
	{
		constexpr ValueArray(std::initializer_list<T> list)
		{
			int i = 0;
			for (const auto& val : list)
				_array[i++] = val;
		}
		constexpr operator std::array<T, 10>() const { return _array; }

		std::array<T, 10> _array = {};
	};

	struct Duration : public ValueArray<uint32_t> {
		constexpr Duration(std::initializer_list<uint32_t> list) : ValueArray(list) {}
	};
	struct MaxLvl
	{
		constexpr explicit MaxLvl(int value) : _value(value) {}
		constexpr operator int() const { return _value; }

		int _value;
	};
	struct Flags
	{
		enum class DispelsOnDeath : bool
		{
			Yes = true,
			No = false
		};

		enum class DispelsOnSkill : bool
		{
			Yes = true,
			No = false
		};

		enum class Stackable : bool
		{
			Yes = true,
			No = false
		};

		constexpr Flags(DispelsOnDeath dispelsOnDeath, DispelsOnSkill dispelsOnSkill, Stackable isStackable)
			:dispelsOnDeath(dispelsOnDeath)
			,dispelsOnSkill(dispelsOnSkill)
			,isStackable(isStackable)
		{}

		DispelsOnDeath dispelsOnDeath;
		DispelsOnSkill dispelsOnSkill;
		Stackable isStackable;
	};

	constexpr BuffDb()
		: _maxLevel(0)
		, _category(Category::First)
		, _duration({})
		, _dispelsOnDeath(false)
		, _dispellsOnSkill(false)
		, _isStackable(false)
		, _reserved(0)
	{}

	constexpr BuffDb(MaxLvl lvl, Category category, Duration duration, Flags flags)
		: _maxLevel(lvl)
		, _category(category)
		, _duration(duration)
		, _dispelsOnDeath(enum_cast(flags.dispelsOnDeath))
		, _dispellsOnSkill(enum_cast(flags.dispelsOnSkill))
		, _isStackable(enum_cast(flags.isStackable))
		, _reserved(0)		
	{}

	//********************* SKILL DATA INITIALIZATION
	static constexpr std::array<BuffDb, enum_cast(BuffId::Last) + 1> initBuffDb()
	{
		std::array<BuffDb, enum_cast(BuffId::Last) + 1> table = {};

		table[enum_cast(BuffId::Sit)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		}; 
		table[enum_cast(BuffId::AspdBuff)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::ExpBuff)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::ItemBuff)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::FoodStr)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::FoodAgi)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::FoodVit)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::FoodDex)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::FoodLuk)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::Blessing)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::IncreaseAgi)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::Angelus)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::Aspersio)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::Gloria)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::KyrieEleison)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::ImpositioManus)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::LexAeterna)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::Magnificat)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::BenedictioSacramentio)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::Assumptio)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::SteelBody)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::CriticalExplosion)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::AttentionConcentrate)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::Falcon)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::TrueSight)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::WindWalk)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::SongHp)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::SongAgi)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::SongMatk)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::SongDef)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::SongMdef)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::SongExp)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::SongReserved1)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::SongReserved2)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::SongReserved3)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::SongReserved4)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::SongReserved5)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::SongReserved6)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::MarionetteControl)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::Cloaking)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::Hiding)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::EnchantPoison)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::EnchantDeadlyPoison)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::EnergyCoat)] = BuffDb
		{
			MaxLvl	 {1},
			Category {Category::Buff},
			Duration {15 * 1000}, //15s
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::Yes, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::EndowEarth)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::EndowFire)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::EndowWater)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::EndowWind)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::Endure)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::Provoke)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::Peco)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::TwoHandQuicken)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::TensionRelax)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::AutoGuard)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::ReflectShield)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::Defender)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::Providence)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::LoudExclamation)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::AdrenalineRush)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::MaximizePower)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::OverThrust)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::WeaponPerfection)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::CartBoost)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::ChemicalArmor)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::ChemicalHelm)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::ChemicalShield)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::ChemicalWeapon)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::Kaahi)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
			Flags{ Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No }
		};
		table[enum_cast(BuffId::Kaizel)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::Kaupe)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::First},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::Weight50)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::Debuff},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::Weight90)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::Debuff},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::Quagmire)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::Debuff},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::Bleeding)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::Debuff},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::CriticalWounds)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::Debuff},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::DecreaseAgi)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::Debuff},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::SlowCast)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::Debuff},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::DropArmor)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::Debuff},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::DropHelm)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::Debuff},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::DropShield)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::Debuff},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::DropWeapon)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::Debuff},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};
		table[enum_cast(BuffId::HellsPower)] = BuffDb
		{
			MaxLvl	 {10},
			Category {Category::Debuff},
			Duration {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
			Flags	 { Flags::DispelsOnDeath::Yes, Flags::DispelsOnSkill::No, Flags::Stackable::No}
		};			
		return table;
	}
};

struct BuffDb::Private
{
	static constexpr std::array<BuffDb, enum_cast(BuffId::Last) + 1> _buff = BuffDb::initBuffDb();
};

inline constexpr const BuffDb& BuffDb::getBuff(BuffId id)
{
	return Private::_buff[enum_cast(id)];
}
