using RO.Common;
using RO.Databases;
using RO.Media;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EditorTools
{
    public static class ItemDataGenerator
    {
		[Serializable]
		private struct ItemDataDeserialize
		{
			public ItemData[] Items;
		}

		[Serializable]
		public struct ItemData
		{
			public ItemType type;
			public EquipmentLocation loc;
			public WeaponShieldAnimatorIDs viewId;
			public ItemSpriteIDs sprId;
			public string name;
			public string desc;
		}

		[MenuItem("Assets/Extra/Miscellaneous/Generate all item data")]
        private static void GenerateItemDatas()
        {
            const string fullDb = "Assets/~Resources/Dbs/itemDb_foreditor.json";
			const string itemDataPath = "Assets/~Resources/GRF/ItemData";
			const string itemIconPath = "Assets/~Resources/GRF/Sprites/Items";
			const string itemViewPath = "Assets/~Resources/GRF/UI/ItemViewImages";

			if (!File.Exists(fullDb))
                throw new Exception("itemDb_foreditor.json not found");

			TextAsset jsonFile = AssetDatabase.LoadAssetAtPath<TextAsset>(fullDb);
			ItemData[] items = JsonUtility.FromJson<ItemDataDeserialize>(jsonFile.text).Items;

			//Empty item data dir and create it again
			if(Directory.Exists(itemDataPath))
				Directory.Delete(itemDataPath, true);
			Directory.CreateDirectory(itemDataPath);
			AssetDatabase.Refresh();

			int count = 0;
			foreach(var item in items)
            {
				RO.Containers.ItemData itemContainer = new RO.Containers.ItemData();
				try
				{
					//General item info
					itemContainer.id = count++;
					itemContainer.name = item.name;
					itemContainer.description = item.desc;

					//sprite info
					string iconPath = Path.Combine(itemIconPath, item.sprId.ToString(), ((int)item.sprId).ToString());
					itemContainer.icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath + "_0.png");
					itemContainer.palette = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath + "_p.png");
					itemContainer.viewImage = AssetDatabase.LoadAssetAtPath<Texture2D>(Path.Combine(itemViewPath, ((int)item.sprId).ToString() + "_v.bmp"));

					string dataPath = Path.Combine(itemDataPath, Enum.GetName(typeof(ItemIDs), itemContainer.id));
					Directory.CreateDirectory(dataPath);
					AssetDatabase.CreateAsset(itemContainer, Path.Combine(dataPath, itemContainer.id.ToString() + ".asset"));

					AssetImporter.GetAtPath(Path.Combine(dataPath, itemContainer.id.ToString() + ".asset")).SetAssetBundleNameAndVariant("itemData", "");
				}
				catch (Exception e)
                {
					Debug.Log(e.StackTrace);
                }
			}
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
    }
}
