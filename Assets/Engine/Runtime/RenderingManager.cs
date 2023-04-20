using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using System.Xml;
using UnityEditor;
#endif
namespace CFEngine
{
#if UNITY_EDITOR
    public delegate void SaveEnv (System.IO.BinaryWriter bw, EnvSetting s);
    public delegate void OnDrawEnvGizmo (EngineContext context, EnvModify envModify, EnvSetting src);

#endif
    public sealed class RenderingManager : IRenderManager
    {
        static RenderingManager s_Instance;

        public static RenderingManager instance
        {
            get
            {
                if (s_Instance == null)
                    s_Instance = new RenderingManager ();

                return s_Instance;
            }
        }
        public bool dataBind = false;

        public ListObjectWrapper<ISceneObject> paramObjects;
        private int runtimeParamCount = 0;

        #region Postprocess
        private PreEffect m_preEffects;
        private RenderContext rc;
        private Vector4 _fxaavalue = new Vector4(0.0312f, 0.063f, 1f, 1f);
        private int captureUIState = 0;
        // private static int CaptureUIState_None = 0;
        private static int CaptureUIState_Render = 1;
        // private static int CaptureUIState_Recover = 2;
        #endregion
#if UNITY_EDITOR
        public static OnDrawEnvGizmo[] drawEnvGizmos = new OnDrawEnvGizmo[(int) EnvSettingType.Num];

        private EnvProfile currentProfile = null;

        private Dictionary<Renderer, RenderResContext> renderCache = new Dictionary<Renderer, RenderResContext> ();

        public void SetCurrentEnvEffect (EnvProfile envProfile, bool force = false)
        {
            if (envProfile != null && (currentProfile != envProfile || force))
            {
                if (EngineContext.IsRunning)
                {
                    // BindEnvSetting (envProfile.runtimeStart, envProfile.runtimeEnd);
                }
                else
                {
                    var context = EngineContext.instance;
                    if (context != null)
                    {
                        envProfile.Refresh ();
                        currentProfile = envProfile;
                        if (currentProfile != null && context.envModifys != null)
                        {
                            for (int i = 0; i < currentProfile.settings.Count; ++i)
                            {
                                var setting = currentProfile.settings[i];
                                if (setting != null)
                                {
                                    var env = context.envModifys[(int) setting.GetEnvType ()];
                                    if (env != null)
                                    {
                                        env.profile = setting;
                                        env.runtime.BindParamaters (
                                            setting,
                                            ref currentProfile.paramObjects,
                                            ref paramObjects);
                                    }
                                }
                            }
                            dataBind = true;
                        }
                    }

                }

            }
        }

        public MaterialPropertyBlock GetMpb (Renderer r)
        {
            RenderResContext rrc = GetRenderRes (r);
            return rrc.mpb;
        }

        public RenderResContext GetRenderRes (Renderer r)
        {
            RenderResContext rrc;
            if (!renderCache.TryGetValue (r, out rrc))
            {
                rrc = new RenderResContext ();
                rrc.r = r;
                rrc.mpb = CommonObject<MaterialPropertyBlock>.Get ();
                rrc.shadowMpb = CommonObject<MaterialPropertyBlock>.Get ();
                renderCache.Add (r, rrc);
            }
            return rrc;
        }

        public void Dump ()
        {
            EngineContext context = EngineContext.instance;
            if (context != null && context.envModifys != null)
            {
                var now = System.DateTime.Now;
                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo ("Assets");
                string dirPath = dir.FullName + "/../Dump";
                if (!System.IO.Directory.Exists (dirPath))
                    System.IO.Directory.CreateDirectory (dirPath);
                string path = string.Format ("{0}/EnvProfileDump_{1}-{2}-{3}_{4}-{5}-{6}.xml",
                    dirPath,
                    now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
                var doc = new XmlDocument ();
                var root = doc.CreateElement ("Settings");
                doc.AppendChild (root);
                for (int i = 0; i < context.envModifys.Length; ++i)
                {
                    var env = context.envModifys[i];
                    if (env != null && env.modify != null)
                    {
                        string name = env.profile.GetType ().Name;
                        XmlElement setting = doc.CreateElement (name);
                        for (int j = 0; j < env.modify.shaderKeyValue.Count; ++j)
                        {
                            var sv = env.modify.shaderKeyValue[j];
                            XmlElement elem = doc.CreateElement ("Param");
                            elem.SetAttribute (sv.name, sv.value);
                            setting.AppendChild (elem);
                        }
                        root.AppendChild (setting);
                    }
                }
                doc.Save (path);
                path = path.Replace ("/", "\\");
                System.Diagnostics.Process.Start ("Explorer", "/select," + path);
            }
        }
        public bool NeedInit ()
        {
            return rc == null || !rc.IsValid ();
        }

        public RenderContext GetContext ()
        {
            return rc;
        }
#endif
        #region  Init/Uninit
        public static void InitEnvCreator ()
        {
            RuntimeEnvModify.settingCreators = new EnvCreator[(int) EnvSettingType.Num];
            RuntimeEnvModify.settingCreators[(int) EnvSettingType.Lighting] =
                (out EnvSetting setting, out EnvModify modify) => EnvSetting.Create<Lighting, LightingModify> (out setting, out modify);
            RuntimeEnvModify.settingCreators[(int) EnvSettingType.Ambient] =
                (out EnvSetting setting, out EnvModify modify) => EnvSetting.Create<Ambient, AmbientModify> (out setting, out modify);
            RuntimeEnvModify.settingCreators[(int) EnvSettingType.Fog] =
                (out EnvSetting setting, out EnvModify modify) => EnvSetting.Create<Fog, FogModify> (out setting, out modify);
            RuntimeEnvModify.settingCreators[(int) EnvSettingType.Shadow] =
                (out EnvSetting setting, out EnvModify modify) => EnvSetting.Create<Shadow, ShadowModify> (out setting, out modify);
            RuntimeEnvModify.settingCreators[(int) EnvSettingType.SceneMisc] =
                (out EnvSetting setting, out EnvModify modify) => EnvSetting.Create<SceneMisc, SceneMiscModify> (out setting, out modify);
            RuntimeEnvModify.settingCreators[(int) EnvSettingType.PPBloom] =
                (out EnvSetting setting, out EnvModify modify) => EnvSetting.Create<Bloom, BloomModify> (out setting, out modify);
            RuntimeEnvModify.settingCreators[(int) EnvSettingType.PPVolumLight] =
                (out EnvSetting setting, out EnvModify modify) => EnvSetting.Create<GodRay, GodRayModify> (out setting, out modify);
            RuntimeEnvModify.settingCreators[(int) EnvSettingType.PPVignette] =
                (out EnvSetting setting, out EnvModify modify) => EnvSetting.Create<Vignette, VignetteModify> (out setting, out modify);
            RuntimeEnvModify.settingCreators[(int) EnvSettingType.PPTonemapping] =
                (out EnvSetting setting, out EnvModify modify) => EnvSetting.Create<ColorGrading, ColorGradingModify> (out setting, out modify);
            RuntimeEnvModify.settingCreators[(int) EnvSettingType.PPDepthOfField] =
                (out EnvSetting setting, out EnvModify modify) => EnvSetting.Create<DepthOfField, DepthOfFieldModify> (out setting, out modify);
            RuntimeEnvModify.settingCreators[(int) EnvSettingType.PPMotionBlur] =
                (out EnvSetting setting, out EnvModify modify) => EnvSetting.Create<MotionBlur, MotionBlurModify> (out setting, out modify);
            RuntimeEnvModify.settingCreators[(int) EnvSettingType.PPRadialBlur] =
                (out EnvSetting setting, out EnvModify modify) => EnvSetting.Create<RadialBlur, RadialBlurModify> (out setting, out modify);
            RuntimeEnvModify.settingCreators[(int) EnvSettingType.PPRTBlur] =
                (out EnvSetting setting, out EnvModify modify) => EnvSetting.Create<RTBlur, RTBlurModify> (out setting, out modify);
            RuntimeEnvModify.settingCreators[(int) EnvSettingType.PPDistortion] =
                (out EnvSetting setting, out EnvModify modify) => EnvSetting.Create<DistortionScreen, DistortionScreenModify> (out setting, out modify);
            RuntimeEnvModify.settingCreators[(int)EnvSettingType.Skybox] =
                (out EnvSetting setting, out EnvModify modify) => EnvSetting.Create<Skybox, SkyboxModify>(out setting, out modify);
#if UNITY_EDITOR
            drawEnvGizmos[(int) EnvSettingType.SceneMisc] = SceneMiscModify.OnDrawGizmos;
            drawEnvGizmos[(int) EnvSettingType.Shadow] = ShadowModify.OnDrawGizmos;
            drawEnvGizmos[(int) EnvSettingType.Lighting] = LightingModify.OnDrawGizmos;
            drawEnvGizmos[(int) EnvSettingType.PPDepthOfField] = DepthOfFieldModify.OnDrawGizmos;
#endif
        }
        private void InitEnv (EngineContext context)
        {
            ListObjectWrapper<ISceneObject>.Get (ref paramObjects);
            context.envModifys = new RuntimeEnvModify[(int) EnvSettingType.Num];

            EnvSetting.paramIndex.Clear ();
            for (int i = 0; i < context.envModifys.Length; ++i)
            {
                var creator = RuntimeEnvModify.settingCreators[i];
                if (creator != null)
                {
                    RuntimeEnvModify rem = CFAllocator.Allocate<RuntimeEnvModify> ();
                    creator (out rem.runtime, out rem.modify);
#if UNITY_EDITOR
                    rem.modify.BeginDump ();
#endif
                    rem.runtime.InitParamaters (paramObjects, rem.modify);
#if UNITY_EDITOR
                    rem.runtime.InitEditorParamaters (paramObjects, rem.modify, false);
#endif
                    rem.runtime.EndInitParamaters (paramObjects);
                    rem.modify.SetSettings (rem.runtime);

                    context.envModifys[i] = rem;
                }
            }

            runtimeParamCount = paramObjects.Count;
        }

        public void Init (EngineContext context)
        {
            InitEnvCreator ();
            InitEnv (context);

            // RuntimeUtilities.CreateIfNull (ref m_dithering);
            rc = RenderContext.singleton;
            rc.Init (context);
            RuntimeUtilities.CreateIfNull (ref m_preEffects);
            m_preEffects.Init (rc);
        }

        public void Uninit (EngineContext context)
        {
            End (context);
            context.envModifys = null;
            if (paramObjects.IsValid ())
            {
                ListObjectWrapper<ISceneObject>.Release (ref paramObjects);
            }
            runtimeParamCount = 0;
            RuntimeEnvModify.settingCreators = null;

            if (rc != null)
            {
                rc.Uninit (context);
                rc = null;
            }
        }

        public void Start (EngineContext context)
        {
            
#if UNITY_EDITOR
            EnvironmentExtra ee;
            if (!context.CameraRef.TryGetComponent (out ee))
            {
                context.CameraRef.gameObject.AddComponent<EnvironmentExtra> ();
            }
            RenderLayer rl;
            if (!context.CameraRef.TryGetComponent (out rl))
            {
                context.CameraRef.gameObject.AddComponent<RenderLayer> ();
            }
#endif
           // GrassInteractManager.singleton.Inst();
            if (rc != null)
            {
                rc.Start (context);
            }
            SetPPEnable (context, true);
        }

        public void End (EngineContext context)
        {
            dataBind = false;
            if (context.envModifys != null)
            {
                for (int i = 0; i < context.envModifys.Length; ++i)
                {
                    var rem = context.envModifys[i];
                    if (rem != null)
                    {
                        if (rem.runtime != null)
                        {
                            rem.runtime.UninitParamaters ();
                            if (paramObjects.IsValid())
                                rem.runtime.UnBindParamaters (ref paramObjects);
                        }
                        if (rem.profile != null)
                        {
                            rem.profile.UninitParamaters ();
                            rem.profile = null;
                        }
                        if (rc != null)
                            rem.modify.Release(context, rc);
                    }
                }
            }

            if (paramObjects.IsValid ())
                paramObjects.RemoveRange (runtimeParamCount);
#if UNITY_EDITOR
            if (EngineContext.IsRunning)
            {
                if (currentProfile != null)
                {
                    currentProfile.settings.Clear ();
                }
                RenderLayer.envProfile = null;
            }
            foreach (var rrc in renderCache.Values)
            {
                if (rrc.mpb != null)
                {
                    CommonObject<MaterialPropertyBlock>.Release (rrc.mpb);
                }
            }
            renderCache.Clear ();
#endif
            if (rc != null)
            {
                rc.End (context);
            }
           // GrassInteractManager.singleton.OnRelease();
        }
        #endregion
        #region interface
        public void SetPPEnable (EngineContext context, bool enable)
        {
            if (rc != null)
            {
                var qs = QualitySettingData.current;
                if (!qs.flag.HasFlag (QualitySet.Flag_EnablePP))
                    enable = false;
                rc.stateFlag.SetFlag (RenderContext.SFlag_PPEnable, enable);
                context.renderflag.SetFlag (EngineContext.RFlag_PrePPDirty, true);
                context.renderflag.SetFlag (EngineContext.RFlag_PPDirty, true);
            }
        }
        public void Update (EngineContext context)
        {
            if (rc != null && rc.IsValid ())
            {
                rc.Update (context);
               // GrassInteractManager.singleton.Updatedata();
#if UNITY_EDITOR
                context.shadowDebug = AssetsConfig.instance.DrawShadowMapExtra;
                if (EngineContext.IsRunning)
#endif
                {
                    context.PostUpdate ();
                }
                UpdateEnv(context);
                UpdateEffect(context);
                if (context.renderflag.HasFlag (EngineContext.RFlag_RenderEnable))
                {
                    PostUpdateEffect(context);
                }
            }
        }

        public void Render (EngineContext context)
        {
#if !PIPELINE_URP
            if (rc != null && rc.IsValid ())
            {
                EnvHelp.RenderEffect (context, rc, EnvSettingType.SceneMisc);
                EnvHelp.RenderEffect (context, rc, EnvSettingType.Shadow);
                rc.PreRender (context);
                UpdatePostprocess (context);
                context.ResetFrameData ();
            }
#endif
        }
        #endregion

        #region env stack
        private void UpdateEffect(EngineContext context)
        {
            if (rc != null && dataBind)
            {
#if UNITY_EDITOR
                if (!EngineContext.IsRunning)
                {
                    EnvModify.dump = context.frameCount % 30 == 0;
                }
#endif
                EnvHelp.UpdateEffect(context, rc, EnvSettingType.SceneMisc);
                EnvHelp.UpdateEffect(context, rc, EnvSettingType.Lighting);
                EnvHelp.UpdateEffect(context, rc, EnvSettingType.Ambient);
                EnvHelp.UpdateEffect(context, rc, EnvSettingType.Fog);
                EnvHelp.UpdateEffect(context, rc, EnvSettingType.Shadow);
            }
        }
        private void PostUpdateEffect (EngineContext context)
        {
            if (rc != null && dataBind)
            {
#if UNITY_EDITOR
                if (!EngineContext.IsRunning)
                {
                    EnvModify.dump = context.frameCount % 30 == 0;
                }
#endif
                EnvHelp.UpdateEffect (context, rc, EnvSettingType.PPTonemapping);
                EnvHelp.UpdateEffect (context, rc, EnvSettingType.PPVolumLight);
                EnvHelp.UpdateEffect (context, rc, EnvSettingType.PPBloom);
                EnvHelp.UpdateEffect (context, rc, EnvSettingType.Skybox);
            }
        }
        #endregion

        #region postprocess stack
        public void UpdatePostprocess (EngineContext context)
        {
            if (!rc.stateFlag.HasFlag (RenderContext.SFlag_PPEnable) &&
                context.CameraRef != null)
            {
                rc.ReleaseRtAndFlag (rc.postPPCommand);
                return;
            }
            BuildCommandBuffers (context);
            BuildUICommandBuffers ();
        }

        private void BuildCommandBuffers (EngineContext context)
        {
            PreRenderPP (context);
            RenderPP (context);
            PostRenderPP (context);
        }

        public void PreRenderPP (EngineContext context)
        {
            m_preEffects.PreRenderAfterOpaque (context, rc);
#if UNITY_EDITOR
            if (!EngineContext.IsRunning)
            {
                context.renderflag.SetFlag (EngineContext.RFlag_PrePPDirty, true);
            }
#endif

            if (context.renderflag.HasFlag (EngineContext.RFlag_PrePPDirty))
            {
                context.renderflag.SetFlag (EngineContext.RFlag_PrePPDirty, false);
                var cmd = rc.prePPCommand;
                if (cmd != null)
                {
                    cmd.Clear ();
                    rc.stateFlag.SetFlag(RenderContext.SFlag_DofEnable, false);
                    rc.stateFlag.SetFlag(RenderContext.SFlag_RadialBlurEnable, false);
                    rc.stateFlag.SetFlag(RenderContext.SFlag_RadialBlurDebugEnable, false);
#if UNITY_EDITOR
                    RenderContext._MainRT1.autoRelease = false;
#endif
                    RuntimeUtilities.BeginProfile (cmd, "PreEffect");
                    m_preEffects.Render (context, rc, cmd);
                    RuntimeUtilities.EndProfile (cmd, "PreEffect");
                }
            }
            else
            {
                rc.flag.SetFlag(RenderContext.Flag_MainRT1Create, true);
                m_preEffects.LogicRender (context, rc);
            }
        }

        public void RenderPP (EngineContext context)
        {
#if UNITY_EDITOR
            if (!EngineContext.IsRunning)
            {
                context.renderflag.SetFlag (EngineContext.RFlag_PPDirty, true);
            }
#endif
            if (context.renderflag.HasFlag(EngineContext.RFlag_PPDirty))
            {
                context.renderflag.SetFlag (EngineContext.RFlag_PPDirty, false);
                var cmd = rc.ppCommand;
                if (cmd != null)
                {
                    cmd.Clear ();
                    var uberSheet = rc.uberSheet;
                    uberSheet.ClearKeywords ();
                    uberSheet.properties.Clear ();
                    rc.flag.SetFlag (RenderContext.Flag_UberClear, true);
                    rc.stateFlag.SetFlag (RenderContext.SFlag_BloomEnable, false);
                    rc.stateFlag.SetFlag (RenderContext.SFlag_ColorgradingEnable, false);
#if UNITY_EDITOR
                    RenderContext._TmpRT0.autoRelease = false;
#endif
                    RuntimeUtilities.BeginProfile (cmd, "PostProcess");
                    EnvHelp.RenderEffect (context, rc, EnvSettingType.PPBloom);
                    EnvHelp.RenderEffect (context, rc, EnvSettingType.PPTonemapping);
                    RuntimeUtilities.EndProfile (cmd, "PostProcess");
                }
            }
        }

        public void PostRenderPP (EngineContext context)
        {
            // if (m_renderContext.stateFlag.HasFlag (RenderContext.SFlag_PostPPDirty))
            {
                var cmd = rc.postPPCommand;
                if (cmd != null)
                {
                    cmd.Clear ();
                    var uberSheet = rc.uberSheet;
#if UNITY_EDITOR
                    uberSheet.EnableKeyword (ShaderManager._ShaderKeyDebugAPP);
#endif
                    RuntimeUtilities.BeginProfile (cmd, "Post Process Uber");
                    EnvHelp.RenderEffect (context, rc, EnvSettingType.PPVignette);
                    m_preEffects.PostRender (context, rc);
                    var qs = QualitySettingData.current;
                    bool fxaa = qs.flag.HasFlag(QualitySet.Flag_EnableFXAA);
                    if (context.renderflag.HasFlag (EngineContext.RFlag_CamertRT))
                    {
                        ApplyFlip (false, uberSheet.properties);
                        rc.FinalBlit (context, cmd, true);
                        rc.flag.SetFlag (RenderContext.Flag_UICameraRT, true);
                         FXAAPASS(context, cmd, uberSheet);
                    }
                    else
                    {
                      
                        ApplyFlip (!fxaa, uberSheet.properties);
                        rc.FinalBlit (context,cmd, fxaa);
                        DrawDebugTex (cmd, context);
                        FXAAPASS(context, cmd, uberSheet);

#if UNITY_EDITOR
                        if (RenderContext.capturing)
                        {
                            if (RenderLayer.caputerRT != null)
                            {
                                ApplyFlip (false, uberSheet.properties);
                                cmd.BlitFullscreenTriangle (ref rc.currentRT, RenderLayer.caputerRT, uberSheet, 0);
                            }
                            RenderContext.capturing = false;
                            ColorGrading.breakLUT = false;
                        }
#endif
                        if (RenderPipelineManager.renderPipeline == OPRenderPipeline.Builtin)
                            rc.ReleaseRtAndFlag (cmd);
                    }

                    RuntimeUtilities.EndProfile (cmd, "Post Process Uber");

                }

                // m_renderContext.stateFlag.SetFlag (RenderContext.SFlag_PostPPDirty, false);
            }

#if UNITY_EDITOR
            // RuntimeUtilities.BeginProfile (cmd, "ui");
            // RuntimeUtilities.EndProfile (cmd, "ui");
#endif
        }

        private void FXAAPASS(EngineContext context, CommandBuffer cmd, PropertySheet uberSheet)
        {
            var qs = QualitySettingData.current;
            bool fxaa = qs.flag.HasFlag(QualitySet.Flag_EnableFXAA);
            if (!fxaa)
                return;
            QualityLevel antialiasinglevel = qs.antialiasinglevel;
            switch (antialiasinglevel)
            {
                case QualityLevel.High:
                    _fxaavalue.w = 1.0f;
                    break;
                case QualityLevel.Medium:
                    _fxaavalue.w = 1.0f;
                    break;
                case QualityLevel.Low:
                    _fxaavalue.w = 0.5f;
                    break;
                default:
                    break;
            }
            Shader.SetGlobalVector(ShaderManager._AntLevel, _fxaavalue);
            ApplyFlip(true, uberSheet.properties);
            rc.SwitchRT();
            rc.FinalBlit(context, cmd, false, 1);
        }


        private void DrawDebugTex (CommandBuffer cmd, EngineContext context)
        {
            switch (context.debugTex)
            {
                case EDebugTex.Layer0:
                case EDebugTex.Layer1:
                case EDebugTex.Layer2:
                    {
                        var mat = RuntimeUtilities.GetDebugMat ();
                        if (mat != null)
                        {
                            var viewPort = new Rect (rc.pixelWidth - rc.pixelHeight, 0, rc.pixelHeight, rc.pixelHeight);
                            cmd.SetViewport (viewPort);
                            mat.SetTexture ("_ShadowMap", rc.rts[RenderContext.SceneShadowRT]);
                            mat.SetInt ("_Slice", (int) (context.debugTex - EDebugTex.Layer0));
                            cmd.DrawMesh (RuntimeUtilities.fullscreenTriangle, Matrix4x4.identity, mat, 0, 1);
                        }
                        break;
                    }
                case EDebugTex.ExtraShaow:
                case EDebugTex.ExtraShaow1:
                    {
                        var mat = RuntimeUtilities.GetDebugMat ();
                        if (mat != null)
                        {
                            int index = context.debugTex - EDebugTex.ExtraShaow;
                            var viewPort = new Rect (rc.pixelWidth - rc.pixelHeight, 0, rc.pixelHeight, rc.pixelHeight);
                            cmd.SetViewport (viewPort);
                            mat.SetTexture ("_ShadowMap1", rc.rts[RenderContext.EXTShadowRT + index]);
                            cmd.DrawMesh (RuntimeUtilities.fullscreenTriangle, Matrix4x4.identity, mat, 0, 2);
                        }
                        break;
                    }
                case EDebugTex.SelfShadow:
                    {
                        break;
                    }
                case EDebugTex.Tmp0:
                case EDebugTex.Tmp1:
                case EDebugTex.Tmp2:
                    {
                        var mat = RuntimeUtilities.GetDebugMat();
                        if (mat != null)
                        {
                            int index = context.debugTex - EDebugTex.Tmp0;
                            var viewPort = new Rect(rc.pixelWidth - rc.pixelHeight, 0, rc.pixelHeight, rc.pixelHeight);
                            cmd.SetViewport(viewPort);
                            mat.SetTexture("_ShadowMap1", rc.rts[RenderContext.SceneTmpShadowRT0 + index]);
                            cmd.DrawMesh(RuntimeUtilities.fullscreenTriangle, Matrix4x4.identity, mat, 0, 2);
                        }
                        break;
                    }
            }
        }
        private void BuildUICommandBuffers ()
        {
            // if (captureUIState == CaptureUIState_Render)
            // {
            //     if (m_engineContetRef.HasFlag (EngineContext.EnableDepth))
            //     {
            //         m_engineContetRef.uiCamera.SetTargetBuffers (m_renderContext.sourceRT.colorBuffer, m_renderContext.depthRT.depthBuffer);
            //     }
            //     else
            //     {
            //         m_engineContetRef.CameraRef.targetTexture = m_renderContext.sourceRT;
            //         // m_engineContetRef.uiCamera.SetTargetBuffers (m_renderContext.sourceRT.colorBuffer, new RenderBuffer ());
            //     }
            //     if (m_CmdUIPostProcess != null)
            //         m_CmdUIPostProcess.Clear ();

            //     m_renderContext.ppCommand = m_CmdUIPostProcess;
            //     var rt = m_renderContext.GetUIBlurRT ();
            //     RTBlurModify.BlurRT (ref m_renderContext.sourceID, rt, m_renderContext, ShaderIDs.UIBlurRT);

            //     var copySheet = m_renderContext.propertySheets.Get (m_renderContext.resources.shaders.copy);
            //     ApplyFlip (copySheet.properties);
            //     m_CmdUIPostProcess.BlitFullscreenTriangle (ref m_renderContext.sourceID, ref cameraTarget, copySheet, 1, true);
            //     captureUIState = CaptureUIState_Recover;
            // }
            // else if (captureUIState == CaptureUIState_Recover)
            // {
            //     if (m_CmdUIPostProcess != null)
            //         m_CmdUIPostProcess.Clear ();
            //     m_engineContetRef.uiCamera.SetTargetBuffers (new RenderBuffer (), new RenderBuffer ());
            //     captureUIState = CaptureUIState_None;
            // }
        }

        private void ApplyFlip (bool flip, MaterialPropertyBlock properties)
        {
            if (flip)
            {
                properties.SetVector (ShaderIDs.UVTransform, new Vector4 (1.0f, 1.0f, 0.0f, 0.0f));
            }
            else
            {
                properties.SetVector (ShaderIDs.UVTransform, SystemInfo.graphicsUVStartsAtTop ? new Vector4 (1.0f, -1.0f, 0.0f, 1.0f) : new Vector4 (1.0f, 1.0f, 0.0f, 0.0f));
            }
        }

        public void UpdatePostRender ()
        {
            // if (uiCaptureCount > 0)
            {
                //uiCaptureCount -= 1;
                // if (uiCaptureCount == 0)
                // {
                //     if (m_engineContetRef != null && m_engineContetRef.uiCamera != null)
                //     {
                //         if (m_UILegacyCmdBuffer != null)
                //             m_UILegacyCmdBuffer.Clear ();
                //         m_engineContetRef.uiCamera.SetTargetBuffers (new RenderBuffer (), new RenderBuffer ());
                //     }
                // }
            }
        }
        #endregion

        #region env
        public void InitEnvSetting (EngineContext context, EnvSetting setting)
        {
            EnvSetting.forceOverride = true;
            setting.InitParamaters (paramObjects, null);
            
#if UNITY_EDITOR
            setting.InitEditorParamaters (paramObjects, null, false);
#endif
            EnvSetting.forceOverride = false;
            var env = context.envModifys[(int) setting.GetEnvType ()];
            if (env != null)
            {
                env.profile = setting;
                env.runtime.BindParamaters (setting, ref paramObjects, ref paramObjects);
            }
            dataBind = true;
#if UNITY_EDITOR
            if (EngineContext.IsRunning)
            {
                if (currentProfile == null)
                {
                    currentProfile = ScriptableObject.CreateInstance<EnvProfile> ();
                }
                RenderLayer.envProfile = currentProfile;
                currentProfile.settings.Add (setting);
            }
#endif
        }

        #endregion

        #region paramoverride
        [System.Obsolete ("timeline used,will be remove soon")]
        public void ResetEnableEffect (int effectType)
        {
            EngineContext context = EngineContext.instance;
            if (context != null && context.envModifys != null)
            {
                var env = context.envModifys[effectType];
                if (env != null && env.profile != null)
                {
                    env.profile.ResetEffect ();
                    // if (env.global.HasPostprocessEffect ())
                    // {
                    //     ppUpdateNeeded = true;
                    // }
                }
            }
        }

        [System.Obsolete ("timeline used,will be remove soon")]
        public void SetEffectParam (int effectType, int paramIndex, ParamOverride po, byte mask)
        {
            EngineContext context = EngineContext.instance;
            if (context != null && context.envModifys != null)
            {
                var env = context.envModifys[effectType];
                if (env != null && env.runtime != null)
                {
                    // if (env.runtime.SetParamValue (ref paramObjects, paramIndex, po, mask))
                    // {
                    //     // if (env.global != null)
                    //     //     env.modify.RecoverSetting(env.global, null);
                    // }
                    // if (env.global != null && env.global.HasPostprocessEffect())
                    // {
                    //     ppUpdateNeeded = true;
                    // }
                }
            }
        }

        [System.Obsolete ("timeline used,will be remove soon")]
        public void ResetEffectParam (int effectType, int paramIndex)
        {
            EngineContext context = EngineContext.instance;
            if (context != null && context.envModifys != null)
            {
                var env = context.envModifys[effectType];
                if (env != null && env.runtime != null)
                {
                    // if (env.runtime.ResetParamValue (ref paramObjects, paramIndex))
                    // {
                    //     // if (env.global != null)
                    //     //     env.modify.RecoverSetting(env.global, null);
                    // }
                    // if (env.global != null && env.global.HasPostprocessEffect())
                    // {
                    //     ppUpdateNeeded = true;
                    // }
                }
            }
        }

        public ref ListObjectWrapper<ISceneObject> GetParamObjects ()
        {
            return ref paramObjects;
        }
#if UNITY_EDITOR
        public ref ListObjectWrapper<ISceneObject> GetProfileParamObjects ()
        {
            if (currentProfile != null)
                return ref currentProfile.paramObjects;
            return ref EnvProfile.emptyParamObjects;
        }
#endif
        public ParamOverride GetEffectParam (EnvSetting setting, int paramIndex)
        {
            return null;
            // return setting.GetParam (ref paramObjects, paramIndex);
        }

        public void SwitchSkyBox (string path)
        {
            EngineContext context = EngineContext.instance;
            if (context != null && context.envModifys != null)
            {
                var env = context.envModifys[(int)EnvSettingType.Skybox];
                if (env != null)
                {

                    UnityEngine.Rendering.Volume vol = UnityEngine.Rendering.VolumeManager.instance.GetVolume("Baratie02_Volume");
                    //GameObject bVol = GameObject.Find("Baratie02_Volume");
                    //if (bVol != null)
                    //{
                        //UnityEngine.Rendering.Volume vol = bVol.GetComponent<Volume>();
                    if (vol != null)
                    {
                        if(vol.profile.TryGet<UnityEngine.Rendering.Universal.Skybox>(out UnityEngine.Rendering.Universal.Skybox sky))
                            sky.Atmospher2Custom();
                    }       
                    else
                    {
                        Debug.LogError("Can not find the volume!");
                    }
                    //}
                    SkyboxModify skybox = env.modify as SkyboxModify;
                    if (skybox != null)
                    {
                        skybox.SwitchSkyBox(path);
                    }
                }
                else
                {
                    env = context.envModifys[(int)EnvSettingType.Ambient];
                    if(env != null)
                    {
                        AmbientModify ambient = env.modify as AmbientModify;
                        if (ambient != null)
                        {
                            ambient.SwitchSkyBox(path);
                        }
                    }
                }

            }
        }

        //public void CaptureSceneBGImage ()
        //{
        //    EnableEffect ((int) EnvSettingType.PPRTBlur, 1);
        //}

        public void CaptureUIBGImage (EngineContext context)
        {
            if (context != null && context.uiCamera != null)
            {
                // if (!attachUICmd)
                // {
                //     m_engineContetRef.uiCamera.AddCommandBuffer (CameraEvent.BeforeImageEffects, m_CmdUIPostProcess);
                //     attachUICmd = true;
                // }
                captureUIState = CaptureUIState_Render;
                DebugLog.AddEngineLog ("capture UI");
            }
        }
#if UNITY_EDITOR
        public EnvSetting GetRuntimeSetting (int effectType)
        {
            EngineContext context = EngineContext.instance;
            if (context != null && context.envModifys != null)
            {
                var env = context.envModifys[effectType];
                if (env != null)
                {
                    return env.runtime;
                }
            }
            return null;
        }
#endif
        #endregion

        #region  public
        #region render

#if UNITY_EDITOR
        public void OnDrawGizmos (EngineContext context)
        {
            if (context != null && context.envModifys != null)
            {
                for (int i = 0; i < drawEnvGizmos.Length; ++i)
                {
                    var drawEnvGizmo = drawEnvGizmos[i];
                    if (drawEnvGizmo != null)
                    {
                        var env = context.envModifys[i];
                        if (env != null && env.modify != null)
                        {
                            drawEnvGizmo (context, env.modify, env.profile);
                        }
                    }
                }
            }
        }
#endif
        #endregion
        #endregion

        #region internal

        private void UpdateEnv (EngineContext context)
        {
            if (context.envModifys != null && dataBind)
            {
                if (context.envLerpTime >= 0.0001f)
                {
                    EnvModify.lerpEnv = true;
                    float t = Mathf.Clamp01 (context.envLerpT / context.envLerpTime);
                    for (int i = 0; i < context.envModifys.Length; ++i)
                    {
                        var rem = context.envModifys[i];
                        var profile = rem != null?rem.profile : null;
                        if (profile != null && profile.active.value)
                        {
                            rem.runtime.LerpParamaters (ref paramObjects, t);
                            // rem.modify.DirtySetting ();
                        }
                    }
                }
                else
                {
                    EnvModify.lerpEnv = false;

#if UNITY_EDITOR
                    if ((!EngineContext.IsRunning))
                    {
                        if (UISceneSystem.currentEnvBlock != null)
                        {
                            EnvHelp.BindEnvBlock (context, UISceneSystem.currentEnvBlock);
                        }
                        else if (context.currentEnvBlock != null)
                        {
                            EnvHelp.BindEnvBlock (context, context.currentEnvBlock);
                        }
                    }
#endif
                    for (int i = 0; i < context.envModifys.Length; ++i)
                    {
                        var rem = context.envModifys[i];
                        var runtime = rem != null?rem.runtime : null;
                        if (runtime != null)
                        {
                            runtime.CopyParamaters (ref paramObjects);
                        }
                    }
                }
            }
        }
        #endregion

        #region common

        public void Render2RTArray (Texture tex, RenderTexture rt, int slice)
        {
            if (rc.preWorkingCmd != null)
            {
                Material mat = RuntimeUtilities.GetCopyMaterial ();
                rc.preWorkingCmd.SetGlobalTexture (ShaderIDs.MainTex, tex);
                rc.preWorkingCmd.SetRenderTarget (rt, 0, CubemapFace.Unknown, slice);
                rc.preWorkingCmd.DrawMesh (RuntimeUtilities.fullscreenTriangle, Matrix4x4.identity, mat, 0, 0, null);
            }
        }
        public Mesh GetTriMesh ()
        {
            return RuntimeUtilities.fullscreenTriangle;
        }
        public Mesh GetQuadMesh()
        {
            return RuntimeUtilities.BillboardQuad;
        }


        public bool SetCapture(int width, int height, Color c, RenderCb setupCb, RenderCb recoverCb, CaptureCb cb)
        {
            EngineContext context = EngineContext.instance;
            if (context != null)
            {
                if (context.captureCb == null)
                {
                    context.captureSize.x = width;
                    context.captureSize.y = height;
                    context.captureSize.z = 0;
                    context.captureColor = c;
                    context.setupCaptureCb = setupCb;
                    context.recoverCaptureCb = recoverCb;
                    context.captureCb = cb;
                    if (context.captureRT == null)
                        context.captureRT = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
                    else if (context.captureRT.width != width || context.captureRT.height != height)
                    {
                        RenderTexture.ReleaseTemporary(context.captureRT);
                        context.captureRT = RenderTexture.GetTemporary(width, width, 0, RenderTextureFormat.ARGB32);
                    }
                    return true;
                }
            }
            return false;
        }
        public RenderTexture CreateRT(int w, int h, string name, RenderTextureFormat f)
        {
            RenderTexture rt = null;
            return rc.CreateRT(w, h, name, f, ref rt);
        }
        public void ReleaseRT(ref RenderTexture rt)
        {
            if (rc != null)
            {
                rc.DestroyRT(ref rt);
                rt = null;
            }
        }
        #endregion 

    }
}