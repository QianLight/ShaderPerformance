using System.Collections;
using System.Collections.Generic;
using UNBridgeLib.LitJson;
using System.Runtime.InteropServices;
using GMSDK;
#if GMEnderOn && UNITY_EDITOR
using Ender;
#endif

namespace UNBridgeLib
{
    public class EnderUtils
    {

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
            CallNative(packet.UnicodeToUtf8(), false);
        }

        /// <summary>
        /// 异步Call通讯，确保调用在主线程执行
        /// </summary>
        /// <param name="msg"></param>
        public static object CallNative(string packet, bool hasRetValue)
        {

#if GMEnderOn && UNITY_EDITOR
            
            string result;
            result = GMEnderMgr.instance.callEnder(Ender.LitJson.JsonMapper.ToObject(packet), hasRetValue);
            if (hasRetValue)
            {
                if (result == null)
                {
                    return null;
                }
                return MsgUtils.ParseResult(result);
            }
#endif
            return null;
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
            return CallNative(packet.UnicodeToUtf8(), true);
        }
    }
}