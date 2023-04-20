using UNBridgeLib.LitJson;

namespace UNBridgeLib
{
    /// <summary>
    /// 回调接口
    /// </summary>
    public class BridgeCallBack
    {
        public OnSuccessDelegate OnSuccess { get; set; }
        public OnFailedDelegate OnFailed { get; set; }
        public OnTimeoutDelegate OnTimeout { get; set; }
        /// <summary>
        /// 回调被添加的事件,ns
        /// </summary>
        public long AddTime { get; set; }

        /// <summary>
        /// 默认Call超时,默认使用通用的超时，10秒
        /// </summary>
        public long TimeoutTime { get; set; }
    }

    /// <summary>
    /// 回调成功
    /// </summary>
    /// <param name="data"></param>
    public delegate void OnSuccessDelegate(JsonData data);
    /// <summary>
    /// 回调失败
    /// </summary>
    /// <param name="code">失败码</param>
    /// <param name="failMsg">失败原因</param>
    public delegate void OnFailedDelegate(int code, string failMsg);
    /// <summary>
    /// 超时
    /// </summary>
    public delegate void OnTimeoutDelegate();
}