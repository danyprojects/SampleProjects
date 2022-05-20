#pragma once

#include <chrono>
#include <cstdint>

// Connections limits
constexpr uint16_t MAX_CONNECTIONS = 5;
constexpr std::chrono::milliseconds READ_TIMEOUT = std::chrono::milliseconds(1000000);

// Rates
constexpr int BASE_DROP_RATE = 5;

// Account limits
constexpr int ACCOUNT_POOL_SIZE = 20;
constexpr int MAX_UNIQUE_ACCOUNT_NAMES = ACCOUNT_POOL_SIZE + 30;
constexpr int ACCOUNT_NAME_POOL_DELETE_THRESHOLD = MAX_UNIQUE_ACCOUNT_NAMES / 4; // At 25% unused, release extra memory to system
constexpr int MAX_PASSWORD_LEN = 32;
constexpr int MAX_ACCOUNT_CHARACTERS = 7;

// Party limits
constexpr int PARTY_POOL_SIZE = 20;
constexpr uint8_t MAX_PARTY_CHARACTERS = 12;
constexpr int MAX_UNIQUE_PARTY_NAMES = PARTY_POOL_SIZE + 30;
constexpr int PARTY_NAME_POOL_DELETE_THRESHOLD = MAX_UNIQUE_PARTY_NAMES / 4; // At 25% unused, release extra memory to system

// Guild limits
constexpr int GUILD_POOL_SIZE = 20;
constexpr uint8_t MAX_GUILD_CHARACTERS = 70;
constexpr int MAX_UNIQUE_GUILD_NAMES = GUILD_POOL_SIZE + 30;
constexpr int GUILD_NAME_POOL_DELETE_THRESHOLD = MAX_UNIQUE_GUILD_NAMES / 4; // At 25% unused, release extra memory to system
constexpr int MAX_GUILD_SKILLS = 20;
constexpr int MAX_GUILD_POSITIONS = 20;
constexpr int GUILD_EMBLEM_SIZE = 32 * 32 * 3; // payload in bytes
constexpr int MAX_GUILD_EXP = -1;
constexpr std::chrono::milliseconds EXP_NOTIFY_DELTA = std::chrono::milliseconds(30000);

// Character limits
constexpr int CHARACTER_POOL_SIZE = 20;
constexpr int MAX_UNIQUE_CHARACTER_NAMES = CHARACTER_POOL_SIZE + 30;
constexpr int CHARACTER_NAME_POOL_DELETE_THRESHOLD = MAX_UNIQUE_CHARACTER_NAMES / 4; // At 25% unused, release extra memory to system
constexpr int MIN_CHARACTER_NAME_SIZE = 3;
constexpr int MAX_CHARACTER_AUTO_TRIGGERS = 10; //RO used 10 per category
constexpr int MAX_HAIR_STYLE = 28;

// Monster limits
constexpr bool MONSTER_POOL_PADDING = false;
constexpr int MONSTER_POOL_SIZE = 500;
constexpr int MONSTER_POOL_DELETE_THRESHOLD = MONSTER_POOL_SIZE / 4; // At 25% unused, release extra memory to system

// Map limits
constexpr int MAX_MAP_THREAD_POOL_SIZE = 1;
constexpr int MAX_MAP_ARRAY_SIZE = 5;
constexpr int8_t MAP_BLOCK_WIDTH = 8;
constexpr int8_t MAP_BLOCK_HEIGHT = 8;
constexpr float DIAGONAL_TO_UNIT_SIZE = 7.07f;
constexpr float CELL_TO_UNIT_SIZE = 5.f;
constexpr uint8_t MAX_MAP_WARP_SIZE = 30;
constexpr int MAX_MAP_SIZE = 504; // to be a multiple of 8
constexpr int MAX_ITEMS_PER_CELL = 9; //Client divides item position in a 3x3 grid in the cell, server spawns items in a 3x3, allowing 81 items in a range of 3x3

// Lobby limits
constexpr int MAX_LOBBY_THREAD_POOL_SIZE = 1;

// Item Limits
constexpr int MAX_CARD_SLOTS = 4;
constexpr int DEFAULT_DESPAWN_TIME = 10 * 1000; // 10s
constexpr int ITEM_POOL_SIZE = 500;
constexpr bool ITEM_POOL_PADDING = false;
constexpr int ITEM_POOL_DELETE_THRESHOLD = ITEM_POOL_SIZE / 4; // At 25% unused, release extra memory to system

// Pathfinding Limits
constexpr int MAX_WALK_DISTANCE = 20; //Need to adapt this to be near the edge of a zoomed out camera to make sense
constexpr int MAX_PATH_LENGTH = MAX_WALK_DISTANCE; // For now this should be the same as walk distance, maybe it won't make sense to have it different

// Skill limits
constexpr int MAX_SKILL_REQUIREMENTS = 10;
constexpr int MAX_SKILLS_PER_JOB = 51; //(supernovice) TODO:: calc at compile time
constexpr int MAX_SKILL_ITEM_REQS = 5;
constexpr int SKILL_POOL_SIZE = 500;
constexpr bool SKILL_POOL_PADDING = false;
constexpr int SKILL_POOL_DELETE_THRESHOLD = SKILL_POOL_SIZE / 4; // At 25% unused, release extra memory to system

// BlockArray limits
constexpr int BLOCK_ARRAY_MAX_ENTRIES = 5000;

// Unit limits
constexpr int DEFAULT_WALK_SPEED = 150;
constexpr int DEFAULT_VIEW_RANGE = 15;
constexpr int MAX_AURA_RANGE = 7; //this is the max area that can be used for stuff like walk_on effects. 7 would be efficient as it only needs to check adjacent blocks in grid
constexpr int MAX_SKILL_TREE_EXTRA_SKILLS = 2; // 2 accessories?
constexpr int MAX_SKILL_TREE_PERMANENT_SKILLS = 54; //(supernovice) TODO:: calculate at compile time
constexpr int MAX_SKILL_TREE_SKILLS = MAX_SKILL_TREE_EXTRA_SKILLS + MAX_SKILL_TREE_PERMANENT_SKILLS;
constexpr int MAX_CHARACTER_BUFFS = 50;
constexpr int MAX_MONSTER_BUFFS = 20;
constexpr int MAX_PICK_UP_RANGE = 1;
constexpr uint16_t INSTANT_CASTIME = 0;

// Animation constants
constexpr uint32_t ACTION_DELAY_BASE_TIME = 25; //Same as client, in ms

// Timers
constexpr int TIMER_POOL_SIZE = 500;
constexpr bool TIMER_POOL_PADDING = false;
constexpr int TIMER_POOL_DELETE_THRESHOLD = TIMER_POOL_SIZE / 4; // At 25% unused, release extra memory to system

//Inventory limits
constexpr int INVENTORY_SIZE = 100;
constexpr int DEFAULT_CHAR_WEIGHT = 2000;

// Chat limits
constexpr int MAX_CHAT_MSG_LEN = 128;

// Message limits
constexpr int MSG_NODE_POOL_SIZE = 1000;
constexpr bool MSG_NODE_POOL_PADDING = false;
constexpr int MSG_NODE_POOL_THRESHOLD = SKILL_POOL_SIZE / 4; // At 25% unused, release extra memory to system
constexpr int MSG_MULTIPOOL_MAX_OBJSIZE = 256;
constexpr int MSG_MULTIPOOL_SIZES[] = { /*8*/1000, /*16*/1000, /*32*/1000, /*64*/500, /*128*/100, /*256*/30 };

// Npc limits
constexpr int MAX_NPC_SHOP_ITEMS = 25; //Gotta either send by parts of enlarge packet buffer