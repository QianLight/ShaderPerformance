using UnityEngine.Scripting;

namespace Zeus.Framework.Asset
{
    public interface IAssetLevelMapper
    {
        string Level
        {
            get;
        }

        string GetAssetPathForLevel(string defaultName);//, AssetLoaderType loaderType);
        string GetFinalAssetPathForLevel(string defaultName, AssetLoaderType loaderType);
        bool IsPathForThisLevel(string path);
    }

    [Preserve]
    public class AssetLevelMapperDefault : IAssetLevelMapper
    {
        public string Level => "Default";

        public virtual string GetAssetPathForLevel(string assetPath)//, AssetLoaderType loaderType)
        {
            return assetPath;
        }

        public string GetFinalAssetPathForLevel(string assetPath, AssetLoaderType loaderType)
        {
            return assetPath;
        }

        public bool IsPathForThisLevel(string path)
        {
            return true;
        }
    }

    [Preserve]
    public class AssetLevelMapperLow : IAssetLevelMapper
    {
        public string Level => "Low";
        

        public string GetAssetPathForLevel(string normalName)
        {
            return normalName + "_low";
        }

        public string GetFinalAssetPathForLevel(string assetPath, AssetLoaderType loaderType)
        {
            if(loaderType == AssetLoaderType.AssetBundle)
            {
                string newPath = GetAssetPathForLevel(assetPath);
                if (AssetBundleUtils.TryGetAssetBundleName(newPath, out _, out _))
                {
                    return newPath;
                }
                else
                {
                    return assetPath;
                }
            }
            else if(loaderType == AssetLoaderType.Resources)
            {
                string newPath = GetAssetPathForLevel(assetPath);
#if UNITY_EDITOR
#else
                UnityEngine.Debug.LogError("Resource mode dont support asset level mode !");
#endif
                return newPath;
            }
            else
            {
                string newPath = GetAssetPathForLevel(assetPath);
                return newPath;

            }
        }

        public bool IsPathForThisLevel(string path)
        {
#if UNITY_EDITOR
            var assetMapBundles = AssetBundleUtils.GetAssetMapBundles();
            if (path.EndsWith("_low") && assetMapBundles.ContainsKey(path) && assetMapBundles.ContainsKey(path.Substring(0,path.Length - "_low".Length)))
            {
                return true;
            }
#endif
            return false;
        }
    }
}
