using System;
using System.Collections.Generic;
using CFEngine;
using UnityEditor;
using UnityEngine;
using BluePrint;
using CFUtilPoolLib;
using XLevel;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.IO;
using System.Text;

namespace LevelEditor
{
    public enum LEVEL_EDITOR_STATE
    {
        editor_mode,
        simulation_mode,
    }

    public class LevelEditor : BlueprintEditor
    {
        public static LevelEditor Instance { get; set; }

        public static int CURRENT_VERSION = 2;
        public static int MINI_SUPPORT_VERSION = 0;

        public LevelGraph MainLevelGraph;

        public static LEVEL_EDITOR_STATE state { get; set; }

        public LevelRuntimeEngine Simulator = new LevelRuntimeEngine();

        public LevelEditorData fullData;

        public static GUILayoutOption[] ButtonLayout = new GUILayoutOption[] { GUILayout.Width(45f) };

        public bool bExpand = false;
        public string CachedOpenFile = "";

        private bool bOpenOnRunning = false;

        public enum EditorMenuItem 
        {
            Item_New = 0,
            Item_Open = 1,
            Item_Save = 2,
            Item_Play = 3,
            Item_Pause = 4,
            Item_Stop = 5,
            Item_SaveTpl = 6,
            Item_LoadTpl = 7,
            Item_WallInfo = 8,
            Item_Compile = 9,
            Item_Fold = 10,

            Item_Max = 11,
        }

        public static GUIContent[] MenuGUIContents = new GUIContent[(int)EditorMenuItem.Item_Max];
        public static string[] MenuImage = new string[(int)EditorMenuItem.Item_Max] {
            "images/BluePrint/btn_editor_new.png",
            "images/BluePrint/btn_editor_open.png",
            "images/BluePrint/btn_editor_save.png",
            "images/BluePrint/btn_editor_play.png",
            "images/BluePrint/btn_editor_pause.png",
            "images/BluePrint/btn_editor_playnext.png",
            "images/BluePrint/btn_editor_favorite0.png",
            "images/BluePrint/btn_editor_favorite1.png",
            "images/BluePrint/btn_editor_wall.png",
            "images/BluePrint/btn_editor_build.png",
            "images/BluePrint/expand_down_arrow.png",
        };

        public static string[] MenuTips = new string[(int)EditorMenuItem.Item_Max] {
            "New",
            "Open",
            "Save",
            "Play",
            "Pause",
            "Stop",
            "SaveTpl",
            "LoadTpl",
            "WallInfo",
            "Compile",
            "Comment",
        };
        // public static GUIContent m_GUINew;
        // public static GUIContent m_GUIOpen;
        // public static GUIContent m_GUISave;
        // public static GUIContent m_GUIPlay;
        // public static GUIContent m_GUIPause;
        // public static GUIContent m_GUIStop;
        // public static GUIContent m_GUISaveTpl;
        // public static GUIContent m_GUILoadTpl;
        // public static GUIContent m_GUIWallInfo;
        // public static GUIContent m_GUICompile;
        // public static GUIContent m_GUIAddComment;

        List<bool> bTabState = new List<bool>();

        public int levelType;
        public GameObject DynamicSceneRoot;
        public GameObject StaticSceneRoot;
        public LevelMapData m_Data;
        private LevelGridDrawer m_Drawer;
        public string unityScene = "";
        public bool loadScene = false;

        public GameObject objGridRoot;

        [MenuItem("Window/Level")]
        public static void InitEmpty()
        {
            var window = (LevelEditor)GetWindow(typeof(LevelEditor));
            window.titleContent = new GUIContent("LevelEditor");
            window.wantsMouseMove = true;
            window.Show();
            window.Repaint();
            Instance = window;
        }

        public override void OnEnable()
        {
            base.OnEnable();

            Instance = this;
            OpenSnap = true;

            UnityEditor.SceneManagement.EditorSceneManager.sceneOpened -= OnSceneOpen;
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += OnSceneOpen;

            LevelEditorTableData.ReadTable();

            if (CurrentGraph == null)
            {
                LevelGraph g = NewGraph<LevelGraph>(0, "main");
                MainLevelGraph = g as LevelGraph;
                OpenGrahps.Add(MainLevelGraph);
            }

            SceneView.duringSceneGui -= OnSceneFunc;
            SceneView.duringSceneGui += OnSceneFunc;

            for(int i = 0 ; i < (int)EditorMenuItem.Item_Max; ++i)
            {
                MenuGUIContents[i] = new GUIContent(EditorGUIUtility.Load(MenuImage[i]) as Texture, MenuTips[i]);
            }

            
        }

        public override int NewSubGraph()
        {
            LevelGraph g = NewGraph<LevelGraph>(0, "", false);
            return g.GraphID;
        }

        public override BluePrintGraph CloneGraph(BluePrintGraph sourceGraph)
        {
            string name = sourceGraph.GraphName +  "(Clone)";
            LevelGraph g = NewGraph<LevelGraph>(0, name, false);

            (sourceGraph as LevelGraph).Clone(ref g);

            return g;
        }


        public override void OnDisable()
        {
            if(state == LEVEL_EDITOR_STATE.simulation_mode)
            {
                StopSimulation();
            }
            SceneView.duringSceneGui -= OnSceneFunc;

            bOpenOnRunning = false;
            if (m_Data != null)
            {
                m_Data.Clear();
                m_Data = null;
            }

            if (m_Drawer != null)
            {
                m_Drawer.Clear();
                m_Drawer = null;
            }

            UnityEditor.SceneManagement.EditorSceneManager.sceneOpened -= OnSceneOpen;
            loadScene = false;
            base.OnDisable();
        }

        private void OnSceneOpen(Scene scene, OpenSceneMode mode)
        {
            if (!loadScene) return;

            FindDynamicRoot(true);
            LoadSceneGrid();
            
            (Graphs[0] as LevelGraph).ReloadStartOrEndPoint();
            // 现在的流程中，走到这里时，InternalLoad(CachedOpenFile)必定已经调用过，所以先注释
            //if(!string.IsNullOrEmpty(CachedOpenFile))
            //    InternalLoad(CachedOpenFile);

            loadScene = false;
        }
       
        public override void Update()
        {
            Instance = this;
            base.Update();

            if (CurrentGraph != null)
                CurrentGraph.Update();
        }
        #region save&load
        public override void ToolBarOpenClicked()
        {
            base.ToolBarOpenClicked();

            if (state == LEVEL_EDITOR_STATE.editor_mode)
            {
                string file = EditorUtility.OpenFilePanel("Select level file", Application.dataPath + "/BundleRes/Table/Level/", "cfg");

                if (file.Length != 0)
                {
                    CachedOpenFile = file;

                    if (EditorApplication.isPlaying)
                    {
                        ShowNotification(new GUIContent("编辑器运行时打开，只能看不能保存哦"), 5);
                        bOpenOnRunning = true;
                    }
                    else
                    {
                        bOpenOnRunning = false;
                    }

                    InternalLoad(file);
                }
            }
        
        }

        public override void ToolBarSaveClicked()
        {
            if(bOpenOnRunning)
            {
                ShowNotification(new GUIContent("运行时打开编辑器，只能看不能保存，否则要出大事"), 5);
                return;
            }
            if(!string.IsNullOrEmpty(CachedOpenFile))
            {
                if (!Compile())
                {
                    return;
                }
                InternalSave(CachedOpenFile, true);              
                CFEngine.Editor.SceneEditTool.Direct_SaveDynamicScene();//同时保存客户端墙文件
                var pType = PrefabUtility.GetPrefabAssetType(DynamicSceneRoot);
                if (pType == PrefabAssetType.Model||pType==PrefabAssetType.Regular||pType==PrefabAssetType.Variant) 
                {
                    var p = PrefabUtility.GetCorrespondingObjectFromOriginalSource(DynamicSceneRoot);
                    var path = AssetDatabase.GetAssetPath(p);
                    PrefabUtility.SaveAsPrefabAssetAndConnect(DynamicSceneRoot, path, InteractionMode.AutomatedAction);
                    AssetDatabase.Refresh();
                }
                return;
            }
            string file = EditorUtility.SaveFilePanel("Select Dir", Application.dataPath + "/BundleRes/Table/Level/",
                CachedOpenFile.Substring(CachedOpenFile.LastIndexOf('/')+1), "cfg");
            if (file.EndsWith("cfg"))
            {
                if (!Compile())
                {
                    return;
                }
                InternalSave(file, true);
                CFEngine.Editor.SceneEditTool.Direct_SaveDynamicScene();//同时保存客户端墙文件
                var pType = PrefabUtility.GetPrefabAssetType(DynamicSceneRoot);
                if (pType == PrefabAssetType.Model || pType == PrefabAssetType.Regular || pType == PrefabAssetType.Variant)
                {
                    var p = PrefabUtility.GetCorrespondingObjectFromOriginalSource(DynamicSceneRoot);
                    var path = AssetDatabase.GetAssetPath(p);
                    PrefabUtility.SaveAsPrefabAssetAndConnect(DynamicSceneRoot, path, InteractionMode.AutomatedAction);
                    AssetDatabase.Refresh();
                }
            } 
        }

        private void ConvenientSave()
        {
            if (string.IsNullOrEmpty(CachedOpenFile))
                return;

            LevelEditorData originData = DataIO.DeserializeData<LevelEditorData>(CachedOpenFile);
            fullData = new LevelEditorData();
            fullData.DynamicRoot = originData.DynamicRoot;
            fullData.LevelWallData = originData.LevelWallData;
            fullData.LevelType = levelType;
            fullData.Version = CURRENT_VERSION;

            editorConfigData = new EditorConfigData();
            editorConfigData.SceneName = unityScene;

            StringBuilder sb = new StringBuilder("未生效改动：");
            sb.AppendLine();

            for(var i=0;i<Graphs.Count;i++)
            {
                LevelGraph graph = Graphs[i] as LevelGraph;
                LevelGraphData originGraphData = originData.GraphDataList.Find(g => g.graphID == graph.GraphID);
                if (originGraphData == null)
                    continue;
                var fs = originGraphData.GetType().GetFields();
                List<int> removeList = new List<int>();
                for(var j=0;j<graph.widgetList.Count;j++)
                {
                    var node = graph.widgetList[j];
                    if(node is LevelWaveNode||node is LevelDoodadNode||node is LevelRobotWaveNode||node is LevelAppointPosNode||
                        (node is LevelScriptNode&&
                        (((node as LevelScriptNode).NodeDesc is LsNodeDescShowTarget)|| ((node as LevelScriptNode).NodeDesc is LSNodeDescTransferLocation))))
                    {
                        var nodeData = node.GetType().GetField("HostData").GetValue(node);
                        int nodeID = (int)nodeData.GetType().GetField("NodeID").GetValue(nodeData);
                        bool find = false;
                        foreach (var f in fs)
                        {
                            var list = f.GetValue(originGraphData);
                            Type listType = list.GetType();
                            if (!listType.IsGenericType)
                                continue;
                            var countProperty = listType.GetProperty("Count");
                            if (countProperty == null)
                                continue;
                            int count = Convert.ToInt32(listType.GetProperty("Count").GetValue(list));
                            BluePrintNodeBaseData hostData = null;
                            for(var k=0;k<count;k++)
                            {
                                var item = listType.GetProperty("Item").GetValue(list, new object[] { k });
                                var nodeIDProperty = item.GetType().GetField("NodeID");
                                if (nodeIDProperty == null)
                                    break;
                                if(Equals(nodeID,nodeIDProperty.GetValue(item)))
                                {
                                    hostData = (BluePrintNodeBaseData)item;
                                    break;
                                }
                            }
                            if(hostData!=null)
                            {
                                find = true;
                                node.GetType().GetField("HostData").SetValue(node,hostData);
                                sb.Append(string.Format("便捷保存时无法保存场景相关节点，数据已还原，nodetype:{0},graphid:{1},nodeid{2}",
                                    node.GetType(),graph.GraphID,nodeID));
                                sb.AppendLine();
                                break;
                            }
                        }
                        if (!find)
                        {
                            sb.Append(string.Format("便捷保存时无法新增场景相关节点，节点已删除，nodetype:{0},graphid:{1},nodeid{2}",
                                   node.GetType(), graph.GraphID, nodeID));
                            sb.AppendLine();
                            removeList.Add(nodeID);
                        }
                    }
                }
                for(var j=0;j<removeList.Count;j++)
                {
                    graph.DeleteNode(graph.GetNode(removeList[j]));
                }
                graph.SaveGraphToData(true);
                fullData.GraphDataList.Add(graph.graphData);
                editorConfigData.GraphEditorConfigList.Add(graph.graphConfigData);
            }
            EditorUtility.DisplayDialog("", sb.ToString(), "确定");

            DataIO.SerializeData<LevelEditorData>(CachedOpenFile, fullData);
            DataIO.SerializeData<EditorConfigData>(CachedOpenFile.Replace(".cfg", ".ecfg"), editorConfigData);
        }

        protected void InternalSave(string file, bool bSaveWallInfo)
        {
            fullData = new LevelEditorData();
            editorConfigData = new EditorConfigData();
            editorConfigData.SceneName = unityScene;

            if (DynamicSceneRoot != null)
                fullData.DynamicRoot = DynamicSceneRoot.name;

            fullData.Version = CURRENT_VERSION;
            fullData.LevelType = levelType;

            for (int i = 0; i < Graphs.Count; ++i)
            {
                LevelGraph lvGraph = Graphs[i] as LevelGraph;
                lvGraph.SaveGraphToData();
                fullData.GraphDataList.Add(lvGraph.graphData);
                editorConfigData.GraphEditorConfigList.Add(lvGraph.graphConfigData);
            }

            if(bSaveWallInfo)
            {
                fullData.LevelWallData = CachedWall;
            }
 
            DataIO.SerializeData<LevelEditorData>(file, fullData);
            DataIO.SerializeData<EditorConfigData>(file.Replace(".cfg", ".ecfg"), editorConfigData);
        }

        public void ExternalLoad (string file)
        {
            InternalLoad (file);
        }

        protected void InternalLoad(string file)
        {
            Reset();

            fullData = DataIO.DeserializeData<LevelEditorData>(file);
            editorConfigData = DataIO.DeserializeData<EditorConfigData>(file.Replace(".cfg", ".ecfg"));

            int version = fullData.Version;

            if(version < MINI_SUPPORT_VERSION)
            {
                ShowNotification(new GUIContent("版本号过低，无法打开"), 5);
            }

            

            FindDynamicRoot(true);
            LoadSceneGrid();
            levelType = fullData.LevelType;

            for (int i = 0; i < fullData.GraphDataList.Count; ++i)
            {
                LevelGraph lvGraph = NewGraph<LevelGraph>(fullData.GraphDataList[i].graphID, fullData.GraphDataList[i].name, false);
                lvGraph.graphData = fullData.GraphDataList[i];
                lvGraph.graphConfigData = editorConfigData.GetGraphConfigByID(fullData.GraphDataList[i].graphID);
                
            }

            for(int i = 0; i < fullData.GraphDataList.Count; ++i)
            {
                LevelGraph lvGraph = (LevelGraph)GetGraph(fullData.GraphDataList[i].graphID);
                lvGraph.LoadDataToGraph();
                if (i == 0)
                {
                    CurrentGraph = MainLevelGraph = lvGraph;
                    OpenGrahps.Add(CurrentGraph);
                }
            }

            LevelOperationStack.Instance.ClearOpStack();
            drawWall = false;
            drawWall_dummmy = false;
        }

        public void CheckAndAddDynamicObject()
        {
            FindDynamicRoot(true);
        }

        private void FindDynamicRoot(bool needAutoAddDynamicObject)
        {
            if (fullData == null) return;
            string strDynamicRoot = fullData.DynamicRoot;
            StaticSceneRoot = GameObject.Find("EditorScene/Collider");
            if (!string.IsNullOrEmpty(strDynamicRoot))
            { 
                GameObject go = GameObject.Find("EditorScene/DynamicObjects");

                if (go != null)
                {
                    Transform dynamicRoot = XCommon.singleton.FindChildRecursively(go.transform, strDynamicRoot);
                    if (dynamicRoot != null)
                    {
                        DynamicSceneRoot = dynamicRoot.gameObject;
                        var pType = PrefabUtility.GetPrefabAssetType(DynamicSceneRoot);
                        if (pType == PrefabAssetType.Model || pType == PrefabAssetType.Regular || pType == PrefabAssetType.Variant)
                        {
                            PrefabUtility.RevertPrefabInstance(DynamicSceneRoot, InteractionMode.AutomatedAction);
                        }                                                               
                    }
                    else if (needAutoAddDynamicObject)
                    {
                        string sceneName = editorConfigData.SceneName;
                        if (string.IsNullOrEmpty(sceneName) || sceneName != SceneManager.GetActiveScene().path)
                        {
                            return;
                        }
                        string prefabPath = sceneName.Substring(0, sceneName.LastIndexOf("/") + 1) + strDynamicRoot + ".prefab";
                        if (!string.IsNullOrEmpty(prefabPath))
                        {
                            GameObject objPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject));
                            if (objPrefab != null)
                            {
                                GameObject dynamicObject = PrefabUtility.InstantiatePrefab(objPrefab) as GameObject;
                                DynamicSceneRoot = dynamicObject.gameObject;
                                Transform parent = go.transform.Find("MainScene");
                                if (parent != null)
                                {
                                    dynamicObject.transform.parent = parent;
                                }
                                else
                                {
                                    Debug.LogError("Can't find MainScene node in this scene");
                                }
                            }
                            else
                            {
                                Debug.LogError("Can't find prefab from " + prefabPath);
                            }
                        }
                    }
                }
            }
        }

        public void BindCurrentEditorScene()
        {
            unityScene = SceneManager.GetActiveScene().path;

            //if(fullData == null)
            if (m_Drawer != null) m_Drawer.Clear();

            SceneContext sceneContext = new SceneContext();
            if (EngineContext.IsRunning)
            {
                SceneAssets.GetSceneContext(ref sceneContext,
                    EngineContext.sceneName,
                    EngineContext.instance.sceneAsset.path);
            }
            else
                SceneAssets.GetCurrentSceneContext(ref sceneContext);

            m_Data = new LevelMapData();
            m_Drawer = new LevelGridDrawer();
            m_Drawer.SetData(m_Data);

            if (m_Data.LoadFromServerFile(ref sceneContext))
            {
                m_Drawer.DrawMap();
            }
        }

        public void LoadGrid(string path)
        {
            if(objGridRoot==null)
            {
                Debug.LogError("请先选择船的挂点");
                return;
            }
            SceneContext context = default;
            context.terrainDir = "Assets/Scenes/Scenelib/create_grid/Terrain";
            var idx1 = path.LastIndexOf("/");
            var idx2 = path.LastIndexOf("_TerrainCollider.mapheight");
            context.name = path.Substring(idx1 + 1, idx2 - idx1 - 1);
            if (m_Data == null)
                m_Data = new LevelMapData();
            m_Data.m_Generator.offset = objGridRoot.transform.position;
            if (m_Data.LoadFromServerFile(ref context))
            {
                m_Drawer?.Clear();
                m_Drawer.SetData(m_Data);
                m_Drawer.DrawMap();
                for (int i = 0; i < Graphs.Count; ++i)
                {
                    LevelGraph lvGraph = Graphs[i] as LevelGraph;
                    lvGraph.ReloadMonster();
                }
            }
        }

        private void LoadSceneGrid()
        {
            if (editorConfigData == null) return;
            unityScene = editorConfigData.SceneName;

            if (SceneManager.GetActiveScene().path == unityScene || string.IsNullOrEmpty(unityScene))
            {
                SceneContext sceneContext = new SceneContext();
                if (EngineContext.IsRunning)
                {
                    SceneAssets.GetSceneContext(ref sceneContext,
                        EngineContext.sceneName,
                        EngineContext.instance.sceneAsset.path);
                }
                else
                    SceneAssets.GetCurrentSceneContext(ref sceneContext);

                m_Data = new LevelMapData();
                m_Drawer = new LevelGridDrawer();
                m_Drawer.SetData(m_Data);

                if (m_Data.LoadFromServerFile(ref sceneContext))
                {
                    m_Drawer.DrawMap();
                }
            }
        }

        private void SaveTpl()
        {
            string file = EditorUtility.SaveFilePanel("Select Dir", "", "MyTpl", "lvtpl");
            if (file.EndsWith("lvtpl"))
            {
                if (CurrentGraph != null)
                    (CurrentGraph as LevelGraph).SaveTpl(file);
            }
        }

        private void LoadTpl()
        {
            string file = EditorUtility.OpenFilePanel("Select skill file", "", "lvtpl");

            if (file.Length != 0)
            {
                if (CurrentGraph != null)
                    (CurrentGraph as LevelGraph).LoadTpl(file, Vector2.zero);
            }
        }
        #endregion

        static public void OnSceneFunc(SceneView sceneView)
        {
            if (Instance.CurrentGraph != null)
                (Instance.CurrentGraph as LevelGraph).OnSceneViewEvent(Event.current);
        }

        public override void ToolBarExtra()
        {
            if (CurrentGraph != null)
            {
                if (GUILayout.Button(MenuGUIContents[(int)EditorMenuItem.Item_Compile], ButtonLayout))
                {
                    Compile();
                }

                if (GUILayout.Button(MenuGUIContents[(int)EditorMenuItem.Item_WallInfo], ButtonLayout))
                {
                    if (DynamicSceneRoot != null)
                    {
                        SaveWallInfo();
                        ShowWallInfo();
                    }
                    else
                    {
                        ShowNotification(new GUIContent("No Dynamic Root"));
                    }
                }

                if (GUILayout.Button(MenuGUIContents[(int)EditorMenuItem.Item_Fold], ButtonLayout))
                {
                    SetGlobalExpand();
                }

                EditorGUILayout.BeginVertical();
                GUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("DynamicRoot", new GUILayoutOption[] { GUILayout.Width(80f), GUILayout.Height(20) });
                DynamicSceneRoot = (GameObject)EditorGUILayout.ObjectField(DynamicSceneRoot, typeof(GameObject), true, LevelGraph.ContentLayout);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("LevelType", new GUILayoutOption[] { GUILayout.Width(80f), GUILayout.Height(20) });
                levelType = (int)(LevelType)EditorGUILayout.EnumPopup((LevelType)levelType, LevelGraph.ContentLayout);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();

                //GUILayout.Space(50);

                //if (GUILayout.Button(MenuGUIContents[(int)EditorMenuItem.Item_Play], ButtonLayout))
                //{
                //    if (PrepareSimulation())
                //        StartSimulation();
                //}

                //if (GUILayout.Button(MenuGUIContents[(int)EditorMenuItem.Item_Pause], ButtonLayout))
                //{
                //    PauseSimulation();
                //}

                //if (GUILayout.Button(MenuGUIContents[(int)EditorMenuItem.Item_Stop], ButtonLayout))
                //{
                //    StopSimulation();

                //}

                //GUILayout.Space(30);
               
            }
        }

        private bool drawWall;
        private bool drawWall_dummmy;
        private void ToggleDrawWall()
        {
            for(var i=0;i<DynamicSceneRoot.transform.childCount;i++)
            {
                var t = DynamicSceneRoot.transform.GetChild(i);
                if(t.TryGetComponent<XDummyWall>(out var wall))
                {
                    wall.showRange = drawWall;
                }
            }
        }

        private string currentKeyWord=string.Empty;
        private string matchType = string.Empty;
        private int currentSearchIndex = 0;

        private void DrawSearchArea()
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label("搜索",GUILayout.Width(30f));          
            currentKeyWord=(EditorGUILayout.TextArea(currentKeyWord, LevelGraph.ContentLayout)).ToLower();
            if (GUILayout.Button(new GUIContent("next"), ButtonLayout))
                FocusSearch();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (!string.IsNullOrEmpty(matchType))
                GUILayout.Label(string.Format("匹配模式:{0}", matchType));
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void QuickLink()
        {
            (CurrentGraph as LevelGraph).QuickLink();
        }

        private void DefineCustomNode()
        {
            LevelCustomEditor.Init();
        }

        private string cacheLevelPath;
        private int cacheLevelIndex;

        private void LoadLevelFileOnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if(cacheLevelIndex>=30)//LevelEditorTableData.MapInfo.Table.Length
            {
                EditorSceneManager.sceneOpened -= LoadLevelFileOnSceneOpened;
                return;
            }
            try
            {
                InternalLoad(cacheLevelPath);
                if(Compile())
                {
                    InternalSave(cacheLevelPath, true);
                    Debug.Log(string.Format("level save successed:{0}", cacheLevelPath));
                }               
            }
            catch(Exception e)
            {
                Debug.LogError(e);
                Debug.LogError(string.Format("level save failed:{0}", cacheLevelPath));
            }
            var curMap= LevelEditorTableData.MapInfo.Table[cacheLevelIndex];
            cacheLevelIndex += 1;
            var nextMap = LevelEditorTableData.MapInfo.Table[cacheLevelIndex];
            while(string.IsNullOrEmpty(nextMap.LevelConfigFile)||EditorSceneManager.GetSceneByName(nextMap.UnitySceneFile)==null)
            {
                cacheLevelIndex +=1;
                nextMap = LevelEditorTableData.MapInfo.Table[cacheLevelIndex];
            }
            string path = string.Format("{0}{1}{2}.cfg", Application.dataPath, "/BundleRes/Table/", nextMap.LevelConfigFile);
            cacheLevelPath = path;
            try
            {
                if (curMap.UnitySceneFile != nextMap.UnitySceneFile)
                    EditorSceneManager.OpenScene("Assets/" + nextMap.ScenePath + "/" + nextMap.UnitySceneFile + ".unity");
                else
                    LoadLevelFileOnSceneOpened(default(Scene), OpenSceneMode.Additive);
            }
            catch(Exception e)
            {
                Debug.LogError(e);
                cacheLevelIndex +=1;
                var map = LevelEditorTableData.MapInfo.Table[cacheLevelIndex];
                cacheLevelPath= string.Format("{0}{1}{2}.cfg", Application.dataPath, "/BundleRes/Table/", map.LevelConfigFile);
                LoadLevelFileOnSceneOpened(default(Scene), OpenSceneMode.Additive);
            }
        }

        private void SaveAllLevel()
        {
            for(var i=0;i<LevelEditorTableData.MapInfo.Table.Length;i++)
            {
                var map = LevelEditorTableData.MapInfo.Table[i];
                if (EditorSceneManager.GetSceneByName(map.UnitySceneFile) == null)
                    continue;
                if (!string.IsNullOrEmpty(map.LevelConfigFile))
                {
                    string path = string.Format("{0}{1}{2}.cfg", Application.dataPath, "/BundleRes/Table/", map.LevelConfigFile);
                    cacheLevelPath = path;
                    cacheLevelIndex = i;
                    EditorSceneManager.sceneOpened += LoadLevelFileOnSceneOpened;
                    EditorSceneManager.OpenScene("Assets/"+map.ScenePath+"/"+map.UnitySceneFile+".unity");
                    break;
                }
            }
        }


        private void SaveAllLevelInsideCurScene()
        {
            List<string> levelList = new List<string>();
            if(string.IsNullOrEmpty(unityScene)||DynamicSceneRoot==null)
            {
                Debug.LogError("请先打开一个关卡脚本");
                return;
            }
            string sceneName = unityScene.Substring(unityScene.LastIndexOf("/")+1).Replace(".unity", string.Empty);
            for(var i=0;i<LevelEditorTableData.MapInfo.Table.Length;i++)
            {
                var map = LevelEditorTableData.MapInfo.Table[i];
                if (!string.IsNullOrEmpty(map.LevelConfigFile)
                    &&map.UnitySceneFile == sceneName 
                    && map.DynamicScene == DynamicSceneRoot.name
                    &&!levelList.Contains(map.LevelConfigFile))
                    levelList.Add(map.LevelConfigFile);
            }
            CFEngine.Editor.SceneEditTool.Direct_SaveDynamicScene();
            SaveWallInfo();
            StringBuilder sb = new StringBuilder();
            for(var i=0;i<levelList.Count;i++)
            {
                try
                {
                    string path = string.Format("{0}{1}{2}.cfg", Application.dataPath, "/BundleRes/Table/", levelList[i]);
                    InternalLoad(path);
                    fullData.LevelWallData = CachedWall;
                    InternalSave(path, true);
                    sb.Append(string.Format("{0} save successed", levelList[i]));
                    sb.AppendLine();
                }
                catch
                {
                    sb.Append(string.Format("{0} save failed", levelList[i]));
                    sb.AppendLine();
                    continue;
                }
            }
            EditorUtility.DisplayDialog("", sb.ToString(), "OK");
        }

        private void CheckMonsterBox()
        {
            StringBuilder sb = new StringBuilder();
            DirectoryInfo di = new DirectoryInfo(Application.dataPath + "/BundleRes/Table/Level/");
            var files = di.GetFiles();
            LevelEditorData data = null;
            EditorConfigData configData = null;
            LevelMapData mapData = new LevelMapData();
            SceneContext context = new SceneContext();
            bool valid = true;
            
            for(var i=0;i<files.Length;i++)
            {
                if (!files[i].FullName.EndsWith(".cfg"))
                    continue;
                try
                {
                    var fileName = files[i].FullName.Substring(files[i].FullName.LastIndexOf("\\")+1);
                    data = DataIO.DeserializeData<LevelEditorData>(string.Format("Assets/BundleRes/Table/Level/{0}",fileName));
                    configData = DataIO.DeserializeData<EditorConfigData>(string.Format("Assets/BundleRes/Table/Level/{0}", fileName).Replace(".cfg", ".ecfg"));
                    if(string.IsNullOrEmpty(configData.SceneName))
                    {
                        Debug.LogError(string.Format("configData.SceneName is Null,LevelName:{0}", fileName));
                        continue;
                    }
                    var sceneName = configData.SceneName.Substring(configData.SceneName.LastIndexOf('/')+1).Replace(".unity",string.Empty);
                    SceneAssets.GetSceneContext(ref context,sceneName,configData.SceneName);
                    if (!mapData.LoadFromServerFile(ref context))
                    {
                        Debug.LogError(string.Format("Can not Read Map,LevelName:{0}",
                                        fileName));
                        continue;
                    }
                    foreach (var graphData in data.GraphDataList)
                    {
                        foreach(var waveData in graphData.WaveData)
                        {
                            foreach(var spwanData in waveData.SpawnsInfo)
                            {
                                QuadTreeElement element = mapData.QueryGrid(spwanData.position);

                                if (element == null || !element.IsValid())
                                {
                                    sb.Append(string.Format("LevelName:{0} GraphName:{1} NodeID:{2} Pos:{3}",
                                        fileName,
                                        graphData.name,
                                        waveData.NodeID,
                                        spwanData.position
                                        ));
                                    sb.AppendLine();
                                    //Debug.LogError(string.Format("LevelName:{0} GraphName:{1} NodeID:{2} Pos:{3}",
                                    //    fileName,
                                    //    graphData.name,
                                    //    waveData.NodeID,
                                    //    spwanData.position
                                    //    ));
                                    valid = false;
                                }
                            }                            
                        }
                    }
                }
                catch(Exception)
                {
                    continue;
                }
            }
            if(valid)
            {
                ShowNotification(new GUIContent("所有怪物都在格子上了"));
            }
            else
            {
                string path = string.Format(@"..\..\LevelLog.txt");
                if (!string.IsNullOrEmpty(path))
                {
                    if (!File.Exists(path))
                        File.CreateText(path).Close();
                    StreamWriter sw = new StreamWriter(path, false);
                    sw.Write(sb);
                    sw.Flush();
                    sw.Close();
                }
            }
        }


        private void FocusSearch()
        {
            if(currentSearchIndex!=0)
                currentSearchIndex = currentSearchIndex >= CurrentGraph.graphConfigData.NodeConfigList.Count - 1 ? 0 : currentSearchIndex + 1;                  
            for (var i=currentSearchIndex;i<CurrentGraph.graphConfigData.NodeConfigList.Count;i++)
            {
                var nodeData = CurrentGraph.graphConfigData.NodeConfigList[i];
                if (string.IsNullOrEmpty(nodeData.Tag))
                    continue;
                var node = CurrentGraph.GetNode(nodeData.NodeID);
                if (node == null)
                    continue;
                var nodeType = node.GetType();
                var hostData = nodeType.GetField("HostData").GetValue(node);
                var dataFields = hostData.GetType().GetFields();
                bool matching = nodeData.Tag.ToLower().Contains(currentKeyWord);
                int nType = -1;
                if (node is LevelScriptNode)
                {
                    var desc = nodeType.GetField("NodeDesc").GetValue(node);
                    nType = (desc is LSNodeCustomNode) ? 2 : 1;
                }
                else
                    nType = 0;
                if (matching)
                    matchType = "节点Tag";
                if(!matching)
                {
                    foreach (var f in dataFields)
                    {
                        switch (nType)
                        {
                            case 0:
                                var value = f.GetValue(hostData);
                                if (value == null)
                                    continue;
                                if (value.ToString().ToLower().Contains(currentKeyWord.ToLower()))
                                {
                                    matchType = "节点变量值";
                                    matching = true;
                                    break;
                                }
                                break;
                            case 1:
                            case 2:
                                switch(f.Name)
                                {
                                    case "valueParam":
                                        var valueList = f.GetValue(hostData) as List<float>;
                                        if (valueList.Exists(v => v.ToString().ToLower().Contains(currentKeyWord)))
                                        {
                                            matchType = "节点变量值";
                                            matching = true;
                                            break;
                                        }
                                        break;
                                    case "vecParam":
                                        var vecList = f.GetValue(hostData) as List<Vector4>;
                                        if (vecList.Exists(v => v.ToString().ToLower().Contains(currentKeyWord)))
                                        {
                                            matchType = "节点变量值";
                                            matching = true;
                                            break;
                                        }
                                        break;
                                    case "stringParam":
                                        var stringList = f.GetValue(hostData) as List<string>;
                                        if (stringList.Exists(v => v.ToString().ToLower().Contains(currentKeyWord)))
                                        {
                                            matchType = "节点变量值";
                                            matching = true;
                                            break;
                                        }
                                        break;
                                }
                                break;
                        }
                    }
                }                
                if(matching&&CurrentGraph.selectNode!=node)
                {
                    currentSearchIndex = i;
                    if (CurrentGraph.selectNode != null)
                        CurrentGraph.selectNode.IsSelected = false;
                    CurrentGraph.selectNode = node;
                    CurrentGraph.selectNode.IsSelected = true;
                    CurrentGraph.FocusNode(CurrentGraph.selectNode);
                    break;
                }
                if(i==CurrentGraph.graphConfigData.NodeConfigList.Count-1)
                {
                    currentSearchIndex = 0;
                    ShowNotification(new GUIContent("未检测到相关节点"));
                    matchType = string.Empty;
                }
            }
        }
        
        public override int DrawToolBar()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(MenuGUIContents[(int)EditorMenuItem.Item_New], ButtonLayout))
            {
                ToolBarNewClicked();
            }

            if(GUILayout.Button(MenuGUIContents[(int)EditorMenuItem.Item_Open], ButtonLayout))
            {
                ToolBarOpenClicked();
            }

            if(GUILayout.Button(MenuGUIContents[(int)EditorMenuItem.Item_Save], ButtonLayout))
            {
                ToolBarSaveClicked();
            }

            if (GUILayout.Button(MenuGUIContents[(int)EditorMenuItem.Item_SaveTpl], ButtonLayout))
            {
                SaveTpl();
            }

            if (GUILayout.Button(MenuGUIContents[(int)EditorMenuItem.Item_LoadTpl], ButtonLayout))
            {
                LoadTpl();
            }

            GUILayout.BeginVertical();
            GUILayout.Label("Scale");
            if (CurrentGraph != null)
            {
                float oldScale = CurrentGraph.Scale;
                CurrentGraph.Scale = GUILayout.HorizontalSlider(CurrentGraph.Scale, 0.5f, 3f, new GUILayoutOption[] { GUILayout.Width(100), GUILayout.Height(20) });
                CurrentGraph.scrollPosition.x = (CurrentGraph.scrollPosition.x + position.width / 2) / oldScale * CurrentGraph.Scale - position.width / 2;
                CurrentGraph.scrollPosition.y = (CurrentGraph.scrollPosition.y + position.height / 2) / oldScale * CurrentGraph.Scale - position.height / 2;
            }
            GUILayout.EndVertical();            
            GUILayout.Label(new GUIContent(string.Format("当前脚本名字:{0}",string.IsNullOrEmpty(CachedOpenFile) ? 
                string.Empty : CachedOpenFile.Substring(CachedOpenFile.LastIndexOf('/') + 1))),new GUILayoutOption[] { GUILayout.Width(250f)});
            //GUILayout.FlexibleSpace();
            GUILayout.Space(50f);

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("SaveAllLevel", GUILayout.Width(100f)))
                SaveAllLevel();
            if (GUILayout.Button("SceneLevelSave", GUILayout.Width(120f)))
                SaveAllLevelInsideCurScene();
            if (GUILayout.Button("link", ButtonLayout))
                QuickLink();
            if (GUILayout.Button("check", ButtonLayout))
                CheckMonsterBox();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("ConvenientSave", GUILayout.Width(100f)))
                ConvenientSave();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();



            //if (GUILayout.Button("define", ButtonLayout))
            //    DefineCustomNode();
            //if (GUILayout.Button("NodeConfig", GUILayout.Width(90f)))
            //{
            //    if (EditorUtility.DisplayDialog("警告", "如果您不清楚这个是干什么的建议点击取消（容易出大事）", "生成配置", "取消"))
            //        LevelHelper.GenerateConfig();
            //}
            //if(GUILayout.Button("GenerateLevelCmd",GUILayout.Width(120f)))
            //{
            //    if (EditorUtility.DisplayDialog("警告", "此操作会生成客户端代码，非客户端人员或不清楚流程者请勿使用", "生成代码", "取消"))
            //    {
            //        LevelCmdGenerator.GenerateLevelCmdCode();
            //    }
            //}


            drawWall = GUILayout.Toggle(drawWall, new GUIContent("显示所有墙"),GUILayout.Width(120f));
            if(drawWall!=drawWall_dummmy)
            {
                ToggleDrawWall();
                drawWall_dummmy = drawWall;
            }
            DrawSearchArea();
            ToolBarExtra();
            GUILayout.EndHorizontal();

            float startX = 5;
            for(int i = 0; i < OpenGrahps.Count; ++i)
            {
                string tabTex = OpenGrahps[i].GraphID + ":" + OpenGrahps[i].GraphName;
                var width = DrawTool.CalculateTextSize(tabTex, BlueprintStyles.TabOn).x + 50;
                var labelRect = new Rect(startX, 50 ,width, 15f);

                var closeRect = new Rect(startX + width - 20, 52, 17f, 14f);
                if (i > 0)
                {
                    if (GUI.Button(closeRect, " "))
                    {
                        CloseOpenGraph(OpenGrahps[i]);
                        return 80;
                    }
                }

                bool IsSelectedTab = CurrentGraph == OpenGrahps[i];
                GUIStyle gs = IsSelectedTab ? BlueprintStyles.TabOn : BlueprintStyles.TabOff;
                IsSelectedTab = GUI.Toggle(labelRect, IsSelectedTab, tabTex, gs);
                if(IsSelectedTab)
                {
                    SetEditGraph(OpenGrahps[i]);
                }

                if (i > 0)  GUI.Label(closeRect, "x");
                startX += width;
            }

            GUILayout.Space(26);
            GUILayout.BeginHorizontal();
            GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(2) });
            GUILayout.EndHorizontal();
            return 80;
        }

        public override void SetEditGraph(BluePrintGraph graph)
        {
            base.SetEditGraph(graph);
            if (CurrentGraph != graph)
                currentSearchIndex = 0;
        }

        private bool PrepareSimulation()
        {
            if (state == LEVEL_EDITOR_STATE.simulation_mode) return false;

            GameObject go = GameObject.Find(@"Main Camera");

            if (go != null)
            {
                string autoSavePath = Application.dataPath + "/Editor Default Resources/Level/AutoSave.cfg";
                InternalSave(autoSavePath, false);

                ExstringManager.Clear();

                for (int i = 0; i < Graphs.Count; ++i)
                    (Graphs[i] as LevelGraph).PrepareSimulation();

                Simulator.Build(autoSavePath);

                for (int i = 0; i < Graphs.Count; ++i)
                {
                    Graphs[i].SetRuntimeEngine(Simulator);
                    Graphs[i].OnEnterSimulation();
                }
                return true;
            }

            return false;
        }

        private void StartSimulation()
        {
            state = LEVEL_EDITOR_STATE.simulation_mode;
            Simulator.StartSimulation();
        }

        private void StopSimulation()
        {
            if (state == LEVEL_EDITOR_STATE.simulation_mode)
            {
                Simulator.EndSimulation();

                for (int i = 0; i < Graphs.Count; ++i)
                {
                    Graphs[i].OnEndSimulation();
                }

                string autoSavePath = Application.dataPath + "/Editor Default Resources/Level/AutoSave.cfg";
                InternalLoad(autoSavePath);

                state = LEVEL_EDITOR_STATE.editor_mode;
            }    
        }

        private void PauseSimulation()
        {
            if (state == LEVEL_EDITOR_STATE.simulation_mode)
            {
                Simulator.PauseSimulation();
            }
        }

        List<LevelWallData> CachedWall = new List<LevelWallData>();
        //List<LevelWallData> CachedBossWall = new List<LevelWallData>();

        private void SaveWallInfo()
        {
            CachedWall.Clear();
            //CachedBossWall.Clear();

            //for (int i = 0; i < Graphs.Count; ++i)
            //{
            //    LevelGraph lvGraph = Graphs[i] as LevelGraph;
            //    lvGraph.GenerateWallInfo(CachedWall);
            //}
            if (DynamicSceneRoot != null)
            {
                Transform t = DynamicSceneRoot.transform;
                if (t != null)
                {
                    for (int i = 0; i < t.childCount; ++i)
                    {
                        Transform child = t.GetChild(i);
                        if (child != null)
                        {
                            LevelHelper.GetWallColliderData(child.gameObject, WallType.normal, CachedWall);
                        }
                    }
                }

                //t = XCommon.singleton.FindChildRecursively(DynamicSceneRoot.transform, "bosswallroot");
                //if (t != null)
                //{
                //    for (int i = 0; i < t.childCount; ++i)
                //    {
                //        Transform child = t.GetChild(i);
                //        if (child != null)
                //        {
                //            LevelWallData lwData = LevelHelper.GetWallColliderData(child.gameObject, WallType.boss);
                //            if (lwData != null)  CachedWall.Add(lwData);
                //        }
                //    }
                //}

                t = XCommon.singleton.FindChildRecursively(DynamicSceneRoot.transform, "playerwallroot");
                if (t != null)
                {
                    for (int i = 0; i < t.childCount; ++i)
                    {
                        Transform child = t.GetChild(i);
                        if (child != null)
                        {
                            LevelHelper.GetWallColliderData(child.gameObject, WallType.player, CachedWall);
                        }
                    }
                }

                ConvertStaticColliderToDynamic();

            }
        }

        private void ConvertStaticColliderToDynamic()
        {
            //string StaticColliderRoot = "EditorScene/Collider/MainScene";
            //GameObject root = DynamicSceneRoot;

            if(StaticSceneRoot != null)
            {
                IAmDynamicWall[] dWall = StaticSceneRoot.GetComponentsInChildren<IAmDynamicWall>();

                for(int i = 0; i < dWall.Length; ++i)
                {
                    GameObject go = dWall[i].gameObject;
                    LevelHelper.GetWallColliderData(go, WallType.normal, CachedWall);
                }
            }
        }

        private void ShowWallInfo()
        {
            var window = (LevelEditorConfigWindow)GetWindow(typeof(LevelEditorConfigWindow));
            window.titleContent = new GUIContent("LevelEditorConfigWindow");
            window.wantsMouseMove = true;
            window.SetMainEditor(this);
            window.SetDataSource(CachedWall);
            window.Show();
        }

        public bool Compile()
        {
            //if(CurrentGraph != null)
            // dynamic root 
            if (DynamicSceneRoot == null)
            {
                ShowNotification(new GUIContent("必须设置场景动态根节点"), 5);
                return false;
            }

            if(unityScene != SceneManager.GetActiveScene().path)
            {
                ShowNotification(new GUIContent("应用场景不匹配"), 5);
                return false;
            }

            SaveWallInfo();
            if(CheckWallInfo())
            {
                ShowNotification(new GUIContent("有重名的墙"), 5);
                return false;
            }

            string s = "";

            for (int g = 0; g < Graphs.Count; ++g)
            {
                BlueprintErrorInfoList errors = ErrorCheckManager.CheckError(Graphs[g]);

                if (errors._graph_error_list.Count > 0)
                {
                    foreach (BlueprintGraphErrorInfo ge in errors._graph_error_list)
                    {
                        for (int i = 0; i < ge.ErrorDataList.Count; ++i)
                        {
                            s += ("子图 " + ge.GraphName + ge.ErrorDataList[i].Desc + "\n");
                        }
                    }
                }

                if (errors._node_error_list.Count > 0)
                {
                    foreach (BlueprintNodeErrorInfo e in errors._node_error_list)
                    {
                        for (int i = 0; i < e.ErrorDataList.Count; ++i)
                        {
                            if (e.ErrorDataList[i].ErrorCode == BlueprintErrorCode.NodeDataError)
                                s += ("子图" + e.graphName + "节点" + e.nodeID + ":" + e.ErrorDataList[i].Desc + "\n");

                            if (e.ErrorDataList[i].ErrorCode == BlueprintErrorCode.DataTypeError)
                            {
                                int sNode = (e.ErrorDataList[i].ErrorConnection.connectStart.GetNode<BluePrintNode>().nodeEditorData.NodeID);
                                int eNode = (e.ErrorDataList[i].ErrorConnection.connectEnd.GetNode<BluePrintNode>().nodeEditorData.NodeID);

                                s += ("子图" + e.graphName + "节点" + sNode + "-节点" + eNode + "数据类型不匹配" + "\n");
                            }

                        }
                    }

                }
            }

            if (s.Length > 0)
            {
                s = s.Substring(0, s.Length - 1);
                ShowNotification(new GUIContent(s), 3);
                return false;
            }

            ShowNotification(new GUIContent("Compile Success"), 3);
            return true;
        }

        private bool CheckWallInfo()
        {
            for(int i = 0; i < CachedWall.Count; ++i)
                for(int j = 0; j < CachedWall.Count; ++j)
                {
                    if (i != j && CachedWall[i].name == CachedWall[j].name) return true;
                }
            return false;
        }

        private void AddComment()
        {
            if(CurrentGraph != null)
                CurrentGraph.AddComment();
        }

        private void SetGlobalExpand()
        {
            bExpand = !bExpand;

            if(CurrentGraph != null)
                CurrentGraph.SetGlobalExpand(bExpand);
        }

        public void RelocateMonster()
        {
            for (int i = 0; i < Graphs.Count; ++i)
            {
                LevelGraph lvGraph = Graphs[i] as LevelGraph;
                lvGraph.RelocateMonster(3.0f);
            }
        }



    }
}

