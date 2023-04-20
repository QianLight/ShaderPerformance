using UNBridgeLib.LitJson;

namespace GSDK
{
    public static class DeepLinkInnerTools
    {
        public const string TAG = "DeepLink"; 
        
        public static Result Convert(JsonData jsonData)
        {
            if (!jsonData.ContainsKey("code") && !jsonData.ContainsKey("message"))
            {
                GLog.LogWarning("JsonData don't contain \"code\" or \"message\"");
                return null;
            }
            
            int extraErrorCode = 0;
            string extraErrorMessage = "";
            string additionalInfo = "";
            if (jsonData.ContainsKey("extraErrorCode"))
            {
                extraErrorCode = int.Parse(jsonData["extraErrorCode"].ToString());
            }
            if (jsonData.ContainsKey("extraErrorMessage"))
            {
                extraErrorMessage = jsonData["extraErrorMessage"].ToString();
            }
            if (jsonData.ContainsKey("additionalInfo"))
            {
                extraErrorMessage = jsonData["additionalInfo"].ToString();
            }
            return new Result(int.Parse(jsonData["code"].ToString()), jsonData["message"].ToString(), extraErrorCode, extraErrorMessage, additionalInfo);
        }
    }
}
