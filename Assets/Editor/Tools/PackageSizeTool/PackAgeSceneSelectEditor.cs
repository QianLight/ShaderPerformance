using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class PackAgeSceneSelectEditorWindow : EditorWindow
{
    private static List<string> _delFile;
    private static List<bool> _ToggleBool;
    private static List<bool> _exist;
    private static PackAgeSceneSelectEditorWindow window;

    public static void OpenDelWindow(List<string> delFile)
    {
        window =
            (PackAgeSceneSelectEditorWindow) EditorWindow.GetWindow(typeof(PackAgeSceneSelectEditorWindow), false,
                "删除文件", true); //创建窗口
        _delFile = delFile;
        _ToggleBool = new List<bool>(delFile.Count);
        _exist = new List<bool>();
        foreach (var file in delFile)
        {
            _exist.Add(File.Exists(file));
            _ToggleBool.Add(true);
        }

        window.Show();
    }

    private void DelFile()
    {
        for (int i = 0; i < _ToggleBool.Count; i++)
        {
            AssetDatabase.StartAssetEditing();
            if (_ToggleBool[i] && _exist[i])
            {
                AssetDatabase.DeleteAsset(_delFile[i]);
            }

            AssetDatabase.StopAssetEditing();
        }
    }
    
    private Vector2 scroll;

    private void OnGUI()
    {
        if (GUI.Button(new Rect(400, 5, 100, 30), "确定"))
        {
            if (EditorUtility.DisplayDialog("确认删除", "确认删除文件", "OK"))
            {
                DelFile();
                window.Close();
            }
        }

        scroll = EditorGUILayout.BeginScrollView(scroll);
        for (int i = 0; i < _ToggleBool.Count; i++)
        {
            var exis = _exist[i] ? "    " : "不存在 ";
            _ToggleBool[i] = GUILayout.Toggle(_ToggleBool[i], exis + _delFile[i]);
        }

        EditorGUILayout.EndScrollView();
    }
}