using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using UnityEngine;

using CFUtilPoolLib;

public class XNativeInfo : INativeInfo
{
    public bool Deprecated { get; set; }


#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern int GetDensity();

    [DllImport("__Internal")]
    private static extern string CheckSIM();

    [DllImport("__Internal")]
    private static extern string getIPv6(string mHost, string mPort);

    [DllImport("__Internal")]
    private static extern void _clearClipboard();
    [DllImport("__Internal")]
    private static extern void _copyTextToClipboard(string text);
    [DllImport("__Internal")]
    private static extern string _getClipboard();
    [DllImport("__Internal")]
    static extern void closeapplication (string str1, string str2);
#endif


    public void ClearClipboard()
    {
#if UNITY_EDITOR
        Debug.LogWarning("not implement is editor pltform");
#elif UNITY_IOS
        _clearClipboard();
#elif UNITY_ANDROID
       Android.ClearClipboard();
#endif
    }
    public void CopyTextToClipboard(string text)
    {
#if UNITY_EDITOR
        //Debug.LogWarning("not implement is editor pltform");
        TextEditor te = new TextEditor();
        te.text = text;
        te.SelectAll();
        te.Copy();
#elif UNITY_IOS
        _copyTextToClipboard(text);
#elif UNITY_ANDROID
        Android.Copy2Clipboard(text);
#endif
    }
    public string GetClipboard()
    {
#if UNITY_EDITOR
        Debug.LogWarning("not implement is editor pltform");
#elif UNITY_IOS
        return _getClipboard();
#elif UNITY_ANDROID
        return Android.GetClipboard();
#endif
        return "";
    }


    private static string GetIPv6(string mHost, string mPort)
    {
#if UNITY_IOS && !UNITY_EDITOR
    var mIPv6 = getIPv6(mHost, mPort);
    return mIPv6;
#else
        return mHost + "&&ipv4";
#endif
    }

    public int GetNativeDensity()
    {
#if UNITY_EDITOR
        return 200;

#elif UNITY_ANDROID
        int density = 200;
        try
        {
            density = Android.SysteminfoActivity.Call<int>("GetDensity");
        }
        catch (Exception e) { XDebug.singleton.AddErrorLog("err: "+e.StackTrace); }
        XDebug.singleton.AddLog("android density is: "+density);
        return density;
#elif UNITY_IOS
        int density = GetDensity();
        XDebug.singleton.AddLog("ios density is: "+density);
        return density;
#else
        return 200;
#endif
    }
    public string GetNativeSim()
    {
#if UNITY_EDITOR
        return "";
#elif UNITY_ANDROID
        string str = "";
        try
        {
            str = Android.SysteminfoActivity.Call<string>("CheckSIM");
        }
        catch (Exception e) { XDebug.singleton.AddErrorLog("err: ",e.StackTrace); }
        XDebug.singleton.AddLog("androidCheckSIM: "+str);
        return str;
#elif UNITY_IOS
        string str = CheckSIM();
        XDebug.singleton.AddLog("ios CheckSIM: "+str);
        return str;
#else
        return "";
#endif
    }

    public void GetNativeIpInfo(string serverIp, string serverPorts, out string newServerIp, out AddressFamily addressFamily)
    {
        addressFamily = AddressFamily.InterNetwork;
        newServerIp = serverIp;
        try
        {
            var mIPv6 = GetIPv6(serverIp, serverPorts);
            if (!string.IsNullOrEmpty(mIPv6))
            {
                var m_StrTemp = System.Text.RegularExpressions.Regex.Split(mIPv6, "&&");
                if (m_StrTemp != null && m_StrTemp.Length >= 2)
                {
                    var IPType = m_StrTemp[1];
                    if (IPType == "ipv6")
                    {
                        newServerIp = m_StrTemp[0];
                        addressFamily = AddressFamily.InterNetworkV6;
                    }
                }
            }
        }
        catch (Exception e) { XDebug.singleton.AddErrorLog("err: ", e.StackTrace); }
    }

    public int CheckPermission(string permission)
    {
#if UNITY_ANDROID
        int state = -1;
        try
        {
            state = Android.SysteminfoActivity.Call<int>("CheckPermission", permission);
            return state;
        }
        catch (Exception e)
        {
            XDebug.singleton.AddErrorLog("err: ", e.StackTrace);
            return state;
        }
#else //UNITY_IOS这个在src-UnityGetiOSPermissionsState处理了
        return 0;
#endif
    }

    public void OpenPermissionSettings(string permission, PermissionRequestCallback obj = null)
    {
#if UNITY_ANDROID
        try
        {
            Android.SysteminfoActivity.Call("OpenPermissionSettings", permission, obj);
        }
        catch (Exception e)
        {
            XDebug.singleton.AddErrorLog("err: ", e.StackTrace);
        }
#else //UNITY_IOS这个在src-UnityGetiOSPermissionsState处理了

#endif
    }
    public void CloseApplication()
    {
#if UNITY_IOS
       Debug.Log("CloseApplication IOS");
       closeapplication("", "");
#elif UNITY_ANDROID
        Debug.Log("Android.SysteminfoActivity.Call KillProcess");
        Debug.Log("Application.Quit");
        Android.SysteminfoActivity.Call("KillProcess");

#else
        Debug.Log("Application.Quit");
        Application.Quit();
#endif
    }

    public bool IsNotificationEnabled()
    {
#if UNITY_ANDROID
        try
        {
            bool flag = Android.SysteminfoActivity.Call<bool>("IsNotificationEnabled");
            XDebug.singleton.AddGreenLog("IsNotificationEnabled=" + flag);
            return flag;
        }
        catch (Exception e)
        {
            XDebug.singleton.AddErrorLog("IsNotificationEnabled error:" + e.ToString());
            return true;
        }
#else
        return true;  // to do ...
#endif
    }

    public void GotoNotificationSetting()
    {
#if UNITY_ANDROID
        try
        {
            Android.SysteminfoActivity.Call("GotoNotificationSetting");
        }
        catch (Exception e)
        {
            XDebug.singleton.AddErrorLog("GotoNotificationSetting error:" + e.ToString());
        }
#else
         // to do ...
#endif
    }
}