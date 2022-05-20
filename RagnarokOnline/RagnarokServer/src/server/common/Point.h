#pragma once

#include <cstdint>
#include <string_view>

struct Point
{
	constexpr bool operator == (const Point& point) const { return point.x == x && point.y == y; }
	constexpr bool operator != (const Point& point) const { return !(*this == point); }
	constexpr Point operator+(const Point& point) const { return Point{ x + point.x, y + point.y }; }
	constexpr Point operator-(const Point& point) const { return Point{ x - point.x, y - point.y }; }

	int16_t x = 0;
	int16_t y = 0;
};

struct PointByte
{
	constexpr bool operator == (const PointByte& point) const { return point.x == x && point.y == y; }
	constexpr bool operator != (const PointByte& point) const { return !(*this == point); }
	constexpr PointByte operator+(const PointByte& point) { return PointByte{ x + point.x, y + point.y }; }
	constexpr PointByte operator-(const PointByte& point) { return PointByte{ x - point.x, y - point.y }; }

	int8_t x = 0;
	int8_t y = 0;
};