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
using UnityEngine;
using System;
using System.Collections;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

using Object = UnityEngine.Object;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

namespace Blueprint.Emmy
{
    public enum MetaOp
    {
        None = 0,
        Add = 1,
        Sub = 2,
        Mul = 4,
        Div = 8,
        Eq = 16,
        Neg = 32,
        ToStr = 64,
        ALL = Add | Sub | Mul | Div | Eq | Neg | ToStr,
    }

    public enum ObjAmbig
    {
        None = 0,
        U3dObj = 1,
        NetObj = 2,
        All = 3
    }

    public static class BlueprintEmmyTLE
    {
        public static string className = string.Empty;
        public static Type type = null;
        public static Type baseType = null;

        public static bool isStaticClass = true;

        static HashSet<string> usingList = new HashSet<string>();
        static MetaOp op = MetaOp.None;
        static StringBuilder sb = null;
        static List<_MethodBase> methods = new List<_MethodBase>();
        static Dictionary<string, int> nameCounter = new Dictionary<string, int>();
        static FieldInfo[] fields = null;
        static PropertyInfo[] props = null;
        static List<PropertyInfo> propList = new List<PropertyInfo>();  //非静态属性
        static List<PropertyInfo> allProps = new List<PropertyInfo>();
        static EventInfo[] events = null;
        static List<EventInfo> eventList = new List<EventInfo>();
        static List<_MethodBase> ctorList = new List<_MethodBase>();
        static List<ConstructorInfo> ctorExtList = new List<ConstructorInfo>();
        static List<_MethodBase> getItems = new List<_MethodBase>();   //特殊属性
        static List<_MethodBase> setItems = new List<_MethodBase>();

        static BindingFlags binding = BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase;

        static ObjAmbig ambig = ObjAmbig.NetObj;
        //wrapClaaName + "Wrap" = 导出文件名，导出类名
        public static string wrapClassName = "";

        public static string libClassName = "";
        public static string extendName = "";
        public static Type extendType = null;

        public static HashSet<Type> eventSet = new HashSet<Type>();
        public static List<Type> extendList = new List<Type>();

        public static List<string> memberFilter = new List<string>
    {
        //Custom
#if UNITY_2019_1_OR_NEWER
        "MeshRenderer.receiveGI",
        "MeshRenderer.scaleInLightmap",
        "MeshRenderer.stitchLightmapSeams",
        "DefaultControls.factory",
#endif

        "String.Chars",
        "Directory.SetAccessControl",
        "File.GetAccessControl",
        "File.SetAccessControl",
        //UnityEngine
        "AnimationClip.averageDuration",
        "AnimationClip.averageAngularSpeed",
        "AnimationClip.averageSpeed",
        "AnimationClip.apparentSpeed",
        "AnimationClip.isLooping",
        "AnimationClip.isAnimatorMotion",
        "AnimationClip.isHumanMotion",
        "AnimatorOverrideController.PerformOverrideClipListCleanup",
        "AnimatorControllerParameter.name",
        "Caching.SetNoBackupFlag",
        "Caching.ResetNoBackupFlag",
        "Light.areaSize",
        "Light.lightmappingMode",
        "Light.lightmapBakeType",
        "Light.shadowAngle",
        "Light.shadowRadius",
        "Light.SetLightDirty",
        "Security.GetChainOfTrustValue",
        "Texture2D.alphaIsTransparency",
        "WWW.movie",
        "WWW.GetMovieTexture",
        "WebCamTexture.MarkNonReadable",
        "WebCamTexture.isReadable",
        "Graphic.OnRebuildRequested",
        "Text.OnRebuildRequested",
        "Resources.LoadAssetAtPath",
        "Application.ExternalEval",
        "Handheld.SetActivityIndicatorStyle",
        "CanvasRenderer.OnRequestRebuild",
        "CanvasRenderer.onRequestRebuild",
        "Terrain.bakeLightProbesForTrees",
        "MonoBehaviour.runInEditMode",
        "TextureFormat.DXT1Crunched",
        "TextureFormat.DXT5Crunched",
        "Texture.imageContentsHash",
        "QualitySettings.streamingMipmapsMaxLevelReduction",
        "QualitySettings.streamingMipmapsRenderersPerFrame",
        //NGUI
        "UIInput.ProcessEvent",
        "UIWidget.showHandlesWithMoveTool",
        "UIWidget.showHandles",
        "Input.IsJoystickPreconfigured",
        "UIDrawCall.isActive",
    "Dictionary.TryAdd",
        "KeyValuePair.Deconstruct",
        "ParticleSystem.SetJob",
        "ParticleSystem.subEmitters", /*2019.09 ios编译出错，也可能是unity版本问题*/
        "Type.IsSZArray"
    };

        class _MethodBase
        {
            public bool IsStatic
            {
                get
                {
                    return method.IsStatic;
                }
            }

            public bool IsConstructor
            {
                get
                {
                    return method.IsConstructor;
                }
            }

            public string Name
            {
                get
                {
                    return method.Name;
                }
            }

            public MethodBase Method
            {
                get
                {
                    return method;
                }
            }

            public bool IsGenericMethod
            {
                get
                {
                    return method.IsGenericMethod;
                }
            }


            MethodBase method;
            ParameterInfo[] args;

            public _MethodBase(MethodBase m, int argCount = -1)
            {
                method = m;
                ParameterInfo[] infos = m.GetParameters();
                argCount = argCount != -1 ? argCount : infos.Length;
                args = new ParameterInfo[argCount];
                Array.Copy(infos, args, argCount);
            }

            public bool BeExtend = false;
        }

        public static List<MemberInfo> memberInfoFilter = new List<MemberInfo>
        {
            //可精确查找一个函数
            //Type.GetMethod(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers);
        };

        public static bool IsMemberFilter(MemberInfo mi)
        {
            if (type.IsGenericType)
            {
                Type genericType = type.GetGenericTypeDefinition();

                if (genericType == typeof(Dictionary<,>) && mi.Name == "Remove")
                {
                    MethodBase mb = (MethodBase)mi;
                    return mb.GetParameters().Length == 2;
                }

                if (genericType == typeof(Dictionary<,>) || genericType == typeof(KeyValuePair<,>))
                {
                    string str = genericType.Name;
                    str = str.Substring(0, str.IndexOf("`"));
                    return memberFilter.Contains(str + "." + mi.Name);
                }
            }

            return memberInfoFilter.Contains(mi) || memberFilter.Contains(type.Name + "." + mi.Name);
        }

        static BlueprintEmmyTLE()
        {
        }

        //记录所有的导出类型
        public static List<Type> allTypes = new List<Type>();

        //没有未知类型的模版类型List<int> 返回false, List<T>返回true
        static bool IsGenericConstraintType(Type t)
        {
            if (!t.IsGenericType)
            {
                return t.IsGenericParameter || t == typeof(System.ValueType);
            }

            Type[] types = t.GetGenericArguments();

            for (int i = 0; i < types.Length; i++)
            {
                Type t1 = types[i];

                if (t1.IsGenericParameter || t1 == typeof(System.ValueType))
                {
                    return true;
                }

                if (IsGenericConstraintType(t1))
                {
                    return true;
                }
            }

            return false;
        }

        static bool IsGenericConstraints(Type[] constraints)
        {
            for (int i = 0; i < constraints.Length; i++)
            {
                if (!IsGenericConstraintType(constraints[i]))
                {
                    return false;
                }
            }

            return true;
        }

        static bool IsGenericMethod(MethodBase md)
        {
            if (md.IsGenericMethod)
            {
                Type[] gts = md.GetGenericArguments();
                List<ParameterInfo> list = new List<ParameterInfo>(md.GetParameters());

                for (int i = 0; i < gts.Length; i++)
                {
                    Type[] ts = gts[i].GetGenericParameterConstraints();

                    if (ts == null || ts.Length == 0 || IsGenericConstraints(ts))
                    {
                        return true;
                    }

                    ParameterInfo p = list.Find((iter) => { return iter.ParameterType == gts[i]; });

                    if (p == null)
                    {
                        return true;
                    }

                    list.RemoveAll((iter) => { return iter.ParameterType == gts[i]; });
                }

                for (int i = 0; i < list.Count; i++)
                {
                    Type t = list[i].ParameterType;

                    if (IsGenericConstraintType(t))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        static bool IsIEnumerator(Type t)
        {
            if (t == typeof(IEnumerator) || t == typeof(CharEnumerator)) return true;

            if (typeof(IEnumerator).IsAssignableFrom(t))
            {
                if (t.IsGenericType)
                {
                    Type gt = t.GetGenericTypeDefinition();

                    if (gt == typeof(List<>.Enumerator) || gt == typeof(Dictionary<,>.Enumerator))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        static bool IsNumberEnum(Type t)
        {
            if (t == typeof(BindingFlags))
            {
                return true;
            }

            return false;
        }

        //decimal 类型扔掉了
        static Dictionary<Type, int> typeSize = new Dictionary<Type, int>()
    {
        { typeof(char), 2 },
        { typeof(byte), 3 },
        { typeof(sbyte), 4 },
        { typeof(ushort),5 },
        { typeof(short), 6 },
        { typeof(uint), 7 },
        { typeof(int), 8 },
        //{ typeof(ulong), 9 },
        //{ typeof(long), 10 },
        { typeof(decimal), 11 },
        { typeof(float), 12 },
        { typeof(double), 13 },

    };

        public static string CombineTypeStr(string space, string name)
        {
            if (string.IsNullOrEmpty(space))
            {
                return name;
            }
            else
            {
                return space + "." + name;
            }
        }

        //获取类型名字
        public static string GetTypeStr(Type t)
        {
            if (t.IsByRef)
            {
                t = t.GetElementType();
                return GetTypeStr(t);
            }
            else if (t.IsArray)
            {
                string str = GetTypeStr(t.GetElementType());
                str += BlueprintEmmyLM.GetArrayRank(t);
                return str;
            }
            else if (t == extendType)
            {
                return GetTypeStr(type);
            }
            else if (IsIEnumerator(t))
            {
                return BlueprintEmmyLM.GetTypeName(typeof(IEnumerator));
            }

            return BlueprintEmmyLM.GetTypeName(t);
        }

        /*static void LuaFuncToDelegate(Type t, string head)
        {
            MethodInfo mi = t.GetMethod("Invoke");
            ParameterInfo[] pi = mi.GetParameters();
            int n = pi.Length;

            if (n == 0)
            {
                sb.AppendLineEx("() =>");

                if (mi.ReturnType == typeof(void))
                {
                    sb.AppendFormat("{0}{{\r\n{0}\tfunc.Call();\r\n{0}}};\r\n", head);
                }
                else
                {
                    sb.AppendFormat("{0}{{\r\n{0}\tfunc.BeginPCall();\r\n", head);
                    sb.AppendFormat("{0}\tfunc.PCall();\r\n", head);
                    GenLuaFunctionRetValue(sb, mi.ReturnType, head + "\t", "ret");
                    sb.AppendFormat("{0}\tfunc.EndPCall();\r\n", head);
                    sb.AppendLineEx(head + "\treturn ret;");
                    sb.AppendFormat("{0}}};\r\n", head);
                }

                return;
            }

            sb.AppendFormat("(param0");

            for (int i = 1; i < n; i++)
            {
                sb.AppendFormat(", param{0}", i);
            }

            sb.AppendFormat(") =>\r\n{0}{{\r\n{0}", head);
            sb.AppendLineEx("\tfunc.BeginPCall();");

            for (int i = 0; i < n; i++)
            {
                string push = GetPushFunction(pi[i].ParameterType);

                if (!IsParams(pi[i]))
                {
                    if (pi[i].ParameterType == typeof(byte[]) && IsByteBuffer(t))
                    {
                        sb.AppendFormat("{0}\tfunc.PushByteBuffer(param{1});\r\n", head, i);
                    }
                    else
                    {
                        sb.AppendFormat("{0}\tfunc.{1}(param{2});\r\n", head, push, i);
                    }
                }
                else
                {
                    sb.AppendLineEx();
                    sb.AppendFormat("{0}\tfor (int i = 0; i < param{1}.Length; i++)\r\n", head, i);
                    sb.AppendLineEx(head + "\t{");
                    sb.AppendFormat("{0}\t\tfunc.{1}(param{2}[i]);\r\n", head, push, i);
                    sb.AppendLineEx(head + "\t}\r\n");
                }
            }

            sb.AppendFormat("{0}\tfunc.PCall();\r\n", head);

            if (mi.ReturnType == typeof(void))
            {
                for (int i = 0; i < pi.Length; i++)
                {
                    if ((pi[i].Attributes & ParameterAttributes.Out) != ParameterAttributes.None)
                    {
                        GenLuaFunctionRetValue(sb, pi[i].ParameterType, head + "\t", "param" + i, true);
                    }
                }

                sb.AppendFormat("{0}\tfunc.EndPCall();\r\n", head);
            }
            else
            {
                GenLuaFunctionRetValue(sb, mi.ReturnType, head + "\t", "ret");

                for (int i = 0; i < pi.Length; i++)
                {
                    if ((pi[i].Attributes & ParameterAttributes.Out) != ParameterAttributes.None)
                    {
                        GenLuaFunctionRetValue(sb, pi[i].ParameterType, head + "\t", "param" + i, true);
                    }
                }

                sb.AppendFormat("{0}\tfunc.EndPCall();\r\n", head);
                sb.AppendLineEx(head + "\treturn ret;");
            }

            sb.AppendFormat("{0}}};\r\n", head);
        }*/

        //static void GenToStringFunction()
        //{
        //    if ((op & MetaOp.ToStr) == 0)
        //    {
        //        return;
        //    }

        //    sb.AppendLineEx("\r\n\t[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]");
        //    sb.AppendLineEx("\tstatic int Lua_ToString(IntPtr L)");
        //    sb.AppendLineEx("\t{");
        //    sb.AppendLineEx("\t\tobject obj = ToLua.ToObject(L, 1);\r\n");

        //    sb.AppendLineEx("\t\tif (obj != null)");
        //    sb.AppendLineEx("\t\t{");
        //    sb.AppendLineEx("\t\t\tLuaDLL.lua_pushstring(L, obj.ToString());");
        //    sb.AppendLineEx("\t\t}");
        //    sb.AppendLineEx("\t\telse");
        //    sb.AppendLineEx("\t\t{");
        //    sb.AppendLineEx("\t\t\tLuaDLL.lua_pushnil(L);");
        //    sb.AppendLineEx("\t\t}");
        //    sb.AppendLineEx();
        //    sb.AppendLineEx("\t\treturn 1;");
        //    sb.AppendLineEx("\t}");
        //}

        static string CreateDelegate = @"
    public static Delegate CreateDelegate(Type t, LuaFunction func = null)
    {
        DelegateCreate Create = null;

        if (!dict.TryGetValue(t, out Create))
        {
            throw new LuaException(string.Format(""Delegate {0} not register"", LuaMisc.GetTypeName(t)));
        }

        if (func != null)
        {
            LuaState state = func.GetLuaState();
            LuaDelegate target = state.GetLuaDelegate(func);

            if (target != null)
            {
                return Delegate.CreateDelegate(t, target, target.method);
            }
            else
            {
                Delegate d = Create(func, null, false);
                target = d.Target as LuaDelegate;
                state.AddLuaDelegate(target, func);
                return d;
            }
        }

        return Create(null, null, false);
    }

    public static Delegate CreateDelegate(Type t, LuaFunction func, LuaTable self)
    {
        DelegateCreate Create = null;

        if (!dict.TryGetValue(t, out Create))
        {
            throw new LuaException(string.Format(""Delegate {0} not register"", LuaMisc.GetTypeName(t)));
        }

        if (func != null)
        {
            LuaState state = func.GetLuaState();
            LuaDelegate target = state.GetLuaDelegate(func, self);

            if (target != null)
            {
                return Delegate.CreateDelegate(t, target, target.method);
            }
            else
            {
                Delegate d = Create(func, self, true);
                target = d.Target as LuaDelegate;
                state.AddLuaDelegate(target, func, self);
                return d;
            }
        }

        return Create(null, null, true);
    }
";

        static string RemoveDelegate = @"
    public static Delegate RemoveDelegate(Delegate obj, LuaFunction func)
    {
        LuaState state = func.GetLuaState();
        Delegate[] ds = obj.GetInvocationList();

        for (int i = 0; i < ds.Length; i++)
        {
            LuaDelegate ld = ds[i].Target as LuaDelegate;

            if (ld != null && ld.func == func)
            {
                obj = Delegate.Remove(obj, ds[i]);
                state.DelayDispose(ld.func);
                break;
            }
        }

        return obj;
    }

    public static Delegate RemoveDelegate(Delegate obj, Delegate dg)
    {
        LuaDelegate remove = dg.Target as LuaDelegate;

        if (remove == null)
        {
            obj = Delegate.Remove(obj, dg);
            return obj;
        }

        LuaState state = remove.func.GetLuaState();
        Delegate[] ds = obj.GetInvocationList();

        for (int i = 0; i < ds.Length; i++)
        {
            LuaDelegate ld = ds[i].Target as LuaDelegate;

            if (ld != null && ld == remove)
            {
                obj = Delegate.Remove(obj, ds[i]);
                state.DelayDispose(ld.func);
                state.DelayDispose(ld.self);
                break;
            }
        }

        return obj;
    }
";

        static string RemoveChar(string str, char c)
        {
            int index = str.IndexOf(c);

            while (index > 0)
            {
                str = str.Remove(index, 1);
                index = str.IndexOf(c);
            }

            return str;
        }

        public static string ConvertToLibSign(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }

            str = str.Replace('<', '_');
            str = RemoveChar(str, '>');
            str = str.Replace('[', 's');
            str = RemoveChar(str, ']');
            str = str.Replace('.', '_');
            return str.Replace(',', '_');
        }

        public static string GetNameSpace(Type t, out string libName)
        {
            if (t.IsGenericType)
            {
                return GetGenericNameSpace(t, out libName);
            }
            else
            {
                string space = t.FullName;

                if (space.Contains("+"))
                {
                    space = space.Replace('+', '.');
                    int index = space.LastIndexOf('.');
                    libName = space.Substring(index + 1);
                    return space.Substring(0, index);
                }
                else
                {
                    libName = t.Namespace == null ? space : space.Substring(t.Namespace.Length + 1);
                    return t.Namespace;
                }
            }
        }

        static string GetGenericNameSpace(Type t, out string libName)
        {
            Type[] gArgs = t.GetGenericArguments();
            string typeName = t.FullName;
            int count = gArgs.Length;
            int pos = typeName.IndexOf("[");
            typeName = typeName.Substring(0, pos);

            string str = null;
            string name = null;
            int offset = 0;
            pos = typeName.IndexOf("+");

            while (pos > 0)
            {
                str = typeName.Substring(0, pos);
                typeName = typeName.Substring(pos + 1);
                pos = str.IndexOf('`');

                if (pos > 0)
                {
                    count = (int)(str[pos + 1] - '0');
                    str = str.Substring(0, pos);
                    str += "<" + string.Join(",", BlueprintEmmyLM.GetGenericName(gArgs, offset, count)) + ">";
                    offset += count;
                }

                name = CombineTypeStr(name, str);
                pos = typeName.IndexOf("+");
            }

            string space = name;
            str = typeName;

            if (offset < gArgs.Length)
            {
                pos = str.IndexOf('`');
                count = (int)(str[pos + 1] - '0');
                str = str.Substring(0, pos);
                str += "<" + string.Join(",", BlueprintEmmyLM.GetGenericName(gArgs, offset, count)) + ">";
            }

            libName = str;

            if (string.IsNullOrEmpty(space))
            {
                space = t.Namespace;

                if (space != null)
                {
                    libName = str.Substring(space.Length + 1);
                }
            }

            return space;
        }

        static Type GetParameterType(ParameterInfo info)
        {
            if (info.ParameterType == extendType)
            {
                return type;
            }

            return info.ParameterType;
        }
    }
}
#endif