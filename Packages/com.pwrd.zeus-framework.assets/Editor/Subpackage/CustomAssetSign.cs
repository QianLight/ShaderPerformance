/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections.Generic;

namespace Zeus.Framework.Asset
{
    public abstract class CustomAssetSign
    {
        public virtual string Prefix => null;
        public abstract List<string> GetAssetList(string originAssetStr);
    }
}