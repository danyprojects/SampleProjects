using RO.Common;
using RO.Containers;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace RO.Media
{
    public static class CursorAnimator
    {
        public sealed class Updater
        {
            public void UpdateCursorAnimation()
            {
                CursorAnimator.UpdateCursorAnimation();
            }
        }

        public enum Animations : int
        {
            Idle = 0,
            Chat,
            Portal,
            Cast,
            Attack,
            Click,
            Blocked,
            PickUp,
            Camera,
            IdleUI //Special UI idle for priorities
        }

        public enum CursorModes : int
        {
            Hardware = UnityEngine.CursorMode.Auto,
            Software = UnityEngine.CursorMode.ForceSoftware
        }

        public enum Levels : int
        {
            Zero = 0,
            One,
            Two,
            Three,
            Four,
            Five,
            Six,
            Seven,
            Eight,
            Nine,
            Ten
        }

        private static int[] _animationLengths = new int[(int)Animations.IdleUI];

        private static readonly float[] ACTION_DELAYS = new float[] { 4 *  MediaConstants.ACTION_DELAY_BASE_TIME, //Idle
                                                                      4 *  MediaConstants.ACTION_DELAY_BASE_TIME, //Chat
                                                                      8 *  MediaConstants.ACTION_DELAY_BASE_TIME, //Portal
                                                                      4 *  MediaConstants.ACTION_DELAY_BASE_TIME  //Cast
                                                                    };
        private static CursorAnimationData _cursorData = null;
        private static float _lastUpdate = float.MinValue;
        private static float _actionDelay = 0;
        private static int _currentFrame = 0;
        private static int _cursorDown = 0;
        private static Levels _level = Levels.Zero;
        private static Animations _animation = Animations.Idle;
        private static bool[] _animFlags = new bool[(int)Animations.IdleUI + 1];
        private static int[] _animToPriority = new int[(int)Animations.IdleUI + 1];
        private static Animations[] _priorityToAnim = new Animations[(int)Animations.IdleUI + 1];

        public static CursorModes CursorMode = CursorModes.Hardware;

        public static Levels Level
        {
            get
            {
                return _level;
            }
            set
            {
                _level = value;
                Globals.UI.IsScrollingAllowed = value == 0;
                SetCursorTexture();
            }
        }

        /// <summary>
        /// Updates cursor if there is an animation
        /// </summary>
        private static void UpdateCursorAnimation()
        {
            if (Globals.Time - _lastUpdate < _actionDelay)
                return;

            SetCursorTexture();

            //TODO: NPC talk animation froze at the end in RO
            _currentFrame = (_currentFrame + 1) % _cursorData._cursorAnimations[(int)_animation].cursorFrames.Length;
            _lastUpdate = Globals.Time; // cursor animation doesn't have to be frame indepent
        }

        public static Animations GetAnimation()
        {
            return _animation;
        }

        /// <summary>
        /// Call this to flag a cursor animation to active
        /// </summary>
        public static void SetAnimation(Animations animation)
        {
            //return if animation is already active / inactve
            if (_animFlags[(int)animation])
                return;
            _animFlags[(int)animation] = true;

            //Don't change anything if animation priority is lower than current animation
            if (_animToPriority[(int)animation] >= _animToPriority[(int)_animation])
                return;

            //Get next animation. Fallback to idle
            _animation = Animations.Idle;
            for (int i = 0; i < _priorityToAnim.Length; i++)
                if (_animFlags[(int)_priorityToAnim[i]])
                {
                    _animation = _priorityToAnim[i];
                    break;
                }

            //Cursor does not update alone during any animation above attack so freeze it
            if (_animation >= Animations.Attack)
            {
                //During click or pickup we need to check if cursor was already down
                _currentFrame = (_animation == Animations.Click || _animation == Animations.PickUp) ? _cursorDown : 0;
                _actionDelay = float.MaxValue;
                SetCursorTexture();
            }
            else
            {
                _currentFrame = 0;
                _lastUpdate = float.MinValue; //make sure mouse changes immediately
                _actionDelay = ACTION_DELAYS[(int)_animation];
                UpdateCursorAnimation();
            }
        }

        /// <summary>
        /// Call this to flag a cursor animation to inactve
        /// </summary>
        public static void UnsetAnimation(Animations animation)
        {
            //return if animation is already active / inactve
            if (!_animFlags[(int)animation])
                return;
            _animFlags[(int)animation] = false;

            if (_animToPriority[(int)animation] < _animToPriority[(int)_animation])
                return;

            //Get next animation. Fallback to idle
            _animation = Animations.Idle;
            for (int i = 0; i < _priorityToAnim.Length; i++)
                if (_animFlags[(int)_priorityToAnim[i]])
                {
                    _animation = _priorityToAnim[i];
                    break;
                }

            _currentFrame = 0;
            //Cursor does not update alone during any animation above attack so freeze it
            if (_animation >= Animations.Attack)
            {
                _actionDelay = float.MaxValue;
                SetCursorTexture();
            }
            else
            {
                _lastUpdate = float.MinValue; //make sure mouse changes immediately
                _actionDelay = ACTION_DELAYS[(int)_animation];
                UpdateCursorAnimation();
            }
        }

        /// <summary>
        /// Does nothing unless animation is in pickup or click. Otherwise shows the pressed animation for those
        /// </summary>
        public static void OnCursorDown()
        {
            _cursorDown = 1;

            if (_animation != Animations.Click && _animation != Animations.PickUp)
                return;

            // pick up and click both have a second frame for mouse down
            _currentFrame = 1;
            SetCursorTexture();
        }

        /// <summary>
        /// Does nothing unless animation is in pickup or click. Otherwise shows the released animation for those
        /// </summary>
        public static void OnCursorUp()
        {
            _cursorDown = 0;

            if (_animation != Animations.Click && _animation != Animations.PickUp)
                return;
            _currentFrame = 0;
            SetCursorTexture();
        }

        /// <summary>
        /// Sets the cursor texture according to configurations
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetCursorTexture()
        {
            var textureId = _cursorData._cursorAnimations[(int)_animation].cursorFrames[_currentFrame].textureId;
            textureId += GetLevelOffset();
            Cursor.SetCursor(_cursorData._cursorAnimations[(int)_animation].textures[textureId],
                             _cursorData._cursorAnimations[(int)_animation].cursorFrames[_currentFrame].hotspot,
                             (UnityEngine.CursorMode)CursorMode);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetLevelOffset()
        {
            return (int)Level * _animationLengths[(int)_animation];
        }

        static CursorAnimator()
        {
            _animation = Animations.Idle;
            _actionDelay = ACTION_DELAYS[0]; // Idle delay
            _cursorData = (CursorAnimationData)AssetBundleProvider.LoadMiscBundleAsset<ScriptableObject>("cursordata");

            //0 index is highest priority
            _animToPriority[(int)Animations.Idle] = 9;
            _animToPriority[(int)Animations.Attack] = 8;
            _animToPriority[(int)Animations.Cast] = 7;
            _animToPriority[(int)Animations.IdleUI] = 6;
            _animToPriority[(int)Animations.Portal] = 5;
            _animToPriority[(int)Animations.Chat] = 4;
            _animToPriority[(int)Animations.PickUp] = 3;
            _animToPriority[(int)Animations.Blocked] = 2;
            _animToPriority[(int)Animations.Click] = 1;
            _animToPriority[(int)Animations.Camera] = 0;

            _priorityToAnim[0] = Animations.Camera;
            _priorityToAnim[1] = Animations.Click;
            _priorityToAnim[2] = Animations.Blocked;
            _priorityToAnim[3] = Animations.PickUp;
            _priorityToAnim[4] = Animations.Chat;
            _priorityToAnim[5] = Animations.Portal;
            _priorityToAnim[6] = Animations.Idle;
            _priorityToAnim[7] = Animations.Cast;
            _priorityToAnim[8] = Animations.Attack;
            _priorityToAnim[9] = Animations.Idle;

            _animationLengths[(int)Animations.Idle] = 6;
            _animationLengths[(int)Animations.Chat] = 7;
            _animationLengths[(int)Animations.Portal] = 5;
            _animationLengths[(int)Animations.Cast] = 4;
            _animationLengths[(int)Animations.Attack] = 1;
            _animationLengths[(int)Animations.Click] = 2;
            _animationLengths[(int)Animations.Blocked] = 1;
            _animationLengths[(int)Animations.PickUp] = 2;
            _animationLengths[(int)Animations.Camera] = 1;
        }
    }
}
