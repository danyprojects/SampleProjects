#pragma once

#include <sdk/uid/Id.h>

class CharacterId final
	: public uid::Id
{
public:
	constexpr CharacterId() = default;
	constexpr CharacterId(const CharacterId&) = default;
	constexpr CharacterId& operator=(const CharacterId&) = default;
private:
	friend class Character;
	friend class CharacterInfo;

	explicit constexpr CharacterId(uid::Id value)
		:Id(value)
	{}
};