/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/

namespace Zeus.Framework.Asset
{
    public interface IBundleRef
    {
        void Retain();
        void Release(BundleAssetRef assetRef);
        string BundleName { get; }
    }
}

