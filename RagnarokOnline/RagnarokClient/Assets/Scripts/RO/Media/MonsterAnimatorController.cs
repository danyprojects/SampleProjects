using RO.Common;
using RO.Databases;
using RO.MapObjects;
using UnityEngine;

namespace RO.Media
{
    public sealed class MonsterAnimatorController : MonoBehaviour
    {
        public class MonsterAnimatorData
        {
            public int CurrentFrame;
            public MediaConstants.MonsterActAnimations CurrentActAnimation;
            public Monster.Direction Direction;
        }

        private enum AnimationMasks : int
        {
            Idle = 1 << MediaConstants.MonsterActAnimations.Idle,
            Walking = 1 << MediaConstants.MonsterActAnimations.Walking,
            Attacking = 1 << MediaConstants.MonsterActAnimations.Attacking,
            ReceiveDmg = 1 << MediaConstants.MonsterActAnimations.ReceiveDmg,
            Dead = 1 << MediaConstants.MonsterActAnimations.Dead,

            None = 1 << 31
        }

        private MonsterAnimator _monsterAnimator = null;
        private MonsterAnimatorData _monsterAnimatorData = null;
        private GameObject _shadow = null;
        private SpriteRenderer _shadowRenderer = null;
        private AudioSource _animationAudio = null;
        private AnimationMasks _currentAnimationMask;
        private float _lastUpdate = 0f, _actionDelay = 1f, _animationSpeed = 0f, _startDelay = 0f;
        private Color _color = Color.white;
        private bool _fadeOnDeath = false;

        private float _frameInterval
        {
            get
            {
                return _actionDelay * _animationSpeed + _startDelay;
            }
        }

        public Monster MonsterInstance { get; private set; }
        public int FadeTag { get; private set; } = int.MinValue;

        public void AnimateMonster(Monster monsterInstance)
        {
            MonsterInstance = monsterInstance;

            _monsterAnimatorData = new MonsterAnimatorData();
            _monsterAnimatorData.Direction = monsterInstance._direction;
            var spriteId = MonsterDb.Monsters[(int)monsterInstance._monsterInfo.dbId].sprId;
            ChangeMonsterSprite(spriteId);

            //Set shadow scaling and position
            float shadowScale = MonsterDb.MonsterShadows[(int)spriteId];
            _shadow.transform.localScale = new Vector3(shadowScale, shadowScale, shadowScale);
            _shadowRenderer = _shadow.GetComponent<SpriteRenderer>();
        }

        public void UpdateAnimations()
        {
            //If _lastUpdate is set to float.MaxValue, this if will always fail and the animation will be frozen until the next call of "PlayXXXXXXAnimation"
            if (Globals.Time - _lastUpdate >= _frameInterval)
            {
                _lastUpdate = _lastUpdate + _frameInterval;
                _startDelay = 0f; // frame delay needs to reset after any frame render so it does not affect afterframes

                //Get the next frame index from body.
                //temporary variable because we might need the previous frame value (in this.CurrentBodyFrame) inside CheckUpdateRendererOnAnimationEnd
                //This way we will not actually implement the frame unless we are the ones to update the renderer
                int nextFrame = _monsterAnimator.NextFrame;

                if (_currentAnimationMask == AnimationMasks.Dead)
                    Debug.Log("Dead frame: " + nextFrame);

                //If animation finished frame = 0 check if we need to update renderer this frame or not
                if (nextFrame == 0 && !CheckUpdateRendererOnAnimationEnd())
                    return;

                //Apply the frame increment and update renderers
                _monsterAnimatorData.CurrentFrame = nextFrame;

                //For frame skip. Maybe enough time has passed that we need to update again. Do it before renderers are updated if we're not on endFrame
                if (Globals.Time - _lastUpdate >= _frameInterval && (_monsterAnimator.NextFrame != 0))
                {
                    UpdateAnimations();
                    return;
                }

                _monsterAnimator.UpdateRenderer();
            }
        }

        public void PlayIdleAnimation()
        {
            //Idle animation does not reset on multiple requests
            if (_currentAnimationMask == AnimationMasks.Idle)
                return;

            _currentAnimationMask = AnimationMasks.Idle;
            _monsterAnimatorData.CurrentActAnimation = MediaConstants.MonsterActAnimations.Idle;
            _monsterAnimatorData.CurrentFrame = 0;
            _actionDelay = _monsterAnimator.ActionDelay;
            _startDelay = 0f;
            _animationSpeed = MediaConstants.ACTION_DELAY_BASE_TIME;
            _monsterAnimator.UpdateRenderer();
            _lastUpdate = Globals.Time;
        }

        public void PlayWalkAnimation(int movementSpeed)
        {
            _animationSpeed = (float)movementSpeed / Constants.DEFAULT_WALK_SPEED * MediaConstants.ACTION_DELAY_BASE_TIME;

            //Walk animation does not reset on multiple requests
            if (_currentAnimationMask == AnimationMasks.Walking)
                return;

            _currentAnimationMask = AnimationMasks.Walking;
            _monsterAnimatorData.CurrentActAnimation = MediaConstants.MonsterActAnimations.Walking;
            _monsterAnimatorData.CurrentFrame = 0;
            _startDelay = 0f;
            _actionDelay = _monsterAnimator.ActionDelay;
            _monsterAnimator.UpdateRenderer();
            _lastUpdate = Globals.Time;
        }

        public void PlayAttackAnimation(int attackSpeed)
        {
            _currentAnimationMask = AnimationMasks.Attacking;
            _monsterAnimatorData.CurrentActAnimation = MediaConstants.MonsterActAnimations.Attacking;
            _monsterAnimatorData.CurrentFrame = 0;

            _actionDelay = 1; //Ignore base action delay. Use delay of 1 so that only animation speed is used for attack animation
            _startDelay = 0f;

            //(2000 - animationTime)/10 = AttackSpeed taken from hercules. Calculate animation time from it
            //Further divide by 8 because 8 frames and by 1000 to convert to ms
            _animationSpeed = -(attackSpeed * 10 - 2000) * 2 / (float)_monsterAnimator.TotalFrames / 1000f;

            _monsterAnimator.UpdateRenderer();
            _lastUpdate = Globals.Time;
        }

        public void PlayCastAnimation(float startDelay)
        {
            _startDelay = startDelay;
            _actionDelay = _monsterAnimator.ActionDelay;
            _animationSpeed = MediaConstants.ACTION_DELAY_BASE_TIME;

            _currentAnimationMask = AnimationMasks.Attacking;
            _monsterAnimatorData.CurrentActAnimation = MediaConstants.MonsterActAnimations.Attacking;
            _monsterAnimatorData.CurrentFrame = 0;
            _monsterAnimator.UpdateRenderer();
            _lastUpdate = Globals.Time;
        }

        public void PlayReceiveDamageAnimation()
        {
            _actionDelay = _monsterAnimator.ActionDelay;
            _animationSpeed = MediaConstants.ACTION_DELAY_BASE_TIME;
            _startDelay = 0f;

            _currentAnimationMask = AnimationMasks.ReceiveDmg;
            _monsterAnimatorData.CurrentActAnimation = MediaConstants.MonsterActAnimations.ReceiveDmg;
            _monsterAnimatorData.CurrentFrame = 0;
            _monsterAnimator.UpdateRenderer();
            _lastUpdate = Globals.Time;
        }

        /// <summary>Plays the dead animation of the monster. Optionally fades out monster at the end</summary>
        /// <returns>Returns the duration of the animation in seconds. Includes the fading time if fade is enabled</returns>
        public float PlayDeadAnimation(bool fadeOnFinish)
        {
            //Set flag in case fade is to be overwritten during a dead animation
            _fadeOnDeath = fadeOnFinish;

            //Dead animation does not reset on multiple requests
            if (_currentAnimationMask == AnimationMasks.Dead)
                return 0;

            _currentAnimationMask = AnimationMasks.Dead;
            _monsterAnimatorData.CurrentActAnimation = MediaConstants.MonsterActAnimations.Dead;
            _monsterAnimatorData.CurrentFrame = 0;
            _actionDelay = _monsterAnimator.ActionDelay;
            _animationSpeed = MediaConstants.ACTION_DELAY_BASE_TIME;
            _startDelay = 0f;
            _monsterAnimator.UpdateRenderer();
            _lastUpdate = Globals.Time;

            return _frameInterval * _monsterAnimator.TotalFrames + (fadeOnFinish ? MediaConstants.UNIT_FADE_TIME : 0);
        }

        public int Fade(FadeDirection fadeDirection)
        {
            _monsterAnimator.Fade(fadeDirection);

            //Fade the shadow too
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            _shadowRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(MediaConstants.SHADER_UNIT_FADE_START_TIME_ID, Globals.TimeSinceLevelLoad);
            propertyBlock.SetFloat(MediaConstants.SHADER_UNIT_FADE_DIRECTION_ID, (float)fadeDirection);
            _shadowRenderer.SetPropertyBlock(propertyBlock);

            if (FadeTag < int.MaxValue)
                FadeTag++;
            else
                FadeTag = int.MinValue;

            return FadeTag;
        }

        public void SetColor(ref Color color)
        {
            //Colors should always be between 0-1 so a multiplication is actually reducing the color
            _color *= color;
            _monsterAnimator.SetColor(ref color);
        }

        public void UnsetColor(ref Color color)
        {
            //colors should always be between 0-1 so divion should nullify the color reduction
            _color.r /= color.r;
            _color.g /= color.g;
            _color.b /= color.b;
            _color.a /= color.a;
            _monsterAnimator.SetColor(ref color);
        }

        public void ChangedDirection()
        {
            _monsterAnimator.UpdateRenderer();
        }

        public void SetEnableRaycast(bool enable)
        {
            _monsterAnimator.EnableRaycast(enable);
        }

        public void SetPalette(Texture palette)
        {
            _monsterAnimator.SetPalette(palette);
        }

        private void ChangeMonsterSprite(MonsterSpriteIDs spriteId)
        {
            //Load the body animator
            GameObject monsterPrefab = AssetBundleProvider.LoadMonsterBundleAsset<GameObject>((int)spriteId);

            if (_monsterAnimator == null)
            {
                GameObject monsterAnimator = Instantiate(monsterPrefab, transform);
                _monsterAnimator = monsterAnimator.GetComponent<MonsterAnimator>();
            }
            //Get the animator reference, and pass it the prefab act + reference to this player animator
            _monsterAnimator.MonsterAnimatorData = _monsterAnimatorData;
            _monsterAnimator.Act = monsterPrefab.GetComponent<MonsterAnimator>().Act;
            _monsterAnimator.UpdateRenderer();
        }

        private bool CheckUpdateRendererOnAnimationEnd()
        {
            //animation does not change, return true to keep updating renderer normally (this will always trigger for walking and standby)
            if (_monsterAnimatorData.CurrentActAnimation == MediaConstants.MonsterNextAnimation[(int)_monsterAnimatorData.CurrentActAnimation])
                return true;

            //Freeze animator if next transition is none. Used in cases like dead
            if (MediaConstants.MonsterNextAnimation[(int)_monsterAnimatorData.CurrentActAnimation] == MediaConstants.MonsterActAnimations.None)
            {
                _lastUpdate = float.MaxValue;

                //If we reached the end of dead animation, automatically start a fade out
                if (_monsterAnimatorData.CurrentActAnimation == MediaConstants.MonsterActAnimations.Dead && _fadeOnDeath)
                    Fade(FadeDirection.Out);

                return false;
            }

            //animation changes on transition. On monsters this only happens on Atk and Receive Dmg and both will run Idle afterwards
            PlayIdleAnimation();
            return false;
        }

        private void Awake()
        {
            _animationAudio = gameObject.AddComponent<AudioSource>();
            SoundController.SetUnitAudioSourceParams(_animationAudio);
            _shadow = AssetBundleProvider.LoadMiscBundleAsset<GameObject>(MediaConstants.ASSET_NAME_SHADOW);
            _shadow = Instantiate(_shadow, transform, false);
        }
    }
}
