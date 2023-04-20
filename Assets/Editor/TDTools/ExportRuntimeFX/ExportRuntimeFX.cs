using UnityEngine;
using UnityEditor;
using CFEngine.Editor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using CFEngine;
using static CFEngine.Editor.SceneAtlas;
using UnityEngine.XR;

public class ExportRuntimeFX : EditorWindow
{
    static ExportRuntimeFX m_window;
    Editor editor;

    [SerializeField] public List<GameObject> originalPrefabs;

    [MenuItem("Tools/TDTools/通用工具/导出运行时特效")]
    private static void Init()
    {
        m_window = GetWindow<ExportRuntimeFX>();
        m_window.minSize = new Vector2(200, 400);
        m_window.titleContent = new GUIContent("一键导出运行时特效Prefab");
        m_window.Show();
    }

    void OnGUI()
    {
        if (!editor) editor = Editor.CreateEditor(this);
        if (editor) editor.OnInspectorGUI();
    }

    void OnInspectorGUI() { Repaint(); }
}

[CustomEditor(typeof(ExportRuntimeFX), true)]
public class ExportRuntimeFXDrawer : Editor
{
    SerializedProperty data;

    private void OnEnable()
    {
        data = serializedObject.FindProperty("originalPrefabs");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(data, new GUIContent("Export FX in Batch"), true);

        if (GUILayout.Button("导出"))
        {
            BatchExport(data);
        }
        if (GUILayout.Button("清除"))
        {
            Refresh(data);
        }

        serializedObject.ApplyModifiedProperties();
    }

    void Refresh(SerializedProperty data)
    {
        data.ClearArray();
    }

    void BatchExport(SerializedProperty data)
    {
        int dataSize = data.arraySize;
        for (int i = 0; i < dataSize; ++i)
            Export(data.GetArrayElementAtIndex(i).objectReferenceValue as UnityEngine.GameObject);
    }

    void Export(GameObject go)
    {
        if (go == null)
        {
            Debug.LogError("<--- The gameobject you assigned is null, please check you've correctly assigned the prefab. --->");
            return;
        }

        MakeFXPrefab(go);
    }

    void MakeFXPrefab(GameObject go)
    {
        NestPrefab.MakeNestPrefab(go);
    }
}