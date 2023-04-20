using System;
using System.Runtime.InteropServices;
using GMSDK;
using UNBridgeLib;
using UNBridgeLib.LitJson;

namespace GSDK
{
    public class PrivacyService : IPrivacyService
    {
        public const string SDKCheckHasAgreePrivacy = "requestCheckHasAgreePrivacy";
        public const string SDKShowPrivacy = "requestShowPrivacy";
        public const string SDKShowPrivacyWithContent = "requestShowPrivacyWithContent";
        public const string SDKShowPrivacyResult = "requestShowPrivacyResult";
        public const string Init = "registerPrivacy";
        
        readonly MainSDK _gsdk;

        public PrivacyService()
        {
#if UNITY_ANDROID
            UNBridge.Call(Init, null);
#endif
        }

        public bool HasAgreePrivacy
        {
            get
            {
#if UNITY_STANDALONE_WIN && !GMEnderOn
                return ComplianceHasAgreePrivacy();
#else
                object res = UNBridge.CallSync(SDKCheckHasAgreePrivacy, null);
                return res != null ? (bool)res : false;
#endif
            }
        }

        public event PrivacyShownEventHandler PrivacyShownEvent;

        public void ShowPrivacy(string content)
        {
#if UNITY_STANDALONE_WIN && !GMEnderOn
            ComplianceSetShowPrivacyCallback(PrivacyCallback);
            ComplianceShowPrivacy();
#else
            if (content == null)
            {
                GMUNCallbackHandler unCallback = new GMUNCallbackHandler()
                {
                    showPrivacyCallBack = res =>
                    {
                        try
                        {
                            if (PrivacyShownEvent != null)
                            {
                                InnerTools.SafeInvoke((() =>
                                {
                                    PrivacyShownEvent(res.hasAgree);
                                }));
                            }
                        }
                        catch (Exception ex)
                        {
                            GLog.LogException(ex);
                        }
                    }
                };
                unCallback.OnSuccess = new OnSuccessDelegate(unCallback.OnShowPrivacyCallBack);
                // 监听隐私协议回调
                UNBridge.Listen(SDKShowPrivacyResult, unCallback);
                UNBridge.Call(SDKShowPrivacy);
            }
            else
            {
                PrivacyCallbackHandler unCallback = new PrivacyCallbackHandler()
                {
                    showPrivacyCallBack = res =>
                    {
                        try
                        {
                            if (PrivacyShownEvent != null)
                            {
                                InnerTools.SafeInvoke((() =>
                                {
                                    PrivacyShownEvent(res.hasAgree);
                                }));
                            }
                        }
                        catch (Exception ex)
                        {
                            GLog.LogException(ex);
                        }
                    }
                };
                unCallback.OnSuccess = new OnSuccessDelegate(unCallback.OnShowPrivacyCallBack);
                // 监听隐私协议回调
                UNBridge.Listen(SDKShowPrivacyResult, unCallback);
                JsonData param = new JsonData();
                param["content"] = content;
                UNBridge.Call(SDKShowPrivacyWithContent, param);
                
                // _gsdk.SDKShowPrivacy(content, });
            }
#endif
        }

#if UNITY_STANDALONE_WIN && !GMEnderOn
        [DllImport(PluginName.GSDK)]
        private static extern void ComplianceSetShowPrivacyCallback(PrivacyShownEventHandler callback);

        [DllImport(PluginName.GSDK)]
        private static extern void ComplianceShowPrivacy();

        [DllImport(PluginName.GSDK)]
        private static extern bool ComplianceHasAgreePrivacy();


        [MonoPInvokeCallback(typeof(PrivacyShownEventHandler))]
        private static void PrivacyCallback(bool agree)
        {
            GLog.LogDebug("agreePrivacy:" + agree);
            PrivacyService service = Compliance.Service.Privacy as PrivacyService;
            if (service.PrivacyShownEvent != null)
            {
                service.PrivacyShownEvent(agree);
            }
        }
#endif

    }

    public class ShowPrivacyResult : CallbackResult
    {
        public bool hasAgree; // 用户是否同意过
    }
}

