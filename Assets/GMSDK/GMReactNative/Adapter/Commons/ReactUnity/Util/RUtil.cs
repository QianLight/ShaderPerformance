using UnityEngine;

namespace GSDK.RNU
{
    public partial class Util {
        private static string RNULog = "ReactUnity: ";
        private static string GECKO_KEY = "783b79a961a58fea81c271851de9f948";


        #if UNITY_EDITOR
            public static bool logFlag = true;
        #else
            public static bool logFlag = false;
        #endif


        public static void Log(string format, params object[] args) {
            if (logFlag)
            {
                Debug.LogFormat(RNULog + format, args);
            }
        }


        public static void Log(string log) {
            if (logFlag)
            {
                Debug.Log(RNULog + log);
            }
        }


        public static void LogAndReport(string log)
        {
            Log(log);
            InfoAndErrorReporter.RuLog(log);
        }


        public static void LogAndReport(string format, params object[] args)
        {
            Log(format, args);
            InfoAndErrorReporter.RuLog(string.Format(format, args));
        }


        public static void LogError(string format, params object[] args) {
            if (logFlag)
            {
                Debug.LogErrorFormat(RNULog + format, args);
            }
        }


        public static void LogError(string log) {
            if (logFlag)
            {
                Debug.LogError(RNULog + log);
            }
        }


    }
}
