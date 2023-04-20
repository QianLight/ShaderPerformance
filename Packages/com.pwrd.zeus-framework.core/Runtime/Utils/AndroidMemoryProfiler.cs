/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/

using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Zeus.Core
{
    public class AndroidMemoryProfiler
    {
        private static string filePath = $"{Application.persistentDataPath}/MemoryDump_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv";
        private static MemoryInfo last;
        private static Queue<MemoryInfo> _InfoQueue = new Queue<MemoryInfo>();
        private static bool enable = false;

        private static int _SDK_INT;
        private static int SDK_INT
        {
            get
            {
                if (_SDK_INT == 0)
                {
                    using (var javaClass = new AndroidJavaClass("android.os.Build$VERSION"))
                    {
                        _SDK_INT = javaClass.GetStatic<int>("SDK_INT");
                    }
                }
                return _SDK_INT;
            }
        }

        private static AndroidJavaClass _Debug;
        private static AndroidJavaClass Debug
        {
            get
            {
                if (_Debug == null)
                    _Debug = new AndroidJavaClass("android.os.Debug");
                return _Debug;
            }
        }

        private static AndroidJavaObject getMemoryInfoJO()
        {
            var memInfo = new AndroidJavaObject("android.os.Debug$MemoryInfo");
            Debug.CallStatic("getMemoryInfo", memInfo);
            return memInfo;
        }

       
        private static MemoryInfo getMemoryInfo()
        {
            var memInfo = new MemoryInfo();
            int count = statNames.Length;
            memInfo.data = new int[count];
            memInfo.delta = new int[count];
            if (SDK_INT >= 23)
            {
                using (var jo = getMemoryInfoJO())
                {
                    for (int index = 0; index < count; index++)
                    {
                        var value = jo.Call<string>("getMemoryStat", statNames[index]);
                        int pss;
                        int.TryParse(value, out pss);
                        memInfo.data[index] = pss;
                        if (last.data != null)
                        {
                            memInfo.delta[index] = pss - last.data[index];
                        }
                    }
                }
            }
            last = memInfo;
            return memInfo;
        }

        [System.Diagnostics.Conditional("UNITY_ANDROID")]
        public static void SetEnable(bool value)
        {
            enable = value;
        }

        [System.Diagnostics.Conditional("UNITY_ANDROID")]
        public static void Sample(string name)
        {
            if (!enable) return;

            var memInfo = getMemoryInfo();
            memInfo.name = name;
            _InfoQueue.Enqueue(memInfo);

            if (_InfoQueue.Count == int.MaxValue)
            {
                WriteLogFile();
            }
        }

        [System.Diagnostics.Conditional("UNITY_ANDROID")]
        public static void WriteLogFile()
        {
            using (TextWriter writer = new StreamWriter(filePath, true))
            {
                //  Header
                writer.Write("name");
                foreach (var stat in statNames)
                {
                    writer.Write(',');
                    writer.Write(stat);
                    writer.Write(" [KB]");
                }
                writer.WriteLine();
                //  Content
                while (_InfoQueue.Count > 0)
                {
                    var memInfo = _InfoQueue.Dequeue();
                    writer.Write(memInfo.name);
                    for (int i = 0; i < memInfo.data.Length; i++)
                    {
                        writer.Write(',');
                        writer.Write(memInfo.data[i]);
                        writer.Write('(');
                        writer.Write(memInfo.delta[i]);
                        writer.Write(')');
                    }
                    writer.WriteLine();
                }
            }
        }

        private static string[] statNames = new string[]
        {
            "summary.native-heap",      // native-heap 
            "summary.private-other",    // unknown
            "summary.graphics",         // dev gfx + EGL mtrack
            "summary.system",           // 
            "summary.code",
            //"summary.java-heap",
            "summary.total-swap",
            "summary.total-pss",
        };

        private struct MemoryInfo
        {
            public string name;
            public int[] data;
            public int[] delta;
        }
    }
}
