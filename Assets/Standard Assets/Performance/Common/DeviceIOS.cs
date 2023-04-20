using System;
using System.Collections;
using System.Collections.Generic;
using CFClient.GSDK;
using CFEngine;
using CFUtilPoolLib;
using UnityEngine;

public class DeviceIOS : IDeviceLevel
{
    private IOSDevice.RowData[] table;

    private IOSDevice.RowData[] Table
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

    private IOSDevice.RowData[] Readtable()
    {
        XTableAsyncLoader loader = XTableAsyncLoader.Get();
        IOSDevice device = CFAllocator.Allocate<IOSDevice>();
        loader.AddTask(@"Table/IOSDevice", device);
        loader.Execute();
        return device?.Table;
    }

    private bool IsMatch(string gpuName, IOSDevice.RowData device)
    {
        gpuName = gpuName.ToLower();
        bool match = gpuName == device.GPUName;
        Debug.LogWarning("gpuName:" + gpuName + "           device.GPUName:" + device.GPUName);
        if (!match)
        {
            Debug.LogError("Can Not Match Device!");
        }

        return match;
    }

    private int GetDeviceLevel()
    {

        var level = 0;
        var gpuName = SystemInfo.graphicsDeviceName.Replace(' ', '-');
#if !UNITY_EDITOR
#if UNITY_IOS

        //if (String.IsNullOrEmpty(cpuName))
        //{
        //    Debug.LogError("failed get cpuName from sdk");
        //}

        if (Table == null)
        {
            Debug.LogError("cant read IOSDevice Table");
            return level;
        }

        for (int i = 0; i < Table.Length; ++i)
        {
            if (IsMatch(gpuName,Table[i]))
            {
                level = Table[i].DeviceLevel;
                break;
            }
        }


        if (level == 0)
        {
            Debug.LogError($"device match failed devicename:{SystemInfo.deviceName} gpu:{gpuName}");
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