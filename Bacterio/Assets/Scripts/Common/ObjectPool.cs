using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bacterio.Common
{ 
    //We restrict it to Component rather than Object so we can access the .gameObject during dispose, while still allowing all component types
    public sealed class ObjectPool<T> where T : Component
    {
        private Stack<T> _objects = new Stack<T>();
        private Action<T> _onPush = null;
        private Action<T> _onPop = null;
        private T _prefab = null;
        private int _growthFactor = 1;
        private Vector3 _initialPosition = Constants.OUT_OF_RANGE_POSITION;
        private Quaternion _initialRotation = Quaternion.identity;
        private Transform _parent = null;

        //OnPush and OnPop are optional callbacks in the case we need to specify extra behaviour every time something is added or taken from the pool
        public ObjectPool(T prefab, int initialSize, int growthFactor, Vector3 initialPosition, Quaternion initialRotation, Transform parent, Action<T> onPush = null, Action<T> onPop = null)
        {
            _prefab = prefab;
            _initialPosition = initialPosition;
            _initialRotation = initialRotation;
            _parent = parent;
            _growthFactor = growthFactor;

            _onPush = onPush;
            _onPop = onPop;

            for (int i = 0; i < initialSize; i++)
            {
                var obj = GameObject.Instantiate(_prefab, _initialPosition, _initialRotation, _parent);
                _onPush?.Invoke(obj);
                _objects.Push(obj);
            }
        }

        public T Pop()
        {
            T obj;
            if (_objects.Count == 0)
            {
                //instantiate the configured amount
                for (int i = 0; i < _growthFactor; i++)
                {
                    obj = GameObject.Instantiate(_prefab, _initialPosition, _initialRotation, _parent);
                    _onPush?.Invoke(obj);
                    _objects.Push(obj);
                }
            }

            obj = _objects.Pop();
            _onPop?.Invoke(obj);
            return obj;
        }

        public void Push(T obj)
        {
            _onPush?.Invoke(obj);
            _objects.Push(obj);
        }

        public void Dispose()
        {
            while(_objects.Count > 0)
            {
                var obj = _objects.Pop();
                GameObject.Destroy(obj.gameObject);
            }
        }
    }
}
