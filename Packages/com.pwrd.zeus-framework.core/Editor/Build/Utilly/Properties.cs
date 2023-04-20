/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Zeus.Build
{
    public class Properties
    {
        private Dictionary<string, string> properties;
        private List<string> _list;
        private List<string> _keyList;
        private List<string> _valueList;
        private string _filePath;

        public string[] Keys
        {
            get
            {
                return _keyList.ToArray();
            }
        }

        public string[] Values
        {
            get
            {
                return _valueList.ToArray();
            }
        }

        public int Count
        {
            get
            {
                return properties.Count;
            }
        }

        /// <summary>
        /// 构造函数，并读取properties文件
        /// </summary>
        /// <param name="path">.properties文件的路径</param>
        public Properties(string path)
        {
            if(!File.Exists(path))
            {
                throw new Exception($"文件:{path}不存在,请检查");
            }
            properties = new Dictionary<string, string>();
            _list = new List<string>();
            _keyList = new List<string>();
            _valueList = new List<string>();
            Load(path);
        }

        public void Add(string key, string value)
        {
            properties.Add(key, value);
            _list.Add(key);
        }

        public void AddComment(string comment)
        {
            if(!comment.StartsWith("#") && !comment.StartsWith("!"))
            {
                _list.Add("#" + comment);
            }
            else
            {
                _list.Add(comment);
            }
        }
        
        public void Update(string key, string value)
        {
            if(properties.ContainsKey(key))
            {
                properties[key] = value;
            }
            else
            {
                Add(key.Trim(), value.Trim());
            }
        }

        public bool TryGetBool(string key, ref bool result)
        {
            if (properties.ContainsKey(key))
            {
                bool temp;
                if (!bool.TryParse(properties[key], out temp))
                {
                    //Debug.LogWarning(string.Format("Warning!! {0}:{1} Can't Convert to bool.", key, properties[key]));
                    return false;
                }
                result = temp;
                return true;
            }
            Debug.LogWarning(string.Format("Warning!! Properties not exist key: {0}.", key));
            return false;
        }

        public bool TryGetString(string key, ref string result)
        {
            if (properties.ContainsKey(key))
            {
                result = properties[key];
                return true;
            }
            Debug.LogWarning(string.Format("Warning!! Properties not exist key: {0}.", key));
            return false;
        }

        public string TryGetString(string key, string defaultValue)
        {
            TryGetString(key, ref defaultValue);
            return defaultValue;
        }

        public bool ContainsKey(string key)
        {
            return properties.ContainsKey(key);
        }

        /// <summary>
        /// 读取properties文件
        /// </summary>
        /// <param name="path">.properties文件的路径</param>
        public void Load(string path)
        {
            _filePath = path;
            properties.Clear();
            _list.Clear();
            _keyList.Clear();
            _valueList.Clear();
            using (StreamReader reader = new StreamReader(path))
            {
                string line;
                int lineL;
                int valueEnd, valueStart;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Length == 0)
                        continue;
                    lineL = line.Length;
                    valueEnd = 0;
                    valueStart = 0;
                    if (line.StartsWith("#") || line.StartsWith("!"))
                    {
                        _list.Add(line);
                        continue;
                    }
                    string key = "";
                    string[] str = line.Split(new char[]{ '=', ':'});
                    if(str.Length == 1) //非法的注释
                    {
                        key = line;
                        valueEnd = lineL;
                    }
                    else
                    {
                        key = str[0];
                        valueEnd = line.IndexOf("=") + 1;
                        valueStart = valueEnd;
                    }
                    string value = "";
                    while (valueEnd < lineL)    //value可以通过\换行
                    {
                        char c = line[valueEnd];
                        if (c == '\\' && valueEnd + 1 < lineL && line[valueEnd + 1] == '\\')
                        {
                            valueEnd += 2;
                        }
                        else if (c == '\\')
                        {
                            value += line.Substring(valueStart, valueEnd - valueStart);
                            valueStart = 0;
                            valueEnd = valueStart;
                            if ((line = reader.ReadLine()) == null)
                            {
                                line = "";
                                break;
                            }
                            lineL = line.Length;
                            while (line[valueStart] == ' ' || line[valueStart] == '\t')  //忽略' '和'\t'
                            {
                                valueStart++;
                            }
                            valueEnd = valueStart;
                        }
                        else
                        {
                            valueEnd++;
                        }
                    }
                    value += line.Substring(valueStart, valueEnd - valueStart);
                    if (key == "")
                    {
                        key += "#";
                        _list.Add(key + value);
                        continue;
                    }
                    if (value == "")
                    {
                        _list.Add("#" + key);
                        continue;
                    }
                    Add(key.Trim(), value.Trim());
                }
            }             
            InitKeyValueList();
        }

        private void InitKeyValueList()
        {
            foreach (KeyValuePair<string, string> kvp in properties)
            {
                _keyList.Add(kvp.Key);
                _valueList.Add(kvp.Value);
            }
        }

        public void Save(string path)
        {
            _filePath = path;
            Save();
        }

        public void Save()
        {
            using (StreamWriter writer = new StreamWriter(_filePath, false))
            {
                foreach (string key in _list)
                {
                    if (key.StartsWith("#") || key.StartsWith("!"))
                    {
                        writer.WriteLine(key);
                    }
                    else if (ContainsKey(key))
                    {
                        writer.WriteLine(key + "=" + properties[key]);
                    }
                    else
                    {
                        Debug.LogWarning("Wrong format of properties file!");
                    }
                }
            }
        }
    }
}


