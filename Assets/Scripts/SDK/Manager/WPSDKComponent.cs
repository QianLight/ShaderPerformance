using LiteWebView;
using System;
using System.Collections.Generic;
using CFEngine;

public class WPSDKComponent : SDKComponent
{
    public override bool on { get { return true; } }

    public override string version { get { return "1.0.0"; } }

    public override int type { get { return SDKEnum.WPSDK; } }

    bool invalid = true;
    public override void Init()
    {
        invalid = false;
#if UNITY_ANDROID
        try
        {
            Android.WPSDK.CallStatic("CreateBuilder", false);
            Android.WPSDK.CallStatic("SetBuilderBasics", 3, 1268, 1, "0.1.23");
            Android.WPSDK.CallStatic("SetDomain", "clientlog.wanmei.com", "log1.wanmei.com", "log2.wanmei.com");
            Android.WPSDK.CallStatic("InitAppInfo");
        }
        catch (Exception e)
        {
            invalid = true;
            CFUtilPoolLib.XDebug.singleton.AddErrorLog("Exception when Init in WPSDK: " + e.ToString());
        }
#endif
    }

    public override void OnQuit()
    {
#if UNITY_ANDROID
        try
        {
            Android.WPSDK.CallStatic("Dispose");
        }
        catch (Exception e)
        {
            CFUtilPoolLib.XDebug.singleton.AddErrorLog("Exception when OnQuit in WPSDK: " + e.ToString());
        }
#endif
    }
    public void UploadEvent(string eventName, Dictionary<string, string> param)
    {
        if (invalid)
            return;

        string[] p = null;
        if (param != null && param.Count > 0)
        {
            p = ArrayPool<string>.Get(param.Count * 2);
            int i = 0;
            foreach (var pair in param)
            {
                p[i++] = pair.Key;
                p[i++] = pair.Value;
            }
        }
#if UNITY_ANDROID
        try
        {
            Android.WPSDK.CallStatic("UploadEvent", eventName, (object)p);
        }
        catch (Exception e)
        {
            CFUtilPoolLib.XDebug.singleton.AddErrorLog("Exception when UploadEvent in WPSDK: " + e.ToString());
        }
#endif
        if (p != null)
            ArrayPool<string>.Release(p);
    }
}