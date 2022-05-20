#pragma once

#include <server/common/Buff.h>
#include <server/common/Direction.h>
#include <server/common/CommonTypes.h>
#include <server/common/ServerLimits.h>
#include <server/map_objects/Block.h>

#include <sdk/Tick.h>
#include <sdk/enum_cast.hpp>
#include <sdk/array/ArrayView.hpp>

#include <array>
#include <bitset>
#include <cstdint>
#include <type_traits>

class Timer;
class Skill;
class FieldSkill;
	
class Unit
	: public Block
{
public:
	typedef std::aligned_storage<sizeof(Buff), alignof(Buff)>::type RawBuff;

	struct StatusChange final
	{
	public:
		StatusChange(ArrayView<RawBuff> buffSlots)
			:_smallestTick(0)
			, _nextFreeBuff(0)
			, _buffCount(0)
			, _lastInsertedTick(0)
			, _categoryLookups({})
			, _buffSlots({ reinterpret_cast<Buff*>(buffSlots.data()), buffSlots.size() })
		{
			// Buff can't need a destructor otherwise we need to call it explicitly
			static_assert(std::is_trivially_destructible<Buff>::value);

			//Initialize the stack of buffs. Init the rest to invalid
			for (int i = 0; i < _buffSlots.size(); i++)
			{
				auto* ptr = &_buffSlots[i];
				new ((void*)ptr) Buff(); // call contructor explicitly
				ptr->nextBuff = i + 1;
			}
			_buffSlots[_buffSlots.size() - 1].nextBuff = INVALID_INDEX;

			//Init all the category lookups to start as invalid
			_categoryLookups.fill(INVALID_INDEX);
		}

		bool contains(BuffId id) const { return buffs[enum_cast(id)]; }

		//bitset to lookup if a certain buff or debuff is active
		std::bitset<enum_cast(BuffId::Last) + 1> buffs;

		//Add other skill / buff specific variables here
		Tick nextStormGustHit;

	private:
		//This section is meant to be used only from buff controller
		friend class BuffController;
		static constexpr auto INVALID_INDEX = Buff::INVALID_INDEX;

		uint8_t _nextFreeBuff;
		uint8_t _smallestTick;
		uint8_t _buffCount;
		uint8_t _lastInsertedTick; //For optimizing tick insertion
		ArrayView<Buff> _buffSlots;
		std::array<uint8_t, BuffDb::Category::Last + 1> _categoryLookups; // contains only the first element in the category
	};

	struct MovementData
	{
		std::array<Point, MAX_PATH_LENGTH> path;
		Tick nextMoveTick;
		int8_t pathLength;
		int8_t moveIndex;
	};

	struct CastData //no point on union fiels here since we gonna get padding
	{
		Tick deadline;
		Skill* skill;
		uint8_t skillLevel;
		struct
		{
			uint8_t	canCastCancel : 1,
				canSpellBreak : 1,
				canInterrupt : 1,
				canMoveInCast : 1,
				reserved : 4;
		};
	};

	struct TargetData
	{
		Point targetPosition;
		BlockId targetBlock; //Don't make this a union with targetposition. It's useful to have both during move to target
	};

	bool isDead()
	{
		return _hp == 0;
	}

	bool isUndead() const
	{
		return _defElement == Element::Undead || _race == Race::Undead;
	}

	auto getStr() const { return (_strBase * (100 + _strBuffRate) / 100 + _strBonus) * (100 - _strDebuffRate) / 100; }
	auto getAgi() const { return (_agiBase * (100 + _agiBuffRate) / 100 + _agiBonus) * (100 - _agiDebuffRate) / 100; }
	auto getVit() const { return (_vitBase * (100 + _vitBuffRate) / 100 + _vitBonus) * (100 - _vitDebuffRate) / 100; }
	auto getDex() const { return (_dexBase * (100 + _dexBuffRate) / 100 + _dexBonus) * (100 - _dexDebuffRate) / 100; }
	auto getInt() const { return (_intBase * (100 + _intBuffRate) / 100 + _intBonus) * (100 - _intDebuffRate) / 100; }
	auto getLuk() const { return (_lukBase * (100 + _lukBuffRate) / 100 + _lukBonus) * (100 - _lukDebuffRate) / 100; }

	auto getMaxHp() const { return _maxHpFlat * (100 + _maxHpRate) / 100; }
	auto getMaxSp() const { return _maxSpFlat * (100 + _maxSpRate) / 100; }

	auto getDef() const { return _defFlat * (100 + _defRate) / 100; }
	auto getDef2() const { return (getVit() + _def2Flat) * (100 + _def2Rate) / 100; }

	auto getMdef() const { return _mdefFlat * (100 + _mdefRate) / 100; }
	auto getMdef2() const { return (getInt() + _mdef2Flat + getVit() / 2) * (100 + _mdef2Rate) / 100; }

	auto getHit() const { return (getDex() + _lvl + _hitFlat) * (100 + _hitRate) / 100; }
	auto getFlee() const { return (getAgi() + _lvl + _fleeFlat) * (100 + _fleeRate) / 100; }

	auto getFlee2() const { return (getLuk() + _flee2Flat + (_type == BlockType::Character ? 10 : 1)) * (100 + _flee2Rate) / 100; }
	auto getCrit() const { return (getLuk()*3 + _critFlat + (_type == BlockType::Character ? 10 : 1)) * (100 + _critRate) / 100;}

	auto getBaseAtk(bool dexBased = false) const
	{
		auto str = getStr();
		
		if (_type == BlockType::Character)
		{
			auto dex = getDex();
			if (dexBased)
				std::swap(dex, str);

			return str + (str/10) * (str/10) + dex/5 + getLuk()/5 + _atkFlat;
		}
		else
			return str + (str/10) * (str/10) + _atkFlat;
	}

	auto getMinMatk() const 
	{
		auto int_ = getInt();
		return (100 + _matkRate) * (int_ + (int_ / 7) * (int_ / 7) + _matkFlat) / 100;
	}

	auto getMaxMatk() const
	{
		auto int_ = getInt();
		return (100 + _matkRate) * (int_ + (int_ / 5) * (int_ / 5) + _matkFlat) / 100;
	}

	auto getLvl() const { return _lvl; }

	Unit(BlockType type, ArrayView<RawBuff> buffSlots)
		:Block(type)
		,_statusChange(buffSlots)
		,_moveAction(MoveAction::None)
	{}
protected:
	~Unit() = default;
	uint8_t _lvl;
public:
	uint8_t _strBase, _strBonus = 0;
	uint16_t _strBuffRate = 0, _strDebuffRate = 0;
	uint8_t _agiBase, _agiBonus = 0;
	uint16_t _agiBuffRate = 0, _agiDebuffRate = 0;
	uint8_t _vitBase, _vitBonus = 0;
	uint16_t _vitBuffRate = 0, _vitDebuffRate = 0;
	uint8_t _intBase, _intBonus = 0;
	uint16_t _intBuffRate = 0, _intDebuffRate = 0;
	uint8_t _dexBase, _dexBonus = 0;
	uint16_t _dexBuffRate = 0, _dexDebuffRate = 0;
	uint8_t _lukBase, _lukBonus = 0;
	uint16_t _lukBuffRate = 0, _lukDebuffRate = 0;
	uint32_t _hp, _sp;
	int32_t _maxHpFlat, _maxSpFlat;
	int16_t _maxHpRate = 0, _maxSpRate = 0;
	int16_t _atkFlat = 0, _atkRate = 0;
	int16_t _matkFlat = 0, _matkRate = 0;
	int16_t _defFlat = 0, _defRate = 0;
	int16_t _def2Flat = 0, _def2Rate = 0;
	int16_t _mdefFlat = 0, _mdefRate = 0;
	int16_t _mdef2Flat = 0, _mdef2Rate = 0;
	int16_t _hitFlat = 0, _hitRate = 0;
	int16_t _critFlat = 0, _critRate = 0;
	int16_t _fleeFlat = 0, _fleeRate = 0;
	int16_t _flee2Flat = 0, _flee2Rate = 0;
	uint8_t _range;
	uint16_t _animationDelay;
	uint16_t _atkSpd;
	uint16_t _walkSpd;
	Element _defElement;
	ElementLvl _defElementLvl;
	Element _atkElement = Element::None;
	Size _size;
	Race _race;
	StatusChange _statusChange;
	Direction _direction = Direction::Up;
	CastData _castData;
	TargetData _targetData;
	MovementData _movementData;
	Tick _animDelayEnd;  //Holds the tick of when animation delay will end
	MoveAction _moveAction;
	uint8_t _resistancePierce[enum_cast(Element::Last) + 1] = {};
	struct
	{
		bool _isMoving : 1;
		bool _isCasting : 1;
		bool _isAutoAttacking : 1;
		uint8_t	_reserved : 5;
	};
	Timer* _actionTimer = nullptr;
	Timer* _movementTimer = nullptr;
	FieldSkill* _activeFieldSkill = nullptr;
};