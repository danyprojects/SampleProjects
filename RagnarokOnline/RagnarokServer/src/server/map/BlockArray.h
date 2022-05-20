#pragma once

#include <server/common/ServerLimits.h>
#include <server/map_objects/Block.h>

#include <array>
#include <cassert>

class Character;

class BlockArray final
{
public:
	BlockArray()
	{
		Node* node = reinterpret_cast<Node*>(_array.data());
		_head = node;

		for (size_t i = 0; i < _array.size() - 1; i++)
		{
			node = node->_next = node + 1;
		}

		_tail = node;
	}

	// use this when you are sure the entry will be there
	Block& unsafeGet(int16_t index)
	{
		assert(index && static_cast<uint16_t>(index) < _array.size());

		Block* dataPtr = reinterpret_cast<Block*>(&_array);
		auto* ptr = _array[index];

		assert(ptr && (ptr < dataPtr || ptr >= dataPtr + _array.size()));
		assert(ptr->_id._index == index);

		return  *_array[index];
	}

	Block* get(BlockId id)
	{
		if (!id.isValid())
			return nullptr;

		auto* ptr = _array[id._index];

		// check if ptr is being used to store free nodes
		Block* dataPtr = reinterpret_cast<Block*>(&_array);
		if (ptr == nullptr || (ptr >= dataPtr && ptr < dataPtr + _array.size()))
			return nullptr;

		return ptr->_id == id ? ptr : nullptr;
	}
private:
	template<typename T> friend class FixedBlockArray;
	template<typename T> friend class DynamicBlockArray;

	struct Node
	{
		Node* _next;
	};

	void release(int16_t index)
	{
		Node* node = reinterpret_cast<Node*>(_array.data() + index);
		node->_next = nullptr;

		if (_tail != nullptr)
			_tail->_next = node;
		else
			_head = node;
		_tail = node;
	}

	void insert(Block* ptr)
	{
		auto* old = _head;
		assert(old); //assuming we never run out of memory
		_head = old->_next;

		Block** blockPtr = reinterpret_cast<Block**>(old);

		*blockPtr = ptr;
		ptr->updateId(static_cast<int16_t>(blockPtr - _array.data()));
	}

	Node* _head;
	Node* _tail;
	std::array<Block*, BLOCK_ARRAY_MAX_ENTRIES> _array;
};