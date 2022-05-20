using System.IO;
using UnityEditor;

public class CreateAssetBundles
{
    [MenuItem("Assets/Bundles/Build AssetBundles")]
    public static void BuildAllAssetBundles()
    {
        string assetBundleDirectory = "Assets/StreamingAssets";
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        var manifest = BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension, 
                                                        BuildTarget.StandaloneWindows);
    }

    [MenuItem("Assets/Bundles/Re-build AssetBundles")]
    public static void ReBuildAllAssetBundles()
    {
        string assetBundleDirectory = "Assets/StreamingAssets";
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }
        var manifest = BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension | BuildAssetBundleOptions.ForceRebuildAssetBundle,
                                                        BuildTarget.StandaloneWindows);
    }
}
