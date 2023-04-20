using UNBridgeLib.LitJson;
using System;
using System.Collections.Generic;
using GSDK;
using GSDK.RNU;
using UNBridgeLib;
using UnityEngine;

namespace GMSDK
{
    public class BaseReactUnitySDK
    {
        public Action<string> sendToUnityMessageAction;
        public BaseReactUnitySDK()
        {

        }

        public void RULinkingCanOpenUrl(string url, Action<RULinkingCanOpenUrlResult> callback)
        {
            GLog.LogInfo("ReactUnity can open url : " + url);
            JsonData param = new JsonData();
            param["url"] = url;

            ReactUnityCallbackHandler unCallBack = new ReactUnityCallbackHandler()
            {
                linkingCanOpenUrlAction = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.onLinkingCanOpenUrlCallback);
            CallNative(SDKReactUnityDefines.RULinkingCanOpenUrl, param, unCallBack);
        }

        public void RULinkingOpenUrl(string url, Action<CallbackResult> callback)
        {
            GLog.LogInfo("ReactUnity can open url : " + url);
            JsonData param = new JsonData();
            param["url"] = url;

            ReactUnityCallbackHandler unCallBack = new ReactUnityCallbackHandler()
            {
                commonSuccessAction = callback,
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnSuccessCallback);
            CallNative(SDKReactUnityDefines.RULinkingOpenUrl, param, unCallBack);
        }

        public void getKVConfig(Action<RUGetKVConfigResult> callback)
        {
            JsonData param = new JsonData();
            ReactUnityCallbackHandler unCallBack = new ReactUnityCallbackHandler()
            {
                getKVConfigAction = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.GetKVConfigCallback);
            CallNative(SDKReactUnityDefines.RUGetKVConfig, param, unCallBack);
        }

        /**
         * 网络请求
         * url:网络请求地址
         * method: post/get
         * parameters:参数
         * headers:请求头
         * isJson: 1:json数据 0:其他
         */
        public void RuRequest(string url, string method, Dictionary<string, object> parameters,
            Dictionary<string, object> headers, int isJson, Action<RURequestResult> callback)
        {
            GLog.LogInfo("ReactUnity RuRequest : " + url + ", "+ method + ", " + parameters + ", " + headers + ", " + isJson);
            JsonData param = new JsonData();
            param["url"] = url;
            param["type"] = method;
            if (parameters == null)
            {
                parameters = new Dictionary<string, object>();
            }
            param["params"] = JsonMapper.ToJson(parameters);
            if (headers == null)
            {
                headers = new Dictionary<string, object>();
            }
            param["headers"] = JsonMapper.ToJson(headers);
            param["isJson"] = isJson;
            ReactUnityCallbackHandler unCallBack = new ReactUnityCallbackHandler()
            {
                requestConfigAction = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnRequestCallback);
            CallNative(SDKReactUnityDefines.RURequest, param, unCallBack);
        }

        public void RUSendMessageToUnity(string message)
        {
            GLog.LogInfo("ReactUnity SendMessageToUnity " + message);
            this.sendToUnityMessageAction.Invoke(message);
        }

        public void RUOpenUrl(string url, Action<RUOpenUrlResult> callback){
            GLog.LogInfo("openWebView " + url);
            // JsonData param = new JsonData();
            // param["url"] = url;
            // ReactUnityCallbackHandler unCallBack = new ReactUnityCallbackHandler()
            // {
            //     openUrlAction = callback
            // };
            // unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OpenUrlCallback);
            // UNBridge.Call(SDKReactUnityDefines.RUOpenUrl, param, unCallBack);
            GMReactNativeMgr.instance.SDK.OpenUrl(url, result=>{
                callback.Invoke(new RUOpenUrlResult{
                    code = result.code,
                    message = result.message,
                });
            });
        }

        public void RUGetUserInfo(Action<RUGetUserInfoResult> callback){
            GLog.LogInfo("getUserInfo");
            JsonData param = new JsonData();
            ReactUnityCallbackHandler unCallBack = new ReactUnityCallbackHandler()
            {
                getUserInfoAction = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.GetUserInfoCallback);
            CallNative(SDKReactUnityDefines.RUGetUserInfo, param, unCallBack);
        }

        public void RUGetCommonParams(Action<RUGetCommonParamsResult> callback){
            GLog.LogInfo("getCommonParams");
            JsonData param = new JsonData();
            ReactUnityCallbackHandler unCallBack = new ReactUnityCallbackHandler()
            {
                getCommonParamsAction = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.GetCommonParamsCallback);
            CallNative(SDKReactUnityDefines.RUGetCommonParams, param, unCallBack);
        }

        public void RUMonitorEvent(string eventName, Dictionary<string, object> category,
            Dictionary<string, object> metric, Dictionary<string, object> extra,
            Action<RUMonitorEventResult> callback){
            GLog.LogInfo("RUMonitorEvent eventName: "+ eventName + ", categoryData: " + category+ ", metricData: "+ metric + ", extraData: " + extra);
            JsonData param = new JsonData();
            param["eventName"] = eventName;
            if (category == null)
            {
                category = new Dictionary<string, object>();
            }
            param["category"] = JsonMapper.ToJson(category);
            if (metric == null)
            {
                metric = new Dictionary<string, object>();
            }
            param["metric"] = JsonMapper.ToJson(metric);
            if (extra == null)
            {
                extra = new Dictionary<string, object>();
            }
            param["extra"] = JsonMapper.ToJson(extra);
            ReactUnityCallbackHandler unCallBack = new ReactUnityCallbackHandler()
            {
                monitorEventAction = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.MonitorEventCallback);
            CallNative(SDKReactUnityDefines.RUMonitorEvent, param, unCallBack);
        }
        /*
         * 游戏设置父节点
         */
        public void SetGameGoParent(string parentGoName)
        {
            RNUMain.SetGameGoParent(parentGoName);
        }

        public void SetGameGoParent(GameObject parentGo)
        {
            RNUMain.SetGameGoParent(parentGo);
        }

        public void SetGameAdvancedInjection(IRuGameAdvancedInjection gim)
        {
            RNUMain.SetRuGameAdvancedInjection(gim);
        }

        /*
        * 设置 NGUI 事件屏蔽接口
        */
        public void SetRNUTouchIgnore(GameObject gameObject, bool isIgnore)
        {
            RNUMain.SetRNUTouchIgnore(gameObject, isIgnore);
        }

        /*
         * 游戏的字体路径
         */
        public void SetGameFont(string fontName, Font font)
        {
            RNUMain.SetGameFont(fontName,font);
        }
        
        /*
        * 提供游戏的用户信息，如角色信息，区服信息等，提供更通用的设置接口
        */
        public void SetGameData(Dictionary<string, object> gameData)
        {
            RNUMain.SetGameData(gameData);
        }
        
        
        /*
         * debugPage
         */
        public void OpenDebugPage(string ip, string port, GameObject parentGo)
        {
            RNUMain.DebugPage(ip, port, parentGo);
        }
        /*
         * debugPage
         */
        public void OpenDebugPage(string debugURL, GameObject parentGo)
        {
            RNUMain.DebugPage(debugURL, parentGo);
        } 
        

        private void CallNative(string target, JsonData param, BridgeCallBack callback){
            UNBridge.InitBridge();
            long callbackId = BridgeCore.Call(target, param, callback, 0);
            if (callbackId > 0)
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    AndroidUtils.CallNative(BridgeCore.TYPE_CALL, target, param, callbackId);
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    IosUtils.CallNative(BridgeCore.TYPE_CALL, target, param, callbackId);
                }
                else if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    WebGlUtils.CallNative(BridgeCore.TYPE_CALL, target, param, callbackId);
                }
                else if (Application.platform == RuntimePlatform.OSXEditor ||
                        Application.platform == RuntimePlatform.WindowsEditor)
                {
                    EnderUtils.CallNative(BridgeCore.TYPE_CALL, target, param, callbackId);
                }
            }
        }

        public void SyncGecko(Action<CallbackResult> callback)
        {
            JsonData param = new JsonData();
            ReactUnityCallbackHandler unCallBack = new ReactUnityCallbackHandler
            {
                syncGeckoCallback = callback
            };
            unCallBack.OnSuccess = unCallBack.OnSyncGeckoCallback;

            CallNative(SDKReactUnityDefines.RUSyncGecko, param, unCallBack);
        }
    }
}
