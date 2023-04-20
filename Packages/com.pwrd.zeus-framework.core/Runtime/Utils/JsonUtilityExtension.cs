/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus.Core
{
    public static class JsonUtilityExtension
    {

        /// <summary> 把对象转换为Json字符串 </summary>
        /// <param name="obj">对象</param>
        public static string ToJson<T>(T obj)
        {
            if (obj == null) return "null";

            if (typeof(T).GetInterface("IList") != null)
            {
                Pack<T> pack = new Pack<T>();
                pack.data = obj;
                string json = JsonUtility.ToJson(pack);
                return json.Substring(8, json.Length - 9);
            }

            return JsonUtility.ToJson(obj);
        }

        /// <summary>
        /// 这里的T是指Array存储的数据的类型，比如是string，而不是string[]
        /// 只支持string,int,float,bool，需要其他类型，请联系zeus的支持同学
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T[] FromJsonTArray<T>(string json)
        {
            var list = ZeusJSON.DeserializetArray<T>(json);
            return list;
        }

        /// <summary> 解析Json </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="json">Json字符串</param>
        public static T FromJsonArray<T>(string json)
        {
            json = "{\"data\":{data}}".Replace("{data}", json);
            //json = string.Format("{{\"data\":{0}}}", json);
            Pack <T> Pack = JsonUtility.FromJson<Pack<T>>(json);
            return Pack.data;
        }

        public static T FromJson<T>(string json)
        {
            if (json == "null" && typeof(T).IsClass) return default(T);

            if (typeof(T).GetInterface("IList") != null)
            {
                return FromJsonArray<T>(json);
            }

            return JsonUtility.FromJson<T>(json);
        }

        public static Dictionary<string,string> FromJsonStringStringDic(string json)
        {
            return ZeusJSON.DeserializeDic<string, string>(json);
        }

        public static Dictionary<string,int> FromJsonStringIntDic(string json)
        {
            return ZeusJSON.DeserializeDic<string, int>(json);
        }

        private static Func<string, string> _string2String;
        private static Func<string, int> _string2Int;
        private static Func<string, float> _string2Float;

        public static Dictionary<string,float> FromJsonStringFloatDic(string json)
        {
            return ZeusJSON.DeserializeDic<string, float>(json);
        }

        /// <summary> 内部包装类 </summary>
        private class Pack<T>
        {
            public T data;
        }
    }
}