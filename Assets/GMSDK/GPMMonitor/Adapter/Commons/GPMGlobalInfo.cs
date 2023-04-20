using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GMSDK
{
    public class GPMGlobalInfo
    {
        public static void Init()
        {
            GPMMonitor.LogGlobalInfo("memory_ram");
            GPMMonitor.LogGlobalInfo("cpu_core");
            GPMMonitor.LogGlobalInfo("cpu_frequency");
            GPMMonitor.LogGlobalInfo("cpu_model");
            GPMMonitor.LogGlobalInfo("memory_rom");
            GPMMonitor.LogGlobalInfo("gpu_model", SystemInfo.graphicsDeviceName);
            GPMMonitor.LogGlobalInfo("gpu_vendor", SystemInfo.graphicsDeviceVendor);
            GPMMonitor.LogGlobalInfo("gl_version", SystemInfo.graphicsDeviceVersion);
        }
    }
}
