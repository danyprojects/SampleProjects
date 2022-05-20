using RO.Common;
using RO.MapObjects;
using System;

namespace RO.Network
{
    enum PacketIds : short
    {
        SND_RegisterAccount = 0,
        SND_Login,
        SND_SelectCharacter,
        SND_CreateCharacter,
        SND_DeleteCharacter,
        SND_ReturnToCharacterSelect,
        SND_PlayerMove,
        SND_OtherMove,

        //battle packets
        SND_CastAreaOfEffectSkill,
        SND_CastSingleTargetSkill,
        SND_CastSelfTargetSkill,

        SND_ElevatedAtCommand,
        SND_LevelUpSingleSkill,
        SND_LevelUpMultipleSkills,
        SND_LevelUpAllTreeSkills,

        SND_Chat,
        SND_End = SND_Chat,

        RCV_Start = SND_End + 1,
        RCV_EnterCharacterSelect = RCV_Start,
        RCV_EnterLobby,
        RCV_ExitLobby,
        RCV_ReplyInvalidLogin,
        RCV_ReplyRegisterAccount,
        RCV_ReplyCreateCharacter,
        RCV_ReplyDeleteCharacter,
        RCV_EnterMap,

        //Player info packets
        RCV_PlayerSkillTreeAddSkill,
        RCV_PlayerSkillTreeRemoveSkill,
        RCV_PlayerSkillTreeReload,
        RCV_PlayerJobOrLevelChanged,
        RCV_PlayerSkillPointsChanged,
        RCV_PlayerSkillTreeLevelUpReply,
        RCV_PlayerBuffApply,
        RCV_PlayerBuffRemove,

        //Movement packets
        RCV_PlayerLocalMove,
        RCV_PlayerOtherMove,
        RCV_MonsterMove,
        RCV_OtherMove,
        RCV_PlayerLocalStopMove,
        RCV_PlayerOtherStopMove,
        RCV_MonsterStopMove,
        RCV_OtherStopMove,
        RCV_PlayerWarpToCell,

        //Information packets
        RCV_PlayerEnterRange,
        RCV_MonsterEnterRange,
        RCV_OtherEnterRange,
        RCV_PlayerLeaveRange,
        RCV_MonsterLeaveRange,
        RCV_OtherLeaveRange,
        RCV_BlockDied,

        //Skill packets
        RCV_PlayerLocalBeginAoECast,
        RCV_PlayerOtherBeginAoECast,
        RCV_MonsterBeginAoECast,
        RCV_OtherBeginAoECast,

        RCV_PlayerLocalBeginTargetCast,
        RCV_PlayerOtherBeginTargetCast,
        RCV_MonsterBeginTargetCast,
        RCV_OtherBeginTargetCast,

        RCV_PlayerLocalBeginSelfCast,
        RCV_PlayerOtherBeginSelfCast,
        RCV_MonsterBeginSelfCast,
        RCV_OtherBeginSelfCast,

        RCV_PlayerLocalFinishedAoECast,
        RCV_PlayerOtherFinishedAoECast,
        RCV_MonsterFinishedAoECast,
        RCV_OtherFinishedAoECast,

        RCV_PlayerLocalFinishedTargetCast,
        RCV_PlayerOtherFinishedTargetCast,
        RCV_MonsterFinishedTargetCast,
        RCV_OtherFinishedTargetCast,

        RCV_PlayerLocalFinishedSelfCast,
        RCV_PlayerOtherFinishedSelfCast,
        RCV_MonsterFinishedSelfCast,
        RCV_OtherFinishedSelfCast,

        RCV_PlayerLocalReceiveDamage,
        RCV_PlayerOtherReceiveDamage,
        RCV_MonsterReceiveDamage,
        RCV_OtherReceiveDamage,

        RCV_Chat,
        RCV_End = RCV_Chat,

        //Internal. Bypass to notify reader of connection closed
        ConnectionClosed,
        None
    }

    public sealed class PacketFactory
    {
        public readonly static short[] RcvPacketSizes = new short[(int)PacketIds.RCV_End - (int)PacketIds.SND_End];
        public readonly static Func<byte[], RCV_Packet>[] PacketFromBytes = new Func<byte[], RCV_Packet>[(int)PacketIds.RCV_End - (int)PacketIds.SND_End];

        static PacketFactory()
        {
            #region FromBytes
            PacketFromBytes[(int)PacketIds.RCV_ReplyRegisterAccount - (int)PacketIds.RCV_Start] = (buffer) =>
                {
                    var packet = new RCV_ReplyRegisterAccount();
                    packet.FromBytes(ref buffer);
                    return packet;
                };
            PacketFromBytes[(int)PacketIds.RCV_EnterCharacterSelect - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_EnterCharacterSelect();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_EnterLobby - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_EnterLobby();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_ExitLobby - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_ExitLobby();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_ReplyInvalidLogin - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_ReplyInvalidLogin();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_EnterMap - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_EnterMap();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_PlayerSkillTreeAddSkill - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_PlayerSkillTreeAddSkill();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_PlayerSkillTreeRemoveSkill - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_PlayerSkillTreeRemoveSkill();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_PlayerSkillTreeReload - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_PlayerSkillTreeReload();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_ReplyCreateCharacter - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_ReplyCreateCharacter();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_PlayerLocalMove - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_PlayerLocalMove();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_PlayerOtherMove - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_PlayerOtherMove();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_MonsterMove - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_MonsterMove();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_OtherMove - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_OtherMove();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_PlayerLocalStopMove - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_PlayerLocalStopMove();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_PlayerOtherStopMove - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_PlayerOtherStopMove();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_MonsterStopMove - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_MonsterStopMove();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_OtherStopMove - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_OtherStopMove();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_PlayerWarpToCell - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_PlayerWarpToCell();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_PlayerEnterRange - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_PlayerEnterRange();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_MonsterEnterRange - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_MonsterEnterRange();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_OtherEnterRange - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_OtherEnterRange();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_PlayerLeaveRange - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_PlayerLeaveRange();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_MonsterLeaveRange - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_MonsterLeaveRange();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_OtherLeaveRange - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_OtherLeaveRange();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_BlockDied - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_BlockDied();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_PlayerLocalBeginAoECast - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_PlayerLocalBeginAoECast();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_PlayerOtherBeginAoECast - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_PlayerOtherBeginAoECast();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_MonsterBeginAoECast - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_MonsterBeginAoECast();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_OtherBeginAoECast - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_OtherBeginAoECast();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_PlayerLocalBeginTargetCast - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_PlayerLocalBeginTargetCast();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_PlayerOtherBeginTargetCast - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_PlayerOtherBeginTargetCast();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_MonsterBeginTargetCast - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_MonsterBeginTargetCast();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_OtherBeginTargetCast - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_OtherBeginTargetCast();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_PlayerLocalBeginSelfCast - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_PlayerLocalBeginSelfCast();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_PlayerOtherBeginSelfCast - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_PlayerOtherBeginSelfCast();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_MonsterBeginSelfCast - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_MonsterBeginSelfCast();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_OtherBeginSelfCast - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_OtherBeginSelfCast();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_PlayerLocalFinishedAoECast - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_PlayerLocalFinishedAoECast();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_PlayerOtherFinishedAoECast - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_PlayerOtherFinishedAoECast();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_MonsterFinishedAoECast - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_MonsterFinishedAoECast();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_OtherFinishedAoECast - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_OtherFinishedAoECast();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_PlayerLocalFinishedTargetCast - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_PlayerLocalFinishedTargetCast();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_PlayerOtherFinishedTargetCast - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_PlayerOtherFinishedTargetCast();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_MonsterFinishedTargetCast - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_MonsterFinishedTargetCast();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_OtherFinishedTargetCast - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_OtherFinishedTargetCast();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_PlayerLocalFinishedSelfCast - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_PlayerLocalFinishedSelfCast();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_PlayerOtherFinishedSelfCast - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_PlayerOtherFinishedSelfCast();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_MonsterFinishedSelfCast - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_MonsterFinishedSelfCast();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_OtherFinishedSelfCast - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_OtherFinishedSelfCast();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_PlayerLocalReceiveDamage - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_PlayerLocalReceiveDamage();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_PlayerOtherReceiveDamage - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_PlayerOtherReceiveDamage();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_MonsterReceiveDamage - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_MonsterReceiveDamage();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_OtherReceiveDamage - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_OtherReceiveDamage();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_PlayerJobOrLevelChanged - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_PlayerJobOrLevelChanged();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_PlayerSkillPointsChanged - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_PlayerSkillPointsChanged();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_PlayerSkillTreeLevelUpReply - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_PlayerSkillTreeLevelUpReply();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_PlayerBuffApply - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_PlayerBuffApply();
                packet.FromBytes(ref buffer);
                return packet;
            };
            PacketFromBytes[(int)PacketIds.RCV_PlayerBuffRemove - (int)PacketIds.RCV_Start] = (buffer) =>
            {
                var packet = new RCV_PlayerBuffRemove();
                packet.FromBytes(ref buffer);
                return packet;
            };
            #endregion

            #region PacketSizes
            //Receive packet sizes. Use max value to say it's dynamic
            RcvPacketSizes[(int)PacketIds.RCV_ReplyRegisterAccount - (int)PacketIds.RCV_Start] = RCV_ReplyRegisterAccount.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_ReplyCreateCharacter - (int)PacketIds.RCV_Start] = RCV_ReplyCreateCharacter.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_EnterMap - (int)PacketIds.RCV_Start] = RCV_EnterMap.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_PlayerSkillTreeAddSkill - (int)PacketIds.RCV_Start] = RCV_PlayerSkillTreeAddSkill.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_PlayerSkillTreeRemoveSkill - (int)PacketIds.RCV_Start] = RCV_PlayerSkillTreeRemoveSkill.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_PlayerSkillTreeReload - (int)PacketIds.RCV_Start] = RCV_PlayerSkillTreeReload.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_EnterCharacterSelect - (int)PacketIds.RCV_Start] = RCV_EnterCharacterSelect.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_EnterLobby - (int)PacketIds.RCV_Start] = RCV_EnterLobby.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_ExitLobby - (int)PacketIds.RCV_Start] = RCV_ExitLobby.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_ReplyInvalidLogin - (int)PacketIds.RCV_Start] = RCV_ReplyInvalidLogin.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_PlayerLocalMove - (int)PacketIds.RCV_Start] = RCV_PlayerLocalMove.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_PlayerOtherMove - (int)PacketIds.RCV_Start] = RCV_PlayerOtherMove.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_MonsterMove - (int)PacketIds.RCV_Start] = RCV_MonsterMove.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_OtherMove - (int)PacketIds.RCV_Start] = RCV_OtherMove.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_PlayerLocalStopMove - (int)PacketIds.RCV_Start] = RCV_PlayerLocalStopMove.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_PlayerOtherStopMove - (int)PacketIds.RCV_Start] = RCV_PlayerOtherStopMove.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_MonsterStopMove - (int)PacketIds.RCV_Start] = RCV_MonsterStopMove.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_OtherStopMove - (int)PacketIds.RCV_Start] = RCV_OtherStopMove.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_PlayerWarpToCell - (int)PacketIds.RCV_Start] = RCV_PlayerWarpToCell.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_PlayerEnterRange - (int)PacketIds.RCV_Start] = RCV_PlayerEnterRange.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_MonsterEnterRange - (int)PacketIds.RCV_Start] = RCV_MonsterEnterRange.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_OtherEnterRange - (int)PacketIds.RCV_Start] = RCV_OtherEnterRange.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_PlayerLeaveRange - (int)PacketIds.RCV_Start] = RCV_PlayerLeaveRange.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_MonsterLeaveRange - (int)PacketIds.RCV_Start] = RCV_MonsterLeaveRange.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_OtherLeaveRange - (int)PacketIds.RCV_Start] = RCV_OtherLeaveRange.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_BlockDied - (int)PacketIds.RCV_Start] = RCV_BlockDied.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_PlayerLocalBeginAoECast - (int)PacketIds.RCV_Start] = RCV_PlayerLocalBeginAoECast.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_PlayerOtherBeginAoECast - (int)PacketIds.RCV_Start] = RCV_PlayerOtherBeginAoECast.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_MonsterBeginAoECast - (int)PacketIds.RCV_Start] = RCV_MonsterBeginAoECast.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_OtherBeginAoECast - (int)PacketIds.RCV_Start] = RCV_OtherBeginAoECast.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_PlayerLocalBeginTargetCast - (int)PacketIds.RCV_Start] = RCV_PlayerLocalBeginTargetCast.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_PlayerOtherBeginTargetCast - (int)PacketIds.RCV_Start] = RCV_PlayerOtherBeginTargetCast.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_MonsterBeginTargetCast - (int)PacketIds.RCV_Start] = RCV_MonsterBeginTargetCast.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_OtherBeginTargetCast - (int)PacketIds.RCV_Start] = RCV_OtherBeginTargetCast.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_PlayerLocalBeginSelfCast - (int)PacketIds.RCV_Start] = RCV_PlayerLocalBeginSelfCast.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_PlayerOtherBeginSelfCast - (int)PacketIds.RCV_Start] = RCV_PlayerOtherBeginSelfCast.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_MonsterBeginSelfCast - (int)PacketIds.RCV_Start] = RCV_MonsterBeginSelfCast.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_OtherBeginSelfCast - (int)PacketIds.RCV_Start] = RCV_OtherBeginSelfCast.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_PlayerLocalFinishedAoECast - (int)PacketIds.RCV_Start] = RCV_PlayerLocalFinishedAoECast.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_PlayerOtherFinishedAoECast - (int)PacketIds.RCV_Start] = RCV_PlayerOtherFinishedAoECast.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_MonsterFinishedAoECast - (int)PacketIds.RCV_Start] = RCV_MonsterFinishedAoECast.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_OtherFinishedAoECast - (int)PacketIds.RCV_Start] = RCV_OtherFinishedAoECast.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_PlayerLocalFinishedTargetCast - (int)PacketIds.RCV_Start] = RCV_PlayerLocalFinishedTargetCast.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_PlayerOtherFinishedTargetCast - (int)PacketIds.RCV_Start] = RCV_PlayerOtherFinishedTargetCast.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_MonsterFinishedTargetCast - (int)PacketIds.RCV_Start] = RCV_MonsterFinishedTargetCast.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_OtherFinishedTargetCast - (int)PacketIds.RCV_Start] = RCV_OtherFinishedTargetCast.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_PlayerLocalFinishedSelfCast - (int)PacketIds.RCV_Start] = RCV_PlayerLocalFinishedSelfCast.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_PlayerOtherFinishedSelfCast - (int)PacketIds.RCV_Start] = RCV_PlayerOtherFinishedSelfCast.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_MonsterFinishedSelfCast - (int)PacketIds.RCV_Start] = RCV_MonsterFinishedSelfCast.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_OtherFinishedSelfCast - (int)PacketIds.RCV_Start] = RCV_OtherFinishedSelfCast.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_PlayerLocalReceiveDamage - (int)PacketIds.RCV_Start] = RCV_PlayerLocalReceiveDamage.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_PlayerOtherReceiveDamage - (int)PacketIds.RCV_Start] = RCV_PlayerOtherReceiveDamage.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_MonsterReceiveDamage - (int)PacketIds.RCV_Start] = RCV_MonsterReceiveDamage.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_OtherReceiveDamage - (int)PacketIds.RCV_Start] = RCV_OtherReceiveDamage.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_PlayerJobOrLevelChanged - (int)PacketIds.RCV_Start] = RCV_PlayerJobOrLevelChanged.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_PlayerSkillPointsChanged - (int)PacketIds.RCV_Start] = RCV_PlayerSkillPointsChanged.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_PlayerSkillTreeLevelUpReply - (int)PacketIds.RCV_Start] = RCV_PlayerSkillTreeLevelUpReply.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_PlayerBuffApply - (int)PacketIds.RCV_Start] = RCV_PlayerBuffApply.FIXED_SIZE;
            RcvPacketSizes[(int)PacketIds.RCV_PlayerBuffRemove - (int)PacketIds.RCV_Start] = RCV_PlayerBuffRemove.FIXED_SIZE;
            #endregion
        }
    }

    public class SND_RegisterAccount : SND_Packet
    {
        const short FIXED_SIZE = sizeof(short) + Constants.MAX_NAME_LEN + Constants.MAX_PASSWORD_SIZE;

        public byte[] ToBytes()
        {
            byte[] buffer = new byte[FIXED_SIZE];
            int index = 0;
            NetworkUtility.AppendNumber(Id, ref buffer, ref index);
            NetworkUtility.AppendFixedString(Name, ref buffer, ref index, Constants.MAX_NAME_LEN);
            NetworkUtility.AppendFixedString(Password, ref buffer, ref index, Constants.MAX_PASSWORD_SIZE);
            return buffer;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.SND_RegisterAccount;
        public string Name;
        public string Password;
    }

    public class SND_Login : SND_Packet
    {
        const short FIXED_SIZE = sizeof(short) + Constants.MAX_NAME_LEN + Constants.MAX_PASSWORD_SIZE;

        public byte[] ToBytes()
        {
            byte[] buffer = new byte[FIXED_SIZE];
            int index = 0;
            NetworkUtility.AppendNumber(Id, ref buffer, ref index);
            NetworkUtility.AppendFixedString(Name, ref buffer, ref index, Constants.MAX_NAME_LEN);
            NetworkUtility.AppendFixedString(Password, ref buffer, ref index, Constants.MAX_PASSWORD_SIZE);
            return buffer;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.SND_Login;
        public string Name;
        public string Password;
    }

    public class SND_CreateCharacter : SND_Packet
    {
        const short FIXED_SIZE = sizeof(short) + sizeof(byte) * 9 + Constants.MAX_NAME_LEN;

        public byte[] ToBytes()
        {
            byte[] buffer = new byte[FIXED_SIZE];
            int index = 0;

            NetworkUtility.AppendNumber(Id, ref buffer, ref index);
            NetworkUtility.AppendNumber(Index, ref buffer, ref index);
            NetworkUtility.AppendNumber(agi, ref buffer, ref index);
            NetworkUtility.AppendNumber(str, ref buffer, ref index);
            NetworkUtility.AppendNumber(vit, ref buffer, ref index);
            NetworkUtility.AppendNumber(_int, ref buffer, ref index);
            NetworkUtility.AppendNumber(dex, ref buffer, ref index);
            NetworkUtility.AppendNumber(luk, ref buffer, ref index);
            NetworkUtility.AppendNumber(gender, ref buffer, ref index);
            NetworkUtility.AppendNumber(hairstyle, ref buffer, ref index);
            NetworkUtility.AppendFixedString(name, ref buffer, ref index, Constants.MAX_NAME_LEN);
            return buffer;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.SND_CreateCharacter;
        public byte Index;
        public byte agi;
        public byte str;
        public byte vit;
        public byte _int;
        public byte dex;
        public byte luk;
        public byte gender;
        public byte hairstyle;
        public string name;

        //other relevant data
    };

    public class SND_SelectCharacter : SND_Packet
    {
        const short FIXED_SIZE = sizeof(short) + sizeof(byte);

        public byte[] ToBytes()
        {
            byte[] buffer = new byte[FIXED_SIZE];
            int index = 0;
            NetworkUtility.AppendNumber(Id, ref buffer, ref index);
            NetworkUtility.AppendNumber(Index, ref buffer, ref index);
            return buffer;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.SND_SelectCharacter;
        public byte Index;
    };

    public class SND_ReturnToCharacterSelect : SND_Packet
    {
        const short FIXED_SIZE = sizeof(short);

        public byte[] ToBytes()
        {
            byte[] buffer = new byte[FIXED_SIZE];
            int index = 0;
            NetworkUtility.AppendNumber(Id, ref buffer, ref index);
            return buffer;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.SND_ReturnToCharacterSelect;
    };

    public class SND_PlayerMove : SND_Packet
    {
        const short FIXED_SIZE = sizeof(short) * 3;

        public byte[] ToBytes()
        {
            byte[] buffer = new byte[FIXED_SIZE];
            int index = 0;
            NetworkUtility.AppendNumber(Id, ref buffer, ref index);
            NetworkUtility.AppendNumber(destX, ref buffer, ref index);
            NetworkUtility.AppendNumber(destY, ref buffer, ref index);
            return buffer;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.SND_PlayerMove;
        public short destX;
        public short destY;
    }

    public class SND_OtherMove : SND_Packet
    {
        const short FIXED_SIZE = sizeof(byte) + sizeof(short) * 3;

        public byte[] ToBytes()
        {
            byte[] buffer = new byte[FIXED_SIZE];
            int index = 0;
            NetworkUtility.AppendNumber(Id, ref buffer, ref index);
            NetworkUtility.AppendNumber((byte)blockType, ref buffer, ref index);
            NetworkUtility.AppendNumber(destX, ref buffer, ref index);
            NetworkUtility.AppendNumber(destY, ref buffer, ref index);
            return buffer;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.SND_OtherMove;
        public BlockTypes blockType;
        public short destX;
        public short destY;
    }

    public partial class SND_ElevatedAtCommand : SND_Packet
    {
        public const short FIXED_SIZE = short.MaxValue;

        public byte[] ToBytes()
        {
            return payload;
        }

        public short PacketId { get { return Id; } }
        public const short Id = (short)PacketIds.SND_ElevatedAtCommand;
        private byte[] payload;
    }

    public class SND_LevelUpSingleSkill : SND_Packet
    {
        const short FIXED_SIZE = sizeof(short) + sizeof(byte) * 2;

        public byte[] ToBytes()
        {
            byte[] buffer = new byte[FIXED_SIZE];
            int index = 0;
            NetworkUtility.AppendNumber(Id, ref buffer, ref index);
            NetworkUtility.AppendNumber(SkillIndex, ref buffer, ref index);
            NetworkUtility.AppendNumber(SkillLvl, ref buffer, ref index);
            return buffer;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.SND_LevelUpSingleSkill;
        public byte SkillIndex;
        public byte SkillLvl;
    }

    public class SND_LevelUpMultipleSkills : SND_Packet
    {
        public const short FIXED_SIZE = short.MaxValue;

        public struct SkillInfo
        {
            public byte SkillIndex;
            public byte SkillLvl;
        }

        public byte[] ToBytes()
        {
            byte[] buffer = new byte[sizeof(short) + sizeof(short) + sizeof(byte) * 2 * Skills.Length];
            int index = 0;
            NetworkUtility.AppendNumber(Id, ref buffer, ref index);
            NetworkUtility.AppendNumber((short)buffer.Length, ref buffer, ref index);

            foreach (var skill in Skills)
            {
                NetworkUtility.AppendNumber(skill.SkillIndex, ref buffer, ref index);
                NetworkUtility.AppendNumber(skill.SkillLvl, ref buffer, ref index);
            }
            return buffer;
        }

        public short PacketId { get { return Id; } }
        private const short Id = (short)PacketIds.SND_LevelUpMultipleSkills;
        private short totalSize;
        public SkillInfo[] Skills;
    }

    public class SND_LevelUpAllTreeSkills : SND_Packet
    {
        public const short FIXED_SIZE = short.MaxValue;

        public byte[] ToBytes()
        {
            byte[] buffer = new byte[sizeof(short) + sizeof(short) + sizeof(byte) * SkillIncrements.Length];
            int index = 0;
            NetworkUtility.AppendNumber(Id, ref buffer, ref index);
            NetworkUtility.AppendNumber((short)buffer.Length, ref buffer, ref index);

            foreach (var lvl in SkillIncrements)
            {
                NetworkUtility.AppendNumber(lvl, ref buffer, ref index);
            }
            return buffer;
        }

        public short PacketId { get { return Id; } }
        private const short Id = (short)PacketIds.SND_LevelUpAllTreeSkills;
        private short totalSize;
        public byte[] SkillIncrements;
    }

    public class SND_Chat : SND_Packet
    {
        const short FIXED_SIZE = sizeof(short) * 2;

        public byte[] ToBytes()
        {
            PacketSize = (short)(FIXED_SIZE); //+ Text.Length);
            byte[] buffer = new byte[PacketSize];
            int index = 0;
            NetworkUtility.AppendNumber(Id, ref buffer, ref index);
            NetworkUtility.AppendNumber(PacketSize, ref buffer, ref index);
            //          NetworkUtility.AppendString(Text, ref buffer, ref index);
            return buffer;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.SND_Chat;
        public short PacketSize;
        //      public string Text;
    }

    //************** battle packets

    public class SND_CastAreaOfEffectSkill : SND_Packet
    {
        const short FIXED_SIZE = sizeof(short) * 3 + sizeof(byte) * 2;

        public byte[] ToBytes()
        {
            byte[] buffer = new byte[FIXED_SIZE];
            int index = 0;
            NetworkUtility.AppendNumber(Id, ref buffer, ref index);
            NetworkUtility.AppendNumber(skillIndex, ref buffer, ref index);
            NetworkUtility.AppendNumber(skillLevel, ref buffer, ref index);
            NetworkUtility.AppendNumber(destinationX, ref buffer, ref index);
            NetworkUtility.AppendNumber(destinationY, ref buffer, ref index);
            return buffer;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.SND_CastAreaOfEffectSkill;
        public byte skillIndex;
        public byte skillLevel;
        public short destinationX;
        public short destinationY;
    }

    public class SND_CastSingleTargetSkill : SND_Packet
    {
        const short FIXED_SIZE = sizeof(short) * 2 + sizeof(byte) * 2;

        public byte[] ToBytes()
        {
            byte[] buffer = new byte[FIXED_SIZE];
            int index = 0;
            NetworkUtility.AppendNumber(Id, ref buffer, ref index);
            NetworkUtility.AppendNumber(skillIndex, ref buffer, ref index);
            NetworkUtility.AppendNumber(skillLevel, ref buffer, ref index);
            NetworkUtility.AppendNumber(instanceId, ref buffer, ref index);
            return buffer;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.SND_CastSingleTargetSkill;
        public byte skillIndex;
        public byte skillLevel;
        public short instanceId;
    }

    public class SND_CastSelfTargetSkill : SND_Packet
    {
        const short FIXED_SIZE = sizeof(short) + sizeof(byte) * 2;

        public byte[] ToBytes()
        {
            byte[] buffer = new byte[FIXED_SIZE];
            int index = 0;
            NetworkUtility.AppendNumber(Id, ref buffer, ref index);
            NetworkUtility.AppendNumber(skillIndex, ref buffer, ref index);
            NetworkUtility.AppendNumber(skillLevel, ref buffer, ref index);
            return buffer;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.SND_CastSelfTargetSkill;
        public byte skillIndex;
        public byte skillLevel;
    }

    //*************************** RECEIVE PACKETS  ****************************************

    // **************************** Lobby packets
    #region Lobby packets

    public class RCV_ReplyInvalidLogin : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short);

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_ReplyInvalidLogin;
    };

    public class RCV_ReplyRegisterAccount : RCV_Packet
    {
        public enum ReplyStatus : byte
        {
            Ok,
            Error_DuplicateName,
            Error_Generic
        };

        public const short FIXED_SIZE = sizeof(short) + sizeof(ReplyStatus);

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out short status, ref buffer, ref index);
            replyStatus = (ReplyStatus)status;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_ReplyRegisterAccount;
        public ReplyStatus replyStatus;

    }

    public class RCV_EnterCharacterSelect : RCV_Packet
    {
        //Missing the pallete ids. Add variables to CharInfoFromBytes too
        public struct CharInfo
        {
            public byte charSlot;
            public byte gender;
            public string name;
            public Jobs job;
            public short hairstyle;
            public byte lvl;
            public int hp;
            public int sp;
            public short str;
            public short agi;
            public short vit;
            public short int_;
            public short dex;
            public short luk;
            public int exp;
            public short upperHeadgear;
            public short midHeadgear;
            public short lowerHeadgear;
            public short mapId;
        };

        public const short FIXED_SIZE = short.MaxValue;

        void CharInfoFromBytes(out CharInfo info, ref byte[] buffer, ref int index)
        {
            info = new CharInfo();
            byte _job;
            NetworkUtility.ExtractNumber(out info.charSlot, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out info.gender, ref buffer, ref index);
            NetworkUtility.ExtractFixedString(out info.name, ref buffer, ref index, Constants.MAX_NAME_LEN);
            NetworkUtility.ExtractNumber(out _job, ref buffer, ref index);
            info.job = (Jobs)_job;
            NetworkUtility.ExtractNumber(out info.hairstyle, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out info.lvl, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out info.hp, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out info.sp, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out info.str, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out info.agi, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out info.vit, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out info.int_, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out info.dex, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out info.luk, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out info.exp, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out info.upperHeadgear, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out info.midHeadgear, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out info.lowerHeadgear, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out info.mapId, ref buffer, ref index);
        }

        public short PacketId { get { return Id; } }
        public void FromBytes(ref byte[] buffer)
        {
            short id;
            int index = 0;
            NetworkUtility.ExtractNumber(out id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out PacketSize, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out CharInfoLength, ref buffer, ref index);
            CharInfos = new CharInfo[CharInfoLength];
            for (int i = 0; i < CharInfoLength; i++)
                CharInfoFromBytes(out CharInfos[i], ref buffer, ref index);
        }

        public short Id = (short)PacketIds.RCV_EnterCharacterSelect;
        public short PacketSize;
        public byte CharInfoLength;
        public CharInfo[] CharInfos;
    };

    public class RCV_EnterLobby : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short);

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_EnterLobby;
    };

    public class RCV_ExitLobby : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short);

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_ExitLobby;
    };

    public class RCV_ReplyCreateCharacter : RCV_Packet
    {
        public enum ReplyStatus : byte
        {
            Ok,
            Error_DuplicateName,
            Error_Generic
        };
        public const short FIXED_SIZE = sizeof(short) + sizeof(ReplyStatus);

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out short status, ref buffer, ref index);
            replyStatus = (ReplyStatus)status;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_ReplyCreateCharacter;
        public ReplyStatus replyStatus;
    };

    public class RCV_EnterMap : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 4 + sizeof(byte);

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out MapId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out PosX, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out PosY, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte bodyDir, ref buffer, ref index);
            direction = new Character.Direction(bodyDir, bodyDir); //body and head is the same on enter map
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_EnterMap;
        public short MapId;
        public short PosX;
        public short PosY;
        public Character.Direction direction;
    }

    public class RCV_PlayerSkillTreeAddSkill : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 2 + sizeof(byte) * 2;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out short skillId, ref buffer, ref index);
            SkillId = (Databases.SkillIds)skillId;
            NetworkUtility.ExtractNumber(out Level, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out Index, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_PlayerSkillTreeAddSkill;
        public Databases.SkillIds SkillId;
        public byte Level;
        public byte Index;
    }

    public class RCV_PlayerSkillTreeRemoveSkill : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 2 + sizeof(byte) * 2;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out short skillId, ref buffer, ref index);
            SkillId = (Databases.SkillIds)skillId;
            NetworkUtility.ExtractNumber(out Level, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out Index, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_PlayerSkillTreeRemoveSkill;
        public Databases.SkillIds SkillId;
        public byte Level;
        public byte Index;
    }

    public class RCV_PlayerSkillTreeReload : RCV_Packet
    {
        public const short FIXED_SIZE = short.MaxValue;

        public struct SkillInfo
        {
            public Databases.SkillIds skillId;
            public int level;
        }

        private void SkillInfoFromBytes(out SkillInfo info, ref byte[] buffer, ref int index)
        {
            info = new SkillInfo();
            NetworkUtility.ExtractNumber(out short skillId, ref buffer, ref index);
            info.skillId = (Databases.SkillIds)skillId;
            NetworkUtility.ExtractNumber(out byte level, ref buffer, ref index);
            info.level = level;
        }

        public short PacketId { get { return Id; } }
        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out PacketSize, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte job, ref buffer, ref index);
            Job = (Jobs)job;
            NetworkUtility.ExtractNumber(out SkillInfoLength, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out PermanentSkillCount, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out UnindexedSkillCount, ref buffer, ref index);

            SkillInfos = new SkillInfo[SkillInfoLength];
            for (int i = 0; i < SkillInfoLength; i++)
                SkillInfoFromBytes(out SkillInfos[i], ref buffer, ref index);
        }

        public short Id = (short)PacketIds.RCV_PlayerSkillTreeReload;
        public short PacketSize;
        public Jobs Job;
        public byte SkillInfoLength;
        public byte PermanentSkillCount;
        public byte UnindexedSkillCount;
        public SkillInfo[] SkillInfos;
    }

    public class RCV_PlayerJobOrLevelChanged : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 2 + sizeof(byte) * 3;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out playerId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte _job, ref buffer, ref index);
            job = (Jobs)_job;
            NetworkUtility.ExtractNumber(out baseLevel, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out jobLevel, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_PlayerJobOrLevelChanged;

        public short playerId;
        public Jobs job;
        public byte baseLevel;
        public byte jobLevel;
    }

    public class RCV_PlayerSkillPointsChanged : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) + sizeof(byte);

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out SkillPoints, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_PlayerSkillPointsChanged;
        public byte SkillPoints;
    }

    public class RCV_PlayerSkillTreeLevelUpReply : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) + sizeof(byte);

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte _success, ref buffer, ref index);
            success = _success == 0;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_PlayerSkillTreeLevelUpReply;
        public bool success;
    }

    public class RCV_PlayerBuffApply : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 2 + sizeof(uint);

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out short _buffId, ref buffer, ref index);
            buffId = (Databases.BuffIDs)_buffId;
            NetworkUtility.ExtractNumber(out buffDuration, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_PlayerBuffApply;
        public Databases.BuffIDs buffId;
        public uint buffDuration;
    }

    public class RCV_PlayerBuffRemove : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 2;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out short _buffId, ref buffer, ref index);
            buffId = (Databases.BuffIDs)_buffId;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_PlayerBuffRemove;
        public Databases.BuffIDs buffId;
    }
    #endregion

    // ************************** Move packets 
    #region Move packets 

    public class RCV_PlayerLocalMove : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 6;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out posX, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out posY, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out destX, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out destY, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out startDelay, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_PlayerLocalMove;
        public short posX;
        public short posY;
        public short destX;
        public short destY;
        public short startDelay;
    }

    public class RCV_PlayerOtherMove : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 7;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out playerId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out posX, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out posY, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out destX, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out destY, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out startDelay, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_PlayerOtherMove;
        public short playerId;
        public short posX;
        public short posY;
        public short destX;
        public short destY;
        public short startDelay;
    }

    public class RCV_MonsterMove : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 7;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out monsterId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out posX, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out posY, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out destX, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out destY, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out startDelay, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_MonsterMove;
        public short monsterId;
        public short posX;
        public short posY;
        public short destX;
        public short destY;
        public short startDelay;
    }

    public class RCV_OtherMove : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(byte) + sizeof(short) * 7;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte auxBlocktype, ref buffer, ref index);
            blockType = (BlockTypes)auxBlocktype;
            NetworkUtility.ExtractNumber(out blockId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out posX, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out posY, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out destX, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out destY, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out startDelay, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_OtherMove;
        public BlockTypes blockType;
        public short blockId;
        public short posX;
        public short posY;
        public short destX;
        public short destY;
        public short startDelay;
    }

    public class RCV_PlayerLocalStopMove : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 3;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out posX, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out posY, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_PlayerLocalStopMove;
        public short posX;
        public short posY;
    }

    public class RCV_PlayerOtherStopMove : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 4;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out playerId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out posX, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out posY, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_PlayerOtherStopMove;
        public short playerId;
        public short posX;
        public short posY;
    }

    public class RCV_MonsterStopMove : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 4;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out monsterId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out posX, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out posY, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_MonsterStopMove;
        public short monsterId;
        public short posX;
        public short posY;
    }

    public class RCV_OtherStopMove : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(byte) + sizeof(short) * 4;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte auxBlocktype, ref buffer, ref index);
            blockType = (BlockTypes)auxBlocktype;
            NetworkUtility.ExtractNumber(out blockId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out posX, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out posY, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_OtherStopMove;
        public BlockTypes blockType;
        public short blockId;
        public short posX;
        public short posY;
    }

    public class RCV_PlayerWarpToCell : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(byte) * 2 + sizeof(short) * 3;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out posX, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out posY, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte bodyDir, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte headDir, ref buffer, ref index);
            direction = new Character.Direction(headDir, bodyDir);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_PlayerWarpToCell;
        public short posX;
        public short posY;
        public Character.Direction direction;
    }
    #endregion

    //****************************** Update remote info packets
    #region Update remote info packets
    public class RCV_PlayerEnterRange : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 8 + sizeof(byte) * 6;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out playerId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out posX, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out posY, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte bodyDir, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte headDir, ref buffer, ref index);
            direction = new Character.Direction(headDir, bodyDir);
            NetworkUtility.ExtractNumber(out byte _gender, ref buffer, ref index);
            gender = (Gender)_gender;
            NetworkUtility.ExtractNumber(out byte _job, ref buffer, ref index);
            job = (Jobs)_job;
            NetworkUtility.ExtractNumber(out hairstyle, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out topHeadgear, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out midHeadgear, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out lowHeadgear, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out movementSpeed, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte _enterType, ref buffer, ref index);
            enterType = (EnterRangeType)_enterType;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_PlayerEnterRange;
        public short playerId;
        public short posX;
        public short posY;
        public Character.Direction direction;
        public Gender gender;
        public Jobs job;
        public byte hairstyle;
        public short topHeadgear;
        public short midHeadgear;
        public short lowHeadgear;
        public short movementSpeed;
        public EnterRangeType enterType;
    }

    public class RCV_MonsterEnterRange : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 6 + sizeof(byte) * 2;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out instanceId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out posX, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out posY, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out direction, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out dbId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out movementSpeed, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte _enterType, ref buffer, ref index);
            enterType = (EnterRangeType)_enterType;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_MonsterEnterRange;
        public short instanceId;
        public short posX;
        public short posY;
        public byte direction;
        public short dbId;
        public short movementSpeed;
        public EnterRangeType enterType;
    }

    public class RCV_OtherEnterRange : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(byte) * 2 + sizeof(short) * 5;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte auxBlocktype, ref buffer, ref index);
            blockType = (BlockTypes)auxBlocktype;
            NetworkUtility.ExtractNumber(out blockId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out dbId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out posX, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out posY, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte _enterType, ref buffer, ref index);
            enterType = (EnterRangeType)_enterType;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_OtherEnterRange;
        public BlockTypes blockType;
        public ushort blockId;
        public short dbId;
        public short posX;
        public short posY;
        public EnterRangeType enterType;
    }

    public class RCV_PlayerLeaveRange : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 2 + sizeof(byte);

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out playerId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte _leaveType, ref buffer, ref index);
            leaveType = (LeaveRangeType)_leaveType;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_PlayerLeaveRange;
        public short playerId;
        public LeaveRangeType leaveType;
    }

    public class RCV_MonsterLeaveRange : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 2 + sizeof(byte);

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out instanceId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte _leaveType, ref buffer, ref index);
            leaveType = (LeaveRangeType)_leaveType;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_MonsterLeaveRange;
        public short instanceId;
        public LeaveRangeType leaveType;
    }

    public class RCV_OtherLeaveRange : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(byte) * 2 + sizeof(short) * 3;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte auxBlocktype, ref buffer, ref index);
            blockType = (BlockTypes)auxBlocktype;
            NetworkUtility.ExtractNumber(out blockId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out dbId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte _leaveType, ref buffer, ref index);
            leaveType = (LeaveRangeType)_leaveType;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_OtherLeaveRange;
        public BlockTypes blockType;
        public ushort blockId;
        public short dbId;
        public LeaveRangeType leaveType;
    }

    public class RCV_BlockDied : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) + sizeof(ushort);

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out blockId, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_BlockDied;
        public ushort blockId;
    }

    #endregion

    //******************************* Skill packets
    #region Skill packets
    public class RCV_PlayerLocalBeginAoECast : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 5;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out skillId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out castTime, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out positionX, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out positionY, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_PlayerLocalBeginAoECast;
        public short skillId;
        public short castTime;
        public short positionX;
        public short positionY;
    }

    public class RCV_PlayerOtherBeginAoECast : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 6;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out playerId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out skillId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out castTime, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out positionX, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out positionY, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_PlayerOtherBeginAoECast;
        public short playerId;
        public short skillId;
        public short castTime;
        public short positionX;
        public short positionY;
    }

    public class RCV_MonsterBeginAoECast : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 6;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out monsterId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out skillId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out castTime, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out positionX, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out positionY, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_MonsterBeginAoECast;
        public short monsterId;
        public short skillId;
        public short castTime;
        public short positionX;
        public short positionY;
    }

    public class RCV_OtherBeginAoECast : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(byte) + sizeof(short) * 6;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte auxBlocktype, ref buffer, ref index);
            blockType = (BlockTypes)auxBlocktype;
            NetworkUtility.ExtractNumber(out blockId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out skillId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out castTime, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out positionX, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out positionY, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_OtherBeginAoECast;
        public BlockTypes blockType;
        public short blockId;
        public short skillId;
        public short castTime;
        public short positionX;
        public short positionY;
    }

    public class RCV_PlayerLocalBeginTargetCast : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 4 + sizeof(byte);

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out skillId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out castTime, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out targetId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte blockType, ref buffer, ref index);
            this.blockType = (BlockTypes)blockType;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_PlayerLocalBeginTargetCast;
        public short skillId;
        public short castTime;
        public short targetId;
        public BlockTypes blockType;
    }

    public class RCV_PlayerOtherBeginTargetCast : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 5 + sizeof(byte);

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out playerId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out skillId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out castTime, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out targetId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte blockType, ref buffer, ref index);
            this.blockType = (BlockTypes)blockType;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_PlayerOtherBeginTargetCast;
        public short playerId;
        public short skillId;
        public short castTime;
        public short targetId;
        public BlockTypes blockType;
    }

    public class RCV_MonsterBeginTargetCast : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 5 + sizeof(byte);

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out monsterId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out skillId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out castTime, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out targetId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte blockType, ref buffer, ref index);
            this.blockType = (BlockTypes)blockType;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_MonsterBeginTargetCast;
        public short monsterId;
        public short skillId;
        public short castTime;
        public short targetId;
        public BlockTypes blockType;
    }

    public class RCV_OtherBeginTargetCast : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 5 + sizeof(byte) * 2;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte auxBlocktype, ref buffer, ref index);
            blockType = (BlockTypes)auxBlocktype;
            NetworkUtility.ExtractNumber(out blockId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out skillId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out castTime, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out targetId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte auxTargetBlockType, ref buffer, ref index);
            this.targetBlockType = (BlockTypes)auxTargetBlockType;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_OtherBeginTargetCast;
        public BlockTypes blockType;
        public short blockId;
        public short skillId;
        public short castTime;
        public short targetId;
        public BlockTypes targetBlockType;
    }

    public class RCV_PlayerLocalBeginSelfCast : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 3;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out skillId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out castTime, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_PlayerLocalBeginSelfCast;
        public short skillId;
        public short castTime;
    }

    public class RCV_PlayerOtherBeginSelfCast : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 4;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out playerId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out skillId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out castTime, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_PlayerLocalBeginSelfCast;
        public short playerId;
        public short skillId;
        public short castTime;
    }

    public class RCV_MonsterBeginSelfCast : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 4;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out monsterId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out skillId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out castTime, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_MonsterBeginSelfCast;
        public short monsterId;
        public short skillId;
        public short castTime;
    }

    public class RCV_OtherBeginSelfCast : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(byte) + sizeof(short) * 4;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte auxBlocktype, ref buffer, ref index);
            blockType = (BlockTypes)auxBlocktype;
            NetworkUtility.ExtractNumber(out blockId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out skillId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out castTime, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_OtherBeginSelfCast;
        public BlockTypes blockType;
        public short blockId;
        public short skillId;
        public short castTime;
    }

    public class RCV_PlayerLocalFinishedAoECast : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 4;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out skillId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out positionX, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out positionY, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_PlayerLocalFinishedAoECast;
        public short skillId;
        public short positionX;
        public short positionY;
    }

    public class RCV_PlayerOtherFinishedAoECast : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 5;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out playerId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out skillId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out positionX, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out positionY, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_PlayerOtherFinishedAoECast;
        public short playerId;
        public short skillId;
        public short positionX;
        public short positionY;
    }

    public class RCV_MonsterFinishedAoECast : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 5;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out monsterId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out skillId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out positionX, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out positionY, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_MonsterFinishedAoECast;
        public short monsterId;
        public short skillId;
        public short positionX;
        public short positionY;
    }

    public class RCV_OtherFinishedAoECast : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(byte) + sizeof(short) * 5;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte auxBlocktype, ref buffer, ref index);
            blockType = (BlockTypes)auxBlocktype;
            NetworkUtility.ExtractNumber(out blockId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out skillId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out positionX, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out positionY, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_OtherFinishedAoECast;
        public BlockTypes blockType;
        public short blockId;
        public short skillId;
        public short positionX;
        public short positionY;
    }

    public class RCV_PlayerLocalFinishedTargetCast : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 3 + sizeof(byte);

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out skillId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out targetId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte blockType, ref buffer, ref index);
            this.blockType = (BlockTypes)blockType;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_PlayerLocalFinishedTargetCast;
        public short skillId;
        public short targetId;
        public BlockTypes blockType;
    }

    public class RCV_PlayerOtherFinishedTargetCast : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 4 + sizeof(byte);

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out playerId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out skillId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out targetId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte blockType, ref buffer, ref index);
            this.blockType = (BlockTypes)blockType;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_PlayerOtherFinishedTargetCast;
        public short playerId;
        public short skillId;
        public short targetId;
        public BlockTypes blockType;
    }

    public class RCV_MonsterFinishedTargetCast : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 4 + sizeof(byte);

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out monsterId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out skillId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out targetId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte blockType, ref buffer, ref index);
            this.blockType = (BlockTypes)blockType;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_MonsterFinishedTargetCast;
        public short monsterId;
        public short skillId;
        public short targetId;
        public BlockTypes blockType;
    }

    public class RCV_OtherFinishedTargetCast : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 4 + sizeof(byte) * 2;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte auxBlocktype, ref buffer, ref index);
            blockType = (BlockTypes)auxBlocktype;
            NetworkUtility.ExtractNumber(out blockId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out skillId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out targetId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte auxTargetBlockType, ref buffer, ref index);
            this.targetBlockType = (BlockTypes)auxTargetBlockType;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_OtherFinishedTargetCast;
        public BlockTypes blockType;
        public short blockId;
        public short skillId;
        public short targetId;
        public BlockTypes targetBlockType;
    }

    public class RCV_PlayerLocalFinishedSelfCast : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 2;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out skillId, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_PlayerLocalFinishedSelfCast;
        public short skillId;
    }

    public class RCV_PlayerOtherFinishedSelfCast : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 3;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out playerId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out skillId, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_PlayerOtherFinishedSelfCast;
        public short playerId;
        public short skillId;
    }

    public class RCV_MonsterFinishedSelfCast : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 3;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out monsterId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out skillId, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_MonsterFinishedSelfCast;
        public short monsterId;
        public short skillId;
    }

    public class RCV_OtherFinishedSelfCast : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(byte) + sizeof(short) * 3;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte auxBlocktype, ref buffer, ref index);
            blockType = (BlockTypes)auxBlocktype;
            NetworkUtility.ExtractNumber(out blockId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out skillId, ref buffer, ref index);
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_OtherFinishedSelfCast;
        public BlockTypes blockType;
        public short blockId;
        public short skillId;
    }


    //Packet to be used to notify game manager that connection dropped
    public class Internal_ConnectionClosed : RCV_Packet
    {
        public void FromBytes(ref byte[] buffer)
        {

        }
        public short PacketId { get { return Id; } }
        public short Id = (short)PacketIds.ConnectionClosed;
    }
    #endregion

    //****************************** Battle packets
    #region Battle Packets

    public class RCV_PlayerLocalReceiveDamage : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) + sizeof(int) + sizeof(byte);

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out damage, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte _damageType, ref buffer, ref index);
            damageType = (DamageType)_damageType;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_PlayerLocalReceiveDamage;
        public int damage;
        public DamageType damageType;
    }

    public class RCV_PlayerOtherReceiveDamage : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 2 + sizeof(int) + sizeof(byte);

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out playerId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out damage, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte _damageType, ref buffer, ref index);
            damageType = (DamageType)_damageType;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_PlayerOtherReceiveDamage;
        public short playerId;
        public int damage;
        public DamageType damageType;
    }

    public class RCV_MonsterReceiveDamage : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 2 + sizeof(int) + sizeof(byte);

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out monsterId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out damage, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte _damageType, ref buffer, ref index);
            damageType = (DamageType)_damageType;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_MonsterReceiveDamage;
        public short monsterId;
        public int damage;
        public DamageType damageType;
    }

    public class RCV_OtherReceiveDamage : RCV_Packet
    {
        public const short FIXED_SIZE = sizeof(short) * 2 + sizeof(int) + sizeof(byte) * 2;

        public void FromBytes(ref byte[] buffer)
        {
            int index = 0;
            NetworkUtility.ExtractNumber(out short id, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte auxBlocktype, ref buffer, ref index);
            blockType = (BlockTypes)auxBlocktype;
            NetworkUtility.ExtractNumber(out blockId, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out damage, ref buffer, ref index);
            NetworkUtility.ExtractNumber(out byte _damageType, ref buffer, ref index);
            damageType = (DamageType)_damageType;
        }
        public short PacketId { get { return Id; } }

        public short Id = (short)PacketIds.RCV_OtherReceiveDamage;
        public BlockTypes blockType;
        public short blockId;
        public int damage;
        public DamageType damageType;
    }

    #endregion Battle Packets
}
