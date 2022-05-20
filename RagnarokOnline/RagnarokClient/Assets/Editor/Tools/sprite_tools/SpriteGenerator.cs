using GrfTools;
using RO.Common;
using RO.Containers;
using RO.Databases;
using RO.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EditorTools
{
    public class SpriteGenerator
    {
        public static float PIXELS_PER_UNIT = Constants.PIXELS_PER_UNIT;

        private static string currentPath;
        private static string destination;
        private static string actPath, sprPath, strPath;
        private static Material spriteBodyMat, spriteBodyZbufferMat, spritePartMat, spritePartZbufferMat, spriteWeapMat, spriteWeapZbufferMat;
        private static Material spriteMonsterMat, spriteMonsterZBufferMat;
        private static Material[] effectOpaqueMats, effectTransparentMats, effectSpriteMats;
        private static UnityEngine.Audio.AudioMixerGroup bgmGroup, effectGroup;

        private enum SortingLayers : int
        {
            Body = 1,
            Head = 2,
            Lower = 3,
            Middle = 4,
            Upper = 5,
            Shield = 6,
            WeaponTrajectory = 7,
            Weapon = 8,
            Item = 9,
            Effect = 10,
            OverlayEffects = 15
        }

        [MenuItem("Assets/Extra/GenerateSprites/Parse Act only")]
        private static void ParseActOnly()
        {
            ValidateSelection();

            GenerateSpritesFromActSpr<FloatingTextAnimator>((int)SortingLayers.OverlayEffects, LayerMask.NameToLayer("Misc"), "sprites/misc", false, false, null, true);
        }

        [MenuItem("Assets/Extra/GenerateSprites/All Soft/All")]
        private static void GenerateAllSpritesSoft()
        {
            GenerateAllBodySpritesSoft();
            GenerateAllHeadSpritesSoft();
            GenerateAllHeadgearsSpritesSoft();
            GenerateAllShieldsSpritesSoft();
            GenerateAllWeaponsSpritesSoft();
            GenerateAllMonstersSoft();
            GenerateAllItemsSoft();
            GenerateAllEffects();
        }

        [MenuItem("Assets/Extra/GenerateSprites/All Hard/All")]
        private static void GenerateAllSpritesHard()
        {
            GenerateAllBodySpritesHard();
            GenerateAllHeadSpritesHard();
            GenerateAllHeadgearsSpritesHard();
            GenerateAllsWeaponsSpritesHard();
            GenerateAllShieldsSpritesHard();
            GenerateAllMonstersHard();
            GenerateAllItemsHard();
            GenerateAllEffects();
        }

        [MenuItem("Assets/Extra/GenerateSprites/All Soft/Bodies")]
        private static void GenerateAllBodySpritesSoft()
        {
            GenerateSprite("Assets/~Resources/GRF/Sprites/Bodies/", GenerateCharacterBodySoft);
        }

        [MenuItem("Assets/Extra/GenerateSprites/All Hard/Bodies")]
        private static void GenerateAllBodySpritesHard()
        {
            GenerateSprite("Assets/~Resources/GRF/Sprites/Bodies/", GenerateCharacterBodyHard);
        }

        [MenuItem("Assets/Extra/GenerateSprites/All Soft/Heads")]
        private static void GenerateAllHeadSpritesSoft()
        {
            GenerateSprite("Assets/~Resources/GRF/Sprites/Heads/", GenerateCharacterHeadSoft);
        }

        [MenuItem("Assets/Extra/GenerateSprites/All Hard/Heads")]
        private static void GenerateAllHeadSpritesHard()
        {
            GenerateSprite("Assets/~Resources/GRF/Sprites/Heads/", GenerateCharacterHeadHard);
        }

        [MenuItem("Assets/Extra/GenerateSprites/All Soft/Headgears")]
        private static void GenerateAllHeadgearsSpritesSoft()
        {

            GenerateSprite(Path.Combine("Assets/~Resources/GRF/Sprites/Headgears/", "Top"), GenerateTopHeadgearSoft);
            GenerateSprite(Path.Combine("Assets/~Resources/GRF/Sprites/Headgears/", "Mid"), GenerateMiddleHeadgearSoft);
            GenerateSprite(Path.Combine("Assets/~Resources/GRF/Sprites/Headgears/", "Lower"), GenerateLowerHeadgearSoft);
        }

        [MenuItem("Assets/Extra/GenerateSprites/All Hard/Headgears")]
        private static void GenerateAllHeadgearsSpritesHard()
        {

            GenerateSprite(Path.Combine("Assets/~Resources/GRF/Sprites/Headgears/", "Top"), GenerateTopHeadgearHard);
            GenerateSprite(Path.Combine("Assets/~Resources/GRF/Sprites/Headgears/", "Mid"), GenerateMiddleHeadgearHard);
            GenerateSprite(Path.Combine("Assets/~Resources/GRF/Sprites/Headgears/", "Lower"), GenerateLowerHeadgearHard);
        }

        [MenuItem("Assets/Extra/GenerateSprites/All Soft/Weapons")]
        private static void GenerateAllWeaponsSpritesSoft()
        {
            GenerateSprite("Assets/~Resources/GRF/Sprites/Weapons/", GenerateWeaponSoft);
        }

        [MenuItem("Assets/Extra/GenerateSprites/All Soft/Shields")]
        private static void GenerateAllShieldsSpritesSoft()
        {
            GenerateSprite("Assets/~Resources/GRF/Sprites/Shields/", GenerateShieldSoft); 
        }

        [MenuItem("Assets/Extra/GenerateSprites/All Hard/Weapons")]
        private static void GenerateAllsWeaponsSpritesHard()
        {
            GenerateSprite("Assets/~Resources/GRF/Sprites/Weapons/", GenerateWeaponHard);
        }

        [MenuItem("Assets/Extra/GenerateSprites/All Hard/Shields")]
        private static void GenerateAllShieldsSpritesHard()
        {
            GenerateSprite("Assets/~Resources/GRF/Sprites/Shields/", GenerateShieldHard);
        }

        [MenuItem("Assets/Extra/GenerateSprites/All Soft/Monsters")]
        private static void GenerateAllMonstersSoft()
        {
            GenerateSprite("Assets/~Resources/GRF/Sprites/Monsters/", GenerateMonsterSoft);
        }

        [MenuItem("Assets/Extra/GenerateSprites/All Hard/Monsters")]
        private static void GenerateAllMonstersHard()
        {
            GenerateSprite("Assets/~Resources/GRF/Sprites/Monsters/", GenerateMonsterHard);
        }

        [MenuItem("Assets/Extra/GenerateSprites/All Soft/Items")]
        private static void GenerateAllItemsSoft()
        {
            GenerateSprite("Assets/~Resources/GRF/Sprites/Items/", GenerateItemSoft);
        }

        [MenuItem("Assets/Extra/GenerateSprites/All Hard/Items")]
        private static void GenerateAllItemsHard()
        {
            GenerateSprite("Assets/~Resources/GRF/Sprites/Items/", GenerateItemHard);
        }

        [MenuItem("Assets/Extra/GenerateSprites/All Hard/Effects")]
        private static void GenerateAllEffects()
        {
            GenerateSprite("Assets/~Resources/GRF/Sprites/Effects/", GenerateEffect);
        }

        [MenuItem("Assets/Extra/GenerateSprites/Part Soft/Character body")]
        private static void GenerateCharacterBodySoft()
        {
            ValidateSelection();
            string path = actPath.Substring(0, actPath.LastIndexOf('/'));
            GenerateCharacterBody(Directory.GetFiles(path).Length <= 4);
        }

        [MenuItem("Assets/Extra/GenerateSprites/Part Hard/Character body")]
        private static void GenerateCharacterBodyHard()
        {
            GenerateCharacterBody(true);
        }

        private static void GenerateCharacterBody(bool regenSpr = true)
        {
            ValidateSelection();
            string fName = Path.GetFileNameWithoutExtension(actPath);
            if (!Regex.IsMatch(fName, "[a-zA-Z]{4,}_(mount_)?[fm]"))
                throw new Exception("Invalid act file name. Format should be FileName_[f or m]");
            try
            {
                string name = ((int)Enum.Parse(typeof(Jobs), fName.Split('_')[0])).ToString() + "_" + fName.Substring(fName.IndexOf('_') + 1);
                GenerateSpritesFromActSpr<BodyAnimator>((int)SortingLayers.Body, LayerMask.NameToLayer("Player"), "sprites/characters", false, true, name, regenSpr);
            }
            catch (ArgumentException)
            {
                actPath = null;
                throw new Exception("Could not find " + fName + " on jobs enum");
            }
        }

        [MenuItem("Assets/Extra/GenerateSprites/Part Soft/Character head")]
        private static void GenerateCharacterHeadSoft()
        {
            ValidateSelection();
            string path = actPath.Substring(0, actPath.LastIndexOf('/'));
            GenerateCharacterHead(Directory.GetFiles(path).Length <= 4);
        }

        [MenuItem("Assets/Extra/GenerateSprites/Part Hard/Character head")]
        private static void GenerateCharacterHeadHard()
        {
            GenerateCharacterHead(true);
        }

        private static void GenerateCharacterHead(bool regenSpr = true)
        {
            ValidateSelection();
            GenerateSpritesFromActSpr<HeadAnimator>((int)SortingLayers.Head, LayerMask.NameToLayer("Player"), "sprites/heads", true, false, null, regenSpr);
        }

        [MenuItem("Assets/Extra/GenerateSprites/Part Soft/Top headgear")]
        private static void GenerateTopHeadgearSoft()
        {
            ValidateSelection();
            string path = actPath.Substring(0, actPath.LastIndexOf('/'));
            GenerateTopHeadgear(Directory.GetFiles(path).Length <= 4);
        }

        [MenuItem("Assets/Extra/GenerateSprites/Part Hard/Top headgear")]
        private static void GenerateTopHeadgearHard()
        {
            GenerateTopHeadgear(true);
        }

        private static void GenerateTopHeadgear(bool regenSpr = true)
        {
            ValidateSelection();
            string fName = Path.GetFileNameWithoutExtension(actPath);
            if (!Regex.IsMatch(fName, "[a-zA-Z]{2,}_[fm]"))
                throw new Exception("Invalid act file name. Format should be FileName_[f or m]");

            try
            {
                string name = ((int)Enum.Parse(typeof(ItemSpriteIDs), fName.Split('_')[0])).ToString() + "_" + fName.Split('_')[1];
                GenerateSpritesFromActSpr<EquipmentAnimator>((int)SortingLayers.Upper, LayerMask.NameToLayer("Player"), "sprites/headgears", true, false, name, regenSpr);
            }
            catch (ArgumentException)
            {
                actPath = null;
                throw new Exception("Could not find " + fName + " on items enum");
            }
        }

        [MenuItem("Assets/Extra/GenerateSprites/Part Soft/Middle headgear")]
        private static void GenerateMiddleHeadgearSoft()
        {
            ValidateSelection();
            string path = actPath.Substring(0, actPath.LastIndexOf('/'));
            GenerateMiddleHeadgear(Directory.GetFiles(path).Length <= 4);
        }

        [MenuItem("Assets/Extra/GenerateSprites/Part Hard/Middle headgear")]
        private static void GenerateMiddleHeadgearHard()
        {
            GenerateMiddleHeadgear(true);
        }

        private static void GenerateMiddleHeadgear(bool regenSpr = true)
        {
            ValidateSelection();
            string fName = Path.GetFileNameWithoutExtension(actPath);
            if (!Regex.IsMatch(fName, "[a-zA-Z]{2,}_[fm]"))
                throw new Exception("Invalid act file name. Format should be FileName_[f or m]");

            try
            {
                string name = ((int)Enum.Parse(typeof(ItemSpriteIDs), fName.Split('_')[0])).ToString() + "_" + fName.Split('_')[1];
                GenerateSpritesFromActSpr<EquipmentAnimator>((int)SortingLayers.Middle, LayerMask.NameToLayer("Player"), "sprites/headgears", true, false, name, regenSpr);
            }
            catch (ArgumentException)
            {
                actPath = null;
                throw new Exception("Could not find " + fName + " on items enum");
            }
        }

        [MenuItem("Assets/Extra/GenerateSprites/Part Soft/Lower headgear")]
        private static void GenerateLowerHeadgearSoft()
        {
            ValidateSelection();
            string path = actPath.Substring(0, actPath.LastIndexOf('/'));
            GenerateLowerHeadgear(Directory.GetFiles(path).Length <= 4);
        }

        [MenuItem("Assets/Extra/GenerateSprites/Part Hard/Lower headgear")]
        private static void GenerateLowerHeadgearHard()
        {
            GenerateLowerHeadgear(true);
        }

        private static void GenerateLowerHeadgear(bool regenSpr = true)
        {
            ValidateSelection();
            string fName = Path.GetFileNameWithoutExtension(actPath);
            if (!Regex.IsMatch(fName, "[a-zA-Z]{2,}_[fm]"))
                throw new Exception("Invalid act file name. Format should be FileName_[f or m]");

            try
            {
                string name = ((int)Enum.Parse(typeof(ItemSpriteIDs), fName.Split('_')[0])).ToString() + "_" + fName.Split('_')[1];
                GenerateSpritesFromActSpr<EquipmentAnimator>((int)SortingLayers.Lower, LayerMask.NameToLayer("Player"), "sprites/headgears", true, false, name, regenSpr);
            }
            catch (ArgumentException)
            {
                actPath = null;
                throw new Exception("Could not find " + fName + " on items enum");
            }
        }

        [MenuItem("Assets/Extra/GenerateSprites/Part Soft/Shield")]
        private static void GenerateShieldSoft()
        {
            ValidateSelection();
            string path = actPath.Substring(0, actPath.LastIndexOf('/'));
            GenerateShield(Directory.GetFiles(path).Length <= 4);
        }

        [MenuItem("Assets/Extra/GenerateSprites/Part Hard/Shield")]
        private static void GenerateShieldHard()
        {
            GenerateShield(true);
        }

        private static void GenerateShield(bool regenSpr = true)
        {
            ValidateSelection();
            string fName = Path.GetFileNameWithoutExtension(actPath);

            if (!Regex.IsMatch(fName, "[a-zA-Z]{2,}_[a-zA-Z]{2,}_[fm]"))
                throw new Exception("Invalid act file name. Format should be ShieldName_ClassName_[f or m]");

            string[] sections = fName.Split('_');
            try
            {
                string name = ((int)Enum.Parse(typeof(WeaponShieldAnimatorIDs), sections[0])).ToString();
                name += "_" + ((int)Enum.Parse(typeof(Jobs), sections[1])).ToString();
                name += sections.Contains("f") ? "_f" : "_m";
                int sortingLayer = (int)SortingLayers.Shield;
                GenerateSpritesFromActSpr<ShieldAnimator>(sortingLayer, LayerMask.NameToLayer("Player"), "sprites/shields", false, false, name, regenSpr);
            }
            catch (ArgumentException e)
            {
                actPath = null;
                Debug.LogWarning("Could not find " + fName + " on jobs or weapons enum" + e.StackTrace);
            }
        }

        [MenuItem("Assets/Extra/GenerateSprites/Part Soft/Weapon")]
        private static void GenerateWeaponSoft()
        {
            ValidateSelection();
            string path = actPath.Substring(0, actPath.LastIndexOf('/'));
            GenerateWeapon(Directory.GetFiles(path).Length <= 4);
        }

        [MenuItem("Assets/Extra/GenerateSprites/Part Hard/Weapon")]
        private static void GenerateWeaponHard()
        {
            GenerateWeapon(true);
        }

        private static void GenerateWeapon(bool regenSpr = true)
        {
            ValidateSelection();
            string fName = Path.GetFileNameWithoutExtension(actPath);

            if (!Regex.IsMatch(fName, "[a-zA-Z]{2,}[0-9]*_[a-zA-Z]{4,}_(mount_)?[fm](_t)?"))
                throw new Exception("Invalid act file name. Format should be WeaponName_ClassName_[optional mount_][f or m][Optional _t]");

            string[] sections = fName.Split('_');
            try
            {
                bool is2Hand = !Enum.GetNames(typeof(Jobs)).Contains(sections[1]);
                string name = ((int)Enum.Parse(typeof(WeaponShieldAnimatorIDs), is2Hand ? sections[0] + "_" + sections[1] : sections[0])).ToString();                
                name += "_" + ((int)Enum.Parse(typeof(Jobs), is2Hand ? sections[2] : sections[1])).ToString(); 
                name += sections.Contains("mount") ? "_mount" : "";
                name += sections.Contains("f") ? "_f" : "_m";
                name += sections.Contains("t") ? "_t" : "";
                int sortingLayer = sections.Contains("t") ? (int)SortingLayers.WeaponTrajectory : (int)SortingLayers.Weapon;
                GenerateSpritesFromActSpr<EquipmentAnimator>(sortingLayer, LayerMask.NameToLayer("Player"), "sprites/weapons", false, false, name, regenSpr);
            }
            catch (ArgumentException e)
            {
                actPath = null;
                Debug.LogWarning("Could not find " + fName + " on jobs or weapons enum" + e.StackTrace);
            }
        }

        [MenuItem("Assets/Extra/GenerateSprites/Other Soft/Monster")]
        private static void GenerateMonsterSoft()
        {
            ValidateSelection();
            string path = actPath.Substring(0, actPath.LastIndexOf('/'));
            GenerateMonster(Directory.GetFiles(path).Length <= 4);
        }

        [MenuItem("Assets/Extra/GenerateSprites/Other Hard/Monster")]
        private static void GenerateMonsterHard()
        {
            GenerateMonster(true);
        }

        private static void GenerateMonster(bool regenSpr = true)
        {
            ValidateSelection();
            string fName = Path.GetFileNameWithoutExtension(actPath);
            try
            {
                string name = ((int)Enum.Parse(typeof(MonsterSpriteIDs), fName)).ToString();
                GenerateSpritesFromActSpr<MonsterAnimator>((int)SortingLayers.Body, LayerMask.NameToLayer("Monster"), "sprites/monsters", false, true, name, regenSpr);
            }
            catch (ArgumentException)
            {
                actPath = null;
                throw new Exception("Could not find " + fName + " in monster db");
            }
        }

        [MenuItem("Assets/Extra/GenerateSprites/Other Soft/Item")]
        private static void GenerateItemSoft()
        {
            ValidateSelection();
            string path = actPath.Substring(0, actPath.LastIndexOf('/'));
            GenerateItem(Directory.GetFiles(path).Length <= 4);
        }

        [MenuItem("Assets/Extra/GenerateSprites/Other Hard/Item")]
        private static void GenerateItemHard()
        {
            GenerateItem(true);
        }

        private static void GenerateItem(bool regenSpr = true)
        {
            ValidateSelection();
            string fName = Path.GetFileNameWithoutExtension(actPath);
            try
            {
                string name = ((int)Enum.Parse(typeof(ItemSpriteIDs), fName)).ToString();
                GenerateSpritesFromActSpr<ItemAnimator>((int)SortingLayers.Item, LayerMask.NameToLayer("Item"), "sprites/items", false, false, name, regenSpr);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            catch (ArgumentException)
            {
                actPath = null;
                throw new Exception("Could not find " + fName + " in item db");
            }
        }

        [MenuItem("Assets/Extra/GenerateSprites/Other/Effect")]
        private static void GenerateEffect()
        {
            ValidateSelection();
            string fName;
            if (actPath != null)
                fName = Path.GetFileNameWithoutExtension(actPath);
            else
                fName = Path.GetFileNameWithoutExtension(strPath);
            try
            {
                EffectIDs effectID = (EffectIDs)Enum.Parse(typeof(EffectIDs), fName);
                string name = ((int)effectID).ToString();
                if (actPath != null)
                    GenerateSpritesFromActSpr<EffectAnimatorSprite>((int)SortingLayers.Effect, LayerMask.NameToLayer("Effect"), "sprites/effects", false, false, name, true);
                else
                    GenerateSpritesEffectFromStr((int)SortingLayers.Effect, "sprites/effects", name, effectID);
            }
            catch (Exception)
            {
                actPath = null;
                strPath = null;
                throw new Exception("Could not find " + fName + " in effect db");
            }
        }

        static void GenerateSpritesFromActSpr<AnimatorType>(int sortingLayerOrder, int objLayer, string bundleName, bool isHead, bool hasAudio, string name, bool regenSpr)
            where AnimatorType : MonoBehaviour
        {
            List<Sprite> files = new List<Sprite>();
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            //Load act file
            ActParser.ActFile actFile = ActParser.LoadAct(actPath, isHead);

            //Gets the target prefab name
            string objName = name;
            if (objName == null)
            {
                objName = actPath.Substring(actPath.LastIndexOf("/") + 1);
                objName = objName.Substring(0, objName.LastIndexOf("."));
            }
            //get directory of selected files
            currentPath = sprPath.Substring(0, sprPath.LastIndexOf("/"));

            //Don't clear sprites if we're not going to regen them
            if (regenSpr)
                EditorScriptsUtility.ClearFolder(currentPath, new string[] { ".act", ".spr", ".str" });
            else
                EditorScriptsUtility.ClearFolder(currentPath, new string[] { ".act", ".spr", ".str", ".png" });

            //Either Load sprites from .spr file and get list of file names or get them from folder
            List<string> sprs = null;
            if (regenSpr)
            {
                sprs = SprParser.LoadSpr(sprPath, Path.Combine(currentPath, objName));
            }
            else
            {
                //Get the correct sprite names
                sprs = SprParser.LoadSprNoSave(sprPath, Path.Combine(currentPath, objName));
                foreach (var file in Directory.GetFiles(currentPath, "*.png"))
                {
                    //Check if we need to rename
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    string oldName = fileName.Substring(0, fileName.LastIndexOf('_')); // get the name without the _XX.png and _p.png
                    if (oldName != name)
                    {
                        string newName = objName + fileName.Substring(fileName.LastIndexOf('_')) + ".png";
                        AssetDatabase.RenameAsset(file.Substring(file.IndexOf("Assets")), newName);
                    }
                }
            }
            RemoveUnusedSprites(sprs, actFile);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //Items only need palette and sprites
            if (sortingLayerOrder == (int)SortingLayers.Item)
                return;

            //Get the palette 
            Texture2D palette = null;
            if (sprs[sprs.Count - 1].Contains("_p."))
            {
                palette = AssetDatabase.LoadAssetAtPath<Texture2D>(sprs[sprs.Count - 1]);
                AssetImporter.GetAtPath(sprs[sprs.Count - 1]).SetAssetBundleNameAndVariant("palettes" + bundleName.Substring(bundleName.IndexOf("/")), "");
                sprs.RemoveAt(sprs.Count - 1);
            }

            //Construct the sprite list from list of sprite paths and assign them all to the bundle
            foreach (string palImagePath in sprs)
            {
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(palImagePath);
                files.Add(sprite);
                TextureImporter texture = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite));
                texture.SetAssetBundleNameAndVariant(bundleName, "");
            }

            bool isEffect = false;
            //Create the prefab that will be used to instantiate the object
            if (typeof(AnimatorType) == typeof(EffectAnimatorSprite))
            {
                isEffect = true;
                ActRemoveEmptyFrames(actFile);
            }

            GameObject spritePrefab = new GameObject(objName);

            if (isEffect || bundleName.Contains("monster") || sortingLayerOrder == (int)SortingLayers.Body)
            {
                var audio = spritePrefab.AddComponent<AudioSource>();
                audio.outputAudioMixerGroup = effectGroup;
                audio.rolloffMode = AudioRolloffMode.Linear;
                audio.minDistance = 1;
                audio.maxDistance = 80;
                audio.dopplerLevel = 0;
                audio.spread = 130;
                audio.spatialBlend = 0.7f;
                audio.pitch = 1;
            }
            if (!isEffect)
            {
                //Put a quad in the filter
                GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                spritePrefab.AddComponent<MeshFilter>().mesh = quad.GetComponent<MeshFilter>().sharedMesh;
                GameObject.DestroyImmediate(quad);

                //Set the renderer
                MeshRenderer meshRenderer = spritePrefab.AddComponent<MeshRenderer>();

                bool isCharacterPart = sortingLayerOrder >= (int)SortingLayers.Head && sortingLayerOrder <= (int)SortingLayers.Upper;
                bool isWeapon = sortingLayerOrder == (int)SortingLayers.Shield || sortingLayerOrder == (int)SortingLayers.Weapon || sortingLayerOrder == (int)SortingLayers.WeaponTrajectory;
                bool isBody = sortingLayerOrder == (int)SortingLayers.Body;
                meshRenderer.materials = isCharacterPart ? new Material[] { spritePartMat, spritePartZbufferMat } :
                                         isBody ? new Material[] { spriteBodyMat, spriteBodyZbufferMat } :
                                         isWeapon ? new Material[] { spriteWeapMat, spriteWeapZbufferMat } :
                                         /*IsMonster*/     new Material[] { spriteMonsterMat, spriteMonsterZBufferMat };

                meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                meshRenderer.allowOcclusionWhenDynamic = false;
                meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                meshRenderer.receiveShadows = false;

                meshRenderer.sortingLayerID = RO.Common.SortingLayers.BlockInt;
                meshRenderer.sortingLayerName = RO.Common.SortingLayers.BlockStr;
                meshRenderer.sortingOrder = 0;

                spritePrefab.layer = objLayer;
                var collider = spritePrefab.AddComponent<BoxCollider>();
                collider.isTrigger = true;
                collider.size = new Vector3(1, 1, 0.05f);
            }
            //Parse act file and load it into serializable scriptable object
            Act act = ScriptableObject.CreateInstance<Act>();
            files.Add(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/~Resources/Misc/empty_sprite.png")); // add the last element for -1 indexes   
            act.Sprites = files.ToArray();
            act.Palette = palette;
            act.OrderInLayer = sortingLayerOrder >= (int)SortingLayers.Head && sortingLayerOrder <= (int)SortingLayers.Weapon ? (sortingLayerOrder - 1) * -0.05f : 0;

            ActParser.ActFile actCpyAttach = null;
            //Weapons 
            if(sortingLayerOrder == (int)SortingLayers.Weapon || sortingLayerOrder == (int)SortingLayers.WeaponTrajectory)
                actCpyAttach = GetClassBodyAct();
            ParseAndFillActFile(act, actFile, ref act.Sprites, hasAudio, actCpyAttach);

            EditorUtility.SetDirty(spritePrefab);
            //Save the act scriptable object into an asset
            AssetDatabase.CreateAsset(act, Path.Combine(currentPath, objName + "_act.asset"));
            //Add the animator component to the prefab
            AnimatorType animator = spritePrefab.AddComponent<AnimatorType>();
            SetActInAnimator(ref animator, ref act, ref palette);

            //Save the object as a prefab
            PrefabUtility.SaveAsPrefabAssetAndConnect(spritePrefab, Path.Combine(currentPath, objName + ".prefab"), InteractionMode.AutomatedAction);
            EditorUtility.SetDirty(spritePrefab);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UnityEngine.Object.DestroyImmediate(spritePrefab);

            //Set the act file and prefab to the asset bundle
            AssetImporter.GetAtPath(Path.Combine(currentPath, objName + "_act.asset")).SetAssetBundleNameAndVariant(bundleName, "");
            AssetImporter.GetAtPath(Path.Combine(currentPath, objName + ".prefab")).SetAssetBundleNameAndVariant(bundleName, "");
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            sprPath = null;
            actPath = null;
        }

        static void GenerateSpritesEffectFromStr(int sortingLayerOrder, string bundleName, string name, EffectIDs effectId)
        {
            StrParser.StrFile strFile = StrParser.ParseStr(strPath);

            //Gets the target prefab name
            string objName = name;
            if (objName == null)
            {
                objName = strPath.Substring(strPath.LastIndexOf("/") + 1);
                objName = objName.Substring(0, objName.LastIndexOf("."));
            }
            //get directory of selected files
            currentPath = strPath.Substring(0, strPath.LastIndexOf("/"));
            EditorScriptsUtility.ClearFolder(currentPath, new string[3] { ".act", ".spr", ".str" });

            var pngs = new List<string>();
            foreach (string file in strFile.files)
                pngs.Add("Assets/~Resources/GRF/Sprites/Effects/pngs/" + file.Split('.')[0] + ".png");

            //Create the prefab that will be used to instantiate the object
            GameObject spritePrefab = new GameObject(objName);
            var audio = spritePrefab.AddComponent<AudioSource>();
            audio.outputAudioMixerGroup = effectGroup;
            audio.rolloffMode = AudioRolloffMode.Linear;
            audio.minDistance = 1;
            audio.maxDistance = 80;
            audio.dopplerLevel = 0;
            audio.spread = 130;
            audio.spatialBlend = 0.7f;
            audio.pitch = 1;

            Str str = ScriptableObject.CreateInstance<Str>();

            //Construct the sprite list from list of sprite paths and assign them all to the bundle
            List<Texture2D> sprites = new List<Texture2D>();
            foreach (string spritePath in pngs)
            {
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(spritePath);
                if (texture == null)
                {
                    Debug.Log("Texture " + texture + "doesn't exist in pngs");
                    continue;
                }
                sprites.Add(texture);
                TextureImporter textureImp = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture));
                textureImp.SetAssetBundleNameAndVariant(bundleName, "");
            }

            //Parse act file and load it into serializable scriptable object            
            ParseAndFillStrFile(str, strFile, sprites, effectId);

            EditorUtility.SetDirty(spritePrefab);
            //Save the act scriptable object into an asset
            AssetDatabase.CreateAsset(str, Path.Combine(currentPath, objName + "_str.asset"));
            var animator = spritePrefab.AddComponent<EffectAnimatorMesh>();
            animator.__Str = str;
            EditorUtility.SetDirty(str);

            //Save the object as a prefab
            PrefabUtility.SaveAsPrefabAssetAndConnect(spritePrefab, Path.Combine(currentPath, objName + ".prefab"), InteractionMode.AutomatedAction);
            EditorUtility.SetDirty(spritePrefab);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UnityEngine.Object.DestroyImmediate(spritePrefab);

            //Set the act file and prefab to the asset bundle
            AssetImporter.GetAtPath(Path.Combine(currentPath, objName + "_str.asset")).SetAssetBundleNameAndVariant(bundleName, "");
            AssetImporter.GetAtPath(Path.Combine(currentPath, objName + ".prefab")).SetAssetBundleNameAndVariant(bundleName, "");
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            strPath = null;
        }

        static void ValidateSelection()
        {
            spriteBodyMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/~Resources/Shaders/Units/materials/sprite_body.mat");
            spriteBodyZbufferMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/~Resources/Shaders/Units/materials/sprite_bodyZWrite.mat");
            spritePartMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/~Resources/Shaders/Units/materials/sprite_head.mat");
            spritePartZbufferMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/~Resources/Shaders/Units/materials/sprite_headZWrite.mat");
            spriteWeapMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/~Resources/Shaders/Units/materials/sprite_weap.mat");
            spriteWeapZbufferMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/~Resources/Shaders/Units/materials/sprite_weapZWrite.mat");
            spriteMonsterMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/~Resources/Shaders/Units/materials/sprite_monster.mat");
            spriteMonsterZBufferMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/~Resources/Shaders/Units/materials/sprite_monsterZWrite.mat");

            effectOpaqueMats = new Material[2];
            effectTransparentMats = new Material[2];
            effectSpriteMats = new Material[2];
            effectOpaqueMats[0] = AssetDatabase.LoadAssetAtPath<Material>("Assets/~Resources/Shaders/Effects/materials/effect_opaque_no_BB.mat");
            effectOpaqueMats[1] = AssetDatabase.LoadAssetAtPath<Material>("Assets/~Resources/Shaders/Effects/materials/effect_opaque_BB.mat");
            effectTransparentMats[0] = AssetDatabase.LoadAssetAtPath<Material>("Assets/~Resources/Shaders/Effects/materials/effect_transparent_no_BB.mat");
            effectTransparentMats[1] = AssetDatabase.LoadAssetAtPath<Material>("Assets/~Resources/Shaders/Effects/materials/effect_transparent_BB.mat");
            effectSpriteMats[0] = AssetDatabase.LoadAssetAtPath<Material>("Assets/~Resources/Shaders/Effects/materials/effect_sprite_no_BB.mat");
            effectSpriteMats[1] = AssetDatabase.LoadAssetAtPath<Material>("Assets/~Resources/Shaders/Effects/materials/effect_sprite_BB.mat");

            var mixer = GameObject.Find("SoundController").GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer;

            if (mixer == null)
                throw new Exception("Audio mixer not found");
            if (spriteBodyMat == null)
                throw new Exception("Body sprite material not found");
            if (spriteWeapMat == null)
                throw new Exception("Weapon sprite material not found");
            if (spritePartMat == null)
                throw new Exception("Head sprite material not found");
            if (spriteMonsterMat == null)
                throw new Exception("Monster sprite material not found");
            if (spriteBodyZbufferMat == null)
                throw new Exception("Body zwrite sprite material not found");
            if (spriteWeapZbufferMat == null)
                throw new Exception("Weapon zwrite sprite material not found");
            if (spritePartZbufferMat == null)
                throw new Exception("Head zwrite sprite material not found");
            if (spriteMonsterZBufferMat == null)
                throw new Exception("Monster zwrite sprite material not found");
            if (effectSpriteMats[0] == null || effectSpriteMats[1] == null)
                throw new Exception("Effect sprite materials not found");
            if (effectOpaqueMats[0] == null || effectOpaqueMats[1] == null)
                throw new Exception("Effect opaque materials not found");
            if (effectTransparentMats[0] == null || effectTransparentMats[1] == null)
                throw new Exception("Effect transparent materials not found");

            if (Selection.objects.Length > 2)
                throw new Exception("Select .act and .spr or .str only");

            //Get act and spr path and check if they're valid
            if (Selection.objects.Length == 2)
            {
                actPath = AssetDatabase.GetAssetPath(Selection.objects[Array.FindIndex(Selection.objects, (obj) =>
                {
                    return obj.GetType().Name == "DefaultAsset" && AssetDatabase.GetAssetPath(obj).Contains(".act");
                })]);

                sprPath = AssetDatabase.GetAssetPath(Selection.objects[Array.FindIndex(Selection.objects, (obj) =>
                {
                    return obj.GetType().Name == "DefaultAsset" && AssetDatabase.GetAssetPath(obj).Contains(".spr");
                })]);
            }
            else
                strPath = AssetDatabase.GetAssetPath(Selection.objects[Array.FindIndex(Selection.objects, (obj) =>
                {
                    return obj.GetType().Name == "DefaultAsset" && AssetDatabase.GetAssetPath(obj).Contains(".str");
                })]);

            if (strPath == null && actPath == null && sprPath == null)
                throw new Exception("Files not valid");
            if (strPath != null && strPath.Length == 0)
                throw new Exception("Files not valid");
            if ((actPath != null && actPath.Length == 0) || (sprPath != null && sprPath.Length == 0))
                throw new Exception("Files not valid");

            bgmGroup = mixer.FindMatchingGroups("Background")[0];
            effectGroup = mixer.FindMatchingGroups("Effects")[0];
        }

        static void SetActInAnimator<T>(ref T animator, ref Act act, ref Texture2D palette)
        {
            Type type = animator.GetType();
            if (type == typeof(BodyAnimator))
                (animator as BodyAnimator).__Act = act;
            if (type == typeof(HeadAnimator))
                (animator as HeadAnimator).__Act = act;
            if (type == typeof(EquipmentAnimator))
                (animator as EquipmentAnimator).__Act = act;
            if (type == typeof(ShieldAnimator))
                (animator as ShieldAnimator).__Act = act;
            if (type == typeof(MonsterAnimator))
                (animator as MonsterAnimator).__Act = act;
            if (type == typeof(EffectAnimatorSprite))
            {
                (animator as EffectAnimatorSprite).__Act = act;
                (animator as EffectAnimatorSprite).__Materials = effectSpriteMats;
            }
        }

        static void ParseAndFillActFile(Act anim, ActParser.ActFile actFile, ref Sprite[] sprites, bool hasAudio, ActParser.ActFile actCpyAttach = null)
        {
            anim.Actions = new Act.Action[actFile.actions.Length];
            if (hasAudio)
                anim.Events = new string[actFile.events.Length];
            else
                anim.Events = new string[0];

            for (int i = 0; i < anim.Events.Length; i++)
            {
                anim.Events[i] = new string(actFile.events[i].eventName.ToCharArray());
                if (anim.Events[i].Contains("."))
                    anim.Events[i] = anim.Events[i].Split('.')[0];
            }

            anim.MaxSprites = 0;
            for (int i = 0; i < actFile.actions.Length; i++)
            {
                anim.Actions[i].Frames = new Act.Action.Frame[actFile.actions[i].motions.Length];
                anim.Actions[i].delay = actFile.delays[i];
                for (int k = 0; k < anim.Actions[i].Frames.Length; k++)
                {
                    int count = 0; //Count how many valid sprites we have inside each frame
                    foreach (ActParser.ActSprClip sprClip in actFile.actions[i].motions[k].sprClips)
                        if (sprClip.sprNo != -1 || (sprClip.sprNo == -1 && actFile.actions[i].motions[k].sprClips.Length == 1))
                            count++;

                    if (anim.MaxSprites < count)
                        anim.MaxSprites = count;

                    //Assign the attach point and sprites to the frame. If act has no attach points and a actCpy was given, use those instead
                    Act.Action.Frame frame = new Act.Action.Frame();
                    if (actFile.actions[i].motions[k].attachpoints != null && actFile.actions[i].motions[k].attachpoints.Length > 0)
                        frame.attachPoint = new Vector2((float)actFile.actions[i].motions[k].attachpoints[0].x / PIXELS_PER_UNIT, (float)actFile.actions[i].motions[k].attachpoints[0].y / PIXELS_PER_UNIT);
                    else if (actCpyAttach != null && actCpyAttach.actions[i].motions[k % actCpyAttach.actions[i].motions.Length].attachpoints.Length > 0)
                        frame.attachPoint = new Vector2((float)actCpyAttach.actions[i].motions[k % actCpyAttach.actions[i].motions.Length].attachpoints[0].x / PIXELS_PER_UNIT, 
                                                        (float)actCpyAttach.actions[i].motions[k % actCpyAttach.actions[i].motions.Length].attachpoints[0].y / PIXELS_PER_UNIT);
                    
                    frame.frameData = new Act.Action.Frame.FrameData(count);
                    frame.eventId = actFile.actions[i].motions[k].eventId;

                    //A frame can have multiple sprites (like dark breath)
                    count = 0;
                    foreach (ActParser.ActSprClip sprClip in actFile.actions[i].motions[k].sprClips)
                        if (sprClip.sprNo != -1 || (sprClip.sprNo == -1 && actFile.actions[i].motions[k].sprClips.Length == 1))
                        {
                            float xOffset = 0, yOffset = 0;
                            if (sprClip.sprNo != -1)
                            {
                                xOffset = sprites[sprClip.sprNo].texture.width % 2 == 0 ? 0.1f : 0f;
                                yOffset = sprites[sprClip.sprNo].texture.height % 2 == 0 ? 0.1f : 0f;
                            }
                            frame.frameData.SpriteId[count] = sprClip.sprNo == -1 ? anim.Sprites.Length - 1 : sprClip.sprNo;
                            frame.frameData.PositionOffset[count] = new Vector2((float)sprClip.x / PIXELS_PER_UNIT + xOffset, (float)sprClip.y / PIXELS_PER_UNIT + yOffset);
                            frame.frameData.IsMirrored[count] = sprClip.mirrorOn == 0 ? 1f : -1f;
                            frame.frameData.Scale[count] = new Vector2(sprClip.xZoom, sprClip.yZoom);
                            frame.frameData.Color[count] = sprClip.color;
                            frame.frameData.Rotation[count] = sprClip.angle;
                            count++;
                        }
                    anim.Actions[i].Frames[k] = frame;
                }
            }
        }

        static void ParseAndFillStrFile(Str str, StrParser.StrFile strFile, List<Texture2D> sprites, EffectIDs effectId)
        {
            str.Layers = new Str.Layer[strFile.layers.Length];
            str.TotalFrames = (int)strFile.fps; // We also need to render frames [0, strFile.fps]
            for (int i = 0; i < strFile.layers.Length; i++)
            {
                str.Layers[i].Frames = new Str.Layer.Frame[strFile.layers[i].frames.Length];
                str.Layers[i].Textures = new Texture2D[strFile.layers[i].textures.Length];
                for (int k = 0; k < strFile.layers[i].frames.Length; k++)
                {
                    str.Layers[i].Frames[k].frameIndex = (int)strFile.layers[i].frames[k].frameIndex;
                    str.Layers[i].Frames[k].textureId = (int)strFile.layers[i].frames[k].textureId;
                    for (int p = 0; p < strFile.layers[i].textures.Length; p++)
                    {
                        str.Layers[i].Textures[p] = sprites.Find((Texture2D texture) =>
                        {
                            return strFile.layers[i].textures[p].name.Contains(texture.name);
                        });
                    }
                    str.Layers[i].Frames[k].offset = new Vector2((strFile.layers[i].frames[k].offset.x) / PIXELS_PER_UNIT,
                                                                 (strFile.layers[i].frames[k].offset.y) / PIXELS_PER_UNIT);
                    //Apply the keyframe offset change to effects that have it 
                    if (k % 2 == 0)
                        str.Layers[i].Frames[k].offset.y += EffectInfo.Offsets[(int)effectId];

                    str.Layers[i].Frames[k].delay = strFile.layers[i].frames[k].animDelay;
                    str.Layers[i].Frames[k].animationType = (int)strFile.layers[i].frames[k].animType;
                    str.Layers[i].Frames[k].color = strFile.layers[i].frames[k].color;
                    str.Layers[i].Frames[k].rotation = -strFile.layers[i].frames[k].rotation;
                    str.Layers[i].Frames[k].type = (int)strFile.layers[i].frames[k].type;
                    //  str.Layers[i].Frames[k].uvs = new Vector2[4];
                    str.Layers[i].Frames[k].vertices = new Vector3[4];
                    str.Layers[i].Frames[k].materials = strFile.layers[i].frames[k].destAlpha == 0 ? effectOpaqueMats : effectTransparentMats;

                    for (int p = 0; p < 2; p++)
                    {
                        // str.Layers[i].Frames[k].uvs[p] = strFile.layers[i].frames[k].uvs[p];
                        str.Layers[i].Frames[k].vertices[p] = new Vector3(strFile.layers[i].frames[k].vertices[p].x / PIXELS_PER_UNIT,
                                                                          -strFile.layers[i].frames[k].vertices[p].y / PIXELS_PER_UNIT, 0);
                    }
                    for (int p = 2; p < 4; p++)
                    {
                        // str.Layers[i].Frames[k].uvs[5 - p] = strFile.layers[i].frames[k].uvs[p];
                        str.Layers[i].Frames[k].vertices[5 - p] = new Vector3(strFile.layers[i].frames[k].vertices[p].x / PIXELS_PER_UNIT,
                                                                          -strFile.layers[i].frames[k].vertices[p].y / PIXELS_PER_UNIT, 0);
                    }
                    var aux = str.Layers[i].Frames[k].vertices[0];
                    str.Layers[i].Frames[k].vertices[0] = str.Layers[i].Frames[k].vertices[2];
                    str.Layers[i].Frames[k].vertices[2] = aux;
                    aux = str.Layers[i].Frames[k].vertices[1];
                    str.Layers[i].Frames[k].vertices[1] = str.Layers[i].Frames[k].vertices[3];
                    str.Layers[i].Frames[k].vertices[3] = aux;
                }
            }
        }

        static void GenerateSprite(string folderPath, Action generateFunction)
        {
            int count = 0;
            List<UnityEngine.Object> objs = new List<UnityEngine.Object>();
            string[] ignore = new string[] { "bmps" };
            //Check if folder is in ignore list

            foreach (string directoryPath in Directory.GetDirectories(folderPath))
            {
                string folderName = directoryPath.Substring(directoryPath.LastIndexOf('/'));
                if (Array.Find(ignore, (element) => folderName.Contains(element)) != null)
                    continue;

                if (Directory.GetDirectories(directoryPath).Length > 0)
                    GenerateSprite(directoryPath, generateFunction);
                EditorUtility.DisplayProgressBar("Processing file", directoryPath, count / (float)Directory.GetDirectories(folderPath).Length);
                string[] files = Directory.GetFiles(directoryPath);
                foreach (string file in Array.FindAll(files, (string file) => { return (file.Contains(".act") || file.Contains(".spr") || file.Contains(".str")) && !file.Contains(".meta"); }))
                    objs.Add(AssetDatabase.LoadAssetAtPath<DefaultAsset>(file));
                Selection.objects = objs.ToArray();
                objs.Clear();
                Debug.Log("Processing " + directoryPath);
                try
                {
                    generateFunction();
                    count++;
                }
                catch (Exception)
                {
                    Debug.Log("Failed to generate sprite at: " + directoryPath);
                    continue;
                }
            }
            EditorUtility.ClearProgressBar();
        }

        static void RemoveUnusedSprites(List<string> sprites, ActParser.ActFile actFile)
        {
            bool[] used = new bool[sprites.Count];

            //Mark the used sprites
            for (int i = 0; i < actFile.actions.Length; i++)
                for (int k = 0; k < actFile.actions[i].motions.Length; k++)
                    for (int g = 0; g < actFile.actions[i].motions[k].sprClips.Length; g++)
                        if (actFile.actions[i].motions[k].sprClips[g].sprNo > -1)
                            used[actFile.actions[i].motions[k].sprClips[g].sprNo] = true;

            int[] offset = new int[sprites.Count];
            for (int i = 0; i < used.Length; i++)
            {
                if (used[i] || sprites[i - offset[i]].Contains("_p"))
                    continue;
                AssetDatabase.DeleteAsset(sprites[i - offset[i]]);
                sprites.RemoveAt(i - offset[i]);
                for (int k = i; k < used.Length; k++)
                    offset[k]++;
            }

            for (int i = 0; i < actFile.actions.Length; i++)
                for (int k = 0; k < actFile.actions[i].motions.Length; k++)
                    for (int g = 0; g < actFile.actions[i].motions[k].sprClips.Length; g++)
                        if (actFile.actions[i].motions[k].sprClips[g].sprNo > -1)
                            actFile.actions[i].motions[k].sprClips[g].sprNo -= offset[actFile.actions[i].motions[k].sprClips[g].sprNo];
        }

        static void ActRemoveEmptyFrames(ActParser.ActFile act)
        {
            if (act.actions.Length > 1)
                Debug.LogWarning("Effect act file had more than 1 action. Only using first");
            List<ActParser.ActMotion> motions = new List<ActParser.ActMotion>();
            for (int i = 0; i < act.actions[0].motions.Length; i++)
                if (act.actions[0].motions[i].sprClips.Length > 0)
                    motions.Add(act.actions[0].motions[i]);

            var motion = new ActParser.ActMotion();
            motion.sprClips = new ActParser.ActSprClip[0];
            motion.attachpoints = new ActParser.ActAttachPoint[0];
            motion.range1 = new int[0];
            motion.range2 = new int[0];
            motions.Add(motion);
            act.actions[0].motions = motions.ToArray();
        }

        static ActParser.ActFile GetClassBodyAct()
        {
            string fName = Path.GetFileNameWithoutExtension(actPath);
            string[] sections = fName.Split('_');
            string name = Enum.GetNames(typeof(Jobs)).Contains(sections[1]) ? sections[1] : sections[2];
            name += sections.Contains("mount") ? "_mount" : "";
            name += sections.Contains("f") ? "_f" : "_m";
            return ActParser.LoadAct(Path.Combine("Assets/~Resources/GRF/Sprites/Bodies/", name, name + ".act"), false);
        }
    }
}