#pragma once

#include <server/map/BlockArray.h>
#include <server/map_objects/Block.h>

#include <sdk/array/FixedSizeArray.hpp>

#include <cassert>
#include <type_traits>

class Block;

template<typename T>
class FixedBlockArray
{
public:
	static_assert(std::is_base_of<Block, T>::value);

	typedef typename FixedSizeArray<T>::iterator iterator;
	typedef typename FixedSizeArray<T>::const_iterator const_iterator;

	FixedBlockArray(BlockArray& blockArray, typename FixedSizeArray<T>::Initializer initializer)
		:_blockArray(blockArray)
		,_blocks(std::move(initializer))
	{

		for (uint16_t i = 0; i < static_cast<uint16_t>(_blocks.size()); i++)
			blockArray.insert(&_blocks[i]);
	}

	iterator begin() { return _blocks.begin(); }
	const_iterator begin() const { return _blocks.begin(); }

	iterator end() { return _blocks.end(); }
	const_iterator end() const { return _blocks.end(); }

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

	FixedBlockArray(const FixedBlockArray&) = delete;
	FixedBlockArray& operator=(const FixedBlockArray&) = delete;
private:
	FixedSizeArray<T> _blocks;
	BlockArray& _blockArray;
};