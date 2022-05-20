#pragma once

#include <server/map_objects/BlockId.h>
#include <server/map_objects/BlockType.h>
#include <server/map/BlockGridIntrusive.h>

#include <cstdint>
#include <cassert>

class Block
	: public BlockGridIntrusive
{
public:
	typedef BlockType Type;
	typedef BlockId Id;

protected:
	explicit Block(Type type)
		:_type(type)
	{
		assert(type != Type::Invalid);
	}

	~Block() = default;
private:
	friend class BlockArray;

	// to update the id after construction, but before it hits the BlockArray 
	void updateId(int16_t index)
	{
		const_cast<Id&>(_id) = Id(index);
	}
public:
	const Id _id;
	Point _position = { 0, 0 };
	const Type _type;
};