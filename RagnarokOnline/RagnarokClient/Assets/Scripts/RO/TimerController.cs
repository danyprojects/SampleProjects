using Algorithms;
using RO.Common;
using System;

namespace RO
{
    public static class TimerController
    {
        public sealed class Dispatcher
        {
            public void Dispatch()
            {
                TimerController.Dispatch();
            }

            public void ClearOnSceneChange()
            {
                TimerController.ClearOnSceneChange();
            }
        }

        public enum Queues : int
        {
            ClearOnSceneLoad = 0,
            DontClearOnSceneLoad,

            Last
        }

        private sealed class Queue
        {
            private const int DEFAULT_TIMER_AMOUNT = 100;

            private Action[] _timerCallbacks = new Action[DEFAULT_TIMER_AMOUNT];
            private TimerPriorityQueue _timerQueue = new TimerPriorityQueue(DEFAULT_TIMER_AMOUNT);

            private FreeNodesStack _freeNodeStack = null;

            public Queue()
            {
                _freeNodeStack = new FreeNodesStack(DEFAULT_TIMER_AMOUNT, Resize);
            }

            /// <param name="duration">In seconds</param>
            public void Push(double duration, Action callback)
            {
                int index = _freeNodeStack.GetFreeIndex();

                _timerCallbacks[index] = callback;
                _timerQueue.Enqueue(duration + Globals.Time, index);
            }

            public void Dispatch()
            {
                while (_freeNodeStack.Used > 0 && _timerQueue.First <= Globals.Time)
                {
                    var index = _timerQueue.Dequeue();

                    _timerCallbacks[index]();
                    Remove(index);
                }
            }

            public void DispatchAll()
            {
                while (_freeNodeStack.Used > 0)
                {
                    var index = _timerQueue.Dequeue();

                    _timerCallbacks[index]();
                    Remove(index);
                }
            }

            private void Remove(int index)
            {
                _timerCallbacks[index] = null;
                _freeNodeStack.SetFreeIndex(index);
            }

            private void Resize(int newCapacity)
            {
                //Resize priority queue
                _timerQueue.Resize(newCapacity);

                //resize callbacks
                Action[] newCallbacks = new Action[newCapacity];
                Array.Copy(_timerCallbacks, newCallbacks, _freeNodeStack.Used);
                _timerCallbacks = newCallbacks;
            }
        }

        private static Queue[] _queues = new Queue[(int)Queues.Last] { new Queue(), new Queue() };

        /// <summary>
        /// Only push timers that are safe to call across scene changes
        /// </summary>
        public static void PushPersistent(double duration, Action callback)
        {
            _queues[(int)Queues.DontClearOnSceneLoad].Push(duration, callback);
        }

        /// <summary>
        /// Push timers that are not safe to call across scene changes
        /// </summary>
        public static void PushNonPersistent(double duration, Action callback)
        {
            _queues[(int)Queues.ClearOnSceneLoad].Push(duration, callback);
        }

        private static void Dispatch()
        {
            for (int i = 0; i < (int)Queues.Last; i++)
                _queues[i].Dispatch();
        }

        private static void ClearOnSceneChange()
        {
            _queues[(int)Queues.ClearOnSceneLoad].DispatchAll();
        }

        //Priority queue for timers
        private class TimerPriorityQueue
        {
            private struct TimerQueueNode
            {
                public double Priority;
                public int QueueIndex;

                //Data
                public int TimerIndex;
            }

            private TimerQueueNode[] _nodes;

            private int _numNodes;

            public TimerPriorityQueue(int maxNodes)
            {
                _numNodes = 0;
                _nodes = new TimerQueueNode[maxNodes + 1];
            }

            public void Clear()
            {
                Array.Clear(_nodes, 1, _numNodes);
                _numNodes = 0;
            }

            public void Enqueue(double priority, int timerIndex)
            {
                _numNodes++;
                _nodes[_numNodes].Priority = priority;
                _nodes[_numNodes].TimerIndex = timerIndex;
                _nodes[_numNodes].QueueIndex = _numNodes;
                CascadeUp(_nodes[_numNodes]);
            }

            private void CascadeUp(TimerQueueNode node)
            {
                //aka Heapify-up
                int parent;
                if (node.QueueIndex > 1)
                {
                    parent = node.QueueIndex >> 1;

                    if (HasHigherOrEqualPriority(_nodes[parent], node))
                        return;

                    //Node has lower priority value, so move parent down the heap to make room
                    _nodes[node.QueueIndex] = _nodes[parent];
                    _nodes[parent].QueueIndex = node.QueueIndex;

                    node.QueueIndex = parent;
                }
                else
                {
                    return;
                }
                while (parent > 1)
                {
                    parent >>= 1;

                    if (HasHigherOrEqualPriority(_nodes[parent], node))
                        break;

                    //Node has lower priority value, so move parent down the heap to make room
                    _nodes[node.QueueIndex] = _nodes[parent];
                    _nodes[parent].QueueIndex = node.QueueIndex;

                    node.QueueIndex = parent;
                }
                _nodes[node.QueueIndex] = node;
            }

            private void CascadeDown(TimerQueueNode node)
            {
                //aka Heapify-down
                int finalQueueIndex = node.QueueIndex;
                int childLeftIndex = 2 * finalQueueIndex;

                // If leaf node, we're done
                if (childLeftIndex > _numNodes)
                {
                    return;
                }

                // Check if the left-child is higher-priority than the current node
                int childRightIndex = childLeftIndex + 1;
                if (HasHigherPriority(_nodes[childLeftIndex], node))
                {
                    // Check if there is a right child. If not, swap and finish.
                    if (childRightIndex > _numNodes)
                    {
                        node.QueueIndex = childLeftIndex;
                        _nodes[childLeftIndex].QueueIndex = finalQueueIndex;
                        _nodes[finalQueueIndex] = _nodes[childLeftIndex];
                        _nodes[childLeftIndex] = node;
                        return;
                    }
                    // Check if the left-child is higher-priority than the right-child
                    if (HasHigherPriority(_nodes[childLeftIndex], _nodes[childRightIndex]))
                    {
                        // left is highest, move it up and continue
                        _nodes[childLeftIndex].QueueIndex = finalQueueIndex;
                        _nodes[finalQueueIndex] = _nodes[childLeftIndex];
                        finalQueueIndex = childLeftIndex;
                    }
                    else
                    {
                        // right is even higher, move it up and continue
                        _nodes[childRightIndex].QueueIndex = finalQueueIndex;
                        _nodes[finalQueueIndex] = _nodes[childRightIndex];
                        finalQueueIndex = childRightIndex;
                    }
                }
                // Not swapping with left-child, does right-child exist?
                else if (childRightIndex > _numNodes)
                {
                    return;
                }
                else
                {
                    // Check if the right-child is higher-priority than the current node
                    if (HasHigherPriority(_nodes[childRightIndex], node))
                    {
                        _nodes[childRightIndex].QueueIndex = finalQueueIndex;
                        _nodes[finalQueueIndex] = _nodes[childRightIndex];
                        finalQueueIndex = childRightIndex;
                    }
                    // Neither child is higher-priority than current, so finish and stop.
                    else
                    {
                        return;
                    }
                }

                while (true)
                {
                    childLeftIndex = 2 * finalQueueIndex;

                    // If leaf node, we're done
                    if (childLeftIndex > _numNodes)
                    {
                        node.QueueIndex = finalQueueIndex;
                        _nodes[finalQueueIndex] = node;
                        break;
                    }

                    // Check if the left-child is higher-priority than the current node
                    childRightIndex = childLeftIndex + 1;

                    if (HasHigherPriority(_nodes[childLeftIndex], node))
                    {
                        // Check if there is a right child. If not, swap and finish.
                        if (childRightIndex > _numNodes)
                        {
                            node.QueueIndex = childLeftIndex;
                            _nodes[childLeftIndex].QueueIndex = finalQueueIndex;
                            _nodes[finalQueueIndex] = _nodes[childLeftIndex];
                            _nodes[childLeftIndex] = node;
                            break;
                        }
                        // Check if the left-child is higher-priority than the right-child
                        if (HasHigherPriority(_nodes[childLeftIndex], _nodes[childRightIndex]))
                        {
                            // left is highest, move it up and continue
                            _nodes[childLeftIndex].QueueIndex = finalQueueIndex;
                            _nodes[finalQueueIndex] = _nodes[childLeftIndex];
                            finalQueueIndex = childLeftIndex;
                        }
                        else
                        {
                            // right is even higher, move it up and continue
                            _nodes[childRightIndex].QueueIndex = finalQueueIndex;
                            _nodes[finalQueueIndex] = _nodes[childRightIndex];
                            finalQueueIndex = childRightIndex;
                        }
                    }
                    // Not swapping with left-child, does right-child exist?
                    else if (childRightIndex > _numNodes)
                    {
                        node.QueueIndex = finalQueueIndex;
                        _nodes[finalQueueIndex] = node;
                        break;
                    }
                    else
                    {
                        // Check if the right-child is higher-priority than the current node
                        if (HasHigherPriority(_nodes[childRightIndex], node))
                        {
                            _nodes[childRightIndex].QueueIndex = finalQueueIndex;
                            _nodes[finalQueueIndex] = _nodes[childRightIndex];
                            finalQueueIndex = childRightIndex;
                        }
                        // Neither child is higher-priority than current, so finish and stop.
                        else
                        {
                            node.QueueIndex = finalQueueIndex;
                            _nodes[finalQueueIndex] = node;
                            break;
                        }
                    }
                }
            }

            private bool HasHigherPriority(in TimerQueueNode higher, in TimerQueueNode lower)
            {
                return (higher.Priority < lower.Priority);
            }

            private bool HasHigherOrEqualPriority(in TimerQueueNode higher, in TimerQueueNode lower)
            {
                return (higher.Priority <= lower.Priority);
            }

            public int Dequeue()
            {

                TimerQueueNode returnMe = _nodes[1];
                //If the node is already the last node, we can remove it immediately
                if (_numNodes == 1)
                {
                    // _nodes[1] = null;
                    _numNodes = 0;
                    return returnMe.TimerIndex;
                }

                //Swap the node with the last node
                TimerQueueNode formerLastNode = _nodes[_numNodes];
                _nodes[1] = formerLastNode;
                formerLastNode.QueueIndex = 1;
                //_nodes[_numNodes] = null;
                _numNodes--;

                //Now bubble formerLastNode (which is no longer the last node) down
                CascadeDown(formerLastNode);
                return returnMe.TimerIndex;
            }

            public void Resize(int maxNodes)
            {
                TimerQueueNode[] newArray = new TimerQueueNode[maxNodes + 1];
                int highestIndexToCopy = Math.Min(maxNodes, _numNodes);
                Array.Copy(_nodes, newArray, highestIndexToCopy + 1);
                _nodes = newArray;
            }

            public double First
            {
                get
                {

                    return _nodes[1].Priority;
                }
            }

        }
    }
}