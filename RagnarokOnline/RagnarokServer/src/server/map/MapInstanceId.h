#pragma once

#include <server/common/MapId.h>

#include <cstdint>

class MapInstanceId final
{
public:
	bool empty() const
	{
		return _authToken == 0;
	}

	MapInstanceId()
		:_authToken(0)
		,_instanceIndex(0)
	{}

private:
	friend class MapManager;

	MapInstanceId(uint16_t token, uint16_t index)
		:_authToken(token)
		,_instanceIndex(index)
	{}

	uint16_t _authToken;
	MapInstanceIndex _instanceIndex;
};