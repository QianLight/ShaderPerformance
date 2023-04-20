/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus.Framework.Asset
{
    class DefaultResourceCollector : IResourceCollector
    {
        private int count = 0;
        private List<ResourceAssetRef> unloadAssetList = new List<ResourceAssetRef>();

        public void Collect(Dictionary<string, ResourceAssetRef> cachedAssetDict)
        {
            count++;
            if (count % 30 == 0)
            {
                count = 0;
                CollectInternal(cachedAssetDict);
            }
        }

        public void CollectAll(Dictionary<string, ResourceAssetRef> cachedAssetDict)
        {
            CollectInternal(cachedAssetDict);
        }

        private void CollectInternal(Dictionary<string, ResourceAssetRef> cachedAssetDict)
        {
            unloadAssetList.Clear();
            foreach (KeyValuePair<string, ResourceAssetRef> pair in cachedAssetDict)
            {
                if (pair.Value.RefCount <= 0)
                {
                    unloadAssetList.Add(pair.Value);
                }
            }
            for (int i = 0; i < unloadAssetList.Count; i++)
            {
                cachedAssetDict.Remove(unloadAssetList[i].AssetPath);
                //Debug.Log("Unload: " + unloadAssetList[i].AssetPath);
            }
            if (unloadAssetList.Count > 0)
            {
                //Resources.UnloadUnusedAssets();
            }
        }
    }
}

