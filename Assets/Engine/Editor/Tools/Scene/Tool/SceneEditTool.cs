using System;
using Impostors.Editor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Impostors;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityObject = UnityEngine.Object;
using CFEngine.WorldStreamer;
using CFEngine.WorldStreamer.Editor;
using MeshLOD;
using CFUtilPoolLib;

namespace CFEngine.Editor
{
    public partial class SceneEditTool : CommonToolTemplate
    {
        enum OpType
        {
            OpNone,

            //init save
            OpCreateEditorScene,
            OpSaveScene,
            OpSaveScenetoBundleRes,
            OpSaveDynamicScene,
            OpSaveAll,

            //terrain
            OpBindTerrainData,
            OpBakeBaseMap,
            OpValidTerrain,
            OpInvalidTerrain,
            OpRefreshTerrainSplat,
            OpExportMirrorMesh,
            OpCalcCollision,

            // OpCluster,
            OpLoadTerrainCollider,

            // OpStaticBatch,
            OpCalcQuadTree,

            OpReplaceShader,
            OpAnalyzeMat,
            OpAnalyzeRes,

            OpSplitObject,
        }

        enum SceneMergeType
        {
            None,
            Object,
            HeightMap,
            AlphaMap,
            Num
        }

        delegate void EditorChunkInfoCB(int chunkID, int x, int z);

        delegate void SaveObject(Transform trans, SaveChunkContext scp, EditorCommon.EnumTransform funPrefabs);

        delegate bool BakeCb(LightmapBakeContext lbc, MeshRenderObject oc);

        delegate bool TerrainBakeCb(LightmapBakeContext lbc, TerrainObject to);

        class ObjectPosInfo
        {
            public Vector3 pos = new Vector3(-10000, -10000, -10000);
            public Vector3 rot = new Vector3(-10000, -10000, -10000);
            public Vector3 scale = new Vector3(-10000, -10000, -10000);
            public List<Transform> objLsit = new List<Transform>();
        }

        class ChunkVertexInfo
        {
            public List<Vector3> pos = new List<Vector3>();
            public List<Vector3> normal = new List<Vector3>();
            public List<Vector4> tangent = new List<Vector4>();
            public List<Vector2> uv = new List<Vector2>();
            public List<Vector2> uv2 = new List<Vector2>();
            public List<int> index = new List<int>();
            public Dictionary<int, int> indexMap = new Dictionary<int, int>();

            public int x;
            public int z;
            public bool hasCenterVertex = false;

            public Rect centerRect;

            public void AddVertex(Vector3 p, Vector3 n, Vector4 t, Vector2 uv0, Vector2 uv2, int srcIndex)
            {
                int newIndex;
                if (!indexMap.TryGetValue(srcIndex, out newIndex))
                {
                    newIndex = pos.Count;
                    pos.Add(p);
                    normal.Add(n);
                    tangent.Add(t);
                    this.uv.Add(uv0);
                    this.uv2.Add(uv2);
                    indexMap.Add(srcIndex, newIndex);
                    if (centerRect.Contains(new Vector2(p.x, p.z)))
                    {
                        hasCenterVertex = true;
                    }
                }

                index.Add(newIndex);
            }

            public void AddVertex(Vector3 p, Vector3 n, Vector4 t, Vector2 uv0, Vector2 uv2)
            {
                int newIndex = pos.Count;
                pos.Add(p);
                normal.Add(n);
                tangent.Add(t);
                this.uv.Add(uv0);
                this.uv2.Add(uv2);
                index.Add(newIndex);
            }

            public float DistWeight(Vector3 center)
            {
                float weight = 0;
                if (pos.Count > 1000)
                {
                    weight += 10;
                }

                float dist = Vector2.Distance(centerRect.center, new Vector2(center.x, center.z));
                weight += 1 / dist;
                return weight;
            }
        }

        class CrossEdgeVertexInfo
        {
            public int index0;
            public int index1;
            public int index2;
            public int chunkIndex0;
            public int chunkIndex1;
            public int chunkIndex2;
        }

        class CommonContext
        {
            public List<MeshRenderObject> tmpObjList = new List<MeshRenderObject>();
            public List<TerrainObject> selectChunks = new List<TerrainObject>();
            public Dictionary<int, TerrainObject> selectChunksMap = new Dictionary<int, TerrainObject>();
        }

        class SaveChunkContext
        {
            public CommonContext commonContext;
            public SaveObject[] saveFun = new SaveObject[(int) EditorSceneObjectType.Num];
            public EditorChunkData ecd;
            public EditorChunkData baseEcd;
            public Stack<int> folderStack = new Stack<int>();
            public int groupIndex = -1;
            public string folderName = "";
            public string tagName = "";
            public string dynamicSceneName = "";
            public Transform root = null;
            public EditorSceneObjectType objType = EditorSceneObjectType.StaticPrefab;
            public Dictionary<GameObject, int> prefabMap = new Dictionary<GameObject, int>();
            public Dictionary<string, int> objIDMap = new Dictionary<string, int>();

            public Dictionary<UnityEngine.Object, List<ObjectPosInfo>> sameObjMap =
                new Dictionary<UnityEngine.Object, List<ObjectPosInfo>>();

            // public Dictionary<string, ChunkLightmapData.LightmapInfo> lightmapInfoMap;

            public List<CrossEdgeVertexInfo> crossEdgeVertex = new List<CrossEdgeVertexInfo>();
            public Dictionary<int, ChunkVertexInfo> splitObjectInfo = new Dictionary<int, ChunkVertexInfo>();

            public Dictionary<int, int> vertexChunkMap = new Dictionary<int, int>();

            public Transform splitTrans = null;
            // public Shader srcShader;
            // public Shader desShader;

            public List<ReflectionProbeBlendInfo> blendInfo = new List<ReflectionProbeBlendInfo>();
        }

        class ClusterObjInfo
        {
            public Vector3 pos = Vector3.zero;
            public float size = 0;
            public int id = 0;

            // public EditorChunkData.GameObjectInfo goi;

            // public MaterialPropertyBlock mpb;
        }

        class ClusterArea
        {
            public List<ClusterObjInfo> coiList = new List<ClusterObjInfo>();
        }

        class InstanceBox : IQuadTreeObject
        {
            public int blockId;
            public AABB aabb;

            public int BlockId
            {
                get { return blockId; }
            }

            public int QuadNodeId
            {
                get { return 0; }
                set { }
            }

            public AABB bounds
            {
                get { return aabb; }
            }
        }

        // struct ResData
        // {
        //     public UnityEngine.Object obj;
        //     public long size;
        //     public string GetSizeStr ()
        //     {
        //         return EditorUtility.FormatBytes (size);
        //     }
        // }

        //common
        private OpType opType = OpType.OpNone;

        private ThreadManager threadManager;
        //scene config

        // private string editTag = "";
        private int chunkWidth;
        private int chunkHeight;
        private int widthCount;
        private int heightCount;
        private SceneContext sceneContext;
        private SceneEditContext editorContext = new SceneEditContext();

        public static int cutnumx = 2;
        public static int cutnumz = 2;
        private static float minwidth = 100f;


        #region serialize data

        private SceneConfigData sceneLocalConfig;

        private SceneData sceneData;
        private DynamicSceneData dynamicSceneData;
        private EditorChunkData editorChunkData;
        private SceneConfig sceneConfig;

        #endregion

        #region scene config create

        private List<string> editTagList = new List<string>();
        // private string createTag = "";

        #endregion

        #region scene merge

        private static string[] mergeText = new string[(int) SceneMergeType.Num]
        {
            "All",
            "Object",
            "HeightMap",
            "AlphaMap",
        };

        // private SceneMergeType mergeType = SceneMergeType.None;

        #endregion

        private CommonContext commonContext;

        private SaveChunkContext saveChunkContext;

        //private bool replaceLocal = false;
        private bool deleteSame = false;

        #region QuadTree

        private List<QuadTreeContext> quadTreeCache = new List<QuadTreeContext>();
        private int quadTreeCount = 0;
        private bool drawLodLevelBox = false;

        #endregion

        #region res

        #endregion

        #region collision

        private CollisionContext collisionContext = new CollisionContext();
        private bool drawCollisionVoxel = false;

        #endregion

        //tools

        // private List<Mesh> noMergeMeshs = new List<Mesh> ();

        //ui
        private ToolsUtility.GridContext gridContext;
        private Vector2Int clickSrcChunk = new Vector2Int(-1, -1);
        private Vector2 mousePos;
        private Rect dragRect;

        private SceneSerializeContext ssContext = new SceneSerializeContext();

        private static SceneEditTool GetDirect_SceneEditTool()
        {
            SceneEditTool _SceneEditTool = ScriptableObject.CreateInstance<SceneEditTool>();
            _SceneEditTool.OnInit();

            return _SceneEditTool;
        }


        #region Override

        public override void OnInit()
        {
            base.OnInit();
            gridContext = new ToolsUtility.GridContext();

            commonContext = new CommonContext();
            saveChunkContext = new SaveChunkContext();
            saveChunkContext.commonContext = commonContext;

            SceneAssets.GetCurrentSceneContext(ref sceneContext);
            string sceneConfigPath = string.Format("{0}/{1}{2}.asset",
                sceneContext.configDir, sceneContext.name, SceneContext.SceneConfigSuffix);
            var sceneConfig = AssetDatabase.LoadAssetAtPath<SceneConfig>(sceneConfigPath);

            if (sceneConfig != null)
            {
                ssContext.sceneConfig = sceneConfig;
                InitSceneData(sceneConfig);
                SceneResConfig.instance.Init(ref sceneContext, ssContext);
                SceneResConfig.instance.InitGUI(ref sceneContext, ssContext);
            }
        }

        public override void OnUninit()
        {
            base.OnUninit();
            SceneResConfig.instance.UninitGUI(ref sceneContext, ssContext);
            SceneResConfig.instance.Uninit(ref sceneContext, ssContext);
            UninitThread();
            //UnInitTerrain ();
            UnInitPreview();
            UnInitLightmap();
            collisionContext.Release();
        }

        Vector2 scrollPos = Vector2.zero;

        public override void DrawGUI(ref Rect rect)
        {
            sceneConfig = ssContext.sceneConfig;
            if (sceneConfig == null)
            {
                EngineContext context = EngineContext.instance;
                GUILayout.BeginHorizontal();
                GUILayout.Label("WidthCount");
                context.xChunkCount = EditorGUILayout.IntSlider(context.xChunkCount, 1, 32);
                EditorGUILayout.IntField(context.xChunkCount * EngineContext.ChunkSize, GUILayout.MaxWidth(80));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("HeightCount");
                context.zChunkCount = EditorGUILayout.IntSlider(context.zChunkCount, 1, 32);
                EditorGUILayout.IntField(context.zChunkCount * EngineContext.ChunkSize, GUILayout.MaxWidth(80));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Create Chunk Config"))
                {
                    opType = OpType.OpCreateEditorScene;
                }

                GUILayout.EndHorizontal();
            }
            else
            {
                editorContext.sc = ssContext.sceneConfig;
                editorContext.localConfig = sceneLocalConfig;
                editorContext.ssc = ssContext;

                if (sceneContext.valid && sceneLocalConfig != null)
                {
                    scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false, GUILayout.Width(rect.width),
                        GUILayout.Height(rect.height - 50));
                    OnCommonOpGUI("1.Common", ref rect);
                    GUILayout.Space(10);
                    // OnLodOpGUI("2.Lod", ref rect);
                    // GUILayout.Space(10);
                    OnSystemGUI("2.Systems", ref rect);
                    GUILayout.Space(10);
                    OnLightmapBakeGUI("3.Lightmap Bake");
                    EditorGUILayout.EndScrollView();
                }
            }
        }

        public override void DrawSceneGUI()
        {
            var configs = SceneResConfig.instance.configs;
            for (int i = 0; i < configs.Count; ++i)
            {
                var config = configs[i];
                if (config.process != null)
                {
                    config.process.OnSceneGUI(ref sceneContext, editorContext);
                }
            }
        }

        public override void DrawGizmos()
        {
            if (sceneLocalConfig != null)
            {
                Color backup = Gizmos.color;
                if (sceneLocalConfig.drawObjBox ||
                    sceneLocalConfig.drawChunkBox ||
                    sceneLocalConfig.drawRootBox ||
                    sceneLocalConfig.drawQuadTreeBox)
                {
                    for (int i = 0; i < quadTreeCache.Count && i < quadTreeCount; ++i)
                    {
                        var treeContext = quadTreeCache[i];
                        if (treeContext.treeNodes != null)
                        {
                            for (int j = 0; j < treeContext.treeNodes.Length; ++j)
                            {
                                EditorQuardTreeNode node = treeContext.treeNodes[j];
                                if (node != null && (node.data.Count > 0 || j == 0))
                                {
                                    if (sceneLocalConfig.drawObjBox)
                                    {
                                        for (int k = 0; k < node.data.Count; ++k)
                                        {
                                            var data = node.data[k];
                                            if (data != null)
                                            {
                                                Gizmos.color = new Color32(255, 255, 0, 255);
                                                Gizmos.DrawWireCube(data.bounds.center, data.bounds.size);
                                            }
                                        }
                                    }

                                    if (j == 0)
                                    {
                                        if (sceneLocalConfig.drawChunkBox)
                                        {
                                            Gizmos.color = new Color32(255, 0, 255, 128);
                                            Gizmos.DrawWireCube(treeContext.aabb.center, treeContext.aabb.size);
                                            Vector3 pos = treeContext.aabb.center;

                                            if (treeContext.chunkId >= 0 && saveChunkContext.baseEcd != null)
                                            {
                                                pos.y += 50;
                                                var chunk = saveChunkContext.baseEcd.chunks[treeContext.chunkId];

                                                Handles.Label(pos,
                                                    string.Format("VB:{0} IB:{1}", chunk.meshInfo.x, chunk.meshInfo.y));
                                            }
                                        }

                                        if (sceneLocalConfig.drawRootBox)
                                        {
                                            Gizmos.color = new Color32(255, 0, 0, 128);
                                            Gizmos.DrawWireCube(node.aabb.center, node.aabb.size);
                                        }
                                    }

                                    if (sceneLocalConfig.drawQuadTreeBox && j != 0)
                                    {
                                        Gizmos.color = new Color32(255, 0, 0, 128);
                                        Gizmos.DrawWireCube(node.aabb.center, node.aabb.size);
                                    }
                                }
                            }
                        }
                    }
                }

                if (drawCollisionVoxel &&
                    collisionContext.gizmoDraw)
                {
                    Gizmos.color = new Color32(0, 255, 0, 128);
                    float unitSize = collisionContext.unitSize;
                    Vector3 size = new Vector3(unitSize, unitSize, unitSize);
                    for (int i = 0; i < collisionContext.clouds.Count; ++i)
                    {
                        Gizmos.DrawWireCube(collisionContext.clouds[i], size);
                    }

                    Gizmos.color = new Color32(255, 255, 255, 128);
                    float s = 0.8f / collisionContext.stepSize;
                    size = new Vector3(s, s, s);
                    for (int i = 0; i < collisionContext.meshCloud.Count; ++i)
                    {
                        var mvi = collisionContext.meshCloud[i];
                        for (int j = 0; j < mvi.cloud.Count; ++j)
                        {
                            Gizmos.DrawWireCube(mvi.cloud[j], size);
                        }
                    }
                }

                var configs = SceneResConfig.instance.configs;
                for (int i = 0; i < configs.Count; ++i)
                {
                    var config = configs[i];
                    if (config.process != null)
                    {
                        config.process.OnDrawGizmos(ref sceneContext, editorContext);
                    }
                }

                Gizmos.color = backup;
            }
        }

        public override void Update()
        {
            editorContext.sc = ssContext.sceneConfig;
            editorContext.localConfig = sceneLocalConfig;
            editorContext.ssc = ssContext;
            switch (opType)
            {
                case OpType.OpCreateEditorScene:
                    SceneSerialize.PrepareSceneFolder(ref sceneContext);
                    SceneResConfig.instance.Init(ref sceneContext, ssContext);
                    if (TerrainSystem.system != null)
                    {
                        TerrainSystem.system.CreateEditScene(ref sceneContext, ssContext);
                        InitSceneData(ssContext.sceneConfig);
                        SceneResConfig.instance.InitGUI(ref sceneContext, ssContext);
                    }

                    break;
                case OpType.OpSaveScene:
                    SaveScene();
                    break;
                case OpType.OpSaveScenetoBundleRes:
                    SaveScenetoBundleRes();
                    break;
                case OpType.OpSaveDynamicScene:
                    SaveDynamicScene();
                    break;
                case OpType.OpSaveAll:
                    SaveAll();
                    break;
                case OpType.OpCalcQuadTree:
                    CalcQuadTree();
                    break;
            }

            opType = OpType.OpNone;
            var configs = SceneResConfig.instance.configs;

            for (int i = 0; i < configs.Count; ++i)
            {
                var config = configs[i];
                if (config.process != null)
                {
                    config.process.Update(ref sceneContext, editorContext);
                }
            }

            // UpdateLights ();
            UpdateLightmap();
            //UpdateCombine ();
            UpdateJobs();
        }

        #endregion

        #region init/unint

        private void InitThread()
        {
            if (threadManager == null)
            {
                threadManager = new ThreadManager();
            }
        }

        private void UninitThread()
        {
            if (threadManager != null)
            {
                threadManager.ClearJob();
            }
        }

        private void ClearJobs()
        {
        }

        #endregion

        #region Draw

        //private void OnSceneInfoGUI (string info)
        //{
        //    SceneSerialize.GetSceneChunkCount (sceneConfig, out chunkWidth, out chunkHeight, out widthCount, out heightCount);
        //    sceneLocalConfig.sceneInfoFolder = EditorGUILayout.Foldout (sceneLocalConfig.sceneInfoFolder, info);
        //    if (!sceneLocalConfig.sceneInfoFolder)
        //        return;
        //    EditorCommon.BeginGroup ("Scene");

        //    GUILayout.BeginHorizontal ();
        //    sceneConfig.widthCount = EditorGUILayout.IntSlider ("WidthCount", sceneConfig.widthCount, 1, 32);
        //    EditorGUILayout.IntField (sceneConfig.widthCount * EngineContext.ChunkSize, GUILayout.MaxWidth (80));
        //    GUILayout.EndHorizontal ();

        //    GUILayout.BeginHorizontal ();
        //    sceneConfig.heightCount = EditorGUILayout.IntSlider ("HeightCount", sceneConfig.heightCount, 1, 32);
        //    EditorGUILayout.IntField (sceneConfig.heightCount * EngineContext.ChunkSize, GUILayout.MaxWidth (80));
        //    GUILayout.EndHorizontal ();

        //    GUILayout.BeginHorizontal ();
        //    terrainGo = EditorGUILayout.ObjectField (terrainGo, typeof (GameObject), true, GUILayout.MaxWidth (260)) as GameObject;
        //    GUILayout.EndHorizontal ();

        //    GUILayout.BeginHorizontal ();
        //    if (GUILayout.Button ("Init Scene", GUILayout.MaxWidth (160)))
        //    {
        //        opType = OpType.OpInitScene;
        //    }
        //    if (GUILayout.Button ("Save Config", GUILayout.MaxWidth (160)))
        //    {
        //        opType = OpType.OpSaveConfig;
        //    }
        //    GUILayout.EndHorizontal ();
        //    EditorCommon.EndGroup ();
        //}

        private static bool IsSplitInput = false;

        private GameObject lastSelectObj;

        private void OnCommonOpGUI(string info, ref Rect rect)
        {
            sceneLocalConfig.commonOpFolder = EditorGUILayout.Foldout(sceneLocalConfig.commonOpFolder, info);
            if (!sceneLocalConfig.commonOpFolder)
                return;
            // List<string> sceneEditorTag = sceneConfig.sceneEditorTag;
            // int deleteIndex = -1;

            // EditorCommon.BeginGroup ("Common Object", true, rect.width - 10);
            // for (int i = 0; i < sceneEditorTag.Count; ++i)
            // {
            //     GUILayout.BeginHorizontal ();
            //     GUILayout.Label (sceneEditorTag[i], GUILayout.MaxWidth (100));
            //     if (GUILayout.Button ("Open", GUILayout.MaxWidth (60)))
            //     {
            //         if (!Application.isPlaying)
            //         {
            //             UnityEngine.SceneManagement.Scene s = UnityEngine.SceneManagement.SceneManager.GetActiveScene ();
            //             EditorSceneManager.MarkSceneDirty (s);
            //             EditorSceneManager.SaveScene (s);
            //             string scenePath = string.Format ("{0}/{1}_{2}.unity", sceneContext.dir, sceneContext.name, sceneEditorTag[i]);
            //             EditorSceneManager.OpenScene (scenePath, OpenSceneMode.Single);
            //         }
            //     }
            //     if (GUILayout.Button ("Delete", GUILayout.MaxWidth (60)))
            //     {
            //         if (EditorUtility.DisplayDialog ("Delete", "Is Delete? ", "OK", "Cancel"))
            //         {
            //             deleteIndex = i;
            //         }
            //     }

            //     string editTag = sceneEditorTag[i];
            //     if (editTag != sceneContext.suffix)
            //     {
            //         if (GUILayout.Button (mergeText[(int) SceneMergeType.Object], GUILayout.MaxWidth (100)))
            //         {
            //             MergeScene (editTag, SceneMergeType.Object);
            //         }
            //     }
            //     GUILayout.EndHorizontal ();
            // }
            // if (deleteIndex >= 0)
            // {
            //     sceneEditorTag.RemoveAt (deleteIndex);
            // }
            // EditorGUILayout.Space ();
            // GUILayout.BeginHorizontal ();
            // if (GUILayout.Button ("Push Obj", GUILayout.MaxWidth (160)))
            // {
            //     opType = OpType.OpMergeObjPush;
            // }
            // staticBatch = GUILayout.Toggle (staticBatch, "StaticBatch", GUILayout.MaxWidth (160));
            // clearStaticBatch = GUILayout.Toggle (clearStaticBatch, "ClearStaticBatch", GUILayout.MaxWidth (160));
            // GUILayout.EndHorizontal ();
            // GUILayout.BeginHorizontal ();
            // if (GUILayout.Button ("Pull Obj", GUILayout.MaxWidth (160)))
            // {
            //     if (EditorUtility.DisplayDialog ("Pull", "Is Pull? ", "OK", "Cancel"))
            //     {
            //         opType = OpType.OpMergeObjPull;
            //     }
            // }
            // replaceLocal = GUILayout.Toggle (replaceLocal, "Override Local", GUILayout.MaxWidth (160));
            // GUILayout.EndHorizontal ();
            // GUILayout.BeginHorizontal ();
            // if (GUILayout.Button ("Detect SameObject", GUILayout.MaxWidth (160)))
            // {
            //     DetectSameObject ();
            // }
            // deleteSame = GUILayout.Toggle (deleteSame, "Delete Same", GUILayout.MaxWidth (160));
            // GUILayout.EndHorizontal ();
            // GUILayout.BeginHorizontal ();
            // if (GUILayout.Button ("Export Mesh", GUILayout.MaxWidth (160)))
            // {
            //     opType = OpType.OpExportMesh;
            // }
            // GUILayout.EndHorizontal ();
            // GUILayout.BeginHorizontal ();
            // if (GUILayout.Button ("Export MirrorMesh", GUILayout.MaxWidth (160)))
            // {
            //     opType = OpType.OpExportMirrorMesh;
            // }

            // GUILayout.EndHorizontal ();
            // // GUILayout.BeginHorizontal ();
            // // if (GUILayout.Button ("Calc Collision", GUILayout.MaxWidth (160)))
            // // {
            // //     opType = OpType.OpCalcCollision;
            // // }
            // // collisionContext.gizmoDraw = GUILayout.Toggle (collisionContext.gizmoDraw, "Only Calc Select", GUILayout.MaxWidth (160));
            // // GUILayout.EndHorizontal ();
            // GUILayout.BeginHorizontal ();
            // if (GUILayout.Button ("Test", GUILayout.MaxWidth (160)))
            // {
            //     EditorCommon.EnumTransform funPrefabs = null;
            //     funPrefabs = (trans, param) =>
            //     {
            //         if (EditorCommon.IsPrefabOrFbx (trans.gameObject))
            //         {
            //             var renders = EditorCommon.GetRenderers (trans.gameObject);
            //             for (int j = 0; j < renders.Count; ++j)
            //             {
            //                 var render = renders[j];
            //                 string name = render.name.ToLower ();
            //                 int index = name.LastIndexOf (".fbx");
            //                 if (index >= 0)
            //                 {
            //                     name = name.Substring (0, index);
            //                     render.name = name;
            //                 }
            //             }
            //         }
            //         else
            //         {
            //             EditorCommon.EnumChildObject (trans, param, funPrefabs);
            //         }
            //     };
            //     for (int i = (int) EditorSceneObjectType.Light; i <= (int) EditorSceneObjectType.Instance; ++i)
            //     {
            //         string path = AssetsConfig.EditorGoPath[0] + "/" + AssetsConfig.EditorGoPath[i] + "/" + sceneContext.suffix;
            //         saveChunkContext.objType = (EditorSceneObjectType) i;
            //         saveChunkContext.objIDMap.Clear ();
            //         EditorCommon.EnumTargetObject (path, (trans, param) =>
            //         {
            //             funPrefabs (trans, null);
            //         });
            //     }
            // }

            // GUILayout.EndHorizontal ();
            // EditorCommon.EndGroup (false);
            EditorCommon.BeginGroup("Scene", true, rect.width - 10);
            GUILayout.BeginHorizontal();

            //string saveSceneStr = "Save Scene";
            //if (EngineContext.UseUrp)
            //    saveSceneStr = "Save Urp";

            //if (GUILayout.Button(saveSceneStr, GUILayout.MaxWidth(160)))
            // {
            //     NullComponentAndMissingPrefabSearchTool.Clear();
            //     opType = OpType.OpSaveScene;
            // }

            if (GUILayout.Button("Save All", GUILayout.MaxWidth(160)))
            {
                NullComponentAndMissingPrefabSearchTool.Clear();
                opType = OpType.OpSaveAll;
            }

            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            //if (GUILayout.Button("SaveScenetoBundleRes", GUILayout.MaxWidth(200)))
            //{
            //    NullComponentAndMissingPrefabSearchTool.Clear();
            //    opType = OpType.OpSaveScenetoBundleRes;
            //}

            IsSplitInput = GUILayout.Toggle(IsSplitInput, "切割场景", GUILayout.MaxWidth(160));

            //ImpostorHelper.IsImpostors = GUILayout.Toggle(ImpostorHelper.IsImpostors, "Impostors", GUILayout.MaxWidth(160));
            GUILayout.EndHorizontal();


            // if (ImpostorHelper.IsImpostors)
            // {
            //     GUILayout.BeginHorizontal();
            //
            //     ImpostorHelper.screenRelativeTransitionHeight = EditorGUILayout.FloatField("屏幕相对高度比0-1", ImpostorHelper.screenRelativeTransitionHeight, GUILayout.MaxWidth(200));
            //     ImpostorHelper.cutout = EditorGUILayout.FloatField("图片边缘裁剪比0-1", ImpostorHelper.cutout, GUILayout.MaxWidth(200));
            //     ImpostorHelper.cutoutTransparentFill = EditorGUILayout.FloatField("透明物体填充Alpha值0-1", ImpostorHelper.cutoutTransparentFill, GUILayout.MaxWidth(200));
            //     GUILayout.EndHorizontal();
            // }

            if (IsSplitInput)
            {
                EditorCommon.BeginGroup("大物体分割设置:太大的物体会被切割成小块（横向分段数-纵向分段数-最小被切割物体的宽度）", true, rect.width - 10);

                //   GUILayout.BeginHorizontal();
                SplitInput.cutnumx =
                    EditorGUILayout.IntSlider("  横向分割", SplitInput.cutnumx, 2, 10, GUILayout.MaxWidth(400));
                SplitInput.cutnumz =
                    EditorGUILayout.IntSlider("  纵向分割", SplitInput.cutnumz, 2, 10, GUILayout.MaxWidth(400));
                minwidth = EditorGUILayout.FloatField("   最小分割大小，单位（米）", minwidth, GUILayout.MaxWidth(200));
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("清理所有之前生成的Splitmesh(临时按钮)", GUILayout.MaxWidth(300)))
                {
                    DeleteALLSplitMesh();
                }

                GUILayout.Label("删除路径：“Assets/Scenes”");

                GUILayout.EndHorizontal();
                //  GUILayout.EndHorizontal();
                EditorCommon.EndGroup();
            }


            EditorCommon.EndGroup();

            EditorCommon.BeginGroup("DynamicScene", true, rect.width - 10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save Dynamic Scene", GUILayout.MaxWidth(160)))
            {
                opType = OpType.OpSaveDynamicScene;
            }

            GUILayout.EndHorizontal();
            EditorCommon.EndGroup();

            GameObject selectImpostor = Selection.activeGameObject;

            if (selectImpostor != null)
            {
                ImpostorsCfg cfg1 = selectImpostor.GetComponent<ImpostorsCfg>();

                if (cfg1 != null && lastSelectObj != selectImpostor)
                {
                    lastSelectObj = selectImpostor;
                    ImpostorHelper.CopyFromCfg(cfg1);
                }

                EditorCommon.BeginGroup("Impostors", true, rect.width - 10);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("子节点批量添加", GUILayout.MaxWidth(160)))
                {
                    ImpostorHelper.ImpostorRemoveLod();
                    ImpostorHelper.AdllImpostorToChild();

                    ImpostorsCfg cfgTarget = selectImpostor.GetComponent<ImpostorsCfg>();

                    if (cfgTarget == null)
                    {
                        cfgTarget = selectImpostor.AddComponent<ImpostorsCfg>();
                    }

                    ImpostorHelper.CopyToCfg(cfgTarget);
                }

                ImpostorHelper.screenRelativeTransitionHeight = EditorGUILayout.FloatField("lod高度比0-1",
                    ImpostorHelper.screenRelativeTransitionHeight, GUILayout.MaxWidth(200));

                ImpostorHelper.IsForceRes =
                    GUILayout.Toggle(ImpostorHelper.IsForceRes, "强制分辨率", GUILayout.MaxWidth(160));

                if (ImpostorHelper.IsForceRes)
                {
                    ImpostorHelper.IsDiscardOriLOD =
                        GUILayout.Toggle(ImpostorHelper.IsDiscardOriLOD, "忽略原有LOD", GUILayout.MaxWidth(160));

                    EditorGUILayout.LabelField("最小",GUILayout.Width(30));
                    ImpostorHelper.forceMinTextureResolution =
                        (TextureResolution) EditorGUILayout.EnumPopup(ImpostorHelper.forceMinTextureResolution,GUILayout.Width(100));

                    EditorGUILayout.LabelField("最大", GUILayout.Width(30));
                    ImpostorHelper.forceMaxTextureResolution =
                        (TextureResolution) EditorGUILayout.EnumPopup(ImpostorHelper.forceMaxTextureResolution,GUILayout.Width(100));
                }

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                ImpostorHelper.deltaCameraAngle = EditorGUILayout.FloatField("变化角度",
                    ImpostorHelper.deltaCameraAngle, GUILayout.MaxWidth(200));

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("子节点批量删除", GUILayout.MaxWidth(160)))
                {
                    ImpostorHelper.ImpostorRemoveLod();
                    ImpostorsCfg cfgTarget = selectImpostor.GetComponent<ImpostorsCfg>();

                    if (cfgTarget != null)
                    {
                        GameObject.DestroyImmediate(cfgTarget);
                    }
                }

                if (GUILayout.Button("预览", GUILayout.MaxWidth(160)))
                {
                    ImpostorHelper.PreivewImpostor();
                }
                GUILayout.EndHorizontal();
                
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                ImpostorHelper.zOffset = EditorGUILayout.Slider(ImpostorHelper.zOffset,0f,1f,GUILayout.MaxWidth(160));
                if (GUILayout.Button("子节点批量设置 ZOffset",GUILayout.MaxWidth(160)))
                {
                    Debug.Log("设置 ZOffset " + ImpostorHelper.zOffset);
                    ImpostorHelper.ImpostorSetZOffset();
                }
                GUILayout.EndHorizontal();
                
                
                EditorCommon.EndGroup();
            }
        }


        private void OnLodOpGUI(string info, ref Rect rect)
        {
            sceneLocalConfig.lodOpFolder = EditorGUILayout.Foldout(sceneLocalConfig.lodOpFolder, info);
            if (!sceneLocalConfig.lodOpFolder)
                return;
            EditorCommon.BeginGroup("Lod", true, rect.width - 10);
            GUILayout.BeginHorizontal();
            sceneConfig.useDistCull =
                EditorGUILayout.Toggle("DistCull", sceneConfig.useDistCull, GUILayout.MaxWidth(300));
            GUILayout.EndHorizontal();
            ToolsUtility.LodSizeGUI("Near", ref sceneConfig.lodNearSize);
            ToolsUtility.LodSizeGUI("Far", ref sceneConfig.lodFarSize);
            sceneConfig.lodHeight = EditorGUILayout.Slider("GlobalObjectHeight", sceneConfig.lodHeight, 0, 10,
                GUILayout.MaxWidth(300));
            EditorCommon.EndGroup();
        }

        private void OnSystemGUI(string info, ref Rect rect)
        {
            sceneLocalConfig.systemFolder = EditorGUILayout.Foldout(sceneLocalConfig.systemFolder, info);
            if (!sceneLocalConfig.systemFolder)
                return;
            var configs = SceneResConfig.instance.configs;
            for (int i = 0; i < configs.Count; ++i)
            {
                var config = configs[i];
                if (config.process != null && config.process.HasGUI)
                {
                    config.process.OnGUI(ref sceneContext, editorContext, ref rect);
                }
            }
        }

        private void OnPreviewGUI(string info)
        {
            sceneLocalConfig.previewFolder = EditorGUILayout.Foldout(sceneLocalConfig.previewFolder, info);
            if (!sceneLocalConfig.previewFolder)
                return;

            EditorCommon.BeginGroup("Preview");

            //GUILayout.BeginHorizontal ();
            //sceneLocalConfig.drawObjBox = GUILayout.Toggle (sceneLocalConfig.drawObjBox, "Draw Obj Box", GUILayout.MaxWidth (200));
            //sceneLocalConfig.drawQuadTreeBox = GUILayout.Toggle (sceneLocalConfig.drawQuadTreeBox, "Draw Quad Box", GUILayout.MaxWidth (200));
            //sceneLocalConfig.drawRootBox = GUILayout.Toggle (sceneLocalConfig.drawRootBox, "Draw Root Box", GUILayout.MaxWidth (200));
            //sceneLocalConfig.drawChunkBox = GUILayout.Toggle (sceneLocalConfig.drawChunkBox, "Draw Chunk Box", GUILayout.MaxWidth (200));
            //GUILayout.EndHorizontal ();

            //GUILayout.BeginHorizontal ();
            ////bool draw = sceneConfigData.drawMergeBox;
            //// sceneConfigData.drawMergeBox = GUILayout.Toggle(sceneConfigData.drawMergeBox, "Draw Merge Box", GUILayout.MaxWidth(200));
            //// if (draw != sceneConfigData.drawMergeBox)
            //// {
            ////     PreveiwCollect();
            //// }
            //bool draw = drawCollisionVoxel;
            //drawCollisionVoxel = GUILayout.Toggle (drawCollisionVoxel, "Draw CollisionVoxel", GUILayout.MaxWidth (200));
            //if (draw != drawCollisionVoxel)
            //{
            //    PreviewCollisionVoxel ();
            //}
            //draw = drawLodLevelBox;
            //drawLodLevelBox = GUILayout.Toggle (drawLodLevelBox, "Draw LodLevel Box", GUILayout.MaxWidth (200));
            //if (draw != drawLodLevelBox)
            //{
            //    PreviewChunkLevel ();
            //}
            //GUILayout.EndHorizontal ();

            //GUILayout.BeginHorizontal ();
            //Color color = EditorGUILayout.ColorField ("Terrain Grid Color", sceneLocalConfig.terrainGridColor, GUILayout.MaxWidth (200));
            //if (color != sceneLocalConfig.terrainGridColor)
            //{
            //    sceneLocalConfig.terrainGridColor = color;
            //    terrainEditMat.SetColor ("_EdgeColor", color);
            //}
            //GUILayout.EndHorizontal ();
            EditorCommon.EndGroup();
        }

        private void OnTerrainGridGUI()
        {
            if (sceneConfig.chunks != null &&
                sceneConfig.chunks.Count > 0 && lightmapBakeContext != null)
            {
                GUILayout.BeginHorizontal();
                sceneLocalConfig.processSelect = GUILayout.Toggle(sceneLocalConfig.processSelect, "Only Process Select",
                    GUILayout.MaxWidth(160));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                ToolsUtility.PrepareGrid(gridContext, widthCount, heightCount, 40, 40);
                var e = Event.current;
                if (!lightmapBakeContext.isBaking)
                {
                    if (e.type == EventType.MouseDown)
                    {
                        bool doubleClick = e.clickCount == 2;
                        bool leftMouse = e.button == 0;

                        if (gridContext.innerRect.Contains(e.mousePosition))
                        {
                            mousePos = e.mousePosition;
                            dragRect = new Rect(0, 0, 0, 0);
                            Vector2 pos = e.mousePosition - gridContext.innerRect.position;
                            int xIndex = Mathf.FloorToInt(pos.x / gridContext.gridOffsetH);
                            int zIndex = (gridContext.vLines - 1) - Mathf.FloorToInt(pos.y / gridContext.gridOffsetV);
                            if (leftMouse)
                            {
                                clickSrcChunk.x = xIndex;
                                clickSrcChunk.y = zIndex;
                            }
                            else
                            {
                                clickSrcChunk.x = -1;
                                clickSrcChunk.y = -1;
                            }

                            if (doubleClick)
                            {
                                FocusChunk(xIndex, zIndex);
                            }
                        }

                        SceneTool.DoRepaint();
                    }
                    else if (e.type == EventType.MouseDrag)
                    {
                        if (gridContext.innerRect.Contains(e.mousePosition) && clickSrcChunk.x >= 0 &&
                            clickSrcChunk.y >= 0)
                        {
                            Vector2 pos = e.mousePosition;
                            float xMin = pos.x > mousePos.x ? mousePos.x : pos.x;
                            float yMin = pos.y > mousePos.y ? mousePos.y : pos.y;
                            float xMax = pos.x < mousePos.x ? mousePos.x : pos.x;
                            float yMax = pos.y < mousePos.y ? mousePos.y : pos.y;
                            dragRect = new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
                            SceneTool.DoRepaint();
                        }
                    }
                    else if (e.type == EventType.MouseUp)
                    {
                        bool leftMouse = e.button == 0;
                        // bool rightMouse = e.button == 1;
                        if (leftMouse && gridContext.innerRect.Contains(e.mousePosition) && clickSrcChunk.x >= 0 &&
                            clickSrcChunk.y >= 0)
                        {
                            float dist = Vector2.Distance(e.mousePosition, mousePos);
                            if (dist > 0.1f)
                            {
                                Vector2 pos = e.mousePosition - gridContext.innerRect.position;
                                int xIndex = Mathf.FloorToInt(pos.x / gridContext.gridOffsetH);
                                int zIndex = (gridContext.vLines - 1) -
                                             Mathf.FloorToInt(pos.y / gridContext.gridOffsetV);

                                int srcX = xIndex < clickSrcChunk.x ? xIndex : clickSrcChunk.x;
                                int srcZ = zIndex < clickSrcChunk.y ? zIndex : clickSrcChunk.y;
                                int endX = xIndex > clickSrcChunk.x ? xIndex : clickSrcChunk.x;
                                int endZ = zIndex > clickSrcChunk.y ? zIndex : clickSrcChunk.y;

                                Vector2Int chunkSrcPoint = new Vector2Int(srcX, srcZ);
                                Vector2Int chunkEndPoint = new Vector2Int(endX, endZ);
                                if (chunkSrcPoint != sceneLocalConfig.chunkSrcPoint ||
                                    chunkEndPoint != sceneLocalConfig.chunkEndPoint)
                                {
                                    sceneLocalConfig.chunkSrcPoint = chunkSrcPoint;
                                    sceneLocalConfig.chunkEndPoint = chunkEndPoint;
                                }
                                else
                                {
                                    sceneLocalConfig.chunkSrcPoint = new Vector2Int(-1, -1);
                                    sceneLocalConfig.chunkEndPoint = new Vector2Int(-1, -1);
                                }

                                FillSelectChunks();
                                opType = OpType.OpCalcQuadTree;
                            }
                        }

                        clickSrcChunk.x = -1;
                        clickSrcChunk.y = -1;
                        SceneTool.DoRepaint();
                    }
                }

                if (e.type == EventType.Repaint)
                {
                    ToolsUtility.DrawGrid(gridContext);
                    EnumSceneChunk(sceneLocalConfig.chunkSrcPoint,
                        sceneLocalConfig.chunkEndPoint,
                        (chunkID, x, z) =>
                        {
                            int xIndex = chunkID % widthCount;
                            int zIndex = (gridContext.vLines - 1) - chunkID / widthCount;
                            if (x >= 0 && x == bakeIndex.x &&
                                z >= 0 && z == bakeIndex.y)
                            {
                                ToolsUtility.DrawBlock(gridContext, xIndex, zIndex, new Color(1.0f, 0.0f, 0.0f, 0.5f));
                            }
                            else
                            {
                                ToolsUtility.DrawBlock(gridContext, xIndex, zIndex,
                                    new Color(0.15f, 0.5f, 0.15f, 0.5f));
                            }
                        });
                    //if (lightmapBakeIndex.x >= 0 &&
                    //    lightmapBakeIndex.y >= 0)
                    //{
                    //    int xIndex = lightmapBakeIndex.x;
                    //    int zIndex = (gridContext.vLines - 1) - lightmapBakeIndex.y;
                    //    ToolsUtility.DrawBlock(gridContext, xIndex, zIndex, new Color(0.15f, 0.5f, 0.15f, 0.1f));
                    //}

                    //drag rect
                    if (clickSrcChunk.x >= 0 && clickSrcChunk.y >= 0)
                        Handles.DrawSolidRectangleWithOutline(dragRect, Color.white, new Color(1f, 1f, 1f, 1f));
                }

                GUILayout.EndHorizontal();
            }
        }

        #endregion

        #region Common

        private void EnumSceneChunk(Vector2Int chunkSrcPoint, Vector2Int chunkEndPoint, EditorChunkInfoCB cb)
        {
            if (sceneLocalConfig != null)
            {
                for (int z = chunkSrcPoint.y; z <= chunkEndPoint.y; ++z)
                {
                    for (int x = chunkSrcPoint.x; x <= chunkEndPoint.x; ++x)
                    {
                        if (x >= 0 && x < widthCount &&
                            z >= 0 && z < heightCount)
                        {
                            int id = z * widthCount + x;
                            cb(id, x, z);
                        }
                    }
                }
            }
        }

        //private void PrepareFolder (string tagName)
        //{
        //    for (int j = (int) EditorSceneObjectType.Light; j <= (int) EditorSceneObjectType.Instance; ++j)
        //    {
        //        Transform trans = commonContext.editorSceneGos[j].transform;
        //        Transform tagTrans = trans.Find (tagName);
        //        if (tagTrans == null)
        //        {
        //            GameObject go = new GameObject (tagName);
        //            go.transform.parent = trans;
        //        }
        //        tagName = SceneContext.MainTagName;
        //        tagTrans = trans.Find (tagName);
        //        if (tagTrans == null)
        //        {
        //            GameObject go = new GameObject (tagName);
        //            go.transform.parent = trans;
        //        }
        //    }
        //}
        //private void SaveConfig ()
        //{
        //    CommonAssets.SaveAsset (sceneConfig);
        //}

        #endregion

        #region Create Config

        //private void CreateEditScene(string tagName, SceneConfig sceneConfig, string mainScenePath)
        //{
        //    string sceneSrcPath = string.Format("{0}/{1}.unity", sceneContext.dir, sceneContext.name);
        //    string scenePath = string.Format("{0}/{1}_{2}.unity", sceneContext.dir, sceneContext.name, tagName);
        //    AssetDatabase.CopyAsset(sceneSrcPath, scenePath);

        //    var newScene = EditorSceneManager.OpenScene(scenePath);
        //    BeginEdit(true);
        //    sceneConfig.sceneEditorTag.Add(tagName);
        //    for (int j = (int)EditorSceneObjectType.Light; j <= (int)EditorSceneObjectType.Instance; ++j)
        //    {
        //        Transform trans = commonContext.editorSceneGos[j].transform;
        //        Transform tagTrans = trans.Find(tagName);
        //        if (tagTrans == null)
        //        {
        //            GameObject go = new GameObject(tagName);
        //            go.transform.parent = trans;
        //        }
        //    }
        //    TerrainData td = null;
        //    GameObject currentTerrainGo = GetTerrainObject(ref td);
        //    if (currentTerrainGo != null)
        //    {
        //        InitTerrain(currentTerrainGo, tagName, commonContext.editorSceneGos[(int)EditorSceneObjectType.UnityTerrain]);
        //        InitChunks(sceneConfig, widthCount, heightCount, chunkWidth, chunkHeight, commonContext.editorSceneGos[(int)EditorSceneObjectType.MeshTerrain]);
        //    }
        //    EditorSceneManager.MarkSceneDirty(newScene);
        //    EditorSceneManager.SaveScene(newScene);
        //    EditorSceneManager.OpenScene(mainScenePath);
        //    BeginEdit(false);

        //}

        //private string BeginAddSceneTag()
        //{
        //    UnityEngine.SceneManagement.Scene s = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        //    EditorSceneManager.MarkSceneDirty(s);
        //    EditorSceneManager.SaveScene(s);
        //    AssetDatabase.SaveAssets();
        //    AssetDatabase.Refresh();
        //    return s.path;
        //}

        //private void EndAddSceneTag(string mainScenePath)
        //{
        //    EditorSceneManager.OpenScene(mainScenePath);
        //}

        #endregion

        #region Chunk edit

        ///////////////////////////Common///////////////////////////

        private void InitSceneData(SceneConfig sceneConfig)
        {
            this.sceneConfig = sceneConfig;
            SceneSerialize.GetSceneChunkCount(sceneConfig,
                out chunkWidth, out chunkHeight, out widthCount, out heightCount);

            EngineContext context = EngineContext.instance;
            ssContext.chunkWidth = EngineContext.ChunkSize;
            ssContext.chunkHeight = EngineContext.ChunkSize;
            ssContext.widthCount = sceneConfig.widthCount;
            ssContext.heightCount = sceneConfig.heightCount;

            if (sceneLocalConfig == null)
            {
                string path = string.Format("{0}/Scene_LocalConfig.asset",
                    sceneContext.configDir);
                if (File.Exists(path))
                {
                    sceneLocalConfig = AssetDatabase.LoadAssetAtPath<SceneConfigData>(path);
                }
                else
                {
                    sceneLocalConfig = ScriptableObject.CreateInstance<SceneConfigData>();
                    CommonAssets.CreateAsset<SceneConfigData>(sceneContext.configDir, "Scene_LocalConfig", ".asset",
                        sceneLocalConfig);
                }
            }

            sceneContext.suffix = sceneLocalConfig.editTag;

            PostInitLightmap();
            PostInitPreview();
            CalcQuadTree();
        }

        private EditorChunkData LoadEditorChunkData()
        {
            if (editorChunkData == null)
            {
                EditorChunkData ecd;
                SceneSerialize.LoadEditorChunkData(ref sceneContext, "", false, out ecd);
                if (ecd == null)
                {
                    ecd = ScriptableObject.CreateInstance<EditorChunkData>();
                    SaveEditorChunkData();
                }

                if (ecd.chunks.Count != widthCount * heightCount)
                {
                    ecd.chunks.Clear();
                    for (int i = 0; i < widthCount * heightCount; ++i)
                    {
                        ecd.chunks.Add(new EditorChunkData.EditorChunk());
                    }
                }

                editorChunkData = ecd;
            }

            return editorChunkData;
        }

        private SceneData LoadSceneData()
        {
            if (sceneData == null)
            {
                SceneSerialize.LoadSceneData<SceneData>(ref sceneContext, true, out sceneData);
                if (sceneData.chunks.Count != widthCount * heightCount)
                {
                    sceneData.chunks.Clear();
                    for (int i = 0; i < widthCount * heightCount; ++i)
                    {
                        sceneData.chunks.Add(new ChunkData());
                    }
                }
            }

            return sceneData;
        }

        private DynamicSceneData LoadDynamicSceneData()
        {
            if (dynamicSceneData == null)
            {
                SceneSerialize.LoadSceneData<DynamicSceneData>(ref sceneContext, true, out dynamicSceneData, "ds");
            }

            return dynamicSceneData;
        }

        private void SaveEditorChunkData()
        {
            if (editorChunkData != null)
            {
                SceneSerialize.SaveEditorChunkData(ref sceneContext, editorChunkData);
            }
        }

        private int FindChunkIndex(Vector3 pos)
        {
            return FindChunkIndex(ref pos);
        }

        private int FindChunkIndex(ref Vector3 pos)
        {
            int x;
            int z;
            int index = SceneQuadTree.FindChunkIndex(ref pos, chunkWidth, widthCount, heightCount, out x, out z);
            return index;
        }


        ///////////////////////////Main///////////////////////////
        private void DetectSameObject()
        {
            EditorCommon.EnumTransform funPrefabs = null;
            funPrefabs = (trans, param) =>
            {
                SaveChunkContext scp = param as SaveChunkContext;
                if (EditorCommon.IsPrefabOrFbx(trans.gameObject))
                {
                    UnityEngine.Object prefab = PrefabUtility.GetCorrespondingObjectFromSource(trans.gameObject);
                    //string path = AssetDatabase.GetAssetPath(prefab);
                    if (prefab != null)
                    {
                        List<ObjectPosInfo> objList;
                        if (!scp.sameObjMap.TryGetValue(prefab, out objList))
                        {
                            objList = new List<ObjectPosInfo>();
                            scp.sameObjMap.Add(prefab, objList);
                        }

                        Vector3 pos = trans.position;
                        Vector3 rot = trans.forward;
                        Vector3 scale = trans.lossyScale;
                        ObjectPosInfo find = null;
                        for (int i = 0; i < objList.Count; ++i)
                        {
                            ObjectPosInfo objPosInfo = objList[i];
                            if (Vector3.Distance(objPosInfo.pos, pos) < 0.1f &&
                                Vector3.Distance(objPosInfo.rot, rot) < 0.1f &&
                                Vector3.Distance(objPosInfo.scale, scale) < 0.1f)
                            {
                                find = objPosInfo;
                                break;
                            }
                        }

                        if (find == null)
                        {
                            find = new ObjectPosInfo();
                            find.pos = pos;
                            find.rot = rot;
                            find.scale = scale;
                            objList.Add(find);
                        }

                        find.objLsit.Add(trans);
                    }
                }
                else
                {
                    //folder
                    EditorCommon.EnumChildObject(trans, scp, funPrefabs);
                }
            };
            saveChunkContext.ecd = LoadEditorChunkData();
            for (int i = (int) EditorSceneObjectType.Light; i <= (int) EditorSceneObjectType.Instance; ++i)
            {
                string path = AssetsConfig.EditorGoPath[0] + "/" + AssetsConfig.EditorGoPath[i];
                saveChunkContext.sameObjMap.Clear();
                EditorCommon.EnumTargetObject(path, (trans, param) =>
                {
                    saveChunkContext.objType = (EditorSceneObjectType) i;
                    funPrefabs(trans, saveChunkContext);
                });
                var it = saveChunkContext.sameObjMap.GetEnumerator();
                while (it.MoveNext())
                {
                    var value = it.Current.Value;
                    for (int j = 0; j < value.Count; ++j)
                    {
                        ObjectPosInfo opi = value[j];
                        if (opi.objLsit.Count > 1)
                        {
                            if (deleteSame)
                            {
                                for (int k = 1; k < opi.objLsit.Count; ++k)
                                {
                                    var trans = opi.objLsit[k].transform;
                                    GameObject.DestroyImmediate(trans.gameObject);
                                }
                            }
                            else
                            {
                                string str = "";
                                for (int k = 0; k < opi.objLsit.Count; ++k)
                                {
                                    str += opi.objLsit[k].transform.name + " ";
                                }

                                Debug.LogError(string.Format("Same Pos:{0}", str));
                            }
                        }
                    }
                }
            }
        }

        //private void ExportMesh ()
        //{
        //    //EditorCommon.EnumTransform funPrefabs = null;
        //    //funPrefabs = (trans, param) =>
        //    //{
        //    //    if (EditorCommon.IsPrefabOrFbx (trans.gameObject))
        //    //    {
        //    //        GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource (trans.gameObject) as GameObject;
        //    //        if (prefab != null)
        //    //        {
        //    //            List<Renderer> renders = EditorCommon.GetRenderers (trans.gameObject);
        //    //            for (int i = 0; i < renders.Count; ++i)
        //    //            {
        //    //                Renderer r = renders[i];
        //    //                MeshFilter meshFilter = r.GetComponent<MeshFilter> ();
        //    //                if (meshFilter != null && r.gameObject.activeInHierarchy)
        //    //                {
        //    //                    string meshName = string.Format ("{0}_{1}", prefab.name, i);
        //    //                    string meshPath = string.Format ("{0}{1}.asset",
        //    //                        LoadMgr.singleton.editorResPath,
        //    //                        meshName);
        //    //                    if (!File.Exists (meshPath))
        //    //                    {
        //    //                        Mesh newMesh;
        //    //                        Material mat;
        //    //                        FBXAssets.GetMeshMat (r, out newMesh, out mat, true, false);
        //    //                        CommonAssets.CreateAsset<Mesh> (meshPath, ".asset", newMesh);
        //    //                        EditorUtility.DisplayProgressBar ("export mesh",
        //    //                            meshPath, 0.5f);
        //    //                    }
        //    //                }
        //    //            }
        //    //        }
        //    //    }
        //    //    else
        //    //    {
        //    //        EditorCommon.EnumChildObject (trans, param, funPrefabs);
        //    //    }
        //    //};
        //    //FBXAssets.removeColor = true;
        //    //for (int i = (int) EditorSceneObjectType.Prefab; i <= (int) EditorSceneObjectType.StaticPrefab; ++i)
        //    //{
        //    //    FBXAssets.removeUV2 = i == (int) EditorSceneObjectType.Prefab;
        //    //    string path = AssetsConfig.EditorGoPath[0] + "/" + AssetsConfig.EditorGoPath[i];
        //    //    EditorCommon.EnumTargetObject (path, (trans, param) =>
        //    //    {
        //    //        saveChunkContext.objType = (EditorSceneObjectType) i;
        //    //        funPrefabs (trans, saveChunkContext);
        //    //    });
        //    //}
        //    //FBXAssets.removeUV2 = false;
        //    //EditorUtility.ClearProgressBar ();
        //}
        private void ExportMirrorMesh()
        {
            EditorCommon.EnumTransform funPrefabs = null;
            funPrefabs = (trans, param) =>
            {
                // SaveChunkContext scp = param as SaveChunkContext;
                if (EditorCommon.IsPrefabOrFbx(trans.gameObject))
                {
                    GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(trans.gameObject) as GameObject;
                    if (prefab != null)
                    {
                        List<Renderer> renders = EditorCommon.GetRenderers(trans.gameObject);
                        for (int i = 0; i < renders.Count; ++i)
                        {
                            Renderer r = renders[i];
                            MeshFilter meshFilter = r.GetComponent<MeshFilter>();
                            if (meshFilter != null && r.gameObject.activeInHierarchy)
                            {
                                Vector3 scale = trans.lossyScale;
                                if ((scale.x * scale.y * scale.z) < 0)
                                {
                                    string meshName = string.Format("{0}_{1}_mirror", prefab.name, i);
                                    string meshPath = string.Format("{0}{1}.asset",
                                        LoadMgr.singleton.editorResPath,
                                        meshName);
                                    if (!File.Exists(meshPath))
                                    {
                                        Mesh mesh = meshFilter.sharedMesh;
                                        Mesh newMesh = UnityEngine.Object.Instantiate(mesh);
                                        newMesh.name = meshName;
                                        newMesh.uv3 = null;
                                        newMesh.uv4 = null;
                                        newMesh.colors = null;
                                        int[] triangles = newMesh.triangles;

                                        for (int j = 0; j < triangles.Length; j += 3)
                                        {
                                            int index = triangles[j + 2];
                                            triangles[j + 2] = triangles[j + 1];
                                            triangles[j + 1] = index;
                                        }

                                        newMesh.triangles = triangles;

                                        MeshUtility.SetMeshCompression(newMesh, ModelImporterMeshCompression.Low);
                                        MeshUtility.Optimize(newMesh);
                                        newMesh.UploadMeshData(true);
                                        CommonAssets.CreateAsset<Mesh>(meshPath, ".asset", newMesh);
                                        Debug.LogFormat("export mirror mesh:{0}", meshPath);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    EditorCommon.EnumChildObject(trans, param, funPrefabs);
                }
            };
            FBXAssets.removeColor = true;
            for (int i = (int) EditorSceneObjectType.Prefab; i <= (int) EditorSceneObjectType.StaticPrefab; ++i)
            {
                FBXAssets.removeUV2 = i == (int) EditorSceneObjectType.Prefab;
                string path = AssetsConfig.EditorGoPath[0] + "/" + AssetsConfig.EditorGoPath[i];
                EditorCommon.EnumTargetObject(path, (trans, param) =>
                {
                    saveChunkContext.objType = (EditorSceneObjectType) i;
                    funPrefabs(trans, saveChunkContext);
                });
            }

            FBXAssets.removeUV2 = false;
        }

        //private void CalcCollision ()
        //{
        //    EditorCommon.EnumTransform funPrefabs = null;
        //    funPrefabs = (trans, param) =>
        //    {
        //        MeshFilter mf = trans.GetComponent<MeshFilter> ();
        //        MeshRenderer mr = trans.GetComponent<MeshRenderer> ();
        //        MeshRenderObject mro = trans.GetComponent<MeshRenderObject> ();
        //        var cc = param as CollisionContext;
        //        if (mf != null && mr != null && mro != null && mro.IsRenderValid ())
        //        {
        //            if (trans.gameObject.layer == DefaultGameObjectLayer.TerrainLayer)
        //            {
        //                cc.terrainObjects.Add (new TerrainObjInfo ()
        //                {
        //                    mesh = mf.sharedMesh,
        //                        matrix = trans.localToWorldMatrix,
        //                });
        //            }
        //            else
        //            {
        //                Material mat = mr.sharedMaterial;
        //                if (mat != null)
        //                {
        //                    string tag = mat.GetTag ("RenderType", false);
        //                    if (tag == "Opaque" ||
        //                        tag != "Transparent" && mat.shader.name == "Custom/Editor/ScenePreview")
        //                    {

        //                        Matrix4x4 matrix = trans.localToWorldMatrix;
        //                        cc.collisionObjects.Add (new CollisionObjInfo ()
        //                        {
        //                            mesh = mf.sharedMesh,
        //                                matrix = trans.localToWorldMatrix,
        //                        });
        //                    }
        //                }
        //            }

        //        }
        //        EditorCommon.EnumChildObject (trans, param, funPrefabs);
        //    };
        //    collisionContext.Init (widthCount, heightCount);
        //    collisionContext.Reset ();
        //    // collisionContext.unitSize = 0.125f;
        //    collisionContext.unitCount = (int) (EngineContext.ChunkSize / EngineContext.terrainGridCount / collisionContext.unitSize);
        //    AddCB2SceneView (cbContext.collisionVexelCB, false);

        //    if (collisionContext.gizmoDraw)
        //    {
        //        GameObject go = Selection.activeGameObject;
        //        if (go != null)
        //        {
        //            MeshFilter mf = go.GetComponent<MeshFilter> ();
        //            if (mf != null && mf.sharedMesh != null)
        //            {
        //                collisionContext.collisionObjects.Add (new CollisionObjInfo ()
        //                {
        //                    mesh = mf.sharedMesh,
        //                        matrix = go.transform.localToWorldMatrix,
        //                });
        //                collisionContext.meshJob.Prepare (0);
        //                collisionContext.meshJob.Execute (0);
        //                collisionContext.meshJob.Collection (0);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        for (int i = (int) EditorSceneObjectType.Prefab; i <= (int) EditorSceneObjectType.StaticPrefab; ++i)
        //        {
        //            string path = AssetsConfig.EditorGoPath[0] + "/" + AssetsConfig.EditorGoPath[i];
        //            EditorCommon.EnumTargetObject (path, (trans, param) =>
        //            {
        //                funPrefabs (trans, collisionContext);
        //            });
        //        }
        //        Transform meshTerrain = GetMeshTerrain ();
        //        for (int z = 0; z < heightCount; ++z)
        //        {
        //            for (int x = 0; x < widthCount; ++x)
        //            {
        //                string chunkName = string.Format ("Chunk_{0}_{1}", x, z);
        //                TerrainObject to = FindMeshTerrain<TerrainObject> (meshTerrain, chunkName);
        //                if (to != null && to.terrainObjData.isValid)
        //                {
        //                    MeshFilter mf = to.GetComponent<MeshFilter> ();
        //                    collisionContext.terrainObjects.Add (new TerrainObjInfo ()
        //                    {
        //                        mesh = mf.sharedMesh,
        //                            matrix = to.transform.localToWorldMatrix,
        //                            chunkID = x + widthCount * z
        //                    });
        //                }
        //            }
        //        }
        //        collisionContext.threadManager.AddJobGroup (
        //            collisionContext.terrainVertexJob,
        //            collisionContext.terrainObjects.Count, 0.5f);
        //        int id = collisionContext.threadManager.AddJobGroup (
        //            collisionContext.meshJob,
        //            collisionContext.collisionObjects.Count, 0.5f);
        //        collisionContext.threadManager.AddJobGroup (
        //            collisionContext.meshCompressJob,
        //            0, 1,
        //            id);
        //        collisionContext.threadRun = true;
        //        collisionContext.threadManager.StartJobs ();
        //    }
        //}

        private void UpdateJobs()
        {
            if (threadManager != null)
            {
                bool finish = false;
                int jobCount = threadManager.GetTotalJobCount();
                if (jobCount > 0)
                {
                    int count = threadManager.CollectionJobs();
                    if (count > 0)
                    {
                        EditorUtility.DisplayProgressBar("runing jobs",
                            string.Format("{0}/{1}", count, jobCount), (float) count / jobCount);
                    }
                    else
                    {
                        finish = true;
                        threadManager.ClearJob();
                        EditorUtility.ClearProgressBar();
                    }
                }

                sceneLightContext.UpdateJobs(finish);
                if (finish)
                {
                    EditorUtility.DisplayDialog("Finish", "Calc Finish!", "OK");
                }
            }
        }

        private void SerializeScene(ESceneType sceneType)
        {
            var configs = SceneResConfig.instance.configs;
            for (int i = 0; i < configs.Count; ++i)
            {
                var config = configs[i];
                if (config.process != null && config.sceneType == sceneType)
                {
                    ssContext.resProcess = config.process;
                    config.process.PreSerialize(ref sceneContext, ssContext);
                }
            }


            for (int i = 0; i < configs.Count; ++i)
            {
                var config = configs[i];
                if (config.process != null && config.sceneType == sceneType)
                {
                    ssContext.resProcess = config.process;
                    config.process.Serialize(ref sceneContext, ssContext);
                }
            }
        }



        private void SaveEnvProfile()
        {
            if (EnvProfile.activeProfile != null)
            {
                CommonAssets.SaveAsset(EnvProfile.activeProfile);
            }
        }

        public static void SaveScenetoBundleRes()
        {
            SaveScene_Urp(out bool _);
        }

        public static bool bIsStreamerScene = false;

        private void SaveScene()
        {
            bIsStreamerScene = StreamerManagerEditor.RefreshStreamer();

            SceneEditTool.StoreStaticEditorFlags();

            //if (EngineContext.UseUrp)
            //{
            //    SaveScene_Urp();
            //    return;
            //}
            string dir = string.Format("{0}/Scene/{1}", AssetsConfig.instance.ResourcePath, sceneContext.name);
            EditorCommon.CreateDir(dir);
            var sd = LoadSceneData();
            sd.Clear();
            EngineContext context = EngineContext.instance;
            sd.saveFlag.SetFlag(EngineContext.SFlag_DistCull,
                sceneConfig.useDistCull /*|| (context.xChunkCount * context.zChunkCount) > 2*/);
            ssContext.sd = sd;
            ssContext.Reset();
            SerializeScene(ESceneType.Scene);
            SaveEnvProfile();
            CommonAssets.SaveAsset(sceneConfig);
            CommonAssets.SaveAsset(sd);
            SceneEditTool.RecoverStaticEditorFlags();

            SceneAssets.SceneModify();
        }

        private static SceneBuildOrNot.RowData[] table;

        private static SceneBuildOrNot.RowData[] Table
        {
            get
            {
                if (table == null)
                {
                    table = Readtable();
                }

                return table;
            }
        }

        private static SceneBuildOrNot.RowData[] Readtable()
        {
            XTableAsyncLoader loader = XTableAsyncLoader.Get();
            SceneBuildOrNot sceneInfo = CFAllocator.Allocate<SceneBuildOrNot>();
            loader.AddTask(@"Table/SceneBuildOrNot", sceneInfo);
            loader.Execute();
            return sceneInfo?.Table;
        }

        private static bool IsMatch(string sceneName, SceneBuildOrNot.RowData sceneInfo)
        {
            var match = false;

            if (sceneName == sceneInfo.SceneName)
                match = true;

            return match;
        }

        public static bool NotBuild(string sceneName)
        {
            if (Table == null)
            {
                Debug.LogError("cant read SceneBuildOrNot Table");
            }

            for (int i = 0; i < Table.Length; ++i)
            {
                if (IsMatch(sceneName, Table[i]) && Table[i].BuildOrNot == 0)
                    return true;
            }

            return false;
        }

        public static void SaveScene_Urp(out bool error, OnProcessRawScene rawSceneAction = null,
            OnProcessCopiedScene newSceneAction = null)
        {
            if (rawSceneAction != null)
            {
                rawSceneAction(out bool dirty, out error);
                if (error || (!dirty && newSceneAction != null))
                    return;
            }

            //test
            Scene scene = SceneManager.GetActiveScene();
            SceneAssets.SceneModify(true);
            AssetDatabase.Refresh();

            string srcPath = scene.path;
            string sceneName = scene.name;

            string copyPath = "Assets/Scenes/" + sceneName + "_Copy.unity";

            AssetDatabase.CopyAsset(srcPath, copyPath);

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            EditorSceneManager.OpenScene(copyPath);
            if (newSceneAction != null)
            {
                newSceneAction(out error);
                if (error)
                    return;
            }

            Scene sceneCopy = SceneManager.GetActiveScene();
            GameObject[] roots = sceneCopy.GetRootGameObjects();

            int countroot = roots.Length;

            string splitpath = "Assets/BundleRes/Scene/" + sceneName + "/SplitMeshFile";
            if (!Directory.Exists(splitpath))
            {
                Directory.CreateDirectory(splitpath);
            }

            GameObject _EditorScene = null;
            for (int i = 0; i < countroot; i++)
            {
                GameObject go = roots[i];
                if (go.name.Equals("EditorScene"))
                {
                    _EditorScene = go;
                    RemoveEditorScene(go);
                    // AnimationSet(go);
                    // LightSet(go);
                    LightAsShadowSet(go);
                    GPUInstancingSet(go);
                    InstanceSet(go);
                    SplitMesh(go, splitpath);
                    CleanSceneSFX(go);
                    CheckAndReplaceLayerBlendTex(go);
                    CheckAndSetBrightAOParams(go);
                    DecalAutoCollection(go);
                    DeleteUseLessObject(go);
                    EnableAllTreeSubObjs();
                    EnableLods();
                    SettingStreamerManager(go);
                    RemoveOpenWorld();


                    EditorCommon.ProcessCheckableComponents(go);

                    // if (ImpostorHelper.IsImpostors)
                    //     TryAddImpostors(go);

                    continue;
                }

                DestroyImmediate(go);
            }

            BytesAddScene(sceneName);

            SceneSFXPrefabSave();
            DeleteNoUseData();
            NullComponentAndMissingPrefabSearchTool.ClearMissing();

            EditorSceneManager.SaveScene(sceneCopy);
            string sceneFilePath = "Assets/BundleRes/Scene/" + sceneName + "/" + sceneName + ".unity";

            File.Copy(copyPath, sceneFilePath, true);
            AssetDatabase.DeleteAsset(copyPath);
            AssetDatabase.Refresh();

            if (bIsStreamerScene)
            {
                StreamerManagerEditor.ReplaceOrAddScenePrefab(_EditorScene, sceneName);
                AssetDatabase.DeleteAsset(sceneFilePath);
                AssetDatabase.Refresh();
            }
            else
            {
                string lightAssetCopyPath = "Assets/BundleRes/Scene/" + sceneName + "/LightingData.asset";
                CopyAndSetLightDataAsset(sceneFilePath, lightAssetCopyPath);
            }

            EditorSceneManager.OpenScene(srcPath);
            AssetDatabase.Refresh();

            if (bIsStreamerScene)
            {
                error = false;
                return;
            }

            // Append scene to build list.
            EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
            GUID guid = new GUID(AssetDatabase.AssetPathToGUID(sceneFilePath));
            if (buildScenes.All(s => s.guid != guid))
            {
                EditorBuildSettingsScene append = new EditorBuildSettingsScene();
                append.enabled = true;
                append.guid = guid;
                ArrayUtility.Add(ref buildScenes, append);
                append.path = sceneFilePath;
                Array.Sort(buildScenes, (a, b) => string.Compare(a.path, b.path, StringComparison.Ordinal));
                EditorBuildSettings.scenes = buildScenes;
            }

            error = false;

            if (NotBuild(sceneName))
            {
                CopyFileAndDir("Assets/BundleRes/Scene/" + sceneName, "Assets/BundleRes/SceneIgnore/" + sceneName);
                DeleteDirectory("Assets/BundleRes/Scene/" + sceneName);
            }
            else
            {
                if(Directory.Exists("Assets/BundleRes/SceneIgnore/" + sceneName))
                    DeleteDirectory("Assets/BundleRes/SceneIgnore/" + sceneName);
            }
        }

        private static void CopyFileAndDir(string dir, string desDir)
        {
            if (!Directory.Exists(desDir))
            {
                Directory.CreateDirectory(desDir);
            }

            IEnumerable<string> files = Directory.EnumerateFileSystemEntries(dir);
            if (files != null && files.Count() > 0)
            {
                foreach (var item in files)
                {
                    string desPath = Path.Combine(desDir, Path.GetFileName(item));

                    var fileExist = File.Exists(item);
                    if (fileExist)
                    {
                        File.Copy(item, desPath, true);
                        continue;
                    }

                    CopyFileAndDir(item, desPath);
                }
            }
        }

        private static void DeleteDirectory(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            FileSystemInfo[] fileSystems = dir.GetFileSystemInfos();
            foreach (var item in fileSystems)
            {
                if (item is DirectoryInfo)
                {
                    DirectoryInfo directory = new DirectoryInfo(item.FullName);
                    directory.Delete(true);
                }
                else
                {
                    File.Delete(item.FullName);
                }
            }
        }

        private static bool CopyAndSetLightDataAsset(string targetScenePath, string copyLightAssetDataPath)
        {
            if (string.IsNullOrEmpty(targetScenePath) || string.IsNullOrEmpty(copyLightAssetDataPath))
            {
                return false;
            }

            EditorSceneManager.OpenScene(targetScenePath);

            Scene curActiveScene = EditorSceneManager.GetActiveScene();

            // 复制 LightDataAsset
            LightingDataAsset srcLightDataAsset = Lightmapping.lightingDataAsset; //当前 active 场景的数据
            if (srcLightDataAsset == null)
            {
                return true;
            }

            string assetPath = AssetDatabase.GetAssetPath(srcLightDataAsset);
            AssetDatabase.CopyAsset(assetPath, copyLightAssetDataPath);

            // 修改 LightDataAsset 场景引用
            LightingDataAsset targetLightAssetDataCopy =
                AssetDatabase.LoadAssetAtPath<LightingDataAsset>(copyLightAssetDataPath);
            if (targetLightAssetDataCopy == null)
            {
                AssetDatabase.DeleteAsset(copyLightAssetDataPath);
                return false;
            }

            SerializedObject serializedObj = new SerializedObject(targetLightAssetDataCopy);
            SerializedProperty sceneProp = serializedObj.FindProperty("m_Scene");
            if (sceneProp == null)
            {
                AssetDatabase.DeleteAsset(copyLightAssetDataPath);
                return false;
            }

            SceneAsset curSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(targetScenePath);
            sceneProp.objectReferenceValue = curSceneAsset;
            sceneProp.serializedObject.ApplyModifiedProperties();
            serializedObj.SetIsDifferentCacheDirty();
            AssetDatabase.SaveAssetIfDirty(targetLightAssetDataCopy);

            // 设置 active LightDataAsset
            Lightmapping.lightingDataAsset = targetLightAssetDataCopy;
            EditorSceneManager.MarkSceneDirty(curActiveScene);
            EditorSceneManager.SaveScene(curActiveScene);
            AssetDatabase.Refresh();

            return true;
        }

        private static void GPUInstancingSet(GameObject go)
        {
            SceneBatchInstancing.ProcessCurrentScene();
        }

        private static void EnableAllTreeSubObjs()
        {
            GameObject tempObj = GameObject.Find("tree");
            if (tempObj == null)
                return;

            GameObject treeRoot = tempObj;
            Transform rootTrans = treeRoot.transform;
            if (rootTrans == null)
                return;

            for (int i = 0; i < rootTrans.childCount; i++)
            {
                bool trunkHidden = false;
                Transform oneTreeTrans = rootTrans.GetChild(i);
                if (oneTreeTrans == null)
                    continue;

                if (oneTreeTrans.childCount > 2)
                {
                    Transform treeChildTrans0 = oneTreeTrans.GetChild(0);
                    Transform treeChildTrans1 = oneTreeTrans.GetChild(1);
                    if (!treeChildTrans0 || !treeChildTrans1 || !treeChildTrans0.gameObject || !treeChildTrans1.gameObject)
                        continue;

                    if (treeChildTrans1.gameObject.name.Contains("trunk") && treeChildTrans1.gameObject.activeInHierarchy == false
                            || treeChildTrans0.gameObject.name.Contains("trunk") && treeChildTrans0.gameObject.activeInHierarchy == false)
                    {
                        trunkHidden = true;
                    }

                    for (int j = 2; j < oneTreeTrans.childCount; j++)
                    {
                        Transform treeChildTrans = oneTreeTrans.GetChild(j);
                        if (!treeChildTrans.gameObject || (!treeChildTrans.gameObject.name.Contains("leaves") && !treeChildTrans.gameObject.name.Contains("trunk")))
                            continue;

                        if (!trunkHidden || !treeChildTrans.gameObject.name.Contains("trunk"))
                        {
                            treeChildTrans.gameObject.SetActive(true);
                        }
                    }
                }
            }
        }

        private static void EnableLods()
        {
            MeshLODRoot meshLODRoot = GameObject.FindObjectOfType<MeshLODRoot>(true);
            LODGroup[] groups = GameObject.FindObjectsOfType<LODGroup>(true);
            MeshLODGroup tempMeshLODGroup = null;
            if (meshLODRoot != null && meshLODRoot.meshLODGroupList != null)
            {
                for (int i = 0; i < groups.Length; i++)
                {
                    if (groups[i] == null || groups[i].gameObject == null)
                    {
                        continue;
                    }
                    
                    bool isHave = false;
                    for (int k = 0; k < meshLODRoot.meshLODGroupList.Count; k++)
                    {
                        tempMeshLODGroup = meshLODRoot.meshLODGroupList[k];
                        if (tempMeshLODGroup.bindTransform == null)
                        {
                            continue;
                        }
                        if (groups[i].gameObject == tempMeshLODGroup.bindTransform.gameObject)
                        {
                            isHave = true;
                            break;
                        }
                    }
                
                    groups[i].enabled = !isHave;
                }
            }
            else
            {
                for (int i = 0; i < groups.Length; i++)
                {
                    groups[i].enabled = true;
                }
            }
            
        }

        private static void SettingStreamerManager(GameObject go)
        {
            if (!bIsStreamerScene) return;
            LightmapVolumn[] alls = go.GetComponentsInChildren<LightmapVolumn>(true);
            for (int i = 0; i < alls.Length; i++)
            {
                alls[i].bALoadBySceneManager = true;
            }
        }

        private static void RemoveOpenWorld()
        {
            OpenWorld openWorld = GameObject.FindObjectOfType<OpenWorld>();
            if (openWorld == null) return;
            openWorld.bDebug = false;

            StreamerPrefab[] allDatas = GameObject.FindObjectsOfType<StreamerPrefab>();
            for (int i = 0; i < allDatas.Length; i++)
            {
                allDatas[i].Show(false);
            }
        }

        private static void TryAddImpostors(GameObject go)
        {
            Camera camObj = new GameObject("Camera").AddComponent<Camera>();
            camObj.tag = "MainCamera";

            Selection.activeGameObject = go;
            ImpostorsEditorTools.CreateSceneManagers();

            List<GameObject> allVolumnsObj = new List<GameObject>();

            LightmapVolumn[] allVolumns = go.GetComponentsInChildren<LightmapVolumn>(true);

            for (int j = 0; j < allVolumns.Length; j++)
            {
                allVolumnsObj.Add(allVolumns[j].gameObject);
            }


            ImpostorsCfg[] impostorsCfg = go.GetComponentsInChildren<ImpostorsCfg>(true);
            for (int j = 0; j < impostorsCfg.Length; j++)
            {
                allVolumnsObj.Add(impostorsCfg[j].gameObject);
            }


            ImpostorHelper.ToolsImpostorLod(allVolumnsObj.ToArray());

            GameObject.DestroyImmediate(camObj.gameObject);
        }

        public static void DeleteNoUseData()
        {
            DestoryScript<InstanceGroup>();
            DestoryScript<InstanceObject>();
            DestoryScript<PVSCollider>();
            DestoryScript<TerrainObject>();
            DestoryScript<PVSChunk>();
            DestoryScript<MeshRenderObject>();
            DestoryScript<SFXWrapper>();
            DestoryScript<StaticCaster>();
            DestoryScript<SceneGroupObject>();
            DestoryScript<EnvGroup>();
            DestoryScript<GodRayLight>();
            // DestoryScript<LightParameterEntity>();
            DestoryScript<ImpostorsCfg>();
            DestoryScript<GrassManager>();
        }

        private static void DestoryScript<T>() where T : MonoBehaviour
        {
            T[] allMonos = GameObject.FindObjectsOfType<T>(true);
            if (allMonos.Length == 0) return;

            for (int i = allMonos.Length - 1; i >= 0; i--)
            {
                DestroyImmediate(allMonos[i]);
            }
        }

        private static void SceneSFXPrefabSave()
        {
            SFXPrefab[] allSFXPrefab = GameObject.FindObjectsOfType<SFXPrefab>(true);
            if (allSFXPrefab.Length == 0) return;

            for (int i = allSFXPrefab.Length - 1; i >= 0; i--)
            {
                var parent = allSFXPrefab[i].transform.parent;
                bool isPrefab = PrefabUtility.IsPartOfPrefabInstance(parent.gameObject);
                if (isPrefab)
                    PrefabUtility.UnpackPrefabInstance(parent.gameObject, PrefabUnpackMode.Completely,
                        InteractionMode.UserAction);
                allSFXPrefab[i].Save();
            }
        }

        private static void RemoveEditorScene(GameObject root)
        {
            GameObject dynamicObjectsObj = GameObject.Find("DynamicObjects");
            if (dynamicObjectsObj != null)
            {
                DestroyImmediate(dynamicObjectsObj);
            }
        }


        private static void LightSet(GameObject _EditorScene)
        {
            GameObject lightObj = _EditorScene.transform.Find("Light/MainScene").gameObject;
            if (lightObj != null)
            {
                foreach (Transform t in lightObj.transform)
                {
                    Light light = t.gameObject.GetComponent<Light>();
                    if (light != null)
                    {
                        t.gameObject.SetActive(false);
                    }
                }
            }
        }

        private static void LightAsShadowSet(GameObject _EditorScene)
        {
            GameObject lightObj = _EditorScene.transform.Find("Light/MainScene").gameObject;
            if (lightObj != null)
            {
                var lightShadow = lightObj.GetComponentsInChildren<SpotLightAsShadow>(true);
                GameObject lightShadowMgr = new GameObject("LightShadowMgr");
                SpotLightAsShadowMgr shadowMgr = _EditorScene.GetComponentInChildren<SpotLightAsShadowMgr>(true);
                if (shadowMgr == null)
                {
                    shadowMgr = lightShadowMgr.AddComponent<SpotLightAsShadowMgr>();
                }

                shadowMgr.enabled = true;
                shadowMgr.gameObject.SetActive(true);
                shadowMgr.shadowLights = lightShadow.ToList();
                shadowMgr.transform.parent = _EditorScene.transform;
            }
        }

        private static void InstanceSet(GameObject _EditorScene)
        {
            GameObject _Instance = _EditorScene.transform.Find("Instance").gameObject;
            GrassManager _grassmanager = _Instance.GetComponent<GrassManager>();
            if (!_grassmanager)
            {
                _Instance.AddComponent<GrassManager>();
            }

            _grassmanager = _Instance.GetComponent<GrassManager>();
            if (_grassmanager)
            {
                _grassmanager.SearchGrass1();
            }

            //InstanceGroup _ig= _Instance.GetComponent<InstanceGroup>();
            //if (_ig)
            //{
            //   UnityEngine. Object.DestroyImmediate(_ig);
            //}
        }

        private static void CleanSceneSFX(GameObject _EditorScene)
        {
            Transform _root = _EditorScene.transform.Find("Effects");
            // _root = _root.transform.Find("MainScene").gameObject;
            // _root.TryGetComponent(out SceneSFXCreator creator);
            // if (creator == null)
            // {
                SceneSFXCreator creator = _root.gameObject.AddComponent<SceneSFXCreator>();
                Queue<GameObject> waitForDestroy = new Queue<GameObject>();
                CleanSceneSFX(_root, creator, waitForDestroy);
                SFXEditorSceneExtraSign[] signs = FindObjectsOfType<SFXEditorSceneExtraSign>();
                CleanSceneSFX(signs, creator, waitForDestroy);
                while (waitForDestroy.Count > 0)
                {
                    var prefab = waitForDestroy.Dequeue();
                    if (prefab != null && PrefabUtility.IsAnyPrefabInstanceRoot(prefab))
                    {
                        DestroyImmediate(prefab);
                        // PrefabUtility.UnpackPrefabInstance(prefab, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                        // int count = prefab.transform.childCount;
                        // while (count > 0)
                        // {
                        //     DestroyImmediate(prefab.transform.GetChild(0).gameObject);
                        //     count = prefab.transform.childCount;
                        // }
                    }
                    
                }
                
            // }
        }

        private static void CleanSceneSFX(Transform parent, SceneSFXCreator creator, Queue<GameObject> waitForDestroy)
        {
            //遍历子物体
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                GameObject origin = parent.transform.GetChild(i).gameObject;
                //是prefab就检定是不是特效的prefab
                if (PrefabUtility.IsAnyPrefabInstanceRoot(origin))
                {
                    origin.TryGetComponent(out SceneSFXData data);
                    if (creator.Add(origin, origin.activeSelf, data))
                    {
                        waitForDestroy.Enqueue(origin);
                    }
                }
                else
                {
                    //非prefab就继续遍历子物体
                    if (origin.transform.childCount > 0)
                    {
                        CleanSceneSFX(origin.transform, creator, waitForDestroy);
                    }
                }
            }
        }
        private static void CleanSceneSFX(SFXEditorSceneExtraSign[] list, SceneSFXCreator creator, Queue<GameObject> waitForDestroy)
        {
            //遍历子物体
            for (int i = 0; i < list.Length; i++)
            {
                GameObject origin = list[i].gameObject;
                //是prefab就检定是不是特效的prefab
                if (PrefabUtility.IsAnyPrefabInstanceRoot(origin))
                {
                    origin.TryGetComponent(out SceneSFXData data);
                    if (creator.Add(origin, origin.activeSelf, data))
                    {
                        waitForDestroy.Enqueue(origin);
                    }
                }
            }
        }

        private static void CheckAndSetBrightAOParams(GameObject editorSceneObj)
        {
            BrightAOParamSetting brightAOParamSetting;
            brightAOParamSetting = editorSceneObj.GetComponent<BrightAOParamSetting>();
            if (brightAOParamSetting == null)
            {
                brightAOParamSetting = editorSceneObj.AddComponent<BrightAOParamSetting>();
                brightAOParamSetting.InitParam();
            }
        }

        private static void DecalAutoCollection(GameObject editorSceneObj)
        {
            DecalRoot decalRoot = editorSceneObj.GetComponentInChildren<DecalRoot>();
            if (decalRoot != null)
            {
                decalRoot.CollectDecal();
                decalRoot.SetVisible(false);
            }
        }
        

        private static void CheckAndReplaceLayerBlendTex(GameObject editorSceneObj)
        {
            GameObject staticPrefabRootObj = editorSceneObj.transform.Find("StaticPrefabs").gameObject;
            Renderer[] tempRenders = staticPrefabRootObj.GetComponentsInChildren<Renderer>(true);
            int len = tempRenders.Length;
            Renderer tempRender;
            Material tempMaterial;
            Shader[] layerShaders = AssetsConfig.instance.layerShader;
            bool isLayerObj = false;
            BlendTexConfig tempBlendTexConfig;
            string assetPath, dir, blendConfigPath;
            Texture layerTex;
            int blendTexProp = Shader.PropertyToID("_BlendTex");
            List<Material> tempMaterialList = new List<Material>();

            for (int i = 0; i < len; i++)
            {
                tempRender = tempRenders[i];

                if (tempRender == null)
                {
                    continue;
                }

                tempMaterial = tempRender.sharedMaterial;
                if (tempMaterial == null)
                {
                    continue;
                }

                if (!tempMaterialList.Contains(tempMaterial))
                {
                    tempMaterialList.Add(tempMaterial);
                }
            }

            len = tempMaterialList.Count;
            for (int i = 0; i < len; i++)
            {
                isLayerObj = false;
                tempMaterial = tempMaterialList[i];
                for (int j = 0; j < layerShaders.Length; j++)
                {
                    if (layerShaders[j] == tempMaterial.shader)
                    {
                        isLayerObj = true;
                        break;
                    }
                }

                if (!isLayerObj)
                {
                    continue;
                }

                layerTex = tempMaterial.GetTexture(blendTexProp);
                if (layerTex == null)
                {
                    continue;
                }

                dir = AssetsPath.GetAssetDir(tempMaterial, out assetPath);
                blendConfigPath = string.Format("{0}/{1}_Blend.asset", dir, tempMaterial.name);
                tempBlendTexConfig = AssetDatabase.LoadAssetAtPath<BlendTexConfig>(blendConfigPath);
                if (tempBlendTexConfig == null || tempBlendTexConfig.runtimeTex == null ||
                    tempBlendTexConfig.editTex == null)
                {
                    Debug.Log("BlendTexConfig 加载失败：" + blendConfigPath);
                    continue;
                }

                if ((layerTex != tempBlendTexConfig.runtimeTex) && (layerTex == tempBlendTexConfig.editTex))
                {
                    tempMaterial.SetTexture(blendTexProp, tempBlendTexConfig.runtimeTex);
                }
            }
        }

        private static void DeleteUseLessObject(GameObject _EditorScene)
        {
            GameObject _MeshTerrain = _EditorScene.transform.Find("MeshTerrain").gameObject;
            GameObject _UnityTerrain = _EditorScene.transform.Find("UnityTerrain").gameObject;
            GameObject _StaticPrefabs = _EditorScene.transform.Find("StaticPrefabs").gameObject;
            if (_MeshTerrain)
            {
                DestroyImmediate(_MeshTerrain);
            }

            if (_UnityTerrain)
            {
                DestroyImmediate(_UnityTerrain);
            }

            if (_StaticPrefabs)
            {
                MeshCollider[] _meshcollider = _StaticPrefabs.transform.GetComponentsInChildren<MeshCollider>();
                if (_meshcollider.Length > 0)
                {
                    int length = _meshcollider.Length;
                    for (int i = 0; i < length; i++)
                    {
                        DestroyImmediate(_meshcollider[i]);
                    }
                }
            }

            MeshRenderer[] _standardshader = _EditorScene.transform.GetComponentsInChildren<MeshRenderer>();
            if (_standardshader.Length > 0)
            {
                int length = _standardshader.Length;
                for (int i = 0; i < length; i++)
                {
                    Material mat = _standardshader[i].sharedMaterial;
                    if (mat == null || mat.shader == null) continue;

                    if (mat.shader.name.Contains("Standard"))
                    {
                        Shader _shader = Shader.Find("Universal Render Pipeline/Lit");
                        if (_shader)
                        {
                            mat.shader = _shader;
                        }
                        //   DestroyImmediate(_standardshader[i].gameObject);
                    }
                }
            }
        }

        private static void SplitMesh(GameObject _EditorScene, string Scenepath)
        {
            GameObject MainScene = _EditorScene.transform.Find("StaticPrefabs/MainScene").gameObject;
            if (MainScene == null) return;

            // MeshRenderer[] _meshrender = MainScene.transform.GetComponentsInChildren<MeshRenderer>();
            Dictionary<MeshRenderer, Transform> _meshrender = new Dictionary<MeshRenderer, Transform>();

            int mainscenechildcount = MainScene.transform.childCount;
            //具体CHUNK
            for (int a = 0; a < mainscenechildcount; a++)
            {
                Transform _chunk = MainScene.transform.GetChild(a);
                if (_chunk.childCount > 0)
                {
                    for (int i = 0; i < _chunk.childCount; i++)
                    {
                        if (!_chunk.GetChild(i).name.Contains("UnSplitObject"))
                        {
                            MeshRenderer[] _tempmeshrender =
                                _chunk.GetChild(i).GetComponentsInChildren<MeshRenderer>(false);
                            if (_tempmeshrender.Length > 0)
                            {
                                for (int b = 0; b < _tempmeshrender.Length; b++)
                                {
                                    if (_tempmeshrender[b].gameObject.activeInHierarchy)
                                    {
                                        _meshrender.Add(_tempmeshrender[b], _chunk);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            GenerateBlockColliders(MainScene);
        }

        public static void GenerateBlockColliders(GameObject root)
        {
            CameraBlockGroup[] cbgArray = root.GetComponentsInChildren<CameraBlockGroup>(true);
            Dictionary<Renderer, CameraBlockGroup> mroMap = new Dictionary<Renderer, CameraBlockGroup>();
            foreach (CameraBlockGroup blockGroup in cbgArray)
            {
                foreach (Collider collider in blockGroup.colliders)
                {
                    if (collider)
                    {
                        collider.gameObject.layer = CameraAvoidBlock.Layer;
                    }
                }

                foreach (Renderer renderer in blockGroup.renderers)
                {
                    if (renderer)
                    {
                        if (mroMap.TryGetValue(renderer, out CameraBlockGroup rawGroup))
                        {
                            string colliderPath = EditorCommon.GetSceneObjectPath(renderer.transform);
                            string rawGroupPath = EditorCommon.GetSceneObjectPath(rawGroup.transform);
                            string newGroupPath = EditorCommon.GetSceneObjectPath(blockGroup.transform);
                            Debug.LogError($"物体已经被配置到其他组。\n物体路径：{colliderPath}\n组1：{rawGroupPath}\n组2：{newGroupPath}");
                        }
                        else
                        {
                            mroMap[renderer] = blockGroup;
                        }
                    }
                }
            }

            MeshRenderObject[] mroArray = root.GetComponentsInChildren<MeshRenderObject>(true);
            List<Collider> colliders = new List<Collider>();
            foreach (MeshRenderObject mro in mroArray)
            {
                if (!mro) continue;
                var renderer = mro.GetRenderer();
                if (!renderer) continue;
                Vector3 aabbSize = mro.aabb.max - mro.aabb.min;
                bool split = IsSplitInput && (aabbSize.x > minwidth || aabbSize.y > minwidth || aabbSize.z > minwidth);
                if (!split && mro.fadeEffect && !mroMap.ContainsKey(renderer))
                {
                    Mesh mesh = mro.GetMesh();
                    if (mesh)
                    {
                        colliders.Clear();
                        string prefix = mro.name + CameraAvoidBlock.suffix;
                        for (int i = 0; i < mro.transform.childCount; i++)
                        {
                            Transform child = mro.transform.GetChild(i);
                            if (child.name.StartsWith(prefix) && child.TryGetComponent(out Collider collider))
                            {
                                colliders.Add(collider);
                                collider.gameObject.layer = CameraAvoidBlock.Layer;
                            }
                        }

                        if (colliders.Count == 0)
                        {
                            GameObject colliderObject = new GameObject(prefix + "0", typeof(BoxCollider));
                            colliderObject.transform.SetParent(mro.transform, false);
                            BoxCollider bc = colliderObject.GetComponent<BoxCollider>();
                            bc.center = mesh.bounds.center;
                            bc.size = mesh.bounds.size;
                            bc.isTrigger = true;
                            colliderObject.layer = CameraAvoidBlock.Layer;
                        }
                    }
                }
            }
        }

        //清理所有的splitmesh
        private static void DeleteALLSplitMesh()
        {
            string deletePath = "Assets/Scenes";
            string[] allPath = AssetDatabase.FindAssets("t:Prefab", new string[] {deletePath});
            for (int i = 0; i < allPath.Length; i++)
            {
                string prefabpath = AssetDatabase.GUIDToAssetPath(allPath[i]);
                if (prefabpath.Contains("_split"))
                {
                    UnityEngine.Object _obj = AssetDatabase.LoadAssetAtPath(prefabpath, typeof(UnityEngine.Object));
                    GameObject goobj = (GameObject) _obj;
                    Mesh _mesh = goobj.GetComponent<MeshFilter>().sharedMesh;
                    string meshpath = AssetDatabase.GetAssetPath(_mesh);
                    AssetDatabase.DeleteAsset(meshpath);
                    AssetDatabase.DeleteAsset(prefabpath);
                    //      Debug.LogError(prefabpath + "         " + meshpath);
                }
            }
        }


        [MenuItem("Tools/Direct_SaveDynamicScene", false, 20)]
        public static void Direct_SaveDynamicScene()
        {
            SceneEditTool data = GetDirect_SceneEditTool();
            data.SaveDynamicScene();
            data.OnUninit();
        }

        private void SaveDynamicScene()
        {
            string dir = string.Format("{0}/Scene/{1}", AssetsConfig.instance.ResourcePath, sceneContext.name);
            EditorCommon.CreateDir(dir);
            var dsd = LoadDynamicSceneData();
            dsd.Clear();
            ssContext.dsd = dsd;
            ssContext.Reset();
            SerializeScene(ESceneType.DynamicScene);
            CommonAssets.SaveAsset(dsd);
            SceneAssets.SceneModify();
            SceneSerialize.SaveDynamicScene(sceneConfig, ref sceneContext);
            // SaveAll();
        }

        private void SaveAll()
        {
            SaveScene();
            SceneSerialize.SaveScene2(sceneConfig, ref sceneContext);
            SaveScenetoBundleRes();
        }

        #endregion

        #region Merge

        //private void LoadColliderData (EditorChunkData.ColliderData cd, GameObject prefabOrGo)
        //{
        //    if (cd != null)
        //    {
        //        switch (cd.colliderType)
        //        {
        //            case EditorChunkData.ColliderType.Box:
        //                {
        //                    BoxCollider bc = prefabOrGo.AddComponent<BoxCollider> ();
        //                    bc.center = cd.center;
        //                    bc.size = cd.size;
        //                }
        //                break;
        //            case EditorChunkData.ColliderType.Sphere:
        //                {
        //                    SphereCollider sc = prefabOrGo.AddComponent<SphereCollider> ();
        //                    sc.center = cd.center;
        //                    sc.radius = cd.radius;
        //                }
        //                break;
        //            case EditorChunkData.ColliderType.Capsule:
        //                {
        //                    CapsuleCollider cc = prefabOrGo.AddComponent<CapsuleCollider> ();
        //                    cc.center = cd.center;
        //                    cc.radius = cd.radius;
        //                    cc.height = cd.height;
        //                    cc.direction = cd.direction;
        //                }
        //                break;
        //            case EditorChunkData.ColliderType.Mesh:
        //                {
        //                    MeshCollider mc = prefabOrGo.AddComponent<MeshCollider> ();
        //                    mc.sharedMesh = cd.sharedMesh;
        //                    mc.convex = cd.convex;
        //                    mc.cookingOptions = cd.cookingOptions;
        //                }
        //                break;
        //        }
        //    }
        //}

        //private void LoadLightData (LightData ld, GameObject prefabOrGo)
        //{
        //    if (ld != null && ld.lightType >= 0)
        //    {
        //        Light light = prefabOrGo.AddComponent<Light> ();
        //        light.type = (LightType) ld.lightType;
        //        light.cullingMask = ld.cullingMask;
        //        light.lightmapBakeType = ld.lightmapBakeType;
        //        light.renderMode = ld.renderMode;
        //        light.color = ld.color;
        //        light.intensity = ld.intensity;
        //        light.bounceIntensity = ld.bounceIntensity;
        //        light.colorTemperature = ld.colorTemperature;

        //        light.shadows = ld.shadows;
        //        light.shadowStrength = ld.shadowStrength;
        //        light.shadowResolution = ld.shadowResolution;
        //        light.shadowBias = ld.shadowBias;
        //        light.shadowNormalBias = ld.shadowNormalBias;
        //        light.shadowNearPlane = ld.shadowNearPlane;
        //        light.shadowCustomResolution = ld.shadowCustomResolution;

        //        light.spotAngle = ld.spotAngle;
        //        light.range = ld.range;
        //    }
        //}
        //private void LoadDynamicObjectData (EditorChunkData.DynamicObjectData dod, GameObject prefabOrGo)
        //{
        //    if (dod != null && dod.type != EditorChunkData.DynamicObjectType.None)
        //    {
        //        XWall wall = null;
        //        switch (dod.type)
        //        {
        //            case EditorChunkData.DynamicObjectType.Dummy:
        //                {
        //                    wall = prefabOrGo.AddComponent<XDummyWall> ();
        //                }
        //                break;
        //            case EditorChunkData.DynamicObjectType.Spawn:
        //                {
        //                    XSpawnWall w = prefabOrGo.AddComponent<XSpawnWall> ();
        //                    w.TriggerType = ((dod.flag & (uint) SceneDynamicObject.TriggerOnce) != 0) ? XSpawnWall.etrigger_type.once : XSpawnWall.etrigger_type.always;
        //                    w.exString = dod.exString;
        //                    wall = w;
        //                }
        //                break;
        //            case EditorChunkData.DynamicObjectType.Terminal:
        //                {
        //                    XTerminalWall w = prefabOrGo.AddComponent<XTerminalWall> ();
        //                    w.exString = dod.exString;
        //                    wall = w;
        //                }
        //                break;
        //            case EditorChunkData.DynamicObjectType.Transfer:
        //                {
        //                    XTransferWall w = prefabOrGo.AddComponent<XTransferWall> ();
        //                    w.targetScene = XTransferWall.transfer_type.other_scene;
        //                    int.TryParse (dod.exString, out w.sceneID);
        //                    wall = w;
        //                }
        //                break;
        //            case EditorChunkData.DynamicObjectType.Circle:
        //                {
        //                    wall = prefabOrGo.AddComponent<XCircleWall> ();
        //                }
        //                break;
        //        }
        //        if (wall != null)
        //        {
        //            wall.hashStr = dod.hashStr;
        //            if ((dod.flag & 0xff) != 0)
        //            {
        //                if ((dod.flag & (uint) SceneDynamicObject.SphereTrigger) != 0)
        //                {
        //                    SphereCollider sphere = prefabOrGo.AddComponent<SphereCollider> ();
        //                    sphere.center = dod.boxCenter;
        //                    sphere.radius = dod.pos1.x;
        //                    sphere.enabled = false;
        //                }
        //                else
        //                {
        //                    BoxCollider box = prefabOrGo.AddComponent<BoxCollider> ();
        //                    box.center = dod.boxCenter;
        //                    box.size = dod.boxSize;
        //                    box.enabled = false;
        //                }
        //            }
        //            for (int i = 0; i < dod.effects.Count; ++i)
        //            {
        //                var effect = dod.effects[i];
        //                if (effect.prefab != null)
        //                {
        //                    GameObject go = PrefabUtility.InstantiatePrefab (effect.prefab) as GameObject;
        //                    go.transform.parent = wall.transform;
        //                    go.transform.position = effect.pos;
        //                    go.transform.rotation = effect.rotate;
        //                    go.transform.localScale = effect.scale;
        //                }
        //            }
        //        }
        //    }
        //}

        //private void LoadEnvData (EnvObject eo, GameObject prefabOrGo)
        //{
        //    EnvArea area = prefabOrGo.AddComponent<EnvArea> ();
        //    area.color = eo.color;
        //    area.CreatLoadProfile (sceneContext.dir);
        //}
        //private void LoadLightProbes (LightProbeObject lpo, GameObject prefabOrGo)
        //{
        //    var lpa = prefabOrGo.AddComponent<LightprobeArea> ();
        //    lpa.Copy (lpo);
        //}

        //private void LoadAudioData (AudioData ad, GameObject prefabOrGo)
        //{
        //    if (ad != null)
        //    {
        //        var ao = prefabOrGo.AddComponent<AudioObject> ();
        //        ao.eventName = ad.eventName;
        //        ao.range = ad.range;
        //    }
        //}

        //private GameObject CreateGameObject (EditorChunkData ecd, EditorChunkData.GameObjectInfo goi, GameObject prefab)
        //{
        //    GameObject prefabOrGo = null;
        //    EditorSceneObjectType prefabType = (EditorSceneObjectType) goi.prefabType;
        //    if (prefab != null)
        //    {
        //        prefabOrGo = PrefabUtility.InstantiatePrefab (prefab) as GameObject;
        //    }
        //    else if (prefabType == EditorSceneObjectType.Collider ||
        //        prefabType == EditorSceneObjectType.Light ||
        //        prefabType == EditorSceneObjectType.Enverinment ||
        //        prefabType == EditorSceneObjectType.DynamicObject ||
        //        prefabType == EditorSceneObjectType.LightProbes)
        //    {
        //        prefabOrGo = new GameObject ();
        //    }
        //    if (prefabOrGo != null)
        //    {
        //        prefabOrGo.name = goi.goname;
        //        prefabOrGo.transform.position = goi.pos;
        //        prefabOrGo.transform.rotation = goi.rotate;
        //        prefabOrGo.transform.localScale = goi.scale;
        //        prefabOrGo.SetActive (goi.HasFlag (ObjectFlag.GameObjectActive));
        //        prefabOrGo.tag = goi.tag;
        //        prefabOrGo.layer = goi.layer;
        //        if (prefabType == EditorSceneObjectType.Collider)
        //        {
        //            Vector4Int chunk = goi.subPrefabChunkInfo[0];
        //            if (chunk.z >= 0 && chunk.z < ecd.colliderDatas.Count)
        //                LoadColliderData (ecd.colliderDatas[chunk.z], prefabOrGo);
        //        }
        //        else if (prefabType == EditorSceneObjectType.Light)
        //        {
        //            Vector4Int chunk = goi.subPrefabChunkInfo[0];
        //            if (chunk.z >= 0 && chunk.z < ecd.lightDatas.Count)
        //                LoadLightData (ecd.lightDatas[chunk.z], prefabOrGo);
        //        }
        //        else if (prefabType == EditorSceneObjectType.Enverinment)
        //        {
        //            Vector4Int chunk = goi.subPrefabChunkInfo[0];
        //            if (chunk.z >= 0 && chunk.z < ecd.envObjects.Count)
        //                LoadEnvData (ecd.envObjects[chunk.z], prefabOrGo);
        //        }
        //        else if (prefabType == EditorSceneObjectType.DynamicObject)
        //        {
        //            Vector4Int chunk = goi.subPrefabChunkInfo[0];
        //            if (chunk.z >= 0 && chunk.z < ecd.dynamicObjectDatas.Count)
        //                LoadDynamicObjectData (ecd.dynamicObjectDatas[chunk.z], prefabOrGo);
        //        }
        //        else if (prefabType == EditorSceneObjectType.LightProbes)
        //        {
        //            //Vector4Int chunk = goi.subPrefabChunkInfo[0];
        //            //if (chunk.z >= 0 && chunk.z < ecd.lightProbes.Count)
        //            //    LoadLightProbes(ecd.lightProbes[chunk.z], prefabOrGo);
        //        }
        //        else if (prefabType == EditorSceneObjectType.Audio)
        //        {
        //            Vector4Int chunk = goi.subPrefabChunkInfo[0];
        //            if (chunk.z >= 0 && chunk.z < ecd.audios.Count)
        //                LoadAudioData (ecd.audios[chunk.z], prefabOrGo);
        //        }
        //    }
        //    if (prefabOrGo == null)
        //    {
        //        Debug.LogErrorFormat ("null prefab:{0}", goi.prefabType);
        //    }
        //    return prefabOrGo;
        //}

        //private EditorChunkData.SubPrefabOverrideInfo GetSubPrefabOverrideInfo (EditorChunkData ecd, Vector4Int chunkIndex)
        //{
        //    if (chunkIndex.z >= 0 && chunkIndex.x >= 0 && chunkIndex.x < ecd.chunks.Count)
        //    {
        //        var chunk = ecd.chunks[chunkIndex.x];
        //        if (chunkIndex.y >= 0 && chunkIndex.y < chunk.subPrefabOverrideInfos.Count)
        //        {
        //            return chunk.subPrefabOverrideInfos[chunkIndex.y];
        //        }
        //    }
        //    return null;
        //}

        //private void OverrideSubPrefab (EditorChunkData ecd, string tagName, EditorChunkData.GameObjectInfo goi, GameObject go, GameObject prefab)
        //{
        //    if (goi.subPrefabChunkInfo.Count > 0)
        //    {
        //        List<Renderer> renderers = EditorCommon.GetRenderers (go);
        //        for (int j = 0; j < goi.subPrefabChunkInfo.Count; ++j)
        //        {
        //            Vector4Int chunkIndex = goi.subPrefabChunkInfo[j];
        //            if (chunkIndex.w != 255)
        //            {
        //                var spoi = GetSubPrefabOverrideInfo (ecd, chunkIndex);
        //                if (spoi != null && spoi.renderIndex >= 0 && spoi.renderIndex < renderers.Count)
        //                {
        //                    int renderIndex = spoi.renderIndex;
        //                    if (spoi.HasFlag (ObjectFlag.MultiMaterial))
        //                    {
        //                        if (renderIndex % 2 != 0)
        //                        {
        //                            continue;
        //                        }
        //                        else
        //                        {
        //                            renderIndex /= 2;
        //                        }

        //                    }
        //                    Renderer r = renderers[renderIndex];
        //                    if (r.name == spoi.name)
        //                    {
        //                        r.enabled = spoi.HasFlag (ObjectFlag.RenderEnable);
        //                        r.transform.localScale = spoi.localScale;
        //                        CommonAssets.SetSerializeValue (r, "m_ScaleInLightmap", spoi.lightMapScale);
        //                        r.sharedMaterial = spoi.mat;
        //                        if (spoi.colliderIndex >= 0 && spoi.colliderIndex < ecd.colliderDatas.Count)
        //                        {
        //                            var colliderData = ecd.colliderDatas[spoi.colliderIndex];
        //                            LoadColliderData (colliderData, r.gameObject);

        //                        }
        //                        EditorSceneObjectType prefabType = (EditorSceneObjectType) goi.prefabType;
        //                        if (prefabType == EditorSceneObjectType.StaticPrefab ||
        //                            prefabType == EditorSceneObjectType.Prefab)
        //                        {
        //                            MeshRenderObject oc = r.gameObject.AddComponent<MeshRenderObject> ();
        //                            oc.lightmapComponent.lightMapScale = spoi.lightMapScale;
        //                            oc.lightmapComponent.lightMapIndex = spoi.lightMapIndex;
        //                            oc.lightmapComponent.lightMapVolumnIndex = spoi.lightMapVolumnIndex;
        //                            oc.lightmapComponent.lightmapUVST = spoi.lightmapUVST;
        //                            // oc.chunkID = chunkIndex.x;

        //                        }
        //                        else if (prefabType == EditorSceneObjectType.Instance)
        //                        {
        //                            InstanceObject io = r.gameObject.AddComponent<InstanceObject> ();
        //                            io.chunkIndex = chunkIndex.x;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //private void LoadFromEditorChunkData (EditorChunkData ecd, string configName, bool replaceLocal = false)
        //{
        //    ////create folder
        //    //for (int i = 0; i < ecd.gameObjectInfoGroups.Count; ++i)
        //    //{
        //    //    var goi = ecd.gameObjectInfoGroups[i];
        //    //    string tagName = string.IsNullOrEmpty (configName) ? goi.tagName : configName;
        //    //    if (!replaceLocal && tagName == sceneContext.suffix)
        //    //    {
        //    //        continue;
        //    //    }
        //    //    string[] paths = goi.path.Split ('/');
        //    //    // string path = AssetsConfig.EditorGoPath[0] + "/" + AssetsConfig.EditorGoPath[goi.objType] + "/" + tagName;
        //    //    // GameObject parent = GameObject.Find (path);
        //    //    Transform parentTrans = null;
        //    //    string path = "";
        //    //    for (int j = 0; j < paths.Length; ++j)
        //    //    {
        //    //        path += paths[j];
        //    //        GameObject go = GameObject.Find (path);
        //    //        if (go == null)
        //    //        {
        //    //            go = new GameObject (paths[j]);
        //    //            go.transform.parent = parentTrans;
        //    //        }
        //    //        goi.transform = go.transform;
        //    //        parentTrans = go.transform;
        //    //        path += "/";
        //    //    }
        //    //    if (goi.transform != null)
        //    //        goi.transform.gameObject.SetActive (goi.visible);
        //    //}

        //    ////create gameobjects
        //    //for (int i = 0; i < ecd.gameObjectInfos.Count; ++i)
        //    //{
        //    //    var goi = ecd.gameObjectInfos[i];
        //    //    Transform parent = null;
        //    //    string tagName = "";
        //    //    if (goi.groupIndex >= 0 && goi.groupIndex < ecd.gameObjectInfoGroups.Count)
        //    //    {
        //    //        var gog = ecd.gameObjectInfoGroups[goi.groupIndex];
        //    //        parent = gog.transform;
        //    //        tagName = gog.tagName;
        //    //    }
        //    //    else
        //    //    {
        //    //        parent = commonContext.editorSceneGos[goi.prefabType].transform.Find (goi.tagName);
        //    //        tagName = goi.tagName;
        //    //    }
        //    //    if (tagName == sceneContext.suffix && !replaceLocal)
        //    //    {
        //    //        continue;
        //    //    }
        //    //    if (goi.prefabIndex >= 0 && goi.prefabIndex < ecd.prefabInfos.Count)
        //    //    {
        //    //        var prefabInfo = ecd.prefabInfos[goi.prefabIndex];
        //    //        GameObject go = CreateGameObject (ecd, goi, prefabInfo.prefab);
        //    //        if (go != null)
        //    //        {
        //    //            go.transform.parent = parent;
        //    //            OverrideSubPrefab (ecd, tagName, goi, go, prefabInfo.prefab);
        //    //        }
        //    //    }
        //    //    else
        //    //    {
        //    //        GameObject go = CreateGameObject (ecd, goi, null);
        //    //        if (go != null)
        //    //        {
        //    //            go.transform.parent = parent;
        //    //        }
        //    //    }
        //    //}

        //    ////instance
        //    //Transform instanceTrans = commonContext.editorSceneGos[(int) EditorSceneObjectType.Instance].transform;
        //    //for (int i = 0; i < ecd.chunks.Count; ++i)
        //    //{
        //    //    var ec = ecd.chunks[i];
        //    //    for (int j = 0; j < ec.blocks.Length; ++j)
        //    //    {
        //    //        var block = ec.blocks[j];
        //    //        if (block != null)
        //    //        {
        //    //            for (int k = 0; k < block.instanceObjects.Count; ++k)
        //    //            {
        //    //                var instanceObject = block.instanceObjects[k];
        //    //                if (instanceObject.prefabIndex >= 0 && instanceObject.prefabIndex < ecd.prefabInfos.Count)
        //    //                {
        //    //                    var prefabInfo = ecd.prefabInfos[instanceObject.prefabIndex];
        //    //                    for (int xx = 0; xx < instanceObject.instanceInfo.Count; ++xx)
        //    //                    {
        //    //                        var prop = instanceObject.instanceInfo[xx];
        //    //                        Transform parent = null;
        //    //                        string tagName = "";
        //    //                        if (prop.groupIndex >= 0 && prop.groupIndex < ecd.gameObjectInfoGroups.Count)
        //    //                        {
        //    //                            var gog = ecd.gameObjectInfoGroups[prop.groupIndex];
        //    //                            parent = gog.transform;
        //    //                            tagName = gog.tagName;
        //    //                        }
        //    //                        else
        //    //                        {
        //    //                            parent = instanceTrans.Find (prop.tagName);
        //    //                            tagName = prop.tagName;
        //    //                        }
        //    //                        if (tagName == sceneContext.suffix && !replaceLocal)
        //    //                        {
        //    //                            continue;
        //    //                        }
        //    //                        GameObject go = PrefabUtility.InstantiatePrefab (prefabInfo.prefab) as GameObject;
        //    //                        go.name = prefabInfo.prefab.name;
        //    //                        go.transform.parent = parent;
        //    //                        go.transform.position = prop.pos;
        //    //                        go.transform.rotation = prop.rot;
        //    //                        go.transform.localScale = new Vector3 (prop.scale, prop.scale, prop.scale);

        //    //                        go.SetActive (prop.visible);
        //    //                        MeshRenderer mr = go.GetComponent<MeshRenderer> ();
        //    //                        if (mr != null)
        //    //                        {
        //    //                            mr.sharedMaterial = instanceObject.mat;
        //    //                        }
        //    //                    }
        //    //                }
        //    //            }
        //    //        }
        //    //    }
        //    //}
        //    ////lightprobes
        //    //var probes = GetTransform (EditorSceneObjectType.LightProbes);
        //    //if (probes != null)
        //    //{
        //    //    for (int z = 0; z < heightCount; ++z)
        //    //    {
        //    //        for (int x = 0; x < widthCount; ++x)
        //    //        {
        //    //            string name = string.Format ("LightProbeArea_{0}_{1}", x.ToString (), z.ToString ());
        //    //            var trans = probes.Find (name);
        //    //            if (trans == null)
        //    //            {
        //    //                GameObject go = new GameObject (name);
        //    //                trans = go.transform;
        //    //                trans.parent = probes;
        //    //            }
        //    //            //to do
        //    //        }
        //    //    }
        //    //}
        //    ////animation
        //    //var animations = GetTransform (EditorSceneObjectType.Animation);
        //    //if (animations != null)
        //    //{
        //    //    int id = 0;
        //    //    for (int i = 0; i < ecd.animationDatas.Count; ++i)
        //    //    {
        //    //        var ad = ecd.animationDatas[i];
        //    //        if (ad is LoopMoveData)
        //    //        {
        //    //            var lmd = ad as LoopMoveData;
        //    //            string animName = string.IsNullOrEmpty (ad.exString) ? string.Format ("", id++.ToString ()) : ad.exString;
        //    //            GameObject go = new GameObject (animName);
        //    //            go.transform.parent = animations;

        //    //            // var loopMove = go.AddComponent<LoopMove> ();
        //    //            // loopMove.exString = lmd.exString;
        //    //            // loopMove.duration = lmd.duration;
        //    //            // loopMove.autoPlay = lmd.autoPlay;
        //    //            // loopMove.moveSpeed = lmd.moveSpeed;

        //    //            //var startPoint = new GameObject ("start").transform;
        //    //            // startPoint.parent = go.transform;
        //    //            // startPoint.position = lmd.start;
        //    //            // var endPoint = new GameObject ("end").transform;
        //    //            // endPoint.parent = go.transform;
        //    //            // endPoint.position = lmd.end;
        //    //            // for (int j = 0; j < lmd.objects.Count; ++j)
        //    //            // {
        //    //            //     var target = GameObject.Find (lmd.objects[j]);
        //    //            //     if (target != null)
        //    //            //     {
        //    //            //         Renderer r;
        //    //            //         target.TryGetComponent (out r);
        //    //            //         loopMove.objects.Add (new ObjectCache ()
        //    //            //         {
        //    //            //             t = target.transform,
        //    //            //                 r = r
        //    //            //         });
        //    //            //     }
        //    //            // }
        //    //            // loopMove.uvMoveX = lmd.uvMove.x;
        //    //            // loopMove.uvMoveY = lmd.uvMove.y;

        //    //            // for (int j = 0; j < lmd.matobjects.Count; ++j)
        //    //            // {
        //    //            //     var target = GameObject.Find (lmd.matobjects[j]);
        //    //            //     if (target != null)
        //    //            //     {
        //    //            //         if (target.TryGetComponent (out MeshRenderObject mro))
        //    //            //         {
        //    //            //             loopMove.matObjects.Add (mro);
        //    //            //         }
        //    //            //     }
        //    //            // }
        //    //        }
        //    //    }
        //    //}
        //    ////ChunkLightData cld = AssetDatabase.LoadAssetAtPath<ChunkLightData> (
        //    ////    string.Format ("{0}/{1}_ChunkLightData.asset", sceneContext.configDir, sceneContext.name));
        //    ////if (cld != null)
        //    ////{
        //    ////    GameObject lightGo = commonContext.editorSceneGos[(int) EditorSceneObjectType.Light];
        //    ////    LightGroup lg = lightGo.GetComponent<LightGroup> ();
        //    ////    if (lg == null)
        //    ////    {
        //    ////        lg = lightGo.AddComponent<LightGroup> ();
        //    ////    }
        //    ////    lg.cld = cld;
        //    ////}

        //    //// if (string.IsNullOrEmpty (configName))
        //    //// {
        //    ////     Transform t = commonContext.editorSceneGos[(int) EditorSceneObjectType.MeshTerrain].transform;
        //    ////     for (int i = 0; i < t.childCount; ++i)
        //    ////     {
        //    ////         Transform child = t.GetChild (i);

        //    ////         TerrainObject to = child.GetComponent<TerrainObject> ();
        //    ////         if (to != null)
        //    ////         {
        //    ////             int chunkID = to.chunkID;
        //    ////             if (chunkID >= 0 && chunkID < ecd.chunks.Count)
        //    ////             {
        //    ////                 var theirEc = ecd.chunks[i];
        //    ////                 to.Copy (theirEc.terrainObjData);
        //    ////             }
        //    ////         }
        //    ////     }
        //    //// }

        //}

        //private void MergeScene (string configName, SceneMergeType type)
        //{
        //    switch (type)
        //    {
        //        case SceneMergeType.Object:
        //            MergeObject (configName);
        //            break;
        //        case SceneMergeType.HeightMap:
        //            MergeHeightMap (configName);
        //            break;
        //        case SceneMergeType.AlphaMap:
        //            MergeAlphaMap (configName);
        //            break;
        //    }
        //}

        //private bool CompareTimeStamp (Vector3Int theirTime0, Vector3Int theirTime1,
        //    Vector3Int ourTime0, Vector3Int ourTime1)
        //{
        //    if (theirTime0 == Vector3Int.zero)
        //    {
        //        theirTime0 = new Vector3Int (2018, 1, 1);
        //    }
        //    DateTime theirLightMapTimeStamp = new DateTime (
        //        theirTime0.x, theirTime0.y, theirTime0.z,
        //        theirTime1.x, theirTime1.y, theirTime1.z);

        //    if (ourTime0 == Vector3Int.zero)
        //    {
        //        ourTime0 = new Vector3Int (2018, 1, 1);
        //    }
        //    DateTime ourLightMapTimeStamp = new DateTime (
        //        ourTime0.x, ourTime0.y, ourTime0.z,
        //        ourTime1.x, ourTime1.y, ourTime1.z);
        //    return DateTime.Compare (theirLightMapTimeStamp, ourLightMapTimeStamp) > 0;
        //}
        ///////////////////////////Objects///////////////////////////


        //private void MergeObject (string configName)
        //{
        //    BeginEdit (true);

        //    EditorChunkData ecd;
        //    SceneSerialize.LoadEditorChunkData (ref sceneContext, configName, false, out ecd);
        //    if (ecd != null)
        //    {
        //        for (int i = (int) EditorSceneObjectType.Light; i <= (int) EditorSceneObjectType.Instance; ++i)
        //        {
        //            Transform trans = commonContext.editorSceneGos[i].transform.Find (configName);
        //            if (trans == null)
        //            {
        //                GameObject go = new GameObject (configName);
        //                go.transform.parent = commonContext.editorSceneGos[i].transform;
        //                trans = go.transform;
        //            }
        //            EditorCommon.DestroyChildObjects (trans.gameObject);
        //        }
        //        LoadFromEditorChunkData (ecd, configName);
        //        SceneAssets.SceneModify ();
        //    }
        //}

        #endregion

        ///////////////////////////Lightmap///////////////////////////

        #region Preview

        private void FocusChunk(int x, int z)
        {
            string chunkName = string.Format("{0}{1}_{2}", AssetsConfig.instance.SceneChunkStr, x, z);
            GameObject chunk = GameObject.Find(chunkName);
            if (chunk != null)
            {
                EditorGUIUtility.PingObject(chunk);
                Selection.activeGameObject = chunk;
                MeshFilter mf = chunk.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    if (SceneView.lastActiveSceneView != null && SceneView.lastActiveSceneView.camera != null)
                    {
                        Transform t = SceneView.lastActiveSceneView.camera.transform;
                        // Vector3 dir = -t.forward;

                        Bounds bound = mf.sharedMesh.bounds;
                        Vector3 center = bound.center;
                        Vector3 size = bound.size;
                        if (size.y == 0)
                            size.y = 0.1f;

                        center.x = (x + 0.5f) * chunkWidth;
                        center.z = (z + 0.5f) * chunkHeight;
                        SceneView.lastActiveSceneView.LookAt(center);
                    }
                }
            }
        }

        private void CalcQuadTree()
        {
            if (sceneLocalConfig != null)
            {
                if (sceneLocalConfig.chunkEndPoint.x >= 0)
                {
                    int width = sceneLocalConfig.chunkEndPoint.x - sceneLocalConfig.chunkSrcPoint.x + 1;
                    int height = sceneLocalConfig.chunkEndPoint.y - sceneLocalConfig.chunkSrcPoint.y + 1;
                    int blockCount = width * height;
                    quadTreeCount = blockCount;
                    List<MeshRenderObject> objInfo = new List<MeshRenderObject>();
                    EditorCommon.EnumTransform funPrefabs = null;
                    funPrefabs = (trans, param) =>
                    {
                        QuadTreeContext treeContext = param as QuadTreeContext;
                        objInfo.Clear();
                        trans.GetComponentsInChildren<MeshRenderObject>(true, objInfo);
                        if (objInfo.Count > 0)
                        {
                            for (int i = 0; i < objInfo.Count; ++i)
                            {
                                // var oc = objInfo[i];
                                // if (oc.IsRenderValid () &&
                                //     oc.chunkInfo.chunkID == treeContext.chunkId)
                                //     SceneQuadTree.Add2QuadTree (treeContext, oc, oc.bounds);
                            }
                        }
                        else
                        {
                            EditorCommon.EnumChildObject(trans, param, funPrefabs);
                        }
                    };

                    EditorCommon.EnumTransform funInstance = null;
                    funInstance = (trans, param) =>
                    {
                        var pc = param as PreviewContext;
                        MeshRenderer meshRenderer = trans.GetComponent<MeshRenderer>();
                        if (meshRenderer != null)
                        {
                            if (meshRenderer.sharedMaterial != null && meshRenderer.enabled &&
                                meshRenderer.gameObject.activeInHierarchy)
                            {
                                Bounds aabb = meshRenderer.bounds;
                                MeshFilter meshFilter = trans.GetComponent<MeshFilter>();
                                if (meshFilter != null && meshFilter.sharedMesh != null)
                                {
                                    aabb = MeshAssets.CalcBounds(meshFilter.sharedMesh, trans.localToWorldMatrix);
                                }

                                Vector3 pos = aabb.center;
                                int x;
                                int z;
                                int index = SceneQuadTree.FindChunkIndex(pos, chunkWidth, widthCount, heightCount,
                                    out x, out z);
                                Vector2 chunkCorner = new Vector4(x * chunkWidth, z * chunkHeight);
                                //Vector2 chunkCenter = new Vector2 ((x + 0.5f) * chunkWidth, (z + 0.5f) * chunkHeight);
                                float halfWidth = chunkWidth * 0.5f;
                                // float halfHeight = chunkHeight * 0.5f;
                                int subIndex = SceneQuadTree.FindChunkIndex(
                                    new Vector3(pos.x - chunkCorner.x, 0, pos.z - chunkCorner.y),
                                    halfWidth, 2, 2, out x, out z);

                                InstanceBox[] boxes;
                                if (!pc.instanceBox.TryGetValue(index, out boxes))
                                {
                                    boxes = new InstanceBox[4];
                                    pc.instanceBox.Add(index, boxes);
                                }

                                InstanceBox box = boxes[subIndex];
                                if (box == null)
                                {
                                    box = new InstanceBox();
                                    box.aabb.Init(ref aabb);
                                    boxes[subIndex] = box;
                                }
                                else
                                {
                                    box.aabb.Encapsulate(ref aabb);
                                }
                            }
                        }
                        else
                        {
                            EditorCommon.EnumChildObject(trans, param, funInstance);
                        }
                    };
                    previewContext.instanceBox.Clear();
                    string path = AssetsConfig.EditorGoPath[0] + "/" +
                                  AssetsConfig.EditorGoPath[(int) EditorSceneObjectType.Instance];
                    EditorCommon.EnumTargetObject(path, (trans, param) => { funInstance(trans, previewContext); });

                    int count = 0;
                    Transform meshTerrain = TerrainSystem.system.GetMeshTerrain();
                    for (int z = sceneLocalConfig.chunkSrcPoint.y; z <= sceneLocalConfig.chunkEndPoint.y; ++z)
                    {
                        for (int x = sceneLocalConfig.chunkSrcPoint.x; x <= sceneLocalConfig.chunkEndPoint.x; ++x)
                        {
                            int index = z * widthCount + x;
                            QuadTreeContext quadTreeContext = null;
                            if (count < quadTreeCache.Count)
                            {
                                quadTreeContext = quadTreeCache[count];
                            }
                            else
                            {
                                quadTreeContext = new QuadTreeContext();
                                quadTreeCache.Add(quadTreeContext);
                            }

                            Vector3 chunkOffset = new Vector3(x * chunkWidth, 0, z * chunkHeight);
                            SceneQuadTree.InitQuadTree(quadTreeContext, chunkWidth, chunkHeight, ref chunkOffset);
                            quadTreeContext.chunkId = index;

                            string chunkName = string.Format("Chunk_{0}_{1}", x, z);
                            TerrainObject to =
                                TerrainSystem.system.FindMeshTerrain<TerrainObject>(meshTerrain, chunkName);
                            if (to != null && to.terrainObjData.isValid)
                            {
                                MeshFilter mf = to.GetComponent<MeshFilter>();
                                if (mf != null && mf.sharedMesh != null)
                                {
                                    Bounds bound = mf.sharedMesh.bounds;
                                    Vector3 center = bound.center;
                                    Vector3 size = bound.size;
                                    if (size.y == 0)
                                        size.y = 0.1f;

                                    center.x = (x + 0.5f) * chunkWidth;
                                    center.z = (z + 0.5f) * chunkHeight;
                                    bound.center = center;
                                    bound.size = size;
                                    EditorQuardTreeNode node = quadTreeContext.treeNodes[0];
                                    node.aabb = AABB.Create(bound);
                                }
                            }

                            path = AssetsConfig.EditorGoPath[0] + "/" +
                                   AssetsConfig.EditorGoPath[(int) EditorSceneObjectType.StaticPrefab];
                            EditorCommon.EnumTargetObject(path,
                                (trans, param) => { funPrefabs(trans, quadTreeContext); });
                            path = AssetsConfig.EditorGoPath[0] + "/" +
                                   AssetsConfig.EditorGoPath[(int) EditorSceneObjectType.Prefab];
                            EditorCommon.EnumTargetObject(path,
                                (trans, param) => { funPrefabs(trans, quadTreeContext); });
                            InstanceBox[] boxes;
                            if (previewContext.instanceBox.TryGetValue(index, out boxes))
                            {
                                for (int i = 0; i < boxes.Length; ++i)
                                {
                                    var box = boxes[i];
                                    if (box != null)
                                    {
                                        int chunkLevel;

                                        SceneQuadTree.CalcBlockId(box.aabb.size, box.aabb.center, x, z, chunkWidth,
                                            chunkHeight, out chunkLevel, out box.blockId);
                                        SceneQuadTree.Add2QuadTree(quadTreeContext, box, box.aabb);
                                    }
                                }
                            }

                            SceneQuadTree.EndQuadTree(quadTreeContext);
                            ++count;
                        }
                    }
                }
                else
                {
                    quadTreeCount = 0;
                }
            }
        }

        void PreviewCollisionVoxel()
        {
            if (!collisionContext.gizmoDraw)
            {
                var cb = GetPreviewCB(ref cbContext.collisionVexelCB, false);
                AddCB2SceneView(cb, drawCollisionVoxel);
            }
        }

        void PreviewChunkLevel()
        {
            CommandBuffer cb = GetPreviewCB(ref cbContext.chunkLodLevelCB, false);
            AddCB2SceneView(cb, drawLodLevelBox);
            SceneView.RepaintAll();
        }

        #endregion

        #region Tool

        void FillSelectChunks()
        {
            commonContext.selectChunks.Clear();
            commonContext.selectChunksMap.Clear();
            for (int z = sceneLocalConfig.chunkSrcPoint.y; z <= sceneLocalConfig.chunkEndPoint.y; ++z)
            {
                for (int x = sceneLocalConfig.chunkSrcPoint.x; x <= sceneLocalConfig.chunkEndPoint.x; ++x)
                {
                    string chunkName = string.Format("{0}{1}_{2}", AssetsConfig.instance.SceneChunkStr, x, z);
                    GameObject chunk = GameObject.Find(chunkName);
                    if (chunk != null)
                    {
                        TerrainObject to = chunk.GetComponent<TerrainObject>();
                        if (to != null)
                        {
                            commonContext.selectChunks.Add(to);
                            commonContext.selectChunksMap.Add(to.chunkID, to);
                        }
                    }
                }
            }
        }

        // class MergeObjectInfo
        // {
        //     public Material mat;
        //     public Bounds aabb;
        //     public int chunkId;
        //     public int blockId;
        //     public Transform parent;
        //     public GameObject go;

        // }
        // class MergeGroupInfo
        // {
        //     public Material mat;
        //     public Bounds aabb;
        //     public List<MergeObjectInfo> objects = new List<MergeObjectInfo> ();

        //     public MergeGroupInfo targetGroup = null;
        //     public float weight = float.MaxValue;
        //     public bool overlap = false;
        // }

        // class MergeChunkInfo
        // {
        //     public List<MergeGroupInfo> groups = new List<MergeGroupInfo> ();
        // }

        // class MergeSceneInfo
        // {
        //     // public List<MergeObjectInfo> totalObjects = new List<MergeObjectInfo> ();
        //     public List<MergeChunkInfo> mergeChunks = new List<MergeChunkInfo> ();
        //     // public List<MergeObjectInfo> unMergeObjects = new List<MergeObjectInfo> ();
        // }

        // float sizeThreshold = 10;

        // void Add2Group(MergeObjectInfo obj, MergeChunkInfo chunk, int index, bool intersect, float dist)
        // {
        //     var group = chunk.groups[index];
        //     var oringG = obj.groupInfo;
        //     if (oringG != null)
        //     {
        //         if (obj.isIntersect && intersect)
        //         {
        //             if (dist < obj.dist)
        //             {

        //             }
        //         }
        //     }
        //     g.aabb.Encapsulate(obj.aabb);
        // }
        // bool AddToGroup (MergeObjectInfo obj, MergeChunkInfo chunk)
        // {
        //     obj.findCount++;
        //     float minDist = 10000;
        //     float minIntersectDist = 10000;
        //     int mostIntersectCloseGroup = -1;
        //     int mostCloseGroup = -1;
        //     for (int i = 0; i < chunk.groups.Count; ++i)
        //     {
        //         var group = chunk.groups[i];
        //         if (obj.groupInfo != group && obj.mat == group.mat && obj.chunkId == group.chunkId && obj.blockId == group.blockId)
        //         {
        //             float score = Vector3.Distance (obj.aabb.center, group.aabb.center);
        //             if (group.aabb.Intersects (obj.aabb))
        //             {
        //                 if (score < minIntersectDist)
        //                 {
        //                     minIntersectDist = score;
        //                     mostIntersectCloseGroup = i;
        //                 }
        //             }
        //             else
        //             {
        //                 if (score < minDist)
        //                 {
        //                     minDist = score;
        //                     mostCloseGroup = i;
        //                 }
        //             }
        //         }
        //     }
        //     MergeGroupInfo g = null;
        //     if (mostIntersectCloseGroup >= 0)
        //     {
        //         g = chunk.groups[mostIntersectCloseGroup];

        //         var oringG = obj.groupInfo;
        //         if (oringG != null)
        //         {

        //         }
        //         g.aabb.Encapsulate (obj.aabb);
        //         return true;
        //     }
        //     else if (mostCloseGroup >= 0 && minDist < sizeThreshold)
        //     {
        //         g = chunk.groups[mostCloseGroup];
        //         g.aabb.Encapsulate (obj.aabb);
        //         return true;
        //     }
        //     else
        //     {
        //         g = new MergeGroupInfo ()
        //         {
        //             mat = obj.mat,
        //             chunkId = obj.chunkId,
        //             blockId = obj.blockId,
        //             aabb = obj.aabb
        //         };

        //         chunk.groups.Add (g);
        //         return true;
        //     }

        // }

        // void GroupStaticBatch ()
        // {
        //     // EditorCommon.EnumTransform funPrefabs = null;
        //     // funPrefabs = (trans, param) =>
        //     // {
        //     //     SaveChunkContext scc = param as SaveChunkContext;
        //     //     scc.objInfo.Clear ();
        //     //     trans.GetComponentsInChildren<ObjectCombine> (true, scc.objInfo);
        //     //     if (scc.objInfo.Count > 0)
        //     //     {
        //     //         for (int i = 0; i < scc.objInfo.Count; ++i)
        //     //         {
        //     //             var oc = scc.objInfo[i];
        //     //             if (oc.GetRenderer ().enabled && oc.GetRenderer ().gameObject.activeInHierarchy)
        //     //             {
        //     //                 MeshFilter mf = oc.GetRenderer ().GetComponent<MeshFilter> ();
        //     //                 Mesh m = mf != null ? mf.sharedMesh : null;
        //     //                 if (m != null)
        //     //                 {
        //     //                     Vector3 pos = oc.bounds.center;
        //     //                     int x = oc.chunkID % widthCount;
        //     //                     int z = oc.chunkID / widthCount;

        //     //                     string chunkName = string.Format ("Chunk_{0}_{1}", x, z);
        //     //                     Transform meshTerrain = scc.editorSceneGos[(int) EditorSceneObjectType.MeshTerrain].transform;
        //     //                     TerrainObject to = FindMeshTerrain<TerrainObject> (meshTerrain, chunkName);
        //     //                     if (to != null)
        //     //                     {
        //     //                         Vector2 chunkCorner = new Vector4 (x * chunkWidth, z * chunkHeight);
        //     //                         float halfWidth = chunkWidth * 0.5f;
        //     //                         float halfHeight = chunkHeight * 0.5f;
        //     //                         int subIndex = SceneQuadTree.FindChunkIndex (new Vector3 (pos.x - chunkCorner.x, 0, pos.z - chunkCorner.y),
        //     //                             halfWidth, halfHeight, 2, 2, out x, out z);

        //     //                         BatchGroupInfo batchGroup = to.GetBatchGroup (subIndex);
        //     //                         batchGroup.mergeGos.Add (new StaticBatchInfo ()
        //     //                         {
        //     //                             gameObject = oc.gameObject,
        //     //                                 meshFilter = mf,
        //     //                                 meshRenderer = oc.GetRenderer () as MeshRenderer,
        //     //                                 srcMesh = m,
        //     //                                 objCombine = oc
        //     //                         });
        //     //                     }
        //     //                 }
        //     //             }
        //     //         }
        //     //     }
        //     //     else
        //     //     {
        //     //        EditorCommon.EnumChildObject (trans, param, funPrefabs);
        //     //     }
        //     // };

        //     Transform t = commonContext.editorSceneGos[(int) EditorSceneObjectType.MeshTerrain].transform;
        //     for (int i = 0; i < t.childCount; ++i)
        //     {
        //         Transform child = t.GetChild (i);

        //         TerrainObject to = child.GetComponent<TerrainObject> ();
        //         if (to != null)
        //         {
        //             to.InitBatchGroup ();
        //         }
        //     }

        //     // string path = AssetsConfig.EditorGoPath[0] + "/" + AssetsConfig.EditorGoPath[(int) EditorSceneObjectType.StaticPrefab];
        //     //EditorCommon.EnumTargetObject (path, (trans, param) =>
        //     // {
        //     //     funPrefabs (trans, saveChunkContext);
        //     // });
        //     // path = AssetsConfig.EditorGoPath[0] + "/" + AssetsConfig.EditorGoPath[(int) EditorSceneObjectType.Prefab];
        //     //EditorCommon.EnumTargetObject (path, (trans, param) =>
        //     // {
        //     //     funPrefabs (trans, saveChunkContext);
        //     // });
        // }

        // class MergeList
        // {
        //     public List<CombineInstance> lod0 = new List<CombineInstance> ();
        //     public List<CombineInstance> lod1 = new List<CombineInstance> ();
        // }
        // void MergeBlock (TerrainObject to, EditorChunkData.EditorChunk chunk)
        // {
        //     int xOffset = (to.chunkID % widthCount) * chunkWidth;
        //     int zOffset = (to.chunkID / heightCount) * chunkHeight;
        //     Vector3 offset = new Vector3 (xOffset, 0, zOffset);

        //     Dictionary<string, ChunkLightmapData.LightmapInfo> objLightMapCache = new Dictionary<string, ChunkLightmapData.LightmapInfo> ();
        //     for (int i = 0; i < chunk.sceneObjLightmapInfo.Count; ++i)
        //     {
        //         ChunkLightmapData.LightmapInfo li = chunk.sceneObjLightmapInfo[i];
        //         objLightMapCache[li.fileID] = li;
        //     }
        //     Dictionary<byte, MergeList> mergeInfos = new Dictionary<byte, MergeList> ();
        //     for (int i = 0; i < to.batchGroup.Count && i < 4; ++i)
        //     {
        //         var mergeGroup = to.batchGroup[i];
        //         var objBlock = chunk.blocks[i];
        //         if (objBlock == null)
        //         {
        //             objBlock = new EditorChunkData.ObjectBlock ();
        //             chunk.blocks[i] = objBlock;
        //         }
        //         for (int j = 0; j < mergeGroup.mergeMats.Count; ++j)
        //         {
        //             var mergeMat = mergeGroup.mergeMats[j];
        //             if (mergeMat.mergeMeshs.Count > 0)
        //             {
        //                 mergeInfos.Clear ();
        //                 for (int k = 0; k < mergeMat.mergeMeshs.Count; ++k)
        //                 {
        //                     var mergeMesh = mergeMat.mergeMeshs[k];
        //                     if (mergeMesh.mergeRenders.Count > 1)
        //                     {
        //                         for (int xx = 0; xx < mergeMesh.mergeRenders.Count; ++xx)
        //                         {
        //                             var mri = mergeMesh.mergeRenders[xx];
        //                             if (!mri.isNoMergeMesh)
        //                             {
        //                                 var trans = mri.render.transform;
        //                                 Vector3 pos = trans.position - offset;
        //                                 Quaternion rot = trans.rotation;
        //                                 Vector3 scale = trans.lossyScale;
        //                                 Matrix4x4 matrix = Matrix4x4.TRS (pos, rot, scale);
        //                                 ChunkLightmapData.LightmapInfo li;
        //                                 Vector4 lightmapST = new Vector4 (1, 1, 0, 0);
        //                                 byte lightmapIndex = 255;
        //                                 if (objLightMapCache.TryGetValue (mri.fileID, out li))
        //                                 {
        //                                     lightmapST = li.lightmapST;
        //                                     lightmapIndex = li.lightmapIndex;
        //                                 }
        //                                 MergeList mergeList;
        //                                 if (!mergeInfos.TryGetValue (lightmapIndex, out mergeList))
        //                                 {
        //                                     mergeList = new MergeList ();
        //                                     mergeInfos[lightmapIndex] = mergeList;
        //                                 }
        //                                 CombineInstance ci = new CombineInstance () { mesh = mergeMesh.mesh, subMeshIndex = 0, transform = matrix, lightmapScaleOffset = lightmapST };
        //                                 mergeList.lod0.Add (ci);
        //                                 if (!mri.isSmallMesh)
        //                                 {
        //                                     mergeList.lod1.Add (ci);
        //                                 }
        //                             }

        //                         }
        //                     }
        //                 }

        //                 if (mergeInfos.Count > 0)
        //                 {
        //                     var it = mergeInfos.GetEnumerator ();

        //                     while (it.MoveNext ())
        //                     {
        //                         // var boi = new EditorChunkData.BlockObjectInfo ();
        //                         // objBlock.objects.Add (boi);
        //                         // boi.blockIndex = i;
        //                         // boi.mat = mergeMat.material;
        //                         // boi.lightmapIndex = it.Current.Key;
        //                         // var MergeList = it.Current.Value;
        //                         // if (MergeList.lod0.Count > 1)
        //                         // {
        //                         //     Mesh mesh = new Mesh ();
        //                         //     mesh.name = string.Format ("M_{0}_{1}_{2}_{3}", to.chunkID, i, index++, boi.mat.name);
        //                         //     mesh.CombineMeshes (MergeList.lod0.ToArray (), true, true, true);
        //                         //     mesh.colors = null;
        //                         //     mesh.uv3 = null;
        //                         //     mesh.UploadMeshData (true);
        //                         //     MeshUtility.SetMeshCompression (mesh, ModelImporterMeshCompression.Medium);
        //                         //     string dir = string.Format ("{0}{1}", AssetsConfig.GlobalAssetsConfig.ResourcePath, AssetsConfig.GlobalAssetsConfig.EditorSceneRes);
        //                         //     Mesh newMesh = CommonAssets.CreateAsset<Mesh> (
        //                         //         dir,
        //                         //         mesh.name, ".asset", mesh);
        //                         //     mergeMat.mergeMesh = newMesh;
        //                         //     boi.mesh = newMesh;

        //                         //     if (MergeList.lod1.Count > 1)
        //                         //     {
        //                         //         Mesh meshL0 = new Mesh ();
        //                         //         meshL0.name = string.Format ("M_{0}_{1}_{2}_{3}_L1", to.chunkID, i, index++, boi.mat.name);
        //                         //         meshL0.CombineMeshes (MergeList.lod1.ToArray (), true, true, true);
        //                         //         meshL0.colors = null;
        //                         //         meshL0.uv4 = null;
        //                         //         meshL0.uv3 = null;
        //                         //         meshL0.uv2 = null;
        //                         //         meshL0.UploadMeshData (true);
        //                         //         MeshUtility.SetMeshCompression (meshL0, ModelImporterMeshCompression.High);
        //                         //         CommonAssets.CreateAsset<Mesh> (
        //                         //             dir,
        //                         //             meshL0.name, ".asset", meshL0);
        //                         //         // var meshLOD = new ModelImporterLODGenerator.MeshLOD();
        //                         //         // meshLOD.inputMesh = meshL0;
        //                         //         // meshLOD.outputMesh = new Mesh();
        //                         //         // meshLOD.quality = 0.75f;
        //                         //         // meshLOD.meshSimplifierType = typeof(QuadricMeshSimplifier);
        //                         //         // ModelImporterLODGenerator.GenerateMeshLOD(meshLOD, null);

        //                         //         // meshLOD.outputMesh.name = meshL0.name + "_L1";
        //                         //         // meshLOD.outputMesh.UploadMeshData(true);
        //                         //         // MeshUtility.SetMeshCompression(meshLOD.outputMesh, ModelImporterMeshCompression.High);
        //                         //         // CommonAssets.CreateAsset<Mesh>(
        //                         //         //     string.Format("{0}{1}",
        //                         //         //         AssetsConfig.GlobalAssetsConfig.ResourcePath,
        //                         //         //         AssetsConfig.GlobalAssetsConfig.EditorSceneRes),
        //                         //         //     meshLOD.outputMesh.name, ".asset", meshLOD.outputMesh);
        //                         //     }

        //                         // }
        //                         // else
        //                         // {
        //                         //     // CombineInstance ci = combineInfo[0];
        //                         //     // moi.mesh = ci.mesh;
        //                         // }
        //                     }

        //                 }
        //             }
        //         }
        //     }
        //     DateTime now = DateTime.Now;
        //     chunk.bergeBlockTimeStamp0 = new Vector3Int (now.Year, now.Month, now.Day);
        //     chunk.bergeBlockTimeStamp1 = new Vector3Int (now.Hour, now.Minute, now.Second);
        // }

        // void MergeBlocks ()
        // {
        //     var ecd = LoadEditorChunkData ();
        //     if (mergeSelect)
        //     {
        //         // string targetSceneDir = string.Format ("{0}/Scene/{1}",
        //         //     AssetsConfig.GlobalAssetsConfig.ResourcePath, sceneContext.name);
        //         // HashSet<int> preprocessMeshes = new HashSet<int> ();

        //         for (int i = 0; i < saveChunkContext.selectTos.Count; ++i)
        //         {
        //             TerrainObject to = saveChunkContext.selectTos[i];
        //             if (to != null)
        //             {
        //                 MergeBlock (to, ecd.chunks[to.chunkID]);
        //             }
        //         }
        //     }
        //     else
        //     {
        //         Transform t = commonContext.editorSceneGos[(int) EditorSceneObjectType.MeshTerrain].transform;
        //         for (int i = 0; i < t.childCount; ++i)
        //         {
        //             Transform child = t.GetChild (i);

        //             TerrainObject to = child.GetComponent<TerrainObject> ();
        //             if (to != null)
        //             {
        //                 MergeBlock (to, ecd.chunks[to.chunkID]);
        //             }
        //         }

        //     }

        //     SaveEditorChunkData ();
        // }

        // void StaticBatch (List<TerrainObject> selectTos = null, bool isStaticBatch = true)
        // {
        //     List<StaticBatchInfo> gos = new List<StaticBatchInfo> ();
        //     List<GameObject> gameObjects = new List<GameObject> ();
        //     if (selectTos == null)
        //         selectTos = saveChunkContext.selectTos;
        //     string chunkMeshDir = string.Format ("{0}/Scene/{1}", AssetsConfig.GlobalAssetsConfig.ResourcePath, sceneContext.name);
        //     for (int ii = 0; ii < selectTos.Count; ++ii)
        //     {
        //         var to = selectTos[ii];
        //         int staticMeshCount = 0;
        //         if (isStaticBatch)
        //         {
        //             EditorUtility.DisplayProgressBar (string.Format ("StaticBatch-{0}/{1}", ii, selectTos.Count), "", (float) ii / selectTos.Count);
        //             gos.Clear ();
        //             int groupMeshCount = 0;
        //             for (int i = 0; i < to.batchGroup.Count; ++i)
        //             {

        //                 var group = to.batchGroup[i];
        //                 if (group.mergeGos.Count > 1)
        //                 {
        //                     gameObjects.Clear ();
        //                     for (int j = 0; j < group.mergeGos.Count; ++j)
        //                     {
        //                         var sbi = group.mergeGos[j];
        //                         int meshCount;
        //                         if (group.meshCount.TryGetValue (sbi.srcMesh, out meshCount))
        //                         {
        //                             if (sbi.srcMesh.vertexCount < 500 && meshCount < 5)
        //                             {
        //                                 sbi.meshRenderer.lightmapScaleOffset = sbi.objCombine.lightmapUVST;
        //                                 gameObjects.Add (sbi.gameObject);
        //                                 gos.Add (group.mergeGos[j]);
        //                                 sbi.needBatch = true;
        //                             }
        //                             else
        //                             {
        //                                 sbi.needBatch = false;
        //                             }
        //                         }
        //                         else
        //                         {
        //                             sbi.needBatch = false;
        //                         }
        //                         sbi.objCombine.batchMeshIndex = -1;
        //                         sbi.objCombine.subMeshIndex = -1;

        //                     }
        //                     if (gameObjects.Count > 1)
        //                     {
        //                         StaticBatchingUtility.Combine (gameObjects.ToArray (), null);
        //                         for (int j = 0; j < group.mergeGos.Count; ++j)
        //                         {
        //                             var staticBatchInfo = group.mergeGos[j];
        //                             if (staticBatchInfo.needBatch)
        //                             {
        //                                 staticBatchInfo.staticBatchMesh = staticBatchInfo.meshFilter.sharedMesh;
        //                                 staticBatchInfo.objCombine.subMeshIndex = (short) staticBatchInfo.gameObject.GetComponent<MeshRenderer> ().subMeshStartIndex;
        //                                 string name = staticBatchInfo.staticBatchMesh.name;
        //                                 int batchMeshIndex = -1;
        //                                 if (name.StartsWith ("Combined Mesh"))
        //                                 {
        //                                     string staticBatchName = string.Format ("ChunkMesh_{0}_{1}", to.chunkID, staticMeshCount++);
        //                                     staticBatchInfo.staticBatchMesh.name = staticBatchName;
        //                                     Vector2[] uv2 = staticBatchInfo.staticBatchMesh.uv2;
        //                                     staticBatchInfo.staticBatchMesh.colors = null;
        //                                     staticBatchInfo.staticBatchMesh.uv4 = null;
        //                                     staticBatchInfo.staticBatchMesh.uv3 = null;
        //                                     staticBatchInfo.staticBatchMesh.bindposes = null;
        //                                     staticBatchInfo.staticBatchMesh.boneWeights = null;
        //                                     staticBatchInfo.staticBatchMesh.UploadMeshData (true);
        //                                     MeshUtility.SetMeshCompression (staticBatchInfo.staticBatchMesh, ModelImporterMeshCompression.Medium);
        //                                     CommonAssets.CreateAsset<Mesh> (chunkMeshDir,
        //                                         staticBatchInfo.staticBatchMesh.name, ".asset", staticBatchInfo.staticBatchMesh);
        //                                     groupMeshCount++;
        //                                     if (groupMeshCount >= 2)
        //                                     {
        //                                         Debug.LogErrorFormat ("more than 1 static batch mesh:{0} chunk:{1} block:{2}", staticBatchInfo.staticBatchMesh.name, to.chunkID, i);
        //                                     }
        //                                 }
        //                                 string batchName = staticBatchInfo.staticBatchMesh.name;
        //                                 if (batchName.StartsWith ("ChunkMesh_"))
        //                                 {
        //                                     int index = batchName.LastIndexOf ("_");
        //                                     string indexStr = batchName.Substring (index + 1);
        //                                     batchMeshIndex = int.Parse (indexStr);
        //                                 }
        //                                 if (batchMeshIndex >= 0)
        //                                 {
        //                                     staticBatchInfo.objCombine.batchMeshIndex = batchMeshIndex;
        //                                 }
        //                                 else
        //                                 {
        //                                     Debug.LogErrorFormat ("error static batch mesh:{0} chunk:{1} block:{2}", staticBatchInfo.staticBatchMesh.name, to.chunkID, i);
        //                                 }
        //                             }

        //                         }
        //                     }

        //                 }
        //             }
        //             if (gos.Count > 0)
        //             {
        //                 for (int j = 0; j < gos.Count; ++j)
        //                 {
        //                     StaticBatchInfo staticBatchInfo = gos[j];
        //                     staticBatchInfo.meshFilter.sharedMesh = staticBatchInfo.srcMesh;
        //                 }
        //             }
        //         }

        //         to.terrainObjData.batchMeshCount = (byte) staticMeshCount;
        //     }
        //     EditorUtility.ClearProgressBar ();
        // }
        // float CalcDistance (ClusterArea src, ClusterArea des, List<float> distances)
        // {

        //     float distance = float.MaxValue;
        //     for (int i = 0; i < src.coiList.Count; ++i)
        //     {
        //         var s = src.coiList[i];
        //         for (int j = 0; j < des.coiList.Count; ++j)
        //         {
        //             var d = des.coiList[j];
        //             int maxId = s.id > d.id ? s.id : d.id;
        //             int minId = s.id > d.id ? d.id : s.id;
        //             int disOffset = maxId * (maxId - 1) / 2 + minId;
        //             float dist = distances[disOffset];
        //             if (dist < distance)
        //             {
        //                 distance = dist;
        //             }
        //         }
        //     }
        //     return distance;
        // }
        // void CalcDistance (List<float> distances, List<ClusterArea> clusterAreas, Vector3 pos, float radius)
        // {
        //     for (int i = 0; i < clusterAreas.Count; ++i)
        //     {
        //         var coi = clusterAreas[i].coiList[0];
        //         float dis = Vector3.Distance (pos, coi.pos);
        //         dis -= radius + coi.size * 0.25f;
        //         if (dis < 0)
        //             dis = 0;
        //         distances.Add (dis);
        //     }
        // }
        // bool MergeCluster (List<ClusterArea> clusterAreas, int startIndex, List<float> distances, float clusterThreshold)
        // {
        //     ClusterArea ca = clusterAreas[startIndex];
        //     float distance = float.MaxValue;
        //     int minIndex = -1;
        //     for (int i = startIndex + 1; i < clusterAreas.Count; ++i)
        //     {
        //         var clusterArea = clusterAreas[i];
        //         float dist = CalcDistance (ca, clusterArea, distances);
        //         if (dist < distance)
        //         {
        //             distance = dist;
        //             minIndex = i;
        //         }
        //     }
        //     if (distance <= clusterThreshold)
        //     {
        //         var clusterArea = clusterAreas[minIndex];
        //         ca.coiList.AddRange (clusterArea.coiList);
        //         clusterAreas.RemoveAt (minIndex);
        //         return true;
        //     }
        //     return false;
        // }

        // void ClusterObject (int chunkIndex)
        // {
        //     if (mainChunkData != null)
        //     {
        //         EditorChunkData.EditorChunk ec = mainChunkData.chunks[chunkIndex];
        //         Bounds bound = editorChunkInfo[chunkIndex].chunkBound;
        //         Rect boundArea = new Rect (bound.min.x, bound.min.z, bound.size.x, bound.size.z);
        //         clusterAreas.Clear ();
        //         distances.Clear ();
        //         for (int i = 0; i < ec.gameObjectInfoGroups.Count; ++i)
        //         {
        //             var go = ec.gameObjectInfoGroups[i];
        //             if (go.path.Contains (AssetsConfig.EditorGoPath[(int) EditorSceneObjectType.StaticPrefab]))
        //             {
        //                 for (int j = 0; j < go.gameObjectInfos.Count; ++j)
        //                 {
        //                     var goi = go.gameObjectInfos[j];
        //                     goi.clusterID = -1;
        //                     if (!boundArea.Contains (new Vector2 (goi.pos.x, goi.pos.z)))
        //                     {
        //                         continue;
        //                     }
        //                     Bounds aabb = new Bounds (goi.pos, new Vector3 (0.1f, 0.1f, 0.1f));
        //                     for (int k = 0; k < goi.sceneObjects.Count; ++k)
        //                     {
        //                         var soi = goi.sceneObjects[k];
        //                         aabb.Encapsulate (soi.bound);
        //                     }

        //                     ClusterArea ca = new ClusterArea ();
        //                     ClusterObjInfo coi = new ClusterObjInfo ();
        //                     coi.pos = goi.pos;
        //                     coi.size = new Vector2 (aabb.size.x, aabb.size.z).magnitude;
        //                     coi.id = clusterAreas.Count;
        //                     coi.goi = goi;
        //                     goi.clusterID = clusterAreas.Count;
        //                     ca.coiList.Add (coi);
        //                     CalcDistance (distances, clusterAreas, goi.pos, coi.size * 0.25f);
        //                     clusterAreas.Add (ca);
        //                 }
        //             }

        //         }
        //         int mergeCount = (int) ec.clusterParam.y;
        //         for (int i = 0; i < mergeCount; ++i)
        //         {
        //             for (int j = 0; j < clusterAreas.Count; ++j)
        //             {
        //                 MergeCluster (clusterAreas, j, distances, ec.clusterParam.x);
        //             }
        //         }
        //         ec.clusterParam.z = clusterAreas.Count;
        //         for (int i = 0; i < clusterAreas.Count; ++i)
        //         {
        //             var ca = clusterAreas[i];
        //             for (int j = 0; j < ca.coiList.Count; ++j)
        //             {
        //                 ca.coiList[j].goi.clusterID = i;
        //             }
        //         }
        //         if (chunkIndex == drawClusterIndex)
        //         {
        //             PreviewCluster (chunkIndex, false);
        //         }
        //     }
        // }

        // void ClusterObject ()
        // {
        //     for (int z = 0; z < heightCount; ++z)
        //     {
        //         for (int x = 0; x < widthCount; ++x)
        //         {
        //             int index = z * widthCount + x;
        //             ClusterObject (index);
        //         }
        //     }
        // }

        // void PreviewCluster (int chunkIndex, bool clear = false)
        // {
        //     if (SceneView.lastActiveSceneView != null &&
        //         SceneView.lastActiveSceneView.camera != null &&
        //         mainChunkData != null)
        //     {
        //         if (previewContext.cb == null)
        //             previewContext.cb = new CommandBuffer () { name = "Temp CB" };
        //         previewContext.cb.Clear ();
        //         previewContext.renderList.Clear ();
        //         if (chunkIndex >= 0)
        //         {
        //             EditorChunkData.EditorChunk ec = mainChunkData.chunks[chunkIndex];

        //             for (int i = 0; i < ec.gameObjectInfoGroups.Count; ++i)
        //             {
        //                 var go = ec.gameObjectInfoGroups[i];
        //                 for (int j = 0; j < go.gameObjectInfos.Count; ++j)
        //                 {
        //                     var goi = go.gameObjectInfos[j];
        //                     if (goi.clusterID >= 0)
        //                     {
        //                         Color color = debugColor[goi.clusterID % debugColor.Count];
        //                         for (int k = 0; k < goi.sceneObjects.Count; ++k)
        //                         {
        //                             var soi = goi.sceneObjects[k];
        //                             Matrix4x4 worldMatrix = Matrix4x4.TRS (soi.pos, soi.rotate, soi.scale);
        //                             MaterialPropertyBlock mpb = new MaterialPropertyBlock ();
        //                             mpb.SetColor ("_Color", color);
        //                             previewContext.cb.DrawMesh (soi.mesh, worldMatrix, AssetsConfig.GlobalAssetsConfig.NumMat, 0, 0, mpb);
        //                         }
        //                     }
        //                 }
        //             }
        //         }
        //         if (clear)
        //         {
        //             SceneView.lastActiveSceneView.camera.RemoveAllCommandBuffers ();
        //         }
        //         if (chunkIndex == drawClusterIndex)
        //             SceneView.lastActiveSceneView.camera.AddCommandBuffer (CameraEvent.BeforeImageEffects, previewContext.cb);
        //         else
        //             SceneView.lastActiveSceneView.camera.RemoveCommandBuffer (CameraEvent.BeforeImageEffects, previewContext.cb);
        //         SceneView.RepaintAll ();
        //     }
        // }

        #endregion

        private static void BytesAddScene(string sceneName)
        {
            string path = string.Format("{0}Scene/{1}/{1}.bytes",
                LoadMgr.singleton.BundlePath,
                sceneName);
            BytesAddScene_Type(path, "Bytes_Scene", false);

            path = string.Format("{0}Scene/{1}/{1}_ds.bytes",
                LoadMgr.singleton.BundlePath,
                sceneName);
            BytesAddScene_Type(path, "Bytes_Scene_Dynamic", true);
        }

        private static void BytesAddScene_Type(string path, string name, bool isDs)
        {
            if (!File.Exists(path)) return;
            GameObject objRoot = GameObject.Find("EditorScene");
            if (objRoot == null) return;
            GameObject target = GameObject.Find("EditorScene/" + name);
            BytesRes bts = null;

            if (target == null)
            {
                target = new GameObject(name);
                target.transform.SetParent(objRoot.transform);
                bts = target.AddComponent<BytesRes>();
            }

            bts = target.GetComponent<BytesRes>();
            bts.bytes = null;

            if (!isDs)
            {
                Byte[] bytes = File.ReadAllBytes(path);
                bts.bytes = bytes;
                File.Delete(path);
            }
            else
            {
                bts.ta = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            }
        }

        [MenuItem("Tools/引擎/Bytes/Scene")]
        public static void BindBytesToScene()
        {
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                string path = scene.path;
                UnityEngine.SceneManagement.Scene openedScene = EditorSceneManager.OpenScene(path);
                BytesAddScene(openedScene.name);
                SceneAssets.SceneModify(true);
            }

            AssetDatabase.Refresh();
        }

        [InitializeOnLoadMethod]
        private static void RegisterEvents()
        {
            EditorApplication.playModeStateChanged += PlayModeChanged;
        }

        private static void PlayModeChanged(PlayModeStateChange stateChange)
        {
            if (stateChange == PlayModeStateChange.ExitingEditMode)
            {
                SceneBatchInstancing.ProcessCurrentScene();
            }
        }
    }
}
