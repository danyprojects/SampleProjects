#pragma once

#include <cstdint>

class Tile final
{
public:
	struct 
	{
		uint8_t walkable : 1,
				snipable : 1,
				water : 1,
				noVending : 1,
				noIcewall : 1,
				basilica : 1,
				landProtector : 1,
				icewall : 1;
	};	

	bool hasLandProtectorOrBasilica() const { return landProtector | basilica; }

	bool isSeeThrough() const { return walkable | snipable; }

	bool isWalkable() const { return walkable & ~icewall; }
};