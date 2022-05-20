using RO.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RO.Media
{
    public static class FloatingTextController
    {
        //Create an instance of this from controllers who are supposed to call the update
        public sealed class Updater
        {
            public void ClearFloatingTexts()
            {
                FloatingTextController.ClearFloatingTexts();
            }
            public void UpdateFloatingTexts()
            {
                FloatingTextController.UpdateFloatingTexts();
            }
        }

        private struct FloatingTextData
        {
            public FloatingTextAnimator floatingTextAnimator;
            public float endTime;
        }

        private const float REMOVE_TIME = 5;

        private static short _depth = short.MinValue;

        private static List<FloatingTextData> _floatingTextAnimators = null;

        public static void PlayStackingNumber(Transform transform, uint number, int nHits)
        {
            const float STACKING_NUMBER_INTERVAL = 0.2f;

            var animator = GetFloatingTextAnimator(REMOVE_TIME);

            animator.transform.SetParent(transform, false);

            //calculate each hit damage
            uint numberDiv = number / (uint)nHits;
            short depth = _depth;

            //Create the action to display number animation
            Action<int> animate = (int frame) =>
            {
                uint num = numberDiv * (uint)frame;

                if (num > MediaConstants.MAX_NUMBER_DISPLAYED)
                    num = MediaConstants.MAX_NUMBER_DISPLAYED;

                animator.AnimateStackingNumber(num, depth);
            };

            //animate first frame
            animate(1);

            // queue the remaining hit animations
            for (int i = 1; i < nHits - 1; i++)
            {
                // because of mono screwing up actions in loops we need to copy frame...
                int frame = i;
                TimerController.PushNonPersistent(STACKING_NUMBER_INTERVAL * frame, () => animate(frame + 1));
            }
        }

        public static void PlayMiss(Transform transform, FloatingTextColor color = FloatingTextColor.White)
        {
            var animator = GetFloatingTextAnimator(REMOVE_TIME);

            animator.transform.SetParent(transform, false);

            animator.AnimateMiss(color, _depth);

        }

        public static void PlayRegularDamage(Transform transform, uint number, FloatingTextColor color = FloatingTextColor.White)
        {
            var animator = GetFloatingTextAnimator(REMOVE_TIME);

            animator.transform.SetParent(transform, false);

            if (number > MediaConstants.MAX_NUMBER_DISPLAYED)
                number = MediaConstants.MAX_NUMBER_DISPLAYED;

            animator.AnimateFloatingNumber(number, color, _depth);
        }

        public static void PlayCritDamage(Transform transform, uint number)
        {
            var animator = GetFloatingTextAnimator(REMOVE_TIME);

            animator.transform.SetParent(transform, false);

            if (number > MediaConstants.MAX_NUMBER_DISPLAYED)
                number = MediaConstants.MAX_NUMBER_DISPLAYED;

            animator.AnimateCriticalNumber(number, _depth);
        }

        public static void PlayHeal(Transform transform, uint number)
        {
            var animator = GetFloatingTextAnimator(REMOVE_TIME);

            animator.transform.SetParent(transform, false);

            if (number > MediaConstants.MAX_NUMBER_DISPLAYED)
                number = MediaConstants.MAX_NUMBER_DISPLAYED;

            animator.AnimateHeal(number, _depth);
        }

        public static void PlayLucky(Transform transform)
        {
            var animator = GetFloatingTextAnimator(REMOVE_TIME);

            animator.transform.SetParent(transform, false);

            animator.AnimateLucky(_depth);
        }

        static FloatingTextController()
        {
            _floatingTextAnimators = new List<FloatingTextData>(MediaConstants.DEFAULT_FLOATING_TEXT_COUNT);
        }

        private static void ClearFloatingTexts()
        {
            for (int i = _floatingTextAnimators.Count - 1; i >= 0; i--)
            {
                ObjectPoll.FloatingTextPoll = _floatingTextAnimators[i].floatingTextAnimator.gameObject;
                _floatingTextAnimators.RemoveAt(i);
            }
        }

        private static void UpdateFloatingTexts()
        {
            //Iterate from the end so we can remove safely
            for (int i = _floatingTextAnimators.Count - 1; i >= 0; i--)
            {
                if (Globals.Time >= _floatingTextAnimators[i].endTime)
                {
                    ObjectPoll.FloatingTextPoll = _floatingTextAnimators[i].floatingTextAnimator.gameObject;
                    _floatingTextAnimators.RemoveAt(i);
                }
                else
                    _floatingTextAnimators[i].floatingTextAnimator.UpdateAnimation();
            }

            //If we get a chance, reset the counter so we wont see a graphical issue
            if (_floatingTextAnimators.Count == 0)
                _depth = short.MinValue;
        }

        private static FloatingTextAnimator GetFloatingTextAnimator(float removeTimeOffset)
        {
            GameObject obj = ObjectPoll.FloatingTextPoll;
            FloatingTextAnimator animator = obj.GetComponent<FloatingTextAnimator>();

            _floatingTextAnimators.Add(new FloatingTextData()
            {
                floatingTextAnimator = animator,
                endTime = Globals.Time + removeTimeOffset
            });

            //This is used to make floating text show in the right order
            //do it here so we don't forget to increment it
            _depth += 2;

            return animator;
        }
    }
}
