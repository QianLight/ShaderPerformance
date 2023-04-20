using System;
using GMSDK;
using UNBridgeLib;
using System.Runtime.InteropServices;
using UNBridgeLib.LitJson;

namespace GSDK
{
    public class RealNameService : IRealNameService
    {
        private MainSDK gsdk;

        private static class RealNameMethodName
        {
            public const string Init = "registerRealName";
            public const string SdkRealNameVerifyWithUI = "requestComplianceRealNameAuth";
            public const string SdkRealNameResult = "complianceRealNameAuthResult";
        }

        public RealNameService()
        {
#if UNITY_ANDROID
            UNBridge.Call(RealNameMethodName.Init, null);
#endif
            gsdk = GMSDKMgr.instance.SDK;
        }

        public void FetchRealNameState(FetchRealNameStateDelegate fetchRealNameStateCallback)
        {
            GLog.LogInfo("FetchRealNameState");
#if UNITY_STANDALONE_WIN && !GMEnderOn
            FetchRealNameStateCallback = fetchRealNameStateCallback;
            ComplianceFetchRealNameState(FetchRealNameStateFunc);
#else
            gsdk.SdkDeviceIsVerifedV2(result => HandleRealNameStateCallback(result, fetchRealNameStateCallback));
#endif
        }

        private void HandleRealNameStateCallback(VerifiedResult result, FetchRealNameStateDelegate fetchRealNameStateCallback)
        {
            if (fetchRealNameStateCallback == null) return;
            try
            {
                fetchRealNameStateCallback(
                    InnerTools.ConvertToResult(result),
                    RealNameInnerTools.Convert(result)
                );
            }
            catch (Exception e)
            {
                GLog.LogException(e);
            }
        }
        
        public void FetchRealNameInfo(FetchRealNameInfoDelegate fetchRealNameInfoCallback)
        {
            GLog.LogInfo("FetchRealNameInfo");
#if UNITY_STANDALONE_WIN && !GMEnderOn
            FetchRealNameInfoCallback = fetchRealNameInfoCallback;
            ComplianceFetchRealNameInfo(FetchRealNameInfoFunc);
#else
            gsdk.SdkCheckRealNameResult(ret =>
            {
                if (fetchRealNameInfoCallback == null) return;
                try
                {
                    fetchRealNameInfoCallback(
                        InnerTools.ConvertToResult(ret),
                        RealNameInnerTools.Convert(ret));
                }
                catch (Exception e)
                {
                    GLog.LogException(e);
                }
            });
#endif
        }

        public void ComplianceRealNameAuth(ComplianceRealNameAuthResultDelegate realNameResultCallback)
        {
            GLog.LogInfo("RequestComplianceRealNameAuth");
#if UNITY_STANDALONE_WIN && !GMEnderOn
            ComplianceRealNameAuthCallback = realNameResultCallback;
            ComplianceRealNameAuth(ComplianceRealNameAuthCallbackFunc);
#else
            Action<ComplianceRealNameAuthResult> callback = (ComplianceRealNameAuthResult result) =>
            {
                InnerTools.SafeInvoke(() => { realNameResultCallback(result.code, result.message); });
            };

            RealNameCallbackHandler unCallBack = new RealNameCallbackHandler()
            {
                ComplianceRealNameAuthCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnComplianceRealNameAuthCallback);

            UNBridge.Listen(RealNameMethodName.SdkRealNameResult, unCallBack);
            UNBridge.Call(RealNameMethodName.SdkRealNameVerifyWithUI, null);
#endif
        }

#if UNITY_STANDALONE_WIN && !GMEnderOn
        public FetchRealNameStateDelegate FetchRealNameStateCallback;

        [MonoPInvokeCallback(typeof(Callback2P))]
        private static void FetchRealNameStateFunc(string resultStr, string callbackStr)
        {
            GLog.LogDebug("callbackstr:" + resultStr);
            Result result = JsonMapper.ToObject<Result>(resultStr);
            RealNameState realNameState = JsonMapper.ToObject<RealNameState>(callbackStr);
            RealNameService service = RealName.Service as RealNameService;

            if (service.FetchRealNameStateCallback != null)
            {
                service.FetchRealNameStateCallback(result, realNameState);
            }
        }

        public FetchRealNameInfoDelegate FetchRealNameInfoCallback;

        [MonoPInvokeCallback(typeof(Callback2P))]
        private static void FetchRealNameInfoFunc(string resultStr, string callbackStr)
        {
            GLog.LogDebug("callbackstr:" + resultStr);
            Result result = JsonMapper.ToObject<Result>(resultStr);
            RealNameInfo realNameInfo = JsonMapper.ToObject<RealNameInfo>(callbackStr);
            RealNameService service = RealName.Service as RealNameService;

            if (service.FetchRealNameInfoCallback != null)
            {
                service.FetchRealNameInfoCallback(result, realNameInfo);
            }
        }
		
		public ComplianceRealNameAuthResultDelegate ComplianceRealNameAuthCallback;

        [MonoPInvokeCallback(typeof(Callback1P))]
        public static void ComplianceRealNameAuthCallbackFunc(string resultJsonStr)
        {
            GLog.LogDebug("ComplianceRealNameAuthCallbackFunc callbackstr: " + resultJsonStr);
            Result result = JsonMapper.ToObject<Result>(resultJsonStr);

            RealNameService realNameService = RealName.Service as RealNameService;
            realNameService?.ComplianceRealNameAuthCallback?.Invoke(result.Error, result.Message);
        }

        [DllImport(PluginName.GSDK)]
        private static extern void ComplianceFetchRealNameState(Callback2P callback);

        [DllImport(PluginName.GSDK)]
        private static extern void ComplianceFetchRealNameInfo(Callback2P callback);

        [DllImport(PluginName.GSDK)]
        private static extern void ComplianceRealNameAuth(Callback1P callback);
#endif
    }

    public class ComplianceRealNameAuthResult
    {
        public int code;
        public string message;
    }
}