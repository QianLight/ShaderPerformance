using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Blueprint.CSharpReflection
{
    
    public class CSharpAPIGenerator
    {
        [InitializeOnLoadMethod]
        static void Generate() {
            DoIt();
        }

        [MenuItem("Blueprint/Gen API for C# Reflection")]
        static public void DoIt()
        {
            Filter<Type> baseFilter = new GeneralFilter<Type>(ToLuaMenu.baseType);
            Filter<Type> dropFilter = new GeneralFilter<Type>(ToLuaMenu.dropType);
            var directory = Directory.CreateDirectory("CSharpAPI");
            DeleteAPIFiles(directory);
            //var collection = new BindTypeCollection(CSharpReflectionSettings.customTypeList);
            var collection = new BindTypeCollection(GetTrueCustomTypeList());
            var types = collection.CollectBindType(baseFilter, dropFilter);
            foreach (var t in types)
            {
                var generator = new CSharpGenerator();
                generator.Gen(t);
            }
            Debug.LogFormat("API 生成完毕. {0}", directory.FullName);
        }

        public static void DoIt(BindType[] bindTypes)
        {
            Filter<Type> baseFilter = new GeneralFilter<Type>(ToLuaMenu.baseType);
            Filter<Type> dropFilter = new GeneralFilter<Type>(ToLuaMenu.dropType);
            var directory = Directory.CreateDirectory("CSharpAPI");
            DeleteAPIFiles(directory);
            var collection = new BindTypeCollection(bindTypes);
            var types = collection.CollectBindType(baseFilter, dropFilter);
            foreach (var t in types)
            {
                var generator = new CSharpGenerator();
                generator.Gen(t);
            }
            Debug.LogFormat("定制API 生成完毕. {0}", directory.FullName);
        }

        static BindType[] GetTrueCustomTypeList()
        {
            HashSet<string> typeNames = new HashSet<string>();
            HashSet<BindType> bindTypeSet = new HashSet<BindType>();
            foreach(var t in CSharpReflectionSettings.customClassList)
            {
                string fullName = $"{t.nameSpace}.{t.name}";
                if (!typeNames.Contains(fullName))
                {
                    typeNames.Add(fullName);
                    bindTypeSet.Add(t);
                }
            }
            foreach (var t in CSharpCustomReflectionSettings.customClassList)
            {
                string fullName = $"{t.nameSpace}.{t.name}";
                if (!typeNames.Contains(fullName))
                {
                    typeNames.Add(fullName);
                    bindTypeSet.Add(t);
                }
            }
            Assembly csharpAssembly = Assembly.GetAssembly(typeof(Blueprint.ParamTypeAttribute));
            bool hasSubclass = false;
            foreach (var t in CSharpCustomReflectionSettings.customTypeAutoSubclassInAssemblyCSharp)
            {
                string fullName = $"{t.nameSpace}.{t.name}";
                if (!typeNames.Contains(fullName))
                {
                    typeNames.Add(fullName);
                    bindTypeSet.Add(t);
                }
                // if (!t.type.Assembly.Equals(csharpAssembly))
                //     continue;
                hasSubclass = true;
            }

            foreach (var item in CSharpCustomReflectionSettings.customStructList)
            {
                string fullName = $"{item.nameSpace}.{item.name}";
                if (!typeNames.Contains(fullName))
                {
                    typeNames.Add(fullName);
                    bindTypeSet.Add(item);
                }
            }

            foreach (var item in CSharpCustomReflectionSettings.customEnumList)
            {
                string fullName = $"{item.nameSpace}.{item.name}";
                if (!typeNames.Contains(fullName))
                {
                    typeNames.Add(fullName);
                    bindTypeSet.Add(item);
                }
            }

            if (hasSubclass)
            {
                Type[] types = csharpAssembly.GetTypes();
                Dictionary<Type, List<Type>> typeTree = new Dictionary<Type, List<Type>>();
                foreach(var t in types)
                {
                    if (t.BaseType == null) continue;

                    if (!t.BaseType.Assembly.Equals(csharpAssembly) && !bindTypeSet.Select(x => x.type).Contains(t.BaseType))
                        continue;
                    if (!typeTree.ContainsKey(t.BaseType))
                        typeTree[t.BaseType] = new List<Type>();
                    typeTree[t.BaseType].Add(t);
                }
                foreach (var t in CSharpCustomReflectionSettings.customTypeAutoSubclassInAssemblyCSharp)
                {
                    if(typeTree.ContainsKey(t.type))
                    {
                        foreach(var child in typeTree[t.type])
                        {
                            var newBindType = new BindType(child);
                            string fullName = $"{newBindType.nameSpace}.{newBindType.name}";
                            if (!typeNames.Contains(fullName))
                            {
                                typeNames.Add(fullName);
                                bindTypeSet.Add(newBindType);
                            }
                        }
                    }
                }
            }
            return bindTypeSet.ToArray();
        }

        private static void DeleteAPIFiles(DirectoryInfo directory)
        {
            List<string> removeList = new List<string>();
            foreach (var file in directory.GetFiles())
            {
                removeList.Add(file.FullName);
            }
            foreach (var fileName in removeList)
            {
                File.Delete(fileName);
            }
        }
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
        public int platform = 1;

        public List<Type> extendList = new List<Type>();

        public BindType(Type t)
        {
            if (typeof(System.MulticastDelegate).IsAssignableFrom(t))
            {
                throw new NotSupportedException(string.Format("\nDon't export Delegate {0} as a class, register it in customDelegateList", LuaMisc.GetTypeName(t)));
            }

            //if (IsObsolete(t))
            //{
            //    throw new Exception(string.Format("\n{0} is obsolete, don't export it!", LuaMisc.GetTypeName(t)));
            //}

            type = t;
            nameSpace = ToLuaExport.GetNameSpace(t, out libName);
            name = ToLuaExport.CombineTypeStr(nameSpace, libName);
            libName = ToLuaExport.ConvertToLibSign(libName);

            var classPlatformInfo = t.GetCustomAttribute<ClassPlatformAttribute>();
            if (classPlatformInfo != null)
                this.platform = classPlatformInfo.platform;

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
                wrapName = ToLuaExport.ConvertToLibSign(wrapName);
            }

            int index = CSharpReflectionSettings.staticClassTypes.IndexOf(type);

            if (index >= 0 || (type.IsAbstract && type.IsSealed))
            {
                IsStatic = true;
            }

            baseType = LuaMisc.GetExportBaseType(type);
        }
    }

    public static class ToLuaExport
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
        static System.Reflection.FieldInfo[] fields = null;
        static System.Reflection.PropertyInfo[] props = null;
        static List<System.Reflection.PropertyInfo> propList = new List<System.Reflection.PropertyInfo>();  //非静态属性
        static List<System.Reflection.PropertyInfo> allProps = new List<System.Reflection.PropertyInfo>();
        static System.Reflection.EventInfo[] events = null;
        static List<System.Reflection.EventInfo> eventList = new List<System.Reflection.EventInfo>();
        static List<_MethodBase> ctorList = new List<_MethodBase>();
        static List<System.Reflection.ConstructorInfo> ctorExtList = new List<System.Reflection.ConstructorInfo>();
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
        "Type.IsSZArray",
        "Net.WriteStreamClosedEventHandler",
        "MeshRenderer.receiveGI",
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
            System.Reflection.ParameterInfo[] args;

            public _MethodBase(MethodBase m, int argCount = -1)
            {
                method = m;
                System.Reflection.ParameterInfo[] infos = m.GetParameters();
                argCount = argCount != -1 ? argCount : infos.Length;
                args = new System.Reflection.ParameterInfo[argCount];
                Array.Copy(infos, args, argCount);
            }

            public System.Reflection.ParameterInfo[] GetParameters()
            {
                return args;
            }

            public int GetParamsCount()
            {
                int c = method.IsStatic ? 0 : 1;
                return args.Length + c;
            }

            public int GetEqualParamsCount(_MethodBase b)
            {
                int count = 0;
                List<Type> list1 = new List<Type>();
                List<Type> list2 = new List<Type>();

                if (!IsStatic)
                {
                    list1.Add(type);
                }

                if (!b.IsStatic)
                {
                    list2.Add(type);
                }

                for (int i = 0; i < args.Length; i++)
                {
                    list1.Add(GetParameterType(args[i]));
                }

                System.Reflection.ParameterInfo[] p = b.args;

                for (int i = 0; i < p.Length; i++)
                {
                    list2.Add(GetParameterType(p[i]));
                }

                for (int i = 0; i < list1.Count; i++)
                {
                    if (list1[i] != list2[i])
                    {
                        break;
                    }

                    ++count;
                }

                return count;
            }

            public string GenParamTypes(int offset = 0)
            {
                StringBuilder sb = new StringBuilder();
                List<Type> list = new List<Type>();

                if (!method.IsStatic)
                {
                    list.Add(type);
                }

                for (int i = 0; i < args.Length; i++)
                {
                    if (IsParams(args[i]))
                    {
                        continue;
                    }

                    if (args[i].Attributes != ParameterAttributes.Out)
                    {
                        list.Add(GetGenericBaseType(method, args[i].ParameterType));
                    }
                    else
                    {
                        Type genericClass = typeof(LuaOut<>);
                        Type t = genericClass.MakeGenericType(args[i].ParameterType.GetElementType());
                        list.Add(t);
                    }
                }

                for (int i = offset; i < list.Count - 1; i++)
                {
                    sb.Append(GetTypeOf(list[i], ", "));
                }

                if (list.Count > 0)
                {
                    sb.Append(GetTypeOf(list[list.Count - 1], ""));
                }

                return sb.ToString();
            }

            public bool HasSetIndex()
            {
                if (method.Name == "set_Item")
                {
                    return true;
                }

                object[] attrs = type.GetCustomAttributes(true);

                for (int i = 0; i < attrs.Length; i++)
                {
                    if (attrs[i] is DefaultMemberAttribute)
                    {
                        return method.Name == "set_ItemOf";
                    }
                }

                return false;
            }

            public bool HasGetIndex()
            {
                if (method.Name == "get_Item")
                {
                    return true;
                }

                object[] attrs = type.GetCustomAttributes(true);

                for (int i = 0; i < attrs.Length; i++)
                {
                    if (attrs[i] is DefaultMemberAttribute)
                    {
                        return method.Name == "get_ItemOf";
                    }
                }

                return false;
            }

            public Type GetReturnType()
            {
                System.Reflection.MethodInfo m = method as System.Reflection.MethodInfo;

                if (m != null)
                {
                    return m.ReturnType;
                }

                return null;
            }

            public string GetTotalName()
            {
                string[] ss = new string[args.Length];

                for (int i = 0; i < args.Length; i++)
                {
                    ss[i] = GetTypeStr(args[i].GetType());
                }

                if (!ToLuaExport.IsGenericMethod(method))
                {
                    return Name + "(" + string.Join(",", ss) + ")";
                }
                else
                {
                    Type[] gts = method.GetGenericArguments();
                    string[] ts = new string[gts.Length];

                    for (int i = 0; i < gts.Length; i++)
                    {
                        ts[i] = GetTypeStr(gts[i]);
                    }

                    return Name + "<" + string.Join(",", ts) + ">" + "(" + string.Join(",", ss) + ")";
                }
            }

            public bool BeExtend = false;

            bool IsByteBuffer()
            {
                object[] attrs = method.GetCustomAttributes(true);

                for (int j = 0; j < attrs.Length; j++)
                {
                    Type t = attrs[j].GetType();

                    if (t == typeof(LuaByteBufferAttribute))
                    {
                        return true;
                    }
                }

                return false;
            }
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
                if (genericType == typeof(HashSet<>) && mi.Name == "TryGetValue")
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

        public static bool IsMemberFilter(Type t)
        {
            string name = LuaMisc.GetTypeName(t);
            return memberInfoFilter.Contains(t) || memberFilter.Find((p) => { return name.Contains(p); }) != null;
        }

        static ToLuaExport()
        {
            //Debugger.useLog = true;
        }

        public static void Clear()
        {
            className = null;
            type = null;
            baseType = null;
            isStaticClass = false;
            usingList.Clear();
            op = MetaOp.None;
            sb = new StringBuilder();
            fields = null;
            props = null;
            methods.Clear();
            allProps.Clear();
            propList.Clear();
            eventList.Clear();
            ctorList.Clear();
            ctorExtList.Clear();
            ambig = ObjAmbig.NetObj;
            wrapClassName = "";
            libClassName = "";
            extendName = "";
            eventSet.Clear();
            extendType = null;
            nameCounter.Clear();
            events = null;
            getItems.Clear();
            setItems.Clear();
        }

        private static MetaOp GetOp(string name)
        {
            if (name == "op_Addition")
            {
                return MetaOp.Add;
            }
            else if (name == "op_Subtraction")
            {
                return MetaOp.Sub;
            }
            else if (name == "op_Equality")
            {
                return MetaOp.Eq;
            }
            else if (name == "op_Multiply")
            {
                return MetaOp.Mul;
            }
            else if (name == "op_Division")
            {
                return MetaOp.Div;
            }
            else if (name == "op_UnaryNegation")
            {
                return MetaOp.Neg;
            }
            else if (name == "ToString" && !isStaticClass)
            {
                return MetaOp.ToStr;
            }
            else if (name == "op_LessThanOrEqual")
            {
                return MetaOp.Le;
            }
            else if (name == "op_GreaterThanOrEqual")
            {
                return MetaOp.Lt;
            }

            return MetaOp.None;
        }

        //操作符函数无法通过继承metatable实现
        static void GenBaseOpFunction(List<_MethodBase> list)
        {
            Type baseType = type.BaseType;

            while (baseType != null)
            {
                if (allTypes.IndexOf(baseType) >= 0)
                {
                    System.Reflection.MethodInfo[] methods = baseType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);

                    for (int i = 0; i < methods.Length; i++)
                    {
                        MetaOp baseOp = GetOp(methods[i].Name);

                        if (baseOp != MetaOp.None && (op & baseOp) == 0)
                        {
                            if (baseOp != MetaOp.ToStr)
                            {
                                list.Add(new _MethodBase(methods[i]));
                            }

                            op |= baseOp;
                        }
                    }
                }

                baseType = baseType.BaseType;
            }
        }

        //记录所有的导出类型
        public static List<Type> allTypes = new List<Type>();

        static bool BeDropMethodType(System.Reflection.MethodInfo md)
        {
            Type t = md.DeclaringType;

            if (t == type)
            {
                return true;
            }

            return allTypes.IndexOf(t) < 0;
        }

        //是否为委托类型，没处理废弃
        public static bool IsDelegateType(Type t)
        {
            if (!typeof(System.MulticastDelegate).IsAssignableFrom(t) || t == typeof(System.MulticastDelegate))
            {
                return false;
            }

            if (IsMemberFilter(t))
            {
                return false;
            }

            return true;
        }

        static void InitPropertyList()
        {
            props = type.GetProperties(BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance | binding);
            propList.AddRange(type.GetProperties(BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase));
            fields = type.GetFields(BindingFlags.GetField | BindingFlags.SetField | BindingFlags.Instance | binding);
            events = type.GetEvents(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);
            eventList.AddRange(type.GetEvents(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public));

            List<System.Reflection.FieldInfo> fieldList = new List<System.Reflection.FieldInfo>();
            fieldList.AddRange(fields);

            for (int i = fieldList.Count - 1; i >= 0; i--)
            {
                if (IsObsolete(fieldList[i]))
                {
                    fieldList.RemoveAt(i);
                }
                else if (IsDelegateType(fieldList[i].FieldType))
                {
                    eventSet.Add(fieldList[i].FieldType);
                }
            }

            fields = fieldList.ToArray();

            List<System.Reflection.PropertyInfo> piList = new List<System.Reflection.PropertyInfo>();
            piList.AddRange(props);

            for (int i = piList.Count - 1; i >= 0; i--)
            {
                if (IsObsolete(piList[i]))
                {
                    piList.RemoveAt(i);
                }
                else if (piList[i].Name == "Item" && IsItemThis(piList[i]))
                {
                    piList.RemoveAt(i);
                }
                else if (piList[i].GetGetMethod() != null && HasGetIndex(piList[i].GetGetMethod()))
                {
                    piList.RemoveAt(i);
                }
                else if (piList[i].GetSetMethod() != null && HasSetIndex(piList[i].GetSetMethod()))
                {
                    piList.RemoveAt(i);
                }
                else if (IsDelegateType(piList[i].PropertyType))
                {
                    eventSet.Add(piList[i].PropertyType);
                }
            }

            props = piList.ToArray();

            for (int i = propList.Count - 1; i >= 0; i--)
            {
                if (IsObsolete(propList[i]))
                {
                    propList.RemoveAt(i);
                }
            }

            allProps.AddRange(props);
            allProps.AddRange(propList);

            List<System.Reflection.EventInfo> evList = new List<System.Reflection.EventInfo>();
            evList.AddRange(events);

            for (int i = evList.Count - 1; i >= 0; i--)
            {
                if (IsObsolete(evList[i]))
                {
                    evList.RemoveAt(i);
                }
                else if (IsDelegateType(evList[i].EventHandlerType))
                {
                    eventSet.Add(evList[i].EventHandlerType);
                }
            }

            events = evList.ToArray();

            for (int i = eventList.Count - 1; i >= 0; i--)
            {
                if (IsObsolete(eventList[i]))
                {
                    eventList.RemoveAt(i);
                }
            }
        }

        static string GetMethodName(MethodBase md)
        {
            if (md.Name.StartsWith("op_"))
            {
                return md.Name;
            }

            object[] attrs = md.GetCustomAttributes(true);

            for (int i = 0; i < attrs.Length; i++)
            {
                if (attrs[i] is LuaRenameAttribute)
                {
                    LuaRenameAttribute attr = attrs[i] as LuaRenameAttribute;
                    return attr.Name;
                }
            }

            return md.Name;
        }

        static bool HasGetIndex(MemberInfo md)
        {
            if (md.Name == "get_Item")
            {
                return true;
            }

            object[] attrs = type.GetCustomAttributes(true);

            for (int i = 0; i < attrs.Length; i++)
            {
                if (attrs[i] is DefaultMemberAttribute)
                {
                    return md.Name == "get_ItemOf";
                }
            }

            return false;
        }

        static bool HasSetIndex(MemberInfo md)
        {
            if (md.Name == "set_Item")
            {
                return true;
            }

            object[] attrs = type.GetCustomAttributes(true);

            for (int i = 0; i < attrs.Length; i++)
            {
                if (attrs[i] is DefaultMemberAttribute)
                {
                    return md.Name == "set_ItemOf";
                }
            }

            return false;
        }

        static bool IsThisArray(MethodBase md, int count)
        {
            System.Reflection.ParameterInfo[] pis = md.GetParameters();

            if (pis.Length != count)
            {
                return false;
            }

            if (pis[0].ParameterType == typeof(int))
            {
                return true;
            }

            return false;
        }

        static bool IsItemThis(System.Reflection.PropertyInfo info)
        {
            System.Reflection.MethodInfo md = info.GetGetMethod();

            if (md != null)
            {
                return md.GetParameters().Length != 0;
            }

            md = info.GetSetMethod();

            if (md != null)
            {
                return md.GetParameters().Length != 1;
            }

            return true;
        }

        static void GenRegisterVariables()
        {
            if (fields.Length == 0 && props.Length == 0 && events.Length == 0 && isStaticClass && baseType == null)
            {
                return;
            }

            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].IsLiteral || fields[i].IsPrivate || fields[i].IsInitOnly)
                {
                    if (fields[i].IsLiteral && fields[i].FieldType.IsPrimitive && !fields[i].FieldType.IsEnum)
                    {
                        double d = Convert.ToDouble(fields[i].GetValue(null));
                        sb.AppendFormat("\t\tL.RegConstant(\"{0}\", {1});\r\n", fields[i].Name, d);
                    }
                    else
                    {
                        sb.AppendFormat("\t\tL.RegVar(\"{0}\", new LuaCSFunction(get_{0}), null);\r\n", fields[i].Name);
                    }
                }
                else
                {
                    sb.AppendFormat("\t\tL.RegVar(\"{0}\", new LuaCSFunction(get_{0}), new LuaCSFunction(set_{0}));\r\n", fields[i].Name);
                }
            }

            for (int i = 0; i < props.Length; i++)
            {
                if (props[i].CanRead && props[i].CanWrite && props[i].GetSetMethod(true).IsPublic)
                {
                    _MethodBase md = methods.Find((p) => { return p.Name == "get_" + props[i].Name; });
                    string get = md == null ? "get" : "_get";
                    md = methods.Find((p) => { return p.Name == "set_" + props[i].Name; });
                    string set = md == null ? "set" : "_set";
                    sb.AppendFormat("\t\tL.RegVar(\"{0}\", new LuaCSFunction({1}_{0}), new LuaCSFunction({2}_{0}));\r\n", props[i].Name, get, set);
                }
                else if (props[i].CanRead)
                {
                    _MethodBase md = methods.Find((p) => { return p.Name == "get_" + props[i].Name; });
                    sb.AppendFormat("\t\tL.RegVar(\"{0}\", new LuaCSFunction({1}_{0}), null);\r\n", props[i].Name, md == null ? "get" : "_get");
                }
                else if (props[i].CanWrite)
                {
                    _MethodBase md = methods.Find((p) => { return p.Name == "set_" + props[i].Name; });
                    sb.AppendFormat("\t\tL.RegVar(\"{0}\", null, new LuaCSFunction({1}_{0}));\r\n", props[i].Name, md == null ? "set" : "_set");
                }
            }

            for (int i = 0; i < events.Length; i++)
            {
                sb.AppendFormat("\t\tL.RegVar(\"{0}\", new LuaCSFunction(get_{0}), new LuaCSFunction(set_{0}));\r\n", events[i].Name);
            }
        }

        static bool IsParams(System.Reflection.ParameterInfo param)
        {
            return param.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0;
        }

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
                List<System.Reflection.ParameterInfo> list = new List<System.Reflection.ParameterInfo>(md.GetParameters());

                for (int i = 0; i < gts.Length; i++)
                {
                    Type[] ts = gts[i].GetGenericParameterConstraints();

                    if (ts == null || ts.Length == 0 || IsGenericConstraints(ts))
                    {
                        return true;
                    }

                    System.Reflection.ParameterInfo p = list.Find((iter) => { return iter.ParameterType == gts[i]; });

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

        static bool IsNotCheckGeneric(Type t)
        {
            if (t.IsEnum || t.IsValueType)
            {
                return true;
            }

            if (t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(List<>) || t.GetGenericTypeDefinition() == typeof(Dictionary<,>)))
            {
                return true;
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

        static string GetCountStr(int count)
        {
            if (count != 0)
            {
                return string.Format("count - {0}", count);
            }

            return "count";
        }

        static int GetDefalutParamCount(MethodBase md)
        {
            int count = 0;
            System.Reflection.ParameterInfo[] infos = md.GetParameters();

            for (int i = 0; i < infos.Length; i++)
            {
                if (!(infos[i].DefaultValue is DBNull))
                {
                    ++count;
                }
            }

            return count;
        }

        static void InitCtorList()
        {
            if (isStaticClass || type.IsAbstract || typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                return;
            }

            System.Reflection.ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Instance | binding);

            if (extendType != null)
            {
                System.Reflection.ConstructorInfo[] ctorExtends = extendType.GetConstructors(BindingFlags.Instance | binding);

                if (HasAttribute(ctorExtends[0], typeof(UseDefinedAttribute)))
                {
                    ctorExtList.AddRange(ctorExtends);
                }
            }

            if (constructors.Length == 0)
            {
                return;
            }

            bool isGenericType = type.IsGenericType;
            Type genericType = isGenericType ? type.GetGenericTypeDefinition() : null;
            Type dictType = typeof(Dictionary<,>);
            Type hashType = typeof(HashSet<>);

            for (int i = 0; i < constructors.Length; i++)
            {
                if (IsObsolete(constructors[i]))
                {
                    continue;
                }

                int count = GetDefalutParamCount(constructors[i]);
                int length = constructors[i].GetParameters().Length;

                if (genericType == dictType && length >= 1)
                {
                    Type pt = constructors[i].GetParameters()[0].ParameterType;

                    if (pt.IsGenericType && pt.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IEnumerable<>))
                    {
                        continue;
                    }
                }
                else if (genericType == hashType && length >= 1)
                {
                    Type pt = constructors[i].GetParameters()[0].ParameterType;

                    if (pt == typeof(int))
                    {
                        continue;
                    }
                }

                for (int j = 0; j < count + 1; j++)
                {
                    _MethodBase r = new _MethodBase(constructors[i], length - j);
                    int index = ctorList.FindIndex((p) => { return CompareMethod(p, r) >= 0; });

                    if (index >= 0)
                    {
                        if (CompareMethod(ctorList[index], r) == 2)
                        {
                            ctorList.RemoveAt(index);
                            ctorList.Add(r);
                        }
                    }
                    else
                    {
                        ctorList.Add(r);
                    }
                }
            }
        }

        static int GetOptionalParamPos(System.Reflection.ParameterInfo[] infos)
        {
            for (int i = 0; i < infos.Length; i++)
            {
                if (IsParams(infos[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        static bool Is64bit(Type t)
        {
            return t == typeof(long) || t == typeof(ulong);
        }

        static int Compare(_MethodBase lhs, _MethodBase rhs)
        {
            int off1 = lhs.IsStatic ? 0 : 1;
            int off2 = rhs.IsStatic ? 0 : 1;

            System.Reflection.ParameterInfo[] lp = lhs.GetParameters();
            System.Reflection.ParameterInfo[] rp = rhs.GetParameters();

            int pos1 = GetOptionalParamPos(lp);
            int pos2 = GetOptionalParamPos(rp);

            if (pos1 >= 0 && pos2 < 0)
            {
                return 1;
            }
            else if (pos1 < 0 && pos2 >= 0)
            {
                return -1;
            }
            else if (pos1 >= 0 && pos2 >= 0)
            {
                pos1 += off1;
                pos2 += off2;

                if (pos1 != pos2)
                {
                    return pos1 > pos2 ? -1 : 1;
                }
                else
                {
                    pos1 -= off1;
                    pos2 -= off2;

                    if (lp[pos1].ParameterType.GetElementType() == typeof(object) && rp[pos2].ParameterType.GetElementType() != typeof(object))
                    {
                        return 1;
                    }
                    else if (lp[pos1].ParameterType.GetElementType() != typeof(object) && rp[pos2].ParameterType.GetElementType() == typeof(object))
                    {
                        return -1;
                    }
                }
            }

            int c1 = off1 + lp.Length;
            int c2 = off2 + rp.Length;

            if (c1 > c2)
            {
                return 1;
            }
            else if (c1 == c2)
            {
                List<System.Reflection.ParameterInfo> list1 = new List<System.Reflection.ParameterInfo>(lp);
                List<System.Reflection.ParameterInfo> list2 = new List<System.Reflection.ParameterInfo>(rp);

                if (list1.Count > list2.Count)
                {
                    if (list1[0].ParameterType == typeof(object))
                    {
                        return 1;
                    }
                    else if (list1[0].ParameterType.IsPrimitive)
                    {
                        return -1;
                    }

                    list1.RemoveAt(0);
                }
                else if (list2.Count > list1.Count)
                {
                    if (list2[0].ParameterType == typeof(object))
                    {
                        return -1;
                    }
                    else if (list2[0].ParameterType.IsPrimitive)
                    {
                        return 1;
                    }

                    list2.RemoveAt(0);
                }

                for (int i = 0; i < list1.Count; i++)
                {
                    if (list1[i].ParameterType == typeof(object) && list2[i].ParameterType != typeof(object))
                    {
                        return 1;
                    }
                    else if (list1[i].ParameterType != typeof(object) && list2[i].ParameterType == typeof(object))
                    {
                        return -1;
                    }
                    else if (list1[i].ParameterType.IsPrimitive && !list2[i].ParameterType.IsPrimitive)
                    {
                        return -1;
                    }
                    else if (!list1[i].ParameterType.IsPrimitive && list2[i].ParameterType.IsPrimitive)
                    {
                        return 1;
                    }
                    else if (list1[i].ParameterType.IsPrimitive && list2[i].ParameterType.IsPrimitive)
                    {
                        if (Is64bit(list1[i].ParameterType) && !Is64bit(list2[i].ParameterType))
                        {
                            return 1;
                        }
                        else if (!Is64bit(list1[i].ParameterType) && Is64bit(list2[i].ParameterType))
                        {
                            return -1;
                        }
                        else if (Is64bit(list1[i].ParameterType) && Is64bit(list2[i].ParameterType) && list1[i].ParameterType != list2[i].ParameterType)
                        {
                            if (list1[i].ParameterType == typeof(ulong))
                            {
                                return 1;
                            }

                            return -1;
                        }
                    }
                }

                return 0;
            }
            else
            {
                return -1;
            }
        }

        static bool HasOptionalParam(System.Reflection.ParameterInfo[] infos)
        {
            for (int i = 0; i < infos.Length; i++)
            {
                if (IsParams(infos[i]))
                {
                    return true;
                }
            }

            return false;
        }

        static void CheckObject(string head, Type type, string className, int pos)
        {
            if (type == typeof(object))
            {
                sb.AppendFormat("{0}object obj = ToLua.CheckObject(L, {1});\r\n", head, pos);
            }
            else if (type == typeof(Type))
            {
                sb.AppendFormat("{0}{1} obj = ToLua.CheckMonoType(L, {2});\r\n", head, className, pos);
            }
            else if (IsIEnumerator(type))
            {
                sb.AppendFormat("{0}{1} obj = ToLua.CheckIter(L, {2});\r\n", head, className, pos);
            }
            else
            {
                if (IsNotCheckGeneric(type))
                {
                    sb.AppendFormat("{0}{1} obj = ({1})ToLua.CheckObject(L, {2}, TypeTraits<{1}>.type);\r\n", head, className, pos);
                }
                else
                {
                    sb.AppendFormat("{0}{1} obj = ({1})ToLua.CheckObject<{1}>(L, {2});\r\n", head, className, pos);
                }
            }
        }

        static void ToObject(string head, Type type, string className, int pos)
        {
            if (type == typeof(object))
            {
                sb.AppendFormat("{0}object obj = ToLua.ToObject(L, {1});\r\n", head, pos);
            }
            else
            {
                sb.AppendFormat("{0}{1} obj = ({1})ToLua.ToObject(L, {2});\r\n", head, className, pos);
            }
        }

        static Type GetRefBaseType(Type argType)
        {
            if (argType.IsByRef)
            {
                return argType.GetElementType();
            }

            return argType;
        }

        static int GetMethodType(MethodBase md, out System.Reflection.PropertyInfo pi)
        {
            pi = null;

            if (!md.IsSpecialName)
            {
                return 0;
            }

            int methodType = 0;
            int pos = allProps.FindIndex((p) => { return p.GetGetMethod() == md || p.GetSetMethod() == md; });

            if (pos >= 0)
            {
                methodType = 1;
                pi = allProps[pos];

                if (md == pi.GetGetMethod())
                {
                    if (md.GetParameters().Length > 0)
                    {
                        methodType = 2;
                    }
                }
                else if (md == pi.GetSetMethod())
                {
                    if (md.GetParameters().Length > 1)
                    {
                        methodType = 2;
                    }
                }
            }

            return methodType;
        }

        static Type GetGenericBaseType(MethodBase md, Type t)
        {
            if (!md.IsGenericMethod)
            {
                return t;
            }

            List<Type> list = new List<Type>(md.GetGenericArguments());

            if (list.Contains(t))
            {
                return t.BaseType;
            }

            return t;
        }

        static bool IsNumberEnum(Type t)
        {
            if (t == typeof(BindingFlags))
            {
                return true;
            }

            return false;
        }

        static bool CompareParmsCount(_MethodBase l, _MethodBase r)
        {
            if (l == r)
            {
                return false;
            }

            int c1 = l.IsStatic ? 0 : 1;
            int c2 = r.IsStatic ? 0 : 1;

            c1 += l.GetParameters().Length;
            c2 += r.GetParameters().Length;

            return c1 == c2;
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

        //-1 不存在替换, 1 保留左面， 2 保留右面
        static int CompareMethod(_MethodBase l, _MethodBase r)
        {
            int s = 0;

            if (!CompareParmsCount(l, r))
            {
                return -1;
            }
            else
            {
                System.Reflection.ParameterInfo[] lp = l.GetParameters();
                System.Reflection.ParameterInfo[] rp = r.GetParameters();

                List<Type> ll = new List<Type>();
                List<Type> lr = new List<Type>();

                if (!l.IsStatic)
                {
                    ll.Add(type);
                }

                if (!r.IsStatic)
                {
                    lr.Add(type);
                }

                for (int i = 0; i < lp.Length; i++)
                {
                    ll.Add(GetParameterType(lp[i]));
                }

                for (int i = 0; i < rp.Length; i++)
                {
                    lr.Add(GetParameterType(rp[i]));
                }

                for (int i = 0; i < ll.Count; i++)
                {
                    if (!typeSize.ContainsKey(ll[i]) || !typeSize.ContainsKey(lr[i]))
                    {
                        if (ll[i] == lr[i])
                        {
                            continue;
                        }
                        else
                        {
                            return -1;
                        }
                    }
                    else if (ll[i].IsPrimitive && lr[i].IsPrimitive && s == 0)
                    {
                        s = typeSize[ll[i]] >= typeSize[lr[i]] ? 1 : 2;
                    }
                    else if (ll[i] != lr[i] && !ll[i].IsPrimitive && !lr[i].IsPrimitive)
                    {
                        return -1;
                    }
                }

                if (s == 0 && l.IsStatic)
                {
                    s = 2;
                }
            }

            return s;
        }

        static int[] CheckCheckTypePos<T>(List<T> list) where T : _MethodBase
        {
            int[] map = new int[list.Count];

            for (int i = 0; i < list.Count;)
            {
                if (HasOptionalParam(list[i].GetParameters()))
                {
                    if (list[0].IsConstructor)
                    {
                        for (int k = 0; k < map.Length; k++)
                        {
                            map[k] = 1;
                        }
                    }
                    else
                    {
                        Array.Clear(map, 0, map.Length);
                    }

                    return map;
                }

                int c1 = list[i].GetParamsCount();
                int count = c1;
                map[i] = count;
                int j = i + 1;

                for (; j < list.Count; j++)
                {
                    int c2 = list[j].GetParamsCount();

                    if (c1 == c2)
                    {
                        count = Mathf.Min(count, list[i].GetEqualParamsCount(list[j]));
                    }
                    else
                    {
                        map[j] = c2;
                        break;
                    }

                    for (int m = i; m <= j; m++)
                    {
                        map[m] = count;
                    }
                }

                i = j;
            }

            return map;
        }

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

        public static string GetBaseTypeStr(Type t)
        {
            if (t.IsGenericType)
            {
                return LuaMisc.GetTypeName(t);
            }
            else
            {
                return t.FullName.Replace("+", ".");
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
                str += LuaMisc.GetArrayRank(t);
                return str;
            }
            else if (t == extendType)
            {
                return GetTypeStr(type);
            }
            else if (IsIEnumerator(t))
            {
                return LuaMisc.GetTypeName(typeof(IEnumerator));
            }

            return LuaMisc.GetTypeName(t);
        }

        //获取 typeof(string) 这样的名字
        static string GetTypeOf(Type t, string sep)
        {
            string str;

            if (t.IsByRef)
            {
                t = t.GetElementType();
            }

            if (IsNumberEnum(t))
            {
                str = string.Format("uint{0}", sep);
            }
            else if (IsIEnumerator(t))
            {
                str = string.Format("{0}{1}", GetTypeStr(typeof(IEnumerator)), sep);
            }
            else
            {
                str = string.Format("{0}{1}", GetTypeStr(t), sep);
            }

            return str;
        }

        static string GenParamTypes(System.Reflection.ParameterInfo[] p, MethodBase mb, int offset = 0)
        {
            StringBuilder sb = new StringBuilder();
            List<Type> list = new List<Type>();

            if (!mb.IsStatic)
            {
                list.Add(type);
            }

            for (int i = 0; i < p.Length; i++)
            {
                if (IsParams(p[i]))
                {
                    continue;
                }

                if (p[i].Attributes != ParameterAttributes.Out)
                {
                    list.Add(GetGenericBaseType(mb, p[i].ParameterType));
                }
                else
                {
                    Type genericClass = typeof(LuaOut<>);
                    Type t = genericClass.MakeGenericType(p[i].ParameterType);
                    list.Add(t);
                }
            }

            for (int i = offset; i < list.Count - 1; i++)
            {
                sb.Append(GetTypeOf(list[i], ", "));
            }

            if (list.Count > 0)
            {
                sb.Append(GetTypeOf(list[list.Count - 1], ""));
            }

            return sb.ToString();
        }

        static void GenLuaFunctionRetValue(StringBuilder sb, Type t, string head, string name, bool beDefined = false)
        {
            if (t == typeof(bool))
            {
                name = beDefined ? name : "bool " + name;
                sb.AppendFormat("{0}{1} = func.CheckBoolean();\r\n", head, name);
            }
            else if (t == typeof(long))
            {
                name = beDefined ? name : "long " + name;
                sb.AppendFormat("{0}{1} = func.CheckLong();\r\n", head, name);
            }
            else if (t == typeof(ulong))
            {
                name = beDefined ? name : "ulong " + name;
                sb.AppendFormat("{0}{1} = func.CheckULong();\r\n", head, name);
            }
            else if (t.IsPrimitive || IsNumberEnum(t))
            {
                string type = GetTypeStr(t);
                name = beDefined ? name : type + " " + name;

                if (t == typeof(float) || t == typeof(double) || t == typeof(decimal))
                {
                    sb.AppendFormat("{0}{1} = ({2})func.CheckNumber();\r\n", head, name, type);
                }
                else
                {
                    sb.AppendFormat("{0}{1} = ({2})func.CheckInteger();\r\n", head, name, type);
                }
            }
            else if (t == typeof(string))
            {
                name = beDefined ? name : "string " + name;
                sb.AppendFormat("{0}{1} = func.CheckString();\r\n", head, name);
            }
            else if (typeof(System.MulticastDelegate).IsAssignableFrom(t))
            {
                name = beDefined ? name : GetTypeStr(t) + " " + name;
                sb.AppendFormat("{0}{1} = func.CheckDelegate();\r\n", head, name);
            }
            else if (t == typeof(Vector3))
            {
                name = beDefined ? name : "UnityEngine.Vector3 " + name;
                sb.AppendFormat("{0}{1} = func.CheckVector3();\r\n", head, name);
            }
            else if (t == typeof(Quaternion))
            {
                name = beDefined ? name : "UnityEngine.Quaternion " + name;
                sb.AppendFormat("{0}{1} = func.CheckQuaternion();\r\n", head, name);
            }
            else if (t == typeof(Vector2))
            {
                name = beDefined ? name : "UnityEngine.Vector2 " + name;
                sb.AppendFormat("{0}{1} = func.CheckVector2();\r\n", head, name);
            }
            else if (t == typeof(Vector4))
            {
                name = beDefined ? name : "UnityEngine.Vector4 " + name;
                sb.AppendFormat("{0}{1} = func.CheckVector4();\r\n", head, name);
            }
            else if (t == typeof(Color))
            {
                name = beDefined ? name : "UnityEngine.Color " + name;
                sb.AppendFormat("{0}{1} = func.CheckColor();\r\n", head, name);
            }
            else if (t == typeof(Ray))
            {
                name = beDefined ? name : "UnityEngine.Ray " + name;
                sb.AppendFormat("{0}{1} = func.CheckRay();\r\n", head, name);
            }
            else if (t == typeof(Bounds))
            {
                name = beDefined ? name : "UnityEngine.Bounds " + name;
                sb.AppendFormat("{0}{1} = func.CheckBounds();\r\n", head, name);
            }
            else if (t == typeof(LayerMask))
            {
                name = beDefined ? name : "UnityEngine.LayerMask " + name;
                sb.AppendFormat("{0}{1} = func.CheckLayerMask();\r\n", head, name);
            }
            else if (t == typeof(object))
            {
                name = beDefined ? name : "object " + name;
                sb.AppendFormat("{0}{1} = func.CheckVariant();\r\n", head, name);
            }
            else if (t == typeof(byte[]))
            {
                name = beDefined ? name : "byte[] " + name;
                sb.AppendFormat("{0}{1} = func.CheckByteBuffer();\r\n", head, name);
            }
            else if (t == typeof(char[]))
            {
                name = beDefined ? name : "char[] " + name;
                sb.AppendFormat("{0}{1} = func.CheckCharBuffer();\r\n", head, name);
            }
            else
            {
                string type = GetTypeStr(t);
                name = beDefined ? name : type + " " + name;
                sb.AppendFormat("{0}{1} = ({2})func.CheckObject(TypeTraits<{2}>.type);\r\n", head, name, type);

                //Debugger.LogError("GenLuaFunctionCheckValue undefined type:" + t.FullName);
            }
        }

        public static bool IsByteBuffer(Type type)
        {
            object[] attrs = type.GetCustomAttributes(true);

            for (int j = 0; j < attrs.Length; j++)
            {
                Type t = attrs[j].GetType();

                if (t == typeof(LuaByteBufferAttribute))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsByteBuffer(MemberInfo mb)
        {
            object[] attrs = mb.GetCustomAttributes(true);

            for (int j = 0; j < attrs.Length; j++)
            {
                Type t = attrs[j].GetType();

                if (t == typeof(LuaByteBufferAttribute))
                {
                    return true;
                }
            }

            return false;
        }

        /*static void LuaFuncToDelegate(Type t, string head)
        {
            System.Reflection.MethodInfo mi = t.GetMethod("Invoke");
            System.Reflection.ParameterInfo[] pi = mi.GetParameters();
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

        static bool IsNeedOp(string name)
        {
            if (name == "op_Addition")
            {
                op |= MetaOp.Add;
            }
            else if (name == "op_Subtraction")
            {
                op |= MetaOp.Sub;
            }
            else if (name == "op_Equality")
            {
                op |= MetaOp.Eq;
            }
            else if (name == "op_Multiply")
            {
                op |= MetaOp.Mul;
            }
            else if (name == "op_Division")
            {
                op |= MetaOp.Div;
            }
            else if (name == "op_UnaryNegation")
            {
                op |= MetaOp.Neg;
            }
            else if (name == "ToString" && !isStaticClass)
            {
                op |= MetaOp.ToStr;
            }
            else if (name == "op_LessThanOrEqual")
            {
                op |= MetaOp.Le;
            }
            else if (name == "op_GreaterThanOrEqual")
            {
                op |= MetaOp.Lt;
            }
            else
            {
                return false;
            }


            return true;
        }

        static void CallOpFunction(string name, int count, string ret)
        {
            string head = string.Empty;

            for (int i = 0; i < count; i++)
            {
                head += "\t";
            }

            if (name == "op_Addition")
            {
                sb.AppendFormat("{0}{1} o = arg0 + arg1;\r\n", head, ret);
            }
            else if (name == "op_Subtraction")
            {
                sb.AppendFormat("{0}{1} o = arg0 - arg1;\r\n", head, ret);
            }
            else if (name == "op_Equality")
            {
                sb.AppendFormat("{0}{1} o = arg0 == arg1;\r\n", head, ret);
            }
            else if (name == "op_Multiply")
            {
                sb.AppendFormat("{0}{1} o = arg0 * arg1;\r\n", head, ret);
            }
            else if (name == "op_Division")
            {
                sb.AppendFormat("{0}{1} o = arg0 / arg1;\r\n", head, ret);
            }
            else if (name == "op_UnaryNegation")
            {
                sb.AppendFormat("{0}{1} o = -arg0;\r\n", head, ret);
            }
            else if (name == "op_LessThanOrEqual")
            {
                sb.AppendFormat("{0}{1} o = arg0 >= arg1;\r\n", head, ret);
            }
            else if (name == "op_GreaterThanOrEqual")
            {
                sb.AppendFormat("{0}{1} o = arg0 >= arg1 ? false : true;\r\n", head, ret);
            }
        }

        public static bool IsObsolete(MemberInfo mb)
        {
            object[] attrs = mb.GetCustomAttributes(true);

            for (int j = 0; j < attrs.Length; j++)
            {
                Type t = attrs[j].GetType();

                if (t == typeof(System.ObsoleteAttribute) || t == typeof(NoToLuaAttribute) || t == typeof(MonoPInvokeCallbackAttribute) ||
                    t.Name == "MonoNotSupportedAttribute" || t.Name == "MonoTODOAttribute") // || t.ToString() == "UnityEngine.WrapperlessIcall")
                {
                    return true;
                }
            }

            if (IsMemberFilter(mb))
            {
                return true;
            }

            return false;
        }

        public static bool HasAttribute(MemberInfo mb, Type atrtype)
        {
            object[] attrs = mb.GetCustomAttributes(true);

            for (int j = 0; j < attrs.Length; j++)
            {
                Type t = attrs[j].GetType();

                if (t == atrtype)
                {
                    return true;
                }
            }

            return false;
        }

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
        Delegate[] ds = obj.GetInvocationList();

        for (int i = 0; i < ds.Length; i++)
        {
            LuaDelegate ld = ds[i].Target as LuaDelegate;

            if (ld != null && ld.func == func)
            {
                obj = Delegate.Remove(obj, ds[i]);
                if (obj != null) obj.AddRef();
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

        Delegate[] ds = obj.GetInvocationList();

        for (int i = 0; i < ds.Length; i++)
        {
            LuaDelegate ld = ds[i].Target as LuaDelegate;

            if (ld != null && ld == remove)
            {
                obj = Delegate.Remove(obj, ds[i]);
                if (obj != null) obj.AddRef();
                break;
            }
        }

        return obj;
    }
";

        static string GetDelegateParams(System.Reflection.MethodInfo mi)
        {
            System.Reflection.ParameterInfo[] infos = mi.GetParameters();
            List<string> list = new List<string>();

            for (int i = 0; i < infos.Length; i++)
            {
                string s2 = GetTypeStr(infos[i].ParameterType) + " param" + i;

                if (infos[i].ParameterType.IsByRef)
                {
                    if (infos[i].Attributes == ParameterAttributes.Out)
                    {
                        s2 = "out " + s2;
                    }
                    else
                    {
                        s2 = "ref " + s2;
                    }
                }

                list.Add(s2);
            }

            return string.Join(", ", list.ToArray());
        }

        static string GetReturnValue(Type t)
        {
            if (t.IsPrimitive)
            {
                if (t == typeof(bool))
                {
                    return "false";
                }
                else if (t == typeof(char))
                {
                    return "'\\0'";
                }
                else
                {
                    return "0";
                }
            }
            else if (!t.IsValueType)
            {
                return "null";
            }
            else
            {
                return string.Format("default({0})", GetTypeStr(t));
            }
        }

        static string GetDefaultDelegateBody(System.Reflection.MethodInfo md)
        {
            string str = "\r\n\t\t\t{\r\n";
            bool flag = false;
            System.Reflection.ParameterInfo[] pis = md.GetParameters();

            for (int i = 0; i < pis.Length; i++)
            {
                if (pis[i].Attributes == ParameterAttributes.Out)
                {
                    str += string.Format("\t\t\t\tparam{0} = {1};\r\n", i, GetReturnValue(pis[i].ParameterType.GetElementType()));
                    flag = true;
                }
            }

            if (flag)
            {
                if (md.ReturnType != typeof(void))
                {
                    str += "\t\t\treturn ";
                    str += GetReturnValue(md.ReturnType);
                    str += ";";
                }

                str += "\t\t\t};\r\n\r\n";
                return str;
            }

            if (md.ReturnType == typeof(void))
            {
                return "{ };\r\n";
            }
            else
            {
                return string.Format("{{ return {0}; }};\r\n", GetReturnValue(md.ReturnType));
            }
        }

        static bool IsUseDefinedAttributee(MemberInfo mb)
        {
            object[] attrs = mb.GetCustomAttributes(false);

            for (int j = 0; j < attrs.Length; j++)
            {
                Type t = attrs[j].GetType();

                if (t == typeof(UseDefinedAttribute))
                {
                    return true;
                }
            }

            return false;
        }

        static bool IsMethodEqualExtend(MethodBase a, MethodBase b)
        {
            if (a.Name != b.Name)
            {
                return false;
            }

            int c1 = a.IsStatic ? 0 : 1;
            int c2 = b.IsStatic ? 0 : 1;

            c1 += a.GetParameters().Length;
            c2 += b.GetParameters().Length;

            if (c1 != c2) return false;

            System.Reflection.ParameterInfo[] lp = a.GetParameters();
            System.Reflection.ParameterInfo[] rp = b.GetParameters();

            List<Type> ll = new List<Type>();
            List<Type> lr = new List<Type>();

            if (!a.IsStatic)
            {
                ll.Add(type);
            }

            if (!b.IsStatic)
            {
                lr.Add(type);
            }

            for (int i = 0; i < lp.Length; i++)
            {
                ll.Add(GetParameterType(lp[i]));
            }

            for (int i = 0; i < rp.Length; i++)
            {
                lr.Add(GetParameterType(rp[i]));
            }

            for (int i = 0; i < ll.Count; i++)
            {
                if (ll[i] != lr[i])
                {
                    return false;
                }
            }

            return true;
        }

        static void ProcessEditorExtend(Type extendType, List<_MethodBase> list)
        {
            if (extendType != null)
            {
                List<System.Reflection.MethodInfo> list2 = new List<System.Reflection.MethodInfo>();
                list2.AddRange(extendType.GetMethods(BindingFlags.Instance | binding | BindingFlags.DeclaredOnly));

                for (int i = list2.Count - 1; i >= 0; i--)
                {
                    if (list2[i].Name.StartsWith("op_") || list2[i].Name.StartsWith("add_") || list2[i].Name.StartsWith("remove_"))
                    {
                        if (!IsNeedOp(list2[i].Name))
                        {
                            continue;
                        }
                    }

                    if (IsUseDefinedAttributee(list2[i]))
                    {
                        list.RemoveAll((md) => { return md.Name == list2[i].Name; });
                    }
                    else
                    {
                        int index = list.FindIndex((md) => { return IsMethodEqualExtend(md.Method, list2[i]); });

                        if (index >= 0)
                        {
                            list.RemoveAt(index);
                        }
                    }

                    if (!IsObsolete(list2[i]))
                    {
                        list.Add(new _MethodBase(list2[i]));
                    }
                }

                System.Reflection.FieldInfo field = extendType.GetField("AdditionNameSpace");

                if (field != null)
                {
                    string str = field.GetValue(null) as string;
                    string[] spaces = str.Split(new char[] { ';' });

                    for (int i = 0; i < spaces.Length; i++)
                    {
                        usingList.Add(spaces[i]);
                    }
                }
            }
        }

        static bool IsGenericType(System.Reflection.MethodInfo md, Type t)
        {
            Type[] list = md.GetGenericArguments();

            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] == t)
                {
                    return true;
                }
            }

            return false;
        }

        static void GetDelegateTypeFromMethodParams(_MethodBase m)
        {
            if (m.IsGenericMethod)
            {
                return;
            }

            System.Reflection.ParameterInfo[] pifs = m.GetParameters();

            for (int k = 0; k < pifs.Length; k++)
            {
                Type t = pifs[k].ParameterType;

                if (IsDelegateType(t))
                {
                    eventSet.Add(t);
                }
            }
        }

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
                    str += "<" + string.Join(",", LuaMisc.GetGenericName(gArgs, offset, count)) + ">";
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
                str += "<" + string.Join(",", LuaMisc.GetGenericName(gArgs, offset, count)) + ">";
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

        static Type GetParameterType(System.Reflection.ParameterInfo info)
        {
            if (info.ParameterType == extendType)
            {
                return type;
            }

            return info.ParameterType;
        }
    }

    [NoToLuaAttribute]
    public static class LuaMisc
    {
        static readonly Il2cppType il2cpp = new Il2cppType();

        public static string GetArrayRank(Type t)
        {
            int count = t.GetArrayRank();

            if (count == 1)
            {
                return "[]";
            }

            //using (CString.Block())
            {
                //CString sb = CString.Alloc(64);
                StringBuilder sb = new StringBuilder();
                sb.Append('[');

                for (int i = 1; i < count; i++)
                {
                    sb.Append(',');
                }

                sb.Append(']');
                return sb.ToString();
            }
        }

        public static string GetTypeName(Type t)
        {
            if (t.IsArray)
            {
                string str = GetTypeName(t.GetElementType());
                str += GetArrayRank(t);
                return str;
            }
            else if (t.IsByRef)
            {
                t = t.GetElementType();
                return GetTypeName(t);
            }
            else if (t.IsGenericType)
            {
                return GetGenericName(t);
            }
            else if (t == il2cpp.TypeOfVoid)
            {
                return "void";
            }
            else
            {
                string name = GetPrimitiveStr(t);
                return name.Replace('+', '.');
            }
        }

        public static string[] GetGenericName(Type[] types, int offset, int count)
        {
            string[] results = new string[count];

            for (int i = 0; i < count; i++)
            {
                int pos = i + offset;

                if (types[pos].IsGenericType)
                {
                    results[i] = GetGenericName(types[pos]);
                }
                else
                {
                    results[i] = GetTypeName(types[pos]);
                }

            }

            return results;
        }

        static string CombineTypeStr(string space, string name)
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

        static string GetGenericName(Type t)
        {
            Type[] gArgs = t.GetGenericArguments();
            string typeName = t.FullName ?? t.Name;
            int count = gArgs.Length;
            int pos = typeName.IndexOf("[");

            if (pos > 0)
            {
                typeName = typeName.Substring(0, pos);
            }

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
                    str += "<" + string.Join(",", GetGenericName(gArgs, offset, count)) + ">";
                    offset += count;
                }

                name = CombineTypeStr(name, str);
                pos = typeName.IndexOf("+");
            }

            str = typeName;

            if (offset < gArgs.Length)
            {
                pos = str.IndexOf('`');
                count = (int)(str[pos + 1] - '0');
                str = str.Substring(0, pos);
                str += "<" + string.Join(",", GetGenericName(gArgs, offset, count)) + ">";
            }

            return CombineTypeStr(name, str);
        }

        public static Delegate GetEventHandler(object obj, Type t, string eventName)
        {
            System.Reflection.FieldInfo eventField = t.GetField(eventName, BindingFlags.GetField | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            return (Delegate)eventField.GetValue(obj);
        }

        public static string GetPrimitiveStr(Type t)
        {
            Il2cppType il = il2cpp;

            if (t == il.TypeOfFloat)
            {
                return "float";
            }
            else if (t == il.TypeOfString)
            {
                return "string";
            }
            else if (t == il.TypeOfInt)
            {
                return "int";
            }
            else if (t == il.TypeOfDouble)
            {
                return "double";
            }
            else if (t == il.TypeOfBool)
            {
                return "bool";
            }
            else if (t == il.TypeOfUInt)
            {
                return "uint";
            }
            else if (t == il.TypeOfSByte)
            {
                return "sbyte";
            }
            else if (t == il.TypeOfByte)
            {
                return "byte";
            }
            else if (t == il.TypeOfShort)
            {
                return "short";
            }
            else if (t == il.TypeOfUShort)
            {
                return "ushort";
            }
            else if (t == il.TypeOfChar)
            {
                return "char";
            }
            else if (t == il.TypeOfLong)
            {
                return "long";
            }
            else if (t == il.TypeOfULong)
            {
                return "ulong";
            }
            else if (t == il.TypeOfDecimal)
            {
                return "decimal";
            }
            else if (t == il.TypeOfObject)
            {
                return "object";
            }
            else
            {
                return t.ToString();
            }
        }

        //可产生导出文件的基类
        public static Type GetExportBaseType(Type t)
        {
            Type baseType = t.BaseType;

            if (baseType == il2cpp.TypeOfStruct)
            {
                return null;
            }

            if (t.IsAbstract && t.IsSealed)
            {
                return baseType == il2cpp.TypeOfObject ? null : baseType;
            }

            return baseType;
        }
    }

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
        Le = 128,
        Lt = 256,
        ALL = Add | Sub | Mul | Div | Eq | Neg | ToStr,
    }

    public enum ObjAmbig
    {
        None = 0,
        U3dObj = 1,
        NetObj = 2,
        All = 3
    }

    public class LuaOut<T> { }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MonoPInvokeCallbackAttribute : Attribute
    {
        public MonoPInvokeCallbackAttribute(Type type)
        {
        }
    }

    public class NoToLuaAttribute : System.Attribute
    {
        public NoToLuaAttribute()
        {

        }
    }

    public class UseDefinedAttribute : System.Attribute
    {
        public UseDefinedAttribute()
        {

        }
    }

    public class OverrideDefinedAttribute : System.Attribute
    {
        public OverrideDefinedAttribute()
        {

        }
    }

    public sealed class LuaByteBufferAttribute : Attribute
    {
        public LuaByteBufferAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class LuaRenameAttribute : Attribute
    {
        public string Name;
        public LuaRenameAttribute()
        {
        }
    }

    public class Il2cppType
    {
        public Type TypeOfFloat = typeof(float);
        public Type TypeOfInt = typeof(int);
        public Type TypeOfUInt = typeof(uint);
        public Type TypeOfDouble = typeof(double);
        public Type TypeOfBool = typeof(bool);
        public Type TypeOfLong = typeof(long);
        public Type TypeOfULong = typeof(ulong);
        public Type TypeOfSByte = typeof(sbyte);
        public Type TypeOfByte = typeof(byte);
        public Type TypeOfShort = typeof(short);
        public Type TypeOfUShort = typeof(ushort);
        public Type TypeOfChar = typeof(char);
        public Type TypeOfDecimal = typeof(decimal);
        public Type TypeOfIntPtr = typeof(IntPtr);
        public Type TypeOfUIntPtr = typeof(UIntPtr);

        public Type TypeOfVector3 = typeof(Vector3);
        public Type TypeOfQuaternion = typeof(Quaternion);
        public Type TypeOfVector2 = typeof(Vector2);
        public Type TypeOfVector4 = typeof(Vector4);
        public Type TypeOfColor = typeof(Color);
        public Type TypeOfBounds = typeof(Bounds);
        public Type TypeOfRay = typeof(Ray);
        public Type TypeOfTouch = typeof(Touch);
        public Type TypeOfLayerMask = typeof(LayerMask);
        public Type TypeOfRaycastHit = typeof(RaycastHit);

        //public Type TypeOfLuaTable = typeof(LuaTable);
        //public Type TypeOfLuaThread = typeof(LuaThread);
        //public Type TypeOfLuaFunction = typeof(LuaFunction);
        //public Type TypeOfLuaBaseRef = typeof(LuaBaseRef);

        public Type MonoType = typeof(Type).GetType();
        public Type TypeOfObject = typeof(object);
        public Type TypeOfType = typeof(Type);
        public Type TypeOfString = typeof(string);
        public Type UObjectType = typeof(UnityEngine.Object);
        public Type TypeOfStruct = typeof(ValueType);
        //public Type TypeOfLuaByteBuffer = typeof(LuaByteBuffer);
        //public Type TypeOfEventObject = typeof(EventObject);
        //public Type TypeOfNullObject = typeof(NullObject);

        public Type TypeOfArray = typeof(Array);
        public Type TypeOfByteArray = typeof(byte[]);
        public Type TypeOfBoolArray = typeof(bool[]);
        public Type TypeOfCharArray = typeof(char[]);
        public Type TypeOfStringArray = typeof(string[]);
        public Type TypeOfObjectArray = typeof(object[]);

        public Type TypeofGenericNullObject = typeof(Nullable<>);

        public Type TypeOfVoid = typeof(void);
    }

    abstract class Filter<T>
    {
        protected Type _type;

        public delegate void EachProcessor(T value);

        public delegate void EachProcessor2(T value, Type type);

        public abstract bool Contains(T type);

        public Filter()
        {

        }

        public Filter(Type type = null)
        {
            _type = type;
        }

        public Filter<T> Exclude(params Filter<T>[] others)
        {
            Filter<T> v = this;
            for (var i = 0; i < others.Length; i++)
            {
                v = new ExcludeFilter<T>(v, others[i], v._type);
            }
            return v;
        }

        public Filter<T> And(params Filter<T>[] others)
        {
            Filter<T> v = this;
            for (var i = 0; i < others.Length; i++)
            {
                v = new AndFilter<T>(v, others[i]);
            }
            return v;
        }

        public Filter<T> Or(params Filter<T>[] others)
        {
            Filter<T> v = this;
            for (var i = 0; i < others.Length; i++)
            {
                v = new OrFilter<T>(v, others[i]);
            }
            return v;
        }

        public virtual void Each(EachProcessor processor)
        {

        }

        public virtual void Each(EachProcessor2 processor)
        {

        }
    }

    class ExcludeFilter<T> : Filter<T>
    {
        private readonly Filter<T> _baseFilter;
        private readonly Filter<T> _excludeFilter;

        public ExcludeFilter(Filter<T> baseFilter, Filter<T> excludeFilter, Type type = null) : base(type)
        {
            _baseFilter = baseFilter;
            _excludeFilter = excludeFilter;
        }

        public override bool Contains(T type)
        {
            return _baseFilter.Contains(type) && !_excludeFilter.Contains(type);
        }

        public override void Each(EachProcessor processor)
        {
            _baseFilter.Each(v =>
            {
                if (!_excludeFilter.Contains(v))
                {
                    processor(v);
                }
            });
        }

        public override void Each(EachProcessor2 processor)
        {
            _baseFilter.Each(v =>
            {
                if (!_excludeFilter.Contains(v))
                {
                    processor(v, _type);
                }
            });
        }
    }

    class OrFilter<T> : Filter<T>
    {
        private readonly Filter<T> _baseFilter;
        private readonly Filter<T> _orFilter;

        public OrFilter(Filter<T> baseFilter, Filter<T> orFilter)
        {
            _baseFilter = baseFilter;
            _orFilter = orFilter;
        }

        public override bool Contains(T type)
        {
            return _baseFilter.Contains(type) || _orFilter.Contains(type);
        }

        public override void Each(EachProcessor processor)
        {
            _baseFilter.Each(processor);
            _orFilter.Each(processor);
        }
    }

    class AndFilter<T> : Filter<T>
    {
        private readonly Filter<T> _baseFilter;
        private readonly Filter<T> _andFilter;

        public AndFilter(Filter<T> baseFilter, Filter<T> andFilter)
        {
            _baseFilter = baseFilter;
            _andFilter = andFilter;
        }

        public override bool Contains(T type)
        {
            return _baseFilter.Contains(type) && _andFilter.Contains(type);
        }

        public override void Each(EachProcessor processor)
        {
            _baseFilter.Each(v =>
            {
                if (_andFilter.Contains(v))
                {
                    processor(v);
                }
            });
        }
    }

    class GeneralFilter<T> : Filter<T>
    {
        private readonly ICollection<T> _arr;

        public GeneralFilter(ICollection<T> arr, Type type = null) : base(type)
        {
            _arr = arr;
        }

        public override bool Contains(T type) { return _arr.Contains(type); }

        public override void Each(EachProcessor processor)
        {
            foreach (T x1 in _arr)
            {
                processor(x1);
            }
        }

        public override void Each(EachProcessor2 processor)
        {
            foreach (T x1 in _arr)
            {
                processor(x1, _type);
            }
        }
    }

    public static class ToLuaMenu
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
        //typeof(UnityEngine.YieldInstruction),               //无需导出的类
        //typeof(UnityEngine.WaitForEndOfFrame),              //内部支持
        //typeof(UnityEngine.WaitForFixedUpdate),
        //typeof(UnityEngine.WaitForSeconds),
        //typeof(UnityEngine.Mathf),                          //lua层支持
        //typeof(Plane),
        //typeof(LayerMask),
        //typeof(Vector3),
        //typeof(Vector4),
        //typeof(Vector2),
        //typeof(Quaternion),
        //typeof(Ray),
        //typeof(Bounds),
        //typeof(Color),
        //typeof(Touch),
        //typeof(RaycastHit),
        //typeof(TouchPhase),
        ////typeof(LuaInterface.LuaOutMetatable),               //手写支持
        ////typeof(LuaInterface.NullObject),
        //typeof(System.Array),
        //typeof(System.Reflection.MemberInfo),
        //typeof(System.Reflection.BindingFlags),
        //typeof(LuaClient),
        //typeof(LuaInterface.LuaFunction),
        //typeof(LuaInterface.LuaTable),
        //typeof(LuaInterface.LuaThread),
        //typeof(LuaInterface.LuaByteBuffer),                 //只是类型标识符
        //typeof(DelegateFactory),                            //无需导出，导出类支持lua函数转换为委托。如UIEventListener.OnClick(luafunc)
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
        //typeof(LuaInterface.EventObject),
        //typeof(LuaInterface.LuaMethod),
        //typeof(LuaInterface.LuaProperty),
        //typeof(LuaInterface.LuaField),
        //typeof(LuaInterface.LuaConstructor),
    };
    }

    class BindTypeCollection : Filter<BindType>
    {
        private readonly Queue<BindType> _typeQueue;
        private List<BindType> _typeList;

        public BindTypeCollection(BindType[] typeArr)
        {
            _typeQueue = new Queue<BindType>(typeArr);
        }

        public BindType[] CollectBindType(Filter<Type> baseFilter, Filter<Type> excludeFilter)
        {
            List<Type> processed = new List<Type>();
            excludeFilter = excludeFilter.Or(new GeneralFilter<Type>(processed));
            _typeList = new List<BindType>();

            baseFilter.Each(t => _typeQueue.Enqueue(new BindType(t)));
            while (_typeQueue.Count > 0)
            {
                var bind = _typeQueue.Dequeue();
                if (!excludeFilter.Contains(bind.type))
                {
                    _typeList.Add(bind);
                    processed.Add(bind.type);
                    CreateBaseBindType(bind.baseType, excludeFilter);
                }
            }
            return _typeList.ToArray();
        }

        void CreateBaseBindType(Type baseType, Filter<Type> excludeFilter)
        {
            if (baseType != null && !excludeFilter.Contains(baseType))
            {
                var bind = new BindType(baseType);
                _typeQueue.Enqueue(bind);
                CreateBaseBindType(bind.baseType, excludeFilter);
            }
        }

        public override bool Contains(BindType type)
        {
            return false;
        }

        public override void Each(EachProcessor processor)
        {
            foreach (var bindType in _typeList)
            {
                processor(bindType);
            }
        }
    }

    class OpMethodFilter : Filter<System.Reflection.MethodInfo>
    {
        public override bool Contains(System.Reflection.MethodInfo mi)
        {
            return mi.Name.StartsWith("Op_") || mi.Name.StartsWith("add_") || mi.Name.StartsWith("remove_");
        }
    }

    /// <summary>
    /// Get/Set 方法过滤
    /// </summary>
    class GetSetMethodFilter : Filter<System.Reflection.MethodInfo>
    {
        public override bool Contains(System.Reflection.MethodInfo type)
        {
            return type.Name.StartsWith("get_") || type.Name.StartsWith("set_");
        }
    }

    /// <summary>
    /// 废弃过滤
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class ObsoleteFilter<T> : Filter<T> where T : MemberInfo
    {
        public override bool Contains(T mb)
        {
            object[] attrs = mb.GetCustomAttributes(true);

            for (int j = 0; j < attrs.Length; j++)
            {
                Type t = attrs[j].GetType();

                if (t == typeof(ObsoleteAttribute) ||
                    t == typeof(NoToLuaAttribute) ||
                    t == typeof(MonoPInvokeCallbackAttribute) ||
                    t.Name == "MonoNotSupportedAttribute" ||
                    t.Name == "MonoTODOAttribute")
                {
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// 黑名单过滤
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class BlackListMemberNameFilter<T> : Filter<T> where T : MemberInfo
    {
        public override bool Contains(T mi)
        {
            if (ToLuaExport.memberFilter.Contains(mi.Name))
                return true;
            var type = mi.ReflectedType;
            if (type != null)
                return ToLuaExport.memberFilter.Contains(type.Name + "." + mi.Name);
            return false;
        }
    }

    /// <summary>
    /// 泛型方法过滤
    /// </summary>
    class GenericMethodFilter : Filter<System.Reflection.MethodInfo>
    {
        public override bool Contains(System.Reflection.MethodInfo mi)
        {
            //return mi.IsGenericMethod;
            return false;
        }
    }

    /// <summary>
    /// 扩展方法过滤
    /// </summary>
    class ExtendMethodFilter : Filter<System.Reflection.MethodInfo>
    {
        private readonly Type _type;

        public ExtendMethodFilter(Type type)
        {
            _type = type;
        }

        public override bool Contains(System.Reflection.MethodInfo mi)
        {
            System.Reflection.ParameterInfo[] infos = mi.GetParameters();
            if (infos.Length == 0) return false;

            var pi = infos[0];
            return pi.ParameterType == _type;
        }
    }

    class MethodData
    {
        public bool IsExtend;
        public bool IsStatic;
        public System.Reflection.MethodInfo Method;
    }

    class MethodDataSet
    {
        public string MethodName = string.Empty;      
        public List<MethodData> MethodList = new List<MethodData>();

        public MethodDataSet(string name)
        {
            MethodName = name;
        }

        public void Add(System.Reflection.MethodInfo mi, bool isExtend)
        {
            MethodData md = new MethodData { IsExtend = isExtend, IsStatic = mi.IsStatic, Method = mi };
            MethodList.Add(md);
        }
    }

    abstract class CodeGenerator
    {
        readonly Filter<System.Reflection.MethodInfo> methodExcludeFilter = new ObsoleteFilter<System.Reflection.MethodInfo>()
            .Or(new OpMethodFilter())
            .Or(new BlackListMemberNameFilter<System.Reflection.MethodInfo>())
            .Or(new GenericMethodFilter())
            .Or(new GetSetMethodFilter());

        protected BindType _bindType;

        public virtual void Gen(BindType bt)
        {
            _bindType = bt;

            bool isClass = CSharpReflectionSettings.customClassList.Contains(bt) || CSharpCustomReflectionSettings.customClassList.Contains(bt);
            bool isStruct = CSharpCustomReflectionSettings.customStructList.Contains(bt);
            bool isEnum = CSharpCustomReflectionSettings.customEnumList.Contains(bt);

            var typeStr = "";
            if (isClass)
                typeStr = "class";
            else if (isStruct)
                typeStr = "struct";
            else if (isEnum)
                typeStr = "enum";
            else typeStr = "class";

            GenMethods(typeStr);
            GenProperties(typeStr);
        }

        protected void GenMethods(string typeStr)
        {
            if (typeStr == "struct" || typeStr == "enum") return;

            var flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase | BindingFlags.Instance |
                        BindingFlags.DeclaredOnly;
            List<MethodDataSet> allMethods = new List<MethodDataSet>();
            Action<System.Reflection.MethodInfo, bool> methodCollector = (mi, isExtend) =>
            {
                MethodDataSet set;
                var saveNameMethodSet = allMethods.FirstOrDefault(s => s.MethodName == mi.Name && s.MethodList.FirstOrDefault(m => m.IsStatic == mi.IsStatic) != null);
                if (saveNameMethodSet != null)
                {
                    saveNameMethodSet.Add(mi, isExtend);
                }
                else
                {
                    set = new MethodDataSet(mi.Name);
                    set.Add(mi, isExtend);
                    allMethods.Add(set);
                }
            };
            //extend
            if (_bindType.extendList != null)
            {
                foreach (var type in _bindType.extendList)
                {
                    System.Reflection.MethodInfo[] methodInfos = type.GetMethods(flags);
                    var extFilter = new GeneralFilter<System.Reflection.MethodInfo>(methodInfos)
                        .Exclude(methodExcludeFilter)
                        .And(new ExtendMethodFilter(_bindType.type));
                    extFilter.Each(mi => { methodCollector(mi, true); });
                }
            }

            //base
            var methods = _bindType.type.GetMethods(flags);
            var filter = new GeneralFilter<System.Reflection.MethodInfo>(methods);
            var methodFilter = filter.Exclude(methodExcludeFilter);
            methodFilter.Each(mi => { methodCollector(mi, false); });

            foreach (var m in allMethods)
            {
                GenMethod(m.MethodName, m);
            }

            var NotReflectConstructorAttr = _bindType.type.GetCustomAttribute<NotReflectConstructorAttribute>();
            var ctors = _bindType.type.GetConstructors();
            if (ctors.Length > 0 && NotReflectConstructorAttr == null)
                GenConstructor(_bindType.name, ctors);
        }

        protected void GenProperties(string typeStr)
        {
            Type type = _bindType.type;
            //props
            var propList = type.GetProperties(BindingFlags.GetProperty | BindingFlags.SetProperty |
                                              BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase |
                                              BindingFlags.DeclaredOnly | BindingFlags.Static);
            var propFilter = new GeneralFilter<System.Reflection.PropertyInfo>(propList)
                .Exclude(new BlackListMemberNameFilter<System.Reflection.PropertyInfo>())
                .Exclude(new ObsoleteFilter<System.Reflection.PropertyInfo>());
            propFilter.Each(GenProperty);

            //fields
            var fields = type.GetFields(BindingFlags.GetField | BindingFlags.SetField | BindingFlags.Instance |
                                        BindingFlags.Public | BindingFlags.Static);
            var fieldFilter = new GeneralFilter<System.Reflection.FieldInfo>(fields, type)
                .Exclude(new BlackListMemberNameFilter<System.Reflection.FieldInfo>())
                .Exclude(new ObsoleteFilter<System.Reflection.FieldInfo>());
            fieldFilter.Each(GenField);

            //events

            var events = type.GetEvents(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public |
                                        BindingFlags.Static);
            var evtFilter = new GeneralFilter<System.Reflection.EventInfo>(events, type)
                .Exclude(new BlackListMemberNameFilter<System.Reflection.EventInfo>())
                .Exclude(new ObsoleteFilter<System.Reflection.EventInfo>());
            evtFilter.Each(GenEvent);

        }

        protected abstract void GenProperty(System.Reflection.PropertyInfo pi);

        protected abstract void GenEvent(System.Reflection.EventInfo ei, Type type);

        protected abstract void GenField(System.Reflection.FieldInfo fi, Type type);

        protected abstract void GenMethod(string name, MethodDataSet methodDataSet);

        protected abstract void GenConstructor(string name, System.Reflection.ConstructorInfo[] ctors);
    }

    class CSharpGenerator : CodeGenerator
    {
#region Blueprint_Testing const 自动化测试属性命名空间与类名字符串
        public const string c_ParamTypeAttrubute = "Blueprint_Testing.ParamTypeAttribute";
        public const string c_ParamTypeAttrubuteTypeField = "type";
        public const string c_MethodDescriptionAttribute = "Blueprint_Testing.MethodDescriptionAttribute";
        public const string c_MethodDescriptionAttributeDescField = "desc";
        public const string c_CommentAttribute = "Blueprint_Testing.CommentAttribute";
        public const string c_CommentAttributeCommendField = "comment";
        public const string c_DisplayNameAttribute = "Blueprint_Testing.DisplayNameAttribute";
        public const string c_DisplayNameAttributeDisplayNameField = "displayName";
        public const string c_CodeAttribute = "Blueprint_Testing.CodeAttribute";
        public const string c_CodeAttributeCodeField = "code";
#endregion

        public const string c_Void = "System.Void";

        private StringBuilder _baseSB;
        private StringBuilder _propBuilder;
        private StringBuilder _methodBuilder;

        public override void Gen(BindType bt)
        {
            _baseSB = new StringBuilder();

            if (CSharpReflectionSettings.customClassList.Contains(bt) || CSharpCustomReflectionSettings.customClassList.Contains(bt)) //类
            {
                if (bt.baseType != null)
                    _baseSB.AppendFormat("---@class {0} : {1}\n", bt.name, bt.baseType.GetTypeStr());
                else
                    _baseSB.AppendFormat("---@class {0}\n", bt.name);
            }
            else if (CSharpCustomReflectionSettings.customStructList.Contains(bt)) //结构
            {
                if (bt.baseType != null)
                    _baseSB.AppendFormat("---@struct {0} : {1}\n", bt.name, bt.baseType.GetTypeStr());
                else
                    _baseSB.AppendFormat("---@struct {0}\n", bt.name);
            }
            else if(CSharpCustomReflectionSettings.customEnumList.Contains(bt)) //枚举
            {
                if (bt.baseType != null)
                    _baseSB.AppendFormat("---@enum {0} : {1}\n", bt.name, bt.baseType.GetTypeStr());
                else
                    _baseSB.AppendFormat("---@enum {0}\n", bt.name);
            }
            else
            {
                if (bt.baseType != null)
                    _baseSB.AppendFormat("---@class {0} : {1}\n", bt.name, bt.baseType.GetTypeStr());
                else
                    _baseSB.AppendFormat("---@class {0}\n", bt.name);
            }

            _baseSB.AppendFormat("---@platform {0}\n", bt.platform);//平台信息

            _propBuilder = new StringBuilder();
            _methodBuilder = new StringBuilder();
            base.Gen(bt);

            _baseSB.Append(_propBuilder);
            _baseSB.Append("local m = {}\n");
            _baseSB.Append(_methodBuilder);

            string[] ns = bt.name.Split('.');
            for (int i = 0; i < ns.Length - 1; i++)
            {
                _baseSB.AppendFormat("{0} = {{}}\n", string.Join(".", ns, 0, i + 1));
            }
            _baseSB.AppendFormat("{0} = m\n", bt.name);
            _baseSB.Append("return m");

            string fileName = string.Format($"CSharpAPI/{bt.name}.txt").Replace("<","_").Replace(">", "_");
            File.WriteAllBytes(fileName, Encoding.GetEncoding("UTF-8").GetBytes(_baseSB.ToString()));
        }

        protected override void GenProperty(System.Reflection.PropertyInfo pi)
        {
            if (!GetReflectAttr(pi))
                return;
            // indexer会默认生成一个Item，这里强制跳过
            if (pi.Name.Equals("Item"))
            {
                return;
            }

            var comment = GetComment(pi);
            if ((pi.GetMethod != null) && ((pi.GetMethod.Attributes & MethodAttributes.Static) != 0))
                _propBuilder.AppendFormat("---@attribute {0} {1} static {2} {3}", pi.Name, pi.PropertyType.GetTypeStr(), (pi.CanWrite ? string.Empty : " readonly"), string.IsNullOrEmpty(comment) ? null : comment);
            else
                _propBuilder.AppendFormat("---@attribute {0} {1} {2} {3}", pi.Name, pi.PropertyType.GetTypeStr(), (pi.CanWrite ? string.Empty : " readonly"), string.IsNullOrEmpty(comment) ? null : comment);

            if (IsBlueprintRecongnizeEvent(pi.PropertyType))
            {
                GenDelegate(pi.PropertyType);
            }
            else if (CheckIsUnityEvent(pi.PropertyType))
            {
                // 表示识别为事件的类
                GenClassDelegate(pi.PropertyType);
            }

            _propBuilder.Append("\n");
        }

        protected override void GenEvent(System.Reflection.EventInfo ei, Type type)
        {
            if (!GetReflectAttr(ei))
                return;
            var comment = GetComment(ei);
            _propBuilder.AppendFormat("---@field {0} {1} {2}", ei.Name, ei.EventHandlerType.GetTypeStr(), string.IsNullOrEmpty(comment) ? null : comment);
            System.Reflection.FieldInfo fi = type.GetField(ei.Name, BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.GetField |
                BindingFlags.Static);

            if (fi != null)
            {
                GenDelegate(fi.FieldType);
            }

            _propBuilder.Append("\n");
        }

        protected override void GenField(System.Reflection.FieldInfo fi, Type type)
        {
            if (!GetReflectAttr(fi))
                return;
            var comment = GetComment(fi);
            bool isreadonly = false;
            if ((fi.Attributes & FieldAttributes.InitOnly) != 0)
                isreadonly = true;
            if ((fi.Attributes & FieldAttributes.Static) != 0)
            {
                if(!CSharpCustomReflectionSettings.customEnumList.Contains(_bindType))
                    _propBuilder.AppendFormat("---@field {0} {1} static {2}", fi.Name, fi.FieldType.GetTypeStr(), string.IsNullOrEmpty(comment) ? null : comment);
                else
                    _propBuilder.AppendFormat("---@field {0} {1} {2} {3}", fi.Name, (int)fi.GetValue(null), isreadonly ? " readonly" : String.Empty, string.IsNullOrEmpty(comment) ? null : comment);
            } 
            else
            {
                if (!CSharpCustomReflectionSettings.customEnumList.Contains(_bindType))
                {

                    _propBuilder.AppendFormat("---@field {0} {1} {2} {3}", fi.Name, fi.FieldType.GetTypeStr(), isreadonly ? " readonly" : String.Empty, string.IsNullOrEmpty(comment) ? null : comment);
                }
                else
                    return;
            }
               

            if (IsBlueprintRecongnizeEvent(fi.FieldType))
            {
                GenDelegate(fi.FieldType);
            }
            else if (CheckIsUnityEvent(fi.FieldType))
            {
                // 表示识别为事件的类
                GenClassDelegate(fi.FieldType);
            }

            _propBuilder.Append("\n");
        }

        protected bool CheckIsUnityEvent(Type type)
        {
            if (type.GetTypeStr().Equals("UnityEngine.Events.UnityEvent"))
            {
                return true;
            }
            else if (type.BaseType != null)
            {
                return CheckIsUnityEvent(type.BaseType);
            }
            return false;
        }

        /// <summary>
        /// 检测类型是否是代理
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool CheckIsDelegate(Type type)
        {
            if (type == typeof(System.Delegate))
            {
                return true;
            }
            else if (type.BaseType != null)
            {
                return CheckIsDelegate(type.BaseType);
            }

            return false;
        }

        /// <summary>
        /// 检测是否是蓝图识别的事件，目前识别没有返回值的delegate
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsBlueprintRecongnizeEvent(Type type)
        {
            bool isDele = CheckIsDelegate(type);

            if (isDele)
            {
                System.Reflection.MethodInfo method = type.GetMethod("Invoke");
                if (method == null)
                {
                    return false;
                }
                return method.ReturnType.FullName.Equals(c_Void);
            }

            return false;
        }

        protected void GenDelegate(Type type)
        {
            Type delegateType = type;
            System.Reflection.MethodInfo method = delegateType.GetMethod("Invoke");
            if (!GetNReflectAttr(method))
                return;
            _propBuilder.Append(" delegate(");
            foreach (System.Reflection.ParameterInfo param in method.GetParameters())
            {
                _propBuilder.Append(param.ParameterType.GetTypeStr() + "|");
            }
            _propBuilder.Append(method.ReturnType.FullName + "|");
            _propBuilder.Append("true");
            _propBuilder.Append(")");
        }

        protected void GenClassDelegate(Type type)
        {
            Type delegateType = type;
            Type[] generics = type.GetGenericArguments();

            if (generics == null || generics.Length == 0)
            {
                if (type.BaseType != null)
                {
                    GenClassDelegate(type.BaseType);
                }
                else
                {
                    _propBuilder.Append(" delegate(");
                    _propBuilder.Append($"{c_Void}|false)");
                }
            }
            else
            {
                _propBuilder.Append(" delegate(");
                foreach (Type t in generics)
                {
                    _propBuilder.Append(t.GetTypeStr() + "|");
                }
                _propBuilder.Append($"{c_Void}|false)");
            }

        }

        protected override void GenConstructor(string name, System.Reflection.ConstructorInfo[] ctors)
        {
            string lastName = name;
            if (lastName.IndexOf('.') > -1)
            {
                lastName = name.Substring(name.LastIndexOf('.') + 1);
            }
            //overload
            for (int j = 1; j < ctors.Length; ++j)
            {
                var mi = ctors[j];
                var parameters = mi.GetParameters();
                string[] paramNames = new string[parameters.Length];
                for (int i = 0; i < parameters.Length; ++i)
                {
                    var pi = parameters[i];
                    paramNames[i] = string.Format("{0}:{1}", pi.Name, pi.ParameterType.GetTypeStr());
                }
                _methodBuilder.AppendFormat("---@overload fun({0}):{1}\n", string.Join(", ", paramNames), name);
            }
            //main
            {
                var mi = ctors[0];
                var parameters = mi.GetParameters();
                string[] paramNames = new string[parameters.Length];
                for (int i = 0; i < parameters.Length; ++i)
                {
                    var pi = parameters[i];
                    _methodBuilder.AppendFormat("---@param {0} {1}\n", pi.Name, pi.ParameterType.GetTypeStr());
                    paramNames[i] = pi.Name;
                }
                _methodBuilder.AppendFormat("---@return {0}\n", name);
                _methodBuilder.AppendFormat("function m.{0}({1}) end\n", lastName, string.Join(", ", paramNames));
            }
        }

        public string GetParamTypeStr(System.Reflection.ParameterInfo p)
        {
            var pta = p.GetCustomAttribute<ParamTypeAttribute>();
            if (pta != null)
            {
                return pta.type;
            }

            string value = GetAttributeValue(p.GetCustomAttributes(true), 
                c_ParamTypeAttrubute, c_ParamTypeAttrubuteTypeField);
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }

            if(p.ParameterType.IsSubclassOf(typeof(Delegate)))
            {
                return "function";
            }
            else
            {
                return p.ParameterType.GetTypeStr();
            }
        }

        public string GetMethodDescriptionStr(System.Reflection.MethodInfo m)
        {
            var mda = m.GetCustomAttribute<MethodDescriptionAttribute>();
            if (mda != null)
            {
                return mda.desc;
            }

            string value = GetAttributeValue(m.GetCustomAttributes(true), 
                c_MethodDescriptionAttribute, c_MethodDescriptionAttributeDescField);
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }
            return null;
        }

        public bool GetReflectAttr(System.Reflection.ParameterInfo p)
        {
            var ca = p.GetCustomAttribute<NotReflectAttribute>();
            if (ca == null)
                return true;
            else
                return false;
        }

        public bool GetNReflectAttr(System.Reflection.MethodInfo p)
        {
            var ca = p.GetCustomAttribute<NotReflectAttribute>();
            if (ca == null)
                return true;
            else
                return false;
        }

        public bool GetReflectAttr(System.Reflection.PropertyInfo p)
        {
            var ca = p.GetCustomAttribute<NotReflectAttribute>();
            if (ca == null)
                return true;
            else
                return false;
        }

        public bool GetReflectAttr(System.Reflection.FieldInfo p)
        {
            var ca = p.GetCustomAttribute<NotReflectAttribute>();
            if (ca == null)
                return true;
            else
                return false;
        }

        public bool GetReflectAttr(System.Reflection.EventInfo p)
        {
            var ca = p.GetCustomAttribute<NotReflectAttribute>();
            if (ca == null)
                return true;
            else
                return false;
        }

        public string GetComment(System.Reflection.ParameterInfo p)
        {
            var ca = p.GetCustomAttribute<CommentAttribute>();
            if(ca != null)
            {
                return $" {ca.comment}";
            }

            string value = GetAttributeValue(p.GetCustomAttributes(true), 
                c_CommentAttribute, c_CommentAttributeCommendField);
            if (!string.IsNullOrEmpty(value))
            {
                return $" {value}";
            }
            return null;
        }

        public string GetComment(System.Reflection.MethodInfo m)
        {
            var ca = m.GetCustomAttribute<CommentAttribute>();
            if (ca != null)
            {
                return $" {ca.comment}";
            }

            string value = GetAttributeValue(m.GetCustomAttributes(true), 
                c_CommentAttribute, c_CommentAttributeCommendField);
            if (!string.IsNullOrEmpty(value))
            {
                return $" {value}";
            }
            return null;
        }

        public string GetDisplayName(System.Reflection.MethodInfo m)
        {
            var ca = m.GetCustomAttribute<DisplayNameAttribute>();
            if (ca != null)
            {
                return $" {ca.displayName}";
            }

            string value = GetAttributeValue(m.GetCustomAttributes(true),
                c_DisplayNameAttribute, c_DisplayNameAttributeDisplayNameField);
            if (!string.IsNullOrEmpty(value))
            {
                return $" {value}";
            }
            return null;
        }

        public string GetComment(System.Reflection.PropertyInfo p)
        {
            var ca = p.GetCustomAttribute<CommentAttribute>();
            if (ca != null)
            {
                return $" {ca.comment}";
            }

            string value = GetAttributeValue(p.GetCustomAttributes(true),
                c_CommentAttribute, c_CommentAttributeCommendField);
            if (value != null)
            {
                return $" {value}";
            }
            return string.Empty;
        }

        public string GetComment(System.Reflection.EventInfo p)
        {
            var ca = p.GetCustomAttribute<CommentAttribute>();
            if (ca != null)
            {
                return $" {ca.comment}";
            }

            string value = GetAttributeValue(p.GetCustomAttributes(true),
                c_CommentAttribute, c_CommentAttributeCommendField);
            if (value != null)
            {
                return $" {value}";
            }
            return string.Empty;
        }

        public string GetComment(System.Reflection.FieldInfo m)
        {
            var ca = m.GetCustomAttribute<CommentAttribute>();
            if (ca != null)
            {
                return $" {ca.comment}";
            }

            string value = GetAttributeValue(m.GetCustomAttributes(true),
                c_CommentAttribute, c_CommentAttributeCommendField);
            if (value != null)
            {
                return $" {value}";
            }
            return string.Empty;
        }

        public string GetCode(System.Reflection.MethodInfo m)
        {
            var ca = m.GetCustomAttribute<CodeAttribute>();
            if (ca != null)
            {
                return $" {ca.code}";
            }
            
            string value = GetAttributeValue(m.GetCustomAttributes(true), 
                c_CodeAttribute, c_CodeAttributeCodeField);
            if (!string.IsNullOrEmpty(value))
            {
                return $" {value}";
            }
            return null;
        }

        public string GetAttributeValue(object[] attributes, string attrFullName, string attrFieldName) 
        {
            if (attributes != null && attributes.Length > 0) 
            {
                foreach (object obj in attributes) 
                {
                    Attribute attr = obj as Attribute;
                    if (attr?.GetType().FullName == attrFullName)
                    {
                        Type attrType = attr.GetType();
                        System.Reflection.FieldInfo info = attrType.GetField(attrFieldName);
                        object value = info?.GetValue(attr);

                        if (value != null) {
                            return (string)value;
                        }
                    }   
                }
            }
            
            return string.Empty;
        }

        public void GetOperationOverloadCode(string name, string[] paramNames, StringBuilder sb)
        {
            switch (name)
            {
                case "op_UnaryPlus":
                    sb.Append($"---@code +({paramNames[0]}|{paramNames[0]})\n");
                    break;
                case "op_UnaryNegation":
                    sb.Append($"---@code -({paramNames[0]}|{paramNames[0]})\n");
                    break;
                case "op_Increment":
                    sb.Append($"---@code ++({paramNames[0]}|{paramNames[0]})\n");
                    break;
                case "op_Decrement":
                    sb.Append($"---@code --({paramNames[0]}|{paramNames[0]})\n");
                    break;
                case "op_LogicalNot":
                    sb.Append($"---@code !({paramNames[0]}|{paramNames[0]})\n");
                    break;
                case "op_Addition":
                    sb.Append($"---@code ({paramNames[0]}|{paramNames[0]}) + ({paramNames[1]}|{paramNames[1]})\n");
                    break;
                case "op_Subtraction":
                    sb.Append($"---@code ({paramNames[0]}|{paramNames[0]}) - ({paramNames[1]}|{paramNames[1]})\n");
                    break;
                case "op_Multiply":
                    sb.Append($"---@code ({paramNames[0]}|{paramNames[0]}) * ({paramNames[1]}|{paramNames[1]})\n");
                    break;
                case "op_Division":
                    sb.Append($"---@code ({paramNames[0]}|{paramNames[0]}) / ({paramNames[1]}|{paramNames[1]})\n");
                    break;
                case "op_BitwiseAnd":
                    sb.Append($"---@code ({paramNames[0]}|{paramNames[0]}) & ({paramNames[1]}|{paramNames[1]})\n");
                    break;
                case "op_BitwiseOr":
                    sb.Append($"---@code ({paramNames[0]}|{paramNames[0]}) | ({paramNames[1]}|{paramNames[1]})\n");
                    break;
                case "op_ExclusiveOr":
                    sb.Append($"---@code ({paramNames[0]}|{paramNames[0]}) ^ ({paramNames[1]}|{paramNames[1]})\n");
                    break;
                case "op_OnesComplement":
                    sb.Append($"---@code ~({paramNames[0]}|{paramNames[0]})\n");
                    break;
                case "op_Equality":
                    sb.Append($"---@code ({paramNames[0]}|{paramNames[0]}) == ({paramNames[1]}|{paramNames[1]})\n");
                    break;
                case "op_Inequality":
                    sb.Append($"---@code ({paramNames[0]}|{paramNames[0]}) != ({paramNames[1]}|{paramNames[1]})\n");
                    break;
                case "op_LessThan":
                    sb.Append($"---@code ({paramNames[0]}|{paramNames[0]}) < ({paramNames[1]}|{paramNames[1]})\n");
                    break;
                case "op_GreaterThan":
                    sb.Append($"---@code ({paramNames[0]}|{paramNames[0]}) > ({paramNames[1]}|{paramNames[1]})\n");
                    break;
                case "op_LessThanOrEqual":
                    sb.Append($"---@code ({paramNames[0]}|{paramNames[0]}) <= ({paramNames[1]}|{paramNames[1]})\n");
                    break;
                case "op_GreaterThanOrEqual":
                    sb.Append($"---@code ({paramNames[0]}|{paramNames[0]}) >= ({paramNames[1]}|{paramNames[1]})\n");
                    break;
                case "op_LeftShift":
                    sb.Append($"---@code ({paramNames[0]}|{paramNames[0]}) << ({paramNames[1]}|{paramNames[1]})\n");
                    break;
                case "op_RightShift":
                    sb.Append($"---@code ({paramNames[0]}|{paramNames[0]}) >> ({paramNames[1]}|{paramNames[1]})\n");
                    break;
                case "op_Modulus":
                    sb.Append($"---@code ({paramNames[0]}|{paramNames[0]}) % ({paramNames[1]}|{paramNames[1]})\n");
                    break;
            }
        }

        public string GetReturnComment(System.Reflection.MethodInfo m)
        {
            if (m.ReturnParameter.GetCustomAttributes(true).Length == 0)
                return string.Empty;
            var ca = m.ReturnParameter?.GetCustomAttribute<CommentAttribute>();
            if (ca != null)
            {
                return $" {ca.comment}";
            }
            else
            {
                return string.Empty;
            }
        }

        protected override void GenMethod(string name, MethodDataSet methodDataSet)
        {
            //overload
            if (methodDataSet.MethodList.Count > 1)
            {
                for (var j = 1; j < methodDataSet.MethodList.Count; j++)
                {
                    var data = methodDataSet.MethodList[j];
                    var mi = data.Method;
                    if (!GetNReflectAttr(mi))
                        continue;
                    string desc = GetMethodDescriptionStr(mi);
                    string comment = GetComment(mi);
                    string dispalyName = GetDisplayName(mi);
                    string code = GetCode(mi);
                    if (!string.IsNullOrEmpty(desc) || !string.IsNullOrEmpty(comment))
                    {
                        _methodBuilder.AppendFormat("---@public {0}{1}\n", desc, comment);
                    }
                    if (!string.IsNullOrEmpty(code))
                    {
                        //_methodBuilder.AppendFormat("---@code {0}\n", code);//暂不支持重载的code
                    }
                    if (!string.IsNullOrEmpty(dispalyName))
                    {
                        _methodBuilder.AppendFormat("---@displayName{0}\n", dispalyName);
                    }
                    var parameters = mi.GetParameters();
                    int startIdx = data.IsExtend ? 1 : 0;
                    string[] paramNamesAndType = new string[parameters.Length - startIdx];
                    string[] paramNames = new string[parameters.Length - startIdx]; 
                    for (var i = startIdx; i < parameters.Length; i++)
                    {
                        var pi = parameters[i];
                        string strVarType = "";
                        if (pi.IsOut)
                            strVarType = $"{GetParamTypeStr(pi)}<out>";
                        else if (pi.ParameterType.IsGenericParameter)
                            strVarType = $"<{GetParamTypeStr(pi)}>";
                        else
                            strVarType = $"{GetParamTypeStr(pi)}";
                        paramNamesAndType[i - startIdx] = string.Format("{0}:{1}", pi.Name, strVarType);
                        paramNames[i - startIdx] = pi.Name;
                    }
                    GetOperationOverloadCode(name, paramNames, _methodBuilder);
                    _methodBuilder.AppendFormat("---@overload fun({0}):{1}\n", string.Join(", ", paramNamesAndType), mi.ReturnType.GetTypeStr());
                }
            }
            //main
            {
                var data = methodDataSet.MethodList[0];
                var mi = data.Method;
                if (!GetNReflectAttr(mi))
                    return;
                var parameters = mi.GetParameters();
                int startIdx = data.IsExtend ? 1 : 0;
                string[] paramNames = new string[parameters.Length - startIdx];
                for (var i = startIdx; i < parameters.Length; i++)
                {
                    var pi = parameters[i];

                    string strVarType = "";
                    if (pi.IsOut)
                        strVarType = $"{GetParamTypeStr(pi)}<out>";
                    else if (pi.ParameterType.IsGenericParameter)
                        strVarType = $"<{GetParamTypeStr(pi)}>";
                    else
                        strVarType = $"{GetParamTypeStr(pi)}";
                    
                    _methodBuilder.AppendFormat("---@param {0} {1}{2}\n", pi.Name, strVarType, GetComment(pi));
                    paramNames[i - startIdx] = pi.Name;
                }
                var returnType = mi.ReturnType;
                if (typeof(void) != returnType)
                {
                    string strVarType = returnType.IsGenericParameter ? $"<{returnType.GetTypeStr()}>" : $"{returnType.GetTypeStr()}";
                    _methodBuilder.AppendFormat("---@return {0} ret{1}\n", strVarType, GetReturnComment(mi));
                }
                string c = mi.IsStatic && !data.IsExtend ? "." : ":";
                string desc = GetMethodDescriptionStr(mi);
                string comment = GetComment(mi);
                string dispalyName = GetDisplayName(mi);
                string code = GetCode(mi);
                if (!string.IsNullOrEmpty(desc) || !string.IsNullOrEmpty(comment))
                {
                    _methodBuilder.AppendFormat("---@public {0}{1}\n", desc, comment);
                }
                if (!string.IsNullOrEmpty(code))
                {
                    _methodBuilder.AppendFormat("---@code{0}\n", code);
                }
                if (!string.IsNullOrEmpty(dispalyName))
                {
                    _methodBuilder.AppendFormat("---@displayName{0}\n", dispalyName);
                }
                GetOperationOverloadCode(name, paramNames, _methodBuilder);

                _methodBuilder.AppendFormat("function m{0}{1}({2}) end\n", c, mi.Name, string.Join(", ", paramNames));
            }
        }
    }

    static class TypeExtension
    {
        public static string GetTypeStr(this Type type)
        {
            if (typeof(IList).IsAssignableFrom(type))
            {
                Type valueType = null;
                if (typeof(Array).IsAssignableFrom(type))
                {
                    valueType = type.GetElementType();
                    if (valueType != null)
                        return "table<array<" + LuaMisc.GetTypeName(valueType) + ">>";
                }
                else
                {
                    valueType = type.GetGenericArguments()[0];
                    if (valueType != null)
                        return "table<list<" + LuaMisc.GetTypeName(valueType) + ">>";
                }
            }
            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                Type keyType = type.GetGenericArguments()[0];
                Type valueType = type.GetGenericArguments()[1];
                if(keyType != null && valueType != null)
                    return "table<map<" + LuaMisc.GetTypeName(keyType) + "," + LuaMisc.GetTypeName(valueType) + ">>";
            }
            if (typeof(ICollection).IsAssignableFrom(type))
            {
                return "table";
            }
            // 事件判断需要放在泛型前
            if (CSharpGenerator.IsBlueprintRecongnizeEvent(type))
            {
                return "BlueprintEvent";
            }
            if (type.IsGenericType)
            {
                var typeName = LuaMisc.GetTypeName(type);
                int pos = typeName.IndexOf("<", StringComparison.Ordinal);
                if (pos > 0)
                    return typeName.Substring(0, pos);
            }
            return LuaMisc.GetTypeName(type);
        }
    }


}
