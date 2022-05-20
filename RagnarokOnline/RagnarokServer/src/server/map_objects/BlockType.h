#pragma once

#include <cstdint>

enum class BlockType : uint8_t
{
	Character,
	Homunculus,
	Mercenary,
	Monster,

	LastUnit = Monster,

	Npc,
	Skill,
	Pet,
	Item,

	Invalid = 255
};

static inline bool operator<=(BlockType l, BlockType r)
{
	return static_cast<uint8_t>(l) <= static_cast<uint8_t>(r);
}