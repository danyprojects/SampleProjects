using System;
using System.IO;
using UnityEditor;


public class EditorScriptsUtility
{
    public static void SortBuildScenes()
    {
        var scenes = EditorBuildSettings.scenes;
        var sorted = new EditorBuildSettingsScene[scenes.Length];

        sorted[0] = scenes[0];
        int index = 1;
        foreach (string mapName in Enum.GetNames(typeof(RO.Databases.MapDb.MapIds)))
        {
            if (mapName == "None")
                continue;
            int i;
            for (i = 1; i < scenes.Length; i++)
                if (Path.GetFileNameWithoutExtension(scenes[i].path) == mapName)
                    break;
            if (i == scenes.Length)
            {
                UnityEngine.Debug.LogWarning("Scene " + mapName + " doesn't exist");
                continue;
            }
            sorted[index] = scenes[i];
            index++;
        }

        //In case we had duplicates or anything
        if (sorted.Length > index)
        {
            var shrinked = new EditorBuildSettingsScene[index];
            System.Array.Copy(sorted, shrinked, shrinked.Length);
            EditorBuildSettings.scenes = shrinked;
            return;
        }

        EditorBuildSettings.scenes = sorted;
    }

    public static void ClearFolder(string folderPath, string[] excludes)
    {
        foreach (string file in Directory.GetFiles(folderPath))
        {
            bool contains = false;
            if (excludes != null)
                foreach (string exclude in excludes)
                    if (file.Contains(exclude))
                    {
                        contains = true;
                        break;
                    }
            if (!contains)
                AssetDatabase.DeleteAsset(file);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}

