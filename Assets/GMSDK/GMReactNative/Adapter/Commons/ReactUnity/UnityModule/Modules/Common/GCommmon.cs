/*
 * @author yankang.nj
 * 提供给JS测的API， 通常不知道位置的API，都放在这里
 */

using System.Collections;
using System.Collections.Generic;
using UNBridgeLib;
using UNBridgeLib.LitJson;


namespace GSDK.RNU
{
    public partial class Common
    {

        [ReactMethod(true)]
        public void request(string url, string method, Dictionary<string, object> parameters,
            Dictionary<string, object> headers, int isJson, Promise promise)
        {
            Util.Log("request url = {0},  method = {1}, parameters = {2}, headers = {3}, isJson = {4}", url, method,
                parameters, headers, isJson);
            GMReactUnityMgr.instance.SDK.RuRequest(url, method, parameters, headers, isJson, result =>
            {
                if (result.IsSuccess())
                {
                    Util.Log("success {0} ", result.data);
                    promise.Resolve(result.data);
                }
                else
                {
                    Util.Log("fail");
                    promise.Reject("request error");
                }
            }
            );
        }


        [ReactMethod(true)]
        public void sendMessageToUnity(string message, Promise promise)
        {
            Util.Log("sendMessageToUnity {0} ", message);
            GMReactUnityMgr.instance.SDK.RUSendMessageToUnity(message);
            promise.Resolve(true);
        }

        
        [ReactMethod(true)]
        public void openUrl(string url, Promise promise)
        {
            Util.Log("openUrl {0} ", url);
            GMReactUnityMgr.instance.SDK.RUOpenUrl(url, result => {
                if (result.IsSuccess())
                {
                    Util.Log("openUrl success");
                    promise.Resolve(true);
                }
                else
                {
                    Util.Log("openUrl fail");
                    promise.Reject("openUrl error");
                }
            });
        }


        [ReactMethod(true)]
        public void getUserInfo(Promise promise)
        {
            Util.Log("getUserInfo");
            GMReactUnityMgr.instance.SDK.RUGetUserInfo(result => {
                if (result.IsSuccess())
                {
                    Util.Log("getUserInfo success");
                    promise.Resolve(new Hashtable(result.data));
                }
                else
                {
                    Util.Log("getUserInfo fail");
                    promise.Reject("getUserInfo fail");
                }
            });
        }


        [ReactMethod(true)]
        public void getCommonParams(Promise promise)
        {
            Util.Log("getCommonParams");
            GMReactUnityMgr.instance.SDK.RUGetCommonParams(result => {
                if (result.IsSuccess())
                {
                    Util.Log("getCommonParams success");
                    promise.Resolve(new Hashtable(result.data));
                }
                else
                {
                    Util.Log("getCommonParams fail");
                    promise.Reject("getCommonParams fail");
                }
            });
        }


        [ReactMethod(true)]
        public static void monitorEvent(string eventName, Dictionary<string, object> category,
            Dictionary<string, object> metric, Dictionary<string, object> extra, Promise promise)
        {
            Util.Log("monitorEvent eventName {0} ", eventName);
            GMReactUnityMgr.instance.SDK.RUMonitorEvent(eventName, category, metric, extra, result =>
            {
                if (result.IsSuccess())
                {
                    Util.Log("monitorEvent success");
                    promise.Resolve(true);
                }
                else
                {
                    Util.Log("monitorEvent fail");
                    promise.Reject("monitorEvent fail");
                }
            });
        }


        [ReactMethod(true)]
        public void reportTrackEvent(string eventName, string jsonParams, Promise promise)
        {
            Report.Service.ET.ReportTrackEvent(eventName, jsonParams);
            promise.Resolve(true);
        }


        [ReactMethod(true)]
        public void callUNBridge(string target, string jsonString, Promise promise)
        {
            JsonData jsonData = JsonMapper.ToObject(jsonString);
            UNBridge.Call(target, jsonData);
            promise.Resolve(true);
        }


        [ReactMethod(true)]
        public void callUNBridgeWithCallBack(string eventName, string jsonString, Promise promise)
        {
            JsonData jsonData = JsonMapper.ToObject(jsonString);

            BridgeCallBack callBack = new BridgeCallBack
            {
                OnSuccess = (JsonData data) =>
                {
                    promise.Resolve(data.ToJson());
                }
            };
            UNBridge.Call(eventName, jsonData, callBack);
        }


        [ReactMethod(true)]
        public void callUNBridgeSync(string target, string jsonString, Promise promise)
        {
            JsonData jsonData = JsonMapper.ToObject(jsonString);
            object obj = UNBridge.CallSync(target, jsonData);
            if (obj != null)
            {
                var result = new Dictionary<string, object>() {
                    { "data", obj }
                };
                promise.Resolve(JsonMapper.ToJson(result));
            }
            else
            {
                promise.Resolve(true);
            }
        }


        [ReactMethod]
        public void callUNBridgeListen(string target, bool overOld = true)
        {
            BridgeCallBack callBack = new BridgeCallBack
            {
                OnSuccess = (JsonData data) =>
                {
                    RNUMainCore.SendUnityEventToJs(target, data.ToJson());
                }
            };
            UNBridge.Listen(target, overOld, callBack);
        }


        [ReactMethod]
        public void callUNBridgeUnListen(string target)
        { 
            UNBridge.UnListen(target);
        }


        [ReactMethod(true)]
        public void callLinkingCanOpenUrl(string url, Promise promise)
        {
            Util.Log("js call linkingCanOpenUrl {0}", url);
            GMReactUnityMgr.instance.SDK.RULinkingCanOpenUrl(url, result => {
                if (result.IsSuccess())
                {
                    Util.Log("js call linkingCanOpenUrl {0} result canOpen : {1}", url, result.canOpen);
                    promise.Resolve(result.canOpen);
                }
                else
                {
                    Util.Log("js call linkingCanOpenUrl fail");
                    promise.Reject("linkingCanOpenUrl fail");
                }
            });
        }


        [ReactMethod(true)]
        public void callLinkingOpenUrl(string url, Promise promise)
        {
            Util.Log("js call linkingOpenUrl {0}", url);
            GMReactUnityMgr.instance.SDK.RULinkingOpenUrl(url, result => {
                if (result.IsSuccess())
                {
                    Util.Log("js call linkingOpenUrl {0} result success : {1}", url, result.IsSuccess());
                    promise.Resolve(result.IsSuccess());
                }
                else
                {
                    Util.Log("js call linkingOpenUrl fail {0}", result.message);
                    promise.Reject("linkingCanOpenUrl fail");
                }
            });
        }
        
                
        [ReactMethod(true)]
        public void getGeckoPath(Promise promise) 
        {
            Util.Log("js call csharp to getGeckoPath");
            promise.Resolve(GMReactUnityMgr.instance.preBundlePath);
        }



    }
}
