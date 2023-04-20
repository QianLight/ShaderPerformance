using GMSDK;
using UnityEngine;
using System.IO;
using System;
using UNBridgeLib;

public class GMSDKMgr : ServiceSingleton<GMSDKMgr>
{
    public MainSDK SDK;
    public GMExeptionInterface exceptionCallback;

    private GMSDKMgr()
    {
        if (SDK == null)
        {
            SDK = new MainSDK();
            UNBridge.setCallBackTimeout(60 * 1000);
        }
    }

    // gradle: screenrecord
    public static bool IsReplayWork()
    {
        return InvokeClassStaticMethodResult.existClass("GMSDK.BaseReplaySDK");
    }

    public static bool IsVoiceWork()
    {
        return InvokeClassStaticMethodResult.existClass("GMSDK.BaseVoiceSDK");
    }

    public static bool IsAdjustWork()
    {
        return InvokeClassStaticMethodResult.existClass("GMSDK.BaseAdjustSDK");
    }

    public static bool IsPushWork()
    {
        return InvokeClassStaticMethodResult.existClass("GMSDK.BasePushSDK");
    }

    public static bool IsShareWork()
    {
        return InvokeClassStaticMethodResult.existClass("GMSDK.ShareSDK");
    }

    public static bool IsTranslateWork()
    {
        return InvokeClassStaticMethodResult.existClass("GMSDK.BaseTranslateSDK");
    }

    public static bool IsForceUpgradeWork()
    {
        return InvokeClassStaticMethodResult.existClass("GMSDK.BaseUpgradeSDK");
    }

    public static bool IsThanosWork()
    {
        return InvokeClassStaticMethodResult.existClass("GMSDK.BaseThanosSDK");
    }

    public static bool IsLocationWork()
    {
        return InvokeClassStaticMethodResult.existClass("GMSDK.BaseLocationSDK");
    }

    public static bool IsDownloadWork()
    {
        return InvokeClassStaticMethodResult.existClass("GMSDK.BaseDownloadSDK");
    }

    // gradle对应 marketing
    public static bool IsPointsSystemWork()
    {
        return InvokeClassStaticMethodResult.existClass("GMSDK.PointsSDK");
    }

    public static bool IsRatingWork()
    {
        return InvokeClassStaticMethodResult.existClass("GMSDK.BaseRatingSDK");
    }

    public static bool IsPosterWork()
    {
        return InvokeClassStaticMethodResult.existClass("GMSDK.BasePosterSDK");
    }

    public static bool IsReactNativeWork()
    {
        return InvokeClassStaticMethodResult.existClass("GMSDK.BaseReactNativeSDK");
    }

    public static bool IsGPMMonitorWork()
    {
        return InvokeClassStaticMethodResult.existClass("GMSDK.BaseGPMSDK");
    }

    public static bool IsMediaUploadWork()
    {
        return InvokeClassStaticMethodResult.existClass("GMSDK.BaseMediaUploadSDK");
    }

    // Unity 对应 MagicBox； gradle debug_sdk
    public static bool IsMagicBoxWork()
    {
        return InvokeClassStaticMethodResult.existClass("GMSDK.BaseMagicBoxSDK");
    }

    public static bool IsIMWork()
    {
        return InvokeClassStaticMethodResult.existClass("GMSDK.BaseIMSDK");
    }

    public static bool IsRecommendContactWork()
    {
        return InvokeClassStaticMethodResult.existClass("GMSDK.BaseRecommendContactSDK");
    }

    // gradle 对应 udp；Unity 3.x: Optimization ; Unity 2.x: Diagnosis
    public static bool IsOptimizationWork()
    {
        return InvokeClassStaticMethodResult.existClass("GMSDK.BaseDiagnosisSDK");
    }

    public static bool IsAdWork()
    {
        return InvokeClassStaticMethodResult.existClass("GMSDK.BaseAdSDK");
    }

    public static bool IsLiveWork()
    {
        return InvokeClassStaticMethodResult.existClass("GMSDK.BaseLiveSDK");
    }

    public static bool IsIEmoticonWork()
    {
        return InvokeClassStaticMethodResult.existClass("GSDK.IEmoticonService");
    }
}