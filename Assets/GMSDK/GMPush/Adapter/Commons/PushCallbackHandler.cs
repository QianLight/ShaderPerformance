using System;
using UNBridgeLib;
using UNBridgeLib.LitJson;
using UnityEngine;

namespace GMSDK
{
    public class PushCallbackHandler : BridgeCallBack
    {
        public Action<LocalPushResult> localPushResult;

        public void OnLocalPushCallBack(JsonData data)
        {
            LogUtils.D("OnLocalPushCallBack ---");
            LocalPushResult result = SdkUtil.ToObject<LocalPushResult>(data.ToJson());
            SdkUtil.InvokeAction<LocalPushResult>(localPushResult, result);
        }
    }
}