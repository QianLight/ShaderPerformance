/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

namespace Zeus.Framework.Asset
{
    public static class AssetsLogger
    {
        [ConditionalAttribute("ZEUS_ASSETS_LOG")]
        public static void Log(params object[] list)
        {
            Zeus.Core.Logger.Log("Assets", list);
        }

        [ConditionalAttribute("ZEUS_ASSETS_LOG")]
        public static void LogError(params object[] list)
        {
            Zeus.Core.Logger.LogError("Assets", list);
        }

        [ConditionalAttribute("ZEUS_ASSETS_LOG")]
        public static void LogWarning(params object[] list)
        {
            Zeus.Core.Logger.LogWarning("Assets", list);
        }
    }
}

