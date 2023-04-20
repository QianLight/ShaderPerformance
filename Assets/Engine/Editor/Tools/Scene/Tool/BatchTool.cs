using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class BatchToolConfig<TProcessor, TContext> : ScriptableObject
    where TProcessor : BatchToolProcessor<TContext>
    where TContext : BatchToolContext
{
    public List<TProcessor> processors = new List<TProcessor>();
}

public class BatchToolContext
{
}

public abstract class BatchTool<TConfig, TProcessor, TContext>
    where TConfig : BatchToolConfig<TProcessor, TContext>
    where TProcessor : BatchToolProcessor<TContext>
    where TContext : BatchToolContext, new()
{
    public bool Initialized { get; private set; }
    protected TConfig config { get; private set; }
    private List<Type> types;
    private string[] typeNames;
    private SavedInt selectedIndex;

    public List<Type> GetSubTypes(Type rootType)
    {
        List<Type> result = new List<Type>();
        Type[] types = rootType.Assembly.GetTypes();
        foreach (Type type in types)
        {
            if (type.IsSubclassOf(rootType) && !type.IsAbstract)
            {
                result.Add(type);
            }
        }

        return result;
    }

    public string[] GetNameArray()
    {
        string[] names = new string[types.Count];
        for (int i = 0; i < types.Count; i++)
        {
            Type type = types[i];
            TProcessor instance = ScriptableObject.CreateInstance(type) as TProcessor;
            string name = (string) instance.GetType().GetProperty("Name").GetValue(instance);
            names[i] = name;
        }

        return names;
    }

    public void Init()
    {
        Initialized = true;
        config = ScriptableObject.CreateInstance<TConfig>();
        selectedIndex = new SavedInt($"{GetType()}.{nameof(selectedIndex)}");
        types = GetSubTypes(typeof(TProcessor));
        typeNames = GetNameArray();
    }

    public virtual void OnGUI()
    {
        config = EditorGUILayout.ObjectField(config, typeof(TConfig), false) as TConfig;
        if (!config)
        {
            if (GUILayout.Button("新建配置"))
            {
                config = ScriptableObject.CreateInstance<TConfig>();
            }
        }
        else if (AssetDatabase.Contains(config))
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("新建配置"))
            {
                config = ScriptableObject.CreateInstance<TConfig>();
            }

            if (GUILayout.Button("保存配置"))
            {
                foreach (TProcessor processor in config.processors)
                    if (processor && !AssetDatabase.Contains(processor))
                        AssetDatabase.AddObjectToAsset(processor, config);
                string path = AssetDatabase.GetAssetPath(config);
                Object[] savedProcessors = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (Object asset in savedProcessors)
                    if (asset is TProcessor savedProcessor && !config.processors.Contains(savedProcessor))
                        Object.DestroyImmediate(savedProcessor);
                EditorUtility.SetDirty(config);
                AssetDatabase.SaveAssetIfDirty(config);
            }
            EditorGUILayout.EndHorizontal();
        }
        else if (GUILayout.Button("保存配置"))
        {
            string path = EditorUtility.SaveFilePanel("保存配置", "Assets/Engine/Editor/EditorResources/",
                config.GetType().Name, "asset");
            if (!string.IsNullOrEmpty(path))
            {
                path = path.Substring(Application.dataPath.Length - "Assets".Length);
                AssetDatabase.CreateAsset(config, path);
                foreach (TProcessor processor in config.processors)
                    if (processor)
                        AssetDatabase.AddObjectToAsset(processor, config);
                Selection.activeObject = config;
            }
        }

        EditorGUILayout.BeginHorizontal();
        selectedIndex.Value = EditorGUILayout.Popup(selectedIndex.Value, typeNames);
        if (GUILayout.Button("Add", GUILayout.MaxWidth(80)))
        {
            TProcessor processor = ScriptableObject.CreateInstance(types[selectedIndex.Value]) as TProcessor;
            config.processors.Add(processor);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical();


        for (int i = 0; i < config.processors.Count; i++)
        {
            TProcessor processor = config.processors[i];
            if (!processor)
            {
                EditorGUILayout.LabelField("Null");
            }
            else
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(processor.Name);
                if (GUILayout.Button("Delete", GUILayout.MaxWidth(80)))
                {
                    config.processors.RemoveAt(i--);
                }

                EditorGUILayout.EndHorizontal();
                processor.OnGUI();
                EditorGUILayout.EndVertical();
            }
        }

        EditorGUILayout.EndVertical();

        if (GUILayout.Button("开刷", GUILayout.MaxWidth(180)))
            Process();
    }

    public void Process()
    {
        TContext context = new TContext();
        OnPreProcess(context);
        foreach (TProcessor processor in config.processors)
            processor.Process(context);
        OnPostProcess(context);
    }

    protected virtual void OnPostProcess(TContext context)
    {
    }

    protected virtual void OnPreProcess(TContext context)
    {
    }
}

public abstract class BatchToolProcessor<TContext> : ScriptableObject
    where TContext : BatchToolContext
{
    public abstract string Name { get; }

    public virtual void Process(TContext context)
    {
    }

    public virtual void OnGUI()
    {
    }
}