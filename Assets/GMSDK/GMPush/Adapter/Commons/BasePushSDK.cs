using UNBridgeLib.LitJson;
using System;
using UNBridgeLib;

namespace GMSDK
{
    public class BasePushSDK
    {
        public BasePushSDK()
        {
#if UNITY_ANDROID
            UNBridge.Call(PushMethodName.Init, null);
#endif
        }

        /// <summary>
        /// 添加本地一次性倒计时任务推送
        /// </summary>
        /// <param name="title">必传，推送标题</param>
        /// <param name="body">必传，推送内容</param>
        /// <param name="timeInterval">必传，倒计时时间（秒）</param>
        /// <param name="subTitle">选传，推送副标题</param>
        /// <param name="identifier">选传，推送任务唯一标识</param>
        /// <param name="userInfo">选传，推送回调中可获取到的额外信息</param>
        /// <param name="callback">结果回调</param>
        public void SDKAddLocalPushCountdownOnce(LocalPushModel model, Action<LocalPushResult> callback)
        {
            PushCallbackHandler pushCallbackHandler = new PushCallbackHandler()
            {
                localPushResult = (LocalPushResult result) =>
                {
                    callback(result);
                }
            };
            JsonData param = new JsonData();
            param["title"] = model.title;
            param["subTitle"] = model.subTitle;
            param["body"] = model.body;
            param["timeInterval"] = model.timeInterval;
            var identifier = model.identifier;
            if (string.IsNullOrEmpty(identifier))
            {
#if UNITY_ANDROID
                identifier = "";
#elif UNITY_IOS
                identifier = null;
#endif
            }
            param["identifier"] = identifier;
            pushCallbackHandler.OnSuccess = new OnSuccessDelegate(pushCallbackHandler.OnLocalPushCallBack);
            UNBridge.Call(PushMethodName.LocalPushCountdownOnce, param, pushCallbackHandler);
        }

        /// <summary>
        /// 添加本地循环倒计时任务推送
        /// </summary>
        /// <param name="title">必传，推送标题</param>
        /// <param name="body">必传，推送内容</param>
        /// <param name="timeInterval">必传，倒计时时间（秒）</param>
        /// <param name="subTitle">选传，推送副标题</param>
        /// <param name="identifier">选传，推送任务唯一标识</param>
        /// <param name="userInfo">选传，推送回调中可获取到的额外信息</param>
        /// <param name="callback">结果回调</param>
        public void SDKAddLocalPushCountdownRepeat(LocalPushModel model, Action<LocalPushResult> callback)
        {
            PushCallbackHandler pushCallbackHandler = new PushCallbackHandler()
            {
                localPushResult = (LocalPushResult result) =>
                {
                    callback(result);
                }
            };
            JsonData param = new JsonData();
            param["title"] = model.title;
            param["subTitle"] = model.subTitle;
            param["body"] = model.body;
            param["timeInterval"] = model.timeInterval;
            var identifier = model.identifier;
            if (string.IsNullOrEmpty(identifier))
            {
#if UNITY_ANDROID
                identifier = "";
#elif UNITY_IOS
                identifier = null;
#endif
            }
            param["identifier"] = identifier;
            pushCallbackHandler.OnSuccess = new OnSuccessDelegate(pushCallbackHandler.OnLocalPushCallBack);
            UNBridge.Call(PushMethodName.LocalPushCountdownRepeat, param, pushCallbackHandler);
        }

        /// <summary>
        /// 添加本地一次性定时任务推送
        /// </summary>
        /// <param name="title">必传，推送标题</param>
        /// <param name="body">必传，推送内容</param>
        /// <param name="year">必传，年</param>
        /// <param name="month">必传，月</param>
        /// <param name="day">必传，日</param>
        /// <param name="hour">必传，小时</param>
        /// <param name="minute">必传，分</param>
        /// <param name="second">必传，秒</param>
        /// <param name="subTitle">选传，推送副标题</param>
        /// <param name="identifier">选传，推送任务唯一标识</param>
        /// <param name="userInfo">选传，推送回调中可获取到的额外信息</param>
        /// <param name="callback">结果回调</param>
        public void SDKAddLocalPushTimingOnce(LocalPushModel model, Action<LocalPushResult> callback)
        {
            PushCallbackHandler pushCallbackHandler = new PushCallbackHandler()
            {
                localPushResult = (LocalPushResult result) =>
                {
                    callback(result);
                }
            };
            JsonData param = new JsonData();
            param["title"] = model.title;
            param["subTitle"] = model.subTitle;
            param["body"] = model.body;
            param["year"] = model.year;
            param["month"] = model.month;
            param["day"] = model.day;
            param["hour"] = model.hour;
            param["minute"] = model.minute;
            param["second"] = model.second;
            var identifier = model.identifier;
            if (string.IsNullOrEmpty(identifier))
            {
#if UNITY_ANDROID
                identifier = "";
#elif UNITY_IOS
                identifier = null;
#endif
            }
            param["identifier"] = identifier;
            pushCallbackHandler.OnSuccess = new OnSuccessDelegate(pushCallbackHandler.OnLocalPushCallBack);
            UNBridge.Call(PushMethodName.LocalPushTimingOnce, param, pushCallbackHandler);
        }

        /// <summary>
        /// 添加本地每天循环任务推送
        /// </summary>
        /// <param name="title">必传，推送标题</param>
        /// <param name="body">必传，推送内容</param>
        /// <param name="hour">必传，小时</param>
        /// <param name="minute">必传，分</param>
        /// <param name="second">必传，秒</param>
        /// <param name="subTitle">选传，推送副标题</param>
        /// <param name="identifier">选传，推送任务唯一标识</param>
        /// <param name="userInfo">选传，推送回调中可获取到的额外信息</param>
        /// <param name="callback">结果回调</param>
        public void SDKAddLocalPushTimingRepeatDay(LocalPushModel model, Action<LocalPushResult> callback)
        {
            PushCallbackHandler pushCallbackHandler = new PushCallbackHandler()
            {
                localPushResult = (LocalPushResult result) =>
                {
                    callback(result);
                }
            };
            JsonData param = new JsonData();
            param["title"] = model.title;
            param["subTitle"] = model.subTitle;
            param["body"] = model.body;
            param["hour"] = model.hour;
            param["minute"] = model.minute;
            param["second"] = model.second;
            var identifier = model.identifier;
            if (string.IsNullOrEmpty(identifier))
            {
#if UNITY_ANDROID
                identifier = "";
#elif UNITY_IOS
                identifier = null;
#endif
            }
            param["identifier"] = identifier;
            pushCallbackHandler.OnSuccess = new OnSuccessDelegate(pushCallbackHandler.OnLocalPushCallBack);
            UNBridge.Call(PushMethodName.LocalPushTimingRepeatDay, param, pushCallbackHandler);
        }

        /// <summary>
        /// 添加本地每周循环任务推送
        /// </summary>
        /// <param name="title">必传，推送标题</param>
        /// <param name="body">必传，推送内容</param>
        /// <param name="weekday">必传，每周的第几天</param>
        /// <param name="hour">必传，小时</param>
        /// <param name="minute">必传，分</param>
        /// <param name="second">必传，秒</param>
        /// <param name="subTitle">选传，推送副标题</param>
        /// <param name="identifier">选传，推送任务唯一标识</param>
        /// <param name="userInfo">选传，推送回调中可获取到的额外信息</param>
        /// <param name="callback">结果回调</param>
        public void SDKAddLocalPushTimingRepeatWeek(LocalPushModel model, Action<LocalPushResult> callback)
        {
            PushCallbackHandler pushCallbackHandler = new PushCallbackHandler()
            {
                localPushResult = (LocalPushResult result) =>
                {
                    callback(result);
                }
            };
            JsonData param = new JsonData();
            param["title"] = model.title;
            param["subTitle"] = model.subTitle;
            param["body"] = model.body;
            param["weekday"] = model.weekday;
            param["hour"] = model.hour;
            param["minute"] = model.minute;
            param["second"] = model.second;
            var identifier = model.identifier;
            if (string.IsNullOrEmpty(identifier))
            {
#if UNITY_ANDROID
                identifier = "";
#elif UNITY_IOS
                identifier = null;
#endif
            }
            param["identifier"] = identifier;
            pushCallbackHandler.OnSuccess = new OnSuccessDelegate(pushCallbackHandler.OnLocalPushCallBack);
            UNBridge.Call(PushMethodName.LocalPushTimingRepeatWeek, param, pushCallbackHandler);
        }

        /// <summary>
        /// 添加本地每月循环任务推送，可选每月的某日推送（传入day），或者每月的某周第N天（传入week+weekday）
        /// </summary>
        /// <param name="title">必传，推送标题</param>
        /// <param name="body">必传，推送内容</param>
        /// <param name="week">选传，每个月的第几周</param>
        /// <param name="weekday">选传，每周的第几天</param>
        /// <param name="day">选传，每个月的第几日</param>
        /// <param name="hour">必传，小时</param>
        /// <param name="minute">必传，分</param>
        /// <param name="second">必传，秒</param>
        /// <param name="subTitle">选传，推送副标题</param>
        /// <param name="identifier">选传，推送任务唯一标识</param>
        /// <param name="userInfo">选传，推送回调中可获取到的额外信息</param>
        /// <param name="callback">结果回调</param>
        public void SDKAddLocalPushTimingRepeatMonth(LocalPushModel model, Action<LocalPushResult> callback)
        {
            PushCallbackHandler pushCallbackHandler = new PushCallbackHandler()
            {
                localPushResult = (LocalPushResult result) =>
                {
                    callback(result);
                }
            };
            JsonData param = new JsonData();
            param["title"] = model.title;
            param["subTitle"] = model.subTitle;
            param["body"] = model.body;
            param["day"] = model.day;
            param["week"] = model.week;
            param["weekday"] = model.weekday;
            param["hour"] = model.hour;
            param["minute"] = model.minute;
            param["second"] = model.second;
            #if UNITY_ANDROID
            param["year"] = model.year;
            param["month"] = model.month;
            #endif
            var identifier = model.identifier;
            if (string.IsNullOrEmpty(identifier))
            {
#if UNITY_ANDROID
                identifier = "";
#elif UNITY_IOS
                identifier = null;
#endif
            }
            param["identifier"] = identifier;
            pushCallbackHandler.OnSuccess = new OnSuccessDelegate(pushCallbackHandler.OnLocalPushCallBack);
            UNBridge.Call(PushMethodName.LocalPushTimingRepeatMonth, param, pushCallbackHandler);
        }

        /// <summary>
        /// 根据任务唯一标识移除单个推送
        /// </summary>
        /// <param name="identifier">必传，推送任务唯一标识</param>
        public void SDKRemoveLocalPush(String identifier)
        {
            JsonData param = new JsonData();
            param["identifier"] = identifier;
            UNBridge.Call(PushMethodName.LocalPushRemoveIdentifier, param);
        }
        
        /// <summary>
        /// 移除所有推送
        /// </summary>
        public void SDKRemoveAllLocalPush()
        {
            UNBridge.Call(PushMethodName.LocalPushRemoveAllIdentifier, null);
        }
    }
}