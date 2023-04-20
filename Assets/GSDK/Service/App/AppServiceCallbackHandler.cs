using System;
using UNBridgeLib;
using UNBridgeLib.LitJson;

namespace GSDK
{
    public class AppServiceCallbackHandler : BridgeCallBack
    {
        public RequestDeviceInfoCallback DeviceInfoCallback;
        public RequestDeviceInfoUpdateEventHandler DeviceInfoUpdateHandler;

        public void onRequestDeviceInfocallback(JsonData jsonData)
        {
            UNBridge.UnListen(AppService.SDKFetchDeviceIdResultEvent);
            GLog.LogInfo("onRequestDeviceInfocallback:" + jsonData.ToJson());
            if (DeviceInfoCallback == null) { return; }
            var result = Result.ResultFromJson(jsonData);
            if (result.IsSuccess)
            {
                string did = jsonData["deviceID"].ToString();
                string iid = jsonData["installID"].ToString();
                DeviceInfoCallback.Invoke(result, did, iid);
            }
            else {
                DeviceInfoCallback.Invoke(result, null, null);
            }

        }

        public void onDeviceInfoUpdateEventHandler(JsonData jsonData)
        {
            UNBridge.UnListen(AppService.SDKDeviceInfoUpdateEvent);
            GLog.LogInfo("onDeviceInfoUpdateEventHandler:" + jsonData.ToJson());
            if (DeviceInfoUpdateHandler == null) { return; }
            string did = jsonData["deviceID"].ToString();
            string iid = jsonData["installID"].ToString();
            DeviceInfoUpdateHandler.Invoke(did, iid);
        }
    }
}


