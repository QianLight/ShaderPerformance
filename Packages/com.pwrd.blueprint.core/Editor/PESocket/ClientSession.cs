using Protocol;
using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace PENet
{
    public class ClientSession : PESession<NetMsg>
    {

        /// <summary>
        /// 请勿监听该消息，因为该消息不在unity主线程内执行，监听ServerStart里的OnReceiveMessage消息
        /// </summary>
        public static event Action<string> OnReceiveMessage;

        protected override void OnConnected()
        {
            UnityEditor.EditorApplication.playModeStateChanged += Bluepirnt.Debug.BlueprintDebugController.ChangedPlaymodeState;
            UnityEngine.Debug.Log("Client OnLine.");

            MessageData data = new MessageData()
            {
                messageType = MessageType.Connected,
                data = BpClient.UnityAssetPath,
            };
           
            SendMsg(new NetMsg
            {
                text = UnityEngine.JsonUtility.ToJson(data),
            });

            MessageData data2 = new MessageData()
            {
                messageType = MessageType.UnityProcessId,
                data = Process.GetCurrentProcess().Id.ToString(),
            };

            SendMsg(new NetMsg
            {
                text = UnityEngine.JsonUtility.ToJson(data2),
            });
        }


        protected override void OnReciveMsg(NetMsg msg)
        {
            UnityEngine.Debug.Log("Client Request:" + msg.text);
            OnReceiveMessage?.Invoke(msg.text);
            BpClient.dataQueue.Enqueue(msg.text);
        }

        protected override void OnDisConnected()
        {
            BpClient.IsConnected = false;
            BpClient.GetAllBpProcesses();
            UnityEngine.Debug.Log("Client OffLine.");
        }

        public void SendMessage(string str)
        {
            SendMsg(new NetMsg
            {
                text = str
            });
        }
    }
}