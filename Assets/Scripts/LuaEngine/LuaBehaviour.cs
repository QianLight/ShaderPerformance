using System.Diagnostics.SymbolStore;
using System.Data.Common;
using System.Timers;
using CFUtilPoolLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using XLua;

public class LuaBehaviour
{
    private LuaEnv luaEnv;
    private string luaPath, tablePath, patchPath;
    private LuaTable mainEnv;
    private LuaWrap wrap;
    private LuaRoute route;

#if UNITY_ANDROID
    private bool needRezip;
#endif

    private Action<float, float> luaUpadte;
    private Action luaOnDestroy, luaEnterScene, luaLeaveScene;
    private Action luaOnReconnect, luaApplicationQuit;
    private ApplicationPauseDelegate luaApplicationPause;

#if UNITY_EDITOR
#pragma warning disable 0414
    // added by wsh @ 2017-12-29
    [SerializeField]
    long updateElapsedMilliseconds = 0;
    [SerializeField]
    long lateUpdateElapsedMilliseconds = 0;
    [SerializeField]
    long fixedUpdateElapsedMilliseconds = 0;
#pragma warning restore 0414
    Stopwatch sw = new Stopwatch();
#endif

    [CSharpCallLua]
    public delegate int ApplicationPauseDelegate(bool pause);

    enum LaunchStep
    {
        Initial,
        AttachUtility,
        Complete,
    }

    private static ILuaUtility iLuaUtility;
    private LaunchStep launchStep;

    internal const string SEP = "";

    public static ILuaUtility LuaUtility
    {
        get
        {
            if (iLuaUtility == null)
            {
                var hash = XCommon.singleton.XHash("ILuaUtility");
                iLuaUtility = XInterfaceMgr.singleton.GetInterface<ILuaUtility>(hash);
            }

            return iLuaUtility;
        }
    }

//#if UNITY_EDITOR
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void LogDelegate(IntPtr message, int iSize);
    [DllImport("xlua", CallingConvention = CallingConvention.Cdecl)]
    public static extern void InitCSharpDelegate(LogDelegate log);//C# Function for C++‘s call[MonoPInvokeCallback(typeof(LogDelegate))]
    public static void LogMessageFromCpp(IntPtr message, int iSize)
    {
        XDebug.singleton.AddErrorLog(Marshal.PtrToStringAnsi(message, iSize));
    }
    public static void ShowLog()
    {
        InitCSharpDelegate(LogMessageFromCpp);
    }
//#endif


    public void DoFile(string lua, LuaTable env = null)
    {
#if !UNITY_EDITOR
        var bytes = Zeus.Core.FileSystem.VFileSystem.ReadAllBytes(string.Format("lua/{0}.lua.txt", lua)); ;
#else
        var bytes = Zeus.Core.FileSystem.VFileSystem.ReadAllBytes(string.Format("StreamingAssets/lua/{0}.lua.txt", lua)); ;
#endif
        luaEnv.DoString(bytes, lua, env);
    }
    private byte[] CustomLoader(ref string filename)
    {
#if EMMYLUA_DEBUG
        if (filename.Contains("emmy_core"))
            return null;
#endif
		
#if !UNITY_EDITOR
        string path = string.Format("lua/{0}.lua.txt", filename);
#else
        string path = string.Format("StreamingAssets/lua/{0}.lua.txt", filename);
#endif
        XDebug.singleton.AddLog(path);
        //var text = Zeus.Core.FileSystem.VFileSystem.ReadAllText(path, System.Text.Encoding.UTF8);
        //return System.Text.Encoding.UTF8.GetBytes(text);
        return Zeus.Core.FileSystem.VFileSystem.ReadAllBytes(path);
    }

    public void Start()
    {

        luaEnv = new LuaEnv();
        luaEnv.AddLoader(CustomLoader);

#if LUA_MEMORY_CHECKER
        data = luaEnv.StartMemoryLeakCheck();
        XDebug.singleton.AddLog("Start, PotentialLeakCount:" + data.ToString());
#endif

#if UNITY_EDITOR

        luaPath = Application.streamingAssetsPath + "/lua/";
        tablePath = Application.dataPath + "/BundleRes/Table/";

#if EMMYLUA_DEBUG
        try
        {
            string luaLibPath = Application.streamingAssetsPath + "/LuaProject/lib/";
            string finalPath = string.Format(@"package.cpath = package.cpath .. ';{0}?.dll'", luaLibPath);
            luaEnv.DoString(finalPath);
            string debugLink = @"
            local dbg = require('emmy_core')
            dbg.tcpListen('localhost', 9964)";
            luaEnv.DoString(debugLink);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log("emmylua 调试连接失败");
        }
#endif

#else
        luaPath = Application.persistentDataPath + "/OuterPackage/lua/";
        tablePath = Application.persistentDataPath + "/OuterPackage/bundleres/table/";
#endif
        patchPath = Application.persistentDataPath + "/OuterPackage/";

        mainEnv = luaEnv.NewTable();
        launchStep = LaunchStep.Initial;

    }

#if LUA_MEMORY_CHECKER
    LuaMemoryLeakChecker.Data data = null;
    int tick = 0;
#endif
    private void LaunchLua()
    {
#if UNITY_EDITOR
        ShowLog();
#endif
        launchStep = LaunchStep.Complete;
        LoadLuaTable();
        DoFile("main");
#if UNITY_EDITOR
        DoFile("debug");
#endif
        LuaFunction mainf = luaEnv.Global.Get<LuaFunction>("main");
        object[] ret = mainf.Call();
        LuaTable hotfixContainer = ret[0] as LuaTable;
        var keys = hotfixContainer.GetKeys<uint>();
        Dictionary<uint, uint> dic = new Dictionary<uint, uint>();
        foreach (var key in keys)
        {
            //    uint hash = XCommon.singleton.XHash(key.ToString());
            hotfixContainer.Get<object, uint>(key, out var v);
            dic.Add(key, v);
        }

        luaUpadte = luaEnv.Global.Get<Action<float, float>>("update");
        luaOnDestroy = luaEnv.Global.Get<Action>("onDestroy");
        luaEnterScene = luaEnv.Global.Get<Action>("onEnterScene");
        luaLeaveScene = luaEnv.Global.Get<Action>("onLeaveScene");
        luaOnReconnect = luaEnv.Global.Get<Action>("onReconnect");
        luaApplicationQuit = luaEnv.Global.Get<Action>("onApplicationQuit");
        luaApplicationPause = luaEnv.Global.Get<ApplicationPauseDelegate>("onApplicationPause");

        LuaTable hotfixViewMgr = ret[1] as LuaTable;
        wrap = new LuaWrap(hotfixViewMgr, luaEnterScene, luaLeaveScene, luaOnReconnect);
        XInterfaceMgr.singleton.AttachInterface<LuaWrap>(XInterfaceMgr.LuaWrap, wrap);

        LuaTable doc = ret[2] as LuaTable;
        LuaTable luaRegistList = ret[3] as LuaTable;
        route = new LuaRoute(this, doc, luaRegistList);
        XInterfaceMgr.singleton.AttachInterface<LuaRoute>(XInterfaceMgr.LuaRoute, route);
        LuaUtility.FillRegisted(dic);
    }

    /*
     * 通过扩展Native接口, 直接在虚拟机上把数据刷到G表
     */
    private void LoadLuaTable()
    {
#if UNITY_EDITOR
        XDebug.singleton.AddLog("LUA Table: " + luaPath + " " + tablePath);
#endif
        int flag = luaEnv.LoadLuaTable(luaPath, tablePath, patchPath);
        XDebug.singleton.AddBlueLog("read tab flag :" + flag);
        // DoFile("table/table");
    }

    public void Update()
    {
#if LUA_MEMORY_CHECKER
        tick++;
        if (tick % 90 == 0)
        {
            data = luaEnv.MemoryLeakCheck(data);
            XDebug.singleton.AddLog("lua, Memory:" + data.ToString());
        }
        else if (tick % 180 == 0)
        {
            XDebug.singleton.AddLog(luaEnv.MemoryLeakReport(data));
        }
        if (tick > 180000) tick = 0;
#endif
        switch (launchStep)
        {
            case LaunchStep.Initial:
                launchStep = LaunchStep.AttachUtility;
                break;
            case LaunchStep.AttachUtility:
                if (LuaUtility != null)
                {
                    LaunchLua();
                    launchStep = LaunchStep.Complete;
                }

                break;
            case LaunchStep.Complete:
#if UNITY_EDITOR
                var start = sw.ElapsedMilliseconds;
#endif
                try
                {
                    if (luaUpadte != null) luaUpadte(Time.deltaTime, Time.unscaledDeltaTime);
                    if (route != null) route.Update();
                }
                catch (Exception ex)
                {
                    XDebug.singleton.AddErrorLog("lua update err!" + ex.Message);
                }
                break;
#if UNITY_EDITOR
                updateElapsedMilliseconds = sw.ElapsedMilliseconds - start;
#endif
            default:
                XDebug.singleton.AddErrorLog("lua unknown step");
                break;
        }
    }

    public void OnDestroy()
    {
        if (launchStep == LaunchStep.Complete)
        {
            luaOnDestroy?.Invoke();
            // luaEnv?.Dispose();
        }
    }

    public void OnApplicationPause(bool pause)
    {
        if (launchStep == LaunchStep.Complete)
        {
            luaApplicationPause?.Invoke(pause);
        }
    }

    public void OnApplicationQuit()
    {
        if (launchStep == LaunchStep.Complete)
        {
            luaApplicationQuit?.Invoke();
        }
    }
}
