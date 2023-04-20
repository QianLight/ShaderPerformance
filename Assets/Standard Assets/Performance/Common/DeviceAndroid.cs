using System;
using System.Collections;
using System.Collections.Generic;
using CFClient.GSDK;
using CFEngine;
using CFUtilPoolLib;
using UnityEngine;

public class DeviceAndroid : IDeviceLevel
{
    private AndroidDevice.RowData[] table;

    private AndroidDevice.RowData[] Table
    {
        get
        {
            if (table == null)
            {
                table = Readtable();
            }

            return table;
        }
    }

    private AndroidDevice.RowData[] Readtable()
    {
        XTableAsyncLoader loader = XTableAsyncLoader.Get();
        AndroidDevice device = CFAllocator.Allocate<AndroidDevice>();
        loader.AddTask(@"Table/AndroidDevice", device);
        loader.Execute();
        return device?.Table;
    }

    private bool IsMatch(string cpuName, string gpuName, AndroidDevice.RowData device)
    {
        cpuName = cpuName.ToLower();
        gpuName = gpuName.ToLower();
        var match = false;

        if (gpuName.StartsWith("adreno"))
        {
            match = gpuName.EndsWith(device.GPUMainModel);
        }

        if (gpuName.StartsWith("mali"))
        {
            match = cpuName == device.CPUName;
        }

        if (!match)
        {
            match = cpuName == device.CPUName;
        }

        if (!match)
        {
            match = gpuName.Contains(device.GPUKind) && gpuName.Contains(device.GPUMainModel) &&
                    gpuName.Contains(device.GPUSecondModel);
        }

        return match;
    }

    private int GetDeviceLevel()
    {

        var level = 0;
        var gpuName = SystemInfo.graphicsDeviceName.Replace(' ', '-');
        var cpuName = "";
#if !UNITY_EDITOR
#if UNITY_ANDROID
        try
        {
            AndroidJavaClass unityPluginLoader = new AndroidJavaClass("com.deviceinfo.DeviceInfo");
            cpuName = unityPluginLoader.CallStatic<string>("GetCPU")?.ToLower().Replace(' ', '-');
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }

        if (String.IsNullOrEmpty(cpuName))
        {
            Debug.LogError("failed get cpuName from sdk");
        }

        if (Table == null)
        {
            Debug.LogError("cant read AndroidDevice Table");
            return level;
        }

            for (int i = 0; i < Table.Length; ++i)
            {
                if (IsMatch(cpuName,gpuName,Table[i]))
                {
                    level = Table[i].DeviceLevel;
                    break;
                }
            }


        if (level == 0)
        {
            Debug.LogError($"device match failed cpu:{cpuName} gpu:{gpuName}");
        }
#endif
#endif

        return level;
    }

    public void SetDeviceLevelHandler(System.Action<int> onDeviceLevel)
    {
        var level = GetDeviceLevel();
        onDeviceLevel(level);
    }
}