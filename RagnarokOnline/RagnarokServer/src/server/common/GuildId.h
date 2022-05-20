#pragma once

#include <sdk/uid/Id.h>

class GuildId final
	: public uid::Id
{
public:
	constexpr GuildId() = default;
	constexpr GuildId(const GuildId&) = default;
	constexpr GuildId& operator=(const GuildId&) = default;
private:
	friend class Guild;
	explicit constexpr GuildId(uid::Id value)
		:Id(value)
	{}
};