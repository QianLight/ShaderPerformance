using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace CFEngine
{
#if UNITY_EDITOR
    public enum ShadowMapDebug
    {
        None,
        Layer0,
        Layer1,
        Layer2,
        ExtraShaow,
        ExtraShaow1,
        SelfShadow,
        DepthShadow,
        TmpShadow,
        EditorTerrainShadow,
    }

#endif
    [Serializable]
    [Env (typeof (ShadowModify), "Env/Shadow")]
    public sealed class Shadow : EnvSetting
    {
        [CFEnum (typeof (ShadowMode), (int) ShadowMode.HardShadow)]
        public ShadowModeParam shadowMode = new ShadowModeParam { value = ShadowMode.HardShadow };

        [CFParam4 ("ShadowVolumnSize0", 32, 1, 32, -1, C4DataType.FloatRange,
            "Extra1ShadowSize", 32, 1, 64, -1, C4DataType.FloatRange,
            "ExtraScale", 1, 0.1f, 1, -1, C4DataType.FloatRange,
            "", 0, 0, 1, -1, C4DataType.None), CFTooltip ("CSM Param.")]
        public Vector4Param shadowParam0 = new Vector4Param { value = new Vector4 (16, 32, 1, 0.5f) };

        [CFParam4 ("NormalBias0", 0.02f, -0.5f, 0.5f, -1, C4DataType.FloatRange,
            "NormalBias1", 0.05f, -1.0f, 1.0f, -1, C4DataType.FloatRange,
            "NormalBias2", 0.15f, -1.5f, 1.5f, -1, C4DataType.FloatRange,
            "NormalBiasExtra", 0.005f, -0.5f, 0.5f, -1, C4DataType.FloatRange), CFTooltip ("CSM Bias Param.")]
        public Vector4Param shadowParam1 = new Vector4Param { value = new Vector4 (0.02f, 0.05f, 0.15f, 0.005f) };

        [CFParam4 ("ShadowFade", 0, 0, 1, -1, C4DataType.FloatRange,
            "RoleShadowMultiply", 0, 0, 1, -1, C4DataType.FloatRange,
            "RoleRimFade", 0.4f, 0, 1, -1, C4DataType.FloatRange,
            "ObjectBias", 0.3f, -1, 1, -1, C4DataType.FloatRange), CFTooltip ("Shadow Misc.")]
        public Vector4Param shadowMisc = new Vector4Param { value = new Vector4 (0, 0, 0.4f, 0.3f) };

        [CFColorUsage(true, true, 0, 0, 0, 1)]
        public ColorParam color = new ColorParam { value = Color.black };

        [CFColorUsage(true, true, 1.097693f, 0.8988358f, 1.519271f, 1f)]
        public ColorParam roleShadowColor = new ColorParam { value = new Color(1.097693f, 0.8988358f, 1.519271f, 1f) };

        [CFParam4 ("ChunkShadowBias", 0.08f, -0.1f, 0.1f, -1, C4DataType.FloatRange,
            "xSpeed", 0, -2, 2, -1, C4DataType.None,
            "zSpeed", 0, -2, 2, -1, C4DataType.None,
            "Scale", 1, 0.001f, 2, -1, C4DataType.None), CFTooltip ("Shadow Misc 1.")]
        public Vector4Param shadowMisc1 = new Vector4Param { value = new Vector4 (0.08f, 0, 0, 1) };

        [CFParam4 ("CloudHeightRatio", 0.3f, 0.001f, 1, -1, C4DataType.None,
            "CloudBrightRatio", 0, 0, 1, -1, C4DataType.None,
            "ExtraShadowBiasX", 0, -8, 8, -1, C4DataType.FloatRange,
            "ExtraShadowBiasZ", 0, -8, 8, -1, C4DataType.FloatRange), CFTooltip ("Shadow Misc 2.")]
        public Vector4Param shadowMisc2 = new Vector4Param { value = new Vector4 (0.3f, 0, 0, 0) };

        [CFResPath (typeof (Texture2D), "", EnvBlock.ResOffset_CloudMap, true)]
        public ResParam cloudMap = new ResParam { value = "" };

#if UNITY_EDITOR
        //[CFCustomDraw ()]
        //public SceneObjectGroupsParam sceneObjectGroups = new SceneObjectGroupsParam { value = new SceneObjectGroups (), overrideState = true };

#endif

        public override void InitParamaters (ListObjectWrapper<ISceneObject> objects, EnvModify envModify)
        {
            InnerInitParamters (objects, envModify, "Shadow");
            CreateParam (ref shadowMode, "shadowMode", objects, envModify);
            CreateParam (ref shadowParam0, nameof (shadowParam0), objects, envModify);
            CreateParam (ref shadowParam1, nameof (shadowParam1), objects, envModify);
            CreateParam (ref shadowMisc, nameof (shadowMisc), objects, envModify);
            CreateParam (ref color, nameof (color), objects, envModify);
            CreateParam (ref roleShadowColor, nameof (roleShadowColor), objects, envModify);
            CreateParam (ref shadowMisc1, nameof (shadowMisc1), objects, envModify);
            CreateParam (ref shadowMisc2, nameof (shadowMisc2), objects, envModify);
            CreateParam (ref cloudMap, nameof (cloudMap), objects, envModify);
        }

#if UNITY_EDITOR
        public override void InitEditorParamaters (ListObjectWrapper<ISceneObject> objects, EnvModify envModify, bool init)
        {
            if (init)
            {
                cloudMap.resOffset = EnvBlock.ResOffset_CloudMap;
                cloudMap.Init ();
            }
        }
#endif
        public override EnvSettingType GetEnvType ()
        {
            return EnvSettingType.Shadow;
        }

        public override void ResetEffect ()
        {
            active.value = true;
        }

        public override void UninitParamaters ()
        {
            base.UninitParamaters ();
            cloudMap.UnInit ();
        }

        // public static void Creator (out EnvSetting setting, out EnvModify modify, bool createModify)
        // {
        //     setting = Create<Shadow> ();
        //     if (createModify)
        //         modify = CFAllocator.Allocate<ShadowModify> ();
        //     else
        //         modify = null;
        // }

        public override EnvSetting Load (CFBinaryReader reader, EngineContext context)
        {
            Shadow setting = Load<Shadow> ((int) EnvSettingType.Shadow);
            setting.shadowMode.value = (ShadowMode) reader.ReadByte ();
            reader.ReadVector (ref setting.shadowParam0.value);
            reader.ReadVector (ref setting.shadowParam1.value);
            reader.ReadVector (ref setting.shadowMisc.value);
            //setting.selfShadowParam.value = reader.ReadVector4();
            //selfShadowOffset.value = reader.ReadVector4();
            //setting.selfShadowScale.value = reader.ReadVector4();
            reader.ReadVector (ref setting.color.value);
#if UNITY_EDITOR
            if (context.IsValidResVersion(RenderContext.ResVersionRoleLightingV3, EngineContext.Cmp_GE))
#endif
            {
                reader.ReadVector (ref setting.roleShadowColor.value);
            }
            reader.ReadVector (ref setting.shadowMisc1.value);
            reader.ReadVector (ref setting.shadowMisc2.value);
            setting.cloudMap.Load (reader);
            return setting;
        }

#if UNITY_EDITOR
        public override void Save (BinaryWriter bw)
        {
            byte mode = (byte) shadowMode.value;
            bw.Write (mode);
            EditorCommon.WriteVector (bw, shadowParam0.value);
            EditorCommon.WriteVector (bw, shadowParam1.value);
            EditorCommon.WriteVector (bw, shadowMisc.value);
            EditorCommon.WriteVector (bw, color.value);
            EditorCommon.WriteVector (bw, roleShadowColor.value);
            if (shadowMisc1.value.x < -1 || shadowMisc1.value.x > 1)
                shadowMisc1.value.x = 0.08f;
            EditorCommon.WriteVector (bw, shadowMisc1.value);
            EditorCommon.WriteVector (bw, shadowMisc2.value);
            EditorCommon.WriteRes (bw, cloudMap);
        }
#endif
    }

    /// <summary>
    /// 0.collect dynamic obj shadow batch (in ShadowCollectSystem): a.calc culling frustum b.intersect with skinmesh animationmesh c.calc near far plane
    /// 1.update scene csm culling frustum (in WorldSystem->RenderingManager.ManualUpdate->Shadow.Update): a.clear all preEffect commandBuffer b.test pos change with 3 csm c.calc csm near far palne if dirty
    /// 2.cull scene objects with csm frustum(in  SceneCullSystem):a.test with scene quadTree and sceneObject b.test 1 cascade 1 frame,total n frame if n csm dirty
    /// 3.render shadowmap(in ScriptRenderSystem->RenderingManager.PostUpdate->Shadow.PostUpdate):a.render dynamic shadowmap b.render 1 csm if shadow batch has shadow data
    /// </summary>
    public sealed class ShadowModify : EnvModify<Shadow>
    {
        private RenderTexture shadowRTArray;

        Vector3 selfShadowLightProjectRight;
        Vector3 selfShadowLightProjectUp;
        Vector3 depthLightProjectRight;
        Vector3 depthLightProjectUp;
        List<DrawBatch> unloadDC = new List<DrawBatch> ();

        public static readonly int _CloudMap = Shader.PropertyToID ("_CloudMap");
        public static readonly int _ShadowParam0 = Shader.PropertyToID ("_ShadowParam0");
        public static readonly int _ShadowParam1 = Shader.PropertyToID ("_ShadowParam1");
        public static readonly int _ShadowParam2 = Shader.PropertyToID ("_ShadowParam2");
        public static readonly int _ShadowParam3 = Shader.PropertyToID ("_ShadowParam3");
        public static readonly int _ShadowParam4 = Shader.PropertyToID ("_ShadowParam4");

        public static readonly int _SelfShadowVP = Shader.PropertyToID ("_SelfShadowVP");
        public static readonly int _SelfShadow2VP = Shader.PropertyToID ("_SelfShadow2VP");
        public static readonly int _ShadowMapSelfTex = Shader.PropertyToID ("_ShadowMapSelfTex");
        public static readonly int _SelfShadowParam = Shader.PropertyToID ("_SelfShadowParam");

        public static readonly int _ShadowMapParam0 = Shader.PropertyToID ("_ShadowMapParam0");
        public static readonly int _ShadowMapParam1 = Shader.PropertyToID ("_ShadowMapParam1");
        public static readonly int _ShadowLightCenter = Shader.PropertyToID ("_ShadowLightCenter");
        public static readonly int _DynamicShadowLightCenter = Shader.PropertyToID ("_DynamicShadowLightCenter");
        //public static readonly int _ShadowLightDir = Shader.PropertyToID ("_ShadowLightDir");
        public static readonly int _ShadowMapTex = Shader.PropertyToID ("_ShadowMapTex");
        public static readonly int _ShadowMapExtraTex = Shader.PropertyToID ("_ShadowMapExtraTex");
        public static readonly int _ShadowMapExtra1Tex = Shader.PropertyToID ("_ShadowMapExtra1Tex");

        public static readonly int _ShadowMapExtra1Center = Shader.PropertyToID ("_ShadowMapExtra1Center");
        public static readonly int _ShadowMapExtra1VP = Shader.PropertyToID ("_ShadowMapExtra1VP");
        public static readonly int _ShadowMapSize = Shader.PropertyToID ("_ShadowMapSize");
        public static readonly int _ShadowMapFade = Shader.PropertyToID ("_ShadowMapFade");
        public static readonly int _ShadowColor = Shader.PropertyToID ("_ShadowColor");
        public static readonly int _RoleShadowColor = Shader.PropertyToID ("_RoleShadowColor");
        public static readonly int _SelfShadowDir = Shader.PropertyToID ("_SelfShadowDir");
        public static readonly int _ObjCenter = Shader.PropertyToID ("_ObjCenter");

        public static readonly int _DepthShadowTex = Shader.PropertyToID("_DepthShadowTex");
        public static readonly int _DepthShadowMartrix = Shader.PropertyToID("_DepthsShadowMatrix");
        public static RenderTargetHandle _TempBlurRT = new RenderTargetHandle ("_TempBlurRT");


        // public static RenderTargetHandle _ShadowExtraRT = new RenderTargetHandle ("_ShadowExtraRT");
#if UNITY_EDITOR

        [NonSerialized]
        private Bounds sceneAABB = new Bounds ();

        private ShadowCullContext cc;

        private OctTreeNode octTree = new OctTreeNode ();
        private bool isOctTreeInit = false;
        private Vector3 lastLookAtPos;
        private Quaternion lastCameraRot;
        private List<Renderer> dynamicShadow = new List<Renderer> ();
        //public static RenderTexture[] shadowSelfRTRef = new RenderTexture[3];
        public static RenderTexture[] debugShadowRTs = new RenderTexture[(int) ShadowMapDebug.EditorTerrainShadow];
        // public static RenderTexture debugShadowRT;
        // public static RenderTexture debugShadowExtraRT;
        // public static RenderTexture debugShadowTmpRT;
        public static RenderTexture debugShadowTerrainRT;
        public static bool drawSelfShadow = false;

        public static bool shadowDirty = false;
        private Vector3 lastShadowProjDir = new Vector3 (0, -1, 0);
        private int delayFrame = 0;

        private int rebuildCSM = 0;
        private static HashSet<int> shadowObjPool = new HashSet<int> ();
        private static HashSet<int> lastShadowObjPool = new HashSet<int> ();
        public static List<Transform> staticShadowList = new List<Transform>();
        public static List<Transform> extraShadowList = new List<Transform> ();
        public static List<Transform> mainShadowList = new List<Transform>();
        public static ShadowRender[] selfShadow = new ShadowRender[3];
        //public static Transform[] depthShadow = new Transform[3];
        private ObjectShadow[] chunkShadows;
        public static SavedBool drawOctTree;
        public static SavedInt selfShadowMapIndex;
        public static SavedBool drawCSM0;
        public static SavedBool drawCSM1;
        public static SavedBool drawCSM2;
        public static SavedBool drawExtra;
        public static SavedBool drawExtra2;

        public static SavedBool drawCullingFrustum;
        public static SavedBool drawCullingObject;


        public static readonly int _ShadowMapTerrainTex = Shader.PropertyToID ("_ShadowMapTerrainTex");
        public static readonly int _ShadowMapTerrainParam = Shader.PropertyToID ("_ShadowMapTerrainParam");
#endif
#if UNITY_EDITOR
        public override void Start (EngineContext context, IRenderContext renderContext)
        {
            shadowDirty = false;
            rebuildCSM = 0;
            shadowObjPool.Clear ();
            lastShadowObjPool.Clear ();
            Shader.SetGlobalVector(ShaderManager._ShadowMoveOffset, Vector4.zero);
        }
        public override void BeginDump ()
        {
            base.BeginDump ();
            AddKeyName (_SelfShadowParam, "_SelfShadowParam");
            AddKeyName (_ShadowMapSize, "_ShadowMapSize");
            //AddKeyName (_ShadowLightDir, "_ShadowLightDir");
            AddKeyName (_ShadowParam0, "_ShadowParam0");
            AddKeyName (_ShadowParam1, "_ShadowParam1");
            AddKeyName (_ShadowMapFade, "_ShadowMapFade");
            AddKeyName (_ShadowColor, "_ShadowColor");
            AddKeyName (_ShadowParam2, "_ShadowParam2");
            AddKeyName (_ShadowParam3, "_ShadowParam3");
            AddKeyName (_CloudMap, "_CloudMap");
        }
        private static void DrawCSMAABB (ref ShadowDrawContext sdc, float scale)
        {
            Gizmos.DrawWireCube (sdc.sceneBound.center, sdc.sceneBound.size * scale);
            var debugBatch = sdc.shadowBatchDebug;
            if (debugBatch != null)
            {
                for (int i = 0; i < debugBatch.Count; ++i)
                {
                    var rb = debugBatch[i];
                    if (rb.mesh != null)
                    {
                        Bounds aabb = rb.mesh.bounds;
                        aabb.center = new Vector3(rb.matrix[0, 3], rb.matrix[1, 3], rb.matrix[2, 3]);
                        Gizmos.DrawWireCube(aabb.center, aabb.size * scale);
                        if (rb.matrix.ValidTRS())
                            Gizmos.DrawWireMesh(rb.mesh, 0, aabb.center, rb.matrix.rotation, rb.matrix.lossyScale);
                    }
                    else if (rb.render != null)
                    {
                        Gizmos.DrawWireCube(rb.render.bounds.center, rb.render.bounds.size * scale);
                    }
                }
            }

        }

        private static void DrawShadowFrustum (EngineContext context, ShadowModify modify,
            ref SelfShadow selfShadow, Color c)
        {
            var config = GetShadowConfig (ref selfShadow);
            Vector3 lightDir = (config.flag & SelfShadowConfig.Flag_CustomLightDir) != 0 ?
                config.lightDir : EngineContext.roleLightDir;
            Gizmos.color = c;
            Vector3 near = selfShadow.lightCenter;
            Gizmos.DrawWireSphere (near, 0.1f);
            Vector3 p0 = near - selfShadow.w * modify.selfShadowLightProjectRight - selfShadow.h * modify.selfShadowLightProjectUp;
            Vector3 p1 = near + selfShadow.w * modify.selfShadowLightProjectRight - selfShadow.h * modify.selfShadowLightProjectUp;
            Vector3 p2 = near + selfShadow.w * modify.selfShadowLightProjectRight + selfShadow.h * modify.selfShadowLightProjectUp;
            Vector3 p3 = near - selfShadow.w * modify.selfShadowLightProjectRight + selfShadow.h * modify.selfShadowLightProjectUp;
            Gizmos.DrawLine (p0, p1);
            Gizmos.DrawLine (p1, p2);
            Gizmos.DrawLine (p2, p3);
            Gizmos.DrawLine (p3, p0);

            Vector3 far = selfShadow.lightCenter + selfShadow.far * lightDir;

            Gizmos.DrawWireSphere (far, 0.1f);
            Gizmos.DrawLine (near, far);

            Vector3 p4 = far - selfShadow.w * modify.selfShadowLightProjectRight - selfShadow.h * modify.selfShadowLightProjectUp;
            Vector3 p5 = far + selfShadow.w * modify.selfShadowLightProjectRight - selfShadow.h * modify.selfShadowLightProjectUp;
            Vector3 p6 = far + selfShadow.w * modify.selfShadowLightProjectRight + selfShadow.h * modify.selfShadowLightProjectUp;
            Vector3 p7 = far - selfShadow.w * modify.selfShadowLightProjectRight + selfShadow.h * modify.selfShadowLightProjectUp;
            Gizmos.DrawLine (p4, p5);
            Gizmos.DrawLine (p5, p6);
            Gizmos.DrawLine (p6, p7);
            Gizmos.DrawLine (p7, p4);
            Gizmos.DrawLine (p0, p4);
            Gizmos.DrawLine (p1, p5);
            Gizmos.DrawLine (p2, p6);
            Gizmos.DrawLine (p3, p7);
        }
        public void UpdateDebug()
        {
            if (drawOctTree == null)
            {
                drawOctTree = new SavedBool($"{EngineContext.sceneNameLower}.{nameof(drawOctTree)}", false);
            }
            else
            {
                drawOctTree.Reset($"{EngineContext.sceneNameLower}.{nameof(drawOctTree)}");
            }

            if (selfShadowMapIndex == null)
            {
                selfShadowMapIndex = new SavedInt($"{EngineContext.sceneNameLower}.{nameof(selfShadowMapIndex)}", -1);
            }
            else
            {
                selfShadowMapIndex.Reset($"{EngineContext.sceneNameLower}.{nameof(selfShadowMapIndex)}");
            }
            if (drawCSM0 == null)
            {
                drawCSM0 = new SavedBool($"{EngineContext.sceneNameLower}.{nameof(drawCSM0)}", false);
            }
            else
            {
                drawCSM0.Reset($"{EngineContext.sceneNameLower}.{nameof(drawCSM0)}");
            }
            if (drawCSM1 == null)
            {
                drawCSM1 = new SavedBool($"{EngineContext.sceneNameLower}.{nameof(drawCSM1)}", false);
            }
            else
            {
                drawCSM1.Reset($"{EngineContext.sceneNameLower}.{nameof(drawCSM1)}");
            }
            if (drawCSM2 == null)
            {
                drawCSM2 = new SavedBool($"{EngineContext.sceneNameLower}.{nameof(drawCSM2)}", false);
            }
            else
            {
                drawCSM2.Reset($"{EngineContext.sceneNameLower}.{nameof(drawCSM2)}");
            }
            if (drawExtra == null)
            {
                drawExtra = new SavedBool($"{EngineContext.sceneNameLower}.{nameof(drawExtra)}", false);
            }
            else
            {
                drawExtra.Reset($"{EngineContext.sceneNameLower}.{nameof(drawExtra)}");
            }
            if (drawExtra2 == null)
            {
                drawExtra2 = new SavedBool($"{EngineContext.sceneNameLower}.{nameof(drawExtra2)}", false);
            }
            else
            {
                drawExtra2.Reset($"{EngineContext.sceneNameLower}.{nameof(drawExtra2)}");
            }
            if (drawCullingFrustum == null)
            {
                drawCullingFrustum = new SavedBool($"{EngineContext.sceneNameLower}.{nameof(drawCullingFrustum)}", false);
            }
            else
            {
                drawCullingFrustum.Reset($"{EngineContext.sceneNameLower}.{nameof(drawCullingFrustum)}");
            }
            if (drawCullingObject == null)
            {
                drawCullingObject = new SavedBool($"{EngineContext.sceneNameLower}.{nameof(drawCullingObject)}", false);
            }
            else
            {
                drawCullingObject.Reset($"{EngineContext.sceneNameLower}.{nameof(drawCullingObject)}");
            }

    }

        private static void DrawCSMGizmos(EngineContext context,
            ref ShadowContext csm,
            ref Matrix4x4 matrix, int index,
            float objectScale, Color objectColor,
            SavedBool draw, SavedBool drawFrustum, SavedBool drawObject)
        {
            if (draw != null && draw.Value)
            {
                if (drawFrustum != null && drawFrustum.Value)
                {
                    EngineUtility.DrawParallel3D(ref csm, ref context.shadowProjDir, Color.yellow);
                    var center = matrix.GetRow(index);
                    Gizmos.DrawSphere(center, 1);
                }
                if (drawObject != null && drawObject.Value)
                {
                    Gizmos.color = objectColor;
                    DrawCSMAABB(ref csm.drawContext, objectScale);
                }
            }
        }
        private static void DrawCSMGizmos(EngineContext context,
            ref ShadowContext csm,
            float objectScale, Color objectColor,
            SavedBool draw, SavedBool drawFrustum, SavedBool drawObject)
        {
            if (draw != null && draw.Value)
            {
                if (drawFrustum != null && drawFrustum.Value)
                {
                    EngineUtility.DrawParallel3D(ref csm, ref context.shadowProjDir, objectColor);
                }
                if (drawObject != null && drawObject.Value)
                {
                    Gizmos.color = objectColor;
                    DrawCSMAABB(ref csm.drawContext, objectScale);
                }
            }
        }
        public static void OnDrawGizmos (EngineContext context, EnvModify envModify, EnvSetting src)
        {
            ShadowModify modify = envModify as ShadowModify;

           
            if (drawOctTree != null && drawOctTree.Value)
            {
                Gizmos.color = Color.green;
                modify.octTree.Draw (false);
            }
            DrawCSMGizmos(context, ref context.csm0, ref context.shadowMapParam0, 0, 1, Color.blue, drawCSM0, drawCullingFrustum, drawCullingObject);
            DrawCSMGizmos(context, ref context.csm1, ref context.shadowMapParam0, 1, 1.01f, Color.red, drawCSM1, drawCullingFrustum, drawCullingObject);
            DrawCSMGizmos(context, ref context.csm2, ref context.shadowMapParam1, 0, 1.02f, Color.white, drawCSM2, drawCullingFrustum, drawCullingObject);
            DrawCSMGizmos(context, ref context.csmExtra, 1, Color.green, drawExtra, drawCullingFrustum, drawCullingObject);
            DrawCSMGizmos(context, ref context.csmExtra1, 1, Color.white, drawExtra2, drawCullingFrustum, drawCullingObject);


            //if (modify.settings.drawSingleObject)
            //{
            //    var it = context.singleShadowObjects.GetEnumerator();
            //    while (it.MoveNext())
            //    {
            //        var ssObject = it.Current.Value;
            //        DrawShadowFrustum(context, modify, ssObject, Color.cyan);

            //    }
            //}

            if (selfShadowMapIndex != null &&
                selfShadowMapIndex.Value >= 0 &&
                selfShadowMapIndex.Value < context.selfShadowInfo.Length)
            {
                ref var selfShadowInfo = ref context.selfShadowInfo[selfShadowMapIndex.Value];
                DrawShadowFrustum(context, modify, ref selfShadowInfo, Color.blue);
                Gizmos.DrawWireCube(selfShadowInfo.shadowCenter, selfShadowInfo.shadowAABB.size);
            }

           
            //if (modify.settings.csmTest.t != null)
            //{
            //    if (modify.settings.csmTest.t.TryGetComponent<Renderer> (out var r))
            //    {
            //        AABB aabb = AABB.Create (r.bounds);
            //        context.csmExtra.CalcQuad (ref aabb,
            //            out var xMin, out var xMax, out var zMin, out var zMax);

            //        Gizmos.color = Color.red;
            //        float y = aabb.max.y;
            //        Vector3 p0 = new Vector3 (xMin, y, zMin);
            //        Vector3 p1 = new Vector3 (xMax, y, zMin);
            //        Vector3 p2 = new Vector3 (xMax, y, zMax);
            //        Vector3 p3 = new Vector3 (xMin, y, zMax);
            //        Gizmos.DrawLine (p0, p1);
            //        Gizmos.DrawLine (p1, p2);
            //        Gizmos.DrawLine (p2, p3);
            //        Gizmos.DrawLine (p3, p0);
            //    }
            //}
        }
        private EditorCommon.EnumTransform initAABB = null;
        private void InitAABB (Transform trans, object param)
        {
            ShadowModify sm = param as ShadowModify;
            Renderer r;
            if (trans.TryGetComponent<Renderer> (out r))
            {
                if (r != null)
                {
                    Bounds bounds = r.bounds;
                    sm.sceneAABB.Encapsulate (bounds);
                }
            }
            EditorCommon.EnumChildObject (trans, param, initAABB);
        }

        private EditorCommon.EnumTransform addAABB = null;
        private void AddAABB (Transform trans, object param)
        {
            ShadowModify sm = param as ShadowModify;
            Renderer r;
            if (trans.TryGetComponent<Renderer> (out r))
            {
                if (r != null &&
                    r.gameObject.activeInHierarchy &&
                    r.enabled &&
                    //!r.name.StartsWith ("Chunk_") &&
                    r.shadowCastingMode != ShadowCastingMode.Off)
                {
                    trans.TryGetComponent<MeshRenderObject> (out var mro);
                    if (mro != null)
                    {
                        // if (mro.dynamicShadow)
                        // {
                        //     dynamicShadow.Add (r);
                        // }
                        // else
                        {
                            Bounds bounds = r.bounds;
                            AABB objBound = AABB.zero;
                            objBound.min = bounds.min;
                            objBound.max = bounds.max;
                            SceneOctTreeData sotd = SharedObjectPool<SceneOctTreeData>.Get ();
                            sotd.r = r;
                            sotd.mro = mro;
                            sotd.objectBoundWS = objBound;
                            sotd.valid = true;
                            sotd.mesh = mro.GetMesh ();
                            if (!sm.octTree.Add (sotd, ref objBound))
                            {
                                sm.octTree.AddAABB (ref objBound);
                                sm.octTree.AddToNode (sotd);
                            }

                            int hash = r.GetHashCode ();
                            if (!lastShadowObjPool.Remove (hash))
                            {
                                shadowDirty = true;
                            }
                            if (!shadowObjPool.Contains (hash))
                            {
                                shadowObjPool.Add (hash);
                            }
                            bool isChange = mro.UpdateShadowState (EngineContext.instance);
                            if (!shadowDirty)
                                shadowDirty |= isChange;
                        }

                    }
                }
            }

            EditorCommon.EnumChildObject (trans, param, addAABB);
        }
        private void RefreshOctTree (EngineContext engineContext)
        {
            if (initAABB == null)
            {
                initAABB = InitAABB;
            }
            if (addAABB == null)
            {
                addAABB = AddAABB;
            }
            Vector3 size = new Vector3 (EngineContext.ChunkSize, 1, EngineContext.ChunkSize);
            sceneAABB = new Bounds (new Vector3 (EngineContext.ChunkSize * 0.5f, 1, EngineContext.ChunkSize * 0.5f), size);
            if (GlobalContex.terrainData != null)
            {
                size = GlobalContex.terrainData.size;
                Bounds terrainBound = GlobalContex.terrainData.bounds;
                sceneAABB = terrainBound;
            }

            for (int i = 0; i < staticShadowList.Count; ++i)
            {
                var group = staticShadowList[i];
                if (group != null)
                    initAABB (group, this);
            }
            if(!isOctTreeInit)
            {
                isOctTreeInit = true;
                octTree.Init(size.x, size.z, sceneAABB.size.y, 0, 0, sceneAABB.min.y);
            }
            octTree.Clear(size.x, size.z);
            shadowDirty = false;
            shadowObjPool.Clear ();
            int lastShadowCount = lastShadowObjPool.Count;
            for (int i = 0; i < staticShadowList.Count; ++i)
            {
                var group = staticShadowList[i];
                if (group != null)
                    addAABB (group, this);
            }
            staticShadowList.Clear();
            if (lastShadowObjPool.Count != 0 || lastShadowCount != shadowObjPool.Count)
            {
                shadowDirty = true;
            }
            lastShadowObjPool.Clear ();
            foreach (var hash in shadowObjPool)
            {
                lastShadowObjPool.Add (hash);
            }
        }
        private void PreUpdateShadowCulling (EngineContext context)
        {
            if (context != null && !EngineContext.IsRunning)
            {
                if (chunkShadows == null || chunkShadows.Length != context.xChunkCount * context.zChunkCount)
                {
                    chunkShadows = new ObjectShadow[context.xChunkCount * context.zChunkCount];
                }
                int frameCount = Time.frameCount % 60; //settings.updateFrame == 0 ? 0 : Time.frameCount % 30;
                if (frameCount == 0)
                {
                    RefreshOctTree(context);
                }
            }

        }
       
        private static void CullSelfShadowObjectCSM (ShadowRender sr, EngineContext context, 
            ref SelfShadow selfShadow)
        {
            Material depthShadowMat = WorldSystem.GetEffectMat(EEffectMaterial.HairShadow);
            foreach (var rp in sr.renders)
            {
                if (rp.r.enabled)
                {
                    AABB aabb = AABB.Create(rp.r.bounds);
                    var db = SharedObjectPool<DrawBatch>.Get();
                    db.render = rp.r;
                    db.mat = rp.shadowCaster;
                    db.passID = EngineContext.ShadowSliceSelf;
                    db.mpbRef = rp.mpb;
                    if (selfShadow.drawCalls.count == 0)
                    {
                        selfShadow.shadowAABB.Init(ref aabb);
                    }
                    else
                    {
                        selfShadow.shadowAABB.Encapsulate(ref aabb);
                    }
                    selfShadow.drawCalls.Push(db);
                    //selfShadow.shadowBatchDebug.Add(RenderBatch.Create(db));

                    //depth
                    var depthdb = SharedObjectPool<DrawBatch>.Get();
                    depthdb.render = rp.r;
                    depthdb.mat = depthShadowMat;
                    depthdb.mpbRef = rp.mpb;

                    if (rp.isHair)
                    {
                        depthdb.passID = EngineContext.DepthShadowHair;
                        selfShadow.drawCallsdepthhair.Push(depthdb);
                    }
                    else if (rp.isFace)
                    {
                        selfShadow.shadowdepthAABB.Init(ref aabb);
                        depthdb.passID = EngineContext.DepthShadow;
                        selfShadow.drawCallsdepthface.Push(depthdb);
                        context.renderflag.SetFlag(EngineContext.RFlag_UISceneShadow, true);
                    }
                }
            }
        }
        private void CullDynamicObjectCSM (EngineContext context, Renderer r, ref ShadowContext sc, bool noTest)
        {
            if (r.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off &&
                r.gameObject.layer != DefaultGameObjectLayer.InVisiblityLayer)
            {
                AABB aabb = AABB.Create (r.bounds);
                SceneCullSystem.AddShadowBatch (context, ref sc, r, ref aabb);
            }
        }

        private void CullDynamicObjectCSM (EngineContext context, Transform t, ref ShadowContext sc, bool noTest = false)
        {
            if (t != null)
            {
                if(t.TryGetComponent<Renderer>(out var r))
                {
                    if (r != null && r.enabled && t.gameObject.activeInHierarchy)
                    {
                        CullDynamicObjectCSM(context, r, ref sc, noTest);
                    }
                }   
                for (int i = 0; i < t.childCount; ++i)
                {
                    CullDynamicObjectCSM (context, t.GetChild (i), ref sc, noTest);
                }
            }
        }

        private void CullCSM (EngineContext context, RenderContext rc, 
            ref ShadowContext csm, ref Matrix4x4 shadowVP, int startRow, uint flag, bool isFitAABB = false)
        {
            int lastCount = csm.DCCount;
            csm.Reset ();
            octTree.Cull(ref cc, ref csm);
            cc.cullCounter++;
            if(isFitAABB)
            {
                SceneCullSystem.CalcDynamicLightFurstum(context, ref csm, ref shadowVP, startRow);
                csm.CalcMatrix(context, ref shadowVP, startRow);
            }
            else
            {
                SceneCullSystem.CalcLightFurstum(context, ref csm, ref shadowVP, startRow);
            }
            context.shadowFlag.SetFlag(flag, true);
        }

        private void CullSelfShadow(EngineContext context, ShadowRender sr, int index)
        {
            WorldSystem.ClearSelfShadow(index);
            if (sr != null)
            {
                ref var selfShadowInfo = ref context.selfShadowInfo[index];
                if (selfShadowInfo.shadowBatchDebug == null)
                {
                    selfShadowInfo.shadowBatchDebug = new List<RenderBatch>();
                }
                selfShadowInfo.shadowBatchDebug.Clear();
                CullSelfShadowObjectCSM(sr, context, ref selfShadowInfo);
                selfShadowInfo.configIndex = 0;
                selfShadowInfo.shadowCenter = selfShadowInfo.shadowAABB.center;
            }
        }

        private void UpdateSingleShadow(EngineContext context)
        {
            if (cc.singleShadowMesh != null)
            {
                //var it = cc.singleShadowMesh.GetEnumerator ();
                //int i = 0;
                //while (it.MoveNext () && i < context.singleShadow.Length)
                //{
                //    var kvp = it.Current;
                //    if (kvp.Value.Count > 0)
                //    {
                //        ref SingleShadowMap sf = ref context.singleShadow[i];
                //        sf.mesh = kvp.Key;
                //        var mro = kvp.Value[0];
                //        var aabb = mro.aabb;
                //        var center = new Vector3 (0, aabb.sizeY * 0.5f, 0);
                //        aabb.Set (ref center);
                //        sf.aabb = aabb;
                //        sf.mpb = mro.GetMPB ();
                //        i++;
                //        sf.dirty = true;
                //    }
                //}
            }
        }

        //private void CullDepthShadow (EngineContext context, RenderContext rc, Transform t, int index)
        //{
        //    if (index >= context.selfShadowInfo.Length || t == null)
        //    {
        //        return;
        //    }

        //    if (t.gameObject.activeSelf && t.TryGetComponent(out Renderer r))
        //    {
        //        context.renderflag.SetFlag(EngineContext.RFlag_UISceneShadow, true);
        //        Material depthShadowMat = WorldSystem.GetEffectMat(EEffectMaterial.HairShadow);
        //        var db = SharedObjectPool<DrawBatch>.Get();            
        //        db.render = r;
        //        db.mat = depthShadowMat;    
        //        db.mpbRef = CommonObject<MaterialPropertyBlock>.Get();
        //        string name = r.name.ToLower();
        //        ref var hairShadow1 = ref context.selfShadowInfo[index];
        //        if (name.EndsWith("_hair"))
        //        {                
        //            var mat = r.sharedMaterial;
        //            if (mat != null)
        //            {
        //                mat.EnableKeyword("_SHADOW_MAP");
        //            }
        //            db.passID = EngineContext.DepthShadowHair;
        //            hairShadow1.drawCallsdepthhair.Push(db);
        //        }
        //        if (name.EndsWith("_face"))
        //        {
        //            Bounds rb = r.bounds;
        //            //  rb.extents = rb.extents + new Vector3(0.01f,0.01f,0.01f);
        //            AABB aabb = AABB.Create(rb);              
        //           hairShadow1.shadowdepthAABB.Init(ref aabb);                
        //            db.passID = EngineContext.DepthShadow;
        //            hairShadow1.drawCallsdepthface.Push(db);
        //        }
        //    }
        //    for (int i = 0; i < t.childCount; ++i)
        //    {
        //        CullDepthShadow(context, rc, t.GetChild(i),index);
        //    }
        //}

        private void UpdateShadowCulling (EngineContext context, RenderContext rc)
        {
            if (!EngineContext.IsRunning &&
                context != null)
            {
                cc.rc = rc;
                cc.lightDir = context.shadowProjDir;
                cc.invSin = context.invSin;
                #region editor dynamic shadow collect

                //dynamic shadow
                context.csmExtra.Reset();
                context.csmExtra.parallel3D.scale = 1;
                context.csmExtra1.Reset();
                context.csmExtra1.parallel3D.scale = 1;
                for (int i = 0; i < mainShadowList.Count; ++i)
                {
                    CullDynamicObjectCSM(context, mainShadowList[i], ref context.csmExtra, true);
                }
                mainShadowList.Clear();
                for (int i = 0; i < extraShadowList.Count; ++i)
                {
                    CullDynamicObjectCSM(context, extraShadowList[i], ref context.csmExtra1, true);
                }
                extraShadowList.Clear();
                SceneCullSystem.PostProcessShadow(context);

                //self shadow
                context.renderflag.SetFlag(EngineContext.RFlag_UISceneShadow,false);
                for (int i = 0; i < selfShadow.Length; ++i)
                {
                    CullSelfShadow(context, selfShadow[i], i);
                    selfShadow[i] = null;
                }

                ////depth shadow
                //context.renderflag.SetFlag(EngineContext.RFlag_UISceneShadow, false);
                //for (int i = 0; i < depthShadow.Length; ++i)
                //{
                //    CullDepthShadow(context, rc, depthShadow[i], i);
                //    depthShadow[i] = null;
                //}

                UpdateSingleShadow(context);
                #endregion
                #region cull static shadow
                if (settings.shadowMode != ShadowMode.OnlyPlayerShadow &&
                    !context.renderflag.HasFlag(EngineContext.RFlag_DisableCalcCaluceShadow))
                {
                    float d0 = (context.shadowProjDir - lastShadowProjDir).sqrMagnitude;
                    float d1 = (lastLookAtPos - context.lookAtPos).sqrMagnitude;
                    Quaternion q = context.CameraTransCache.rotation;
                    float d2 = new Vector4 (
                        lastCameraRot.x - q.x,
                        lastCameraRot.y - q.y,
                        lastCameraRot.z - q.z,
                        lastCameraRot.w - q.w).sqrMagnitude;
                    if (d0 > 0.001f || d1 > 0.001f || d2 > 0.001f)
                    {
                        shadowDirty = true;
                    }
                    lastShadowProjDir = context.shadowProjDir;
                    lastLookAtPos = context.lookAtPos;
                    lastCameraRot = q;
                    if (shadowDirty)
                    {
                        delayFrame = 3;
                        shadowDirty = false;
                    }
                    else if (delayFrame >= 0)
                    {
                        delayFrame--;
                    }
                    if (delayFrame == 0)
                    {
                       // delayFrame = 0;
                        if (cc.singleShadowMesh != null)
                            cc.singleShadowMesh.Clear ();
                        rebuildCSM = 0;
                    }
                    // rebuildCSM = 0;
                    float h = context.cameraPos.y;
                    switch (rebuildCSM)
                    {
                        case 0:
                            {
                                CullCSM (context, rc, ref context.csm0, ref context.shadowMapParam0, 0, 
                                    EngineContext.ShadowFlag_CSM0RenderingDirty);
                                rebuildCSM = 1;
                                //DebugLog.AddEngineLog2 ("Cull CSM0:{0} frame:{1}", rc.workingBatchCount.ToString (), context.frameCount.ToString ());
                            }
                            break;
                        case 1:
                            {
                                CullCSM (context, rc, ref context.csm1, ref context.shadowMapParam0, 2,
                                    EngineContext.ShadowFlag_CSM1RenderingDirty);
                                rebuildCSM = 2;
                                //DebugLog.AddEngineLog2 ("Cull CSM1:{0} frame:{1}", rc.workingBatchCount.ToString (), context.frameCount.ToString ());
                            }
                            break;
                        case 2:
                            {
                                CullCSM(context, rc, ref context.csm2, ref context.shadowMapParam1, 0,
                                    EngineContext.ShadowFlag_CSM2RenderingDirty, true);
                                //DebugLog.AddEngineLog2 ("Cull CSM2:{0} frame:{1}", rc.workingBatchCount.ToString (), context.frameCount.ToString ());

                                rebuildCSM = -1;
                            }
                            break;
                    }
                }
                #endregion
            }
        }
#endif
        //self shadow
        private static Matrix4x4 CalculateBaseAxis(EngineContext context, ref Vector3 lightDir,
            out Vector3 lightProjectRight, out Vector3 lightProjectUp)
        {
            float dot = Mathf.Abs(Vector3.Dot(Vector3.up, lightDir));
            if (dot > 0.99f)
            {
                Vector3 lookAt = context.lookAtPos;
                lightProjectRight = context.cameraPos - lookAt;
                lightProjectUp = Vector3.Cross(lightDir, lightProjectRight);
                lightProjectUp.Normalize();
                lightProjectRight = Vector3.Cross (lightProjectUp, lightDir);
            }
            else
            {
                lightProjectUp = Vector3.up;
                lightProjectRight = Vector3.Cross (lightProjectUp, lightDir);
                lightProjectRight.Normalize ();
                lightProjectUp = Vector3.Cross (lightDir, lightProjectRight);
                lightProjectUp.Normalize ();
            }

            Matrix4x4 baseAxis = Matrix4x4.identity;
            baseAxis.SetRow (0, lightProjectRight);
            baseAxis.SetRow (1, lightProjectUp);
            baseAxis.SetRow (2, -lightDir);

            return baseAxis;
        }

        private void TestMinMax (ref Vector3 boundPoint, ref Vector3 minValue, ref Vector3 maxValue)
        {
            // max //
            if (boundPoint.x > maxValue.x)
            {
                maxValue.x = boundPoint.x;
            }

            if (boundPoint.y > maxValue.y)
            {
                maxValue.y = boundPoint.y;
            }

            if (boundPoint.z > maxValue.z)
            {
                maxValue.z = boundPoint.z;
            }

            // min //
            if (boundPoint.x < minValue.x)
            {
                minValue.x = boundPoint.x;
            }

            if (boundPoint.y < minValue.y)
            {
                minValue.y = boundPoint.y;
            }

            if (boundPoint.z < minValue.z)
            {
                minValue.z = boundPoint.z;
            }
        }
        private void FindMaxAndMin (ref AABB worldAABB,
            ref Matrix4x4 matrixV, ref Vector3 minValue, ref Vector3 maxValue)
        {
            minValue.x = minValue.y = minValue.z = float.MaxValue;
            maxValue.x = maxValue.y = maxValue.z = float.MinValue;

            float halfX = worldAABB.sizeX * 0.5f;
            float halfY = worldAABB.sizeY * 0.5f;
            float halfZ = worldAABB.sizeZ * 0.5f;

            // SceneBound 在世界空间的8个点 //
            Vector3 boundPoint = matrixV.MultiplyPoint3x4 (worldAABB.center + new Vector3 (-halfX, -halfY, -halfZ));
            TestMinMax (ref boundPoint, ref minValue, ref maxValue);
            boundPoint = matrixV.MultiplyPoint3x4 (worldAABB.center + new Vector3 (halfX, -halfY, -halfZ));
            TestMinMax (ref boundPoint, ref minValue, ref maxValue);
            boundPoint = matrixV.MultiplyPoint3x4 (worldAABB.center + new Vector3 (halfX, -halfY, halfZ));
            TestMinMax (ref boundPoint, ref minValue, ref maxValue);
            boundPoint = matrixV.MultiplyPoint3x4 (worldAABB.center + new Vector3 (-halfX, -halfY, halfZ));
            TestMinMax (ref boundPoint, ref minValue, ref maxValue);
            boundPoint = matrixV.MultiplyPoint3x4 (worldAABB.center + new Vector3 (-halfX, halfY, -halfZ));
            TestMinMax (ref boundPoint, ref minValue, ref maxValue);
            boundPoint = matrixV.MultiplyPoint3x4 (worldAABB.center + new Vector3 (halfX, halfY, -halfZ));
            TestMinMax (ref boundPoint, ref minValue, ref maxValue);
            boundPoint = matrixV.MultiplyPoint3x4 (worldAABB.center + new Vector3 (halfX, halfY, halfZ));
            TestMinMax (ref boundPoint, ref minValue, ref maxValue);
            boundPoint = matrixV.MultiplyPoint3x4 (worldAABB.center + new Vector3 (-halfX, halfY, halfZ));
            TestMinMax (ref boundPoint, ref minValue, ref maxValue);
        }

        private static SelfShadowConfig GetShadowConfig(ref SelfShadow selfShadow)
        {
            SelfShadowConfig config = SelfShadowConfig.defaultConfig;
            if (WorldSystem.miscConfig != null)
            {
                if (selfShadow.configIndex == MiscConfig.RuntimeSelfShadowConfigIndex)
                {
                    config = WorldSystem.miscConfig.runtimeSelfShadowConfig;
                }
                else if (selfShadow.configIndex > 0)
                {
                    int configIndex = selfShadow.configIndex - 1;
                    var configs = WorldSystem.miscConfig.selfShadowConfig;
                    if (configs != null && configIndex < configs.Length)
                    {
                        config = configs[configIndex];
                    }
                    else
                    {
                        config = WorldSystem.miscConfig.defaultShadowConfig;
                    }
                }
                else
                {
                    config = WorldSystem.miscConfig.defaultShadowConfig;
                }
            }
            return config;
        }

        private void CalcShadowMatrix(Vector3 center, Vector3 offset, Vector3 scale,
            ref AABB aabb, ref Vector3 lightDir, ref Matrix4x4 baseAxis,
            ref Matrix4x4 viewMatrix, ref Matrix4x4 projMatrix
#if UNITY_EDITOR
                ,
            ref Vector3 lightCenter,
            ref float length,
            ref float width,
            ref float hight
#endif
            ,bool isdepthshadow=false
            )
        {
            Matrix4x4 matrixV = Matrix4x4.identity;
            Vector3 lightPosWS = center - lightDir * 1000;

            Matrix4x4 translate = Matrix4x4.Translate(-lightPosWS);
            matrixV = baseAxis * translate;

            // 把世界空间包围盒转到灯光空间 //
            // 求出最大最小值 //
            Vector3 minPtLS = Vector3.zero; // 灯光空间最小坐标 //
            Vector3 maxPtLS = Vector3.zero; // 灯光空间最大坐标 //            
            FindMaxAndMin(ref aabb, ref matrixV, ref minPtLS, ref maxPtLS);

            Vector3 frustumCenterPos = (maxPtLS + minPtLS) * 0.5f;
            Vector3 lightPosLS = new Vector3(frustumCenterPos.x, frustumCenterPos.y, maxPtLS.z);

            // 加上Offset //
            lightPosLS += offset;
            // 灯光的世界空间坐标 //
            lightPosWS = matrixV.inverse.MultiplyPoint3x4(lightPosLS);
            Vector3 whz = maxPtLS - minPtLS;

            // 加上Scale //
            whz.x *= scale.x;
            whz.y *= scale.y;
            whz.z *= scale.z;
            float far = whz.z;
            float w = whz.x * 0.5f;
            float h = whz.y * 0.5f;
#if UNITY_EDITOR
            lightCenter = lightPosWS;
            length = far;
            width = w;
            hight = h;
#endif
            // 修正V矩阵 //
            translate = Matrix4x4.Translate(-lightPosWS);
            viewMatrix = baseAxis * translate;
            if (isdepthshadow)
            {
                w = -w;
                h = -h;
            }
            projMatrix = Matrix4x4.Ortho(-w, w, -h, h,
                0.1f, far);
            projMatrix = GL.GetGPUProjectionMatrix(projMatrix, true);
        }

        private void CalcSelfShadowMatrix(EngineContext context,
            ref SelfShadow selfShadow, ref Vector3 lightDir, ref Matrix4x4 baseAxis, ref SelfShadowConfig config)
        {

            CalcShadowMatrix(selfShadow.shadowCenter, config.offset, config.scale,
                ref selfShadow.shadowAABB, ref lightDir, ref baseAxis,
                ref selfShadow.viewMatrix, ref selfShadow.projMatrix
#if UNITY_EDITOR
                ,
                ref selfShadow.lightCenter,
                ref selfShadow.far,
                ref selfShadow.w,
                ref selfShadow.h
#endif
                );
        }

        private void CalcDepthShadowMatrix(EngineContext context,
            ref SelfShadow selfShadow, ref Vector3 lightDir, ref Matrix4x4 baseAxis)
        {
            CalcShadowMatrix(selfShadow.shadowCenterdepth, Vector3.zero, Vector3.one,
                    ref selfShadow.shadowdepthAABB, ref lightDir, ref baseAxis,
                    ref selfShadow.viewMatrixdepth, ref selfShadow.projMatrixdepth
#if UNITY_EDITOR
                    ,
                    ref selfShadow.depthLightCenter,
                    ref selfShadow.depthFar,
                    ref selfShadow.depthW,
                    ref selfShadow.depthH
#endif
                ,true   );
        }

//        private void CalcSingleShadowMatrix(EngineContext context,
//            SingleShadowObject ssOject, ref Vector3 lightDir,
//            ref Matrix4x4 baseAxis)
//        {
//            CalcShadowMatrix(ssOject.aabb.center, Vector3.zero, Vector3.one,
//                ref ssOject.aabb, ref lightDir, ref baseAxis,
//                ref ssOject.viewMatrix, ref ssOject.projMatrix
//#if UNITY_EDITOR
//                ,
//                ref ssOject.lightCenter,
//                ref ssOject.far,
//                ref ssOject.w,
//                ref ssOject.h
//#endif
//                );
//        }

        

        //csm
        private void UpdateShadowFrustum(EngineContext context)
        {
            if (context.renderflag.HasFlag(EngineContext.RFlag_DisableCalcCaluceShadow))
                return;
            context.extraShadowSize = 8;
            context.extraShadowSize *= context.extraShadowSize;
            context.extraShadow1Size = settings.shadowParam0.value.y;
            context.extraShadow1Size *= context.extraShadow1Size;
            context.extraShadow1Scale = settings.shadowParam0.value.z;
            if (context.extraShadow1Scale > 1)
                context.extraShadow1Scale = 1;
            SceneCullSystem.UpdateShadowLod(context, settings.shadowParam0.value.x);
        }

        public override void Update(EngineContext context, IRenderContext renderContext)
        {
            var qs = QualitySettingData.current;
            if (qs.flag.HasFlag (QualitySet.Flag_EnableShadow))
            {
                UpdateShadowFrustum (context);
#if UNITY_EDITOR
                PreUpdateShadowCulling(context);
                UpdateShadowCulling (context, renderContext as RenderContext);
                UpdateDebug();
#endif
            }
        }

#if UNITY_EDITOR
        private void ClearShadowRT (CommandBuffer cb, ref ShadowContext csm, RenderTexture rt)
        {
            if (csm.DCCount == 0)
            {
                if (csm.drawContext.passID < 3)
                {
                    cb.SetRenderTarget (rt, 0, CubemapFace.Unknown, csm.drawContext.passID);
                }
                else
                {
                    cb.SetRenderTarget (rt);
                }
                cb.ClearRenderTarget (true, true, Color.clear, 1.0f);
            }
        }
#endif
        #region Common Shadow Batch
        private void DrawShadowDepthBatch(CommandBuffer cb, DrawBatch db, int passID)
        {
            if (db.mesh != null)
            {
                cb.DrawMesh(db.mesh, db.matrix, db.mat, 0, passID, db.mpbRef);
            }
            SharedObjectPool<DrawBatch>.Release(db);
        }
        private void DrawShadowBatch(CommandBuffer cb, ref OrderQueue drawCalls, int passID)
        {
            while (drawCalls.Pop<DrawBatch>(out var db))
            {
                if (db.mat != null)
                {
                    if (db.mesh != null)
                    {
                        cb.DrawMesh(db.mesh, db.matrix, db.mat, 0, passID, db.mpbRef);
                    }
                    else if (db.render != null && db.render.enabled)
                    {
                        cb.DrawRenderer(db.render, db.mat, 0, passID);
                    }
                }
                SharedObjectPool<DrawBatch>.Release(db);
            }
        }
        private void PrepareRT(EngineContext context, RenderContext rc,
            ref RenderTexture rt, int shadowMapSize, string name, int index, int slice,
            int shadowMapTexId)
        {
            if (rt == null && shadowMapSize > 0
#if UNITY_EDITOR
                ||
                rt.width != shadowMapSize
#endif
            )
            {
                rt = rc.GetShadowRT(shadowMapSize, shadowMapSize, slice, index, name);
                if (shadowMapTexId > 0)
                    Shader.SetGlobalTexture(shadowMapTexId, rt);
            }
        }
        #endregion

        #region Static Shadow Batch
        private void DrawStaticShadowBatch(EngineContext context, CommandBuffer cb,
            ref OrderQueue drawCalls, int passID, ref int drawShadowCount)
        {
            while (drawCalls.Pop<DrawBatch>(out var db))
            {
                if (db.mat != null)
                {
                    if (db.mh != null)
                    {
                        if (db.mh.GetMesh(context, out db.mesh, out db.mpbRef))
                        {
                            DrawShadowDepthBatch(cb, db, passID);
                            drawShadowCount--;
                        }                       
                        else
                        {
                            unloadDC.Add(db);
                        }
                    }
                    else
                    {
                        DrawShadowDepthBatch(cb, db, passID);
                        drawShadowCount--;
                    }
                }
                else
                {
                    SharedObjectPool<DrawBatch>.Release(db);
                }
                if (drawShadowCount == 0)
                {
                    break;
                }
            }
            for (int i = 0; i < unloadDC.Count; ++i)
            {
                var db = unloadDC[i];
                drawCalls.Push(db);
            }
            unloadDC.Clear();
        }

        private bool RenderStaticShadow(
            EngineContext context,
            RenderTexture rt,
            RenderTexture tmpRt,
            CommandBuffer cb,
            ref Matrix4x4 shadowVP,
            int shadowVPID,
            int shadowCenterID,
            ref ShadowContext csm,
            ref int drawShadowCount,
            uint flag)
        {
            if (context.shadowFlag.HasFlag(flag))
            {
                ref var drawContext = ref csm.drawContext;
                ref var drawCalls = ref drawContext.shadowDrawCall;
                int slice = csm.drawContext.passID;
                if (drawCalls.count > 0)
                {
                    int passID = 0;
                    bool directDraw = drawShadowCount == MaxShadowDrawCount;
                    if (directDraw)
                    {
                        cb.SetRenderTarget(rt, 0, CubemapFace.Unknown, slice);
                    }
                    else
                    {
                        cb.SetRenderTarget(tmpRt,
                        RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare,
                        RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
                    }
                    // cb.SetRenderTarget (rt, 0, CubemapFace.Unknown, slice);
                    if (csm.drawContext.clear)
                    {
                        //DebugLog.AddEngineLog2 ("clear tmp csm {0} count:{1}", slice.ToString (), drawCalls.count.ToString ());
                        cb.ClearRenderTarget(true, true, Color.clear, 1.0f);
                        csm.drawContext.clear = false;
                    }

                    cb.SetViewProjectionMatrices(Matrix4x4.identity, drawContext.projMatrix);
                    cb.SetGlobalVector(shadowCenterID, drawContext.shadowCenter);
                    DrawStaticShadowBatch(context, cb, ref drawCalls, passID, ref drawShadowCount);                   
                    if (drawCalls.count == 0)
                    {
                        if (!directDraw)
                        {
                            Material mat = WorldSystem.GetEffectMat(EEffectMaterial.ShadowCopy);
                            if (mat != null)
                            {
                                //DebugLog.AddEngineLog2 ("flush csm {0}", slice.ToString ());
                                cb.SetGlobalTexture(ShaderIDs.MainTex, tmpRt);
                                cb.SetRenderTarget(rt, 0, CubemapFace.Unknown, slice);

                                cb.ClearRenderTarget(true, true, Color.clear, 1.0f);
                                cb.DrawMesh(RuntimeUtilities.fullscreenTriangle, Matrix4x4.identity, mat, 0, 0);

                            }
                        }
                        cb.SetGlobalMatrix(shadowVPID, shadowVP);
                        context.shadowFlag.SetFlag(flag, false);
                        // cb.SetGlobalMatrix (shadowVPID, shadowVP);
                        // context.currentCSMIndex = -1;
                    }

                }
                else if (csm.drawContext.clear)
                {
                    // DebugLog.AddEngineLog2 ("clear csm {0}", slice.ToString ());
                    cb.SetRenderTarget(rt, 0, CubemapFace.Unknown, slice);
                    cb.ClearRenderTarget(true, true, Color.clear, 1.0f);
                    cb.SetGlobalMatrix(shadowVPID, shadowVP);
                    csm.drawContext.clear = false;
                    context.shadowFlag.SetFlag(flag, false);
                }
                return true;
                //if (drawCalls.count == 0)
                //{
                //    csm.shadowParam.z = EngineContext.CastShadowState_None;
                //}
            }
            return false;
        }

        private bool DrawStaticShadow(EngineContext context, RenderContext rc,
            int shadowMapSize, string name, int index,
            ref Matrix4x4 shadowMapParam, int shadowVPID,
            ref ShadowContext csm, ref int drawShadowCount, uint flag)
        {
            RenderTexture sceneShadowTmp = null;
            if (drawShadowCount != MaxShadowDrawCount)
            {
                PrepareRT(context, rc, ref sceneShadowTmp, shadowMapSize, name,
                        index, 0, -1);
            }
            var rt = shadowRTArray;
            bool isRender = RenderStaticShadow(context,
                rt,
                sceneShadowTmp,
                rc.preWorkingCmd,
                ref shadowMapParam,
                shadowVPID,
                _ShadowLightCenter,
                ref csm,
                ref drawShadowCount, flag);
#if UNITY_EDITOR
            if (!EngineContext.IsRunning && debugShadowTerrainRT != null)
            {
                RenderDebugShadow(context,
                    debugShadowTerrainRT,
                    rc.preWorkingCmd,
                    _ShadowMapTerrainParam,
                    _ShadowLightCenter,
                    ref context.csm2,
                    true);
            }
#endif
            return isRender;
        }

        private static int MaxShadowDrawCount = 100000;

        private bool TestOjbectLoad(EngineContext context, ref ObjectShadow objShadow)
        {
            ref var dc = ref objShadow.drawContext.drawContext;
            if (dc.shadowDrawCall.count > 0)
            {
                var it = dc.shadowDrawCall.BeginGet();
                while (dc.shadowDrawCall.Get(ref it, out DrawBatch db))
                {
                    if (db.mat != null)
                    {
                        if (db.mh != null)
                        {
                            if (!db.mh.GetMesh(context, out db.mesh, out db.mpbRef))
                            {
                                return false;
                            }
                        }
                        else if (db.mesh != null)
                        {

                        }
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool DrawObjectShadow(EngineContext context, RenderContext rc, 
            ref ObjectShadow objShadow, string name,int id, ref int drawShadowCount,bool isDirty = false)
        {
            ref var dc = ref objShadow.drawContext.drawContext;
            if (dc.shadowDrawCall.count > 0)
            {
                if (objShadow.shadowMap == null)
                {
                    string shadowName = name;
#if UNITY_EDITOR
                    shadowName = string.Format("_{0}_{1}", name, id.ToString());
#endif
                    rc.CreateRT(256, 256, shadowName, RenderTextureFormat.Shadowmap, ref objShadow.shadowMap, 16);
                }
                rc.preWorkingCmd.SetRenderTarget(objShadow.shadowMap, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
                if (objShadow.isDirty)
                {
                    rc.preWorkingCmd.ClearRenderTarget(true, true, Color.clear, 1.0f);
                    objShadow.isDirty = isDirty;
                }
                
                rc.preWorkingCmd.SetViewProjectionMatrices(Matrix4x4.identity, dc.projMatrix);
                rc.preWorkingCmd.SetGlobalVector(_ShadowLightCenter, dc.shadowCenter);
                DrawStaticShadowBatch(context, rc.preWorkingCmd, ref dc.shadowDrawCall, 0, ref drawShadowCount);
                return true;
            }
            return false;
        }
        private void DrawStaticShadow(EngineContext context, RenderContext rc, int shadowMapSize, ref int drawShadowCount)
        {

            if (settings.shadowMode != ShadowMode.OnlyPlayerShadow)
            {
                //DebugLog.AddEngineLog2 ("Draw StaticShadow :{0} frame:{1}", shadowCount.ToString (), context.frameCount.ToString ());
                PrepareRT(context, rc, ref shadowRTArray, shadowMapSize, "_ShadowRTArray",
                    RenderContext.SceneShadowRT, 3, _ShadowMapTex);
                if (shadowRTArray != null)
                {
#if UNITY_EDITOR
                    if (!EngineContext.IsRunning)
                    {
                        if (debugShadowTerrainRT == null)
                        {
                            rc.CreateRT(2048, 2048,
                                "_Debug_ShadowTerrainRT",
                                RenderTextureFormat.Shadowmap,
                                ref debugShadowTerrainRT,
                                16);
                        }
                        Shader.SetGlobalTexture(_ShadowMapTerrainTex, debugShadowTerrainRT);
                    }
#endif
                    context.lastShadowProjDir = context.shadowProjDir;
                    //render csm0
#if UNITY_EDITOR
                    int lastDrawShadowCount = drawShadowCount;
#endif
                    if (DrawStaticShadow(context, rc,
                            shadowMapSize, "_ShadowTmpRT0",
                            RenderContext.SceneTmpShadowRT0,
                            ref context.shadowMapParam0,
                            _ShadowMapParam0,
                            ref context.csm0,
                            ref drawShadowCount,
                            EngineContext.ShadowFlag_CSM0RenderingDirty))
                    {

                    }
                    else if (DrawStaticShadow(context, rc,
                                shadowMapSize, "_ShadowTmpRT1",
                                RenderContext.SceneTmpShadowRT1,
                                ref context.shadowMapParam0,
                                _ShadowMapParam0,
                                ref context.csm1,
                                ref drawShadowCount,
                                EngineContext.ShadowFlag_CSM1RenderingDirty))
                    {

                    }
                    else if (DrawStaticShadow(context, rc,
                                shadowMapSize, "_ShadowTmpRT2",
                                RenderContext.SceneTmpShadowRT2,
                                ref context.shadowMapParam1,
                                _ShadowMapParam1,
                                ref context.csm2,
                                ref drawShadowCount,
                                EngineContext.ShadowFlag_CSM2RenderingDirty))
                    {

                    }
                }
#if UNITY_EDITOR
                if (EngineContext.IsRunning)
#endif
                {
                    if (context.renderflag.HasFlag(EngineContext.RFlag_RenderEnable))
                    {
                        if (drawShadowCount == MaxShadowDrawCount)
                        {
                            for (int i = 0; i < context.currentChunks.Length; ++i)
                            {
                                SceneChunk sc = context.currentChunks[i];
                                if (sc != null)
                                {
                                    ref var chunkShadow = ref sc.chunkShadow;
                                    if (TestOjbectLoad(context, ref chunkShadow))
                                    {
                                        if (DrawObjectShadow(context, rc, ref chunkShadow, "_ChunkShadow", sc.chunkId, ref drawShadowCount))
                                        {
                                            break;
                                        }
                                    }                                        
                                }
                            }

                        }
                        if (drawShadowCount == MaxShadowDrawCount)
                        {
                            var it = context.virtualChunks.GetEnumerator();
                            while (it.MoveNext())
                            {
                                var vc = it.Current.Value;
                                ref var chunkShadow = ref vc.chunkShadow;
                                if (TestOjbectLoad(context, ref chunkShadow))
                                {
                                    if (DrawObjectShadow(context, rc, ref chunkShadow, "_VirtualChunkShadow", vc.chunkId, ref drawShadowCount))
                                    {
                                        break;
                                    }
                                }
                                    
                            }
                        }
                    }                    
                }

            }
#if UNITY_EDITOR
            else if (!EngineContext.IsRunning && shadowRTArray != null)
            {
                ClearShadowRT(rc.preWorkingCmd, ref context.csm0, shadowRTArray);
                ClearShadowRT(rc.preWorkingCmd, ref context.csm1, shadowRTArray);
                ClearShadowRT(rc.preWorkingCmd, ref context.csm2, shadowRTArray);
            }
#endif

        }

        static Matrix4x4 rot90 = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(-90, 0, 0), Vector3.one);
        private void DrawSingleShadow(EngineContext context, RenderContext rc,ref int drawShadowCount)
        {
            var cb = rc.preWorkingCmd;
            var it = context.singleShadowObjects.GetEnumerator();
            while (it.MoveNext())
            {
                var ssObject = it.Current.Value;
                ref var objShadow = ref ssObject.objectShadow;
                if (objShadow.isDirty)
                {
#if UNITY_EDITOR
                    ssObject.gsd.transform.position = objShadow.drawContext.drawContext.sceneBound.center;
#endif
                    SceneCullSystem.PostProcessShadow(context, ref objShadow.drawContext, ref objShadow.shadowVP, 0, true);
                }
                if (DrawObjectShadow(context, rc, ref ssObject.objectShadow, "SingleShadow", it.Current.Key, ref drawShadowCount, true))
                {
                    break;
                }
            }

#if UNITY_EDITOR
            //if (!EngineContext.IsRunning && cc.singleShadowMesh != null)
            //{
            //    var it = cc.singleShadowMesh.GetEnumerator();
            //    int j = 0;
            //    while (it.MoveNext())
            //    {
            //        var kvp = it.Current;
            //        ref SingleShadowMap sf = ref context.singleShadow[j++];
            //        Matrix4x4 vp = sf.projMatrix * sf.viewMatrix;
            //        if (sf.rt != null)
            //        {
            //            for (int i = 0; i < kvp.Value.Count; ++i)
            //            {
            //                var mro = kvp.Value[i];
            //                var mpb = mro.GetMPB();
            //                mpb.SetTexture(_ShadowMapSelfTex, sf.rt);
            //                mpb.SetMatrix(_SelfShadow2VP, vp);
            //                mpb.SetVector(_ObjCenter, mro.transform.position);
            //                mro.Refresh();
            //            }
            //        }
            //    }
            //}
#endif
        }
        #endregion

        #region Dynamic Shadow Batch
        private void RenderExtraShadow(
           EngineContext context,
           RenderTexture rt,
           CommandBuffer cb,
           ref Matrix4x4 shadowVP,
           int shadowVPID,
           int shadowCenterID,
           ref ShadowContext csm)
        {
            ref var drawContext = ref csm.drawContext;
            ref var drawCalls = ref drawContext.shadowDrawCall;

            if (drawCalls.count > 0)
            {
                cb.SetRenderTarget(rt,
                    RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare,
                    RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
                cb.ClearRenderTarget(true, true, Color.clear, 1.0f);
                int passID = 0;

                cb.SetViewProjectionMatrices(Matrix4x4.identity, drawContext.projMatrix);
                cb.SetGlobalVector(shadowCenterID, drawContext.shadowCenter);
                if (shadowVPID > 0)
                    cb.SetGlobalMatrix(shadowVPID, shadowVP);

                DrawShadowBatch(cb, ref drawCalls, passID);
            }
            else if (csm.drawContext.clear)
            {
                cb.SetRenderTarget(rt,
                    RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare,
                    RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
                cb.ClearRenderTarget(true, true, Color.clear, 1.0f);
                csm.drawContext.clear = false;
            }
        }

        private void DrawExtraShadow(EngineContext context, RenderContext rc, int shadowMapSize)
        {
            RenderTexture shadowExtraRT = null;
            PrepareRT(context, rc, ref shadowExtraRT, shadowMapSize, "_ShadowExtraRT",
                RenderContext.EXTShadowRT, 0, _ShadowMapExtraTex);
            if (shadowExtraRT != null)
            {
                RenderExtraShadow(
                    context,
                    shadowExtraRT,
                    rc.preWorkingCmd,
                    ref context.shadowMapParam1,
                    _ShadowMapParam1,
                    _ShadowLightCenter,
                    ref context.csmExtra);
            }
            if (context.csmExtra1.DCCount > 0)
            {
                RenderTexture shadowExtra1RT = null;
                PrepareRT(context, rc, ref shadowExtra1RT, shadowMapSize, "_ShadowExtra1RT",
                    RenderContext.EXTShadow1RT, 0, _ShadowMapExtra1Tex);
                if (shadowExtra1RT != null)
                {
                    rc.preWorkingCmd.SetGlobalVector(_ShadowMapExtra1Center, context.shadowMapParam2.GetRow(2));
                    rc.preWorkingCmd.SetGlobalVector(_ShadowMapExtra1VP, context.shadowMapParam2.GetRow(3));
                    RenderExtraShadow(
                        context,
                        shadowExtra1RT,
                        rc.preWorkingCmd,
                        ref context.shadowMapParam0, -1,
                        _ShadowLightCenter,
                        ref context.csmExtra1);

                }
            }
        }
        #endregion

        #region Self Shadow Batch
        private void RenderSelfShadow(
            EngineContext context,
            RenderTexture rt,
            CommandBuffer cb,
            ref SelfShadow selfShadow,
            ref Vector3 lightDir)
        {
            cb.SetRenderTarget(rt, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            cb.ClearRenderTarget(true, true, Color.clear, 1.0f);
            Matrix4x4 vp = selfShadow.projMatrix * selfShadow.viewMatrix;
            cb.SetGlobalMatrix(_SelfShadowVP, vp);

            ref var drawCalls = ref selfShadow.drawCalls;
            while (drawCalls.Pop<DrawBatch>(out var db))
            {
                if (db.mat != null)
                {
                    if (db.mesh != null)
                    {
                        cb.DrawMesh(db.mesh, db.matrix, db.mat, 0, db.passID, db.mpbRef);
                    }
                    else if (db.render != null)
                    {
                        if (db.mpbRef != null)
                        {
                            db.mpbRef.SetTexture(_ShadowMapSelfTex, rt);
                            db.mpbRef.SetMatrix(_SelfShadowVP, vp);
                            db.mpbRef.SetVector(_SelfShadowDir, lightDir);
                            db.render.SetPropertyBlock(db.mpbRef);
                        }
                        cb.DrawRenderer(db.render, db.mat, 0, db.passID);
                    }
                }
                SharedObjectPool<DrawBatch>.Release(db);
            }
        }

        private void DrawShadowDepthBatch(CommandBuffer cb, ref OrderQueue drawCalls, Matrix4x4 _vp, RenderTexture _rt)
        {
            while (drawCalls.Pop<DrawBatch>(out var db))
            {
                if (db.mat != null)
                {
                    if (db.mesh != null)
                    {
                        cb.DrawMesh(db.mesh, db.matrix, db.mat, 0, db.passID, db.mpbRef);
                    }
                    else if (db.render != null)
                    {
                        if (db.mpbRef != null)
                        {
                            db.mpbRef.SetMatrix(_DepthShadowMartrix, _vp);
                            db.mpbRef.SetTexture(_DepthShadowTex, _rt);
                            db.render.SetPropertyBlock(db.mpbRef);
                        }
                        cb.DrawRenderer(db.render, db.mat, 0, db.passID);
                    }
                }
                SharedObjectPool<DrawBatch>.Release(db);
            }
        }

        private void DrawShadowDepthBatch(CommandBuffer cb, ref SelfShadow _SelfShadow)
        {
            Matrix4x4 mvp = _SelfShadow.projMatrixdepth * _SelfShadow.viewMatrixdepth;
            while (_SelfShadow.drawCallsdepthhair.Pop<DrawBatch>(out var db))
            {
                if (db.mat != null)
                {
                    if (db.mesh != null)
                    {
                        cb.DrawMesh(db.mesh, db.matrix, db.mat, 0, db.passID, db.mpbRef);
                    }
                    else if (db.render != null)
                    {
                        if (db.mpbRef != null)
                        {
                            db.mpbRef.SetMatrix(_DepthShadowMartrix, mvp);
                            db.render.SetPropertyBlock(db.mpbRef);
                        }
                        cb.DrawRenderer(db.render, db.mat, 0, db.passID);
                    }
                }
                SharedObjectPool<DrawBatch>.Release(db);
            }
        }

        private void DrawSelfShadow(EngineContext context, RenderContext rc, ref bool calcSelfShadow, ref Matrix4x4 baseAxis,
            ref SelfShadow ssInfo, int index, ref Vector4 selfShadowParam, int shadowMapSize = 1024)
        {
            if (!calcSelfShadow)
            {
                baseAxis = CalculateBaseAxis(context, ref EngineContext.roleLightDir,
                    out selfShadowLightProjectRight, out selfShadowLightProjectUp);
                calcSelfShadow = true;
            }
            var shadowSelfRT = ssInfo.rt;
            if (shadowSelfRT == null)
            {
                shadowSelfRT = RenderTexture.GetTemporary(shadowMapSize, shadowMapSize, 16, RenderTextureFormat.Shadowmap);
#if UNITY_EDITOR
                shadowSelfRT.name = string.Format("_SelfShadowMap_{0}", index.ToString());
#endif
                ssInfo.rt = shadowSelfRT;
            }
            var config = GetShadowConfig(ref ssInfo);
            CalcSelfShadowMatrix(context, ref ssInfo, ref EngineContext.roleLightDir, ref baseAxis, ref config);
            RenderSelfShadow(context, shadowSelfRT, rc.preWorkingCmd, ref ssInfo, ref EngineContext.roleLightDir);
            if ((config.flag & SelfShadowConfig.Flag_Blur) != 0)
            {
                int blurIteration = (int)selfShadowParam.z;
                float blurSampleDistance = selfShadowParam.y;
                if (blurSampleDistance > 0 && blurIteration > 0)
                {
                    Material mat = WorldSystem.GetEffectMat(EEffectMaterial.ShadowBlur);
                    if (mat != null)
                    {
                        rc.GetTmpRT(rc.preWorkingCmd, _TempBlurRT.id,
                            shadowMapSize, shadowMapSize, 16,
                            shadowSelfRT.format, shadowSelfRT.filterMode);

                        for (int j = 0; j < blurIteration; j++)
                        {
                            rc.preWorkingCmd.BlitFullscreenTriangle(shadowSelfRT, ref _TempBlurRT.rtID, mat, 0);
                            rc.preWorkingCmd.BlitFullscreenTriangle(ref _TempBlurRT.rtID, shadowSelfRT, mat, 1);
                        }
                        rc.ReleaseTmpRT(rc.preWorkingCmd, ref _TempBlurRT);
                    }
                }
            }
        }

        private void DrawDepthShadow(EngineContext context, RenderContext rc, ref bool calcSelfShadow, ref Matrix4x4 baseAxis,
            ref SelfShadow ssInfo, int index)
        {

            var cameraForward = -context.CameraTransCache.forward;
            if (!calcSelfShadow)
            {
                baseAxis = CalculateBaseAxis(context, ref cameraForward,
                    out depthLightProjectRight, out depthLightProjectUp);
                calcSelfShadow = true;
            }
            var shadowDepthRT = ssInfo.rtdepth;
            if (shadowDepthRT == null)
            {
                shadowDepthRT = RenderTexture.GetTemporary(512, 512, 16, RenderTextureFormat.Default);
#if UNITY_EDITOR
                shadowDepthRT.name = string.Format("_ShadowDepthRT_{0}", index.ToString());
#endif
                ssInfo.rtdepth = shadowDepthRT;
            }
            CalcDepthShadowMatrix(context, ref ssInfo, ref cameraForward, ref baseAxis);
            rc.preWorkingCmd.SetRenderTarget(shadowDepthRT, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            rc.preWorkingCmd.ClearRenderTarget(true, true, Color.clear, 1.0f);
            rc.preWorkingCmd.SetViewProjectionMatrices(ssInfo.viewMatrixdepth, ssInfo.projMatrixdepth);
            var vp = ssInfo.projMatrixdepth * ssInfo.viewMatrixdepth;
            Shader.SetGlobalMatrix(_DepthShadowMartrix, vp);
            DrawShadowDepthBatch(rc.preWorkingCmd, ref ssInfo.drawCallsdepthface, vp, shadowDepthRT);
            DrawShadowDepthBatch(rc.preWorkingCmd, ref ssInfo);
        }



        private void DrawSelfShadow(EngineContext context, RenderContext rc,
            ref Vector4 selfShadowParam, int shadowMapSize)
        {
            Matrix4x4 baseAxis = Matrix4x4.identity;
            Matrix4x4 depthBaseAxis = Matrix4x4.identity;
            bool calcSelfShadow = false;
            bool calcDepthShadow = false;
            for (int i = 0; i < context.selfShadowInfo.Length; ++i)
            {
                ref var ssi = ref context.selfShadowInfo[i];
                if (ssi.drawCalls.count > 0)
                {
                    DrawSelfShadow(context, rc, ref calcSelfShadow, ref baseAxis, ref ssi, i, ref selfShadowParam, shadowMapSize);
                }

                if (ssi.drawCallsdepthhair.count > 0)
                {
                    DrawDepthShadow(context, rc, ref calcDepthShadow, ref depthBaseAxis, ref ssi, i);
                }
                else
                {
                    if(ssi.rtdepth != null)
                    {
                        RenderTexture.ReleaseTemporary(ssi.rtdepth);
                        ssi.rtdepth = null;
                    }
                }
            }
        }
        #endregion

        #region Plane Shadow Batch
        public void DrawPlaneShadow(EngineContext context, RenderContext rc)
        {
            var cb = rc.afterOpaqueCmd;
            for (int i = 0; i < context.planeshadowInfos.Length; i++)
            {
                ref var planeShadewInfo = ref context.planeshadowInfos[i];
                while (planeShadewInfo.drawCalls.Pop<DrawBatch>(out var db))
                {
                    if (db.render != null)
                    {
                        cb.DrawRenderer(db.render, db.mat);
                    }
#if UNITY_EDITOR

                    if (!EngineContext.IsRunning && db.mpbRef != null)
                    {
                        CommonObject<MaterialPropertyBlock>.Release(db.mpbRef);
                    }
#endif
                    SharedObjectPool<DrawBatch>.Release(db);
                }
            }
        }
        #endregion

#if UNITY_EDITOR
        private void RenderDebugShadow (
            EngineContext context,
            RenderTexture rt,
            CommandBuffer cb,
            int shadowVPID,
            int shadowCenterID,
            ref ShadowContext csm,
            bool clear)
        {
            ref var drawCalls = ref csm.drawContext.shadowDrawCall;
            if (drawCalls.count > 0)
            {
                float far = csm.parallel3D.minCullPlane.y;
                float near = csm.drawContext.sceneBound.max.y;
                near += 5;
                float heigh = near - far;
                float size = csm.parallel3D.size * 0.5f;
                Matrix4x4 projMatrix = Matrix4x4.Ortho (-size, size, -size, size, 0.1f, heigh);
                var realMaxPlane = csm.parallel3D.minCullPlane - context.shadowProjDir * heigh * context.invSin;
                Vector3 center = realMaxPlane + new Vector3 (size, 0, size);

                Vector4 lightCenter = Vector4.zero;
                lightCenter.x = center.x;
                lightCenter.y = center.y;
                lightCenter.z = center.z;
                lightCenter.w = size;

                int passID = 0;
                cb.SetRenderTarget (rt, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
                if (clear)
                {
                    cb.ClearRenderTarget (true, true, Color.clear, 1.0f);
                }
                cb.SetViewProjectionMatrices (Matrix4x4.identity, projMatrix);

                Vector4 projVec = Vector4.zero;
                projMatrix = GL.GetGPUProjectionMatrix (projMatrix, false);
                projVec.x = projMatrix.m00;
                projVec.y = projMatrix.m11;
                projVec.z = projMatrix.m22;
                projVec.w = projMatrix.m23;
                Matrix4x4 shadowVP = Matrix4x4.identity;
                shadowVP.SetRow (0, lightCenter);
                shadowVP.SetRow (1, projVec);
                cb.SetGlobalMatrix (shadowVPID, shadowVP);
                cb.SetGlobalVector (shadowCenterID, lightCenter);
                var it = drawCalls.BeginGet ();
                while (drawCalls.Get (ref it, out DrawBatch db))
                {
                    if (db.mat != null)
                    {
                        if (db.mesh != null)
                        {
                            cb.DrawMesh (db.mesh, db.matrix, db.mat, 0, passID, db.mpbRef);
                        }
                        else if (db.render != null)
                        {
                            cb.DrawRenderer (db.render, db.mat, 0, passID);
                        }
                    }
                }
            }
        }
#endif

        public override void Render (EngineContext context, IRenderContext renderContext)
        {
            var rc = renderContext as RenderContext;

            context.shadowDrawTotalCount = 0;
            int extraShadowMapSize = context.shadowRTSize;
            int shadowMapSize = context.shadowRTSize;
            Vector4 selfShadowParam = WorldSystem.miscConfig != null?
            WorldSystem.miscConfig.GetShadowParam () : new Vector4 (0.02f, 0.05f, 2, 60);
            var qs = QualitySettingData.current;
            QualityLevel shadowQuality = qs.shadowQuality;
#if UNITY_EDITOR
            if (!EngineContext.IsRunning)
                EditorSetShadow();
#endif
            int softShadow = 1;
            switch (shadowQuality)
            {
                case QualityLevel.Low:
                  
                    softShadow = -1;
                    break;
                case QualityLevel.Medium:
                    softShadow = 0;
                    break;
                case QualityLevel.High:                 
                    break;
                default:
                    break;
            }


            // bool softShadow = shadowMode == ShadowMode.SoftShaow;
            SetShaderValue(_ShadowParam1, new Vector4(
               softShadow,
                context.renderflag.HasFlag(EngineContext.RFlag_UISceneShadow) ? 1 : 0,
                context.csmExtra1.DCCount > 0 ? 1 : 0,
                settings.shadowMisc.value.x));

            if (BeginUpdate ())
            {
                SetShaderValue (_ShadowMapSize,
                    new Vector4 (shadowMapSize,
                        1.0f / shadowMapSize,
                        extraShadowMapSize,
                        1.0f / extraShadowMapSize));

                //SetShaderValue (_ShadowLightDir, new Vector4 (context.shadowProjDir.x, context.shadowProjDir.y, context.shadowProjDir.z, context.invSin));

                SetShaderValue (_ShadowParam0, ref settings.shadowParam1.value);
                SetShaderValue (_ShadowMapFade, Vector4.one);

                Vector4 color = settings.color.value;
                color *= color.w;
                SetShaderValue (_ShadowColor, ref color);
                SetShaderValue (_ShadowParam2, ref settings.shadowMisc1.value);
                SetShaderValue (_ShadowParam3, ref settings.shadowMisc2.value);
                SetShaderValue (_ShadowParam4, ref settings.shadowMisc.value);

                //与Ambient控制冲突
                // Vector4 roleShadowColor = settings.roleShadowColor.value;
                // SetShaderValue(_RoleShadowColor, ref roleShadowColor);


                //context.baseAxis = CalculateBaseAxis(context, ref context.mainLightDir, 
                //    out context.lightProjectRight, out context.lightProjectUp);
                // Texture2D cloudShadow = settings.cloudMap.resHandle.obj as Texture2D;
                // SetShaderValue (_CloudMap, cloudShadow);
                EndUpdate();
            }

            DrawSelfShadow(context, rc, ref selfShadowParam, 1024);
            DrawPlaneShadow(context, rc);

            int maxShadowCount = MaxShadowDrawCount;
#if UNITY_EDITOR
            if (!EngineContext.IsRunning)
                maxShadowCount = MaxShadowDrawCount;
#endif
            int drawShadowCount = maxShadowCount;

            DrawStaticShadow(context, rc, shadowMapSize,ref drawShadowCount);
            //DrawSingleShadow(context, rc, ref drawShadowCount);

            context.shadowDrawTotalCount += maxShadowCount - drawShadowCount;

            // 2021年9月17日：新管线没用到ExtShadow，但调用了一次Clear，先注释掉。
            // DrawExtraShadow(context, rc, extraShadowMapSize);            
            
            SetShaderValue(_SelfShadowParam, ref selfShadowParam);
        }
#if UNITY_EDITOR
        private void EditorSetShadow()
        {
            ShadowMode shadowMode = settings.shadowMode;
            switch (shadowMode)
            {
                case ShadowMode.OnlyPlayerShadow:
                    QualitySettingData.current.shadowQuality = QualityLevel.Low;
                    break;
                case ShadowMode.HardShadow:
                    QualitySettingData.current.shadowQuality = QualityLevel.Medium;
                    break;
                case ShadowMode.SoftShaow:
                    QualitySettingData.current.shadowQuality = QualityLevel.High;
                    break;
                default:
                    break;
            }
        }

#endif
        public override void Release (EngineContext context, IRenderContext renderContext)
        {
            base.Release (context, renderContext);
            shadowRTArray = null;

            for (int i = 0; i < context.selfShadowInfo.Length; ++i)
            {
                ref var selfShadowInfo = ref context.selfShadowInfo[i];
                if (selfShadowInfo.rt != null)
                {
                    RenderTexture.ReleaseTemporary (selfShadowInfo.rt);
                    selfShadowInfo.rt = null;
                }
                if (selfShadowInfo.rtdepth != null)
                {
                    RenderTexture.ReleaseTemporary(selfShadowInfo.rtdepth);
                    selfShadowInfo.rtdepth = null;
                }
            }
            Shader.SetGlobalVector(ShaderManager._ShadowMoveOffset, Vector4.zero);
#if UNITY_EDITOR
            extraShadowList.Clear ();
            isOctTreeInit = false;
#endif
        }
    }

}
