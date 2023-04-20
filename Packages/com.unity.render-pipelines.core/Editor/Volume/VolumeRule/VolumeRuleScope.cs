using System;
using System.Collections.Generic;
using System.Reflection;
using CFEngine;
using UnityEditor;
using UnityEngine.Profiling;

namespace UnityEngine.Rendering
{
    public class VolumeRuleScope : IDisposable
    {
        private static VolumeRuleConfig config;
        private static List<Type> types;
        private static Dictionary<SerializedPropertyType, List<Type>> typeCache;
        private static readonly List<Type> emptyTypes = new List<Type>();
        private static List<int> ruleList = new List<int>();
        private static Dictionary<Type, VolumeRuleAttribute> descMap = new Dictionary<Type, VolumeRuleAttribute>(); 

        private readonly SerializedProperty rulesSp;
        private readonly SerializedPropertyType type;
        private readonly string path;
        private readonly SerializedObject so;
        private Color rawColor;
        private readonly SerializedProperty property;
        public static bool showState { get; private set; }
        
        public static void ToggleShowState()
        {
            showState = !showState;
        }

        public VolumeRuleScope(SerializedProperty property, params GUILayoutOption[] options)
        {
            static void GetMathcedTypes(SerializedPropertyType type, ref Dictionary<SerializedPropertyType, List<Type>> typeCache, ref List<Type> result)
            {
                if (typeCache == null)
                {
                    typeCache = new Dictionary<SerializedPropertyType, List<Type>>();
                    Type[] allTypes = typeof(VolumeRuleConfig).Assembly.GetTypes();
                    foreach (Type t in allTypes)
                    {
                        if (t.IsSubclassOf(typeof(VolumeRule)) && !t.IsAbstract)
                        {
                            VolumeRuleAttribute attr = t.GetCustomAttribute<VolumeRuleAttribute>();
                            if (attr != null)
                            {
                                typeCache.ForceGetValue(attr.type).Add(t);
                                descMap[t] = attr;
                            }
                        }
                    }
                }

                if (!typeCache.TryGetValue(type, out result))
                {
                    result = emptyTypes;
                }
            }

            static void GetVolumeRules(SerializedPropertyType type, string path, ref List<int> ruleList)
            {
                ruleList.Clear();
                for (int i = 0; i < config.rules.Count; i++)
                {
                    VolumeRule rule = config.rules[i];
                    if (rule && descMap[rule.GetType()].type == type && rule.path == path)
                    {
                        ruleList.Add(i);
                    }       
                }
            }

            this.property = property;
            
            string configPath = "Packages/com.unity.render-pipelines.core/Editor/Volume/VolumeRuleConfig.asset";
            config = config == null ? AssetDatabase.LoadAssetAtPath<VolumeRuleConfig>(configPath) : config;
            type = property.propertyType;
            path = $"{property.serializedObject.targetObject.GetType()}.{property.propertyPath}";
            so = new SerializedObject(config);

            so.Update();
            rulesSp = so.FindProperty(nameof(VolumeRuleConfig.rules));
            GetMathcedTypes(type, ref typeCache, ref types);
            GetVolumeRules(type, path, ref ruleList);
            bool invalid = false;
            foreach (int index in ruleList)
            {
                invalid |= !config.rules[index].IsValid(property);
            }
            rawColor = GUI.color;
            GUI.color = invalid ? Color.red : rawColor;

            if (showState)
            {
                EditorGUILayout.BeginVertical("box", options);
            }
        }

        public void Dispose()
        {
            GUI.color = rawColor;

            if (showState)
            {
                int removeCount = 0;
                for (int i = 0; i < ruleList.Count; i++)
                {
                    int index = ruleList[i] - removeCount;
                    SerializedProperty ruleSp = rulesSp.GetArrayElementAtIndex(index);
                    if (ruleSp != null && ruleSp.objectReferenceValue)
                    {
                        VolumeRule rule = ruleSp.objectReferenceValue as VolumeRule;
                        SerializedObject subSo = new SerializedObject(rule);
                        EditorGUILayout.BeginHorizontal();
                        subSo.Update();
                        rule.GUI(subSo, property);
                        subSo.ApplyModifiedProperties();
                        Rect buttonRect = GUILayoutUtility.GetRect(1f, 17f, GUILayout.ExpandHeight(true));
                        if (GUI.Button(buttonRect, "X"))
                        {
                            rulesSp.DeleteArrayElementAtIndex(index);
                            rulesSp.DeleteArrayElementAtIndex(index);
                            AssetDatabase.RemoveObjectFromAsset(rule);
                            Object.DestroyImmediate(rule);
                            removeCount ++;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        rulesSp.DeleteArrayElementAtIndex(index);
                    }
                }
                
                if (types.Count > 0 && showState)
                {
                    string key = $"VolumeRuleSelection:{path}";
                    int index = EditorPrefs.GetInt(key);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUI.BeginChangeCheck();
                    string[] popup = new string[types.Count];
                    for (int i = 0; i < types.Count; i++)
                        popup[i] = types[i].GetCustomAttribute<VolumeRuleAttribute>().displayName;
                    Rect pupupRect = GUILayoutUtility.GetRect(1f, 17f);
                    index = EditorGUI.Popup(pupupRect, index, popup);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorPrefs.SetInt(key, index);
                    }
                    
                    Rect buttonRect = GUILayoutUtility.GetRect(1f, 17f, GUILayout.Width(17));
                    if (GUI.Button(buttonRect, EditorGUIUtility.TrTempContent("+")))
                    {
                        so.Update();
                        VolumeRule newRule = ScriptableObject.CreateInstance(types[index]) as VolumeRule;
                        newRule.name = path;
                        newRule.path = path;
                        rulesSp.InsertArrayElementAtIndex(rulesSp.arraySize);
                        SerializedProperty newRuleSp = rulesSp.GetArrayElementAtIndex(rulesSp.arraySize - 1);
                        AssetDatabase.AddObjectToAsset(newRule, config);
                        newRuleSp.objectReferenceValue = newRule;
                        so.ApplyModifiedProperties();
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }

                so.ApplyModifiedProperties();
                
                string configPath = AssetDatabase.GetAssetPath(config);
                Object[] rs = AssetDatabase.LoadAllAssetsAtPath(configPath);
                for (int i = 0; i < rs.Length; i++)
                {
                    Object asset = rs[i];
                    if (!asset || asset is VolumeRule && !config.rules.Contains(asset as VolumeRule))
                    {
                        AssetDatabase.RemoveObjectFromAsset(asset);
                        Object.DestroyImmediate(asset);
                    }
                }
                
                EditorGUILayout.EndVertical();
            }
        }
    }
}