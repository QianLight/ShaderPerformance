using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.CFUI;
using CFUtilPoolLib;


public class LuaRequreChange : EditorWindow {

    string target_path = "";
    
    //string[] options = new string[]{ "Button", "Toggle"};
    //int select_index = 0;

    //string target_audio = "";
    string message = "";
    [MenuItem("Tools/UI/Lua Require Change")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<LuaRequreChange>();
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


        bool disable =  string.IsNullOrEmpty(target_path);
        EditorGUI.BeginDisabledGroup(disable);
        if (GUILayout.Button("Apply"))
        {
            FlushAll();
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndHorizontal();
        if (disable || string.IsNullOrEmpty(message))
        {
            message = "此操作为批量修改，请设置UI文件夹路径!";
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
        string[] files = Directory.GetFiles(genPath, "*.lua.txt",SearchOption.AllDirectories);
        bool change = false; 
        string[] lines = null;
        string newlines = string.Empty;
        for (int i = 0; i < files.Length; i++)
        {
            //files[i] = files[i].Substring(files[i].IndexOf("Assets"));
            change = false;          
            lines = File.ReadAllLines(files[i]);
            for(int j = 0; j < lines.Length;j++){

                if(lines[j].Contains("require")){
                    int index = lines[j].IndexOf("require");

                    string fstr = lines[j].Substring(0,index);
                    string content = lines[j].Substring(index+8).Replace('.','/');
                    lines[j] = fstr+" require (" + content + ")";
                    change = true;
                }
            }
            if(change){
                File.WriteAllLines(files[i],lines,Encoding.UTF8);
            }           
        }
    }
}
