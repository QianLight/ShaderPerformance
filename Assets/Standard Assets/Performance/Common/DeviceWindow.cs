using System;
using System.Collections;
using System.Collections.Generic;
using CFClient.GSDK;
using CFEngine;
using CFUtilPoolLib;
using UnityEngine;

public class DeviceWindow : IDeviceLevel
{
    private WindowDevice.RowData[] table;
    
    private WindowDevice.RowData[] Table
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
    private WindowDevice.RowData[]  Readtable()
    {
        XTableAsyncLoader loader = XTableAsyncLoader.Get();
        WindowDevice device = CFAllocator.Allocate<WindowDevice>();
        loader.AddTask(@"Table/WindowDevice",device);
        loader.Execute();
        return device?.Table;
    } 
    private int GetDeviceLevel()
    {
        var device = SystemInfo.graphicsDeviceName.ToLower();
        var level = 4;
        var isMarch = false;
        if (Table == null)
        {
            return level;
        }
        for (int i = 0; i < Table.Length; i++)
        {
            var data = Table[i];
            if (device.Contains(data.GPUCompare) &&
                device.Contains(data.GPUKind) &&
                device.Contains(data.GPUMainModel) &&
                device.Contains(data.GPUSuffix))
            {
                level = data.DeviceLevel;
                break;
            }
        }

        return level;
    }

    public void SetDeviceLevelHandler(System.Action<int> onDeviceLevel)
    {
        var level = GetDeviceLevel();
        onDeviceLevel(level);
    }
}
