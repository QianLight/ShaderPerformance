using GMSDK;
using UNBridgeLib.LitJson;

namespace GSDK
{
    public class ScanResult : CallbackResult
    {
        public string result;
    }
    
    public class QRCodeInnerTools
    {
        public static ScanResult ConvertToScanResult(JsonData jsonData)
        {
            return SdkUtil.ToObject<ScanResult>(jsonData.ToJson());
        }
    }
}