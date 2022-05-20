using RO.Common;
using RO.Containers;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace RO.Media
{
    public sealed class EffectAnimatorMesh : MonoBehaviour
    {
        //When generating the animator with SpriteGenerator, this will pre process the animation and 
        //map which frame to render during each update cycle for each of the layers
#if UNITY_EDITOR
        public Str __Str
        {
            set
            {
                _str = value;
                _meshFilters = new MeshFilter[_str.Layers.Length];
                _renderers = new Renderer[_str.Layers.Length];
                _frameMaterials = new FrameMaterials[_str.Layers.Length];
                _layerActions = new LayerAction[_str.TotalFrames];

                //simulate update from animation start to finish
                for (int i = 0; i < _str.TotalFrames; i++)
                    PreprocessUpdateCycle(i);
            }
        }

        private void PreprocessUpdateCycle(int updateCycle)
        {
            int count = 1; // to count how many frames we actually "renderered"
            int[] frameIndexes = new int[_str.Layers.Length]; //max possible size. Will shrink at the end
            for (int i = 1; i < _str.Layers.Length; i++)
            {
                int index = GetLastProcessableFrame(updateCycle, ref _str.Layers[i]);
                if (index == -1) // check if we pre-rendered a frame
                    break;
                if (_frameMaterials[i].materials == null)
                    _frameMaterials[i].materials = _str.Layers[i].Frames[index].materials;
                frameIndexes[count] = index;
                count++;
            }
            //Copy frame indexes into serializable array.
            _layerActions[updateCycle].frameIndexes = new int[count];
            for (int i = 1; i < count; i++)
                _layerActions[updateCycle].frameIndexes[i] = frameIndexes[i];
        }

        private int GetLastProcessableFrame(int updateCycle, ref Str.Layer layer)
        {
            int lastProcessable = -1;
            for (int i = 0; i < layer.Frames.Length; i++)
                if (layer.Frames[i].frameIndex <= updateCycle && i > lastProcessable) // found a valid frame                
                    lastProcessable = i;
            //If last found frame was a morph frame at the exact cycle then we should be showing the previous key frame first
            if (lastProcessable > 0 && layer.Frames[lastProcessable].type == 1 && layer.Frames[lastProcessable].frameIndex == updateCycle)
                lastProcessable--;

            if (lastProcessable > 0 && lastProcessable == layer.Frames.Length - 1)
                layer.Frames[lastProcessable].type = 2;
            return lastProcessable;
        }
#endif

        [Serializable]
        private struct LayerAction
        {
            public int[] frameIndexes;
        }

        [Serializable]
        private struct FrameMaterials
        {
            public Material[] materials;
        }

        [SerializeField] private LayerAction[] _layerActions = null; // This will contain the exact frame index to process per update
        [SerializeField] private Str _str = null;
        [SerializeField] private MeshFilter[] _meshFilters = null;
        [SerializeField] private Renderer[] _renderers = null;
        [SerializeField] private FrameMaterials[] _frameMaterials = null;
        private MaterialPropertyBlock _propertyBlock = null;
        private int _currentFrame = 0;
        private Vector3 _pos = Vector3.zero;
        private Vector3 _rot = Vector3.zero;
        private Vector3[] _vertices = new Vector3[4];
        private Color _vertexColor = Color.white;
        private float _endTime = float.MaxValue;
        bool _framesDone = false;

        public AudioSource AudioSource = null;

        /// <summary>
        /// Updates effect animation by 1 frame.
        /// If this is called after animation is done it will throw out of range exception. So make sure to catch the return value
        /// </summary>
        /// <returns>True if animation has finished. False if not</returns>
        public bool UpdateRenderer()
        {
            //This will only be used when frames are done and music was not. Branch prediction should make good use of it
            if (_framesDone)
                return Globals.Time >= _endTime;

            //Ro ignored fps checks in effects. We will do the same and only add them if game runs too fast
            for (int i = 1; i < _layerActions[_currentFrame].frameIndexes.Length; i++)  //Update all preprocessed layers
            {
                int frameIndex = _layerActions[_currentFrame].frameIndexes[i];
                if (_str.Layers[i].Frames[frameIndex].type == 0)
                    RenderKeyFrame(ref _str.Layers[i].Frames[frameIndex], i);
                else if (_str.Layers[i].Frames[frameIndex].type == 1)
                    RenderMorphFrame(ref _str.Layers[i].Frames[frameIndex], ref _str.Layers[i].Frames[frameIndex - 1], i);
                else //type 2 set during pre rendering in build type will enter here
                    _renderers[i].enabled = false;
            }

            //Experimental. This is so we end up skipping frames when we're lagging, as effects are the main source of fps drop
            _currentFrame += Common.Globals.FrameIncrement;

            //Check if frames are all done
            _framesDone = _currentFrame >= _str.TotalFrames;
            if (_framesDone)
                return OnFramesDone();

            return false;
        }

        /// <summary>
        /// Call this to reset frame back to 0. Use this if planning to cache animation
        /// </summary>
        public void ResetAnimation()
        {
            _currentFrame = 0;
            for (int i = 1; i < _meshFilters.Length; i++)
            {
                _renderers[i].GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(MediaConstants.SHADER_TINT_PROPERTY_ID, Color.white);
                _renderers[i].SetPropertyBlock(_propertyBlock);
                _renderers[i].enabled = false;
            }
        }

        public void SetColor(ref Color color)
        {
            for (int i = 1; i < _renderers.Length; i++)
            {
                _renderers[i].GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(MediaConstants.SHADER_TINT_PROPERTY_ID, color);
                _renderers[i].SetPropertyBlock(_propertyBlock);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RenderKeyFrame(ref Str.Layer.Frame frame, int layer)
        {
            _vertexColor.r = frame.color[0];
            _vertexColor.g = frame.color[1];
            _vertexColor.b = frame.color[2];
            _vertexColor.a = frame.color[3];

            _pos.x = frame.offset.x;
            _pos.y = frame.offset.y;
            _pos.z = -0.05f * layer;

            _renderers[layer].enabled = true; // first frames are always type 0 so only key frames need to enable renderer
            _renderers[layer].GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetTexture(MediaConstants.SHADER_TEXTURE_PROPERTY_ID, _str.Layers[layer].Textures[frame.textureId]);
            _propertyBlock.SetColor(MediaConstants.SHADER_VERTEX_COLOR_PROPERTY_ID, _vertexColor);
            _propertyBlock.SetFloat(MediaConstants.SHADER_ROTATION_PROPERTY_ID, frame.rotation);
            _propertyBlock.SetVector(MediaConstants.SHADER_POSITION_PROPERTY_ID, _pos);
            _renderers[layer].SetPropertyBlock(_propertyBlock);

            _meshFilters[layer].mesh.vertices = frame.vertices;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RenderMorphFrame(ref Str.Layer.Frame morphFrame, ref Str.Layer.Frame keyFrame, int layer)
        {
            int textureId = GetTextureFromAnimationType(ref morphFrame, ref keyFrame, layer);
            int delta = _currentFrame - keyFrame.frameIndex;

            _vertexColor.r = keyFrame.color[0] + morphFrame.color[0] * delta;
            _vertexColor.g = keyFrame.color[1] + morphFrame.color[1] * delta;
            _vertexColor.b = keyFrame.color[2] + morphFrame.color[2] * delta;
            _vertexColor.a = keyFrame.color[3] + morphFrame.color[3] * delta;

            _pos.x = keyFrame.offset.x + morphFrame.offset.x * delta;
            _pos.y = keyFrame.offset.y + morphFrame.offset.y * delta;
            _pos.z = -0.05f * layer;

            _renderers[layer].GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetTexture(MediaConstants.SHADER_TEXTURE_PROPERTY_ID, _str.Layers[layer].Textures[textureId]);
            _propertyBlock.SetColor(MediaConstants.SHADER_VERTEX_COLOR_PROPERTY_ID, _vertexColor);
            _propertyBlock.SetFloat(MediaConstants.SHADER_ROTATION_PROPERTY_ID, keyFrame.rotation + morphFrame.rotation * delta);
            _propertyBlock.SetVector(MediaConstants.SHADER_POSITION_PROPERTY_ID, _pos);
            _renderers[layer].SetPropertyBlock(_propertyBlock);


            for (int i = 0; i < 4; i++)
            {
                _vertices[i].x = keyFrame.vertices[i].x + morphFrame.vertices[i].x * delta;
                _vertices[i].y = keyFrame.vertices[i].y + morphFrame.vertices[i].y * delta;
            }
            _meshFilters[layer].mesh.vertices = _vertices;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetTextureFromAnimationType(ref Str.Layer.Frame morphFrame, ref Str.Layer.Frame keyFrame, int index)
        {
            var delta = _currentFrame - keyFrame.frameIndex;

            //Sorted types by occurence probability for better branch prediction
            if (morphFrame.animationType == 0)
                return keyFrame.textureId;
            else if (morphFrame.animationType == 3)
                return (keyFrame.textureId + (int)(morphFrame.delay * delta)) % _str.Layers[index].Textures.Length;
            else if (morphFrame.animationType == 1)
                return keyFrame.textureId + morphFrame.textureId * delta;
            else if (morphFrame.animationType == 2)
                return Mathf.Min(keyFrame.textureId + (int)(morphFrame.delay * delta), _str.Layers[index].Textures.Length - 1);
            else
                return (keyFrame.textureId - (int)(morphFrame.delay * delta)) % _str.Layers[index].Textures.Length;
        }

        private bool OnFramesDone()
        {
            //If frames are done and audio is done, return true
            if (AudioSource.clip == null || !AudioSource.isPlaying || (AudioSource.clip.length - AudioSource.time) <= 0.005f)
                return true;

            //Otherwise set end time and disable all renders
            _endTime = Globals.Time + AudioSource.clip.length - AudioSource.time;

            for (int i = 1; i < _renderers.Length; i++)
                _meshFilters[i].gameObject.SetActive(false);

            return false;
        }

        private void Awake()
        {
            int billboard = 0;
            if (transform.parent == null)
                billboard = 1;

            _propertyBlock = new MaterialPropertyBlock();
            AudioSource = gameObject.GetComponent<AudioSource>();
            //Get all the mesh filterers needed and assign them the pre-calculated materials
            for (int i = 1; i < _meshFilters.Length; i++)
            {
                _meshFilters[i] = ObjectPoll.EffectSpriteQuadsPoll.GetComponent<MeshFilter>();
                _meshFilters[i].transform.SetParent(transform, false);
                _renderers[i] = _meshFilters[i].GetComponent<Renderer>();
                _renderers[i].material = _frameMaterials[i].materials[billboard];
            }
        }

        private void OnDisable()
        {
            //Release the allocated quads back to poll.
            for (int i = 1; i < _meshFilters.Length; i++)
            {
                _renderers[i].enabled = false;
                ObjectPoll.EffectSpriteQuadsPoll = _meshFilters[i].gameObject;
            }
            Destroy(gameObject);
        }
    }
}
