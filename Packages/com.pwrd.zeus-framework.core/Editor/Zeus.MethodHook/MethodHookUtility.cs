/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Zeus.MethodHook;
public class MethodHookUtility
{
    static MethodHookUtility()
    {
        MethodHook.IsUnity3D = true;
    }

#if UNSAFE
    /// <summary>
    /// 1.Unity中需要再Jit之前交换，Jit之后不再起作用
    /// 2.要交换的两个方法不要有互调情况，否则会陷入死循环。
    /// </summary>
    public static void Swap(MethodInfo source, MethodInfo destination)
    {
        MethodHook.Swap(source, destination);
    }
#endif
    public static void Hook(MethodInfo source, MethodInfo destination)
    {
        MethodHook.Hook(source, destination);
    }

    public static void UnHook(MethodInfo source)
    {
        MethodHook.UnHook(source);
    }

    public static void UnHookAll()
    {
        MethodHook.UnHookAll();
    }
}

