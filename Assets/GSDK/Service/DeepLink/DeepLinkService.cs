using UNBridgeLib;
using UNBridgeLib.LitJson;
using NotImplementedException = System.NotImplementedException;

namespace GSDK
{
    public class DeepLinkService : IDeepLinkService
    {
        public event DeepLinkParseZLinkEventHandler ParseZLinkEvent;
        public event DeepLinkAttributionEventHandler AttributionEvent;
        
        private const string DeepLinkRegister = "registerDeeplink";
        private const string DeepLinkCheckClipboard = "deeplinkPasteboard";
        private const string DeepLinkGetInvitationCode = "deeplinkGetInvitation";
        private const string DeepLinkShareURL = "deeplinkShareUrl";
        private const string DeepLinkCheckFission = "deeplinkCheckFission";
        private const string DeepLinkDoAttribution = "deeplinkDoAttribution";
        
        private const string RequestOpenResult = "requestDPOpenResult";
        private const string RequestCustomResult = "requestDPCustomResult";
        
#if UNITY_IOS
        private const string DeepLinkStartASAAttribution = "deeplinkStartASAAttribution";
        private const string DeepLinkIsASAAttributed = "deeplinkIsASAAttributed";
#endif
        public void Initialize()
        {
            GLog.LogInfo("Initialize", DeepLinkInnerTools.TAG);
            ListenParseZLinkEvent();
            ListenAttributionEvent();
            UNBridge.Call(DeepLinkRegister);
        }
        
        private void ListenParseZLinkEvent()
        {
            var deepLinkCallbackHandler = new DeepLinkCallbackHandler {ParseZLinkEventHandler = ParseZLinkEvent};
            deepLinkCallbackHandler.OnSuccess = deepLinkCallbackHandler.OnParseZLinkEvent;
            UNBridge.Listen(RequestOpenResult, deepLinkCallbackHandler);
        }

        
        
        private void ListenAttributionEvent()
        {
            var deepLinkCallbackHandler = new DeepLinkCallbackHandler {AttributionEventHandler = AttributionEvent};
            deepLinkCallbackHandler.OnSuccess = deepLinkCallbackHandler.OnAttributionEvent;
            UNBridge.Listen(RequestCustomResult, deepLinkCallbackHandler);
        }
        
        
        
        public void CheckClipboard()
        {
            GLog.LogInfo("CheckClipboard", DeepLinkInnerTools.TAG);
            UNBridge.Call(DeepLinkCheckClipboard,null);
        }
        
#if UNITY_ANDROID
        public void DoAttribution()
        {
            GLog.LogInfo("DoAttribution", DeepLinkInnerTools.TAG);
            UNBridge.Call(DeepLinkDoAttribution);
        }
#endif
        
        public void GetInvitationCode(DeepLinkInvitationCodeCallback callback)
        {
            GLog.LogInfo("GetInvitationCode", DeepLinkInnerTools.TAG);
            var callbackHandler = new DeepLinkCallbackHandler {InvitationCodeCallback = callback,};
            callbackHandler.OnSuccess = callbackHandler.OnInvitationCodeCallback;
            UNBridge.Call(DeepLinkGetInvitationCode, null, callbackHandler);   
        }

        public string GenerateShareURL(string url, string invitationCode,string extraJson="")
        {
            GLog.LogInfo(string.Format("GenerateShareURL, url:{0}, invitationCode:{1}, extraJson:{2}", 
                             url, invitationCode, extraJson), DeepLinkInnerTools.TAG);
            JsonData param = new JsonData();
            param["url"] = url;
            param["invitationCode"] = invitationCode;
            JsonData extra = new JsonData();
            extra.SetJsonType(JsonType.Object);
            try
            {
                extra = JsonMapper.ToObject(extraJson);
                extra.SetJsonType(JsonType.Object);
            }
            catch
            {
                //转换成Json失败，不向下传递参数
            }
            param["extra"] = extra;
            object obj = UNBridge.CallSync(DeepLinkShareURL,param);
            var res = obj != null ? (string) obj : "";
            return res;
        }

        public void BindFission(DeepLinkFissionCallback callback, string invitationCode = "")
        {
            GLog.LogInfo("BindFission, invitationCode:" + invitationCode, DeepLinkInnerTools.TAG);
            JsonData param = new JsonData();
            param["invitationCode"] = invitationCode;
            var callbackHandler = new DeepLinkCallbackHandler {FissionCallback = callback,};
            callbackHandler.OnSuccess = callbackHandler.OnFissionCallback;
            UNBridge.Call(DeepLinkCheckFission,param,callbackHandler);
        }

#if  UNITY_IOS
        public void StartASAAttribution()
        {
            GLog.LogInfo("StartASAAttribution");
            JsonData param = new JsonData();
            UNBridge.Call(DeepLinkStartASAAttribution, param);
        }

        public bool IsASAAttributed()
        {
            GLog.LogInfo("IsASAAttributed");
            JsonData param = new JsonData();
            return (bool)UNBridge.CallSync(DeepLinkIsASAAttributed, param);
        }
#endif
    }
}
