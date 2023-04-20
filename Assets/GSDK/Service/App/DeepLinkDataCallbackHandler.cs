using UNBridgeLib;
using UNBridgeLib.LitJson;

namespace GSDK
{
    public class DeepLinkUrlCallbackHandler : BridgeCallBack
    {
        public RequestDeepLinkURLEventHandler DeepLinkUrlEventHandler;

        public void OnDeeplinkURLCallback(JsonData jsonData)
        {
            GLog.LogInfo("Handle OnDeeplinkDataCallback:" + jsonData.ToJson(), "AppService");
            string deeplinkUrl = null;
            if (jsonData.ContainsKey("data"))
            {
                deeplinkUrl = jsonData["data"].ToString();
            }

            if (DeepLinkUrlEventHandler != null)
            {
                DeepLinkUrlEventHandler.Invoke(deeplinkUrl);
            }
        }
    }
}