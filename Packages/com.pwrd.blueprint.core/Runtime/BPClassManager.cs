using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Text;

namespace Blueprint
{
    public class BPClassManager
    {
        public static BPClassManager Instance;

        private Dictionary<string, Type> bpClassDic = new Dictionary<string, Type>();

        /// <summary>
        /// 协议枚举字典
        /// </summary>
        /// <typeparam name="int">协议编号</typeparam>
        /// <typeparam name="string">协议名称</typeparam>
        /// <returns></returns>
        private Dictionary<int, string> ProtoEnumDic = new Dictionary<int, string>();

        /// <summary>
        ///  根据名称获取Type，名称需要是命名空间+类型，例如Blueprint.UI.MainUI
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Type GetClass(string name)
        {
            Type type = null;

            if (string.IsNullOrEmpty(name))
            {
                return type;
            }

            bpClassDic.TryGetValue(name, out type);
            return type;
        }

        /// <summary>
        /// 根据命名空间和proto类型获取proto类
        /// </summary>
        /// <param name="namesp"></param>
        /// <param name="protoType"></param>
        /// <returns></returns>
        public Type GetProtoClass(string namesp, int protoType)
        {
            if (ProtoEnumDic.ContainsKey(protoType))
            {
                string className = namesp + ProtoEnumDic[protoType];
                return GetClass(className);
            }
            return null;
        }

        /// <summary>
        /// 根据名称获取无参构造函数，名称需要是命名空间+类型，例如Blueprint.UI.MainUI
        /// </summary>
        /// <param name="className"></param>
        /// <returns></returns>
        public ConstructorInfo GetEmptyConstructorInfo(string className)
        {
            Type type = GetClass(className);
            if (type == null)
            {
                Debug.LogError(string.Format("Can't find class type: {0}", className));
                return null;
            }

            return type.GetConstructor(System.Type.EmptyTypes);
        }

        public static void Init()
        {
            if (Instance != null)
            {
                return ;
            }

            Instance = new BPClassManager();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsClass)
                    {
                        if (!Instance.bpClassDic.ContainsKey(type.FullName))
                        {
                            Instance.bpClassDic.Add(type.FullName, type);
                        }
                    }
                }
            }
            
        }
    }

}