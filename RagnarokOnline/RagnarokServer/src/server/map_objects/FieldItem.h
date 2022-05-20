#pragma once

#include <server/common/Item.h>
#include <server/map_objects/Block.h>

#include <cstdint>

class FieldItem final
	: public Block
{
public:
	FieldItem(ItemDbId id, uint16_t amount)
		: Block(BlockType::Item)
		, _itemData(id, amount)
	{
	}

	Item _itemData;

private:
	friend class ItemController;
};