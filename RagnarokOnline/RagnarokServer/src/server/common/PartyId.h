#pragma once

#include <sdk/uid/Id.h>

class PartyId final
	: public uid::Id
{
public:
	constexpr PartyId() = default;
	constexpr PartyId(const PartyId&) = default;
	constexpr PartyId& operator=(const PartyId&) = default;
private:
	friend class Party;
	explicit constexpr PartyId(uid::Id value)
		:Id(value)
	{}
};