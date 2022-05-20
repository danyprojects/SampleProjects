using RO.Common;
using RO.Databases;
using UnityEngine;

namespace RO.Media
{
    using RemoveEffectCb = System.Action<EffectCancelToken>;

    public static partial class EffectsAnimatorController
    {
        private sealed class JupitelThunderContext
        {
            public float startTime;
            public float currentExplosions;
            public Transform jtObj;
            public Transform target;
            public bool reachedTarget;
        }

        public static EffectCancelToken PlayJupitelThunder(Transform source, Transform target, RemoveEffectCb removeEffectCb)
        {
            var funcEffect = GetFreeFuncEffect(UpdateJupitelThunder, removeEffectCb);

            //Create the context and assign to func effect
            var context = new JupitelThunderContext();
            funcEffect.context = context;

            //Save the information for the update
            context.startTime = Globals.Time;
            context.jtObj = ObjectPoll.EmptyGameObjectsPoll.transform;
            context.target = target;
            context.reachedTarget = false;
            SoundController.PlayAudioEffect(SoundDb.SoundIds.JupitelThunder, context.jtObj.GetComponent<AudioSource>());

            //Set object to source position but as local of target
            context.jtObj.position = source.position;

            //Create the JT ball
            funcEffect.subEffectTokens.Add(PlayEffect(EffectIDs.JupitelThunderBall, context.jtObj, funcEffect.RemoveEffect));

            return funcEffect.cancelToken;
        }

        private static bool UpdateJupitelThunder(FuncEffect effect)
        {
            const float MOVE_SPEED = 2.4f;
            const float EXPLOSION_INTERVAL = 0.2f;
            const int TOTAL_EXPLOSIONS = 13;
            const float JT_SOUND_DURATION = 2;

            JupitelThunderContext context = (JupitelThunderContext)effect.context;

            //Check if we need to start more explosions and return if no more explosions are running
            if (context.reachedTarget)
            {
                //Start another explosion if needed
                if (context.currentExplosions < TOTAL_EXPLOSIONS && Globals.Time >= context.startTime)
                {
                    effect.subEffectTokens.Add(PlayEffect(EffectIDs.JupitelThunderExplosion, context.target, effect.RemoveEffect));
                    context.currentExplosions++;
                    context.startTime = Globals.Time + EXPLOSION_INTERVAL;
                }

                //Return true to remove effect once all explosions are gone
                return effect.subEffectTokens.Count == 0;
            }

            //Otherwise Move towards target
            context.jtObj.position = Vector3.MoveTowards(context.jtObj.position, context.target.position, MOVE_SPEED * Globals.FrameIncrement);
            //Make sure ball has same rotation has rest of the game
            context.jtObj.eulerAngles = context.target.eulerAngles;

            //If JT reached target.Start the explosion
            if (context.jtObj.position == context.target.position)
            {
                //If ball hasn't finished yet, transfer it to target
                if (effect.subEffectTokens.Count != 0)
                {
                    Transform child = context.jtObj.GetChild(0);
                    child.SetParent(context.target, false);
                    child.localPosition = Vector3.zero;
                }

                //If sound has finished, object is no longer needed so put it back to the pool. Otherwise set a timer to remove it once sound ends
                //It's safe to remove the object when timer dispatches since nothing else will use it until we free it
                if (Globals.Time >= context.startTime + JT_SOUND_DURATION)
                    ObjectPoll.EmptyGameObjectsPoll = context.jtObj.gameObject;
                else
                    TimerController.PushNonPersistent(context.startTime + JT_SOUND_DURATION - Globals.Time, () => ObjectPoll.EmptyGameObjectsPoll = context.jtObj.gameObject);

                //Start the explosion effect
                effect.subEffectTokens.Add(PlayEffect(EffectIDs.JupitelThunderExplosion, context.target, effect.RemoveEffect));

                //Prepare for next loop
                context.currentExplosions = 1;
                context.startTime = Globals.Time + EXPLOSION_INTERVAL;
                context.reachedTarget = true;
            }
            // It's possible for the ball str effect to finish before reaching target. Restart the ball if it ended already.
            else if (effect.subEffectTokens.Count == 0)
                effect.subEffectTokens.Add(PlayEffect(EffectIDs.JupitelThunderBall, context.jtObj, effect.RemoveEffect));

            //Return false to keep effect from being removed
            return false;
        }
    }
}
