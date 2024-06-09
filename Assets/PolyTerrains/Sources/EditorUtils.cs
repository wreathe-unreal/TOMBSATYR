#if UNITY_EDITOR
using UnityEditor;
using Object = UnityEngine.Object;


namespace PolyTerrains.Sources
{
    public static class EditorUtils
    {
        public static T CreateOrReplaceAssetHard<T>(T asset, string path) where T : Object
        {
            if (!AssetDatabase.LoadAssetAtPath<T>(path))
                AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
    }
}

#endif