using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;

namespace UNBridgeLib
{
    public delegate void UnBridgeLogDelegate(object msg);

    /// <summary>
    /// 日志辅助类
    /// </summary>
    public class LogUtils
    {
        public static readonly int LV = 1;
        public static readonly int LD = 2;
        public static readonly int LW = 3;
        public static readonly int LE = 4;

        public static bool DEBUG = false;
        public static int LogLevel = LD;
        
        private static string prefix = "[UNBridge]:";
        public static UnBridgeLogDelegate LogDelegate;

        private static Dictionary<int, StringBuilder> _dictionary = new Dictionary<int, StringBuilder>();

        public static void V(string msg)
        {
            if (DEBUG && LogLevel <= LV)
            {
                if (LogDelegate != null)
                {
                    LogDelegate.Invoke(prefix + msg);
                }

                Debug.Log(prefix + msg);
            }
        }

        public static void D(string msg)
        {
            if (DEBUG && LogLevel <= LD)
            {
                if (LogDelegate != null)
                {
                    LogDelegate.Invoke(prefix + msg);
                }

                Debug.Log(prefix + msg);
            }
        }
        
        public static void D(string definePrefix, string msg)
        {
            if (DEBUG && LogLevel <= LD)
            {
                var threadID = Thread.CurrentThread.ManagedThreadId;
                if (!_dictionary.ContainsKey(threadID))
                {
                    _dictionary[threadID] = new StringBuilder();
                }
                var sb = _dictionary[threadID];
                sb.Append(definePrefix).Append(msg);
                D(sb.ToString());
                sb.Remove(0, sb.Length);
            }
        }

        public static void W(string msg)
        {
            if (LogLevel <= LW)
            {
                if (LogDelegate != null)
                {
                    LogDelegate.Invoke(prefix + msg);
                }

                Debug.LogWarning(prefix + msg);
            }
        }


        public static void E(string msg)
        {
            if (LogLevel <= LE)
            {
                if (LogDelegate != null)
                {
                    LogDelegate.Invoke(prefix + msg);
                }

                Debug.LogError(prefix + msg);
            }
        }
    }
}