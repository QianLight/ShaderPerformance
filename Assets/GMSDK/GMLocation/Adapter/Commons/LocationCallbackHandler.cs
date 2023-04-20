using System;
using UNBridgeLib;
using UNBridgeLib.LitJson;
using UnityEngine;

namespace GMSDK
{
    public class LocationCallbackHandler : BridgeCallBack
    {
        public Action<LocationCallback> LocationCallback;
        public Action<GMNearPoiDataResult> GETPoiDataCallBack;
        public Action<CallbackResult> deletePoiDataCallBack;
        public Action<CallbackResult> reportPoiDataCallBack;


        public void OnReportPoiDataCallBack(JsonData jd)
        {
            CallbackResult ret = SdkUtil.ToObject<CallbackResult>(jd.ToJson());
            SdkUtil.InvokeAction<CallbackResult>(reportPoiDataCallBack, ret);
        }

        public LocationCallbackHandler()
        {
            OnFailed = OnFailCallBack;
            OnTimeout = OnTimeoutCallBack;
        }

        /// <summary>
        /// native回调成功
        /// </summary>
        /// <param name="data">native返回的json数据</param>
        public void OnLocationCallBack(JsonData data)
        {
            LogUtils.D("OnLocationCallBack");
            GMLocationModel location = SdkUtil.ToObject<GMLocationModel>(data.ToJson());
            LocationCallback result = new LocationCallback(location);
            result.code = location.code;
            result.message = location.message;
            SdkUtil.InvokeAction(LocationCallback, result);
        }

        public void OnGetPoiDataCallBack(JsonData jd)
        {
            GMNearPoiDataResult ret = SdkUtil.ToObject<GMNearPoiDataResult>(jd.ToJson());
            SdkUtil.InvokeAction<GMNearPoiDataResult>(GETPoiDataCallBack, ret);
        }

        public void OnDeletePoiDataCallBack(JsonData jd)
        {
            CallbackResult ret = SdkUtil.ToObject<CallbackResult>(jd.ToJson());
            SdkUtil.InvokeAction<CallbackResult>(deletePoiDataCallBack, ret);
        }

        /// <summary>
        /// 失败统一回调，用于调试接口
        /// </summary>
        public void OnFailCallBack(int code, string failMsg)
        {
            LogUtils.E("接口访问失败 " + code + " " + failMsg);
        }

        /// <summary>
        /// 超时统一回调
        /// </summary>
        public void OnTimeoutCallBack()
        {
            JsonData jd = new JsonData();
            jd["code"] = -321;
            jd["message"] = "Location - request time out";
            if (OnSuccess != null)
            {
                OnSuccess.Invoke(jd);
            }
        }
    }
}