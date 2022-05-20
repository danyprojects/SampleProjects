#pragma once

#include <cstdint>

struct EquipMask final
{
	enum Value : uint16_t
	{
		None = 0,
		HeadLower = 1 << 0,
		Weapon = 1 << 1,
		Garment = 1 << 2,
		AccessoryLeft = 1 << 3,
		Armor = 1 << 4,
		Shield = 1 << 5,
		Shoes = 1 << 6,
		AccessoryRight = 1 << 7,
		HeadUpper = 1 << 8,
		HeadMiddle = 1 << 9,

		Ammunition = 1 << 10,

		Accessory = AccessoryLeft | AccessoryRight,
		TwoHanded = Weapon | Shield,
		HeadMiddleLower = HeadLower | HeadMiddle,
		HeadUpperMiddle = HeadUpper | HeadMiddle,
		HeadUpperMiddleLower = HeadUpper | HeadMiddle | HeadLower,

		Visible = HeadUpperMiddleLower | TwoHanded
	};

	constexpr uint16_t getValue() const
	{
		return _value;
	}

	constexpr Value operator&(Value values) const
	{
		return static_cast<Value>(_value & values);
	}

	constexpr bool operator!=(const EquipMask mask) const
	{
		return _value != mask._value;
	}

	constexpr bool operator==(const EquipMask mask) const
	{
		return _value == mask._value;
	}

	constexpr bool operator!=(const Value value) const
	{
		return _value != value;
	}

	constexpr bool operator==(const Value value) const
	{
		return _value == value;
	}

	constexpr EquipMask& operator=(const Value value)
	{
		_value = value;
		return *this;
	}

	constexpr explicit EquipMask(Value value)
		:_value(value)
	{}

	constexpr EquipMask() : _value(Value::None) {}
private:
	union
	{
		struct
		{
			bool
				_headLow : 1,
				_handRight : 1,
				_garment : 1,
				_accessoryLeft : 1,
				_armor : 1,
				_handLeft : 1,
				_shoes : 1,
				_accessoryRight : 1,
				_headTop : 1,
				_headMid : 1;
		};
		uint16_t _value;
	};
};

inline EquipMask::Value operator|(EquipMask::Value left, EquipMask::Value right)
{
	return left | right;
}
