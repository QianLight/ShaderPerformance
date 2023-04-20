/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Zeus.Framework.Asset
{
    [Serializable]
    internal class DumpInfo
    {
        public AssetLoaderType assetLoaderType;
        public List<CachedAsset> cachedAssetList;
        public List<CachedBundle> cachedBundleList;

        public DumpInfo(AssetLoaderType type)
        {
            assetLoaderType = type;
            cachedAssetList = new List<CachedAsset>();
            cachedBundleList = new List<CachedBundle>();
        }

    }

    [Serializable]
    internal class CachedAsset
    {
        public string path;
        public string bundleName;
        public int refCount;

        public CachedAsset(string path, string bundleName, int refCount)
        {
            this.path = path;
            this.bundleName = bundleName;
            this.refCount = refCount;
        }
    }

    [Serializable]
    internal class CachedBundle
    {
        public string name;
        List<string> depBundleList;
        List<string> assetList;
        public int refCount;

        public CachedBundle(string name, List<string> depBundleList, List<string> assetList, int refCount)
        {
            this.name = name;
            this.depBundleList = depBundleList;
            this.assetList = assetList;
            this.refCount = refCount;
        }
    }

    internal class AssetDumpHelper
    {   
        public const string SnapshotExtension = ".zeusassetsnap";
        private const string SnapshotFolder = "AssetSnapshots";

        public static void GenerateDumpSnapshotFile(Dictionary<string, BundleAssetRef> cachedAssetDic, Dictionary<string, BundleRef> cachedBundleDic, string dir = "")
        {
            string fileNameWithoutExtension = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ffff");
            DumpInfo info = new DumpInfo(AssetLoaderType.AssetBundle);
            foreach(var pair in cachedAssetDic)
            {
                info.cachedAssetList.Add(new CachedAsset(pair.Value.AssetPath, pair.Value.BundleName, pair.Value.RefCount));
            }
            foreach(var pair in cachedBundleDic)
            {
                List<string> depList = new List<string>(AssetBundleUtils.GetAllDependencies(pair.Value.BundleName));
                List<string> assetPaths = new List<string>();
                foreach(var asset in pair.Value.AssetRefList)
                {
                    assetPaths.Add(asset.AssetPath);
                }
                info.cachedBundleList.Add(new CachedBundle(pair.Value.BundleName, depList, assetPaths, pair.Value.RefCount));
            }
            string content = JsonUtility.ToJson(info, true);
            fileNameWithoutExtension = info.assetLoaderType.ToString() + "_" + fileNameWithoutExtension;
            string path;
            if (string.IsNullOrEmpty(dir))
            {
                path = Zeus.Core.FileSystem.OuterPackage.GetRealPath(Path.Combine(SnapshotFolder, fileNameWithoutExtension));
                Zeus.Core.FileUtil.EnsureFolder(path);
            }
            else
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                path = Path.Combine(dir, fileNameWithoutExtension);
            }
            ScreenCapture.CaptureScreenshot(path + ".png");
            File.WriteAllText(path + SnapshotExtension, content);
        }

        public static void GenerateDumpSnapshotFile(Dictionary<string, ResourceAssetRef> cachedAssetDic, string dir = "")
        {
            string fileNameWithoutExtension = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ffff");
            DumpInfo info = new DumpInfo(AssetLoaderType.Resources);
            foreach (var pair in cachedAssetDic)
            {
                info.cachedAssetList.Add(new CachedAsset(pair.Value.AssetPath, string.Empty, pair.Value.RefCount));
            }
            string content = JsonUtility.ToJson(info, true);
            fileNameWithoutExtension = info.assetLoaderType.ToString() + "_" + fileNameWithoutExtension;
            string path;
            if (string.IsNullOrEmpty(dir))
            {
                path = Zeus.Core.FileSystem.OuterPackage.GetRealPath(Path.Combine(SnapshotFolder, fileNameWithoutExtension));
                Zeus.Core.FileUtil.EnsureFolder(path);
            }
            else
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                path = Path.Combine(dir, fileNameWithoutExtension);
            }
            ScreenCapture.CaptureScreenshot(path + ".png");
            File.WriteAllText(path + SnapshotExtension, content);
        }

        public static void GenerateDumpSnapshotFile(Dictionary<string, EditorAssetRef> cachedAssetDic, string dir = "")
        {
            string fileNameWithoutExtension = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ffff");
            DumpInfo info = new DumpInfo(AssetLoaderType.AssetDatabase);
            foreach (var pair in cachedAssetDic)
            {
                info.cachedAssetList.Add(new CachedAsset(pair.Value.AssetPath, string.Empty, pair.Value.RefCount));
            }
            string content = JsonUtility.ToJson(info, true);
            fileNameWithoutExtension = info.assetLoaderType.ToString() + "_" + fileNameWithoutExtension;
            string path;
            if (string.IsNullOrEmpty(dir))
            {
                path = Zeus.Core.FileSystem.OuterPackage.GetRealPath(Path.Combine(SnapshotFolder, fileNameWithoutExtension));
                Zeus.Core.FileUtil.EnsureFolder(path);
            }
            else
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                path = Path.Combine(dir, fileNameWithoutExtension);
            }
            ScreenCapture.CaptureScreenshot(path + ".png");
            File.WriteAllText(path + SnapshotExtension, content);
        }

#if UNITY_EDITOR
        public static string EditorSnapshotDirectory = "OuterPackage/" + SnapshotFolder;

        public static DumpInfo LoadDumpInfoFromFile(string path)
        {
            if(string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("The Argument \"path\" is null or empty.");
            }
            string content = File.ReadAllText(path);
            return JsonUtility.FromJson<DumpInfo>(content);
        }

        public static void ClearAllSnapshotFiles()
        {
            var snapDirectory = Directory.GetFiles(EditorSnapshotDirectory);
            foreach (var file in snapDirectory)
            {
                File.Delete(file);
            }
        }
#endif
    }
}