using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GMSDK;
using UNBridgeLib.LitJson;


namespace GSDK
{

    public class AppService : IAppService, IRequestPermissionCallback
    {
        #region Variables

        private readonly GMSDK.MainSDK _gsdk;

        private string _boeHeader = "prod";


        private RequestPermissionDelegate _permissionDelegate;

        public event RequestDeepLinkURLEventHandler DeeplinkURLEvent;
        private const string AppServiceRegister = "registerAppService";
        private const string getAllPermissionsStates = "getAllPermissionsStates";
        private const string changePermissionState = "changePermissionState";
        private const string DeepLinkRequestURL = "requestDeepLinkURLResult";

        public const string SDKFetchDeviceId = "fetchDeviceInfo";
        public const string SDKFetchDeviceIdResultEvent = "fetchDeviceInfoResultEvent";
        public const string SDKDeviceInfoUpdateEvent = "deviceInfoUpdateEvent";



        #endregion

        public void Initialize()
        {
            GSDKProfilerTools.BeginSample("Main Initialize");
            GLog.LogInfo("Initialize", "AppService");
            ListenDeepLinkURLEvent();
            UNBridge.Call(AppServiceRegister);
            GSDKProfilerTools.EndSample();
        }

        public Dictionary<GSDKPermissionTypeKey, bool> RequestAllPermissionsStates()
        {
            String loc = "";
            Object res = UNBridge.CallSync(getAllPermissionsStates, null);
            if (res != null)
            {
                loc = res.ToString();
            }
            Dictionary<String, int> result = SdkUtil.ToObject<Dictionary<String,int>>(loc);
            GLog.LogInfo("RequestAllPermissionsStates", result.ToString());
            
            Dictionary<GSDKPermissionTypeKey, bool> formatResult = new Dictionary<GSDKPermissionTypeKey, bool>();
            foreach (String key in result.Keys)
            {
                formatResult.Add((GSDKPermissionTypeKey)int.Parse(key), result[key] != 0);
            }
            return formatResult;
        }

        public void changePermissionStates(GSDKPermissionTypeKey key, bool newState)
        {
            JsonData param = new JsonData();
            param["permission"] = (int)key;
            param["newState"] = newState;
            UNBridge.Call(changePermissionState, param);
        }

        public void updateConfiguration(Dictionary<String, Object> extraParams)
        {
            _gsdk.SDKUpdateConfig(extraParams);
        }

        #region Properties

        public string DeviceID
        {
            get
            {
#if UNITY_STANDALONE_WIN && !GMEnderOn
                return Marshal.PtrToStringAnsi(GetDeviceID());
#else
                return _gsdk.SdkGetDeviceId();
#endif
            }
        }

        public string InstalledID
        {
            get
            {
#if UNITY_STANDALONE_WIN && !GMEnderOn
                return Marshal.PtrToStringAnsi(GetInstallID());
#else
                return _gsdk.SdkGetInstallId();
#endif
            }
        }

        public void FetchDeviceInfo(int timeout, RequestDeviceInfoCallback callback)
        {
            GLog.LogInfo("FetchDeviceInfo timeout:" + timeout);
            if (callback == null) return;
            JsonData para = new JsonData();
            para["timeout"] = timeout;
            var appServiceHandler = new AppServiceCallbackHandler
            {
                DeviceInfoCallback = callback        
            };
            appServiceHandler.OnSuccess = appServiceHandler.onRequestDeviceInfocallback;
            UNBridge.Listen(SDKFetchDeviceIdResultEvent, appServiceHandler);
            UNBridge.Call(SDKFetchDeviceId, para);
        }


        public void RegisterDeviceInfoUpdateHandler(RequestDeviceInfoUpdateEventHandler handler)
        {
            GLog.LogInfo("RegisterDeviceInfoUpdateHandler");
            if (handler == null) return;
            string did = DeviceID;
            string iid = InstalledID;
            if(did.Length > 0 && iid.Length > 0)
            {
                GLog.LogInfo("RegisterDeviceInfoUpdateHandler from local");
                handler.Invoke(did, iid);
                return;
            }

            var appServiceHandler = new AppServiceCallbackHandler
            {
                DeviceInfoUpdateHandler = handler
            };
            appServiceHandler.OnSuccess = appServiceHandler.onDeviceInfoUpdateEventHandler;
            UNBridge.Listen(SDKDeviceInfoUpdateEvent, appServiceHandler);
            GLog.LogInfo("RegisterDeviceInfoUpdateHandler from server");
        }

        public string AppID
        {
            get
            {
#if UNITY_STANDALONE_WIN && !GMEnderOn
                return Marshal.PtrToStringAnsi(GetAppID());
#else
                return _gsdk.SdkGetAppId();
#endif
            }
        }

        public string ChannelOp
        {
            get
            {
#if UNITY_STANDALONE_WIN && !GMEnderOn
                return Marshal.PtrToStringAnsi(GetChannelOp());
#else
                return _gsdk.SdkGetChannelOp();
#endif                
            }

        }

        public string DownloadSource
        {
            get { return _gsdk.SDKGetDownloadSource(); }               
        }
        

        public string Channel
        {
            get
            {
#if UNITY_STANDALONE_WIN && !GMEnderOn
                return Marshal.PtrToStringAnsi(GetChannel());
#else
                return _gsdk.SdkGetChannel();
#endif
            }
        }

        public bool EnableDebugMode
        {
            set
            {
#if UNITY_STANDALONE_WIN && !GMEnderOn
                SetDebug(value);
#else                
                _gsdk.SdksetDebug(value);
#endif
            }
            get
            {
#if UNITY_STANDALONE_WIN && !GMEnderOn
                return IsDebugEnable();
#else
                return _gsdk.SdkIsDebugEnable();
#endif
            }
        }
        
        public bool EnableRNDebugMode
        {
            set
            {
                _gsdk.SdksetRnDebug(value);
            }
            get
            {
                return _gsdk.SdkIsRNDebugEnable();
            }
        }

        public string BOEHeader
        {
            set
            {
                _boeHeader = value;
            }
            get
            {
                return _boeHeader;
            }
        }

        public bool EnableBOEMode
        {
            set
            {
#if UNITY_STANDALONE_WIN && !GMEnderOn
                SetBOE(value);
#else                
                _gsdk.SdksetBOEEnable(value, _boeHeader);
#endif
            }
            get
            {
#if UNITY_STANDALONE_WIN && !GMEnderOn
                return IsBOEEnable();
#else                
                return _gsdk.SdkIsBOEEnable();
#endif
            }
        }
#if UNITY_STANDALONE_WIN && !GMEnderOn
        bool IAppService.EnablePPEMode
        {
            set
            {
                SetPPE(value);
            }
            get
            {
                return IsPPEEnable();
            }
        }
#endif        

        public bool EnableSandboxMode
        {
            set
            {
#if UNITY_STANDALONE_WIN && !GMEnderOn
                SetSandbox(value);
#else
                _gsdk.SdksetSandboxEnable(value);
#endif
            }
            get
            {
#if UNITY_STANDALONE_WIN && !GMEnderOn
                return IsSandboxEnable();
#else
                return _gsdk.SdkIsSandboxEnable();
#endif
            }
        }

        public string GSDKNativeVersion
        {
            get
            {
#if UNITY_STANDALONE_WIN && !GMEnderOn
                return Marshal.PtrToStringAnsi(GetGSDKVersion());
#else
                return _gsdk.SdkGetSDKVersion();
#endif
            }
        }

#if UNITY_STANDALONE_WIN && !GMEnderOn
        public string CommonParam
        {
            get
            {
                return Marshal.PtrToStringAnsi(GSDKGetCommonParam());
            }
        }
#endif

        #endregion

        #region Methods

        public AppService()
        {
            _gsdk = GMSDKMgr.instance.SDK;
        }

        //清除本地缓存的DeviceId和InstallId
        public bool ClearDeviceIdAndInstallId()
        {
            object res = UNBridge.CallSync(SDKMethodName.SDKClearDidAndIid, null);
            var result = false;
            Boolean.TryParse(res.ToString(), out result);
            return result;
        }
        

#if UNITY_STANDALONE_WIN && !GMEnderOn
        public void OpenPCBrowser(string url)
        {
            GSDKOpenBrowser(url);
        }
#endif

        public void RequestPermission(List<String> permissions, RequestPermissionDelegate callback, String extraJsonData = null)
        {
            _permissionDelegate = callback;
            if (extraJsonData != null)
            {
                _gsdk.SDKSetGameInfo(JsonMapper.ToObject(extraJsonData));
            }
            _gsdk.SDKRequestPermission(permissions, this);
        }

        public void OnPermissionResult(PermissionRequestResult result)
        {
            if (_permissionDelegate != null)
            {
                _permissionDelegate(InnerTools.ConvertToResult(result), AppInnerTools.ConvertPermissionResult(result));
            }
        }

        public void onPrivacyResult(PrivacyResult result)
        {
        }



#if UNITY_IOS
        public void TriggerBridgeInit(Action<Result> triggerCallback)
        {
            _gsdk.SDKTriggerBridgeInit((GMSDK.CallbackResult ret) =>
            {
                InnerTools.SafeInvoke(() =>
                {
                    triggerCallback(new Result(ret.code, ret.message));
                });
            });
        }
#endif

        private void ListenDeepLinkURLEvent()
        {
            var deepLinkCallbackHandler = new DeepLinkUrlCallbackHandler { DeepLinkUrlEventHandler = DeeplinkURLEvent };
            deepLinkCallbackHandler.OnSuccess = deepLinkCallbackHandler.OnDeeplinkURLCallback;
            UNBridge.Listen(DeepLinkRequestURL, deepLinkCallbackHandler);
        }

        #endregion

#if UNITY_STANDALONE_WIN && !GMEnderOn
        [DllImport(PluginName.GSDK)]
        private static extern IntPtr GetDeviceID();

        [DllImport(PluginName.GSDK)]
        private static extern IntPtr GetInstallID();

        [DllImport(PluginName.GSDK)]
        private static extern IntPtr GetAppID();

        [DllImport(PluginName.GSDK)]
        private static extern IntPtr GetGSDKVersion();
        
        [DllImport(PluginName.GSDK)]
        private static extern bool IsDebugEnable();

        [DllImport(PluginName.GSDK)]
        private static extern void SetDebug(bool enable);

        [DllImport(PluginName.GSDK)]
        private static extern bool IsBOEEnable();
        
        [DllImport(PluginName.GSDK)]
        private static extern void SetBOE(bool enable);

        [DllImport(PluginName.GSDK)]
        private static extern bool IsPPEEnable();
        
        [DllImport(PluginName.GSDK)]
        private static extern void SetPPE(bool enable);

        [DllImport(PluginName.GSDK)]
        private static extern bool IsSandboxEnable();

        [DllImport(PluginName.GSDK)]
        private static extern void SetSandbox(bool enable);
        
        [DllImport(PluginName.GSDK)]
        private static extern IntPtr GetChannel();

        [DllImport(PluginName.GSDK)]
        private static extern IntPtr GetChannelOp();

        [DllImport(PluginName.GSDK)]
        private static extern IntPtr GetPayChannelOp();

        [DllImport(PluginName.GSDK)]
        private static extern IntPtr GSDKGetCommonParam();

        [DllImport(PluginName.GSDK)]
        private static extern void GSDKOpenBrowser(string url);

        [DllImport(PluginName.GSDK)]
        private static extern void GSDKSetLanguage(string language);


#endif
    }
}

