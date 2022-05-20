using System.Collections.Generic;
using UnityEngine;
using Bacterio.Common;
using Bacterio.Databases;

namespace Bacterio
{
    public sealed class EffectsController : System.IDisposable
    {
        private enum EffectType
        {
            TrailEffect = 0,
            ParticleEffect,

            None = -1
        }

        public struct EffectToken
        {
            public readonly short index;
            public readonly short tag;

            public EffectToken(short index, short tag)
            {
                this.index = index;
                this.tag = tag;
            }
        }

        private struct EffectSlot
        {
            public short tag;
            public EffectType type;
            public object data;
        }

        private readonly TimerController _timerController = null;

        //pools
        private readonly ObjectPool<TrailRenderer> _trailsPool = null;
        private readonly ObjectPool<ParticleSystem> _particlesPool = null;

        //Empty gameobject to hold map effects without parents. This way we can keep track when we need to destroy them
        private readonly Transform _emptyEffectParent = null;

        //For holding the tokens of effects
        private EffectSlot[] _effectSlots = null;
        private int _count = 0;

        public EffectsController(TimerController timerController)
        {
            _timerController = timerController;

            _emptyEffectParent = new GameObject("EmptyEffectParent").transform;

            //Load prefabs
            var trailObj = GlobalContext.assetBundleProvider.LoadObjectAsset("DefaultTrail");
            var particleObj = GlobalContext.assetBundleProvider.LoadObjectAsset("DefaultParticle");

            WDebug.Assert(trailObj != null, "No default trail renderer prefab");
            WDebug.Assert(particleObj != null, "No default particle system prefab");

            //Create the pools
            _trailsPool = new ObjectPool<TrailRenderer>(trailObj.GetComponent<TrailRenderer>(), Constants.TRAILS_POOL_INITIAL_AMOUNT, Constants.TRAILS_POOL_GROWTH_AMOUNT, Vector3.zero, Quaternion.identity, _emptyEffectParent, OnPushTrailRenderer);
            _particlesPool = new ObjectPool<ParticleSystem>(particleObj.GetComponent<ParticleSystem>(), Constants.PARTICLES_POOL_INITIAL_AMOUNT, Constants.PARTICLES_POOL_GROWTH_AMOUNT, Vector3.zero, Quaternion.identity, _emptyEffectParent, OnPushParticleSystem);

            //Allocate the effect slots
            _effectSlots = new EffectSlot[Constants.DEFAULT_EFFECT_AMOUNT];
            for (int i = 0; i < _effectSlots.Length; i++)
                _effectSlots[i].type = EffectType.None;
        }

        public void Dispose()
        {
            _trailsPool.Dispose();
            _particlesPool.Dispose();
            Object.Destroy(_emptyEffectParent.gameObject);
            //Any effects that are not in the pool, should be destroyed alongside the game's objects.
        }

        //Play effect with duration and cancel midway
        //Play effect without duration and cancel

        //******************************************************************* public Utility methods
        //Trail effect methods. First one does the work. Others are overloads.
        //If any of the overloads has duration 0, the default duration will be used.
        //If the duration is 0, a timer won't be started.
        private EffectToken PlayTrailEffect(TrailEffectId trailEffectId, Transform target, Vector2 position, int durationMs)
        {
            ref var trailEffectData = ref GlobalContext.effectDb.GetTrailData(trailEffectId);

            //Check if it's default duration or not
            durationMs = durationMs == 0 ? GlobalContext.effectDb.GetTrailData(trailEffectId).durationMs : durationMs;

            //Get a trail effect and fill it
            var trail = _trailsPool.Pop();
            trail.colorGradient = trailEffectData.gradient;

            //Assign it to the parent
            trail.transform.localPosition = position;
            trail.transform.SetParent(target, false);

            //Activate it
            trail.enabled = true;

            //Get a slot for the effect
            var slotIndex = GetEffectSlotIndex();
            _effectSlots[slotIndex].type = EffectType.TrailEffect;
            _effectSlots[slotIndex].data = trail;

            //Get a cancel token
            var token = new EffectToken(slotIndex, _effectSlots[slotIndex].tag);

            //Set the removal timer if duration is valid
            if(durationMs > 0)
                _timerController.Add(durationMs, () => CancelEffect(token));

            //Return the token
            return token;
        }

        public EffectToken PlayTrailEffect(TrailEffectId trailEffectId, Transform target, int durationMs = 0)
        {
            return PlayTrailEffect(trailEffectId, target, Vector3.zero, durationMs);
        }

        public EffectToken PlayTrailEffect(TrailEffectId trailEffectId, Vector2 position, int durationMs = 0)
        {
            return PlayTrailEffect(trailEffectId, _emptyEffectParent, position, durationMs);
        }

        private EffectToken PlayParticleEffect(ParticleEffectId particleEffectId, Transform target, Vector2 position, int durationMs)
        {
            ref var particleEffectData = ref GlobalContext.effectDb.GetParticleData(particleEffectId);

            //Check if it's default duration or not
            durationMs = durationMs == 0 ? GlobalContext.effectDb.GetParticleData(particleEffectId).durationMs : durationMs;

            //Get a particle effect and fill it
            var system = _particlesPool.Pop();

            var main = system.main;
            main.duration = particleEffectData.durationMs / 1000.0f;
            main.loop = particleEffectData.looping;
            main.startColor = particleEffectData.startColor;
            main.startLifetime = particleEffectData.startLifetime;
            main.startSpeed = particleEffectData.startSpeed;

            var emission = system.emission;
            emission.rateOverTime = particleEffectData.rateOverTime;

            var velocityOverLifetime = system.velocityOverLifetime;
            velocityOverLifetime.speedModifier = particleEffectData.speedModifier;

            var colorOverLifetime = system.colorOverLifetime;
            colorOverLifetime.color = particleEffectData.colorOverLifetime;

            //Assign it to the parent
            system.transform.localPosition = position;
            system.transform.SetParent(target, false);

            //Activate it
            system.Play();

            //Get a slot for the effect
            var slotIndex = GetEffectSlotIndex();
            _effectSlots[slotIndex].type = EffectType.ParticleEffect;
            _effectSlots[slotIndex].data = system;

            //Get a cancel token
            var token = new EffectToken(slotIndex, _effectSlots[slotIndex].tag);

            //Set the removal timer if duration is valid
            if(durationMs > 0)
                _timerController.Add(durationMs + (main.startLifetime.constantMax * Constants.ONE_SECOND_MS), () => CancelEffect(token));

            //Return the token
            return token;
        }

        public EffectToken PlayParticleEffect(ParticleEffectId particleEffectId, Transform target, int durationMs = 0)
        {
            return PlayParticleEffect(particleEffectId, target, Vector3.zero, durationMs);
        }

        public EffectToken PlayParticleEffect(ParticleEffectId particleEffectId, Vector2 position, int durationMs = 0)
        {
            return PlayParticleEffect(particleEffectId, _emptyEffectParent, position, durationMs);
        }

        public void CancelEffect(EffectToken token)
        {
            WDebug.Assert(token.index < _effectSlots.Length, "Invalid token index passed to cancel");

            //Tags differ, skip. Could happen if it was already canceled by either outside logic, or the timer
            if (_effectSlots[token.index].tag != token.tag)
                return;

            //Otherwise it's a valid cancel request
            switch(_effectSlots[token.index].type)
            {
                case EffectType.TrailEffect: RemoveTrailEffect((TrailRenderer)_effectSlots[token.index].data); break;
                case EffectType.ParticleEffect: RemoveParticleEffect((ParticleSystem)_effectSlots[token.index].data); break;
            }

            _effectSlots[token.index].tag++; //Increment the tag so the next cancel / remove fails and also in preparation for the next effect who might use this slot
            _effectSlots[token.index].type = EffectType.None;
            _effectSlots[token.index].data = null;
        }

        private void RemoveTrailEffect(TrailRenderer trail)
        {
            //Disable it
            trail.enabled = false;

            //Remove the parent
            trail.transform.SetParent(_emptyEffectParent);

            //Put it back in the pool
            _trailsPool.Push(trail);
        }

        private void RemoveParticleEffect(ParticleSystem system)
        {
            //Stop it
            system.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);

            //Remove the parent
            system.transform.SetParent(_emptyEffectParent);

            //Put it back in the pool
            _particlesPool.Push(system);
        }

        //******************************************************************* private utility methods
        private short GetEffectSlotIndex()
        {
            //If we ran out of slots, add a new one. 
            if (_effectSlots.Length == _count)
            {
                EnlargeSlots();

                var index = (short)_count;
                _effectSlots[index].type = EffectType.None;
                _effectSlots[index].tag = Constants.FIRST_EFFECT_TAG;

                return index;
            }

            //We'll be adding an effect for sure
            _count++;

            //Otherwise we look for a free slot.
            for (short i = 0; i < _effectSlots.Length; i++)            
                if (_effectSlots[i].type == EffectType.None)
                    return i;            

            WDebug.Assert(false, "Should never get here");
            return -1;
        }

        private void EnlargeSlots()
        {
            //Allocate new array
            var newSlots = new EffectSlot[_effectSlots.Length + Constants.EFFECT_SLOTS_GROWTH_AMOUNT];

            //Copy old to new
            System.Array.Copy(_effectSlots, newSlots, _effectSlots.Length);

            //overwrite old to new
            _effectSlots = newSlots;
        }

        //******************************************************************* Methods for object pools
        private void OnPushTrailRenderer(TrailRenderer renderer)
        {
            renderer.enabled = false;
        }

        private void OnPushParticleSystem(ParticleSystem system)
        {
            system.Stop();
        }
    }
}
