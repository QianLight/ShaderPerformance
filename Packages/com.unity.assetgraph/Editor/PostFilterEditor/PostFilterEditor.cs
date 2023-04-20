using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

public class PostFilterEditor : EditorWindow
{
    string newFilterFolder = string.Empty;
    string newFilterExtension = string.Empty;
    List<string> listToRemove;
    List<string> listToRemove_extensions;
    PostFilterSetting  setting;
    GUIContent batPathGUIContent = new GUIContent("");

    [MenuItem("Zeus/AssetGraph/PostFilter", false, 21)]
    private static void Open()
    {
        GetWindow<PostFilterEditor>();
    }

    private void OnEnable()
    {
        setting = PostFilterSetting.GetInstance();
        listToRemove = new List<string>();
        listToRemove_extensions = new List<string>();
    }

    private void OnGUI()
    {
        OnDrawFilterFolder();
        EditorGUILayout.Space(20);
        OnDrawFilterExtensions();
    }

    private void OnDrawFilterFolder()
    {
        EditorGUILayout.LabelField("AssetGraphPostprocessor.OnPostprocessAllAssets 忽略的文件夹：");

        foreach (var item in setting.filterFolders)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.TextField(item);

            if (GUILayout.Button("删除"))
                listToRemove.Add(item);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        foreach (var item in listToRemove.ToArray())
            setting.filterFolders.Remove(item);

        listToRemove.Clear();

        EditorGUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();
        newFilterFolder = EditorGUILayout.TextField(newFilterFolder);
        if (GUILayout.Button("选择文件夹"))
        { 
            newFilterFolder = EditorUtility.OpenFolderPanel("choose filter folder", Application.dataPath,"haha");
            newFilterFolder = newFilterFolder.Substring(Application.dataPath.Length - "Assets".Length);
            EditorGUILayout.EndHorizontal();
        }
        else
            EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("添加文件夹"))
            setting.filterFolders.Add(newFilterFolder);
    }

    private void OnDrawFilterExtensions()
    {
        EditorGUILayout.LabelField("AssetGraphPostprocessor.OnPostprocessAllAssets 忽略的扩展名：");

        foreach (var item in setting.filterExtensions)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.TextField(item);

            if (GUILayout.Button("删除"))
                listToRemove_extensions.Add(item);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        foreach (var item in listToRemove_extensions.ToArray())
            setting.filterExtensions.Remove(item);

        listToRemove_extensions.Clear();

        EditorGUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();
        newFilterExtension = EditorGUILayout.TextField(newFilterExtension);
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("添加新扩展名"))
            setting.filterExtensions.Add(newFilterExtension);
    }

    private void OnDestroy()
    {
        setting.Save();
    }

    private void OnFocus()
    {
        setting = PostFilterSetting.GetInstance();
    }

    private void OnLostFocus()
    {
        setting.Save();
    }
}
