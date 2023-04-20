/*******************************************************************
* Copyright © 2017—2022 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEditor;
using UnityEngine;
using Zeus.Build;

namespace Zeus.Core
{
    /// <summary>
    /// 过滤打包过程中的log，提取出符合配置中配置的pattern的log，方便打包失败的时候进行排查
    /// 再xml的配置中，需要配置在所有操作的最前面和最后面
    /// 不同的接口会被创建不同实例，为了数据共享，大部分数据都设置成static
    /// 配套文件及修改包含:
    /// 1.预设配置在同目录的Config目录下，默认应copy到Assets下的ZeusSetting/EditorSetting目录下
    /// 2.ZeusBuildManifest，需将此类添加到最前和最后
    /// 3.配置中的输出路径名称可以添加{time}会被替换为时间字符串
    /// </summary>
    public class BuildLogFilterProcessor : IModifyPlayerSettings, IFinallyBuild
    {
        public class FilterSetting
        {
            /// <summary>
            /// 过滤后的log的输出路径，相对路径，相对根目录
            /// </summary>
            public string OutputPath;
            /// <summary>
            /// 进行匹配的模式
            /// </summary>
            public List<string> Pattern;
        }

        public string filterSettingPath = Path.Combine(Application.dataPath, "ZeusSetting/EditorSetting/ZeusBuildLogFilterSetting.json");
        private static FilterSetting m_setting;
        private static StringBuilder m_filteredStringBuilder;
        private static StringBuilder m_fullStringBuilder;
        private static int m_spinLock;
        private static int m_logCount = 0;
        private static int m_fullLogCount = 0;
        private static bool m_test;

        public static string GetOutput()
        {
            return m_filteredStringBuilder.ToString();
        }

        public static string GetFullOutput()
        {
            return m_fullStringBuilder.ToString();
        }

        public static int GetOutputCount()
        {
            return m_logCount;
        }

        public static int GetFullLogCont()
        {
            return m_fullLogCount;
        }

        public static void EnableTest()
        {
            m_test = true;
        }

        public static void DisableTest()
        {
            m_test = false;
        }

        public static void OnLogMessageReceived(string condition, string stackTrace, LogType logType)
        {
            //等待spinLock变成0
            while (Interlocked.CompareExchange(ref m_spinLock, 1, 0) != 0) ;
            try
            {
                foreach (var pattern in m_setting.Pattern)
                {
                    if (Regex.IsMatch(condition, pattern))
                    {
                        m_filteredStringBuilder.AppendLine(condition);
                        m_filteredStringBuilder.AppendLine(stackTrace);
                        m_logCount++;
                        break;
                    }
                }
                if(m_test)
                {
                    m_fullLogCount++;
                    m_fullStringBuilder.AppendLine(condition);
                }
            }
            catch (System.Exception e)
            {
            }
            finally
            {
                //spinLock设置成0
                Interlocked.Decrement(ref m_spinLock);
            }
        }

        public void OnFinallyBuild(BuildTarget target, string locationPathName)
        {
            Debug.LogFormat("logFilterProcessor OnFinallyBuild");
            if(null == m_setting)
            {
                return;
            }
            Application.logMessageReceivedThreaded -= OnLogMessageReceived;
            if(!string.IsNullOrEmpty(m_setting.OutputPath))
            {
                var absolutePath = Path.Combine(Application.dataPath, "..", m_setting.OutputPath);
                Zeus.Core.FileUtil.EnsureFolder(absolutePath);
                File.WriteAllText(absolutePath.Replace("{time}", System.DateTime.Now.ToString("yyyy--mm-dd-hh-mm-ss")), m_filteredStringBuilder.ToString());
            }

            if (m_filteredStringBuilder.Length > 0)
            {
                Debug.LogError("[Zeus]Build Failed Due to :\n" + m_filteredStringBuilder);
            }
        }

        public void OnModifyPlayerSettings(BuildTarget target)
        {
            //非强制，如果没有配置就不处理
            if(!File.Exists(filterSettingPath))
            {
                return;
            }
            try
            {
                var jsonStr = File.ReadAllText(filterSettingPath);
                m_setting = JsonUtility.FromJson<FilterSetting>(jsonStr);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
                m_setting = null;
                return;
            }
            m_filteredStringBuilder = new StringBuilder();
            if(m_test)
            {
                m_fullStringBuilder = new StringBuilder();
            }
            m_logCount = 0;
            m_fullLogCount = 0;
            Application.logMessageReceivedThreaded += OnLogMessageReceived;
        }
    }
}
