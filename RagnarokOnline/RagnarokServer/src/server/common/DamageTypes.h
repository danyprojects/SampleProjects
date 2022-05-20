#pragma once

#include <cstdint>

struct DmgFlag
{
	enum Value : uint8_t
	{
		None = 0,
		IgnoreDef = 1 << 0,
		RangedAttack = 1 << 1,
		CriticalAttack = 1 << 2,
		IsAoe = 1 << 3,
	};

	DmgFlag(Value value) : _value(value) {}
	bool operator &(Value value) const { return _value & value; }
	auto operator |=(Value value) { return _value|= value; }
private:
	uint8_t _value;
};

struct DmgMatkRate
{
	explicit DmgMatkRate(int value) : _value(value) {}
	operator int() const { return _value; }
private:
	int _value;
};

struct DmgAtkRate
{
	explicit DmgAtkRate(int value) : _value(value) {}
	operator int() const { return _value; }
private:
	int _value;
};

struct DmgAtkFlat
{
	explicit DmgAtkFlat(int value) : _value(value) {}
	operator int() const { return _value; }
private:
	int _value;
};

struct DmgRaw
{
	explicit DmgRaw(int value) :_value(value) {}
	operator int() const { return _value; }
private:
	int _value;
};

struct DmgInitial
{
	operator int() const { return _value; }
	DmgInitial operator *=(int value) { return _value *= value; }
	DmgInitial operator /=(int value) { return _value /= value; }
	DmgInitial operator +=(DmgInitial initial) { return _value += initial; }
private:
	friend class BattleController;

	DmgInitial() : _value(0) {}
	DmgInitial(int value) : _value(value) {}

	int _value;
};
