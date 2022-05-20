
namespace Bacterio.Network
{
    //This array can have holes. But it prioritizes filling holes when adding new objects
    //Only the owner of the object type is meant to use the Add method. The clients should only use insert, using the ID the owner sent.
    // This is so that the IDs remain controller over the network. Otherwise it is impossible to maintain.
    public sealed class NetworkArray<T> where T : NetworkArrayObject
    {
        public int LastIndex { get; private set; } = -1;
        public int Count { get; private set; } = 0;

        private T[] _dataArray;
        private int _growthAmount;
        private short _nextTag = 0;

        public NetworkArray(int initialCapacity, int growthAmount)
        {
            _growthAmount = growthAmount;
            _dataArray = new T[initialCapacity];
        }

        /// <summary>Generates a unique ID and adds a T to array. Should only be used by the host</summary>
        public void Add(T data)
        {
            WDebug.Assert(NetworkInfo.IsHost, "Non host tried to add data into a network array");

            //If last index is equal to count, that means there's no holes
            if (LastIndex == Count - 1)
            {
                //If count is equal to array length, then we're full and need more space
                if (Count == _dataArray.Length)
                    EnlargeArray();

                //In either case, Count is the index we want to insert at
                data._uniqueId = new NetworkArrayObject.UniqueId((short)Count, _nextTag++);
                _dataArray[Count] = data;
                LastIndex = Count;
                Count++;
                return;
            }

            //Otherwise we have holes and should fill them
            for (int i = 0; i < LastIndex; i++) 
            {
                //Find a free slot to insert and return
                if(_dataArray[i] == null)
                {
                    data._uniqueId = new NetworkArrayObject.UniqueId((short)i, _nextTag++);
                    _dataArray[i] = data;
                    Count++;
                    return;
                }
            }
        }

        /// <summary>Inserts a T with an already generated unique ID. Should only be used by clients</summary>
        public void Insert(T data)
        {
            WDebug.Assert(!NetworkInfo.IsHost, "Host tried to insert data into a network array");

            //Check if we need to enlarge the array. In extreme lag conditions, might need to enlarge multiple times
            while(data._uniqueId.index >= _dataArray.Length)
                EnlargeArray();

            WDebug.Assert(_dataArray[data._uniqueId.index] == null, "Attempted to insert data into a non empty slot");

            _dataArray[data._uniqueId.index] = data;
            if (data._uniqueId.index > LastIndex)
                LastIndex = data._uniqueId.index;

            Count++;
        }

        /// <summary>Removes a T with an already generated unique ID if tags match. Can be used by clients and host</summary>
        public void Remove(T data)
        {
            WDebug.Assert(_dataArray[data._uniqueId.index] != null, "Attempted to remove data from an empty slot");
            WDebug.Assert(data._uniqueId.index < _dataArray.Length, "Attempted to remove an index larger than array");

            //Only remove if tag matches
            if (_dataArray[data._uniqueId.index]._uniqueId.tag != data._uniqueId.tag)
                return;

            _dataArray[data._uniqueId.index] = null;
            Count--;

            //Update last index. Does nothing if Last index is still valid
            for (; LastIndex >= 0; LastIndex--)
                if (_dataArray[LastIndex] != null)
                    break;
        }

        /// <summary>Gets a T from a unique ID. Returns null if tags don't match</summary>
        public T GetFromId(NetworkArrayObject.UniqueId uniqueId)
        {
            WDebug.Assert(_dataArray[uniqueId.index] != null, "Attempted to get data from an empty slot");
            WDebug.Assert(uniqueId.index < _dataArray.Length, "Attempted to get an index larger than array");

            T data = _dataArray[uniqueId.index];

            return data._uniqueId.tag == uniqueId.tag ? data : null;
        }

        /// <summary>Checks if there is an object with the exact unique ID in the array</summary>
        public bool Contains(NetworkArrayObject.UniqueId uniqueId)
        {
            if (_dataArray.Length <= uniqueId.index || _dataArray[uniqueId.index] == null)
                return false;

            return _dataArray[uniqueId.index]._uniqueId.tag == uniqueId.tag;
        }

        //This is meant to be used for when quick iteration is required in the array. LastIndex should be used as the stopping condition when iterating
        public ref T[] GetAll()
        {
            return ref _dataArray;
        }

        public void Dispose()
        {
            for (int i = 0; i < LastIndex; i++)
                UnityEngine.Object.Destroy(_dataArray[i]?.gameObject);
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
