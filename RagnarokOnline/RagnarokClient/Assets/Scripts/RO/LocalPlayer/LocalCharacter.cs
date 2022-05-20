using RO.Common;
using RO.IO;
using RO.MapObjects;
using RO.Network;
using UnityEngine;

namespace RO.LocalPlayer
{
    public sealed class LocalCharacter : Character
    {
        private UI.UIController _UIController = null;
        private readonly SkillTree SkillTree;

        public LocalCharacter(UI.UIController uiController, InputHandler inputHandler)
            : base(Constants.LOCAL_SESSION_ID)
        {
            RegisterPacketHandlers();
            isFriendly = true;

            SkillTree = new SkillTree(inputHandler, this);
            _UIController = uiController;
        }

        public void load(UI.UIController.CharacterInfo uiCharacterStatus)
        {
            _charInfo.name = uiCharacterStatus.name;
            _charInfo.hairstyle = uiCharacterStatus.hairstyle;
            _charInfo.job = uiCharacterStatus.job;
            _charInfo.gender = uiCharacterStatus.gender;
            _charInfo.isMounted = uiCharacterStatus.isMounted;

            _charInfo.zeny = uiCharacterStatus.zeny;
            _charInfo.weight = uiCharacterStatus.weight;
            _charInfo.maxWeight = uiCharacterStatus.maxWeight;
            _charInfo.currentBaseExp = uiCharacterStatus.currentBaseExp;
            _charInfo.currentJobExp = uiCharacterStatus.currentJobExp;

            status.baseLvl = uiCharacterStatus.baseLevel;
            status.jobLvl = uiCharacterStatus.jobLevel;

            status.currentHp = uiCharacterStatus.currentHp;
            status.maxHp = uiCharacterStatus.maxHp;
            status.currentSp = uiCharacterStatus.currentSp;
            status.maxSp = uiCharacterStatus.maxSp;

            status.str = uiCharacterStatus.str;
            status.agi = uiCharacterStatus.agi;
            status.vit = uiCharacterStatus.vit;
            status.int_ = uiCharacterStatus.int_;
            status.dex = uiCharacterStatus.dex;
            status.luk = uiCharacterStatus.luk;

            PlayerAnimator.AnimateCharacter(this);
        }

        //API for setting fields. Use these to set stats for local player since UI needs to be notified
        public string Name
        {
            get { return _charInfo.name; }

            set
            {
                _charInfo.name = value;
                _UIController.UpdatePlayerName(value);
            }
        }

        public int Hp
        {
            get { return status.currentHp; }

            set
            {
                status.currentHp = value;
                _UIController.UpdatePlayerHP(value);
            }
        }

        public int MaxHp
        {
            get { return status.maxHp; }

            set
            {
                status.maxHp = value;
                _UIController.UpdatePlayerMaxHP(value);
            }
        }

        public int Sp
        {
            get { return status.currentSp; }

            set
            {
                status.currentSp = value;
                _UIController.UpdatePlayerSP(value);
            }
        }

        public int MaxSp
        {
            get { return status.maxSp; }

            set
            {
                status.maxSp = value;
                _UIController.UpdatePlayerMaxSP(value);
            }
        }

        public Jobs Job
        {
            get { return _charInfo.job; }

            set
            {
                _charInfo.job = value;
                PlayerAnimator.ChangeJob(value);
                _UIController.UpdatePlayerJob(JobTable.table[(int)value]);
            }
        }

        public int BaseExp
        {
            get { return _charInfo.currentBaseExp; }

            set
            {
                _charInfo.currentBaseExp = value;
                _UIController.UpdatePlayerBaseExp(value);
            }
        }

        public int JobExp
        {
            get { return _charInfo.currentJobExp; }

            set
            {
                _charInfo.currentJobExp = value;
                _UIController.UpdatePlayerJobExp(value);
            }
        }

        public int BaseLvl
        {
            get { return status.baseLvl; }

            set
            {
                status.baseLvl = value;
                _UIController.UpdatePlayerBaseLvl(value);
            }
        }

        public int JobLvl
        {
            get { return status.jobLvl; }

            set
            {
                status.jobLvl = value;
                _UIController.UpdatePlayerJobLvl(value);
            }
        }

        public int Weight
        {
            get { return _charInfo.weight; }

            set
            {
                _charInfo.weight = value;
                _UIController.UpdatePlayerWeight(value);
            }
        }

        public int MaxWeight
        {
            get { return _charInfo.maxWeight; }

            set
            {
                _charInfo.maxWeight = value;
                _UIController.UpdatePlayerMaxWeight(value);
            }
        }

        public int Zeny
        {
            get { return _charInfo.zeny; }

            set
            {
                _charInfo.zeny = value;
                _UIController.UpdatePlayerZeny(value);
            }
        }

        private int _skillPoints;
        public int SkillPoints
        {
            get { return _skillPoints; }

            set
            {
                _skillPoints = value;
                _UIController.UpdatePlayerSkillPoints(value);
            }
        }

        private void RegisterPacketHandlers()
        {
            PacketDistributer.RegisterCallback(PacketIds.RCV_PlayerSkillPointsChanged, OnSkillPointsChanged);
            PacketDistributer.RegisterCallback(PacketIds.RCV_PlayerSkillTreeReload, OnSkillTreeReload);
        }

        private void OnSkillPointsChanged(RCV_Packet rawPacket)
        {
            var packet = (RCV_PlayerSkillPointsChanged)rawPacket;
            Debug.Log("Received player skill points change to " + packet.SkillPoints);

            SkillPoints = packet.SkillPoints;
        }

        private void OnSkillTreeReload(RCV_Packet rawPacket)
        {
            var packet = (RCV_PlayerSkillTreeReload)rawPacket;
            Debug.Log("Received update skill tree for " + packet.Job + "with " + packet.SkillInfoLength + " skills");

            Job = packet.Job;

            var list = SkillTree.Reload(packet.SkillInfos, packet.PermanentSkillCount, packet.UnindexedSkillCount);
            _UIController.SkillTreeReload(list, Job);
        }
    }
}
