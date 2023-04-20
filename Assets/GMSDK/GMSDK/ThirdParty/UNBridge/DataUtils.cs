using System;
using UNBridgeLib.LitJson;

namespace UNBridgeLib
{

    /// <summary>
    ///  从JsonData中安全的读取数据的帮助类
    /// </summary>
    public class DataUtils
    {
        public DataUtils()
        {
        }

        /// <summary>
        /// 读取int
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static int GetInt(JsonData data, string key)
        {
            if (data.ContainsKey(key))
            {
                return int.Parse(data[key].ToString());
            }
            return 0;
        }

        /// <summary>
        /// 读取long
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static long GetLong(JsonData data, string key)
        {
            if (data.ContainsKey(key))
            {
                return long.Parse(data[key].ToString());
            }
            return 0;
        }

        /// <summary>
        /// 读取string
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetString(JsonData data, string key)
        {
            if (data.ContainsKey(key))
            {
                return data[key].ToString();
            }
            return "";
        }

        /// <summary>
        /// 读取double
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static double GetDouble(JsonData data, string key)
        {
            if (data.ContainsKey(key))
            {
                return double.Parse(data[key].ToString());
            }
            return 0;
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
        /// 读取Object
        /// </summary>
        /// <param name="map"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static object GetObject(JsonData map, string key)
        {
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

    }
}
