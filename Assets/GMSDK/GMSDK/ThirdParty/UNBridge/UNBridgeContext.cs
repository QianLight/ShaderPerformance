using UNBridgeLib.LitJson;
using UnityEngine;

namespace UNBridgeLib
{
    /// <summary>
    /// Bridge的上下文
    /// </summary>
    public class UNBridgeContext
    {
        public BridgeMsg Msg;

        public UNBridgeContext(BridgeMsg msg)
        {
            Msg = msg;
        }

        /// <summary>
        /// 回调成功
        /// </summary>
        /// <param name="data">数据</param>
        public void CallBackResult(JsonData data)
        {
            string msg = MsgUtils.PacketSuccessBackMsg(Msg, data).ToJson();
            if (Application.platform == RuntimePlatform.Android)
            {
                AndroidUtils.CallNative(msg);
                LogUtils.D("Android CallBackResult");
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                IosUtils.CallNative(msg);
                LogUtils.D("iOS CallBackResult");
            } else if(Application.platform == RuntimePlatform.WebGLPlayer)
            {
                WebGlUtils.CallNative(msg);
                LogUtils.D("WebGl CallBackResult");
            }
        }

        /// <summary>
        /// 回调失败
        /// </summary>
        /// <param name="code">错误码</param>
        /// <param name="failMsg">错误信息</param>
        public void CallBackFailed(int code, string failMsg)
        {
            string msg = MsgUtils.PacketFailedMsg(Msg, code, failMsg).ToJson();
            if (Application.platform == RuntimePlatform.Android)
            {
                AndroidUtils.CallNative(msg);
                LogUtils.D("Android CallBackFailed");
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                IosUtils.CallNative(msg);
                LogUtils.D("iOS CallBackFailed");
            }
            else if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                WebGlUtils.CallNative(msg);
                LogUtils.D("WebGl CallBackResult");
            }
        }
    }
}
