using UNBridgeLib.LitJson;

namespace UNBridgeLib
{
    /// <summary>
    /// Bridge接口
    /// </summary>
    public interface IBridgeAPI
    {
        void OnCall(UNBridgeContext context, JsonData param);
    }
}