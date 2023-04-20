using UNBridgeLib.LitJson;
using System;
using System.Collections.Generic;
using GSDK.RNU;
using UNBridgeLib;
using UnityEngine;

namespace GMSDK
{
    public class BaseReactNativeSDK
    {

        private const string GumihoEngineWebViewCloseCallBackKey = "GumihoEngineWebViewCloseCallBackKey";

        private Dictionary<string, Action<PageCloseResult>> pageCloseDic;

        public BaseReactNativeSDK()
        {
#if UNITY_ANDROID
            UNBridge.Call(ReactNativeMethodName.Init, null);
#endif
            pageCloseDic = new Dictionary<string, Action<PageCloseResult>>();

            ReactNativeCallbackHandler unCallBack = new ReactNativeCallbackHandler()
            {
                gumihoEngineNotificationCallBack = (JsonData jsonData) =>
                {
                    string action = jsonData["action"].ToString();
                    if (String.Compare("window_status_change", action) == 0)
                    {
                        JsonData param = jsonData["params"];
                        int type = 0;
                        if (param.ContainsKey("type")) {
                            type = Int32.Parse(param["type"].ToString());
                        }
                        Action<PageCloseResult> closeCallBack;
                        string windowID = param.ContainsKey("window_id") ? param["window_id"].ToString() : "";
                        string key = windowID;
                        if (type == 1) { // Web页面关闭
                            key = GumihoEngineWebViewCloseCallBackKey;
                            pageCloseDic.TryGetValue(key, out closeCallBack);
                        } else if(type == 2) { // RU 页面关闭 不走通知直接走回调
                            closeCallBack = null;
                        } else { // RN页面关闭
                            pageCloseDic.TryGetValue(windowID, out closeCallBack);
                        }
                        if (null != closeCallBack) {
                            PageCloseResult closeResult = new PageCloseResult
                            {
                                code = 0,
                                windowId = windowID,
                                inGameId = jsonData["params"]["ingame_id"].ToString(),
                                pageType = Int32.Parse(jsonData["params"]["page_type"].ToString())
                            };
                            closeCallBack.Invoke(closeResult);
                            pageCloseDic.Remove(key);
                        }
                    }
                }
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.gumihoEngineNotificationCallBack);
            // 监听引擎侧的消息
            UNBridge.Listen(ReactNativeResultName.GMGumihoEngineNotification, unCallBack);
        }

        #region 新引擎

        /// <summary>
        /// 更新游戏配置
        /// </summary>
        /// <param name="callback">回调</param>
        public void updateGameConfig(string roleId, string roleName, string serverId,
            Action<updateGameConfigRet> callback)
        {
            JsonData param = new JsonData();
            param["roleId"] = roleId;
            param["roleName"] = roleName;
            param["serverId"] = serverId;

            ReactNativeCallbackHandler unCallBack = new ReactNativeCallbackHandler()
            {
                updateGameConfigCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnupdateGameConfigCallback);
            UNBridge.Call(ReactNativeMethodName.updateGameConfig, param, unCallBack);

            // 新增根据云控 RU初始化
            if (!GMReactUnityMgr.instance.config.ruDisable)
            {
                // 读取 commonParams 当做 initProp
                GMReactUnityMgr.instance.SDK.RUGetCommonParams(result =>
                {
                    RNUMain.Init(result.data);
                });
            }
        }

        public void syncGecko(Action<CallbackResult> callback)
        {
            JsonData param = new JsonData();
            ReactNativeCallbackHandler unCallBack = new ReactNativeCallbackHandler
            {
                syncGeckoCallback = callback
            };
            unCallBack.OnSuccess = unCallBack.OnSyncGeckoCallback;

            UNBridge.Call(ReactNativeMethodName.syncGecko, param, unCallBack);
        }

        /// <summary>
        /// 获取iocnclick通知
        /// </summary>
        /// <param name="callback">回调</param>
        /// <summary>
        /// 根据场景获取活动数据
        /// </summary>
        /// <param name="callback">回调</param>
        public void getSceneData(string type, Action<getSceneDataRet> callback)
        {
            JsonData param = new JsonData();
            param["type"] = type;

            ReactNativeCallbackHandler unCallBack = new ReactNativeCallbackHandler()
            {
                getSceneDataCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OngetSceneDataCallback);
            UNBridge.Call(ReactNativeMethodName.getSceneData, param, unCallBack);
        }

        /// <summary>
        /// 根据场景获取活动红点数据（新接口，修复类型问题）
        /// </summary>
        /// <param name="callback">回调</param>
        public void queryActivityNotifyDataByType(string type, Action<queryActivityNotifyDataRet> callback)
        {
            JsonData param = new JsonData();
            param["type"] = type;

            ReactNativeCallbackHandler unCallBack = new ReactNativeCallbackHandler()
            {
                queryActivityNotifyDataCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnQueryActivityNotifyDataCallback);
            UNBridge.Call(ReactNativeMethodName.queryActivityNotifyDataByType, param, unCallBack);
        }

        /// <summary>
        /// 根据id获取活动红点数据（新接口，修复类型问题）
        /// </summary>
        /// <param name="callback">回调</param>
        public void queryActivityNotifyDataById(string id, Action<queryActivityNotifyDataRet> callback)
        {
            JsonData param = new JsonData();
            param["id"] = id;

            ReactNativeCallbackHandler unCallBack = new ReactNativeCallbackHandler()
            {
                queryActivityNotifyDataCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnQueryActivityNotifyDataCallback);
            UNBridge.Call(ReactNativeMethodName.queryActivityNotifyDataById, param, unCallBack);
        }

        /// <summary>
        /// 根据场景获取活动红点数据
        /// </summary>
        /// <param name="callback">回调</param>
        [Obsolete("请使用queryActivityNotifyDataByType")]
        public void queryActivityNotifyByType(string type, Action<queryActivityNotifyByTypeRet> callback)
        {
            JsonData param = new JsonData();
            param["type"] = type;

            ReactNativeCallbackHandler unCallBack = new ReactNativeCallbackHandler()
            {
                queryActivityNotifyByTypeCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnqueryActivityNotifyByTypeCallback);
            UNBridge.Call(ReactNativeMethodName.queryActivityNotifyByType, param, unCallBack);
        }


        /// <summary>
        /// 根据id获取活动红点数据
        /// </summary>
        /// <param name="callback">回调</param>
        [Obsolete("queryActivityNotifyDataById")]
        public void queryActivityNotifyById(string id, Action<queryActivityNotifyByIdRet> callback)
        {
            JsonData param = new JsonData();
            param["id"] = id;

            ReactNativeCallbackHandler unCallBack = new ReactNativeCallbackHandler()
            {
                queryActivityNotifyByIdCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnqueryActivityNotifyByIdCallback);
            UNBridge.Call(ReactNativeMethodName.queryActivityNotifyById, param, unCallBack);
        }

        /// <summary>
        /// 根据in_game_id, url打开对应的活动
        /// </summary>
        /// <param name="callback">回调</param>
        public void openPage(string url, string inGameId, Dictionary<string, object> parameters,
            Action<openPageRet> callback)
        {
            GameObject parentGo = null;


            openPage(url, parentGo, inGameId, parameters, callback);
        }

        /// <summary>
        /// 打开九尾页面
        /// </summary>
        /// <param name="url">页面URL</param>
        /// <param name="inGameId"> 页面的九尾ingameid 可为空</param>
        /// <param name="parameters">传给九尾页面的特殊参数</param>
        /// <param name="callback">打开的回调</param>
        /// <param name="closeCallBack">页面关闭的回调</param>
        public void openPage(string url, string inGameId, Dictionary<string, object> parameters,
            Action<openPageRet> callback, Action<PageCloseResult> closeCallBack)
        {
            GameObject parentGo = null;


            openPage(url, parentGo, inGameId, parameters, callback, closeCallBack);
        }


        /// <summary>
        /// 打开九尾页面
        /// </summary>
        /// <param name="url">页面URL</param>
        /// <param name="parentGo"></param>
        /// <param name="inGameId"> 页面的九尾ingameid 可为空</param>
        /// <param name="parameters">传给九尾页面的特殊参数</param>
        /// <param name="callback">打开的回调</param>
        public void openPage(string url, GameObject parentGo, string inGameId, Dictionary<string, object> parameters,
            Action<openPageRet> callback)
        {
            openPage(url, parentGo, inGameId, parameters, callback, null);
        }

        public void openPage(string url, GameObject parentGo, string inGameId, Dictionary<string, object> parameters,
            Action<openPageRet> callback, Action<PageCloseResult> closeCallBack)
        {
            JsonData param = new JsonData();
            param["url"] = url;
            param["inGameId"] = inGameId;

            if (parameters == null)
            {
                parameters = new Dictionary<string, object>();
            }

            param["params"] = JsonMapper.ToJson(parameters);

            if (!GMReactUnityMgr.instance.config.ruDisable && !string.IsNullOrEmpty(url) && url.Contains("://rupage"))
            {
                // 打开 2.0 unity 页面
                // 传递打开结果信息
                callback.Invoke(new openPageRet()
                {
                    type = 3,
                    windowId = "RUPage",
                });

                Action ruCloseCallBack = null;
                if (null != closeCallBack) {
                    ruCloseCallBack = () =>
                    {
                        PageCloseResult closeResult = new PageCloseResult
                        {
                            code = 0,
                            windowId = "RUPage",
                            inGameId = string.IsNullOrEmpty(inGameId) ? "" : inGameId,
                            pageType = 2
                        };
                        closeCallBack.Invoke(closeResult);
                    };
                }
                RNUMain.OpenPage(url, parentGo, ruCloseCallBack);
            }
            else
            {
                ReactNativeCallbackHandler unCallBack = new ReactNativeCallbackHandler()
                {
                    openPageCallback = (result) => {
                        if (result.IsSuccess() && null != closeCallBack) {
                            // [3.15.0] 新增如果页面打开成功，监听关闭回调
                            if (result.type == 2){
                                if (pageCloseDic.ContainsKey(GumihoEngineWebViewCloseCallBackKey))
                                {
                                    pageCloseDic.Remove(GumihoEngineWebViewCloseCallBackKey);
                                }
                                pageCloseDic.Add(GumihoEngineWebViewCloseCallBackKey, closeCallBack);
                            } else if (!string.IsNullOrEmpty(result.windowId))
                            {
                                pageCloseDic.Add(result.windowId, closeCallBack);
                            }
                        }
                        callback.Invoke(result);
                    }
                };
                unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnopenPageCallback);
                UNBridge.Call(ReactNativeMethodName.openPage, param, unCallBack);
            }
        }

        /// <summary>
        /// 根据 in_game_id, url 打开对应的活动
        /// </summary>
        /// <param name="callback">回调</param>
        public void openPage(string url, string parentStr, string inGameId, Dictionary<string, object> parameters,
            Action<openPageRet> callback)
        {
            openPage(url, parentStr, inGameId, parameters, callback, null);
        }

        public void openPage(string url, string parentStr, string inGameId, Dictionary<string, object> parameters,
            Action<openPageRet> callback, Action<PageCloseResult> closeCallBack)
        {
            GameObject parentGo = null;
            if (!string.IsNullOrEmpty(parentStr))
            {
                parentGo = GameObject.Find(parentStr);
            }
            openPage(url, parentGo, inGameId, parameters, callback, closeCallBack);
        }


        /// <summary>
        /// params type 活动类型
        /// 根据活动类型拉取网络活动数据
        /// </summary>
        /// <param name="callback">回调</param>
        public void openFaceVerify(string type, Action<openFaceVerifyRet> callback)
        {
            JsonData param = new JsonData();
            param["type"] = type;

            ReactNativeCallbackHandler unCallBack = new ReactNativeCallbackHandler()
            {
                openFaceVerifyCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnopenFaceVerifyCallback);
            UNBridge.Call(ReactNativeMethodName.openFaceVerify, param, unCallBack);
        }

        /// <summary>
        /// 根据key获取配置的kv
        /// </summary>
        /// <param name="callback">回调</param>
        public void QueryConfigValueByKey(string key, Action<QueryConfigValueResultRet> callback)
        {
            JsonData param = new JsonData();
            param["key"] = key;

            ReactNativeCallbackHandler unCallBack = new ReactNativeCallbackHandler()
            {
                queryConfigValueCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnQueryConfigValueCallback);
            UNBridge.Call(ReactNativeMethodName.QueryConfigValueByKey, param, unCallBack);
        }

        /// <summary>
        /// 获取红点配置
        /// </summary>
        /// <param name="callback">回调</param>
        public void QueryActivityNotify(Action<QueryActivityNotifyRet> callback)
        {
            JsonData param = new JsonData();
            ReactNativeCallbackHandler unCallBack = new ReactNativeCallbackHandler()
            {
                queryActivityNotifyCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnQueryActivityNotifyCallback);
            UNBridge.Call(ReactNativeMethodName.QueryActivityNotify, param, unCallBack);
        }
        /// <summary>
        /// 获取打开的九尾页面，倒序存放，即最后一个为显示在最上面的页面
        /// </summary>
        /// <returns>
        /// 返回打开的九尾页面
        /// </returns>
        public List<RNPage> getRNPages()
        {
            object data = UNBridge.CallSync(ReactNativeMethodName.getRNPages, null);
            List<RNPage> list;
            if (data != null)
                list = SdkUtil.ToObject<List<RNPage>>((string)data);
            else
                list = new List<RNPage>();
            return list;
        }
        /// <summary>
        /// 关闭所有打开的九尾页面
        /// </summary>
        public void closeAllPages()
        {
            if (!GMReactUnityMgr.instance.config.ruDisable)
            {
                // 关闭 2.0 unity 页面
                RNUMain.Close();
            }
            UNBridge.Call(ReactNativeMethodName.closeAllPages, null);
        }
        
        /// <summary>
        /// 隐藏/打开 九尾活动面板
        /// </summary>
        public void SetGumihoPanelActive(bool isActive)
        {
            RNUMain.SetGumihoPanelActive(isActive);
        }

        /// <summary>
        /// 显示页面。
        /// </summary>
        /// <param name="windowId">窗口ID，对应 openPage 的返回值。</param>
        public void showPage(string windowId, Action<OperatePageRet> callback)
        {
            JsonData param = new JsonData();
            param["windowId"] = windowId;
            ReactNativeCallbackHandler unCallBack = new ReactNativeCallbackHandler
            {
                operatePageCallback = callback
            };
            unCallBack.OnSuccess = unCallBack.OnOperatePageCallback;
            UNBridge.Call(ReactNativeMethodName.showPage, param, unCallBack);
        }

        /// <summary>
        /// 隐藏页面。
        /// </summary>
        /// <param name="windowId">窗口ID，对应 openPage 的返回值。</param>
        public void hidePage(string windowId, Action<OperatePageRet> callback)
        {
            JsonData param = new JsonData();
            param["windowId"] = windowId;
            ReactNativeCallbackHandler unCallBack = new ReactNativeCallbackHandler
            {
                operatePageCallback = callback
            };
            unCallBack.OnSuccess = unCallBack.OnOperatePageCallback;
            UNBridge.Call(ReactNativeMethodName.hidePage, param, unCallBack);
        }

        /// <summary>
        /// 关闭页面，关闭后不可再操作该页面。
        /// </summary>
        /// <param name="windowId">窗口ID，对应 openPage 的返回值。</param>
        public void closePage(string windowId, Action<OperatePageRet> callback)
        {
            JsonData param = new JsonData();
            param["windowId"] = windowId;
            ReactNativeCallbackHandler unCallBack = new ReactNativeCallbackHandler
            {
                operatePageCallback = callback
            };
            unCallBack.OnSuccess = unCallBack.OnOperatePageCallback;
            UNBridge.Call(ReactNativeMethodName.closePage, param, unCallBack);
        }

        /// <summary>
        /// 给前端页面发送信息。
        /// </summary>
        /// <param name="windowId">窗口ID，对应 openPage 的返回值。</param>
        /// <param name="event">事件类型。</param>
        /// <param name="message">消息内容。</param>
        public void sendMessageToPage(string windowId, string @event, string message, Action<CallbackResult> callback)
        {
            JsonData param = new JsonData();
            param["windowId"] = windowId;
            param["event"] = @event;
            param["message"] = message;

            ReactNativeCallbackHandler unCallBack = new ReactNativeCallbackHandler
            {
                sendMessageToPageCallback = callback
            };
            unCallBack.OnSuccess = unCallBack.OnSendMessageToPageCallback;
            UNBridge.Call(ReactNativeMethodName.sendMessageToPage, param, unCallBack);
        }

#if UNITY_ANDROID
        /// <summary>
        /// 设置 Debug 状态。
        /// </summary>
        /// <param name="isEnabled">是否启用。</param>
        public void setRNDebug(bool isEnabled)
        {
            JsonData param = new JsonData();
            param["rnDebug"] = isEnabled;
            UNBridge.Call(ReactNativeMethodName.setRNDebug, param);
        }

        /// <summary>
        /// 获取 Debug 状态。
        /// </summary>
        public void getRNDebug(Action<GetRNDebugRet> callback)
        {
            JsonData param = new JsonData();
            ReactNativeCallbackHandler unCallBack = new ReactNativeCallbackHandler
            {
                getRNDebugCallback = callback
            };
            unCallBack.OnSuccess = unCallBack.OnGetRNDebugCallback;
            UNBridge.Call(ReactNativeMethodName.getRNDebug, param, unCallBack);
        }
#endif

        /// <summary>
        /// 打开绑定页面。
        /// </summary>
        public void startBindPage(Action<openPageRet> callback)
        {
            JsonData param = new JsonData();
            ReactNativeCallbackHandler unCallBack = new ReactNativeCallbackHandler
            {
                openPageCallback = callback
            };
            unCallBack.OnSuccess = unCallBack.OnopenPageCallback;
            UNBridge.Call(ReactNativeMethodName.startBindPage, param, unCallBack);
        }

        #endregion

        #region 旧引擎

        /// <summary>
        /// 初始化bundle package
        /// </summary>
        /// <param name="callback">回调</param>
        public void initBundlePackages(Action<initBundlePackagesRet> callback)
        {
            JsonData param = new JsonData();

            ReactNativeCallbackHandler unCallBack = new ReactNativeCallbackHandler()
            {
                initBundlePackagesCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OninitBundlePackagesCallback);
            UNBridge.Call(ReactNativeMethodName.initBundlePackages, param, unCallBack);
        }

        /// <summary>
        /// 绑定师徒关系
        /// </summary>
        /// <param name="callback">回调</param>
        public void invitationPreBind(Action<invitationPreBindRet> callback)
        {
            JsonData param = new JsonData();

            ReactNativeCallbackHandler unCallBack = new ReactNativeCallbackHandler()
            {
                invitationPreBindCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OninvitationPreBindCallback);
            UNBridge.Call(ReactNativeMethodName.invitationPreBind, param, unCallBack);
        }


        /// <summary>
        /// Opens the URL.
        /// </summary>
        /// <param name="url">URL.</param>
        /// <param name="callback">Callback.</param>
        public void OpenUrl(string url, Action<OpenUrlResultRet> callback)
        {
            JsonData param = new JsonData();
            param["url"] = url;
            ReactNativeCallbackHandler unCallBack = new ReactNativeCallbackHandler()
            {
                openUrlCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OpenUrlCallback);
            UNBridge.Call(ReactNativeMethodName.OpenUrl, param, unCallBack);
        }

        /// <summary>
        /// Homes the did load.
        /// </summary>
        /// <param name="roleId">Role identifier.</param>
        /// <param name="roleName">Role name.</param>
        /// <param name="serverId">Server identifier.</param>
        /// <param name="callback">Callback.</param>
        public void HomeDidLoad(string roleId, string roleName, string serverId, Action<HomeDidLoadResultRet> callback)
        {
            JsonData param = new JsonData();
            param["roleId"] = roleId;
            param["roleName"] = roleName;
            param["serverId"] = serverId;

            ReactNativeCallbackHandler unCallBack = new ReactNativeCallbackHandler()
            {
                homeDidLoadCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.HomeDidLoadCallback);
            UNBridge.Call(ReactNativeMethodName.GameHomeDidLoadWithRoleId, param, unCallBack);
        }

        /// <summary>
        /// Wars the did end.
        /// </summary>
        /// <param name="callback">Callback.</param>
        public void WarDidEnd(Action<WarDidEndResultRet> callback)
        {
            JsonData param = new JsonData();

            ReactNativeCallbackHandler unCallBack = new ReactNativeCallbackHandler()
            {
                warDidEndCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.WarDidEndCallback);
            UNBridge.Call(ReactNativeMethodName.WarDidFinish, param, unCallBack);
        }

        public void notifyIconClickSceneDid(Action<notifyIconClickSceneDidRet> callback)
        {
            JsonData param = new JsonData();

            ReactNativeCallbackHandler unCallBack = new ReactNativeCallbackHandler()
            {
                notifyIconClickSceneDidCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnnotifyIconClickSceneDidCallback);
            UNBridge.Call(ReactNativeMethodName.notifyIconClickSceneDid, param, unCallBack);
        }

        /// <summary>
        /// send message to Ru
        /// </summary>
        /// <param name="message">content message.</param>
        public void sendMessageToGumiho(string message)
        {
            RNUMain.SendMessageToRu(message);
        }
        
        
        /// <summary>
        /// Listens the native notification.
        /// </summary>
        /// <param name="callback">Callback.</param>
        public void listenNativeNotification(Action<string> callback)
        {
            ReactNativeCallbackHandler unCallBack = new ReactNativeCallbackHandler()
            {
                unityNotificationCallback = callback
            };
            GMReactUnityMgr.instance.SDK.sendToUnityMessageAction = callback;

            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.ListenNativeNotificationCallback);
            UNBridge.Listen(ReactNativeResultName.GMRNUnityNotification, unCallBack);
        }
#if UNITY_IOS
        // 设置横竖屏，默认横屏，一般不需要设置
        public void SetOrientation(int orientation, Action<SetOrientationResultRet> callback)
        {
            JsonData param = new JsonData();
            param["orientation"] = orientation;

            ReactNativeCallbackHandler unCallBack = new ReactNativeCallbackHandler()
            {
                setOrientationCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.SetUpOrentationCallback);
            UNBridge.Call(ReactNativeMethodName.SetUpOrientation, param, unCallBack);
        }
#endif

        /// <summary>
        /// 打开自助验收页面
        /// </summary>
        /// <param name="callback"></param>
        public void showTestPage(Action<ShowTestPageRet> callback)
        {
            JsonData param = new JsonData();
            ReactNativeCallbackHandler unCallBack = new ReactNativeCallbackHandler()
            {
                showTestPageCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.ShowTestPageCallBack);
            UNBridge.Call(ReactNativeMethodName.showTestPage, param, unCallBack);
        }
        
        /*
         * 游戏设置父节点
         */
        public void SetGameGoParent(string parentGoName)
        {
            GMReactUnityMgr.instance.SDK.SetGameGoParent(parentGoName);
        }

        public void SetGameGoParent(GameObject parentGo)
        {
            GMReactUnityMgr.instance.SDK.SetGameGoParent(parentGo);
        }
        
        
        /*
         * 游戏设置资源共享预置
         */
        public void SetGameAdvancedInjection(IRuGameAdvancedInjection gim)
        { 
            GMReactUnityMgr.instance.SDK.SetGameAdvancedInjection(gim);
        }
        
        
        public void SetRNUTouchIgnore(GameObject gameObject, bool isIgnore)
        {
            GMReactUnityMgr.instance.SDK.SetRNUTouchIgnore(gameObject, isIgnore);
        }

        /*
         * 游戏的字体路径
         */
        public void SetGameFont(string fontName, Font font)        
        {
            GMReactUnityMgr.instance.SDK.SetGameFont(fontName,font);
        }
        
        /*
         * 提供游戏的用户信息，如角色信息，区服信息等，提供更通用的设置接口
         */
        public void SetGameData(Dictionary<string, object> gameData)       
        {
            GMReactUnityMgr.instance.SDK.SetGameData(gameData);
        }
        
        /*
        * debugPage
        */
        public void OpenDebugPage(string ip, string port, GameObject parentGo)
        {
            GMReactUnityMgr.instance.SDK.OpenDebugPage(ip, port, parentGo);
        }
        /*
         * debugPage
         */
        public void OpenDebugPage(string debugURL, GameObject parentGo)
        {
            GMReactUnityMgr.instance.SDK.OpenDebugPage(debugURL, parentGo);
        } 
        
        

        #endregion

        #region 通用接口

        /// <summary>
        /// 根据活动Id获取活动链接
        /// </summary>
        /// <param name="callback">回调</param>
        public void FetchActivityUrlWithId(string activityId, Action<FetchActivityUrlWithIdResultRet> callback)
        {
            JsonData param = new JsonData();
            param["activityId"] = activityId;

            ReactNativeCallbackHandler unCallBack = new ReactNativeCallbackHandler()
            {
                fetchActivityUrlCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnFetchActivityUrlCallback);
            UNBridge.Call(ReactNativeMethodName.FetchActivityUrlWithId, param, unCallBack);
        }

        /// <summary>
        /// 获取活动url,默认返回活动第一条
        /// </summary>
        /// <param name="callback">回调</param>
        public void getSenceUrl(string type, Action<getSenceUrlRet> callback)
        {
            JsonData param = new JsonData();
            param["type"] = type;

            ReactNativeCallbackHandler unCallBack = new ReactNativeCallbackHandler()
            {
                getSenceUrlCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OngetSenceUrlCallback);
            UNBridge.Call(ReactNativeMethodName.getSenceUrl, param, unCallBack);
        }

        #endregion
    }
}