using UNBridgeLib;
using UNBridgeLib.LitJson;
using UnityEngine;

namespace GSDK
{
    public class DeepLinkCallbackHandler:BridgeCallBack
    {
        public DeepLinkInvitationCodeCallback InvitationCodeCallback;
        public DeepLinkFissionCallback FissionCallback;

        public DeepLinkParseZLinkEventHandler ParseZLinkEventHandler;
        public DeepLinkAttributionEventHandler AttributionEventHandler;

        public void OnInvitationCodeCallback(JsonData jsonData)
        {
            GLog.LogInfo("Handle InvitationCodeCallback:" + jsonData.ToJson(), DeepLinkInnerTools.TAG);
            var result = DeepLinkInnerTools.Convert(jsonData);
            if (!result.IsSuccess)
            {
                InvitationCodeCallback.Invoke(result,"");
            }
            else
            {
                string invitationCode = "";
                if (jsonData.ContainsKey("data"))
                {
                    invitationCode = jsonData["data"].ToString();
                }
                InvitationCodeCallback.Invoke(result,invitationCode);
            }
        }

        public void OnFissionCallback(JsonData jsonData)
        {
            GLog.LogInfo("Handle FissionCallback:"+jsonData.ToJson(), DeepLinkInnerTools.TAG);
            var result = DeepLinkInnerTools.Convert(jsonData);
            if (!result.IsSuccess)
            {
                FissionCallback.Invoke(result,"");
            }
            else
            {
                string extra = "";
                if (jsonData.ContainsKey("data"))
                {
                    var data = jsonData["data"];
                    extra = data.IsBasicType ? data.ToString() : data.ToJson();
                }
                FissionCallback.Invoke(result,extra);
            }
        }
        
        public void OnParseZLinkEvent(JsonData jsonData)
        {
            var dlOpenInfo = new DeepLinkZLinkInfo {Scheme = jsonData["scheme"].ToString()};
            try
            {
                dlOpenInfo.SourceType = (DeepLinkSourceType) int.Parse(jsonData["type"].ToString());
            }
            catch
            {
                dlOpenInfo.SourceType = DeepLinkSourceType.Unknown;
            }

            if (ParseZLinkEventHandler != null)
            {
                GLog.LogInfo("Handle OpenResultEvent:" + jsonData.ToJson(), DeepLinkInnerTools.TAG);
                ParseZLinkEventHandler.Invoke(dlOpenInfo);
            }
            else
            {
                GLog.LogInfo("OpenResultEvent is null", DeepLinkInnerTools.TAG);
            }
        }
        
        public void OnAttributionEvent(JsonData jsonData)
        {
            var dlCustomInfo = new DeepLinkAttributionInfo{Data = jsonData["data"].ToJson()};
            try
            {
                dlCustomInfo.SourceType = (DeepLinkSourceType) int.Parse(jsonData["type"].ToString());
            }
            catch
            {
                dlCustomInfo.SourceType = DeepLinkSourceType.Unknown;
            }

            if (AttributionEventHandler != null)
            {
                GLog.LogInfo("Handle AttributionEvent:" + jsonData.ToJson(), DeepLinkInnerTools.TAG);
                AttributionEventHandler.Invoke(dlCustomInfo);
            }
            else
            {
                GLog.LogInfo("AttributionEvent is null", DeepLinkInnerTools.TAG);
            }
        }
    }
}
