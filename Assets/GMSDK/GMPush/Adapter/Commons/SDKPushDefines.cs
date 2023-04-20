using System;
using UnityEngine;
using UNBridgeLib.LitJson;

namespace GMSDK
{
    public class PushMethodName
    {
        public const string Init = "registerPush";
        public const string LocalPushRemoveIdentifier = "localPushRemoveIdentifier";
        public const string LocalPushRemoveAllIdentifier = "localPushRemoveAllIdentifier";
        public const string LocalPushTimingRepeatMonth = "localPushTimingRepeatMonth";
        public const string LocalPushTimingRepeatWeek = "localPushTimingRepeatWeek";
        public const string LocalPushTimingRepeatDay = "localPushTimingRepeatDay";
        public const string LocalPushTimingOnce = "localPushTimingOnce";
        public const string LocalPushCountdownRepeat = "localPushCountdownRepeat";
        public const string LocalPushCountdownOnce = "localPushCountdownOnce";
    }

    public class LocalPushResult : CallbackResult
    {
        public String identifier;
    }

    public class LocalPushModel
    {
        public string title;
        public string subTitle;
        public string body;
        public long timeInterval;
        public int year;
        public int month;
        public int day;
        public int hour;
        public int minute;
        public int second;
        public int weekday;
        public int week;
        public string identifier;
        public JsonData userInfo;
    }
}