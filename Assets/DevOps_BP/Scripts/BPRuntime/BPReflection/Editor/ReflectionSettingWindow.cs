//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using UnityEditor;
//using UnityEditor.PackageManager.UI;
//using UnityEngine;
//using static UnityEditor.EditorGUILayout;
//using System.Linq;

//namespace Blueprint.CSharpReflection
//{
//    public class ReflectionSettingWindow : EditorWindow
//    {
//        private List<NameSpaceIter> nameSpaceIterList = new List<NameSpaceIter>();
//        static ReflectionSettingWindow Window;
//        public bool IsChange = false;

//        public static ReflectionSettingWindow Instance
//        {
//            get { return Window; }
//        }

//        [MenuItem("Blueprint/Reflection Setting")]
//        public static void Open()
//        {
//            Window = GetWindow(typeof(ReflectionSettingWindow), false, "Reflection Setting") as ReflectionSettingWindow;
//            Window.Show();
//        }

//        private void Awake()
//        {
//            IsChange = false;
//            CreatAllBindTypes(GetAllDlls());
//            FilterAll();
//        }

//        static void Main()
//        {
//            Console.ReadLine();
//        }

//        private List<Assembly> GetAllDlls()
//        {
//            return AppDomain.CurrentDomain.GetAssemblies().ToList();
//            //var assemblys = new List<Assembly>();
//            //var currentPath = Directory.GetCurrentDirectory() + "\\Library\\ScriptAssemblies";
//            //foreach (var assetPath in Directory.GetFiles(currentPath, "*", SearchOption.AllDirectories))
//            //{
//            //    var lastPath = assetPath.Substring(assetPath.Length - 4, 4);
//            //    if (lastPath == ".dll")
//            //    {
//            //        var assDll = Assembly.LoadFile(assetPath);
//            //        if (assDll != null && !assemblys.Contains(assDll) && Filter(assDll))
//            //            assemblys.Add(assDll);
//            //    }
//            //}
//            //foreach (var assetPath in Directory.GetFiles("C:\\Program Files\\Unity\\Editor\\Data\\Managed", "*", SearchOption.AllDirectories))
//            //{
//            //    if (assetPath.Contains("Unity.Cecil")) continue;
//            //    var lastPath = assetPath.Substring(assetPath.Length - 4, 4);
//            //    if (lastPath == ".dll")
//            //    {
//            //        var assDll = Assembly.LoadFile(assetPath);
//            //        if (assDll != null && !assemblys.Contains(assDll) && Filter(assDll))
//            //            assemblys.Add(assDll);
//            //    }
//            //}
//            ////Debug.Log(assemblys.Count);
//            //var a = AppDomain.CurrentDomain.GetAssemblies();
//            //foreach(var assembly in a)
//            //{
//            //    if (assemblys.FirstOrDefault(aa => aa.FullName == assembly.FullName) == null)
//            //        Debug.Log(assembly.FullName);
//            //}
//            //return assemblys;
//        }

//        private void CreatAllBindTypes(List<Assembly> assemblys)
//        {
//            var propertyFalgs = BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly | BindingFlags.Static;
//            var fieldFalgs = BindingFlags.GetField | BindingFlags.SetField | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;
//            var methodFalgs = BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.DeclaredOnly;

//            foreach (var assbly in assemblys)
//            {
//                foreach (var type in assbly.GetTypes())
//                {
//                    var nameSpace = string.IsNullOrEmpty(type.Namespace) ? "空命名空间" : type.Namespace;
//                    NameSpaceIter spaceIter = nameSpaceIterList.Find(x => x.Name == nameSpace);
//                    if (spaceIter == null)
//                    {
//                        spaceIter = new NameSpaceIter() { Name = nameSpace };
//                        nameSpaceIterList.Add(spaceIter);
//                    }

//                    PropertyInfo[] properties = type?.GetProperties(propertyFalgs);
//                    FieldInfo[] fieldss = type?.GetFields(fieldFalgs);
//                    MethodInfo[] funcs = type?.GetMethods(methodFalgs);

//                    //结构
//                    if (!type.IsPrimitive && !type.IsEnum && type.IsValueType)
//                    {
//                        var structIter = new StructIter(type);
//                        foreach (var item in fieldss)
//                        {
//                            structIter.fields.Add(GetFieldStr(item));
//                        }
//                        foreach (var item in properties)
//                        {
//                            structIter.properties.Add(GetPropertyStr(item));
//                        }
//                        spaceIter.structes.Add(structIter);
//                    }
//                    //类
//                    else if (type.IsClass)
//                    {
//                        var classIter = new ClassIter(type);
//                        foreach (var item in fieldss)
//                        {
//                            classIter.fields.Add(GetFieldStr(item));
//                        }
//                        foreach (var item in properties)
//                        {
//                            classIter.properties.Add(GetPropertyStr(item));
//                        }
//                        foreach (var item in funcs)
//                        {
//                            classIter.functions.Add(GetMethodStr(item));
//                        }
//                        spaceIter.classes.Add(classIter);
//                    }
//                    //枚举
//                    else if (type.IsEnum)
//                    {
//                        var enumIter = new EnumIter(type);
//                        foreach (var item in fieldss)
//                        {
//                            enumIter.fields.Add(GetFieldStr(item));
//                        }
//                        spaceIter.enums.Add(enumIter);
//                    }
//                    else
//                        continue;
//                }
//            }

//            string GetFieldStr(FieldInfo fieldInfo)
//            {
//                return $"{fieldInfo.Name}({fieldInfo.FieldType.Name})";
//            }

//            string GetPropertyStr(PropertyInfo propertyInfo)
//            {
//                return $"{propertyInfo.Name}({propertyInfo.PropertyType.Name})";
//            }

//            string GetMethodStr(MethodInfo methodInfo)
//            {
//                var parames = methodInfo.GetParameters();
//                string ParamStr = "";
//                foreach (var param in parames)
//                {
//                    ParamStr += param.ParameterType.Name;
//                    if (parames[parames.Length - 1] != param)
//                        ParamStr += ", ";
//                }
//                return $"{methodInfo.Name}({ParamStr})";
//            }
//        }

//        Vector2 scrollPos;
//        private void OnGUI()
//        {
//            BeginHorizontal();
//            LabelField("Unity API", EditorStyles.boldLabel);
//            LabelField($"Unity Version: {Application.unityVersion}", EditorStyles.miniBoldLabel, GUILayout.Width(150));
//            EndHorizontal();

//            DrawSearch();
//            DrawData();
//            DrawSelection();
//        }

//        string m_InputSearchText = "";
//        GUIStyle TextFieldRoundEdge;
//        GUIStyle TextFieldRoundEdgeCancelButton;
//        GUIStyle TextFieldRoundEdgeCancelButtonEmpty;
//        GUIStyle TransparentTextField;
//        private void DrawSearch()
//        {
//            Space();
//            BeginVertical();
//            if (TextFieldRoundEdge == null)
//            {
//                TextFieldRoundEdge = new GUIStyle("SearchTextField");
//                TextFieldRoundEdgeCancelButton = new GUIStyle("SearchCancelButton");
//                TextFieldRoundEdgeCancelButtonEmpty = new GUIStyle("SearchCancelButtonEmpty");
//                TransparentTextField = new GUIStyle(EditorStyles.whiteLabel);
//                TransparentTextField.normal.textColor = EditorStyles.textField.normal.textColor;
//            }

//            Rect position = EditorGUILayout.GetControlRect();
//            GUIStyle textFieldRoundEdge = TextFieldRoundEdge;
//            GUIStyle transparentTextField = TransparentTextField;
//            GUIStyle gUIStyle = (m_InputSearchText != "") ? TextFieldRoundEdgeCancelButton : TextFieldRoundEdgeCancelButtonEmpty;

//            position.width -= gUIStyle.fixedWidth;
//            if (Event.current.type == EventType.Repaint)
//            {
//                if (string.IsNullOrEmpty(m_InputSearchText))
//                {
//                    textFieldRoundEdge.Draw(position, new GUIContent("     请输入(类名，结构名或者枚举名)"), 0);
//                }
//                else
//                {
//                    textFieldRoundEdge.Draw(position, new GUIContent(""), 0);
//                }
//                GUI.contentColor = Color.white;
//            }
//            Rect rect = position;
//            float num = textFieldRoundEdge.CalcSize(new GUIContent("")).x - 2f;
//            rect.x += num;

//            m_InputSearchText = EditorGUI.TextField(rect, m_InputSearchText, transparentTextField);
//            position.x += position.width;
//            position.width = gUIStyle.fixedWidth;
//            position.height = gUIStyle.fixedHeight;
//            if (GUI.Button(position, GUIContent.none, gUIStyle) && m_InputSearchText != "")
//            {
//                m_InputSearchText = "";
//                GUI.changed = true;
//                GUIUtility.keyboardControl = 0;
//            }
//            EndVertical();
//        }

//        private void DrawData()
//        {
//            Space();
//            BeginVertical();
//            scrollPos = BeginScrollView(scrollPos);

//            if (string.IsNullOrEmpty(m_InputSearchText))
//            {
//                foreach (var nameSpaceIter in nameSpaceIterList)
//                {
//                    DrawIter(nameSpaceIter, false);

//                    if (nameSpaceIter.isExpand)
//                    {
//                        if (nameSpaceIter.classes.Count > 0) Draw_CSE_Iter(nameSpaceIter, "Class");
//                        if (nameSpaceIter.classExpand)
//                        {
//                            foreach (var classIter in nameSpaceIter.classes)
//                            {
//                                DrawIter(classIter);
//                                if (classIter.isExpand)
//                                    DrawMemberIter(classIter);
//                            }
//                        }

//                        if (nameSpaceIter.structes.Count > 0) Draw_CSE_Iter(nameSpaceIter, "Struct");
//                        if (nameSpaceIter.structExpand)
//                        {
//                            foreach (var structIter in nameSpaceIter.structes)
//                            {
//                                DrawIter(structIter);
//                                if (structIter.isExpand)
//                                    DrawMemberIter(structIter);
//                            }
//                        }

//                        if (nameSpaceIter.enums.Count > 0) Draw_CSE_Iter(nameSpaceIter, "Enum");
//                        if (nameSpaceIter.enumExpand)
//                        {
//                            foreach (var enumIter in nameSpaceIter.enums)
//                            {
//                                DrawIter(enumIter);
//                                if (enumIter.isExpand)
//                                    DrawMemberIter(enumIter);
//                            }
//                        }
//                    }
//                }
//            }
//            else
//            {
//                foreach (var nameSpaceIter in nameSpaceIterList)
//                {
//                    foreach (var item in nameSpaceIter.allMembers)
//                    {
//                        if (item.Name.ToLower().Contains(m_InputSearchText.ToLower()))
//                        {
//                            DrawIter(item, false);
//                            if (item.isExpand)
//                                DrawMemberIter(item, 15, 30);
//                        }
//                    }
//                }
//            }

//            EndScrollView();
//            EndVertical();
//        }

//        private void DrawSelection()
//        {
//            Space();
//            BeginVertical();
//            BeginHorizontal();
//            if (GUILayout.Button("CustomReflection Setting"))
//            {
//                System.Diagnostics.Process.Start($"{Directory.GetCurrentDirectory()}\\Assets\\Scripts\\BPRuntime\\BPReflection\\Editor\\CSharpCustomReflectionSettings.cs");
//            }
//            var content = new GUIContent();
//            content.text = IsChange ? "反射" : "关闭";
//            if (GUILayout.Button(content))
//            {
//                if (content.text == "反射")
//                {
//                    HashSet<BindType> types = new HashSet<BindType>();
//                    foreach (var nameSpace in nameSpaceIterList)
//                    {
//                        foreach (var item in nameSpace.classes)
//                        {
//                            if (item.Enable) types.Add(new BindType(item.onerType));
//                        }
//                        foreach (var item in nameSpace.structes)
//                        {
//                            if (item.Enable) types.Add(new BindType(item.onerType));
//                        }
//                        foreach (var item in nameSpace.enums)
//                        {
//                            if (item.Enable) types.Add(new BindType(item.onerType));
//                        }
//                    }
//                    foreach (var item in CSharpReflectionSettings.customClassList)
//                    {
//                        types.Add(item);
//                    }
//                    CSharpAPIGenerator.DoIt(types.ToArray());
//                }
//                else
//                {
//                    Close();
//                }
//            }
//            //if (GUILayout.Button("全选"))
//            //{
//            //    foreach (var nameSpace in nameSpaceIterList)
//            //    {
//            //        nameSpace.Enable = !nameSpace.Enable;
//            //    }
//            //}
//            BeginHorizontal();
//            EndVertical();

//            BeginVertical();
//            LabelField("");
//            Space();
//            EndVertical();
//            if (Window != null) Show();
//        }

//        private void DrawMemberIter(Iter iter, float pix1 = 45f, float pix2 = 75f)
//        {
//            if (iter is ClassIter classIter)
//            {
//                if (classIter.fields.Count > 0) Draw(iter, "字段");
//                if (classIter.fieldExpand)
//                {
//                    foreach (var field in classIter.fields)
//                    {
//                        DrawLable(field);
//                    }
//                }

//                if (classIter.properties.Count > 0) Draw(iter, "属性");
//                if (classIter.propertyExpand)
//                {
//                    foreach (var field in classIter.properties)
//                    {
//                        DrawLable(field);
//                    }
//                }

//                if (classIter.functions.Count > 0) Draw(iter, "函数");
//                if (classIter.functionExpand)
//                {
//                    foreach (var field in classIter.functions)
//                    {
//                        DrawLable(field);
//                    }
//                }
//            }
//            else if (iter is StructIter structIter)
//            {
//                if (structIter.fields.Count > 0) Draw(iter, "字段");
//                if (structIter.fieldExpand)
//                {
//                    foreach (var field in structIter.fields)
//                    {
//                        DrawLable(field);
//                    }
//                }

//                if (structIter.properties.Count > 0) Draw(iter, "属性");
//                if (structIter.propertyExpand)
//                {
//                    foreach (var field in structIter.properties)
//                    {
//                        DrawLable(field);
//                    }
//                }
//            }
//            else if (iter is EnumIter enumIter)
//            {
//                if (enumIter.fields.Count > 0) Draw(iter, "字段");
//                if (enumIter.fieldExpand)
//                {
//                    foreach (var field in enumIter.fields)
//                    {
//                        DrawLable(field);
//                    }
//                }
//            }
//            else
//                return;

//            void Draw(Iter iterMember, string str)
//            {
//                BeginHorizontal();
//                GUILayout.Space(pix1);
//                var content = new GUIContent(str);
//                if (str == "字段")
//                {
//                    iterMember.fieldExpand = BeginFoldoutHeaderGroup(iterMember.fieldExpand, content);
//                }
//                else if (str == "属性")
//                {
//                    iterMember.propertyExpand = BeginFoldoutHeaderGroup(iterMember.propertyExpand, content);
//                }
//                else if (str == "函数")
//                {
//                    iterMember.functionExpand = BeginFoldoutHeaderGroup(iterMember.functionExpand, content);
//                }
//                EndHorizontal();
//                EndFoldoutHeaderGroup();
//            }

//            void DrawLable(string name)
//            {
//                BeginHorizontal();
//                GUILayout.Space(pix2);
//                var content = new GUIContent(name);
//                LabelField(name);
//                EndHorizontal();
//            }
//        }

//        private void DrawIter(Iter item, bool isChild = true)
//        {
//            BeginHorizontal();
//            if (isChild) GUILayout.Space(30);
//            var content = new GUIContent(item.Name);
//            item.isExpand = BeginFoldoutHeaderGroup(item.isExpand, content);
//            item.Enable = Toggle(item.Enable, GUILayout.Width(30));
//            EndHorizontal();
//            EndFoldoutHeaderGroup();
//        }

//        private void Draw_CSE_Iter(NameSpaceIter nameSpaceIter, string str)
//        {
//            BeginHorizontal();
//            GUILayout.Space(15);
//            var content = new GUIContent(str);
//            if (str == "Class")
//            {
//                nameSpaceIter.classExpand = BeginFoldoutHeaderGroup(nameSpaceIter.classExpand, content);
//                nameSpaceIter.ClassEnable = Toggle(nameSpaceIter.ClassEnable, GUILayout.Width(30));
//            }
//            else if (str == "Enum")
//            {
//                nameSpaceIter.enumExpand = BeginFoldoutHeaderGroup(nameSpaceIter.enumExpand, content);
//                nameSpaceIter.EnumEnable = Toggle(nameSpaceIter.EnumEnable, GUILayout.Width(30));
//            }
//            else if (str == "Struct")
//            {
//                nameSpaceIter.structExpand = BeginFoldoutHeaderGroup(nameSpaceIter.structExpand, content);
//                nameSpaceIter.StructEnable = Toggle(nameSpaceIter.StructEnable, GUILayout.Width(30));
//            }
//            else return;
//            EndHorizontal();
//            EndFoldoutHeaderGroup();
//        }

//        private bool Filter(Assembly assembly)
//        {
//            var assemblyKeyWord = new string[]
//            {
//                "Assembly-CSharp-Editor",
//            };

//            foreach (var item in assemblyKeyWord)
//            {
//                if (assembly.FullName.Contains(item))
//                    return false;
//            }
//            return true;
//        }

//        private bool Filter(string nameSpace)
//        {
//            if (string.IsNullOrEmpty(nameSpace) || nameSpace == null)
//                return true;

//            //命名空间过滤（全字符串匹配）
//            var nameSpaceKeyWord = new string[]
//            {
//                "Blueprint",
//                "Blueprint.Logic",
//                "Blueprint.UnityLogic",
//                "Blueprint.CSharpReflection",
//                "Blueprint.ActorEditor",
//                "Blueprint.Actor.EventSystem",
//                "Blueprint.ActorAsset",

//                "Unity.Services.Core.Editor",
//                "UnityEditor.TestTools",
//                "UnityEditor.Timeline",
//                "UnityEngine.AssetGraph",

//                "PENet",
//                "Packages.Rider.Editor",
//                "LitJson",
//                "NiceJson",
//                "SimpleJson",
//                "SimpleJson.Reflection",
//                "Util",
//                "Utils",
//                "Utils.Pool",
//                "Collections",
//                "Buffer",

//            };

//            //包含
//            var containsKeyWord = new string[]
//            {
//                "Zeus",
//                "UnityEditor.TestRunner",
//                "UnityEditor.TestTools",
//                "UnityEngine.TestRunner",
//                "UnityEngine.TestTools",
//            };

//            foreach (var item in nameSpaceKeyWord)
//            {
//                if (nameSpace == item)
//                    return false;
//            }

//            foreach (var item in containsKeyWord)
//            {
//                if (nameSpace.Contains(item))
//                    return false;
//            }

//            return true;
//        }

//        private bool Filter(Iter iter)
//        {
//            //名称（全字符串匹配）
//            var nameKeyWord = new string[]
//            {
//                "ParticleEmitter",
//                "ParticleAnimator",
//                "ParticleRenderer",
//            };

//            foreach (var item in nameKeyWord)
//            {
//                if (iter.Name == item)
//                    return false;
//            }

//            var type = (iter as IIter).onerType;
//            if (iter.Name.Length >= 2)
//            {
//                if (iter.Name.Substring(0, 2) == "<>")
//                    return false;
//                if (iter.Name.Substring(iter.Name.Length - 2, 2) == "`1")
//                    return false;
//                if (iter.Name.Contains("<") && iter.Name.Contains(">"))
//                    return false;
//                if (iter.Name.Contains("__"))
//                    return false;
//                if (iter.Name.Contains("="))
//                    return false;
//            }
//            if (typeof(System.MulticastDelegate).IsAssignableFrom(type))
//                return false;
//            if (type.IsGenericType)
//                return false;
//            return true;
//        }

//        private void FilterAll()
//        {
//            for (int i = nameSpaceIterList.Count - 1; i >= 0; i--)
//            {
//                var item = nameSpaceIterList[i];
//                if (!Filter(item.Name)) nameSpaceIterList.RemoveAt(i);
//            }
//            foreach (var item in nameSpaceIterList)
//            {
//                for (int i = item.classes.Count - 1; i >= 0; i--)
//                {
//                    if (!Filter(item.classes[i])) item.classes.RemoveAt(i);
//                }
//                for (int i = item.structes.Count - 1; i >= 0; i--)
//                {
//                    if (!Filter(item.structes[i])) item.structes.RemoveAt(i);
//                }
//                for (int i = item.enums.Count - 1; i >= 0; i--)
//                {
//                    if (!Filter(item.enums[i])) item.enums.RemoveAt(i);
//                }
//            }
//        }
//    }

//    public class Iter
//    {
//        public string Name = "";
//        public bool isExpand = false;

//        private bool enable = false;
//        public virtual bool Enable
//        {
//            get => enable;
//            set
//            {
//                enable = value;
//                if (enable == true)
//                    ReflectionSettingWindow.Instance.IsChange = true;
//            }
//        }

//        public bool fieldExpand = false;
//        public bool propertyExpand = false;
//        public bool functionExpand = false;

//        public virtual int AllMembersCount { get; }
//    }

//    interface IIter
//    {
//        public Type onerType { get; set; }
//    }


//    public class NameSpaceIter : Iter
//    {
//        private bool enable = false;
//        public override bool Enable
//        {
//            get => enable;
//            set
//            {
//                if (enable != value)
//                {
//                    enable = value;
//                    ClassEnable = value;
//                    StructEnable = value;
//                    EnumEnable = value;
//                }

//                if (enable == true)
//                    ReflectionSettingWindow.Instance.IsChange = true;
//            }
//        }

//        public bool classEnable = false;
//        public bool structEnable = false;
//        public bool enumEnable = false;
//        public bool ClassEnable
//        {
//            get => classEnable;
//            set
//            {
//                if (classEnable != value)
//                {
//                    classEnable = value;
//                    foreach (var item in classes)
//                    {
//                        item.Enable = value;
//                    }
//                }
//            }
//        }
//        public bool StructEnable
//        {
//            get => structEnable;
//            set
//            {
//                if (structEnable != value)
//                {
//                    structEnable = value;
//                    foreach (var item in structes)
//                    {
//                        item.Enable = value;
//                    }
//                }
//            }
//        }
//        public bool EnumEnable
//        {
//            get => enumEnable;
//            set
//            {
//                if (enumEnable != value)
//                {
//                    enumEnable = value;
//                    foreach (var item in enums)
//                    {
//                        item.Enable = value;
//                    }
//                }
//            }
//        }


//        public bool classExpand = false;
//        public bool structExpand = false;
//        public bool enumExpand = false;

//        public List<ClassIter> classes = new List<ClassIter>();
//        public List<StructIter> structes = new List<StructIter>();
//        public List<EnumIter> enums = new List<EnumIter>();
//        public List<Iter> allMembers => GetAllMembers();
//        public override int AllMembersCount => allMembers.Count;
//        protected List<Iter> GetAllMembers()
//        {
//            List<Iter> members = new List<Iter>();
//            foreach (var item in classes)
//            {
//                members.Add(item);
//            }
//            foreach (var item in structes)
//            {
//                members.Add(item);
//            }
//            foreach (var item in enums)
//            {
//                members.Add(item);
//            }
//            return members;
//        }

//    }

//    public class ClassIter : Iter, IIter
//    {
//        public ClassIter(Type type)
//        {
//            Name = type.Name;
//            onerType = type;
//        }
//        public Type onerType { get; set; }
//        public List<string> fields = new List<string>();
//        public List<string> properties = new List<string>();
//        public List<string> functions = new List<string>();
//        public override int AllMembersCount => fields.Count + properties.Count + functions.Count;
//    }

//    public class StructIter : Iter, IIter
//    {
//        public StructIter(Type type)
//        {
//            Name = type.Name;
//            onerType = type;
//        }
//        public Type onerType { get; set; }
//        public List<string> fields = new List<string>();
//        public List<string> properties = new List<string>();
//        public override int AllMembersCount => fields.Count + properties.Count;
//    }

//    public class EnumIter : Iter, IIter
//    {
//        public EnumIter(Type type)
//        {
//            Name = type.Name;
//            onerType = type;
//        }
//        public Type onerType { get; set; }
//        public List<string> fields = new List<string>();
//        public override int AllMembersCount => fields.Count;
//    }


//    public class TTT
//    {
//        [MenuItem("Blueprint/Reflection Setting TTTT")]
//        public static void Open()
//        {
//            var a = AppDomain.CurrentDomain.GetAssemblies();
//            foreach (var aa in a)
//            {
//                Debug.Log(aa.FullName);
//            }
//        }
//    }

//}
