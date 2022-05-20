#include <catch_v2_8_0.hpp>

#include <queue>
#include <stack>
#include <array>
#include <vector>
#include <iostream>

#include <cmath>

namespace astar_tests
{
	constexpr int MAX_WALK_DISTANCE = 15;
	constexpr int MAX_PATH_SIZE = MAX_WALK_DISTANCE*2;
	typedef std::array<int, MAX_PATH_SIZE> Path;

	namespace map
	{
		struct Tile
		{
			struct
			{
				uint8_t walkable : 1;
			};

			bool isSeeThrough() const
			{
				return walkable;
			}
		};
	}

	struct Point
	{
		bool operator == (Point& point) { return point.x == x && point.y == y; }

		int16_t x = 0;
		int16_t y = 0;
	};

	struct PointByte
	{
		bool operator == (Point& point) { return point.x == x && point.y == y; }
		bool operator != (Point& point) { return !(*this == point); }

		int8_t x = 0;
		int8_t y = 0;
	};

	#include <sdk/FixedSizeArray2D.hpp>

	#define UNIT_TEST_ASTAR
	#include <server/map/pathfinding/Pathfinder.h>
	#include <server/map/pathfinding/Pathfinder.cpp>

	TEST_CASE("Astar")
	{
		FixedSizeArray2D<map::Tile, uint16_t> tiles = FixedSizeArray2D<map::Tile, uint16_t>(20, 20);

		auto astar = map::Pathfinder(tiles);
		std::array<int, MAX_PATH_SIZE> path = std::array<int, MAX_PATH_SIZE>();

		auto pathLength = astar.findPath({ 1, 1 }, { MAX_WALK_DISTANCE + 2, MAX_WALK_DISTANCE + 2 }, path);
		REQUIRE(pathLength == 0);

		pathLength = astar.findPath({ 1, 1 }, { 5, 5 }, path);
		REQUIRE(pathLength == 4);

		pathLength = astar.findPath({ 1, 1 }, { 1, MAX_WALK_DISTANCE + 1 }, path);
		REQUIRE(pathLength == MAX_WALK_DISTANCE);
	}

	TEST_CASE("RectangleDistances")
	{
		int range = 15, minRange = 15, maxRange = 18;

		REQUIRE(map::Pathfinder::isInSquareDistance({ 10,10 }, { 15,15 }, minRange, maxRange) == false);
		REQUIRE(map::Pathfinder::isInSquareDistance({ 10,10 }, { 25,25 }, minRange, maxRange) == true);
		REQUIRE(map::Pathfinder::isInSquareDistance({ 10,10 }, { 26,25 }, minRange, maxRange) == true);
		REQUIRE(map::Pathfinder::isInSquareDistance({ 10,10 }, { 25,26 }, minRange, maxRange) == true);
		REQUIRE(map::Pathfinder::isInSquareDistance({ 10,10 }, { 10,25 }, minRange, maxRange) == true);
		REQUIRE(map::Pathfinder::isInSquareDistance({ 10,10 }, { 25,10 }, minRange, maxRange) == true);
		REQUIRE(map::Pathfinder::isInSquareDistance({ 10,10 }, { 10,25 }, minRange, maxRange) == true);
		REQUIRE(map::Pathfinder::isInSquareDistance({ 10,10 }, { 10,29 }, minRange, maxRange) == false);

		
		REQUIRE(map::Pathfinder::isInSquareDistance({ 10,10 }, { 15,15 }, range) == true);
		REQUIRE(map::Pathfinder::isInSquareDistance({ 10,10 }, { 25,25 }, range) == true);
		REQUIRE(map::Pathfinder::isInSquareDistance({ 10,10 }, { 26,25 }, range) == false);
		REQUIRE(map::Pathfinder::isInSquareDistance({ 10,10 }, { 25,26 }, range) == false);
		REQUIRE(map::Pathfinder::isInSquareDistance({ 10,10 }, { 10,25 }, range) == true);
		REQUIRE(map::Pathfinder::isInSquareDistance({ 10,10 }, { 25,10 }, range) == true);


		REQUIRE(map::Pathfinder::isExactlyRectangularDistance({ 10,10 }, { 15,15 }, range) == false);
		REQUIRE(map::Pathfinder::isExactlyRectangularDistance({ 10,10 }, { 25,15 }, range) == true);
		REQUIRE(map::Pathfinder::isExactlyRectangularDistance({ 10,10 }, { 25,25 }, range) == true);
		REQUIRE(map::Pathfinder::isExactlyRectangularDistance({ 10,10 }, { 25,24 }, range) == true);
		REQUIRE(map::Pathfinder::isExactlyRectangularDistance({ 10,10 }, { 26,25 }, range) == false);
		REQUIRE(map::Pathfinder::isExactlyRectangularDistance({ 10,10 }, { 25,26 }, range) == false);
	}
}