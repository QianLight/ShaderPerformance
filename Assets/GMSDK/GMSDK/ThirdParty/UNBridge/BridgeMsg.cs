using UNBridgeLib.LitJson;

namespace UNBridgeLib
{
    /// <summary>
    /// 消息的封装
    /// </summary>
    public class BridgeMsg
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 消息类型，消息类型0-call消息，1-listen消息,2-unListen消息，3-event消息
        /// </summary>
        public int Type { get; set; }
        /// <summary>
        /// 回调，唯一
        /// </summary>
        public long CallbackId { get; set; }
        /// <summary>
        /// 错误码
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 消息来源
        /// </summary>
        public int Source { get; set; }
        /// <summary>
        /// 目标接口或者事件
        /// </summary>
        public string Target { get; set; }
        /// <summary>
        /// 错误消息
        /// </summary>
        public string FailMsg { get; set; }
        /// <summary>
        /// 数据
        /// </summary>
        public JsonData Data { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        public JsonData Param { get; set; }

        /// <summary>
        /// Unity侧的SDK版本号
        /// </summary>
        public int UnitySdkVer { get; set; }

        /// <summary>
        /// Native侧的SDK版本号
        /// </summary>
        public int NativeSdkVer { get; set; }
    }
}