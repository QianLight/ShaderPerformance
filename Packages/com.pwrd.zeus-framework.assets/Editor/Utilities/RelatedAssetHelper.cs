/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Zeus.Framework.Asset;

public class RelatedAssetHelper
{
    public static void AddRelatedAsset(ref List<string> assetList)
    {
        IRelatedAsset relatedAssetRuler = null;
        var assembly = Assembly.Load("Assembly-CSharp-Editor");
        var types = assembly.GetTypes();
        foreach (var type in types)
        {
            if (!type.GetInterfaces().ToList().Contains(typeof(IRelatedAsset)))
                continue;
            relatedAssetRuler = Activator.CreateInstance(type) as IRelatedAsset;
            break;
        }

        if (relatedAssetRuler != null)
        {
            AssetBundleUtils.Init();
            var assetMapBundles = AssetBundleUtils.GetAssetMapBundles();
            var assetToAdd = new List<string>();
            var allAssetSet = new HashSet<string>();
            var currentAssetSet = new HashSet<string>();

            foreach (var pair in assetMapBundles)
            {
                allAssetSet.Add(pair.Key);
            }

            foreach (var asset in assetList)
            {
                currentAssetSet.Add(asset);
            }

            var addLog = new StringBuilder();
            addLog.AppendLine("添加了: ");
            foreach (var asset in assetList)
            {
                if (asset.StartsWith(AssetListHelper.ExtraPrefix))
                {
                    continue;
                }
                List<string> relativeAssetList = relatedAssetRuler.GetRelativeAssetName(asset);
                foreach (var relativeAsset in relativeAssetList)
                {
                    if (allAssetSet.Contains(relativeAsset) && !currentAssetSet.Contains(relativeAsset))
                    {
                        assetToAdd.Add(relativeAsset);
                        currentAssetSet.Add(relativeAsset);
                        addLog.AppendLine(asset);
                    }
                }
            }

            assetList.AddRange(assetToAdd);
            Debug.Log(addLog);
        }
    }
}