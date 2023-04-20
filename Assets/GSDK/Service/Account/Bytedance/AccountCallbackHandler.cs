using System.Collections.Generic;
using UNBridgeLib;
using UNBridgeLib.LitJson;

namespace GSDK
{
    public class AccountCallbackHandler:BridgeCallBack
    {
        public AccountLinkInfoDelegate AccountLinkInfoDelegate;

        public void HandleLinkInfo(JsonData jsonData)
        {
            GLog.LogDebug("HandleLinkInfoResult:" + jsonData.ToJson(), AccountInnerTools.Tag);
            var result = InnerTools.ConvertJsonToResult(jsonData);
            var linkInfo = result.IsSuccess ? AccountInnerTools.Convert(jsonData) : new List<LinkInfo>();
            if (AccountLinkInfoDelegate != null)
            {
                AccountLinkInfoDelegate(result, linkInfo);
            }
            else
            {
                GLog.LogError("AccountLinkInfoDelegate is null, ", AccountInnerTools.Tag);
            }
        }
    }
}
