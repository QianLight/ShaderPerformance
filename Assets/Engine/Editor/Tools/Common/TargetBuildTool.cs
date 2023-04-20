using System.IO;
using UnityEditor;

namespace CFEngine.Editor
{
    public class TargetBuildTool
    {
        [MenuItem("Tools/引擎/TargetBuild")]
        public static void BuildPrefab()
        {
            var o = Selection.activeObject;
            string path = AssetDatabase.GetAssetPath(o);
            
            AssetBundleBuild b = new AssetBundleBuild();
            b.assetNames = new[] { path };
            b.assetBundleName = o.name;
            
            AssetBundleBuild db = new AssetBundleBuild();
            var deps = AssetDatabase.GetDependencies(path, true);
            db.addressableNames = deps;
            db.assetBundleName = "deps";
            
            AssetBundleBuild[] bds = new[] { b, db };
            
            string folder = "Bundle";
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder);
            }
            Directory.CreateDirectory(folder);
            
            BuildPipeline.BuildAssetBundles(folder, bds, BuildAssetBundleOptions.ForceRebuildAssetBundle,
                BuildTarget.StandaloneWindows64);
        }
    }
}