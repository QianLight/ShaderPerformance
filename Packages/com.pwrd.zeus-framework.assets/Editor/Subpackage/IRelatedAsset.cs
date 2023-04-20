/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections.Generic;

namespace Zeus.Framework.Asset
{
    public interface IRelatedAsset
    {
        List<string> GetRelativeAssetName(string originAsset);
    }
}