#pragma once

#include <server/map_objects/Block.h>

#include <cassert>

class Block;

template<typename T>
class DynamicBlockArray
{
public:
	DynamicBlockArray(BlockArray& blockArray)
		:_blockArray(blockArray)
	{}

	T& unsafeGet(int16_t index)
	{
		auto& block = _blockArray.unsafeGet(index);
		assert(block._type == typename T::BLOCK_TYPE);

		return reinterpret_cast<T&>(block);
	}

	const T& unsafeGet(int16_t index) const
	{
		const auto& block = _blockArray.unsafeGet(index);
		assert(block._type == typename T::BLOCK_TYPE);

		return reinterpret_cast<const T&>(block);
	}

	void insert(T& t)
	{
		_blockArray.insert(&t);
	}

	void release(T& t)
	{
		assert(t._id.isValid());
		_blockArray.release(t._id._index);
	}
private:
	BlockArray& _blockArray;
};