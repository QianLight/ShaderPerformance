using System.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using CFEngine;
using UnityEngine.CFUI;


public class UIComponentFind : EditorWindow {

    string target_path = "";
    
    string[] options = new string[]{ "UIScroll", "Toggle"};
    int select_index = 0;

    private string m_searchStr = null;
    string message = "";
    [MenuItem("Tools/UI/Component Find")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<UIComponentFind>();
    }
    private void OnGUI()
    {
        message = string.Empty;
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Component:",GUILayout.Width(60));
        m_searchStr = EditorGUILayout.TextField(m_searchStr);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Folder:",GUILayout.Width(60));     
        target_path = EditorGUILayout.TextField(target_path);
        if (GUILayout.Button("Browse")){
            string path = EditorUtility.OpenFolderPanel("Select UI Prefab Folder!",Application.dataPath,"");
            string need = "Assets/";
            int index = path.IndexOf(need);
            if(index >= 0)
            {
                target_path = path.Substring(index + need.Length);
            }
            else
            {
                message = "无效的路径!";
            }      
        }
        EditorGUILayout.EndHorizontal();

        bool disable = string.IsNullOrEmpty(m_searchStr) || string.IsNullOrEmpty(target_path);
        EditorGUI.BeginDisabledGroup(disable);
        if (GUILayout.Button("Apply"))
        {
            Search();
        }
        EditorGUI.EndDisabledGroup();

        if (disable || string.IsNullOrEmpty(message))
        {
            message = "此操作为批量修改，请设置UI路径,然后再选择需要更换的音效名称，例如：UI / Start";
        }
        if (!string.IsNullOrEmpty(message))
        {
            EditorGUILayout.HelpBox(message,MessageType.Warning,true);
        }
        
        EditorGUILayout.EndVertical();
    }

    private void Search()
    {
        string assemblyPath = Application.dataPath + "/Engine/Lib/CFEngine.dll";

        Assembly ass = Assembly.LoadFile(assemblyPath);
        
        Type deviceType = ass.GetType(m_searchStr);

        if(deviceType == null) {
            Debug.LogError("not device type");
            return;
        }

        string genPath = Application.dataPath + "/"+ target_path;
        string[] files = Directory.GetFiles(genPath, "*.prefab",SearchOption.AllDirectories);

        for (int i = 0; i < files.Length; i++)
        {
            files[i] = files[i].Substring(files[i].IndexOf("Assets"));
            GameObject _prefab = AssetDatabase.LoadAssetAtPath(files[i], typeof(GameObject)) as GameObject;
            if (_prefab == null) continue;
            Component[] components = _prefab.transform.GetComponentsInChildren(deviceType,true);
            if(components == null || components.Length == 0)  continue; 
            Debug.Log(" use " + deviceType.Name +" : " + files[i]);        
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Flush Success!");
    }
}
