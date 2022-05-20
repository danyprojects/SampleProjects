using Algorithms;
using NUnit.Framework;
using RO.Containers;
using System;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

namespace Tests
{
    class AstarTest : MonoBehaviour
    {
        MapData _mapData;

        public void InitAstar()
        {
            Type mapDataType = typeof(MapData);
            _mapData = new MapData();
            mapDataType.GetField("_width", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_mapData, 50);
            mapDataType.GetField("_height", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_mapData, 50);
            mapDataType.GetField("_tiles", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_mapData, new MapData.Tile[50 * 50]);

            for (int i = 0; i < 50 * 50; i++)
                _mapData.Tiles[i].IsWalkable = true;

            Pathfinder.SetMap(_mapData);
        }

        [Test]
        public void AstarPathingTest()
        {
            InitAstar();

            Vector2Int[] path = new Vector2Int[RO.Common.Constants.MAX_WALK];
            int pathLength;
            Vector2Int start = new Vector2Int(1, 1);
            Vector2Int end = new Vector2Int(3, 3);

            //Test the overloads
            pathLength = Pathfinder.FindPath(start.x, start.y, end.x, end.y, ref path);
            Assert.IsTrue(pathLength == 2);
            pathLength = Pathfinder.FindPath(start.x, start.y, ref end, ref path);
            Assert.IsTrue(pathLength == 2);
            pathLength = Pathfinder.FindPath(ref start, end.x, end.y, ref path);
            Assert.IsTrue(pathLength == 2);
            pathLength = Pathfinder.FindPath(ref start, ref end, ref path);
            Assert.IsTrue(pathLength == 2);

            //Test impossible moves
            pathLength = Pathfinder.FindPath(1, 1, 1, 1, ref path);
            Assert.IsTrue(pathLength == 0);
            pathLength = Pathfinder.FindPath(1, 1, 17, 1, ref path);
            Assert.IsTrue(pathLength == 0);
            pathLength = Pathfinder.FindPath(1, 1, 1, 17, ref path);
            Assert.IsTrue(pathLength == 0);
            pathLength = Pathfinder.FindPath(1, 1, 16, 17, ref path);
            Assert.IsTrue(pathLength == 0);

            //Test limits
            pathLength = Pathfinder.FindPath(1, 1, 16, 1, ref path);
            Assert.IsTrue(pathLength == 15);
            pathLength = Pathfinder.FindPath(1, 1, 1, 16, ref path);
            Assert.IsTrue(pathLength == 15);
            pathLength = Pathfinder.FindPath(1, 1, 16, 16, ref path);
            Assert.IsTrue(pathLength == 15);
        }

        [Test]
        public void AstarPerformanceTest()
        {
            InitAstar();

            //Start variables and set path to max possible path
            Vector2Int[] path = new Vector2Int[RO.Common.Constants.MAX_WALK];
            int pathLength;
            Vector2Int start = new Vector2Int(1, 1);
            Vector2Int end = new Vector2Int(5, 15);

            Stopwatch stopwatch = new Stopwatch();
            int tries = 500000;

            stopwatch.Restart();
            for (int i = 0; i < tries; i++)
                pathLength = Pathfinder.FindPath(ref start, ref end, ref path);
            stopwatch.Stop();
            UnityEngine.Debug.Log("Elapsed seconds " + stopwatch.ElapsedMilliseconds / 1000);
            UnityEngine.Debug.Log("Average Astars per second: " + (tries / (stopwatch.ElapsedMilliseconds / 1000f)));
        }
    }
}
