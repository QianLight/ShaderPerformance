using GMSDK;
using System;
using UNBridgeLib.LitJson;
using System.Runtime.InteropServices;


namespace GSDK
{
    public class SecurityService : ISecurityService
    {
        public event FrameCaptureDelegate FrameCaptureEvent;
        
        private readonly MainSDK _gsdk;

        public SecurityService()
        {
            _gsdk = GMSDKMgr.instance.SDK;
        }

        public void ReportUserInfo(AccountType userType, string roleId, string serverId)
        {
#if UNITY_STANDALONE_WIN && !GMEnderOn
            GPSetUserInfo(roleId, serverId);
#else
            _gsdk.SetUserInfo((int) userType, roleId, serverId);
#endif
        }

        public byte[] ReadData()
        {
            var result = _gsdk.SdkReadPacketText();
            if (result == null)
            {
                GLog.LogError("ReadPacketText returns null.");
                return null;
            }

            // bridge会做多一次encode，因此这边需要把string做一次decode
            var packet = _gsdk.SdkReadPacketText();
            return Convert.FromBase64String(packet);
        }

        public Result WriteData(byte[] data)
        {
            if (data == null)
            {
                GLog.LogError("data of WritePacketText is null");
                return new Result(ErrorCode.SecurityDataNullError, "data of WritePacketText is null");
            }

            // bridge会做多一次decode，因此这边需要把string做一次encode
            var dataString = global::System.Text.Encoding.UTF8.GetString(data);
            Result res = SecurityInnerTools.ConvertError(_gsdk.SdkWritePacketText(dataString));
            return res;
        }

        public void SetPriority(PerformancePriority priority)
        {
            _gsdk.SetPriority((int) priority);
        }

        private const string RequestFrameCapture = "isFrameCapture";
        private const string RequestFrameCaptureWithTimeout = "isFrameCaptureWithTimeout";
        
        public void isFrameCapture(FrameCaptureDelegate callbackDelegate)
        {
            GLog.LogInfo("call isFrameCapture");
            var callbackHandler = new FrameCaptureCallbackHandler {  Callback = callbackDelegate};
            callbackHandler.OnSuccess = callbackHandler.OnFrameCaptureEvent;
            UNBridge.Call(RequestFrameCapture, null, callbackHandler);
            
        }

        public void isFrameCapture(FrameCaptureDelegate callbackDelegate, long timeout)
        {
            GLog.LogInfo("call isFrameCapture, timeout: " + timeout);
            var callbackHandler = new FrameCaptureCallbackHandler {  Callback = callbackDelegate};
            callbackHandler.OnSuccess = callbackHandler.OnFrameCaptureEvent;
            var param = new JsonData();
            param["timeout"] = timeout;
            UNBridge.Call(RequestFrameCaptureWithTimeout, param, callbackHandler);
        }

#if UNITY_STANDALONE_WIN && !GMEnderOn
        public bool IsGPDllExist()
        {
            return GSDKIsGPDllExist();
        }

        public void DisableGP()
        {
            GSDKDisableGP();
        }
#endif

#if UNITY_STANDALONE_WIN && !GMEnderOn

        [DllImport(PluginName.GSDK)]
        private static extern void GPSetUserInfo(string roleId, string serverId);

        [DllImport(PluginName.GSDK)]
        private static extern bool GSDKIsGPDllExist();

        [DllImport(PluginName.GSDK)]
        private static extern void GSDKDisableGP();


#endif
    }
}