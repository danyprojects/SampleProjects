using RO.Common;
using System.Collections.Generic;
using UnityEngine;

namespace RO.Media
{
    using RemoveEffectCb = System.Action<EffectCancelToken>;
    public static class FillBarController
    {
        private class CancelToken : EffectCancelToken
        {
            public CancelToken(short index, int token)
                : base(EffectType.FillBar, index, token)
            {
            }

            public override void Cancel()
            {
                FillBarController.CancelFillBar(index, token);
            }
        }

        private class FillBar
        {
            public FillBar(Renderer fillBar, RemoveEffectCb removeEffectCb)
            {
                this.removeEffectCb = removeEffectCb;
                this.fillBar = fillBar;
            }

            public void Clear()
            {
                ObjectPoll.FillBarPool = fillBar.gameObject;
                fillBar = null;
                cancelToken = null;
                removeEffectCb = null;
            }

            public CancelToken cancelToken;
            public Renderer fillBar;
            RemoveEffectCb removeEffectCb;
        }

        private static MaterialPropertyBlock _propertyBlock = new MaterialPropertyBlock();
        private static Vector3 _castBarOffset = new Vector3(0, 15, 0);

        private static Stack<short> _freeIndexes = new Stack<short>(MediaConstants.DEFAULT_FLOATING_BAR_COUNT);
        private static List<FillBar> _fillBars = new List<FillBar>(MediaConstants.DEFAULT_FLOATING_BAR_COUNT);

        private const int START_TOKEN = 1; // This way we have to do nothing to classes that run default init as the token will default 0 in the struct
        private static int _nextToken = START_TOKEN;

        public static EffectCancelToken StartCastBar(Transform target, float castTime, RemoveEffectCb removeEffectCb)
        {
            var fillBarObj = ObjectPoll.FillBarPool;
            var rend = fillBarObj.GetComponent<Renderer>();

            //Configure the cast bar
            rend.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(MediaConstants.SHADER_PROGRESS_COLOR_ID, Color.green);
            _propertyBlock.SetVector(MediaConstants.SHADER_START_TIME_ID, new Vector2(Globals.TimeSinceLevelLoad, castTime));
            rend.SetPropertyBlock(_propertyBlock);

            //set it's position
            fillBarObj.transform.SetParent(target, false);
            fillBarObj.transform.localPosition = _castBarOffset;

            short index;
            if (_freeIndexes.Count > 0)
            {
                index = _freeIndexes.Pop();
                _fillBars[index] = new FillBar(rend, removeEffectCb);
            }
            else
            {
                index = (short)_fillBars.Count;
                _fillBars.Add(new FillBar(rend, removeEffectCb));
            }

            CancelToken token = new CancelToken(index, IncrementAndGetNextToken());
            _fillBars[index].cancelToken = token;

            //Make the timer to destroy it
            TimerController.PushNonPersistent(castTime, () =>
                {
                    _fillBars[index].Clear();
                    _freeIndexes.Push(index);
                });

            return token;
        }

        private static void CancelFillBar(short index, int token)
        {
            if (token == START_TOKEN - 1 || !_fillBars[index].cancelToken.CheckToken(token))
                return;

            //Simply disable it visually, let the timer destroy it later
            _fillBars[index].fillBar.enabled = false;
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
