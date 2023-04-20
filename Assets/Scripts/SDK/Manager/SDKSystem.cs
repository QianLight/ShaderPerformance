using System;
using System.Collections;
using System.Collections.Generic;
using CFUtilPoolLib;
using UnityEngine;

public class SDKEnum
{
    public static readonly int BUGLY = 0;
    public static readonly int WebView = 1;
    public static readonly int WPSDK = 2;
    //must put the max at last
    public static readonly int MAX = 3;
}

public class SDKSystem : ISDKSystem
{

    private SDKComponent[] components;
    private bool initial = false;

    public bool Deprecated { get; set; }

    WPSDKComponent wpsdkComponent;
    public SDKSystem()
    {
        components = new SDKComponent[SDKEnum.MAX];

        components[SDKEnum.BUGLY] = new BuglyComponent();
        components[SDKEnum.WebView] = new WebViewComponent();

        wpsdkComponent = new WPSDKComponent();
        components[SDKEnum.WPSDK] = wpsdkComponent;
    }

    public void Start(Transform transform)
    {
        int max = components.Length;
        for (int i = 0; i < max; i++)
        {
            if (components[i].on)
            {
                components[i].transform = transform;
                components[i].Init();
            }
        }
        initial = true;
    }


    public void OnApplicationPause(bool pause)
    {
        if (initial)
        {
            int max = components.Length;
            for (int i = 0; i < max; i++)
            {
                if (components[i].on)
                {
                    components[i].OnPause(pause);
                }
            }
        }
    }


    public void OnApplicationQuit()
    {
        if (initial)
        {
            int max = components.Length;
            for (int i = 0; i < max; i++)
            {
                if (components[i].on)
                {
                    components[i].OnQuit();
                }
            }
        }
    }


    public void OnLogin(string account, string userid)
    {
        if (initial)
        {
            int max = components.Length;
            for (int i = 0; i < max; i++)
            {
                if (components[i].on)
                {
                    components[i].Login(account, userid);
                }
            }
        }
    }


    public void OnLogout()
    {
        if (initial)
        {
            int max = components.Length;
            for (int i = 0; i < max; i++)
            {
                if (components[i].on)
                {
                    components[i].Logout();
                }
            }
        }
    }

    public void LoadUrl(string url, string arg, Action<string> cb, int top = 0, int btm = 0, int left = 0, int right = 0)
    {
        if (initial)
        {
            (components[SDKEnum.WebView] as WebViewComponent).LoadUrl(url, arg, cb, top, btm, left, right);
        }
    }

    public void Close()
    {
        if (initial)
        {
            (components[SDKEnum.WebView] as WebViewComponent).Close();
        }
    }

    public bool NativeCallback(string msg)
    {
        var obj = MiniJSON.jsonDecode(msg);
        var table = obj as Hashtable;
        var code = (double)table["code"];
        Debug.Log("UnityRecv: " + code);
        switch (code)
        {
            case 102:
                XDebug.singleton.AddLog("do extract");
                break;
            case 103:
            case 104:
                return true;
            default:
                XDebug.singleton.AddErrorLog("native unknown code: " + code);
                break;
        }
        return false;
    }

    public void RegisterJSCallback(string name, Action<string> action)
    {
        if (initial)
        {
            (components[SDKEnum.WebView] as WebViewComponent).RegisterJSCallback(name, action);
        }        
    }

    public T Get<T>() where T : SDKComponent
    {
        int max = components.Length;
        for (int i = 0; i < max; i++)
        {
            if (components[i] is T)
            {
                return components[i] as T;
            }
        }
        return null;
    }


    public SDKComponent Get(int enu)
    {
        int max = components.Length;
        for (int i = 0; i < max; i++)
        {
            if (components[i].type == enu)
            {
                return components[i];
            }
        }
        return null;
    }

    public void BuglyReport(Exception e)
    {
        (components[SDKEnum.BUGLY] as BuglyComponent).ManuReport(e);
    }

    public void BuglyConfig(string chanel, string version, string user, long delay)
    {
        (components[SDKEnum.BUGLY] as BuglyComponent).Config(chanel, version, user, delay);
    }

    public void WPSDKUploadEvent(string eventName, Dictionary<string, string> param)
    {
        wpsdkComponent.UploadEvent(eventName, param);
    }
}
