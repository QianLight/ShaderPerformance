using System;
using System.Collections.Generic;
using System.Reflection;


public static class ExtendFunctions
{
    public static T GetCustomAttribute<T>(this Type obj, bool isInhert) where T : System.Attribute
    {
        object[] attrs = obj.GetCustomAttributes(typeof(T), isInhert);
        T retObj = null;
        if (attrs.Length > 1)
        {
            throw new AmbiguousMatchException("类型" + obj.ToString() + "的属性个数匹配多余一个");
        }
        if (attrs.Length == 1)
        {
            retObj = attrs[0] as T;
        }
        return retObj;
    }

    public static IEnumerable<T> GetCustomAttributes<T>(this Type obj, bool isInhert) where T : System.Attribute
    {
        T[] objs = (T[])obj.GetCustomAttributes(typeof(T), isInhert);
        return objs;
    }
}

 

