using System;
using System.Collections.Generic;
using System.IO;
using BluePrint;
using CFEngine;
using CFEngine.Editor;
using CFUtilPoolLib;
using EcsData;
using EditorNode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using VirtualSkill;

class SkillEditor : BlueprintEditor
{
    public static SkillEditor Instance { get; set; }
    [MenuItem ("Window/SkillEditor &%[")]
    public static void InitEmpty ()
    {
        var window = (SkillEditor) GetWindow (typeof (SkillEditor));
        window.titleContent = new GUIContent ("SkillEditor");
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
            if (SkillHoster.GetHoster != null)
                SkillHoster.GetHoster.partTag = CurrentGraph.graphConfigData.tag;
        }
    }

    public override void OnEnable ()
    {
        Instance = this;
        if (Instance != null && CurrentGraph == null)
        {
            SkillEditor.Instance.ToolBarNewClicked ();
            (CurrentGraph as SkillGraph).ReloadBackupData ();

            (CurrentGraph as SkillGraph).LoadRuntimeData ();
        }

        LoadEditorGUIData ();
    }

    public override void OnDisable ()
    {
        Instance = this;
        if (Instance != null && CurrentGraph != null)
        {
            (CurrentGraph as SkillGraph).BackupData ();

            (CurrentGraph as SkillGraph).CacheRuntimeData ();
        }

        base.OnDisable ();
    }

    public override void ToolBarNewClicked ()
    {
        SkillGraph graph = new SkillGraph ();
        graph.Init (this);
        Graphs.Clear ();
        AddGraph (graph);
    }

    public override void ToolBarOpenClicked ()
    {
        if (CurrentGraph != null)
        {
            string file = EditorUtility.OpenFilePanel("Select skill file", Application.dataPath + "/BundleRes/SkillPackage/" + (CurrentGraph as SkillGraph).LoadCacheDirectory(), "bytes");
            if (file.Length != 0)
            {
                SkillEditor.Instance.ToolBarNewClicked ();
                (CurrentGraph as SkillGraph).OpenData (file);

                (CurrentGraph as SkillGraph).CacheRuntimeData();
            }

            //Path.GetFileNameWithoutExtension(file);
            string configDataName = (CurrentGraph as SkillGraph)?.configData.Name;
            if (configDataName != null) 
            {
                List<SkillProfileType> skilltype = EditorSFXData.instance.skillTypeByDoc;
                for (int i = 0; i < skilltype.Count; i++)
                {
                    if (skilltype[i].skillName.Equals(configDataName))
                    {
                        (CurrentGraph as SkillGraph).sfxType = skilltype[i].skillType;
                        //add by shiyu 如果匹配 就不再继续匹配
                        break;
                    }
                }
            }
        }
    }

    public void OpenFile(string file)
    {
        SkillEditor.Instance.ToolBarNewClicked();
        (CurrentGraph as SkillGraph).OpenData(file);

        (CurrentGraph as SkillGraph).CacheRuntimeData();
    }
    public void OpenResFile(string file)
    {
        (CurrentGraph as SkillGraph).resMap.Clear();
        string name = Path.GetFileNameWithoutExtension(file);
        string assetPath = string.Format("{0}Config/SkillPackage/{1}.asset", 
            LoadMgr.singleton.editorResPath, name);
        if(File.Exists(assetPath))
        {
            var asset = AssetDatabase.LoadAssetAtPath<ResRedirectConfig>(assetPath);
            foreach (var res in asset.resPath)
            {
                (CurrentGraph as SkillGraph).resMap[res.name + res.ext] = res.physicPath + res.name;
            }
        }
    }

    public override void ToolBarSaveClicked ()
    {
        if (CurrentGraph != null)
        {
            SaveSkillType();
            (CurrentGraph as SkillGraph).SaveData ("");
        }
    }
    private static void GetFxPath (ref string FxPath, NodeConfigData ncd, ref bool dirty)
    {
        if (!string.IsNullOrEmpty (FxPath))
        {
            var fxName = EngineUtility.GetFileName (FxPath);

            if (FxPath != fxName)
            {
                FxPath = fxName.ToLower ();
                dirty = true;
            }
            if (!string.IsNullOrEmpty (fxName))
            {
                var asset = SFXWrapper.GetSFXEditorAsset (fxName);
                if (asset != null)
                {
                    ncd.CustomData1 = AssetDatabase.GetAssetPath (asset);
                }
            }
        }
    }

    public static bool RefreshNode (BaseSkillNode node)
    {
        if (node is SpecialActionNode)
        {
            SpecialActionNode specialAction = node as SpecialActionNode;
            float lastFadeoutTime = specialAction.HosterData.FadeOutTime;
            float lastLifeTime = specialAction.HosterData.LifeTime;
            if (lastFadeoutTime > 0)
            {
                specialAction.HosterData.LifeTime = lastFadeoutTime + lastLifeTime;
            }
            return true;
        }
        return false;
    }

    private void RefreshAllScript ()
    {
        string[] files = Directory.GetFiles (Application.dataPath + "/BundleRes/SkillPackage", "*.bytes", SearchOption.AllDirectories);

        AssetDatabase.StartAssetEditing();
        int index = 0;
        foreach (string file in files)
        {
            SkillGraph graph = (CurrentGraph as SkillGraph);
            graph.OpenData (file);
            bool change = false;
            for (int i = 0; i < graph.widgetList.Count; ++i)
            {
                //(graph.widgetList[i] as BaseSkillNode).DrawDataInspector ();
                // change |= RefreshFX (graph.widgetList[i] as BaseSkillNode);
                change |= RefreshNode (graph.widgetList[i] as BaseSkillNode);
            }
            if (change)
            {
                DebugLog.AddEngineLog2 ("change Effect:{0}", file);
                graph.SaveData (file, true);
            }

            EditorUtility.DisplayProgressBar ("Progress Bar " + index.ToString () + "/" + files.Length.ToString (), file, (++index / (1.0f * files.Length)));
        }
        EditorUtility.ClearProgressBar ();

        AssetDatabase.StopAssetEditing();
    }

    public override void ToolBarLeftExtra ()
    {
        if (CurrentGraph != null)
        {
            if (!string.IsNullOrEmpty ((CurrentGraph as SkillGraph).DataPath))
            {
                if (GUILayout.Button ("Save", BlueprintStyles.ToolbarButton ()))
                {
                    SaveSkillType();
                    (CurrentGraph as SkillGraph).SaveData((CurrentGraph as SkillGraph).DataPath, true);
                }
                   
            }
        }
    }

    public void SaveSkill(string FileName)
    {
        if (CurrentGraph != null)
        {          
            if (FileName.Length != 0)
            {
                (CurrentGraph as SkillGraph).OpenData(FileName);
                (CurrentGraph as SkillGraph).CacheRuntimeData();
            }
            else
            {
                Debug.Log("读取到了空的技能文件名");
            }

            string configDataName = (CurrentGraph as SkillGraph)?.configData.Name;
            if (configDataName != null)
            {
                List<SkillProfileType> skilltype = EditorSFXData.instance.skillTypeByDoc;
                for (int i = 0; i < skilltype.Count; i++)
                {
                    if (skilltype[i].skillName.Equals(configDataName))
                    {
                        (CurrentGraph as SkillGraph).sfxType = skilltype[i].skillType;
                        break;
                    }
                }
            }
        }

        SaveSkillType();
        (CurrentGraph as SkillGraph).SaveData((CurrentGraph as SkillGraph).DataPath, true);
    }
    
    public override void ToolBarExtra ()
    {
        if (CurrentGraph != null)
        {
            //if (GUILayout.Button("RefreshAll", BlueprintStyles.ToolbarButton ()))
            //{
            //   RefreshAllScript();
            //}
            
            currentLod = (EnumLodDes)EditorGUILayout.EnumPopup(currentLod);
            

            if (GUILayout.Button ("RefreshTemplate", BlueprintStyles.ToolbarButton ()))
            {
                (CurrentGraph as SkillGraph).SaveData ("", false, true);
            }

            if (GUILayout.Button("ReloadTable", BlueprintStyles.ToolbarButton()))
            {
                XSkillReader.Reload();
            }
            
            if (!Application.isPlaying)
            {
                if (GUILayout.Button ("BuildScene", BlueprintStyles.ToolbarButton ()))
                {
                    SkillGraph graph = (CurrentGraph as SkillGraph);
                    graph.BackupData ();

                    if (graph.PrefabObject != null)
                    {
                        string file = EditorUtility.OpenFilePanel ("Select Scene file", Application.dataPath + "/Scenes/Scenelib", "unity");
                        VirtualSkill.SkillHoster.BuildSkillScene (file);
                        VirtualSkill.SkillHoster.BindCameraToEntity (String.IsNullOrEmpty (file), SkillGraph.SoloCamera, AssetDatabase.GetAssetPath (SkillGraph.SoloCameraObj));

                        VirtualSkill.SkillHoster.CreateEntity (GetLodObject(graph.PrefabObject), "Player", SkillHoster.PlayerIndex, graph.PrefabPos, graph.configData.PresentID);
                        VirtualSkill.SkillHoster.RefreshSkill (GetExSkill);
                    }
                    GUIUtility.ExitGUI ();
                }
            }

            if (GUILayout.Button (Application.isPlaying ? "■Stop" : "▶Play", BlueprintStyles.ToolbarButton ()))
            {
                EditorApplication.ExecuteMenuItem ("Edit/Play");
                GUIUtility.ExitGUI ();
            }

            if (GUILayout.Button ("Build", BlueprintStyles.ToolbarButton ()))
            {
                if (SkillHoster.GetHoster != null) SkillHoster.GetHoster.RefreshHitScript ();
                Build ();
            }
        }
    }

    private void GetExSkill (ref List<string> skillList, ref List<float> timeList, ref List<string> combatList)
    {
        SkillGraph graph = (CurrentGraph as SkillGraph);
        CFUtilPoolLib.XEntityPresentation.RowData data = XEntityPresentationReader.GetData ((uint) graph.configData.PresentID);
        if (data == null) return;
        skillList = new List<string> ();
        for (int i = 0; i < graph.KeyboardData.PresentList.Count; ++i)
            if (graph.KeyboardData.PresentList[i] == graph.configData.PresentID)
            {
                for (int j = 0; j < graph.KeyboardData.SkillList[i].Count; ++j)
                    skillList.Add ("/BundleRes/SkillPackage/" + data.SkillLocation + graph.KeyboardData.SkillList[i][j] + ".bytes");
            }

        timeList = new List<float> ();
        combatList = new List<string> ();
        for (int i = 0; i < graph.KeyboardData.MapKey_Skill.Count; ++i)
            if (graph.KeyboardData.MapKey_Skill[i] == graph.configData.Name)
            {
                for (int j = 0; j < graph.KeyboardData.MapValue_TriggerTime[i].Count; ++j)
                {
                    timeList.Add (graph.KeyboardData.MapValue_TriggerTime[i][j]);
                    combatList.Add ("/BundleRes/SkillPackage/" + data.SkillLocation + graph.KeyboardData.MapValue_TriggerSkill[i][j] + ".bytes");
                }
            }

    }

    private void Build ()
    {
        SkillGraph graph = (CurrentGraph as SkillGraph);
        graph.BackupData ();
        if (!Application.isPlaying)
        {
            if (graph.PrefabObject != null)
            {
                float lag = 0, fluctuations = 0;
                int lagType = 0, fluctuationsType = 0;
                if (SkillHoster.GetHoster != null)
                {
                    lagType = SkillHoster.GetHoster.LagType;
                    fluctuationsType = SkillHoster.GetHoster.FluctuationsType;
                    lag = SkillHoster.GetHoster.Lag;
                    fluctuations = SkillHoster.GetHoster.Fluctuations;
                }

                VirtualSkill.SkillHoster.BuildSkillScene ();
                VirtualSkill.SkillHoster.BindCameraToEntity (true, SkillGraph.SoloCamera, AssetDatabase.GetAssetPath (SkillGraph.SoloCameraObj));
                List<string> skillList = new List<string> ();
                for (int i = 0; i < graph.KeyboardData.PresentList.Count; ++i)
                    if (graph.KeyboardData.PresentList[i] == graph.configData.PresentID) skillList = graph.KeyboardData.SkillList[i];
                VirtualSkill.SkillHoster.CreateEntity (GetLodObject(graph.PrefabObject), "Player", SkillHoster.PlayerIndex, graph.PrefabPos, graph.configData.PresentID);

                if (SkillHoster.GetHoster != null)
                {
                    SkillHoster.GetHoster.Lag = lag;
                    SkillHoster.GetHoster.Fluctuations = fluctuations;
                    SkillHoster.GetHoster.LagType = lagType;
                    SkillHoster.GetHoster.FluctuationsType = fluctuationsType;
                }
            }
            VirtualSkill.SkillHoster.RefreshSkill (GetExSkill);
            GUIUtility.ExitGUI ();
        }
        VirtualSkill.SkillHoster.RefreshSkill(GetExSkill, false);
    }

    public enum EnumLodDes
    {
        lod0 = 0,
        lod1 = 1,
        lod2 = 2,
    }

    private static EnumLodDes currentLod = EnumLodDes.lod1;
    public static EnumLodDes GetCurrentLod
    {
        get { return currentLod; }
    }

    public void SetcurrentLod(EnumLodDes Clod)
    {
        currentLod = Clod;
    }

    private GameObject GetLodObject(GameObject obj)
    {
        string path = AssetDatabase.GetAssetPath(obj);
        path = path.Replace(".prefab", "");
        switch (currentLod)
        {
            case EnumLodDes.lod0:
                return obj;
            case EnumLodDes.lod1:
                path += "_lod1";
                break;
            case EnumLodDes.lod2:
                path += "_lod2";
                break;
        }
        
        UnityEngine.Object newObject = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path + ".prefab");
        if (newObject == null) return obj;

        return newObject as GameObject;
    }


    public override void ShowInspector (int unusedWindowID)
    {
        //LabelField ("编辑器规则：");
        //LabelField ("1.时间触发节点，不能连线触发！！！");
        //LabelField ("2.LoopNode不能相互嵌套！！！");
        //LabelField ("3.Loop 内需要以Continue,Break,End,为分支终点！！！");
        //LabelField ("4.Loop 内单个引脚只能拉出一根线！！！");
        //LabelField ("5.In 引脚只能有一根线进入！！！");
        //LabelField ("6.Charge节点End会在曲线执行完再触发！！！");
        //GUILayout.Box ("", InspectorLine);
        //EditorGUILayout.Space ();
        base.ShowInspector (unusedWindowID);
    }

    public static void PreBuildScript ()
    {
        string data = DateTime.Now.Ticks.ToString ();
        string root = Application.dataPath + "/BundleRes/SkillPackage/";
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
    
    private void SaveSkillType()
    {
        List<SkillProfileType> skilltype = EditorSFXData.instance.skillTypeByDoc;
        if (skilltype == null)
        {
            skilltype = new List<SkillProfileType>();
        }
        else
        {
            for (int i = 0; i < skilltype.Count; i++)
            {
                if (skilltype[i].skillName != null && skilltype[i].skillName.Equals((CurrentGraph as SkillGraph).configData.Name))
                {
                    skilltype.RemoveAt(i);
                    break;
                }
            }
        }

        int type = (CurrentGraph as SkillGraph).sfxType;
        if ((CurrentGraph as SkillGraph).configData.Name != null &&
            (CurrentGraph as SkillGraph).configData.Name.Contains("Role_"))
        {
            type = 1;
        }
        skilltype.Add(new SkillProfileType(){skillName = (CurrentGraph as SkillGraph).configData.Name, skillType = type});
        EditorSFXData.instance.Save();
    }
}

[Serializable]
public class SkillEditorRuntimeData
{
    [SerializeField]
    public string DataPath;
    [SerializeField]
    public bool SoloCamera;
    [SerializeField]
    public string SoloCameraPath;

    public void CacheEditorRuntimeData (string name)
    {
        string path = Application.dataPath + "/Editor Default Resources/SkillBackup/" + name + "RuntimeData.cache";

        DataIO.SerializeData<SkillEditorRuntimeData>(path, this, false);
    }

    public static SkillEditorRuntimeData LoadEditorRuntimeData (string name)
    {
        string path = Application.dataPath + "/Editor Default Resources/SkillBackup/" + name + "RuntimeData.cache";

        return File.Exists(path) ? DataIO.DeserializeData<SkillEditorRuntimeData>(path) : null;
    }
}