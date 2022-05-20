#pragma once

#include <server/common/Direction.h>
#include <server/map_objects/Block.h>
#include <server/map/pathfinding/Pathfinder.h>

#include <sdk/array/FixedSizeArray2D.hpp>

#include <cassert>
#include <tuple>

class BlockGrid
{
public:
	BlockGrid(int width, int height)
		:_grid(width, height)
	{
		std::memset(_grid.data(), 0, sizeof(Block*) * _grid.height() * _grid.width());
	}

	void push(Block& block)
	{
		assert(block._gridPosition == INVALID_POSITION);

		auto position = calculateGridPosition(block._position);
		auto& old = _grid(position.x, position.y);

		block._nextBlock = old;
		block._previousBlock = nullptr;

		if (block._nextBlock != nullptr)
			block._nextBlock->_previousBlock = &block;

		old = &block;
		block._gridPosition = position;
	}

	void remove(Block& block)
	{
		assert(block._gridPosition != INVALID_POSITION);

		if (auto previous = block._previousBlock; previous != nullptr)
		{
			previous->_nextBlock = block._nextBlock;
			if (block._nextBlock)
				block._nextBlock->_previousBlock = previous;
		}
		else
		{
			if(block._nextBlock)
				block._nextBlock->_previousBlock = nullptr;

			_grid(block._gridPosition.x, block._gridPosition.y) = block._nextBlock;
		}

		block._gridPosition = INVALID_POSITION;
	}

	void updateBlock(Block& block)
	{
		auto position = calculateGridPosition(block._position);
		//No change in block position
		if (block._gridPosition == position)
			return;

		remove(block); 
		push(block);
	}

	static PointByte calculateGridPosition(Point position)
	{
		return { static_cast<int8_t>(position.x / MAP_BLOCK_WIDTH),
					static_cast<int8_t>(position.y / MAP_BLOCK_HEIGHT) };
	}

	//Action is void(Block& block)
	template<typename Action>
	void runActionInRange(const Block& centerBlock, int range, Action action)
	{
		int gridRange = rangeToTotalGridSquares(range);
		//Run all the horizontals for each y
		for (int y = -gridRange; y <= gridRange; y++)
			runActionHorizontalInRange(centerBlock, range, y, gridRange, action);			
	}

	//Action is void(Block& block)
	template<typename Action>
	void runActionInRange(Point position, int range, Action action)
	{
		int gridRange = rangeToTotalGridSquares(range);
		PointByte centerGridPosition = calculateGridPosition(position);
		//Run all the horizontals for each y
		for (int y = -gridRange; y <= gridRange; y++)
			runActionHorizontalInRange(centerGridPosition, position, range, y, gridRange, action);
	}

	//For the time being knockback will use the one bellow 3 times. For optmization this one should be coded and used though
	/*
	template<typename Action>
	void runActionInRangeBorder(const Block& centerBlock, int minRange, int maxRange, Direction direction, Action action)
	{
		int minGridRange = rangeToTotalGridSquares(minRange);
		int maxGridRange = rangeToTotalGridSquares(maxRange);

		//TODO:
	}*/

	template<typename Action>
	void runActionInRangeBorder(const Block& centerBlock, int range, Direction direction, Action action)
	{
		int gridRange = rangeToTotalGridSquares(range);

		Point axis = direction.getIncrements();

		//Is diagonal direction
		if (axis.x != 0 && axis.y != 0)
		{
			int gridX = std::abs((centerBlock._position.x + range * axis.x) / MAP_BLOCK_WIDTH - centerBlock._gridPosition.x);
			int gridY = std::abs((centerBlock._position.y + range * axis.y) / MAP_BLOCK_WIDTH - centerBlock._gridPosition.y);

			//Run the whole row with diagonal corner awareness
			runActionHorizontalAtRangeWithDiagonalCorners(centerBlock, range, gridY * axis.y, gridRange, gridX, axis.x, action);

			//Now run vertical but ignore the row that we did the horizontal at
			if (axis.y == -1) //If we processed the row from below in horizontal
			{
				int minGridY = gridRange != gridY ? -gridY : -gridRange;
				runActionVerticalAtRange(centerBlock, range, gridX * axis.x, minGridY + 1, gridRange, action);
			}
			else //If we processed the row from above in horizontal
			{
				int maxGridY = gridRange != gridY ? gridY : gridRange;
				runActionVerticalAtRange(centerBlock, range, gridX * axis.x, -gridRange, maxGridY - 1, action);
			}
		}
		else //direction is just 1 axis
		{
			if (axis.x != 0) // is along x axis so run blocks from down to up
			{
				int gridX = std::abs((centerBlock._position.x + range * axis.x) / MAP_BLOCK_WIDTH - centerBlock._gridPosition.x);
				runActionVerticalAtRange(centerBlock, range, gridX * axis.x, -gridRange, gridRange, action);
			}
			else //is along y axis
			{
				int gridY = std::abs((centerBlock._position.y + range * axis.y) / MAP_BLOCK_WIDTH - centerBlock._gridPosition.y);
				runActionHorizontalAtRange(centerBlock, range, gridY * axis.y, -gridRange, gridRange, action);
			}
		}
	}

private:
	int rangeToTotalGridSquares(int range) const
	{
		return range / MAP_BLOCK_WIDTH + 1;
	}

	template<typename Action>
	void runActionHorizontalAtRange(const Block& centerBlock, int range, int gridY, int minGridX, int maxGridX, Action action)
	{
		//Check edges
		PointByte position;
		position.y = centerBlock._gridPosition.y + gridY;
		if (position.y < 0 || position.y >= _grid.height())
			return;

		for (int x = minGridX; x <= maxGridX; x++)
		{
			position.x = centerBlock._gridPosition.x + x;

			//Check edges
			if (position.x < 0)
				continue;
			else if (position.x >= _grid.width())
				break;				
			for (Block* block = _grid(position.x, position.y); block != nullptr; block = block->_nextBlock)
			{
				//Only needs to check for Y equality only since that is the one that will change change. X just ensures it's still within range
				if (block != &centerBlock && std::abs(centerBlock._position.y - block->_position.y) == range &&
					std::abs(centerBlock._position.x - block->_position.x) <= range)
				{
					action(*block);
				}
			}				
		}
	}

	template<typename Action>
	void runActionHorizontalAtRangeWithDiagonalCorners(const Block& centerBlock, int range, int gridY, int gridRange, int gridX, int axisX, Action action)
	{
		//Check edges
		PointByte position;
		position.y = centerBlock._gridPosition.y + gridY;

		if (position.y < 0 || position.y >= _grid.height())
			return;

		int minGridX;
		int maxGridX;
		//If diagonal is to the right, then do diagonal awareness on on right corner, otherwise do it on left corner
		if (axisX == 1)
		{
			position.x = centerBlock._gridPosition.x + static_cast<int8_t>(gridX);
			minGridX = -gridRange;
			maxGridX = gridX - 1;
		}
		else
		{
			position.x = centerBlock._gridPosition.x - static_cast<int8_t>(gridX);
			minGridX = -gridX + 1;
			maxGridX = gridRange;
		}

		if (position.x >= 0 && position.x < _grid.width())
		{
			for (Block* block = _grid(position.x, position.y); block != nullptr; block = block->_nextBlock)
			{
				//Need to check for both X and Y since this is a diagonal borner check
				if (block != &centerBlock &&
					((std::abs(centerBlock._position.y - block->_position.y) == range && std::abs(centerBlock._position.x - block->_position.x) <= range) ||
						(std::abs(centerBlock._position.x - block->_position.x) == range && std::abs(centerBlock._position.y - block->_position.y) <= range)))
				{
					action(*block);
				}
			}
		}

		//depending on the corner we do the diagonal in, we want to start ahead or end early
		for (int x = minGridX; x <= maxGridX; x++)
		{
			position.x = centerBlock._gridPosition.x + x;

			//Check edges
			if (position.x < 0)
				continue;
			else if (position.x >= _grid.width())
				break;
			for (Block* block = _grid(position.x, position.y); block != nullptr; block = block->_nextBlock)
			{
				//Only needs to check for Y equality only since that is the one that will change change. X just ensures it's still within range
				if (block != &centerBlock && std::abs(centerBlock._position.y - block->_position.y) == range &&
					std::abs(centerBlock._position.x - block->_position.x) <= range)
				{
					action(*block);
				}
			}
		}
	}

	template<typename Action>
	void runActionHorizontalBetweenRange(const Block& centerBlock, int minRange, int maxRange, int gridY, int maxGridX, Action action)
	{
		//Check edges
		PointByte position;
		position.y = centerBlock._gridPosition.y + gridY;
		if (position.y < 0 || position.y >= _grid.height())
			return;

		for (int x = -maxGridX; x <= maxGridX; x++)
		{
			position.x = centerBlock._gridPosition.x + x;

			//Check edges
			if (position.x < 0)
				continue;
			else if (position.x >= _grid.width())
				break;
			for (Block* block = _grid(position.x, position.y); block != nullptr; block = block->_nextBlock)
			{
				//Do range check as rectangle distance
				if (block != &centerBlock &&
					Pathfinder::isInSquareDistance(centerBlock._position, block->_position, minRange, maxRange))
				{
					action(*block);
				}
			}
		}
	}

	template<typename Action>
	void runActionHorizontalInRange(const Block& centerBlock, int range, int gridY, int maxGridX, Action action)
	{
		//Check edges
		PointByte position;
		position.y = centerBlock._gridPosition.y + gridY;
		if (position.y < 0 || position.y >= _grid.height())
			return;

		for (int x = -maxGridX; x <= maxGridX; x++)
		{
			position.x = centerBlock._gridPosition.x + x;

			//Check edges
			if (position.x < 0)
				continue;
			else if (position.x >= _grid.width())
				break;
			for (Block* block = _grid(position.x, position.y); block != nullptr; block = block->_nextBlock)
			{
				//Do range check as rectangle distance
				if (block != &centerBlock &&
					Pathfinder::isInSquareDistance(centerBlock._position, block->_position, range))
				{
					action(*block);
				}
			}
		}
	}

	template<typename Action>
	void runActionHorizontalInRange(PointByte centerGridPosition, Point position, int range, int gridY, int maxGridX, Action action)
	{
		//Check edges
		PointByte gridPosition;
		gridPosition.y = centerGridPosition.y + gridY;
		if (gridPosition.y < 0 || gridPosition.y >= _grid.height())
			return;

		for (int x = -maxGridX; x <= maxGridX; x++)
		{
			gridPosition.x = centerGridPosition.x + x;

			//Check edges
			if (gridPosition.x < 0)
				continue;
			else if (gridPosition.x >= _grid.width())
				break;
			for (Block* block = _grid(gridPosition.x, gridPosition.y); block != nullptr; block = block->_nextBlock)
			{
				//Do range check as rectangle distance
				if (Pathfinder::isInSquareDistance(position, block->_position, range))					
					action(*block);					
			}
		}
	}

	template<typename Action>
	void runActionVerticalAtRange(const Block& centerBlock, int range, int gridX, int minGridY, int maxGridY, Action action)
	{
		//Check edges
		PointByte position;
		position.x = centerBlock._gridPosition.x + gridX;
		if (position.x < 0 || position.x >= _grid.width())
			return;

		for (int y = minGridY; y <= maxGridY; y++)
		{
			position.y = centerBlock._gridPosition.y + y;

			//Check edges
			if (position.y < 0)
				continue;
			else if (position.y >= _grid.height())
				break;
			for (Block* block = _grid(position.x, position.y); block != nullptr; block = block->_nextBlock)
			{
				//Only needs to check for X equality only since that is the one that will change change. Y just ensures it's still within range
				if (block != &centerBlock && std::abs(centerBlock._position.x - block->_position.x) == range &&
					std::abs(centerBlock._position.y - block->_position.y) <= range)
				{
					action(*block);
				}
			}
		}
	}

	template<typename Action>
	void runActionVerticalBetweenRange(const Block& centerBlock, int minRange, int maxRange, int gridX, int maxGridY, Action action)
	{
		//Check edges
		PointByte position;
		position.x = centerBlock._gridPosition.x + gridX;
		if (position.x < 0 || position.x >= _grid.width())
			return;

		for (int y = -maxGridY; y <= maxGridY; y++)
		{
			position.y = centerBlock._gridPosition.y + y;

			//Check edges
			if (position.y < 0)
				continue;
			else if (position.y >= _grid.height())
				break;
			for (Block* block = _grid(position.x, position.y); block != nullptr; block = block->_nextBlock)
			{
				//Do range check as rectangle distance
				if (block != &centerBlock &&
					Pathfinder::isInSquareDistance(centerBlock._position, block->_position, minRange, maxRange))
				{
					action(*block);
				}
			}
		}
	}

	template<typename Action>
	void runActionVerticalInRange(const Block& centerBlock, int range, int gridX, int maxGridY, Action action)
	{
		//Check edges
		PointByte position;
		position.x = centerBlock._gridPosition.x + gridX;
		if (position.x < 0 || position.x >= _grid.width())
			return;

		for (int y = -maxGridY; y <= maxGridY; y++)
		{
			position.y = centerBlock._gridPosition.y + y;

			//Check edges
			if (position.y < 0)
				continue;
			else if (position.y >= _grid.height())
				break;
			for (Block* block = _grid(position.x, position.y); block != nullptr; block = block->_nextBlock)
			{
				//Do range check as rectangle distance
				if (block != &centerBlock &&
					Pathfinder::isInSquareDistance(centerBlock._position, block->_position, range))
				{
					action(*block);
				}
			}
		}
	}

	static constexpr PointByte INVALID_POSITION = {-1, -1};

	FixedSizeArray2D<Block*, int> _grid;
};