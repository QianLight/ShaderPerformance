using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CFEngine.Editor
{
    public class MatScanTool : CommonToolTemplate
    {
        private List<Type> conditionTypes = new List<Type>();
        private List<Type> operationTypes = new List<Type>();
        private string[] conditionNames;
        private string[] operationNames;
        private int selectedConditionIndex;
        private int selectedOperationIndex;
        private readonly List<ICondition> conditions = new List<ICondition>();
        private readonly List<IOperation> operations = new List<IOperation>();
        private Vector2 conditionViewPos;
        private Vector2 operationViewPos;

        private readonly List<Material> matchMaterials = new List<Material>();
        private readonly SavedString directory = new SavedString($"{nameof(MatScanTool)}.{nameof(directory)}", "Assets");
        private Rect rect;

        private GUIPage<Material> materialPage;
        private GUIPage<string> referencePage;
        private Dictionary<string, List<Material>> dependencies = new Dictionary<string, List<Material>>();
        private readonly List<string> references = new List<string>();

        [AttributeUsage(AttributeTargets.Class)]
        private class RenameAtribute : Attribute
        {
            public string name;
            public RenameAtribute(string name)
            {
                this.name = name;
            }
        }
        
        #region Conditions

        public enum CompareOperator
        {
            Equal,
            Less,
            LessEqual,
            Large,
            LargeEqual,
            Near,
            NotNear,
        }

        public interface ICondition
        {
            bool Match(Material material);
            void GUI();
        }

        [RenameAtribute("路径条件")]
        public class FileCondition : ICondition
        {
            public string extension;
            public string prefix;
            public string suffix;
            public string folder;
            public string contains;

            public void GUI()
            {
                DrawItem("目录", ref folder);
                DrawItem("扩展名", ref extension);
                DrawItem("前缀", ref prefix);
                DrawItem("后缀", ref suffix);
                DrawItem("包含", ref contains);
                if (folder != null && !Directory.Exists(folder))
                    EditorGUILayout.HelpBox("目录不存在", MessageType.Error);
            }

            private void DrawItem(string name, ref string value)
            {
                GUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                GUILayout.Label(name, GUILayout.Width(50));
                bool newEnabled = EditorGUILayout.Toggle(value != null, GUILayout.Width(EditorGUIUtility.singleLineHeight));
                if (EditorGUI.EndChangeCheck())
                {
                    value = newEnabled
                        ? value ?? ""
                        : null;
                }
                if (newEnabled)
                {
                    value = EditorGUILayout.TextField(value);
                }
                GUILayout.EndHorizontal();
            }

            public bool Match(Material material)
            {
                string path = AssetDatabase.GetAssetPath(material);
                return Match(path);
            }

            public bool Match(string path)
            {
                if (extension != null && extension.ToLower() != Path.GetExtension(path).ToLower())
                {
                    return false;
                }
                if (prefix != null && !prefix.ToLower().StartsWith(Path.GetFileNameWithoutExtension(path).ToLower()))
                {
                    return false;
                }
                if (suffix != null && !suffix.ToLower().EndsWith(Path.GetFileNameWithoutExtension(path).ToLower()))
                {
                    return false;
                }
                if (folder != null && !path.ToLower().StartsWith(folder.ToLower()))
                {
                    return false;
                }
                if (contains != null && !path.ToLower().Contains(contains.ToLower()))
                {
                    return false;
                }
                return true;
            }
        }


        [RenameAtribute("Pass条件")]
        public class PassCondition : ICondition
        {
            public string passName;
            public bool enabled;

            public bool Match(Material material)
            {
                for (int i = 0; i < material.passCount; i++)
                {
                    string name = material.GetPassName(i);
                    if (name == passName)
                    {
                        return material.GetShaderPassEnabled(passName) == enabled;
                    }
                }

                return false;
            }

            public void GUI()
            {
                passName = EditorGUILayout.TextField(passName);
                enabled = EditorGUILayout.Toggle("启用", enabled);
            }
        }

        [RenameAtribute("引用条件")]
        public class ReferenceCondition : ICondition
        {
            private static readonly Dictionary<Material, List<string>> dependencyCache = new Dictionary<Material, List<string>>();
            private static readonly string[] empty = new string[0];
            public FileCondition referenceCondition = new FileCondition();
            private string initPath;

            private void Init()
            {
                if (string.IsNullOrEmpty(referenceCondition.folder))
                {
                    referenceCondition.folder = "Assets";
                }
                if (initPath != referenceCondition.folder)
                {
                    string[] files = Directory.GetFiles(referenceCondition.folder, "*", SearchOption.AllDirectories);
                    for (int i = 0; i < files.Length; i++)
                    {
                        string file = files[i];
                        if (i % 100 == 0 && EditorUtility.DisplayCancelableProgressBar("查找引用", $"({i}/{files.Length}) {file}", i / (float)files.Length))
                        {
                            EditorUtility.ClearProgressBar();
                            break;
                        }
                        if (!referenceCondition.Match(file))
                        {
                            continue;
                        }
                        string[] dependencies = AssetDatabase.GetDependencies(file, true);
                        foreach (string dependency in dependencies)
                        {
                            if (dependency.EndsWith(".mat"))
                            {
                                Material material = AssetDatabase.LoadAssetAtPath<Material>(dependency);
                                if (material)
                                {
                                    if (!dependencyCache.TryGetValue(material, out var list))
                                    {
                                        list = new List<string>();
                                        dependencyCache[material] = list;
                                    }
                                    list.Add(file);
                                }
                            }
                        }
                    }
                    EditorUtility.ClearProgressBar();
                    initPath = referenceCondition.folder;
                }
            }

            public void GUI()
            {
                referenceCondition.GUI();
                if (GUILayout.Button("清除搜索缓存"))
                {
                    initPath = null;
                }
            }

            public bool Match(Material material)
            {
                Init();
                if (dependencyCache.TryGetValue(material, out List<string> references))
                {
                    foreach (var reference in references)
                    {
                        if (referenceCondition.Match(reference))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            public static IReadOnlyList<string> GetReferences(Material material)
            {
                if (dependencyCache.TryGetValue(material, out List<string> result))
                {
                    return result;
                }
                return empty;
            }
        }

        [RenameAtribute("属性条件 - 数值(Float)")]
        public class FloatPropertyCondition : ICondition
        {
            public string propertyName;
            public float compareValue;
            public float nearValue;
            public CompareOperator comparer;

            public bool Match(Material material)
            {
                if (!material.HasProperty(propertyName))
                {
                    return false;
                }
                float value = material.GetFloat(propertyName);
                return Compare(value, compareValue, nearValue, comparer);
            }

            public void GUI()
            {
                propertyName = EditorGUILayout.TextField("属性名", propertyName);
                comparer = (CompareOperator)EditorGUILayout.EnumPopup("判断逻辑", comparer);
                compareValue = EditorGUILayout.FloatField("对比值", compareValue);
                if (comparer == CompareOperator.Near || comparer == CompareOperator.NotNear)
                {
                    nearValue = EditorGUILayout.FloatField("近似度", nearValue);
                }
            }
        }

        private static bool Compare(float value, float compareValue, float nearValue, CompareOperator compare)
        {
            switch (compare)
            {
                case CompareOperator.Equal:
                    return value == compareValue;
                case CompareOperator.Less:
                    return value < compareValue;
                case CompareOperator.LessEqual:
                    return value <= compareValue;
                case CompareOperator.Large:
                    return value > compareValue;
                case CompareOperator.LargeEqual:
                    return value >= compareValue;
                case CompareOperator.Near:
                    return Mathf.Abs(value - compareValue) <= nearValue;
                case CompareOperator.NotNear:
                    return Mathf.Abs(value - compareValue) > nearValue;
                default:
                    return false;
            }
        }

        [RenameAtribute("属性条件 - 向量(Vector)")]
        public class VectorCondition : ICondition
        {
            public enum VectorComponent
            {
                X,
                Y,
                Z,
                W
            }

            public string propertyName;
            public VectorComponent component;
            public float compareValue;
            public float nearValue;
            public CompareOperator comparer;

            public void GUI()
            {
                propertyName = EditorGUILayout.TextField("属性名", propertyName);
                component = (VectorComponent)EditorGUILayout.EnumPopup("通道", component);
                comparer = (CompareOperator)EditorGUILayout.EnumPopup("判断逻辑", comparer);
                compareValue = EditorGUILayout.FloatField("对比值", compareValue);
                if (comparer == CompareOperator.Near || comparer == CompareOperator.NotNear)
                {
                    nearValue = EditorGUILayout.FloatField("近似度", nearValue);
                }
            }

            public bool Match(Material material)
            {
                if (!material.HasProperty(propertyName))
                {
                    return false;
                }
                float value = material.GetVector(propertyName)[(int)component];
                return Compare(value, compareValue, nearValue, comparer);
            }
        }

        [RenameAtribute("属性条件 - 颜色(Color)")]
        public class ColorCondition : ICondition
        {
            public enum ColorComponent
            {
                R,
                G,
                B,
                A,
            }

            public string propertyName;
            public ColorComponent component;
            public float compareValue;
            public float nearValue;
            public CompareOperator comparer;

            public void GUI()
            {
                propertyName = EditorGUILayout.TextField("属性名", propertyName);
                component = (ColorComponent)EditorGUILayout.EnumPopup("通道", component);
                comparer = (CompareOperator)EditorGUILayout.EnumPopup("判断逻辑", comparer);
                compareValue = EditorGUILayout.FloatField("对比值", compareValue);
                if (comparer == CompareOperator.Near || comparer == CompareOperator.NotNear)
                {
                    nearValue = EditorGUILayout.FloatField("近似度", nearValue);
                }
            }

            public bool Match(Material material)
            {
                if (!material.HasProperty(propertyName))
                {
                    return false;
                }
                float value = material.GetColor(propertyName)[(int)component];
                return Compare(value, compareValue, nearValue, comparer);
            }
        }

        [RenameAtribute("属性条件 - 贴图(Texture)")]
        public class TextureCondition : ICondition
        {
            public enum ColorComponent
            {
                R,
                G,
                B,
                A,
            }

            public string propertyName;
            public Texture texture;
            public bool equals = true;

            public void GUI()
            {
                propertyName = EditorGUILayout.TextField("属性名", propertyName);
                texture = EditorGUILayout.ObjectField("Shader", texture, typeof(Texture), false) as Texture;
                equals = EditorGUILayout.Toggle("相等", equals);
            }

            public bool Match(Material material)
            {
                if (!material.HasProperty(propertyName))
                {
                    return false;
                }
                Texture value = material.GetTexture(propertyName);
                return (value == texture) == equals;
            }
        }

        [RenameAtribute("关键字条件(Keyword)")]
        public class KeywordCondition : ICondition
        {
            public string keyword;
            public bool contains;

            public bool Match(Material material)
            {
                return (Array.IndexOf(material.shaderKeywords, keyword) >= 0) == contains;
            }

            public void GUI()
            {
                keyword = EditorGUILayout.TextField("Keyword", keyword);
                contains = EditorGUILayout.Toggle("包含", contains);
            }
        }

        [RenameAtribute("Shader条件")]
        public class ShaderCondition : ICondition
        {
            public Shader shader;
            public bool equals = true;

            public void GUI()
            {
                shader = EditorGUILayout.ObjectField("Shader", shader, typeof(Shader), false) as Shader;
                equals = EditorGUILayout.Toggle("相等", equals);
            }

            public bool Match(Material material)
            {
                return (material.shader == shader) == equals;
            }
        }

        private static void GetTypeInfos(Type interfaceType, List<Type> conditionTypes, ref string[] conditionNames)
        {
            conditionTypes.Clear();
            Type[] types = interfaceType.Assembly.GetTypes();
            foreach (Type type in types)
            {
                foreach (var item in type.GetInterfaces())
                {
                    if (item == interfaceType)
                    {
                        conditionTypes.Add(type);
                        break;
                    }
                }
            }
            conditionNames = new string[conditionTypes.Count];
            for (int i = 0; i < conditionTypes.Count; i++)
            {
                var type = conditionTypes[i];
                var attribute = type.GetCustomAttribute<RenameAtribute>(true);
                conditionNames[i] = attribute == null ? type.Name : attribute.name;
            }
        }

        private void DrawConditions()
        {
            GUILayout.BeginVertical("box", GUILayout.MaxWidth(300));

            DrawCreateCondition();

            DrawDisplayConditions();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("搜索"))
            {
                Search();
            }

            GUILayout.EndVertical();
        }

        private void Search()
        {
            matchMaterials.Clear();
            string[] files = Directory.GetFiles(directory.Value, "*.mat", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];
                Material material = AssetDatabase.LoadAssetAtPath<Material>(file);
                if (material)
                {
                    bool match = true;
                    foreach (var condition in conditions)
                    {
                        if (!condition.Match(material))
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                    {
                        matchMaterials.Add(material);
                    }
                }
            }
            materialPage.page.Value = 0;
        }

        private void DrawDisplayConditions()
        {
            conditionViewPos = GUILayout.BeginScrollView(conditionViewPos);
            for (int i = 0; i < conditions.Count; i++)
            {
                GUILayout.BeginHorizontal("box");
                GUILayout.BeginVertical();
                Type type = conditions[i].GetType();
                RenameAtribute renameAttribute = type.GetAttribute<RenameAtribute>();
                string name = renameAttribute == null ? type.Name : renameAttribute.name;
                GUILayout.Box(name, GUILayout.ExpandWidth(true));
                conditions[i].GUI();
                GUILayout.EndVertical();
                if (GUILayout.Button("删除", GUILayout.Width(35)))
                {
                    conditions.RemoveAt(i--);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }

        private void DrawCreateCondition()
        {
            directory.Value = EditorGUILayout.TextField("搜索目录", directory.Value);

            GUILayout.BeginHorizontal();
            selectedConditionIndex = EditorGUILayout.Popup(selectedConditionIndex, conditionNames);
            if (GUILayout.Button("创建"))
            {
                ICondition condition = Activator.CreateInstance(conditionTypes[selectedConditionIndex]) as ICondition;
                conditions.Add(condition);
            }
            GUILayout.EndHorizontal();
        }
        #endregion

        #region Operation

        public interface IOperation
        {
            void StartProcess();
            bool IsReady(Material material, out string reason);
            bool Execute(Material material);

            bool IsFinish();
            void GUI();
        }

        [RenameAtribute("设置Shader")]
        public class OptionSetShader : IOperation
        {
            public Shader shader;

            public bool Execute(Material material)
            {
                int oldqueue = material.renderQueue;
                bool result = material.shader = shader;
                material.renderQueue = oldqueue;
                return result;
            }

            public bool IsFinish()
            {
                return true;
            }

            public void StartProcess()
            {
                
            }

            public bool IsReady(Material material, out string reason)
            {
                reason = null;
                return true;
            }

            public void GUI()
            {
                shader = EditorGUILayout.ObjectField("Shader", shader, typeof(Shader), false) as Shader;
            }
        }

        [RenameAtribute("开关Pass")]
        public class OptionSetPassEnable : IOperation
        {
            public string passName;
            public bool newState;

            public bool Execute(Material material)
            {
                for (int i = 0; i < material.passCount; i++)
                {
                    string passName = material.GetPassName(i);
                    if (passName != this.passName)
                        continue;
                    bool enabled = material.GetShaderPassEnabled(passName);
                    if (enabled != newState)
                    {
                        material.SetShaderPassEnabled(passName, newState);
                        EditorUtility.SetDirty(material);
                        return true;
                    }
                }
                return false;
            }

            public bool IsFinish()
            {
                return true;
            }

            public void StartProcess()
            {
                
            }

            public bool IsReady(Material material, out string reason)
            {
                reason = null;
                return true;
            }

            public void GUI()
            {
                passName = EditorGUILayout.TextField("Pass名", passName);
                newState = EditorGUILayout.Toggle("开关", newState);
            }
        }

        [RenameAtribute("统计贴图使用")]
        public class OptionCalTex : IOperation
        {
            public int num = 0;
            public string[] texName;
            private int oldNum = 0;
            public static List<MatTexPair> MatTexPairs;
            public void StartProcess()
            {
                MatTexPairs = new List<MatTexPair>();
            }

            public bool IsReady(Material material, out string reason)
            {
                reason = null;
                return true;
            }

            public bool Execute(Material material)
            {
                MatTexPair pair = new MatTexPair(){matName = material.name};
                pair.texName = new string[texName.Length];
                for (int i = 0; i < texName.Length; i++)
                {
                    pair.texName[i] = material.GetTexture(texName[i]).name;
                }
                MatTexPairs.Add(pair);
                return true;
            }

            public bool IsFinish()
            {
                var path = "Assets/test";
                StreamWriter sw = null;//流信息
                var now = string.Format("{0:yyyMMddHHmmss}", DateTime.Now);
                var name = $"MatTexComparer_{now}";
                FileInfo t = new FileInfo ( path+ "//" + name+".csv");
                sw = t.CreateText();
                string data = "Material";
                for (int i = 0; i < texName.Length; i++)
                {
                    data += " " + texName[i];
                }
                data += "\n";
                for (int i = 0; i < MatTexPairs.Count; i++)
                {
                    data += MatTexPairs[i].matName;
                    for (int j = 0; j < MatTexPairs[i].texName.Length; j++)
                    {
                        data += " " + MatTexPairs[i].texName[j];
                    }
                    data += "\n";
                }
                
                sw.Write(data);
                sw.Close ();//关闭流
                sw.Dispose ();//销毁流
                return true;
            }

            public void GUI()
            {
                num = EditorGUILayout.IntField("需要统计的贴图数量", num);
                if (oldNum != num)
                {
                    oldNum = num;
                    texName = new string[oldNum];
                }
                for (int i = 0; i < num; i++)
                {
                    texName[i] = EditorGUILayout.TextField($"{i}", texName[i]);
                }
                
            }

            public struct MatTexPair
            {
                public string matName;
                public string[] texName;
            }
        }

        [RenameAtribute("设置向量(Vector)")]
        public class OptionSetVector : IOperation
        {
            [Flags]
            public enum ComponentMask : uint
            {
                X = 1u << 0,
                Y = 1u << 1,
                Z = 1u << 2,
                W = 1u << 3,

                All = X | Y | Z | W,
            }

            public string propertyName;
            public Vector4 target;
            public ComponentMask mask = ComponentMask.All;

            public bool Execute(Material material)
            {
                var value = material.GetVector(propertyName);
                for (int i = 0; i < 4; i++)
                {
                    if (((uint)mask & (1u << i)) > 0)
                    {
                        value[i] = target[i];
                    }
                }
                material.SetVector(propertyName, value);
                return material;
            }

            public bool IsFinish()
            {
                return true;
            }

            public void StartProcess()
            {
                
            }

            public bool IsReady(Material material, out string reason)
            {
                bool success = material.HasProperty(propertyName);
                reason = success ? null : "属性名不存在";
                return success;
            }

            public void GUI()
            {
                propertyName = EditorGUILayout.TextField("属性名", propertyName);
                target = EditorGUILayout.Vector4Field("设置值", target);
                mask = (ComponentMask)EditorGUILayout.EnumFlagsField("修改通道", mask);
            }
        }
        
        [RenameAtribute("设置Keyword")]
        public class OptionSetKeyword : IOperation
        {
            public string propertyName;
            public bool setEnable;
            public string keyword;
            public bool Execute(Material material)
            {
                if (setEnable)
                {
                    material.EnableKeyword(keyword);
                }
                else
                {
                    material.DisableKeyword(keyword);
                }
                
                return material;
            }

            public bool IsFinish()
            {
                return true;
            }

            public void StartProcess()
            {
                
            }

            public bool IsReady(Material material, out string reason)
            {
                bool success = material.HasProperty(propertyName);
                reason = success ? null : "属性名不存在";
                return success;
            }

            public void GUI()
            {
                propertyName = EditorGUILayout.TextField("属性(开关)名", propertyName);
                keyword = EditorGUILayout.TextField("Keyword", keyword);
                setEnable = EditorGUILayout.Toggle("Set Enable", setEnable);
            }
        }


        [RenameAtribute("设置向量(Color)")]
        public class OptionSetColor : IOperation
        {
            [Flags]
            public enum ComponentMask : uint
            {
                R = 1u << 0,
                G = 1u << 1,
                B = 1u << 2,
                A = 1u << 3,

                All = R | G | B | A,
            }

            public string propertyName;
            public Color target;
            public ComponentMask mask = ComponentMask.All;
            private static readonly GUIContent colorFieldContent = new GUIContent("设置值");

            public bool Execute(Material material)
            {
                var value = material.GetVector(propertyName);
                for (int i = 0; i < 4; i++)
                {
                    if (((uint)mask & (1u << i)) > 0)
                    {
                        value[i] = target[i];
                    }
                }
                material.SetColor(propertyName, value);
                return material;
            }

            public bool IsFinish()
            {
                return true;
            }

            public void StartProcess()
            {
                
            }

            public bool IsReady(Material material, out string reason)
            {
                bool success = material.HasProperty(propertyName);
                reason = success ? null : "属性名不存在";
                return success;
            }

            public void GUI()
            {
                propertyName = EditorGUILayout.TextField("属性名", propertyName);
                target = EditorGUILayout.ColorField(colorFieldContent, target, true, true, true);
                mask = (ComponentMask)EditorGUILayout.EnumFlagsField("修改通道", mask);
            }
        }

        [RenameAtribute("设置数值(Float)")]
        public class OptionSetFloat : IOperation
        {
            public string propertyName;
            public float target;

            public bool Execute(Material material)
            {
                material.SetFloat(propertyName, target);
                return material;
            }

            public bool IsFinish()
            {
                return true;
            }

            public void StartProcess()
            {
                
            }

            public bool IsReady(Material material, out string reason)
            {
                bool success = material.HasProperty(propertyName);
                reason = success ? null : "属性名不存在";
                return success;
            }

            public void GUI()
            {
                propertyName = EditorGUILayout.TextField("属性名", propertyName);
                target = EditorGUILayout.FloatField("设置值", target);
            }
        }
        
        [RenameAtribute("重新保存")]
        public class OptionResave : IOperation
        {
            public bool Execute(Material material)
            {
                // EditorUtility.SetDirty不管用，修改序列化字段可以简单暴力地解决问题。
                Shader shader = material.shader;
                material.shader = null;
                material.shader = shader;
                return true;
            }

            public bool IsFinish()
            {
                return true;
            }

            public void StartProcess()
            {
                
            }

            public bool IsReady(Material material, out string reason)
            {
                reason = null;
                return true;
            }

            public void GUI()
            {
                EditorGUILayout.LabelField("重新保存材质");
            }
        }

        [RenameAtribute("移除无关keyword")]
        public class RemoveKeywords : IOperation
        {
            public string propertyName;
            private bool isToggle;
            public void StartProcess()
            {
                
            }

            public bool IsReady(Material material, out string reason)
            {
                reason = null;
                return true;
            }

            public bool Execute(Material material)
            {
                if (!material || !material.shader)
                    return true;
                Shader shader  = material.shader;
                if (!ShaderUtility.GetShaderInfo(shader, out ShaderUtility.ShaderInfo shaderInfo) || shaderInfo.keywords == null)
                    return false;
                List<string> keywords = new List<string>(material.shaderKeywords);
                bool dirty = false;
                bool propertyBool;
                if (isToggle)
                {
                    propertyBool = material.GetFloat(propertyName) == 0;
                }
                else
                {
                    propertyBool = false;
                }
                
                for (int i = 0; i < keywords.Count; i++)
                {
                    //Debug.Log(material.name +"   "+ keywords[i]+"  "+material.IsKeywordEnabled(keywords[i])+"   "+material.GetFloat(propertyName));
                    if (!shaderInfo.keywords.Contains(keywords[i])|| !material.IsKeywordEnabled(keywords[i]) || propertyBool)
                    {
                        dirty = true;
                        keywords.RemoveAt(i--);
                    }
                }

                if (dirty)
                {
                    material.shaderKeywords = keywords.ToArray();
                    EditorUtility.SetDirty(material);    
                }
                
                return true;
            }

            public bool IsFinish()
            {
                return true;
            }

            public void GUI()
            {
                EditorGUILayout.LabelField("移除无关keyword");
                isToggle = EditorGUILayout.Toggle("需要设置的Toggle", isToggle);
                if (isToggle)
                {
                    propertyName = EditorGUILayout.TextField("属性名", propertyName);
                }
                
            }
        }
        [RenameAtribute("设置队列")]
        public class SetQueue : IOperation
        {
            int queueInt;
            public void StartProcess()
            {
                
            }

            public bool IsReady(Material material, out string reason)
            {
                reason = null;
                return true;
            }

            public bool Execute(Material material)
            {
                material.renderQueue = queueInt;
                return material;
            }

            public bool IsFinish()
            {
                return true;
            }

            public void GUI()
            {
                EditorGUILayout.LabelField("设置队列");
                queueInt = EditorGUILayout.IntField("设置值", queueInt);
            }
        }

        private void DrawOperations()
        {
            GUILayout.BeginVertical("box", GUILayout.MaxWidth(300));

            DrawCreateOperation();

            DrawDisplayOperations();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("执行"))
            {
                ExecuteOperations();
            }

            GUILayout.EndVertical();
        }

        private void DrawDisplayOperations()
        {
            operationViewPos = GUILayout.BeginScrollView(operationViewPos);
            for (int i = 0; i < operations.Count; i++)
            {
                GUILayout.BeginHorizontal("box");
                GUILayout.BeginVertical();
                operations[i].GUI();
                GUILayout.EndVertical();
                if (GUILayout.Button("删除", GUILayout.Width(35)))
                {
                    operations.RemoveAt(i--);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }

        private void DrawCreateOperation()
        {
            GUILayout.BeginHorizontal();
            selectedOperationIndex = EditorGUILayout.Popup(selectedOperationIndex, operationNames);
            if (GUILayout.Button("创建"))
            {
                IOperation operation = Activator.CreateInstance(operationTypes[selectedOperationIndex]) as IOperation;
                operations.Add(operation);
            }
            GUILayout.EndHorizontal();
        }

        private void ExecuteOperations()
        {
            bool containsError = false;
            AssetDatabase.StartAssetEditing();
            foreach (var operation in operations)
            { 
                operation.StartProcess();   
            }
            for (int i = 0; i < matchMaterials.Count; i++)
            {
                Material material = matchMaterials[i];
                if (material)
                {
                    foreach (IOperation operation in operations)
                    {
                        if (!operation.IsReady(material, out string reason))
                        {
                            Debug.LogError($"材质批量操作失败，原因:{reason}");
                            containsError = true;
                        }
                        else
                        {
                            operation.Execute(material);
                        }
                    }
                    if (EditorUtility.DisplayCancelableProgressBar("材质批量修改", AssetDatabase.GetAssetPath(material), i / (float)matchMaterials.Count))
                    {
                        break;
                    }
                }
            }

            foreach (var operation in operations)
            {
                operation.IsFinish();
            }
            AssetDatabase.StopAssetEditing();
            EditorUtility.ClearProgressBar();
            if (containsError)
            {
                EditorUtility.DisplayDialog("批量操作部分失败", "批量操作过程中有设置失败的情况，具体请看Console窗口的报错。", "好的");
            }
            AssetDatabase.SaveAssets();
        }

        #endregion
        
        public override void OnInit()
        {
            base.OnInit();

            GetTypeInfos(typeof(ICondition), conditionTypes, ref conditionNames);
            GetTypeInfos(typeof(IOperation), operationTypes, ref operationNames);

            materialPage = new GUIPage<Material>($"{nameof(MatScanTool)}.{nameof(materialPage)}", "查找材质", GUIMatchedMaterials);
            referencePage = new GUIPage<string>($"{nameof(MatScanTool)}.{nameof(referencePage)}", "引用列表", GUIReferences);
        }

        private void GUIMatchedMaterials(IList<Material> datas, Material material, int index)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"{index:000} / {datas.Count:000}", GUILayout.Width(100));
            EditorGUILayout.ObjectField(material, typeof(Material), false);
            if (GUILayout.Button("查看引用", GUILayout.Width(100)))
            {
                references.Clear();
                references.AddRange(ReferenceCondition.GetReferences(material));
            }
            EditorGUILayout.EndHorizontal();
        }

        private void GUIReferences(IList<string> datas, string path, int index)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"{index:000} / {datas.Count:000}", GUILayout.Width(100));
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            EditorGUILayout.ObjectField(asset, typeof(Object), false);
            EditorGUILayout.EndHorizontal();
        }

        public override void DrawGUI(ref Rect rect)
        {
            this.rect = rect;

            base.DrawGUI(ref rect);

            GUILayout.BeginHorizontal();

            DrawConditions();

            GUILayout.Box("", GUILayout.ExpandHeight(true), GUILayout.Width(2));

            EditorGUILayout.BeginVertical("box");
            materialPage.Draw(matchMaterials);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("选中全部", GUILayout.MaxWidth(100)))
                    Selection.objects = matchMaterials.ToArray();
                if (GUILayout.Button("复制列表到文本", GUILayout.MaxWidth(100)))
                {
                    string nameList = "";
                    for (int i = 0; i < matchMaterials.Count; i++)
                    {
                        nameList += matchMaterials[i].name + '\n';
                    }

                    GUIUtility.systemCopyBuffer = nameList;
                }
            }
            EditorGUILayout.EndVertical();

            GUILayout.Box("", GUILayout.ExpandHeight(true), GUILayout.Width(2));

            GUILayout.BeginVertical(GUILayout.Width(400), GUILayout.ExpandHeight(true));
            if (GUILayout.Button("显示所有材质的引用"))
            {
                CollectAllReferences();
            }
            referencePage.Draw(references);
            GUILayout.EndVertical();

            DrawOperations();

            GUILayout.EndHorizontal();
        }

        private void CollectAllReferences()
        {
            dependencies.Clear();
            references.Clear();
            foreach (Material material in matchMaterials)
            {
                var materialReferences = ReferenceCondition.GetReferences(material);
                foreach (string reference in materialReferences)
                {
                    if (!dependencies.TryGetValue(reference, out var collection))
                    {
                        collection = new List<Material>();
                        dependencies[reference] = collection;
                        references.Add(reference);
                    }
                    collection.Add(material);
                }
            }
        }
    }
}