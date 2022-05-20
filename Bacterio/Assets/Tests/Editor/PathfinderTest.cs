using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Bacterio.Common;

public class PathfinderTest
{/*
    private List<PathNode> GetGraph()
    {
        List<PathNode> nodes = new List<PathNode>();
        /*      /\
            ___/  \___      
           |          |  
           |_/\____/\_|
         From bottom left, going counter clockwise in indexes.
         */
    /*
        //index 0
        nodes.Add(new PathNode()
        {
            position = new Vector2(0, 0),
            connections = new List<int>() { 1, 12 }
        });

        //index 1
        nodes.Add(new PathNode()
        {
            position = new Vector2(1, 0),
            connections = new List<int>() { 0, 3, 2 }
        });

        //index 2
        nodes.Add(new PathNode()
        {
            position = new Vector2(1.5f, 1),
            connections = new List<int>() { 1, 3 }
        });

        //index 3
        nodes.Add(new PathNode()
        {
            position = new Vector2(2, 0),
            connections = new List<int>() { 2, 4, 1 }
        });

        //index 4
        nodes.Add(new PathNode()
        {
            position = new Vector2(6, 0),
            connections = new List<int>() { 3, 5, 6 }
        });

        //index 5
        nodes.Add(new PathNode()
        {
            position = new Vector2(6.5f, 1),
            connections = new List<int>() { 4, 6 }
        });

        //index 6
        nodes.Add(new PathNode()
        {
            position = new Vector2(7, 0),
            connections = new List<int>() { 5, 7, 4 }
        });

        //index 7
        nodes.Add(new PathNode()
        {
            position = new Vector2(8, 0),
            connections = new List<int>() { 6, 8 }
        });

        //index 8
        nodes.Add(new PathNode()
        {
            position = new Vector2(8, 2),
            connections = new List<int>() { 7, 9, 10 }
        });

        //index 9
        nodes.Add(new PathNode()
        {
            position = new Vector2(5, 2),
            connections = new List<int>() { 8, 10 }
        });

        //index 10
        nodes.Add(new PathNode()
        {
            position = new Vector2(4, 4),
            connections = new List<int>() { 8, 9, 11, 12 }
        });

        //index 11
        nodes.Add(new PathNode()
        {
            position = new Vector2(3, 2),
            connections = new List<int>() { 10, 12 }
        });

        //index 12
        nodes.Add(new PathNode()
        {
            position = new Vector2(0, 2),
            connections = new List<int>() { 11, 0, 10 }
        });

        return nodes;
    }

    [Test]
    public void GreedyPathingTest()
    {
        var nodes = GetGraph();
        Pathfinder pathfinder = new Pathfinder();

     //   var path = pathfinder.GetPath(new Vector2(8, -2), new Vector2(4, 6), nodes);

        foreach (var vec in path)
            Debug.Log(vec);
    }*/
}
