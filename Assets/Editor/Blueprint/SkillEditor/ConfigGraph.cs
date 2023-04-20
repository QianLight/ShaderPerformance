using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BluePrint;
using EcsData;
using EditorNode;
using UnityEditor;
using UnityEngine;
using VirtualSkill;
using CFEngine;
using System.Runtime.Serialization.Formatters.Binary;

public abstract class ConfigGraph<D> : BaseSkillGraph where D : XConfigData, new ()
{
    private static GUIContent
    SaveButtonContent = new GUIContent ("Save", "Save to file");
    private static GUIContent
    LoadButtonContent = new GUIContent ("Load", "Load from file");

    public virtual string[] NodeNameArray { get { return null; } }
    public virtual string[] DynamicNodeNameArray { get { return dynamicNodeNameArray; } }
    private string[] dynamicNodeNameArray = new string[]
    {
        "ShaderEffect",
        "MatEffect"
    };
    private List<string> dynamicNodeNameList = new List<string>();

    public HashSet<string> LogicNodeNameSet = new HashSet<string> ()
    {
        "Condition",
        "MultiCondition",
        "Timer",
        "Empty",
        "End",
        "Loop",
        "Continue",
        "Break",
        "Switch",
        "Until",
        "ScriptTrans",
        "GetPosition",
        "GetDirection",
        "Curve",
    };
    public virtual string BackupFileName { get { return "back.bytes"; } }

    //public static new SkillEditor Instance;
    public D configData;
    public override T GetConfigData<T> () { return configData as T; }
    //public XEditorConfigData EditorConfigData;
    public float FrameCount = 30;
    public override float Length { get { return configData.Length; } }



    public ConfigGraph ()
    {
        EffectConfig.instance.LoadEffectTemplate(dynamicNodeNameList);
    }

    public override void Init (BlueprintEditor editor)
    {
        base.Init (editor);
        configData = new D ();
        graphConfigData = new GraphConfigData ();
        for (int i = 0; i < NodeNameArray.Length; ++i)
        {
            if (NodeNameArray[i] == "Condition")
            {
                string tmp = NodeNameArray[i];
                NodeNameArray[i] = NodeNameArray[0];
                NodeNameArray[0] = tmp;
            }
            if (NodeNameArray[i] == "MultiCondition")
            {
                string tmp = NodeNameArray[i];
                NodeNameArray[i] = NodeNameArray[1];
                NodeNameArray[1] = tmp;
            }
        }
        Array.Sort (NodeNameArray, 2, NodeNameArray.Length - 2);
        LoadFunctionHash (Application.dataPath + "/Editor/Blueprint/SkillEditor/XScriptFunc.txt");
    }

    public override void UnInit ()
    {

    }

    public override void Update ()
    {
        if (widgetList.Count != graphConfigData.NodeConfigList.Count)
        {
            ReBuildNodeByData (0);
        }

        if (SkillHoster.GetHoster != null)
        {
            if (!SkillHoster.GetHoster.SkillGraphInited && this is SkillGraph)
            {
                SkillHoster.GetHoster.SkillGraphInited = true;
                foreach (BaseSkillNode node in widgetList)
                {
                    node.SetDebug (false);
                }
                editorWindow.Repaint();
            }

            if (!SkillHoster.GetHoster.HitGraphInited && this is BehitGraph)
            {
                SkillHoster.GetHoster.HitGraphInited = true;
                foreach (BaseSkillNode node in widgetList)
                {
                    node.SetDebug (false);
                }
                editorWindow.Repaint();
            }

            int count = SkillHoster.GetHoster.debugQueue.Count;
            if (count > 0)
            {
                while (count-- > 0)
                {
                    SkillDebugData data = SkillHoster.GetHoster.debugQueue.Dequeue();
                    if (!SetDebug(data))
                        SkillHoster.GetHoster.debugQueue.Enqueue(data);
                }
                editorWindow.Repaint();
            }
            if (SkillHoster.GetHoster.debugQueue.Count > 1000)
                SkillHoster.GetHoster.debugQueue.Clear ();
        }
        else if (XEcsGamePlay.XEcs.DebugOpen)
        {
            if (XEcsGamePlay.XEcs.DebugBeginSkill == CFUtilPoolLib.XCommon.singleton.XHash (configData.Name))
            {
                XEcsGamePlay.XEcs.DebugBeginSkill = 0;
                foreach (BaseSkillNode node in widgetList)
                {
                    node.SetDebug (false);
                }
                editorWindow.Repaint();
            }

            int count = XEcsGamePlay.XEcs.DebugQueue.Count;
            if (count > 0)
            {
                while (count-- > 0)
                {
                    SkillDebugData data = XEcsGamePlay.XEcs.DebugQueue.Dequeue();
                    if (!SetDebug(data))
                        XEcsGamePlay.XEcs.DebugQueue.Enqueue(data);
                }
                editorWindow.Repaint();
            }
            if (XEcsGamePlay.XEcs.DebugQueue.Count > 1000)
                XEcsGamePlay.XEcs.DebugQueue.Clear ();
        }

        if (configData != null && Time.frameCount % SnapDelay == 0)
        {
            Snapshot();
        }
    }

    protected bool SetDebug (SkillDebugData data)
    {
        if (data.hash == CFUtilPoolLib.XCommon.singleton.XHash (configData.Name) ||
            ((this is SkillGraph) && data.hash == 2061703477))
        {
            foreach (BaseSkillNode node in widgetList)
            {
                if (node.GetHosterData<XBaseData> ().Index == data.index)
                {
                    node.SetDebug ();
                    break;
                }
            }
            return true;
        }

        return false;
    }

    public void RefreshDebugQueue ()
    {
        while (SkillHoster.GetHoster.debugQueue.Count != 0)
        {
            SkillDebugData data = SkillHoster.GetHoster.debugQueue.Dequeue ();
            foreach (BaseSkillNode node in widgetList)
            {
                if (node.GetHosterData<XBaseData> ().Index == data.index)
                {
                    node.SetDebug ();
                    break;
                }
            }
        }
    }

    public override void DrawExtra ()
    {
        base.DrawExtra ();

        DrawTimeLine ();
    }

    public void DrawTimeLine ()
    {
        float delta = GetDelta () * Scale;
        Vector2 start;
        Vector2 end;
        Color oldHandleColor = Handles.color;
        Color oldGUIColor = GUI.color;
        Handles.color = Color.white;
        GUI.color = Color.green;
        for (int i = (int) (scrollPosition.x / delta); i * delta < scrollPosition.x + editorWindow.position.width; ++i)
        {

            start = new Vector2 (i * delta, scrollPosition.y + 0);
            if (i % 10 == 0)
            {
                end = new Vector2 (i * delta, scrollPosition.y + 20);
            }
            else if (i % 5 == 0)
            {
                end = new Vector2 (i * delta, scrollPosition.y + 10);
            }
            else
            {
                end = new Vector2 (i * delta, scrollPosition.y + 5);
            }

            Handles.DrawLine (start, end);
            if (i % 10 == 0)
            {
                GUI.Label (new Rect (start.x, start.y, 100, 25), new GUIContent (i.ToString ()));
            }
        }
        Handles.color = Color.red;
        DrawEndLine (delta);

        GUI.color = oldGUIColor;
        Handles.color = oldHandleColor;
    }

    public override void DrawLine (float posX, Color color)
    {
        Vector2 start = new Vector2 (posX, scrollPosition.y);
        Vector2 end = new Vector2 (posX, scrollPosition.y + editorWindow.position.height);
        Color oldColor = Handles.color;
        Handles.color = color;
        Handles.DrawLine (start, end);
        Handles.color = oldColor;
    }

    protected virtual void DrawEndLine (float delta)
    {
        Vector2 start = new Vector2 (GetEndPosX () * delta, scrollPosition.y);
        Vector2 end = new Vector2 (GetEndPosX () * delta, scrollPosition.y + 50);
        Handles.DrawLine (start, end);
        Handles.DrawLine (start + new Vector2 (1, 0), end + new Vector2 (1, 0));
    }

    protected virtual void InitGraphData () { }

    public void OpenData (string file)
    {
        if (file.Length != 0)
        {
            curIndex = 0;
            ClearSnapShoot();

            InitGraphData ();

            DataPath = file;
            configData = DataIO.DeserializeEcsData<D> (file);
            graphConfigData = DataIO.DeserializeEcsData<GraphConfigData> (DataPath.Replace (".bytes", ".ecfg"));
            OpenResFile(file);
            ReBuildNodeByData ();
        }
    }

    #region SnapShoot
    string tmpPath = Application.dataPath.Replace("Assets", "Temp/SkillEditorTemp");
    int _curIndex = 0;
    int curIndex
    {
        get 
        {
            if (_curIndex == 0 && File.Exists(SnapShootTmpPath + "Index"))
                _curIndex = int.Parse(File.ReadAllText(SnapShootTmpPath + "Index"));
            return _curIndex; 
        }
        set
        {
            if (!Directory.Exists(tmpPath)) Directory.CreateDirectory(tmpPath);
            _curIndex = value;
            File.WriteAllText(SnapShootTmpPath + "Index", _curIndex.ToString());
        } 
    }
    float SnapDelay = 15;
    protected virtual string TmpKey => "";
    private string SnapShootPath => tmpPath + "/" + curIndex+ TmpKey;
    private string SnapShootTmpPath => tmpPath + "/tmp"+ TmpKey;
    private string UndoSnapShootPath => tmpPath + "/" + (curIndex - 2)+ TmpKey;
    private string RedoSnapShootPath => tmpPath + "/" + curIndex+ TmpKey;
    public override void Snapshot()
    {
        if (!Directory.Exists(tmpPath)) Directory.CreateDirectory(tmpPath);

        DataIO.SerializeEcsData<D>(SnapShootTmpPath + ".cfg", configData);
        DataIO.SerializeEcsData<GraphConfigData>(SnapShootTmpPath + ".ecfg", graphConfigData);

        string f1 = "";
        if (curIndex != 0) f1 = File.ReadAllText(tmpPath + "/" + (curIndex - 1) + TmpKey + ".cfg");
        else if (curIndex == 0 && File.Exists(SnapShootPath + ".cfg"))
            f1 = File.ReadAllText(SnapShootPath + ".cfg");
        string f2 = File.ReadAllText(SnapShootTmpPath + ".cfg");
        if (!f1.Equals(f2))
        {
            File.Delete(SnapShootPath + ".cfg");
            File.Delete(SnapShootPath + ".ecfg");
            File.Move(SnapShootTmpPath + ".cfg", SnapShootPath + ".cfg");
            File.Move(SnapShootTmpPath + ".ecfg", SnapShootPath + ".ecfg");
            ++curIndex;
            ClearSnapShoot();
        }
    }

    private void ClearSnapShoot()
    {
        int index = curIndex;
        while (File.Exists(tmpPath + "/" + index + TmpKey + ".cfg"))
        {
            File.Delete(tmpPath + "/" + index + TmpKey + ".cfg");
            File.Delete(tmpPath + "/" + index + TmpKey + ".ecfg");
            ++index;
        }
    }

    public override void UndoSnapshot()
    {
        if (!File.Exists(UndoSnapShootPath + ".cfg")) return;

        InitGraphData();

        configData = DataIO.DeserializeEcsData<D>(UndoSnapShootPath + ".cfg");
        graphConfigData = DataIO.DeserializeEcsData<GraphConfigData>(UndoSnapShootPath + ".ecfg");
        ReBuildNodeByData();

        --curIndex;
        File.WriteAllText(SnapShootTmpPath + "Index", curIndex.ToString());
    }

    public override void RedoSnapshot()
    {
        if (!File.Exists(RedoSnapShootPath + ".cfg")) return;

        InitGraphData();

        configData = DataIO.DeserializeEcsData<D>(RedoSnapShootPath + ".cfg");
        graphConfigData = DataIO.DeserializeEcsData<GraphConfigData>(RedoSnapShootPath + ".ecfg");
        ReBuildNodeByData();

        ++curIndex;
    }
    #endregion

    private void OpenResFile(string file)
    {
        var fi = new FileInfo(file);
        var parentDir = fi.Directory.Parent.Name;
        resMap.Clear();
        string assetPath = string.Format("{0}Config/{1}/{2}.asset",
            LoadMgr.singleton.editorResPath, parentDir, fi.Name.Replace(".bytes", ""));
        if (File.Exists(assetPath))
        {
            var asset = AssetDatabase.LoadAssetAtPath<ResRedirectConfig>(assetPath);
            foreach (var res in asset.resPath)
            {
                resMap[res.name + res.ext] = res.physicPath;
            }
        }
    }
    
    public void SaveData (string dir, bool force = false, bool refreshTemplate = false)
    {
        resMap.Clear();
        string file = dir;
        if (!force)
            file = EditorUtility.SaveFilePanel ("Select Dir", dir, configData.Name, "bytes");
        if (file.EndsWith ("bytes"))
        {
            if (Path.GetFileNameWithoutExtension (file) != configData.Name)
            {
                ShowNotification (new GUIContent ("文件名和路径名不匹配!!\n"));
                return;
            }
            if (PreBuildConfigData ())
            {
                DataPath = file;
                PreBuild ();
                foreach (BaseSkillNode node in widgetList)
                {
                    node.PreBuild ();
                }

                configData.Version = (int)System.DateTime.Now.Ticks;
                configData = SerializeConfigData (DataPath, configData);
                DataIO.SerializeEcsData<GraphConfigData> (DataPath.Replace (".bytes", ".ecfg"), graphConfigData);
                if (refreshTemplate) RefreshScriptByTemplate ();
                SaveResFile(Path.GetDirectoryName(file), configData.Name);
                ShowNotification (new GUIContent ("Save Success!!\n" + DataPath));
            }
        }
    }

    private void SaveResFile(string file, string name)
    {
        var fi = new FileInfo(file);
        var parentDir = fi.Directory.Parent.Name;
        string dirPath = string.Format("{0}Config/{1}",
            LoadMgr.singleton.editorResPath, parentDir);
        if(!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        string assetPath = string.Format("{0}Config/{1}/{2}.asset",
            LoadMgr.singleton.editorResPath, parentDir, name);
        ResRedirectConfig srcAsset = null;
        if (File.Exists(assetPath))
        {
            srcAsset = AssetDatabase.LoadAssetAtPath<ResRedirectConfig>(assetPath);
        }
        else if (resMap.Count > 0)
        {
            srcAsset = ScriptableObject.CreateInstance<ResRedirectConfig>();
            srcAsset.name = name;
        }
        if (srcAsset != null)
        {
            srcAsset.resPath.Clear();
            foreach (var res in resMap)
            {

                srcAsset.resPath.Add(new ResPathRedirect()
                {
                    name = Path.GetFileNameWithoutExtension(res.Key),
                    ext = Path.GetExtension(res.Key),
                    physicPath = res.Value
                });
            }
            EditorCommon.CreateAsset<ResRedirectConfig>(assetPath, ".asset", srcAsset);
        }
    }

    public D SerializeConfigData(string path, D data)
    {
        DataIO.SerializeEcsData<D>(path, data);
        return data;
    }

    public void BackupData ()
    {
        string backupFile = Application.dataPath + "/Editor Default Resources/SkillBackup/" + BackupFileName;
        PreBuildConfigData ();
        {
            configData = SerializeConfigData (backupFile, configData);
            string tmp = configData.Name;
            configData.Name = BackupFileName.Replace (".bytes", "");
            configData = SerializeConfigData (backupFile + "RT", configData);
            configData.Name = tmp;
            graphConfigData.resMap.Clear();
            foreach (var res in resMap)
            {
                graphConfigData.resMap.Add(new ResRedirectData()
                {
                    nameWithExt = res.Key,
                    physicPath = res.Value
                });
            }
            DataIO.SerializeEcsData<GraphConfigData> (backupFile.Replace (".bytes", ".ecfg"), graphConfigData);

        }
    }

    public void ReloadBackupData ()
    {
        string backupFile = Application.dataPath + "/Editor Default Resources/SkillBackup/" + BackupFileName;
        if (File.Exists (backupFile))
        {
            configData = DataIO.DeserializeEcsData<D> (backupFile);
            graphConfigData = DataIO.DeserializeEcsData<GraphConfigData> (backupFile.Replace (".bytes", ".ecfg"));
            foreach (var res in graphConfigData.resMap)
            {
                resMap[res.nameWithExt] = res.physicPath;
            }
            ReBuildNodeByData ();
        }
    }

    #region EditorDataToXmlData
    private int NodeCount = 0;
    public bool PreBuildConfigData (bool NeedCompile = true)
    {
        var newData = new GraphConfigData ();
        if (graphConfigData != null)
        {
            newData.Copy (graphConfigData);
        }
        graphConfigData = newData;

        NodeCount = 0;

        foreach (string nodeName in NodeNameArray)
        {
            string dataType = string.Format ("EcsData.X{0}Data", nodeName);
            string nodeType = string.Format ("EditorNode.{0}Node", nodeName);
            string listName = string.Format ("{0}Data", nodeName);
            ReBuildDataByNodeTemplate (dataType, nodeType, listName);
        }
        if (DynamicNodeNameArray != null)
        {
            foreach (string nodeName in DynamicNodeNameArray)
            {
                string dataType = string.Format ("EcsData.X{0}Data", nodeName);
                string nodeType = string.Format ("EditorNode.{0}Node", nodeName);
                string listName = string.Format ("{0}Data", nodeName);
                ReBuildDataByNodeTemplate (dataType, nodeType, listName);
            }
        }

        foreach (BaseSkillNode node in widgetList)
        {
            node.BuildDataByPin ();
        }

        BuildDataFinish ();

        foreach (BaseSkillNode node in widgetList)
        {
            node.BuildDataFinish ();
        }
        
        if (NeedCompile)
        {

            foreach (BaseSkillNode node in widgetList)
            {
                node.hasError = false;
                try
                {
                    if (!node.CompileCheck ())
                    {
                        return false;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError (e.Message + "\n" + e.StackTrace);
                    string NStr = (e is EntryPointNotFoundException || e is TypeInitializationException) ? "C++Native报错 (请重启Unity)" : "";
                    ShowNotification (new GUIContent (NStr + "\n" + e.Message), string.IsNullOrEmpty (NStr) ? 5 : 30);
                    return false;
                }
            }
        }

        return true;
    }

    public virtual void BuildDataFinish () { }

    public void ListClear<T> (ref List<T> list) where T : XBaseData
    {
        list.Clear ();
    }

    public virtual void PreBuild () { }

    private void ReBuildDataByNodeTemplate (string typename1, string typename2, string listName)
    {
        Type t1 = assembly.GetType (typename1);
        Type t2 = Type.GetType (typename2);
        object[] list = new object[] { configData.GetType ().GetField (listName).GetValue (configData) };
        MethodInfo clear = this.GetType ().GetMethod ("ListClear").MakeGenericMethod (new Type[] { t1 });
        clear.Invoke (this, list);
        MethodInfo mi = this.GetType ().GetMethod ("ReBuildDataByNode").MakeGenericMethod (new Type[] { t1, t2 });
        mi.Invoke (this, list);
    }

    public void ReBuildDataByNode<T, N> (ref List<T> dataList) where T : XBaseData where N : BaseSkillNode, new ()
    {
        foreach (BaseSkillNode tmp in widgetList)
        {
            if (tmp is N)
            {
                N node = tmp as N;
                graphConfigData.NodeConfigList.Add (node.nodeEditorData);
                dataList.Add (node.GetHosterData<T> ());
                node.GetHosterData<T> ().Index = NodeCount++;

                string tag = typeof (T).ToString ();
                tag = tag.Remove (0, tag.LastIndexOf ('.') + 2).Replace ("Data", "");
                node.nodeEditorData.Tag = tag;
                if (node.GetHosterData<T> ().Index != -1) node.nodeEditorData.Tag += "_" + node.GetHosterData<T> ().Index.ToString ();

                if (!node.GetHosterData<T> ().TimeBased) node.GetHosterData<T> ().At = 0;
            }
        }
    }
    #endregion

    #region XmlDataToEditorData
    Dictionary<int, BaseSkillNode> IndexToNodeDic = new Dictionary<int, BaseSkillNode> ();
    private void ReBuildNodeByData (int Index = -1)
    {
        widgetList.Clear ();
        selectNode = null;
        NodeCount = 0;
        IndexToNodeDic.Clear ();

        foreach (string nodeName in NodeNameArray)
        {
            string dataType = string.Format ("EcsData.X{0}Data", nodeName);
            string nodeType = string.Format ("EditorNode.{0}Node", nodeName);
            string listName = string.Format ("{0}Data", nodeName);
            ReBuildNodeByDataTemplate (dataType, nodeType, listName, Index);
        }
        if (DynamicNodeNameArray != null)
        {
            foreach (string nodeName in DynamicNodeNameArray)
            {
                string dataType = string.Format ("EcsData.X{0}Data", nodeName);
                string nodeType = string.Format ("EditorNode.{0}Node", nodeName);
                string listName = string.Format ("{0}Data", nodeName);
                ReBuildNodeByDataTemplate (dataType, nodeType, listName, Index);
            }
        }
        foreach (BaseSkillNode node in widgetList)
        {
            node.BuildPinByData (IndexToNodeDic);
        }

        foreach (BaseSkillNode node in widgetList)
        {
            node.BuildDataFinish ();
        }
    }

    private static Assembly assembly = Assembly.Load ("CFUtilPoolLib");
    private void ReBuildNodeByDataTemplate (string typename1, string typename2, string listName, int index)
    {
        if (configData == null) return;

        Type t1 = assembly.GetType (typename1);
        Type t2 = Type.GetType (typename2);
        MethodInfo mi = this.GetType ().GetMethod ("ReBuildNodeByData").MakeGenericMethod (new Type[] { t1, t2 });
        object[] list = new object[] { configData.GetType ().GetField (listName).GetValue (configData), index };
        mi.Invoke (this, list);
    }

    public void ReBuildNodeByData<T, N> (List<T> dataList, int index) where T : XBaseData where N : BaseSkillNode, new ()
    {
        NodeConfigData nodeConfig;
        foreach (T data in dataList)
        {
            BaseSkillNode node = null;
            try
            {
                nodeConfig = graphConfigData.NodeConfigList[index == -1 ? data.Index : NodeCount++];
                node = AddNode<T>(new N(), data, nodeConfig, nodeConfig.Position);

                IndexToNodeDic.Add(node.GetHosterData<T>().Index, node);
            }
            catch
            {
                Debug.LogError("ConfigName: " + configData.Name + "Error !!!" + "   Index: " + (index == -1 ? data.Index : NodeCount));
            }
        }
    }
    #endregion

    #region AddEditorInGraph

    protected virtual void AddDynamicMenu (GenericMenu menu, Event e)
    {
        foreach (var nodeName in dynamicNodeNameList)
        {
            ShaderEffectNode.AddMenu(this, menu, e, nodeName);
        }
        menu.AddSeparator("AddAction/ShaderEffect/");
        menu.AddItem(
            new GUIContent("AddAction/ShaderEffect/Reload"),
            false,
            (object o) => { EffectConfig.instance.LoadEffectTemplate(null); }, e);
    }

    protected override void OnMouseRightClicked (Event e)
    {
        var genericMenu = new GenericMenu ();
        if (CacheNode != null || CacheNodeList.Count != 0)
        {
            genericMenu.AddItem(new GUIContent("Paste"), false, OnPasteClicked, e);
        }

        foreach (string nodeName in NodeNameArray)
        {
            string dataType = string.Format ("EcsData.X{0}Data", nodeName);
            string nodeType = string.Format ("EditorNode.{0}Node", nodeName);
            string listName = string.Format ("{0}Data", nodeName);
            genericMenu.AddItem (
                new GUIContent ((LogicNodeNameSet.Contains (nodeName) ? "AddLogic/" : "AddAction/") + nodeName),
                false,
                (object o) => { AddNodeInGraphTemplate (dataType, nodeType, listName, o); }, e);
        }
        AddDynamicMenu (genericMenu, e);
        genericMenu.ShowAsContext ();
    }

    Dictionary<int, int> copyIndexDic = new Dictionary<int, int> ();
    int _paste_index = 0;
    protected override void OnPasteClicked (object o)
    {
        PreBuildConfigData (false);
        _paste_index = widgetList.Count;
        copyIndexDic.Clear ();
        if (CacheNodeList.Count == 0)
        {
            copyIndexDic[CacheNode.GetHosterData<XBaseData>().Index + _paste_index] = _paste_index;

            string nodeName = CacheNode.nodeEditorData.Tag;
            if (nodeName.LastIndexOf('_') != -1) nodeName = nodeName.Remove(nodeName.LastIndexOf('_'));

            string dataType = string.Format("EcsData.X{0}Data", nodeName);
            string nodeType = string.Format("EditorNode.{0}Node", nodeName);
            string listName = string.Format("{0}Data", nodeName);
            MethodInfo mi = this.GetType().GetMethod("CopyNodeInGraph").MakeGenericMethod(new Type[] { assembly.GetType(dataType), Type.GetType(nodeType) });
            object[] list = new object[]
            {
                ((Event) o).mousePosition, configData.GetType ().GetField (listName).GetValue (configData), CacheNode
            };
            mi.Invoke(this, list);
        }
        else
        {
            int i = 0;
            foreach (BaseSkillNode node in CacheNodeList)
            {
                copyIndexDic[node.GetHosterData<XBaseData>().Index + _paste_index] = _paste_index + i;
                i++;
            }

            foreach (BaseSkillNode node in CacheNodeList)
            {
                string nodeName = node.nodeEditorData.Tag;
                if (nodeName.LastIndexOf('_') != -1) nodeName = nodeName.Remove(nodeName.LastIndexOf('_'));

                string dataType = string.Format("EcsData.X{0}Data", nodeName);
                string nodeType = string.Format("EditorNode.{0}Node", nodeName);
                string listName = string.Format("{0}Data", nodeName);
                MethodInfo mi = this.GetType().GetMethod("CopyNodeInGraph").MakeGenericMethod(new Type[] { assembly.GetType(dataType), Type.GetType(nodeType) });
                object[] list = new object[]
                {
                    ((Event) o).mousePosition + node.Bounds.position - CacheNodeList[0].Bounds.position, configData.GetType ().GetField (listName).GetValue (configData), node
                };
                mi.Invoke(this, list);
            }

        }

        ReBuildNodeByData ();
        ClearMultiselect ();
        foreach (BaseSkillNode node in widgetList)
        {
            if (node.GetHosterData<XBaseData> ().Index >= _paste_index)
            {
                node.IsSelected = true;
                selectNodeList.Add (node);
            }
        }
        PreBuildConfigData (false);
    }

    public void CopyNodeInGraph<T, N>(Vector2 pos, ref List<T> list, N from) where T : XBaseData where N : BaseSkillNode, new()
    {
        N node = new N();
        T fromData = from.GetHosterData<T>();
        T data = node.CopyData<T>(fromData);
        list.Add(data);
        data.Index = copyIndexDic[data.Index + _paste_index];
        data.TransferData = new List<XTransferData>();
        for (int i = 0; i < fromData.TransferData.Count; ++i)
        {
            XTransferData tdata = new XTransferData();
            tdata.Index = fromData.TransferData[i].Index == -1 ? -1 : (copyIndexDic.ContainsKey(fromData.TransferData[i].Index + _paste_index) ? copyIndexDic[fromData.TransferData[i].Index + _paste_index] : -1);
            data.TransferData.Add(tdata);
        }
        Vector3 targetPos = (pos + scrollPosition / Scale);
        targetPos.x = targetPos.x < 0 ? 0 : targetPos.x;
        targetPos.y = targetPos.y < 0 ? 0 : targetPos.y;
        targetPos.z = targetPos.z < 0 ? 0 : targetPos.z;
        AddNode<T>(node, data, node.nodeEditorData, targetPos);
        node.nodeEditorData.TitleName = from.nodeEditorData.TitleName;
        node.nodeEditorData.CustomData = from.nodeEditorData.CustomData;
        node.nodeEditorData.CustomData1 = from.nodeEditorData.CustomData1;

        graphConfigData.NodeConfigList.Add(node.nodeEditorData);
    }

    private void AddNodeInGraphTemplate (string typename1, string typename2, string listName, object o)
    {
        Type t1 = assembly.GetType (typename1);
        Type t2 = Type.GetType (typename2);
        MethodInfo mi = this.GetType ().GetMethod ("AddNodeInGraph").MakeGenericMethod (new Type[] { t1, t2 });
        object[] list = new object[] { o, configData.GetType ().GetField (listName).GetValue (configData) };
        mi.Invoke (this, list);

        PreBuildConfigData (false);
    }

    public void AddNodeInGraph<T, N> (Event e, ref List<T> list) where T : XBaseData where N : BaseSkillNode, new ()
    {
        N node = new N ();
        list.Add (node.GetHosterData<T> ());
        AddNode<T> (node, node.GetHosterData<T> (), node.nodeEditorData, (e.mousePosition + scrollPosition) / Scale);
        graphConfigData.NodeConfigList.Add (node.nodeEditorData);
    }

    public override N AddNodeInGraphByScript<T, N> (Vector2 pos, ref List<T> list, bool absPos = false)
    {
        N node = new N ();
        list.Add (node.GetHosterData<T> ());
        if (absPos)
        {
            pos = pos / Scale;
        }
        else
        {
            pos = (pos + scrollPosition) / Scale;
        }
        AddNode<T> (node, node.GetHosterData<T> (), node.nodeEditorData, pos);
        graphConfigData.NodeConfigList.Add (node.nodeEditorData);
        PreBuildConfigData (false);

        return node;
    }

    protected BaseSkillNode AddNode<T> (BaseSkillNode node, T data, NodeConfigData nodeData, Vector2 pos) where T : XBaseData
    {
        node.InitData<T> (data, nodeData);
        node.Init (this, pos);
        AddNode (node);

        return node;
    }
    #endregion

    #region Template
    public bool BuildFromTemplate(bool check = true)
    {
        string file = configData.CopyFrom;
        if (this is SkillGraph)
            file = Application.dataPath + "/BundleRes/SkillPackage/" + file;
        else if (this is BehitGraph)
            file = Application.dataPath + "/BundleRes/HitPackage/" + file;

        D templateData = DataIO.DeserializeEcsData<D>(file);

        graphConfigData = DataIO.DeserializeEcsData<GraphConfigData>(file.Replace(".bytes", ".ecfg"));
        int templateID = templateData.PresentID;
        templateData.PresentID = configData.PresentID;
        templateData.CopyFrom = configData.CopyFrom;
        templateData.AnimTemplate = configData.AnimTemplate;
        templateData.AudioTemplate = configData.AudioTemplate;
        templateData.Name = configData.Name;
        templateData.ParamNames = new List<string>();
        for (int i = 0; i < configData.ParamNames.Count; ++i)
            templateData.ParamNames.Add(configData.ParamNames[i]);

        List<BluePrintWidget> tempWidgetList = new List<BluePrintWidget>();
        for (int i = 0; i < widgetList.Count; ++i)
            tempWidgetList.Add(widgetList[i]);

        configData = templateData;
        ReBuildNodeByData();

        if (check)
        {
            if (widgetList.Count != tempWidgetList.Count) return false;
            for (int i = 0; i < widgetList.Count; ++i)
            {

                if (((BaseSkillNode)widgetList[i]).GetHosterData<XBaseData>().GetType() != ((BaseSkillNode)tempWidgetList[i]).GetHosterData<XBaseData>().GetType() ||
                    ((BaseSkillNode)widgetList[i]).GetHosterData<XBaseData>().Index != ((BaseSkillNode)tempWidgetList[i]).GetHosterData<XBaseData>().Index)
                    return false;
            }
        }

        for (int i = 0; i < widgetList.Count; ++i)
        {
            ((BaseSkillNode)widgetList[i]).CopyDataFromTemplate(templateID, configData.PresentID);
        }
        ReBuildNodeByData();

        return true;
    }

    private void RefreshScriptByTemplate ()
    {
        if (!string.IsNullOrEmpty (configData.CopyFrom))
            return;
        string[] files = Directory.GetFiles (Application.dataPath + ((this is BehitGraph) ? "/BundleRes/HitPackage" : "/BundleRes/SkillPackage"), "*.bytes", SearchOption.AllDirectories);

        int index = 0;
        string templatePath = DataPath;
        string errorFile = "";
        bool forceAll = false;
        bool forceAllSelect = false;
        foreach (string file in files)
        {
            OpenData (file);

            EditorUtility.DisplayProgressBar ("Refresh", "Refresh Script By Template", (++index / (1.0f * files.Length)));
            if (string.IsNullOrEmpty (configData.CopyFrom) || !templatePath.EndsWith (configData.CopyFrom)) continue;
            if (BuildFromTemplate())
                SaveData(file, true);
            else
            {
                if(forceAll)
                {
                    if (forceAllSelect)
                    {
                        BuildFromTemplate(false);
                        SaveData(file, true);
                    }
                    else errorFile += "\n" + file;
                }
                else
                {
                    ForceRefreshTemplateCheck(file,
                        () => { forceAll = true; forceAllSelect = true; },
                        () => { forceAll = true; forceAllSelect = false; },
                        () => { BuildFromTemplate(false); SaveData(file, true); },
                        () => { errorFile += "\n" + file; });
                }
            }
        }

        if (!string.IsNullOrEmpty(errorFile))
        {
            Debug.LogError("Refresh By Template Error!!\n" + errorFile);
            EditorUtility.DisplayDialog("Refresh By Template Error!!", errorFile, "OK");
        }
        EditorUtility.ClearProgressBar ();
        OpenData (templatePath);
    }
    private void ForceRefreshTemplateCheck(string file, Action okAll, Action cancelAll, Action ok, Action cancel)
    {
        switch (EditorUtility.DisplayDialogComplex("Refresh Script By Template", file + "与模板结构有差异，是否强制刷新？", "是", "对剩余脚本执行统一操作", "否"))
        {
            case 0:
                ok();
                break;
            case 1:
                RefreshAllCheck(file, okAll, cancelAll, ok, cancel);
                break;
            case 2:
                cancel();
                break;
        }
    }
    private void RefreshAllCheck(string file, Action okAll, Action cancelAll, Action ok, Action cancel)
    {
        switch (EditorUtility.DisplayDialogComplex("Refresh Script By Template", "对剩余脚本执行强制刷新？", "是", "取消", "否"))
        {
            case 0:
                ok();
                okAll();
                break;
            case 1:
                ForceRefreshTemplateCheck(file, okAll, cancelAll, ok, cancel);
                break;
            case 2:
                cancel();
                cancelAll();
                break;
        }
    }
    #endregion

    #region KeyboardData
    protected SkillEditorKeyboardData keyboardData = null;
    protected List<string> keyboardSkillList = null;
    protected List<float> combatTime = null;
    protected List<string> combatSkill = null;
    bool showKeyboardData = false;
    void ShowKeyboardDataInspector (int presentID, string skill)
    {
        if (presentID == 0) return;

        if (keyboardData == null)
        {
            keyboardData = SkillEditorKeyboardData.LoadData ();
            keyboardSkillList = null;
            combatTime = null;
            combatSkill = null;
        }
        if (keyboardData == null) keyboardData = new SkillEditorKeyboardData ();
        int index = 0;

        if (keyboardSkillList == null)
        {
            for (; index < keyboardData.PresentList.Count; ++index)
            {
                if (keyboardData.PresentList[index] == presentID)
                {
                    keyboardSkillList = keyboardData.SkillList[index];
                    break;
                }
            }
            if (keyboardSkillList == null) keyboardSkillList = new List<string> ();

            if (index >= keyboardData.PresentList.Count)
            {
                keyboardData.PresentList.Add (presentID);
                keyboardData.SkillList.Add (keyboardSkillList);
            }
        }

        index = 0;
        if (combatTime == null)
        {
            for (; index < keyboardData.MapKey_Skill.Count; ++index)
            {
                if (keyboardData.MapKey_Skill[index] == skill)
                {
                    combatTime = keyboardData.MapValue_TriggerTime[index];
                    combatSkill = keyboardData.MapValue_TriggerSkill[index];
                    break;
                }
            }
            if (combatTime == null)
            {
                combatTime = new List<float> ();
                combatSkill = new List<string> ();
            }

            if (index >= keyboardData.MapKey_Skill.Count)
            {
                keyboardData.MapKey_Skill.Add (skill);
                keyboardData.MapValue_TriggerTime.Add (combatTime);
                keyboardData.MapValue_TriggerSkill.Add (combatSkill);
            }
        }

        if (showKeyboardData)
        {
            EditorGUITool.LabelField ("CombatSkill", (GUILayoutOption)null, this.GetType().ToString().Replace("EditorNode.", ""));
            for (int i = 0; i < combatTime.Count; ++i)
            {
                EditorGUILayout.BeginHorizontal ();
                combatSkill[i] = EditorGUILayout.TextField (combatSkill[i]);
                GUILayout.FlexibleSpace ();
                combatTime[i] = EditorGUILayout.FloatField (combatTime[i], GUILayout.Width (50));
                EditorGUITool.LabelField ("(s)", GUILayout.Width (20));
                combatTime[i] = FrameToTime (EditorGUILayout.IntField ((int) TimeToFrame (combatTime[i]), GUILayout.Width (30)));
                EditorGUITool.LabelField ("(frame)", GUILayout.Width (45));
                EditorGUILayout.EndHorizontal ();
            }
        }

        EditorGUILayout.BeginHorizontal ();
        showKeyboardData = EditorGUILayout.Toggle (showKeyboardData);

        if (GUILayout.Button ("AddC") && combatTime.Count < 9)
        {
            showKeyboardData = true;
            combatTime.Add (0);
            combatSkill.Add ("");
        }
        if (GUILayout.Button ("DelC") && combatTime.Count > 0)
        {
            combatTime.RemoveAt (combatTime.Count - 1);
            combatSkill.RemoveAt (combatSkill.Count - 1);
        }

        if (GUILayout.Button ("Add") && keyboardSkillList.Count < 9)
        {
            showKeyboardData = true;
            keyboardSkillList.Add ("");
        }
        if (GUILayout.Button ("Del") && keyboardSkillList.Count > 0) keyboardSkillList.RemoveAt (keyboardSkillList.Count - 1);
        if (GUILayout.Button ("Save")) keyboardData.CacheData ();
        if (GUILayout.Button ("Load"))
        {
            keyboardData = SkillEditorKeyboardData.LoadData ();
            keyboardSkillList = null;
            combatTime = null;
            combatSkill = null;
        }
        EditorGUILayout.EndHorizontal ();

        if (showKeyboardData)
        {
            for (int i = 0; keyboardSkillList != null && i < keyboardSkillList.Count; ++i)
            {
                keyboardSkillList[i] = EditorGUITool.TextField ("Slot_" + (i + 1), keyboardSkillList[i]);
            }
        }
        EditorGUILayout.Space ();
    }
    #endregion

    public GameObject PrefabObject = null;
    public Vector3 PrefabPos = Vector3.zero;
    #region HitObject
    private int HitPresentID = 0;
    #endregion
    #region ActionRatio
    private float ActionRatio = 1;
    #endregion
    
    public override void DrawDataInspector ()
    {
        if (configData != null)
        {
            if (!string.IsNullOrEmpty (configData.CopyFrom))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUITool.LabelField ("CopyFrom:");
                if (GUILayout.Button("RemoveTemplate"))
                {
                    configData.CopyFrom = null;
                    configData.AudioTemplate = null;
                    configData.AnimTemplate = null;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUITool.LabelField (configData.CopyFrom);
                configData.AudioTemplate = EditorGUITool.TextField("AudioTemplate", configData.AudioTemplate);
                configData.AnimTemplate = EditorGUITool.TextField("AnimTemplate", configData.AnimTemplate);
                if (GUILayout.Button ("RefreshByPresentID"))
                {
                    if (!BuildFromTemplate())
                    {
                        Debug.LogError("Refresh By Template Error!!");
                        ShowNotification(new GUIContent("Refresh By Template Error!!\n"), 5);
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty (DataPath))
                {
                    if (GUILayout.Button ("BuildTemplate"))
                    {
                        if (this is SkillGraph)
                        {
                            configData.CopyFrom = DataPath.Remove (0, DataPath.IndexOf ("SkillPackage") + 13);
                        }
                        else if (this is BehitGraph)
                        {
                            configData.CopyFrom = DataPath.Remove (0, DataPath.IndexOf ("HitPackage") + 11);
                        }
                    }
                }
            }

            CFUtilPoolLib.XEntityPresentation.RowData presentData = XEntityPresentationReader.GetData ((uint) configData.PresentID);
            configData.PresentID = EditorGUITool.IntField ("ID(" + (presentData != null ? presentData.Prefab : "") + ")", configData.PresentID);

            PrefabObject = XEntityPresentationReader.GetDummy ((uint) configData.PresentID);
            ShowKeyboardDataInspector (configData.PresentID, configData.Name);
            if (this is SkillGraph)
            {
                presentData = XEntityPresentationReader.GetData ((uint) HitPresentID);
                HitPresentID = EditorGUITool.IntField ("HitID(" + (presentData != null ? presentData.Prefab : "") + ")", HitPresentID);

                if (HitPresentID != 0 && Application.isPlaying)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("AddPupet"))
                    {
                        SkillHoster.GetHoster.CreatePuppet(HitPresentID,0,0,0,0,0, (int)SkillEditor.GetCurrentLod);
                    }
                    
                    
                    if (GUILayout.Button("AddPupetAtPos"))
                    {
                        var playerPos = SkillHoster.GetHoster.EntityDic[SkillHoster.PlayerIndex].obj.transform.position;
                        SkillHoster.GetHoster.CreatePuppet(HitPresentID, 0, PrefabPos.x + playerPos.x, PrefabPos.y + playerPos.y, PrefabPos.z + playerPos.z);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                if (Application.isPlaying)
                {
                    ActionRatio = EditorGUITool.Slider("ActionRatio", ActionRatio, 0.01f, 3f);
                    if (GUILayout.Button ("SetActionRatio"))
                    {
                        SkillHoster.GetHoster.SetActionRatio (ActionRatio);
                    }
                }

                EditorGUILayout.Space ();
            }
            PrefabPos = EditorGUILayout.Vector3Field ("", PrefabPos);
            configData.Name = EditorGUITool.TextField("脚本名: ", configData.Name);
            configData.Length = TimeFrameField("时长: ", configData.Length, false);
        }
    }

    protected float GetEndPosX ()
    {
        if (configData == null) return 0;

        return configData.Length * FrameCount;
    }

    public override float TimeToPosX (float time)
    {
        return time * FrameCount * GetDelta ();
    }

    public override float PosXToTime (float posX)
    {
        return posX / FrameCount / GetDelta ();
    }

    public override float TimeToFrame (float time)
    {
        return time * FrameCount;
    }
    public override float FrameToTime (float frame)
    {
        return frame / FrameCount;
    }

    public override void DeleteNode (BluePrintNode node)
    {
        base.DeleteNode (node);
        PreBuildConfigData (false);
    }

    private void LoadFunctionHash (string path)
    {
        FunctionHash.Clear ();
        FunctionName.Clear ();
        FunctionFlag.Clear ();
        FunctionName2Hash.Clear ();
        FunctionHash2Name.Clear ();

        using (StreamReader reader = File.OpenText (path))
        {
            if (reader == null) return;

            string file = reader.ReadToEnd ();
            while (!string.IsNullOrEmpty (file))
            {
                int start = file.IndexOf ("script_proto_type* ");
                if (start == -1) return;
                file = file.Substring (start);
                file = file.Substring (file.IndexOf (" ") + 1);
                string key = file.Substring (0, file.IndexOf ('=')).Trim (' ', '\t');
                file = file.Substring (file.IndexOf (";") + 1);
                string flag = file.Substring (0, file.IndexOf ('\n'));

                int hash = (int) CFUtilPoolLib.XCommon.singleton.XHash (key);
                FunctionHash.Add (hash);
                FunctionName.Add (key);
                //TODO FunctionFlag大概率存在问题
                FunctionFlag.Add (flag);
                FunctionName2Hash.Add (key, hash);
                FunctionHash2Name.Add (hash, key);
            }
        }
    }

    protected virtual string RuntimeCacheName { get; }

    public static bool SoloCamera = false;
    public static string SoloCameraPath = "";
    public static GameObject SoloCameraObj = null;

    public void CacheRuntimeData (string name = "")
    {
        SkillEditorRuntimeData data = new SkillEditorRuntimeData ();
        data.DataPath = DataPath;
        data.SoloCamera = SoloCamera;
        if (SoloCameraObj != null) data.SoloCameraPath = AssetDatabase.GetAssetPath(SoloCameraObj);
        else data.SoloCameraPath = "";
        data.CacheEditorRuntimeData (RuntimeCacheName);
    }

    public void LoadRuntimeData(string name = "")
    {
        SkillEditorRuntimeData data = SkillEditorRuntimeData.LoadEditorRuntimeData(RuntimeCacheName);
        if (data == null) return;

        DataPath = data.DataPath;
        SoloCamera = data.SoloCamera;
        SoloCameraPath = data.SoloCameraPath;
        if (!string.IsNullOrEmpty(SoloCameraPath)) SoloCameraObj = AssetDatabase.LoadAssetAtPath(SoloCameraPath, typeof(GameObject)) as GameObject;
        else SoloCameraObj = null;
    }

    public string LoadCacheDirectory()
    {
        SkillEditorRuntimeData data = SkillEditorRuntimeData.LoadEditorRuntimeData(RuntimeCacheName);
        if (data == null) return "";
        return Path.GetFileName(Path.GetDirectoryName(data.DataPath));
    }
}