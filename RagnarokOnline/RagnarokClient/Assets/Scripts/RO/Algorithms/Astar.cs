using RO.Common;
using RO.Containers;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Algorithms
{
    public sealed partial class Pathfinder
    {
        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 11;

        //"Forward declarations" This way we can keep visibility of the AstarPathNode just inside Astar while letting the queue access the astar path node
        private sealed partial class AstarPriorityQueue { }
        private sealed partial class AstarStack { }

        private class AstarPathNode
        {
            //For fast priorityQueue. No longer an interface and no longer has getters / setters
            public int Priority;
            public int QueueIndex;

            public int X = 0, Y = 0;
            public int Iteration;
            public int NodeNr, ParentNodeNr; // used to backtrack and find the final path

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public AstarPathNode SetNodeInfo(int x, int y, int iteration, int nodeNr, int parentNodeNr)
            {
                X = x;
                Y = y;
                Iteration = iteration;
                NodeNr = nodeNr;
                ParentNodeNr = parentNodeNr;
                Priority = _astarMap.Tiles[x, y].Distance;
                return this;
            }
        }
        private class AstarWalkedMap
        {
            public struct AstarTile
            {
                public int Tag;
                public int Distance; // "int max value" marks as walked

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void SetTileWalked()
                {
                    Distance = int.MinValue;
                }
            }

            //Tiles contains the tag per tile. Tag is an int to save space. If clears happen to often then can upgrade to a long
            public AstarTile[,] Tiles;

            public AstarWalkedMap()
            {
                Tiles = new AstarTile[Constants.MAX_MAP_WIDTH, Constants.MAX_MAP_HEIGHT];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool IsTileBetter(int x, int y, int moveCost)
            {
                int distance = ManhattanDistance(x, y) + _iteration + moveCost;
                if (Tiles[x, y].Tag != _tag)
                {
                    Tiles[x, y].Tag = _tag;
                    Tiles[x, y].Distance = int.MaxValue;
                }
                if (Tiles[x, y].Distance > distance)
                {
                    Tiles[x, y].Distance = distance;
                    return true;
                }
                else
                    return false;
            }
            public void Reset()
            {
                for (int i = 0; i < Constants.MAX_MAP_HEIGHT; i++)
                    for (int k = 0; k < Constants.MAX_MAP_WIDTH; k++)
                        Tiles[k, i].Tag = 0;
            }
        }

        static AstarPriorityQueue _priorityQueue = new AstarPriorityQueue(Constants.MAX_NODES);

        //Tag will be a int instead of uint because it generates a smaller assembly due to passing overflow check to programmer
        static int _tag = 0;
        static MapData _map = null;

        //Fields for astar algorithm
        static AstarWalkedMap _astarMap = null;
        static AstarStack _spareNodes = new AstarStack(), _walkedNodes = new AstarStack();
        static int _endX, _endY;
        static int _iteration = 0;

        static Pathfinder()
        {
            _astarMap = new AstarWalkedMap();
            //_astarMap.Reset(); // no longer need reset on set map as map cache has the maximum possible size
            //Tag = 0; // Since we don't reset map, also don't reset tag 
            for (int i = 0; i < Constants.MAX_NODES; i++) // Create our node pool
                _spareNodes.Push(new AstarPathNode());
        }

        //Call this every time we change map so we calculate path according to the correct walkable cells
        static public void SetMap(MapData map)
        {
            _map = map; // save a reference to map data
        }

        //Different overloads for calculating the path
        static public int FindPath(ref Vector2Int start, ref Vector2Int end, ref Vector2Int[] path, int maxLength = Constants.MAX_WALK)
        {
            return FindPath(start.x, start.y, end.x, end.y, ref path, maxLength);
        }
        static public int FindPath(ref Vector2Int start, int endX, int endY, ref Vector2Int[] path, int maxLength = Constants.MAX_WALK)
        {
            return FindPath(start.x, start.y, endX, endY, ref path, maxLength);
        }
        static public int FindPath(int startX, int startY, ref Vector2Int end, ref Vector2Int[] path, int maxLength = Constants.MAX_WALK)
        {
            return FindPath(startX, startY, end.x, end.y, ref path, maxLength);
        }
        static public int FindPath(int startX, int startY, int endX, int endY, ref Vector2Int[] path, int maxLength = Constants.MAX_WALK)
        {
            if (DiagonalDistance(startX, startY, endX, endY) > Constants.MAX_WALK || (startX == endX && startY == endY))
                return 0;

            _endX = endX;
            _endY = endY;

            //Try straight path calculation first
            int pathLength = CalcStraightPath(startX, startY, ref path, maxLength);
            if (pathLength != -1)
                return pathLength;

            //Straight path failed, use regular Astar
            if (_tag == int.MaxValue)
            {
                // in the case too long has been spent on the same map, reset to prevent false alarms of same tag
                // This way we only clear 
                _astarMap.Reset();
                _tag = 0;
            }
            _tag++; // Tag will be used to determine wether a tile has been processed yet or not

            _astarMap.IsTileBetter(startX, startY, 0); // To mark the first cell for non reuse.

            return CalcAstarPath(startX, startY, ref path, maxLength);
        }

        static public bool IsInLineOfSight(ref Vector2Int start, ref Vector2Int target)
        {
            return IsInLineOfSight(start.x, start.y, target.x, target.y);
        }
        static public bool IsInLineOfSight(int startX, int startY, ref Vector2Int target)
        {
            return IsInLineOfSight(startX, startY, target.x, target.y);
        }
        static public bool IsInLineOfSight(ref Vector2Int start, int targetX, int targetY)
        {
            return IsInLineOfSight(start.x, start.y, targetX, targetY);
        }
        static public bool IsInLineOfSight(int startX, int startY, int targetX, int targetY)
        {
            if (startX == targetX && startY == targetY)
                return true;

            int dirX, dirY;
            int weightX = 0, weightY = 0;
            int weight;

            dirX = targetX - startX;
            if (dirX < 0)
            {
                int aux = startX;
                startX = targetX;
                targetX = aux;

                aux = startY;
                startY = targetY;
                targetY = aux;

                dirX = -dirX;
            }
            dirY = targetY - startY;

            if (dirX > Mathf.Abs(dirY))
                weight = dirX;
            else
                weight = Mathf.Abs(dirY);

            bool isFinalCell = false;
            do
            {
                weightX += dirX;
                weightY += dirY;

                if (weightX >= weight)
                {
                    weightX -= weight;
                    startX++;
                }
                if (weightY >= weight)
                {
                    weightY -= weight;
                    startY++;
                }
                else if (weightY < 0) // we don't do this for X because dirX is always positive
                {
                    weightY += weight;
                    startY--;
                }
                isFinalCell = startX == targetX && startY == targetY;
                if (!isFinalCell && !_map.Tiles[startX + startY * _map.Width].IsSeeThrough)
                    return false;
            } while (!isFinalCell);

            return true;
        }

        //Methods for Astar
        static private int CalcStraightPath(int startX, int startY, ref Vector2Int[] path, int maxLength)
        {
            int startIndex;

            int xInc = startX < _endX ? 1 : startX > _endX ? -1 : 0;
            int yInc = startY < _endY ? 1 : startY > _endY ? -1 : 0;

            int x = startX, y = startY;

            //try moving diagonally first
            for (startIndex = 0; startIndex < maxLength; startIndex++)
            {
                //Check if it's diagonal movement and sides are valid
                if (xInc != 0 && yInc != 0)
                {
                    if (!_map.Tiles[x + (y + yInc) * _map.Width].IsWalkable ||
                        !_map.Tiles[x + xInc + y * _map.Width].IsWalkable)
                        return -1; // diagonal was not valid. Don't cut corners)
                }
                else //not a diagonal movement. Break and move into straight movement                
                    break;

                x += xInc;
                y += yInc;
                //Check if current cell is walkable
                if (!_map.Tiles[x + y * _map.Width].IsWalkable)
                    return -1;

                path[startIndex].x = x;
                path[startIndex].y = y;

                if (x == _endX)
                    xInc = 0;
                if (y == _endY)
                    yInc = 0;

                //Sucess at finding path
                if (xInc == 0 && yInc == 0)
                    return startIndex + 1;
            }

            //We get here if so far the path is valid and we are done moving diagonally. Continue moving in a straight line without diagonal checks
            for (; startIndex < maxLength; startIndex++)
            {
                x += xInc;
                y += yInc;
                //Check if current cell is walkable
                if (!_map.Tiles[x + y * _map.Width].IsWalkable)
                    return -1;

                path[startIndex].x = x;
                path[startIndex].y = y;

                if (x == _endX)
                    xInc = 0;
                if (y == _endY)
                    yInc = 0;

                //Sucess at finding path
                if (xInc == 0 && yInc == 0)
                    return startIndex + 1;
            }

            return -1; // couldnt find path
        }

        static int CalcAstarPath(int startX, int startY, ref Vector2Int[] path, int maxLength)
        {
            int x = startX, y = startY;
            int nodeNrCount = 0;
            int nodeNr = 0;
            AstarPathNode node;
            _iteration = 1;
            int w = _map.Width;

            //Calc the path into a stack
            while ((x != _endX || y != _endY) && _iteration - 1 < maxLength)
            {
                //In case we run out of space just return we couldnt calculate
                if (_spareNodes.Count() < 8)
                {
                    ClearContainers();
                    return 0;
                }

                int right = x + 1, left = x - 1;
                int up = y + 1, down = y - 1;

                bool rightIsWalkable = _map.Tiles[right + y * w].IsWalkable;
                bool leftIsWalkable = _map.Tiles[left + y * w].IsWalkable;
                bool upIsWalkable = _map.Tiles[x + up * w].IsWalkable;
                bool downIsWalkable = _map.Tiles[x + down * w].IsWalkable;

                #region Enqueues  

                //Enqueue Top right
                if (_map.Tiles[right + up * w].IsWalkable && rightIsWalkable && upIsWalkable && _astarMap.IsTileBetter(right, up, MOVE_DIAGONAL_COST))
                    _priorityQueue.Enqueue(_spareNodes.Pop().SetNodeInfo(right, up, _iteration, ++nodeNrCount, nodeNr));
                //Enqueue Top left
                if (_map.Tiles[left + up * w].IsWalkable && leftIsWalkable && upIsWalkable && _astarMap.IsTileBetter(left, up, MOVE_DIAGONAL_COST))
                    _priorityQueue.Enqueue(_spareNodes.Pop().SetNodeInfo(left, up, _iteration, ++nodeNrCount, nodeNr));
                //Enqueue Bottom right
                if (_map.Tiles[right + down * w].IsWalkable && rightIsWalkable && downIsWalkable && _astarMap.IsTileBetter(right, down, MOVE_DIAGONAL_COST))
                    _priorityQueue.Enqueue(_spareNodes.Pop().SetNodeInfo(right, down, _iteration, ++nodeNrCount, nodeNr));
                //Enqueue Bottom left
                if (_map.Tiles[left + down * w].IsWalkable && leftIsWalkable && downIsWalkable && _astarMap.IsTileBetter(left, down, MOVE_DIAGONAL_COST))
                    _priorityQueue.Enqueue(_spareNodes.Pop().SetNodeInfo(left, down, _iteration, ++nodeNrCount, nodeNr));

                //Enqueue Right
                if (rightIsWalkable && _astarMap.IsTileBetter(right, y, MOVE_STRAIGHT_COST))
                    _priorityQueue.Enqueue(_spareNodes.Pop().SetNodeInfo(right, y, _iteration, ++nodeNrCount, nodeNr));
                //Enqueue Left
                if (leftIsWalkable && _astarMap.IsTileBetter(left, y, MOVE_STRAIGHT_COST))
                    _priorityQueue.Enqueue(_spareNodes.Pop().SetNodeInfo(left, y, _iteration, ++nodeNrCount, nodeNr));
                //Enqueue Up
                if (upIsWalkable && _astarMap.IsTileBetter(x, up, MOVE_STRAIGHT_COST))
                    _priorityQueue.Enqueue(_spareNodes.Pop().SetNodeInfo(x, up, _iteration, ++nodeNrCount, nodeNr));
                //Enqueue Down
                if (downIsWalkable && _astarMap.IsTileBetter(x, down, MOVE_STRAIGHT_COST))
                    _priorityQueue.Enqueue(_spareNodes.Pop().SetNodeInfo(x, down, _iteration, ++nodeNrCount, nodeNr));
                #endregion

                //Get the next node.
                node = _priorityQueue.Dequeue();

                while (_astarMap.Tiles[node.X, node.Y].Distance == int.MinValue)
                {
                    _spareNodes.Push(node);
                    node = _priorityQueue.Dequeue();
                };

                _astarMap.Tiles[node.X, node.Y].SetTileWalked();

                _iteration = node.Iteration + 1;
                x = node.X;
                y = node.Y;
                nodeNr = node.NodeNr;

                // push the dequeued node into the walked stack
                _walkedNodes.Push(node);
            }


            int pathLength = 0;

            //Don't bother backtracking if we didnt reach our goal path
            if (x == _endX && y == _endY)
            {
                node = _walkedNodes.Pop();
                _spareNodes.Push(node);
                while (true)
                {
                    _iteration = node.Iteration;
                    path[node.Iteration - 1].x = node.X;
                    path[node.Iteration - 1].y = node.Y;
                    pathLength++;
                    int parentBranch = node.ParentNodeNr;

                    if (parentBranch == 0)
                        break; // this is the only exit point which will always happen once the parent is the start node
                    do
                    {
                        node = _walkedNodes.Pop();
                        _spareNodes.Push(node);
                    } while (node.NodeNr != parentBranch);
                }
            }

            ClearContainers();

            return pathLength;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void ClearContainers()
        {
            //Clear the remaining queues so we don't leak nodes
            while (_priorityQueue.Count > 0)
                _spareNodes.Push(_priorityQueue.Dequeue());
            while (_walkedNodes.Count() > 0)
                _spareNodes.Push(_walkedNodes.Pop());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int ManhattanDistance(int startX, int startY)
        {
            return Math.Abs(_endX - startX) + Math.Abs(_endY - startY);
        }

        static int DiagonalDistance(int startX, int startY, int endX, int endY)
        {
            return Mathf.Max(Mathf.Abs(endX - startX), Mathf.Abs(endY - startY));
        }
    }
}