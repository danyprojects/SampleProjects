using RO.Common;
using RO.IO;
using RO.MapObjects;
using UnityEngine;

namespace RO
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Vector2 _targetAspect = new Vector2(16, 9);
        [SerializeField] private readonly bool _destroyOnLoad = false;
        [SerializeField] private InputHandler _inputHandler = null;

        private const float SmoothTime = 0.2F;
        private const int MIN_CAMERA_SIZE = 45, MAX_CAMERA_SIZE = 68, DEFAULT_Y = 100;
        private const float MIN_ROTATION = 30f, MAX_ROTATION = 70f;
        private const float CAMERA_SPEED = 100f;
        private Vector3 _prevTargetPos = Vector3.zero;
        private Vector3 velocity = Vector3.zero;

        public int CameraDirection = 0;
        public bool RotationChanged = false;
        public bool DirectionChanged = false;
        public Transform Target { get; private set; }
        public Unit TargetUnit { get; private set; }

        void Start()
        {
            AdjustResolution();
            if (!_destroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
                DontDestroyOnLoad(Globals.Camera.gameObject);
            }
        }

        public void AssignTarget(Transform target, Unit unit)
        {
            Target = target;
            TargetUnit = unit;
            transform.localPosition = new Vector3(Target.position.x, Target.position.y + DEFAULT_Y, Target.position.z);
            transform.RotateAround(Target.position, Vector3.right, -50);
            transform.localEulerAngles = new Vector3(50, 0, 0);
            _prevTargetPos = Target.localPosition;
            CameraDirection = 0;
            RotationChanged = true;
            DirectionChanged = true;

            Shader.SetGlobalVector(Media.MediaConstants.SHADER_CAMERA_ROTATION_ID, transform.localEulerAngles);
        }

        public void UpdateCamera()
        {
            RotationChanged = false;
            DirectionChanged = false;

            //Check X and Y rotation
            if ((_inputHandler.ButtonsHeld & InputHandler.MouseButtons.Right) > InputHandler.MouseButtons.None)
            {
                Media.CursorAnimator.SetAnimation(Media.CursorAnimator.Animations.Camera);
                if (_inputHandler.CommandKeys == KeyBinder.CommandKeys.Shift)
                {
                    float mouseY = Input.GetAxisRaw("Mouse Y");
                    if (mouseY != 0)
                        if ((mouseY < 0 && transform.localEulerAngles.x > MIN_ROTATION) || (mouseY > 0 && transform.localEulerAngles.x < MAX_ROTATION))
                        {
                            transform.RotateAround(Target.position, transform.right, CAMERA_SPEED * mouseY * Time.deltaTime);
                            transform.eulerAngles = new Vector3(Mathf.Clamp(transform.eulerAngles.x, MIN_ROTATION, MAX_ROTATION), transform.eulerAngles.y, transform.eulerAngles.z);
                            //Set the camera angle for reading in shader
                            Shader.SetGlobalVector(Media.MediaConstants.SHADER_CAMERA_ROTATION_ID, transform.localEulerAngles);
                            RotationChanged = true;
                        }
                }
                else if (Input.GetAxisRaw("Mouse X") != 0)
                {
                    transform.RotateAround(Target.position, Vector3.up, CAMERA_SPEED * Input.GetAxisRaw("Mouse X") * Time.deltaTime);
                    RotationChanged = true;

                    //Shift angle by 22.5fº to the right so that 1 division can get the direction
                    //Remove 0.05f off the final angle to make sure it never lands in 360, otherwise we would get direction 8
                    int newDirection = Mathf.FloorToInt(((transform.localEulerAngles.y + 22.5f) % 360) - 0.05f) / 45;
                    DirectionChanged = newDirection != CameraDirection;
                    CameraDirection = newDirection;

                }
            }
            else //unset every time it's not held due to frame skip
                Media.CursorAnimator.UnsetAnimation(Media.CursorAnimator.Animations.Camera);

            //Check zoom in and zoom out
            if (_inputHandler.MouseScrollDelta > 0 && !Globals.UI.IsOverUI && Globals.Camera.orthographicSize > MIN_CAMERA_SIZE)
                Globals.Camera.orthographicSize -= 2;
            else if (_inputHandler.MouseScrollDelta < 0 && !Globals.UI.IsOverUI && Globals.Camera.orthographicSize < MAX_CAMERA_SIZE)
                Globals.Camera.orthographicSize += 2;

            UpdateCameraPosition();
        }

        public void SnapToTarget()
        {
            transform.localPosition = Target.localPosition;
            _prevTargetPos = Target.localPosition;
        }

        private void UpdateCameraPosition()
        {
            Vector3 offset = Target.localPosition - _prevTargetPos;
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Target.localPosition + offset, ref velocity, SmoothTime);
            _prevTargetPos = Target.localPosition;
        }

        private void AdjustResolution()
        {
#if !UNITY_EDITOR
            float screenRatio = Screen.width / (float)Screen.height;
            float targetRatio = _targetAspect.x / _targetAspect.y;
            if (Mathf.Approximately(screenRatio, targetRatio))
            {
                // Screen or window is the target aspect ratio: use the whole area.
                Globals.Camera.rect = new Rect(0, 0, 1, 1);
            }
            else if (screenRatio > targetRatio)
            {
                // Screen or window is wider than the target: pillarbox.
                float normalizedWidth = targetRatio / screenRatio;
                float barThickness = (1f - normalizedWidth) / 2f;
                Globals.Camera.rect = new Rect(barThickness, 0, normalizedWidth, 1);
            }
            else
            {
                // Screen or window is narrower than the target: letterbox.
                float normalizedHeight = screenRatio / targetRatio;
                float barThickness = (1f - normalizedHeight) / 2f;
                Globals.Camera.rect = new Rect(0, barThickness, 1, normalizedHeight);
            }
#endif
        }
    }
}