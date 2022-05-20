using RO.Common;
using RO.Databases;
using RO.MapObjects;
using UnityEngine;

namespace RO.Media
{
    public sealed class UIPlayerAnimatorController : MonoBehaviour
    {
        public class PlayerAnimatorData
        {
            public int CurrentBodyFrame;
            public Vector2 BodyAttachPoint;
            public MediaConstants.PlayerActAnimations CurrentActAnimation;
            public Character.Direction Direction;
        }

        private enum AnimationMasks : int
        {
            Idle = 1 << MediaConstants.PlayerActAnimations.Idle,
            Sitting = 1 << MediaConstants.PlayerActAnimations.Sitting,
            Dead = 1 << MediaConstants.PlayerActAnimations.Dead,
            Walking = 1 << MediaConstants.PlayerActAnimations.Walking,
            PickUp = 1 << MediaConstants.PlayerActAnimations.PickUp,
            Freeze1 = 1 << MediaConstants.PlayerActAnimations.Freeze1,
            Freeze2 = 1 << MediaConstants.PlayerActAnimations.Freeze2,
            Casting = 1 << MediaConstants.PlayerActAnimations.Casting,
            Standby = 1 << MediaConstants.PlayerActAnimations.Standby,
            Attacking1 = 1 << MediaConstants.PlayerActAnimations.Attacking1,
            Attacking2 = 1 << MediaConstants.PlayerActAnimations.Attacking2,
            Attacking3 = 1 << MediaConstants.PlayerActAnimations.Attacking3,
            ReceiveDmg = 1 << MediaConstants.PlayerActAnimations.ReceiveDmg,

            None = 1 << 31
        }

        //animator references
        private UIBodyAnimator _bodyAnimator = null;
        private UICharacterPartAnimator _headAnimator = null;
        private UICharacterPartAnimator _headgearTopAnimator = null;
        private UICharacterPartAnimator _headgearMidAnimator = null;
        private UICharacterPartAnimator _headgearLowerAnimator = null;
        private UICharacterPartAnimator _weaponAnimator = null;
        private UICharacterPartAnimator _shieldAnimator = null;

        //internal animator fields
        //private GameObject _shadow = null; // Shadow not required in UI
        private PlayerAnimatorData _playerAnimatorData = null;
        private AnimationMasks _currentAnimationMask;
        private MediaConstants.CastingAnimation _currentCastingAnimation = null;
        private bool _freezeOnFinish = true;
        private int _endFrame = 16; // Endframe should be 16 for all animations other than casting. This is because only casting might end at weird values
        private float _lastUpdate = 0f, _actionDelay = 1f, _animationSpeed = 0f, _startDelay = 0f;
        private float _frameInterval
        {
            get
            {
                return _actionDelay * _animationSpeed + _startDelay;
            }
        }

        //internal copies of the player data we are animating
        private Gender _gender;
        private Jobs _job;
        private bool _isMounted = false;
        private WeaponShieldAnimatorIDs _viewingWeaponType = WeaponShieldAnimatorIDs.None;

        ///<summary>Call to assign the this animator to a player.</summary>
        public void AnimatePlayer(Gender gender, Jobs job, Character.Direction direction, int airstyleId, bool isMounted)
        {
            _playerAnimatorData = new PlayerAnimatorData();
            _gender = gender;
            _job = job;
            _playerAnimatorData.Direction = direction;

            ChangeHairstyle(airstyleId);
            IsMounted = isMounted; // This will also update body and enable / disabled the shield / weapon animators accordingly

            //Sort body and head animators
            _bodyAnimator.transform.SetSiblingIndex(0);
            _headAnimator.transform.SetSiblingIndex(1);
        }

        ///<summary>Call every main loop to update the rendering</summary>
        public void UpdateAnimations()
        {
            // Update headers here due to moving headgears
            if ((_currentAnimationMask & (AnimationMasks.Idle | AnimationMasks.Sitting | AnimationMasks.Dead)) > 0)
            {
                //Each headgear will update individually due to them possibly having different frame intervals
                _headgearTopAnimator?.UpdateMovingHeadgearRenderer();
                _headgearMidAnimator?.UpdateMovingHeadgearRenderer();
                _headgearLowerAnimator?.UpdateMovingHeadgearRenderer();
            }
            //Remaining animations all share the same number of frames for all parts
            //If _lastUpdate is set to float.MaxValue, this if will always fail and the animation will be frozen until the next call of "PlayXXXXXXAnimation"
            else if (Globals.Time - _lastUpdate >= _frameInterval)
            {
                //Last update needs to be updated before start delay is reset
                _lastUpdate = _lastUpdate + _frameInterval;
                _startDelay = 0f; // frame delay needs to reset after any frame render so it does not affect afterframes

                //Get the next frame index from body.
                //temporary variable because we might need the previous frame value (in this.CurrentBodyFrame) inside CheckUpdateRendererOnAnimationEnd
                //This way we will not actually implement the frame unless we are the ones to update the renderer
                int nextFrame = _bodyAnimator.NextFrame;

                //If animation finished (frame = 0 or equal to _endFrame) check if we need to update renderer this frame or not
                if ((nextFrame % _endFrame == 0) && !CheckUpdateRendererOnAnimationEnd())
                    return;

                //Apply the frame increment and update renderers
                _playerAnimatorData.CurrentBodyFrame = nextFrame;
                UpdatePartAnimatorRenderers();
            }
        }

        //Methods to start an animation
        public void PlayIdleAnimation()
        {
            //Idle animation does not reset on multiple requests
            if (_currentAnimationMask == AnimationMasks.Idle)
                return;

            _currentAnimationMask = AnimationMasks.Idle;
            _playerAnimatorData.CurrentActAnimation = MediaConstants.PlayerActAnimations.Idle;
            _playerAnimatorData.CurrentBodyFrame = 0;
            UpdatePartAnimatorRenderers();
        }

        public void PlayWalkAnimation(int movementSpeed)
        {
            //Walk animation speed has to be updated regardless if we're resetting or not
            //movement speed of 150 will default animation speed to the grf speed
            _animationSpeed = (float)movementSpeed / Constants.DEFAULT_WALK_SPEED * MediaConstants.ACTION_DELAY_BASE_TIME;

            //Walk animation does not reset on multiple requests
            if (_currentAnimationMask == AnimationMasks.Walking)
                return;

            _currentAnimationMask = AnimationMasks.Walking;
            _actionDelay = _bodyAnimator.ActionDelay;
            _playerAnimatorData.CurrentActAnimation = MediaConstants.PlayerActAnimations.Walking;
            _playerAnimatorData.CurrentBodyFrame = 0;
            _endFrame = 16;

            UpdatePartAnimatorRenderers();
            _lastUpdate = Globals.Time;
        }

        public void PlayDeadAnimation()
        {
            //Dead animation does not reset on multiple requests
            if (_currentAnimationMask == AnimationMasks.Dead)
                return;

            _currentAnimationMask = AnimationMasks.Dead;
            _playerAnimatorData.CurrentActAnimation = MediaConstants.PlayerActAnimations.Dead;
            _playerAnimatorData.CurrentBodyFrame = 0;

            UpdatePartAnimatorRenderers();
        }

        public void PlayStandbyAnimation()
        {
            //Standby animation does not reset on multiple requests
            if (_currentAnimationMask == AnimationMasks.Standby)
                return;

            _currentAnimationMask = AnimationMasks.Standby;
            _actionDelay = _bodyAnimator.ActionDelay;
            _playerAnimatorData.CurrentActAnimation = MediaConstants.PlayerActAnimations.Standby;
            _playerAnimatorData.CurrentBodyFrame = 0;
            _endFrame = 16;
            _animationSpeed = MediaConstants.ACTION_DELAY_BASE_TIME;

            UpdatePartAnimatorRenderers();
            _lastUpdate = Globals.Time;
        }

        private void OnDisable()
        {
            if (_headAnimator != null)
            {
                _headAnimator.gameObject.SetActive(false);
                _bodyAnimator.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (_headAnimator != null)
            {
                _headAnimator.gameObject.SetActive(true);
                _bodyAnimator.gameObject.SetActive(true);
                _headAnimator.UpdateRenderer();
                _bodyAnimator.UpdateRenderer();
            }
        }

        public void PlayPickUpAnimation()
        {
            //Pick up resets on multiple animation requests
            _currentAnimationMask = AnimationMasks.PickUp;
            _actionDelay = _bodyAnimator.ActionDelay;
            _playerAnimatorData.CurrentActAnimation = MediaConstants.PlayerActAnimations.PickUp;
            _playerAnimatorData.CurrentBodyFrame = 0;
            _endFrame = 16;
            _animationSpeed = MediaConstants.ACTION_DELAY_BASE_TIME;

            UpdatePartAnimatorRenderers();
            _lastUpdate = Globals.Time;
        }

        public void PlayReceivedDmgAnimation()
        {
            //Received dmg resets on multiple animation resets
            _currentAnimationMask = AnimationMasks.ReceiveDmg;
            _actionDelay = _bodyAnimator.ActionDelay;
            _playerAnimatorData.CurrentActAnimation = MediaConstants.PlayerActAnimations.ReceiveDmg;
            _playerAnimatorData.CurrentBodyFrame = 0;
            _endFrame = 16;
            _animationSpeed = MediaConstants.ACTION_DELAY_BASE_TIME;

            UpdatePartAnimatorRenderers();
            _lastUpdate = Globals.Time;
        }

        public void PlayFreezeAnimation()
        {
            //Freeze animation does not reset on multiple requests
            if (_currentAnimationMask == AnimationMasks.Freeze1)
                return;

            _currentAnimationMask = AnimationMasks.Freeze1;
            _playerAnimatorData.CurrentActAnimation = MediaConstants.PlayerActAnimations.Freeze1;
            _playerAnimatorData.CurrentBodyFrame = 0;
            UpdatePartAnimatorRenderers();

            _lastUpdate = float.MaxValue; // Freeze only has 1 frame, no need to keep it looping
        }

        /// <summary>This will play the corresponding attack animation according to the equipped weapon type</summary>
        public void PlayAttackAnimation(int attackSpeed, float startDelay, bool freezeOnFinish = true)
        {
            MediaConstants.PlayerActAnimations atkAnim = MediaConstants.WeaponAnimations[(int)_job * (int)_viewingWeaponType];
            PlayAttackAnimation(attackSpeed, (PlayerAttackAnimations)atkAnim, startDelay, freezeOnFinish);
        }

        /// <summary>Call this if need to specify the exact attack animation to run. For example in throw stone which is always Attack1 regardless of weapon </summary>
        public void PlayAttackAnimation(int attackSpeed, PlayerAttackAnimations attackAnim, float startDelay, bool freezeOnFinish = true)
        {
            _playerAnimatorData.CurrentActAnimation = (MediaConstants.PlayerActAnimations)attackAnim;
            _playerAnimatorData.CurrentBodyFrame = 0;
            _endFrame = 16;
            _actionDelay = 1; //Ignore base action delay. Use delay of 1 so that only animation speed is used for attack animation
            _currentAnimationMask = (AnimationMasks)(1 << (int)_playerAnimatorData.CurrentActAnimation);

            //(2000 - animationTime)/10 = AttackSpeed taken from hercules. Calculate animation time from it
            //Further divide by 8 because 8 frames and by 1000 to convert to ms
            _animationSpeed = -(attackSpeed * 10 - 2000) * 2 / 8f / 1000f;

            _startDelay = startDelay;
            _freezeOnFinish = freezeOnFinish;

            UpdatePartAnimatorRenderers();
            _lastUpdate = Globals.Time;
        }

        /// <summary>Call this for the default casting animation</summary>
        public void PlayCastingAnimation(float startDelay, bool freezeOnFinish = true)
        {
            PlayCastingAnimation(0, startDelay, freezeOnFinish);
        }

        /// <summary> Call this if need to specify the exact cast animation to run</summary>
        public void PlayCastingAnimation(JobCastingAnimations castAnim, float startDelay, bool freezeOnFinish = true)
        {
            _currentAnimationMask = AnimationMasks.Casting;
            _playerAnimatorData.CurrentActAnimation = MediaConstants.PlayerActAnimations.Casting;

            //Get the casting animation and set the start and end frames
            //Store the current casting animation for deciding on the animation transition on animation end
            _currentCastingAnimation = MediaConstants.CastingAnimations[MediaConstants.MAX_CASTING_ANIMATIONS * (int)_job + (int)castAnim];
            _playerAnimatorData.CurrentBodyFrame = _currentCastingAnimation.startFrame;
            _endFrame = _currentCastingAnimation.endFrame;
            _actionDelay = _currentCastingAnimation.actionDelay;

            _animationSpeed = MediaConstants.ACTION_DELAY_BASE_TIME;
            UpdatePartAnimatorRenderers();
            _lastUpdate = Globals.Time;

            _startDelay = startDelay;
            _freezeOnFinish = freezeOnFinish;
        }

        //Methods to change a part renderer
        public void ChangeHairstyle(int hairstyleId)
        {
            //If it's the first head for this playerAnimator, instantiate the head. 
            //Heads are mandatory so no need to poll them as we already poll players
            if (_headAnimator == null)
            {
                GameObject partAnimator = AssetBundleProvider.LoadMiscBundleAsset<GameObject>(MediaConstants.UI_CHARACTER_PART_PREFAB);
                partAnimator = Instantiate(partAnimator, transform, false);
                //Get the bodyAnimator script reference for the just instanced component
                _headAnimator = partAnimator.GetComponent<UICharacterPartAnimator>();
            }

            //instantiate the original head 
            GameObject hairPrefab = AssetBundleProvider.LoadHairstyleBundleAsset<GameObject>(hairstyleId, _gender);
            hairPrefab = Instantiate(hairPrefab);

            //Get the act from the head
            _headAnimator.PlayerAnimatorData = _playerAnimatorData;
            _headAnimator.Act = hairPrefab.GetComponent<HeadAnimator>().Act;
            _headAnimator.UpdateRenderer();

            Destroy(hairPrefab); //don't need it anymore and we don't poll heads
        }

        public void ChangeJob(Jobs job)
        {
            _job = job;

            //If it's the first body for this playerAnimator, instantiate the body. 
            //Bodies are mandatory so no need to poll them as we already poll players
            if (_bodyAnimator == null)
            {
                GameObject bodyAnimator = AssetBundleProvider.LoadMiscBundleAsset<GameObject>(MediaConstants.UI_CHARACTER_BODY_PREFAB);
                bodyAnimator = Instantiate(bodyAnimator, transform, false);
                //Get the bodyAnimator script reference for the just instanced component
                _bodyAnimator = bodyAnimator.GetComponent<UIBodyAnimator>();
            }

            //instantiate the original body
            GameObject jobPrefab = AssetBundleProvider.LoadBodyBundleAsset<GameObject>(_job, _gender, _isMounted);
            jobPrefab = Instantiate(jobPrefab);

            //Get the act
            _bodyAnimator.PlayerAnimatorData = _playerAnimatorData;
            _bodyAnimator.Act = jobPrefab.GetComponent<BodyAnimator>().Act;

            //Because attach points change, need to update all renders
            UpdatePartAnimatorRenderers();

            Destroy(jobPrefab); //don't need it anymore and we don't poll bodies
        }

        public void ChangeTopHeadgear(ItemIDs headgearId)
        {
            ChangeHeadgear(ref _headgearTopAnimator, headgearId);
        }

        public void ChangeMiddleHeadgear(ItemIDs headgearId)
        {
            ChangeHeadgear(ref _headgearMidAnimator, headgearId);
        }

        public void ChangeLowerHeadgear(ItemIDs headgearId)
        {
            ChangeHeadgear(ref _headgearLowerAnimator, headgearId);
        }

        public void ChangeWeapon(ItemIDs weaponId, bool isDualWielding = false)
        {
            if (weaponId == ItemIDs.None)
            {
                //Destroy if not null, we don't poll UI parts
                if (_weaponAnimator != null)
                {
                    Destroy(_weaponAnimator.gameObject);
                    _weaponAnimator = null;
                }
                return;
            }

            //Only instantiate a new weapon if this slot does not have one. Otherwise just replace the old
            if (_weaponAnimator == null)
            {
                GameObject weaponAnimatorObj = AssetBundleProvider.LoadMiscBundleAsset<GameObject>(MediaConstants.UI_CHARACTER_PART_PREFAB);
                weaponAnimatorObj = Instantiate(weaponAnimatorObj, transform, false);
                _weaponAnimator = weaponAnimatorObj.GetComponent<UICharacterPartAnimator>();
            }

            //Load the weapon animator
            GameObject weaponPrefab = GetWeaponPrefab(weaponId, isDualWielding);
            //If couldn't get a weapon
            if (weaponPrefab == null)
            {
                //Destroy if not null, we don't poll UI parts
                if (_weaponAnimator != null)
                {
                    Destroy(_weaponAnimator.gameObject);
                    _weaponAnimator = null;
                }
                return;
            }
            //Set act
            _weaponAnimator.PlayerAnimatorData = _playerAnimatorData;
            _weaponAnimator.Act = weaponPrefab.GetComponent<EquipmentAnimator>().Act;
            _weaponAnimator.UpdateRenderer();
            SortChildren();

            //Weapons are polled for actual players
            ObjectPoll.EquipmentAnimatorsPoll = weaponPrefab;

            //Weapons can be enabled / disabled depending on mount status
            //This only disables the graphical rendering. This way when we unmount, we have the correct gear showing
            CheckMountedStatus();
        }

        public void ChangeShield(ItemIDs shieldId)
        {
            if (shieldId == ItemIDs.None)
            {
                //Destroy if not null, we don't poll UI parts
                if (_shieldAnimator != null)
                {
                    Destroy(_shieldAnimator.gameObject);
                    _shieldAnimator = null;
                }
                return;
            }

            //Only instanciate a new shield if this slot does not have one. Otherwise just replace the old
            if (_shieldAnimator == null)
            {
                //Try to get a shield animator from the poll
                GameObject shieldAnimatorObj = AssetBundleProvider.LoadMiscBundleAsset<GameObject>(MediaConstants.UI_CHARACTER_PART_PREFAB);
                shieldAnimatorObj = Instantiate(shieldAnimatorObj, transform, false);
                //Get the animator reference, and pass it the prefab act + reference to this player animator
                _shieldAnimator = shieldAnimatorObj.GetComponent<UICharacterPartAnimator>();
            }

            //Load the shield animator
            Jobs fallbackJob = MediaConstants.ShieldClassFallbackId[(int)_job];
            GameObject shieldPrefab = AssetBundleProvider.LoadShieldBundleAsset<GameObject>((int)ItemDb.Items[(int)shieldId].viewId, fallbackJob, _gender);
            //If no shield was found
            if (shieldPrefab == null)
            {
                if (_shieldAnimator != null)
                {
                    ObjectPoll.ShieldAnimatorsPoll = _shieldAnimator.gameObject;
                    _shieldAnimator = null;
                }
                return;
            }
            shieldPrefab = Instantiate(shieldPrefab);

            _shieldAnimator.PlayerAnimatorData = _playerAnimatorData;
            _shieldAnimator.Act = shieldPrefab.GetComponent<ShieldAnimator>().Act;
            _shieldAnimator.UpdateRenderer();
            SortChildren();

            ObjectPoll.ShieldAnimatorsPoll = shieldPrefab; // Shields are polled for actual player

            //Weapons can be enabled / disabled depending on mount status
            //This only disables the graphical rendering. This way when we unmount, we have the correct gear showing
            CheckMountedStatus();
        }

        public void ChangedDirection()
        {
            //We have a copy to direction, which is a class so it updates on it's own
            UpdatePartAnimatorRenderers();
        }

        public bool IsMounted
        {
            get
            {
                return _isMounted;
            }
            set
            {
                _isMounted = value;
                CheckMountedStatus();
                // change job takes into account if we're mounted or not. It also updates all the renders, so we make sure to do this after (de)activating weapons and shields
                ChangeJob(_job);
            }
        }

        //Methods to change a part palette. Might remove / replace with receiving the palette ID instead of texture
        public void SetHairstylePalette(Texture palette)
        {
            _headAnimator.SetPalette(palette);
        }

        public void SetBodyPalette(Texture palette)
        {
            _bodyAnimator.SetPalette(ref palette);
        }

        public void SetTopHeadgearPalette(Texture palette)
        {
            _headgearTopAnimator?.SetPalette(palette);
        }

        public void SetMiddleHeadgearPalette(Texture palette)
        {
            _headgearMidAnimator?.SetPalette(palette);
        }

        public void SetLowHeadgearPalette(Texture palette)
        {
            _headgearLowerAnimator?.SetPalette(palette);
        }

        public void SetWeaponPalette(Texture palette)
        {
            _weaponAnimator?.SetPalette(palette);
        }

        public void SetShieldPalette(Texture palette)
        {
            _shieldAnimator?.SetPalette(palette);
        }

        //Internal animator methods
        private void UpdatePartAnimatorRenderers()
        {
            _bodyAnimator.UpdateRenderer();
            _headAnimator.UpdateRenderer();
            _headgearTopAnimator?.UpdateRenderer();
            _headgearMidAnimator?.UpdateRenderer();
            _headgearLowerAnimator?.UpdateRenderer();

            _weaponAnimator?.UpdateRenderer();

            //Only update shield renderer if character is not mounted since mounted characters do not show shields
            if (!_isMounted)
                _shieldAnimator?.UpdateRenderer();
        }

        private void ChangeHeadgear(ref UICharacterPartAnimator headgearAnimator, ItemIDs headgearId)
        {
            if (headgearId == ItemIDs.None)
            {
                //Destroy if not null, we don't poll UI parts
                if (headgearAnimator != null)
                {
                    Destroy(headgearAnimator.gameObject);
                    headgearAnimator = null;
                }
                return;
            }

            //Only instanciate a new heagear if this slot does not have one. Otherwise just replace the old
            if (headgearAnimator == null)
            {
                //Try to get a body animator from the poll
                GameObject headgearAnimatorObj = AssetBundleProvider.LoadMiscBundleAsset<GameObject>(MediaConstants.UI_CHARACTER_PART_PREFAB);
                headgearAnimatorObj = Instantiate(headgearAnimatorObj, transform, false);
                headgearAnimator = headgearAnimatorObj.GetComponent<UICharacterPartAnimator>();
            }

            //Load the body animator
            GameObject headgearPrefab = AssetBundleProvider.LoadHeadgearBundleAsset<GameObject>((int)ItemDb.Items[(int)headgearId].sprId, _gender);
            headgearPrefab = Instantiate(headgearPrefab);

            //Pass a reference to this playerAnimator, assign the act and update the renderer
            headgearAnimator.PlayerAnimatorData = _playerAnimatorData;
            headgearAnimator.Act = headgearPrefab.GetComponent<EquipmentAnimator>().Act;
            headgearAnimator.UpdateRenderer();
            SortChildren();

            ObjectPoll.EquipmentAnimatorsPoll = headgearPrefab; // we poll headgears for actual players
        }

        // Returns True if renderer needs to be updated and false if it doesnt</returns>
        private bool CheckUpdateRendererOnAnimationEnd()
        {
            //Idle, sitting, dead should never get here because they are treated differently

            //animation does not change, return true to keep updating renderer normally (this will always trigger for walking and standby)
            if (_playerAnimatorData.CurrentActAnimation == MediaConstants.PlayerNextAnimation[(int)_playerAnimatorData.CurrentActAnimation])
                return true;

            //Freeze animator if told to not transition animation
            if (!_freezeOnFinish)
            {
                _freezeOnFinish = true;
                _lastUpdate = float.MaxValue;
                return false;
            }

            //animation changes            
            //Different behavior for different current mask. Ifs are sorted from more to less likely to happen
            if ((_currentAnimationMask & (AnimationMasks.Freeze1 | AnimationMasks.ReceiveDmg | AnimationMasks.Attacking1 | AnimationMasks.Attacking2 | AnimationMasks.Attacking3)) > 0)
                PlayStandbyAnimation();
            else if (_currentAnimationMask == AnimationMasks.Casting)
                OnCastAnimationEnd();
            else if (_currentAnimationMask == AnimationMasks.PickUp)
                PlayIdleAnimation();

            return false;
        }

        private GameObject GetWeaponPrefab(ItemIDs weaponId, bool isDualWielding)
        {
            int sprId;
            if (isDualWielding && _viewingWeaponType != WeaponShieldAnimatorIDs.None)
            {
                _viewingWeaponType = MediaConstants.DualWieldLookup[(int)_viewingWeaponType + (int)ItemDb.Items[(int)weaponId].viewId];
                sprId = (int)_viewingWeaponType;
            }
            else
            {
                _viewingWeaponType = ItemDb.Items[(int)weaponId].viewId;
                sprId = (int)ItemDb.Items[(int)weaponId].sprId;
            }

            //Load the weapon animator
            Jobs fallbackJob = MediaConstants.WeaponClassFallbackId[(int)_job];
            GameObject weaponPrefab = AssetBundleProvider.LoadWeaponBundleAsset<GameObject>(sprId, fallbackJob, _gender, _isMounted);
            //fallback
            if (weaponPrefab == null)
                weaponPrefab = AssetBundleProvider.LoadWeaponBundleAsset<GameObject>((int)ItemDb.Items[(int)weaponId].viewId, fallbackJob, _gender, _isMounted);

            return weaponPrefab;
        }

        private void OnCastAnimationEnd()
        {
            //Play the corresponding next animation according to current cast, If theres no end delay and animation has finished
            if (_currentCastingAnimation.nextAnimation == MediaConstants.PlayerActAnimations.Standby)
                PlayStandbyAnimation();
            else if (_currentCastingAnimation.nextAnimation == MediaConstants.PlayerActAnimations.Idle)
                PlayIdleAnimation();
            else if (_currentCastingAnimation.nextAnimation == MediaConstants.PlayerActAnimations.None)
                _lastUpdate = float.MaxValue; //No next animation, freeze on the current frame            
        }

        private void CheckMountedStatus()
        {
            // disable shield animator while mounted for all classes
            _shieldAnimator?.gameObject.SetActive(!_isMounted);

            //Deactivate weapon if class is mounted and not a knight, lk, crusader or paladin
            bool activateWeapon = _isMounted && _job != Jobs.Knight && _job != Jobs.LordKnight && _job != Jobs.Crusader && _job != Jobs.Paladin;
            _weaponAnimator?.gameObject.SetActive(!activateWeapon);
        }

        private void SortChildren()
        {
            int index = 2;
            _shieldAnimator?.transform.SetSiblingIndex(index++);
            _weaponAnimator?.transform.SetSiblingIndex(index++);
            _headgearLowerAnimator?.transform.SetSiblingIndex(index++);
            _headgearMidAnimator?.transform.SetSiblingIndex(index++);
            _headgearTopAnimator?.transform.SetSiblingIndex(index++);
        }

        private void Awake()
        {

        }
    }
}
