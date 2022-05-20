namespace RO.Network
{
#if STANDALONE_CLIENT
    using System;
    using System.Reflection;

    public static class PacketMirrorer
    {
        public static GameController GameController;
        public static Action<SND_Packet>[] PacketMirrors = new Action<SND_Packet>[(int)PacketIds.SND_End];
        
        private static LocalPlayer.LocalCharacter LocalCharacter
        {
            get
            {
                Type localCharType = typeof(GameController);
                FieldInfo fieldInfo = localCharType.GetField("_localPlayer", BindingFlags.NonPublic | BindingFlags.Instance);

                return (LocalPlayer.LocalCharacter)fieldInfo.GetValue(GameController);
            }
        }

        static PacketMirrorer()
        {
            PacketMirrors[(int)PacketIds.SND_PlayerMove] = (packet) => MirrorSendPlayerMove((SND_PlayerMove)packet);
            PacketMirrors[(int)PacketIds.SND_Login] = (packet) => MirrorLoginReply((SND_Login)packet);
            PacketMirrors[(int)PacketIds.SND_SelectCharacter] = (packet) => MirrorSelectCharacter((SND_SelectCharacter)packet);
            PacketMirrors[(int)PacketIds.SND_CastAreaOfEffectSkill] = (packet) => MirrorBeginCastAoE((SND_CastAreaOfEffectSkill)packet);
            PacketMirrors[(int)PacketIds.SND_CastSingleTargetSkill] = (packet) => MirrorBeginCastTarget((SND_CastSingleTargetSkill)packet);
        }

        static void MirrorSendPlayerMove(SND_PlayerMove playerMove)
        {        
            RCV_PlayerLocalMove packet = new RCV_PlayerLocalMove
            {
                posX = (short)LocalCharacter.position.x,
                posY = (short)LocalCharacter.position.y,
                destX = playerMove.destX,
                destY = playerMove.destY,
                startDelay = 0
            };
            NetworkController.QueuePacketsIn.Enqueue(packet);
        }

        static void MirrorLoginReply(SND_Login login)
        {
            RCV_EnterCharacterSelect packet = new RCV_EnterCharacterSelect();
            packet.CharInfoLength = 1;
            packet.CharInfos = new RCV_EnterCharacterSelect.CharInfo[1];
            packet.CharInfos[0].charSlot = 0;
            packet.CharInfos[0].gender = 1;
            packet.CharInfos[0].hairstyle = 4;
            packet.CharInfos[0].job = Common.Jobs.Wizard;
            packet.CharInfos[0].mapId = 0;
            packet.CharInfos[0].name = "Standalone";

            packet.PacketSize = 73;
            NetworkController.QueuePacketsIn.Enqueue(packet);
        }

        static void MirrorSelectCharacter(SND_SelectCharacter sel)
        {
            RCV_ExitLobby ack = new RCV_ExitLobby();
            NetworkController.QueuePacketsIn.Enqueue(ack);

            NetworkController.QueuePacketsIn.Enqueue(FillWizardSkillTree());

            RCV_EnterMap packet = new RCV_EnterMap();
            packet.MapId = 0;
            packet.PosX = 100;
            packet.PosY = 100;
            packet.direction = new MapObjects.Character.Direction(0, 0);
            NetworkController.QueuePacketsIn.Enqueue(packet);
        }

        static RCV_PlayerSkillTreeReload FillWizardSkillTree()
        { 
            Func<Databases.SkillIds, int, RCV_PlayerSkillTreeReload.SkillInfo> skill = 
            (Databases.SkillIds id, int lvl) =>
            {
                RCV_PlayerSkillTreeReload.SkillInfo info;
                info.skillId = id;
                info.level = lvl;

                return info;
            };

            RCV_PlayerSkillTreeReload.SkillInfo[] skillInfos = new RCV_PlayerSkillTreeReload.SkillInfo[]
            {
                skill(Databases.SkillIds.BasicSkill, 5),
                skill(Databases.SkillIds.FirstAid, 1),
                skill(Databases.SkillIds.TrickDead, 0),
                skill(Databases.SkillIds.StoneCurse, 0),
                skill(Databases.SkillIds.ColdBolt, 0),
                skill(Databases.SkillIds.LightningBolt, 0),
                skill(Databases.SkillIds.NapalmBeat, 0),
                skill(Databases.SkillIds.FireBolt, 0),
                skill(Databases.SkillIds.Sight, 0),
                skill(Databases.SkillIds.SpRecovery, 0),
                skill(Databases.SkillIds.FrostDiver, 0),
                skill(Databases.SkillIds.ThunderStorm, 10),
                skill(Databases.SkillIds.SoulStrike, 0),
                skill(Databases.SkillIds.FireBall, 0),
                skill(Databases.SkillIds.EnergyCoat, 1),
                skill(Databases.SkillIds.SafetyWall, 0),
                skill(Databases.SkillIds.FireWall, 0),
                skill(Databases.SkillIds.Sense, 1),
                skill(Databases.SkillIds.IceWall, 0),
                skill(Databases.SkillIds.JupitelThunder, 1),
                skill(Databases.SkillIds.EarthSpike, 0),
                skill(Databases.SkillIds.SightTrasher, 0),
                skill(Databases.SkillIds.FirePillar, 0),
                skill(Databases.SkillIds.SightBlaster, 0),
                skill(Databases.SkillIds.FrostNova, 1),
                skill(Databases.SkillIds.LordOfVermilion, 0),
                skill(Databases.SkillIds.HeavensDrive, 1),
                skill(Databases.SkillIds.MeteorStorm, 0),
                skill(Databases.SkillIds.WaterBall, 0),
                skill(Databases.SkillIds.Quagmire, 0),
                skill(Databases.SkillIds.StormGust, 1),
                skill(Databases.SkillIds.Ganbantein, 0),
                skill(Databases.SkillIds.MagicCrasher, 0),
                skill(Databases.SkillIds.SoulDrain, 0),
                skill(Databases.SkillIds.NapalmVulcan, 0),
                skill(Databases.SkillIds.MysticAmplification, 0),
                skill(Databases.SkillIds.GravitationalField, 0),
            };

            RCV_PlayerSkillTreeReload packet = new RCV_PlayerSkillTreeReload();

            packet.PacketSize = (short)(skillInfos.Length * (sizeof(short)+ sizeof(byte)));
            packet.PacketSize += sizeof(short) * 2 + sizeof(byte) * 3;
            packet.PermanentSkillCount = (byte)skillInfos.Length;
            packet.UnindexedSkillCount = 0;
            packet.SkillInfoLength = (byte)skillInfos.Length;
            packet.SkillInfos = skillInfos;
            packet.Job = Common.Jobs.Wizard;

            return packet;
        }

        static void MirrorBeginCastAoE(SND_CastAreaOfEffectSkill packet)
        {
            RCV_PlayerLocalBeginAoECast cast = new RCV_PlayerLocalBeginAoECast();
            cast.positionX = packet.destinationX;
            cast.positionY = packet.destinationY;
            cast.skillId = (short)Databases.SkillIds.StormGust; // for now always say it's a storm gust
            cast.castTime = 3000; // always say 3s cast time

            NetworkController.QueuePacketsIn.Enqueue(cast);

            TimerController.PushNonPersistent(cast.castTime / 1000f,
                () =>
                {
                    RCV_PlayerLocalFinishedAoECast aoeCast = new RCV_PlayerLocalFinishedAoECast();
                    aoeCast.positionX = cast.positionX;
                    aoeCast.positionY = cast.positionY;
                    aoeCast.skillId = cast.skillId;

                    NetworkController.QueuePacketsIn.Enqueue(aoeCast);
                });
        }

        static void MirrorBeginCastTarget(SND_CastSingleTargetSkill packet)
        {
            RCV_PlayerLocalBeginTargetCast cast = new RCV_PlayerLocalBeginTargetCast();

            cast.blockType = MapObjects.BlockTypes.Monster;
            cast.castTime = 2000;
            cast.skillId = (short)Databases.SkillIds.JupitelThunder; //For now
            cast.targetId = packet.instanceId;

            NetworkController.QueuePacketsIn.Enqueue(cast);

            TimerController.PushNonPersistent(cast.castTime / 1000f,
            () =>
            {
                RCV_PlayerLocalFinishedTargetCast targetCast = new RCV_PlayerLocalFinishedTargetCast();
                targetCast.targetId = cast.targetId;
                targetCast.blockType = cast.blockType;
                targetCast.skillId = cast.skillId;

                NetworkController.QueuePacketsIn.Enqueue(targetCast);
            });
        }
    }
#endif
}
