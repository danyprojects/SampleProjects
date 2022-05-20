using RO.Common;
using System;

namespace RO.Network
{
    public partial class SND_ElevatedAtCommand
    {
        public void makeSizeCMD(Sizes size)
        {
            throw new NotImplementedException();
        }

        public void makeDexCMD(int dex)
        {
            throw new NotImplementedException();
        }

        public void makeIntCMD(int int_)
        {
            throw new NotImplementedException();
        }

        public void makeAgiCMD(int agi)
        {
            throw new NotImplementedException();
        }

        public void makeVitCMD(int vit)
        {
            throw new NotImplementedException();
        }

        public void makeLuckCMD(int luck)
        {
            throw new NotImplementedException();
        }

        public void makeStrCMD(int str)
        {
            throw new NotImplementedException();
        }

        public void makeGoCMD(int cityNumber)
        {
            throw new NotImplementedException();
        }

        public void makeWarpCMD(int mapid, int x, int y)
        {
            throw new NotImplementedException();
        }

        public void makeZenyCMD(int zeny)
        {
            throw new NotImplementedException();
        }

        public void makeItemCMD(int itemId, int amount)
        {
            throw new NotImplementedException();
        }

        public void makeJobChangeCMD(Jobs job)
        {
            payload = new byte[sizeof(short) + sizeof(byte) + sizeof(byte)];

            int index = 0;
            NetworkUtility.AppendNumber((Id), ref payload, ref index);
            NetworkUtility.AppendNumber((byte)ElevatedAtCommand.JobChange, ref payload, ref index);
            NetworkUtility.AppendNumber((byte)job, ref payload, ref index);
        }

        public void makeJobLvlCMD(int lvl)
        {
            payload = new byte[sizeof(short) + sizeof(byte) + sizeof(short)];

            int index = 0;
            NetworkUtility.AppendNumber((Id), ref payload, ref index);
            NetworkUtility.AppendNumber((byte)ElevatedAtCommand.JobLvl, ref payload, ref index);
            NetworkUtility.AppendNumber((short)lvl, ref payload, ref index);
        }

        public void makeBaseLvlCMD(int lvl)
        {
            throw new NotImplementedException();
        }

        public void makeGuildLvlCMD(int lvl)
        {
            throw new NotImplementedException();
        }

        public void makeAllSkillsCMD()
        {
            payload = new byte[sizeof(short) + sizeof(byte)];

            int index = 0;
            NetworkUtility.AppendNumber((Id), ref payload, ref index);
            NetworkUtility.AppendNumber((byte)ElevatedAtCommand.AllSkills, ref payload, ref index);
        }

        public void makeSkillCMD()
        {
            throw new NotImplementedException();
        }
    }
}