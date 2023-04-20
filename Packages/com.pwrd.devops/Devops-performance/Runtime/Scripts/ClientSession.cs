using PENet;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Devops.Performance
{
    public class ClientSession : PESession
    {
        public static Action OnServerConnected;
        public static Action OnServerDisconnected;
        Queue<int> mQueueMsg = new Queue<int>();
        protected override void OnConnected()
        {
            base.OnConnected();
            //OnServerConnected?.Invoke();
            //NetMsg msg = new NetMsg(EMessageType.Connect);
            //OnReciveMsg(msg);
            mQueueMsg.Enqueue((int)EMessageType.Connect);
        }

        /// <summary>
        /// Receive network message
        /// </summary>
        //protected override void OnReciveMsg(NetMsg msg)
        //{
        //    base.OnReciveMsg(msg);
        //    //mQueueMsg.Enqueue(msg);
        //}

        protected override void OnReciveMsg(int msgId, byte[] buffer)
        {
            base.OnReciveMsg(msgId, buffer);
            mQueueMsg.Enqueue(msgId);
        }

        public void OnUpdate()
        {
            while (mQueueMsg.Count > 0)
            {
                int msgId = mQueueMsg.Dequeue();
                switch((EMessageType)msgId)
                {
                    case EMessageType.Connect:
                        OnServerConnected?.Invoke();
                        break;
                    case EMessageType.Disconnect:
                        OnServerDisconnected?.Invoke();
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Disconnect network
        /// </summary>
        protected override void OnDisConnected()
        {
            base.OnDisConnected();
            mQueueMsg.Enqueue((int)EMessageType.Disconnect);
        }
    }
}