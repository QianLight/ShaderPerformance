/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections.Generic;
using System;

namespace Zeus
{
	public static class ZeusUtil
	{
        /// <summary>
        /// 删除指定list中的重复项
        /// </summary>
	    public static void StripRepeat<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i >= 1; --i)
            {
                for (int j = i - 1; j >= 0; --j)
                {
                    if (EqualityComparer<T>.Default.Equals(list[j], list[i]))
                    {
                        list.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        private static Dictionary<Type, Dictionary<object, string>> EnumStringCache = new Dictionary<Type, Dictionary<object, string>>();

        private static void InitDictEnum(Type type)
        {
            if(!EnumStringCache.ContainsKey(type))
            {
                Dictionary<object, string> dict = new Dictionary<object, string>();
                foreach(object v in Enum.GetValues(type))
                {
                    dict.Add(v, v.ToString());
                }
                EnumStringCache.Add(type, dict);
            }
        }

        public static string GetEnumString(Enum e)
        {
            Type type = e.GetType();
            InitDictEnum(type);
            return EnumStringCache[type][e];
        }

        public static object GetEnumValue(Type type, string eName)
        {
            if(doesEnumHaveName(type, eName))
            {
                return System.Enum.Parse(type, eName);
            }
            return null;
        }

        public static bool doesEnumHaveName(Type type, string eName)
        {
            InitDictEnum(type);
            return EnumStringCache[type].ContainsValue(eName);
        }

    }
}
