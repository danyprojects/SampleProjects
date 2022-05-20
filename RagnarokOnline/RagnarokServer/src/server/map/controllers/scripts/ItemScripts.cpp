#include "ItemScripts.h"

#include <server/common/Item.h>

#include <sdk/enum_cast.hpp>

#include <array>


void ItemScripts::useItem(const Item& item, Character& src, OperationType useType)
{
	using ArrayType = std::array<ItemScripts::ItemUseHandler, enum_cast(ItemDbId::Last) + 1>;

	static constexpr ArrayType _table = []() constexpr
	{
		ArrayType table = {};

		for (int i = 0; i <= enum_cast(ItemDbId::Last); i++)
			table[i] = &ItemScripts::defaultOnUse;

		//Construct table here

		return table;
	}();

	static_assert([]() constexpr
		{
#ifdef NDEBUG
			for (auto&& v : _table)
				if (v == nullptr)
					return false;
#endif
			return true;
		}());

	(this->*_table[enum_cast(item._dbId)])(item, src, useType);
}

void ItemScripts::unequipItem(const Item& item, Character& src, OperationType useType)
{
	using ArrayType = std::array<ItemScripts::ItemUseHandler, enum_cast(ItemDbId::Last) + 1>;

	static constexpr ArrayType _table = []() constexpr
	{
		ArrayType table = {};

		for (int i = 0; i <= enum_cast(ItemDbId::Last); i++)
			table[i] = &ItemScripts::defaultOnDeEquip;

		//Construct table here

		return table;
	}();

	static_assert([]() constexpr
		{
#ifdef NDEBUG
			for (auto&& v : _table)
				if (v == nullptr)
					return false;
#endif
			return true;
		}());

	(this->*_table[enum_cast(item._dbId)])(item, src, useType);
}

//*************** Function definitions for item handlers
void ItemScripts::dummy(const Item& item, Character& src, OperationType useType)
{

}
