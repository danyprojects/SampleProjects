using UnityEngine;

namespace RO.Databases
{
    //This contains only STR and act effect
    public enum EffectIDs : int
    {
        SingleTargetStart = 0,
        Sleep = SingleTargetStart,
        Devotion,
        CurseItem,
        EnergyCoat,

        JupitelThunderBall,
        JupitelThunderExplosion,

        Windhit1,
        Windhit2,
        Windhit3,
        Windhit4,

        BluePotion,

        LevelUpBase,
        LevelUpJob,

        GroundTargetStart = 100, //remove this when mass adding effects later
        Stormgust = GroundTargetStart,
        MeteorStorm,
        Burnt,
        Icemine,

        Sort, //Use this just to find out what the effect is while generating
        Last
    }

    public enum CylinderEffectIDs : int
    {
        MagicPillarBlue = 0,
        MagicPillarRed,
        MagicPillarYellow,
        MagicPillarBlack,
        MagicPillarBrown,
        MagicPillarJade,
        MagicPillarPurple,
        MagicPillarWhite,

        WarpIn,
        WarpOut,

        Last
    }

    public enum PyramidEffectIds : int
    {
        IceWall = 0,

        Last
    }

    public enum FuncEffectIds : int
    {
        AsuraStrike = 0,
        SoulLink,
        JupitelThunder,

        Last
    }

    public static class EffectDb
    {
        public struct EffectData
        {
            public SoundDb.SoundIds soundId;
        }

        public struct CylEffectData
        {
            public Texture2D texture;
            public Color color1;
            public Color color2;
            public Color color3;
            public Color color4;
            public Vector4 bottomWidths;
            public Vector4 topWidths;
            public Vector4 minHeights;
            public Vector4 maxHeights;
            public Vector4 heightSpeed; //Also interpreted as wave speed for wave cylinders
            public Vector4 rotateSpeeds;
            public float fadeTime;
            public float defaultDuration;
            public Material[] materials; //0 is for area material, 1 for target material
            public SoundDb.SoundIds soundId;
        }

        public static readonly CylEffectData[] CylindersEffects = new CylEffectData[(int)CylinderEffectIDs.Last];
        public static readonly EffectData[] Effects = new EffectData[(int)EffectIDs.Last];

        static EffectDb()
        {
            #region cylinderData
            CylindersEffects[(int)CylinderEffectIDs.MagicPillarBlue] = new CylEffectData
            {
                texture = AssetBundleProvider.LoadEffectBundleTextureAsset("ring_blue"),
                color1 = new Color(1, 1, 1, 0.6f * 0.3f),
                color2 = new Color(1, 1, 1, 0.6f),
                color3 = new Color(1, 1, 1, 0.6f),
                color4 = new Color(1, 1, 1, 0.6f),
                bottomWidths = new Vector4(2.7f, 2.7f, 2.9f, 3.0f),
                topWidths = new Vector4(5.3f, 9.0f, 13.0f, 17.0f),
                minHeights = new Vector4(44, 4, 3, 2),
                maxHeights = new Vector4(220, 20, 15, 10),
                heightSpeed = new Vector4(75, 75, 75, 75),
                rotateSpeeds = new Vector4(0, 2, 2.5f, 3.5f),
                fadeTime = 0.5f,
                materials = new Material[] { AssetBundleProvider.LoadMiscBundleAsset<Material>("cylinder_4pass_area"),
                                             AssetBundleProvider.LoadMiscBundleAsset<Material>("cylinder_4pass_target") },
                soundId = SoundDb.SoundIds.BeginSpell
            };
            CylindersEffects[(int)CylinderEffectIDs.MagicPillarBlack] = CylindersEffects[(int)CylinderEffectIDs.MagicPillarBlue];
            CylindersEffects[(int)CylinderEffectIDs.MagicPillarBlack].texture = AssetBundleProvider.LoadEffectBundleTextureAsset("ring_black");
            CylindersEffects[(int)CylinderEffectIDs.MagicPillarRed] = CylindersEffects[(int)CylinderEffectIDs.MagicPillarBlue];
            CylindersEffects[(int)CylinderEffectIDs.MagicPillarRed].texture = AssetBundleProvider.LoadEffectBundleTextureAsset("ring_red");
            CylindersEffects[(int)CylinderEffectIDs.MagicPillarYellow] = CylindersEffects[(int)CylinderEffectIDs.MagicPillarBlue];
            CylindersEffects[(int)CylinderEffectIDs.MagicPillarYellow].texture = AssetBundleProvider.LoadEffectBundleTextureAsset("ring_yellow");
            CylindersEffects[(int)CylinderEffectIDs.MagicPillarBrown] = CylindersEffects[(int)CylinderEffectIDs.MagicPillarBlue];
            CylindersEffects[(int)CylinderEffectIDs.MagicPillarBrown].texture = AssetBundleProvider.LoadEffectBundleTextureAsset("ring_brown");
            CylindersEffects[(int)CylinderEffectIDs.MagicPillarJade] = CylindersEffects[(int)CylinderEffectIDs.MagicPillarBlue];
            CylindersEffects[(int)CylinderEffectIDs.MagicPillarJade].texture = AssetBundleProvider.LoadEffectBundleTextureAsset("ring_jadu");
            CylindersEffects[(int)CylinderEffectIDs.MagicPillarPurple] = CylindersEffects[(int)CylinderEffectIDs.MagicPillarBlue];
            CylindersEffects[(int)CylinderEffectIDs.MagicPillarPurple].texture = AssetBundleProvider.LoadEffectBundleTextureAsset("ring_purple");
            CylindersEffects[(int)CylinderEffectIDs.MagicPillarWhite] = CylindersEffects[(int)CylinderEffectIDs.MagicPillarBlue];
            CylindersEffects[(int)CylinderEffectIDs.MagicPillarWhite].texture = AssetBundleProvider.LoadEffectBundleTextureAsset("ring_white");

            CylindersEffects[(int)CylinderEffectIDs.WarpOut] = new CylEffectData
            {
                texture = AssetBundleProvider.LoadEffectBundleTextureAsset("ring_blue"),
                color1 = new Color(1, 1, 1, 0.67f),
                color2 = new Color(1, 1, 1, 0.4f),
                color3 = new Color(1, 1, 1, 0.2f),
                color4 = new Color(1, 1, 1, 0.1f),
                bottomWidths = new Vector4(0.5f, 1.5f, 2.2f, 2.8f),
                topWidths = new Vector4(0.5f, 1.5f, 2.2f, 2.8f),
                minHeights = new Vector4(10, 8, 6, 4),
                maxHeights = new Vector4(150, 110, 70, 30),
                heightSpeed = new Vector4(4, 4, 4, 4),
                rotateSpeeds = new Vector4(2, 2, 2, 2),
                fadeTime = 0,
                defaultDuration = 1.5f,
                materials = new Material[] { AssetBundleProvider.LoadMiscBundleAsset<Material>("cylinder_wave_4pass_area"),
                                             AssetBundleProvider.LoadMiscBundleAsset<Material>("cylinder_wave_4pass_target") },
                soundId = SoundDb.SoundIds.WarpOut
            };
            CylindersEffects[(int)CylinderEffectIDs.WarpIn] = new CylEffectData
            {
                texture = AssetBundleProvider.LoadEffectBundleTextureAsset("ring_blue"),
                color1 = new Color(1, 1, 1, 0.3f),
                color2 = new Color(1, 1, 1, 0.3f),
                color3 = new Color(1, 1, 1, 0.15f),
                color4 = Color.black,
                bottomWidths = new Vector4(2.5f, 3f, 3.5f, 0),
                topWidths = new Vector4(2.5f, 3f, 6, 0),
                minHeights = new Vector4(4, 4, 4, 0),
                maxHeights = new Vector4(45, 45, 4, 0),
                heightSpeed = new Vector4(8, 8, 1, 0),
                rotateSpeeds = new Vector4(10, 10, 8, 1),
                fadeTime = 0,
                defaultDuration = 0.8f,
                materials = new Material[] { AssetBundleProvider.LoadMiscBundleAsset<Material>("cylinder_wave_3pass_area"),
                                             AssetBundleProvider.LoadMiscBundleAsset<Material>("cylinder_wave_3pass_target") },
                soundId = SoundDb.SoundIds.WarpIn
            };

            #endregion

            #region normalEffectData
            Effects[(int)EffectIDs.LevelUpBase] = new EffectData { soundId = SoundDb.SoundIds.BaseLevelUp };
            Effects[(int)EffectIDs.LevelUpJob] = new EffectData { soundId = SoundDb.SoundIds.None };
            Effects[(int)EffectIDs.Stormgust] = new EffectData { soundId = SoundDb.SoundIds.StormGust };
            Effects[(int)EffectIDs.EnergyCoat] = new EffectData { soundId = SoundDb.SoundIds.None };
            Effects[(int)EffectIDs.JupitelThunderBall] = new EffectData { soundId = SoundDb.SoundIds.None };
            Effects[(int)EffectIDs.JupitelThunderExplosion] = new EffectData { soundId = SoundDb.SoundIds.None };
            #endregion
        }
    }
}
