using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Bacterio.Databases;

namespace BacterioEditor
{
    [CustomEditor(typeof(TrailGenerator))]
    public class TrailGeneratorEditor : Editor
    {
        TrailEffectId _previousTrail = TrailEffectId.Invalid;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var trailObj = (TrailGenerator)target;

            if(_previousTrail != trailObj._trailId)
            {
                _previousTrail = trailObj._trailId;

                if(_previousTrail != TrailEffectId.Invalid)
                    LoadEffect();
            }

            if (GUILayout.Button("Save Effect"))
                SaveEffect();
        }

        private void SaveEffect()
        {
            var trailObj = (TrailGenerator)target;
            var effects = DatabaseGenerator.GetEffects();

            //If we skipped a couple effects, add empty ones until ours
            if (effects.TrailEffects.Count <= (int)trailObj._trailId)
            {
                for (int i = effects.TrailEffects.Count; i <= (int)trailObj._trailId; i++)
                    effects.TrailEffects.Add(new EffectDb.TrailEffectData());
            }

            //Get the effect data
            var trailEffect = new EffectDb.TrailEffectData();
            trailEffect.effectId = trailObj._trailId;
            trailEffect.durationMs = trailObj._durationMs;
            trailEffect.gradient = trailObj._trailRenderer.colorGradient;

            //Save the effect
            effects.TrailEffects[(int)trailObj._trailId] = trailEffect;
            
            //Update database
            DatabaseGenerator.SaveJson(effects.JsonName, effects);
        }

        private void LoadEffect()
        {
            var trailObj = (TrailGenerator)target;

            //get the effect
            var effects = DatabaseGenerator.GetEffects();
            var effect = effects.TrailEffects[(int)trailObj._trailId];

            //Fill the renderer
            trailObj._trailRenderer.colorGradient = effect.gradient;
        }
    }
}