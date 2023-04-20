using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class CFHookManager
{
    private static readonly Dictionary<MethodInfo, MethodHook> hooks = new Dictionary<MethodInfo, MethodHook>();

    public static void Install(MethodInfo target, MethodInfo replace, MethodInfo proxy = null)
    {
        MethodHook mh = new MethodHook(target, replace, proxy);
        mh.Install();
        hooks[target] = mh;
    }

    public static void Uninstall(MethodInfo mi)
    {
        if (hooks.TryGetValue(mi, out MethodHook hook))
        {
            hook.Uninstall();
            hooks.Remove(mi);
        }
    }
}
