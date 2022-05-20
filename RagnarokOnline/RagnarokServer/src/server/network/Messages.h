#pragma once

#include <server/network/MessageBase.h>
#include <server/network/MessageId.h>
#include <server/common/CommonTypes.h>

namespace msg
{
	#define MESSAGE_ID(x) Id::x; static_assert(x::ID == Id::x)

	struct Chat : public Message
	{
		static constexpr Id ID = MESSAGE_ID(Chat);

		Chat(int size) : Message(ID, size) {}

		ChatType type;
		uint8_t msg[];
	};

	struct GuildRecall : public Message
	{
		static constexpr Id ID = MESSAGE_ID(GuildRecall);

		GuildRecall(int size) : Message(ID, size) {}
	};

	struct PartyInvite : public Message
	{
		static constexpr Id ID = MESSAGE_ID(PartyInvite);

		PartyInvite(int size) : Message(ID, size) {}
	};

	struct PartyKick : public Message
	{
		static constexpr Id ID = MESSAGE_ID(PartyKick);

		PartyKick(int size) : Message(ID, size) {}
	};

	struct GuildInvite : public Message
	{
		static constexpr Id ID = MESSAGE_ID(GuildInvite);

		GuildInvite(int size) : Message(ID, size) {}
	};

	struct GuildKick : public Message
	{
		static constexpr Id ID = MESSAGE_ID(GuildKick);

		GuildKick(int size) : Message(ID, size) {}
	};

	#undef MESSAGE_ID
}