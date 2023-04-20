/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class SimpleJsonWindowDescAttribute : Attribute
{
    public string m_desc;
    public SimpleJsonWindowDescAttribute(string desc)
    {
        m_desc = desc;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class SimpleJsonWindowPathAttribute : Attribute
{
    public enum RootType
    {
        StreamingAssets,
        Assets,
    }
    public enum ContentType
    {
        Folder,
        File,
    }

    public string m_rootPath;
    public ContentType m_contentType;
    public SimpleJsonWindowPathAttribute(RootType type, ContentType contentType)
    {
        InitRootPath(type);
        m_contentType = contentType;
    }

    public SimpleJsonWindowPathAttribute()
    {
        InitRootPath(RootType.Assets);
        m_contentType = ContentType.Folder;
    }

    private void InitRootPath(RootType type)
    {
        switch(type)
        {
            case RootType.Assets:
                m_rootPath = Application.dataPath.Replace("\\", "/");
                break;
            case RootType.StreamingAssets:
                m_rootPath = Application.streamingAssetsPath.Replace("\\", "/");
                break;
        }
    }
}

/// <summary>
/// 通用的Json编辑窗口
/// 避免做无用功，增加测试量，未提供完全支持，后续有需求再额外修改测试
/// </summary>
public class SimpleJsonWindow : EditorWindow 
{
    private object m_content;
    private FieldInfo[] m_fields;
    private Type m_type;
    private string m_jsonPath;
    private Dictionary<FieldInfo, List<string>> m_strListMap = new Dictionary<FieldInfo, List<string>>();
    public static void GetWindow(string jsonPath, Type type, string title)
    {
        var window = CreateWindow<SimpleJsonWindow>(title);
        window.Init(jsonPath, type);
    }

    private void Init(string jsonPath, Type type)
    {
        m_jsonPath = jsonPath;
        if(File.Exists(jsonPath))
        {
            var json = File.ReadAllText(jsonPath);
            m_content = JsonUtility.FromJson(json, type);
        }
        else
        {
            m_content = Activator.CreateInstance(type);
        }
        m_fields = type.GetFields();
        InitStringList(m_fields);
    }

    private void OnGUI()
    {
        foreach(var field in m_fields)
        {
            if(m_strListMap.ContainsKey(field))
            {
                continue;
            }
            DrawDesc(field);
            DrawField(field);
        }
        DrawStringLists();
        GUILayout.Space(50);
        if(GUILayout.Button("Save"))
        {
            foreach(var strListPair in m_strListMap)
            {
                strListPair.Key.SetValue(m_content, strListPair.Value);
            }
            var json = JsonUtility.ToJson(m_content);
            File.WriteAllText(m_jsonPath, json);
        }
    }

    private void DrawField(FieldInfo field)
    {
        if(field.FieldType == typeof(int))
        {
            DrawInt(field);
        }
        else if(field.FieldType == typeof(string))
        {
            if (!DrawPath(field))
            {
                DrawString(field);
            }
        }
        else if(field.FieldType == typeof(bool))
        {
            DrawBool(field);
        }
        else if(field.FieldType == typeof(float))
        {
            DrawFloat(field);
        }
        else
        {
            Debug.LogErrorFormat("type {0} is not supported, please add it youself", field.FieldType);
        }
    }

    private void DrawInt(FieldInfo field)
    {
        var val = field.GetValue(m_content);
        field.SetValue(m_content, EditorGUILayout.IntField(field.Name, (int)val));
    }

    private void DrawString(FieldInfo field)
    {
        field.SetValue(m_content, EditorGUILayout.TextField(field.Name, (string)field.GetValue(m_content)));
    }

    private void DrawFloat(FieldInfo field)
    {
        field.SetValue(m_content, EditorGUILayout.FloatField(field.Name, (float)field.GetValue(m_content)));
    }

    private void DrawBool(FieldInfo field)
    {
        field.SetValue(m_content, EditorGUILayout.Toggle(field.Name, (bool)field.GetValue(m_content)));
    }

    private void InitStringList(FieldInfo[] fields)
    {
        foreach(var field in fields)
        {
            switch(field.FieldType.Name.ToLower())
            {
                case "list`1":
                    var val = field.GetValue(m_content);
                    var type = GetListGenericType(field.FieldType);
                    if(type == typeof(string))
                    {
                        if(null == val)
                        {
                            m_strListMap[field] = new List<string>();
                        }
                        else
                        {
                            m_strListMap[field] = (List<string>)val;
                        }
                    }
                    break;
            }
        }
    }

    private Type GetListGenericType(Type listType)
    {
        if (listType.IsGenericType)
        {
            Type[] at = listType.GetGenericArguments();
            return at.First();
        }
        return null;
    }

    private bool DrawPathList(KeyValuePair<FieldInfo, List<string>> strListPair )
    {
        var pathAttr = strListPair.Key.GetCustomAttribute<SimpleJsonWindowPathAttribute>();
        if (null != pathAttr)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(string.Format("{0} :", strListPair.Key.Name));
            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                var isDir = pathAttr.m_contentType == SimpleJsonWindowPathAttribute.ContentType.Folder;
                var newPath = GetFormatPath(pathAttr.m_rootPath, isDir);
                if(!string.IsNullOrEmpty(newPath))
                {
                    strListPair.Value.Add(newPath);
                }
            }
            GUILayout.EndHorizontal();
            int? indexToRemove = null;
            for (var i = 0; i < strListPair.Value.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(strListPair.Value[i]);
                if (GUILayout.Button("...", GUILayout.Width(20)))
                {
                    var isDir = pathAttr.m_contentType == SimpleJsonWindowPathAttribute.ContentType.Folder;
                    var newPath = GetFormatPath(pathAttr.m_rootPath, isDir);
                    if(!string.IsNullOrEmpty(newPath))
                    {
                        strListPair.Value[i] = newPath;
                    }
                }
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    indexToRemove = i;
                }
                GUILayout.EndHorizontal();
            }
            if (indexToRemove.HasValue)
            {
                strListPair.Value.RemoveAt(indexToRemove.Value);

            }
            return true;
        }
        return false;
    }

    private string GetFormatPath(string rootPath, bool isDir)
    {
        string newPath = isDir ? 
            EditorUtility.OpenFolderPanel("", Application.dataPath, "") : 
            EditorUtility.OpenFilePanel("", Application.dataPath, "");
        if(string.IsNullOrEmpty(newPath))
        {
            return "";
        }
        var formatNewPath = newPath.Replace("\\", "/");
        if(!formatNewPath.StartsWith(rootPath) || formatNewPath == rootPath)
        {
            EditorUtility.DisplayDialog("Error", string.Format("路径需要在{0}下", rootPath), "ok");
            return "";
        }
        return formatNewPath.Replace(rootPath + "/", "");
    }

    private void DrawStringLists()
    {
        foreach(var strListPair in m_strListMap)
        {
            DrawDesc(strListPair.Key);
            if(!DrawPathList(strListPair))
            {
                DrawStringList(strListPair);
            }
        }
    }

    private void DrawStringList(KeyValuePair<FieldInfo, List<string>> strListPair )
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(string.Format("{0} :", strListPair.Key.Name));
        if (GUILayout.Button("+", GUILayout.Width(20)))
        {
            strListPair.Value.Add("");
        }
        GUILayout.EndHorizontal();
        int? indexToRemove = null;
        for (var i = 0; i < strListPair.Value.Count; i++)
        {
            GUILayout.BeginHorizontal();
            strListPair.Value[i] = EditorGUILayout.TextField(strListPair.Value[i]);
            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                indexToRemove = i;
            }
            GUILayout.EndHorizontal();
        }
        if (indexToRemove.HasValue)
        {
            strListPair.Value.RemoveAt(indexToRemove.Value);

        }
    }
    private void DrawDesc(FieldInfo field)
    {
        var descAttr = field.GetCustomAttribute<SimpleJsonWindowDescAttribute>();
        if(null != descAttr)
        {
            EditorGUILayout.HelpBox(descAttr.m_desc, MessageType.Info);
        }
    }

    private bool DrawPath(FieldInfo field)
    {
        var pathAttr = field.GetCustomAttribute<SimpleJsonWindowPathAttribute>();
        if(null != pathAttr)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label((string)field.GetValue(m_content));
            if (GUILayout.Button("...", GUILayout.Width(20)))
            {
                field.SetValue(m_content, EditorUtility.OpenFolderPanel("", pathAttr.m_rootPath, ""));
            }
            GUILayout.EndHorizontal();
            return true;
        }
        return false;
    }
}
