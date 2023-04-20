namespace GSDK
{
    /// <summary>
    /// 返回通知是否设置成功
    /// </summary>
    /// <para>
    /// 可能返回的错误码：
    /// Success：成功
    /// PushIntervalTimeError：ios:循环倒计时任务时间不能小于60s
    /// PushLackOfParametersError：缺少参数
    /// PushUnsupportedError：ios:类型错误（ios9 有些推送无法支持）
    /// PushExceedLimitError：数量已到上线，不能添加更多，需要去云控调整
    /// </para>
    /// <param name="result">判断是否设置成功，包含上述错误码</param>
    /// <param name="identifier">返回通知的唯一表示同意，若调用接口时没有自定义，则会在回调时返回一串随机码</param>
    public delegate void SendNotificationDelegate(Result result, string identifier);

    /// <summary>
    /// 用于获取实例进行接口调用
    /// e.g.Push.Service.MethodName();
    /// </summary>
    public static class Push
    {
        public static IPushService Service
        {
            get { return ServiceProvider.Instance.GetService(ServiceType.Push) as IPushService; }
        }
    }

    /// <summary>
    /// 您可自定义推送规则，本模块支持以下重复推送：
    /// 1 间隔重复 通过CreateIntervalRepeat, isOnce = true
    /// 2 间隔不重复，仅一次 通过CreateIntervalRepeat, isOnce = false
    /// 3 特定日期和时间推送一次 CreateOncePush
    /// 4 每日重复 CreateDailyRepeat
    /// 5 每周重复 CreateWeeklyRepeat
    /// 6 每月重复 CreateMonthlyRepeat
    /// /// </summary>
    public interface IPushService : IService
    {
        /// <summary>
        /// 定时发送通知
        /// 
        /// 1 间隔一段时间推送 CreateIntervalRepeat
        /// 2 特定日期推送一次 CreateOncePush
        /// 3 每日重复 CreateDailyRepeat
        /// 4 每周重复 CreateWeeklyRepeat
        /// 5 每月重复 CreateMonthlyRepeat
        /// </summary>
        /// <param name="notification">设置重复的规则和推送内容，通过NotificationRepeat的Create接口完成</param>
        /// <param name="callback">是否设置成功</param>
        void SendNotification(Notification notification, SendNotificationDelegate callback);

        /// <summary>
        /// 根据唯一标识移除单个推送
        /// </summary>
        /// <param name="identifier">推送的唯一标识，可通过自定义设置，若未设置则可在设置</param>
        void RemoveNotification(string identifier);
        
        /// <summary>
        /// 移除所有推送通知
        /// </summary>
        void RemoveAllNotification();
    }

    /// <summary>
    /// 仅推送一次的规则
    /// </summary>
    public class OneTimeNotification : Notification
    {
        /// <summary>
        /// 具体某年某月某日的某一时刻推送一次
        /// </summary>
        /// <param name="content">通知内容</param>
        /// <param name="date">推送日期</param>
        /// <param name="time">推送时间（时分秒）</param>
        /// <returns></returns>
        public static OneTimeNotification CreateOncePush(PushContent content, PushDate date, PushTime time)
        {
            var createOncePush = new OneTimeNotification(content, time);
            createOncePush.Clock.Type = RepeatType.Once;
            createOncePush.Clock.Date = date;

            return createOncePush;
        }

        private OneTimeNotification(PushContent content, PushTime pushTime) : base(content, pushTime)
        {
        }
    }

    /// <summary>
    /// 制定重复推送的规则
    /// </summary>
    public class RepeatNotification : Notification
    {
        #region Interval

        /// <summary>
        /// 间隔性重复。
        /// 例如若isOnce设置为false，每间隔10s发送一次通知；
        /// 若isOnce设置为true，则只发送一次，不会重复。
        /// </summary>
        /// <param name="content">推送内容</param>
        /// <param name="timeInterval">发送间隔时间，单位：秒</param>
        /// <param name="isRepeat">是否重复推送，true为重复推送；false为只推送一次</param>
        /// <returns></returns>
        public static RepeatNotification CreateIntervalRepeat(PushContent content, long timeInterval, bool isRepeat)
        {
            var repeat = new RepeatNotification(content);
            repeat.IsRepeat = isRepeat;
            repeat.TimeInterval = timeInterval;

            return repeat;
        }

        #endregion

        #region Repeat

        /// <summary>
        /// 每天重复
        /// </summary>
        /// <param name="content">通知内容</param>
        /// <param name="time">推送时间（时分秒）</param>
        /// <returns></returns>
        public static RepeatNotification CreateDailyRepeat(PushContent content, PushTime time)
        {
            var repeat = new RepeatNotification(content, time);
            repeat.Clock.Type = RepeatType.Daily;

            return repeat;
        }

        /// <summary>
        /// 每周第x天重复
        /// </summary>
        /// <param name="content">通知内容</param>
        /// <param name="weekday">一周内的第x天</param>
        /// <param name="time">推送时间（时分秒）</param>
        /// <returns></returns>
        public static RepeatNotification CreateWeeklyRepeat(PushContent content, int weekday, PushTime time)
        {
            var repeat = new RepeatNotification(content, time);
            repeat.Clock.Type = RepeatType.Weekly;
            repeat.Clock.Weekday = weekday;

            return repeat;
        }

        /// <summary>
        /// 每个月的第x周的第x天重复
        /// </summary>
        /// <param name="content">通知内容</param>
        /// <param name="week">一个月内的第x周</param>
        /// <param name="weekday">一周内的第x天</param>
        /// <param name="time">推送时间（时分秒）</param>
        /// <returns></returns>
        public static RepeatNotification CreateMonthlyRepeat(PushContent content, int week, int weekday, PushTime time)
        {
            var repeat = new RepeatNotification(content, time);
            repeat.Clock.Type = RepeatType.WeekDayOfMonth;
            repeat.Clock.Week = week;
            repeat.Clock.Weekday = weekday;

            return repeat;
        }

        /// <summary>
        /// 每个月的第x天重复
        /// </summary>
        /// <param name="content">通知内容</param>
        /// <param name="day">每个月的第x天</param>
        /// <param name="time">推送时间（时分秒）</param>
        /// <returns></returns>
        public static RepeatNotification CreateMonthlyRepeat(PushContent content, int day, PushTime time)
        {
            var repeat = new RepeatNotification(content, time);
            repeat.Clock.Type = RepeatType.DayOfMonth;
            repeat.Clock.Date = new PushDate(day);

            return repeat;
        }

        #endregion

        private RepeatNotification(PushContent content) : base(content)
        {
        }

        private RepeatNotification(PushContent content, PushTime pushTime) : base(content, pushTime)
        {
        }
    }

    /// <summary>
    /// 通知内容，包括必选：标题、描述；可选：副标题、通知的唯一标识
    /// </summary>
    public class PushContent
    {
        /** 必选 **/
        public string Title; // 标题

        public string Content; // 内容描述

        /** 可选 **/
        public string SubTitle; // 副标题

        public string Identifier; // 唯一标识，若不选择，则直接传入空，传入""会报错；若选择，则传入自定义id并记录

        public string UserInfo; // 推送回调中可获取到的额外信息，为json字符串

        public PushContent(string title, string content)
        {
            Title = title;
            Content = content;
        }
    }

    /// <summary>
    /// 制定推送时间，包含时间（时分秒）、日期等
    /// </summary>
    public class PushClock
    {
        protected internal RepeatType Type;

        public PushTime Time;
        public PushDate Date;
        public int Weekday;
        public int Week;
    }

    public class PushDate
    {
        public int Year;
        public int Month;
        public int Day;

        public PushDate(int day)
        {
            Day = day;
        }

        public PushDate(int year, int month, int day)
        {
            Day = day;
            Year = year;
            Month = month;
        }
    }

    /// <summary>
    /// 推送时间，包括时分秒
    /// </summary>
    public class PushTime
    {
        public int Hour;
        public int Minute;
        public int Second;

        public PushTime(int hour, int minute, int second)
        {
            Hour = hour;
            Minute = minute;
            Second = second;
        }
    }

    public static partial class ErrorCode
    {
        /// <summary>
        /// 缺少参数
        /// </summary>
        public const int PushLackOfParametersError = -150001;

        /// <summary>
        /// 数量已到上限，不能添加更多，需要去云控调整
        /// </summary>
        public const int PushExceedLimitError = -150002;

        /// <summary>
        /// ios:循环倒计时任务时间不能小于60s
        /// </summary>
        public const int PushIntervalTimeError = -151001;
        
        /// <summary>
        /// ios:类型错误（ios9 有些推送无法支持）
        /// </summary>
        public const int PushUnsupportedError = -151002;
        
        /// <summary>
        /// 未知错误
        /// </summary>
        public const int PushUnknownError = -159999;

        /// <summary>
        /// 字符串专程json失败，例如userInfo
        /// </summary>
        public const int PushJsonConvertError = -159901;
    }
}