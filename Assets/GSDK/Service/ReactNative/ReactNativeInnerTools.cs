using System;
using System.Collections.Generic;
using GMSDK;
using UNBridgeLib.LitJson;

namespace GSDK
{
    internal class ReactNativeInnerTools
    {
        #region 错误转换

        internal static Result ConvertResult(CallbackResult result)
        {
            int code = ErrorCode.Success;
            if (result.code != 0)
            {
                switch (result.code)
                {
                    case -3200001:
                        code = ErrorCode.ReactNativeInitWithoutParams;
                        break;
                    case -3200002:
                        code = ErrorCode.ReactNativeNotSupport;
                        break;
                    case -3210001:
                        code = ErrorCode.ReactNativeNetworkError;
                        break;
                    default:
                        code = ErrorCode.ReactNativeUnknownError;
                        break;
                }
            }

            if (code != ErrorCode.Success)
            {
                GLog.LogError("[九尾] Error! " + code + "[" +
                              InnerTools.ConvertErrorCode(code) + "]" + ", source:" +
                              result.code + "[" + result.message + "]");
            }

            return new Result(code, result.code, 0, result.message);
        }

        public static int ConvertNotificationError(int code)
        {
            int errorCode;
            switch (code)
            {
                case -1001:
                    errorCode = ErrorCode.ReactNativeJSBundleNotFound;
                    break;
                case -1002:
                    errorCode = ErrorCode.ReactNativeLoadJSBundleFailed;
                    break;
                case -1003:
                    errorCode = ErrorCode.ReactNativeUnsupportedDevice;
                    break;
                case -1004:
                    errorCode = ErrorCode.ReactNativeInvalidURLParameters;
                    break;
                case -2001:
                    errorCode = ErrorCode.ReactNativeJSRuntimeError;
                    break;
                case -9999:
                    errorCode = ErrorCode.ReactNativeUnknownError;
                    break;
                default:
                    GLog.LogError("未知的错误: " + code);
                    errorCode = ErrorCode.ReactNativeUnknownError;
                    break;
            }

            GLog.LogError("[九尾] Error! " + errorCode + "[" +
                          InnerTools.ConvertErrorCode(errorCode) + "]");
            return errorCode;
        }

        public static int ConvertReactNativeSendMessageError(int code)
        {
            if (code == 0)
            {
                return ErrorCode.Success;
            }
            else if (code == -1)
            {
                return ErrorCode.ReactNativeOperationFailed;
            }
            else
            {
                return ErrorCode.ReactNativeUnknownError;
            }
        }

        internal static Result ConvertControlWindowResult(OperatePageRet result)
        {
            Result gResult = ReactNativeInnerTools.ConvertResult(result);
            if (gResult.IsSuccess && !result.status)
            {
                gResult.Error = ErrorCode.ReactNativeOperationFailed;
                GLog.LogError("[九尾] Error! " + gResult.Error + "[" +
                              InnerTools.ConvertErrorCode(gResult.Error) + "]" + ", source:" +
                              result.code + "[" + result.message + "]");
            }

            return gResult;
        }

        internal static Result ConvertOpenPageResult(openPageRet result)
        {
            Result gResult;
            if (result.code == -1)
            {
                gResult = new Result(ErrorCode.ReactNativeCreateWindowFailed, result.message);
                GLog.LogError("[九尾] Error! " + gResult.Error + "[" +
                              InnerTools.ConvertErrorCode(gResult.Error) + "]" + ", source:" +
                              result.code + "[" + result.message + "]");
            }
            else
            {
                gResult = ConvertResult(result);
            }

            return gResult;
        }

        internal static Result ConvertFetchBadgeResult(queryActivityNotifyDataRet result)
        {
            Result gResult;
            if (result.data.Count != 1)
            {
                gResult = new Result(ErrorCode.ReactNativeInvalidPageID, result.message);
                GLog.LogError("错误的红点数量: " + result.data.Count);
                GLog.LogError("[ReactNative] Error! " + gResult.Error + "[" +
                              InnerTools.ConvertErrorCode(gResult.Error) + "]" + ", source:" +
                              result.code + "[" + result.message + "]");
            }
            else
            {
                gResult = ConvertResult(result);
            }

            return gResult;
        }

        internal static Result ConvertOpenReferralPageResult(openPageRet result)
        {
            Result gResult;
            if (result.code == -1)
            {
                gResult = new Result(ErrorCode.ReactNativeOpenReferralPageFailed, result.message);
                GLog.LogError("[九尾] Error! " + gResult.Error + "[" +
                              InnerTools.ConvertErrorCode(gResult.Error) + "]" + ", source:" +
                              result.code + "[" + result.message + "]");
            }
            else
            {
                gResult = ConvertResult(result);
            }

            return gResult;
        }

        #endregion

        #region 数据转换

        public static List<ReactNativePage> ConvertPages(List<openFaceData> list)
        {
            List<ReactNativePage> newList = new List<ReactNativePage>();
            foreach (var data in list)
            {
                newList.Add(new ReactNativePageImplementation(ConvertActivityID(data.activityId), data.activityUrl,
                    data.inGameId));
            }

            return newList;
        }

        public static List<ReactNativePage> ConvertPages(List<SceneData> list)
        {
            List<ReactNativePage> newList = new List<ReactNativePage>();
            foreach (var data in list)
            {
                newList.Add(new ReactNativePageImplementation(ConvertActivityID(data.activityId), data.activityUrl,
                    data.inGameId));
            }

            return newList;
        }

        public static ReactNativeBadgeType ConvertBadgeType(int type)
        {
            switch (type)
            {
                case 1:
                    return ReactNativeBadgeType.Dot;
                case 2:
                    return ReactNativeBadgeType.Number;
                case 0:
                    return ReactNativeBadgeType.None;
                default:
                    GLog.LogError("未知的红点类型: " + type);
                    return ReactNativeBadgeType.None;
            }
        }

        public static ReactNativeBadge ConvertBadge(NotifyDataBean notify)
        {
            return new ReactNativeBadge()
            {
                InGameID = notify.inGameId,
                ID = ConvertActivityID(notify.id),
                Count = notify.notify.count,
                Type = ConvertBadgeType(notify.notify.type),
                Extra = notify.notify.custom
            };
        }

        public static ReactNativeBadge ConvertBadge(List<NotifyDataBean> data)
        {
            if (data.Count == 1)
            {
                return ConvertBadge(data[0]);
            }
            else
            {
                return null;
            }
        }

        public static ulong ConvertActivityID(string activityId)
        {
            ulong id = 0;
            if (!ulong.TryParse(activityId, out id))
            {
                GLog.LogError("转换活动ID失败: " + id);
            }

            return id;
        }

        public static List<ReactNativeBadge> ConvertBadgeData(List<NotifyDataBean> data)
        {
            List<ReactNativeBadge> newList = new List<ReactNativeBadge>();
            foreach (var notify in data)
            {
                newList.Add(ConvertBadge(notify));
            }

            return newList;
        }

        public static string ConvertSceneType(ReactNativeSceneType type)
        {
            switch (type)
            {
                case ReactNativeSceneType.Home:
                    return "home_show";
                case ReactNativeSceneType.Lobby:
                    return "icon_click";
                case ReactNativeSceneType.HighlightMoments:
                    return "war_end";
                default:
                    GLog.LogError("未知的活动类型: " + type);
                    return null;
            }
        }

        public static ReactNativeSceneType ConvertSceneType(int type)
        {
            switch (type)
            {
                case 0:
                    return ReactNativeSceneType.Unknown;
                case 1:
                    return ReactNativeSceneType.Home;
                case 2:
                    return ReactNativeSceneType.Lobby;
                case 3:
                    return ReactNativeSceneType.HighlightMoments;
                case 4:
                    return ReactNativeSceneType.Referral;
                default:
                    GLog.LogError("未知的活动类型: " + type);
                    return ReactNativeSceneType.Unknown;
            }
        }

        public static ReactNativeWindowType ConvertWindowType(int type)
        {
            switch (type)
            {
                case 1:
                    return ReactNativeWindowType.ReactNative;
                case 2:
                    return ReactNativeWindowType.Web;
            }

            GLog.LogError("Unexpected type received: " + type);
            return ReactNativeWindowType.ReactNative;
        }

        public static ReactNativeMessage ConvertMessage(string payload)
        {
            JsonData data = JsonMapper.ToObject(payload);
            ReactNativeMessage message = new ReactNativeMessage();
            message.Raw = payload;
            try
            {
                message.Type = (string)data["action"];
                message.Message = (string)data["message"];
                message.Parameters = data["params"].ToJson();
            }
            catch (Exception e)
            {
                GLog.LogException(e);
            }

            return message;
        }

        public static ReactNativeWindow ConvertWindow(openPageRet result)
        {
            return new ReactNativeWindowImplementation(result.windowId, ConvertWindowType(result.type));
        }

        public static ReactNativeWindow ConvertWindow(PageCloseResult result)
        {
            return new ReactNativeWindowImplementation(result.windowId, false, "", result.inGameId, ConvertSceneType(result.pageType));
        }

        public static ReactNativeWindow ConvertWindow(RNPage rnpage)
        {
            return new ReactNativeWindowImplementation(rnpage.windowId, rnpage.isShowing, rnpage.url, rnpage.inGameId, ConvertSceneType(rnpage.type));
        }

        #endregion
    }
}