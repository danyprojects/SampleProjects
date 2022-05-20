using RO.Common;
using static RO.Databases.SkillTreeDb.SkillEntry;

namespace RO.Databases
{
    public sealed class SkillTreeDb
    {
        private static readonly SkillTreeDb[] _skillTree;
        public readonly SkillEntry[] Skills;
        public static readonly int MaxSlotIndex;

        public struct SkillEntry
        {
            public struct Requirement
            {
                public Requirement(SkillIds id, int lvl, int dependencyIndex)
                {
                    Id = id;
                    Lvl = lvl;
                    DependencyIndex = dependencyIndex;

                }

                public readonly SkillIds Id;
                public readonly int DependencyIndex;
                public readonly int Lvl;
            }

            public SkillEntry(SkillIds id, int slotIndex, params Requirement[] requirements)
            {
                Id = id;
                SlotIndex = slotIndex;
                Requirements = requirements;
            }

            public readonly SkillIds Id;
            public readonly int SlotIndex;
            public readonly Requirement[] Requirements;
        }

        public static ref SkillTreeDb GetSkillTree(Jobs job)
        {
            return ref _skillTree[(int)job];
        }

        private SkillTreeDb(params SkillEntry[] skills)
        {
            Skills = skills;
        }

        static private int slot(int i, ref int max)
        {
            max = i > max ? i : max;
            return i;
        }

        static private Requirement BasicSkill { get; } = new Requirement(SkillIds.BasicSkill, 9, 0);

        static SkillTreeDb()
        {
            MaxSlotIndex = 0;

            _skillTree = new SkillTreeDb[(int)Jobs.Last + 1];

            _skillTree[(int)Jobs.Novice] = new SkillTreeDb
            (
                /*0*/ new SkillEntry(SkillIds.BasicSkill, slot(0, ref MaxSlotIndex)),
                /*1*/ new SkillEntry(SkillIds.FirstAid, slot(7, ref MaxSlotIndex)),
                /*2*/ new SkillEntry(SkillIds.TrickDead, slot(14, ref MaxSlotIndex))
            );

            _skillTree[(int)Jobs.Swordsman] = new SkillTreeDb
            (
                /*3*/  new SkillEntry(SkillIds.SwordMastery, slot(1, ref MaxSlotIndex), BasicSkill),
                /*4*/  new SkillEntry(SkillIds.HpRecovery, slot(2, ref MaxSlotIndex), BasicSkill),
                /*5*/  new SkillEntry(SkillIds.Bash, slot(3, ref MaxSlotIndex), BasicSkill),
                /*6*/  new SkillEntry(SkillIds.Provoke, slot(4, ref MaxSlotIndex), BasicSkill),
                /*7*/  new SkillEntry(SkillIds.AutoBerserk, slot(5, ref MaxSlotIndex), BasicSkill),
                /*8*/  new SkillEntry(SkillIds.MovingHpRecovery, slot(6, ref MaxSlotIndex), BasicSkill),
                /*9*/  new SkillEntry(SkillIds.TwoHandSwordMastery, slot(8, ref MaxSlotIndex), new Requirement(SkillIds.SwordMastery, 1, 3), BasicSkill),
                /*10*/ new SkillEntry(SkillIds.MagnumBreak, slot(10, ref MaxSlotIndex), new Requirement(SkillIds.Bash, 5, 5), BasicSkill),
                /*11*/ new SkillEntry(SkillIds.Endure, slot(11, ref MaxSlotIndex), new Requirement(SkillIds.Provoke, 5, 6), BasicSkill),
                /*12*/ new SkillEntry(SkillIds.FatalBlow, slot(12, ref MaxSlotIndex), BasicSkill)
            );

            _skillTree[(int)Jobs.Mage] = new SkillTreeDb
            (
                /*3*/  new SkillEntry(SkillIds.StoneCurse, slot(1, ref MaxSlotIndex), BasicSkill),
                /*4*/  new SkillEntry(SkillIds.ColdBolt, slot(2, ref MaxSlotIndex), BasicSkill),
                /*5*/  new SkillEntry(SkillIds.LightningBolt, slot(3, ref MaxSlotIndex), BasicSkill),
                /*6*/  new SkillEntry(SkillIds.NapalmBeat, slot(4, ref MaxSlotIndex), BasicSkill),
                /*7*/  new SkillEntry(SkillIds.FireBolt, slot(5, ref MaxSlotIndex), BasicSkill),
                /*8*/  new SkillEntry(SkillIds.Sight, slot(6, ref MaxSlotIndex), BasicSkill),
                /*9*/  new SkillEntry(SkillIds.SpRecovery, slot(8, ref MaxSlotIndex), BasicSkill),
                /*10*/ new SkillEntry(SkillIds.FrostDiver, slot(9, ref MaxSlotIndex), new Requirement(SkillIds.ColdBolt, 5, 4), BasicSkill),
                /*11*/ new SkillEntry(SkillIds.ThunderStorm, slot(10, ref MaxSlotIndex), new Requirement(SkillIds.LightningBolt, 4, 5), BasicSkill),
                /*12*/ new SkillEntry(SkillIds.SoulStrike, slot(11, ref MaxSlotIndex), new Requirement(SkillIds.NapalmBeat, 4, 6), BasicSkill),
                /*13*/ new SkillEntry(SkillIds.FireBall, slot(12, ref MaxSlotIndex), new Requirement(SkillIds.FireBolt, 4, 7), BasicSkill),
                /*14*/ new SkillEntry(SkillIds.EnergyCoat, slot(13, ref MaxSlotIndex), BasicSkill),
                /*15*/ new SkillEntry(SkillIds.SafetyWall, slot(18, ref MaxSlotIndex), new Requirement(SkillIds.NapalmBeat, 7, 6), new Requirement(SkillIds.SoulStrike, 5, 12), BasicSkill),
                /*16*/ new SkillEntry(SkillIds.FireWall, slot(19, ref MaxSlotIndex), new Requirement(SkillIds.FireBall, 5, 13), new Requirement(SkillIds.Sight, 1, 8), BasicSkill)
            );

            _skillTree[(int)Jobs.Wizard] = new SkillTreeDb
            (
                /*17*/ new SkillEntry(SkillIds.Sense, slot(0, ref MaxSlotIndex), BasicSkill),
                /*18*/ new SkillEntry(SkillIds.IceWall, slot(1, ref MaxSlotIndex), new Requirement(SkillIds.StoneCurse, 1, 3), new Requirement(SkillIds.FrostDiver, 1, 10), BasicSkill),
                /*19*/ new SkillEntry(SkillIds.JupitelThunder, slot(2, ref MaxSlotIndex), new Requirement(SkillIds.NapalmBeat, 1, 6), new Requirement(SkillIds.LightningBolt, 1, 5), BasicSkill),
                /*20*/ new SkillEntry(SkillIds.EarthSpike, slot(3, ref MaxSlotIndex), new Requirement(SkillIds.StoneCurse, 1, 3), BasicSkill),
                /*21*/ new SkillEntry(SkillIds.SightTrasher, slot(4, ref MaxSlotIndex), new Requirement(SkillIds.Sight, 1, 8), new Requirement(SkillIds.LightningBolt, 1, 5), BasicSkill),
                /*22*/ new SkillEntry(SkillIds.FirePillar, slot(5, ref MaxSlotIndex), new Requirement(SkillIds.FireWall, 1, 16), BasicSkill),
                /*23*/ new SkillEntry(SkillIds.SightBlaster, slot(6, ref MaxSlotIndex), BasicSkill),
                /*24*/ new SkillEntry(SkillIds.FrostNova, slot(8, ref MaxSlotIndex), new Requirement(SkillIds.IceWall, 1, 18), BasicSkill),
                /*25*/ new SkillEntry(SkillIds.LordOfVermilion, slot(9, ref MaxSlotIndex), new Requirement(SkillIds.ThunderStorm, 1, 11), new Requirement(SkillIds.JupitelThunder, 5, 19), BasicSkill),
                /*26*/ new SkillEntry(SkillIds.HeavensDrive, slot(10, ref MaxSlotIndex), new Requirement(SkillIds.EarthSpike, 3, 20), BasicSkill),
                /*27*/ new SkillEntry(SkillIds.MeteorStorm, slot(11, ref MaxSlotIndex), new Requirement(SkillIds.SightTrasher, 2, 21), new Requirement(SkillIds.ThunderStorm, 1, 11), BasicSkill),
                /*28*/ new SkillEntry(SkillIds.WaterBall, slot(15, ref MaxSlotIndex), new Requirement(SkillIds.ColdBolt, 1, 4), new Requirement(SkillIds.LightningBolt, 1, 5), BasicSkill),
                /*29*/ new SkillEntry(SkillIds.Quagmire, slot(17, ref MaxSlotIndex), new Requirement(SkillIds.HeavensDrive, 1, 26), BasicSkill),
                /*30*/ new SkillEntry(SkillIds.StormGust, slot(22, ref MaxSlotIndex), new Requirement(SkillIds.FrostDiver, 1, 10), new Requirement(SkillIds.JupitelThunder, 3, 19), BasicSkill)
            );

            _skillTree[(int)Jobs.HighWizard] = new SkillTreeDb
            (
                /*31*/ new SkillEntry(SkillIds.Ganbantein, slot(7, ref MaxSlotIndex), new Requirement(SkillIds.Sense, 1, 17), new Requirement(SkillIds.IceWall, 1, 18), BasicSkill),
                /*32*/ new SkillEntry(SkillIds.MagicCrasher, slot(12, ref MaxSlotIndex), new Requirement(SkillIds.SpRecovery, 1, 9), BasicSkill),
                /*33*/ new SkillEntry(SkillIds.SoulDrain, slot(13, ref MaxSlotIndex), new Requirement(SkillIds.SpRecovery, 5, 9), new Requirement(SkillIds.SoulStrike, 7, 12), BasicSkill),
                /*34*/ new SkillEntry(SkillIds.NapalmVulcan, slot(19, ref MaxSlotIndex), new Requirement(SkillIds.NapalmBeat, 5, 6), BasicSkill),
                /*35*/ new SkillEntry(SkillIds.MysticAmplification, slot(20, ref MaxSlotIndex), BasicSkill),
                /*36*/ new SkillEntry(SkillIds.GravitationalField, slot(24, ref MaxSlotIndex), new Requirement(SkillIds.Quagmire, 1, 29), new Requirement(SkillIds.MagicCrasher, 1, 32), new Requirement(SkillIds.MysticAmplification, 10, 35), BasicSkill)
            );
        }
    }
}