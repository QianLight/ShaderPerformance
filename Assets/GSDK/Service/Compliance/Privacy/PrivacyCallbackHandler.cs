
using System;
using GMSDK;
using UNBridgeLib;
using UNBridgeLib.LitJson;

namespace GSDK
{
    public class PrivacyCallbackHandler : BridgeCallBack
    {
        public Action<ShowPrivacyResult> showPrivacyCallBack;

        public void OnShowPrivacyCallBack(JsonData jd)
        {
            ShowPrivacyResult ret = SdkUtil.ToObject<ShowPrivacyResult>(jd.ToJson());
            SdkUtil.InvokeAction<ShowPrivacyResult>(showPrivacyCallBack, ret);
        }
    }
}

