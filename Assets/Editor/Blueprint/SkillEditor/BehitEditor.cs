using System;
using System.IO;
using BluePrint;
using EditorNode;
using UnityEditor;
using UnityEngine;
using VirtualSkill;

class BehitEditor : BlueprintEditor
{
    public static BehitEditor Instance { get; set; }

    [MenuItem ("Window/BehitEditor &%]")]
    public static void InitEmpty ()
    {
        var window = (BehitEditor) GetWindow (typeof (BehitEditor));
        window.titleContent = new GUIContent ("BehitEditor");
        window.wantsMouseMove = true;
        window.Show ();
        window.Repaint ();
        Instance = window;

        Instance.ToolBarNewClicked ();
    }

    public override void Update ()
    {
        base.Update ();
        Instance = this;

        if (CurrentGraph != null)
        {
            CurrentGraph.Update ();
        }
    }

    public override void OnEnable ()
    {
        Instance = this;
        if (Instance != null && CurrentGraph == null)
        {
            BehitEditor.Instance.ToolBarNewClicked ();
            (CurrentGraph as BehitGraph).ReloadBackupData ();
            (CurrentGraph as BehitGraph).LoadRuntimeData ();
        }

        LoadEditorGUIData ();
    }

    public override void OnDisable ()
    {
        Instance = this;
        if (Instance != null && CurrentGraph != null)
        {
            (CurrentGraph as BehitGraph).BackupData ();
            (CurrentGraph as BehitGraph).CacheRuntimeData ();
        }

        base.OnDisable ();
    }

    public override void ToolBarNewClicked ()
    {
        BehitGraph graph = new BehitGraph ();
        graph.Init (this);
        AddGraph (graph);
    }

    public override void ToolBarOpenClicked ()
    {
        if (CurrentGraph != null)
        {
            string file = EditorUtility.OpenFilePanel("Select behit file", Application.dataPath + "/BundleRes/HitPackage/" + (CurrentGraph as BehitGraph).LoadCacheDirectory(), "bytes");
            if (file.Length != 0)
            {
                BehitEditor.Instance.ToolBarNewClicked ();
                (CurrentGraph as BehitGraph).OpenData (file);

                (CurrentGraph as BehitGraph).CacheRuntimeData();
            }
        }
    }

    public void OpenFile(string file)
    {
        BehitEditor.Instance.ToolBarNewClicked();
        (CurrentGraph as BehitGraph).OpenData(file);
        (CurrentGraph as BehitGraph).CacheRuntimeData();
    }

    public override void ToolBarSaveClicked ()
    {
        if (CurrentGraph != null)
        {
            (CurrentGraph as BehitGraph).SaveData (Application.dataPath + "/BundleRes/HitPackage");
        }
    }

    private void RefreshAllScript ()
    {
        string[] files = Directory.GetFiles (Application.dataPath + "/BundleRes/HitPackage", "*.bytes", SearchOption.AllDirectories);

        int index = 0;
        foreach (string file in files)
        {
            BehitGraph graph = (CurrentGraph as BehitGraph);
            graph.OpenData (file);
            bool change = false;
            for (int i = 0; i < graph.widgetList.Count; ++i)
            {
                //(graph.widgetList[i] as BaseSkillNode).DrawDataInspector();
                change |= SkillEditor.RefreshNode (graph.widgetList[i] as BaseSkillNode);
            }
            if (change)
                graph.SaveData (file, true);
            EditorUtility.DisplayProgressBar("Progress Bar " + index.ToString() + "/" + files.Length.ToString(), file, (++index / (1.0f * files.Length)));
        }
        EditorUtility.ClearProgressBar ();
    }

    public override void ToolBarLeftExtra ()
    {
        if (!string.IsNullOrEmpty ((CurrentGraph as BehitGraph).DataPath))
        {
            if (GUILayout.Button ("Save", BlueprintStyles.ToolbarButton ()))
                (CurrentGraph as BehitGraph).SaveData ((CurrentGraph as BehitGraph).DataPath, true);
        }
    }

    public override void ToolBarExtra ()
    {
        if (CurrentGraph != null)
        {
            // if (GUILayout.Button("RefreshAll", BlueprintStyles.ToolbarButton ()))
            // {
            //    RefreshAllScript();
            // }

            if (GUILayout.Button ("RefreshTemplate", BlueprintStyles.ToolbarButton ()))
            {
                if (CurrentGraph != null)
                {
                    (CurrentGraph as BehitGraph).SaveData ("", false, true);
                }
            }

            if (GUILayout.Button("ReloadTable", BlueprintStyles.ToolbarButton()))
            {
                XSkillReader.Reload();
            }

            if (GUILayout.Button ("ClearDebug", BlueprintStyles.ToolbarButton ()))
            {
                if (SkillHoster.GetHoster != null) SkillHoster.GetHoster.HitGraphInited = false;
            }
        }
    }

    public static void PreBuildScript ()
    {
        string data = "";
        data += DateTime.Now.Minute;
        string root = Application.dataPath + "/BundleRes/HitPackage/";
        string[] files = Directory.GetFiles (root, "*.bytes", SearchOption.AllDirectories);

        for (int i = 0; i < files.Length; ++i)
        {
            data += '\n' + files[i].Replace (root, "").Replace (".bytes", "");
        }

        using (FileStream wfs = new FileStream (string.Format ("{0}Version.txt", root), FileMode.Create))
        {
            BinaryWriter bw = new BinaryWriter (wfs);
            bw.Write (data);
        }
    }
}