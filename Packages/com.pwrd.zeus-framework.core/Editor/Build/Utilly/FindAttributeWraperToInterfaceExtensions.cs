/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Zeus.Build;
namespace Zeus
{
    public static class FindAttributeWraperToInterfaceExtensions
    {
        /// <summary>
        /// 遍历已加载到此应用程序域的自定义程序集中Interface实现的所有实例，添加到list。
        /// </summary>
        /// <param name="ignoreWrapper">是否忽略继承IWrapper接口的实例</param>
        public static void AddInterfaceFromCustomAssemblies<T_Interface>(this List<T_Interface> list, bool ignoreWrapper = true) where T_Interface : class
        {
            Assembly[] assemblyes = EditorReflectionUtilly.GetCustomAssemblies().Where(a => a.FullName.StartsWith("Assembly-CSharp",StringComparison.CurrentCultureIgnoreCase)).ToArray();
            Type[] types = assemblyes.SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(T_Interface)))).ToArray();
            foreach (Type type in types)
            {
                if (!ignoreWrapper || !typeof(IWrapper).IsAssignableFrom(type))
                {
                    list.Add(Activator.CreateInstance(type) as T_Interface);
                }
            }
        }

        /// <summary>
        /// 遍历已加载到此应用程序域的自定义程序集中引用Attribute的方法，包装成对于的Interface，添加到list。
        /// </summary>
        /// <param name="bingdingAttr"> 一个位屏蔽，由一个或多个指定搜索执行方式的 System.Reflection.BindingFlags 组成。 - 或 - 零，以返回 null。</param>
        /// <param name="expectedArguments">要验证的参数形式</param>
        /// <param name="wrapperType">包装Interface的Wrapper</param>
        public static void AddAttributeFromCustomAssemblies<T_Interface, T_Attribute>(this List<T_Interface> list, BindingFlags bingdingAttr, Type[] expectedArguments, Type wrapperType) where T_Interface : class where T_Attribute : class
        {
            if (!typeof(IWrapperAttributeToInferface<T_Interface, T_Attribute>).IsAssignableFrom(wrapperType))
            {
                Debug.LogError(string.Format("{0} can not convert to IWrapper<{1},{2}>.", wrapperType.FullName, typeof(T_Interface).FullName, typeof(T_Attribute).FullName));
                return;
            }
            Assembly[] assemblyes = EditorReflectionUtilly.GetCustomAssemblies().Where(a => a.FullName.StartsWith("Assembly-CSharp", StringComparison.CurrentCultureIgnoreCase)).ToArray();
            MethodInfo[] methods = assemblyes.SelectMany(a => a.GetTypes().SelectMany(t => t.GetMethods(bingdingAttr).Where(m => m.ValidateStaticMethod(typeof(T_Attribute), expectedArguments)))).ToArray();
            foreach (MethodInfo method in methods)
            {
                if (!typeof(IWrapperAttributeToInferface<T_Interface, T_Attribute>).IsAssignableFrom(method.GetType()))
                {
                    IWrapperAttributeToInferface<T_Interface, T_Attribute> result = Activator.CreateInstance(wrapperType) as IWrapperAttributeToInferface<T_Interface, T_Attribute>;
                    result.Wrapper(method);
                    list.Add(result as T_Interface);
                }
            }
        }

        /// <summary>
        /// 遍历已加载到此应用程序域的自定义程序集中引用Attribute的方法，包装成对于的Interface，添加到list。
        /// BindingFlag默认为：BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic。
        /// </summary>
        /// <param name="expectedArguments">要验证的参数形式</param>
        /// <param name="wrapperType">包装Interface的Wrapper</param>
        public static void AddAttributeFromCustomAssemblies<T_Interface, T_Attribute>(this List<T_Interface> list, Type[] expectedArguments, Type wrapperType) where T_Interface : class where T_Attribute : class
        {
            BindingFlags _BindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            list.AddAttributeFromCustomAssemblies<T_Interface, T_Attribute>(_BindingAttr, expectedArguments, wrapperType);
        }

    }
}
#endif