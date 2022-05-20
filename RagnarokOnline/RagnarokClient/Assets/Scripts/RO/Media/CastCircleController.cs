using RO.Common;
using RO.Containers;
using System.Collections.Generic;
using UnityEngine;

namespace RO.Media
{

    using RemoveEffectCb = System.Action<EffectCancelToken>;

    public static class CastCircleController
    {
        public static class Updater
        {
            public static void AssignMap(Map map)
            {
                _map = map;
            }
        }

        private static Map _map;

        private sealed class CancelToken : EffectCancelToken
        {
            public CancelToken(short index, int token)
                : base(EffectType.CastCircle, index, token)
            {
            }

            public override void Cancel()
            {
                CastCircleController.CancelCastCircle(index, token);
            }
        }

        private class CastCircle
        {
            public CastCircle(CastCircleAnimator animator, RemoveEffectCb removeEffectCb)
            {
                castCircleAnimator = animator;
                this.removeEffectCb = removeEffectCb;
            }

            public void Clear()
            {
                ObjectPoll.CastCirclePoll = castCircleAnimator.gameObject;
                castCircleAnimator = null;
                cancelToken = null;
                removeEffectCb = null;
            }

            public CancelToken cancelToken;
            public CastCircleAnimator castCircleAnimator;
            public RemoveEffectCb removeEffectCb;
        }

        private static Stack<short> _freeIndexes = new Stack<short>(MediaConstants.DEFAULT_CAST_CIRCLES_COUNT);
        private static List<CastCircle> _castCircles = new List<CastCircle>(MediaConstants.DEFAULT_CAST_CIRCLES_COUNT);

        private const int START_TOKEN = 1;
        private static int _nextToken = START_TOKEN;

        public static EffectCancelToken CreateCastCircle(ref Vector2Int center, int size, double duration, RemoveEffectCb removeEffectCb)
        {
            string name = ConstStrings.GROUND_MESH_DATA_NAME + ConstStrings.NumberStrings[size];
            GroundProjectedMesh groundProjectedMesh = (GroundProjectedMesh)AssetBundleProvider.LoadMiscBundleAsset<ScriptableObject>(name);
            var castCircle = ObjectPoll.CastCirclePoll.GetComponent<CastCircleAnimator>();
            castCircle.ProjectMesh(groundProjectedMesh, center, _map);

            Utility.GameToWorldCoordinatesCenter(center, out Vector3 coordinates);
            coordinates.y = 0.3f;
            castCircle.transform.position = coordinates;

            short index;
            if (_freeIndexes.Count > 0)
            {
                index = _freeIndexes.Pop();
                _castCircles[index] = new CastCircle(castCircle, removeEffectCb);
            }
            else
            {
                index = (short)_castCircles.Count;
                _castCircles.Add(new CastCircle(castCircle, removeEffectCb));
            }

            CancelToken token = new CancelToken(index, IncrementAndGetNextToken());
            _castCircles[index].cancelToken = token;

            TimerController.PushNonPersistent(duration, () =>
                {
                    _castCircles[index].removeEffectCb?.Invoke(_castCircles[index].cancelToken);
                    _castCircles[index].Clear();
                    _freeIndexes.Push(index);
                });

            return token;
        }

        public static EffectCancelToken CreateCastCircle(int centerX, int centerY, int size, double duration, RemoveEffectCb removeEffectCb)
        {
            Vector2Int center = new Vector2Int(centerX, centerY);
            return CreateCastCircle(ref center, size, duration, removeEffectCb);
        }

        private static void CancelCastCircle(short index, int token)
        {
            if (token <= START_TOKEN - 1 || !_castCircles[index].cancelToken.CheckToken(token))
                return;

            //Simply disable it visually, let the timer destroy it later
            _castCircles[index].castCircleAnimator.enabled = false;
        }

        private static int IncrementAndGetNextToken()
        {
            int token = _nextToken;
            if (_nextToken == int.MaxValue)
                _nextToken = START_TOKEN;
            else
                _nextToken++;
            return token;
        }
    }
}