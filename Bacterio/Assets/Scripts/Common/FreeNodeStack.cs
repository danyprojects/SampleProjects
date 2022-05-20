using System;

namespace Bacterio.Common
{
    public sealed class FreeNodesStack
    {
        private int[] _freeNodes = null;
        private int _firstFree = 0;

        private int _currentCapacity = 0, _capacityIncrement = 0;


        public int Used { get; private set; }

        private Action<int> _onResize = null;

        public FreeNodesStack(int initialCapacity, Action<int> onResize = null)
        {
            _freeNodes = new int[initialCapacity];
            _currentCapacity = initialCapacity;
            _capacityIncrement = initialCapacity;

            _onResize = onResize;

            for (int i = 0; i < initialCapacity - 1; i++)
                _freeNodes[i] = i + 1;
            _freeNodes[initialCapacity - 1] = -1;
            _firstFree = 0;
        }

        public int GetFreeIndex()
        {
            //no elements left
            if (_freeNodes[_firstFree] == -1)
                Resize();

            int index = _firstFree;
            _firstFree = _freeNodes[_firstFree];

            Used++;

            return index;
        }

        public void SetFreeIndex(int index)
        {
            _freeNodes[index] = _firstFree;
            _firstFree = index;

            Used--;
        }

        private void Resize()
        {
            _currentCapacity += _capacityIncrement;

            //Resize priority queue
            _onResize?.Invoke(_currentCapacity);

            //Resize free indexes
            int[] newFreeIndexes = new int[_currentCapacity + 1];
            Array.Copy(_freeNodes, newFreeIndexes, Used);
            _freeNodes = newFreeIndexes;

            //init the new array for free indexes
            for (int i = Used; i < _currentCapacity; i++)
                _freeNodes[i] = i + 1;
            _freeNodes[_currentCapacity] = -1;

            _firstFree = Used;
        }
    }
}
