using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GSDK.RNU
{
    /*
     * 维持一个可执行Action队列，用来实现WebSocketJSExecutor和远端JS引擎的交互。
     * * action的执行顺序是需要严格保证的，在前一个action的结果没有回来之前 不允许执行下一个action,
     * * action需要保证在主线程执行
     */
    public class WebSocketActionIFrameListener : IFrameListener
    {
        // CallFunction/InvokeCallback 的调用可能发生在任何线程，这里需要保证线程安全
        private readonly Hashtable reqActions = Hashtable.Synchronized(new Hashtable());
        private readonly Hashtable resActions = Hashtable.Synchronized(new Hashtable());
        private int reqId;
        private int currentReqId;
        

        // websocket的消息统一在其他线程获取，故这里需要volatile保证res引用的线程可见性
        private volatile Dictionary<string, object> res;

        public void Do()
        {

            if (res != null)
            {
                var resId = (int) res["replyID"];
                var result = (ArrayList) res["result"];

                if (resActions.ContainsKey(resId))
                {
                    var action = (Action<ArrayList>) resActions[resId];
                    resActions.Remove(resId);
                    action(result);
                }

                currentReqId++;
                res = null;
            }
            
            
            if (!reqActions.ContainsKey(currentReqId)) return;
            var currentAction = (Action) reqActions[currentReqId];
            reqActions.Remove(currentReqId);
            currentAction();
        }

        public int GetReqId()
        {
            return reqId++;
        }

        // 添加action。 
        // 需要注意的是当一个action 执行被阻塞的时候，再次添加可以覆盖old action。 现在这种情况用于prepareJS阶段
        public void AddAction(int id, Action reqAction, Action<ArrayList> resAction = null)
        {
            if (reqActions.ContainsKey(id) && currentReqId != id)
            {
                Util.Log("req has invoked with id:" + id);
                return;
            }

            if (reqActions.ContainsKey(id))
            {
                reqActions.Remove(id); 
            }

            if (resActions.ContainsKey(id))
            {
                resActions.Remove(id);
            }

            reqActions.Add(id, reqAction);

            if (resAction != null)
            {
                resActions.Add(id, resAction);
            }
        }

        public void ConsumeRes(Dictionary<string, object> resDic)
        {
            res = resDic;
        }
    }
}