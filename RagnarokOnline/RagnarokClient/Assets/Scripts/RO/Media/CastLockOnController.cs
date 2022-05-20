using System.Collections.Generic;
using UnityEngine;

namespace RO.Media
{
    using RemoveEffectCb = System.Action<EffectCancelToken>;

    public static class CastLockOnController
    {
        private sealed class CancelToken : EffectCancelToken
        {
            public CancelToken(short index, int token)
                : base(EffectType.CastLockOn, index, token)
            {
            }

            public override void Cancel()
            {
                CastLockOnController.CancelCastLockOn(index, token);
            }
        }

        private class CastLockOn
        {
            public CastLockOn(CastLockOnAnimator animator, RemoveEffectCb removeEffectCb)
            {
                this.removeEffectCb = removeEffectCb;
                castLockOnAnimator = animator;
            }

            public void Clear()
            {
                ObjectPoll.CastLockOnPoll = castLockOnAnimator.gameObject;
                castLockOnAnimator = null;
                cancelToken = null;
                removeEffectCb = null;
            }

            public CancelToken cancelToken;
            public CastLockOnAnimator castLockOnAnimator;
            public RemoveEffectCb removeEffectCb;
        }

        private static Stack<short> _freeIndexes = new Stack<short>(MediaConstants.DEFAULT_CAST_LOCK_ON_COUNT);
        private static List<CastLockOn> _castLockOn = new List<CastLockOn>(MediaConstants.DEFAULT_CAST_LOCK_ON_COUNT);

        private const int START_TOKEN = 1;
        private static int _nextToken = START_TOKEN;


        public static EffectCancelToken CreateCastLockOn(Transform target, double duration, RemoveEffectCb removeEffectCb)
        {
            var castLockOn = ObjectPoll.CastLockOnPoll.GetComponent<CastLockOnAnimator>();
            castLockOn.Animate();

            castLockOn.transform.SetParent(target, false);

            short index;
            if (_freeIndexes.Count > 0)
            {
                index = _freeIndexes.Pop();
                _castLockOn[index] = new CastLockOn(castLockOn, removeEffectCb);
            }
            else
            {
                index = (short)_castLockOn.Count;
                _castLockOn.Add(new CastLockOn(castLockOn, removeEffectCb));
            }

            CancelToken token = new CancelToken(index, IncrementAndGetNextToken());
            _castLockOn[index].cancelToken = token;

            TimerController.PushNonPersistent(duration, () =>
            {
                _castLockOn[index].removeEffectCb?.Invoke(_castLockOn[index].cancelToken);
                _castLockOn[index].Clear();
                _freeIndexes.Push(index);
            });

            return token;
        }

        private static void CancelCastLockOn(short index, int token)
        {
            if (token <= START_TOKEN - 1 || !_castLockOn[index].cancelToken.CheckToken(token))
                return;

            //Simply disable it visually, let the timer destroy it later
            _castLockOn[index].castLockOnAnimator.enabled = false;
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
