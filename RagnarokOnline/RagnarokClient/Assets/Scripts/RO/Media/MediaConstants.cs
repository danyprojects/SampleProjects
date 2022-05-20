using RO.Common;
using RO.Databases;
using UnityEngine;

namespace RO.Media
{
    public enum JobCastingAnimations : int
    {
        ChampDefaultCast = 0,
        ChampCriticalExplosion = 1,
        ChampThrowSpiritSphere = 2,
        ChampTrifectaCombo = 3
    }

    public enum PlayerAttackAnimations : int
    {
        Attacking1 = MediaConstants.PlayerActAnimations.Attacking1,
        Attacking2 = MediaConstants.PlayerActAnimations.Attacking2,
        Attacking3 = MediaConstants.PlayerActAnimations.Attacking3
    }

    public enum FloatingTextColor : int
    {
        White,
        Red
    }

    public enum FadeDirection : int
    {
        In = 1,
        Out = -1
    }

    public sealed class MediaConstants
    {
        //The values have to be these otherwise there will be inconsistencies with RO grfs
        public enum PlayerActAnimations : int
        {
            Idle = 0,
            Walking = 1,
            Sitting = 2,
            PickUp = 3,
            Standby = 4,
            Attacking1 = 5,
            ReceiveDmg = 6,
            Freeze1 = 7,
            Dead = 8,
            Freeze2 = 9,
            Attacking2 = 10,
            Attacking3 = 11,
            Casting = 12,

            None = 13
        }

        public enum MonsterActAnimations : int
        {
            Idle = 0,
            Walking = 1,
            Attacking = 2,
            ReceiveDmg = 3,
            Dead = 4,

            None = 5
        }

        

        //Class containing extra information on the casting animation that is not included in the .act file
        public sealed class CastingAnimation
        {
            public int startFrame;
            public int endFrame;
            public PlayerActAnimations nextAnimation;
            public int actionDelay;
        }

        //Others
        public const int MAX_MONSTER_SPRITES = 12; // TODO: need to check the real max
        public const int MAX_EFFECT_SPRITES = 5;
        public const int MAX_MONSTER_AUDIO_CLIPS = 7;
        public const int MAX_PLAYER_AUDIO_CLIPS = 3;
        public const int DEFAULT_MESH_EFFECTS_COUNT = 100;
        public const int DEFAULT_SPRITE_EFFECTS_COUNT = 100;
        public const int DEFAULT_CYLINDER_EFFECTS_COUNT = 100;
        public const int DEFAULT_FUNCTION_EFFECTS_COUNT = 100;
        public const int DEFAULT_FLOATING_BAR_COUNT = 100;
        public const int DEFAULT_CAST_CIRCLES_COUNT = 100;
        public const int DEFAULT_CAST_LOCK_ON_COUNT = 100;
        public const int DEFAULT_FLOATING_TEXT_COUNT = 100;
        public const int DEFAULT_UNIT_EFFECTS = 10;
        public const int DEFAULT_FUNC_SUB_EFFECTS = 5;
        public const uint MAX_NUMBER_DISPLAYED = 999999; // 6 renderers all with 9s
        public const float BLACK_FADE_TIME = 0.5f;
        public const float UNIT_FADE_TIME = 0.5f;

        //Animator strings
        public const string ASSET_NAME_SHADOW = "shadow";
        public const string UI_CHARACTER_PART_PREFAB = "UICharacterPart";
        public const string UI_CHARACTER_BODY_PREFAB = "UICharacterBody";

        //Misc media strings
        public const string UNIT_INFO_TEXT_PREFAB = "HoveredUnitInfoText";

        //Sprite shader property names are declared here, if the name changes it is only used as a string in here
        //These cannot be const strings due to the initializer Shader.PropertyToId not being able to run at compile time
        public static readonly int SHADER_MAIN_TEX_PROPERTY_ID = Shader.PropertyToID("_MainTex");
        public static readonly int SHADER_DIMENSIONS_PROPERTY_ID = Shader.PropertyToID("_Dimensions");
        public static readonly int SHADER_PALETTE_PROPERTY_ID = Shader.PropertyToID("_Palette");
        public static readonly int SHADER_TINT_PROPERTY_ID = Shader.PropertyToID("_Tint");
        public static readonly int SHADER_TINT2_PROPERTY_ID = Shader.PropertyToID("_Tint2");
        public static readonly int SHADER_TINT3_PROPERTY_ID = Shader.PropertyToID("_Tint3");
        public static readonly int SHADER_TINT4_PROPERTY_ID = Shader.PropertyToID("_Tint4");
        public static readonly int SHADER_VCOLOR_PROPERTY_ID = Shader.PropertyToID("_VColor");
        public static readonly int SHADER_POSITION_PROPERTY_ID = Shader.PropertyToID("_Position");
        public static readonly int SHADER_OFFSET_PROPERTY_ID = Shader.PropertyToID("_Offset");
        public static readonly int SHADER_ROTATION_PROPERTY_ID = Shader.PropertyToID("_Rotation");
        public static readonly int SHADER_CAMERA_ROTATION_ID = Shader.PropertyToID("_CameraRotation");
        public static readonly int SHADER_ORDER_IN_LAYER_ID = Shader.PropertyToID("_OrderInLayer");
        public static readonly int SHADER_SCALE_PROPERTY_ID = Shader.PropertyToID("_Scale");
        public static readonly int SHADER_TEXTURE_PROPERTY_ID = Shader.PropertyToID("_MainTex");
        public static readonly int SHADER_VERTEX_COLOR_PROPERTY_ID = Shader.PropertyToID("_VertexColor");
        public static readonly int SHADER_START_TIME_ID = Shader.PropertyToID("_StartTime");
        public static readonly int SHADER_WAVE_HEIGHT_ID = Shader.PropertyToID("_WaveHeight");
        public static readonly int SHADER_WAVE_SPEED_ID = Shader.PropertyToID("_WaveSpeed");
        public static readonly int SHADER_WAVE_PITCH_ID = Shader.PropertyToID("_WavePitch");
        public static readonly int SHADER_AMBIENT_LIGHT_COLOR_ID = Shader.PropertyToID("_AmbientLightColor");
        public static readonly int SHADER_AMBIENT_LIGHT_INTENSITY_ID = Shader.PropertyToID("_AmbientLighIntensity");
        public static readonly int SHADER_DIFFUSE_DIRECTION_ID = Shader.PropertyToID("_DiffuseDirection");
        public static readonly int SHADER_DIFFUSE_COLOR_ID = Shader.PropertyToID("_DiffuseColor");
        public static readonly int SHADER_LIGHTMAP_ID = Shader.PropertyToID("_ColorMap");
        public static readonly int SHADER_CYL_BOTTOM_WIDTH_ID = Shader.PropertyToID("_BottomWidth");
        public static readonly int SHADER_CYL_TOP_WIDTH_ID = Shader.PropertyToID("_TopWidth");
        public static readonly int SHADER_CYL_MIN_HEIGHT_ID = Shader.PropertyToID("_MinHeight");
        public static readonly int SHADER_CYL_MAX_HEIGHT_ID = Shader.PropertyToID("_MaxHeight");
        public static readonly int SHADER_CYL_HEIGHT_SPEED_ID = Shader.PropertyToID("_HeightSpeed");
        public static readonly int SHADER_CYL_ROTATE_SPEED_ID = Shader.PropertyToID("_RotateSpeed");
        public static readonly int SHADER_PROGRESS_COLOR_ID = Shader.PropertyToID("_ProgressColor");
        public static readonly int SHADER_UI_FADER_START_TIME_ID = Shader.PropertyToID("_UIFadeStartTime");
        public static readonly int SHADER_UI_BUFF_START_TIMES_ID = Shader.PropertyToID("_BuffStartTimes");
        public static readonly int SHADER_UI_BUFF_DURATIONS_ID = Shader.PropertyToID("_BuffDurations");
        public static readonly int SHADER_UNIT_FADE_START_TIME_ID = Shader.PropertyToID("_UnitFadeStartTime");
        public static readonly int SHADER_UNIT_FADE_DIRECTION_ID = Shader.PropertyToID("_UnitFadeDirection");

        public const string SHADER_BILLBOARD_ON = "BILLBOARD_ON";
        public const string SHADER_BILLBOARD_OFF = "BILLBOARD_OFF";

        // Action delay of 1 = 0.025s. This defaults to 10 animations a second when action delay is 4 (most animations)
        public const float ACTION_DELAY_BASE_TIME = 0.025f;

        public static readonly PlayerActAnimations[] WeaponAnimations = new PlayerActAnimations[((int)Jobs.Last + 1) * ((int)WeaponShieldAnimatorIDs.LastWeaponAnim + 1)];
        
        //Lookup table to match every job into a class that contains the weapon / shield animations
        public static readonly Jobs[] WeaponClassFallbackId = new Jobs[(int)Jobs.Last + 1];
        public static readonly Jobs[] ShieldClassFallbackId = new Jobs[(int)Jobs.Last + 1];

        public static readonly WeaponShieldAnimatorIDs[] DualWieldLookup = new WeaponShieldAnimatorIDs[(int)WeaponShieldAnimatorIDs.Shuriken * 2 + 2];

        //Lookup table for next player animation
        //Idle -> Idle, Walking -> walking, sitting -> sitting, pickup -> idle, standby -> standby, attacking1 -> standby, 
        //ReceiveDmg -> Standby, Freeze1 -> Freeze1, Dead -> Dead, freeze2 -> standby, attacking2 -> standby, attacking3 -> standby, casting -> This is handled separatly
        public static readonly PlayerActAnimations[] PlayerNextAnimation = new PlayerActAnimations[]
            {
                PlayerActAnimations.Idle, PlayerActAnimations.Walking, PlayerActAnimations.Sitting,
                PlayerActAnimations.Idle, PlayerActAnimations.Standby, PlayerActAnimations.Standby,
                PlayerActAnimations.Standby, PlayerActAnimations.Freeze1, PlayerActAnimations.Dead,
                PlayerActAnimations.Standby, PlayerActAnimations.Standby, PlayerActAnimations.Standby,
                PlayerActAnimations.None
            };

        //Lookup table for next monster animation.
        //Idle -> Idle, Walk -> Walk, Attack -> Idle, ReceiveDmg -> Idle, Dead -> None
        public static readonly MonsterActAnimations[] MonsterNextAnimation = new MonsterActAnimations[]
            {
                MonsterActAnimations.Idle, MonsterActAnimations.Walking, MonsterActAnimations.Idle,
                MonsterActAnimations.Idle, MonsterActAnimations.None
            };

        //Lookup table for start and end frame of each casting animation. Assuming the max cast animations per act is 4
        //This also contains the next animation after cast. A next animation of "None" means the sprite should be permanently stuck on last frame
        public const int MAX_CASTING_ANIMATIONS = 4;
        public static readonly CastingAnimation[] CastingAnimations = new CastingAnimation[MAX_CASTING_ANIMATIONS * ((int)Jobs.Last + 1)];

        static MediaConstants()
        {
            #region Weapon fallbacks initialization
            WeaponClassFallbackId[(int)Jobs.Novice] = Jobs.Novice;
            WeaponClassFallbackId[(int)Jobs.Swordsman] = Jobs.Swordsman;
            WeaponClassFallbackId[(int)Jobs.Archer] = Jobs.Archer;
            WeaponClassFallbackId[(int)Jobs.Acolyte] = Jobs.Acolyte;
            WeaponClassFallbackId[(int)Jobs.Mage] = Jobs.Mage;
            WeaponClassFallbackId[(int)Jobs.Thief] = Jobs.Thief;
            WeaponClassFallbackId[(int)Jobs.Merchant] = Jobs.Merchant;
            WeaponClassFallbackId[(int)Jobs.SuperNovice] = Jobs.SuperNovice;
            WeaponClassFallbackId[(int)Jobs.Taekwon] = Jobs.Taekwon;
            WeaponClassFallbackId[(int)Jobs.Gunslinger] = Jobs.Gunslinger;
            WeaponClassFallbackId[(int)Jobs.Ninja] = Jobs.Ninja;
            WeaponClassFallbackId[(int)Jobs.Knight] = Jobs.Knight;
            WeaponClassFallbackId[(int)Jobs.Crusader] = Jobs.Crusader;
            WeaponClassFallbackId[(int)Jobs.Hunter] = Jobs.Hunter;
            WeaponClassFallbackId[(int)Jobs.Bard] = Jobs.Bard;
            WeaponClassFallbackId[(int)Jobs.Dancer] = Jobs.Dancer;
            WeaponClassFallbackId[(int)Jobs.Priest] = Jobs.Priest;
            WeaponClassFallbackId[(int)Jobs.Monk] = Jobs.Monk;
            WeaponClassFallbackId[(int)Jobs.Wizard] = Jobs.Wizard;
            WeaponClassFallbackId[(int)Jobs.Sage] = Jobs.Sage;
            WeaponClassFallbackId[(int)Jobs.Assassin] = Jobs.Assassin;
            WeaponClassFallbackId[(int)Jobs.Rogue] = Jobs.Rogue;
            WeaponClassFallbackId[(int)Jobs.Blacksmith] = Jobs.Blacksmith;
            WeaponClassFallbackId[(int)Jobs.Alchemist] = Jobs.Alchemist;
            WeaponClassFallbackId[(int)Jobs.SoulLinker] = Jobs.Wizard;
            WeaponClassFallbackId[(int)Jobs.StarGladiator] = Jobs.Taekwon;
            WeaponClassFallbackId[(int)Jobs.LordKnight] = Jobs.Knight;
            WeaponClassFallbackId[(int)Jobs.Paladin] = Jobs.Crusader;
            WeaponClassFallbackId[(int)Jobs.Sniper] = Jobs.Hunter;
            WeaponClassFallbackId[(int)Jobs.Clown] = Jobs.Bard;
            WeaponClassFallbackId[(int)Jobs.Gypsy] = Jobs.Dancer;
            WeaponClassFallbackId[(int)Jobs.HighPriest] = Jobs.Priest;
            WeaponClassFallbackId[(int)Jobs.Champion] = Jobs.Monk;
            WeaponClassFallbackId[(int)Jobs.HighWizard] = Jobs.Wizard;
            WeaponClassFallbackId[(int)Jobs.Professor] = Jobs.Sage;
            WeaponClassFallbackId[(int)Jobs.AssassinCross] = Jobs.Assassin;
            WeaponClassFallbackId[(int)Jobs.Stalker] = Jobs.Rogue;
            WeaponClassFallbackId[(int)Jobs.Whitesmith] = Jobs.Blacksmith;
            WeaponClassFallbackId[(int)Jobs.Creator] = Jobs.Alchemist;
            #endregion

            #region Shield fallbacks initialization
            ShieldClassFallbackId[(int)Jobs.Novice] = Jobs.Novice;
            ShieldClassFallbackId[(int)Jobs.Swordsman] = Jobs.Swordsman;
            ShieldClassFallbackId[(int)Jobs.Archer] = Jobs.Archer;
            ShieldClassFallbackId[(int)Jobs.Acolyte] = Jobs.Acolyte;
            ShieldClassFallbackId[(int)Jobs.Mage] = Jobs.Mage;
            ShieldClassFallbackId[(int)Jobs.Thief] = Jobs.Thief;
            ShieldClassFallbackId[(int)Jobs.Merchant] = Jobs.Merchant;
            ShieldClassFallbackId[(int)Jobs.SuperNovice] = Jobs.SuperNovice;
            ShieldClassFallbackId[(int)Jobs.Taekwon] = Jobs.None;
            ShieldClassFallbackId[(int)Jobs.Gunslinger] = Jobs.Gunslinger;
            ShieldClassFallbackId[(int)Jobs.Ninja] = Jobs.Ninja;
            ShieldClassFallbackId[(int)Jobs.Knight] = Jobs.Knight;
            ShieldClassFallbackId[(int)Jobs.Crusader] = Jobs.Crusader;
            ShieldClassFallbackId[(int)Jobs.Hunter] = Jobs.Hunter;
            ShieldClassFallbackId[(int)Jobs.Bard] = Jobs.Bard;
            ShieldClassFallbackId[(int)Jobs.Dancer] = Jobs.Dancer;
            ShieldClassFallbackId[(int)Jobs.Priest] = Jobs.Priest;
            ShieldClassFallbackId[(int)Jobs.Monk] = Jobs.Monk;
            ShieldClassFallbackId[(int)Jobs.Wizard] = Jobs.Wizard;
            ShieldClassFallbackId[(int)Jobs.Sage] = Jobs.Sage;
            ShieldClassFallbackId[(int)Jobs.Assassin] = Jobs.Assassin;
            ShieldClassFallbackId[(int)Jobs.Rogue] = Jobs.Rogue;
            ShieldClassFallbackId[(int)Jobs.Blacksmith] = Jobs.Blacksmith;
            ShieldClassFallbackId[(int)Jobs.Alchemist] = Jobs.Alchemist;
            ShieldClassFallbackId[(int)Jobs.SoulLinker] = Jobs.None;
            ShieldClassFallbackId[(int)Jobs.StarGladiator] = Jobs.None;
            ShieldClassFallbackId[(int)Jobs.LordKnight] = Jobs.Knight;
            ShieldClassFallbackId[(int)Jobs.Paladin] = Jobs.Crusader;
            ShieldClassFallbackId[(int)Jobs.Sniper] = Jobs.Hunter;
            ShieldClassFallbackId[(int)Jobs.Clown] = Jobs.Bard;
            ShieldClassFallbackId[(int)Jobs.Gypsy] = Jobs.Dancer;
            ShieldClassFallbackId[(int)Jobs.HighPriest] = Jobs.Priest;
            ShieldClassFallbackId[(int)Jobs.Champion] = Jobs.Monk;
            ShieldClassFallbackId[(int)Jobs.HighWizard] = Jobs.Wizard;
            ShieldClassFallbackId[(int)Jobs.Professor] = Jobs.Sage;
            ShieldClassFallbackId[(int)Jobs.AssassinCross] = Jobs.Assassin;
            ShieldClassFallbackId[(int)Jobs.Stalker] = Jobs.Rogue;
            ShieldClassFallbackId[(int)Jobs.Whitesmith] = Jobs.Blacksmith;
            ShieldClassFallbackId[(int)Jobs.Creator] = Jobs.Alchemist;
            #endregion

            #region Weapon animations initialization
            // Default to idle for all weapons at first. This will cover weapons that should not be equipable
            for (int i = 0; i < WeaponAnimations.Length; i++)
                WeaponAnimations[i] = PlayerActAnimations.Idle;
            //Specify animations only for relevant jobs and weapons
            WeaponAnimations[(int)Jobs.Hunter * (int)WeaponShieldAnimatorIDs.Barehand] = PlayerActAnimations.Attacking2;
            WeaponAnimations[(int)Jobs.Hunter * (int)WeaponShieldAnimatorIDs.ShortSword] = PlayerActAnimations.Attacking2;
            WeaponAnimations[(int)Jobs.Hunter * (int)WeaponShieldAnimatorIDs.Bow] = PlayerActAnimations.Attacking3;

            WeaponAnimations[(int)Jobs.Champion * (int)WeaponShieldAnimatorIDs.Barehand] = PlayerActAnimations.Attacking3;
            #endregion

            #region Dual wield lookup
            //The rest can stay as 0 (barehand)
            DualWieldLookup[(int)WeaponShieldAnimatorIDs.ShortSword + (int)WeaponShieldAnimatorIDs.ShortSword] = WeaponShieldAnimatorIDs.ShortSword_ShortSword;
            DualWieldLookup[(int)WeaponShieldAnimatorIDs.Sword + (int)WeaponShieldAnimatorIDs.Sword] = WeaponShieldAnimatorIDs.Sword_Sword;
            DualWieldLookup[(int)WeaponShieldAnimatorIDs.Axe + (int)WeaponShieldAnimatorIDs.Axe] = WeaponShieldAnimatorIDs.Axe_Axe;
            DualWieldLookup[(int)WeaponShieldAnimatorIDs.ShortSword + (int)WeaponShieldAnimatorIDs.Sword] = WeaponShieldAnimatorIDs.ShortSword_Sword;
            DualWieldLookup[(int)WeaponShieldAnimatorIDs.ShortSword + (int)WeaponShieldAnimatorIDs.Axe] = WeaponShieldAnimatorIDs.ShortSword_Axe;
            #endregion

            #region Casting animations initialization
            //Set default casting animations of all jobs to start at 0 and end at 16. Default next animation is Idle
            //We'll be using end frame in a division operation, so we use 16, which is the first power of 2 that definitely does not have a valid sprite frame
            for (int i = 0; i < CastingAnimations.Length; i++)
                CastingAnimations[i] = new CastingAnimation { startFrame = 0, endFrame = 16, nextAnimation = PlayerActAnimations.Idle, actionDelay = 4 };

            //Specify casting animations only for relevant jobs
            //Start frame needs to be the index of the first renderered image.
            //End frame needs to be the index + 1 of the last rendered image
            //For example Champ animation 0 is only 1 frame, so starts at 0 and ends at 1.
            CastingAnimations[MAX_CASTING_ANIMATIONS * (int)Jobs.Champion + (int)JobCastingAnimations.ChampDefaultCast] = new CastingAnimation { startFrame = 0, endFrame = 1, nextAnimation = PlayerActAnimations.Standby, actionDelay = 4 };
            CastingAnimations[MAX_CASTING_ANIMATIONS * (int)Jobs.Champion + (int)JobCastingAnimations.ChampCriticalExplosion] = new CastingAnimation { startFrame = 1, endFrame = 3, nextAnimation = PlayerActAnimations.Idle, actionDelay = 20 };
            CastingAnimations[MAX_CASTING_ANIMATIONS * (int)Jobs.Champion + (int)JobCastingAnimations.ChampThrowSpiritSphere] = new CastingAnimation { startFrame = 3, endFrame = 5, nextAnimation = PlayerActAnimations.Standby, actionDelay = 8 };
            CastingAnimations[MAX_CASTING_ANIMATIONS * (int)Jobs.Champion + (int)JobCastingAnimations.ChampTrifectaCombo] = new CastingAnimation { startFrame = 5, endFrame = 6, nextAnimation = PlayerActAnimations.Standby, actionDelay = 8 };
            #endregion
        }
    }
}
