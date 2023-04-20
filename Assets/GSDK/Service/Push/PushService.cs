using System;
using GMSDK;
using UNBridgeLib.LitJson;

namespace GSDK
{
    public class PushService : IPushService
    {
        private BasePushSDK _pushSDK;

        public PushService()
        {
            if (_pushSDK == null)
            {
                _pushSDK = new BasePushSDK();
            }
        }

        private void SendNotificationAfter(Notification notification, SendNotificationDelegate callback)
        {
            var content = notification.Content;
            if (content == null)
            {
                var ret = new Result(ErrorCode.PushLackOfParametersError, "PushContent is null.");
                callback.Invoke(ret, null);
                GLog.LogError("PushContent is null.");
                return;
            }

            LocalPushModel model;
            try
            {
                model = ContentConvertToModel(content);
            }
            catch (JsonException e)
            {
                callback.Invoke(new Result(ErrorCode.PushJsonConvertError, "userInfo is not json, others: " + e), "");
                return;
            }

            model.timeInterval = notification.TimeInterval;

            if (notification.IsRepeat)
            {
                _pushSDK.SDKAddLocalPushCountdownRepeat(model, result =>
                {
                    InnerTools.SafeInvoke((() =>
                    {
                        var ret = PushInnerTools.ConvertPushError(result);
                        callback.Invoke(ret, result.identifier);
                    }));
                });
            }
            else
            {
                _pushSDK.SDKAddLocalPushCountdownOnce(model, result =>
                {
                    InnerTools.SafeInvoke((() =>
                    {
                        var ret = PushInnerTools.ConvertPushError(result);
                        callback.Invoke(ret, result.identifier);
                    }));
                });
            }
        }

        public void SendNotification(Notification repeat, SendNotificationDelegate callback)
        {
            if (repeat == null)
            {
                var ret = new Result(ErrorCode.PushLackOfParametersError, "Notification is null.");
                callback.Invoke(ret, "");
                GLog.LogError("Notification is null.");
                return;
            }

            var clock = repeat.Clock;

            // Clock是用于精确到时分秒的推送时间，若为空则说明是间隔性推送
            if (repeat.Clock == null)
            {
                SendNotificationAfter(repeat, callback);
                return;
            }

            LocalPushModel model;
            try
            {
                model = ContentConvertToModel(repeat.Content, repeat.Clock.Time);
            }
            catch (JsonException e)
            {
                InnerTools.SafeInvoke((() =>
                {
                    callback.Invoke(
                        new Result(ErrorCode.PushJsonConvertError, "userInfo is not json, others: " + e), "");
                }));
                return;
            }

            switch (clock.Type)
            {
                case RepeatType.Once:
                    var date = clock.Date;
                    model.year = date.Year;
                    model.month = date.Month;
                    model.day = date.Day;
                    _pushSDK.SDKAddLocalPushTimingOnce(model, result =>
                    {
                        var ret = PushInnerTools.ConvertPushError(result);
                        InnerTools.SafeInvoke((() => { callback.Invoke(ret, result.identifier); }));
                    });
                    break;
                case RepeatType.Daily:
                    _pushSDK.SDKAddLocalPushTimingRepeatDay(model, result =>
                    {
                        var ret = PushInnerTools.ConvertPushError(result);
                        InnerTools.SafeInvoke((() => { callback.Invoke(ret, result.identifier); }));
                    });
                    break;
                case RepeatType.Weekly:
                    model.weekday = clock.Weekday;
                    _pushSDK.SDKAddLocalPushTimingRepeatWeek(model, result =>
                    {
                        var ret = PushInnerTools.ConvertPushError(result);
                        InnerTools.SafeInvoke((() => { callback.Invoke(ret, result.identifier); }));
                    });
                    break;
                case RepeatType.WeekDayOfMonth:
                    model.weekday = clock.Weekday;
                    model.week = clock.Week;
                    _pushSDK.SDKAddLocalPushTimingRepeatMonth(model, result =>
                    {
                        var ret = PushInnerTools.ConvertPushError(result);
                        InnerTools.SafeInvoke((() => { callback.Invoke(ret, result.identifier); }));
                    });
                    break;
                case RepeatType.DayOfMonth:
                    model.day = clock.Date.Day;
                    _pushSDK.SDKAddLocalPushTimingRepeatMonth(model, result =>
                    {
                        var ret = PushInnerTools.ConvertPushError(result);
                        InnerTools.SafeInvoke((() => { callback.Invoke(ret, result.identifier); }));
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void RemoveNotification(string identifier)
        {
            _pushSDK.SDKRemoveLocalPush(identifier);
        }

        public void RemoveAllNotification()
        {
            _pushSDK.SDKRemoveAllLocalPush();
        }

        /// <summary>
        /// 将PushContent和PushTime转成LocalPushModel
        /// </summary>
        private static LocalPushModel ContentConvertToModel(PushContent content, PushTime time)
        {
            var model = ContentConvertToModel(content);
            model.hour = time.Hour;
            model.minute = time.Minute;
            model.second = time.Second;

            return model;
        }

        /// <summary>
        /// 将PushContent转成LocalPushModel
        /// </summary>
        private static LocalPushModel ContentConvertToModel(PushContent content)
        {
            var model = new LocalPushModel
            {
                title = content.Title,
                body = content.Content,
                subTitle = content.SubTitle,
                identifier = content.Identifier
            };

            try
            {
                if (!string.IsNullOrEmpty(content.UserInfo))
                {
                    var json = JsonMapper.ToObject(content.UserInfo);
                    model.userInfo = json;
                }
            }
            catch (Exception e)
            {
                GLog.LogError("Convert to JsonData Error: " + e.Message);
                throw;
            }

            return model;
        }
    }

    #region Others

    /// <summary>
    /// 重复类型
    /// </summary>
    public enum RepeatType
    {
        Once, // 特定日期推送一次
        Daily, // 每日重复
        Weekly, // 每周重复
        WeekDayOfMonth, // 每月第x周的第x天重复
        DayOfMonth, // 每月第x天重复
    }


    /// <summary>
    /// 推送通知
    /// </summary>
    public abstract class Notification
    {
        /// <summary>
        /// 推送时间
        /// </summary>
        public PushClock Clock;

        /// <summary>
        /// 推送内容
        /// </summary>
        public PushContent Content;

        /// <summary>
        /// 间隔时间
        /// </summary>
        public long TimeInterval;

        /// <summary>
        /// 是否支持重复推送
        /// </summary>
        public bool IsRepeat;

        #region Initialize

        protected Notification(PushContent content)
        {
            if (content == null)
            {
                GLog.LogError("PushContent is null.");
                return;
            }

            Content = content;
        }

        protected Notification(PushContent content, PushTime pushTime) : this(content)
        {
            Clock = new PushClock();
            Clock.Time = pushTime;
        }

        #endregion
    }

    #endregion
}