using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Bacterio.Databases;

namespace BacterioEditor
{
    [CustomEditor(typeof(ParticleGenerator))]
    public class ParticleGeneratorEditor : Editor
    {
        ParticleEffectId _previousParticle = ParticleEffectId.Invalid;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var particleObj = (ParticleGenerator)target;

            if (_previousParticle != particleObj._particleId)
            {
                _previousParticle = particleObj._particleId;

                if (_previousParticle != ParticleEffectId.Invalid)
                    LoadEffect();
            }

            if (GUILayout.Button("Save Effect"))
                SaveEffect();
        }

        private void SaveEffect()
        {
            var particleObj = (ParticleGenerator)target;
            var effects = DatabaseGenerator.GetEffects();

            //If we skipped a couple effects, add empty ones until ours
            if (effects.ParticleEffects.Count <= (int)particleObj._particleId)
            {
                for (int i = effects.ParticleEffects.Count; i <= (int)particleObj._particleId; i++)
                    effects.ParticleEffects.Add(new EffectDb.ParticleEffectData());
            }

            //Get the effect data
            var particleEffect = new EffectDb.ParticleEffectData();
            particleEffect.effectId = particleObj._particleId;
            var system = particleObj._particleSystem;

            //generator
            particleEffect.durationMs = (int)(system.main.duration * 1000);
            particleEffect.looping = system.main.loop;
            particleEffect.startColor = system.main.startColor;
            particleEffect.startLifetime = system.main.startLifetime;
            particleEffect.startSpeed = system.main.startSpeed;

            //emission
            particleEffect.rateOverTime = system.emission.rateOverTime;

            //velocity over lifetime
            particleEffect.speedModifier = system.velocityOverLifetime.speedModifier;

            //Color over lifetime
            particleEffect.colorOverLifetime = system.colorOverLifetime.color;

            //Save the effect
            effects.ParticleEffects[(int)particleObj._particleId] = particleEffect;

            //Update database
            DatabaseGenerator.SaveJson(effects.JsonName, effects);
        }

        private void LoadEffect()
        {
            var particleObj = (ParticleGenerator)target;
            var system = particleObj._particleSystem;

            //get the effect
            var effects = DatabaseGenerator.GetEffects();
            var effect = effects.ParticleEffects[(int)particleObj._particleId];

            system.Stop();

            //Fill the system
            //generator
            var main = system.main;
            main.duration = effect.durationMs / 1000.0f;
            main.loop = effect.looping;
            main.startColor = effect.startColor;
            main.startLifetime = effect.startLifetime;
            main.startSpeed = effect.startSpeed;

            //emission
            var emission = system.emission;
            emission.rateOverTime = effect.rateOverTime;

            //velocity over lifetime
            var velocityOverLifetime = system.velocityOverLifetime;
            velocityOverLifetime.speedModifier = effect.speedModifier;

            //Color over lifetime
            var colorOverLifetime = system.colorOverLifetime;
            colorOverLifetime.color = effect.colorOverLifetime;

            system.Play();
        }
    }
}
