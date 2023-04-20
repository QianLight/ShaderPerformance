using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using Ender.LitJson;

namespace Ender
{
    public class GMEnderHelper
    {

        public static int GetInt(JsonData data, string key)
        {
            if (data.ContainsKey(key))
            {
                return int.Parse(data[key].ToJson());
            }
            return 0;
        }

        public static string GetString(JsonData data, string key)
        {
            if (data.ContainsKey(key))
            {
                return data[key].ToString();
            }
            return "";
        }

        public static object GetObject(JsonData map, string key)
        {
            if (map == null)
            {
                return null;
            }

            object value = null;
            if (map.Keys.Contains(key))
            {
                JsonData data = map["value"];
                if (data.IsBoolean)
                {
                    value = (bool)data;
                }
                else if (data.IsDouble)
                {
                    value = (double)data;
                }
                else if (data.IsInt)
                {
                    value = (int)data;
                }
                else if (data.IsLong)
                {
                    value = (long)data;
                }
                else if (data.IsString)
                {
                    value = (string)data;
                }
                else
                {
                    value = data;
                }
            }
            return value;
        }

        /// <summary>
        /// 读取JsonData
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static JsonData GetData(JsonData data, string key)
        {
            if (data.ContainsKey(key))
            {
                return data[key];
            }
            return null;
        }

        /// <summary>
        /// 获取对应数据的类型
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Type getDataType(JsonData data)
        {
            if (data == null)
            {
                return null;
            }

            if (data.IsBoolean)
            {
                return typeof(bool);
            }
            else if (data.IsDouble)
            {
                return typeof(double);
            }
            else if (data.IsInt)
            {
                return typeof(int);
            }
            else if (data.IsLong)
            {
                return typeof(long);
            }
            else if (data.IsString)
            {
                return typeof(string);
            }

            return null;

        }


        /// <summary>
        /// 获取对应数据的类型
        /// </summary>
        /// <param name="map"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static object GetObjectByIndex(JsonData data)
        {
            if (data == null)
            {
                return null;
            }
            object value;
            if (data.IsBoolean)
            {
                value = (bool)data;
            }
            else if (data.IsDouble)
            {
                value = (double)data;
            }
            else if (data.IsInt)
            {
                value = (int)data;
            }
            else if (data.IsLong)
            {
                value = (long)data;
            }
            else if (data.IsString)
            {
                value = (string)data;
            }
            else
            {
                value = data;
            }
            return value;
        }
    }
}
#endif
