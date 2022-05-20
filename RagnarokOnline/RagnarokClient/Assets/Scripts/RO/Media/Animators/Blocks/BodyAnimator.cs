using RO.Containers;
using UnityEngine;

namespace RO.Media
{
    public class BodyAnimator : MonoBehaviour
    {
        //This has to be here due to limitations on generating the sprites on editor
#if UNITY_EDITOR
        public Act __Act { set { _act = value; } }
#endif

        private const int NORMAL_MATERIAL_INDEX = 0;
        private const int ZWRITE_MATERIAL_INDEX = 1;

        //Static array of attachpoints for when head direction differs from body direction
        private static readonly int[] AttachPointFrames = new int[15] { 1, 2, 2, 2, 2, 2, 2, 0, 1, 1, 1, 1, 1, 1, 2 };

        private MaterialPropertyBlock _propertyBlock = null;
        private MeshRenderer _meshRenderer = null;
        private BoxCollider _collider = null;
        private AudioClip[] _audioClips = null;
        private AudioSource _audioSink = null;
        [SerializeField] private Act _act = null;

        private Act.Action CurrentAction
        {
            get
            {
                return _act.Actions[(int)PlayerAnimatorData.CurrentActAnimation * 8 + PlayerAnimatorData.Direction.BodyCamera];
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
                return (PlayerAnimatorData.CurrentBodyFrame + 1) % CurrentAction.Frames.Length;
            }
        }
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
        public PlayerAnimatorController.PlayerAnimatorData PlayerAnimatorData
        {
            get; set;
        }

        public void UpdateRenderer()
        {
            var frame = CurrentAction.Frames[PlayerAnimatorData.CurrentBodyFrame];
            Sprite sprite = _act.Sprites[frame.frameData.SpriteId[0]];

            Act.Action.Frame.FrameData frameData = CurrentAction.Frames[PlayerAnimatorData.CurrentBodyFrame].frameData;

            //Play the sound if there is one
            if (frame.eventId != -1)
                _audioSink.PlayOneShot(_audioClips[frame.eventId]);

            int width = sprite.texture.width;
            int height = sprite.texture.height;

            //Update the renderer material
            _meshRenderer.GetPropertyBlock(_propertyBlock, NORMAL_MATERIAL_INDEX);
            _propertyBlock.SetTexture(MediaConstants.SHADER_TEXTURE_PROPERTY_ID, sprite.texture);
            _propertyBlock.SetVector(MediaConstants.SHADER_DIMENSIONS_PROPERTY_ID, new Vector2(width, height));
            _propertyBlock.SetColor(MediaConstants.SHADER_VCOLOR_PROPERTY_ID, frameData.Color[0]);
            _propertyBlock.SetVector(MediaConstants.SHADER_OFFSET_PROPERTY_ID, new Vector3(frameData.PositionOffset[0].x,
                                                                                           frameData.PositionOffset[0].y,
                                                                                           frameData.IsMirrored[0]));
            _propertyBlock.SetVector(MediaConstants.SHADER_SCALE_PROPERTY_ID, frameData.Scale[0]); //Apply scale in shader to not fuck up rotations
            _meshRenderer.SetPropertyBlock(_propertyBlock, NORMAL_MATERIAL_INDEX);

            //Update the zwrite material
            _meshRenderer.GetPropertyBlock(_propertyBlock, ZWRITE_MATERIAL_INDEX);
            _propertyBlock.SetTexture(MediaConstants.SHADER_TEXTURE_PROPERTY_ID, sprite.texture);
            _propertyBlock.SetVector(MediaConstants.SHADER_DIMENSIONS_PROPERTY_ID, new Vector2(width, height));
            _propertyBlock.SetVector(MediaConstants.SHADER_OFFSET_PROPERTY_ID, new Vector3(frameData.PositionOffset[0].x,
                                                                                           frameData.PositionOffset[0].y,
                                                                                           frameData.IsMirrored[0]));
            _propertyBlock.SetVector(MediaConstants.SHADER_SCALE_PROPERTY_ID, frameData.Scale[0]); //Apply scale in shader to not fuck up rotations
            _meshRenderer.SetPropertyBlock(_propertyBlock, ZWRITE_MATERIAL_INDEX);

            //Update the transform properties
            gameObject.transform.localPosition = new Vector3(frameData.PositionOffset[0].x, -frameData.PositionOffset[0].y, 0);
            gameObject.transform.localEulerAngles = new Vector3(0, 0, -frameData.Rotation[0]);

            //Update the collider
            Vector2 colliderSize;
            colliderSize.x = Mathf.Abs(sprite.bounds.size.x);
            colliderSize.y = Mathf.Abs(sprite.bounds.size.y);
            _collider.size = colliderSize;

            //Update attach point for other parts to use
            UpdateBodyAttachPoint();
        }

        public void UpdateBodyAttachPoint()
        {
            if (PlayerAnimatorData.Direction.HeadCamera != PlayerAnimatorData.Direction.BodyCamera)
            {
                int frame = AttachPointFrames[PlayerAnimatorData.Direction.HeadCamera - PlayerAnimatorData.Direction.BodyCamera + 7];
                PlayerAnimatorData.BodyAttachPoint = CurrentAction.Frames[frame].attachPoint;
            }
            else
                PlayerAnimatorData.BodyAttachPoint = CurrentAction.Frames[PlayerAnimatorData.CurrentBodyFrame].attachPoint;
        }

        public void SetPalette(ref Texture palette)
        {
            _act.Palette = palette;
            _meshRenderer.GetPropertyBlock(_propertyBlock, NORMAL_MATERIAL_INDEX);
            _propertyBlock.SetTexture(MediaConstants.SHADER_PALETTE_PROPERTY_ID, _act.Palette);
            _meshRenderer.SetPropertyBlock(_propertyBlock, NORMAL_MATERIAL_INDEX);
        }

        public void SetColor(ref Color color)
        {
            _meshRenderer.GetPropertyBlock(_propertyBlock, NORMAL_MATERIAL_INDEX);
            _propertyBlock.SetColor(MediaConstants.SHADER_TINT_PROPERTY_ID, color);
            _meshRenderer.SetPropertyBlock(_propertyBlock, NORMAL_MATERIAL_INDEX);
        }

        public void Fade(FadeDirection fadeDirection)
        {
            _meshRenderer.GetPropertyBlock(_propertyBlock, NORMAL_MATERIAL_INDEX);
            _propertyBlock.SetFloat(MediaConstants.SHADER_UNIT_FADE_START_TIME_ID, Common.Globals.TimeSinceLevelLoad);
            _propertyBlock.SetFloat(MediaConstants.SHADER_UNIT_FADE_DIRECTION_ID, (float)fadeDirection);
            _meshRenderer.SetPropertyBlock(_propertyBlock, NORMAL_MATERIAL_INDEX);
        }

        private void ReloadAct()
        {
            //Re-apply palette and default color from new act
            _meshRenderer.GetPropertyBlock(_propertyBlock, NORMAL_MATERIAL_INDEX);
            _propertyBlock.SetTexture(MediaConstants.SHADER_PALETTE_PROPERTY_ID, _act.Palette);
            _propertyBlock.SetColor(MediaConstants.SHADER_TINT_PROPERTY_ID, Color.white);
            //By default start with no fade
            _propertyBlock.SetFloat(MediaConstants.SHADER_UNIT_FADE_START_TIME_ID, -1);
            _propertyBlock.SetFloat(MediaConstants.SHADER_UNIT_FADE_DIRECTION_ID, (float)FadeDirection.In);
            _meshRenderer.SetPropertyBlock(_propertyBlock, NORMAL_MATERIAL_INDEX);

            for (int i = 0; i < _act.Events.Length; i++)
                _audioClips[i] = AssetBundleProvider.LoadSoundBundleAsset(_act.Events[i]);
        }

        private void Awake()
        {
            _meshRenderer = gameObject.GetComponent<MeshRenderer>();
            _collider = gameObject.GetComponent<BoxCollider>();
            _propertyBlock = new MaterialPropertyBlock();
            _audioSink = gameObject.GetComponent<AudioSource>();
            _audioClips = new AudioClip[MediaConstants.MAX_PLAYER_AUDIO_CLIPS];
        }

    }
}
