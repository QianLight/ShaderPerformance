using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MultiProcessBuild
{
    [System.Serializable]
    public class BuildJob
    {
        [System.Serializable]
        public class AssetBundleBuild
        {
            public string assetBundleName;
            public string[] assetNames;
            public int weight;
        }
        public string output;
        [SerializeField]
        public AssetBundleBuild[] builds;
        public int options;
        public int target;
        public int slaveID;
        public int weight;

        public UnityEngine.AssetBundleManifest Build()
        {
            if (!Directory.Exists(this.output))
                Directory.CreateDirectory(this.output);
            List<UnityEditor.AssetBundleBuild> builds = new List<UnityEditor.AssetBundleBuild>();
            foreach (var v in this.builds)
            {
                UnityEditor.AssetBundleBuild build = new UnityEditor.AssetBundleBuild();
                build.assetBundleName = v.assetBundleName;
                build.assetNames = v.assetNames;
                builds.Add(build);
            }
            return UnityEditor.BuildPipeline.BuildAssetBundles(this.output, builds.ToArray(), (BuildAssetBundleOptions)this.options, (BuildTarget)this.target);
        }
    }

    [System.Serializable]
    public class AssetBundleManifest
    {
        [System.Serializable]
        public class AssetBundleBuild
        {
            public string assetBundleName;
            public string[] dependency;
            public string hash;
        }
        [SerializeField]
        public AssetBundleBuild[] builds;
        public float buildTime;

        public string[] GetAllAssetBundles()
        {
            string[] result = new string[builds.Length];
            for (int i = 0; i < builds.Length; ++i)
                result[i] = builds[i].assetBundleName;
            return result;
        }
        public string[] GetAllAssetBundlesWithVariant() { return new string[0]; }
        public string[] GetAllDependencies(string assetBundleName)
        {
            HashSet<string> deps = new HashSet<string>();
            var direct = GetDirectDependencies(assetBundleName);
            deps.UnionWith(direct);
            foreach (var dep in direct)
                deps.UnionWith(GetAllDependencies(dep));
            List<string> sorted = new List<string>(deps);
            sorted.Sort();
            return sorted.ToArray();
        }
        public Hash128 GetAssetBundleHash(string assetBundleName)
        {
            var v = ArrayUtility.Find(builds, (a) => a.assetBundleName == assetBundleName);
            if (v == null)
                return Hash128.Parse("");
            return Hash128.Parse(v.hash);
        }
        public string[] GetDirectDependencies(string assetBundleName)
        {
            var v = ArrayUtility.Find(builds, (a) => a.assetBundleName == assetBundleName);
            if (v == null)
                return new string[0];
            return v.dependency;
        }
    }
}