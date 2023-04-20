using System;
using GMSDK;
using UNBridgeLib;
using UNBridgeLib.LitJson;
using System.Runtime.InteropServices;

namespace GSDK
{
    public class AntiAddictionService : IAntiAddictionService, IAntiAddictionStatusListener
    {
        public const string SdkRegisterAntiAddictionStatusListener = "registerAntiAddictionStatusListener";
        public const string SdkSetAntiAddictionEnableAlert = "setAntiAddictionEnableAlert";
        public const string SdkGetLatestAntiAddictionStatus = "getLatestAntiAddictionStatus";
        public const string SdkGetServiceAntiAddictionStatus = "getServiceAntiAddictionStatus";
        public const string SdkAntiAddictionResult = "antiAddictionResult";
        public const string Init = "registerAntiAddiction";

        public event AntiAddictionStatusEventHandler AntiAddictionStatusEvent;

        public AntiAddictionService()
        {
#if UNITY_ANDROID
            UNBridge.Call(Init, null);
#endif
            RegisterAntiAddictionStatusListener();
        }

        public void RegisterAntiAddictionStatusListener()
        {
            GLog.LogInfo("RegisterAntiAddictionStatusListener");
#if UNITY_STANDALONE_WIN && !GMEnderOn
            SetAntiAddictionStatusCallback(AntiAddictionStatusCallbackFunc);
#else
            AntiAddictionCallbackHandler unCallBack = new AntiAddictionCallbackHandler();
            unCallBack.antiAddictionStatusListener = this;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnAntiAddictionStatusChangedCallBack);
            UNBridge.Listen(SdkAntiAddictionResult, unCallBack);
            UNBridge.Call(SdkRegisterAntiAddictionStatusListener, null);
#endif
        }

        public bool EnableAlert
        {
            set
            {
                GLog.LogInfo("SetAlertEnable enable: " + value);
                JsonData param = new JsonData();
                param["enable"] = value;
                UNBridge.CallSync(SdkSetAntiAddictionEnableAlert, param);
            }
        }

        public void FetchLatestAntiAddictionStatus(AntiAddictionStatusEventHandler callback)
        {
            GLog.LogInfo("FetchLatestAntiAddictionStatus");
#if UNITY_STANDALONE_WIN && !GMEnderOn
            FetchLatestAntiAddictionStatusCallback = callback;
            FetchLatestAntiAddictionStatus(FetchLatestAntiAddictionStatusCallbackFunc);
#else
            AntiAddictionCallbackHandler unCallBack = new AntiAddictionCallbackHandler()
            {
                antiAddictionStatusCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnFetchLatestAntiAddictionStatusCallBack);
            UNBridge.Call(SdkGetLatestAntiAddictionStatus, null, unCallBack);
#endif
        }
        
        public void FetchServiceAntiAddictionStatus(AntiAddictionStatusEventHandler callback)
        {
            GLog.LogInfo("FetchServiceAntiAddictionStatus");
#if UNITY_STANDALONE_WIN && !GMEnderOn
            FetchServiceAntiAddictionStatusCallback = callback;
            FetchServiceAntiAddictionStatus(FetchServiceAntiAddictionStatusCallbackFunc);
#else
            GLog.LogInfo("FetchServiceAntiAddictionStatus");
            AntiAddictionCallbackHandler unCallBack = new AntiAddictionCallbackHandler()
            {
                antiAddictionStatusCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnFetchServiceAntiAddictionStatusCallBack);
            UNBridge.Call(SdkGetServiceAntiAddictionStatus, null, unCallBack);
#endif
        }

        public void onChangedCallback(AntiAddictionInfo ret)
        {
            if (AntiAddictionStatusEvent != null)
            {
                try
                {
                    AntiAddictionStatusEvent(ret);
                }
                catch (Exception ex)
                {
                    GLog.LogException(ex);
                }
            }
        }

#if UNITY_STANDALONE_WIN && !GMEnderOn
        [MonoPInvokeCallback(typeof(Callback1P))]
        public static void AntiAddictionStatusCallbackFunc(string antiAddictionStatusJson)
        {
            GLog.LogInfo("AntiAddictionStatusCallbackFunc: antiAddictionStatus = " + antiAddictionStatusJson);
            AntiAddictionInfo antiAddictionStatus = SdkUtil.ToObject<AntiAddictionInfo>(antiAddictionStatusJson);

            AntiAddictionService antiAddictionService = AntiAddiction.Service as AntiAddictionService;
            antiAddictionService?.AntiAddictionStatusEvent?.Invoke(antiAddictionStatus);

            antiAddictionService = Compliance.Service.AntiAddiction as AntiAddictionService;
            antiAddictionService?.AntiAddictionStatusEvent?.Invoke(antiAddictionStatus);
        }

        public AntiAddictionStatusEventHandler FetchLatestAntiAddictionStatusCallback;

        [MonoPInvokeCallback(typeof(Callback1P))]
        public static void FetchLatestAntiAddictionStatusCallbackFunc(string antiAddictionStatusJson)
        {
            GLog.LogInfo("FetchLatestAntiAddictionStatusCallbackFunc: antiAddictionStatus = " + antiAddictionStatusJson);
            AntiAddictionInfo antiAddictionStatus = SdkUtil.ToObject<AntiAddictionInfo>(antiAddictionStatusJson);

            AntiAddictionService antiAddictionService = AntiAddiction.Service as AntiAddictionService;
            antiAddictionService?.FetchLatestAntiAddictionStatusCallback?.Invoke(antiAddictionStatus);

            antiAddictionService = Compliance.Service.AntiAddiction as AntiAddictionService;
            antiAddictionService?.FetchLatestAntiAddictionStatusCallback?.Invoke(antiAddictionStatus);
        }

        public AntiAddictionStatusEventHandler FetchServiceAntiAddictionStatusCallback;

        [MonoPInvokeCallback(typeof(Callback1P))]
        public static void FetchServiceAntiAddictionStatusCallbackFunc(string antiAddictionStatusJson)
        {
            GLog.LogInfo("FetchServiceAntiAddictionStatusCallbackFunc: antiAddictionStatus = " + antiAddictionStatusJson);
            AntiAddictionInfo antiAddictionStatus = SdkUtil.ToObject<AntiAddictionInfo>(antiAddictionStatusJson);

            AntiAddictionService antiAddictionService = AntiAddiction.Service as AntiAddictionService;
            antiAddictionService?.FetchServiceAntiAddictionStatusCallback?.Invoke(antiAddictionStatus);

            antiAddictionService = Compliance.Service.AntiAddiction as AntiAddictionService;
            antiAddictionService?.FetchServiceAntiAddictionStatusCallback?.Invoke(antiAddictionStatus);
        }

        [DllImport(PluginName.GSDK)]
        public static extern void SetAntiAddictionStatusCallback(Callback1P callback);

        [DllImport(PluginName.GSDK)]
        public static extern void FetchLatestAntiAddictionStatus(Callback1P callback);

        [DllImport(PluginName.GSDK)]
        public static extern void FetchServiceAntiAddictionStatus(Callback1P callback);
#endif
    }

    public interface IAntiAddictionStatusListener
    {
        void onChangedCallback(AntiAddictionInfo ret);
    }
}
