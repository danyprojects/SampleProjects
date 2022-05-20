using UnityEngine;

namespace RO.Common
{
    public static class Globals
    {
        public static float Time = UnityEngine.Time.time;
        public static float TimeSinceLevelLoad = UnityEngine.Time.timeSinceLevelLoad;
        public static int FrameIncrement = 1;
        public readonly static Camera Camera = UnityEngine.Camera.main;

        public static class UI
        {
            public static bool IsOverUI = false;           // UI sets it
            public static bool IsOverChatBox = false;      // UI sets it
            public static bool IsScrollingAllowed = true; // UI reads it
        }
    }

    public static class ConstStrings
    {
        public const string CAST_CIRCLE_NAME = "cast_circle";
        public const string FLOATING_TEXT_ACT_NAME = "msg_act";
        public const string GROUND_MESH_NAME = "groundmesh";
        public const string GROUND_MESH_DATA_NAME = "groundmeshdata";
        public readonly static string[] NumberStrings = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11" };
    }

    public static class AudioTracks
    {
        public const string CAST_SOUND = "cast_sound";
    }

    public static class Materials
    {
        public static Material castCircleMaterial = AssetBundleProvider.LoadMiscBundleAsset<Material>(ConstStrings.CAST_CIRCLE_NAME);
    }

    public static class LayerIndexes
    {
        public readonly static int Map = LayerMask.NameToLayer("Map");
        public readonly static int Player = LayerMask.NameToLayer("Player");
        public readonly static int Monster = LayerMask.NameToLayer("Monster");
        public readonly static int Mercenary = LayerMask.NameToLayer("Mercenary");
        public readonly static int Homunculus = LayerMask.NameToLayer("Homunculus");
        public readonly static int Pet = LayerMask.NameToLayer("Pet");
        public readonly static int Npc = LayerMask.NameToLayer("Npc");
        public readonly static int Item = LayerMask.NameToLayer("Item");

        public const int MAP_LAYER_START = 8; //Unity has 7 built in layers
        public const int BLOCK_LAYER_START = 14;
        public const int OTHER_LAYER_START = 24;
    }

    public static class LayerMasks
    {
        public readonly static int Map = 1 << LayerIndexes.Map;
        public readonly static int Player = 1 << LayerIndexes.Player;
        public readonly static int Monster = 1 << LayerIndexes.Monster;
        public readonly static int Mercenary = 1 << LayerIndexes.Mercenary;
        public readonly static int Homunculus = 1 << LayerIndexes.Homunculus;
        public readonly static int Pet = 1 << LayerIndexes.Pet;
        public readonly static int Npc = 1 << LayerIndexes.Npc;
        public readonly static int Item = 1 << LayerIndexes.Item;
    }

    public static class SortingLayers
    {
        public readonly static string BlockStr = "Block";
        public readonly static string EffectStr = "Effect";
        public readonly static string EffectOverlayStr = "Effect";
        public readonly static int BlockInt = SortingLayer.NameToID(BlockStr);
        public readonly static int EffectInt = SortingLayer.NameToID(EffectStr);
        public readonly static int EffectOverlayInt = SortingLayer.NameToID(EffectOverlayStr);
    }

    public static class Constants
    {
        //Misc
        public const int MAX_READ_PACKETS_PER_LOOP = 50;
        public const int CELL_TO_UNIT_SIZE = 5;
        public const float DIAGONAL_TO_UNIT_SIZE = 7.07f; //Sqrt( CELL_TO_UNIT_SIZE^2 + CELL_TO_UNIT_SIZE^2)
        public const float HALF_CELL_UNIT_SIZE = CELL_TO_UNIT_SIZE / 2f;
        public const int MAP_SCENE_START = 1;
        public const float FPS_TIME_INTERVAL = 1f / 60; // in seconds. 60 fps
        public const int UI_SCALING_FACTOR = 5;
        public const float PIXELS_PER_UNIT = 6.25f;
        public const int MAX_WATER_TEXTURES = 32;

        //Player Limits
        public const int MAX_HAIR_ID = 29;

        //For units
        public const int DEFAULT_VIEW_RANGE = 15;
        public const short LOCAL_SESSION_ID = short.MaxValue;

        //Initials for game controller
        public const int MAX_BLOCK_COUNT = 5000;

        //Limits for Map
        public const int MAX_MAP_HEIGHT = 500;
        public const int MAX_MAP_WIDTH = 500;
        public const int MAX_WARP_PORTAL_COUNT = 50;

        //Limits for inputs
        public const float SEND_INPUT_DELAY = 1f / 15; // in seconds. Number of inputs per second

        //Limits for class fields
        public const int MAX_PLAYER_SKILLS = 54; // same as in server
        public const int MAX_PLAYER_EXTRA_SKILLS = 2;   // same as in server
        public const int EQUIP_SLOTS = 10;
        public const int MAX_NAME_LEN = 30;
        public const int MAX_PASSWORD_SIZE = 32;
        public const int MAX_CHARACTERS = 4;

        // Taken from hercules
        public const int DEFAULT_WALK_SPEED = 150;
        public const int MIN_WALK_SPEED = 20; /* below 20 clips animation */
        public const int MAX_WALK_SPEED = 1000;

        //Limits for pathfinding
        public const int MAX_WALK = 15;
        public const int MAX_NODES = 300 + ((MAX_WALK - 15) / 5) * 200; // Lets just say that each 5 cells will add 200 nodes for now

        //Limits for skills
        public const int MIN_SKILL_LEVEL = 1;
        public const int MAX_SKILL_LEVEL = 10;
        public const int MAX_SKILL_REQUIREMENTS = 10;
    }

    public enum Gender : int
    {
        Male = 0,
        Female,

        None
    }

    public enum Jobs : int
    {
        Novice = 0,

        FirstJobStart,
        Swordsman = FirstJobStart,
        Archer,
        Acolyte,
        Mage,
        Thief,
        Merchant,

        ExtraFirstJobStart,
        SuperNovice = ExtraFirstJobStart,
        Taekwon,
        Gunslinger,
        Ninja,

        SecondJobStart,
        Knight = SecondJobStart,
        Crusader,
        Hunter,
        Bard,
        Dancer,
        Priest,
        Monk,
        Wizard,
        Sage,
        Assassin,
        Rogue,
        Blacksmith,
        Alchemist,

        ExtraSecondJobStart,
        SoulLinker = ExtraSecondJobStart,
        StarGladiator,

        TransJobStart,
        LordKnight = TransJobStart,
        Paladin,
        Sniper,
        Clown,
        Gypsy,
        HighPriest,
        Champion,
        HighWizard,
        Professor,
        AssassinCross,
        Stalker,
        Whitesmith,
        Creator,

        Last = Creator,
        None
    }

    public enum Sizes : int
    {
        Small,
        Normal,
        Large
    }

    public enum Races : int
    {
        Formless = 0,
        Undead,
        Brute,
        Plant,
        Insect,
        Fish,
        Demon,
        Demi,
        Human,
        Angel,
        Dragon
    }

    public enum Elements : int
    {
        Neutral = 0,
        Water,
        Earth,
        Fire,
        Wind,
        Poison,
        Holy,
        Dark,
        Ghost,
        Undead
    };

    public enum ElementLvls : int
    {
        _0,
        _1,
        _2,
        _3,
        _4
    }

    public enum DamageType : int
    {
        SingleHit = 0,
        MultiHit
    }

    public enum ItemType : int
    {
        Healing = 0,
	    Usable,
	    Etc,
	    Armor,
	    Weapon,
	    Card,
	    PetEgg,
	    PetArmor,
	    Ammo,
	    DelayConsume,
	    Cash,

        None = -1
    }

    public enum EquipmentLocation
    {
        None         = 0,
        HeadLow      = 1 << 0,
        HandRight    = 1 << 1,
        Garment      = 1 << 2,
        AccessoryL   = 1 << 3,
        Armor        = 1 << 4,
        HandLeft     = 1 << 5,
        Shoes        = 1 << 6,
        AccessoryR   = 1 << 7,
        HeadTop      = 1 << 8,
        HeadMiddle   = 1 << 9,

        Accessory            = AccessoryL | AccessoryR, 
        TwoHanded            = HandRight | HandLeft,
        HeadMiddleLower      = HeadLow | HeadMiddle,
        HeadUpperMiddle      = HeadTop | HeadMiddle,
        HeadUpperMiddleLower = HeadTop | HeadMiddle | HeadLow,
    }


    public static class JobDependents
    {
        public readonly static Jobs[] table = new Jobs[(int)Jobs.Last + 1];

        static JobDependents()
        {
            table[(int)Jobs.Novice] = Jobs.Novice;
            table[(int)Jobs.Swordsman] = Jobs.Novice;
            table[(int)Jobs.Archer] = Jobs.Novice;
            table[(int)Jobs.Acolyte] = Jobs.Novice;
            table[(int)Jobs.Mage] = Jobs.Novice;
            table[(int)Jobs.Thief] = Jobs.Novice;
            table[(int)Jobs.Merchant] = Jobs.Novice;
            table[(int)Jobs.SuperNovice] = Jobs.Novice;
            table[(int)Jobs.Taekwon] = Jobs.Novice;
            table[(int)Jobs.Gunslinger] = Jobs.Novice;
            table[(int)Jobs.Ninja] = Jobs.Novice;
            table[(int)Jobs.Knight] = Jobs.Swordsman;
            table[(int)Jobs.Crusader] = Jobs.Swordsman;
            table[(int)Jobs.Hunter] = Jobs.Archer;
            table[(int)Jobs.Bard] = Jobs.Archer;
            table[(int)Jobs.Dancer] = Jobs.Archer;
            table[(int)Jobs.Priest] = Jobs.Acolyte;
            table[(int)Jobs.Monk] = Jobs.Acolyte;
            table[(int)Jobs.Wizard] = Jobs.Mage;
            table[(int)Jobs.Sage] = Jobs.Mage;
            table[(int)Jobs.Assassin] = Jobs.Thief;
            table[(int)Jobs.Rogue] = Jobs.Thief;
            table[(int)Jobs.Merchant] = Jobs.Blacksmith;
            table[(int)Jobs.Merchant] = Jobs.Alchemist;
            table[(int)Jobs.SoulLinker] = Jobs.Taekwon;
            table[(int)Jobs.StarGladiator] = Jobs.Taekwon;
            table[(int)Jobs.LordKnight] = Jobs.Knight;
            table[(int)Jobs.Paladin] = Jobs.Crusader;
            table[(int)Jobs.Sniper] = Jobs.Hunter;
            table[(int)Jobs.Clown] = Jobs.Bard;
            table[(int)Jobs.Gypsy] = Jobs.Dancer;
            table[(int)Jobs.HighPriest] = Jobs.Priest;
            table[(int)Jobs.Champion] = Jobs.Monk;
            table[(int)Jobs.HighWizard] = Jobs.Wizard;
            table[(int)Jobs.Professor] = Jobs.Sage;
            table[(int)Jobs.AssassinCross] = Jobs.Assassin;
            table[(int)Jobs.Stalker] = Jobs.Rogue;
            table[(int)Jobs.Whitesmith] = Jobs.Blacksmith;
            table[(int)Jobs.Creator] = Jobs.Alchemist;
        }
    }

    public static class JobTable
    {
        public readonly static string[] table = new string[(int)Jobs.Last + 1];

        static JobTable()
        {
            for (int i = 0; i < table.Length; i++)
            {
                //TODO:: do it manually later due to spaces
                table[i] = ((Jobs)i).ToString();
            }
        }
    }

    public enum ElevatedAtCommand
    {
        Size,
        Dex,
        Int,
        Agi,
        Vit,
        Luk,
        Str,
        Go,
        Warp,
        Zeny,
        Item,
        JobChange,
        JobLvl,
        BaseLvl,
        GuildLvl,
        AllSkills,
        Skill,

        None
    };

    public enum EnterRangeType : int
    {
        Default = 0x1,
        Teleport = 0x2,
        Unhide = 0x4,
        Uncloak = 0x8
    }

    public enum LeaveRangeType : int
    {
        Default = 0x1,
        Teleport = 0x2,
        Hide = 0x4,
        Cloak = 0x8
    }
}
