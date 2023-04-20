using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using BluePrint;
using LevelEditor;

public class FindAllUsedCutscenes : EditorWindow
{
    private static FindAllUsedCutscenes m_Window;

    private string m_LevelFolderPath;

    private string m_ExportPath;

    private LevelEditorData m_fullData;

    private EditorConfigData m_configData;

    private List<string> m_UsedTimelines;

    [MenuItem("Tools/TDTools/�ؿ���ع���/�������йؿ��ű������õ�Timeline", priority = 99999)]
    public static void Init()
    {
        m_Window = GetWindow<FindAllUsedCutscenes>();

        m_Window.titleContent = new GUIContent("�������йؿ��ű������õ�Timeline");

        m_Window.Show();
    }

    private void OnEnable()
    {
        m_LevelFolderPath = Application.dataPath + "/BundleRes/Table/Level";

        m_ExportPath = Application.dataPath + "/export.txt";

        if (m_UsedTimelines == null)
        {
            m_UsedTimelines = new List<string>();
        }

        m_UsedTimelines.Clear();
    }

    public void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("ѡ�����·����", GUILayout.Width(100));

        GUILayout.Label(m_ExportPath);

        if (GUILayout.Button("ѡ�񵼳�·��", GUILayout.Width(100)))
        {
            m_ExportPath = EditorUtility.SaveFilePanel("����·��", Application.dataPath, "export", "txt");
        }

        if (GUILayout.Button("����!"))
        {
            Run(m_LevelFolderPath);

            Export(m_ExportPath);
             
            ShowNotification(new GUIContent("�����ɹ���"), 2);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void Run(string currentPath)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(currentPath);

        foreach (DirectoryInfo di in directoryInfo.GetDirectories())
        {
            Run(di.FullName);
        }

        foreach (FileInfo file in directoryInfo.GetFiles())
        {
            if (file.FullName.EndsWith(".cfg"))
            {
                RunSingleScript(file.FullName);
            }
        }
    }

    private void RunSingleScript(string scriptPath)
    {
        m_fullData = DataIO.DeserializeData<LevelEditorData>(scriptPath);

        foreach (var graph in m_fullData.GraphDataList)
        {
            foreach (var node in graph.ScriptData)
            {
                if (node.Cmd == LevelScriptCmd.Level_Cmd_Cutscene && !m_UsedTimelines.Contains(node.stringParam[0]))
                {
                    m_UsedTimelines.Add(node.stringParam[0]);
                }
            }
        }
    }

    private void Export(string exportPath)
    {
        using (StreamWriter sw = new StreamWriter(exportPath))
        {
            foreach(string fileName in m_UsedTimelines)
            {
                sw.WriteLine(fileName);
            }
        }
    }
}
