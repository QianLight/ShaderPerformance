using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using Unity.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace CFEngine
{
    public enum ClearFlag
    {
        /// <summary>Don't clear.</summary>
        None = 0,
        /// <summary>Clear the color buffer.</summary>
        Color = 1,
        /// <summary>Clear the depth buffer.</summary>
        Depth = 2,

        /// <summary>Clear both color and depth buffers.</summary>
        All = Depth | Color
    }

#if UNITY_EDITOR
    public class RenderResContext
    {
        public Renderer r;
        public MaterialPropertyBlock mpb;
        public MaterialPropertyBlock shadowMpb;
    }
#endif

    // Context object passed around all post-fx in a frame
    public sealed class RenderContext : CFSingleton<RenderContext>, IRenderContext
    {
        internal PostProcessResources resources;
        internal PropertySheetFactory propertySheets;
        internal PropertySheet uberSheet;
        //camera
        public int pixelWidth = 1920;
        public int pixelHeight = 1080;
        public int halfWidth = 1920 / 2;
        public int halfHeight = 1080 / 2;
        public Rect pixelRect = new Rect (0, 0, 1, 1);
        public float aspectRatio = 1920 / 1080;
        public RenderTextureDescriptor cameraDesc;
        private RenderTargetBinding rtBinding;
        private RenderTargetIdentifier[] colorRTSetup;
        private RenderBufferStoreAction[] storeSetup;
        private RenderBufferLoadAction[] loadSetup;

        //not use srp
        private RenderBuffer[] colorBuffer;
        private RenderBuffer depthBuffer;
        private RenderTexture sceneRT0;
        private RenderTexture sceneRT1;
        public RenderTexture ppTmpRT;

        public RenderTextureFormat defaultFormat;
        internal CommandBuffer preWorkingCmd; //shadow, compute lighting, color grading
        internal CommandBuffer afterOpaqueCmd; //grab, decal, fog
        internal CommandBuffer ppCommand; //postprocess : bloom
        internal CommandBuffer prePPCommand; //pre postprocess
        internal CommandBuffer distortionCmd; //distortion
        internal CommandBuffer postPPCommand; //post postprocess : uber 
        internal CommandBuffer uiPPCommand; //ui postprocess
                                            // internal CommandBuffer postWorkingCommand;
        private RenderTexture blurRT;
        private RenderTexture uiBlurRT;
        public RenderTexture[] rts = new RenderTexture[RTCount];

        public static int SceneShadowRT = 0;
        public static int SceneTmpShadowRT0 = SceneShadowRT + 1;
        public static int SceneTmpShadowRT1 = SceneTmpShadowRT0 + 1;
        public static int SceneTmpShadowRT2 = SceneTmpShadowRT1 + 1;
        public static int EXTShadowRT = SceneTmpShadowRT2 + 1;
        public static int EXTShadow1RT = EXTShadowRT + 1;
        //public static int DepthShadowRT = EXTShadow1RT + 1;
        public static int MultiLayerRT0 = EXTShadow1RT + 1;
        public static int MultiLayerRT1 = MultiLayerRT0 + 1;
        public static int MultiLayerRT2 = MultiLayerRT1 + 1;
        public static int MultiLayerRT3 = MultiLayerRT2 + 1;

        public static int RTCount = MultiLayerRT3 + 1;

        private RenderTexture bakedLightRT;
        private RenderTexture dynamicLightRT;
        private Action<AsyncGPUReadbackRequest> captureCb;

        public Texture2D[] colorCurveCache = new Texture2D[8];

        public RenderTargetIdentifier currentRT;
        public RenderTargetIdentifier des;
        private int rtSize = 0;
        private int tmpRTSize = 0;
        public static int k_DepthStencilBufferBits = 24;
        public static RenderTargetHandle _MainRT0 = new RenderTargetHandle ("_MainRT0"); //msaa full res
        public static RenderTargetHandle _DepthRT = new RenderTargetHandle ("_DepthRT"); //msaa not resolve full res
        public static readonly int CameraDepthTex = Shader.PropertyToID ("_CameraDepthRT");
        public static readonly int _GrabTex = Shader.PropertyToID ("_GrabTex");


        //if grab switch rt
        public static RenderTargetHandle _MainRT1 = new RenderTargetHandle ("_MainRT1"); //pp rt full res
        public static RenderTargetHandle _MainRT2 = new RenderTargetHandle ("_MainRT2"); //pp rt full res
        public static RenderTargetHandle _SceneRT = new RenderTargetHandle("_SceneRT");
        public static RenderTargetHandle _UIBlurRT = new RenderTargetHandle ("_UIBlurRT"); //not tmp half res
        /// <summary>
        /// size = size >> 2
        /// </summary>
        public static RenderTargetHandle _DofRT = new RenderTargetHandle ("_DofRT");
        public static RenderTargetHandle _RadialBlurRt = new RenderTargetHandle ("_RadialBlurRT");
        public static RenderTargetHandle _RadialBlurDebugRt = new RenderTargetHandle ("_RadialBlurDebugRT");
        public static RenderTargetHandle _BloomRT = new RenderTargetHandle ("_BloomRT");
        public static RenderTargetHandle _TmpRT0 = new RenderTargetHandle ("_TmpRT0");
        public static RenderTargetHandle _TmpRT1 = new RenderTargetHandle ("_TmpRT1");
        public static RenderTargetHandle _DistortionRT = new RenderTargetHandle ("_DistortionRT");

        public static RenderTargetHandle _TmpQuarterRT0 = new RenderTargetHandle ("_TmpQuarterRT0");
        public static RenderTargetHandle _TmpQuarterRT1 = new RenderTargetHandle ("_TmpQuarterRT1");
        public static RenderTargetHandle _DepthShadowDepth = new RenderTargetHandle("_DepthShadowDepth");
        public static RenderTargetHandle _DepthShadowRT = new RenderTargetHandle("_DepthShadowRT");

        public static RenderTargetHandle _CaptureRT = new RenderTargetHandle("_CaptureRT");

        public static int RTSize_Half = 1; //1/2
        public static int RTSize_Quarter = 2; //1/4
        public static int RTSize_HalfQuarter = 3; //1/8
        public static int RTSize_Hex = 4; //1/16
        public static int RTSize_HalfHex = 5; //1/32
        public static int RTSize_QuarterHex = 6; //1/64
        public static int CopyPass_Defalut = 0;

        public static int _Global_Setting = Shader.PropertyToID ("_Global_Setting");
        public static int _Global_Setting2 = Shader.PropertyToID ("_Global_Setting2");

        public FlagMask flag;
        public static uint Flag_CreateRT = 0x00000001;
        public static uint Flag_IsOverlay = 0x00000002;
        public static uint Flag_IsSceneView = 0x00000004;
        public static uint Flag_IsDefaultViewport = 0x00000008;
        public static uint Flag_SupportGrabRT = 0x00000010;
        // public static uint Flag_CreateExtraRT = 0x00000020;
        public static uint Flag_UberClear = 0x00000040;

        public static uint Flag_MainRT2Create = 0x00000100;

        public static uint Flag_GodrayEnable = 0x00000800;
        public static uint Flag_UICameraRT = 0x00001000;
        public static uint Flag_DepthShadowDepthRT = 0x00002000;
        public static uint Flag_MainRT1Create = 0x00004000;
        public FlagMask stateFlag;

        public static uint SFlag_PPEnable = 0x00000001;
        public static uint SFlag_SetCameraRT = 0x00000002;
        public static uint SFlag_DofEnable = 0x00000020;
        public static uint SFlag_BloomEnable = 0x00000040;
        public static uint SFlag_ColorgradingEnable = 0x00000080;
        public static uint SFlag_GodRayEnable = 0x00000100;
        public static uint SFlag_RadialBlurEnable = 0x00000200;
        public static uint SFlag_DistortionEnable = 0x00000400;
        public static uint SFlag_RadialBlurDebugEnable = 0x00000800;
        public static uint SFlag_CustomLutEnable = 0x00001000;

#if UNITY_EDITOR
        internal CommandBuffer debugCmd;
        private RenderTexture debugRT;
        public static bool gameOverdrawViewMode;
        public static bool sceneOverdrawViewMode;
        public static bool shadedWireframeMode;
        public static bool opaqueOverdraw;
        public static bool transparentOverdraw;
        // public static Material overdrawOpaque;
        // public static Material overdrawTransparent;
        public static int currentDebugMode = 0;
        public List<RenderBatch> workingRenderBatchRef;
        public int workingBatchCount = 0;
        public static bool bakingLightMap;
        public static int ResVersionStart = 1;
        public static int ResVersionWind = 2;
        public static int ResVersionBloom = 3;
        public static int ResVersionTexCurve = 4;
        public static int ResVersionFogNoise3D = 5;
        public static int ResVersionDof = 6;
        public static int ResVersionNewWind = 7;
        public static int ResVersionRoleShadow = 8;
        public static int ResVersionOptimizedFog = 9;
        public static int ResVersionOptimizedFog2 = 10;
        public static int ResVersionScatteredFog = 11;
        public static int ResVersionFogDecorator = 12;
        public static int ResVersionCustomLut = 13;
        public static int ResVersionRoleLightingV2 = 134;
        public static int ResVersionRoleLightingV3 = 135;
        public static int ResVersionLatest = 135;

        public static bool pausePostProcess = false;

        public delegate bool CustomClearColorGetter(Camera camera, out Color color);
        public static CustomClearColorGetter customClearColorGetter;
        public static bool capturing;
#endif
        #region common
        public void Init (EngineContext context)
        {
#if UNITY_EDITOR
            resources = AssetDatabase.LoadAssetAtPath<PostProcessResources>(
                            "Assets/Engine/Runtime/Res/EngineResources.asset");
#else
//             resources = BundleMgr.singleton.LoadAssetFromBundle("cfpostprocess",
//                     false, typeof(PostProcessResources)) as PostProcessResources;
            resources = ZeusAssetManager.singleton.GetAsset("AssetRes/EngineResources.asset", false, typeof(PostProcessResources)) as PostProcessResources;
#endif
            {


                //var objs = BundleMgr.singleton.LoadAssetFromBundle("cfpostprocess");
                //if (objs != null)
                //{
                //    for (int i = 0; i < objs.Length; ++i)
                //    {
                //        if (objs[i] is PostProcessResources)
                //        {
                //            resources = objs[i] as PostProcessResources;
                //            break;
                //        }
                //    }
                //}
            }
            propertySheets = new PropertySheetFactory ();
            if (resources != null)
            {
                uberSheet = propertySheets.Get (resources.shaders.uber);
                RuntimeUtilities.SetCopyMaterial (resources.shaders.copy);
            }
            defaultFormat = RenderTextureFormat.RGB111110Float;

     
            preWorkingCmd = new CommandBuffer { name = "Working Cmd" };
            afterOpaqueCmd = new CommandBuffer { name = "After Opaque Cmd" };
            prePPCommand = new CommandBuffer { name = "Pre Postprocess Cmd" };
            distortionCmd = new CommandBuffer { name = "Distortion Cmd" };
            ppCommand = new CommandBuffer { name = "Postprocess Cmd" };
            postPPCommand = new CommandBuffer { name = "Post Postprocess Cmd" };
            uiPPCommand = new CommandBuffer { name = "UI Post Postprocess Cmd" };
#if UNITY_EDITOR
            debugCmd = new CommandBuffer { name = "SceneView Debug Cmd" };
            workingRenderBatchRef = EngineContext.instance.workingRenderBatch;
#endif
            cameraDesc.dimension = TextureDimension.Tex2D;
            cameraDesc.colorFormat = defaultFormat;
            cameraDesc.enableRandomWrite = false;
            cameraDesc.useMipMap = false;
            cameraDesc.sRGB = true;
            captureCb = CaptureRTCb;
            End (context);
        }
        private void ReleaseCmd (ref CommandBuffer cmd)
        {
            if (cmd != null)
            {
                cmd.Release ();
                cmd = null;
            }
        }

        public void Uninit (EngineContext context)
        {
            End (context);
            for (int i = 0; i < colorCurveCache.Length; ++i)
            {
                ref var tex = ref colorCurveCache[i];
                if (tex != null)
                {
#if UNITY_EDITOR
                    if (!EngineContext.IsRunning)
                    {
                        UnityEngine.Object.DestroyImmediate (tex);
                    }
                    else
#endif
                    {
                        UnityEngine.Object.Destroy (tex);
                    }
                    tex = null;
                }
            }
     
            ReleaseCmd (ref preWorkingCmd);
            ReleaseCmd (ref afterOpaqueCmd);
            ReleaseCmd (ref prePPCommand);
            ReleaseCmd (ref distortionCmd);
            ReleaseCmd (ref ppCommand);
            ReleaseCmd (ref postPPCommand);
            ReleaseCmd (ref uiPPCommand);
            if (propertySheets != null)
            {
                propertySheets.Release ();
                propertySheets = null;
            }
            uberSheet = null;
            resources = null;
        }

        public void Start (EngineContext context)
        {
            var qs = QualitySettingData.current;
            defaultFormat = qs.mainRTFormat;
            context.renderflag.SetFlag (EngineContext.RFlag_PrePPDirty, true);
            context.renderflag.SetFlag (EngineContext.RFlag_PPDirty, true);
            if (context.CameraRef != null)
            {
                pixelWidth = (int) (context.CameraRef.pixelWidth * qs.rtScale);
                pixelHeight = (int) (context.CameraRef.pixelHeight * qs.rtScale);
            }
            cameraDesc = new RenderTextureDescriptor (pixelWidth, pixelHeight); //must be new
            Shader.SetGlobalTexture("_GlobalNoise", resources.distortion0);
#if UNITY_EDITOR
            pausePostProcess = false;
#endif

        }

        public void End (EngineContext context)
        {

            flag.Reset ();
            stateFlag.Reset ();
            pixelWidth = 1920;
            pixelHeight = 1080;
            halfWidth = 1920 / 2;
            halfHeight = 1080 / 2;

            if (context.CameraRef != null)
            {
                context.CameraRef.RemoveAllCommandBuffers ();
                if (RenderPipelineManager.renderPipeline == OPRenderPipeline.Builtin)
                    context.CameraRef.SetTargetBuffers (new RenderBuffer (), new RenderBuffer ());
            }
            if (context.uiCamera != null)
            {
                context.uiCamera.RemoveAllCommandBuffers ();
            }

            if (preWorkingCmd != null)
                preWorkingCmd.Clear ();
            if (distortionCmd != null)
                distortionCmd.Clear ();
            if (afterOpaqueCmd != null)
                afterOpaqueCmd.Clear ();
            ClearPPCmd ();
            ClearUIPPCmd ();
            ReleaseRT ();

#if UNITY_EDITOR
            Camera c = SceneView.lastActiveSceneView != null ? SceneView.lastActiveSceneView.camera : null;
            if (c != null)
            {
                c.RemoveAllCommandBuffers ();
            }
            if (debugCmd != null)
                debugCmd.Clear ();
            if (workingRenderBatchRef != null)
                workingRenderBatchRef.Clear ();
            workingBatchCount = 0;
#endif
        }


        public void Update (EngineContext context)
        {
            EngineContext.UseSrp = GraphicsSettings.renderPipelineAsset == null ? false : true;
            flag.Reset ();
            tmpRTSize = 0;
            var camera = context.CameraRef;
            if (camera != null)
            {
                //camera.allowHDR = false;
                //camera.allowMSAA = false;
                context.cameraPos = context.CameraTransCache.position;
                context.worldToProjectionMatrix = camera.projectionMatrix * camera.worldToCameraMatrix;
                GeometryUtility.CalculateFrustumPlanes (context.worldToProjectionMatrix, EngineContext.frustumPlanes);

                var qs = QualitySettingData.current;
                pixelWidth = (int) (camera.pixelWidth * qs.rtScale);
                pixelHeight = (int) (camera.pixelHeight * qs.rtScale);

                halfWidth = pixelWidth / 2;
                halfHeight = pixelHeight / 2;
                pixelRect = camera.pixelRect;
                aspectRatio = (float) pixelWidth / (float) pixelHeight;

                Rect cameraRect = camera.rect;
                bool isDefaultViewport = (!(Math.Abs (cameraRect.x) > 0.0f || Math.Abs (cameraRect.y) > 0.0f ||
                    Math.Abs (cameraRect.width) < 1.0f || Math.Abs (cameraRect.height) < 1.0f));
                flag.SetFlag (Flag_IsDefaultViewport, isDefaultViewport);

                bool createRT = qs.flag.HasFlag (QualitySet.Flag_EnableHDR) ||
                    qs.flag.HasFlag (QualitySet.Flag_EnableMRT) ||
                    stateFlag.HasFlag (SFlag_PPEnable);
#if UNITY_EDITOR
                if (RenderPipelineManager.renderPipeline == OPRenderPipeline.Builtin)
                {
                    stateFlag.SetFlag (RenderContext.SFlag_PPEnable, false);
                    createRT = false;
                }
#endif
                flag.SetFlag (Flag_CreateRT, createRT);
                flag.SetFlag (Flag_SupportGrabRT, true);

                if (createRT)
                {
                    cameraDesc.width = pixelWidth;
                    cameraDesc.height = pixelHeight;
                    cameraDesc.colorFormat = defaultFormat;
                    cameraDesc.msaaSamples = qs.msaaCount; 

                }
          
                if (preWorkingCmd != null)
                    preWorkingCmd.Clear ();
                if (afterOpaqueCmd != null)
                    afterOpaqueCmd.Clear ();
                if (distortionCmd != null)
                    distortionCmd.Clear();
                // if (postWorkingCommand != null)
                //     postWorkingCommand.Clear ();
                if (!stateFlag.HasFlag (SFlag_PPEnable))
                {
                    ClearPPCmd ();
                }

#if UNITY_EDITOR
                if (debugCmd != null)
                    debugCmd.Clear ();
                workingBatchCount = 0;
                RenderTargetHandle.frameTargets.Clear ();
#endif
            }
        }

        public void PreRender (EngineContext context)
        {
            if (RenderPipelineManager.renderPipeline == OPRenderPipeline.LegacySRP)
            {

            }
            else if (context.CameraRef != null)
            {
                if (stateFlag.HasFlag (SFlag_PPEnable))
                {
                    CreateRT (pixelWidth, pixelHeight, "_MainRT1", defaultFormat, ref ppTmpRT);
                }

                bool createRT = CreateCameraRT ();
                SetCameraRT (context.CameraRef, createRT);
                ExecuteWorkCmd ();

            }
            if (flag.HasFlag (Flag_CreateRT))
            {
                if (RenderPipelineManager.renderPipeline == OPRenderPipeline.LegacySRP)
                {
                    currentRT = _MainRT0.rtID;
                }
                else
                {
                    currentRT = sceneRT0;
                }
            }
            else
            {
                currentRT = RenderTargetHandle.CameraTarget.rtID;
            }
        }
        public void ExecuteWorkCmd ()
        {
            if (preWorkingCmd != null)
            {
                Graphics.ExecuteCommandBuffer (preWorkingCmd);
                preWorkingCmd.Clear ();
                // preWorkingCmd.DebugCmd();
            }
        }
        private void SetupRT (int count)
        {
            if (colorRTSetup == null || colorRTSetup.Length != count)
            {
                colorRTSetup = new RenderTargetIdentifier[count];
                storeSetup = new RenderBufferStoreAction[count];
                loadSetup = new RenderBufferLoadAction[count];
            }
            rtBinding.colorRenderTargets = colorRTSetup;
            rtBinding.colorStoreActions = storeSetup;
            rtBinding.colorLoadActions = loadSetup;
            rtBinding.depthLoadAction = RenderBufferLoadAction.DontCare;
            rtBinding.depthStoreAction = RenderBufferStoreAction.Store;
        }

        public bool CreateCameraRT (CommandBuffer cmd)
        {
            var qs = QualitySettingData.current;
            bool useMRT = qs.flag.HasFlag (QualitySet.Flag_EnableMRT);
            bool createRT = flag.HasFlag (Flag_CreateRT);
            SetupRT (useMRT ? 2 : 1);
            if (createRT)
            {
                var colorDescriptor = cameraDesc;
                var depthDescriptor = cameraDesc;
                depthDescriptor.colorFormat = RenderTextureFormat.ARGB32;
                colorDescriptor.memoryless = RenderTextureMemoryless.Depth;
                if (qs.msaaCount > 1)
                {
                    colorDescriptor.memoryless |= RenderTextureMemoryless.MSAA;
                    depthDescriptor.memoryless |= RenderTextureMemoryless.MSAA;
                }
                //else
                //{
                //    //colorDescriptor.memoryless |= RenderTextureMemoryless.Color;
                //}
                colorDescriptor.depthBufferBits = k_DepthStencilBufferBits;
                cmd.GetTemporaryRT (_MainRT0.id, colorDescriptor, FilterMode.Bilinear);
                tmpRTSize += colorDescriptor.width * colorDescriptor.height * GetFormatStride (colorDescriptor.colorFormat);

                //depthDescriptor.colorFormat = RenderTextureFormat.ARGBHalf;
                depthDescriptor.depthBufferBits = 0;
                depthDescriptor.bindMS = false; // qs.msaaCount > 1 && !SystemInfo.supportsMultisampleAutoResolve && (SystemInfo.supportsMultisampledTextures != 0);
                depthDescriptor.sRGB = false;
                cmd.GetTemporaryRT (_DepthRT.id, depthDescriptor, FilterMode.Bilinear);
                tmpRTSize += colorDescriptor.width * colorDescriptor.height * GetFormatStride (colorDescriptor.colorFormat);

                rtBinding.depthRenderTarget = _MainRT0.rtID;
                colorRTSetup[0] = _MainRT0.rtID;
                storeSetup[0] = RenderBufferStoreAction.Store;
                loadSetup[0] = RenderBufferLoadAction.DontCare;
                if (useMRT)
                {
                    colorRTSetup[1] = _DepthRT.rtID;
                    storeSetup[1] = RenderBufferStoreAction.Store;
                    loadSetup[1] = RenderBufferLoadAction.DontCare;
                    cmd.SetGlobalTexture (CameraDepthTex, _DepthRT.rtID);
                }
                return true;
            }
            else
            {
                colorRTSetup[0] = RenderTargetHandle.CameraTarget.id;
                rtBinding.depthRenderTarget = RenderTargetHandle.CameraTarget.id;
                storeSetup[0] = RenderBufferStoreAction.Store;
                loadSetup[0] = RenderBufferLoadAction.DontCare;
                return false;
            }
        }
        public void SetCameraRT (CommandBuffer cmd, Camera camera, ref Color c)
        {
#if UNITY_EDITOR
            camera.SetTargetBuffers (new RenderBuffer (), new RenderBuffer ());
#endif
            RuntimeUtilities.SetRenderTarget (cmd, ref rtBinding, ClearFlag.All,
                c);

        }
        public void ReBindCameraRT (CommandBuffer cmd)
        {
            RuntimeUtilities.SetRenderTarget (cmd, ref rtBinding, ClearFlag.None,
                Color.clear);
        }
        private bool CreateCameraRT ()
        {
            var qs = QualitySettingData.current;
            bool useMRT = qs.flag.HasFlag (QualitySet.Flag_EnableMRT);
            bool createRT = flag.HasFlag (Flag_CreateRT);
            if (createRT)
            {
                int count = useMRT ? 2 : 1;
                if (colorBuffer == null || colorBuffer.Length != count)
                {
                    colorBuffer = new RenderBuffer[count];
                }
                CreateRT (pixelWidth, pixelHeight, "_MainRT0_NoSRP", defaultFormat, ref sceneRT0,
                    k_DepthStencilBufferBits, FilterMode.Bilinear, TextureWrapMode.Clamp, false, 1, false, qs.msaaCount);
                colorBuffer[0] = sceneRT0.colorBuffer;
                depthBuffer = sceneRT0.depthBuffer;
                if (useMRT)
                {
                    CreateRT (pixelWidth, pixelHeight, "_MainRT1_NoSRP", defaultFormat, ref sceneRT1,
                        0, FilterMode.Bilinear, TextureWrapMode.Clamp, false, 1, false, qs.msaaCount);
                    colorBuffer[1] = sceneRT1.colorBuffer;
                    Shader.SetGlobalTexture (CameraDepthTex, sceneRT1);
                }
                return true;
            }
            return false;
        }

        private void SetCameraRT (Camera camera, bool createRT)
        {
            if (!stateFlag.HasFlag (SFlag_SetCameraRT))
            {
                if (createRT)
                {
                    camera.SetTargetBuffers (colorBuffer, depthBuffer);
                }
                else
                {
                    camera.SetTargetBuffers (new RenderBuffer (), new RenderBuffer ());
                }
                stateFlag.SetFlag (SFlag_SetCameraRT, true);
            }
        }

        public bool IsValid ()
        {
            return resources != null && preWorkingCmd != null;
        }

        public void ClearPPCmd ()
        {
            if (prePPCommand != null)
                prePPCommand.Clear();
            if (ppCommand != null)
                ppCommand.Clear ();
            if (postPPCommand != null)
                postPPCommand.Clear ();
        }
        public void ClearUIPPCmd ()
        {
            if (uiPPCommand != null)
                uiPPCommand.Clear ();
        }

#endregion

        public void FirstBlit (CommandBuffer cmd)
        {
            cmd.SetGlobalTexture (ShaderIDs.MainTex, currentRT);
            cmd.SetRenderTarget (des, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            cmd.DrawMesh (RuntimeUtilities.fullscreenTriangle, Matrix4x4.identity, RuntimeUtilities.GetCopyMaterial (), 0, 0);
        }

        private void CaptureRTCb(AsyncGPUReadbackRequest request)
        {
            EngineContext context = EngineContext.instance;
            if (context != null&& context.captureCb != null)
            {
                if (!request.hasError)
                {
                    var data = request.GetData<byte>(0);
                    context.captureCb(ref data);
                }
                context.captureCb = null;
            }
        }

        private bool ProcessCapture(EngineContext context, CommandBuffer cmd)
        {
            if (context.captureCb != null)
            {
                if (context.captureSize.z == 0)
                {
                    var rt = GetBlurRT();

                    cmd.SetGlobalTexture(ShaderIDs.MainTex, _MainRT1.rtID);
                    cmd.SetRenderTarget(rt,
                        RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                        RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
                    cmd.DrawMesh(RuntimeUtilities.fullscreenTriangle, Matrix4x4.identity, uberSheet.material, 0, 0, uberSheet.properties);

                 //   cmd.CopyRT(rt, ref RenderTargetHandle.CameraTarget.rtID);
                    //not use rt2 again(grab pass)
                    context.renderflag.SetFlag(EngineContext.RFlag_Capture, true);
                    context.captureSize.z = 1;
                    return true;
                }
                else if (context.captureSize.z == 1)
                {
                    if (context.setupCaptureCb != null)
                    {
                        //before real capture
                        context.setupCaptureCb();
                    }

                    ShaderManager.SetGlobalSettings(ShaderManager.ShaderGlobalSetting.Rt1zForUirt, true);
                    Shader.SetGlobalColor(ShaderManager._BackgroundColor, context.captureColor.linear);
                    context.renderflag.SetFlag(EngineContext.RFlag_Capture, false);
                    //copy current rt
                    if (context.captureRT != null)
                    {
                        float t = (float)pixelHeight / pixelWidth;
                        cmd.SetGlobalVector(ShaderManager._UVTransform, new Vector4(t, 1, 0, 0));
                        cmd.CopyRT(ref _MainRT1.rtID, context.captureRT, 2);
                    }
                    var rt = GetBlurRT();
                   // cmd.CopyRT(rt, ref RenderTargetHandle.CameraTarget.rtID);
                    context.captureSize.z++;
                    return true;
                }
                else if (context.captureSize.z == 2)
                {
                    if (context.recoverCaptureCb != null)
                    {
                        //before real render
                        context.recoverCaptureCb();
                    }
                    if (context.captureRT != null)
                    {
                        AsyncGPUReadback.Request(context.captureRT, 0, captureCb);
                    }
                    ShaderManager.SetGlobalSettings(ShaderManager.ShaderGlobalSetting.Rt1zForUirt, false);
                    DestroyRT(ref blurRT);
                    context.captureSize.z++;
                    return true;
                }
                else if (context.captureSize.z > 2)
                {
                    context.captureSize.z++;
                    if (context.captureSize.z > 5)
                    {
                        if (context.captureCb != null)
                        {
                            //capture fail
                            //DebugLog.AddEngineLog("capture failed");
                            var data = new NativeArray<byte>();
                            context.captureCb(ref data);
                            context.captureCb = null;
                        }
                        context.setupCaptureCb = null;
                        if (context.captureRT != null)
                        {
                            RenderTexture.ReleaseTemporary(context.captureRT);
                            context.captureRT = null;
                        }
                        context.captureSize.z = 0;
                    }
                }
            }
            return false;
        }

        public void FinalBlit(EngineContext context, CommandBuffer cmd, bool hasUIPP, int pass = 0)
        {
      
            //if(!ProcessCapture(context, cmd))
            //{
                if (hasUIPP)
                {
                    if (!ProcessCapture(context, cmd))
                    {
                        cmd.SetGlobalTexture(ShaderIDs.MainTex, _MainRT1.rtID);
                        int targetID;
                        if (!flag.HasFlag(RenderContext.Flag_MainRT2Create))
                        {
                            GetTmpRT(cmd, ref RenderContext._MainRT2, 0);
                            flag.SetFlag(RenderContext.Flag_MainRT2Create, true);
                        }
                        targetID = _MainRT2.id;
                        cmd.SetRenderTarget(targetID,
                            RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                            RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
                        cmd.DrawMesh(RuntimeUtilities.fullscreenTriangle, Matrix4x4.identity, uberSheet.material, 0, pass, uberSheet.properties);
                        cmd.SetRenderTarget(RenderTargetHandle.CameraTarget.rtID,
                            RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                            RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
                        cmd.SetGlobalTexture(_SceneRT.id, targetID);
                    }
                }
                else
                {
                       var qs = QualitySettingData.current;
                       bool fxaa = qs.flag.HasFlag(QualitySet.Flag_EnableFXAA);
                    if (fxaa)
                    {
                        cmd.SetGlobalTexture(ShaderIDs.MainTex, _MainRT2.rtID);
                    }
                    else
                    {
                        cmd.SetGlobalTexture(ShaderIDs.MainTex, _MainRT1.rtID);
                    }
                  //  cmd.SetGlobalTexture(ShaderIDs.MainTex, _MainRT1.rtID);
                    cmd.SetRenderTarget(RenderTargetHandle.CameraTarget.rtID,
                        RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                        RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
                    cmd.DrawMesh(RuntimeUtilities.fullscreenTriangle, Matrix4x4.identity, uberSheet.material, 0, pass, uberSheet.properties);
                    if (fxaa)
                    {
                        if (flag.HasFlag(RenderContext.Flag_MainRT2Create))
                        {
                            ReleaseTmpRT(cmd, ref RenderContext._MainRT2);
                            flag.SetFlag(RenderContext.Flag_MainRT2Create, false);
                        }
                    }
                }
            //}            
        }

        public void ReleaseRtAndFlag (CommandBuffer cmd)
        {
            if (stateFlag.HasFlag (SFlag_BloomEnable))
            {
                ReleaseTmpRT (cmd, ref _BloomRT);
            }
            if (stateFlag.HasFlag (SFlag_DofEnable))
            {
                ReleaseTmpRT (cmd, ref _DofRT);
            }
            if (stateFlag.HasFlag(SFlag_RadialBlurEnable))
            {
                ReleaseTmpRT(cmd, ref _RadialBlurRt);
            }
            if (stateFlag.HasFlag(SFlag_RadialBlurDebugEnable))
            {
                ReleaseTmpRT(cmd, ref _RadialBlurDebugRt);
            }
            if (stateFlag.HasFlag (SFlag_GodRayEnable))
            {
                ReleaseTmpRT (cmd, ref _TmpQuarterRT0);
            }
            if (flag.HasFlag (Flag_CreateRT) && RenderPipelineManager.renderPipeline == OPRenderPipeline.LegacySRP)
            {
                cmd.ReleaseTemporaryRT (_MainRT0.id);
                cmd.ReleaseTemporaryRT (_DepthRT.id);
            }    
            if (!flag.HasFlag (Flag_UICameraRT))
            {
                if (flag.HasFlag (Flag_MainRT1Create))
                {
                    ReleaseTmpRT (cmd, ref _MainRT1);
                    flag.SetFlag(Flag_MainRT1Create, false);
                }
                if (flag.HasFlag(Flag_MainRT2Create))
                {
                    ReleaseTmpRT(cmd, ref _MainRT2);
                }
                if (flag.HasFlag(Flag_DepthShadowDepthRT))
                {
                    ReleaseTmpRT(cmd, ref _DepthShadowDepth);
                    ReleaseTmpRT(cmd, ref _DepthShadowRT);
                }
                flag.Reset ();

                ChectRTState();
            }

            EngineContext.instance.stateflag.Reset();

        }

        [Conditional("UNITY_EDITOR")]
        public void ChectRTState ()
        {
#if UNITY_EDITOR
            if (RenderTargetHandle.frameTargets.Count != 0)
            {
                var it = RenderTargetHandle.frameTargets.GetEnumerator ();
                while (it.MoveNext ())
                {
                    RenderTargetHandle.targets.TryGetValue (it.Current, out string name);
                    DebugLog.AddErrorLog2 ("not release tmp rt:{0}", name);
                }
            }
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public void AdCheck(int id)
        {
#if UNITY_EDITOR
            RenderTargetHandle.frameTargets.Add(id);
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public void RemoveCheck(int id)
        {
#if UNITY_EDITOR
            RenderTargetHandle.frameTargets.Remove(id);
#endif
        }

        public void ReleaseUIEndFrameRT (CommandBuffer cmd)
        {
            if (flag.HasFlag (Flag_MainRT1Create))
            {
                ReleaseTmpRT (cmd, ref _MainRT1);
                flag.SetFlag(Flag_MainRT1Create, false);
            }
            if (flag.HasFlag (Flag_MainRT2Create))
            {
                ReleaseTmpRT (cmd, ref _MainRT2);
            }
            if (flag.HasFlag(Flag_DepthShadowDepthRT))
            {
                ReleaseTmpRT(cmd, ref _DepthShadowDepth);
                ReleaseTmpRT(cmd, ref _DepthShadowRT);
            }
            flag.Reset ();
            ChectRTState();
        }
        public void SwitchRT ()
        {
            var tmp = currentRT;
            currentRT = des;
            des = tmp;
        }

#region RT
        public int GetFormatStride (RenderTextureFormat f)
        {
            switch (f)
            {
                case RenderTextureFormat.Shadowmap:
                    return 2;
                case RenderTextureFormat.Depth:
                    return 3;
                case RenderTextureFormat.ARGB32:
                    return 4;
                case RenderTextureFormat.RGB111110Float:
                    return 4;
                case RenderTextureFormat.ARGBHalf:
                    return 8;
                case RenderTextureFormat.R16:
                    return 2;
            }
            DebugLog.AddWarningLog2 ("not support format:{0}", f);
            return 1;
        }
        public RenderTexture CreateRT (int w, int h, string name,
            RenderTextureFormat f, ref RenderTexture rt, int depth = 0,
            FilterMode filterMode = FilterMode.Bilinear,
            TextureWrapMode wrapMode = TextureWrapMode.Clamp,
            bool useMipmap = false,
            int slice = 1,
            bool rw = false,
            int msaa = 1)
        {
            if (rt == null || rt.width != w && rt.height != h ||
                f != rt.format)
            {
                DestroyRT (ref rt);
                rt = new RenderTexture (w, h, depth, f, RenderTextureReadWrite.Linear)
                {
                    name = name,
                    hideFlags = HideFlags.DontSave,
                    filterMode = filterMode,
                    wrapMode = wrapMode,
                    anisoLevel = 0,
                    autoGenerateMips = useMipmap,
                    useMipMap = useMipmap,
                    enableRandomWrite = rw,
                    antiAliasing = msaa,
                    bindTextureMS = false
                };
                if (slice > 1)
                {
                    rt.dimension = TextureDimension.Tex2DArray;
                    rt.volumeDepth = slice;
                }
                rt.Create ();
                rtSize += w * h * GetFormatStride (f) * slice;
            }
            return rt;
        }

        public void DestroyRT (ref RenderTexture rt)
        {
            if (rt != null)
            {
                rtSize -= rt.width * rt.height * GetFormatStride (rt.format);
                EngineUtility.Destroy (rt);
                rt = null;
            }
        }
        public RenderTexture GetBlurRT ()
        {
            return CreateRT (halfWidth, halfHeight, "_BlurRT", defaultFormat, ref blurRT);
        }
        public RenderTexture GetUIBlurRT ()
        {
            return CreateRT (halfWidth, halfHeight, "_UIBlurRT", defaultFormat, ref uiBlurRT);
        }

        public RenderTexture GetShadowRT (int w, int h, int slice, int index, string name)
        {
            if (slice > 0)
            {
                return CreateRT (w, h, name, RenderTextureFormat.Shadowmap, ref rts[index], 16, FilterMode.Bilinear, TextureWrapMode.Clamp, false, slice);
            }
            else
            {
                return CreateRT (w, h, name, RenderTextureFormat.Shadowmap, ref rts[index], 16);
            }
        }

        public RenderTexture GetMultiLayerRT(int index, int rtID, string name)
        {
            var rt =  CreateRT(halfWidth, halfHeight, name, RenderTextureFormat.ARGB32, ref rts[index]);
            Shader.SetGlobalTexture(rtID, rt);
            return rt;
        }
        public void GetDepthShadowRT(CommandBuffer cmd, int rtID)
        {
            GetTmpRT(cmd, _DepthShadowRT.id, 512, 512, 0);
            //   flag.SetFlag(Flag_MainRT1Create, true);
            GetTmpRT(cmd, _DepthShadowDepth.id, 512, 512, 16,
                RenderTextureFormat.Depth, FilterMode.Bilinear);
            flag.SetFlag(Flag_DepthShadowDepthRT, true);
            cmd.SetGlobalTexture(rtID, _DepthShadowRT.rtID);
        }

        // public RenderTexture GetShadowRTArray (int w, int h, int slice)
        // {
        //     return CreateRT (w, h, "_ShadowRTArray", RenderTextureFormat.Shadowmap, ref shadowRTArray, 16, FilterMode.Bilinear, TextureWrapMode.Clamp, false, slice);
        // }

        // public RenderTexture GetShadowExtraRT0 (int w, int h)
        // {
        //     return CreateRT (w, h, "_ShadowExtraRT0", RenderTextureFormat.Shadowmap, ref shadowExtraRT0, 16);
        // }

        // public RenderTexture GetShadowExtraRT1 (int w, int h)
        // {
        //     return CreateRT (w, h, "_ShadowExtraRT1", RenderTextureFormat.Shadowmap, ref shadowExtraRT1, 16);
        // }

        // public RenderTexture GetShadowTmpRT (int w, int h)
        // {
        //     return CreateRT (w, h, "_ShadowTmpRT", RenderTextureFormat.Shadowmap, ref shadowTmpRT, 16);
        // }

        public RenderTexture GetBakeLightRT (int w, int h)
        {
            return CreateRT (w, h, "_BakedLightRT", RenderTextureFormat.ARGB32, ref bakedLightRT, 0,
                FilterMode.Point, TextureWrapMode.Clamp, false, 1, true);
        }

        public RenderTexture GetDynamicLightRT ()
        {
            return CreateRT (64, 64, "_DynamicLightRT", RenderTextureFormat.ARGB32, ref dynamicLightRT, 0,
                FilterMode.Point, TextureWrapMode.Clamp, false, 1, true);
        }

        public void ReleaseRT ()
        {
            DestroyRT (ref blurRT);
            DestroyRT (ref uiBlurRT);
            for (int i = 0; i < rts.Length; ++i)
            {
                DestroyRT (ref rts[i]);
            }
            DestroyRT (ref sceneRT0);
            DestroyRT (ref sceneRT1);
            DestroyRT (ref ppTmpRT);
            DestroyRT (ref bakedLightRT);
            DestroyRT (ref dynamicLightRT);
            rtSize = 0;

#if UNITY_EDITOR
            if (debugRT != null)
            {
                EngineUtility.Destroy (debugRT);
                debugRT = null;
            }
#endif
        }

#if UNITY_EDITOR
        private bool DebugRT (int debugMode)
        {
            if (currentDebugMode == debugMode)
            {
                if (debugRT == null)
                {
                debugRT = new RenderTexture (pixelWidth, pixelHeight, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear)
                {
                name = "_DebugRT",
                hideFlags = HideFlags.DontSave,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                anisoLevel = 0,
                autoGenerateMips = false,
                useMipMap = false,
                    };
                    debugRT.Create ();
                    Shader.SetGlobalTexture ("_DebugRT", debugRT);
                }
                return true;

            }
            return false;
        }
#endif

        [Conditional ("UNITY_EDITOR")]
        public void DebugRT (string debugMode, ref RenderTargetIdentifier rt, CommandBuffer cmd, bool debugAlpha = false)
        {
#if UNITY_EDITOR
            if (DebugRT (AssetsConfig.FindDebugIndex (debugMode, true)))
            {
                string debugName = "Debug." + debugMode.Replace ("/", ".");
                cmd.BeginSample (debugName);
                cmd.BlitFullscreenTriangle (ref rt, debugRT, RuntimeUtilities.GetCopyMaterial (), CopyPass_Defalut);
                cmd.EndSample (debugName);
            }
#endif
        }

        public void GetTmpRT (CommandBuffer cmd,
            int nameID,
            int width,
            int height,
            int depth,
            RenderTextureFormat f = RenderTextureFormat.Default,
            FilterMode filterMode = FilterMode.Bilinear,
            RenderTextureMemoryless memoryless = RenderTextureMemoryless.None)
        {
            RenderTextureFormat targetFormat = defaultFormat;
            if (f != RenderTextureFormat.Default && f.IsSupported ())
            {
                targetFormat = f;
            }
            cmd.GetTemporaryRT (nameID, width, height, depth, filterMode, targetFormat, RenderTextureReadWrite.Linear, 1, false, memoryless);
            tmpRTSize += width * height * GetFormatStride (targetFormat);
#if UNITY_EDITOR
            AddTmpRT (nameID);
#endif
        }

#if UNITY_EDITOR
        public void AddTmpRT (int nameID)
        {
            if (RenderTargetHandle.frameTargets.Contains (nameID))
            {
                string name = "";
                RenderTargetHandle.targets.TryGetValue (nameID, out name);
                DebugLog.AddErrorLog2 ("alloc tmp rt duplicate:{0}", name);
            }
            else
            {
                RenderTargetHandle.frameTargets.Add (nameID);
            }
        }
#endif
        public void GetTmpRT (CommandBuffer cmd,
            int nameID,
            int mip,
            GraphicsFormat f,
            int bytesSize,
            FilterMode filterMode = FilterMode.Bilinear)
        {
            int w = pixelWidth >> mip;
            int h = pixelHeight >> mip;
            cmd.GetTemporaryRT (nameID, w, h, 0, filterMode, f, 1, false, RenderTextureMemoryless.None, false);
            tmpRTSize += w * h * bytesSize;
#if UNITY_EDITOR
            AddTmpRT (nameID);
#endif
        }
        public void GetTmpRT (CommandBuffer cmd,
            int nameID,
            int mip,
            RenderTextureFormat f = RenderTextureFormat.Default,
            FilterMode filterMode = FilterMode.Bilinear)
        {
            int w = pixelWidth >> mip;
            int h = pixelHeight >> mip;
            GetTmpRT (cmd, nameID, w, h, 0, f, filterMode);
        }

        public void GetTmpRT (CommandBuffer cmd,
            ref RenderTargetHandle nameID,
            int mip,
            RenderTextureFormat f = RenderTextureFormat.Default,
            FilterMode filterMode = FilterMode.Bilinear)
        {
            GetTmpRT (cmd, nameID.id, mip, f, filterMode);
        }

        public void ReleaseTmpRT (CommandBuffer cmd, ref RenderTargetHandle nameID)
        {
            cmd.ReleaseTemporaryRT (nameID.id);
#if UNITY_EDITOR
            if (RenderTargetHandle.frameTargets.Contains (nameID.id))
            {
                RenderTargetHandle.frameTargets.Remove (nameID.id);
            }
            else if (!nameID.autoRelease)
            {
                string name = "";
                RenderTargetHandle.targets.TryGetValue (nameID.id, out name);
                DebugLog.AddErrorLog2 ("release tmp rt duplicate:{0}", name);
            }
#endif
        }
#if UNITY_EDITOR
        public RenderBatch AllocWorkingBatch (Renderer r, out int index, bool shadow = false, MaterialPropertyBlock mpb = null)
        {
            if (mpb == null)
            {
                RenderResContext rrc = RenderingManager.instance.GetRenderRes (r);
                mpb = shadow ? rrc.shadowMpb : rrc.mpb;
            }
            index = workingBatchCount++;
            if (workingBatchCount < workingRenderBatchRef.Count)
            {
                RenderBatch rb = workingRenderBatchRef[index];
                rb.mpbRef = mpb;
                return rb;
            }
            RenderBatch batch = new RenderBatch ();
            batch.mpbRef = mpb;
            workingRenderBatchRef.Add (batch);
            return batch;
        }

        public void SetWorkingBatch (ref RenderBatch batch, int index)
        {
            workingRenderBatchRef[index] = batch;
        }
#endif
#endregion

    }

}