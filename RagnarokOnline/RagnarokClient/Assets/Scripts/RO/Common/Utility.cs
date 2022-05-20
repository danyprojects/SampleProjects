using RO.Containers;
using UnityEngine;

namespace RO.Common
{

    public static class Utility
    {
        public const float RAYCAST_DISTANCE = 120f;
        public const float DEFAULT_Y = RAYCAST_DISTANCE - 20f;

        private readonly static int[,] DirectionLookup;
        private const int LOOKUP_RANGE = 16;

        static Utility()
        {
            DirectionLookup = new int[LOOKUP_RANGE * 2 + 1, LOOKUP_RANGE * 2 + 1];

            Vector2 center = new Vector2Int(0, 0);
            //Let's assume we cannot do any action outside default view range so we only need to pre calculate directions for that
            for (int y = -LOOKUP_RANGE; y < LOOKUP_RANGE + 1; y++)
            {
                for (int x = -LOOKUP_RANGE; x < LOOKUP_RANGE + 1; x++)
                {
                    Vector2 index = new Vector2(center.x + x + LOOKUP_RANGE, center.y + y + LOOKUP_RANGE);

                    if (center.x == x && center.y == y)
                        DirectionLookup[(int)index.x, (int)index.y] = 0;

                    Vector2 direction = new Vector2(x - center.x, y - center.y);

                    float angle = Vector2.SignedAngle(direction, Vector2.down) + 180;
                    //Shift angle by 22.5fº to the right so that 1 division can get the direction
                    //Remove 0.05f off the final angle to make sure it never lands in 360, otherwise we would get direction 8
                    DirectionLookup[(int)index.x, (int)index.y] = Mathf.FloorToInt(((angle + 22.5f) % 360) - 0.05f) / 45;
                }
            }

            /* //Uncomment for printing the direction array
            string s = "";
            for (int y = 0; y < LOOKUP_RANGE * 2 + 1; y++)
            {
                s += "{";
                for (int x = 0; x < LOOKUP_RANGE * 2 + 1; x++)                
                    s += DirectionLookup[x, y] + ", ";                
                s += "},\n";
            }
            Debug.Log(s);
            */
        }

        public static void GameToWorldCoordinates(int x, int y, out Vector3 coordinates)
        {
            coordinates.x = x * Constants.CELL_TO_UNIT_SIZE;
            coordinates.y = DEFAULT_Y;  // for raycasting
            coordinates.z = y * Constants.CELL_TO_UNIT_SIZE;
        }

        public static void GameToWorldCoordinatesCenter(int x, int y, out Vector3 coordinates)
        {
            coordinates.x = x * Constants.CELL_TO_UNIT_SIZE + Constants.HALF_CELL_UNIT_SIZE;
            coordinates.y = DEFAULT_Y;  // for raycasting
            coordinates.z = y * Constants.CELL_TO_UNIT_SIZE + Constants.HALF_CELL_UNIT_SIZE;
        }

        public static void GameToWorldCoordinatesCenter(Vector2Int position, out Vector3 coordinates)
        {
            coordinates.x = position.x * Constants.CELL_TO_UNIT_SIZE + Constants.HALF_CELL_UNIT_SIZE;
            coordinates.y = DEFAULT_Y;  // for raycasting
            coordinates.z = position.y * Constants.CELL_TO_UNIT_SIZE + Constants.HALF_CELL_UNIT_SIZE;
        }

        public static void WorldToGameCoordinates(ref Vector3 point, in MapData mapData, ref Vector2Int coordinates)
        {
            int x = (int)point.x / Constants.CELL_TO_UNIT_SIZE;
            coordinates.x = x % mapData.Width;
            coordinates.y = (int)(point.z / Constants.CELL_TO_UNIT_SIZE);
        }

        public static void WorldToGameCoordinates(Vector3 point, ref MapData mapData, ref Vector2Int coordinates)
        {
            int x = (int)point.x / Constants.CELL_TO_UNIT_SIZE;
            coordinates.x = x % mapData.Width;
            coordinates.y = (int)(point.z / Constants.CELL_TO_UNIT_SIZE);
        }

        public static float CapValue(float value, float max, float min)
        {
            return value > max ? max : value < min ? min : value;
        }

        public static bool IsInRectangularDistance(ref Vector2Int center, ref Vector2Int point, int minRange, int maxRange)
        {
            int max = Mathf.Max(Mathf.Abs(center.x - point.x), Mathf.Abs(center.y - point.y));
            return minRange <= max && max <= maxRange;
        }

        public static bool IsInRectangularDistance(ref Vector2Int center, ref Vector2Int point, int range)
        {
            return Mathf.Abs(center.x - point.x) <= range && Mathf.Abs(center.y - point.y) <= range;
        }

        public static bool IsExactlyRectangularDistance(ref Vector2Int center, ref Vector2Int point, int range)
        {
            return Mathf.Max(Mathf.Abs(center.x - point.x), Mathf.Abs(center.y - point.y)) == range;
        }

        public static bool PointIsAdjacentToBothPositions(int x, int y, int xA, int yA, int xB, int yB)
        {
            int auxAX = Mathf.Abs(xA - x);
            int auxAY = Mathf.Abs(yA - y);
            int auxBX = Mathf.Abs(xB - x);
            int auxBY = Mathf.Abs(yB - y);

            return auxAX + auxAY + auxBX + auxBY <= 4;
        }

        public static bool PointIsAdjacentToBothPositions(ref Vector2Int point, ref Vector2Int positionA, ref Vector2Int positionB)
        {
            return PointIsAdjacentToBothPositions(point.x, point.y, positionA.x, positionA.y, positionB.x, positionB.y);
        }

        public static bool PointIsAdjacentToBothPositions(ref Vector2Int point, int xA, int yA, ref Vector2Int positionB)
        {
            return PointIsAdjacentToBothPositions(point.x, point.y, xA, yA, positionB.x, positionB.y);
        }

        public static bool PointIsAdjacentToBothPositions(int x, int y, ref Vector2Int positionA, ref Vector2Int positionB)
        {
            return PointIsAdjacentToBothPositions(x, y, positionA.x, positionA.y, positionB.x, positionB.y);
        }

        public static bool PointIsAdjacentToBothPositions(int x, int y, ref Vector2Int positionA, int xB, int yB)
        {
            return PointIsAdjacentToBothPositions(x, y, positionA.x, positionA.y, xB, yB);
        }

        public static bool PointIsAdjacentToBothPositions(int x, int y, int xA, int yA, ref Vector2Int positionB)
        {
            return PointIsAdjacentToBothPositions(x, y, xA, yA, positionB.x, positionB.y);
        }

        //Overloads for looking up direction. Unsafe ones don't do % LOOKUP_RANGE so it assumes the different from and to is always inbounds. Useful for movement
        public static int LookUpDirectionSafe(Vector2Int from, Vector2Int to)
        {
            return DirectionLookup[(from.x - to.x) % LOOKUP_RANGE + LOOKUP_RANGE, (from.y - to.y) % LOOKUP_RANGE + LOOKUP_RANGE];
        }

        public static int LookUpDirectionSafe(int fromX, int fromY, int toX, int toY)
        {
            return DirectionLookup[(fromX - toX) % LOOKUP_RANGE + LOOKUP_RANGE, (fromY - toY) % LOOKUP_RANGE + LOOKUP_RANGE];
        }

        public static int LookUpDirectionUnsafe(Vector2Int from, Vector2Int to)
        {
            return DirectionLookup[from.x - to.x + LOOKUP_RANGE, from.y - to.y + LOOKUP_RANGE];
        }

        public static int LookUpDirectionUnsafe(int fromX, int fromY, int toX, int toY)
        {
            return DirectionLookup[fromX - toX + LOOKUP_RANGE, fromY - toY + LOOKUP_RANGE];
        }
    }
}
