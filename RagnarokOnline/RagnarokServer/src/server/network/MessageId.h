#pragma once

#ifndef UNIT_TEST_PACKET_ID

#include <cstdint>
#endif

namespace msg
{
	enum class Id : uint8_t
	{
		Chat,
		GuildRecall,
		PartyInvite, // we may want then together or separate we will se later
		PartyKick,
		GuildInvite,
		GuildKick,
		Last = GuildKick
	};
}