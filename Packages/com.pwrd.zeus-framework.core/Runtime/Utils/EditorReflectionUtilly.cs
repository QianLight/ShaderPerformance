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
using JetBrains.Annotations;

namespace Zeus
{
    public static class EditorReflectionUtilly
    {
        #region Type 判断是否内置类型/UnityObject类型/自定义类型[非泛型，非集合]
        /// <summary>
        /// 是否是内置类型
        /// </summary>
        public static bool IsBulitinType(Type type)
        {
            return (type == typeof(object) || Type.GetTypeCode(type) != TypeCode.Object);
        }

        /// <summary>
        /// 是否是Unity的Object类型
        /// </summary>
        public static bool IsUnityObjectType(Type type)
        {
            return typeof(UnityEngine.Object).IsAssignableFrom(type);
        }

        /// <summary>
        /// 是否是自定义的类型(非泛型，非集合)
        /// </summary>
        public static bool IsCustomType(Type type)
        {
            return !type.IsArray && !type.IsGenericType && (type != typeof(object) && Type.GetTypeCode(type) == TypeCode.Object);
        }
        #endregion

        #region Type的一些判断方法
        /// <summary>
        /// 指示Value中是否包含flag
        /// </summary>
        public static bool ContainsFlag(FieldAttributes value, FieldAttributes flag)
        {
            return (value & flag) == flag;
        }

        /// <summary>
        /// 指示字段是否是只读属性（readonly  |  const）
        /// </summary>
        public static bool IsReadOnly(FieldAttributes value)
        {
            return ContainsFlag(value, FieldAttributes.InitOnly) || ContainsFlag(value, FieldAttributes.Literal);
        }

        /// <summary>
        /// 指示字段是否是只读属性（readonly  |  const）
        /// </summary>
        public static bool IsReadOnly([NotNull]FieldInfo fieldInfo)
        {
            return IsReadOnly(fieldInfo.Attributes);
        }

        /// <summary>
        /// 指示instance是否是字典
        /// </summary>
        public static bool IsDictionary<T>([NotNull]T instance)
        {
            return IsDictionary(instance.GetType());
        }

        /// <summary>
        /// 指示type是否是字典
        /// </summary>
        public static bool IsDictionary([NotNull]Type type)
        {
            return type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(IDictionary<,>) || type.GetGenericTypeDefinition() == typeof(IDictionary));
        }

        /// <summary>
        /// 指示instance是否是列表
        /// </summary>
        public static bool IsList<T>([NotNull]T instance)
        {
            return IsList(instance.GetType());
        }

        /// <summary>
        /// 指示type是否是列表
        /// </summary>
        public static bool IsList([NotNull]Type type)
        {
            return type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(IList<>) || type.GetGenericTypeDefinition() == typeof(IList));
        }

        /// <summary>
        /// 指示instance是否是[rank]数组
        /// </summary>
        /// <param name="instance">instance</param>
        /// <param name="rank">Array中的维数</param>
        /// <returns></returns>
        public static bool IsArray<T>([NotNull]T instance, int rank)
        {
            return IsArray(instance.GetType(), rank);
        }

        /// <summary>
        /// 指示type是否是[rank]数组
        /// </summary>
        /// <param name="type">type</param>
        /// <param name="rank">Array中的维数</param>
        /// <returns></returns>
        public static bool IsArray([NotNull]Type type, int rank)
        {
            return type.IsArray && type.GetArrayRank() == rank;
        }

        /// <summary>
        /// 指示instance是否是数组
        /// </summary>
        public static bool IsArray<T>([NotNull]T instance)
        {
            return instance.GetType().IsArray;
        }

        /// <summary>
        /// 指示type是否是数组
        /// </summary>
        public static bool IsArray([NotNull]Type type)
        {
            return type.IsArray;
        }

        /// <summary>
        /// type是否包含attributeType特性
        /// </summary>
        /// <param name="type"></param>
        /// <param name="attributeType"></param>
        /// <returns></returns>
        public static bool IsContainsAttribute(Type type, Type attributeType)
        {
            return type.GetCustomAttributes(attributeType, false).Length > 0;
        }
        #endregion

        #region 要排除的Assembly
        /// <summary>
        /// 要排除的Assembly
        /// </summary>
        static List<string> AssemblyExclusion = new List<string>() {
                    "Unity.Locator",
                    "Unity.PackageManager",
                    "nunit.framework",

                    "UnityEditor",
                    "UnityEditor.Analytics",
                    "UnityEditor.Advertisements",
                    "UnityEditor.HoloLens",
                    "UnityEditor.Networking",
                    "UnityEditor.TreeEditor",
                    "UnityEditor.Purchasing",
                    "UnityEditor.Graphs",
                    "UnityEditor.UI",
                    "UnityEditor.VR",
                    "UnityEditor.TestRunner",
                    "UnityEditor.Android.Extensions",
                    "UnityEditor.iOS.Extensions",
                    "UnityEditor.iOS.Extensions.Xcode",
                    "UnityEditor.iOS.Extensions.Common",
                    "UnityEditor.WindowsStandalone.Extensions",

                    "UnityEngine.Analytics",
                    "UnityEngine.TestRunner",
                    "UnityEngine.UI",
                    "UnityEngine.VR",
                    "UnityEngine.Networking",
                    "UnityEngine.HoloLens",

                    "SyntaxTree.VisualStudio.Unity.Bridge"};
        #endregion

        /// <summary>
        /// 获取已加载到此应用程序域的执行上下文中的自定义的程序集
        /// </summary>
        public static Assembly[] GetCustomAssemblies()
        {
            Type type = typeof(EditorWindow).Assembly.GetType("UnityEditor.EditorAssemblies");
            return (type.GetProperty("loadedAssemblies", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null, null) as Assembly[]).Where(a => !AssemblyExclusion.Contains(a.FullName.Substring(0, a.FullName.IndexOf(',')))).ToArray();
        }

        /// <summary>
        /// 验证一个方法是否是使用了指定的Attribute，指定的参数形式的静态方法，如果同时满足，则返回true，否则返回false。
        /// </summary>
        /// <param name="method">要验证的方法</param>
        /// <param name="attributeType">要验证的Attribute的Type</param>
        /// <param name="expectedArguments">要验证的参数形式</param>
        /// <returns></returns>
        public static bool ValidateStaticMethod(this MethodInfo method, Type attributeType, Type[] expectedArguments)
        {
            bool result;
            if (method.IsDefined(attributeType, false))
            {
                if (!method.IsStatic)
                {
                    string text = attributeType.Name.Replace("Attribute", "");
                    string error = string.Format("Method {0} with {1}Attribute must be static.", new object[]
                    {
                        method.DeclaringType + "." + method.Name,
                        text
                    });
                    throw new Exception(error);
                    //Debug.LogError(error);
                    //result = false;
                }
                else if (method.IsGenericMethod || method.IsGenericMethodDefinition)
                {
                    string text2 = attributeType.Name.Replace("Attribute", "");
                    string error = string.Format("Method {0} with {1}Attribute cannot be generic.", new object[]
                    {
                        method.DeclaringType + "." + method.Name,
                        text2
                    });
                    throw new Exception(error);
                    //Debug.LogError(error);
                    //result = false;
                }
                else
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    bool flag = parameters.Length == (expectedArguments == null ? 0 : expectedArguments.Length);
                    if (flag)
                    {
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            if (parameters[i].ParameterType != expectedArguments[i])
                            {
                                flag = false;
                                break;
                            }
                        }
                    }
                    if (!flag)
                    {
                        string text3 = attributeType.Name.Replace("Attribute", "");
                        string text4 = "static void " + method.Name + "(";
                        for (int j = 0; j < expectedArguments.Length; j++)
                        {
                            text4 += expectedArguments[j].Name;
                            if (j != expectedArguments.Length - 1)
                            {
                                text4 += ", ";
                            }
                        }
                        text4 += ")";
                        string error = string.Format("Method {0} with {1}Attribute does not have the correct signature, expected: {2}.", new object[]
                        {
                            method.DeclaringType + "." + method.Name,
                            text3,
                            text4
                        });
                        throw new Exception(error);
                        //Debug.LogError(error);
                        //result = false;
                    }
                    else
                    {
                        result = true;
                    }
                }
            }
            else
            {
                result = false;
            }
            return result;
        }
    }
}
#endif