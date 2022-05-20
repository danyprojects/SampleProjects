#ifndef UNIT_TEST_ASTAR
#include "Pathfinder.h"

#include <cmath>
#endif

namespace
{
	using TileArray = FixedSizeArray2D<Tile, uint16_t>;

	inline bool hasTopRightPath(int x, int y, const TileArray& tiles)
	{
		int right = x + 1;
		int up    = y + 1;

		return tiles(right, up).walkable && tiles(right, y).walkable && tiles(x, up).walkable;
	}

	inline bool hasTopLeftPath(int x, int y, const TileArray& tiles)
	{
		int left = x - 1;
		int up   = y + 1;

		return tiles(left, up).walkable && tiles(left, y).walkable && tiles(x, up).walkable;
	}

	inline bool hasBottomRightPath(int x, int y, const TileArray& tiles)
	{
		int right = x + 1;
		int down  = y - 1;

		return tiles(right, down).walkable && tiles(right, y).walkable && tiles(x, down).walkable;
	}

	inline bool hasBottomLeftPath(int x, int y, const TileArray& tiles)
	{
		int left = x - 1;
		int down = y - 1;

		return tiles(left, down).walkable && tiles(left, y).walkable && tiles(x, down).walkable;
	}
}

Pathfinder::Pathfinder()
	: _mapFlags(MAX_MAP_SIZE, MAX_MAP_SIZE)
	, _nextFreeIndex(0)
	, _tiles(nullptr)
{
	for (int y = 0; y < MAX_MAP_SIZE; y++)
		for (int x = 0; x < MAX_MAP_SIZE; x++)
			_mapFlags(x, y).flags = 0;
}

//*****public API

//Find path with astar
int Pathfinder::findPath(Point start, Point end, PathType pathType, Path& outPath)
{
	if (start == end || !(*_tiles)(end.x, end.y).isWalkable() || CalculateDiagonalDistance(start, end) > MAX_WALK_DISTANCE)
	{
		outPath[0] = Point{ INT16_MAX, INT16_MAX };
		return 0;
	}

	//Try straight path calculation first
	int pathLength = calcStraightPath(start, end, outPath);
	if (pathLength != -1)
		return pathLength;

	//Skip astar if we couldn't find a straight path and flag easy only was active
	if (pathType == PathType::Weak)
		return 0;

	return calcAstarPath(start, end, outPath);
}

void Pathfinder::setTiles(const FixedSizeArray2D<Tile, uint16_t>* tiles)
{
	_tiles = tiles;
}

bool Pathfinder::isInLineOfSight(Point start, Point end)
{
	if (start.x == end.x && start.y == end.y)
		return true;

	int dirX, dirY;
	int weightX = 0, weightY = 0;
	int weight;

	dirX = end.x - start.x;
	if (dirX < 0)
	{
		int aux = start.x;
		start.x = end.x;
		end.x = aux;

		aux = start.y;
		start.y = end.y;
		end.y = aux;

		dirX = -dirX;
	}
	dirY = end.y - start.y;

	if (dirX > std::abs(dirY))
		weight = dirX;
	else
		weight = std::abs(dirY);

	bool isFinalCell = false;
	do
	{
		weightX += dirX;
		weightY += dirY;

		if (weightX >= weight)
		{
			weightX -= weight;
			start.x++;
		}
		if (weightY >= weight)
		{
			weightY -= weight;
			start.y++;
		}
		else if (weightY < 0) // we don't do this for X because dirX is always positive
		{
			weightY += weight;
			start.y--;
		}
		isFinalCell = start.x == end.x && start.y == end.y;
		if (!isFinalCell && !((*_tiles)(start.x, start.y).isSeeThrough()))
			return false;
	} while (!isFinalCell);

	return true;
}

//Fast lazy straight path calculation
int Pathfinder::calcStraightPath(Point start, Point end, Path& path)
{
	int startIndex;

	int xInc = start.x < end.x ? 1 : start.x > end.x ? -1 : 0;
	int yInc = start.y < end.y ? 1 : start.y > end.y ? -1 : 0;

	Point current = start;

	//try moving diagonally first
	for (startIndex = 0; startIndex < MAX_WALK_DISTANCE; startIndex++)
	{
		//Check if it's diagonal movement and sides are valid
		if (xInc != 0 && yInc != 0)
		{

			if (!(*_tiles)(current.x, current.y + yInc).walkable ||
				!(*_tiles)(current.x + xInc, current.y).walkable)
				return -1; // diagonal was not valid. Don't cut corners)
		}
		else //not a diagonal movement. Break and move into straight movement                
			break;

		current.x += xInc;
		current.y += yInc;
		//Check if current cell is walkable
		if (!(*_tiles)(current.x, current.y).walkable)
			return -1;

		path[startIndex] = current;

		if (current.x == end.x)
			xInc = 0;
		if (current.y == end.y)
			yInc = 0;

		//Sucess at finding path
		if (xInc == 0 && yInc == 0)
			return startIndex + 1;
	}

	//We get here if so far the path is valid and we are done moving diagonally. Continue moving in a straight line without diagonal checks
	for (; startIndex < MAX_WALK_DISTANCE; startIndex++)
	{
		current.x += xInc;
		current.y += yInc;
		//Check if current cell is walkable
		if (!(*_tiles)(current.x, current.y).walkable)
			return -1;

		path[startIndex] = current;

		if (current.x == end.x)
			xInc = 0;
		if (current.y == end.y)
			yInc = 0;

		//Sucess at finding path
		if (xInc == 0 && yInc == 0)
			return startIndex + 1;
	}

	return -1; // couldnt find path
}

//***********Methods for Astar

int Pathfinder::calcAstarPath(Point start, Point end, Path& path)
{
	//Use up the first node
	int16_t currentIndex = 0;
	_nextFreeIndex = 1;

	//Set variables for starting node
	_nodes[currentIndex].cost = 0;
	_nodes[currentIndex].parent = -1;
	_nodes[currentIndex].tile = start;

	while (_nodes[currentIndex].tile != end)
	{
		//Mark node as processed
		auto& currentTile = _nodes[currentIndex].tile;
		_mapFlags(currentTile.x, currentTile.y).processed = true;

		const int16_t right = currentTile.x + 1;
		const int16_t left = currentTile.x - 1;
		const int16_t up = currentTile.y + 1;
		const int16_t down = currentTile.y - 1;

		//push Right
		if ((*_tiles)(right, currentTile.y).isWalkable() && !_mapFlags(right, currentTile.y).processed)
			tryEnqueueNode<STRAIGHT_COST>(Point{ right, currentTile.y }, currentIndex, end);

		//push Left
		if ((*_tiles)(left, currentTile.y).isWalkable() && !_mapFlags(left, currentTile.y).processed)
			tryEnqueueNode<STRAIGHT_COST>(Point{ left, currentTile.y }, currentIndex, end);

		//push Up
		if ((*_tiles)(currentTile.x, up).isWalkable() && !_mapFlags(currentTile.x, up).processed)
			tryEnqueueNode<STRAIGHT_COST>(Point{ currentTile.x, up }, currentIndex, end);

		//push Down
		if ((*_tiles)(currentTile.x, down).isWalkable() && !_mapFlags(currentTile.x, down).processed)
			tryEnqueueNode<STRAIGHT_COST>(Point{ currentTile.x, down }, currentIndex, end);

		//Diagonals do extra checks so they don't cut corners
		//push top right 
		if ((*_tiles)(right, up).isWalkable() && (*_tiles)(currentTile.x, up).isWalkable() && (*_tiles)(right, currentTile.y).isWalkable() && !_mapFlags(right, up).processed)
			tryEnqueueNode<DIAGONAL_COST>(Point{ right, up }, currentIndex, end);

		//push Top left
		if ((*_tiles)(left, up).isWalkable() && (*_tiles)(currentTile.x, up).isWalkable() && (*_tiles)(left, currentTile.y).isWalkable() && !_mapFlags(left, up).processed)
			tryEnqueueNode<DIAGONAL_COST>(Point{ left, up }, currentIndex, end);

		//push Bottom right
		if ((*_tiles)(right, down).isWalkable() && (*_tiles)(currentTile.x, down).isWalkable() && (*_tiles)(right, currentTile.y).isWalkable() && !_mapFlags(right, down).processed)
			tryEnqueueNode<DIAGONAL_COST>(Point{ right, down }, currentIndex, end);

		//push Bottom left
		if ((*_tiles)(left, down).isWalkable() && (*_tiles)(currentTile.x, down).isWalkable() && (*_tiles)(left, currentTile.y).isWalkable() && !_mapFlags(left, down).processed)
			tryEnqueueNode<DIAGONAL_COST>(Point{ left, down }, currentIndex, end);

		/* //this is to visualize the processed path with debug
		std::ofstream file("output.txt");
		for (int y = 0; y < _tiles.height(); y++)
		{
			for (int x = 0; x < _tiles.width(); x++)
			{					
				if (Point{ (int16_t)x, (int16_t)y } == start)
					file << "S";
				else if (Point{ (int16_t)x, (int16_t)y } == currentTile)
					file << "x";
				else if (_mapFlags(x, y).processed)
					file << "-";
				else if (_mapFlags(x, y).awaitingProcessing)
					file << "~";
				else						
					file << (_tiles(x, y).isWalkable() ? " " : "O");
			}
			file << "\n";
		}
		file.close();
		*/

		//If there are no nodes to process and we haven't reached the end or we have no more free nodes then it's an impossible path
		if (_nodesToProcess.size() == 0 || _nextFreeIndex >= MAX_NODES)
		{
			cleanupAstar();
			return 0;
		}

		//Get info for next node to process and remove it from toProcess array
		currentIndex = _nodesToProcess.begin()->second;
		_nodesToProcess.erase(_nodesToProcess.begin()); 
	}

	//Path was found if we get here. CurrentIndex has the index to the node containing the end tile
	int16_t pathLength = 0;
	int16_t pathIndexes[MAX_PATH_LENGTH * 10]; //will temporarily store the path, from end to start. Raise this if we get stack corruption by having larger paths
	//backtrack to get path indexes and length
	do
	{
		pathIndexes[pathLength++] = currentIndex;
		currentIndex = _nodes[currentIndex].parent;
	} while (currentIndex != 0); //0 was our starting cell

	// It's possible that it calculated a longer path than we expected. Copy only a max of MAX_PATH_LENGTH tiles
	int16_t min = pathLength <= MAX_PATH_LENGTH ? pathLength : MAX_PATH_LENGTH;
	//Now copy in reverse path cells to return path
	for (int16_t i = pathLength - 1; i >= pathLength - min; i--)
		path[pathLength - 1 - i] = _nodes[pathIndexes[i]].tile;

	cleanupAstar();

	return min;
}

void Pathfinder::cleanupAstar()
{
	_nodesToProcess.clear();

	//Only need to clear flags for nodes we used
	for (int i = 0; i < _nextFreeIndex; i++)
		_mapFlags(_nodes[i].tile.x, _nodes[i].tile.y).flags = 0;
}