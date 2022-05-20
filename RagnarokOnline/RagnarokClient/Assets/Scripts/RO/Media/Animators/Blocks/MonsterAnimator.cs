using RO.Containers;
using UnityEngine;

namespace RO.Media
{
    public class MonsterAnimator : MonoBehaviour
    {
        //This has to be here due to limitations on generating the sprites on editor
#if UNITY_EDITOR
        public Act __Act { set { _act = value; } }
#endif

        private const int NORMAL_MATERIAL_INDEX = 0;
        private const int ZWRITE_MATERIAL_INDEX = 1;

        private Act.Action CurrentAction
        {
            get
            {
                return _act.Actions[(int)MonsterAnimatorData.CurrentActAnimation * 8 + MonsterAnimatorData.Direction.BodyCamera];
            }
        }
        private MaterialPropertyBlock _propertyBlock = null;
        private MeshRenderer[] _meshRenderers = new MeshRenderer[MediaConstants.MAX_MONSTER_SPRITES];
        private BoxCollider[] _spriteColliders = new BoxCollider[MediaConstants.MAX_MONSTER_SPRITES];
        private AudioSource _audioSink = null;
        private AudioClip[] _audioClips = new AudioClip[MediaConstants.MAX_MONSTER_AUDIO_CLIPS];

        private Vector3 _spriteColliderSize = Vector3.zero;
        [SerializeField] private Act _act = null;

        public MonsterAnimatorController.MonsterAnimatorData MonsterAnimatorData = null;
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
        public float ActionDelay
        {
            get
            {
                return CurrentAction.delay;
            }
        }
        public int NextFrame
        {
            get
            {
                return (MonsterAnimatorData.CurrentFrame + 1) % CurrentAction.Frames.Length;
            }
        }
        public int TotalFrames
        {
            get
            {
                return CurrentAction.Frames.Length;
            }
        }

        public void UpdateRenderer()
        {
            var frame = CurrentAction.Frames[MonsterAnimatorData.CurrentFrame];
            var frameData = frame.frameData;
            int i;

            //act files have -1 in index when no event is to be played
            if (frame.eventId != -1)
                if (frame.eventId > 0)
                    _audioSink.PlayOneShot(_audioClips[frame.eventId]);
            //TODO: Add the else for eventID == 0 which is to trigger the floating atk box (hit, crit, miss, etc)

            //update sprite renderers with actual sprite info
            Vector3 position = transform.parent.position;
            for (i = 0; i < frameData.SpriteId.Length; i++)
            {
                _meshRenderers[i].enabled = true;
                _spriteColliders[i].enabled = true;
                FillSpriteRenderer(i, frameData, ref position);
            }
            //disable the remaining sprite renderers
            for (int k = frameData.SpriteId.Length; k < _act.MaxSprites; k++)
            {
                _meshRenderers[k].enabled = false;
                _spriteColliders[k].enabled = false;
            }
        }

        public void SetPalette(Texture palette)
        {
            _act.Palette = palette;
            for (int i = 0; i < _act.MaxSprites; i++)
            {
                _meshRenderers[i].GetPropertyBlock(_propertyBlock, NORMAL_MATERIAL_INDEX);
                _propertyBlock.SetTexture(MediaConstants.SHADER_PALETTE_PROPERTY_ID, _act.Palette);
                _meshRenderers[i].SetPropertyBlock(_propertyBlock, NORMAL_MATERIAL_INDEX);
            }
        }

        public void SetColor(ref Color color)
        {
            for (int i = 0; i < _act.MaxSprites; i++)
            {
                _meshRenderers[i].GetPropertyBlock(_propertyBlock, NORMAL_MATERIAL_INDEX);
                _propertyBlock.SetColor(MediaConstants.SHADER_TINT_PROPERTY_ID, color);
                _meshRenderers[i].SetPropertyBlock(_propertyBlock, NORMAL_MATERIAL_INDEX);
            }
        }

        public void Fade(FadeDirection fadeDirection)
        {
            for (int i = 0; i < _act.MaxSprites; i++)
            {
                _meshRenderers[i].GetPropertyBlock(_propertyBlock, NORMAL_MATERIAL_INDEX);
                _propertyBlock.SetFloat(MediaConstants.SHADER_UNIT_FADE_START_TIME_ID, Common.Globals.TimeSinceLevelLoad);
                _propertyBlock.SetFloat(MediaConstants.SHADER_UNIT_FADE_DIRECTION_ID, (float)fadeDirection);
                _meshRenderers[i].SetPropertyBlock(_propertyBlock, NORMAL_MATERIAL_INDEX);
            }
        }

        public void EnableRaycast(bool enable)
        {
            for (int i = 0; i < _act.MaxSprites; i++)
                _spriteColliders[i].enabled = enable;
        }

        private void FillSpriteRenderer(int index, Act.Action.Frame.FrameData frameData, ref Vector3 parentPosition)
        {
            Sprite sprite = _act.Sprites[frameData.SpriteId[index]];

            //Update the shader property block
            int width = sprite.texture.width;
            int height = sprite.texture.height;

            //Update the renderer material
            _meshRenderers[index].GetPropertyBlock(_propertyBlock, NORMAL_MATERIAL_INDEX);
            _propertyBlock.SetTexture(MediaConstants.SHADER_TEXTURE_PROPERTY_ID, sprite.texture);
            _propertyBlock.SetColor(MediaConstants.SHADER_VCOLOR_PROPERTY_ID, frameData.Color[index]);
            _propertyBlock.SetVector(MediaConstants.SHADER_DIMENSIONS_PROPERTY_ID, new Vector2(width, height));
            _propertyBlock.SetVector(MediaConstants.SHADER_OFFSET_PROPERTY_ID, new Vector3(frameData.PositionOffset[index].x,
                                                                                           frameData.PositionOffset[index].y,
                                                                                           frameData.IsMirrored[index]));
            _propertyBlock.SetVector(MediaConstants.SHADER_SCALE_PROPERTY_ID, frameData.Scale[index]); //Apply scale in shader to not fuck up rotations
            _meshRenderers[index].SetPropertyBlock(_propertyBlock, NORMAL_MATERIAL_INDEX);

            //Update the zwrite material
            _meshRenderers[index].GetPropertyBlock(_propertyBlock, ZWRITE_MATERIAL_INDEX);
            _propertyBlock.SetTexture(MediaConstants.SHADER_TEXTURE_PROPERTY_ID, sprite.texture);
            _propertyBlock.SetVector(MediaConstants.SHADER_DIMENSIONS_PROPERTY_ID, new Vector2(width, height));
            _propertyBlock.SetVector(MediaConstants.SHADER_OFFSET_PROPERTY_ID, new Vector3(frameData.PositionOffset[index].x,
                                                                                           frameData.PositionOffset[index].y,
                                                                                           frameData.IsMirrored[index]));
            _propertyBlock.SetVector(MediaConstants.SHADER_SCALE_PROPERTY_ID, frameData.Scale[index]); //Apply scale in shader to not fuck up rotations
            _meshRenderers[index].SetPropertyBlock(_propertyBlock, ZWRITE_MATERIAL_INDEX);

            //Update the transform properties
            _meshRenderers[index].transform.localPosition = new Vector3(frameData.PositionOffset[index].x, -frameData.PositionOffset[index].y, index * -0.05f);
            _meshRenderers[index].transform.localEulerAngles = new Vector3(0, 0, -frameData.Rotation[index]);

            _spriteColliderSize.x = Mathf.Abs(sprite.bounds.size.x * frameData.Scale[index].x);
            _spriteColliderSize.y = Mathf.Abs(sprite.bounds.size.y * frameData.Scale[index].y);
            _spriteColliders[index].size = _spriteColliderSize;
        }

        private void ReloadAct()
        {
            //Get all the extra sprite renderers we'll need from the poll. Skip element if we already have one            
            for (int i = 1; i < _act.MaxSprites; i++)
            {
                if (_meshRenderers[i] != null)
                    continue;
                GameObject obj = ObjectPoll.MonsterRenderersPoll;
                obj.transform.SetParent(transform.parent, false);
                _meshRenderers[i] = obj.GetComponent<MeshRenderer>();
                _meshRenderers[i].enabled = false;
                _spriteColliders[i] = obj.GetComponent<BoxCollider>();
                _spriteColliders[i].enabled = false;
            }

            //update the starting shader properties 
            for (int i = 0; i < _act.MaxSprites; i++)
            {
                _meshRenderers[i].GetPropertyBlock(_propertyBlock, NORMAL_MATERIAL_INDEX);
                _propertyBlock.SetTexture(MediaConstants.SHADER_PALETTE_PROPERTY_ID, _act.Palette);
                _propertyBlock.SetColor(MediaConstants.SHADER_TINT_PROPERTY_ID, Color.white);
                //By default start with no fade
                _propertyBlock.SetFloat(MediaConstants.SHADER_UNIT_FADE_START_TIME_ID, -1);
                _propertyBlock.SetFloat(MediaConstants.SHADER_UNIT_FADE_DIRECTION_ID, (float)FadeDirection.In);
                _meshRenderers[i].SetPropertyBlock(_propertyBlock, NORMAL_MATERIAL_INDEX);
            }

            //Release the extra sprites
            for (int i = _act.MaxSprites; i < MediaConstants.MAX_MONSTER_SPRITES; i++)
            {
                if (_meshRenderers[i] == null)
                    break; //No more sprite renderers
                ObjectPoll.MonsterRenderersPoll = _meshRenderers[i].gameObject;
                _meshRenderers[i] = null;
            }

            //Get all the sound clips for this monster
            for (int i = 1; i < _act.Events.Length; i++)
                _audioClips[i] = AssetBundleProvider.LoadSoundBundleAsset(_act.Events[i]);
        }

        private void Awake()
        {
            _meshRenderers[0] = GetComponent<MeshRenderer>();
            _spriteColliders[0] = GetComponent<BoxCollider>();
            _propertyBlock = new MaterialPropertyBlock();
            _audioSink = GetComponent<AudioSource>();
            _spriteColliderSize.z = 0.05f;
        }

    }
}
