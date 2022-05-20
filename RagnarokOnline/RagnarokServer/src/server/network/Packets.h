#pragma once

#include <server/common/CommonTypes.h>
#include <server/common/PacketEnums.h>
#include <server/common/Direction.h>
#include <server/common/Job.h>
#include <server/common/ServerLimits.h>
#include <server/common/SkillId.h>
#include <server/common/BuffId.h>
#include <server/common/MonsterId.h>
#include <server/common/ItemDbId.h>
#include <server/common/MapId.h>
#include <server/network/SessionId.h>
#include <server/map_objects/Block.h>
#include <server/map_objects/BlockId.h>
#include <server/map_objects/BlockType.h>
#include <server/network/PacketId.h>
#include <server/network/PacketBase.h>

#include <sdk/Common.h>

#include <string_view>

namespace packet
{

#define PACKET_ID(x) Id::x; static_assert(x::ID == Id::x)

#pragma pack(push, 1)
#pragma warning(disable:4200)

struct RCV_RegisterAccount : public RCV_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(RCV_RegisterAccount);

	RCV_RegisterAccount() : RCV_FixedSizePacket(ID){}

	bool validate() const;

	char name[MAX_NAME_LEN];
	char password[MAX_PASSWORD_LEN];
}; 

struct RCV_Login : public RCV_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(RCV_Login);

	RCV_Login() : RCV_FixedSizePacket(ID) {}

	bool validate() const;

	char name[MAX_NAME_LEN];
	char password[MAX_PASSWORD_LEN];
};

struct RCV_SelectCharacter : public RCV_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(RCV_SelectCharacter);

	RCV_SelectCharacter() : RCV_FixedSizePacket(ID) {}

	uint8_t index;
};

struct RCV_CreateCharacter : public RCV_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(RCV_CreateCharacter);

	RCV_CreateCharacter() : RCV_FixedSizePacket(ID) {}

	bool validate() const;

	uint8_t index;
	uint8_t agi;
	uint8_t str;
	uint8_t vit;
	uint8_t int_;
	uint8_t dex;
	uint8_t luck;
	Gender gender;
	HairStyle hairStyle;
	char name[MAX_NAME_LEN];
};

struct RCV_DeleteCharacter : public RCV_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(RCV_DeleteCharacter);

	RCV_DeleteCharacter() : RCV_FixedSizePacket(ID) {}

	uint8_t index;
};

struct RCV_ReturnToCharacterSelect : public RCV_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(RCV_ReturnToCharacterSelect);

	RCV_ReturnToCharacterSelect() : RCV_FixedSizePacket(ID) {}
};

struct RCV_PlayerMove : public RCV_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(RCV_PlayerMove);

	RCV_PlayerMove() : RCV_FixedSizePacket(ID) {}

	Point position;
};

struct RCV_OtherMove : public RCV_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(RCV_OtherMove);

	RCV_OtherMove() : RCV_FixedSizePacket(ID) {}

	BlockType blockType;
	Point position;
};

struct RCV_PickUpItem : public RCV_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(RCV_PickUpItem);

	RCV_PickUpItem() : RCV_FixedSizePacket(ID) {}

	BlockId itemBlockId;
};

struct RCV_DropItem : public RCV_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(RCV_DropItem);

	RCV_DropItem() : RCV_FixedSizePacket(ID) {}

	uint16_t amount;
	uint8_t index;
};

struct RCV_UseItem : public RCV_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(RCV_UseItem);

	RCV_UseItem() : RCV_FixedSizePacket(ID) {}

	uint8_t index;
};

struct RCV_UnequipItem : public RCV_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(RCV_UnequipItem);

	RCV_UnequipItem() : RCV_FixedSizePacket(ID) {}

	uint8_t index;
};

struct RCV_NpcAction : public RCV_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(RCV_NpcAction);

	RCV_NpcAction() : RCV_FixedSizePacket(ID) {}

	BlockId npcBlockId;
	NpcAction action;
};

struct RCV_CastAreaOfEffectSkill : public RCV_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(RCV_CastAreaOfEffectSkill);

	RCV_CastAreaOfEffectSkill() : RCV_FixedSizePacket(ID) {}
	
	uint8_t skillIndex;
	uint8_t skilllevel;
	Point position;
};

struct RCV_CastSingleTargetSkill : public RCV_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(RCV_CastSingleTargetSkill);

	RCV_CastSingleTargetSkill() : RCV_FixedSizePacket(ID) {}

	uint8_t skillIndex;
	uint8_t skilllevel;
	BlockId targetId;
};

struct RCV_CastSelfTargetSkill : public RCV_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(RCV_CastSelfTargetSkill);

	RCV_CastSelfTargetSkill() : RCV_FixedSizePacket(ID) {}

	uint8_t skillIndex;
	uint8_t skilllevel;
};

struct RCV_ElevatedAtCommand : public Packet
{
	static constexpr Id ID = PACKET_ID(RCV_ElevatedAtCommand);
	static constexpr size_t HeaderSize = sizeof(Packet) + sizeof(ElevatedAtCommand);

	RCV_ElevatedAtCommand() : Packet(ID) {}
	
	int size() const;

	ElevatedAtCommand cmd;
	union
	{
		struct
		{
			Size _size;
		}_CmdSize;
		struct
		{
			uint16_t _stat;
		}_CmdStat;	
		struct
		{
			Job _job;
		}_CmdJobChange;
		struct
		{
			int16_t _jobLvl;
		}_CmdJobLvl;
		struct
		{
			SkillId _id;
		}_CmdSkill;
	};
};

struct RCV_LevelUpSingleSkill : public RCV_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(RCV_LevelUpSingleSkill);

	RCV_LevelUpSingleSkill() : RCV_FixedSizePacket(ID) {}

	uint8_t skillIndex;
	uint8_t skillIncrement;
};

struct RCV_LevelUpMultipleSkills : public RCV_DynamicSizePacket
{
	static constexpr Id ID = PACKET_ID(RCV_LevelUpMultipleSkills);
	static constexpr uint16_t MAX_SIZE = sizeof(packet::RCV_DynamicSizePacket) + MAX_SKILL_TREE_PERMANENT_SKILLS;

	struct SkillInfo
	{
		uint8_t skillIndex;
		uint8_t skillIncrement;
	};

	RCV_LevelUpMultipleSkills() : RCV_DynamicSizePacket(ID) {}

	uint8_t skillInfoSize() const
	{
		return static_cast<uint8_t>((_totalSize - sizeof(RCV_DynamicSizePacket)) / sizeof(SkillInfo));
	}

	SkillInfo skillsInfo[];
};

struct RCV_LevelUpAllTreeSkills : public RCV_DynamicSizePacket
{
	static constexpr Id ID = PACKET_ID(RCV_LevelUpAllTreeSkills);
	static constexpr uint16_t MAX_SIZE = sizeof(packet::RCV_DynamicSizePacket) + MAX_SKILL_TREE_PERMANENT_SKILLS;

	RCV_LevelUpAllTreeSkills() : RCV_DynamicSizePacket(ID) {}

	uint8_t skillIncrementSize() const
	{
		return _totalSize - static_cast<uint8_t>(sizeof(RCV_DynamicSizePacket));
	}

	uint8_t skillIncrement[]; //contiguous and matches permanent skills
};

struct RCV_Whisper1 : public RCV_DynamicSizePacket
{
	static constexpr Id ID = PACKET_ID(RCV_Whisper1);

	RCV_Whisper1() : RCV_DynamicSizePacket(ID) {}

	char name[MAX_NAME_LEN];
	uint8_t msg[];
};

struct RCV_Whisper2 : public RCV_DynamicSizePacket
{
	static constexpr Id ID = PACKET_ID(RCV_Whisper2);

	RCV_Whisper2() : RCV_DynamicSizePacket(ID) {}

	SessionId sessionId;
	uint8_t msg[];
};

struct RCV_GroupChat : public RCV_DynamicSizePacket
{
	static constexpr Id ID = PACKET_ID(RCV_GroupChat);
	static constexpr uint16_t MAX_SIZE = sizeof(packet::RCV_DynamicSizePacket) + MAX_CHAT_MSG_LEN;

	RCV_GroupChat() : RCV_DynamicSizePacket(ID) {}

	GroupChat _group;
	uint8_t msg[];
};

struct RCV_Attack : public RCV_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(RCV_Attack);

	RCV_Attack() : RCV_FixedSizePacket(ID) {}

	BlockId _target;
};

struct RCV_AutoAttack : public RCV_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(RCV_AutoAttack);

	RCV_AutoAttack() : RCV_FixedSizePacket(ID) {}

	BlockId _target;
};

struct SND_Chat : public SND_DynamicSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_Chat);

	SND_Chat() : SND_DynamicSizePacket(ID) {}

	void writeHeader(uint8_t msgLen)
	{
		_id = ID;
		_totalSize = sizeof(SND_Chat) + msgLen;
	}

	ChatType _type;
	uint8_t msg[];
};

struct SND_EnterCharacterSelect : public SND_DynamicSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_EnterCharacterSelect);

	//Missing the pallete ids
	struct CharInfo
	{
		uint8_t slotIndex;
		Gender gender;
		char name[MAX_NAME_LEN];
		Job job;
		uint16_t hairstyle;
		uint8_t lvl;
		Hp hp;
		Sp sp;
		Str str;
		Agi agi;
		Vit vit;
		Int int_;
		Dex dex;
		Luck luk;
		uint32_t exp;
		ItemDbId upperHeadgear;
		ItemDbId middleHeadgear;
		ItemDbId lowerHeadgear;
		MapId mapId;
	};

	SND_EnterCharacterSelect() : SND_DynamicSizePacket(ID) {}

	void writeHeader(uint8_t charInfoLen)
	{
		_id = ID;
		this->charInfoLen = charInfoLen;
		_totalSize = sizeof(SND_EnterCharacterSelect) + charInfoLen * sizeof(CharInfo);
	}

	uint8_t charInfoLen;
	CharInfo charInfo[];
};

struct SND_ExitLobby : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_ExitLobby);

	SND_ExitLobby() : SND_FixedSizePacket(ID) {}
};

struct SND_EnterLobby : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_EnterLobby);

	SND_EnterLobby() : SND_FixedSizePacket(ID) {}
};

struct SND_ReplyRegisterAccount : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_ReplyRegisterAccount);

	SND_ReplyRegisterAccount() : SND_FixedSizePacket(ID) {}

	ErrorCode status;
};

struct SND_ReplyInvalidLogin : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_ReplyInvalidLogin);

	SND_ReplyInvalidLogin() : SND_FixedSizePacket(ID) {}
};

struct SND_ReplyCreateCharacter : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_ReplyCreateCharacter);

	SND_ReplyCreateCharacter() : SND_FixedSizePacket(ID) {}

	ErrorCode status;
};

struct SND_ReplyDeleteCharacter : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_ReplyDeleteCharacter);

	SND_ReplyDeleteCharacter() : SND_FixedSizePacket(ID) {}

	ErrorCode status;
};

struct SND_EnterMap : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_EnterMap);

	SND_EnterMap(MapId mapId, Point position, Direction direction)
		:SND_FixedSizePacket(ID) 
		,_mapId(mapId)
		,_position(position)
		,_direction(direction)
	{}

	MapId _mapId;
	Point _position;
	Direction _direction;
};

//****************************** Player info packets

struct SND_PlayerInventoryReload : public SND_DynamicSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerInventoryReload);

	struct ItemInfo
	{
		ItemDbId dbId;
		uint16_t amount;
		uint8_t index;
	};

	static constexpr int MAX_SIZE()
	{
		return INVENTORY_SIZE * sizeof(ItemInfo) + sizeof(SND_PlayerInventoryReload);
	}

	static constexpr int MIN_BUFFER_SIZE()
	{
		return sizeof(ItemInfo) + sizeof(SND_PlayerInventoryReload);
	}

	SND_PlayerInventoryReload() : SND_DynamicSizePacket(ID) {}

	void writeHeader(uint8_t inventoryInfoLen)
	{
		_id = ID;
		_inventoryInfoLen = inventoryInfoLen;
		_totalSize = sizeof(SND_PlayerInventoryReload) + inventoryInfoLen * sizeof(ItemInfo);
	}

	uint8_t _inventoryInfoLen;
	ItemInfo itemInfos[];
};
static_assert(SSL_CONNECTION_BUFFERS_SIZE >= SND_PlayerInventoryReload::MIN_BUFFER_SIZE());

struct SND_PlayerFullStatus : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerFullStatus);

	SND_PlayerFullStatus() : SND_FixedSizePacket(ID) {}

	//TODO: add other battle status here
	uint8_t _equipIndexes[enum_cast(EquipSlot::Last) + 1];
	WalkSpd _walkSpeed;
	AtkSpd _atkSpeed;
};

struct SND_PlayerSkillTreeAddSkill : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerSkillTreeAddSkill);

	SND_PlayerSkillTreeAddSkill() : SND_FixedSizePacket(ID) {}

	SkillId skillId;
	uint8_t lvl;
	uint8_t index;
};

struct SND_PlayerSkillTreeRemoveSkill : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerSkillTreeRemoveSkill);

	SND_PlayerSkillTreeRemoveSkill() : SND_FixedSizePacket(ID) {}

	SkillId skillId;
	uint8_t lvl;
	uint8_t index;
};

struct SND_PlayerSkillTreeReload : public SND_DynamicSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerSkillTreeReload);

	struct SkillInfo
	{
		SkillId id;
		uint8_t level;
	};

	static constexpr int MAX_SIZE()
	{
		return MAX_SKILL_TREE_SKILLS * sizeof(SkillInfo) + sizeof(SND_PlayerSkillTreeReload);
	}

	SND_PlayerSkillTreeReload() : SND_DynamicSizePacket(ID) {}

	void writeHeader(uint8_t skillInfoLen, uint8_t permanentSkillCount, uint8_t unindexedSkillCount, Job job)
	{
		_id = ID;
		this->skillInfoLength = skillInfoLen;
		this->permanentSkillCount = permanentSkillCount;
		this->unindexedSkillCount = unindexedSkillCount;
		this->job = job;
		_totalSize = sizeof(SND_PlayerSkillTreeReload) + skillInfoLen * sizeof(SkillInfo);
	}

	Job job;
	uint8_t skillInfoLength;
	uint8_t permanentSkillCount;
	uint8_t unindexedSkillCount;
	SkillInfo skillInfos[];
};
static_assert(SSL_CONNECTION_BUFFERS_SIZE >= SND_PlayerSkillTreeReload::MAX_SIZE());

struct SND_PlayerJobOrLevelChanged : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerJobOrLevelChanged);

	SND_PlayerJobOrLevelChanged(BlockId playerId, Job job, uint8_t baseLvl, uint8_t jobLvl) 
		:SND_FixedSizePacket(ID)
		, _blockId(playerId)
		, _job(job)
		, _baseLvl(baseLvl)
		, _jobLvl(jobLvl)
	{}

	BlockId _blockId;
	Job _job;
	uint8_t _baseLvl;
	uint8_t _jobLvl;
};

struct SND_PlayerSkillPointsChanged : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerSkillPointsChanged);

	SND_PlayerSkillPointsChanged(uint8_t skillPoints) 
		:SND_FixedSizePacket(ID)
		,_skillPoints(skillPoints)
	{}

	uint8_t _skillPoints;
};

struct SND_PlayerSkillTreeLevelUpReply : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerSkillTreeLevelUpReply);

	enum class Result : uint8_t
	{
		Success,
		Failure
	};

	SND_PlayerSkillTreeLevelUpReply(bool success)
		:SND_FixedSizePacket(ID)
		,_result(success ? Result::Success : Result::Failure)
	{}

	Result _result;
};

struct SND_PlayerBuffApply : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerBuffApply);

	SND_PlayerBuffApply(BuffId buffId, uint32_t buffDuration)
		:SND_FixedSizePacket(ID)
		,_buffId(buffId)
		,_buffDuration(buffDuration)
	{}

	BuffId _buffId;
	uint32_t _buffDuration;
};

struct SND_PlayerBuffRemove : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerBuffRemove);

	SND_PlayerBuffRemove(BuffId buffId)
		:SND_FixedSizePacket(ID)
		,_buffId(buffId)
	{}
	BuffId _buffId;
};

//****************************** NPC packets
struct SND_OpenNpcShop : public SND_DynamicSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_OpenNpcShop);
	struct ItemInfo
	{
		ItemDbId dbId;
		uint32_t price;
	};

	static constexpr int MAX_SIZE()
	{
		return MAX_NPC_SHOP_ITEMS * sizeof(ItemInfo) + sizeof(SND_OpenNpcShop);
	}

	SND_OpenNpcShop() : SND_DynamicSizePacket(ID) {}

	void writeHeader(Currency currency, uint8_t discountRate, uint8_t shopItemsLength)
	{
		_id = ID;
		_currency = currency;
		_discountRate = discountRate;
		_shopItemsLength = shopItemsLength;
		_totalSize = sizeof(SND_OpenNpcShop) + shopItemsLength * sizeof(ItemInfo);
	}

	Currency _currency;
	uint8_t _discountRate;
	uint8_t _shopItemsLength;
	ItemInfo _itemInfos[];
};
static_assert(SSL_CONNECTION_BUFFERS_SIZE >= SND_OpenNpcShop::MAX_SIZE());

//****************************** Other packets
struct SND_PlayerPickUpItem : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerPickUpItem);

	SND_PlayerPickUpItem(BlockId playerId, BlockId itemId)
		:SND_FixedSizePacket(ID)
		, _playerId(playerId)
		, _itemId(itemId)
	{}

	BlockId _playerId;
	BlockId _itemId;
};

struct SND_PlayerPickUpItemFail : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerPickUpItemFail);

	SND_PlayerPickUpItemFail(ErrorCode error)
		:SND_FixedSizePacket(ID)
		, _error(error)
	{}

	ErrorCode _error;
};

struct SND_GetItem : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_GetItem);

	SND_GetItem(ItemDbId itemId, uint8_t index, int16_t amount)
		: SND_FixedSizePacket(ID)
		, _itemId(itemId)
		, _index(index)
		, _amount(amount)
	{}

	ItemDbId _itemId;
	int16_t _amount;
	uint8_t _index;
};

struct SND_DeleteItem : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_DeleteItem);

	SND_DeleteItem(uint8_t index, int16_t amount)
		: SND_FixedSizePacket(ID)
		, _index(index)
		, _amount(amount)
	{}

	int16_t _amount;
	uint8_t _index;
};

struct SND_EquipItem : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_EquipItem);

	SND_EquipItem(uint8_t index, ErrorCode error)
		: SND_FixedSizePacket(ID)
		, _index(index)
		, _error(error)
	{}

	uint8_t _index;
	ErrorCode _error;
};

struct SND_UnequipItem : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_UnequipItem);

	SND_UnequipItem(uint8_t index, ErrorCode error)
		: SND_FixedSizePacket(ID)
		, _index(index)
		, _error(error)
	{}

	uint8_t _index;
	ErrorCode _error;
};


//****************************** Movement packets

struct SND_PlayerLocalMove : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerLocalMove);

	SND_PlayerLocalMove(Point currentPosition, Point targetPosition, short startDelay)
		:SND_FixedSizePacket(ID)
		,_currentPos(currentPosition)
		,_targetPos(targetPosition)
		,_startDelay(startDelay)
	{}

	Point _currentPos;
	Point _targetPos;
	short _startDelay;
};

struct SND_PlayerOtherMove : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerOtherMove);

	SND_PlayerOtherMove(BlockId blockId, Point currentPosition, Point targetPosition, short startDelay)
		:SND_FixedSizePacket(ID)
		,_blockId(blockId)
		,_currentPos(currentPosition)
		,_targetPos(targetPosition)
		,_startDelay(startDelay)
	{}
	
	BlockId _blockId;
	Point _currentPos;
	Point _targetPos;
	short _startDelay;
};

struct SND_MonsterMove : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_MonsterMove);

	SND_MonsterMove(BlockId mobId, Point currentPosition, Point targetPosition, short startDelay)
		:SND_FixedSizePacket(ID)
		,_mobId(mobId)
		,_currentPos(currentPosition)
		,_targetPos(targetPosition)
		,_startDelay(startDelay)
	{}

	BlockId _mobId;
	Point _currentPos;
	Point _targetPos;
	short _startDelay;
};

struct SND_OtherMove : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_OtherMove);

	SND_OtherMove(BlockType blockType, BlockId blockId, Point currentPosition, Point targetPosition, short startDelay)
		:SND_FixedSizePacket(ID)
		,_blockType(blockType)
		,_blockId(blockId)
		,_currentPos(currentPosition)
		,_targetPos(targetPosition)
		,_startDelay(startDelay)
	{}

	BlockType _blockType;
	BlockId _blockId;
	Point _currentPos;
	Point _targetPos;
	short _startDelay;
};

struct SND_PlayerLocalStopMove : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerLocalStopMove);

	SND_PlayerLocalStopMove(Point position) 
		:SND_FixedSizePacket(ID)
		,position(position)
	{}

	Point position;
};

struct SND_PlayerOtherStopMove : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerOtherStopMove);

	SND_PlayerOtherStopMove(BlockId playerId, Point position)
		:SND_FixedSizePacket(ID)
		,playerId(playerId)
		,position(position)
	{}

	BlockId playerId;
	Point position;
};

struct SND_MonsterStopMove : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_MonsterStopMove);

	SND_MonsterStopMove(BlockId mobId, Point position)
		:SND_FixedSizePacket(ID)
		,mobId(mobId)
		,position(position)
	{}

	BlockId mobId;
	Point position;
};

struct SND_OtherStopMove : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_OtherStopMove);

	SND_OtherStopMove(BlockType blockType, BlockId blockId, Point position)
		:SND_FixedSizePacket(ID)
		,blockType(blockType)
		,blockId(blockId)
		,position(position)
	{}

	BlockType blockType;
	BlockId blockId;
	Point position;
};

struct SND_PlayerWarpToCell : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerWarpToCell);

	SND_PlayerWarpToCell(Point position, Direction bodyDirection, Direction headDirection)
		:SND_FixedSizePacket(ID)
		,_position(position)
		,_bodyDirection(bodyDirection)
		,_headDirection(headDirection)
	{}

	Point _position;
	Direction _bodyDirection;
	Direction _headDirection;
};

//**************************** Information packets

struct SND_PlayerEnterRange : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerEnterRange);

	SND_PlayerEnterRange(BlockId playerId, Point position, Direction bodyDirection, Direction headDirection,
						 Gender gender, Job job, uint8_t hairstyle, ItemDbId topHeadgear, ItemDbId midHeadgear,
						 ItemDbId weapon, ItemDbId shield, ItemDbId lowHeadgear, WalkSpd walkSpd, 
						 AtkSpd atkSpeed, EnterRangeType type)
		:SND_FixedSizePacket(ID)
		,_playerId(playerId)
		,_position(position)
		,_bodyDirection(bodyDirection)
		,_headDirection(headDirection)
		,_gender(gender)
		,_job(job)
		,_hairstyle(hairstyle)
		,_topHeadgear(topHeadgear)
		,_midHeadgear(midHeadgear)
		,_lowHeadgear(lowHeadgear)
		,_weapon(weapon)
		,_shield(shield)
		,_walkSpeed(walkSpd)
		,_atkSpeed(atkSpeed)
		,_type(type)
	{}

	BlockId _playerId;
	Point _position;
	Direction _bodyDirection;
	Direction _headDirection;
	Gender _gender;
	Job _job;
	uint8_t _hairstyle;
	ItemDbId _topHeadgear;
	ItemDbId _midHeadgear;
	ItemDbId _lowHeadgear;
	ItemDbId _weapon;
	ItemDbId _shield;
	WalkSpd _walkSpeed;
	AtkSpd _atkSpeed;
	EnterRangeType _type;
};

struct SND_MonsterEnterRange : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_MonsterEnterRange);

	SND_MonsterEnterRange(BlockId instanceId, Point position, Direction direction, MonsterId monsterId, WalkSpd walkSpd, AtkSpd atkSpd, EnterRangeType type)
		:SND_FixedSizePacket(ID) 
		,_mobId(instanceId)
		,_position(position)
		,_direction(direction)
		,_dbId(monsterId)
		,_walkSpeed(walkSpd)
		,_atkSpd(atkSpd)
		,_type(type)
	{}

	BlockId _mobId;
	Point _position;
	Direction _direction;
	MonsterId _dbId;
	WalkSpd _walkSpeed;
	AtkSpd _atkSpd;
	EnterRangeType _type;
};

struct SND_ItemEnterRange : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_ItemEnterRange);

	SND_ItemEnterRange(uint16_t dbId, BlockId blockId, Point position, uint16_t amount, bool showDropAnim, bool identified)
		:SND_FixedSizePacket(ID)
		,_blockId(blockId)
		,_dbId(dbId)
		,_amount(amount)
		,_position(position)
		,_showDropAnim(showDropAnim)
		,_identified(identified)
	{}

	BlockId _blockId;
	uint16_t _dbId;
	uint16_t _amount;
	Point _position;

	struct
	{
		uint8_t _showDropAnim : 1,
				_identified : 1,
				_reserved : 6;
	};
};

struct SND_OtherEnterRange : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_OtherEnterRange);

	SND_OtherEnterRange(BlockType blockType, uint16_t dbId, BlockId blockId, Point position, EnterRangeType type)
		:SND_FixedSizePacket(ID)
		,_blockType(blockType)
		,_blockId(blockId)
		,_position(position)
		,_dbId(dbId)
		,_type(type)
	{}

	BlockType _blockType;
	BlockId _blockId;
	int16_t _dbId;
	Point _position;
	EnterRangeType _type;
};

struct SND_PlayerLeaveRange : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerLeaveRange);

	SND_PlayerLeaveRange(BlockId playerId, LeaveRangeType type)
		:SND_FixedSizePacket(ID)
		,_playerId(playerId)
		,_type(type)
	{}

	BlockId _playerId;
	LeaveRangeType _type;
};

struct SND_MonsterLeaveRange : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_MonsterLeaveRange);

	SND_MonsterLeaveRange(BlockId instanceId, LeaveRangeType type)
		:SND_FixedSizePacket(ID)
		,_mobId(instanceId)
		,_type(type)
	{}

	BlockId _mobId;
	LeaveRangeType _type;
};

struct SND_OtherLeaveRange : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_OtherLeaveRange);

	SND_OtherLeaveRange(BlockType blockType, uint16_t dbId, BlockId blockId, LeaveRangeType type)
		:SND_FixedSizePacket(ID)
		,blockType(blockType)
		,blockId(blockId)
		,dbId(dbId)
		,type(type)
	{}

	BlockType blockType;
	BlockId blockId;
	uint16_t dbId;
	LeaveRangeType type;
};

struct SND_BlockDied : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_BlockDied);

	SND_BlockDied(BlockId blockId)
		:SND_FixedSizePacket(ID)
		,_blockId(blockId)
	{}

	BlockId _blockId;
};

struct SND_LocalPlayerStatusChange : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_LocalPlayerStatusChange);

	SND_LocalPlayerStatusChange(LocalPlayerChangeType type, uint32_t value)
		:SND_FixedSizePacket(ID)
		, _type(type)
		, _value(value)
	{}

	LocalPlayerChangeType _type;
	uint32_t _value;
};

struct SND_OtherPlayerStatusChange : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_OtherPlayerStatusChange);

	SND_OtherPlayerStatusChange(BlockId blockId, OtherPlayerChangeType type, uint32_t value)
		:SND_FixedSizePacket(ID)
		, _blockId(blockId)
		, _type(type)
		, _value(value)
	{}

	BlockId _blockId;
	OtherPlayerChangeType _type;
	uint32_t _value;
};

struct SND_MonsterStatusChange : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_MonsterStatusChange);

	SND_MonsterStatusChange(BlockId blockId, MonsterChangeType type, uint32_t value)
		:SND_FixedSizePacket(ID)
		, _blockId(blockId)
		, _type(type)
		, _value(value)
	{}

	BlockId _blockId;
	MonsterChangeType _type;
	uint32_t _value;
};

//**************************** Skill packets
struct SND_PlayerLocalBeginAoECast : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerLocalBeginAoECast);

	SND_PlayerLocalBeginAoECast(SkillId skillId, CastTime castTime, Point position)
		:SND_FixedSizePacket(ID)
		,skillId(skillId)
		,castTime(castTime)
		,targetPosition(position)
	{}

	SkillId skillId;
	CastTime castTime;
	Point targetPosition;
};

struct SND_PlayerOtherBeginAoECast : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerOtherBeginAoECast);

	SND_PlayerOtherBeginAoECast(BlockId playerId, SkillId skillId, CastTime castTime, Point position)
		:SND_FixedSizePacket(ID)
		,playerId(playerId)
		,skillId(skillId)
		,castTime(castTime)
		,targetPosition(position)
	{}

	BlockId playerId;
	SkillId skillId;
	CastTime castTime;
	Point targetPosition;
};

struct SND_MonsterBeginAoECast : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_MonsterBeginAoECast);

	SND_MonsterBeginAoECast(BlockId monsterId, SkillId skillId, CastTime castTime, Point position)
		:SND_FixedSizePacket(ID)
		,monsterId(monsterId)
		,skillId(skillId)
		,castTime(castTime)
		,targetPosition(position)
	{}

	BlockId monsterId;
	SkillId skillId;
	CastTime castTime;
	Point targetPosition;
};

struct SND_OtherBeginAoECast : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_OtherBeginAoECast);

	SND_OtherBeginAoECast(BlockType blockType, BlockId blockId, SkillId skillId, CastTime castTime, Point position)
		:SND_FixedSizePacket(ID)
		,blockType(blockType)
		,blockId(blockId)
		,skillId(skillId)
		,castTime(castTime)
		,targetPosition(position)
	{}

	BlockType blockType;
	BlockId blockId;
	SkillId skillId;
	CastTime castTime;
	Point targetPosition;
};

struct SND_PlayerLocalBeginTargetCast : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerLocalBeginTargetCast);

	SND_PlayerLocalBeginTargetCast(SkillId skillId, CastTime castTime, const Block& target) 
		:SND_FixedSizePacket(ID)
		,skillId (skillId)
		,castTime(castTime)
		,targetId(target._id)
		,targetBlockType(target._type)
	{}

	SkillId skillId;
	CastTime castTime;
	BlockId targetId;
	BlockType targetBlockType;
};

struct SND_PlayerOtherBeginTargetCast : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerOtherBeginTargetCast);

	SND_PlayerOtherBeginTargetCast(BlockId playerId, SkillId skillId, CastTime castTime, const Block& target)
		:SND_FixedSizePacket(ID)
		,playerId(playerId)
		,skillId(skillId)
		,castTime(castTime)
		,targetId(target._id)
		,targetBlockType(target._type)
	{}

	BlockId playerId;
	SkillId skillId;
	CastTime castTime;
	BlockId targetId;
	BlockType targetBlockType;
};

struct SND_MonsterBeginTargetCast : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_MonsterBeginTargetCast);

	SND_MonsterBeginTargetCast(BlockId srcId, SkillId skillId, CastTime castTime, const Block& target)
		:SND_FixedSizePacket(ID)
		,monsterId(srcId)
		,skillId(skillId)
		,castTime(castTime)
		,targetId(target._id)
		,targetBlockType(target._type)
	{}

	BlockId monsterId;
	SkillId skillId;
	CastTime castTime;
	BlockId targetId;
	BlockType targetBlockType;
};

struct SND_OtherBeginTargetCast : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_OtherBeginTargetCast);

	SND_OtherBeginTargetCast(BlockType blockType, BlockId srcId, SkillId skillId, CastTime castTime, const Block& target)
		:SND_FixedSizePacket(ID)
		,blockType(blockType)
		,blockId(srcId)
		,skillId(skillId)
		,castTime(castTime)
		,targetId(target._id)
		,targetBlockType(target._type)
	{}

	BlockType blockType;
	BlockId blockId;
	SkillId skillId;
	CastTime castTime;
	BlockId targetId;
	BlockType targetBlockType;
};

struct SND_PlayerLocalBeginSelfCast : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerLocalBeginSelfCast);

	SND_PlayerLocalBeginSelfCast(SkillId skillId, CastTime castTime)
		:SND_FixedSizePacket(ID)
		,skillId(skillId)
		,castTime(castTime)
	{}

	SkillId skillId;
	CastTime castTime;
};

struct SND_PlayerOtherBeginSelfCast : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerOtherBeginSelfCast);

	SND_PlayerOtherBeginSelfCast(BlockId srcId, SkillId skillId, CastTime castTime)
		:SND_FixedSizePacket(ID)
		,playerId(srcId)
		,skillId(skillId)
		,castTime(castTime)
	{}

	BlockId playerId;
	SkillId skillId;
	CastTime castTime;
};

struct SND_MonsterBeginSelfCast : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_MonsterBeginSelfCast);

	SND_MonsterBeginSelfCast(BlockId srcId, SkillId skillId, CastTime castTime)
		:SND_FixedSizePacket(ID)
		,monsterId(srcId)
		,skillId(skillId)
		,castTime(castTime)
	{}

	BlockId monsterId;
	SkillId skillId;
	CastTime castTime;
};

struct SND_OtherBeginSelfCast : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_OtherBeginSelfCast);

	SND_OtherBeginSelfCast(BlockType blockType, BlockId srcId, SkillId skillId, CastTime castTime)
		:SND_FixedSizePacket(ID)
		,blockType(blockType)
		,blockId(srcId)
		,skillId(skillId)
		,castTime(castTime)
	{}
	
	BlockType blockType;
	BlockId blockId;
	SkillId skillId;
	CastTime castTime;
};

struct SND_PlayerLocalFinishedAoECast : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerLocalFinishedAoECast);

	SND_PlayerLocalFinishedAoECast(SkillId skillId, Point position)
		:SND_FixedSizePacket(ID)
		,skillId(skillId)
		,position(position)
	{}

	SkillId skillId;
	Point position;
};

struct SND_PlayerOtherFinishedAoECast : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerOtherFinishedAoECast);

	SND_PlayerOtherFinishedAoECast(BlockId playerId, SkillId skillId, Point position)
		:SND_FixedSizePacket(ID)
		,playerId(playerId)
		,skillId(skillId)
		,position(position)
	{}

	BlockId playerId;
	SkillId skillId;
	Point position;
};

struct SND_MonsterFinishedAoECast : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_MonsterFinishedAoECast);

	SND_MonsterFinishedAoECast(BlockId monsterId, SkillId skillId, Point position)
		:SND_FixedSizePacket(ID)
		,monsterId(monsterId)
		,skillId(skillId)
		,position(position) 
	{}

	BlockId monsterId;
	SkillId skillId;
	Point position;
};

struct SND_OtherFinishedAoECast : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_OtherFinishedAoECast);

	SND_OtherFinishedAoECast(const BlockType blockType, BlockId blockId, SkillId skillId, Point position)
		:SND_FixedSizePacket(ID)
		,blockType(blockType)
		,blockId(blockId)
		,skillId(skillId)
		,position(position)
	{}

	BlockType blockType;
	BlockId blockId;
	SkillId skillId;
	Point position;
};

struct SND_PlayerLocalFinishedTargetCast : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerLocalFinishedTargetCast);

	SND_PlayerLocalFinishedTargetCast(SkillId skillId, const Block& target)
		:SND_FixedSizePacket(ID)
		,skillId(skillId)
		,targetId(target._id)
		,targetBlockType(target._type)
	{}

	SkillId skillId;
	BlockId targetId;
	BlockType targetBlockType;
};

struct SND_PlayerOtherFinishedTargetCast : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerOtherFinishedTargetCast);

	SND_PlayerOtherFinishedTargetCast(BlockId playerId, SkillId skillId, const Block& target)
		:SND_FixedSizePacket(ID)
		,playerId(playerId)
		,skillId(skillId)
		,targetId(target._id)
		,targetBlockType(target._type)
	{}

	BlockId playerId;
	SkillId skillId;
	BlockId targetId;
	BlockType targetBlockType;
};

struct SND_MonsterFinishedTargetCast : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_MonsterFinishedTargetCast);

	SND_MonsterFinishedTargetCast(BlockId blockId, SkillId skillId, const Block& target)
		:SND_FixedSizePacket(ID)
		,monsterId(blockId)
		,skillId(skillId)
		,targetId(target._id)
		,targetBlockType(target._type)
	{}

	BlockId monsterId;
	SkillId skillId;
	BlockId targetId;
	BlockType targetBlockType;
};

struct SND_OtherFinishedTargetCast : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_OtherFinishedTargetCast);

	SND_OtherFinishedTargetCast(BlockType blockType, BlockId blockId, SkillId skillId, const Block& target)
		:SND_FixedSizePacket(ID)
		,blockType(blockType)
		,blockId(blockId)
		,skillId(skillId)
		,targetId(target._id)
		,targetBlockType(target._type)
	{}

	BlockType blockType;
	BlockId blockId;
	SkillId skillId;
	BlockId targetId;
	BlockType targetBlockType;
};

struct SND_PlayerLocalFinishedSelfCast : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerLocalFinishedSelfCast);

	SND_PlayerLocalFinishedSelfCast(SkillId skillId)
		:SND_FixedSizePacket(ID)
		,skillId(skillId)
	{}

	SkillId skillId;
};

struct SND_PlayerOtherFinishedSelfCast : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerOtherFinishedSelfCast);

	SND_PlayerOtherFinishedSelfCast(BlockId playerId, SkillId skillId)
		:SND_FixedSizePacket(ID)
		,playerId(playerId)
		,skillId(skillId)
	{}

	BlockId playerId;
	SkillId skillId;
};

struct SND_MonsterFinishedSelfCast : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_MonsterFinishedSelfCast);

	SND_MonsterFinishedSelfCast(BlockId mobId, SkillId skillId)
		:SND_FixedSizePacket(ID)
		,monsterId(mobId)
		,skillId(skillId)
	{}

	BlockId monsterId;
	SkillId skillId;
};

struct SND_OtherFinishedSelfCast : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_OtherFinishedSelfCast);

	SND_OtherFinishedSelfCast(BlockType blockType, BlockId blockId, SkillId skillId)
		:SND_FixedSizePacket(ID)
		,blockType(blockType)
		,blockId(blockId)
		,skillId(skillId)
	{}

	BlockType blockType;
	BlockId blockId;
	SkillId skillId;
};

struct SND_PlayerLocalReceiveDamage : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerLocalReceiveDamage);

	SND_PlayerLocalReceiveDamage(uint32_t damage, DamageHitType dmgHitType)
		:SND_FixedSizePacket(ID)
		,damage(damage)
		,damageType(dmgHitType)
	{}

	uint32_t damage;
	DamageHitType damageType;
};

struct SND_PlayerOtherReceiveDamage : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_PlayerOtherReceiveDamage);

	SND_PlayerOtherReceiveDamage(BlockId playerId, uint32_t damage, DamageHitType dmgHitType)
		:SND_FixedSizePacket(ID)
		,playerId(playerId)
		,damage(damage)
		,dmgHitType(dmgHitType)
	{}

	BlockId playerId;
	uint32_t damage;
	DamageHitType dmgHitType;
};

struct SND_MonsterReceiveDamage : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_MonsterReceiveDamage);

	SND_MonsterReceiveDamage(BlockId monsterId, uint32_t damage, DamageHitType dmgHitType)
		:SND_FixedSizePacket(ID)
		,monsterId(monsterId)
		,damage(damage)
		,damageType(dmgHitType)
	{}

	BlockId monsterId;
	uint32_t damage;
	DamageHitType damageType;
};

struct SND_OtherReceiveDamage : public SND_FixedSizePacket
{
	static constexpr Id ID = PACKET_ID(SND_OtherReceiveDamage);

	SND_OtherReceiveDamage(BlockId blockId, BlockType blockType, uint32_t damage, DamageHitType dmgHitType)
		:SND_FixedSizePacket(ID)
		,blockType(blockType)
		,blockId(blockId)
		,damage(damage)
		,damageType(dmgHitType)
	{}

	BlockType blockType;
	BlockId blockId;
	uint32_t damage;
	DamageHitType damageType;
};


#pragma warning(default:4200)
#pragma pack(pop)

#undef PACKET_ID
}