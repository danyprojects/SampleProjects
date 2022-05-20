using UnityEngine;

//Delegates for actions with ref since default "Action" doesn't allow it
public delegate void ActionRef<T>(ref T item);
public delegate void ActionOut<T1, T2, T3>(T1 t1, T2 t2 , out T3 t3);

public static class Constants
{
    //*************************************************************************************** Miscelaneous constants
    public const int PIXEL_TO_UNIT_RATIO = 100;
    public const int ONE_SECOND_MS = 1000; //second to MS
    public const int ONE_MINUTE_SEC = 60; //minute to 
    public const int ONE_MINUTE_MS = ONE_MINUTE_SEC * ONE_SECOND_MS; //minute to ms
    public const int MAX_PLAYERS = 5;
    public const int INVALID_SEED = int.MaxValue;
    public const long DEFAULT_GAME_DURATION_MS = 2 * ONE_MINUTE_MS; //2 mins
    public const int GAME_START_COUTDOWN_TIME = 2 * ONE_SECOND_MS;
    public const int DEFAULT_UPGRADE_POINTS_PER_KILL = 100;

    //*************************************************************************************** Network constants
    public const ushort DEFAULT_PORT = 7777;
    public const int INVALID_CLIENT_TOKEN = int.MinValue;
    public const int TIMEOUT_MS = 5000;
    public const byte INVALID_CELL_INDEX = byte.MaxValue;
    public const short INVALID_BACTERIA_INDEX = short.MinValue;

    //Map constants
    public const int GRID_UNIT_RATIO = 20; //20 ingame units correspond to 1 cell in the map grid
    public const int SMALL_MAP_DIMENSIONS = GRID_UNIT_RATIO * 10;

    //Bullet constants
    public const int BULLET_HOST_ADDITIONAL_REMOVE_TIME = (TIMEOUT_MS + 1) * ONE_SECOND_MS; //Minimum time needs to be at least the timeout so that clients that are lagging don't get 2 messages wrong

    //structure constants
    public const int DEFAULT_TERRITORY_DEFORM_INTERVAL = 5 * ONE_SECOND_MS;
    public const int TERRITORY_Z_POS = 1;
    public const int DEFAULT_TERRITORY_RADIUS = 3;

    //Unit constants
    public const float WANDER_DISTANCE = 5;
    public const int MAXIMUM_WANDER_RETRIES = 5;
    public const int MAX_WANDER_TIME_INTERVAL = 5 * ONE_SECOND_MS;
    public const int DEFAULT_KNOCKBACK_DISTANCE = 1;
    public const int DEFAULT_WOUND_HEALING_POWER = 1;

    //Aura constants

    //Pill constants
    public const int MIN_PILL_SPAWN_RADIUS = MAX_HEART_RADIUS;
    public const int DEFAULT_PILL_SPAWN_RADIUS = 7;
    public const int DEFAULT_PILL_SPAWN_INTERVAL = ONE_SECOND_MS * 10;

    //Wound constants
    public const int DEFAULT_WOUND_SPAWN_INTERVAL = ONE_SECOND_MS * 10;
    public const int DEFAULT_WOUND_HP = 10;
    public const int DEFAULT_WOUND_HEAL_INTERVAL = ONE_SECOND_MS * 1; //1 heal per 1 second

    //Respawn constants
    public const int MIN_HEART_RADIUS = 2;
    public const int MAX_HEART_RADIUS = 4;
    public const int MIN_BACTERIA_SPAWN_RADIUS = 2;
    public const int DEFAULT_BACTERIA_SPAWN_INTERVAL = 5 * ONE_SECOND_MS;
    public const int DEFAULT_CELL_RESPAWN_TIME = 10 * ONE_SECOND_MS;
    public const int INVALID_CELL_RESPAWN_TIME = int.MaxValue;

    //*************************************************************************************** pool sizes / growths
    public const int BACTERIA_POOL_INITIAL_SIZE = 15;
    public const int BACTERIA_POOL_GROWTH_AMOUNT = 2;
    public const int BACTERIA_INITIAL_MAX_AMOUNT = 100;
    public const int BACTERIA_GROWTH_AMOUNT = 50;

    public const int BULLET_CELL_INITIAL_AMOUNT = 10;
    public const int BULLET_CELL_GROWTH_AMOUNT = 5;
    public const int BULLET_BACTERIA_INITIAL_AMOUNT = 10;
    public const int BULLET_BACTERIA_GROWTH_AMOUNT = 5;
    public const int BULLET_POOL_INITIAL_SIZE = 20;
    public const int BULLET_POOL_GROWTH_AMOUNT = 10;

    public const int TRAP_POOL_INITIAL_SIZE = 2;
    public const int TRAP_POOL_GROWTH_AMOUNT = 1;
    public const int TRAP_CELL_INITIAL_AMOUNT = 10;
    public const int TRAP_CELL_GROWTH_AMOUNT = 5;
    public const int TRAP_BACTERIA_INITIAL_AMOUNT = 10;
    public const int TRAP_BACTERIA_GROWTH_AMOUNT = 5;

    public const int AURA_CELL_INITIAL_AMOUNT = 10;
    public const int AURA_CELL_GROWTH_AMOUNT = 5;
    public const int AURA_BACTERIA_INITIAL_AMOUNT = 10;
    public const int AURA_BACTERIA_GROWTH_AMOUNT = 5;
    public const int AURA_POOL_INITIAL_SIZE = 2;
    public const int AURA_POOL_GROWTH_AMOUNT = 1;

    public const int PILL_INITIAL_AMOUNT = 2;
    public const int PILL_GROWTH_AMOUNT = 1;
    public const int PILL_POOL_INITIAL_SIZE = 2;
    public const int PILL_POOL_GROWTH_AMOUNT = 1;

    public const int STRUCTURE_INITIAL_AMOUNT = 2;
    public const int STRUCTURE_GROWTH_AMOUNT = 1;
    public const int STRUCTURE_POOL_INITIAL_SIZE = 2;
    public const int STRUCTURE_POOL_GROWTH_AMOUNT = 1;

    //Effects pools
    public const int DEFAULT_EFFECT_AMOUNT = 50;
    public const short INVALID_EFFECT_TAG = short.MinValue;
    public const short FIRST_EFFECT_TAG = INVALID_EFFECT_TAG + 1;
    public const int EFFECT_SLOTS_GROWTH_AMOUNT = 20;
    public const int TRAILS_POOL_INITIAL_AMOUNT = 5;
    public const int TRAILS_POOL_GROWTH_AMOUNT = 2;
    public const int PARTICLES_POOL_INITIAL_AMOUNT = 5;
    public const int PARTICLES_POOL_GROWTH_AMOUNT = 2;

    //*************************************************************************************** Shop constants
    public const short SHOP_INFINITE_LIMIT = short.MaxValue;
    public const int SHOP_ENTRY_SPACING = 10;

    //*************************************************************************************** Default vectors
    public static readonly Vector2 DEFAULT_UNIT_DIRECTION = Vector2.up;
    public static readonly Vector2 BULLET_DIRECTION_AXIS = Vector2.up;
    public static readonly Vector3 OUT_OF_RANGE_POSITION = new Vector3(-1000000, -1000000, 0);

    //*************************************************************************************** Colors
    public static readonly Color SHOP_GRAYED_OUT_TEXT = Color.gray;
    public static readonly Color SHOP_NOT_ENOUGH_CURRENCY = Color.red;
    public static readonly Color SHOP_TEXT_NORMAL = Color.white;

    //*************************************************************************************** Layers for fast acces
    public static readonly int CELLS_LAYER = LayerMask.NameToLayer("Cells");
    public static readonly int BACTERIA_LAYER = LayerMask.NameToLayer("Bacteria");
    public static readonly int BULLETS_LAYER = LayerMask.NameToLayer("Bullets");
    public static readonly int PILLS_LAYER = LayerMask.NameToLayer("Pills");
    public static readonly int TRAPS_LAYER = LayerMask.NameToLayer("Traps");
    public static readonly int AURAS_LAYER = LayerMask.NameToLayer("Auras");
    public static readonly int STRUCTURES_LAYER = LayerMask.NameToLayer("Structures");
    public static readonly int TERRITORY_LAYER = LayerMask.NameToLayer("Territory");

    //Layer masks
    public static readonly int CELLS_MASK = 1 << CELLS_LAYER;
    public static readonly int BACTERIA_MASK = 1 << BACTERIA_LAYER;
    public static readonly int BULLETS_MASK = 1 << BULLETS_LAYER;
    public static readonly int PILLS_MASK = 1 << PILLS_LAYER;
    public static readonly int TRAPS_MASK = 1 << TRAPS_LAYER;
    public static readonly int AURAS_MASK = 1 << AURAS_LAYER;
    public static readonly int STRUCTURES_MASK = 1 << STRUCTURES_LAYER;
    public static readonly int TERRITORY_MASK = 1 << TERRITORY_LAYER;

    //Sprite sorting layers
    public static readonly int OVERLAY_SPRITE_LAYER = SortingLayer.NameToID("Overlay");

    //Default collision layer masks, according to table in https://danyprojects.atlassian.net/wiki/spaces/BAC/pages/2228225/Collision+detection
    public static readonly int CELL_MOVE_COLLISION_MASK = BACTERIA_MASK | PILLS_MASK | TRAPS_MASK;
    public static readonly int BACTERIA_MOVE_COLLISION_MASK = CELLS_MASK | PILLS_MASK | TRAPS_MASK;
    public static readonly int BULLET_MOVE_COLLISION_MASK = CELLS_MASK | BACTERIA_MASK | TRAPS_MASK | AURAS_MASK | STRUCTURES_MASK;
    public static readonly int AURA_COLLISION_MASK = CELLS_MASK | BACTERIA_MASK | PILLS_MASK | TRAPS_MASK | STRUCTURES_MASK;
    public static readonly int PATH_COLLISION_MASK = STRUCTURES_MASK;
}

public class ShaderConstants
{
    public static readonly int SHADER_MAIN_TEX_PROPERTY_ID = Shader.PropertyToID("_MainTex");
    public static readonly int SHADER_OFFSET_PROPERTY_ID = Shader.PropertyToID("_Offset");
}

public enum NetworkMode : byte 
{
    LAN,
    Online,

    None = byte.MaxValue
}

public enum GameMode : byte 
{
    Survival = 0,

    None = byte.MaxValue
}

public enum GameRoomType : byte
{
    SinglePlayer = 0,
    Multiplayer_Host,
    Multiplayer_Client,

    None = byte.MaxValue
}

public enum GameState : byte 
{
    MainMenu = 0,
    JoiningRoom,
    AwaitingGameStart,
    CountdownForStart,
    InGame,
    GameModeQuit,

    Invalid = byte.MaxValue
}

public enum GameEndResult : byte
{
    Win = 0,
    Lose,
    Draw,

    Invalid = byte.MaxValue
}

public enum BlockType : byte
{
    Cell = 0,
    Bacteria,
    Bullet,
    Pill,
    Trap,
    Aura,
    Structure, 

    Invalid = byte.MaxValue
}

public enum PlayableType : byte
{
    Cell,

    None
}