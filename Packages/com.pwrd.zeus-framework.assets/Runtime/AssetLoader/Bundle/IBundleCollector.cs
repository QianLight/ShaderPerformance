/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections.Generic;

namespace Zeus.Framework.Asset
{
    public interface IBundleCollector
    {
        void Collect(Dictionary<string, BundleRef> cachedBundleDict, Dictionary<string, BundleAssetRef> cachedAssetDict);
        void CollectAll(Dictionary<string, BundleRef> cachedBundleDict, Dictionary<string, BundleAssetRef> cachedAssetDict);
        void OnRefCountToZero(BundleRef bundleRef);
        void SetCachedBundleLifeCycle(float lifeCycle);
        void SetCachedBundleLimit(int limit);
    }
}

