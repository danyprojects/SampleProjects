using System;
using System.Collections.Generic;
using UnityEngine;

namespace RO.UI
{
    public static class ExecuteEvents
    {
        private static readonly Stack<List<object>> _handlerListPool = new Stack<List<object>>();
        private static readonly Stack<List<Component>> _compListPool = new Stack<List<Component>>();
        // Execute the specified event on the first game object underneath the current touch.
        private static readonly List<Transform> s_InternalTransformList = new List<Transform>(30);

        public static bool Execute<T, C>(GameObject target, C context, Action<T, C> functor)
        {
            var internalHandlers = _handlerListPool.Count > 0 ? _handlerListPool.Pop() : new List<object>();

            GetEventList<T>(target, internalHandlers);

            for (var i = 0; i < internalHandlers.Count; i++)
            {
                T arg;
                try
                {
                    arg = (T)internalHandlers[i];
                }
                catch (Exception e)
                {
                    var temp = internalHandlers[i];
                    Debug.LogException(new Exception(string.Format("Type {0} expected {1} received.", typeof(T).Name, temp.GetType().Name), e));
                    continue;
                }

                try
                {
                    functor(arg, context);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            var handlerCount = internalHandlers.Count;
            internalHandlers.Clear();
            _handlerListPool.Push(internalHandlers);

            return handlerCount > 0;
        }

        public static GameObject ExecuteHierarchy<T, C>(GameObject root, C context, Action<T, C> callbackFunction)
        {
            GetEventChain(root, s_InternalTransformList);

            for (var i = 0; i < s_InternalTransformList.Count; i++)
            {
                var transform = s_InternalTransformList[i];
                if (Execute(transform.gameObject, context, callbackFunction))
                    return transform.gameObject;
            }
            return null;
        }

        // Whether the specified game object will be able to handle the specified event.
        public static bool CanHandleEvent<T>(GameObject go)
        {
            var internalHandlers = _handlerListPool.Count > 0 ? _handlerListPool.Pop() : new List<object>();

            GetEventList<T>(go, internalHandlers);
            var handlerCount = internalHandlers.Count;
            internalHandlers.Clear();
            _handlerListPool.Push(internalHandlers);

            return handlerCount != 0;
        }

        // Bubble the specified event on the game object, figuring out which object will actually receive the event.
        public static GameObject GetEventHandler<T>(GameObject root)
        {
            if (root == null)
                return null;

            Transform t = root.transform;
            while (t != null)
            {
                if (CanHandleEvent<T>(t.gameObject))
                    return t.gameObject;
                t = t.parent;
            }
            return null;
        }

        private static bool ShouldSendToComponent<T>(Component component)
        {
            var valid = component is T;
            if (!valid)
                return false;

            var behaviour = component as Behaviour;
            if (behaviour != null)
                return behaviour.isActiveAndEnabled;
            return true;
        }

        // Get the specified object's event event.
        private static void GetEventList<T>(GameObject go, IList<object> results)
        {
            if (results == null)
                throw new ArgumentException("Results array is null", "results");

            if (go == null || !go.activeInHierarchy)
                return;

            var components = _compListPool.Count > 0 ? _compListPool.Pop() : new List<Component>();
            go.GetComponents(components);

            for (var i = 0; i < components.Count; i++)
            {
                if (!ShouldSendToComponent<T>(components[i]))
                    continue;
                results.Add(components[i]);
            }

            components.Clear();
            _compListPool.Push(components);
        }

        private static void GetEventChain(GameObject root, IList<Transform> eventChain)
        {
            eventChain.Clear();
            if (root == null)
                return;

            var t = root.transform;
            while (t != null)
            {
                eventChain.Add(t);
                t = t.parent;
            }
        }
    }
}
