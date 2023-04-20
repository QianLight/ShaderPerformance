using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.CFUI;


public class UIAudioFlush : EditorWindow {

    string target_path = "";
    
    string[] options = new string[]{ "Button", "Toggle"};
    int select_index = 0;

    string target_audio = "";
    string message = "";
    [MenuItem("Tools/UI/Audio Flush")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<UIAudioFlush>();
    }

    private void OnGUI()
    {
        message = string.Empty;
        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Target:",GUILayout.Width(60));
        
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

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Type:", GUILayout.Width(60));
        select_index = EditorGUILayout.Popup(select_index, options);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Audio:", GUILayout.Width(60));
        target_audio = EditorGUILayout.TextField(target_audio);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();


        bool disable = string.IsNullOrEmpty(target_audio) || string.IsNullOrEmpty(target_path);
        EditorGUI.BeginDisabledGroup(disable);
        if (GUILayout.Button("Apply"))
        {
            FlushAll();
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndHorizontal();
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

    private void FlushAll()
    {
        string genPath = Application.dataPath + "/"+ target_path;
        string[] files = Directory.GetFiles(genPath, "*.prefab",SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            files[i] = files[i].Substring(files[i].IndexOf("Assets"));
            GameObject _prefab = AssetDatabase.LoadAssetAtPath(files[i], typeof(GameObject)) as GameObject;
            if (_prefab == null) continue;
            if (FlushPrefab(_prefab))
            {
                EditorUtility.SetDirty(_prefab);
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Flush Success!");
    }

    private bool FlushPrefab( GameObject prefab )
    {
        bool up = false;
        switch (select_index)
        {
            case 0:
                up = FlushButton(prefab);
                break;
            case 1:
                up = FlushToggle(prefab);
                break;
        }
        return up;
    }

    private List<CFButton> _templist = new List<CFButton>();

    private bool FlushButton(GameObject prefab)
    {
        _templist.Clear();
        prefab.GetComponentsInChildren<CFButton>(true, _templist);
        if (_templist.Count == 0) return false;

        for(int i = 0,length = _templist.Count;i < length; i++)
        {
            _templist[i].ClickAudio = target_audio;
        }
        _templist.Clear();
        return true;
    }

    private List<CFToggle> _togs = new List<CFToggle>();
    private bool FlushToggle(GameObject prefab)
    {
        _togs.Clear();
        prefab.GetComponentsInChildren<CFToggle>(true, _togs);
        if (_templist.Count == 0) return false;

        for (int i = 0, length = _templist.Count; i < length; i++)
        {
            _togs[i].ClickAudio = target_audio;
        }
        _togs.Clear();
        return true;
    }
}
