/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus.Framework.Asset
{
    [System.Serializable]
    class DefaultBundleCollectorSetting
    {
        public int maxGcNumPerFrame;
        //缓存的最大bundle数量
        public int cacheBundleCount;
        //缓存bundle最大生命周期，超过该限制也会被释放
        public int cacheBundleLifeCycle;

        public DefaultBundleCollectorSetting()
        {
            maxGcNumPerFrame = -1;
            cacheBundleCount = 0;
            cacheBundleLifeCycle = 3;
        }
    }
}

