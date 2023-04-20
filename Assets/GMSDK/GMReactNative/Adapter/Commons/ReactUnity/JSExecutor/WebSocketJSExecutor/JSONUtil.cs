/*
 * @author yankang.nj
 * JSON 工具类。 只能用于调试模式 被WebSocketJSExecutor使用。 
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GSDK.RNU
{
    public static class JSONUtil
    {
        //TODO 避免使用魔法数字
        private static int JSONObjKey = -1000086;
        
        // 初始化环境
        public static void Init()
        {
            RNUNative.InitRuntime();
        }

        public static void Destroy()
        {
            RNUNative.FreeRuntime();
        }
        
        public static ArrayList ParseList(string jsonStr)
        {

            RNUNative.ParseJSON(JSONObjKey, jsonStr);
            return UnmanageJSDataOp.GetCSharpArrayList(JSONObjKey);
        }

        public static Dictionary<string, object> ParseMap(string jsonStr)
        {
            RNUNative.ParseJSON(JSONObjKey, jsonStr);
            return UnmanageJSDataOp.GetCSharpMap(JSONObjKey);
        }

        public static string StringifyList(ArrayList list)
        {

            IntPtr p = UnmanageJSDataOp.GeneJSArray(list);
            IntPtr strP = RNUNative.StringifyJSON(p);
            return Marshal.PtrToStringAuto(strP);
        }

        public static string StringifyMap(Hashtable map)
        {
            IntPtr p = UnmanageJSDataOp.GeneJSObject(map);
            IntPtr strP = RNUNative.StringifyJSON(p);
            return Marshal.PtrToStringAuto(strP);
        }
        
        public static string StringifyMap(Dictionary<string, object> map)
        {
            IntPtr p = UnmanageJSDataOp.GeneJSObject(map);
            IntPtr strP = RNUNative.StringifyJSON(p);
            return Marshal.PtrToStringAuto(strP);
        }
        
    }
}