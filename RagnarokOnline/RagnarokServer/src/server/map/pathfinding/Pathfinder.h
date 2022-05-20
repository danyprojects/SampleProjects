#pragma once

#ifndef UNIT_TEST_ASTAR
#include <server/common/Point.h>
#include <server/common/CommonTypes.h>
#include <server/common/ServerLimits.h>
#include <server/map/data/Tile.h>

#include <sdk/array/FixedSizeArray2D.hpp>

#include <queue>
#include <stack>
#include <array>
#include <vector>
#include <map>
#endif

class Pathfinder
{
private:
	struct AstarFlags
	{
		union
		{
			uint8_t flags; // this is so that we can reset quickly with a single assignment operation
			struct
			{
				uint8_t processed : 1;
				uint8_t awaitingProcessing : 1;
				uint8_t reserved : 6;
			};
		};		
	};

	struct AstarNode
	{
		int16_t cost;
		Point tile;
		int16_t parent;
	};

public:
	typedef std::array<Point, MAX_PATH_LENGTH> Path;

	Pathfinder();

	/// <summary>Path does not included starting cell and contains ending cell</summary>
	/// <returns>Path length</returns>
	int findPath(Point start, Point end, PathType pathType, Path& outPath);

	void setTiles(const FixedSizeArray2D<Tile, uint16_t>* tiles);

	bool isInLineOfSight(Point start, Point end);

	static int CalculateManhattanDistance(const Point &center, const Point &point)
	{
		return std::abs(point.x - center.x) + std::abs(point.y - center.y);
	}

	static int CalculateManhattanDistance(const PointByte &center, const Point &point)
	{
		return std::abs(point.x - center.x) + std::abs(point.y - center.y);
	}

	static int CalculateManhattanDistance(const Point &center, const PointByte &point)
	{
		return std::abs(point.x - center.x) + std::abs(point.y - center.y);
	}

	static int CalculateManhattanDistance(const PointByte &center, const PointByte &point)
	{
		return std::abs(point.x - center.x) + std::abs(point.y - center.y);
	}

	static int CalculateManhattanDistance(int startX, int startY, int endX, int endY)
	{
		return std::abs(endX - startX) + std::abs(endY - startY);
	}

	static int CalculateDiagonalDistance(const Point &center, const Point &point)
	{
		return std::max(std::abs(point.x - center.x), std::abs(point.y - center.y));
	}

	static int CalculateDiagonalDistance(const PointByte &center, const Point &point)
	{
		return std::max(std::abs(point.x - center.x), std::abs(point.y - center.y));
	}

	static int CalculateDiagonalDistance(const Point &center, const PointByte &point)
	{
		return std::max(std::abs(point.x - center.x), std::abs(point.y - center.y));
	}

	static int CalculateDiagonalDistance(const PointByte &center, const PointByte &point)
	{
		return std::max(std::abs(point.x - center.x), std::abs(point.y - center.y));
	}

	static int CalculateDiagonalDistance(int startX, int startY, int endX, int endY)
	{
		return std::max(std::abs(endX - startX), std::abs(endY - startY));
	}

	static bool isInSquareDistance(const Point &center, const Point &point, int minRange, int maxRange)
	{
		int max = std::max(std::abs(center.x - point.x), std::abs(center.y - point.y));
		return minRange <= max && max <= maxRange;
	}

	static bool isInSquareDistance(const Point &center, const Point &point, int range)
	{
		return std::abs(center.x - point.x) <= range &&
				std::abs(center.y - point.y) <= range;
	}

	static bool isInRectangularDistance(const Point &center, const Point &point, int rangeX, int rangeY) 
	{
		return std::abs(center.x - point.x) <= rangeX &&
			std::abs(center.y - point.y) <= rangeY;
	}

	static bool isExactlyRectangularDistance(const Point &center, const Point &point, int range)
	{
		return std::max(std::abs(center.x - point.x), std::abs(center.y - point.y)) == range;
	}

private:

	//Methods for Astar
	//int calcPath(Point start, Point end, Path& path);
	int calcStraightPath(Point start, Point end, Path &path);

	int calcAstarPath(Point start, Point end, Path& path);

	template<int16_t COST> //cost will be either diagonal or straight. Template it for no loss
	void tryEnqueueNode(Point tile, int16_t currentIndex, Point end)
	{
		int16_t newCost = _nodes[currentIndex].cost + COST;

		//if node is already awaiting processing then find it, check the cost and update it if needed
		if (_mapFlags(tile.x, tile.y).awaitingProcessing)
		{
			auto& nodeIt = _nodesToProcess.begin();
			for (; _nodes[nodeIt->second].tile != tile; nodeIt++) {} //Because the flag awaiting processing was true, this will always contain the node

			//If new cost is greater or equal than previously calculated cose, we can ignore this node
			if (_nodes[nodeIt->second].cost <= newCost)
				return;

			//otherwise change the cost and parent, and also update it in the multimap
			_nodes[nodeIt->second].cost = newCost;
			_nodes[nodeIt->second].parent = currentIndex;

			//Extract unlinks the node and lets us change the key and insert it back with move semantics
			auto node = _nodesToProcess.extract(nodeIt);
			node.key() = newCost + STRAIGHT_COST * CalculateManhattanDistance(tile, end); // the * STRAIGHT_COST is to scale manhattan distance properly to the cost
			_nodesToProcess.insert(std::move(node));
		}
		else //Don't waste time searching, just update it and insert it 
		{
			//Set the fields
			_mapFlags(tile.x, tile.y).awaitingProcessing = true;
			_nodes[_nextFreeIndex].cost = newCost;
			_nodes[_nextFreeIndex].parent = currentIndex;
			_nodes[_nextFreeIndex].tile = tile;

			//Insert it in nodes to process
			_nodesToProcess.insert({ newCost + STRAIGHT_COST * CalculateManhattanDistance(tile, end), _nextFreeIndex });

			_nextFreeIndex++;
		}
	}

	void cleanupAstar();

	static constexpr int16_t DIAGONAL_COST = 14;
	static constexpr int16_t STRAIGHT_COST = 10;

	// Max path lenght theoretically would be going all the way left then all the way right and passing all the cells
	// So max number of nodes is max path lenght * 2 squared
	static constexpr int16_t MAX_NODES = (MAX_PATH_LENGTH * 2) * (MAX_PATH_LENGTH * 2);
	std::array<AstarNode, MAX_NODES + 8> _nodes; // give it 8 more to avoid crashes
	int16_t _nextFreeIndex;

	//Will contain the node indexes astar has to process. The key will be the cost so it's sorted by cost, the value will be the index to the nodes array
	std::multimap<int16_t, int16_t> _nodesToProcess;
	FixedSizeArray2D<AstarFlags, int16_t> _mapFlags;

	const FixedSizeArray2D<Tile, uint16_t>* _tiles;
};