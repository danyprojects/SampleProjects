using RO.Databases;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace RO.Media
{
    using RemoveEffectCb = System.Action<EffectCancelToken>;

    public abstract class EffectCancelToken
    {
        public enum EffectType : byte
        {
            Mesh = 0,
            Sprite,
            Cylinder,
            Pyramid,
            Func,
            CastCircle,
            CastLockOn,
            FillBar,

            None
        }

        public EffectCancelToken(EffectType type, short index, int token)
        {
            this.type = type;
            this.index = index;
            this.token = token;
        }

        //These will purposely not check for null, as it should not happen in most cases so no point in paying for it every time
        public static bool operator ==(EffectCancelToken cancelTokenA, EffectCancelToken cancelTokenB)
        {
            return cancelTokenA.type == cancelTokenB.type && cancelTokenA.token == cancelTokenB.token && cancelTokenA.index == cancelTokenB.index;
        }

        public static bool operator !=(EffectCancelToken cancelTokenA, EffectCancelToken cancelTokenB)
        {
            return !(cancelTokenA == cancelTokenB);
        }

        public override bool Equals(object obj)
        {
            var objToken = (EffectCancelToken)obj;
            return objToken == this;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool IsNull(EffectCancelToken cancelToken)
        {
            return ReferenceEquals(cancelToken, null);
        }

        public bool CheckToken(int token)
        {
            return this.token == token;
        }

        public abstract void Cancel();

        public readonly EffectType type;
        protected readonly int token;
        protected readonly short index;
    }

    public static partial class EffectsAnimatorController
    {
        public sealed class Updater
        {
            public void UpdateEffects()
            {
                EffectsAnimatorController.UpdateEffects();
            }

            public void ClearEffects()
            {
                EffectsAnimatorController.ClearEffects();
            }
        }

        private sealed class CancelToken : Media.EffectCancelToken
        {
            public CancelToken(EffectType type, short index, int token)
                : base(type, index, token)
            {
            }

            public override void Cancel()
            {
                EffectsAnimatorController.CancelEffect(type, index, token);
            }
        }

        private sealed class MeshEffect
        {
            public MeshEffect(EffectAnimatorMesh effect, RemoveEffectCb removeEffectCb)
            {
                this.effect = effect;
                this.removeEffectCb = removeEffectCb;
            }

            public void Clear()
            {
                effect.enabled = false;
                effect = null;
                removeEffectCb = null;
                cancelToken = null;
            }

            public CancelToken cancelToken;
            public EffectAnimatorMesh effect;
            public RemoveEffectCb removeEffectCb;
        }

        private sealed class SpriteEffect
        {
            public SpriteEffect(EffectAnimatorSprite effect, RemoveEffectCb removeEffectCb)
            {
                this.effect = effect;
                this.removeEffectCb = removeEffectCb;
            }

            public void Clear()
            {
                effect.enabled = false;
                effect = null;
                removeEffectCb = null;
                cancelToken = null;
            }

            public CancelToken cancelToken;
            public EffectAnimatorSprite effect;
            public RemoveEffectCb removeEffectCb;
        }

        private sealed class CylinderEffect
        {
            public CylinderEffect(MeshRenderer effect, RemoveEffectCb removeEffectCb)
            {
                this.effect = effect;
                this.removeEffectCb = removeEffectCb;
            }

            public void Clear()
            {
                ObjectPoll.CylinderPoll = effect.gameObject;
                effect = null;
                removeEffectCb = null;
                cancelToken = null;
            }

            public CancelToken cancelToken;
            public MeshRenderer effect;
            public RemoveEffectCb removeEffectCb;
        }

        private sealed class FuncEffect
        {
            public FuncEffect(Func<FuncEffect, bool> updateFuncEffect, RemoveEffectCb removeEffectCb)
            {
                this.updateFuncEffect = updateFuncEffect;
                this.removeEffectCb = removeEffectCb;
                subEffectTokens = new List<EffectCancelToken>(MediaConstants.DEFAULT_FUNC_SUB_EFFECTS);
            }

            public void Clear()
            {
                //A func effect should not last less than the longest of it's children so there shouldn't be any children to clear
                Assert.IsTrue(subEffectTokens.Count == 0);

                removeEffectCb = null;
                cancelToken = null;
            }

            public void CancelSubEffects()
            {
                for (int i = 0; i < subEffectTokens.Count; i++)
                    subEffectTokens[i].Cancel();

                subEffectTokens.Clear();
            }

            public void RemoveEffect(EffectCancelToken cancelToken)
            {
                subEffectTokens.Remove(cancelToken);
            }

            public CancelToken cancelToken;
            public RemoveEffectCb removeEffectCb;
            public Func<FuncEffect, bool> updateFuncEffect;
            public List<EffectCancelToken> subEffectTokens;

            public object context; //For any effect specific context object
        }

        //Stacks for managing free indexes so we don't need to use removeAt as it would re-order the indexes
        private static Stack<short> _freeMeshIndexes = new Stack<short>(MediaConstants.DEFAULT_MESH_EFFECTS_COUNT);
        private static Stack<short> _freeSpriteIndexes = new Stack<short>(MediaConstants.DEFAULT_SPRITE_EFFECTS_COUNT);
        private static Stack<short> _freeCylinderIndexes = new Stack<short>(MediaConstants.DEFAULT_CYLINDER_EFFECTS_COUNT);
        private static Stack<short> _freeFuncIndexes = new Stack<short>(MediaConstants.DEFAULT_FUNCTION_EFFECTS_COUNT);

        //Lists that store the actual effect reference. These can have holes
        private static List<MeshEffect> _meshEffects = new List<MeshEffect>(MediaConstants.DEFAULT_MESH_EFFECTS_COUNT);
        private static List<SpriteEffect> _spriteEffects = new List<SpriteEffect>(MediaConstants.DEFAULT_SPRITE_EFFECTS_COUNT);
        private static List<CylinderEffect> _cylinderEffects = new List<CylinderEffect>(MediaConstants.DEFAULT_CYLINDER_EFFECTS_COUNT);
        private static List<FuncEffect> _funcEffects = new List<FuncEffect>(MediaConstants.DEFAULT_FUNCTION_EFFECTS_COUNT);

        //Sorted / No holes lists that will map to the lists above. This is for optimized iteration during update and clear
        private static List<short> _meshEffectIndexes = new List<short>(_meshEffects.Capacity);
        private static List<short> _spriteEffectIndexes = new List<short>(_spriteEffects.Capacity);
        private static SortedList<double, short> _cylinderEffectTicks = new SortedList<double, short>(_cylinderEffects.Capacity);
        private static List<short> _funcEffectIndexes = new List<short>(_funcEffects.Capacity);

        private static MaterialPropertyBlock materialBlock = new MaterialPropertyBlock();
        private const int START_TOKEN = 0;
        private static int _nextToken = START_TOKEN;

        //Sprite and mesh PlayEffect methods
        public static EffectCancelToken PlayEffect(EffectIDs effectId, ref Vector3 center, Color color, RemoveEffectCb removeEffectCb, SoundDb.SoundIds soundId = SoundDb.SoundIds.Default)
        {
            GameObject effect = AssetBundleProvider.LoadEffectBundleAsset<GameObject>((int)effectId);
            effect = GameObject.Instantiate(effect, null);
            effect.transform.localPosition = center;

            return PlaySpriteMeshEffect(effectId, effect, color, removeEffectCb, soundId);
        }

        public static EffectCancelToken PlayEffect(EffectIDs effectId, ref Vector3 center, RemoveEffectCb removeEffectCb, SoundDb.SoundIds soundId = SoundDb.SoundIds.Default)
        {
            return PlayEffect(effectId, ref center, Color.white, removeEffectCb, soundId); //Calls the one above
        }

        public static EffectCancelToken PlayEffect(EffectIDs effectId, Transform target, Color color, RemoveEffectCb removeEffectCb, SoundDb.SoundIds soundId = SoundDb.SoundIds.Default)
        {
            GameObject effect = AssetBundleProvider.LoadEffectBundleAsset<GameObject>((int)effectId);
            effect = GameObject.Instantiate(effect, target, false);

            return PlaySpriteMeshEffect(effectId, effect, color, removeEffectCb, soundId);
        }

        public static EffectCancelToken PlayEffect(EffectIDs effectId, Transform target, RemoveEffectCb removeEffectCb, SoundDb.SoundIds soundId = SoundDb.SoundIds.Default)
        {
            return PlayEffect(effectId, target, Color.white, removeEffectCb, soundId); //Calls the one above
        }

        //Cylinder PlayEffect methods
        public static EffectCancelToken PlayEffect(CylinderEffectIDs effectId, Transform target, double duration, RemoveEffectCb removeEffectCb, SoundDb.SoundIds soundId = SoundDb.SoundIds.Default)
        {
            Transform transform = PlayCylinderEffect(effectId, 1, duration, removeEffectCb, soundId, out short index).transform;
            transform.position = Vector3.zero;
            transform.SetParent(target, false);
            return _cylinderEffects[index].cancelToken;
        }

        public static EffectCancelToken PlayEffect(CylinderEffectIDs effectId, ref Vector3 center, double duration, RemoveEffectCb removeEffectCb, SoundDb.SoundIds soundId = SoundDb.SoundIds.Default)
        {
            PlayCylinderEffect(effectId, 0, duration, removeEffectCb, soundId, out short index).transform.localPosition = center;
            return _cylinderEffects[index].cancelToken;
        }

        public static EffectCancelToken PlayEffect(CylinderEffectIDs effectId, Transform target, RemoveEffectCb removeEffectCb, SoundDb.SoundIds soundId = SoundDb.SoundIds.Default)
        {
            Transform transform = PlayCylinderEffect(effectId, 1, EffectDb.CylindersEffects[(int)effectId].defaultDuration, removeEffectCb, soundId, out short index).transform;
            transform.position = Vector3.zero;
            transform.SetParent(target, false);
            return _cylinderEffects[index].cancelToken;
        }

        public static EffectCancelToken PlayEffect(CylinderEffectIDs effectId, ref Vector3 center, RemoveEffectCb removeEffectCb, SoundDb.SoundIds soundId = SoundDb.SoundIds.Default)
        {
            PlayCylinderEffect(effectId, 0, EffectDb.CylindersEffects[(int)effectId].defaultDuration, removeEffectCb, soundId, out short index).transform.localPosition = center;
            return _cylinderEffects[index].cancelToken;
        }


        //Pyramid PlayEffect methods
        public static EffectCancelToken PlayEffect(PyramidEffectIds effectId, Transform target, double duration, RemoveEffectCb removeEffectCb, SoundDb.SoundIds soundId = SoundDb.SoundIds.Default)
        {
            //TODO;
            return new CancelToken(Media.EffectCancelToken.EffectType.Pyramid, 0, 0);
        }

        public static EffectCancelToken PlayEffect(PyramidEffectIds effectId, ref Vector3 center, double duration, RemoveEffectCb removeEffectCb, SoundDb.SoundIds soundId = SoundDb.SoundIds.Default)
        {
            //TODO
            return new CancelToken(Media.EffectCancelToken.EffectType.Pyramid, 0, 0);
        }

        //"Func" PlayEffect methods will be in a seperate file 

        //**** private stuff
        private static int IncrementAndGetNextToken()
        {
            int token = _nextToken;
            if (_nextToken == int.MaxValue)
                _nextToken = START_TOKEN;
            else
                _nextToken++;
            return token;
        }

        private static EffectCancelToken PlaySpriteMeshEffect(EffectIDs effectId, GameObject effect, Color color, RemoveEffectCb removeEffectCb, SoundDb.SoundIds soundId)
        {
            AudioSource audioSource;

            EffectAnimatorSprite effectAnimatorSprite = effect.GetComponent<EffectAnimatorSprite>();

            int index;
            CancelToken cancelToken;

            if (effectAnimatorSprite != null)
            {
                //If we have a free index then re-use it
                if (_freeSpriteIndexes.Count > 0)
                {
                    index = _freeSpriteIndexes.Pop();
                    _spriteEffects[index].effect = effectAnimatorSprite;
                    _spriteEffects[index].removeEffectCb = removeEffectCb;
                }
                else //Otherwise add it to the end
                {
                    index = _spriteEffects.Count;
                    _spriteEffects.Add(new SpriteEffect(effectAnimatorSprite, removeEffectCb));
                }

                cancelToken = new CancelToken(EffectCancelToken.EffectType.Sprite, (short)index, IncrementAndGetNextToken());
                _spriteEffects[index].cancelToken = cancelToken;

                _spriteEffectIndexes.Add((short)index);
                effectAnimatorSprite.SetColor(ref color);
                audioSource = effectAnimatorSprite.AudioSource;
            }
            else
            {
                EffectAnimatorMesh effectAnimatorMesh = effect.GetComponent<EffectAnimatorMesh>();

                //If we have a free index then re-use it
                if (_freeMeshIndexes.Count > 0)
                {
                    index = _freeMeshIndexes.Pop();
                    _meshEffects[index].effect = effectAnimatorMesh;
                    _meshEffects[index].removeEffectCb = removeEffectCb;
                }
                else
                {
                    index = _meshEffects.Count;
                    _meshEffects.Add(new MeshEffect(effectAnimatorMesh, removeEffectCb));
                }

                cancelToken = new CancelToken(EffectCancelToken.EffectType.Mesh, (short)index, IncrementAndGetNextToken());
                _meshEffects[index].cancelToken = cancelToken;

                _meshEffectIndexes.Add((short)index);
                effectAnimatorMesh.SetColor(ref color);
                audioSource = effectAnimatorMesh.AudioSource;
            }

            //Check if we need to play default sound
            if (soundId == SoundDb.SoundIds.Last)
                SoundController.PlayAudioEffect(EffectDb.Effects[(int)effectId].soundId, audioSource);
            else
                SoundController.PlayAudioEffect(soundId, audioSource);

            return cancelToken;
        }

        private static GameObject PlayCylinderEffect(CylinderEffectIDs effectId, int isTarget, double duration, RemoveEffectCb removeEffectCb, SoundDb.SoundIds soundId, out short index)
        {
            //Get cylinder from pool
            GameObject obj = ObjectPoll.CylinderPoll;
            var rend = obj.GetComponent<MeshRenderer>();

            //Reuse index if we have one free
            if (_freeCylinderIndexes.Count > 0)
            {
                index = _freeCylinderIndexes.Pop();
                _cylinderEffects[index].effect = rend;
                _cylinderEffects[index].removeEffectCb = removeEffectCb;
            }
            else
            {
                index = (short)_cylinderEffects.Count;
                _cylinderEffects.Add(new CylinderEffect(rend, removeEffectCb));
            }

            var token = new CancelToken(EffectCancelToken.EffectType.Cylinder, index, IncrementAndGetNextToken());
            _cylinderEffects[index].cancelToken = token;

            ///Insert it in the tick list
            _cylinderEffectTicks.Add(Common.Globals.Time + duration, index);

            //set material and properties
            rend.material = EffectDb.CylindersEffects[(int)effectId].materials[isTarget];

            rend.GetComponent<MeshRenderer>().GetPropertyBlock(materialBlock);
            materialBlock.SetTexture(MediaConstants.SHADER_MAIN_TEX_PROPERTY_ID, EffectDb.CylindersEffects[(int)effectId].texture);
            materialBlock.SetColor(MediaConstants.SHADER_TINT_PROPERTY_ID, EffectDb.CylindersEffects[(int)effectId].color1);
            materialBlock.SetColor(MediaConstants.SHADER_TINT2_PROPERTY_ID, EffectDb.CylindersEffects[(int)effectId].color2);
            materialBlock.SetColor(MediaConstants.SHADER_TINT3_PROPERTY_ID, EffectDb.CylindersEffects[(int)effectId].color3);
            materialBlock.SetColor(MediaConstants.SHADER_TINT4_PROPERTY_ID, EffectDb.CylindersEffects[(int)effectId].color4);
            materialBlock.SetVector(MediaConstants.SHADER_CYL_BOTTOM_WIDTH_ID, EffectDb.CylindersEffects[(int)effectId].bottomWidths);
            materialBlock.SetVector(MediaConstants.SHADER_CYL_TOP_WIDTH_ID, EffectDb.CylindersEffects[(int)effectId].topWidths);
            materialBlock.SetVector(MediaConstants.SHADER_CYL_MIN_HEIGHT_ID, EffectDb.CylindersEffects[(int)effectId].minHeights);
            materialBlock.SetVector(MediaConstants.SHADER_CYL_MAX_HEIGHT_ID, EffectDb.CylindersEffects[(int)effectId].maxHeights);
            materialBlock.SetVector(MediaConstants.SHADER_CYL_HEIGHT_SPEED_ID, EffectDb.CylindersEffects[(int)effectId].heightSpeed);
            materialBlock.SetVector(MediaConstants.SHADER_CYL_ROTATE_SPEED_ID, EffectDb.CylindersEffects[(int)effectId].rotateSpeeds);
            materialBlock.SetVector(MediaConstants.SHADER_START_TIME_ID, new Vector4(Common.Globals.TimeSinceLevelLoad,
                                                                                     EffectDb.CylindersEffects[(int)effectId].fadeTime,
                                                                                     0,
                                                                                     (float)duration));
            rend.GetComponent<MeshRenderer>().SetPropertyBlock(materialBlock);

            //Check if we need to play default sound
            if (soundId == SoundDb.SoundIds.Default)
                SoundController.PlayAudioEffect(EffectDb.CylindersEffects[(int)effectId].soundId, obj.GetComponent<AudioSource>());
            else
                SoundController.PlayAudioEffect(soundId, rend.gameObject.GetComponent<AudioSource>());

            return obj;
        }

        private static FuncEffect GetFreeFuncEffect(Func<FuncEffect, bool> updateFuncEffect, RemoveEffectCb removeEffectCb)
        {
            int index;
            //Reuse index if we have one free
            if (_freeFuncIndexes.Count > 0)
            {
                index = _freeFuncIndexes.Pop();
                _funcEffects[index].updateFuncEffect = updateFuncEffect;
                _funcEffects[index].removeEffectCb = removeEffectCb;
            }
            else
            {
                index = (short)_funcEffects.Count;
                _funcEffects.Add(new FuncEffect(updateFuncEffect, removeEffectCb));
            }

            //Create the cancel token
            var token = new CancelToken(EffectCancelToken.EffectType.Func, (short)index, IncrementAndGetNextToken());
            _funcEffects[index].cancelToken = token;

            _funcEffectIndexes.Add((short)index);

            return _funcEffects[index];
        }

        private static void ClearEffects()
        {
            //Clear the sprite animator effects
            for (int i = _spriteEffectIndexes.Count - 1; i >= 0; i--)
            {
                _spriteEffects[_spriteEffectIndexes[i]].removeEffectCb?.Invoke(_spriteEffects[_spriteEffectIndexes[i]].cancelToken);
                _spriteEffects[_spriteEffectIndexes[i]].Clear();
                _freeSpriteIndexes.Push(_spriteEffectIndexes[i]);
            }
            _spriteEffectIndexes.Clear();

            //Clear the mesh animator effects
            for (int i = _meshEffectIndexes.Count - 1; i >= 0; i--)
            {
                _meshEffects[_meshEffectIndexes[i]].removeEffectCb?.Invoke(_meshEffects[_meshEffectIndexes[i]].cancelToken);
                _meshEffects[_meshEffectIndexes[i]].Clear();
                _freeMeshIndexes.Push(_meshEffectIndexes[i]);
            }
            _meshEffectIndexes.Clear();

            //Clear the cylinder effects
            foreach (var cyl in _cylinderEffectTicks.Values)
            {
                _cylinderEffects[cyl].removeEffectCb?.Invoke(_cylinderEffects[cyl].cancelToken);
                _cylinderEffects[cyl].Clear();
                _freeCylinderIndexes.Push(cyl);
            }
            _cylinderEffectTicks.Clear();

            //Clear the func effects last
            for (int i = _funcEffectIndexes.Count - 1; i >= 0; i--)
            {
                _funcEffects[_funcEffectIndexes[i]].removeEffectCb?.Invoke(_funcEffects[_funcEffectIndexes[i]].cancelToken);
                _funcEffects[_funcEffectIndexes[i]].Clear();
                _freeFuncIndexes.Push(_funcEffectIndexes[i]);
            }
            _funcEffectIndexes.Clear();
        }

        private static void UpdateEffects()
        {
            //Update sprite animator effects
            for (int i = _spriteEffectIndexes.Count - 1; i >= 0; i--)
            {
                if (_spriteEffects[_spriteEffectIndexes[i]].effect.UpdateRenderer())
                {
                    //Check if we need to remove effect from block before clearing
                    _spriteEffects[_spriteEffectIndexes[i]].removeEffectCb?.Invoke(_spriteEffects[_spriteEffectIndexes[i]].cancelToken);
                    _spriteEffects[_spriteEffectIndexes[i]].Clear();

                    //Free the index
                    _freeSpriteIndexes.Push(_spriteEffectIndexes[i]);
                    _spriteEffectIndexes.RemoveAt(i);
                }
            }

            //Update mesh animator effects
            for (int i = _meshEffectIndexes.Count - 1; i >= 0; i--)
            {
                if (_meshEffects[_meshEffectIndexes[i]].effect.UpdateRenderer())
                {
                    //Check if we need to remove effect from block before clearing
                    _meshEffects[_meshEffectIndexes[i]].removeEffectCb?.Invoke(_meshEffects[_meshEffectIndexes[i]].cancelToken);
                    _meshEffects[_meshEffectIndexes[i]].Clear();

                    //Free the index
                    _freeMeshIndexes.Push(_meshEffectIndexes[i]);
                    _meshEffectIndexes.RemoveAt(i);
                }
            }

            //Go through sorted list of cylinders to check if any need removing
            while (_cylinderEffectTicks.Count > 0 && _cylinderEffectTicks.Keys[0] <= Common.Globals.Time)
            {
                //If audio is still playing then stop the renderer but schedule remove for later
                AudioSource audio = _cylinderEffects[_cylinderEffectTicks.Values[0]].effect.GetComponent<AudioSource>();
                if (audio.clip != null && audio.isPlaying)
                {
                    _cylinderEffects[_cylinderEffectTicks.Values[0]].effect.enabled = false;
                    short index = _cylinderEffectTicks.Values[0];
                    _cylinderEffectTicks.RemoveAt(0);
                    _cylinderEffectTicks.Add(Common.Globals.Time + audio.clip.length - audio.time, index);
                    continue;
                }

                //Check if we need to remove effect from block before clearing
                _cylinderEffects[_cylinderEffectTicks.Values[0]].removeEffectCb?.Invoke(_cylinderEffects[_cylinderEffectTicks.Values[0]].cancelToken);
                _cylinderEffects[_cylinderEffectTicks.Values[0]].Clear();

                //Free the index
                _freeCylinderIndexes.Push(_cylinderEffectTicks.Values[0]);
                _cylinderEffectTicks.RemoveAt(0);
            }

            //Update func effects last. If it ends now then all sub effects must have finished as well
            for (int i = _funcEffectIndexes.Count - 1; i >= 0; i--)
            {
                if (_funcEffects[_funcEffectIndexes[i]].updateFuncEffect(_funcEffects[_funcEffectIndexes[i]]))
                {
                    //Check if we need to remove effect from block before clearing
                    _funcEffects[_funcEffectIndexes[i]].removeEffectCb?.Invoke(_funcEffects[_funcEffectIndexes[i]].cancelToken);
                    _funcEffects[_funcEffectIndexes[i]].Clear();

                    //Free the index
                    _freeFuncIndexes.Push(_funcEffectIndexes[i]);
                    _funcEffectIndexes.RemoveAt(i);
                }
            }
        }

        private static void CancelEffect(EffectCancelToken.EffectType type, short index, int token)
        {
            switch (type)
            {
                case EffectCancelToken.EffectType.Mesh:
                    {
                        if (_meshEffects[index].cancelToken.CheckToken(token))
                        {
                            _meshEffects[index].Clear();
                            _freeMeshIndexes.Push(index);
                            _meshEffectIndexes.Remove(index); // So we don't have duplicates in the array when we iterate
                        }
                    }
                    break;
                case EffectCancelToken.EffectType.Sprite:
                    {
                        if (_spriteEffects[index].cancelToken.CheckToken(token))
                        {
                            _spriteEffects[index].Clear();
                            _freeSpriteIndexes.Push(index);
                            _spriteEffectIndexes.Remove(index); // So we don't have duplicates in the array when we iterate
                        }
                    }
                    break;
                case EffectCancelToken.EffectType.Cylinder:
                    {
                        if (_cylinderEffects[index].cancelToken.CheckToken(token))
                        {
                            _cylinderEffects[index].Clear();
                            _freeCylinderIndexes.Push(index);
                            _cylinderEffectTicks.RemoveAt(_cylinderEffectTicks.IndexOfValue(index));
                        }
                    }
                    break;
                case EffectCancelToken.EffectType.Func:
                    {
                        if (_funcEffects[index].cancelToken.CheckToken(token))
                        {
                            _funcEffects[index].CancelSubEffects();
                            _funcEffects[index].Clear();
                            _freeFuncIndexes.Push(index);
                            _funcEffectIndexes.Remove(index);
                        }
                    }
                    break;
                case EffectCancelToken.EffectType.Pyramid: break;
            }
        }

    }
}
