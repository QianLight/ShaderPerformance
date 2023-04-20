/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public static class HotfixLogger
{
    [ConditionalAttribute("ZEUS_HOTFIX")]
    public static void Log(params object[] list)
    {
        Zeus.Core.Logger.Log("Hotfix", list);
    }

    [ConditionalAttribute("ZEUS_HOTFIX")]
    public static void LogError(params object[] list)
    {
        Zeus.Core.Logger.LogError("Hotfix", list);
    }
}
