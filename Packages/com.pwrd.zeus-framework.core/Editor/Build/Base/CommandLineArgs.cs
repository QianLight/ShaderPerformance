/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Zeus.Build
{
    public static class CommandLineArgs
    {
        private static Dictionary<string, string> _ArgsDictionary = new Dictionary<string, string>();
        public static bool Inited = false;

        //  Command Line Args separtor
        static char separtor = '=';

        /// <summary>
        /// 获取包含当前进程的命令行参数的字符串，并存入ArgsDictionary
        /// </summary>
        /// <param name="propertiesFilePath">传入文件名</param>
        /// <param name="isToLower">是否转换成小写</param>
        /// <param name="exclusion">不需要转换成小写的字符串集合</param>
        public static void Initialize(string propertiesFilePath = null, bool isToLower = false, params string[] exclusion)
        {
            _ArgsDictionary.Clear();
            if (propertiesFilePath == null)
            {
                string[] args = System.Environment.GetCommandLineArgs();
                foreach (string arg in args)
                {
                    if (!arg.ToLower().Contains("config_name=")) 
                        continue;
                    propertiesFilePath = arg.Split('=')[1].Trim();
                    break;
                }
            }
            InitializeProperties(propertiesFilePath);
            InitializeCommandLine(isToLower, exclusion);
            OutputBuildParams();
            Inited = true;
        }

        /// <param name="propertyName">传入文件名</param>
        private static void InitializeProperties(string propertyName = null)
        {
            const string pathTail = ".properties";
            string pathHead = GlobalBuild.BuildConst.ZEUS_BUILD_PATH_CONFIG_SLASH;
            string filePath;
            if (null == propertyName)
            {
                filePath = pathHead + "BUILD_DEFAULT" + pathTail;
                string[] args = System.Environment.GetCommandLineArgs();
                foreach (string arg in args)
                {
                    if (arg.ToLower().Contains(pathTail))
                    {
                        filePath = pathHead + arg.Split('=')[1].Trim();
                    }
                }
            }
            else
            {
                filePath = pathHead + propertyName + (propertyName.Contains(pathTail)?"":pathTail);
            }
            Properties _properties = new Properties(filePath);
            foreach (string key in _properties.Keys)
            {
                string value = "";
                if (_properties.TryGetString(key, ref value))
                {
                    CommandLineArgs.Add(key, value);
                }
            }
        }

        /// <summary>
        /// 初始化参数字符串集合，并存入ArgsDictionary，参数字符串如：key=value
        /// </summary>
        /// <param name="isToLower">是否转换成小写</param>
        /// <param name="exclusion">不需要转换成小写的字符串集合</param>
        public static void InitializeCommandLine(bool isToLower = true, params string[] exclusion) {
            string[] args = System.Environment.GetCommandLineArgs();
            if (args != null)
            {
                for (int i = 0; i < args.Length; ++i)
                {
                    string paramStr = args[i];
                    if (paramStr.Contains(separtor.ToString()))
                    {
                        if (isToLower && !paramStr.Contains(exclusion)) paramStr = paramStr.ToLower();
                        string key = paramStr.Substring(0, paramStr.IndexOf(separtor));
                        string value = paramStr.Substring(paramStr.IndexOf(separtor) + 1);
                        _ArgsDictionary[key] = value;
                    }
                }
            }
        }

        public static void OutputBuildParams()
        {
            StringBuilder temp = new StringBuilder();
            temp.AppendLine("zeus build params:");
            foreach (var arg in _ArgsDictionary)
            {
                temp.AppendFormat("{0}={1}\n", arg.Key, arg.Value);
            }
            temp.AppendLine("end");
            Debug.Log(temp.ToString());
        }

        public static void Add(string key, string value) {
            if (!_ArgsDictionary.ContainsKey(key))
            {
                _ArgsDictionary.Add(key, value);
            }
            else {
                _ArgsDictionary[key] = value;
            }
        }
        public static void Remove(string key) {
            if (_ArgsDictionary.ContainsKey(key)) {
                _ArgsDictionary.Remove(key);
            }
        }

        public static void Clear() {
            _ArgsDictionary.Clear();
        }

        public static string GetString(string key) {
            return _ArgsDictionary[key];
        }

        /// <summary>
        /// 尝试获取字符串并返回结果，如果失败，则返回默认值。
        /// </summary>
        /// <param name="defaultValue">默认值</param>
        public static string GetString(string key, string defaultValue)
        {
            TryGetString(key, ref defaultValue);
            return defaultValue;
        }
        
        /// <summary>
        /// 尝试获取，如果获取失败，不影响原有值。
        /// </summary>
        /// <returns>true为获取成功，false为获取失败</returns>
        public static bool TryGetString(string key, ref string result) {
            if (_ArgsDictionary.ContainsKey(key)) {
                result = _ArgsDictionary[key];
                return true;
            }
            Debug.LogWarning(string.Format("Warming!! Command line not exist key: {0}.", key));
            return false;
        }

        /// <summary>
        /// 判断是否包含指定参数
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool ContainKey(string key)
        {
            return _ArgsDictionary.ContainsKey(key);
        }

        public static bool GetBool(string key) {
            return bool.Parse(_ArgsDictionary[key]);
        }

        /// <summary>
        /// 尝试获取，如果获取失败，不影响原有值。
        /// </summary>
        /// <returns>true为获取成功，false为获取失败</returns>
        public static bool TryGetBool(string key, ref bool result) {
            if (_ArgsDictionary.ContainsKey(key)) {
                bool temp;
                if (!bool.TryParse(_ArgsDictionary[key],out temp)) {
                    Debug.LogWarning(string.Format("Warming!! {0}:{1} Can't Convert to bool.", key, _ArgsDictionary[key]));
                    return false;
                }
                result = temp;
                return true;
            }
            Debug.LogWarning(string.Format("Warming!! Command line not exist key: {0}.", key));
            return false;
        }

        public static Dictionary<string, string> GetProperties(string platform)
        {
            string path = Application.dataPath + "/../_Build/Config/" + platform + ".properties";
            Dictionary<string, string> properties = new Dictionary<string, string>();
            if (File.Exists(path))
            {
                Properties _properties = new Properties(path);
                foreach (string key in _properties.Keys)
                {
                    string value = "";
                    if (_properties.TryGetString(key, ref value))
                    {
                        properties.Add(key, value);
                    }
                }
            }
            return properties;
        }

        public static int GetInt(string key) {
            return int.Parse(_ArgsDictionary[key]);
        }

        /// <summary>
        /// 尝试获取，如果获取失败，不影响原有值。
        /// </summary>
        /// <returns>true为获取成功，false为获取失败</returns>
        public static bool TryGetInt(string key, ref int result)
        {
            if (_ArgsDictionary.ContainsKey(key))
            {
                int temp;
                if (!int.TryParse(_ArgsDictionary[key], out temp))
                {
                    Debug.LogWarning(string.Format("Warming!! {0}:{1} Can't Convert to int.", key, _ArgsDictionary[key]));
                    return false;
                }
                result = temp;
                return true;
            }
            Debug.LogWarning(string.Format("Warming!! Command line not exist key: {0}.", key));
            return false;
        }

        /// <summary>
        /// 尝试获取Int值并返回结果，如果失败，则返回默认值。
        /// </summary>
        /// <param name="defaultValue">默认值</param>
        public static int TryGetInt(string key, int defaultValue)
        {
            TryGetInt(key, ref defaultValue);
            return defaultValue;
        }

        /// <summary>
        /// 扩展方法:指示字符串中是否包含args中的某个字符串
        /// </summary>
        private static bool Contains(this string str, string[] args)
        {
            foreach (string s in args)
            {
                if (str.Contains(s)) return true;
            }
            return false;
        }
    }
}
#endif