using System;
using System.Runtime.InteropServices;
using GMSDK;
using UNBridgeLib.LitJson;
using UnityEngine;

namespace GSDK
{
    public delegate void Callback1P(string param1);

    public delegate void Callback2P(string param1, string param2);

    public delegate void Callback3P(string param1, string param2, string param3);

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MonoPInvokeCallbackAttribute : Attribute
    {
        public MonoPInvokeCallbackAttribute(Type t)
        {
        }
    }
    
    public class GSDKInner : IGSDK
    {
        public void Initialize(InitializeDelegate callback)
        {
            GLog.LogInfo("Unity SDK Version: " + VersionCode.UnityVersion);
#if UNITY_STANDALONE_WIN && !GMEnderOn
#if UNITY_EDITOR
            GSDKSetUnityEditor(true);
#endif
            GameSDKLifeCycle lifeCycle = new GameSDKLifeCycle();
            GMSDKManager.Instance.AddObject(lifeCycle);
            GSDKSetUnityCompanyName(Application.companyName);
            GSDKSetUnityPlayerName(Application.productName);
            GSDKSetUnityVersion(Application.unityVersion);
            GSDKSetApplicationVersion(Application.version);
            _initcallback = callback;
            GSDKInit(InitCallback);
#else
            SdkUtil.IsFromUnity3 = true;
            GMSDKMgr.instance.SDK.SdkInit((CallbackResult ret) =>
            {

                if (!ret.IsSuccess())
                {
                    GLog.LogError("SDKInit failed, errorCode is:" + ret.code);
                }
                else
                {
                    // 初始化成功，异步调用一个空实现，触发NativeBridge初始化
                    // 只在iOS里需要用到
#if UNITY_IOS
                    App.Service.TriggerBridgeInit((result) => {
                        GLog.LogInfo("TriggerBridgeInit result:\n" + result.ToString());
                    });
#endif
                    // 创建一个防沉迷实例对象，注册防沉迷监听事件
                    var antiAddictionService = AntiAddiction.Service;
                }

                if (callback != null)
                {
                    callback(InnerTools.ConvertToResult(ret));
                }
            });
#endif
        }
        
#if UNITY_STANDALONE_WIN && !GMEnderOn
        private InitializeDelegate _initcallback;

        public class GameSDKLifeCycle : GMSDKObject
        {
            public override void OnApplicationQuit()
            {
                GSDKUninit();
            }

            protected override void OnUpdate(float deltaTime)
            {
                GSDKUpdate();
            }
        }
#endif
        
#if UNITY_STANDALONE_WIN && !GMEnderOn
        [MonoPInvokeCallback(typeof(InitializeDelegate))]
        private static void InitCallback(string resultStr)
        {
            Debug.Log("callbackstr:" + resultStr);
            Result result = JsonMapper.ToObject<Result>(resultStr);
            if (((GSDKInner)GameSDK.Instance)._initcallback != null)
            {
                ((GSDKInner)GameSDK.Instance)._initcallback(result);
            }
        }
#endif
        
#if UNITY_STANDALONE_WIN && !GMEnderOn
        [DllImport(PluginName.GSDK)]
        private static extern void GSDKInit(Callback1P callback);

        [DllImport(PluginName.GSDK)]
        private static extern void GSDKUninit();

        [DllImport(PluginName.GSDK)]
        private static extern void GSDKUpdate();

        [DllImport(PluginName.GSDK)]
        private static extern void GSDKSetUnityCompanyName(string unity_company_name);

        [DllImport(PluginName.GSDK)]
        private static extern void GSDKSetUnityVersion(string unity_version);

        [DllImport(PluginName.GSDK)]
        private static extern void GSDKSetUnityPlayerName(string unity_player_name);

        [DllImport(PluginName.GSDK)]
        private static extern void GSDKSetUnityEditor(bool is_unity_editor);

        [DllImport(PluginName.GSDK)]
        private static extern void GSDKSetApplicationVersion(string version);
#endif
    }
}