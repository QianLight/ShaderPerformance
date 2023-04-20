using System;
using GSDK;

/**
 * 双端共用一个参数输入框，同时双端参数值不相同情况下的处理工具类
 */
public class ConfigSettingParamHandleUtils
{
    public static char Delimiter = '|';

    public const string ANDROID_OPERATING_TYPE = "android";
    public const string IOS_OPERATING_TYPE = "iOS";

    public static string GetAndroidStringConfigValue(string value)
    {
        return GetStringConfigValue(value, ANDROID_OPERATING_TYPE);
    }

    public static string GetIOSStringConfigValue(string value)
    {
        return GetStringConfigValue(value, IOS_OPERATING_TYPE);
    }

    public static long GetAndroidLongConfigValue(string value)
    {
        return GetLongConfigValue(value, ANDROID_OPERATING_TYPE);
    }

    public static long GetIOSLongConfigValue(string value)
    {
        return GetLongConfigValue(value, IOS_OPERATING_TYPE);
    }

    public static int GetAndroidIntConfigValue(string value)
    {
        return GetIntConfigValue(value, ANDROID_OPERATING_TYPE);
    }

    public static int GetIOSIntConfigValue(string value)
    {
        return GetIntConfigValue(value, IOS_OPERATING_TYPE);
    }

    private static string GetStringConfigValue(string value, string operatType)
    {
        if (!string.IsNullOrEmpty(value) && value.Contains(Delimiter.ToString()))
        {
            string[] result = value.Split(Delimiter);
            if (result != null && result.Length >= 2)
            {
                return ANDROID_OPERATING_TYPE == operatType ? result[0] : result[1];
            }
        }

        return value;
    }


    private static long GetLongConfigValue(string value, string operatType)
    {
        string result = value;
        if (!string.IsNullOrEmpty(result) && result.Contains(Delimiter.ToString()))
        {
            string[] results = result.Split(Delimiter);
            if (result != null && results.Length >= 2)
            {
                result = ANDROID_OPERATING_TYPE == operatType ? results[0] : results[1];
                
                if (results[1].Length != 0)
                {
                    try
                    { 
                        long.Parse(results[1]);
                    }
                    catch (Exception exception)
                    {
                        GLog.LogError("参数不合法：" + value);
                    }
                }
            }
        }

        if (result.Length == 0)
        {
            return 0;
        }
        
        try
        {
            return long.Parse(result);
        }
        catch (Exception exception)
        {
            GLog.LogError("参数不合法：" + value);
        }

        return 0;
    }

    private static int GetIntConfigValue(string value, string operatType)
    {
        if (string.IsNullOrEmpty(value))
        {
            value = "0";
        }

        string result = value;
        if (!string.IsNullOrEmpty(result) && result.Contains(Delimiter.ToString()))
        {
            string[] results = result.Split(Delimiter);
            if (result != null && results.Length >= 2)
            {
                result = ANDROID_OPERATING_TYPE == operatType ? results[0] : results[1];
                
                // try catch为了检测iOS参数的合法性
                if (results[1].Length != 0)
                {
                    try
                    { 
                        int.Parse(results[1]);
                    }
                    catch (Exception exception)
                    {
                        GLog.LogError("参数不合法：" + value);
                    }
                }
            }
        }

        if (result.Length == 0)
        {
            return 0;
        }

        try
        {
            return int.Parse(result);
        }
        catch (Exception exception)
        {
            GLog.LogError("参数不合法：" + value);
        }

        return 0;
    }
}