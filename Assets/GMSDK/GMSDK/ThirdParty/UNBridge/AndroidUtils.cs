using UNBridgeLib.LitJson;
using UnityEngine;

namespace UNBridgeLib
{

    /// <summary>
    /// 处理Android通讯的工具类
    /// </summary>
    internal static class AndroidUtils
    {
        private static AndroidJavaClass _androidJavaClass;

        /// <summary>
        /// 确保在主线程执行初始化。
        /// </summary>
        public static void Init()
        {
        }

        /// <summary>
        /// 发送消息到Native
        /// </summary>
        /// <param name="target"></param>
        /// <param name="data"></param>
        //public static void SendEventNative(string target, JsonData data)
        //{
        //    JsonData packet = MsgUtils.PacketEventMsg(target, data);
        //    // Loom.QueueOnMainThread(() => { CallNative(packet.ToJson().UnicodeToUtf8()); });
        //    CallNative(packet.ToJson().UnicodeToUtf8());

        //}

        ///<summary>
        /// 异步Call通讯
        /// </summary>
        ///<param name="type">消息类型0-call消息，1-listen消息,2-unlisten消息，3-event消息</param>
        ///<param name="target">目标接口</param>
        ///<param name="param">参数</param>
        ///
        ///
        public static void CallNative(int type, string target, JsonData param, long callbackId)
        {
            string packet = MsgUtils.PacketCallMsgV2(type, target, param, callbackId);
            // Loom.QueueOnMainThread(() => { CallNative(packet.ToJson().UnicodeToUtf8()); });
            CallNative(packet.UnicodeToUtf8());

        }


        /// <summary>
        /// 异步Call通讯
        /// </summary>
        /// <param name="msg"></param>
        public static void CallNative(string msg)
        {
            LogUtils.D("CallNative:", msg);
            if (_androidJavaClass == null)
            {
                _androidJavaClass = new AndroidJavaClass("com.bytedance.unbridge.UNBridge");
            }
            _androidJavaClass.CallStatic("handleMsgFromUnity", msg);
        }

        /// <summary>
        /// 同步Call通讯
        /// </summary>
        ///<param name="type">消息类型0-call消息，1-listen消息,2-unlisten消息，3-event消息</param>
        ///<param name="target">目标接口</param>
        ///<param name="param">参数</param>
        ///
        public static object CallNativeSync(int type, string target, JsonData param)
        {
            string packet = MsgUtils.PacketCallSyncMsgV2(type, target, param);
            return CallNativeSync(packet.UnicodeToUtf8());

        }

        /// <summary>
        /// 同步Call通讯
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static object CallNativeSync(string msg)
        {
            LogUtils.D("CallNativeSync:", msg);
            if (_androidJavaClass == null)
            {
                _androidJavaClass = new AndroidJavaClass("com.bytedance.unbridge.UNBridge");
            }
            string result = _androidJavaClass.CallStatic<string>("handleMsgFromUnitySync", msg);
            LogUtils.D("CallNativeSync result:",result);
            return MsgUtils.ParseResult(result);
        }





    }
}
