using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformanceMgr : MonoBehaviour
{
    public static void SetDefaultDeviceLevelHandler(Action<int> onDeviceLevel)
    {
        if (deviceHandle == null)
        {
#if !UNITY_EDITOR
#if UNITY_ANDROID
            deviceHandle = new DeviceAndroid();
#elif UNITY_IPHONE || UNITY_IOS
            deviceHandle = new DeviceIOS();
#elif UNITY_STANDALONE_WIN
            deviceHandle = new DeviceWindow();
#endif
#else
            deviceHandle = new DeviceWindow();
#endif
        }
        Debug.Log($"---------------------------------------------------usehandler:{deviceHandle} berofe:{deviceLevel}");

        if (deviceHandle != null)
        {

            deviceHandle?.SetDeviceLevelHandler(onDeviceLevel);
        }
        else
        {
        #if UNITY_STANDALONE
                onDeviceLevel(4);
        #else
            onDeviceLevel(0);
        #endif
            

        }
        Debug.Log($"---------------------------------------------------after:{deviceLevel}");

    }

    public static void SetPerformanceLevel(PerformanceSetting.SettingInfo info)
    {
        if (setting != null)
        {
            //setting.SetConf(info);
            _instance.StartCoroutine(setting.SetConfEnumerator(info));
        }
    }

    public static void SetIfEmulator(bool isEmulator)
    {
        if (setting != null)
        {
            setting.ifEmulator(isEmulator);
        }
    }

    public static PerformanceSetting.SettingInfo GetDefault(RenderQualityLevel level)
    {
        PerformanceSetting.SettingInfo info = null;
        if (setting != null)
        {
            info = setting.GetDefault(level);
        }

        return info;
    }

    public static void EnterUI()
    {
        Debug.LogWarning("=============================Performance Manager EnterUI");
        if (setting != null)
        {
            setting.EnterUI();
        }
    }

    public static void EnterWar()
    {
        Debug.LogWarning("==============================Performance Manager EnterWar");
        if (setting != null)
        {
            setting.EnterWar();
        }
    }
    public static void TimelineEnter()
    {
        if (setting != null)
        {
            setting.TimelineEnter();
        }
    }
    public static void TimelineLeave()
    {
        if (setting != null)
        {
            setting.TimelineLeave();
        }
    }
    
    private static PerformanceMgr _instance;
    void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(this);
        setting = new PerformanceSetting();
    }


    static void SetDeviceLevel(int level)
    {
        deviceLevel = level;
        Debug.LogWarning("PerformanceMgr Select device level :" + deviceLevel);
    }

    static void SetDefaultDeviceLevel()
    {
        if (deviceHandle == null)
        {
            return;
        }

        deviceHandle.SetDeviceLevelHandler(SetDeviceLevel);

    }
    private static int deviceLevel = 0;
    private static PerformanceSetting setting = null;
    static IDeviceLevel deviceHandle = null;

    public static int DefaultDeviceLevel
    {
        get
        {
            if (deviceHandle == null || deviceLevel == 0)
            {
                SetDefaultDeviceLevelHandler(SetDeviceLevel);
            }
            return deviceLevel;
        }
    }
}