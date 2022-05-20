using RO.Databases;
using System;
using UnityEditor;
using UnityEngine;

namespace RO.UI
{
    [CustomEditor(typeof(BuffPanelController))]
    public class BuffPanelEditor : Editor
    {
        SerializedProperty buffIcons;

        void OnEnable()
        {
            buffIcons = serializedObject.FindProperty("_buffIcons");
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("update buff icons"))
            {
                var controller = target as BuffPanelController;

                buffIcons.arraySize = (int)BuffIDs.Last;
                string pngPath = "Assets/~Resources/GRF/UI/BuffIcons/";

                int i = 0;
                foreach (var buff in Enum.GetNames(typeof(BuffIDs)))
                {
                    if (buff == "FirstDebuff" || buff == "Last" || buff == "FirstBuff")
                        continue;

                    var path = pngPath + buff + ".tga";

                    var importer = AssetImporter.GetAtPath(path);
                    if (importer != null)
                    {
                        TextureImporter texImporter = (TextureImporter)importer;
                        texImporter.textureType = TextureImporterType.Sprite;
                        texImporter.spritePixelsPerUnit = 5;
                        texImporter.mipmapEnabled = false;
                        texImporter.filterMode = FilterMode.Point;

                        importer.SetAssetBundleNameAndVariant("ui", "");
                        importer.SaveAndReimport();

                        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

                        buffIcons.GetArrayElementAtIndex(i).objectReferenceValue = sprite;
                    }
                    i++;
                }

                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}