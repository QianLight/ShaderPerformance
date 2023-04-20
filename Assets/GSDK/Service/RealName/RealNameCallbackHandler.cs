using System;
using GMSDK;
using UNBridgeLib;
using UNBridgeLib.LitJson;

namespace GSDK
{
    public class RealNameCallbackHandler : BridgeCallBack
    {
        public Action<ComplianceRealNameAuthResult> ComplianceRealNameAuthCallback;
        public void OnComplianceRealNameAuthCallback(JsonData jd)
        {
            ComplianceRealNameAuthResult result = SdkUtil.ToObject<ComplianceRealNameAuthResult>(jd.ToJson());
            SdkUtil.InvokeAction<ComplianceRealNameAuthResult>(ComplianceRealNameAuthCallback, result);	
        }
    }
}
