using System.Runtime.CompilerServices;

namespace Algorithms
{
    //Custom stack to use an array instead of a list. Less cache misses this way     
    public sealed partial class Pathfinder
    {
        private sealed partial class AstarStack
        {
            AstarPathNode[] nodes = new AstarPathNode[RO.Common.Constants.MAX_NODES];
            int current = 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public AstarPathNode Pop()
            {
                return nodes[--current];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Push(AstarPathNode node)
            {
                nodes[current++] = node;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public int Count()
            {
                return current;
            }

            public void Reset(bool max = false)
            {
                if (max)
                    current = RO.Common.Constants.MAX_NODES;
                else
                    current = 0;
            }
        }
    }
}
