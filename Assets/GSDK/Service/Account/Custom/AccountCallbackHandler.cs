using System;
using UNBridgeLib;
using UNBridgeLib.LitJson;

namespace GSDK.CustomAccount
{
    public class AccountCallbackHandler : BridgeCallBack
    {
        public Action<Result> LoginEventHandler;

        public void OnLoginEvent(JsonData jsonData)
        {
            var result = InnerTools.ConvertJsonToResult(jsonData);
            GLog.LogDebug("OnLoginEvent, result:" + result, AccountInnerTools.TAG);
            LoginEventHandler(result);
        }
    }
}