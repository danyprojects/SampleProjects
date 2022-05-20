#pragma once

#include <server/common/ServerLimits.h>

class BlockId
{
public:
	BlockId() = default;

	constexpr explicit BlockId(int16_t index)
		:_index(index)
	{}

	static constexpr BlockId LOCAL_SESSION_ID()
	{
		return BlockId{ INT16_MAX };
	}
		
	bool isValid() const
	{
		return _index >= 0 && _index < BLOCK_ARRAY_MAX_ENTRIES;
	};
		
	bool operator==(BlockId id) const
	{
		return id._index == _index;
	}
		
	bool operator!=(BlockId id) const
	{
		return id._index != _index;
	}

	operator int16_t() const 
	{
		return _index;
	}

	int16_t _index = -1;
};