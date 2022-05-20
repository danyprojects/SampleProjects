#pragma once

#include <server/network/MessageId.h>

namespace msg
{
	struct Message
	{
		Message(Id id, int size)
			:_id(id)
			,_size(size)
		{}

		const uint16_t _size;
		const Id _id;
	};
}