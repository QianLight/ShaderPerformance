using System;
using System.IO;
using Google.Protobuf;
using UnityEngine;
using XLua;
using System.Collections.Generic;
using System.Text;
using CFUtilPoolLib;

public class LuaRoute : ILuaRoute
{
    private LuaBehaviour luaEnv;
    private LuaTable mRegistTable;

    private LuaFunction processHander, overrideHander;
    private LuaFunction testHander;
    private List<uint> overide, ptc, ptccb;
    private bool launched = false;

    // Don't Edit, define same in LuaNotifyProcess.lua
    internal const string strProcess = "Process";
    internal const string strOverride = "ProcessOveride";
    internal const string strRgisted = "FetchRegistedID";
    internal const string strTest = "LuaTest";

    private static ILuaNetwork iLuaNetwork;
    
    public bool Deprecated { get; set; }

    public static ILuaNetwork LuaNetwork
    {
        get
        {
            if (iLuaNetwork == null)
            {
                var hash = XCommon.singleton.XHash("ILUANET");
                iLuaNetwork = XInterfaceMgr.singleton.GetInterface<ILuaNetwork>(hash);
            }

            return iLuaNetwork;
        }
    }

    public LuaRoute(LuaBehaviour beh, LuaTable doc, LuaTable registList)
    {
        launched = false;
        luaEnv = beh;
        mRegistTable = registList;
        processHander = doc.GetInPath<LuaFunction>(strProcess);
        overrideHander = doc.GetInPath<LuaFunction>(strOverride);
        // testHander = doc.GetInPath<LuaFunction>(strTest);

        FetchRegistID(out overide, out ptc, out ptccb);
    }

    public void Update()
    {
        if (!launched && LuaNetwork != null)
        {
            LuaNetwork?.LuaRegistDispacher(ptc, ptccb, overide);
            ptc.Clear();
            overide.Clear();
            ptccb.Clear();
            launched = true;
        }
    }


    /// <summary>
    /// ptc 处理
    /// type: 协议ID
    /// bytes: 协议字段
    /// </summary>
    public void NotifyWithBuffer(uint type, byte[] buffer)
    {
        Debug.Log("NotifyWithBuffer:"+ type);
        processHander?.Call(LuaBehaviour.SEP, type, buffer);
    }

    public void NotifyOnly(uint type)
    {
        processHander?.Call(LuaBehaviour.SEP, type, "");
    }

    /// <summary>
    /// 重载 c#协议
    /// </summary>
    public void OverideNet(uint type, byte[] buffer)
    {
        overrideHander?.Call(LuaBehaviour.SEP, type, buffer);
    }


    /// <summary>
    /// 抓取lua初始化的协议
    /// </summary>
    public void FetchRegistID(out List<uint> overide, out List<uint> ptc, out List<uint> ptcCB)
    {
        LuaTable table = mRegistTable;
        int len = table.Length;
        overide = new List<uint>();
        ptc = new List<uint>();
        ptcCB = new List<uint>();
        for (int i = 1; i < len; i += 2)
        {
            table.Get<int, uint>(i, out var type);
            table.Get<int, uint>(i + 1, out var sign);
            if (sign == 1)
            {
                ptc.Add(type);
            }
            else if (sign == 0)
            {
                ptcCB.Add(type);
            }
            else if (sign == 2)
            {
                overide.Add(type);
            }
            else
            {
                XDebug.singleton.AddErrorLog("unknown sign: ", sign.ToString());
            }
        }
    }

    public static void PrintBytes(string tag, byte[] bytes)
    {
#if UNITY_EDITOR
            StringBuilder sb = new StringBuilder(tag.ToUpper());
            sb.Append(" length:");
            sb.Append(bytes.Length.ToString());
            sb.Append(" =>");
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i]);
                sb.Append(" ");
            }
            XDebug.singleton.AddLog(sb.ToString());
#endif
    }

    public void Test()
    {
#if UNITY_EDITOR
        MemoryStream ms = new MemoryStream();
        KKSG.ClientLoginArg arg = new KKSG.ClientLoginArg();
        arg.Account = "av123";
        arg.Password = "12121x";
        arg.Serverid = 1232;
        arg.Token = "heloworldhello7&net";

        var objs = testHander?.Call("", arg.ToByteArray());
        MemoryStream stream = objs[1] as MemoryStream;
        long len = (long) objs[2];
        stream.Position = 0;
        byte[] bytes = new byte[len];
        stream.Read(bytes, 0, (int) len);
        arg= KKSG.ClientLoginArg.Parser.ParseFrom(bytes);
        Debug.Log(arg.Account + " " + arg.Password);
#endif
    }

    
}