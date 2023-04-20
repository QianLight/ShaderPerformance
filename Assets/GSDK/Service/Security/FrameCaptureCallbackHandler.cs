using System;
using UNBridgeLib;
using UNBridgeLib.LitJson;

namespace GSDK
{
    public class FrameCaptureCallbackHandler: BridgeCallBack
    {
        public FrameCaptureDelegate Callback;

        public void OnFrameCaptureEvent(JsonData jsonData)
        {
            var result = convertData(jsonData);
            GLog.LogDebug("onFrameCaptureEvent, result:" + result);
            FrameCaptureInfo frameCaptureInfo = new FrameCaptureInfo();
            if (result.IsSuccess)
            {
                frameCaptureInfo.Detail = jsonData["detail"].ToString();
                frameCaptureInfo.Result = bool.Parse(jsonData["result"].ToString());
            }
            else
            {
                frameCaptureInfo.ErrorMsg = jsonData["errorMsg"].ToString();
            }
            Callback(result, frameCaptureInfo);
        }

        private Result convertData(JsonData jsonData)
        {
            try
            {
                var code = int.Parse(jsonData["code"].ToString());
                var message = jsonData["message"].ToString();
                
                var result = new Result(code, message);
                
                return result;
            }
            catch (FormatException e)
            {
                GLog.LogWarning("code of JsonData error: " + e.Message);
                return null;
            }
        }
    }
}