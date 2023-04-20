using UNBridgeLib.LitJson;

namespace UNBridgeLib
{
    /// <summary>
    /// BridgeAPI的包装类
    /// </summary>
    public class BridgeAPI
    {
        public OnCallBridgeAPI OnCallBridgeAPI { get; set; }
    }

    /// <summary>
    /// BridgeAPI的委托
    /// </summary>
    /// <param name="context"></param>
    /// <param name="para"></param>
    public delegate void OnCallBridgeAPI(UNBridgeContext context, JsonData para);
}