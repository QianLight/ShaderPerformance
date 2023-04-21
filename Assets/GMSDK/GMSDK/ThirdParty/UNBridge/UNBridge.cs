using UnityEngine;
using UNBridgeLib.LitJson;
using UNBridgeLib;
using AOT;
using System.Runtime.InteropServices;
using GMSDK;
using System.Collections.Generic;
#if GMEnderOn && UNITY_EDITOR
using Ender;
#endif

/// <summary>
/// 封装了和Native通讯的接口。
/// </summary>
public class UNBridge : MonoBehaviour
{
    public const int ERROR_CODE_DISASTER_RECOVERY = -9999;
    public const string ERROR_MSG_DISASTER_RECOVERY = "method invalid";
    public delegate void sendToUnityObject(string json);
// #if UNITY_IOS
//     [DllImport("__Internal")]
//     private static extern void setiOSUnityObject(sendToUnityObject unityObject);
// #endif

    private static bool initialized;
    private static bool settingRegister;
    private static string recoveryMethods;
    private static UNBridge _current;
    private static List<string> ignoreTarget = new List<string>();
    private static Dictionary<string, bool> checkedTarget = new Dictionary<string, bool>();


    // Start is called before the first frame update
    void Start()
    {
        LogUtils.D("UNBridge Start");
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnDestroy()
    {
        //停止超时检测定时器
        BridgeCore.StopTimer();
        LogUtils.D("UNBridge Destroy");
    }

    /// <summary>
    /// 初始化Bridge
    /// </summary>
    public static void InitBridge()
    {
        if (!initialized)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            initialized = true;
            GameObject g = new GameObject("UNBridge");
            //####永不销毁
            DontDestroyOnLoad(g);
            _current = g.AddComponent<UNBridge>();
            LogUtils.D("InitBridge");
            _current.Init();
            ignoreTarget.Add(SDKMethodName.SDKInit);
            ignoreTarget.Add(SDKMethodName.SDKRequestPermissions);
            ignoreTarget.Add(SDKResultName.SDKRequestPermissionsResult);
            ignoreTarget.Add(SDKResultName.SDKPrivacyResult);
            ignoreTarget.Add(SDKMethodName.SDKSetPatchVersion);
            ignoreTarget.Add(SDKMethodName.SdkMonitorEvent);
            ignoreTarget.Add(SDKMethodName.SdkTrackSDKEvent);
            ignoreTarget.Add(SDKMethodName.SDKShowPrivacy);
            ignoreTarget.Add(SDKMethodName.SDKCheckHasAgreePrivacy);
            ignoreTarget.Add(SDKResultName.SDKShowPrivacyResult);

#if GM_Mock
           MockUtils.InitMockData();      
#endif            
            
        }
    }

    private void Init()
    {
        UNBridgeLib.Loom.Initialize();
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidUtils.Init();
            LogUtils.D("Init Android");
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            IosUtils.Init();
// #if UNITY_IOS
//             setiOSUnityObject(HandleMsgFromiOSNative);
// #endif
            LogUtils.D("Init iOS");
        }
        else if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            WebGlUtils.Init();
            LogUtils.D("Init WebGL");
        }
        else if (Application.platform == RuntimePlatform.OSXEditor ||
                 Application.platform == RuntimePlatform.WindowsEditor)
        {
#if GMEnderOn && UNITY_EDITOR
            GMEnderMgr.instance.setEnderMessageCallbackObject(HandleMsgFromEnder);
#endif
        }

        // 启动超时检测定时器
        BridgeCore.StartTimer();
    }

    /// <summary>
    /// 重新设置Call的超时时间，单位毫秒
    /// </summary>
    /// <param name="time">单位ms</param>
    public static void setCallBackTimeout(long time)
    {
        InitBridge();
        BridgeCore.setCallBackTimeout(time);
    }

    /// <summary>
    /// Unity注册API,以接口的形式
    /// </summary>
    /// <param name="name">接口名</param>
    /// <param name="api">接口的实现</param>
    public static void RegisterAPI(string name, IBridgeAPI api)
    {
        InitBridge();
        BridgeCore.RegisterAPI(name, api);
    }


    /// <summary>
    /// Unity注册API，以委托的形式提供
    /// </summary>
    /// <param name="name"></param>
    /// <param name="api"></param>
    public static void RegisterAPI(string name, BridgeAPI api)
    {
        InitBridge();
        BridgeCore.RegisterAPI(name, api);
    }


    /// <summary>
    /// Unity注册事件
    /// </summary>
    /// <param name="target"></param>
    public static void RegisterEvent(string target)
    {
        InitBridge();
        BridgeCore.RegisterEvent(target);
    }

    /// <summary>
    ///  Unity发送事件消息
    /// </summary>
    /// <param name="target"></param>
    /// <param name="data"></param>
    //public static void SendEvent(string target, JsonData data)
    //{
    //    InitBridge();
    //    bool flag = BridgeCore.SendEvent(target, data);
    //    if (flag)
    //    {
    //        if (Application.platform == RuntimePlatform.Android)
    //        {
    //            AndroidUtils.SendEventNative(target, data);
    //        }
    //        else if (Application.platform == RuntimePlatform.IPhonePlayer)
    //        {
    //            IosUtils.SendEventNative(target, data);
    //        }
    //        else if (Application.platform == RuntimePlatform.WebGLPlayer)
    //        {
    //            WebGlUtils.SendEventNative(target, data);
    //        }
    //        else if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
    //        {
    //            EnderUtils.SendEventNative(target, data);
    //        }
    //    }
    //}

    ///<summary>
    ///异步调用native的方法，不需要回调和参数
    ///</summary>
    ///<param name="target">目标接口</param>
    ///
    public static void Call(string target)
    {
        InitBridge();
        Call(target, null);
    }

    /// <summary>
    /// 异步调用native的方法，不需要回调
    /// </summary>
    /// <param name="target">目标接口</param>
    /// <param name="param">参数数据</param> 
    ///
    public static void Call(string target, JsonData param)
    {
        InitBridge();
#if GM_Mock
        if (MockUtils.Call(target, param, null)) return;
#endif
        if (!checkMethodEnable(target))
        {
            return;
        }

        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidUtils.CallNative(BridgeCore.TYPE_CALL, target, param, 0);
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            IosUtils.CallNative(BridgeCore.TYPE_CALL, target, param, 0);
        }
        else if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            WebGlUtils.CallNative(BridgeCore.TYPE_CALL, target, param, 0);
        }
        else if (Application.platform == RuntimePlatform.OSXEditor ||
                 Application.platform == RuntimePlatform.WindowsEditor)
        {
            
            EnderUtils.CallNative(BridgeCore.TYPE_CALL, target, param, 0);
        }
    }

    ///<summary>
    ///异步调用native的方法
    ///</summary>
    ///<param name="target">目标接口</param>
    ///<param name="param">参数数据</param>
    ///<param name="callback">回调</param>
    ///
    public static void Call(string target, JsonData param, BridgeCallBack callback)
    {
        Call(target, param, callback, 0);
    }

    /// <summary>
    /// 异步调用native的方法
    /// </summary>
    /// <param name="target">目标接口</param>
    /// <param name="param">参数数据</param>
    /// <param name="callback">回调</param>
    /// <param name="timeout">超时，默认10秒，单位毫秒</param>
    public static void Call(string target, JsonData param, BridgeCallBack callback, long timeout)
    {
        InitBridge();
#if GM_Mock
        if (MockUtils.Call(target, param, callback)) return;
#endif   
        if (!checkMethodEnable(target))
        {
            //callback.OnFailed.Invoke(ERROR_CODE_DISASTER_RECOVERY, ERROR_MSG_DISASTER_RECOVERY);
            callbackRecovery(callback);
            return;
        }
        long callbackId = BridgeCore.Call(target, param, callback, timeout);
        if (callbackId > 0)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                AndroidUtils.CallNative(BridgeCore.TYPE_CALL, target, param, callbackId);
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                IosUtils.CallNative(BridgeCore.TYPE_CALL, target, param, callbackId);
            }
            else if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                WebGlUtils.CallNative(BridgeCore.TYPE_CALL, target, param, callbackId);
            }
            else if (Application.platform == RuntimePlatform.OSXEditor ||
                     Application.platform == RuntimePlatform.WindowsEditor)
            {
                EnderUtils.CallNative(BridgeCore.TYPE_CALL, target, param, callbackId);
            }
        }
    }

    ///<summary>
    ///MOCK异步调用native的方法
    ///</summary>
    ///<param name="target">目标接口</param>
    ///<param name="param">参数数据</param>
    ///<param name="mock">回包的MOCK数据</param>
    ///<param name="callBack">回调</param>
    ///
    public static void CallMock(string target, JsonData param, JsonData mock, BridgeCallBack callBack)
    {
        InitBridge();
        BridgeCore.CallMock(target, param, mock, callBack);
    }

    ///<summary>
    ///同步调用native的方法
    ///</summary>
    ///<param name="target">目标接口</param>
    ///<param name="param">参数数据</param> 
    ///
    public static object CallSync(string target, JsonData param)
    {
      
        InitBridge();
#if GM_Mock
        object value = null;
        if (MockUtils.CallSync(target, ref value)) return value;
#endif  
        if (!checkMethodEnable(target))
        {
            return null;
        }

        if (Application.platform == RuntimePlatform.Android)
        {
            return AndroidUtils.CallNativeSync(BridgeCore.TYPE_CALL, target, param);
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            return IosUtils.CallNativeSync(BridgeCore.TYPE_CALL, target, param);
        }
        else if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            return WebGlUtils.CallNativeSync(BridgeCore.TYPE_CALL, target, param);
        }
        else if (Application.platform == RuntimePlatform.OSXEditor ||
                 Application.platform == RuntimePlatform.WindowsEditor)
        {
            return EnderUtils.CallNativeSync(BridgeCore.TYPE_CALL, target, param);
        }

        return null;
    }

    private static object CallSyncInternal(string target, JsonData param)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            return AndroidUtils.CallNativeSync(BridgeCore.TYPE_CALL, target, param);
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            return IosUtils.CallNativeSync(BridgeCore.TYPE_CALL, target, param);
        }
        else if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            return WebGlUtils.CallNativeSync(BridgeCore.TYPE_CALL, target, param);
        }
        else if (Application.platform == RuntimePlatform.OSXEditor ||
                 Application.platform == RuntimePlatform.WindowsEditor)
        {
            return EnderUtils.CallNativeSync(BridgeCore.TYPE_CALL, target, param);
        }

        return null;
    }

    private static bool CheckMethodForOtherPlatform(string target)
    {
        if (!settingRegister)
        {
            JsonData p = new JsonData();
            p["key"] = "UnityDisasterRecovery";
            p["description"] = "unity容灾降级";
            p["owner"] = "libohan.rd";
            p["defaultValue"] = "";
            JsonData param = new JsonData();
            param["data"] = p;
            object res = UNBridge.CallSyncInternal(SDKMethodName.SdkRegisterExperiment, param);
            settingRegister = res != null ? (bool) res : false;
        }

        if (settingRegister)
        {
            JsonData param = new JsonData();
            param["key"] = "UnityDisasterRecovery";
            param["withExposure"] = true;
            if (recoveryMethods == null)
            {
                object setting = UNBridge.CallSyncInternal(SDKMethodName.SdkGetExperiment, param);
                recoveryMethods = setting != null ? setting.ToString() : "";
                LogUtils.D("recovery methods : " , recoveryMethods);
            }

            if (recoveryMethods.Length > 0 && recoveryMethods != "[]")
            {
                JsonData jsonData = JsonMapper.ToObject(recoveryMethods);
                foreach (var item in jsonData)
                {
                    if (target.Equals(item.ToString()))
                    {
                        LogUtils.D(ERROR_CODE_DISASTER_RECOVERY + ERROR_MSG_DISASTER_RECOVERY + ":" + item.ToString());
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private static bool CheckMethodForAndroid(string target)
    {
        JsonData param = new JsonData();
        param["key"] = target;
        object res = UNBridge.CallSyncInternal(SDKMethodName.SdkCheckMethodEnable, param);
        bool enable = true;
        if (res != null)
        {
            enable = (bool) res;
        }
        LogUtils.D("check method result: " + enable + ", res: " + (res != null ? res.ToString(): "null"));
        if (enable) return true;
        LogUtils.D(ERROR_CODE_DISASTER_RECOVERY + ERROR_MSG_DISASTER_RECOVERY + ":" + target);
        return false;
    }

    public static bool checkMethodEnable(string target)
    {
        if (target == null)
        {
            return false;
        }

        if (ignoreTarget.Contains(target))
        {
            return true;
        }

        bool res = true;
        if (checkedTarget.TryGetValue(target, out res))
        {
            return res;
        }
#if UNITY_ANDROID
        // Android容灾降级实现转移到了native侧，调用CacheService方法防止出现卡死，并减少json序列化使用防止内存波动
        res = CheckMethodForAndroid(target);
#else
        res = CheckMethodForOtherPlatform(target);
#endif
        checkedTarget[target] = res;
        return res;
    }

    ///<summary>
    ///监听事件
    ///</summary>
    ///<param name="target">目标接口</param>
    ///<param name="callBack">回调</param> 
    ///
    public static void Listen(string target, BridgeCallBack callBack)
    {
        Listen(target, true, callBack);
    }

    ///<summary>
    ///监听事件
    ///</summary>
    ///<param name="target">目标接口</param>
    ///<param name="overOld">新的监听是否覆盖旧的</param>
    ///<param name="callBack">回调</param> 
    ///
    public static void Listen(string target, bool overOld, BridgeCallBack callBack)
    {
        InitBridge();
        if (!checkMethodEnable(target))
        {
            // callBack.OnFailed.Invoke(ERROR_CODE_DISASTER_RECOVERY, ERROR_MSG_DISASTER_RECOVERY);
            callbackRecovery(callBack);
            return;
        }

        BridgeCore.Listen(target, overOld, callBack);
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidUtils.CallNative(BridgeCore.TYPE_LISTEN, target, null, 0);
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            IosUtils.CallNative(BridgeCore.TYPE_LISTEN, target, null, 0);
        }
        else if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            WebGlUtils.CallNative(BridgeCore.TYPE_LISTEN, target, null, 0);
        }
        else if (Application.platform == RuntimePlatform.OSXEditor ||
                 Application.platform == RuntimePlatform.WindowsEditor)
        {
            EnderUtils.CallNative(BridgeCore.TYPE_LISTEN, target, null, 0);
        }
    }

    private static void callbackRecovery(BridgeCallBack callBack)
    {
        JsonData jsonData = new JsonData();
        jsonData["code"] = ERROR_CODE_DISASTER_RECOVERY;
        jsonData["message"] = ERROR_MSG_DISASTER_RECOVERY;
        callBack.OnSuccess.Invoke(jsonData);
    }

    ///<summary>
    ///MOCK监听事件
    ///</summary>
    ///<param name="target">目标接口</param>
    ///<param name="mock">MOCK数据</param>
    ///<param name="callBack">回调</param> 
    ///
    public static void ListenMock(string target, JsonData mock, BridgeCallBack callBack)
    {
        InitBridge();
        BridgeCore.ListenMock(target, mock, callBack);
    }


    ///<summary>
    ///关闭监听事件
    ///</summary>
    ///
    ///<param name="target">目标接口</param>
    ///
    public static void UnListen(string target)
    {
        InitBridge();
        BridgeCore.UnListen(target);
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidUtils.CallNative(BridgeCore.TYPE_UNLISTEN, target, null, 0);
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            IosUtils.CallNative(BridgeCore.TYPE_UNLISTEN, target, null, 0);
        }
        else if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            WebGlUtils.CallNative(BridgeCore.TYPE_UNLISTEN, target, null, 0);
        }
        else if (Application.platform == RuntimePlatform.OSXEditor ||
                 Application.platform == RuntimePlatform.WindowsEditor)
        {
            EnderUtils.CallNative(BridgeCore.TYPE_UNLISTEN, target, null, 0);
        }
    }


    [MonoPInvokeCallback(typeof(sendToUnityObject))]
    public static void HandleMsgFromEnder(string msg)
    {
        UNBridgeLib.Loom.QueueOnMainThread(() => { BridgeCore.HandleMsgFromNative(msg); });
    }

    /// <summary>
    /// 处理Native调用过来的消息，通讯接口
    /// </summary>
    /// <param name="msg">消息数据，JSON格式</param>
    void HandleMsgFromNative(string msg)
    {
        BridgeCore.HandleMsgFromNative(msg);
    }

    [MonoPInvokeCallback(typeof(sendToUnityObject))]
    public static void HandleMsgFromiOSNative(string json)
    {
        BridgeCore.HandleMsgFromNative(json);
    }
}