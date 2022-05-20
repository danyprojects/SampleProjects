using System;
using UnityEngine;
using System.Collections.Generic;

namespace Bacterio.Common
{
    public sealed class Pathfinder
    {
        [Serializable]
        public struct PathNode
        {
            public Vector2 position;
            public int[] connections;
            [NonSerialized] public bool isUsed;
        }

        public Pathfinder()
        {

        }

        private int _originIndex;
        private int _destinationIndex;

        public List<Vector2> GetPath(Vector2 origin, Vector2 destination, ref PathNode[] nodes, Vector3 nodeOrigin)
        {
            //Reset the relevant node fields that we'll need
            for (int i = 0; i < nodes.Length; i++)
                nodes[i].isUsed = false;

            Vector2 nodeOriginPoint = new Vector2(nodeOrigin.x, nodeOrigin.y);

            //Get the closest node to each of the points
            _originIndex = GetClosestNodeIndex(origin - nodeOriginPoint, ref nodes);
            _destinationIndex = GetClosestNodeIndex(destination - nodeOriginPoint, ref nodes);

            //If points are the same, skip the pathfinding and just move to that one
            if(_originIndex == _destinationIndex)            
                return new List<Vector2>() { nodes[_originIndex].position };

            var path = GetGreedyPath(destination, ref nodes, ref nodeOriginPoint);

            return path;
        }

        //Guaranteed to find a path if it exists, but not guaranteed to be optimal. Complexity is O(n - 1) where n is the number of nodes
        //In the case a path tries to be calculated from inside a box to the outside, it will fail to calculate
        private List<Vector2> GetGreedyPath(Vector2 destination, ref PathNode[] nodes, ref Vector2 origin)
        {
            List<Vector2> path = new List<Vector2>();

            var index = _originIndex;
            while (index != _destinationIndex)
            {
                if (index == -1)
                    return null;

                path.Add(nodes[index].position + origin);

                ref var node = ref nodes[index];
                node.isUsed = true;
                index = -1;

                float smallest = float.MaxValue;
                //greedy search for next closest node
                for (int i = 0; i < node.connections.Length; i++)
                {
                    if (nodes[node.connections[i]].isUsed)
                        continue;

                    var dist = Vector2.Distance(nodes[node.connections[i]].position, destination);
                    if (dist < smallest)
                    {
                        smallest = dist;
                        index = node.connections[i];
                    }
                }
            }

            path.Add(nodes[_destinationIndex].position + origin);
            path.Add(destination);

            return path;
        }

        private int GetClosestNodeIndex(Vector2 point, ref PathNode[] nodes)
        {
            int closest = 0;
            var distance = Vector2.Distance(point, nodes[0].position);
            for (int i = 1; i < nodes.Length; i++)
            {
                var dist = Vector2.Distance(point, nodes[i].position);
                if(dist < distance)
                {
                    distance = dist;
                    closest = i;
                }
            }
            return closest;
        }
    }
}
