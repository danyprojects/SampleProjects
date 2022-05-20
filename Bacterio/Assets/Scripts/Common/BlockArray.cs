using Bacterio.MapObjects;

namespace Bacterio.Common
{
    public sealed class BlockArray<T> where T : Block
    {
        public int LastIndex { get; private set; } = -1;
        public int Count { get; private set; } = 0;

        private T[] _dataArray = null;
        private int _growthAmount = 0;

        private short _nextTag = 0;

        public BlockArray(int initialCapacity, int growthAmount)
        {
            _growthAmount = growthAmount;
            _dataArray = new T[initialCapacity];
            _nextTag = 0;
        }

        /// <summary>Generates a unique ID and adds a T to array. Should only be used by the owner of T</summary>
        public void Add(T data)
        {
            //If last index is equal to count, that means there's no holes
            if (LastIndex == Count - 1)
            {
                //If count is equal to array length, then we're full and need more space
                if (Count == _dataArray.Length)
                    EnlargeArray();

                //In either case, Count is the index we want to insert at
                data._uniqueId = new Block.UniqueId((short)Count, _nextTag++);
                _dataArray[Count] = data;
                LastIndex = Count;
                Count++;
                return;
            }

            //Otherwise we have holes and should fill them
            for (int i = 0; i < LastIndex; i++)
            {
                //Find a free slot to insert and return
                if (_dataArray[i] == null)
                {
                    data._uniqueId = new Block.UniqueId((short)i, _nextTag++);
                    _dataArray[i] = data;
                    Count++;
                    return;
                }
            }
        }

        /// <summary>Inserts a T with an already generated unique ID. Should only be used by clients</summary>
        public void Insert(T data, Block.UniqueId uniqueId)
        {
            //Check if we need to enlarge the array. In extreme lag conditions we might have to enlarge it multiple times
            while(uniqueId.arrayIndex >= _dataArray.Length)
                EnlargeArray();

            WDebug.Assert(_dataArray[uniqueId.arrayIndex] == null, "Attempted to insert data into a non empty slot");

            data._uniqueId = uniqueId;
            _dataArray[data._uniqueId.arrayIndex] = data;
            if (data._uniqueId.arrayIndex > LastIndex)
                LastIndex = data._uniqueId.arrayIndex;

            Count++;
        }

        /// <summary>Removes a T with an already generated unique ID if tags match. Can be used by clients and host</summary>
        public void Remove(T data)
        {
            WDebug.Assert(_dataArray[data._uniqueId.arrayIndex] != null, "Attempted to remove data from an empty slot");
            WDebug.Assert(data._uniqueId.arrayIndex < _dataArray.Length, "Attempted to remove an index larger than array");

            //Only remove if tag matches
            if (_dataArray[data._uniqueId.arrayIndex]._uniqueId.tag != data._uniqueId.tag)
                return;

            _dataArray[data._uniqueId.arrayIndex] = null;
            Count--;

            //Update last index. Does nothing if Last index is still valid
            for (; LastIndex >= 0; LastIndex--)
                if (_dataArray[LastIndex] != null)
                    break;
        }

        /// <summary>Gets a T from a unique ID. Returns null if tags don't match</summary>
        public T GetFromId(Block.UniqueId uniqueId)
        {
            WDebug.Assert(_dataArray[uniqueId.arrayIndex] != null, "Attempted to get data from an empty slot");
            WDebug.Assert(uniqueId.arrayIndex < _dataArray.Length, "Attempted to get an index larger than array");

            T data = _dataArray[uniqueId.arrayIndex];

            return data._uniqueId.tag == uniqueId.tag ? data : null;
        }

        /// <summary>Checks if there is an object with the exact unique ID in the array</summary>
        public bool Contains(Block.UniqueId uniqueId)
        {
            if (_dataArray.Length <= uniqueId.arrayIndex || _dataArray[uniqueId.arrayIndex] == null)
                return false;

            return _dataArray[uniqueId.arrayIndex]._uniqueId.tag == uniqueId.tag;
        }

        //This is meant to be used for when quick iteration is required in the array. LastIndex should be used as the stopping condition when iterating
        public ref T[] GetAll()
        {
            return ref _dataArray;
        }

        //Destroys all objects in the array
        public void Dispose()
        {
            for (int i = 0; i <= LastIndex; i++)
                UnityEngine.Object.Destroy(_dataArray[i]?.gameObject);

            //invalidate data
            LastIndex = -1;
            Count = 0;
            _dataArray = new T[0];
        }

        private void EnlargeArray()
        {
            WDebug.Log("Enlarging array. Old limit: " + _dataArray.Length + ", New limit: " + (_dataArray.Length + _growthAmount));
            T[] newArray = new T[_dataArray.Length + _growthAmount];
            System.Array.Copy(_dataArray, newArray, LastIndex + 1);
            _dataArray = newArray;
        }
    }
}
