using System;
using System.Collections.Generic;
using UNBridgeLib;
using UNBridgeLib.LitJson;

namespace GMSDK
{
    public class ReactUnityCallbackHandler : BridgeCallBack
    {
        public Action<CallbackResult> commonSuccessAction;

        public Action<RUGetKVConfigResult> getKVConfigAction;
        public Action<RURequestResult> requestConfigAction;

        public Action<RUOpenUrlResult> openUrlAction;

        public Action<RUGetUserInfoResult> getUserInfoAction;

        public Action<RUGetCommonParamsResult> getCommonParamsAction;
        public Action<RUMonitorEventResult> monitorEventAction;

        public Action<RULinkingCanOpenUrlResult> linkingCanOpenUrlAction;
        public Action<CallbackResult> syncGeckoCallback;


        public ReactUnityCallbackHandler()
        {
            this.OnFailed = new OnFailedDelegate(OnFailCallback);
            this.OnTimeout = new OnTimeoutDelegate(OnTimeoutCallback);
        }

        // 获取云控信息
        public void GetKVConfigCallback(JsonData jd)
        {
            RUGetKVConfigResult ret = JsonMapper.ToObject<RUGetKVConfigResult>(jd.ToJson());
            getKVConfigAction.Invoke(ret);
        }

        /// <summary>
        /// 通用成功回调
        /// </summary>
        public void OnSuccessCallback(JsonData jd) {
            CallbackResult ret = JsonMapper.ToObject<CallbackResult>(jd.ToJson());
            commonSuccessAction.Invoke(ret);
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

        public void OnRequestCallback(JsonData jd)
        {
            RURequestResult ret = JsonMapper.ToObject<RURequestResult>(jd.ToJson());
            requestConfigAction.Invoke(ret);
        }

        public void OpenUrlCallback(JsonData jd)
        {
            RUOpenUrlResult ret = JsonMapper.ToObject<RUOpenUrlResult>(jd.ToJson());
            openUrlAction.Invoke(ret);
        }

        public void GetUserInfoCallback(JsonData jd)
        {
            RUGetUserInfoResult ret = JsonMapper.ToObject<RUGetUserInfoResult>(jd.ToJson());
            getUserInfoAction.Invoke(ret);
        }

        public void GetCommonParamsCallback(JsonData jd)
        {
            RUGetCommonParamsResult ret = JsonMapper.ToObject<RUGetCommonParamsResult>(jd.ToJson());
            getCommonParamsAction.Invoke(ret);
        }

        public void MonitorEventCallback(JsonData jd)
        {
            RUMonitorEventResult ret = JsonMapper.ToObject<RUMonitorEventResult>(jd.ToJson());
            monitorEventAction.Invoke(ret);
        }

        public void onLinkingCanOpenUrlCallback(JsonData jd)
        {
            RULinkingCanOpenUrlResult ret = JsonMapper.ToObject<RULinkingCanOpenUrlResult>(jd.ToJson());
            linkingCanOpenUrlAction.Invoke(ret);
        }

        public void OnSyncGeckoCallback(JsonData jd)
        {
            CallbackResult ret = JsonMapper.ToObject<CallbackResult>(jd.ToJson());
            syncGeckoCallback.Invoke(ret);
        }

    }
}
