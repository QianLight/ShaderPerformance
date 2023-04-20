#if BP_TOLUA
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UI;
using BindType = Blueprint.Emmy.BlueprintEmmyTLM.BindType;
using System.Reflection;

namespace Blueprint.Emmy
{
    public static class BlueprintEmmyCS
    {
        //导出时强制做为静态类的类型(注意customTypeList 还要添加这个类型才能导出)
        //unity 有些类作为sealed class, 其实完全等价于静态类
        public static List<Type> staticClassTypes;

        //在这里添加你要导出注册到lua的类型列表
        public static BindType[] customTypeList;

    }
}
#endif