#if BP_TOLUA
/*
Copyright (c) 2015-2017 topameng(topameng@qq.com)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
//打开开关没有写入导出列表的纯虚类自动跳过
//#define JUMP_NODEFINED_ABSTRACT

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.IO;
using System.Diagnostics;

using Object = UnityEngine.Object;
using Debug = UnityEngine.Debug;
using System.Threading;

//[InitializeOnLoad]
namespace Blueprint.Emmy
{
    public static class BlueprintEmmyTLM
    {
        //不需要导出或者无法导出的类型
        public static List<Type> dropType = new List<Type>
    {
        typeof(ValueType),                                  //不需要
#if UNITY_4_6 || UNITY_4_7
        typeof(Motion),                                     //很多平台只是空类
#endif

#if UNITY_5_3_OR_NEWER
        typeof(UnityEngine.CustomYieldInstruction),
#endif
        typeof(UnityEngine.YieldInstruction),               //无需导出的类
        // typeof(UnityEngine.WaitForEndOfFrame),              //内部支持
        // typeof(UnityEngine.WaitForFixedUpdate),
        // typeof(UnityEngine.WaitForSeconds),
        typeof(UnityEngine.Mathf),                          //lua层支持
        typeof(Plane),
        typeof(LayerMask),
        typeof(Vector3),
        typeof(Vector4),
        typeof(Vector2),
        typeof(Quaternion),
        typeof(Ray),
        typeof(Bounds),
        typeof(Color),
        typeof(Touch),
        typeof(RaycastHit),
        typeof(TouchPhase),
        /*
        //typeof(LuaInterface.LuaOutMetatable),               //手写支持
        typeof(LuaInterface.NullObject),
        typeof(System.Array),
        typeof(System.Reflection.MemberInfo),
        typeof(System.Reflection.BindingFlags),
        typeof(LuaClient),
        typeof(LuaInterface.LuaFunction),
        typeof(LuaInterface.LuaTable),
        typeof(LuaInterface.LuaThread),
        typeof(LuaInterface.LuaByteBuffer),                 //只是类型标识符
        typeof(DelegateFactory),                            //无需导出，导出类支持lua函数转换为委托。如UIEventListener.OnClick(luafunc)
        */
    };

        //可以导出的内部支持类型
        public static List<Type> baseType = new List<Type>
    {
        typeof(System.Object),
        typeof(System.Delegate),
        typeof(System.String),
        typeof(System.Enum),
        typeof(System.Type),
        typeof(System.Collections.IEnumerator),
        typeof(UnityEngine.Object),
        /*
        typeof(LuaInterface.EventObject),
        typeof(LuaInterface.LuaMethod),
        typeof(LuaInterface.LuaProperty),
        typeof(LuaInterface.LuaField),
        typeof(LuaInterface.LuaConstructor),
        */
    };

        private static bool beAutoGen = false;
        private static bool beCheck = true;
        static List<BindType> allTypes = new List<BindType>();

        static BlueprintEmmyTLM()
        {
        }

        public class BindType
        {
            public string name;                 //类名称
            public Type type;
            public bool IsStatic;
            public string wrapName = "";        //产生的wrap文件名字
            public string libName = "";         //注册到lua的名字
            public Type baseType = null;
            public string nameSpace = null;     //注册到lua的table层级

            public List<Type> extendList = new List<Type>();

            public BindType(Type t)
            {
                if (typeof(System.MulticastDelegate).IsAssignableFrom(t))
                {
                    throw new NotSupportedException(string.Format("\nDon't export Delegate {0} as a class, register it in customDelegateList", BlueprintEmmyLM.GetTypeName(t)));
                }

                //if (IsObsolete(t))
                //{
                //    throw new Exception(string.Format("\n{0} is obsolete, don't export it!", LuaMisc.GetTypeName(t)));
                //}

                type = t;
                nameSpace = BlueprintEmmyTLE.GetNameSpace(t, out libName);
                name = BlueprintEmmyTLE.CombineTypeStr(nameSpace, libName);
                libName = BlueprintEmmyTLE.ConvertToLibSign(libName);

                if (name == "object")
                {
                    wrapName = "System_Object";
                    name = "System.Object";
                }
                else if (name == "string")
                {
                    wrapName = "System_String";
                    name = "System.String";
                }
                else
                {
                    wrapName = name.Replace('.', '_');
                    wrapName = BlueprintEmmyTLE.ConvertToLibSign(wrapName);
                }

                int index = BlueprintEmmyCS.staticClassTypes.IndexOf(type);

                if (index >= 0 || (type.IsAbstract && type.IsSealed))
                {
                    IsStatic = true;
                }

                baseType = BlueprintEmmyLM.GetExportBaseType(type);
            }
        }

        static void GetAllDirs(string dir, List<string> list)
        {
            string[] dirs = Directory.GetDirectories(dir);
            list.AddRange(dirs);

            for (int i = 0; i < dirs.Length; i++)
            {
                GetAllDirs(dirs[i], list);
            }
        }
    }
}
#endif