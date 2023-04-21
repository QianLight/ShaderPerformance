using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace UnityEngine.AssetGraph
{
    internal class UnloadableBundleUtility
    {

        public static Dictionary<string, List<string>> GenerateBeDependencies(AssetBundleBuild[] builds, AssetBundleManifest manifest, Dictionary<string, List<string>> bundle2AtlasBundle)
        {
            var beDeps = new Dictionary<string, List<string>>();
            foreach (var build in builds)
            {
                var deps = manifest.GetAllDependencies(build.assetBundleName);
                foreach (var dep in deps)
                {
                    if (!beDeps.TryGetValue(dep, out var list))
                    {
                        beDeps[dep] = list = new List<string>();
                    }
                    list.Add(build.assetBundleName);
                }

                List<string> atlasList = null;
                if (bundle2AtlasBundle.TryGetValue(build.assetBundleName, out atlasList))
                {
                    foreach (var dep in atlasList)
                    {
                        if (!beDeps.TryGetValue(dep, out var list))
                        {
                            beDeps[dep] = list = new List<string>();
                        }
                        list.Add(build.assetBundleName);
                    }
                }
            }
            return beDeps;
        }

        public static List<string> GetAllUnloadableBundles(AssetBundleBuild[] builds, AssetBundleManifest manifest, Dictionary<string, List<string>> bundle2AtlasBundle)
        {
            var beDeps = GenerateBeDependencies(builds, manifest, bundle2AtlasBundle);
            var result = new List<string>();
            foreach (var build in builds)
            {
                if (IsNotOnlyOneAssetInBundle(build))
                {
                    continue;
                }
                var bundleName = build.assetBundleName;
                var assetName = build.assetNames[0];
                if (IsBeDependentByOtherBundles(bundleName, beDeps))
                {
                    continue;
                }
                var extension = Path.GetExtension(assetName);
                if (IsAssetNotUnloadableWithExtension(extension))
                {
                    continue;
                }
                if (IsAssetNotUnloadableWithImporterSetting(assetName))
                {
                    continue;
                }
                result.Add(bundleName);
            }
            return result;
        }

        public static bool IsNotOnlyOneAssetInBundle(AssetBundleBuild build)
        {
            return build.assetNames.Length != 1;
        }

        public static bool IsBeDependentByOtherBundles(string bundle, Dictionary<string, List<string>> beDeps)
        {
            return beDeps.ContainsKey(bundle);
        }

        public static bool IsAssetNotUnloadableWithExtension(string extension)
        {
            return extension == ".prefab" || extension == ".unity";
        }

        public static bool IsAssetNotUnloadableWithImporterSetting(string assetPath)
        {
            //assetPath = Path.Combine(m_assetPrefix, assetPath);
            var assetImporter = AssetImporter.GetAtPath(assetPath);
            if (assetImporter == null)
            {
                Debug.LogError($"no importer for {assetPath}");
                return true;
            }
            var texImporter = assetImporter as TextureImporter;
            if (texImporter != null && texImporter.streamingMipmaps)
            {
                return true;
            }
            var audioImporter = assetImporter as AudioImporter;
            if (audioImporter != null)
            {
                AudioImporterSampleSettings setting;
                switch (EditorUserBuildSettings.activeBuildTarget)
                {
                    case BuildTarget.Android:
                        setting = audioImporter.GetOverrideSampleSettings("Android");
                        break;
                    case BuildTarget.StandaloneWindows:
                    case BuildTarget.StandaloneOSX:
                        setting = audioImporter.GetOverrideSampleSettings("Standalone");
                        break;
                    case BuildTarget.iOS:
                        setting = audioImporter.GetOverrideSampleSettings("iOS");
                        break;
                    case BuildTarget.WebGL:
                        setting = audioImporter.GetOverrideSampleSettings("WebGL");
                        break;
                    default:
                        setting = audioImporter.defaultSampleSettings;
                        break;
                }
                if (setting.loadType == AudioClipLoadType.Streaming)
                {
                    return true;
                }
                else if(setting.loadType == AudioClipLoadType.CompressedInMemory )
                {
                    return true;
                }
            }
            return false;
        }
    }
}
