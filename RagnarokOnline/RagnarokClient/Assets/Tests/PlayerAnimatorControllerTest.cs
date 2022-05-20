using RO;
using RO.Common;
using RO.Databases;
using RO.MapObjects;
using RO.Media;
using UnityEngine;

namespace Tests
{

    public class PlayerAnimatorControllerTest : MonoBehaviour
    {
        [System.Serializable]
        public struct CharacterData
        {
            public Gender Gender;
            public Jobs Job;
            public int Hairstyle;
        };

        [System.Serializable]
        public struct PlayerVariables
        {
            public bool PlayerOn;
            public int HeadDirection, BodyDirection;
            public int AttackSpeed;
            public int MovementSpeed;
            public bool IsMounted;
            public Color Color;

            public PlayerVariables(bool a = false)
            {
                PlayerOn = true;
                HeadDirection = 0;
                BodyDirection = 0;
                AttackSpeed = 100;
                MovementSpeed = 150;
                IsMounted = false;
                Color = Color.white;
            }
        }

        [System.Serializable]
        public struct EquipSettings
        {
            public ItemIDs WeaponId, ShieldId, TopHeadgear, MidHeadgear, LowHeadgear;

            public EquipSettings(bool f = false)
            {
                WeaponId = ItemIDs.None;
                ShieldId = ItemIDs.None;
                TopHeadgear = ItemIDs.None;
                MidHeadgear = ItemIDs.None;
                LowHeadgear = ItemIDs.None;
            }
        }

        [System.Serializable]
        public struct AnimationSettings
        {
            public bool DoAnimationTransition;
            public JobCastingAnimations CastAnimation;
            public PlayerAttackAnimations AttackAnimation;
            public float StartDelay;
            public MediaConstants.PlayerActAnimations Animation;
            public bool AutoAttack;

            public AnimationSettings(bool f = false)
            {
                DoAnimationTransition = true;
                CastAnimation = JobCastingAnimations.ChampDefaultCast;
                AttackAnimation = PlayerAttackAnimations.Attacking1;
                StartDelay = 0f;
                Animation = MediaConstants.PlayerActAnimations.Idle;
                AutoAttack = false;
            }
        }

        [System.Serializable]
        public struct PaletteSettings
        {
            public Texture TopHeadgearPalette, MidHeadgearPalette, LowHeadgearPalette;
            public Texture BodyPalette, HeadPalette;
            public Texture WeaponPalette, ShieldPalette;

            public PaletteSettings(bool f = false)
            {
                TopHeadgearPalette = null; MidHeadgearPalette = null; LowHeadgearPalette = null;
                BodyPalette = null; HeadPalette = null;
                WeaponPalette = null; ShieldPalette = null;
            }
        }

        public PlayerVariables playerVariables = new PlayerVariables();
        public EquipSettings equipSettings = new EquipSettings();
        public AnimationSettings animSettings = new AnimationSettings();
        public PaletteSettings paletteSettings = new PaletteSettings();
        public CharacterData characterData = new CharacterData();

        private int prevHeadDirection = -1, prevBodyDirection = -1;
        private Character.Direction direction = new Character.Direction(0, 0);
        private float lastUpdate = 0f;
        private PlayerAnimatorController playerAnimator;

        // Start is called before the first frame update
        void Start()
        {
            Globals.Time = Time.time;
        }

        // Update is called once per frame
        void Update()
        {
            Globals.Time += Time.deltaTime;

            int animationSpeed = -(playerVariables.AttackSpeed * 10 - 2000);

            if (playerVariables.PlayerOn)
            {
                //Test getting a player from player poll
                if (playerAnimator == null)
                {
                    playerAnimator = ObjectPoll.PlayerAnimatorControllersPoll.GetComponent<PlayerAnimatorController>();
                    playerAnimator.AnimateCharacter(new Character(0));
                    playerAnimator.gameObject.transform.SetParent(gameObject.transform, false);
                    playerAnimator.transform.position = transform.position;
                }

                //Change gender                
                if (characterData.Gender != Gender.None)
                {
                    Character oldChar = playerAnimator.CharacterInstance;
                    oldChar._charInfo.gender = characterData.Gender;
                    playerAnimator.AnimateCharacter(oldChar);
                    characterData.Gender = Gender.None;
                }

                //Test change job
                if (characterData.Job != Jobs.None)
                {
                    playerAnimator.CharacterInstance._charInfo.job = characterData.Job;
                    playerAnimator.ChangeJob(characterData.Job);
                    characterData.Job = Jobs.None;
                }               

                //Test hairstyle
                if(characterData.Hairstyle != -1)
                {
                    playerAnimator.CharacterInstance._charInfo.hairstyle = characterData.Hairstyle;
                    playerAnimator.ChangeHairstyle(characterData.Hairstyle);
                    characterData.Hairstyle = -1;
                }

                //Test direction rendering
                if (prevHeadDirection != playerVariables.HeadDirection || prevBodyDirection != playerVariables.BodyDirection)
                {
                    direction.UpdateDirection(playerVariables.HeadDirection, playerVariables.BodyDirection);
                    playerAnimator.ChangedDirection();
                    prevHeadDirection = playerVariables.HeadDirection;
                    prevBodyDirection = playerVariables.BodyDirection;
                }

                //Test mounting
                if (playerAnimator.IsMounted != playerVariables.IsMounted)
                    playerAnimator.IsMounted = playerVariables.IsMounted;

                //Test auto attack animation
                if (animSettings.AutoAttack && ((Time.time - lastUpdate) >= (animationSpeed * 2 / 1000f)))
                {
                    playerAnimator.PlayAttackAnimation(playerVariables.AttackSpeed, animSettings.AttackAnimation, animSettings.StartDelay, animSettings.DoAnimationTransition);
                    lastUpdate = lastUpdate + (animationSpeed * 2 / 1000f);
                }
                else
                    switch (animSettings.Animation)  //Test running each animation
                    {
                        case MediaConstants.PlayerActAnimations.Idle: playerAnimator.PlayIdleAnimation(); break;
                        case MediaConstants.PlayerActAnimations.Walking: playerAnimator.PlayWalkAnimation(playerVariables.MovementSpeed); break;
                        case MediaConstants.PlayerActAnimations.Dead: playerAnimator.PlayDeadAnimation(); break;
                        case MediaConstants.PlayerActAnimations.Standby: playerAnimator.PlayStandbyAnimation(); break;
                        case MediaConstants.PlayerActAnimations.PickUp: playerAnimator.PlayPickUpAnimation(); break;
                        case MediaConstants.PlayerActAnimations.ReceiveDmg: playerAnimator.PlayReceivedDmgAnimation(); break;
                        case MediaConstants.PlayerActAnimations.Freeze1: playerAnimator.PlayFreezeAnimation(); break;
                        case MediaConstants.PlayerActAnimations.Casting: playerAnimator.PlayCastingAnimation(animSettings.CastAnimation, animSettings.StartDelay, animSettings.DoAnimationTransition); break;
                        case MediaConstants.PlayerActAnimations.Attacking1:
                        case MediaConstants.PlayerActAnimations.Attacking2:
                        case MediaConstants.PlayerActAnimations.Attacking3: playerAnimator.PlayAttackAnimation(playerVariables.AttackSpeed, animSettings.AttackAnimation, animSettings.StartDelay, animSettings.DoAnimationTransition); break;
                        default: break;
                    }


                //Test equip and dequip of headgears
                if (equipSettings.TopHeadgear != ItemIDs.None)
                    playerAnimator.ChangeTopHeadgear(equipSettings.TopHeadgear);
                if (equipSettings.MidHeadgear != ItemIDs.None)
                    playerAnimator.ChangeMiddleHeadgear(equipSettings.MidHeadgear);
                if (equipSettings.LowHeadgear != ItemIDs.None)
                    playerAnimator.ChangeLowerHeadgear(equipSettings.LowHeadgear);
                if (equipSettings.ShieldId != ItemIDs.None)
                    playerAnimator.ChangeShield(equipSettings.ShieldId);
                if (equipSettings.WeaponId != ItemIDs.None)
                    playerAnimator.ChangeWeapon(equipSettings.WeaponId);

                //Test updating animations
                playerAnimator.UpdateAnimations();
            }
            else
            {
                //Test releasing a player into the player poll
                if (playerAnimator == null)
                    return;
                ObjectPoll.PlayerAnimatorControllersPoll = playerAnimator.gameObject;
                playerAnimator = null;
            }
            RunPaletteTests(playerAnimator);

            if (playerVariables.Color != Color.black)
            {
                playerAnimator.SetColor(ref playerVariables.Color);
                playerVariables.Color = Color.black;
            }

            equipSettings.TopHeadgear = ItemIDs.None;
            equipSettings.MidHeadgear = ItemIDs.None;
            equipSettings.LowHeadgear = ItemIDs.None;
            equipSettings.ShieldId = ItemIDs.None;
            equipSettings.WeaponId = ItemIDs.None;
            animSettings.Animation = MediaConstants.PlayerActAnimations.None;
            paletteSettings.TopHeadgearPalette = null;
            paletteSettings.MidHeadgearPalette = null;
            paletteSettings.LowHeadgearPalette = null;
            paletteSettings.BodyPalette = null;
            paletteSettings.HeadPalette = null;
            paletteSettings.WeaponPalette = null;
            paletteSettings.ShieldPalette = null;
        }

        private void RunPaletteTests(PlayerAnimatorController playerAnimator)
        {
            if (paletteSettings.TopHeadgearPalette != null)
                playerAnimator.SetTopHeadgearPalette(paletteSettings.TopHeadgearPalette);
            if (paletteSettings.MidHeadgearPalette != null)
                playerAnimator.SetMiddleHeadgearPalette(paletteSettings.MidHeadgearPalette);
            if (paletteSettings.LowHeadgearPalette != null)
                playerAnimator.SetLowHeadgearPalette(paletteSettings.LowHeadgearPalette);
            if (paletteSettings.BodyPalette != null)
                playerAnimator.SetBodyPalette(paletteSettings.BodyPalette);
            if (paletteSettings.HeadPalette != null)
                playerAnimator.SetHairstylePalette(paletteSettings.HeadPalette);
            if (paletteSettings.WeaponPalette != null)
                playerAnimator.SetWeaponPalette(paletteSettings.WeaponPalette);
            if (paletteSettings.ShieldPalette != null)
                playerAnimator.SetShieldPalette(paletteSettings.ShieldPalette);

        }
    }
}