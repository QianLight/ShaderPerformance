#if UNITY_ANDROID

using UnityEngine;
using System;
using CFUtilPoolLib;

public class Android
{

    private static AndroidJavaClass _unityPlayerClass;

    public static AndroidJavaClass UnityPlayerClass
    {
        get
        {
            if (_unityPlayerClass == null)
            {
                _unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            }
            return _unityPlayerClass;
        }
    }

    private static AndroidJavaObject _currentActivity;

    public static AndroidJavaObject CurrentActivity
    {
        get
        {
            if (_currentActivity == null)
            {
                _currentActivity = UnityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
            }
            return _currentActivity;
        }
    }

    private static AndroidJavaObject _systeminfoActivity;

    public static AndroidJavaObject SysteminfoActivity
    {
        get
        {
            if (_systeminfoActivity == null)
            {
                AndroidJavaClass jc = new AndroidJavaClass("com.sdk.SystemInfo");
                _systeminfoActivity = jc.GetStatic<AndroidJavaObject>("uniqueInstance");
            }
            return _systeminfoActivity;
        }
    }

    private static AndroidJavaClass _nativeHelper;

    private static AndroidJavaClass NativeHelper
    {
        get
        {
            if (_nativeHelper == null)
            {
                _nativeHelper = new AndroidJavaClass("com.sdk.NativeHelper");
            }
            return _nativeHelper;
        }
    }


    private static AndroidJavaClass _clipboard;

    private static AndroidJavaClass ClipBoard
    {
        get
        {
            if (_clipboard == null)
            {
                _clipboard = new AndroidJavaClass("com.sdk.MClipboard");
            }
            return _clipboard;
        }
    }
    
    private static AndroidJavaClass _wpsdk;

    public static AndroidJavaClass WPSDK
    {
        get
        {
            if (_wpsdk == null)
            {
                _wpsdk = new AndroidJavaClass("com.sdk.MWPSDK");
            }
            return _wpsdk;
        }
    }

    public static float GetMemory()
    {
        float rest = 0;
        try
        {
            rest = NativeHelper.CallStatic<float>("GetMemory");
        }
        catch (Exception e)
        {
            XDebug.singleton.AddErrorLog(e.Message);
        }
        return rest;
    }

    public static int CopyStreamingAsset(string name, string dest = "")
    {
        int errorcode = 0;
        if (string.IsNullOrEmpty(dest))
        {
            dest = Application.persistentDataPath;
        }
        try
        {
            errorcode = NativeHelper.CallStatic<int>("CopyStreamingAsset", name, dest);
        }
        catch (Exception e)
        {
            XDebug.singleton.AddErrorLog(e.Message);
        }
        return errorcode;
    }

    public static byte[] LoadStream(string path)
    {
        byte[] rst = null;
        try
        {
            rst = NativeHelper.CallStatic<byte[]>("LoadStream", path);
        }
        catch (Exception e)
        {
            XDebug.singleton.AddErrorLog(e.Message);
        }
        return rst;
    }

    public static string LoadSteamString(string path)
    {
        string rst = string.Empty;
        try
        {
            rst = NativeHelper.CallStatic<string>("LoadSteamString", path);
        }
        catch (Exception e)
        {
            XDebug.singleton.AddErrorLog(e.Message);
        }
        return rst;
    }

    public static void ExtractInBackground(string version)
    {
        try
        {
            var dest = Application.persistentDataPath;
            NativeHelper.CallStatic("ExtractInBackground", dest, version);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public static void UnZip(string src, string dest = "", bool rewrite = true)
    {
        try
        {
            if (string.IsNullOrEmpty(dest))
            {
                dest = Application.persistentDataPath;
            }
            NativeHelper.CallStatic("UnZip", src, dest, rewrite);
        }
        catch (Exception e)
        {
            XDebug.singleton.AddErrorLog(e.Message);
        }
    }

    /*
     *  只有解压成功，才返回true
     *  解压失败或者解压中 都返回false
     */ 
    public static bool IsZiped(string asset)
    {
        bool v = false;
        try
        {
            int vl = NativeHelper.CallStatic<int>("ZipState", asset);
            v = vl == 1;
        }
        catch (Exception e)
        {
            XDebug.singleton.AddErrorLog(e.Message);
        }
        return v;
    }

    /*
     * asset:
     *  资源名 zip格式
     *  
     * state: 
     *  1   解压成功
     *  0   正在解压中
     *  -1  解压失败， 可能是磁盘溢出， 可能是没write权限
     *      可以通过 GetZipFailedDescription 获取失败原因
     */
    public static void IsZiped(string asset, out int state)
    {
        state = 0;
        try
        {
            state = NativeHelper.CallStatic<int>("ZipState", asset);
        }
        catch (Exception e)
        {
            XDebug.singleton.AddErrorLog(e.Message);
        }
    }

    public static string GetZipFailedDescription(string asset)
    {
        string error = "";
        try
        {
           error = NativeHelper.CallStatic<string>("GetFailDescription", asset);
        }
        catch (Exception e)
        {
            XDebug.singleton.AddErrorLog(e.Message);
        }
        return error;
    }


    public static void Copy2Clipboard(string content)
    {
        try
        {
             ClipBoard.CallStatic("Copy", content);
        }
        catch (Exception e)
        {
            XDebug.singleton.AddErrorLog(e.Message);
        }
    }


    public static string GetClipboard()
    {
        string rest = string.Empty;
        try
        {
            rest= ClipBoard.CallStatic<string>("Get");
        }
        catch (Exception e)
        {
            XDebug.singleton.AddErrorLog(e.Message);
        }
        return rest;
    }

    public static void ClearClipboard()
    {
        try
        {
            ClipBoard.CallStatic("Clear");
        }
        catch (Exception e)
        {
            XDebug.singleton.AddErrorLog(e.Message);
        }
    }

}


#endif