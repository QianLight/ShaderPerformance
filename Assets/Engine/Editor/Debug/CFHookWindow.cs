using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CFHookWindow : EditorWindow
{
    private static CFDebugHook[] hooks;

    [MenuItem("XEditor/C#钩子编辑器 CFHookWindow")]
    public static void Open()
    {
        CFHookWindow window = GetWindow<CFHookWindow>();
        window.titleContent = new GUIContent("C#钩子编辑器 CFHookWindow");
        window.Show();
    }

    private void OnGUI()
    {
        foreach (CFDebugHook hook in hooks)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(hook.Name, GUILayout.ExpandWidth(true));
            hook.Enabled = EditorGUILayout.Toggle(hook.Enabled, GUILayout.Width(18));
            EditorGUILayout.EndHorizontal();
            
            if (hook.Enabled)
            {
                EditorGUI.indentLevel++;
                hook.OnGUI();
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    [InitializeOnLoadMethod]
    public static void Init()
    {
        Type baseType = typeof(CFDebugHook);
        Type[] allType = baseType.Assembly.GetTypes();
        List<Type> types = new List<Type>();
        foreach (Type type in allType)
        {
            if (type.IsSubclassOf(baseType) && !type.IsAbstract)
            {
                types.Add(type);
            }
        }

        hooks = new CFDebugHook[types.Count];
        for (int i = 0; i < types.Count; i++)
        {
            Type type = types[i];
            CFDebugHook hook = (CFDebugHook) Activator.CreateInstance(type);
            hook.Init();
            hooks[i] = hook;
        }
    }
}