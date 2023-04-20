using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;


namespace GSDK.RNU
{
    public class WebSocketJSExecutor : JSExecutor
    {
        private readonly SimpleWebSocket webSocket;
        private string fbBatchedBridgeConfig;
        private WebSocketActionIFrameListener actionIFrame;
        
        private RNUMainCore context;


        static WebSocketJSExecutor()
        {
            JSONUtil.Init();
        }
        
        public WebSocketJSExecutor(string ip, int port, RNUMainCore context)
        {
            this.context = context;
            this.webSocket = new SimpleWebSocket();

            this.webSocket.OnMessage(this.OnMessage);
            this.webSocket.OnClose(() =>
            {
                Util.Log("webSocket OnClose...");
            });
            
            actionIFrame = new WebSocketActionIFrameListener();
            StaticCommonScript.AddFrameListener(actionIFrame);
            try
            {
                var wsUrl = "ws://" + ip + ":" + port + "/debugger-proxy?role=client";
                this.webSocket.Connect(wsUrl);
            }
            catch (Exception e)
            {
                context.debugPanel.ShowError("Connect to " + ip + ":" + port + " error\n" + e.Message + "\n" + e.StackTrace);
                return;
            }
            
            // 打开浏览器调试面板，JS收到这个请求之后，会在没有打开debbuger页面的时候，打开debbuger
            StaticCommonScript.StaticStartCoroutine(NoticeServerToOpenDebugger(ip, port));

            StaticCommonScript.StaticStartCoroutine(PrepareJsRuntime());
        }

        
        /**
         * PrepareJSRuntime。 有可能在PrepareJSRuntime发生的时候，远端JSRuntime还没准备好，这里需要在没有PrepareJSRuntime
         * 没有收到成功消息的时候，不断的Prepare.
         */
        private IEnumerator PrepareJsRuntime()
        {
            var reqId = actionIFrame.GetReqId();
            Util.Log("PrepareJSRuntime...");

            var hasPrepare = false;

            while (!hasPrepare)
            {
                actionIFrame.AddAction(reqId, () =>
                {
                    Hashtable o = new Hashtable
                    {
                        {"id", reqId},
                        {"method", "prepareJSRuntime"}
                    };
                    Send(o);
                }, (data) =>
                {
                    hasPrepare = true;
                });
                yield return new WaitForSeconds(0.2f);
            }
        }
        
        public void LoadApplicationScript(string uri)
        {
            var reqId = actionIFrame.GetReqId();
            actionIFrame.AddAction(reqId, () =>
            {
                Hashtable o = new Hashtable()
                {
                    {"id", reqId},
                    {"method", "executeApplicationScript"},
                    {"url", uri},
                    {
                        "inject", new Hashtable()
                        {
                            {
                                "__fbBatchedBridgeConfig", fbBatchedBridgeConfig
                            }
                        }
                    }
                };
                context.debugPanel.ReloadJs();
                Send(o);
            });
        }

        public void CallFunction(string moduleName, string methodName, ArrayList args)
        {
            var reqId = actionIFrame.GetReqId();

            actionIFrame.AddAction(reqId, () =>
            {
                ArrayList jArgs = new ArrayList()
                {
                    moduleName,
                    methodName,
                    args
                };
                Hashtable o = new Hashtable()
                {
                    {"id", reqId},
                    {"method", "callFunctionReturnFlushedQueue"},
                    {"arguments", jArgs}
                };
                Send(o);
            }, CallNativeModules);
        }

        public void InvokeCallback(int callID, ArrayList args)
        {
            var reqId = actionIFrame.GetReqId();

            actionIFrame.AddAction(reqId, () =>
            {
                ArrayList jArgs = new ArrayList()
                {
                    callID,
                    args
                };
                Hashtable o = new Hashtable()
                {
                    {"id", reqId},
                    {"method", "invokeCallbackAndReturnFlushedQueue"},
                    {"arguments", jArgs}
                };

                Send(o);
            }, CallNativeModules);
        }

        private void CallNativeModules(ArrayList result)
        {
            if (result.Count == 0) return;

            ArrayList moduleIds = (ArrayList) result[0];
            ArrayList methodIds = (ArrayList) result[1];
            ArrayList argss = (ArrayList) result[2];

            for (int i = 0; i < moduleIds.Count; i++)
            {
                int moduleId = (int) moduleIds[i];
                int methodId = (int) methodIds[i];
                ArrayList argsHere = (ArrayList) argss[i];
                RNUMainCore.CallNativeModule(moduleId, methodId, argsHere);
            }

            RNUMainCore.CallNativeModule(-1, -1, null);
            RNUMainCore.CloseIfNecessary();
        }

        public void SetGlobalVariable(string propertyName, Hashtable jsonValue)
        {
            if (propertyName == "__fbBatchedBridgeConfig")
            {
                fbBatchedBridgeConfig = JSONUtil.StringifyMap(jsonValue);
            }
            else
            {
                //TODO
                //no-op
            }
        }


        private void Send(Hashtable msg)
        {
            webSocket.Send(JSONUtil.StringifyMap(msg));
        }


        private void OnMessage(string msg)
        {
            Util.Log("JSDebuggerWebSocketClient OnMessage msg - {0}", msg);
            var o = JSONUtil.ParseMap(msg);
            
            if (!o.ContainsKey("result") || o["result"] == null)
            {
                o["result"] = new ArrayList();
            }
            else
            {
                var resultStr = (string) o["result"];

                if (resultStr == "null")
                {
                    o["result"] = new ArrayList();
                }
                else
                {
                    o["result"] = JSONUtil.ParseList(resultStr);
                }
            }

            actionIFrame.ConsumeRes(o);
        }

        public void Destroy()
        {
            this.webSocket.Close();
            StaticCommonScript.RemoveFrameListener(actionIFrame);
        }


        // 用来打开浏览器调试面板
        private IEnumerator NoticeServerToOpenDebugger(string ip, int port)
        {
            var jsDevTools = "http://" + ip + ":" + port + "/launch-js-devtools";
            Debug.Log("OpenBrowser:" + jsDevTools);
            UnityWebRequest devToolsRequest = UnityWebRequest.Get(jsDevTools);
            yield return devToolsRequest.SendWebRequest();
        }
    }
}