/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using UnityEngine;

public class TimeUtil
{
    public const int TickToSecond = 10000000;
    public const int TickToMS = 10000;
    static public readonly DateTime TIME1970 = new DateTime(1970, 1, 1);

    public const int DaySecondCount = 86400;
    public const int HourSecondCount = 3600;
    public const int MinuteSecondCount = 60;

    static public TimeSpan m_TimeOffestToServer = new TimeSpan(0);
    static public TimeSpan m_DayOffest = new TimeSpan(0, 0, 0);

    // Sync time to a spefic time(server time).
    public static void SyncTickToServerTime(long serverTick)
    {
        m_TimeOffestToServer = TickToDateTime(serverTick) - System.DateTime.Now;
    }

    public static void SyncMsToServerTime(long serverMs)
    {
        m_TimeOffestToServer = TickToDateTime(serverMs * TickToMS) - System.DateTime.Now;
    }

    public static DateTime ServerNow
    {
        get { return DateTime.Now + m_TimeOffestToServer; }
    }

    /// <summary>
    /// 毫秒级时间戳
    /// </summary>
    public static long ServerNowMS
    {
        get { return DateTimeToMS(DateTime.Now + m_TimeOffestToServer); }
    }

    public static long Tick2MS(long tick)
    {
        return tick / (10 * 1000);
    }

    public static long MS2Tick(long MS)
    {
        return MS * (10 * 1000);
    }

    public static float MS2S(long MS)
    {
        return (float)MS / 1000.0f;
    }

    public static long S2MS(float S)
    {
        return (long)(S * 1000.0f);
    }

    public static DateTime TickToDateTime(long t)
    {
        return new DateTime(TIME1970.Ticks + t).ToLocalTime();
    }

    public static DateTime SecondToDateTime(long t)
    {
        return new DateTime(TIME1970.Ticks + (long)((double)t * TickToSecond)).ToLocalTime();
    }

    public static TimeSpan SecondToTimeSpan(long t)
    {
        return new TimeSpan((long)t * TickToSecond);
    }

    static public long DateTimeToTick(DateTime date)
    {
        return (date.ToUniversalTime().Ticks - TIME1970.Ticks);
    }

    static public long DateTimeToSecond(DateTime date)
    {
        return (long)((double)(date.ToUniversalTime().Ticks - TIME1970.Ticks) / TickToSecond);
    }

    static public long DateTimeToMS(DateTime date)
    {
        return (long)((double)(date.ToUniversalTime().Ticks - TIME1970.Ticks) / TickToMS);
    }


    static public bool IsSameDay(DateTime v1, DateTime v2)
    {
        return v1.Date.Equals(v2.Date);
    }

    static public DateTime ParseTimeStr(string str)
    {
        DateTime dt = DateTime.UtcNow;
        DateTime.TryParse(str, out dt);
        return dt;
    }
}