using System;
using UNBridgeLib.LitJson;
using UnityEngine;

public class CombinParamManager
{
    /**
     * 将GDev返回参数(GSDK-Hub参数)和本地参数SDKConfigModule进行合并
     * gdevParams:Gdev返回参数/GSDK-Hub参数
     * originSDKConfigModule:unity本地参数
     */
    public static void ParamCombin(string gdevParams,
        SDKConfigModule originSdkConfigModule)
    {
        if (!string.IsNullOrEmpty(gdevParams) && originSdkConfigModule != null)
        {
            SDKConfigModule gDevSdkConfigModule = ParseGDevParamsToSdkConfigModule(gdevParams);
            if (gDevSdkConfigModule == null)
            {
                return;
            }

            if (gdevParams.Contains("commonModule"))
            {
                CombinCommonParam.ParamCombin(gDevSdkConfigModule, originSdkConfigModule);
            }

            if (gdevParams.Contains("loginConfigModule"))
            {
                CombinLoginParame.ParamCombin(gDevSdkConfigModule, originSdkConfigModule);
            }

            if (gdevParams.Contains("pushConfigModule"))
            {
                CombinPushParam.ParamCombin(gDevSdkConfigModule, originSdkConfigModule);
            }

            if (gdevParams.Contains("shareConfigModule"))
            {
                CombinShareParam.ParamCombin(gDevSdkConfigModule, originSdkConfigModule);
            }

            if (gdevParams.Contains("rtcConfigModule"))
            {
                CombinRTCParam.ParamCombin(gDevSdkConfigModule, originSdkConfigModule);
            }

            if (gdevParams.Contains("ratingConfigModule"))
            {
                CombinAppScoreParam.ParamCombin(gDevSdkConfigModule, originSdkConfigModule);
            }

            Debug.Log("CombinParamManager ParamCombin Success");
        }
    }

    /**
     * 将GDev返回参数转换成SDKConfigModule
     */
    private static SDKConfigModule ParseGDevParamsToSdkConfigModule(string data)
    {
        SDKConfigModule sdkConfigModule = null;
        if (!string.IsNullOrEmpty(data))
        {
            try
            {
                sdkConfigModule = JsonMapper.ToObject<SDKConfigModule>(data);
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
            }
        }

        return sdkConfigModule;
    }

    /**
     * localValue:本地值
     * gdevValue：gdev平台返回值
     */
    public static string GetCombineValue(string localValue, string gdevValue)
    {
        return !string.IsNullOrEmpty(gdevValue) ? gdevValue : localValue;
    }
}