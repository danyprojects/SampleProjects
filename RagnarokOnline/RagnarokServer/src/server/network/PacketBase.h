#pragma once

#include <server/network/PacketId.h>

#include <type_traits>
#include <cstdint>

namespace packet
{
#pragma pack(push, 1)

	struct Packet
	{
		explicit Packet(Id id) :_id(id) {}

		Id _id;
	};

	// Just placeholder so we can do compile time checks on packet
	struct SND_FixedSizePacket : public Packet
	{
		explicit SND_FixedSizePacket(Id id)
			:Packet(id)
		{}
	};

	// If it's dynamic packet it shoud extend this
	struct SND_DynamicSizePacket : public Packet
	{
		SND_DynamicSizePacket(Id id)
			:Packet(id)
		{}

		uint16_t _totalSize;
	};

	// Just placeholder so we can do compile time checks on packet
	struct RCV_FixedSizePacket : public Packet
	{
		explicit RCV_FixedSizePacket(Id id)
			:Packet(id)
		{}
	};

	// If it's dynamic packet it shoud extend this and
	// packet must provide a static constexpr uint16_t MAX_SIZE
	struct RCV_DynamicSizePacket : public Packet
	{
		RCV_DynamicSizePacket(Id id)
			:Packet(id)
		{}

		// Packet must provide static constexpr uint16_t MAX_SIZE

		uint16_t _totalSize;
	};
#pragma pack(pop)
}