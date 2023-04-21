using UNBridgeLib.LitJson;
using System.Runtime.InteropServices;

namespace UNBridgeLib
{
    /// <summary>
    /// iOS的通信接口
    /// </summary>
    internal static class IosUtils
    {

        /// <summary>
        /// 确保在主线程执行初始化。
        /// </summary>
        public static void Init()
        {
          
        }

        //以下接口为iOS使用，用于调用OC代码。

#if UNITY_IOS
        // [DllImport("__Internal")]
        // private static extern void iosHandleMsgFromUnity(string msg);
        //
        // [DllImport("__Internal")]
        // private static extern string iosHandleMsgFromUnitySync(string msg);
#endif

        /// <summary>
        /// 发送消息到Native
        /// </summary>
        /// <param name="target"></param>
        /// <param name="data"></param>
        //public static void SendEventNative(string target, JsonData data)
        //{
        //    JsonData packet = MsgUtils.PacketEventMsg(target, data);
        //    CallNative(packet.ToJson());

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
            CallNative(packet.UnicodeToUtf8());
        }

        /// <summary>
        /// 异步Call通讯，确保调用在主线程执行
        /// </summary>
        /// <param name="msg"></param>
        public static void CallNative(string msg)
        {
            LogUtils.D("CallNative:", msg);
// #if UNITY_IOS
// 			iosHandleMsgFromUnity(msg);
// #endif          
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
            string result = "";
// #if UNITY_IOS
//             result = iosHandleMsgFromUnitySync(msg);
// #endif
            LogUtils.D("CallNativeSync result:",result);
            return MsgUtils.ParseResult(result);
        }

    }
}