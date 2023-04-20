/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Zeus.Framework.Asset
{
    public class SubpackageEditorUtil
    {
        public bool IsBundleBeIncluded(string srcBundleName, string[] tags)
        {
            AssetBundleUtils.Init();
            AssetListHelper.Refresh();
            List<string> assetList = AssetListHelper.LoadAssetListFromFiles(tags);
            Debug.Log("AssetList Length: " + assetList.Count);
            Dictionary<string, string> assetMapBundles = AssetBundleUtils.GetAssetMapBundles();
            foreach (string asset in assetList)
            {
                string bundle;
                if (!assetMapBundles.TryGetValue(asset, out bundle))
                {
                    Debug.Log(string.Format("Cannot get asset \"{0}\" in assetMapBundles!", asset));
                }
                else
                {
                    if (bundle == srcBundleName)
                    {
                        return true;
                    }
                    string[] dependencies = AssetBundleUtils.GetDirectDependencies(bundle);
                    if (dependencies != null && dependencies.Length > 0)
                    {
                        foreach (string depBundle in dependencies)
                        {
                            if (depBundle == srcBundleName)
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("Can't get dependency of bundle \"" + bundle + "\" .");
                    }
                }
            }
            return false;
        }

        public List<string> GetAssetList(string srcBundleName)
        {
            AssetBundleUtils.Init();
            Dictionary<string, List<string>> bundleMapAssets = AssetBundleUtils.GetBundleMapAssets();
            HashSet<string> assetSet = new HashSet<string>();
            foreach(string bundle in AssetBundleUtils.GetAllAssetBundles())
            {
                foreach (string depBundle in AssetBundleUtils.GetDirectDependencies(bundle))
                {
                    if (depBundle == srcBundleName)
                    {
                        List<string> m_assetList;
                        if (bundleMapAssets.TryGetValue(bundle, out m_assetList))
                        {
                            foreach (string asset in m_assetList)
                            {
                                assetSet.Add(asset);
                            }
                        }
                        else
                        {
                            Debug.LogError("Cannot get bundle \"" + depBundle + "\" in _bundleMapAssets.");
                        }
                    }
                }
            }
            return new List<string>(assetSet);
        }

        public void GenerateURLListOfSubpackage(string desDirectory, List<string> urlList)
        {
            int index = 0;
            foreach(string url in urlList)
            {
                string destFilePath = Path.Combine(desDirectory, "CDN预热列表" + index++.ToString() + ".txt");
                Zeus.Core.FileUtil.EnsureFolder(destFilePath);
                using (FileStream fw = File.Create(destFilePath))
                {
                    using (StreamWriter sw = new StreamWriter(fw))
                    {
                        SubPackageBundleInfoContainer container = SubPackageBundleInfoContainer.LoadSubPackageInfo(AssetLevelManager.Instance.GetDefaultLevel());
                        foreach (string file in container.ChunkNames)
                        {
                            string line = Zeus.Core.UriUtil.CombineUri(url, file);
                            sw.WriteLine(line);
                        }
                    }
                }
            }
        }
    }
}
