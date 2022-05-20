using RO.Databases;
using RO.Media;
using System;
using System.Reflection;
using UnityEngine;

namespace Tests
{
    public class EffectsControllerTest : MonoBehaviour
    {
        [Serializable]
        public struct CylinderData
        {
            public Texture2D texture;
            public Color[] colors;
            public Vector4 bottomWidths;
            public Vector4 topWidths;
            public Vector4 minHeights;
            public Vector4 maxHeights;
            public Vector4 heightSpeed;
            public Vector4 rotateSpeeds;
            public float fadeTime;
            public Material areaMaterial;
            public Material targetMaterial;
            public SoundDb.SoundIds soundId;
        }

        public EffectIDs effect = EffectIDs.Last;

        public CylinderEffectIDs cylinderEffect = CylinderEffectIDs.Last;
        public float cylinderEffectDuration = 2;

        public CylinderData cylinderData = new CylinderData();
        public bool makeCylinder = false;

        public Color color = Color.white;
        public Transform Target = null;

        public bool AtGround = false;


        private EffectsAnimatorController.Updater _updater = null;

        public void Start()
        {
            RO.Common.Globals.Time = Time.time;
            _updater = new EffectsAnimatorController.Updater();
            Target = new GameObject("Player").transform;
            AddCylinderData();
        }

        public void Update()
        {
            if (Target != null)
            {
                Vector3 vec = Target.transform.position;

                if (effect != EffectIDs.Last)
                {
                    if (AtGround)
                        EffectsAnimatorController.PlayEffect(effect, ref vec, color, null);
                    else
                        EffectsAnimatorController.PlayEffect(effect, Target.transform, null);
                    effect = EffectIDs.Last;
                }

                if (cylinderEffect != CylinderEffectIDs.Last)
                {
                    if (AtGround)
                        EffectsAnimatorController.PlayEffect(cylinderEffect, ref vec, cylinderEffectDuration, null);
                    else
                        EffectsAnimatorController.PlayEffect(cylinderEffect, Target.transform, cylinderEffectDuration, null);
                    cylinderEffect = CylinderEffectIDs.Last;
                }

                if (makeCylinder)
                {
                    SetCylinderData();
                    if (AtGround)
                        EffectsAnimatorController.PlayEffect(CylinderEffectIDs.Last, ref vec, cylinderEffectDuration, null);
                    else
                        EffectsAnimatorController.PlayEffect(CylinderEffectIDs.Last, Target.transform, cylinderEffectDuration, null);
                    makeCylinder = false;
                }
            }

            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "SpriteBuilder")
            {
                Shader.SetGlobalVector(MediaConstants.SHADER_CAMERA_ROTATION_ID, Camera.main.transform.eulerAngles);
                Target.transform.eulerAngles = Camera.main.transform.eulerAngles;
                RO.Common.Globals.Time += Time.deltaTime;
                RO.Common.Globals.TimeSinceLevelLoad = Time.timeSinceLevelLoad;

                _updater.UpdateEffects();
            }
        }

        private void AddCylinderData()
        {
            Type effectDb = typeof(EffectDb);
            FieldInfo fieldInfo = effectDb.GetField("CylindersEffects", BindingFlags.Static | BindingFlags.Public);
            EffectDb.CylEffectData[] cylData = (EffectDb.CylEffectData[])fieldInfo.GetValue(null);

            EffectDb.CylEffectData[] newData = new EffectDb.CylEffectData[cylData.Length + 1];
            Array.Copy(cylData, newData, cylData.Length);

            fieldInfo.SetValue(null, newData);
        }

        private void SetCylinderData()
        {
            Type effectDb = typeof(EffectDb);
            FieldInfo fieldInfo = effectDb.GetField("CylindersEffects", BindingFlags.Static | BindingFlags.Public);
            EffectDb.CylEffectData[] cylData = (EffectDb.CylEffectData[])fieldInfo.GetValue(null);

            cylData[(int)CylinderEffectIDs.Last].texture = cylinderData.texture;
            cylData[(int)CylinderEffectIDs.Last].color1 = cylinderData.colors[0];
            cylData[(int)CylinderEffectIDs.Last].color2 = cylinderData.colors[1];
            cylData[(int)CylinderEffectIDs.Last].color3 = cylinderData.colors[2];
            cylData[(int)CylinderEffectIDs.Last].color4 = cylinderData.colors[3];
            cylData[(int)CylinderEffectIDs.Last].bottomWidths = cylinderData.bottomWidths;
            cylData[(int)CylinderEffectIDs.Last].topWidths = cylinderData.topWidths;
            cylData[(int)CylinderEffectIDs.Last].minHeights = cylinderData.minHeights;
            cylData[(int)CylinderEffectIDs.Last].maxHeights = cylinderData.maxHeights;
            cylData[(int)CylinderEffectIDs.Last].heightSpeed = cylinderData.heightSpeed;
            cylData[(int)CylinderEffectIDs.Last].rotateSpeeds = cylinderData.rotateSpeeds;
            cylData[(int)CylinderEffectIDs.Last].fadeTime = cylinderData.fadeTime;
            cylData[(int)CylinderEffectIDs.Last].materials = new Material[] { cylinderData.areaMaterial, cylinderData.targetMaterial };
            cylData[(int)CylinderEffectIDs.Last].soundId = cylinderData.soundId;
        }
    }
}
