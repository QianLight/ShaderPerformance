/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus.Framework.Asset
{
    interface IResourceCollector
    {
        void Collect(Dictionary<string, ResourceAssetRef> cachedAssetDict);
        void CollectAll(Dictionary<string, ResourceAssetRef> cachedAssetDict);
    }
}

