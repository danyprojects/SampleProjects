using UnityEngine;

namespace Bacterio
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Vector2 _targetAspect = new Vector2(16, 9);
        [SerializeField] private SpriteRenderer _background = null;

        private const int Z_POS = -30;
        private const float SMOOTH_TIME = 0.2F;

        //For background
        private MaterialPropertyBlock _backgroundProperties = null;

        //For target and movement
        private Transform _target = null;
        private Vector3 _prevTargetPos = Vector3.zero;
        private Vector3 _velocity = Vector3.zero;

        private void Awake()
        {
            AdjustResolution();
            _backgroundProperties = new MaterialPropertyBlock();
        }

        public void AssignTarget(Transform target)
        {
            _target = target;
            transform.localPosition = new Vector3(_target.position.x, _target.position.y, Z_POS);
            _prevTargetPos = _target.localPosition;
        }

        public void SetBackground(/*TODO: tell which background*/)
        {
            _background.enabled = true;
        }

        public void RunOnce()
        {
            WDebug.Assert(_target != null, "No target assigned to camera");

            //Move camera
            Vector3 offset = _target.localPosition - _prevTargetPos;
            var smooth = Vector3.SmoothDamp(transform.localPosition, _target.localPosition + offset, ref _velocity, SMOOTH_TIME);
            smooth.z = Z_POS;
            transform.localPosition = smooth;
            _prevTargetPos = _target.localPosition;

            //Move background 
            smooth.x /= _background.bounds.size.x;
            smooth.y /= _background.bounds.size.y;
            _background.GetPropertyBlock(_backgroundProperties);
            _backgroundProperties.SetVector(ShaderConstants.SHADER_OFFSET_PROPERTY_ID, smooth);
            _background.SetPropertyBlock(_backgroundProperties);
        }

        public void SnapToTarget()
        {
            transform.localPosition = _target.localPosition;
            _prevTargetPos = _target.localPosition;
        }

        public void ResetPosition()
        {
            _target = null;
            _prevTargetPos = Vector3.zero;
            transform.localPosition = new Vector3(0,0, Z_POS);
        }

        private void AdjustResolution()
        {
#if !UNITY_EDITOR
            Screen.SetResolution(800, 480, false);
            float screenRatio = Screen.width / (float)Screen.height;
            float targetRatio = _targetAspect.x / _targetAspect.y;
            if (Mathf.Approximately(screenRatio, targetRatio))
            {
                // Screen or window is the target aspect ratio: use the whole area.
                Camera.main.rect = new Rect(0, 0, 1, 1);
            }
            else if (screenRatio > targetRatio)
            {
                // Screen or window is wider than the target: pillarbox.
                float normalizedWidth = targetRatio / screenRatio;
                float barThickness = (1f - normalizedWidth) / 2f;
                Camera.main.rect = new Rect(barThickness, 0, normalizedWidth, 1);
            }
            else
            {
                // Screen or window is narrower than the target: letterbox.
                float normalizedHeight = screenRatio / targetRatio;
                float barThickness = (1f - normalizedHeight) / 2f;
                Camera.main.rect = new Rect(0, barThickness, 1, normalizedHeight);
            }
#endif
        }
    }
}