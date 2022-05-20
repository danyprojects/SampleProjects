using RO.Common;
using RO.Containers;
using UnityEngine;

namespace RO.Media
{
    public sealed class EffectAnimatorSprite : MonoBehaviour
    {
        //This has to be here due to limitations on generating the sprites on editor
#if UNITY_EDITOR
        public Act __Act { set { _act = value; } }
        public Material[] __Materials
        {
            get { return _materials; }
            set { _materials = value; }
        }
#endif

        private MaterialPropertyBlock _propertyBlock = null;
        private SpriteRenderer[] _spriteRenderers = null;
        private Renderer[] _renderers = null;
        [SerializeField] private Act _act = null;
        [SerializeField] private Material[] _materials = null;

        private float _lastUpdate = 0;
        private int _currentFrame = 0;
        private float _actionDelay = 0;

        private float _endTime = float.MaxValue;
        bool _framesDone = false;

        public AudioSource AudioSource = null;

        /// <summary>
        /// Call this to recycle instance of effectAnimatorSprite to run another animation
        /// </summary>
        public Act Act
        {
            get
            {
                return _act;
            }
            set
            {
                _act = value;
                ReloadAct();
            }
        }

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

            //Effects should only have 1 action
            if (Globals.Time - _lastUpdate < _actionDelay)
                return false;

            var frameData = _act.Actions[0].Frames[_currentFrame].frameData;
            int i;

            //update sprite renderers with actual sprite info
            for (i = 0; i < frameData.SpriteId.Length; i++)
            {
                _spriteRenderers[i].enabled = true;
                FillSpriteRenderer(i, frameData);
            }
            //disable the remaining sprite renderers
            for (int k = i; k < _act.MaxSprites; k++)
                _spriteRenderers[k].enabled = false;

            _currentFrame += Globals.FrameIncrement;
            _lastUpdate += _actionDelay;

            //Check for audio done if frames are done. Otherwise just return false
            _framesDone = _currentFrame >= _act.Actions[0].Frames.Length;
            if (_framesDone)
                return OnFramesDone();
            return false;
        }

        public void SetPalette(Texture palette)
        {
            _act.Palette = palette;
            for (int i = 0; i < _act.MaxSprites; i++)
            {
                _renderers[i].GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetTexture(MediaConstants.SHADER_PALETTE_PROPERTY_ID, _act.Palette);
                _renderers[i].SetPropertyBlock(_propertyBlock);
            }
        }

        public void SetColor(ref Color color)
        {
            for (int i = 0; i < _act.MaxSprites; i++)
            {
                _renderers[i].GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(MediaConstants.SHADER_TINT_PROPERTY_ID, color);
                _renderers[i].SetPropertyBlock(_propertyBlock);
            }
        }

        private void FillSpriteRenderer(int index, Act.Action.Frame.FrameData frameData)
        {
            Sprite sprite = _act.Sprites[frameData.SpriteId[index]];
            _spriteRenderers[index].sprite = sprite;
            _spriteRenderers[index].color = frameData.Color[index];

            //Update the shader property block
            int width = sprite.texture.width;
            int height = sprite.texture.height;
            _renderers[index].GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetVector(MediaConstants.SHADER_DIMENSIONS_PROPERTY_ID, new Vector2(width, height));
            _propertyBlock.SetFloat(MediaConstants.SHADER_ROTATION_PROPERTY_ID, -frameData.Rotation[index]);
            _propertyBlock.SetVector(MediaConstants.SHADER_POSITION_PROPERTY_ID, new Vector3(frameData.PositionOffset[index].x,
                                                                                 -frameData.PositionOffset[index].y, index * -0.05f));
            _propertyBlock.SetVector(MediaConstants.SHADER_SCALE_PROPERTY_ID, new Vector2(frameData.IsMirrored[index] * frameData.Scale[index].x,
                                                                                 frameData.Scale[index].y));

            _renderers[index].SetPropertyBlock(_propertyBlock);
        }

        private void ReloadAct()
        {
            int billboard = 0;
            if (transform.parent == null)
                billboard = 1;

            //Get the sprite renderers we need
            for (int i = 0; i < _act.MaxSprites; i++)
            {
                if (_spriteRenderers[i] != null)
                    continue;
                _spriteRenderers[i] = ObjectPoll.EffectSpriteRenderersPoll.GetComponent<SpriteRenderer>();
                _renderers[i] = _spriteRenderers[i].GetComponent<Renderer>();
                _renderers[i].material = _materials[billboard];
                _spriteRenderers[i].transform.SetParent(transform, false);
                _spriteRenderers[i].enabled = false;
            }

            //update the starting shader properties
            for (int i = 0; i < _act.MaxSprites; i++)
            {
                _renderers[i].GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetTexture(MediaConstants.SHADER_PALETTE_PROPERTY_ID, _act.Palette);
                _propertyBlock.SetColor(MediaConstants.SHADER_TINT_PROPERTY_ID, Color.white);
                _renderers[i].SetPropertyBlock(_propertyBlock);
            }

            //Release the extra sprites
            for (int i = _act.MaxSprites; i < MediaConstants.MAX_EFFECT_SPRITES; i++)
            {
                if (_spriteRenderers[i] == null)
                    break; //No more sprite renderers
                ObjectPoll.EffectSpriteRenderersPoll = _spriteRenderers[i].gameObject;
                _spriteRenderers[i] = null;
                _renderers[i] = null;
            }

            _lastUpdate = Globals.Time;
            _currentFrame = 0;
            _actionDelay = MediaConstants.ACTION_DELAY_BASE_TIME * _act.Actions[0].delay;
        }

        private bool OnFramesDone()
        {
            //If frames are done and audio is done, return true
            if (AudioSource.clip == null || !AudioSource.isPlaying || (AudioSource.clip.length - AudioSource.time) <= 0.005f)
                return true;

            //Otherwise set end time and disable all renders
            _endTime = Globals.Time + AudioSource.clip.length - AudioSource.time;

            for (int i = 0; i < _spriteRenderers.Length; i++)
                _spriteRenderers[i].enabled = false;

            return false;
        }

        private void Awake()
        {
            _renderers = new Renderer[MediaConstants.MAX_EFFECT_SPRITES];
            _spriteRenderers = new SpriteRenderer[MediaConstants.MAX_EFFECT_SPRITES];
            _propertyBlock = new MaterialPropertyBlock();
            AudioSource = gameObject.GetComponent<AudioSource>();
            ReloadAct();
        }

        private void OnDisable()
        {
            for (int i = 0; i < _act.MaxSprites; i++)
            {
                ObjectPoll.EffectSpriteRenderersPoll = _spriteRenderers[i].gameObject;
                _spriteRenderers[i] = null;
            }
            Destroy(gameObject);
        }
    }
}
