using RO.Containers;
using System;
using UnityEngine;

namespace RO.Media
{
    public class EquipmentAnimator : MonoBehaviour
    {
        //This has to be here due to limitations on generating the sprites on editor
#if UNITY_EDITOR
        public Act __Act { set { _act = value; } }
#endif
        private const int NORMAL_MATERIAL_INDEX = 0;
        private const int ZWRITE_MATERIAL_INDEX = 1;

        public PlayerAnimatorController.PlayerAnimatorData PlayerAnimatorData { get; set; }
        public Action UpdateMovingHeadgearRenderer = null;

        private MaterialPropertyBlock _propertyBlock = null;
        private MeshRenderer _meshRenderer = null;
        private BoxCollider _collider = null;
        [SerializeField] private Act _act = null;
        private float _lastUpdate = 0f;
        private int _currentFrame = 0;
        private int _framesPerDirection = 0;
        private Act.Action CurrentAction
        {
            get
            {
                return _act.Actions[(int)PlayerAnimatorData.CurrentActAnimation * 8 + PlayerAnimatorData.Direction.HeadCamera];
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

        public void UpdateRenderer()
        {
            UpdateRenderer(PlayerAnimatorData.CurrentBodyFrame);
        }

        public void SetPalette(Texture palette)
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

        private void MovingHeadgearRenderer()
        {
            //Not all moving headgears move in all frames so we need to check as we don't differenciate this on object build time
            if (CurrentAction.Frames.Length <= 3)
                return;

            //Each moving headgear can have it's own delay
            if (Time.time - _lastUpdate >= CurrentAction.delay * MediaConstants.ACTION_DELAY_BASE_TIME)
            {
                _currentFrame = (_currentFrame + 1) % _framesPerDirection;
                UpdateRenderer(_currentFrame);
                _lastUpdate = Time.time;
            }
        }

        private void NormalHeadgearRenderer()
        {
            //Empty function, normal headgears don't have anything to update on their own
            return;
        }

        private void UpdateRenderer(int frameIndex)
        {
            Sprite sprite = _act.Sprites[CurrentAction.Frames[frameIndex].frameData.SpriteId[0]];

            Vector2 headAttachPoint = CurrentAction.Frames[frameIndex].attachPoint;
            Act.Action.Frame.FrameData frameData = CurrentAction.Frames[frameIndex].frameData;
            int width = sprite.texture.width;
            int height = sprite.texture.height;

            Vector2 pos;
            pos.x = PlayerAnimatorData.BodyAttachPoint.x - headAttachPoint.x + frameData.PositionOffset[0].x;
            pos.y = -PlayerAnimatorData.BodyAttachPoint.y + headAttachPoint.y - frameData.PositionOffset[0].y;

            //Update the renderer material
            _meshRenderer.GetPropertyBlock(_propertyBlock, NORMAL_MATERIAL_INDEX);
            _propertyBlock.SetTexture(MediaConstants.SHADER_TEXTURE_PROPERTY_ID, sprite.texture);
            _propertyBlock.SetVector(MediaConstants.SHADER_DIMENSIONS_PROPERTY_ID, new Vector2(width, height));
            _propertyBlock.SetColor(MediaConstants.SHADER_VCOLOR_PROPERTY_ID, frameData.Color[0]);
            _propertyBlock.SetVector(MediaConstants.SHADER_OFFSET_PROPERTY_ID, new Vector3(pos.x, pos.y, frameData.IsMirrored[0]));
            _propertyBlock.SetVector(MediaConstants.SHADER_SCALE_PROPERTY_ID, frameData.Scale[0]); //Apply scale in shader to not fuck up rotations
            _meshRenderer.SetPropertyBlock(_propertyBlock, NORMAL_MATERIAL_INDEX);

            //Update the zwrite material
            _meshRenderer.GetPropertyBlock(_propertyBlock, ZWRITE_MATERIAL_INDEX);
            _propertyBlock.SetTexture(MediaConstants.SHADER_TEXTURE_PROPERTY_ID, sprite.texture);
            _propertyBlock.SetVector(MediaConstants.SHADER_DIMENSIONS_PROPERTY_ID, new Vector2(width, height));
            _propertyBlock.SetVector(MediaConstants.SHADER_OFFSET_PROPERTY_ID, new Vector3(pos.x, pos.y, frameData.IsMirrored[0]));
            _propertyBlock.SetVector(MediaConstants.SHADER_SCALE_PROPERTY_ID, frameData.Scale[0]); //Apply scale in shader to not fuck up rotations
            _meshRenderer.SetPropertyBlock(_propertyBlock, ZWRITE_MATERIAL_INDEX);

            // update headgear transform       
            gameObject.transform.localPosition = new Vector3(pos.x, pos.y, _act.OrderInLayer);
            gameObject.transform.localEulerAngles = new Vector3(0, 0, -frameData.Rotation[0]);

            //Update collider
            Vector2 colliderSize;
            colliderSize.x = Mathf.Abs(sprite.bounds.size.x);
            colliderSize.y = Mathf.Abs(sprite.bounds.size.y);
            _collider.size = colliderSize;
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

            //Check if it's a moving headgear. Moving headgears all have more than 3 frames per idle animation. Where 3 frames is 1 per head position
            //All moving headgears move in idle action
            if (_act.Actions[0].Frames.Length > 3)
                UpdateMovingHeadgearRenderer = this.MovingHeadgearRenderer;
            else
                UpdateMovingHeadgearRenderer = this.NormalHeadgearRenderer;

            _framesPerDirection = _act.Actions[0].Frames.Length / 3;
        }

        private void Awake()
        {
            _meshRenderer = gameObject.GetComponent<MeshRenderer>();
            _propertyBlock = new MaterialPropertyBlock();
            _collider = gameObject.GetComponent<BoxCollider>();
        }
    }
}
