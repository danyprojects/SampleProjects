using System.IO;
using UnityEngine;

namespace Bacterio
{

    public sealed class AssetBundleProvider : System.IDisposable
    {
        private readonly AssetBundle _ui, _objects, _misc;

        public AssetBundleProvider()
        {
            _ui = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "ui"));
            _objects = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "objects"));
            _misc = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "misc"));
        }


        public GameObject LoadUIScreenAsset(string name)
        {
            WDebug.Assert(_ui.Contains(name), "UI Asset bundle doesn't contain asset " + name);
            WDebug.Assert(_ui.LoadAsset<GameObject>(name) != null, "UI asset bundle contains object name: " + name + " but failed to load. Check if type is correct");
            return _ui.LoadAsset<GameObject>(name);
        }

        public T LoadUIImageAsset<T>(string name) where T : Object
        {
            WDebug.Assert(_ui.Contains(name), "UI Asset bundle doesn't contain asset " + name);
            WDebug.Assert(_ui.LoadAsset<T>(name) != null, "UI asset bundle contains object name: " + name + " but failed to load. Check if type is correct");
            return _ui.LoadAsset<T>(name);
        }

        public GameObject LoadObjectAsset(string name)
        {
            WDebug.Assert(_objects.Contains(name), "Objects Asset bundle doesn't contain asset " + name);
            WDebug.Assert(_objects.LoadAsset<GameObject>(name) != null, "Objects asset bundle contains object name: " + name + " but failed to load. Check if type is correct");
            return _objects.LoadAsset<GameObject>(name);
        }

        public T LoadMiscAsset<T>(string name) where T : Object
        {
            WDebug.Assert(_misc.Contains(name), "Misc Asset bundle doesn't contain asset " + name);
            WDebug.Assert(_misc.LoadAsset<T>(name) != null, "Misc asset bundle contains object name: " + name + " but failed to load. Check if type is correct");
            return _misc.LoadAsset<T>(name);
        }

        public void Dispose()
        {
            AssetBundle.UnloadAllAssetBundles(true);

            if (!Caching.ClearCache())
                WDebug.LogWarn("Failed to clear cache");
        }
    }

}