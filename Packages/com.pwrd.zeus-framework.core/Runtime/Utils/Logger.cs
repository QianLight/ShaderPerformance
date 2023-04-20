/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using Zeus.Core.FileSystem;

namespace Zeus.Core
{
    public static class Logger
    {
        private static Dictionary<string, StreamWriter> _streams = new Dictionary<string, StreamWriter>();
        static System.Text.StringBuilder builder = new System.Text.StringBuilder();

        public static void Log(string tag, params object[] list)
        {
            string logContent = AssembleLog(tag, list);
            UnityEngine.Debug.Log(logContent);
        }
        
        public static void Log(bool withTime, string tag, params object[] list)
        {
            string logContent = AssembleLog(withTime, tag, list);
            UnityEngine.Debug.Log(logContent);
        }
        
        public static void LogError(string tag, params object[] list)
        {
            string logContent = AssembleLog(tag, list);
            UnityEngine.Debug.LogError(logContent);
        }

        public static void LogWarning(string tag, params object[] list)
        {
            string logContent = AssembleLog(tag, list);
            UnityEngine.Debug.LogWarning(logContent);
        }

        /// <summary>
        /// 打印日志到指定文件
        /// </summary>
        public static void LogToFile(string virtualPath, string tag, params object[] list)
        {
            StreamWriter stream;
            if(_streams.Count == 0)
            {
                ZeusCore.Instance.RegisterOnApplicationQuit(CloseLogFileStream);
            }
            if (!_streams.TryGetValue(virtualPath,out stream))
            {
                Stream fileStream = VFileSystem.OpenFile(virtualPath, FileMode.Append, FileAccess.Write);
                stream = new StreamWriter(fileStream);
                _streams.Add(virtualPath, stream);
            }
            stream.Write(AssembleLog(true,tag,list));
            stream.Flush();
        }

        private static void CloseLogFileStream()
        {
            foreach(var pair in _streams)
            {
                pair.Value.Close();
            }
            _streams.Clear();
        }

        private static string AssembleLog(string tag, params object[] list)
        {
            return AssembleLog(false, tag, list);
        }
        
        private static string AssembleLog(bool withTime, string tag, params object[] list)
        {
            builder.Clear();
            if (withTime)
            {
                builder.Append("[");
                builder.Append(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
                builder.Append("] ");
            }
            if (tag != null)
            {
                builder.Append("[");
                builder.Append(tag);
                builder.Append("] ");
            }
            if(list != null)
            {
                for (int i = 0; i < list.Length; i++)
                {
                    builder.Append(list[i] == null ? "null" : list[i].ToString());
                    builder.Append(" ");
                }
            }
            builder.Append("\n");
            return builder.ToString();
        }
    }
}