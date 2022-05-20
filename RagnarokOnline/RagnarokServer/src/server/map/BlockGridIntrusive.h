#pragma once

#include <server/common/Point.h>

class Block;

class BlockGridIntrusive
{
public:
	PointByte getGridPosition() const { return _gridPosition; }
protected:
	~BlockGridIntrusive() = default;
private:

	friend class BlockGrid;

	Block* _nextBlock = nullptr;
	Block* _previousBlock = nullptr;
	PointByte _gridPosition = { -1, -1 };
};