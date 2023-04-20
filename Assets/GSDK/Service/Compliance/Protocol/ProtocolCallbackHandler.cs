using System;
using GMSDK;
using UNBridgeLib;
using UNBridgeLib.LitJson;

namespace GSDK
{
    public class ProtocolCallbackHandler : BridgeCallBack
    {
        public Action<CallbackResult> panelClosedResult;
        
        public Action<ProtocolAddressResult> protocolAddressCallBack;
        
        public void OnPanelClosedCallBack(JsonData jd)
        {
            String json = jd.ToJson();
            LogUtils.D("OnPanelClosedCallBack:" , json);
            CallbackResult ret = SdkUtil.ToObject<CallbackResult>(json);
            SdkUtil.InvokeAction<CallbackResult>(panelClosedResult, ret);
        }
        
        public void OnProtocolAddressCallBack(JsonData jd)
        {
            LogUtils.D("OnProtocolAddressCallBack --- ");
            ProtocolAddressResult result = SdkUtil.ToObject<ProtocolAddressResult>(jd.ToJson());
            SdkUtil.InvokeAction<ProtocolAddressResult> (protocolAddressCallBack, result);
        }
    }
    
}