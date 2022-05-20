namespace RO.Databases
{
    public enum BuffIDs : int
    {
        FirstBuff = 0,

        Sit = FirstBuff,
        AspdBuff,
        ExpBuff,
        ItemBuff,
        FoodStr,
        FoodAgi,
        FoodVit,
        FoodDex,
        FoodLuk,

        Blessing,
        IncreaseAgi,
        Angelus,
        Aspersio,
        Gloria,
        KyrieEleison,
        ImpositioManus,
        LexAeterna,
        Magnificat,
        BenedictioSacramentio,
        Assumptio,
        SteelBody,
        CriticalExplosion,

        AttentionConcentrate,
        Falcon,
        TrueSight,
        WindWalk,
        SongHp,
        SongAgi,
        SongMatk,
        SongDef,
        SongMdef,
        SongExp,
        SongReserved1,
        SongReserved2,
        SongReserved3,
        SongReserved4,
        SongReserved5,
        SongReserved6,
        MarionetteControl,

        Cloaking,
        Hide,
        EnchantPoison,
        EnchantDeadlyPoison,

        EnergyCoat,
        EndowEarth,
        EndowFire,
        EndowWater,
        EndowWind,

        Endure,
        Provoke,
        Peco,
        TwoHandQuicken,
        TensionRelax,
        AutoGuard,
        ReflectShield,
        Defender,
        Providence,

        LoudExclamation,
        AdrenalineRush,
        MaximizePower,
        OverThrust,
        WeaponPerfection,
        CartBoost,
        ChemicalArmor,
        ChemicalHelm,
        ChemicalShield,
        ChemicalWeapon,

        Kaahi,
        Kaizel,
        Kaupe,

        FirstDebuff,

        Weight50 = FirstDebuff,
        Weight90,

        Quagmire,

        Bleeding,
        CriticalWounds,
        DecreaseAgi,
        SlowCast,

        DropArmor,
        DropHelm,
        DropShield,
        DropWeapon,

        HellsPower,

        Last = HellsPower
    }

    public enum BuffIconIDs : int
    {
        Sit = 0,
        AspdBuff,
        ExpBuff,
        ItemBuff,
        FoodStr,
        FoodAgi,
        FoodVit,
        FoodDex,
        FoodLuk,

        Blessing,
        IncreaseAgi,
        Angelus,
        Aspersio,
        Gloria,
        KyrieEleison,
        ImpositioManus,
        LexAeterna,
        Magnificat,
        BenedictioSacramentio,
        Assumptio,
        SteelBody,
        CriticalExplosion,

        AttentionConcentrate,
        Falcon,
        TrueSight,
        WindWalk,
        SongHp,
        SongAgi,
        SongMatk,
        SongDef,
        SongMdef,
        SongExp,
        SongReserved1,
        SongReserved2,
        SongReserved3,
        SongReserved4,
        SongReserved5,
        SongReserved6,
        MarionetteControl,

        Cloaking,
        Hide,
        EnchantPoison,
        EnchantDeadlyPoison,

        EnergyCoat,
        EndowEarth,
        EndowFire,
        EndowWater,
        EndowWind,

        Endure,
        Provoke,
        Peco,
        TwoHandQuicken,
        TensionRelax,
        AutoGuard,
        ReflectShield,
        Defender,
        Providence,

        LoudExclamation,
        AdrenalineRush,
        MaximizePower,
        OverThrust,
        WeaponPerfection,
        CartBoost,
        ChemicalArmor,
        ChemicalHelm,
        ChemicalShield,
        ChemicalWeapon,

        Kaahi,
        Kaizel,
        Kaupe,

        Weight50,
        Weight90,

        Quagmire,

        Bleeding,
        CriticalWounds,
        DecreaseAgi,
        SlowCast,

        DropArmor,
        DropHelm,
        DropShield,
        DropWeapon,

        HellsPower,

        Last = HellsPower,

        None = -1
    }

    public static class BuffDb
    {
        public enum Priority : byte
        {
            First = 0,

            Top = First,
            Red,
            Purple,
            Yellow,
            DarkBlue,
            Blue,
            Green,
            DarkYellow,
            White,

            Last
        }

        public struct BuffData
        {
            public Priority priority;
            public BuffIconIDs iconId;
        }

        public static readonly BuffData[] Buffs = new BuffData[(int)BuffIDs.Last + 1];

        static BuffDb()
        {
            Buffs[(int)BuffIDs.Sit]                     = new BuffData { priority = Priority.Yellow, iconId = BuffIconIDs.Sit };
            Buffs[(int)BuffIDs.AspdBuff]                = new BuffData { priority = Priority.Green, iconId = BuffIconIDs.AspdBuff };
            Buffs[(int)BuffIDs.ExpBuff]                 = new BuffData { priority = Priority.Top, iconId = BuffIconIDs.ExpBuff };
            Buffs[(int)BuffIDs.ItemBuff]                = new BuffData { priority = Priority.Top, iconId = BuffIconIDs.ItemBuff };
            Buffs[(int)BuffIDs.FoodStr]                 = new BuffData { priority = Priority.Top, iconId = BuffIconIDs.FoodStr };
            Buffs[(int)BuffIDs.FoodAgi]                 = new BuffData { priority = Priority.Top, iconId = BuffIconIDs.FoodAgi };
            Buffs[(int)BuffIDs.FoodVit]                 = new BuffData { priority = Priority.Top, iconId = BuffIconIDs.FoodVit };
            Buffs[(int)BuffIDs.FoodDex]                 = new BuffData { priority = Priority.Top, iconId = BuffIconIDs.FoodDex };
            Buffs[(int)BuffIDs.FoodLuk]                 = new BuffData { priority = Priority.Top, iconId = BuffIconIDs.FoodLuk };
            Buffs[(int)BuffIDs.Blessing]                = new BuffData { priority = Priority.White, iconId = BuffIconIDs.Blessing };
            Buffs[(int)BuffIDs.IncreaseAgi]             = new BuffData { priority = Priority.White, iconId = BuffIconIDs.IncreaseAgi };
            Buffs[(int)BuffIDs.Angelus]                 = new BuffData { priority = Priority.White, iconId = BuffIconIDs.Angelus };
            Buffs[(int)BuffIDs.Aspersio]                = new BuffData { priority = Priority.White, iconId = BuffIconIDs.Aspersio };
            Buffs[(int)BuffIDs.Gloria]                  = new BuffData { priority = Priority.White, iconId = BuffIconIDs.Gloria };
            Buffs[(int)BuffIDs.KyrieEleison]            = new BuffData { priority = Priority.White, iconId = BuffIconIDs.KyrieEleison };
            Buffs[(int)BuffIDs.ImpositioManus]          = new BuffData { priority = Priority.White, iconId = BuffIconIDs.ImpositioManus };
            Buffs[(int)BuffIDs.LexAeterna]              = new BuffData { priority = Priority.White, iconId = BuffIconIDs.LexAeterna };
            Buffs[(int)BuffIDs.Magnificat]              = new BuffData { priority = Priority.White, iconId = BuffIconIDs.Magnificat };
            Buffs[(int)BuffIDs.BenedictioSacramentio]   = new BuffData { priority = Priority.White, iconId = BuffIconIDs.BenedictioSacramentio };
            Buffs[(int)BuffIDs.Assumptio]               = new BuffData { priority = Priority.White, iconId = BuffIconIDs.Assumptio };
            Buffs[(int)BuffIDs.SteelBody]               = new BuffData { priority = Priority.DarkYellow, iconId = BuffIconIDs.SteelBody };
            Buffs[(int)BuffIDs.CriticalExplosion]       = new BuffData { priority = Priority.Yellow, iconId = BuffIconIDs.CriticalExplosion };
            Buffs[(int)BuffIDs.AttentionConcentrate]    = new BuffData { priority = Priority.Green, iconId = BuffIconIDs.AttentionConcentrate };
            Buffs[(int)BuffIDs.Falcon]                  = new BuffData { priority = Priority.Green, iconId = BuffIconIDs.Falcon };
            Buffs[(int)BuffIDs.TrueSight]               = new BuffData { priority = Priority.White, iconId = BuffIconIDs.TrueSight };
            Buffs[(int)BuffIDs.WindWalk]                = new BuffData { priority = Priority.Yellow, iconId = BuffIconIDs.WindWalk };
            Buffs[(int)BuffIDs.SongHp]                  = new BuffData { priority = Priority.DarkBlue, iconId = BuffIconIDs.SongHp };
            Buffs[(int)BuffIDs.SongAgi]                 = new BuffData { priority = Priority.DarkBlue, iconId = BuffIconIDs.SongAgi };
            Buffs[(int)BuffIDs.SongMatk]                = new BuffData { priority = Priority.DarkBlue, iconId = BuffIconIDs.SongMatk };
            Buffs[(int)BuffIDs.SongDef]                 = new BuffData { priority = Priority.DarkBlue, iconId = BuffIconIDs.SongDef };
            Buffs[(int)BuffIDs.SongMdef]                = new BuffData { priority = Priority.DarkBlue, iconId = BuffIconIDs.SongMdef };
            Buffs[(int)BuffIDs.SongExp]                 = new BuffData { priority = Priority.DarkBlue, iconId = BuffIconIDs.SongExp };
            Buffs[(int)BuffIDs.SongReserved1]           = new BuffData { priority = Priority.DarkBlue, iconId = BuffIconIDs.SongReserved1 };
            Buffs[(int)BuffIDs.SongReserved2]           = new BuffData { priority = Priority.DarkBlue, iconId = BuffIconIDs.SongReserved2 };
            Buffs[(int)BuffIDs.SongReserved3]           = new BuffData { priority = Priority.DarkBlue, iconId = BuffIconIDs.SongReserved3 };
            Buffs[(int)BuffIDs.SongReserved4]           = new BuffData { priority = Priority.DarkBlue, iconId = BuffIconIDs.SongReserved4 };
            Buffs[(int)BuffIDs.SongReserved5]           = new BuffData { priority = Priority.DarkBlue, iconId = BuffIconIDs.SongReserved5 };
            Buffs[(int)BuffIDs.SongReserved6]           = new BuffData { priority = Priority.DarkBlue, iconId = BuffIconIDs.SongReserved6 };
            Buffs[(int)BuffIDs.MarionetteControl]       = new BuffData { priority = Priority.Red, iconId = BuffIconIDs.MarionetteControl };
            Buffs[(int)BuffIDs.Cloaking]                = new BuffData { priority = Priority.Blue, iconId = BuffIconIDs.Cloaking };
            Buffs[(int)BuffIDs.Hide]                    = new BuffData { priority = Priority.Blue, iconId = BuffIconIDs.Hide };
            Buffs[(int)BuffIDs.EnchantPoison]           = new BuffData { priority = Priority.Green, iconId = BuffIconIDs.EnchantPoison };
            Buffs[(int)BuffIDs.EnchantDeadlyPoison]     = new BuffData { priority = Priority.Green, iconId = BuffIconIDs.EnchantDeadlyPoison };
            Buffs[(int)BuffIDs.EnergyCoat]              = new BuffData { priority = Priority.Green, iconId = BuffIconIDs.EnergyCoat };
            Buffs[(int)BuffIDs.EndowEarth]              = new BuffData { priority = Priority.Green, iconId = BuffIconIDs.EndowEarth };
            Buffs[(int)BuffIDs.EndowFire]               = new BuffData { priority = Priority.Red, iconId = BuffIconIDs.EndowFire };
            Buffs[(int)BuffIDs.EndowWater]              = new BuffData { priority = Priority.Blue, iconId = BuffIconIDs.EndowWater };
            Buffs[(int)BuffIDs.EndowWind]               = new BuffData { priority = Priority.Yellow, iconId = BuffIconIDs.EndowWind };
            Buffs[(int)BuffIDs.Endure]                  = new BuffData { priority = Priority.Blue, iconId = BuffIconIDs.Endure };
            Buffs[(int)BuffIDs.Provoke]                 = new BuffData { priority = Priority.Blue, iconId = BuffIconIDs.Provoke };
            Buffs[(int)BuffIDs.Peco]                    = new BuffData { priority = Priority.Green, iconId = BuffIconIDs.Peco };
            Buffs[(int)BuffIDs.TwoHandQuicken]          = new BuffData { priority = Priority.Blue, iconId = BuffIconIDs.TwoHandQuicken };
            Buffs[(int)BuffIDs.TensionRelax]            = new BuffData { priority = Priority.Purple, iconId = BuffIconIDs.TensionRelax };
            Buffs[(int)BuffIDs.AutoGuard]               = new BuffData { priority = Priority.Blue, iconId = BuffIconIDs.AutoGuard };
            Buffs[(int)BuffIDs.ReflectShield]           = new BuffData { priority = Priority.Blue, iconId = BuffIconIDs.ReflectShield };
            Buffs[(int)BuffIDs.Defender]                = new BuffData { priority = Priority.Blue, iconId = BuffIconIDs.Defender };
            Buffs[(int)BuffIDs.Providence]              = new BuffData { priority = Priority.White, iconId = BuffIconIDs.Providence };
            Buffs[(int)BuffIDs.LoudExclamation]         = new BuffData { priority = Priority.Green, iconId = BuffIconIDs.LoudExclamation };
            Buffs[(int)BuffIDs.AdrenalineRush]          = new BuffData { priority = Priority.Blue, iconId = BuffIconIDs.AdrenalineRush };
            Buffs[(int)BuffIDs.MaximizePower]           = new BuffData { priority = Priority.Blue, iconId = BuffIconIDs.MaximizePower };
            Buffs[(int)BuffIDs.OverThrust]              = new BuffData { priority = Priority.Blue, iconId = BuffIconIDs.OverThrust };
            Buffs[(int)BuffIDs.WeaponPerfection]        = new BuffData { priority = Priority.Blue, iconId = BuffIconIDs.WeaponPerfection };
            Buffs[(int)BuffIDs.CartBoost]               = new BuffData { priority = Priority.Yellow, iconId = BuffIconIDs.CartBoost };
            Buffs[(int)BuffIDs.ChemicalArmor]           = new BuffData { priority = Priority.Green, iconId = BuffIconIDs.ChemicalArmor };
            Buffs[(int)BuffIDs.ChemicalHelm]            = new BuffData { priority = Priority.Green, iconId = BuffIconIDs.ChemicalHelm };
            Buffs[(int)BuffIDs.ChemicalShield]          = new BuffData { priority = Priority.Green, iconId = BuffIconIDs.ChemicalShield };
            Buffs[(int)BuffIDs.ChemicalWeapon]          = new BuffData { priority = Priority.Green, iconId = BuffIconIDs.ChemicalWeapon };
            Buffs[(int)BuffIDs.Kaahi]                   = new BuffData { priority = Priority.DarkBlue, iconId = BuffIconIDs.Kaahi };
            Buffs[(int)BuffIDs.Kaizel]                  = new BuffData { priority = Priority.DarkBlue, iconId = BuffIconIDs.Kaizel };
            Buffs[(int)BuffIDs.Kaupe]                   = new BuffData { priority = Priority.DarkBlue, iconId = BuffIconIDs.Kaupe };
            Buffs[(int)BuffIDs.Weight50]                = new BuffData { priority = Priority.Red, iconId = BuffIconIDs.Weight50 };
            Buffs[(int)BuffIDs.Weight90]                = new BuffData { priority = Priority.Red, iconId = BuffIconIDs.Weight90 };
            Buffs[(int)BuffIDs.Quagmire]                = new BuffData { priority = Priority.Red, iconId = BuffIconIDs.Quagmire };
            Buffs[(int)BuffIDs.Bleeding]                = new BuffData { priority = Priority.Red, iconId = BuffIconIDs.Bleeding };
            Buffs[(int)BuffIDs.CriticalWounds]          = new BuffData { priority = Priority.Yellow, iconId = BuffIconIDs.CriticalWounds };
            Buffs[(int)BuffIDs.DecreaseAgi]             = new BuffData { priority = Priority.Red, iconId = BuffIconIDs.DecreaseAgi };
            Buffs[(int)BuffIDs.SlowCast]                = new BuffData { priority = Priority.Yellow, iconId = BuffIconIDs.SlowCast };
            Buffs[(int)BuffIDs.DropArmor]               = new BuffData { priority = Priority.Red, iconId = BuffIconIDs.DropArmor };
            Buffs[(int)BuffIDs.DropHelm]                = new BuffData { priority = Priority.Red, iconId = BuffIconIDs.DropHelm };
            Buffs[(int)BuffIDs.DropShield]              = new BuffData { priority = Priority.Red, iconId = BuffIconIDs.DropShield };
            Buffs[(int)BuffIDs.DropWeapon]              = new BuffData { priority = Priority.Red, iconId = BuffIconIDs.DropWeapon };
            Buffs[(int)BuffIDs.HellsPower]              = new BuffData { priority = Priority.Purple, iconId = BuffIconIDs.HellsPower };
        }
    }
}
