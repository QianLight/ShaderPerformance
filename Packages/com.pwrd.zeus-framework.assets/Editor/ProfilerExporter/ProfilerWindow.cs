/*******************************************************************
* Copyright © 2017—2022 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

public static class ProfilerWindow
{
    private static List<Reflector> _windows;

    private static Reflector _GetWindow(ProfilerArea area)
    {
        if (null == _windows)
        {
            var type = new Reflector(typeof(EditorWindow).Assembly.GetType("UnityEditor.ProfilerWindow"));
            var list = type.PrivateStaticField<IList>("m_ProfilerWindows");
            _windows = new List<Reflector>();
            foreach (var window in list)
            {
                _windows.Add(new Reflector(window));
            }
        }
        foreach (var reflector in _windows)
        {
            var val = (ProfilerArea)reflector.PrivateInstanceField("m_CurrentArea");
            if (val == area)
            {
                return reflector;
            }
        }
        return null;
    }

    public static MemoryElement GetMemoryDetailRoot()
    {
        var windowReflector = _GetWindow(ProfilerArea.Memory);
        if (windowReflector == null) return null;
        var memoryProfilerReflector = new Reflector((windowReflector.PrivateInstanceField("m_ProfilerModules") as object[])?[3]);
        var listViewReflector = new Reflector(memoryProfilerReflector.PrivateInstanceField("m_MemoryListView"));
        var rootReflector = listViewReflector.PrivateInstanceField("m_Root");
        return rootReflector != null ? MemoryElement.Create(new Reflector(rootReflector)) : null;
    }

    public static void RefreshMemoryData()
    {
        var reflector = _GetWindow(ProfilerArea.Memory);
        if (null != reflector)
        {
            reflector.CallPrivateInstanceMethod("RefreshMemoryData");
        }
        else
        {
            Debug.Log("请打开Profiler 窗口的 Memory 视图");
        }
    }
}