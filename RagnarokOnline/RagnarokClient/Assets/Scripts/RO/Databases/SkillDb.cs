using System;

namespace RO.Databases
{
    public enum SkillIds : int
    {
        First = 0,

        AreaOfEffectStart = First,

        StormGust = AreaOfEffectStart,
        MeteorStorm,
        MagnumBreak,
        FireWall,
        SafetyWall,
        ThunderStorm,
        IceWall,
        SightTrasher,
        FirePillar,
        LordOfVermilion,
        HeavensDrive,
        Quagmire,
        Ganbantein,
        GravitationalField,

        SingleTargetStart,
        Devotion = SingleTargetStart,
        Mammomite,
        Bash,
        Provoke,
        StoneCurse,
        ColdBolt,
        LightningBolt,
        NapalmBeat,
        FireBolt,
        FrostDiver,
        FireBall,
        SoulStrike,
        Sense,
        JupitelThunder,
        EarthSpike,
        WaterBall,
        MagicCrasher,
        NapalmVulcan,

        NoTargetStart,
        FirstAid = NoTargetStart,
        TrickDead,
        BasicSkill,
        SwordMastery,
        TwoHandSwordMastery,
        HpRecovery,
        SpRecovery,
        Endure,
        FatalBlow,
        MovingHpRecovery,
        AutoBerserk,
        Sight,
        EnergyCoat,
        SightBlaster,
        FrostNova,
        SoulDrain,
        MysticAmplification,

        Last
    }

    public sealed class SkillDb
    {
        private static readonly SkillDb[] _skills = new SkillDb[(int)SkillIds.Last];

        public readonly string Name;
        public readonly int MaxLevel;
        public readonly bool QuestSkill;
        public readonly SpCost[] Sp;
        public readonly AttackRange[] Range;
        public readonly SkillFlag SkillFlags;

        [Flags]
        public enum SkillFlag
        {
            None = 0,
            QuestSkill = 1 << 0,
            SelectLvl = 1 << 1,
            Passive = 1 << 2,
        }

        public struct SpCost
        {
            public SpCost(int value) { Value = value; }
            public readonly int Value;
        }

        public struct AttackRange
        {
            public AttackRange(int value) { Value = value; }
            public readonly int Value;
        }

        public static ref SkillDb GetSkill(SkillIds skillId)
        {
            return ref _skills[(int)skillId];
        }

        private SkillDb(string name, int maxLevel, SpCost[] sp, AttackRange[] range, SkillFlag flags = SkillFlag.None)
        {
            Name = name;
            MaxLevel = maxLevel;
            SkillFlags = flags;
            Sp = sp;
            Range = range;
        }

        static SkillDb()
        {
            _skills[(int)SkillIds.StormGust] = new SkillDb("Storm Gust", 10,
            Cost(78, 78, 78, 78, 78, 78, 78, 78, 78, 78),
            Ranges(9, 9, 9, 9, 9, 9, 9, 9, 9, 9),
            SkillFlag.SelectLvl);

            _skills[(int)SkillIds.MeteorStorm] = new SkillDb("Meteor Storm", 10,
            Cost(20, 24, 30, 34, 40, 44, 50, 54, 60, 64),
            Ranges(9, 9, 9, 9, 9, 9, 9, 9, 9, 9),
            SkillFlag.SelectLvl);

            _skills[(int)SkillIds.MagnumBreak] = new SkillDb("Magnum Break", 10,
            Cost(30, 30, 30, 30, 30, 30, 30, 30, 30, 30),
            Ranges(1, 1, 1, 1, 1, 1, 1, 1, 1, 1));

            _skills[(int)SkillIds.FireWall] = new SkillDb("Fire Wall", 10,
            Cost(40, 40, 40, 40, 40, 40, 40, 40, 40, 40),
            Ranges(9, 9, 9, 9, 9, 9, 9, 9, 9, 9));

            _skills[(int)SkillIds.SafetyWall] = new SkillDb("Safety Wall", 10,
            Cost(30, 30, 30, 35, 35, 35, 40, 40, 40, 40),
            Ranges(9, 9, 9, 9, 9, 9, 9, 9, 9, 9),
            SkillFlag.SelectLvl);

            _skills[(int)SkillIds.ThunderStorm] = new SkillDb("Thunder Storm", 10,
            Cost(29, 34, 39, 44, 49, 54, 59, 64, 69, 74),
            Ranges(9, 9, 9, 9, 9, 9, 9, 9, 9, 9),
            SkillFlag.SelectLvl);

            _skills[(int)SkillIds.IceWall] = new SkillDb("Ice Wall", 10,
            Cost(20, 20, 20, 20, 20, 20, 20, 20, 20, 20),
            Ranges(9, 9, 9, 9, 9, 9, 9, 9, 9, 9));

            _skills[(int)SkillIds.SightTrasher] = new SkillDb("Sight Trasher", 10,
            Cost(35, 37, 39, 41, 43, 45, 47, 49, 51, 53),
            Ranges(1, 1, 1, 1, 1, 1, 1, 1, 1, 1),
            SkillFlag.SelectLvl);

            _skills[(int)SkillIds.FirePillar] = new SkillDb("Fire Pillar", 10,
            Cost(75, 75, 75, 75, 75, 75, 75, 75, 75, 75),
            Ranges(9, 9, 9, 9, 9, 9, 9, 9, 9, 9),
            SkillFlag.SelectLvl);

            _skills[(int)SkillIds.LordOfVermilion] = new SkillDb("Lord of Vermillion", 10,
            Cost(60, 64, 68, 72, 76, 80, 84, 88, 92, 96),
            Ranges(9, 9, 9, 9, 9, 9, 9, 9, 9, 9),
            SkillFlag.SelectLvl);

            _skills[(int)SkillIds.HeavensDrive] = new SkillDb("Heaven's Drive", 5,
            Cost(28, 32, 36, 40, 44),
            Ranges(9, 9, 9, 9, 9),
            SkillFlag.SelectLvl);

            _skills[(int)SkillIds.Quagmire] = new SkillDb("Quagmire", 5,
            Cost(5, 10, 15, 20, 25),
            Ranges(9, 9, 9, 9, 9));

            _skills[(int)SkillIds.Ganbantein] = new SkillDb("Ganbantein", 1,
            Cost(40),
            Ranges(18));

            _skills[(int)SkillIds.GravitationalField] = new SkillDb("Gravitational Field", 5,
            Cost(20, 40, 60, 80, 100),
            Ranges(18, 18, 18, 18, 18),
            SkillFlag.SelectLvl);

            _skills[(int)SkillIds.Devotion] = new SkillDb("Devotion", 5,
            Cost(25, 25, 25, 25, 25),
            Ranges(7, 8, 9, 10, 11));

            _skills[(int)SkillIds.Mammomite] = new SkillDb("Mammomite", 10,
            Cost(5, 5, 5, 5, 5, 5, 5, 5, 5, 5),
            Ranges(1, 1, 1, 1, 1, 1, 1, 1, 1, 1));

            _skills[(int)SkillIds.Bash] = new SkillDb("Bash", 10,
            Cost(8, 8, 8, 8, 8, 15, 15, 15, 15, 15),
            Ranges(1, 1, 1, 1, 1, 1, 1, 1, 1, 1),
            SkillFlag.SelectLvl);

            _skills[(int)SkillIds.Provoke] = new SkillDb("Provoke", 10,
            Cost(4, 5, 6, 7, 8, 9, 10, 11, 12, 13),
            Ranges(9, 9, 9, 9, 9, 9, 9, 9, 9, 9),
            SkillFlag.SelectLvl);

            _skills[(int)SkillIds.StoneCurse] = new SkillDb("Stone Curse", 10,
            Cost(25, 24, 23, 22, 21, 20, 19, 18, 17, 16),
            Ranges(2, 2, 2, 2, 2, 2, 2, 2, 2, 2));

            _skills[(int)SkillIds.ColdBolt] = new SkillDb("Cold Bolt", 10,
            Cost(12, 14, 16, 18, 20, 22, 24, 26, 28, 30),
            Ranges(9, 9, 9, 9, 9, 9, 9, 9, 9, 9),
            SkillFlag.SelectLvl);

            _skills[(int)SkillIds.LightningBolt] = new SkillDb("Lightning Bolt", 10,
            Cost(12, 14, 16, 18, 20, 22, 24, 26, 28, 30),
            Ranges(9, 9, 9, 9, 9, 9, 9, 9, 9, 9),
            SkillFlag.SelectLvl);

            _skills[(int)SkillIds.NapalmBeat] = new SkillDb("Napalm Beat", 10,
            Cost(9, 9, 9, 12, 12, 12, 15, 15, 15, 18),
            Ranges(9, 9, 9, 9, 9, 9, 9, 9, 9, 9));

            _skills[(int)SkillIds.FireBolt] = new SkillDb("Fire Bolt", 10,
            Cost(12, 14, 16, 18, 20, 22, 24, 26, 28, 30),
            Ranges(9, 9, 9, 9, 9, 9, 9, 9, 9, 9),
            SkillFlag.SelectLvl);

            _skills[(int)SkillIds.FrostDiver] = new SkillDb("Frost Diver", 10,
            Cost(25, 24, 23, 22, 21, 20, 19, 18, 17, 16),
            Ranges(9, 9, 9, 9, 9, 9, 9, 9, 9, 9));

            _skills[(int)SkillIds.FireBall] = new SkillDb("Fire Ball", 10,
            Cost(25, 25, 25, 25, 25, 25, 25, 25, 25, 25),
            Ranges(9, 9, 9, 9, 9, 9, 9, 9, 9, 9));

            _skills[(int)SkillIds.SoulStrike] = new SkillDb("Soul Strike", 10,
            Cost(18, 14, 24, 20, 30, 26, 36, 32, 42, 38),
            Ranges(9, 9, 9, 9, 9, 9, 9, 9, 9, 9),
            SkillFlag.SelectLvl);

            _skills[(int)SkillIds.Sense] = new SkillDb("Sense", 1,
            Cost(10),
            Ranges(9));

            _skills[(int)SkillIds.JupitelThunder] = new SkillDb("Jupitel Thunder", 10,
            Cost(20, 23, 26, 29, 32, 35, 38, 41, 44, 47),
            Ranges(9, 9, 9, 9, 9, 9, 9, 9, 9, 9),
            SkillFlag.SelectLvl);

            _skills[(int)SkillIds.EarthSpike] = new SkillDb("Earth Spike", 5,
            Cost(12, 14, 16, 18, 20),
            Ranges(9, 9, 9, 9, 9),
            SkillFlag.SelectLvl);

            _skills[(int)SkillIds.WaterBall] = new SkillDb("Water ball", 5,
            Cost(15, 20, 20, 25, 25),
            Ranges(9, 9, 9, 9, 9),
            SkillFlag.SelectLvl);

            _skills[(int)SkillIds.MagicCrasher] = new SkillDb("Magic Crasher", 1,
            Cost(8),
            Ranges(9));

            _skills[(int)SkillIds.NapalmVulcan] = new SkillDb("Napalm Vulcan", 5,
            Cost(10, 25, 40, 55, 70),
            Ranges(9, 9, 9, 9, 9),
            SkillFlag.SelectLvl);

            _skills[(int)SkillIds.FirstAid] = new SkillDb("First Aid", 1,
            Cost(3),
            Ranges(1),
            SkillFlag.QuestSkill);

            _skills[(int)SkillIds.TrickDead] = new SkillDb("Play Dead", 1,
            Cost(5),
            Ranges(1),
            SkillFlag.QuestSkill);

            _skills[(int)SkillIds.BasicSkill] = new SkillDb("Basic Skill", 9,
            Cost(0, 0, 0, 0, 0, 0, 0, 0, 0),
            Ranges(1, 1, 1, 1, 1, 1, 1, 1, 1),
            SkillFlag.Passive);

            _skills[(int)SkillIds.SwordMastery] = new SkillDb("Sword Mastery", 10,
            Cost(0, 0, 0, 0, 0, 0, 0, 0, 0, 0),
            Ranges(1, 1, 1, 1, 1, 1, 1, 1, 1, 1),
            SkillFlag.Passive);

            _skills[(int)SkillIds.TwoHandSwordMastery] = new SkillDb("Two-Handed Sword Mastery", 10,
            Cost(0, 0, 0, 0, 0, 0, 0, 0, 0, 0),
            Ranges(1, 1, 1, 1, 1, 1, 1, 1, 1, 1),
            SkillFlag.Passive);

            _skills[(int)SkillIds.HpRecovery] = new SkillDb("Increase HP Recovery", 10,
            Cost(0, 0, 0, 0, 0, 0, 0, 0, 0, 0),
            Ranges(1, 1, 1, 1, 1, 1, 1, 1, 1, 1),
            SkillFlag.Passive);

            _skills[(int)SkillIds.SpRecovery] = new SkillDb("Increase SP Recovery", 10,
            Cost(0, 0, 0, 0, 0, 0, 0, 0, 0, 0),
            Ranges(1, 1, 1, 1, 1, 1, 1, 1, 1, 1),
            SkillFlag.Passive);

            _skills[(int)SkillIds.Endure] = new SkillDb("Endure", 10,
            Cost(10, 10, 10, 10, 10, 10, 10, 10, 10, 10),
            Ranges(1, 1, 1, 1, 1, 1, 1, 1, 1, 1));

            _skills[(int)SkillIds.FatalBlow] = new SkillDb("Fatal Blow", 1,
            Cost(0),
            Ranges(1),
            SkillFlag.QuestSkill | SkillFlag.Passive);

            _skills[(int)SkillIds.MovingHpRecovery] = new SkillDb("Moving HP Recovery", 1,
            Cost(0),
            Ranges(1),
            SkillFlag.QuestSkill | SkillFlag.Passive);

            _skills[(int)SkillIds.AutoBerserk] = new SkillDb("Auto-Berserk", 1,
            Cost(1),
            Ranges(1),
            SkillFlag.QuestSkill);

            _skills[(int)SkillIds.Sight] = new SkillDb("Sight", 1,
            Cost(10),
            Ranges(1));

            _skills[(int)SkillIds.EnergyCoat] = new SkillDb("Energy Coat", 1,
            Cost(30),
            Ranges(1),
            SkillFlag.QuestSkill);

            _skills[(int)SkillIds.SightBlaster] = new SkillDb("Sight Blaster", 1,
            Cost(40),
            Ranges(1),
            SkillFlag.QuestSkill);

            _skills[(int)SkillIds.FrostNova] = new SkillDb("Frost Nova", 10,
            Cost(45, 43, 41, 39, 37, 35, 33, 31, 29, 27),
            Ranges(9, 9, 9, 9, 9, 9, 9, 9, 9, 9));

            _skills[(int)SkillIds.SoulDrain] = new SkillDb("Soul Drain", 10,
            Cost(0, 0, 0, 0, 0, 0, 0, 0, 0, 0),
            Ranges(1, 1, 1, 1, 1, 1, 1, 1, 1, 1),
            SkillFlag.Passive);

            _skills[(int)SkillIds.MysticAmplification] = new SkillDb("Mystic Amplification", 10,
            Cost(14, 18, 22, 26, 30, 34, 38, 42, 46, 50),
            Ranges(1, 1, 1, 1, 1, 1, 1, 1, 1, 1),
            SkillFlag.SelectLvl);
        }

        private static SpCost[] Cost(params int[] values)
        {
            SpCost[] spArray = new SpCost[values.Length];

            for (int i = 0; i < values.Length; i++)
                spArray[i] = new SpCost(values[i]);

            return spArray;
        }

        private static AttackRange[] Ranges(params int[] values)
        {
            AttackRange[] ranges = new AttackRange[values.Length];

            for (int i = 0; i < values.Length; i++)
                ranges[i] = new AttackRange(values[i]);

            return ranges;
        }
    }
}
