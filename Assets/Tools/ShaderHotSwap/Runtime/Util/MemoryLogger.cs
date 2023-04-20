using System.Text;
using UnityEngine;

namespace UsingTheirs.ShaderHotSwap
{
    public static class MemoryLogger
    {
        static private StringBuilder sb = new StringBuilder();

        public static string Flush()
        {
            var str = sb.ToString();
            sb.Length = 0;
            return str;
        }

        private static void LogImpl(LogType type, string msg)
        {
            sb.AppendFormat("[{0}] {1}\n", type, msg);
        }
        
        public static void Log(string format, params object[] args)
        {
            var msg = string.Format(format, args);
            LogImpl(LogType.Log, msg);
        }

        public static void LogWarning(string format, params object[] args)
        {
            var msg = string.Format(format, args);
            LogImpl(LogType.Warning, msg);
        }
        public static void LogError(string format, params object[] args)
        {
            var msg = string.Format(format, args);
            LogImpl(LogType.Error, msg);
        }
    }

}