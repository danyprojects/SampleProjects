using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Bacterio.Databases;
using Bacterio.Common;

namespace BacterioEditor
{
    public static class DatabaseGenerator
    {
        [Serializable]
        public class StructureDbSerialize
        {
            public List<StructureDb.StructureData> Structures;
            public List<string> Meshes;

            [NonSerialized] public string JsonName;
        }

        [Serializable]
        public class AuraDbSerialize
        {
            public List<AuraDb.AuraData> Auras;

            [NonSerialized] public string JsonName;
        }

        [Serializable]
        public struct EffectsDbSerialize
        {
            public List<EffectDb.TrailEffectData> TrailEffects;
            public List<EffectDb.ParticleEffectData> ParticleEffects;

            [NonSerialized] public string JsonName;
        }

        [Serializable]
        private struct TrapDbSerialize
        {
            public List<TrapDb.TrapData> Traps;

            [NonSerialized] public string JsonName;
        }

        [Serializable]
        private struct ShopDbSerialize
        {
            public List<ShopDb.ShopItemData> ShopItems;

            [NonSerialized] public string JsonName;
        }

        [Serializable]
        private struct BulletDbSerialize
        {
            public List<BulletDb.BulletData> Bullets;

            [NonSerialized] public string JsonName;
        }

        [Serializable]
        private struct CellDbSerialize
        {
            public List<CellDb.CellData> Cells;

            [NonSerialized] public string JsonName;
        }

        [MenuItem("Assets/Custom/Databases/Generate empty structure DB")]
        public static void GenerateEmptyStructureDbFile()
        {
            var data = new StructureDbSerialize();
            data.Structures = new List<StructureDb.StructureData>();

            foreach (var value in Enum.GetNames(typeof(StructureDbId)))
            {
                if (value == Enum.GetName(typeof(StructureDbId), StructureDbId.Last))
                    break;
                data.Structures.Add(new StructureDb.StructureData());
            }

            SaveJson(StructureDb.DB_FILE_NAME, data);
        }

        [MenuItem("Assets/Custom/Databases/Generate empty aura DB")]
        public static void GenerateEmptyAuraDbFile()
        {
            var data = new AuraDbSerialize();
            data.Auras = new List<AuraDb.AuraData>();

            foreach (var value in Enum.GetNames(typeof(AuraDbId)))
            {
                if (value == Enum.GetName(typeof(AuraDbId), AuraDbId.Last))
                    break;
                data.Auras.Add(new AuraDb.AuraData());
            }

            SaveJson(AuraDb.DB_FILE_NAME, data);
        }

        [MenuItem("Assets/Custom/Databases/Generate empty trap DB")]
        public static void GenerateEmptyTrapDbFile()
        {
            var data = new TrapDbSerialize();
            data.Traps = new List<TrapDb.TrapData>();

            foreach (var value in Enum.GetNames(typeof(TrapDbId)))
            {
                if (value == Enum.GetName(typeof(TrapDbId), TrapDbId.Last))
                    break;
                data.Traps.Add(new TrapDb.TrapData());
            }

            SaveJson(TrapDb.DB_FILE_NAME, data);
        }

        [MenuItem("Assets/Custom/Databases/Generate empty effects DB")]
        public static void GenerateEmptyEffectDbFile()
        {
            var data = new EffectsDbSerialize();
            data.TrailEffects = new List<EffectDb.TrailEffectData>();
            data.ParticleEffects = new List<EffectDb.ParticleEffectData>();

            data.TrailEffects.Add(new EffectDb.TrailEffectData() { effectId = TrailEffectId.MoveSpeedUp, gradient = new Gradient() });

            SaveJson(EffectDb.DB_FILE_NAME, data);
        }

        [MenuItem("Assets/Custom/Databases/Generate empty shop items DB")]
        public static void GenerateEmptyShopItemsDbFile()
        {
            var data = new ShopDbSerialize();
            data.ShopItems = new List<ShopDb.ShopItemData>();

            for (int i = 0; i <= (int)ShopItemId.Last; i++) 
                data.ShopItems.Add(new ShopDb.ShopItemData() {limit = Constants.SHOP_INFINITE_LIMIT });

            SaveJson(ShopDb.DB_FILE_NAME, data);
        }

        [MenuItem("Assets/Custom/Databases/Generate empty bullet DB")]
        public static void GenerateEmptyBulletDbFile()
        {
            var data = new BulletDbSerialize();
            data.Bullets = new List<BulletDb.BulletData>();

            for (int i = 0; i <= (int)BulletDbId.Last; i++)
                data.Bullets.Add(new BulletDb.BulletData() { });

            SaveJson(BulletDb.DB_FILE_NAME, data);
        }
       
        [MenuItem("Assets/Custom/Databases/Generate empty cell DB")]
        public static void GenerateEmptyCellDbFile()
        {
            var data = new CellDbSerialize();
            data.Cells = new List<CellDb.CellData>();

            for (int i = 0; i <= (int)CellDbId.Last; i++)
                data.Cells.Add(new CellDb.CellData() { });

            SaveJson(CellDb.DB_FILE_NAME, data);
        }
         
        public static StructureDbSerialize GetStructures()
        {
            Bacterio.AssetBundleProvider provider = new Bacterio.AssetBundleProvider();
            StructureDb db = new StructureDb(provider);
            provider.Dispose();

            //load structures
            var structuresData = new List<StructureDb.StructureData>();
            foreach (var value in Enum.GetNames(typeof(StructureDbId)))
            {
                if (value == "Last")
                    break;
                
                structuresData.Add(db.GetStructureData((StructureDbId)Enum.Parse(typeof(StructureDbId), value)));
            }

            //load names
            var names = new List<string>();
            for (int i = 0; i < db.TerritoryNameCount; i++)
                names.Add(db.GetTerritoryMeshName(i));

            return new StructureDbSerialize() { Structures = structuresData, Meshes = names, JsonName = StructureDb.DB_FILE_NAME };
        }

        public static EffectsDbSerialize GetEffects()
        {
            Bacterio.AssetBundleProvider provider = new Bacterio.AssetBundleProvider();
            var db = new EffectDb(provider);
            provider.Dispose();

            var trailEffectData = new List<EffectDb.TrailEffectData>();
            var particleEffectData = new List<EffectDb.ParticleEffectData>();

            foreach (var value in Enum.GetNames(typeof(TrailEffectId)))
            {
                if (value == "Last")
                    break;
                try
                {
                    trailEffectData.Add(db.GetTrailData((TrailEffectId)Enum.Parse(typeof(TrailEffectId), value)));
                }catch(Exception e)
                {
                    trailEffectData.Add(new EffectDb.TrailEffectData() { effectId = TrailEffectId.Invalid });
                }
            }

            foreach (var value in Enum.GetNames(typeof(ParticleEffectId)))
            {
                if (value == "Last")
                    break;
                try
                {
                    particleEffectData.Add(db.GetParticleData((ParticleEffectId)Enum.Parse(typeof(ParticleEffectId), value)));
                }catch(Exception e)
                {
                    particleEffectData.Add(new EffectDb.ParticleEffectData() { effectId = ParticleEffectId.Invalid });
                }
            }

            return new EffectsDbSerialize() { TrailEffects = trailEffectData, ParticleEffects = particleEffectData, JsonName = EffectDb.DB_FILE_NAME };
        }

        public static void SaveJson<T>(string dbFileName, T jsonObj, bool prettyPrint = true)
        {
            string path = Path.Combine(EditorStrings.DATABASE_PATH,dbFileName + ".json");
            var json = EditorJsonUtility.ToJson(jsonObj, prettyPrint);
            File.WriteAllText(path, json);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Assert(AssetDatabase.LoadAssetAtPath<TextAsset>(path), "Failed to create db file");

            var asset = AssetImporter.GetAtPath(path);
            asset.assetBundleName = "misc";
        }
    }
}
