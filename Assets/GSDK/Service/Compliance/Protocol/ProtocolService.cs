using System;
using System.Runtime.InteropServices;
using System.Text;
using GMSDK;
using UNBridgeLib;
using UNBridgeLib.LitJson;

namespace GSDK
{
    public class ProtocolService : IProtocolService
    {
        public const string SDKManagementPanelClosedResult = "requestManagementPanelClosedResult";
        public const string SDKShowLicensePanel = "requestShowLicense";
        public const string SDKProtocolVersion = "checkProtocolVersion";
        public const string SDKProtocolAddress = "requestProtocolAddress";
        public const string Init = "registerProtocol";
        
        public ProtocolService()
        {
#if UNITY_ANDROID
            UNBridge.Call(Init, null);
#endif
        }
        
        public bool IsProtocolUpdated()
        {
            object res = UNBridge.CallSync(SDKProtocolVersion, null);
            return res != null ? (bool)res : false;
        }

        public void ShowLicense(PanelClosedDelegate panelClosedCallback)
        {
#if UNITY_STANDALONE_WIN && !GMEnderOn            
            ShowLicenseCallback = panelClosedCallback;
            ComplianceShowLicense(ShowLicenseFunc);    
#else
            LogUtils.D("Sdk -- Unity -- SdkShowLicense");
            ProtocolCallbackHandler panelClosedUnCallBack = new ProtocolCallbackHandler()
            {
                panelClosedResult = result =>
                {
                    InnerTools.SafeInvoke(() =>
                    {
                        if (panelClosedCallback != null)
                        {
                            panelClosedCallback();
                        }
                    });
                }
            };
            panelClosedUnCallBack.OnSuccess = panelClosedUnCallBack.OnPanelClosedCallBack;
            UNBridge.Listen(SDKManagementPanelClosedResult, panelClosedUnCallBack);
            UNBridge.Call(SDKShowLicensePanel);
#endif
        }
        
        public void SdkProtocolAddress(Action<ProtocolAddressResult> callback)
        {
            LogUtils.D("Sdk -- Unity -- SdkProtocolAddress");
#if UNITY_STANDALONE_WIN && !GMEnderOn
            ProtocolAddressCallback = callback;
            ComplianceSdkProtocolAddress(ProtocolAddressFunc);
#else
            ProtocolCallbackHandler unCallBack = new ProtocolCallbackHandler()
            {
                protocolAddressCallBack = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnProtocolAddressCallBack);
            UNBridge.Call(SDKProtocolAddress, null, unCallBack);
#endif
        }

#if UNITY_STANDALONE_WIN && !GMEnderOn
        [DllImport(PluginName.GSDK)]
        private static extern void ComplianceShowLicense(PanelClosedDelegate callback);

        public PanelClosedDelegate ShowLicenseCallback;

        [MonoPInvokeCallback(typeof(PanelClosedDelegate))]
        private static void ShowLicenseFunc(bool agree)
        {
            ProtocolService service = Compliance.Service.Protocol as ProtocolService;

            if (service.ShowLicenseCallback != null)
            {
                service.ShowLicenseCallback(agree);
            }
        }


        [DllImport(PluginName.GSDK)]
        private static extern void ComplianceSdkProtocolAddress(Callback1P callback);

        public Action<ProtocolAddressResult> ProtocolAddressCallback;

        [MonoPInvokeCallback(typeof(Action<ProtocolAddressResult>))]
        private static void ProtocolAddressFunc(string resultStr)
        {
            GLog.LogDebug("callbackstr:" + resultStr);
            ProtocolAddressResult result = JsonMapper.ToObject<ProtocolAddressResult>(resultStr);
            ProtocolService service = Compliance.Service.Protocol as ProtocolService;

            if (service.ProtocolAddressCallback != null)
            {
                service.ProtocolAddressCallback(result);
            }
        }

        [DllImport(PluginName.GSDK)]
        private static extern void ComplianceAuthRealNameWithUI(int type, Callback2P callback);

#endif
    }

    public class ProtocolAddressResult : CallbackResult
    {
        public string userAgreementUrl;
        public string privacyPolicy;
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .Append("code=").Append(code)
                .Append(", message='").Append(message).Append("'")
                .Append(", userAgreementUrl='").Append(userAgreementUrl).Append("'")
                .Append(", privacyPolicy='").Append(privacyPolicy).Append("'")
                .Append("}");
            return sb.ToString();
        }
    }
}