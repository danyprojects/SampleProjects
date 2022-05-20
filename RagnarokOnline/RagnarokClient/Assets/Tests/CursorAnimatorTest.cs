using RO.Common;
using RO.Media;
using UnityEngine;

namespace Tests
{
    public class CursorAnimatorTest : MonoBehaviour
    {
        public CursorAnimator.Animations Animation = CursorAnimator.Animations.Idle;
        public CursorAnimator.Levels Level = CursorAnimator.Levels.Zero;
        private CursorAnimator.Animations prevAnimation = CursorAnimator.Animations.Portal;

        private CursorAnimator.Updater _updater = null;

        public void Start()
        {
            Globals.Time = Time.time;
            _updater = new CursorAnimator.Updater();
        }

        public void Update()
        {
            Globals.Time += Time.deltaTime;
            _updater.UpdateCursorAnimation();

            if (Animation != prevAnimation)
            {
                CursorAnimator.SetAnimation(Animation);
                prevAnimation = Animation;
            }

            if (Input.mouseScrollDelta.y != 0)
            {
                int increment = Input.mouseScrollDelta.y > 0 ? 1 : -1;
                Level = Level + increment;
                Level = Level < 0 ? 0 : (int)Level > 10 ? (CursorAnimator.Levels)10 : Level;
                CursorAnimator.Level = Level;
            }

            if (Input.GetMouseButtonDown(0))
                CursorAnimator.OnCursorDown();
            if (Input.GetMouseButtonUp(0))
                CursorAnimator.OnCursorUp();
        }
    }
}
