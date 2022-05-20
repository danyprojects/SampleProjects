using RO.Common;
using RO.Containers;
using UnityEngine;
using UnityEngine.UI;

namespace RO.Media
{
    public class UIBodyAnimator : MonoBehaviour
    {
        //Static array of attachpoints for when head direction differs from body direction
        private static readonly int[] AttachPointFrames = new int[15] { 1, 2, 2, 2, 2, 2, 2, 0, 1, 1, 1, 1, 1, 1, 2 };

        private Image _uiImage = null;
        private CanvasRenderer _renderer = null;
        private RectTransform _rectTransform = null;
        Color _dimensions = Color.clear;
        private Act _act = null;

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
        public UIPlayerAnimatorController.PlayerAnimatorData PlayerAnimatorData
        {
            get; set;
        }

        public void UpdateRenderer()
        {
            var frame = CurrentAction.Frames[PlayerAnimatorData.CurrentBodyFrame];
            Sprite sprite = _act.Sprites[frame.frameData.SpriteId[0]];
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

            //Update the transform properties
            Act.Action.Frame.FrameData frameData = CurrentAction.Frames[PlayerAnimatorData.CurrentBodyFrame].frameData;
            gameObject.transform.localPosition = new Vector3(frameData.PositionOffset[0].x * Constants.PIXELS_PER_UNIT,
                                                             -frameData.PositionOffset[0].y * Constants.PIXELS_PER_UNIT,
                                                             0);
            gameObject.transform.localScale = new Vector3(frameData.IsMirrored[0] * frameData.Scale[0].x, frameData.Scale[0].y, 1);
            gameObject.transform.localEulerAngles = new Vector3(0, 0, -frameData.Rotation[0]);

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
            _renderer.SetAlphaTexture(_act.Palette);
        }

        private void ReloadAct()
        {
            //Re-apply palette and default color from new act
            _renderer.SetAlphaTexture(_act.Palette);
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
