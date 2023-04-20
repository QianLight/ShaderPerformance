/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class CpuInfoUtil
{
    public static List<string> GetCpuInfo()
    {
        List<string> result = new List<string>();
#if !UNITY_EDITOR
#if UNITY_ANDROID
        string str = null;
        str = GetBuildBoard();
        if (!string.IsNullOrEmpty(str))
        {
            result.Add(str);
        }
        str = GetBuildHardware();
        if (!string.IsNullOrEmpty(str))
        {
            result.Add(str);
        }
        str = GetProductBoard();
        if (!string.IsNullOrEmpty(str))
        {
            result.Add(str);
        }
        str = GetFromCpuInfo();
        if (!string.IsNullOrEmpty(str))
        {
            result.Add(str);
        }
#endif
#endif
        return result;
    }

    /// <summary>
    /// android/os/Build.BOARD
    /// </summary>
    private static string GetBuildBoard()
    {
        using (var jc = new AndroidJavaClass("android/os/Build"))
        {
            return jc.GetStatic<string>("BOARD");
        }
    }

    /// <summary>
    /// android/os/Build.HARDWARE
    /// </summary>
    private static string GetBuildHardware()
    {
        using (var jc = new AndroidJavaClass("android/os/Build"))
        {
            return jc.GetStatic<string>("HARDWARE");
        }
    }

    /// <summary>
    /// system/build.prop[ro.product.board]
    /// </summary>
    private static string GetProductBoard()
    {
        string result = null;
        AndroidJavaObject fileReader = null;
        AndroidJavaObject bufferedReader = null;
        try
        {
            fileReader = new AndroidJavaObject("java/io/FileReader", "system/build.prop");
            bufferedReader = new AndroidJavaObject("java/io/BufferedReader", fileReader, 8192);
            string line;
            while ((line = bufferedReader.Call<string>("readLine")) != null)
            {
                if (line.StartsWith("ro.product.board="))
                {
                    result = line.Substring("ro.product.board=".Length);
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        finally
        {
            if (bufferedReader != null)
            {
                bufferedReader.Call("close");
                bufferedReader.Dispose();
            }
            if (fileReader != null)
            {
                fileReader.Dispose();
            }
        }
        return result;
    }

    /// <summary>
    /// system/build.prop[ro.product.board]
    /// </summary>
    private static string GetFromCpuInfo()
    {
        string result = null;
        AndroidJavaObject fileReader = null;
        AndroidJavaObject bufferedReader = null;
        try
        {
            fileReader = new AndroidJavaObject("java.io.FileReader", "/proc/cpuinfo");
            bufferedReader = new AndroidJavaObject("java.io.BufferedReader", fileReader, 8192);
            string info = null;
            while ((info = bufferedReader.Call<string>("readLine")) != null)
            {
                if (info.Contains("Hardware"))
                {
                    result = info.Replace(" ", "").Replace("Hardware", "").Replace(":", "").Trim();
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        finally
        {
            if (bufferedReader != null)
            {
                bufferedReader.Call("close");
                bufferedReader.Dispose();
            }
            if (fileReader != null)
            {
                fileReader.Dispose();
            }
        }
        return result;
    }
}
