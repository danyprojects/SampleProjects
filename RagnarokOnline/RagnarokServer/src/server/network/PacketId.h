#pragma once

#ifndef UNIT_TEST_PACKET_ID

#include <limits>
#include <cstdint>
#endif

namespace packet
{
	enum class Id : uint16_t
	{
		RCV_RegisterAccount = 0,
		RCV_Login,
		RCV_SelectCharacter,
		RCV_CreateCharacter,
		RCV_DeleteCharacter,
		LastLobbyPacket = RCV_DeleteCharacter,
		RCV_ReturnToCharacterSelect,

		//Move
		RCV_PlayerMove,
		RCV_OtherMove,

		//Item related
		RCV_PickUpItem,
		RCV_DropItem,
		RCV_UseItem,
		RCV_UnequipItem,

		//Npc related
		RCV_NpcAction,

		//battle packets
		RCV_CastAreaOfEffectSkill,
		RCV_CastSingleTargetSkill,
		RCV_CastSelfTargetSkill,
		RCV_Attack,
		RCV_AutoAttack,

		RCV_ElevatedAtCommand,
		RCV_LevelUpSingleSkill,
		RCV_LevelUpMultipleSkills,
		RCV_LevelUpAllTreeSkills,

		RCV_Whisper1,
		RCV_Whisper2,
		RCV_GroupChat,
		LastReceivePacket = RCV_GroupChat,

		SND_EnterCharacterSelect,
		SND_EnterLobby,
		SND_ExitLobby,
		SND_ReplyInvalidLogin,
		SND_ReplyRegisterAccount,
		SND_ReplyCreateCharacter,
		SND_ReplyDeleteCharacter,
		SND_EnterMap,

		//Player info packets
		SND_PlayerInventoryReload,
		SND_PlayerFullStatus,
		SND_PlayerSkillTreeAddSkill,
		SND_PlayerSkillTreeRemoveSkill,
		SND_PlayerSkillTreeReload,
		SND_PlayerJobOrLevelChanged,
		SND_PlayerSkillPointsChanged,
		SND_PlayerSkillTreeLevelUpReply,
		SND_PlayerBuffApply,
		SND_PlayerBuffRemove,

		//Npc packets
		SND_OpenNpcShop,

		//Item packets
		SND_PlayerPickUpItem,
		SND_PlayerPickUpItemFail,
		SND_GetItem,
		SND_DeleteItem,
		SND_EquipItem,
		SND_UnequipItem,

		//Move packets
		SND_PlayerLocalMove,
		SND_PlayerOtherMove,
		SND_MonsterMove,
		SND_OtherMove,
		SND_PlayerLocalStopMove,
		SND_PlayerOtherStopMove,
		SND_MonsterStopMove,
		SND_OtherStopMove,
		SND_PlayerWarpToCell,

		//Information packets
		SND_PlayerEnterRange,
		SND_MonsterEnterRange,
		SND_ItemEnterRange,
		SND_OtherEnterRange,
		SND_PlayerLeaveRange,
		SND_MonsterLeaveRange,
		SND_OtherLeaveRange,
		SND_BlockDied,
		SND_LocalPlayerStatusChange,
		SND_OtherPlayerStatusChange,
		SND_MonsterStatusChange,

		//Skill packets
		SND_PlayerLocalBeginAoECast,
		SND_PlayerOtherBeginAoECast,
		SND_MonsterBeginAoECast,
		SND_OtherBeginAoECast,

		SND_PlayerLocalBeginTargetCast,
		SND_PlayerOtherBeginTargetCast,
		SND_MonsterBeginTargetCast,
		SND_OtherBeginTargetCast,

		SND_PlayerLocalBeginSelfCast,
		SND_PlayerOtherBeginSelfCast,
		SND_MonsterBeginSelfCast,
		SND_OtherBeginSelfCast,

		SND_PlayerLocalFinishedAoECast,
		SND_PlayerOtherFinishedAoECast,
		SND_MonsterFinishedAoECast,
		SND_OtherFinishedAoECast,

		SND_PlayerLocalFinishedTargetCast,
		SND_PlayerOtherFinishedTargetCast,
		SND_MonsterFinishedTargetCast,
		SND_OtherFinishedTargetCast,

		SND_PlayerLocalFinishedSelfCast,
		SND_PlayerOtherFinishedSelfCast,
		SND_MonsterFinishedSelfCast,
		SND_OtherFinishedSelfCast,

		SND_PlayerLocalReceiveDamage,
		SND_PlayerOtherReceiveDamage,
		SND_MonsterReceiveDamage,
		SND_OtherReceiveDamage,

		SND_Chat,

		//NEVER received/sent to network only for internal processing uses
		DiscardWithSize = std::numeric_limits<uint16_t>::max() - 1,
		Discard         = std::numeric_limits<uint16_t>::max()
	};
}