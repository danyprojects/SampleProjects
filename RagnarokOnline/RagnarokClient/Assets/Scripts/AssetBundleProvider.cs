using RO.Common;
using System.IO;
using UnityEngine;

public abstract class AssetBundleProvider
{
    private static readonly AssetBundle bodies, heads, headgears, shields, weapons;
    private static readonly AssetBundle monsters, effects, itemSprites, itemData;
    private static readonly AssetBundle bodyPalettes, headgearPalettes, headPalettes, shieldPalettes, weaponPalettes;
    private static readonly AssetBundle monsterPalettes, effectPalettes, itemPalettes;
    private static readonly AssetBundle mapObjects, mapsTextures, mapMisc;
    private static readonly AssetBundle ui, minimaps, misc, cursors;
    private static readonly AssetBundle sounds;

    static AssetBundleProvider()
    {
        cursors = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "cursors"));
        misc = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "etc"));

        //These are dependencies for other bundles so load them first
        bodyPalettes = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "palettes/characters"));
        headgearPalettes = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "palettes/headgears"));
        headPalettes = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "palettes/heads"));
        weaponPalettes = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "palettes/weapons"));
        shieldPalettes = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "palettes/shields"));
        monsterPalettes = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "palettes/monsters"));
        effectPalettes = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "palettes/effects"));
        itemPalettes = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "palettes/items"));
        itemSprites = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "sprites/items"));

        bodies = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "sprites/characters"));
        heads = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "sprites/heads"));
        headgears = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "sprites/headgears"));
        weapons = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "sprites/weapons"));
        shields = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "sprites/shields"));
        monsters = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "sprites/monsters"));
        effects = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "sprites/effects"));

        mapsTextures = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "maps/textures"));
        mapObjects = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "maps/maps"));
        mapMisc = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "maps/misc"));

        ui = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "ui"));
        minimaps = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "minimaps"));
        sounds = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "sounds"));

        itemData = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "itemData"));
    }

    public static T LoadUiBundleAsset<T>(string name) where T : Object
    {
        return ui.LoadAsset<T>(name);
    }

    public static Sprite LoadMiniMapBundleAsset(string name)
    {
        return minimaps.LoadAsset<Sprite>(name);
    }

    public static AudioClip LoadSoundBundleAsset(string name)
    {
        return sounds.LoadAsset<AudioClip>(name);
    }

    public static T LoadMiscBundleAsset<T>(string name) where T : Object
    {
        return misc.LoadAsset<T>(name);
    }

    public static T LoadBodyBundleAsset<T>(Jobs job, Gender gender, bool isMounted) where T : Object
    {
        return bodies.LoadAsset<T>(((int)job).ToString() + (isMounted ? "_mount" : "") + (gender == Gender.Male ? "_m" : "_f"));
    }

    public static T LoadHairstyleBundleAsset<T>(int hairstyleId, Gender gender) where T : Object
    {
        return heads.LoadAsset<T>(hairstyleId.ToString() + (gender == Gender.Male ? "_m" : "_f"));
    }

    public static T LoadHeadgearBundleAsset<T>(int headgearId, Gender gender) where T : Object
    {
        return headgears.LoadAsset<T>(headgearId.ToString() + (gender == Gender.Male ? "_m" : "_f"));
    }

    public static T LoadWeaponBundleAsset<T>(int spriteId, Jobs _class, Gender gender, bool isMounted) where T : Object
    {
        return weapons.LoadAsset<T>(spriteId.ToString() + "_" + ((int)_class).ToString() + (isMounted ? "_mount" : "") + (gender == Gender.Male ? "_m" : "_f"));
    }

    public static T LoadWeaponBundleAsset<T>(int spriteId, Jobs _class, Gender gender, bool isMounted, bool isTrajectory) where T : Object //trajectory overload for const string
    {
        return weapons.LoadAsset<T>(spriteId.ToString() + "_" + ((int)_class).ToString() + (isMounted ? "_mount" : "") + (gender == Gender.Male ? "_m_t" : "_f_t"));
    }

    public static T LoadShieldBundleAsset<T>(int shieldId, Jobs _class, Gender gender) where T : Object
    {
        return shields.LoadAsset<T>(shieldId + "_" + ((int)_class).ToString() + (gender == Gender.Male ? "_m" : "_f"));
    }

    public static T LoadMonsterBundleAsset<T>(int monsterSpriteId) where T : Object
    {
        return monsters.LoadAsset<T>(monsterSpriteId.ToString());
    }

    public static T LoadEffectBundleAsset<T>(int effectId) where T : Object
    {
        return effects.LoadAsset<T>(effectId.ToString());
    }

    public static Texture2D LoadEffectBundleTextureAsset(string textureName)
    {
        return effects.LoadAsset<Texture2D>(textureName);
    }

    public static RO.Containers.ItemData LoadItemDataAsset(RO.Databases.ItemIDs itemId)
    {
        return itemData.LoadAsset<RO.Containers.ItemData>(((int)itemId).ToString());
    }

    public static void GetWaterBundleTextures(int type, out Texture2D[] waterTextures)
    {
        waterTextures = new Texture2D[Constants.MAX_WATER_TEXTURES];
        string waterType = "water" + type;
        for (int i = 0; i < Constants.MAX_WATER_TEXTURES; i++)
            waterTextures[i] = mapMisc.LoadAsset<Texture2D>(waterType + (i <= 9 ? "0" : "") + i.ToString());
    }
}
