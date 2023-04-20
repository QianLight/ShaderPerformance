using System;
using UNBridgeLib.LitJson;
using UnityEngine;

namespace UNBridgeLib
{
    /// <summary>
    /// 处理消息
    /// </summary>
    internal static class MsgUtils
    {

        public static long GetMsgId()
        {
            return DateTime.Now.Ticks;
        }

        public static long GetCallBackId()
        {
            return DateTime.Now.Ticks;
        }

        /// <summary>
        /// 打包成功的回包
        /// </summary>
        /// <param name="msg">请求的消息</param>
        /// <param name="data">回包的数据</param>
        /// <returns></returns>
        public static JsonData PacketSuccessBackMsg(BridgeMsg msg, JsonData data)
        {
            JsonData packet = new JsonData();
            packet["unity_sdk_ver"] = BridgeVersion.VERSION_CODE;
            packet["msg_id"] = msg.Id;
            packet["source"] = msg.Source;
            packet["type"] = msg.Type;
            packet["target"] = msg.Target;
            packet["callback_id"] = msg.CallbackId;
            packet["code"] = 0;
            packet["failMsg"] = null;
            if (data != null)
            {
                packet["data"] = data;
            }

            return packet;
        }

        /// <summary>
        /// 打包失败的回包
        /// </summary>
        /// <param name="msg">请求的消息</param>
        /// <param name="code">错误码</param>
        /// <param name="failMsg">错误信息</param>
        /// <returns></returns>
        public static JsonData PacketFailedMsg(BridgeMsg msg, int code, string failMsg)
        {
            JsonData packet = new JsonData();
            packet["unity_sdk_ver"] = BridgeVersion.VERSION_CODE;
            packet["msg_id"] = msg.Id;
            packet["source"] = msg.Source;
            packet["type"] = msg.Type;
            packet["target"] = msg.Target;
            packet["callback_id"] = msg.CallbackId;
            packet["code"] = code;
            packet["failMsg"] = failMsg;
            packet["data"] = null;

            return packet;
        }

        /// <summary>
        /// 打包异步Call消息
        /// </summary>
        /// <param name="type">消息类型0-call消息，1-listen消息,2-unListen消息，3-event消息</param>
        /// <param name="target">目标接口</param>
        /// <param name="param">参数</param>
        /// <param name="callbackId">回调的id</param>
        /// <returns></returns>
        public static JsonData PacketCallMsg(int type, string target, JsonData param, long callbackId)
        {
            JsonData packet = new JsonData();
            packet["unity_sdk_ver"] = BridgeVersion.VERSION_CODE;
            packet["msg_id"] = GetMsgId();
            packet["source"] = BridgeCore.SOURCE_UNITY;
            packet["type"] = type;
            packet["target"] = target;
            if (callbackId > 0)
            {
                packet["callback_id"] = callbackId;
            }
            if (param != null && !string.IsNullOrEmpty(param.ToJson()))
            {
                packet["param"] = param;
            }
            return packet;
        }
        
        public static string PacketCallMsgV2(int type, string target, JsonData param, long callbackId)
        {
            CallModel model = new CallModel
            {
                unity_sdk_ver = BridgeVersion.VERSION_CODE,
                msg_id = GetMsgId(),
                source = BridgeCore.SOURCE_UNITY,
                type = type,
                target = target
            };
            if (callbackId > 0)
            {
                model.callback_id = callbackId;
            }
            if (param != null)
            {
                var json = param.ToJson();
                if (!string.IsNullOrEmpty(json))
                {
                    model.param = json;
                }

            }

            return JsonUtility.ToJson(model);
            
        }

        /// <summary>
        /// 打包同步Call消息
        /// </summary>
        /// <param name="type">消息类型0-call消息，1-listen消息,2-unListen消息，3-event消息</param>
        /// <param name="target">目标接口</param>
        /// <param name="param">参数</param>
        /// <returns></returns>
        public static JsonData PacketCallSyncMsg(int type, string target, JsonData param)
        {
            JsonData packet = new JsonData();
            packet["unity_sdk_ver"] = BridgeVersion.VERSION_CODE;
            packet["msg_id"] = GetMsgId();
            packet["type"] = type;
            packet["source"] = BridgeCore.SOURCE_UNITY;
            packet["target"] = target;
            if (param != null && !string.IsNullOrEmpty(param.ToJson()))
            {
                packet["param"] = param;
            }
            return packet;
        }
        
        public static string PacketCallSyncMsgV2(int type, string target, JsonData param)
        {
            CallModel model = new CallModel
            {
                unity_sdk_ver = BridgeVersion.VERSION_CODE,
                msg_id = GetMsgId(),
                source = BridgeCore.SOURCE_UNITY,
                type = type,
                target = target
            };
            if (param != null)
            {
                string json = param.ToJson();
                if (!string.IsNullOrEmpty(json))
                {
                    model.param = json;
                }

            }

            return JsonUtility.ToJson(model);
        }

        /// <summary>
        /// 打包事件的消息
        /// </summary>
        /// <param name="target">目标事件</param>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public static JsonData PacketEventMsg(string target, JsonData data)
        {
            JsonData packet = new JsonData();
            packet["unity_sdk_ver"] = BridgeVersion.VERSION_CODE;
            packet["msg_id"] = GetMsgId();
            packet["type"] = BridgeCore.TYPE_EVENT;
            packet["target"] = target;
            if(data!=null && !string.IsNullOrEmpty(data.ToJson()))
            {
                packet["data"] = data;
            }
            packet["code"] = 0;
            packet["failMsg"] = null;
            packet["source"] = BridgeCore.SOURCE_UNITY;
            return packet;
        }

        /// <summary>
        /// 解析消息
        /// </summary>
        /// <param name="json">JSON格式的消息</param>
        /// <returns></returns>
        public static BridgeMsg ParseMsg(string json)
        {
            JsonData packet = JsonMapper.ToObject(json);
            BridgeMsg msg = new BridgeMsg();
            msg.Id = DataUtils.GetLong(packet, "msg_id");
            msg.Type = DataUtils.GetInt(packet, "type");
            msg.CallbackId = DataUtils.GetLong(packet, "callback_id");
            msg.Code = DataUtils.GetInt(packet, "code");
            msg.Source = DataUtils.GetInt(packet, "source");
            msg.NativeSdkVer = DataUtils.GetInt(packet, "native_sdk_ver");
            msg.UnitySdkVer = BridgeVersion.VERSION_CODE;
            msg.Target = DataUtils.GetString(packet, "target");
            msg.FailMsg = DataUtils.GetString(packet, "failMsg");
            msg.Data = DataUtils.GetData(packet, "data");
            msg.Param = DataUtils.GetData(packet, "param");
            return msg;
        }

  


        /// <summary>
        /// 同步调用的结果转换
        /// </summary>
        /// <param name="json">JSON字符串</param>
        /// <returns></returns>
        public static object ParseResult(string json)
        {
            JsonData packet = JsonMapper.ToObject(json);
            int code = DataUtils.GetInt(packet, "code");
            string failMsg = DataUtils.GetString(packet, "failMsg");

            if (code != 0)
            {
                LogUtils.E("{code:" + code + "],failMsg:" + failMsg + "}");
            }
            object value = DataUtils.GetObject(packet, "value");
            return value;
        }
    }
}