using System;
using System.Collections.Generic;
using B;
using CFEngine;
using CFEngine.Editor;
using CFUtilPoolLib;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using XLevel;

namespace XEditor.Level
{
    class LevelGridTool
    {
        public static readonly string Sce = "Assets/Scenes/";
        public static readonly string ResPath = "Assets/Tools/Common/Editor/Level/Res/";
        public string m_CurrentLevel = "";

        public LevelMapData m_Data;
        public LevelGridDrawer m_Drawer;
        //public bool bDevelop = false;
        public int GridSize = 20;

        // editor state var
        public ViewMode m_ViewMode = ViewMode.WalkableMode;

        private List<LevelDisplayBaseMode> OperationMode = new List<LevelDisplayBaseMode> ();
        public LevelDisplayBaseMode CurrentMode = null;

        Rect windowRect = new Rect (20, 20, 150, 50);
        string pos = "";

        // editor GUI var
        public GUIContent[] ViewModeIcons = null;
        public static GUIContent m_GUIWalk;
        public static GUIContent m_GUIBrush;
        public static GUIContent m_GUIPatrol;

        public bool IsSeaBlock = false;
        public float UniqueHeight;

        public GameObject GridRoot;

        private SceneContext sceneContext;

        GameObject sceneRoot;

        public void OnEnable ()
        {
            m_Data = new LevelMapData ();
            m_Drawer = new LevelGridDrawer ();
            m_Drawer.SetData (m_Data);

            OperationMode.Add (new LevelDisplayWalkableMode (this));
            OperationMode.Add (new LevelDisplayAreaMode(this));
            OperationMode.Add (new LevelDisplayPatrolMode (this));

            for (int i = 0; i < OperationMode.Count; ++i)
            {
                OperationMode[i].OnEnable ();
            }

            CurrentMode = OperationMode[0];
            m_Drawer.IsCurrentGridSelectCb = CurrentMode.IsCurrentGridSelect;
            m_Drawer.IsCurrentGridDebugCb = CurrentMode.IsCurrentGridDebug;
            m_Drawer.viewMode = ViewMode.WalkableMode;
            m_Drawer.CheckPointCb = ClimbEditor.CheckPoint;

            m_GUIWalk = new GUIContent (EditorGUIUtility.Load ("Tools/btn_walk_0.png") as Texture);
            m_GUIBrush = new GUIContent (EditorGUIUtility.Load ("Tools/btn_brush_0.png") as Texture);
            m_GUIPatrol = new GUIContent (EditorGUIUtility.Load ("Tools/btn_patrol_0.png") as Texture);
            if (ViewModeIcons == null)
            {
                ViewModeIcons = new GUIContent[]
                {
                m_GUIWalk,
                m_GUIBrush,
                m_GUIPatrol
                };
            }
            if (EngineContext.IsRunning)
            {
                SceneAssets.GetSceneContext (ref sceneContext,
                    EngineContext.sceneName,
                    EngineContext.instance.sceneAsset.path);
                sceneContext.terrainDir = "Assets/BundleRes/Table/SceneBlock/Terrain";
            }
            else
                SceneAssets.GetCurrentSceneContext (ref sceneContext);
            
        }

        public void OnDisable()
        {
            for (int i = 0; i < OperationMode.Count; ++i)
            {
                OperationMode[i].OnDisable();
            }

            OperationMode.Clear();

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

        }

        public void OnDestroy ()
        {
            for (int i = 0; i < OperationMode.Count; ++i)
            {
                OperationMode[i].OnDisable ();
            }

            OperationMode.Clear ();

            m_CurrentLevel = "";
            if (m_Data != null)
            {
                m_Data.Clear ();
                m_Data = null;
            }

            if (m_Drawer != null)
            {
                m_Drawer.Clear ();
                m_Drawer = null;
            }
        }

        public bool OnMouseClick (SceneView sceneView)
        {
            if (CurrentMode != null)
                return CurrentMode.OnMouseClick (sceneView);

            return false;
        }

        public bool OnDoubleClick (SceneView sceneView)
        {
            if (CurrentMode != null)
                return CurrentMode.OnMouseDoubleClick (sceneView);

            return false;
        }

        public void OnMouseMove (SceneView sceneView)
        {
            if (CurrentMode != null)
                CurrentMode.OnMouseMove (sceneView);
        }

        public bool OnMouseDrag (SceneView sceneView)
        {
            if (CurrentMode != null)
                return CurrentMode.OnMouseDrag (sceneView);

            return false;
        }

        public bool OnKeyDown (SceneView sceneView)
        {
            if (CurrentMode != null)
                return CurrentMode.OnKeyDown (sceneView);

            return false;
        }

        void DrawTestWindow (int windowID)
        {
            GUI.contentColor = Color.yellow;

            GUILayout.BeginHorizontal ();
            pos = GUILayout.TextArea (pos, GUILayout.Width (80));
            GUILayout.EndHorizontal ();

            if (GUILayout.Button ("Test RayCast", GUILayout.Width (60)))
            {
                string[] s = pos.Split (new char[] { ',' });
                Vector3 inputPos = new Vector3 (float.Parse (s[0]), 0, float.Parse (s[1]));
                m_Data.TestRaycast (inputPos);
            }

            GUI.DragWindow ();
        }

        public void OnSceneGUI ()
        {
            int windowID = 22333;
            windowRect = GUILayout.Window (windowID, windowRect, DrawTestWindow, "射线测试", GUILayout.Width (100));

            if (CurrentMode != null)
                CurrentMode.OnSceneGUI ();
        }
        public void OnGUI ()
        {
            Scene scene = EditorSceneManager.GetActiveScene ();

            if (scene.name.Length == 0)
            {
                EditorGUILayout.LabelField ("OPEN UNITY SCENE FIRST");
            }
            else
            {
                m_CurrentLevel = scene.name;
                EditorGUILayout.Space ();
                EditorGUILayout.LabelField ("CURRENT SCENE:     " + m_CurrentLevel);
            }

            TestGUI ();

            if (m_CurrentLevel.Length > 0)
            {
                if (!m_Data.IsDataReady)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    //bDevelop = GUILayout.Toggle (bDevelop, "Develop", new GUILayoutOption[] { GUILayout.Width (70) });

                    GUIContent seaBlockContent = new GUIContent("IsSeaBlock","是否海域格子");
                    IsSeaBlock = GUILayout.Toggle(IsSeaBlock, seaBlockContent, GUILayout.Width(80f));
                    if(IsSeaBlock)
                    {
                        GUIContent seaBlockHeightContent = new GUIContent("SeaBlockHeight", "海域唯一高度");
                        GUILayout.Label(seaBlockHeightContent, GUILayout.Width(75f));
                        UniqueHeight = EditorGUILayout.FloatField(UniqueHeight, GUILayout.Width(100f));
                    }

                    GUIContent GridSizeContent = new GUIContent(GridSize.ToString(), "gridsize");
                    if (GUILayout.Button(GridSizeContent, new GUILayoutOption[] { GUILayout.Width(150) }))
                    {
                        if (GridSize == 10) GridSize = 20;
                        else if (GridSize == 20) GridSize = 50;
                        else if (GridSize == 50) GridSize = 200;
                        else if (GridSize == 200) GridSize = 10;
                    }

                    GUIContent ButtonContent = new GUIContent ("生成网格数据", "generate map grid");
                    if (GUILayout.Button (ButtonContent, new GUILayoutOption[] { GUILayout.Width (150) }))
                    {
                        var context = EngineContext.instance;                       
                        if(context != null)
                        {
                            int widthCount = 0;
                            int heightCount = 0;
                            widthCount = context.xChunkCount;
                            heightCount = context.zChunkCount;
                            GeneratorPreProcessor();
                            m_Data.GenerateData(GridSize, false, widthCount, heightCount, IsSeaBlock, UniqueHeight, Vector3.zero, string.Empty,false);
                        }
                    }

                    GUIContent LoadContent = new GUIContent ("加载服务器网格", "Load Server");
                    if (GUILayout.Button (LoadContent, new GUILayoutOption[] { GUILayout.Width (150) }))
                    {
                        //string path = EditorUtility.OpenFilePanel("Select a file to load", XEditorPath.Lev, "mapheight");
                        if (m_Data.LoadFromServerFile (ref sceneContext))
                        {
                            AudioSurfaceRecognition.Instance.InitAllAudioSurfaces();
                            ClimbEditor.InitCheck();
                            m_Drawer.DrawMap ();
                            ClimbEditor.CheckResult();
                        }
                    }
                    
                    GUIContent SurfaceContent = new GUIContent ("地表类型数据", "Load Server");
                    if (GUILayout.Button(SurfaceContent, new GUILayoutOption[] {GUILayout.Width(150)}))
                    {
                        //string path = EditorUtility.OpenFilePanel("Select a file to load", XEditorPath.Lev, "mapheight");
                        if (m_Data.LoadFromServerFile(ref sceneContext))
                        {
                            m_Data.IsAudioSurface = true;
                            ClimbEditor.InitCheck();
                            AudioSurfaceRecognition.Instance.InitAllAudioSurfaces();
                        }
                        else
                        {
                            Debug.LogError("需要先生成网格数据！");
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    //GUIContent LoadContent2 = new GUIContent ("加载客户端网格", "Load Client");
                    //if (GUILayout.Button (LoadContent2, new GUILayoutOption[] { GUILayout.Width (150) }))
                    //{
                    //    //string path = EditorUtility.OpenFilePanel("Select a file to load", XEd0itorPath.Lev, "mapheight");
                    //    if (m_Data.LoadFromClientFile (ref sceneContext))
                    //    {
                    //        ClimbEditor.InitCheck();
                    //        m_Drawer.DrawMap ();
                    //        ClimbEditor.CheckResult();
                    //    }
                    //}
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    GUIContent objContent = new GUIContent("生成物体格子");
                    if(GUILayout.Button(objContent,GUILayout.Width(100f)))
                    {
                        if (GridRoot == null)
                        {
                            Debug.LogError("请先选择挂点");
                            return;
                        }
                        var context = EngineContext.instance;
                        if(context!=null)
                        {
                            int widthCount = 0;
                            int heightCount = 0;
                            widthCount = context.xChunkCount;
                            heightCount = context.zChunkCount;
                            m_Data.GenerateData(GridSize, false, widthCount, heightCount, false, 0, GridRoot.transform.position, GridRoot.transform.parent.name,true);
                        }                        
                    }
                    EditorGUILayout.LabelField("挂点", GUILayout.Width(50f));
                    GridRoot = (GameObject)EditorGUILayout.ObjectField(GridRoot, typeof(GameObject), true, GUILayout.Width(150f));

                    
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    
                    if (m_Data.IsAudioSurface)
                    {
                        AudioSurfaceInfoGUI();
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        BlockBaseInfoGUI();
                        EditorGUILayout.EndHorizontal();
                    }
                  
                }                

                if (m_Data.IsGenerateFinish() && !m_Data.IsAudioSurface)
                {
                    DisplayModeGUI();

                    if (CurrentMode != null)
                        CurrentMode.OnGUI();

                    GeneratorPostProcessor();
                }
            }

        }

        private void TestGUI ()
        {
            //bInBrushMode = GUI.Toggle(new Rect(50, 50, 36, 36), bInBrushMode, bInBrushMode ? m_GUIBrushActive : m_GUIBrush, GUIStyle.none);
        }
        private void BlockBaseInfoGUI ()
        {
            EditorGUILayout.LabelField ("Block Info:   " + LevelMapData.BlockRowCount + "x" + LevelMapData.BlockColCount);
            EditorGUILayout.Space ();
            {
                // block info GUI
                EditorGUILayout.BeginHorizontal ();
                GUIContent SaveContent = new GUIContent ("Save", "Save To File");

                if (GUILayout.Button (SaveContent))
                {
                    //string path = EditorUtility.SaveFilePanel("Select a file to save", XEditorPath.Lev, "temp.bytes", "bytes");
                    if (m_Data.SaveToFile (ref sceneContext))
                    {
                        AssetDatabase.Refresh ();
                        EditorUtility.DisplayDialog ("Save Level Data Dialog", "Save Success", "OK");
                    }
                }
                EditorGUILayout.EndHorizontal ();

                EditorGUILayout.BeginHorizontal ();
                GUIContent SaveBlockContent = new GUIContent ("SaveBlock(程序用)", "Save Block To File");
                if (GUILayout.Button (SaveBlockContent))
                {
                    m_Data.TestSaveBlock (ref sceneContext);
                    AssetDatabase.Refresh ();

                }
                EditorGUILayout.EndHorizontal ();
                
                EditorGUILayout.BeginHorizontal ();
                GUIContent LoadBlockContent = new GUIContent ("LoadBlock(程序用)", "Load Block From File");
                if (GUILayout.Button (LoadBlockContent))
                {
                    m_Data.TestLoadBlock (ref sceneContext);

                }


                GUIContent IsDebugAudioSurfaceContent = new GUIContent("调试地表类型", "调试地表类型");
                m_Data.IsDebugAudioSurface = GUILayout.Toggle(m_Data.IsDebugAudioSurface, IsDebugAudioSurfaceContent);
                EditorGUILayout.EndHorizontal ();
            }
        }
        
        private void AudioSurfaceInfoGUI()
        {
            
            EditorGUILayout.BeginHorizontal ();
            AudioSurfaceRecognition.UnitMeter = EditorGUILayout.FloatField("距离密度", AudioSurfaceRecognition.UnitMeter, GUILayout.MaxWidth(200));

            GUIContent SaveContent = new GUIContent ("生成", "生成所有地面类型数据");
            if (GUILayout.Button(SaveContent))
            {
                ShowTypeData(false);
            }

            GUIContent PreviewContent = new GUIContent ("预览", "预览所有地面类型数据");
            if (GUILayout.Button (PreviewContent))
            {
                ShowTypeData(true);
            }
            EditorGUILayout.EndHorizontal ();
            
            EditorGUILayout.BeginVertical();
            AudioSurfaceRecognition.useOriBlend = EditorGUILayout.Slider("原地表Blend:", AudioSurfaceRecognition.useOriBlend,0,1);
            EditorGUILayout.EndHorizontal ();
            
            EditorGUILayout.BeginVertical();
           // EditorGUILayout.BeginHorizontal ();
            float min = 0;
            float max = 2;
            AudioSurfaceRecognition.layer1 = EditorGUILayout.Slider("layer1", AudioSurfaceRecognition.layer1,min,max);
            AudioSurfaceRecognition.layer2 = EditorGUILayout.Slider("layer2", AudioSurfaceRecognition.layer2,min,max);
            AudioSurfaceRecognition.layer3 = EditorGUILayout.Slider("layer3", AudioSurfaceRecognition.layer3,min,max);
            AudioSurfaceRecognition.layer4 = EditorGUILayout.Slider("layer4", AudioSurfaceRecognition.layer4,min,max);
            //EditorGUILayout.EndHorizontal ();
            EditorGUILayout.EndVertical();
        }

        private void ShowTypeData(bool preview)
        {
            ClearGridRoot();
            AudioSurfaceRecognition.Instance.ShowAllLevelMapData(m_Data,preview);
            m_Drawer.DrawMap();
            ClimbEditor.CheckResult();
        }

        private void ClearGridRoot()
        {
            GameObject root = GameObject.Find("GridRoot");
            if (root)
            {
                GameObject.DestroyImmediate(root);
            }
        }


        private void DisplayModeGUI ()
        {
            EditorGUILayout.Space ();
            GUILayout.Box ("", new GUILayoutOption[] { GUILayout.ExpandWidth (true), GUILayout.Height (1) });
            EditorGUILayout.LabelField ("Display Mode: ");

            EditorGUILayout.BeginHorizontal ();
            GUILayout.Space (20);
            EditorGUI.BeginChangeCheck ();
            int toolbarIndex = (int) m_ViewMode - 1;
            toolbarIndex = GUILayout.Toolbar (toolbarIndex, ViewModeIcons, new GUIStyle (EditorStyles.miniButton), new GUILayoutOption[] { GUILayout.Width (120), GUILayout.Height (40) });
            if (EditorGUI.EndChangeCheck ())
            {
                if (m_ViewMode != (ViewMode) (toolbarIndex + 1))
                {
                    m_ViewMode = (ViewMode) (toolbarIndex + 1);

                    if (CurrentMode != null)
                        CurrentMode.OnLeaveMode ();

                    CurrentMode = OperationMode[toolbarIndex];
                    m_Drawer.IsCurrentGridSelectCb = CurrentMode.IsCurrentGridSelect;
                    m_Drawer.IsCurrentGridDebugCb = CurrentMode.IsCurrentGridDebug;
                    m_Drawer.viewMode = m_ViewMode;

                    CurrentMode.OnEnterMode ();
                }
            }
            EditorGUILayout.EndHorizontal ();
        }

        public void OnUpdate ()
        {
            m_Data.OnUpdate ();

            if (m_Data.IsGenerateJustFinish ())
            {
                m_Drawer.SmartDrawGrid ();
            }

            if (CurrentMode != null)
                CurrentMode.OnUpdate ();
        }

        public static void ClearUnusedRoot (string name)
        {
            while (true)
            {
                GameObject go = GameObject.Find (name);

                if (go == null) break;

                GameObject.DestroyImmediate (go);
            }
        }

        private void GeneratorPreProcessor()
        {
            GameObject colliders = GameObject.Find("EditorScene/Collider");
            if(colliders != null)
            {
                GameObject duplicateCollider = GameObject.Instantiate(colliders);
                duplicateCollider.name = "TempCollider";
                colliders.SetActive(false);
            }

            GameObject meshTerrain = GameObject.Find("EditorScene/MeshTerrain");
            if(meshTerrain != null)
            {
                GameObject duplicateMeshTerrain = GameObject.Instantiate(meshTerrain);
                duplicateMeshTerrain.name = "TempMeshTerrain";
                meshTerrain.SetActive(false);
            }

            GameObject unityTerrain = GameObject.Find("EditorScene/UnityTerrain");
            if (unityTerrain != null)
            {
                GameObject duplicateUnityTerrain = GameObject.Instantiate(unityTerrain);
                duplicateUnityTerrain.name = "TempUnityTerrain";
                unityTerrain.SetActive(false);
            }

            sceneRoot = GameObject.Find("EditorScene");
            sceneRoot.SetActive(false);
        }

        private void GeneratorPostProcessor()
        {
            if (sceneRoot == null) return;
            sceneRoot.SetActive(true);

            GameObject colliders = sceneRoot.transform.Find("Collider").gameObject;
            if (colliders != null)
            {
                colliders.SetActive(true);
                GameObject duplicateCollider = GameObject.Find("TempCollider"); 
                if(duplicateCollider != null) GameObject.DestroyImmediate(duplicateCollider);
            }

            GameObject meshTerrain = sceneRoot.transform.Find("MeshTerrain").gameObject;
            if (meshTerrain != null)
            {
                meshTerrain.SetActive(true);
                GameObject duplicateMeshTerrain = GameObject.Find("TempMeshTerrain");
                
                if (duplicateMeshTerrain != null)  GameObject.DestroyImmediate(duplicateMeshTerrain);
            }

            GameObject unityTerrain = sceneRoot.transform.Find("UnityTerrain").gameObject;
            if (unityTerrain != null)
            {
                unityTerrain.SetActive(true);
                GameObject duplicateUnityTerrain = GameObject.Find("TempUnityTerrain");
                
                if(duplicateUnityTerrain != null) GameObject.DestroyImmediate(duplicateUnityTerrain);
            }
        }
    }
}