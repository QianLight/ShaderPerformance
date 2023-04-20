#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
// using CFEngine.Game;
// using Unity.Collections;
using UnityEditor;
using UnityEngine;
// using UnityEngine.Timeline;

namespace CFEngine
{
    public delegate void OnEngineEvent ();
    public static class GlobalContex
    {
        public static EnvironmentExtra ee = null;
        public static int globalDebugMode = 0;

        public static TerrainData terrainData = null;
        public static GameObject terrainGo = null;
        public static int forceUpdateCount = 0;

        public static string currentSceneFolder = "";
        public static Queue<OnEngineEvent> updateOnceCb = new Queue<OnEngineEvent> ();
        public static List<OnEngineEvent> updateCb = new List<OnEngineEvent> ();

        public static void AddEngineEvent (OnEngineEvent cb, bool runOnce = true)
        {
            if (runOnce)
            {
                updateOnceCb.Enqueue (cb);
            }
            else
            {
                updateCb.Add (cb);
            }
        }
    }

    public enum QuadTreeLevel
    {
        None,
        Level0,
        Level1,
        Level2,
        Level3
    }

    public enum DrawType
    {
        Both,
        Draw,
        Cull,
    }

    public enum StatisticsType
    {
        None,
        SRP,
        Counter
    }
    public interface IMatObject
    {
        void Refresh (RenderingManager mgr);
        void OnDrawGizmo(EngineContext context);

        void SetAreaMask(uint area);
    }
    public delegate void OnDrawGizmoCb ();

    [DisallowMultipleComponent, ExecuteInEditMode]
    [RequireComponent (typeof (Camera))]
    public partial class EnvironmentExtra : MonoBehaviour
    {
        #region globalEffect
        public bool globalEffectFolder = false;
        public EnvProfile envProfile;
        public Camera uiCamera = null;
        public bool forceUpdate = false;
        private EngineContext context = null;
        #endregion

        #region testObj
        [NonSerialized]
        public SFXManager sfxManager;
        public bool testFolder = true;
        public bool debugEnvArea = false;
        #endregion

        #region fastRun
        public bool fastRunFolder = true;
        public bool loadGameAtHere = false;
        public bool gotoScene = false;
        public int sceneID = -1;
        public bool replaceStartScene = false;
        public bool useStaticBatch = false;
        public bool debugHeadData = false;
        public int debugChunkIndex = -1;
        public static readonly SavedInt frameRate = new SavedInt($"{nameof(EnvironmentExtra)}.{nameof(frameRate)}", 30);
        #endregion

        #region freeCamera
        public bool freeCameraFolder = true;
        static Texture2D ms_invisibleCursor;

        public bool forceUpdateFreeCamera = false;
        public bool forceIgnore = false;
        public bool holdRightMouseCapture = false;

        public float lookSpeed = 5f;
        public float moveSpeed = 5f;
        public float sprintSpeed = 50f;

        bool m_inputCaptured;
        float m_yaw;
        float m_pitch;
        #endregion

        #region lod

        #endregion
        #region ui
        public bool uiFolder = false;
        [NonSerialized]
        public bool uiBlur = false;
        [Range (1, 4)]
        public float uiBlurRTDebugSize = 1.0f;
        public RenderTexture uiBlurRT = null;

        #endregion

        #region debug
        public bool debugFolder = true;
        public bool bytesPrefabProgress = false;
        public bool testBytesExport = false;
        public bool drawFrustum = false;
        public bool disableSceneObject = false;
        public bool disableTerrainObject = false;
        public bool drawDynamicWall = false;

        public bool drawLodGrid = false;
        public bool drawCameraPos = false;
        public bool drawCacheChunk = false;
        public bool preiewLightProbe = false;
        [Range (-1, 3)]
        public int matLod = -1;
        public bool drawTerrainGrid = false;
        public bool drawCrossChunkPoint = false;
        public bool drawLookAtPoint = false;
        public bool previewDummyCamera = false;
        public bool previewDrawCall = false;
        public ELodLevel lodLevel = ELodLevel.None;

        public bool drawGlobalShadowObject = false;

        #region chunk objects
        public QuadTreeLevel quadLevel = QuadTreeLevel.None;
        public int quadIndex = -1;
        public DrawType drawType = DrawType.Both;
        #endregion
        [NonSerialized]
        public bool drawTerrainHeight = false;
        public StatisticsType statistics = StatisticsType.None;
        #endregion

        #region shaderDebug
        public bool debugShaderFolder = true;
        public ShaderDebugContext debugContext = new ShaderDebugContext ();
        public bool isPostProcessDebug = false;
        public int ppDebugMode = 0;
        public int debugMode = 0;
        public static int[] debugShaderIDS = new int[]
        {
            Shader.PropertyToID ("_GlobalDebugMode"),
            Shader.PropertyToID ("_DebugDisplayType"),
            Shader.PropertyToID ("_SplitAngle"),
            Shader.PropertyToID ("_SplitPos"),
        };
        public static int[] ppDebugShaderIDS = new int[]
        {
            Shader.PropertyToID ("_PPDebugMode"),
            Shader.PropertyToID ("_PPDebugDisplayType"),
            Shader.PropertyToID ("_PPSplitAngle"),
            Shader.PropertyToID ("_PPSplitPos"),
        };
        #endregion

        #region misc
        Camera mainCamera;
        Camera sceneViewCamera;
        private List<IMatObject> matObject = new List<IMatObject> ();
        // private OpType opType = OpType.OpNone;

        private static List<OnDrawGizmoCb> m_drawGizmo = new List<OnDrawGizmoCb> ();
        // private static bool compileDirty = true;
        private Transform meshTerrain;
        #endregion

        #region external
        private ReflectFun initExternal;
        private ReflectFun updateExternal;
        #endregion

        void Awake ()
        {
            if (!ms_invisibleCursor)
            {
                ms_invisibleCursor = new Texture2D (1, 1, TextureFormat.RGBA32, false);
                ms_invisibleCursor.SetPixel (0, 0, new Color32 (0, 0, 0, 0));
            }
            if (EngineContext.IsRunning ||
                loadGameAtHere && Application.isPlaying)
            {
                context = EngineContext.instance;
            }
            InitExternal();
        }

        void Start ()
        {
            Shader.SetGlobalFloat ("_GlobalDebugMode", 0);
            if (!Application.isPlaying)
                EngineContext.IsRunning = false;
            LoadGameAtHere(loadGameAtHere);
        }

        void OnDisable ()
        {
            if (!EngineContext.IsRunning && context != null)
            {
                //VoxelLightingSystem.Uninit ();
                RenderingManager.instance.Uninit (context);
                EngineContext.instance = null;
            }
            context = null;
            EditorApplication.update -= ExtraUpdate;
            SceneView.duringSceneGui -= SFXWrapper.OnSceneGUI;
            SceneView.duringSceneGui -= ShaderDebugModel.DuringSceneGUI;
            EnvProfile.activeProfile = null;
            m_drawGizmo.Clear ();
        }

        void OnDestroy ()
        {
            OnDisable ();
        }

        void Update ()
        {
            GlobalContex.ee = this;
            if (EngineContext.IsRunning ||
                loadGameAtHere && Application.isPlaying)
            {
                if (EngineContext.IsRunning)
                    DrawTerrainHeight ();
            }
            else
            {
                PreUpdate ();
                if (context != null)
                {
                    UpdateEngine ();
                    UpdateEditor ();
                }
            }
            if ((Application.isPlaying && !EngineContext.IsRunning ||
                    forceUpdateFreeCamera) && !forceIgnore)
                UpdateFreeCamera ();
            UpdateDebugMode ();
        }

        void LateUpdate ()
        {
            if (!EngineContext.IsRunning)
            {
                EngineProfiler.Init ();
                EngineProfiler.Update ();
            }
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        static void OnEditorReload () { }
        public void PreInitEngine ()
        {
            bool needInit = context == null;
#if UNITY_EDITOR
            needInit |= RenderingManager.instance.NeedInit ();
#endif
            if (needInit)
            {
                var engineContext = CFAllocator.Allocate<EngineContext> ();
                EngineContext.instance = engineContext;
                EngineContext.renderManager = RenderingManager.instance;
                ThreadManager.Init();
                WorldSystem.InitBothForEditor (engineContext);
                context = null;
            }
        }

        public bool PreInitScene ()
        {
            if (context == null)
            {
                context = EngineContext.instance;
                if (context != null)
                {
                    GameObject cameraGo = GameObject.Find ("Main Camera");
                    if (cameraGo != null)
                    {
                        WorldSystem.InitSceneObject (cameraGo);
                        if (context != null)
                        {
                            SceneContext sceneContext = new SceneContext ();
                            SceneAssets.GetCurrentSceneContext (ref sceneContext);
                            GlobalContex.currentSceneFolder = sceneContext.dir;
                            EngineContext.sceneName = sceneContext.name;
                            EngineContext.sceneNameLower = sceneContext.name.ToLower();
                        }
                        RenderingManager.instance.Start (context);
                        return true;
                    }
                }
            }
            return false;
        }

        public bool PreInit ()
        {
            PreInitEngine ();
            return PreInitScene ();
        }
        public void InitTerrain()
        {
            if(GlobalContex.terrainGo==null|| GlobalContex.terrainData == null)
            {
                var td = SceneAssets.GetUnityTerrain(ref GlobalContex.terrainGo);
                GlobalContex.terrainData = td;
                if (td != null)
                {
                    Vector3 size = td.size;
                    context.Width = (int)size.x;
                    context.Height = (int)size.z;
                    context.xChunkCount = Mathf.RoundToInt(size.x / EngineContext.ChunkSize);
                    context.zChunkCount = Mathf.RoundToInt(size.z / EngineContext.ChunkSize);
                }
            }

        }
        public void InitEditor()
        {
            SceneContext sceneContext = new SceneContext ();
            SceneAssets.GetCurrentSceneContext (ref sceneContext);
            string sceneConfigPath = string.Format ("{0}/{1}{2}.asset",
                sceneContext.configDir, sceneContext.name, SceneContext.SceneConfigSuffix);
            SceneConfig sceneConfig = AssetDatabase.LoadAssetAtPath<SceneConfig> (sceneConfigPath);
            if (sceneConfig != null)
            {
                TerrainObject.globalPbsParam = sceneConfig.terrainParam;
            }
            context.Width = 0;
            InitTerrain();
           

            if (context.Width == 0)
            {
                int size = EngineContext.ChunkSize;
                context.Width = size;
                context.Height = size;
                context.xChunkCount = Mathf.RoundToInt (size / EngineContext.ChunkSize);
                context.zChunkCount = Mathf.RoundToInt (size / EngineContext.ChunkSize);
            }
            //VoxelLightingSystem.Init ();
            RefreshDebug (isPostProcessDebug);
            //init baked light            
            InitMatObject ();
            CallExternalFun (ref initExternal, "InitExternal");
        }

        public void InitMatObject ()
        {
            EditorCommon.EnumTransform funTerrains = null;
            funTerrains = (trans, param) =>
            {
                EnvironmentExtra ee = param as EnvironmentExtra;
                TerrainObject to = trans.GetComponent<TerrainObject> ();
                if (to != null)
                {
                    ee.matObject.Add (to);
                }

            };
            EditorCommon.EnumTransform funStatic = null;
            funStatic = (trans, param) =>
            {
                EnvironmentExtra ee = param as EnvironmentExtra;
                if (trans.TryGetComponent (out LightmapVolumn volumn))
                {
                    var monos = EditorCommon.GetScripts<MeshRenderObject> (trans.gameObject);
                    for (int i = 0; i < monos.Count; ++i)
                    {
                        var mono = monos[i];
                        if (mono.gameObject.activeInHierarchy)
                            ee.matObject.Add (mono as IMatObject);
                    }
                }
                else if (trans.TryGetComponent (out MeshRenderObject mro))
                {
                    if (mro.gameObject.activeInHierarchy)
                        ee.matObject.Add (mro);
                }
                else
                {
                    // if (!EditorCommon.IsPrefabOrFbx (trans.gameObject))
                    {
                        EditorCommon.EnumChildObject (trans, param, funStatic);
                    }
                }
            };
            matObject.Clear ();
            EditorCommon.EnumPath ("MeshTerrain", funTerrains, this);
            EditorCommon.EnumPath ("StaticPrefabs", funStatic, this);
            EditorCommon.EnumPath ("Prefabs", funStatic, this);
            UpdateMatObject ();
        }

        public void PreUpdate ()
        {
            if (PreInit ())
            {
                InitEditor ();
                EditorApplication.update += ExtraUpdate;
                SceneView.duringSceneGui += SFXWrapper.OnSceneGUI;
                SceneView.duringSceneGui += ShaderDebugModel.DuringSceneGUI;
                //SceneAssets.SceneModify(true);
            }
            Camera c = GetComponent<Camera> ();
            if (context != null)
            {
                context.CameraRef = c;
                context.CameraTransCache = c.transform;
                context.uiCamera = uiCamera;
                context.cameraPos = context.CameraTransCache.position;
            }
        }

        private void ExtraUpdate ()
        {
            if (!Application.isPlaying)
            {
                if (forceUpdate || GlobalContex.forceUpdateCount > 0)
                {
                    EditorApplication.QueuePlayerLoopUpdate ();
                }
            }
        }

        public void UpdateEngine ()
        {
            context.renderflag.SetFlag (EngineContext.RFlag_RenderEnable, true);
            context.Update (Time.deltaTime);
            GameObjectSystem.OnUpdate(context);
            UpdateSFX ();
            // if (RenderPipelineManager.renderPipeline == OPRenderPipeline.Builtin)
            // {
            //     context.PostUpdate (false);
            //     context.PreRender ();
            //     context.Render ();
            //     context.PostRender ();
            // }
        }

        public void UpdateEditor ()
        {
            InitTerrain();
            RenderingManager.instance.SetPPEnable (context, true);
            if (RenderLayer.envProfile != envProfile)
            {
                RenderLayer.envProfile = envProfile;
                if (envProfile != null)
                {
                    envProfile.Refresh ();
                }
            }
            bool force = false;
            if (EnvProfile.activeProfile == null)
            {
                EnvProfile.activeProfile = envProfile;
                force = true;
            }
            RenderingManager.instance.SetCurrentEnvEffect (EnvProfile.activeProfile, force);
            #if !UNITY_ANDROID
            EnviromentSHBakerHelper.singleton.Init (AssetsConfig.instance.shBaker);
            #endif

            EnvArea.OnUpdate (context);
            while (GlobalContex.updateOnceCb.Count > 0)
            {
                var cb = GlobalContex.updateOnceCb.Dequeue ();
                cb ();
            }
            for (int i = 0; i < GlobalContex.updateCb.Count; ++i)
            {
                var cb = GlobalContex.updateCb[i];
                cb ();
            }
            EngineTest.singleton.Update ();
            CallExternalFun (ref updateExternal, "UpdateExternal");
            LoadMgr.singleton.Update (context);
        }

        public void UpdateMatObject ()
        {
            for (int i = 0; i < matObject.Count; ++i)
            {
                var mo = matObject[i];
                if (mo != null)
                    mo.Refresh (RenderingManager.instance);
            }

        }
        public void UpdateInstanceObject ()
        {
            EditorCommon.EnumTransform funInstance = null;
            funInstance = (trans, param) =>
            {
                InstanceObject io = trans.GetComponent<InstanceObject> ();
                if (io != null)
                {
                    io.Refresh (RenderingManager.instance);
                }
                EditorCommon.EnumChildObject (trans, param, funInstance);
            };
            EditorCommon.EnumPath ("Instance", funInstance, this);
        }

        public void ClearMatObject ()
        {
            EditorCommon.EnumTransform funStatic = null;
            funStatic = (trans, param) =>
            {
                MeshRenderObject mro = trans.GetComponent<MeshRenderObject> ();
                if (mro != null && mro.gameObject.activeInHierarchy)
                {
                    mro.Clear (RenderingManager.instance);
                }
                EditorCommon.EnumChildObject (trans, param, funStatic);
            };
            EditorCommon.EnumPath ("StaticPrefabs", funStatic, this);
            EditorCommon.EnumPath ("Prefabs", funStatic, this);
        }

        private void UpdateSFX ()
        {
            if (sfxManager == null)
            {
                GameObject sfx = GameObject.Find ("SFXManager");
                if (sfx == null)
                {
                    sfx = new GameObject ("SFXManager");
                }
                sfx.TryGetComponent (out sfxManager);
                if (sfxManager == null)
                {
                    sfxManager = sfx.AddComponent<SFXManager> ();
                }
            }
            SFXMgr.singleton.PostUpdate (context);
        }

        private void CallExternalFun (ref ReflectFun fun, string name)
        {
            if (fun == null)
                fun = EditorCommon.GetInternalFunction (typeof (EnvironmentExtra), name, false, false, true, false);
            if (fun != null)
            {
                fun.Call (this, null);
            }
        }
        #region terrain
        public float GetTerrainY (ref Vector3 pos)
        {
            float terrainY = -1;
            if (context != null)
            {
                if (meshTerrain == null)
                {
                    GameObject go = GameObject.Find (string.Format ("{0}/{1}",
                        AssetsConfig.EditorGoPath[0],
                        AssetsConfig.EditorGoPath[(int) EditorSceneObjectType.MeshTerrain]));
                    meshTerrain = go != null ? go.transform : null;
                }

                int x;
                int z;
                int index = SceneQuadTree.FindChunkIndex (pos, EngineContext.ChunkSize,
                    context.xChunkCount, context.zChunkCount, out x, out z);
                string chunkName = string.Format ("Chunk_{0}_{1}", x, z);
                Transform terrainChunk = meshTerrain.Find (chunkName);
                if (terrainChunk != null && terrainChunk.gameObject.activeInHierarchy)
                {
                    TerrainObject to = terrainChunk.GetComponent<TerrainObject> ();
                    if (to != null && to.terrainObjData.isValid)
                    {
                        if (to.heights == null)
                        {
                            FillTerrainVertex ();
                        }
                        SceneMiscSystem.SampleTerrainHegiht (to.heights, ref pos, x, z, EngineContext.ChunkSize, EngineContext.ChunkSize,
                            ref terrainY);
                    }
                }

            }
            return terrainY;
        }
        #endregion
        #region debug
        public void RefreshDebug (bool isPP)
        {
            debugContext.shaderID = EnvironmentExtra.debugShaderIDS;
            debugContext.Reset ();
            debugContext.Refresh ();
            debugContext.shaderID = EnvironmentExtra.ppDebugShaderIDS;
            debugContext.Reset ();
            debugContext.Refresh ();
            debugContext.shaderID = isPP ? EnvironmentExtra.ppDebugShaderIDS : EnvironmentExtra.debugShaderIDS;
        }
        private int ConvertDebugMode (int debugMode)
        {
            int customStart = AssetsConfig.shaderPPDebugContext.customStart;
            if (debugMode >= customStart)
            {
                string modeStr = debugMode.ToString ();
                if (modeStr.EndsWith ("_A"))
                    return customStart + 1;
                return customStart;
            }
            return (int) debugMode;
        }
        public void UpdateDebugMode ()
        {
            if (GlobalContex.globalDebugMode >= 0)
            {
                if (isPostProcessDebug)
                {
                    ppDebugMode = GlobalContex.globalDebugMode;
                    debugContext.modeModify = (int) ppDebugMode != debugContext.debugMode;
                    debugContext.debugMode = (int) ppDebugMode;
                    debugContext.convertDebugMode = ConvertDebugMode (ppDebugMode);
                }
                else
                {
                    debugMode = GlobalContex.globalDebugMode;
                    debugContext.modeModify = debugMode != debugContext.debugMode;
                    debugContext.debugMode = debugMode;
                    debugContext.convertDebugMode = debugMode;
                }
            }
            debugContext.Refresh ();
        }

        public static void EnableEditorMat(Material mat)
        {
            //mat.EnableKeyword("_DEBUG_APP");
            //mat.DisableKeyword("_ADD_LIGHT");
            //mat.DisableKeyword("_SHADOW_MAP");
            //mat.DisableKeyword("_EXTRA_SHADOW");
            //mat.EnableKeyword("_LOD0");

        }

        public void LoadGameAtHere(bool startGame)
        {
            if (startGame && Application.isPlaying && !EngineContext.IsRunning)
            {
                EngineContext.bytesPrefabProgress = bytesPrefabProgress;
                LoadMgr.singleton.forceExport = testBytesExport;
                UnityEngine.SceneManagement.SceneManager.LoadScene("entrance");
                //SceneChunkLoadSystem.gotoSceneID = gotoScene ? sceneID : -1;
                if (replaceStartScene)
                {
                    //SceneChunkLoadSystem.startSceneID = sceneID;
                    //SceneChunkLoadSystem.gotoSceneID = -1;
                    //SceneChunkLoadSystem.startPos = transform.position;
                    //SceneChunkLoadSystem.startRot = transform.rotation.eulerAngles;
                }

                DebugLog.SetFlag(EDebugFlag.SceneHead, debugHeadData);
                //SceneChunkLoadSystem.DebugChunkIndex = debugChunkIndex;
                DebugLog.SetFlag(EDebugFlag.SceneChunk, debugChunkIndex >= 0);

            }
        }
        #endregion
        #region freeCamera
        void OnValidate ()
        {
            // if (Application.isPlaying)
            //     enabled = enableInputCapture;
        }

        void CaptureInput ()
        {
            Cursor.lockState = CursorLockMode.Locked;

            Cursor.SetCursor (ms_invisibleCursor, Vector2.zero, CursorMode.ForceSoftware);
            m_inputCaptured = true;

            m_yaw = transform.eulerAngles.y;
            m_pitch = transform.eulerAngles.x;
        }
        void ReleaseInput ()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.SetCursor (null, Vector2.zero, CursorMode.Auto);
            m_inputCaptured = false;
        }
        void OnApplicationFocus (bool focus)
        {
            if (m_inputCaptured && !focus)
                ReleaseInput ();
        }
        void UpdateFreeCamera ()
        {
            if (!m_inputCaptured)
            {
                if (!holdRightMouseCapture && Input.GetMouseButtonDown (0))
                    CaptureInput ();
                else if (holdRightMouseCapture && Input.GetMouseButtonDown (1))
                    CaptureInput ();
            }

            if (!m_inputCaptured)
                return;

            if (m_inputCaptured)
            {
                if (!holdRightMouseCapture && Input.GetKeyDown (KeyCode.Escape))
                    ReleaseInput ();
                else if (!holdRightMouseCapture && Input.GetKeyDown (KeyCode.Q))
                {
                    ReleaseInput ();
                    forceUpdateFreeCamera = false;
                }
                else if (holdRightMouseCapture && Input.GetMouseButtonUp (1))
                    ReleaseInput ();

            }

            var rotStrafe = Input.GetAxis ("Mouse X");
            var rotFwd = Input.GetAxis ("Mouse Y");

            m_yaw = (m_yaw + lookSpeed * rotStrafe) % 360f;
            m_pitch = (m_pitch - lookSpeed * rotFwd) % 360f;
            transform.rotation = Quaternion.AngleAxis (m_yaw, Vector3.up) * Quaternion.AngleAxis (m_pitch, Vector3.right);

            var speed = Time.deltaTime * (Input.GetKey (KeyCode.LeftShift) ? sprintSpeed : moveSpeed);
            var forward = speed * Input.GetAxis ("Vertical");
            var right = speed * Input.GetAxis ("Horizontal");
            var up = speed * ((Input.GetKey (KeyCode.E) ? 1f : 0f) - (Input.GetKey (KeyCode.Q) ? 1f : 0f));
            transform.position += transform.forward * forward + transform.right * right + Vector3.up * up;

        }
        #endregion

        #region gizmo
        public static void RegisterDrawGizmo (OnDrawGizmoCb cb)
        {
            if (cb != null && m_drawGizmo.IndexOf (cb) < 0)
                m_drawGizmo.Add (cb);
        }
        public static void UnRegisterDrawGizmo (OnDrawGizmoCb cb)
        {
            m_drawGizmo.Remove (cb);
        }

        #region CameraWall
        void DrawCameraWall (int wallIndex, int hitIndex, ref Color color,
            ref Vector3 normal, ref Vector3 hitPoint,
            ref Vector3 corner0, ref Vector3 corner1,
            ref Vector3 corner0Bottom, ref Vector3 corner1Bottom)
        {
            if (wallIndex == hitIndex)
            {
                Gizmos.color = new Color (1, 0, 1, 0.3f);
                Gizmos.DrawSphere (hitPoint, 0.2f);
            }
            else
            {
                Gizmos.color = color;
            }
            Gizmos.DrawLine (corner0, corner1);
            Gizmos.DrawLine (corner1, corner1Bottom);
            Gizmos.DrawLine (corner1Bottom, corner0Bottom);
            Gizmos.DrawLine (corner0Bottom, corner0);
            Vector3 center = corner0 + corner1 + corner0Bottom + corner1Bottom;
            center /= 4;
            Gizmos.DrawRay (center, normal);
        }

        //         void DrawCameraWall ()
        //         {
        //             if (drawCameraWall)
        //             {
        // #if DEBUG
        //                 Gizmos.color = Color.red;
        //                 Gizmos.DrawSphere (context.cameraTarget, 0.3f);
        //                 Gizmos.DrawLine (context.lookAnchor, context.cameraTarget);

        //                 var it = context.cachedChunks.GetEnumerator ();
        //                 while (it.MoveNext ())
        //                 {
        //                     var chunk = it.Current.Value;
        //                     for (int i = chunk.wallStart; i < chunk.wallEnd; ++i)
        //                     {
        //                         CameraWall wall = chunk.sceneObjects.Get<CameraWall> (i);
        //                         if (wall.needCull)
        //                         {
        //                             Vector3 corner0 = wall.corner0;
        //                             float top = corner0.y;

        //                             Vector3 corner1 = wall.corner1;
        //                             float buttom = corner1.y;
        //                             corner1.y = top;
        //                             Vector3 corner2 = wall.corner2;
        //                             Vector3 corner3 = wall.corner3;

        //                             Vector3 corner0Bottom = corner0;
        //                             corner0Bottom.y = buttom;
        //                             Vector3 corner1Bottom = corner1;
        //                             corner1Bottom.y = buttom;
        //                             Vector3 corner2Bottom = corner2;
        //                             corner2Bottom.y = buttom;
        //                             Vector3 corner3Bottom = corner3;
        //                             corner3Bottom.y = buttom;

        //                             Color hitColor = Color.white;
        //                             int hitIndex = -1;
        //                             if (wall.hitType != -1)
        //                             {
        //                                 hitColor = Color.green;
        //                             }
        //                             if (wall.needCull2)
        //                             {
        //                                 if (wall.hitType != -1)
        //                                 {

        //                                     //Gizmos.color = Color.green;
        //                                 }
        //                                 else
        //                                 {
        //                                     hitColor = new Color (0, 0.5f, 0.5f, 0.5f);
        //                                 }
        //                             }
        //                             else
        //                             {
        //                                 hitColor = new Color (0.5f, 0.5f, 0, 0.5f);
        //                             }
        //                             hitIndex = wall.hitWallIndex;

        //                             DrawCameraWall (0, hitIndex, ref hitColor,
        //                                 ref wall.normal0, ref wall.hitpos0, ref corner0, ref corner1, ref corner0Bottom, ref corner1Bottom);
        //                             DrawCameraWall (1, hitIndex, ref hitColor,
        //                                 ref wall.normal1, ref wall.hitpos1, ref corner1, ref corner2, ref corner1Bottom, ref corner2Bottom);
        //                             DrawCameraWall (2, hitIndex, ref hitColor,
        //                                 ref wall.normal2, ref wall.hitpos2, ref corner2, ref corner3, ref corner2Bottom, ref corner3Bottom);
        //                             DrawCameraWall (3, hitIndex, ref hitColor,
        //                                 ref wall.normal3, ref wall.hitpos3, ref corner3, ref corner0, ref corner3Bottom, ref corner0Bottom);

        //                         }
        //                     }
        //                 }
        // #endif
        //             }
        //         }
        #endregion
        #region DynamicWall
        void DrawDynamicWall ()
        {

            if (drawDynamicWall)
            {
                SceneRoamingSystem.DrawWallGizmo (context);
            }
        }
        #endregion

        #region ChunkLod
        void DrawChunkLod ()
        {
            if (drawLodGrid || drawTerrainGrid || drawCacheChunk)
            {
                var it = context.cachedChunks.GetEnumerator ();
                while (it.MoveNext ())
                {
                    var chunk = it.Current.Value;
                    var qt = chunk.quadTree;
                    if (qt != null && qt.valid)
                    {
                        if (drawLodGrid)
                        {
                            for (int i = 5; i < 21; ++i)
                            {
                                ref var node = ref qt.nodes[i];
                                if (node.lodLevel == 0)
                                {
                                    Gizmos.color = Color.yellow;
                                    Gizmos.DrawWireCube (node.aabb.center, node.aabb.size);
                                }
                            }
                        }

                        if (drawTerrainGrid)
                        {
                            ref var tr = ref chunk.terrainRes;
                            if (tr.terrainMat != null)
                            {
                                Gizmos.color = Color.cyan;
                                for (int i = 0; i < 4; ++i)
                                {
                                    ref var node = ref qt.nodes[i + 1];
                                    if (node.lodLevel == EngineContext.Lod0)
                                    {
                                        var tml = tr.terrainMat[i];
                                        Gizmos.DrawWireCube (tml.aabb.center, tml.aabb.size);
                                    }
                                }
                            }
                        }
                        if (drawCacheChunk)
                        {
                            ref var node = ref qt.nodes[0];
                            Gizmos.color = Color.white;
                            Gizmos.DrawWireCube (node.aabb.center, node.aabb.size);
                        }
                    }
                }
            }

        }
        #endregion
        #region ChunkObjects
        void DrawBox (bool draw, ref AABB aabb)
        {
            switch (drawType)
            {
                case DrawType.Both:
                    {
                        if (draw)
                        {
                            Gizmos.color = Color.red;
                        }
                        else
                            Gizmos.color = Color.magenta;
                        Gizmos.DrawWireCube (aabb.center, aabb.size);
                    }
                    break;
                case DrawType.Draw:
                    {
                        if (draw)
                        {
                            Gizmos.color = Color.red;
                            Gizmos.DrawWireCube (aabb.center, aabb.size);
                        }

                    }
                    break;
                case DrawType.Cull:
                    {
                        if (!draw)
                        {
                            Gizmos.color = Color.magenta;
                            Gizmos.DrawWireCube (aabb.center, aabb.size);
                        }
                    }
                    break;
            }

        }
        void DrawSceneObjectBox (SceneChunk sc, ref SceneQuadTreeNode node)
        {
            if (sc.sceneObjects.IsValid () && sc.chunkState >= SceneChunk.ESceneChunkState.ChunkDataLoadFinish)
            {
                var it = node.soList.BeginGet ();
                while (node.soList.Get (ref it, out SceneObject so))
                {
                    if (so != null && so.asset.obj != null)
                    {
                        DrawBox (so.draw, ref so.aabb);
                    }
                }
            }
        }
        void DrawChunkObjects ()
        {
            // if (updateSceneObject)
            // {
            //     sceneObjects.Clear ();
            // }
            int level = (int) quadLevel;
            if (level != 0 &&
                context.cachedChunks != null)
            {
                var it = context.cachedChunks.GetEnumerator ();
                while (it.MoveNext ())
                {
                    var sc = it.Current.Value;
                    var qt = sc.quadTree;
                    if (qt != null && qt.valid)
                    {
                        int start = 0;
                        int end = 1;
                        if (level == 2)
                        {
                            start = 1;
                            end = 5;
                        }
                        else if (level == 3 || level == 4)
                        {
                            start = 5;
                            end = 21;
                        }
                        if (level != 4)
                        {
                            if (level == 1)
                            {
                                ref var node = ref qt.nodes[0];
                                DrawBox (node.draw, ref node.aabb);
                            }
                            else
                            {
                                for (int i = start; i < end; ++i)
                                {
                                    ref var node = ref qt.nodes[i];
                                    DrawBox (node.draw, ref node.aabb);
                                }
                            }
                        }
                        else
                        {
                            if (quadIndex == -1)
                            {
                                int ii = 0;
                                try
                                {
                                    for (ii = 0; ii < 16; ++ii)
                                    {
                                        DrawSceneObjectBox (sc, ref qt.nodes[5 + ii]);
                                    }
                                }
                                catch (Exception)
                                {
                                    Debug.LogErrorFormat ("chunk id:{0} block id:{1}", sc.chunkId, ii);
                                }
                            }
                            else
                            {
                                DrawSceneObjectBox (sc, ref qt.nodes[5 + quadIndex]);
                            }
                        }
                    }
                }
            }
        }

        #endregion
        #region MatLod
        private void DrawMatClipmap (ref Vector4 clipmap, Vector3 pos, int matOffset0, int matOffset1, Color c)
        {
            if (context.currentChunks != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube (pos, new Vector3 (clipmap.z - clipmap.x, 1, clipmap.w - clipmap.y));
                Gizmos.color = c;
                for (int i = 0; i < context.currentChunks.Length; ++i)
                {
                    var sc = context.currentChunks[i];
                    if (sc != null)
                    {
                        int j = sc.sceneObject.Begin ();
                        while (sc.sceneObject.Get (ref sc.sceneObjects, ref j, out SceneObject so))
                        {
                            if (so.MatLod >= matOffset0 && so.MatLod <= matOffset1)
                            {
                                Gizmos.DrawWireCube (so.aabb.center, so.aabb.size);
                            }
                        }
                    }
                }
            }
        }
        private void DrawLod ()
        {
            //switch (matLod)
            //{
            //    case 0:
            //        {
            //            if (context.lookAtPos.x > 0)
            //                DrawMatClipmap (ref context.clipmap0, context.lookAtPos, 0, 1, Color.red);
            //            else
            //                DrawMatClipmap (ref context.clipmap0, context.cameraPos, 0, 1, Color.red);
            //        }
            //        break;
            //    case 1:
            //        {
            //            DrawMatClipmap (ref context.clipmap1, context.cameraPos, 2, 3, Color.blue);
            //        }
            //        break;
            //    case 2:
            //        {
            //            DrawMatClipmap (ref context.clipmap2, context.cameraPos, 4, 5, Color.white);
            //        }
            //        break;
            //}

            if (lodLevel != ELodLevel.None)
            {
                switch (lodLevel)
                {
                    case ELodLevel.Lod0:
                        Gizmos.color = Color.blue;
                        break;
                    case ELodLevel.Lod1:
                        Gizmos.color = Color.red;
                        break;
                    case ELodLevel.LodFade:
                        Gizmos.color = Color.white;
                        break;
                }
                if (context.cachedChunks != null)
                {
                    var it = context.cachedChunks.GetEnumerator ();
                    while (it.MoveNext ())
                    {
                        var sc = it.Current.Value;
                        for (int i = sc.sceneObject.start; i < sc.sceneObject.end; ++i)
                        {
                            SceneObject so = sc.sceneObjects.Get<SceneObject> (i);
                            if (so.m != null)
                            {
                                bool draw = false;
                                if (so.groupObjectRef != null)
                                {
                                    draw = so.groupObjectRef.lodData.lodDist.lodLevel == lodLevel;
                                }
                                else
                                {

                                    draw = so.lodDist.lodLevel == lodLevel;
                                }
                                if (draw)
                                {
                                    Gizmos.DrawWireCube (so.aabb.center, so.aabb.size);
                                }

                            }
                        }
                    }
                }
            }
        }

        private void DrawLightProbe ()
        {
            if (LightProbeDebug.draw != preiewLightProbe)
            {
                if (preiewLightProbe)
                {
                    LightProbeDebug.m = AssetsConfig.instance.sphereMesh;
                    LightProbeDebug.mat = AssetsConfig.instance.PreviewSH2;
                }
                LightProbeDebug.draw = preiewLightProbe;
            }

        }

        private void DrawLines(List<Vector4> points)
        {
            Vector3 lastP = Vector3.zero;
            for (int i = 0; i < points.Count; ++i)
            {
                var p = points[i];
                Vector3 point = p;
                Gizmos.DrawWireSphere(p, 0.2f);
                Handles.color = Color.black;
                Handles.Label(point + new Vector3(0, 3, 0),
                    string.Format("Index:{0} Frame:{1} P:{2}", i, (int)p.w, point.ToString()));
                if (i > 0)
                {
                    Gizmos.DrawLine(lastP, p);
                }
                lastP = point;
            }
        }
        private void DrawPoints ()
        {
            if (drawCrossChunkPoint)
            {
                Gizmos.color = Color.red;
                DrawLines(context.crossChunkPoint);
            }

            if(drawLookAtPoint)
            {
                Gizmos.color = Color.red;
                DrawLines(context.lookAtPoint);
            }
        }
        #endregion

        #region envarea
        private void DrawEnvArea ()
        {
            if (debugEnvArea)
            {
                if (EngineContext.IsRunning)
                {
                    //int i = context.envObject.Begin ();
                    //while (context.envObject.Get (ref context.globalObjects, ref i, out EnvBlock envBlock))
                    //{
                    //    if (envBlock.data.Length > 0)
                    //    {
                    //        int boxDataCount = (int) envBlock.data.Get (0);
                    //        for (int j = 1; j < boxDataCount; j += 8)
                    //        {
                    //            float minY = envBlock.data.Get (j + 6); //y min
                    //            float maxY = envBlock.data.Get (j + 7); //y max
                    //            float x = envBlock.data.Get (j); //center x
                    //            float z = envBlock.data.Get (j + 1); //center z
                    //            float cosy = envBlock.data.Get (j + 2); //cos y
                    //            float siny = envBlock.data.Get (j + 3); //sin y
                    //            float halfWidth = envBlock.data.Get (j + 4); //half width
                    //            float halfHeight = envBlock.data.Get (j + 5); //half height
                    //            if (context.currentEnvBlock == envBlock)
                    //            {
                    //                Gizmos.color = Color.red;
                    //            }
                    //            else
                    //            {
                    //                Gizmos.color = Color.white;
                    //            }

                    //            Vector3 worldPos = new Vector3 (x, (minY + maxY) * 0.5f, z);
                    //            Vector3 forwardPos = -Vector3.forward * halfHeight;
                    //            Vector3 boxSpaceRotPos = new Vector3 (forwardPos.x * cosy - forwardPos.z * siny, 0, forwardPos.x * siny + forwardPos.z * cosy);
                    //            Gizmos.matrix = Matrix4x4.TRS (worldPos, Quaternion.LookRotation (boxSpaceRotPos, Vector3.up), Vector3.one);
                    //            Gizmos.DrawWireCube (Vector3.zero, new Vector3 (halfWidth * 2, maxY - minY, halfHeight * 2));
                    //            Gizmos.matrix = Matrix4x4.identity;
                    //        }
                    //    }
                    //}
                }
                else
                {
                    EnvArea.DrawGizmos ();
                }
            }
        }
        public static void FillTerrainVertex ()
        {
            EngineContext context = EngineContext.instance;
            if (context != null)
            {
                GameObject go = GameObject.Find (string.Format ("{0}/{1}",
                    AssetsConfig.EditorGoPath[0],
                    AssetsConfig.EditorGoPath[(int) EditorSceneObjectType.MeshTerrain]));
                Transform meshTerrain = go != null?go.transform : null;
                if (meshTerrain != null)
                {
                    SceneContext sceneContext = new SceneContext ();
                    SceneAssets.GetCurrentSceneContext (ref sceneContext);
                    string path = string.Format ("{0}/TerrainVertex.bytes", sceneContext.configDir);
                    if (File.Exists (path))
                    {
                        using (FileStream fs = new FileStream (path, FileMode.Open))
                        {
                            BinaryReader br = new BinaryReader (fs);
                            int count = br.ReadInt32 ();
                            int gridCount = EngineContext.terrainGridCount + 1;
                            gridCount *= gridCount;
                            float[] data = new float[gridCount];
                            for (int i = 0; i < count; ++i)
                            {
                                for (int j = 0; j < gridCount; ++j)
                                {
                                    data[j] = br.ReadSingle ();
                                }

                                int x = i % context.xChunkCount;
                                int z = i / context.zChunkCount;
                                string chunkName = string.Format ("Chunk_{0}_{1}", x, z);
                                Transform t = meshTerrain.Find (chunkName);
                                if (t != null)
                                {
                                    TerrainObject to = t.GetComponent<TerrainObject> ();
                                    if (to != null)
                                    {
                                        if (to.dataBuffer == null)
                                            to.dataBuffer = new ComputeBuffer (gridCount, sizeof (float), ComputeBufferType.Default);

                                        to.dataBuffer.SetData (data);
                                        if (to.heights == null)
                                        {
                                            to.heights = new float[gridCount];
                                        }
                                        Array.Copy (data, to.heights, gridCount);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void EnableDrawTerrainHeight ()
        {
            if (!EngineContext.IsRunning)
            {
                EditorCommon.EnumTransform funTerrain = null;
                funTerrain = (trans, param) =>
                {
                    TerrainObject to = trans.GetComponent<TerrainObject> ();
                    if (to != null)
                    {
                        to.preview = drawTerrainHeight;
                    }
                    EditorCommon.EnumChildObject (trans, param, funTerrain);
                };
                EditorCommon.EnumPath ("MeshTerrain", funTerrain, this);
            }
        }
        public void DrawTerrainHeight ()
        {
            if (drawTerrainHeight)
            {
                var terrainHeight = AssetsConfig.instance.PreviewTerrainQuad;
                if (terrainHeight != null)
                {
                    int gridCount = EngineContext.terrainGridCount + 1;
                    gridCount *= gridCount;
                    var it = context.cachedChunks.GetEnumerator ();
                    while (it.MoveNext ())
                    {
                        var chunk = it.Current.Value;
                        if (chunk.heights.IsCreated)
                        {
                            if (chunk.heightBuffer == null)
                            {
                                chunk.heightBuffer = new ComputeBuffer (gridCount, sizeof (float), ComputeBufferType.Default);
                            }
                            chunk.heightBuffer.SetData (chunk.heights);
                            chunk.debugMpb.SetBuffer (TerrainObject.vertexHeight, chunk.heightBuffer);
                            float gridSizeInv = 1.0f / EngineContext.terrainGridCount;
                            chunk.debugMpb.SetVector (TerrainObject._GridSize,
                                new Vector4 (chunk.x * EngineContext.ChunkSize,
                                    chunk.z * EngineContext.ChunkSize,
                                    gridSizeInv, gridSizeInv));
                            ref var tr = ref chunk.terrainRes;

                            if (tr.terrainMat != null)
                            {
                                ref var tml = ref tr.terrainMat[0];
                                Graphics.DrawProcedural (terrainHeight,
                                    tml.aabb.ToUnityAABB (),
                                    MeshTopology.Quads,
                                    4,
                                    64 * 64,
                                    null,
                                    chunk.debugMpb);
                            }
                            else
                            {

                                Bounds aabb = new Bounds (
                                    new Vector3 ((chunk.x + 0.5f) * EngineContext.ChunkSize, 0, (chunk.z + 0.5f) * EngineContext.ChunkSize),
                                    new Vector3 (EngineContext.ChunkSize, 100, EngineContext.ChunkSize));
                                Graphics.DrawProcedural (terrainHeight,
                                    aabb,
                                    MeshTopology.Quads,
                                    4,
                                    64 * 64,
                                    null,
                                    chunk.debugMpb);
                            }

                        }
                    }
                }

            }
        }
        #endregion
        #region debug
        private void DrawDebug ()
        {
            if (previewDummyCamera)
            {
                SplitScreenSystem.OnDrawGizmo(context);
            }
            if (drawGlobalShadowObject)
            {
                for (int i = 0; i < matObject.Count; ++i)
                {
                    var mo = matObject[i];
                    if (mo != null)
                        mo.OnDrawGizmo(context);
                }
            }
        }
        #endregion
        void OnDrawGizmos()
        {
            Color color = Gizmos.color;
            Color hcolor = Handles.color;
            if (mainCamera == null)
                mainCamera = GetComponent<Camera> ();
            if (mainCamera != null)
            {
                if (drawFrustum)
                    CameraEditorUtils.DrawFrustumGizmo (mainCamera);

            }
            if (context != null)
            {
                if (drawCameraPos)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere (context.cameraPos, 0.1f);
                }
                if (context.cachedChunks != null)
                {
                    DrawDynamicWall ();
                    DrawChunkLod ();
                    DrawChunkObjects ();
                    DrawLod ();
                    DrawLightProbe ();
                    DrawPoints ();
                    DrawCustomGizmo ();
                }
                DrawEnvArea ();
                DrawDebug ();
                if (EngineContext.IsRunning)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere (context.currentEntityPos, 0.1f);
                }
            }

            for (int i = 0; i < m_drawGizmo.Count; ++i)
            {
                if (m_drawGizmo[i] != null)
                {
                    m_drawGizmo[i] ();
                }
            }
            RenderingManager.instance.OnDrawGizmos (context);
            SFXMgr.singleton.OnDrawGizmo(context);
            Gizmos.color = color;
            Handles.color = hcolor;
        }

        private static void DrawCustomGizmo ()
        {
            if (EngineContext.CustomGizmosLines.Count != 0)
            {
                for (int i = 0; i < EngineContext.CustomGizmosColors.Length; i++)
                {
                    Gizmos.color = EngineContext.CustomGizmosColors[i];
                    for (int j = 0; j < EngineContext.CustomGizmosLines.Count; j++)
                    {
                        if (EngineContext.CustomGizmosLines[j].color == i)
                        {
                            Gizmos.DrawLine (EngineContext.CustomGizmosLines[j].p1, EngineContext.CustomGizmosLines[j].p2);
                        }
                    }
                }
            }
        }

        // private static ReflectFun getMainGameView;
        // internal static Rect GetGameViewRect ()
        // {
        //     if (getMainGameView == null)
        //     {
        //         var es = EditorCommon.GetUnityEditorAssembly ();
        //         if (es != null)
        //         {
        //             var t = es.GetType ("UnityEditor.GameView");
        //             if (t != null)
        //             {
        //                 getMainGameView = EditorCommon.GetInternalFunction (t, "GetMainGameView", true, true, false, false);
        //             }
        //         }
        //     }
        //     if (getMainGameView != null)
        //     {
        //         var window = getMainGameView.Call (null, null) as EditorWindow;
        //         if (window != null)
        //             return window.position;
        //     }
        //     return new Rect ();
        // }
        void OnGUI ()
        {
            //if (!EngineContext.IsRunning)
            {
                EngineProfiler.SetStatisticsType ((int) statistics);
                var rect = new Rect (0, 0, Screen.width, Screen.height);
                DebugDraw.OnGUI (ref rect);
            }
        }
        #endregion

        #region editor

        [MenuItem("Assets/Tool/SelectCamera _F1")]
        public static void FocusMainCamera()
        {
            GameObject go = GameObject.Find("Main Camera");
            if (go != null)
            {
                Selection.activeGameObject = go;
            }
        }

        [MenuItem("Assets/Tool/ShowEngineProfile _F11")]
        public static void ShowEngineProfile()
        {
            if (GlobalContex.ee != null)
            {
                if (GlobalContex.ee.statistics == StatisticsType.None)
                {
                    GlobalContex.ee.statistics = StatisticsType.SRP;
                }
                else
                {
                    GlobalContex.ee.statistics = StatisticsType.None;

                }
            }

        }
        [MenuItem("Assets/Tool/ShowEngineStatisc _F12")]
        public static void ShowEngineStatisc()
        {
            if (GlobalContex.ee != null)
            {
                if (GlobalContex.ee.statistics == StatisticsType.None)
                {
                    GlobalContex.ee.statistics = StatisticsType.Counter;
                }
                else
                {
                    GlobalContex.ee.statistics = StatisticsType.None;

                }
            }

        }

        #endregion
    }
}
#endif