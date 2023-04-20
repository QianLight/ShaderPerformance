using System;
using System.Collections.Generic;
using GSDK.RNU;
using UNBridgeLib;
using UNBridgeLib.LitJson;

namespace GMSDK
{
    public class ReactNativeCallbackHandler : BridgeCallBack
    {
        public Action<FetchActivityUrlWithIdResultRet> fetchActivityUrlCallback;
        public Action<OpenUrlResultRet> openUrlCallback;
        public Action<HomeDidLoadResultRet> homeDidLoadCallback;
        public Action<WarDidEndResultRet> warDidEndCallback;
        public Action<SetOrientationResultRet> setOrientationCallback;
        public Action<string> unityNotificationCallback;
        public Action<JsonData> gumihoEngineNotificationCallBack;
        public Action<QueryConfigValueResultRet> queryConfigValueCallback;
        public Action<QueryActivityNotifyRet> queryActivityNotifyCallback;
        public Action<initBundlePackagesRet> initBundlePackagesCallback;
        public Action<invitationPreBindRet> invitationPreBindCallback;
        public Action<getSenceUrlRet> getSenceUrlCallback;
        public Action<updateGameConfigRet> updateGameConfigCallback;
        public Action<notifyIconClickSceneDidRet> notifyIconClickSceneDidCallback;
        public Action<getSceneDataRet> getSceneDataCallback;
        public Action<queryActivityNotifyByTypeRet> queryActivityNotifyByTypeCallback;
        public Action<queryActivityNotifyByIdRet> queryActivityNotifyByIdCallback;
        public Action<queryActivityNotifyDataRet> queryActivityNotifyDataCallback;
        public Action<openPageRet> openPageCallback;
        public Action<openFaceVerifyRet> openFaceVerifyCallback;
        public Action<OperatePageRet> operatePageCallback;
        public Action<CallbackResult> sendMessageToPageCallback;
        public Action<GetRNDebugRet> getRNDebugCallback;
        public Action<ShowTestPageRet> showTestPageCallback;
        public Action<CallbackResult> syncGeckoCallback;


        public ReactNativeCallbackHandler()
        {
            this.OnFailed = new OnFailedDelegate(OnFailCallback);
            this.OnTimeout = new OnTimeoutDelegate(OnTimeoutCallback);
        }

        public void OngetSceneDataCallback(JsonData jd)
        {
            getSceneDataRet ret = JsonMapper.ToObject<getSceneDataRet>(jd.ToJson());
            getSceneDataCallback.Invoke(ret);
        }

        public void OnqueryActivityNotifyByTypeCallback(JsonData jd)
        {
            queryActivityNotifyByTypeRet ret = JsonMapper.ToObject<queryActivityNotifyByTypeRet>(jd.ToJson());
            queryActivityNotifyByTypeCallback.Invoke(ret);
        }

        public void OnqueryActivityNotifyByIdCallback(JsonData jd)
        {
            queryActivityNotifyByIdRet ret = JsonMapper.ToObject<queryActivityNotifyByIdRet>(jd.ToJson());
            queryActivityNotifyByIdCallback.Invoke(ret);
        }

        public void OnopenPageCallback(JsonData jd)
        {
            openPageRet ret = JsonMapper.ToObject<openPageRet>(jd.ToJson());
            openPageCallback.Invoke(ret);
        }

        public void OnopenFaceVerifyCallback(JsonData jd)
        {
            openFaceVerifyRet ret = JsonMapper.ToObject<openFaceVerifyRet>(jd.ToJson());
            if (null == ret.list)
            {
                ret.list = new List<openFaceData>();
            }
            openFaceVerifyCallback.Invoke(ret);
        }

        public void OninitBundlePackagesCallback(JsonData jd)
        {
            initBundlePackagesRet ret = JsonMapper.ToObject<initBundlePackagesRet>(jd.ToJson());
            initBundlePackagesCallback.Invoke(ret);
        }

        public void OninvitationPreBindCallback(JsonData jd)
        {
            invitationPreBindRet ret = JsonMapper.ToObject<invitationPreBindRet>(jd.ToJson());
            invitationPreBindCallback.Invoke(ret);
        }

        public void OngetSenceUrlCallback(JsonData jd)
        {
            getSenceUrlRet ret = JsonMapper.ToObject<getSenceUrlRet>(jd.ToJson());
            getSenceUrlCallback.Invoke(ret);
        }

        public void OnupdateGameConfigCallback(JsonData jd)
        {
            updateGameConfigRet ret = JsonMapper.ToObject<updateGameConfigRet>(jd.ToJson());
            updateGameConfigCallback.Invoke(ret);
        }

        public void OnnotifyIconClickSceneDidCallback(JsonData jd)
        {
            notifyIconClickSceneDidRet ret = JsonMapper.ToObject<notifyIconClickSceneDidRet>(jd.ToJson());
            notifyIconClickSceneDidCallback.Invoke(ret);
        }

        public void OnFetchActivityUrlCallback(JsonData jd)
        {
            FetchActivityUrlWithIdResultRet ret = JsonMapper.ToObject<FetchActivityUrlWithIdResultRet>(jd.ToJson());
            fetchActivityUrlCallback.Invoke(ret);
        }

        public void OnQueryConfigValueCallback(JsonData jd)
        {
            QueryConfigValueResultRet ret = JsonMapper.ToObject<QueryConfigValueResultRet>(jd.ToJson());
            queryConfigValueCallback.Invoke(ret);
        }

        public void OnQueryActivityNotifyCallback(JsonData jd)
        {
            QueryActivityNotifyRet ret = JsonMapper.ToObject<QueryActivityNotifyRet>(jd.ToJson());
            queryActivityNotifyCallback.Invoke(ret);
        }

        public void OnQueryActivityNotifyDataCallback(JsonData jd)
        {
            queryActivityNotifyDataRet ret = JsonMapper.ToObject<queryActivityNotifyDataRet>(jd.ToJson());
            if (null == ret.data)
            {
                ret.data = new List<NotifyDataBean>();
            }
            queryActivityNotifyDataCallback.Invoke(ret);
        }

        public void OnOperatePageCallback(JsonData jd)
        {
            OperatePageRet ret = JsonMapper.ToObject<OperatePageRet>(jd.ToJson());
            operatePageCallback.Invoke(ret);
        }

        public void OnSendMessageToPageCallback(JsonData jd)
        {
            CallbackResult ret = JsonMapper.ToObject<CallbackResult>(jd.ToJson());
            sendMessageToPageCallback.Invoke(ret);
        }

        public void OnSyncGeckoCallback(JsonData jd)
        {
            CallbackResult ret = JsonMapper.ToObject<CallbackResult>(jd.ToJson());
            syncGeckoCallback.Invoke(ret);
        }

        public void OnGetRNDebugCallback(JsonData jd)
        {
            GetRNDebugRet ret = JsonMapper.ToObject<GetRNDebugRet>(jd.ToJson());
            getRNDebugCallback.Invoke(ret);
        }

        public void OpenUrlCallback(JsonData jd)
        {
            OpenUrlResultRet ret = JsonMapper.ToObject<OpenUrlResultRet>(jd.ToJson());
            openUrlCallback.Invoke(ret);
        }

        public void HomeDidLoadCallback(JsonData jd)
        {
            HomeDidLoadResultRet ret = JsonMapper.ToObject<HomeDidLoadResultRet>(jd.ToJson());
            homeDidLoadCallback.Invoke(ret);
        }

        public void WarDidEndCallback(JsonData jd)
        {
            WarDidEndResultRet ret = JsonMapper.ToObject<WarDidEndResultRet>(jd.ToJson());
            warDidEndCallback.Invoke(ret);
        }

        public void SetUpOrentationCallback(JsonData jd)
        {
            SetOrientationResultRet ret = JsonMapper.ToObject<SetOrientationResultRet>(jd.ToJson());
            setOrientationCallback.Invoke(ret);
        }

        public void ListenNativeNotificationCallback(JsonData jd)
        {
            if (!GMReactUnityMgr.instance.config.ruDisable)
            {
                if (jd.ContainsKey("messageType"))
                {
                    string typeString = jd["messageType"].ToString();
                    if (!String.IsNullOrEmpty(typeString) && typeString.Equals("ru_message"))
                    {
                        string action = jd["action"].ToString();
                        if (!String.IsNullOrEmpty(action))
                        {
                            if (action.Equals("init"))
                            {
                                // 初始化
                                RNUMain.Init();
                            } else if (action.Equals("openurl"))
                            {
                                // 使用RU 打开 URL
                                JsonData paramsbody = jd["params"];
                                if (paramsbody !=null && paramsbody.ContainsKey("url"))
                                {
                                    string ruUrl = paramsbody["url"].ToString();
                                    RNUMain.OpenPage(ruUrl);
                                }
                            } else
                            {
                                // 把消息透传给RU
                            }
                        }
                        
                        // 发送给RU的消息，不再传递给游戏Unity
                        return;
                    }
                }
            }
            unityNotificationCallback.Invoke(jd.ToJson());
        }

        public void GumihoEngineNativeNotificationCallback(JsonData jd)
        {
            gumihoEngineNotificationCallBack.Invoke(jd);
        }

        public void ShowTestPageCallBack(JsonData jd)
        {
            ShowTestPageRet ret = JsonMapper.ToObject<ShowTestPageRet>(jd.ToJson());
            showTestPageCallback.Invoke(ret);
        }
        
        /// <summary>
        /// 失败统一回调，用于调试接口
        /// </summary>
        public void OnFailCallback(int code, string failMsg)
        {
            LogUtils.D("接口访问失败 " + code.ToString() + " " + failMsg);
        }

        /// <summary>
        /// 超时统一回调
        /// </summary>
        public void OnTimeoutCallback()
        {
            JsonData jd = new JsonData();
            jd["code"] = -1;
            jd["message"] = "RN - request time out";
            if (this.OnSuccess != null)
            {
                this.OnSuccess(jd);
            }
        }
    }
}