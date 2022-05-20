using RO.Common;
using RO.Containers;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace RO.Media
{
    public class UICharacterPartAnimator : MonoBehaviour
    {
        public UIPlayerAnimatorController.PlayerAnimatorData PlayerAnimatorData { get; set; }
        public Action UpdateMovingHeadgearRenderer = null;

        private Image _uiImage = null;
        private CanvasRenderer _renderer = null;
        private RectTransform _rectTransform = null;
        Color _dimensions = Color.clear;
        private Act _act = null;
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
            _renderer.SetAlphaTexture(_act.Palette);
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
            _uiImage.sprite = sprite;

            //Update the dimensions. Because of UI, we use the color as a hack to pass in dimensions for billinear
            Vector2 size;
            size.x = sprite.texture.width;
            size.y = sprite.texture.height;

            _dimensions.r = size.x;
            _dimensions.g = size.y;
            _dimensions.a = _uiImage.color.a;
            _uiImage.color = _dimensions;

            _rectTransform.sizeDelta = size;

            // update headgear transform
            Vector2 headAttachPoint = CurrentAction.Frames[frameIndex].attachPoint;
            Act.Action.Frame.FrameData frameData = CurrentAction.Frames[frameIndex].frameData;
            gameObject.transform.localPosition = new Vector3((PlayerAnimatorData.BodyAttachPoint.x - headAttachPoint.x + frameData.PositionOffset[0].x) * Constants.PIXELS_PER_UNIT,
                                                             (-PlayerAnimatorData.BodyAttachPoint.y + headAttachPoint.y - frameData.PositionOffset[0].y) * Constants.PIXELS_PER_UNIT,
                                                             0);
            gameObject.transform.localScale = new Vector3(frameData.IsMirrored[0] * frameData.Scale[0].x, frameData.Scale[0].y, 1);
            gameObject.transform.localEulerAngles = new Vector3(0, 0, -frameData.Rotation[0]);

        }

        private void ReloadAct()
        {
            //Re-apply palette and default color from new act
            _renderer.SetAlphaTexture(_act.Palette);

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
            _renderer = gameObject.GetComponent<CanvasRenderer>();
            _uiImage = gameObject.GetComponent<Image>();
            _rectTransform = (RectTransform)transform;
        }

        private void OnEnable()
        {
            if (_act != null)
                _renderer.SetAlphaTexture(_act.Palette);
        }
    }
}
