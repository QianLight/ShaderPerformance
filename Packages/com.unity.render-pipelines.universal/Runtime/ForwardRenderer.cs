using UnityEngine.Rendering.Universal.Internal;
using System.Reflection;
using CFEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal
{
    /// <summary>
    /// Rendering modes for Universal renderer.
    /// </summary>
    public enum RenderingMode
    {
        /// <summary>Render all objects and lighting in one pass, with a hard limit on the number of lights that can be applied on an object.</summary>
        Forward,
        /// <summary>Render all objects first in a g-buffer pass, then apply all lighting in a separate pass using deferred shading.</summary>
        Deferred
    };

    /// <summary>
    /// Default renderer for Universal RP.
    /// This renderer is supported on all Universal RP supported platforms.
    /// It uses a classic forward rendering strategy with per-object light culling.
    /// </summary>
    public sealed class ForwardRenderer : ScriptableRenderer
    {
        const int k_DepthStencilBufferBits = 32;

        private static class Profiling
        {
            private const string k_Name = nameof(ForwardRenderer);
            public static readonly ProfilingSampler createCameraRenderTarget = new ProfilingSampler($"{k_Name}.{nameof(CreateCameraRenderTarget)}");
        }

        // Rendering mode setup from UI.
        internal RenderingMode renderingMode { get { return RenderingMode.Forward; } }
        // Actual rendering mode, which may be different (ex: wireframe rendering, harware not capable of deferred rendering).
        internal RenderingMode actualRenderingMode { get { return GL.wireframe || m_DeferredLights == null || !m_DeferredLights.IsRuntimeSupportedThisFrame() ? RenderingMode.Forward : this.renderingMode; } }
        internal bool accurateGbufferNormals { get { return m_DeferredLights != null ? m_DeferredLights.AccurateGbufferNormals : false; } }
        ColorGradingLutPass m_ColorGradingLutPass;
        DepthOnlyPass m_DepthPrepass;
        // DrawObjectsPass m_SceneDepthPrepass;
        SceneDepthPrepass m_SceneDepthPrepass;
        private SceneDepthPrepass m_TransparentSceneDepthPrepass;
        DepthNormalOnlyPass m_DepthNormalPrepass;
        MainLightShadowCasterPass m_MainLightShadowCasterPass;

        AdditionalLightsShadowCasterPass m_AdditionalLightsShadowCasterPass;
        GBufferPass m_GBufferPass;
        CopyDepthPass m_GBufferCopyDepthPass;
        TileDepthRangePass m_TileDepthRangePass;
        TileDepthRangePass m_TileDepthRangeExtraPass; // TODO use subpass API to hide this pass
        DeferredPass m_DeferredPass;
        DrawObjectsPass m_RenderOpaqueForwardOnlyPass;
        DrawObjectsPass m_RenderOpaqueForwardPass;
        // DrawObjectsPass m_FaceShadowCasterPass;
        // DrawObjectsPass m_FaceShadowPass;
        // DrawObjectsPass m_PlanarShadowPass;
        // DrawObjectsPass m_RenderOutlinePass;
        // DrawObjectsPass m_RenderScreenSpaceRimPass;
        DrawRoleEffectsPass m_roleEffectsBeforeDepthPass; // Add by: Takeshi
        DrawRoleEffectsPass m_roleEffectsAfterDepthPass; // Add by: Takeshi
        //DrawObjectsPass m_ForwardTransparentPass;
        HBAOPass m_HbaoPass;
        DrawSkyboxPass m_DrawSkyboxPass;
        CopyDepthPass m_CopyDepthPass;
        CopyColorPass m_CopyColorPass;
        TransparentSettingsPass m_TransparentSettingsPass;
        DrawObjectsPass m_RenderTransparentForwardPass;
        InvokeOnRenderObjectCallbackPass m_OnRenderObjectCallbackPass;
        PostProcessPass m_PostProcessPass;

        GrassRenderPass m_GrassRenderPass;
        DecalPass m_DecalPass;

        GPUInstancingPass m_GPUInstancingColorPass;

        PostProcessPass m_FinalPostProcessPass;
        FinalBlitPass m_FinalBlitPass;

        /* Add By:Takeshi; Fix SceneView Gamma*/
#if UNITY_EDITOR
        BlitBeforeUIInSceneViewPass m_BlitBeforeUIInSceneViewPass;
        FixGammaBlitInSceneViewPass m_FixGammaBlitInSceneViewPass;
        BlitFixGammaPass m_BlitWhenPostProcessOffSceneViwePass;
#endif
        BlitFixGammaPass m_BlitFixGammaPass;
        //BlitFixGammaPass m_BlitWhenFXAAOffPass;
        StaticGaussianBlurPass m_staticGaussianBlurPass;

        private URPDistortionPass m_distortionPass;
        /* End Add */

        CapturePass m_CapturePass;
#if ENABLE_VR && ENABLE_XR_MODULE
        XROcclusionMeshPass m_XROcclusionMeshPass;
        CopyDepthPass m_XRCopyDepthPass;
#endif
#if UNITY_EDITOR
        SceneViewDepthCopyPass m_SceneViewDepthCopyPass;
        public delegate void Overdraw();
        public static Overdraw OverdrawProcess;
        OverdrawPass m_OverdrawOpaquePass;
        OverdrawPass m_OverdrawTransparentPass;
#endif
        Tempora m_TemporaPass;
        RenderTargetHandle m_ActiveCameraColorAttachment;
        RenderTargetHandle m_ActiveCameraDepthAttachment;
        RenderTargetHandle m_CameraColorAttachment;
        RenderTargetHandle m_CameraDepthAttachment;
        RenderTargetHandle m_DepthTexture;
        RenderTargetHandle m_NormalsTexture;
        RenderTargetHandle[] m_GBufferHandles;
        RenderTargetHandle m_OpaqueColor;
        RenderTargetHandle m_AfterPostProcessColor;
        RenderTargetHandle m_ColorGradingLut;
        // For tiled-deferred shading.
        RenderTargetHandle m_DepthInfoTexture;
        RenderTargetHandle m_TileDepthInfoTexture;

        ForwardLights m_ForwardLights;
        DeferredLights m_DeferredLights;
#pragma warning disable 414
        RenderingMode m_RenderingMode;
#pragma warning restore 414
        StencilState m_DefaultStencilState;

        Material m_BlitMaterial;
        Material m_CopyDepthMaterial;
        Material m_SamplingMaterial;
        Material m_ScreenspaceShadowsMaterial;
        Material m_TileDepthInfoMaterial;
        Material m_TileDeferredMaterial;
        Material m_StencilDeferredMaterial;

        bool firstSetup = true;
        public ForwardRenderer(ForwardRendererData data) : base(data)
        {
            GameQualitySetting.SetEnvironment();

#if ENABLE_VR && ENABLE_XR_MODULE
            UniversalRenderPipeline.m_XRSystem.InitializeXRSystemData(data.xrSystemData);
#endif 
            m_BlitMaterial = CoreUtils.CreateEngineMaterial(data.shaders.blitPS);
            m_CopyDepthMaterial = CoreUtils.CreateEngineMaterial(data.shaders.copyDepthPS);
            m_SamplingMaterial = CoreUtils.CreateEngineMaterial(data.shaders.samplingPS);
            m_ScreenspaceShadowsMaterial = CoreUtils.CreateEngineMaterial(data.shaders.screenSpaceShadowPS);
            //m_TileDepthInfoMaterial = CoreUtils.CreateEngineMaterial(data.shaders.tileDepthInfoPS);
            //m_TileDeferredMaterial = CoreUtils.CreateEngineMaterial(data.shaders.tileDeferredPS);
            m_StencilDeferredMaterial = CoreUtils.CreateEngineMaterial(data.shaders.stencilDeferredPS);

            StencilStateData stencilData = data.defaultStencilState;
            m_DefaultStencilState = StencilState.defaultValue;
            m_DefaultStencilState.enabled = stencilData.overrideStencilState;
            m_DefaultStencilState.SetCompareFunction(stencilData.stencilCompareFunction);
            m_DefaultStencilState.SetPassOperation(stencilData.passOperation);
            m_DefaultStencilState.SetFailOperation(stencilData.failOperation);
            m_DefaultStencilState.SetZFailOperation(stencilData.zFailOperation);

            m_ForwardLights = new ForwardLights();
            //m_DeferredLights.LightCulling = data.lightCulling;
            this.m_RenderingMode = RenderingMode.Forward;

            // Note: Since all custom render passes inject first and we have stable sort,
            // we inject the builtin passes in the before events.
            m_MainLightShadowCasterPass = new MainLightShadowCasterPass(RenderPassEvent.BeforeRenderingShadows);

            m_AdditionalLightsShadowCasterPass = new AdditionalLightsShadowCasterPass(RenderPassEvent.BeforeRenderingShadows);
#if ENABLE_VR && ENABLE_XR_MODULE
            m_XROcclusionMeshPass = new XROcclusionMeshPass(RenderPassEvent.BeforeRenderingOpaques);
            // Schedule XR copydepth right after m_FinalBlitPass(AfterRendering + 1)
            m_XRCopyDepthPass = new CopyDepthPass(RenderPassEvent.AfterRendering + 2, m_CopyDepthMaterial);
#endif
            m_DepthPrepass = new DepthOnlyPass(RenderPassEvent.BeforeRenderingPrepasses, RenderQueueRange.opaque, data.opaqueLayerMask);
            // m_SceneDepthPrepass = new DrawObjectsPass(URPProfileId.PreZ, true,
            //     RenderPassEvent.BeforeRenderingOpaques, RenderQueueRange.all, data.transparentLayerMask, m_DefaultStencilState, stencilData.stencilReference, new[] { new ShaderTagId("DepthPrepass") });

            m_DepthNormalPrepass = new DepthNormalOnlyPass(RenderPassEvent.BeforeRenderingPrepasses, RenderQueueRange.opaque, data.opaqueLayerMask);
            m_ColorGradingLutPass = new ColorGradingLutPass(RenderPassEvent.BeforeRenderingPrepasses, data.postProcessData);

            if (this.renderingMode == RenderingMode.Deferred)
            {
                m_DeferredLights = new DeferredLights(m_TileDepthInfoMaterial, m_TileDeferredMaterial, m_StencilDeferredMaterial);
                m_DeferredLights.AccurateGbufferNormals = data.accurateGbufferNormals;
                //m_DeferredLights.TiledDeferredShading = data.tiledDeferredShading;
                m_DeferredLights.TiledDeferredShading = false;
                UniversalRenderPipelineAsset urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;

                m_GBufferPass = new GBufferPass(RenderPassEvent.BeforeRenderingOpaques, RenderQueueRange.opaque, data.opaqueLayerMask, m_DefaultStencilState, stencilData.stencilReference, m_DeferredLights);
                // Forward-only pass only runs if deferred renderer is enabled.
                // It allows specific materials to be rendered in a forward-like pass.
                // We render both gbuffer pass and forward-only pass before the deferred lighting pass so we can minimize copies of depth buffer and
                // benefits from some depth rejection.
                // - If a material can be rendered either forward or deferred, then it should declare a UniversalForward and a UniversalGBuffer pass.
                // - If a material cannot be lit in deferred (unlit, bakedLit, special material such as hair, skin shader), then it should declare UniversalForwardOnly pass
                // - Legacy materials have unamed pass, which is implicitely renamed as SRPDefaultUnlit. In that case, they are considered forward-only too.
                // TO declare a material with unnamed pass and UniversalForward/UniversalForwardOnly pass is an ERROR, as the material will be rendered twice.
                StencilState forwardOnlyStencilState = DeferredLights.OverwriteStencil(m_DefaultStencilState, (int)StencilUsage.MaterialMask);
                int forwardOnlyStencilRef = stencilData.stencilReference | (int)StencilUsage.MaterialUnlit;
                m_RenderOpaqueForwardOnlyPass = new DrawObjectsPass("Render Opaques Forward Only", true, RenderPassEvent.BeforeRenderingOpaques + 1, RenderQueueRange.opaque, data.opaqueLayerMask, forwardOnlyStencilState, forwardOnlyStencilRef);
                m_GBufferCopyDepthPass = new CopyDepthPass(RenderPassEvent.BeforeRenderingOpaques + 2, m_CopyDepthMaterial);
                m_TileDepthRangePass = new TileDepthRangePass(RenderPassEvent.BeforeRenderingOpaques + 3, m_DeferredLights, 0);
                m_TileDepthRangeExtraPass = new TileDepthRangePass(RenderPassEvent.BeforeRenderingOpaques + 4, m_DeferredLights, 1);
                m_DeferredPass = new DeferredPass(RenderPassEvent.BeforeRenderingOpaques + 5, m_DeferredLights);
            }

            // Always create this pass even in deferred because we use it for wireframe rendering in the Editor or offscreen depth texture rendering.
            m_RenderOpaqueForwardPass = new DrawObjectsPass(URPProfileId.DrawOpaqueObjects, true, RenderPassEvent.BeforeRenderingOpaques, RenderQueueRange.opaque, data.opaqueLayerMask, m_DefaultStencilState, stencilData.stencilReference);
            m_DecalPass = new DecalPass(RenderPassEvent.BeforeRenderingTransparents - 10, data.opaqueLayerMask);
            m_GrassRenderPass = new GrassRenderPass(URPProfileId.GrassRenderPass, true, RenderPassEvent.BeforeRenderingTransparents - 9, RenderQueueRange.opaque, data.opaqueLayerMask, m_DefaultStencilState, stencilData.stencilReference);

            // m_GPUInstancingPreZPass = new GPUInstancingPass(URPProfileId.DrawGPUInstancingPreZ, RenderPassEvent.BeforeRenderingOpaques, 0);
            //if(UniversalRenderPipeline.msaaSampleCount > 1)
            {
                //m_GPUInstancingColorPass = new GPUInstancingPass(URPProfileId.DrawGPUInstancingColor, RenderPassEvent.AfterRenderingSkybox + 3, 1);
                //m_HbaoPass = new HBAOPass(RenderPassEvent.AfterRenderingSkybox + 1, data.postProcessData);
                //m_SceneDepthPrepass = new SceneDepthPrepass(URPProfileId.SceneDepthPrepass, RenderPassEvent.BeforeRenderingOpaques, new[] { new ShaderTagId("DepthPrepass") });
            }
            //else
            {
                m_GPUInstancingColorPass = new GPUInstancingPass(URPProfileId.DrawGPUInstancingColor, RenderPassEvent.BeforeRenderingTransparents - 9, 1);
                m_HbaoPass = new HBAOPass(RenderPassEvent.AfterRenderingSkybox + 2, data.postProcessData);
                m_SceneDepthPrepass = new SceneDepthPrepass(URPProfileId.SceneDepthPrepass, RenderPassEvent.BeforeRenderingOpaques, new[] { new ShaderTagId("DepthPrepass") });
                m_TransparentSceneDepthPrepass = new SceneDepthPrepass(URPProfileId.SceneDepthPrepass,
                    RenderPassEvent.AfterRenderingOpaques, new[] { new ShaderTagId("TransparentDepthPrepass") }, true);
            }

            // m_FaceShadowCasterPass = new DrawObjectsPass(URPProfileId.DrawFaceShadowCasters, true,
            //     RenderPassEvent.BeforeRenderingOpaques, RenderQueueRange.opaque, data.opaqueLayerMask,
            //     m_DefaultStencilState, stencilData.stencilReference,
            //     new []{new ShaderTagId("FaceShadowCaster")});
            // m_FaceShadowPass =  new DrawObjectsPass(URPProfileId.DrawFaceShadows, true,
            //     RenderPassEvent.BeforeRenderingOpaques, RenderQueueRange.opaque, data.opaqueLayerMask,
            //     m_DefaultStencilState, stencilData.stencilReference,
            //     new []{new ShaderTagId("FaceShadow")});

            // m_RenderScreenSpaceRimPass = new DrawObjectsPass(URPProfileId.DrawScreenSpaceRimPass, true,
            //     RenderPassEvent.AfterRenderingSkybox, RenderQueueRange.opaque, data.opaqueLayerMask,
            //     m_DefaultStencilState, stencilData.stencilReference,
            //     new []{new ShaderTagId("ScreenSpaceRim")});
            // m_RenderOutlinePass = new DrawObjectsPass(URPProfileId.DrawOutlines, true,
            //     RenderPassEvent.BeforeRenderingTransparents, RenderQueueRange.opaque, data.opaqueLayerMask,
            //     m_DefaultStencilState, stencilData.stencilReference,
            //     new []{new ShaderTagId("Outline")});
            // m_PlanarShadowPass = new DrawObjectsPass(URPProfileId.DrawPlanarShadows, true,
            //     RenderPassEvent.BeforeRenderingTransparents, RenderQueueRange.opaque, data.opaqueLayerMask,
            //     m_DefaultStencilState, stencilData.stencilReference,
            //     new []{new ShaderTagId("PlanarShadow")});


            //ShaderTagId outlineID = new ShaderTagId();

            //if (Shader.IsKeywordEnabled("_TESSELLATION_ON"))
            //    outlineTagID = new ShaderTagId("TessOutline");
            //else
            //    outlineTagID = new ShaderTagId("Outline");

            //m_roleEffectsBeforeDepthPass = new DrawRoleEffectsPass(URPProfileId.RoleEffects, true,
            //    RenderPassEvent.BeforeRenderingOpaques, RenderQueueRange.opaque, data.opaqueLayerMask,
            //    m_DefaultStencilState, stencilData.stencilReference,
            //    new[] { new ShaderTagId("FaceShadowCaster"), new ShaderTagId("FaceShadow"), outlineTagID });

            m_roleEffectsBeforeDepthPass = new DrawRoleEffectsPass(URPProfileId.RoleEffects, true,
                RenderPassEvent.BeforeRenderingOpaques, RenderQueueRange.opaque, data.opaqueLayerMask,
                m_DefaultStencilState, stencilData.stencilReference,
                new[] { new ShaderTagId("FaceShadowCaster"), new ShaderTagId("FaceShadow"), new ShaderTagId("Outline") });

            m_roleEffectsAfterDepthPass = new DrawRoleEffectsPass(URPProfileId.RoleEffects, true,
                RenderPassEvent.BeforeRenderingTransparents + 1 , RenderQueueRange.opaque, data.opaqueLayerMask,
                m_DefaultStencilState, stencilData.stencilReference,
                new[] { new ShaderTagId("ScreenSpaceRim"), new ShaderTagId("PlanarShadow") });

            //m_ForwardTransparentPass = new DrawObjectsPass("Forawrd Transparent", new [] { new ShaderTagId("ForwardTransparent")}, true,
            //RenderPassEvent.AfterRenderingSkybox, RenderQueueRange.all, data.transparentLayerMask, m_DefaultStencilState, stencilData.stencilReference);

            m_CopyDepthPass = new CopyDepthPass(RenderPassEvent.AfterRenderingSkybox, m_CopyDepthMaterial);
            m_DrawSkyboxPass = new DrawSkyboxPass(RenderPassEvent.BeforeRenderingSkybox);
            m_CopyColorPass = new CopyColorPass(RenderPassEvent.AfterRenderingSkybox + 1, m_SamplingMaterial, m_BlitMaterial);
#if ADAPTIVE_PERFORMANCE_2_1_0_OR_NEWER
            if (!UniversalRenderPipeline.asset.useAdaptivePerformance || AdaptivePerformance.AdaptivePerformanceRenderSettings.SkipTransparentObjects == false)
#endif
            {
                m_TransparentSettingsPass = new TransparentSettingsPass(RenderPassEvent.BeforeRenderingTransparents, data.shadowTransparentReceive);
                m_RenderTransparentForwardPass = new DrawObjectsPass(URPProfileId.DrawTransparentObjects, false, RenderPassEvent.BeforeRenderingTransparents, RenderQueueRange.transparent, -1, m_DefaultStencilState, stencilData.stencilReference);
            }
            m_OnRenderObjectCallbackPass = new InvokeOnRenderObjectCallbackPass(RenderPassEvent.BeforeRenderingPostProcessing);
            m_PostProcessPass = new PostProcessPass(RenderPassEvent.BeforeRenderingPostProcessing, data.postProcessData, m_BlitMaterial);
            m_FinalPostProcessPass = new PostProcessPass(RenderPassEvent.AfterRendering + 1, data.postProcessData, m_BlitMaterial);
            m_CapturePass = new CapturePass(RenderPassEvent.AfterRendering);
            m_FinalBlitPass = new FinalBlitPass(RenderPassEvent.AfterRendering + 1, m_BlitMaterial);

            // if (StaticGaussianBlurContext.Context != null)
            {
                m_staticGaussianBlurPass = new StaticGaussianBlurPass(RenderPassEvent.BeforeRenderingTransparents - 2);
            }

            /* Add By:Takeshi; Fix SceneView Gamma*/
#if UNITY_EDITOR
            m_BlitBeforeUIInSceneViewPass = new BlitBeforeUIInSceneViewPass(RenderPassEvent.BeforeRenderingTransparents + 10, m_BlitMaterial/*,"_BlitBeforeUIInSceneView",ShaderKeywordStrings.LinearToSRGBConversion*/);
            m_FixGammaBlitInSceneViewPass = new FixGammaBlitInSceneViewPass(RenderPassEvent.AfterRenderingTransparents, m_BlitMaterial/*,"FixGammaBlit_InSceneView",ShaderKeywordStrings.SRGBToLinearConversion*/);
            m_BlitWhenPostProcessOffSceneViwePass = new BlitFixGammaPass(RenderPassEvent.AfterRendering, m_BlitMaterial);
#endif
            m_BlitFixGammaPass = new BlitFixGammaPass(RenderPassEvent.BeforeRenderingTransparents, m_BlitMaterial);
            //m_BlitWhenFXAAOffPass = new BlitFixGammaPass(RenderPassEvent.AfterRenderingPostProcessing, m_BlitMaterial, "Blit ( When FXAA Off ) Pass");
            /* End Add */
            m_distortionPass = new URPDistortionPass(RenderPassEvent.AfterRenderingTransparents, new[] { "Distortion" }, RenderQueueType.Transparent, 0);
            //m_distortionPass.Setup();
#if UNITY_EDITOR
            m_SceneViewDepthCopyPass = new SceneViewDepthCopyPass(RenderPassEvent.AfterRendering + 9, m_CopyDepthMaterial);
            m_OverdrawOpaquePass = new OverdrawPass(URPProfileId.OverdrawOpaque, true,
                RenderPassEvent.BeforeRenderingOpaques, RenderQueueRange.opaque, data.opaqueLayerMask,
                m_DefaultStencilState, stencilData.stencilReference,
                new[] { new ShaderTagId("OverdrawForwardBase") });
            m_OverdrawTransparentPass = new OverdrawPass(URPProfileId.OverdrawTransparent, false,
                RenderPassEvent.BeforeRenderingTransparents, RenderQueueRange.transparent, data.transparentLayerMask,
                m_DefaultStencilState, stencilData.stencilReference,
                new[] { new ShaderTagId("OverdrawForwardBaseT") });
#endif
            m_TemporaPass = new Tempora();
            // RenderTexture format depends on camera and pipeline (HDR, non HDR, etc)
            // Samples (MSAA) depend on camera and pipeline
            m_CameraColorAttachment.Init("_CameraColorTexture");
            m_CameraDepthAttachment.Init("_CameraDepthAttachment");
            m_DepthTexture.Init("_CameraDepthTexture");
            m_NormalsTexture.Init("_CameraNormalsTexture");
            if (this.renderingMode == RenderingMode.Deferred)
            {
                m_GBufferHandles = new RenderTargetHandle[(int)DeferredLights.GBufferHandles.Count];
                m_GBufferHandles[(int)DeferredLights.GBufferHandles.DepthAsColor].Init("_GBufferDepthAsColor");
                m_GBufferHandles[(int)DeferredLights.GBufferHandles.Albedo].Init("_GBuffer0");
                m_GBufferHandles[(int)DeferredLights.GBufferHandles.SpecularMetallic].Init("_GBuffer1");
                m_GBufferHandles[(int)DeferredLights.GBufferHandles.NormalSmoothness].Init("_GBuffer2");
                m_GBufferHandles[(int)DeferredLights.GBufferHandles.Lighting] = new RenderTargetHandle();
                m_GBufferHandles[(int)DeferredLights.GBufferHandles.ShadowMask].Init("_GBuffer4");
            }
            m_OpaqueColor.Init("_CameraOpaqueTexture");
            m_AfterPostProcessColor.Init("_AfterPostProcessTexture");
            m_ColorGradingLut.Init("_InternalGradingLut");
            m_DepthInfoTexture.Init("_DepthInfoTexture");
            m_TileDepthInfoTexture.Init("_TileDepthInfoTexture");

            supportedRenderingFeatures = new RenderingFeatures()
            {
                cameraStacking = true,
            };

            if (this.renderingMode == RenderingMode.Deferred)
            {
                unsupportedGraphicsDeviceTypes = new GraphicsDeviceType[] {
                    GraphicsDeviceType.OpenGLCore,
                    GraphicsDeviceType.OpenGLES2,
                    GraphicsDeviceType.OpenGLES3
                };
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            m_TemporaPass.Cleanup();
            // always dispose unmanaged resources
            m_PostProcessPass.Cleanup();
            m_FinalPostProcessPass.Cleanup();
            m_ColorGradingLutPass.Cleanup();

            CoreUtils.Destroy(m_BlitMaterial);
            CoreUtils.Destroy(m_CopyDepthMaterial);
            CoreUtils.Destroy(m_SamplingMaterial);
            CoreUtils.Destroy(m_ScreenspaceShadowsMaterial);
            CoreUtils.Destroy(m_TileDepthInfoMaterial);
            CoreUtils.Destroy(m_TileDeferredMaterial);
            CoreUtils.Destroy(m_StencilDeferredMaterial);
        }

        /// <inheritdoc />
        public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            ref CameraData cameraData = ref renderingData.cameraData;
            bool isUICamera = cameraData.isUiCamera;
            if (isUICamera)
            {
                SetupCameraUI(context, ref renderingData, ref cameraData);
            }
            else
            {
                SetupCameraOther(context, ref renderingData, ref cameraData);
            }
            //System.Text.StringBuilder sb = new System.Text.StringBuilder();
            //foreach (ScriptableRenderPass p in activeRenderPassQueue)
            //{
            //    if (p.GetType() == typeof(UnityEngine.Rendering.Universal.Internal.DrawObjectsPass))
            //    {
            //        UnityEngine.Rendering.Universal.Internal.DrawObjectsPass f = p as UnityEngine.Rendering.Universal.Internal.DrawObjectsPass;
            //        sb.AppendLine(p.ToString() + ":" + f.m_ProfilerTag);
            //    }
            //    else if (p.GetType() == typeof(UnityEngine.Experimental.Rendering.Universal.RenderObjectsPass))
            //    {
            //        UnityEngine.Experimental.Rendering.Universal.RenderObjectsPass f = p as UnityEngine.Experimental.Rendering.Universal.RenderObjectsPass;
            //        sb.AppendLine(p.ToString() + ":" + f.m_ProfilerTag);
            //    }
            //    else
            //    {
            //        sb.AppendLine(p.ToString());
            //    }
            //}
            //Debug.LogError(isUICamera + "," + cameraData.isMainCamera + ":" + activeRenderPassQueue.Count + "\t\r" + sb.ToString());
        }

        void SetupCameraUI(ScriptableRenderContext context, ref RenderingData renderingData, ref CameraData cameraData)
        {

#if UNITY_EDITOR
            bool drawOverdraw = (OverdrawState.gameOverdrawViewMode &&
                                 renderingData.cameraData.camera.cameraType.Equals(CameraType.Game))
                                || (OverdrawState.sceneOverdrawViewMode &&
                                    renderingData.cameraData.camera.cameraType.Equals(CameraType.SceneView));
            bool drawTransparentOverdraw = drawOverdraw && OverdrawState.transparentOverdraw;
#else

            bool drawTransparentOverdraw = false;
#endif
#if ADAPTIVE_PERFORMANCE_2_1_0_OR_NEWER
            bool needTransparencyPass = !UniversalRenderPipeline.asset.useAdaptivePerformance || !AdaptivePerformance.AdaptivePerformanceRenderSettings.SkipTransparentObjects;
#endif

            RenderTextureDescriptor cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;

            // Special path for depth only offscreen cameras. Only write opaques + transparents.
            bool isOffscreenDepthTexture = cameraData.targetTexture != null && cameraData.targetTexture.format == RenderTextureFormat.Depth;
            if (isOffscreenDepthTexture)
            {
                ConfigureCameraTarget(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CameraTarget);
                if (!drawTransparentOverdraw) EnqueuePass(m_RenderTransparentForwardPass);
                return;
            }

            // Assign the camera color target early in case it is needed during AddRenderPasses.
            bool isPreviewCamera = cameraData.isPreviewCamera;
            var createColorTexture = rendererFeatures.Count != 0 && !isPreviewCamera;
            if (createColorTexture)
            {
                m_ActiveCameraColorAttachment = m_CameraColorAttachment;
                var activeColorRenderTargetId = m_ActiveCameraColorAttachment.Identifier();
#if ENABLE_VR && ENABLE_XR_MODULE
                if (cameraData.xr.enabled) activeColorRenderTargetId = new RenderTargetIdentifier(activeColorRenderTargetId, 0, CubemapFace.Unknown, -1);
#endif
                ConfigureCameraColorTarget(activeColorRenderTargetId);
            }

            RenderPassInputSummary renderPassInputs = GetRenderPassInputs(ref renderingData);

            // Should apply post-processing after rendering this camera?
            bool applyPostProcessing = cameraData.postProcessEnabled;

            // There's at least a camera in the camera stack that applies post-processing
            bool anyPostProcessing = renderingData.postProcessingEnabled;

            // TODO: We could cache and generate the LUT before rendering the stack
            bool isSceneViewCamera = cameraData.isSceneViewCamera;
            bool requiresDepthTexture = cameraData.requiresDepthTexture || renderPassInputs.requiresDepthTexture || this.actualRenderingMode == RenderingMode.Deferred;

            bool transparentsNeedSettingsPass = m_TransparentSettingsPass.Setup(ref renderingData);

            // Depth prepass is generated in the following cases:
            // - If game or offscreen camera requires it we check if we can copy the depth from the rendering opaques pass and use that instead.
            // - Scene or preview cameras always require a depth texture. We do a depth pre-pass to simplify it and it shouldn't matter much for editor.
            // - Render passes require it
            bool requiresDepthPrepass = requiresDepthTexture && !CanCopyDepth(ref renderingData.cameraData);
            //requiresDepthPrepass |= isSceneViewCamera;
            requiresDepthPrepass |= isPreviewCamera;
            requiresDepthPrepass |= renderPassInputs.requiresDepthPrepass;
            requiresDepthPrepass |= renderPassInputs.requiresNormalsTexture;

            // The copying of depth should normally happen after rendering opaques.
            // But if we only require it for post processing or the scene camera then we do it after rendering transparent objects
            //m_CopyDepthPass.renderPassEvent = (!requiresDepthTexture && (applyPostProcessing || isSceneViewCamera)) ? RenderPassEvent.AfterRenderingTransparents : RenderPassEvent.AfterRenderingOpaques;
            createColorTexture |= RequiresIntermediateColorTexture(ref cameraData);
            createColorTexture |= renderPassInputs.requiresColorTexture;
            createColorTexture &= !isPreviewCamera;

            // If camera requires depth and there's no depth pre-pass we create a depth texture that can be read later by effect requiring it.
            // When deferred renderer is enabled, we must always create a depth texture and CANNOT use BuiltinRenderTextureType.CameraTarget. This is to get
            // around a bug where during gbuffer pass (MRT pass), the camera depth attachment is correctly bound, but during
            // deferred pass ("camera color" + "camera depth"), the implicit depth surface of "camera color" is used instead of "camera depth",
            // because BuiltinRenderTextureType.CameraTarget for depth means there is no explicit depth attachment...
            bool createDepthTexture = cameraData.requiresDepthTexture && !requiresDepthPrepass;
            createDepthTexture |= (cameraData.renderType == CameraRenderType.Base && !cameraData.resolveFinalTarget);
            // Deferred renderer always need to access depth buffer.
            createDepthTexture |= this.actualRenderingMode == RenderingMode.Deferred;

#if UNITY_ANDROID || UNITY_WEBGL
            if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Vulkan)
            {
                // GLES can not use render texture's depth
                // buffer with the color buffer of the backbuffer
                // in such case we create a color texture for it too.
                createColorTexture |= createDepthTexture;
            }
#endif
            // Configure all settings require to start a new camera stack (base camera only)
            if (cameraData.renderType == CameraRenderType.Base)
            {
                RenderTargetHandle cameraTargetHandle = RenderTargetHandle.GetCameraTarget(cameraData.xr);

                m_ActiveCameraColorAttachment = (createColorTexture) ? m_CameraColorAttachment : cameraTargetHandle;
                m_ActiveCameraDepthAttachment = (createDepthTexture) ? m_CameraDepthAttachment : cameraTargetHandle;

                bool intermediateRenderTexture = createColorTexture || createDepthTexture;

                // Doesn't create texture for Overlay cameras as they are already overlaying on top of created textures.
                bool useTAA = UniversalRenderPipeline.asset.Antialiasing == AntialiasingMode.TAA && UnityEngine.Rendering.Universal.GameQualitySetting.ToneMapping;
                if (intermediateRenderTexture)
                    CreateCameraRenderTarget(context, ref cameraTargetDescriptor, createColorTexture, createDepthTexture, useTAA);
            }
            else
            {
                m_ActiveCameraColorAttachment = m_CameraColorAttachment;
                m_ActiveCameraDepthAttachment = m_CameraDepthAttachment;
            }

            // Assign camera targets (color and depth)
            {
                var activeColorRenderTargetId = m_ActiveCameraColorAttachment.Identifier();
                var activeDepthRenderTargetId = m_ActiveCameraDepthAttachment.Identifier();

                ConfigureCameraTarget(activeColorRenderTargetId, activeDepthRenderTargetId);
            }


#if ADAPTIVE_PERFORMANCE_2_1_0_OR_NEWER
            if (needTransparencyPass)
#endif
            {
                //if (transparentsNeedSettingsPass && !drawTransparentOverdraw)
                //{
                //    EnqueuePass(m_TransparentSettingsPass);
                //}

                if (!drawTransparentOverdraw) EnqueuePass(m_RenderTransparentForwardPass);
            }

            bool lastCameraInTheStack = cameraData.resolveFinalTarget;
            if (StaticGaussianBlurContext.Context != null && StaticGaussianBlurContext.Context.IsRenderBlurRT)
            {
                m_staticGaussianBlurPass.Setup(m_ActiveCameraColorAttachment);
                EnqueuePass(m_staticGaussianBlurPass);
            }

            // UI不需要扭曲
            // if (EngineContext.none3DCamera)
            // {
                // EnqueuePass(m_distortionPass);
            // }
            if (lastCameraInTheStack)
            {
                // if we applied post-processing for this camera it means current active texture is m_AfterPostProcessColor
                var sourceForFinalPass = (applyPostProcessing) ? m_AfterPostProcessColor : m_ActiveCameraColorAttachment;

                m_FinalBlitPass.Setup(cameraTargetDescriptor, sourceForFinalPass);
                EnqueuePass(m_FinalBlitPass);
            }
        }
        void SetupCameraOther(ScriptableRenderContext context, ref RenderingData renderingData, ref CameraData cameraData)
        {
            Camera camera = cameraData.camera;
            bool isMainCamera = cameraData.isMainCamera;
            bool isUICamera = cameraData.isUiCamera;
            // bool isUISceneCamera = cameraData.isUiSceneCamera;

            bool useTAA = UniversalRenderPipeline.asset.Antialiasing == AntialiasingMode.TAA && isMainCamera && UnityEngine.Rendering.Universal.GameQualitySetting.ToneMapping;

            bool isUISceneCamera = camera.gameObject.layer == LayerMaskName.UIScene;
            bool volumeChange = true;
#if UNITY_EDITOR
            if (isMainCamera && UnityEngine.Rendering.Universal.UniversalRenderPipeline.asset.AADebug)
            {
                volumeChange = !UnityEditor.EditorApplication.isPlaying || VolumeManager.instance.ChangeColorGradingLUT() || firstSetup;
                firstSetup = false;
            }
#else
            if (isMainCamera)
            {
                volumeChange = VolumeManager.instance.ChangeColorGradingLUT() || firstSetup;
                firstSetup = false;
            }
#endif

#if UNITY_EDITOR
            bool drawOverdraw = (OverdrawState.gameOverdrawViewMode &&
                                 renderingData.cameraData.camera.cameraType.Equals(CameraType.Game))
                                || (OverdrawState.sceneOverdrawViewMode &&
                                    renderingData.cameraData.camera.cameraType.Equals(CameraType.SceneView));
            bool drawOpaqueOverdraw = drawOverdraw && OverdrawState.opaqueOverdraw;
            bool drawTransparentOverdraw = drawOverdraw && OverdrawState.transparentOverdraw;
#else
            bool drawOpaqueOverdraw = false; 
            bool drawTransparentOverdraw = false;
#endif
#if ADAPTIVE_PERFORMANCE_2_1_0_OR_NEWER
            bool needTransparencyPass = !UniversalRenderPipeline.asset.useAdaptivePerformance || !AdaptivePerformance.AdaptivePerformanceRenderSettings.SkipTransparentObjects;
#endif
            RenderTextureDescriptor cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;

            // Special path for depth only offscreen cameras. Only write opaques + transparents.
            bool isOffscreenDepthTexture = cameraData.targetTexture != null && cameraData.targetTexture.format == RenderTextureFormat.Depth;
            if (isOffscreenDepthTexture)
            {
                ConfigureCameraTarget(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CameraTarget);
                AddRenderPasses(ref renderingData);

                // if (!drawOpaqueOverdraw) EnqueuePass(m_GPUInstancingPreZPass);
                bool flag = false;
                //if (Shader.GetGlobalInt("_ZTestHSR") == 4)
                //flag = true;
                if (!drawOpaqueOverdraw && !flag) EnqueuePass(m_SceneDepthPrepass);
                if (!drawOpaqueOverdraw) EnqueuePass(m_RenderOpaqueForwardPass);


                if (GameQualitySetting.DrawGrass)
                {
                    GPUInstancingManager.Instance.Update();
                    if (!drawOpaqueOverdraw) EnqueuePass(m_GPUInstancingColorPass);
                    if (!drawOpaqueOverdraw) EnqueuePass(m_GrassRenderPass);
                }

                // Change By Takeshi
                // if (UniversalRenderPipeline.planarShadowEnabled)
                //     if (!drawTransparentOverdraw) EnqueuePass(m_PlanarShadowPass);
                // if (!drawOpaqueOverdraw) EnqueuePass(m_RenderOutlinePass);

                //if (!drawOpaqueOverdraw) EnqueuePass(m_PreZPass);
                //if (!drawTransparentOverdraw) EnqueuePass(m_ForwardTransparentPass);

                // TODO: Do we need to inject transparents and skybox when rendering depth only camera? They don't write to depth.
                if (!drawTransparentOverdraw && !drawOpaqueOverdraw) EnqueuePass(m_DrawSkyboxPass);
#if ADAPTIVE_PERFORMANCE_2_1_0_OR_NEWER
                if (!needTransparencyPass)
                    return;
#endif
                if (!drawTransparentOverdraw) EnqueuePass(m_RenderTransparentForwardPass);
                return;
            }

            if (m_DeferredLights != null)
                m_DeferredLights.ResolveMixedLightingMode(ref renderingData);

            // Assign the camera color target early in case it is needed during AddRenderPasses.
            bool isPreviewCamera = cameraData.isPreviewCamera;
            var createColorTexture = rendererFeatures.Count != 0 && !isPreviewCamera;
            if (createColorTexture)
            {
                m_ActiveCameraColorAttachment = m_CameraColorAttachment;
                var activeColorRenderTargetId = m_ActiveCameraColorAttachment.Identifier();
#if ENABLE_VR && ENABLE_XR_MODULE
                if (cameraData.xr.enabled) activeColorRenderTargetId = new RenderTargetIdentifier(activeColorRenderTargetId, 0, CubemapFace.Unknown, -1);
#endif
                ConfigureCameraColorTarget(activeColorRenderTargetId);
            }

            // Add render passes and gather the input requirements
            isCameraColorTargetValid = true;
            AddRenderPasses(ref renderingData);
            isCameraColorTargetValid = false;
            RenderPassInputSummary renderPassInputs = GetRenderPassInputs(ref renderingData);

            // Should apply post-processing after rendering this camera?
            bool applyPostProcessing = cameraData.postProcessEnabled && UnityEngine.Rendering.Universal.GameQualitySetting.ToneMapping;
            UnityEngine.Rendering.Universal.GameQualitySetting.SetToneMappingEnable();

            // There's at least a camera in the camera stack that applies post-processing
            bool anyPostProcessing = renderingData.postProcessingEnabled;

            // TODO: We could cache and generate the LUT before rendering the stack
            bool generateColorGradingLUT = cameraData.postProcessEnabled;
            bool isSceneViewCamera = cameraData.isSceneViewCamera;
            bool requiresDepthTexture = cameraData.requiresDepthTexture || renderPassInputs.requiresDepthTexture || this.actualRenderingMode == RenderingMode.Deferred;

            bool mainLightShadows = m_MainLightShadowCasterPass.Setup(ref renderingData);

            bool additionalLightShadows = m_AdditionalLightsShadowCasterPass.Setup(ref renderingData);
            bool transparentsNeedSettingsPass = m_TransparentSettingsPass.Setup(ref renderingData);

            // Depth prepass is generated in the following cases:
            // - If game or offscreen camera requires it we check if we can copy the depth from the rendering opaques pass and use that instead.
            // - Scene or preview cameras always require a depth texture. We do a depth pre-pass to simplify it and it shouldn't matter much for editor.
            // - Render passes require it
            bool requiresDepthPrepass = requiresDepthTexture && !CanCopyDepth(ref renderingData.cameraData);
            //requiresDepthPrepass |= isSceneViewCamera;
            requiresDepthPrepass |= isPreviewCamera;
            requiresDepthPrepass |= renderPassInputs.requiresDepthPrepass;
            requiresDepthPrepass |= renderPassInputs.requiresNormalsTexture;

            // The copying of depth should normally happen after rendering opaques.
            // But if we only require it for post processing or the scene camera then we do it after rendering transparent objects
            //m_CopyDepthPass.renderPassEvent = (!requiresDepthTexture && (applyPostProcessing || isSceneViewCamera)) ? RenderPassEvent.AfterRenderingTransparents : RenderPassEvent.AfterRenderingOpaques;
            createColorTexture |= RequiresIntermediateColorTexture(ref cameraData);
            createColorTexture |= renderPassInputs.requiresColorTexture;
            createColorTexture &= !isPreviewCamera;

            // If camera requires depth and there's no depth pre-pass we create a depth texture that can be read later by effect requiring it.
            // When deferred renderer is enabled, we must always create a depth texture and CANNOT use BuiltinRenderTextureType.CameraTarget. This is to get
            // around a bug where during gbuffer pass (MRT pass), the camera depth attachment is correctly bound, but during
            // deferred pass ("camera color" + "camera depth"), the implicit depth surface of "camera color" is used instead of "camera depth",
            // because BuiltinRenderTextureType.CameraTarget for depth means there is no explicit depth attachment...
            bool createDepthTexture = cameraData.requiresDepthTexture && !requiresDepthPrepass;
            createDepthTexture |= (cameraData.renderType == CameraRenderType.Base && !cameraData.resolveFinalTarget);
            // Deferred renderer always need to access depth buffer.
            createDepthTexture |= this.actualRenderingMode == RenderingMode.Deferred;
#if ENABLE_VR && ENABLE_XR_MODULE
            if (cameraData.xr.enabled)
            {
                // URP can't handle msaa/size mismatch between depth RT and color RT(for now we create intermediate textures to ensure they match)
                createDepthTexture |= createColorTexture;
                createColorTexture = createDepthTexture;
            }
#endif

#if UNITY_ANDROID || UNITY_WEBGL
            if (SystemInfo.graphicsDeviceType != GraphicsDeviceType.Vulkan)
            {
                // GLES can not use render texture's depth
                // buffer with the color buffer of the backbuffer
                // in such case we create a color texture for it too.
                createColorTexture |= createDepthTexture;
            }
#endif
            // Configure all settings require to start a new camera stack (base camera only)
            if (cameraData.renderType == CameraRenderType.Base)
            {
                RenderTargetHandle cameraTargetHandle = RenderTargetHandle.GetCameraTarget(cameraData.xr);

                m_ActiveCameraColorAttachment = (createColorTexture) ? m_CameraColorAttachment : cameraTargetHandle;
                m_ActiveCameraDepthAttachment = (createDepthTexture) ? m_CameraDepthAttachment : cameraTargetHandle;

                bool intermediateRenderTexture = createColorTexture || createDepthTexture;

                // Doesn't create texture for Overlay cameras as they are already overlaying on top of created textures.
                if (intermediateRenderTexture)
                    CreateCameraRenderTarget(context, ref cameraTargetDescriptor, createColorTexture, createDepthTexture, useTAA);
            }
            else
            {
                m_ActiveCameraColorAttachment = m_CameraColorAttachment;
                m_ActiveCameraDepthAttachment = m_CameraDepthAttachment;
            }

            // Assign camera targets (color and depth)
            {
                var activeColorRenderTargetId = m_ActiveCameraColorAttachment.Identifier();
                var activeDepthRenderTargetId = m_ActiveCameraDepthAttachment.Identifier();

#if ENABLE_VR && ENABLE_XR_MODULE
                if (cameraData.xr.enabled)
                {
                    activeColorRenderTargetId = new RenderTargetIdentifier(activeColorRenderTargetId, 0, CubemapFace.Unknown, -1);
                    activeDepthRenderTargetId = new RenderTargetIdentifier(activeDepthRenderTargetId, 0, CubemapFace.Unknown, -1);
                }
#endif

                ConfigureCameraTarget(activeColorRenderTargetId, activeDepthRenderTargetId);
            }

            bool hasPassesAfterPostProcessing = activeRenderPassQueue.Find(x => x.renderPassEvent == RenderPassEvent.AfterRendering) != null;


            if (mainLightShadows)
                if (!drawOpaqueOverdraw) EnqueuePass(m_MainLightShadowCasterPass);

            if (additionalLightShadows)
                if (!drawOpaqueOverdraw) EnqueuePass(m_AdditionalLightsShadowCasterPass);

            if (requiresDepthPrepass)
            {
                if (UnityEngine.Rendering.Universal.GameQualitySetting.Depth)
                {
                    if (renderPassInputs.requiresNormalsTexture)
                    {
                        m_DepthNormalPrepass.Setup(cameraTargetDescriptor, m_DepthTexture, m_NormalsTexture);
                        if (!drawOpaqueOverdraw) EnqueuePass(m_DepthNormalPrepass);
                    }
                    else
                    {
                        m_DepthPrepass.Setup(cameraTargetDescriptor, m_DepthTexture);
                        if (!drawOpaqueOverdraw) EnqueuePass(m_DepthPrepass);
                    }
                }
            }

            if (volumeChange && generateColorGradingLUT && GameQualitySetting.ToneMapping)
            {
                VolumeManager.instance.ChangeColorGradingLUT(true);
                m_ColorGradingLutPass.Setup(m_ColorGradingLut);
                EnqueuePass(m_ColorGradingLutPass);
            }

#if ENABLE_VR && ENABLE_XR_MODULE
            if (cameraData.xr.hasValidOcclusionMesh)
                EnqueuePass(m_XROcclusionMeshPass);
#endif

            if (this.actualRenderingMode == RenderingMode.Deferred)
                EnqueueDeferred(ref renderingData, requiresDepthPrepass, mainLightShadows, additionalLightShadows);
            else
            {
                // if (!drawOpaqueOverdraw) EnqueuePass(m_GPUInstancingPreZPass);
                bool flag = false;
                //if (Shader.GetGlobalInt("_ZTestHSR") == 4)
                //flag = true;
                if (!drawOpaqueOverdraw && !flag) EnqueuePass(m_SceneDepthPrepass);
                if (!drawOpaqueOverdraw) EnqueuePass(m_TransparentSceneDepthPrepass);
                if (!drawOpaqueOverdraw) EnqueuePass(m_RenderOpaqueForwardPass);

                if (GameQualitySetting.DrawGrass)
                {
                    GPUInstancingManager.Instance.Update();
                    if (!drawOpaqueOverdraw) EnqueuePass(m_GPUInstancingColorPass);
                    if (!drawOpaqueOverdraw) EnqueuePass(m_DecalPass);
                    if (!drawOpaqueOverdraw) EnqueuePass(m_GrassRenderPass);
                }

                // Change By Takeshi
                // if (UniversalRenderPipeline.planarShadowEnabled)
                //     if (!drawTransparentOverdraw) EnqueuePass(m_PlanarShadowPass);
                // if (!drawOpaqueOverdraw) EnqueuePass(m_RenderOutlinePass);

                //if (!drawTransparentOverdraw) EnqueuePass(m_ForwardTransparentPass);
#if UNITY_EDITOR
                if (drawOpaqueOverdraw) EnqueuePass(m_OverdrawOpaquePass);
                if (drawTransparentOverdraw) EnqueuePass(m_OverdrawTransparentPass);
#endif
            }

            UnityEngine.Skybox cameraSkybox;
            cameraData.camera.TryGetComponent<UnityEngine.Skybox>(out cameraSkybox);
            bool isOverlayCamera = cameraData.renderType == CameraRenderType.Overlay;
            if (camera.clearFlags == CameraClearFlags.Skybox
                && (RenderSettings.skybox != null || (cameraSkybox && cameraSkybox.material))
                && !isOverlayCamera
#if UNITY_EDITOR
                && (CaptureWindow.captureRenderCounter < 0 || CaptureWindow.skybox.Value)
#endif
                )
                EnqueuePass(m_DrawSkyboxPass);

            // If a depth texture was created we necessarily need to copy it, otherwise we could have render it to a renderbuffer.
            // If deferred rendering path was selected, it has already made a copy.
            bool requiresDepthCopyPass = !requiresDepthPrepass
                                         && renderingData.cameraData.requiresDepthTexture
                                         && createDepthTexture
                                         && this.actualRenderingMode != RenderingMode.Deferred
                                         && UniversalRenderPipeline.asset.RequireCopyDepth;

            if (!isUICamera && requiresDepthCopyPass)
            {
                m_CopyDepthPass.Setup(m_ActiveCameraDepthAttachment, m_DepthTexture);
                EnqueuePass(m_CopyDepthPass);
            }

            // Change by: Takeshi
            // if (UniversalRenderPipeline.asset && UniversalRenderPipeline.asset.roleFaceShadow && !drawOpaqueOverdraw)
            // {
            //     EnqueuePass(m_FaceShadowCasterPass);
            //     EnqueuePass(m_FaceShadowPass);
            // }
            //
            // if (UniversalRenderPipeline.asset && UniversalRenderPipeline.asset.roleScreenSpaceRim && !drawOpaqueOverdraw)
            // {
            //     EnqueuePass(m_RenderScreenSpaceRimPass);
            // }


            // For Base Cameras: Set the depth texture to the far Z if we do not have a depth prepass or copy depth
            if (cameraData.renderType == CameraRenderType.Base && !requiresDepthPrepass && !requiresDepthCopyPass)
            {
                Shader.SetGlobalTexture(m_DepthTexture.id, SystemInfo.usesReversedZBuffer ? Texture2D.blackTexture : Texture2D.whiteTexture);
            }

            if (renderingData.cameraData.requiresOpaqueTexture || renderPassInputs.requiresColorTexture)
            {
                // TODO: Downsampling method should be store in the renderer instead of in the asset.
                // We need to migrate this data to renderer. For now, we query the method in the active asset.
                Downsampling downsamplingMethod = UniversalRenderPipeline.asset.opaqueDownsampling;
                m_CopyColorPass.Setup(m_ActiveCameraColorAttachment.Identifier(), m_OpaqueColor, downsamplingMethod);
                EnqueuePass(m_CopyColorPass);
            }

            if (UniversalRenderPipeline.asset.HBAOSetting.Enable)
            {
                m_HbaoPass.Setup(m_ActiveCameraColorAttachment, m_ActiveCameraDepthAttachment, m_OpaqueColor.Identifier(), UniversalRenderPipeline.asset.HBAOSetting);
                if (!drawOpaqueOverdraw) EnqueuePass(m_HbaoPass);
            }

#if ADAPTIVE_PERFORMANCE_2_1_0_OR_NEWER
            if (needTransparencyPass)
#endif
            {
                if (transparentsNeedSettingsPass && !drawTransparentOverdraw)
                {
                    EnqueuePass(m_TransparentSettingsPass);
                }

                if (!drawTransparentOverdraw) EnqueuePass(m_RenderTransparentForwardPass);
            }
            EnqueuePass(m_OnRenderObjectCallbackPass);

            bool lastCameraInTheStack = cameraData.resolveFinalTarget;
            bool hasCaptureActions = renderingData.cameraData.captureActions != null && lastCameraInTheStack;
            bool applyFinalPostProcessing = anyPostProcessing && lastCameraInTheStack;

            // When post-processing is enabled we can use the stack to resolve rendering to camera target (screen or RT).
            // However when there are render passes executing after post we avoid resolving to screen so rendering continues (before sRGBConvertion etc)
            bool resolvePostProcessingToCameraTarget = !hasCaptureActions && !hasPassesAfterPostProcessing && !applyFinalPostProcessing;


            /* Add By:   Takeshi
             * Apply:    This is The first process of Fixing UI Alpha Gamma (in case of Post Processing Off)
             *           make 3D render image transform to sRGB Color, before the render of UI images,
             *           alpha blend will be hapened in sRGB color space.
             */
            if (!anyPostProcessing)
            {
                // if (!cameraData.isSceneViewCamera && camera.gameObject.layer == LayerMaskName.Default && cameraData.camera.cameraType != CameraType.Reflection)
                if (isMainCamera)
                {
                    m_BlitFixGammaPass.Setup(m_CameraColorAttachment);
                    EnqueuePass(m_BlitFixGammaPass);
                }

#if UNITY_EDITOR
                if (cameraData.isSceneViewCamera)
                {
                    m_BlitWhenPostProcessOffSceneViwePass.Setup(m_CameraColorAttachment);
                    EnqueuePass(m_BlitWhenPostProcessOffSceneViwePass);
                }
#endif
            }

            if (isUICamera && StaticGaussianBlurContext.Context != null && StaticGaussianBlurContext.Context.IsRenderBlurRT)
            {
                m_staticGaussianBlurPass.Setup(m_ActiveCameraColorAttachment);
                EnqueuePass(m_staticGaussianBlurPass);
            }

            if (!isUICamera && UnityEngine.Rendering.Universal.GameQualitySetting.ToneMapping && !drawOpaqueOverdraw)
            {
                EnqueuePass(m_roleEffectsBeforeDepthPass);
                EnqueuePass(m_roleEffectsAfterDepthPass);
            }

            // 暂时优化合并掉了
            // /* 没有 Final Post 时启用的小黑屋校色 Pass */
            // var sourceForBlitWhenFxaaOffPass = RenderTargetHandle.CameraTarget;
            // if (!applyFinalPostProcessing && camera.gameObject.layer == LayerMaskName.UIScene)
            // {
            //     m_BlitWhenFXAAOffPass.Setup(cameraTargetDescriptor, sourceForBlitWhenFxaaOffPass );
            //     EnqueuePass(m_BlitWhenFXAAOffPass);
            // }
            // /* End Add */

            /* Add By Takeshi; Fix SceneView Gamma */
#if UNITY_EDITOR
            if (renderingData.cameraData.renderType == CameraRenderType.Base && renderingData.cameraData.isSceneViewCamera)
            {

                m_BlitBeforeUIInSceneViewPass.Setup(cameraTargetDescriptor, m_ActiveCameraColorAttachment, m_ActiveCameraDepthAttachment);//
                EnqueuePass(m_BlitBeforeUIInSceneViewPass);
                m_FixGammaBlitInSceneViewPass.Setup(cameraTargetDescriptor, m_ActiveCameraColorAttachment);
                EnqueuePass(m_FixGammaBlitInSceneViewPass);
            }
#endif
#if UNITY_EDITOR
            if (OverdrawProcess != null) OverdrawProcess();
#endif
            /* End Add */

            if (!EngineContext.none3DCamera)
            {
                if (EngineContext.IsRunning && EngineContext.instance != null &&
                    EngineContext.instance.stateflag.HasFlag(EngineContext.SFlag_Distortion) || !EngineContext.IsRunning)
                {
                    EnqueuePass(m_distortionPass);
                }
            }
            if (lastCameraInTheStack)
            {
                // Post-processing will resolve to final target. No need for final blit pass.
                if (applyPostProcessing)
                {
                    var destination = resolvePostProcessingToCameraTarget ? RenderTargetHandle.CameraTarget : m_AfterPostProcessColor;

                    // if resolving to screen we need to be able to perform sRGBConvertion in post-processing if necessary
                    bool doSRGBConvertion = resolvePostProcessingToCameraTarget;

                    m_PostProcessPass.Setup(m_TemporaPass, cameraTargetDescriptor, m_ActiveCameraColorAttachment, destination, m_ActiveCameraDepthAttachment, m_ColorGradingLut, applyFinalPostProcessing, doSRGBConvertion);
                    EnqueuePass(m_PostProcessPass);
                }


                // if we applied post-processing for this camera it means current active texture is m_AfterPostProcessColor
                var sourceForFinalPass = (applyPostProcessing) ? m_AfterPostProcessColor : m_ActiveCameraColorAttachment;

                // Do FXAA or any other final post-processing effect that might need to run after AA.
                if (applyFinalPostProcessing)
                {
                    m_FinalPostProcessPass.SetupFinalPass(m_TemporaPass, cameraTargetDescriptor, sourceForFinalPass);
                    EnqueuePass(m_FinalPostProcessPass);
                }

                if (renderingData.cameraData.captureActions != null)
                {
                    m_CapturePass.Setup(sourceForFinalPass);
                    EnqueuePass(m_CapturePass);
                }

                // if post-processing then we already resolved to camera target while doing post.
                // Also only do final blit if camera is not rendering to RT.
                bool cameraTargetResolved =
                    // final PP always blit to camera target
                    applyFinalPostProcessing ||
                    // no final PP but we have PP stack. In that case it blit unless there are render pass after PP
                    (applyPostProcessing && !hasPassesAfterPostProcessing) ||
                    // offscreen camera rendering to a texture, we don't need a blit pass to resolve to screen
                    m_ActiveCameraColorAttachment == RenderTargetHandle.GetCameraTarget(cameraData.xr) ||
                    // Camera is used to UIScene
                    isUISceneCamera
                    ;

                // We need final blit to resolve to screen
                if (!cameraTargetResolved && !isUISceneCamera && UnityEngine.Rendering.Universal.GameQualitySetting.ToneMapping)
                {
                    m_FinalBlitPass.Setup(cameraTargetDescriptor, sourceForFinalPass);
                    EnqueuePass(m_FinalBlitPass);
                }



#if ENABLE_VR && ENABLE_XR_MODULE
                bool depthTargetResolved =
                    // active depth is depth target, we don't need a blit pass to resolve
                    m_ActiveCameraDepthAttachment == RenderTargetHandle.GetCameraTarget(cameraData.xr);

                if (!depthTargetResolved && cameraData.xr.copyDepth)
                {
                    m_XRCopyDepthPass.Setup(m_ActiveCameraDepthAttachment, RenderTargetHandle.GetCameraTarget(cameraData.xr));
                    EnqueuePass(m_XRCopyDepthPass);
                }
#endif
            }

            // stay in RT so we resume rendering on stack after post-processing
            else if (applyPostProcessing)
            {
                m_PostProcessPass.Setup(m_TemporaPass, cameraTargetDescriptor, m_ActiveCameraColorAttachment, m_AfterPostProcessColor, m_ActiveCameraDepthAttachment, m_ColorGradingLut, false, false);
                EnqueuePass(m_PostProcessPass);
            }

#if UNITY_EDITOR
            if (isSceneViewCamera)
            {
                // Scene view camera should always resolve target (not stacked)
                Assertions.Assert.IsTrue(lastCameraInTheStack, "Editor camera must resolve target upon finish rendering.");
                m_SceneViewDepthCopyPass.Setup(m_DepthTexture);
                EnqueuePass(m_SceneViewDepthCopyPass);
            }
#endif
            if (useTAA)
                EnqueuePass(m_TemporaPass.Swap(useTAA, UniversalRenderPipeline.asset.TemporaAASetting));
        }
        /// <inheritdoc />
        public override void SetupLights(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            m_ForwardLights.Setup(context, ref renderingData);

            // Perform per-tile light culling on CPU
            if (this.actualRenderingMode == RenderingMode.Deferred)
                m_DeferredLights.SetupLights(context, ref renderingData);
        }

        /// <inheritdoc />
        public override void SetupCullingParameters(ref ScriptableCullingParameters cullingParameters,
            ref CameraData cameraData)
        {
            // TODO: PerObjectCulling also affect reflection probes. Enabling it for now.
            // if (asset.additionalLightsRenderingMode == LightRenderingMode.Disabled ||
            //     asset.maxAdditionalLightsCount == 0)
            // {
            //     cullingParameters.cullingOptions |= CullingOptions.DisablePerObjectCulling;
            // }

            // We disable shadow casters if both shadow casting modes are turned off
            // or the shadow distance has been turned down to zero
            if (cameraData.isUiCamera)
            {
                cullingParameters.cullingOptions &= ~CullingOptions.ShadowCasters;
                cullingParameters.cullingOptions &= ~CullingOptions.NeedsLighting;
                cullingParameters.cullingOptions &= ~CullingOptions.OcclusionCull;
                cullingParameters.cullingOptions &= ~CullingOptions.NeedsReflectionProbes;
                cullingParameters.maximumVisibleLights = 0;
                cullingParameters.shadowDistance = 0;
            }
            else
            {
                bool isShadowCastingDisabled = !UniversalRenderPipeline.asset.supportsMainLightShadows && !UniversalRenderPipeline.asset.supportsAdditionalLightShadows;
                bool isShadowDistanceZero = Mathf.Approximately(cameraData.maxShadowDistance, 0.0f);
                if (isShadowCastingDisabled || isShadowDistanceZero)
                {
                    cullingParameters.cullingOptions &= ~CullingOptions.ShadowCasters;
                }

                if (this.actualRenderingMode == RenderingMode.Deferred)
                    cullingParameters.maximumVisibleLights = 0xFFFF;
                else
                {
                    // We set the number of maximum visible lights allowed and we add one for the mainlight...
                    cullingParameters.maximumVisibleLights = UniversalRenderPipeline.maxVisibleAdditionalLights + 1;
                }
                cullingParameters.shadowDistance = cameraData.maxShadowDistance;
            }
        }

        /// <inheritdoc />
        public override void FinishRendering(CommandBuffer cmd)
        {
            if (m_ActiveCameraColorAttachment != RenderTargetHandle.CameraTarget)
            {
                cmd.ReleaseTemporaryRT(m_ActiveCameraColorAttachment.id);
                m_ActiveCameraColorAttachment = RenderTargetHandle.CameraTarget;
            }

            if (m_ActiveCameraDepthAttachment != RenderTargetHandle.CameraTarget)
            {
                cmd.ReleaseTemporaryRT(m_ActiveCameraDepthAttachment.id);
                m_ActiveCameraDepthAttachment = RenderTargetHandle.CameraTarget;
            }
        }

        void EnqueueDeferred(ref RenderingData renderingData, bool hasDepthPrepass, bool applyMainShadow, bool applyAdditionalShadow)
        {
            // the last slice is the lighting buffer created in DeferredRenderer.cs
            m_GBufferHandles[(int)DeferredLights.GBufferHandles.Lighting] = m_ActiveCameraColorAttachment;

            m_DeferredLights.Setup(
                ref renderingData,
                applyAdditionalShadow ? m_AdditionalLightsShadowCasterPass : null,
                hasDepthPrepass,
                renderingData.cameraData.renderType == CameraRenderType.Overlay,
                m_DepthTexture,
                m_DepthInfoTexture,
                m_TileDepthInfoTexture,
                m_ActiveCameraDepthAttachment, m_GBufferHandles
            );

            EnqueuePass(m_GBufferPass);

            EnqueuePass(m_RenderOpaqueForwardOnlyPass);

            // Change By Takeshi
            // if (UniversalRenderPipeline.planarShadowEnabled)
            //     EnqueuePass(m_PlanarShadowPass);
            // EnqueuePass(m_RenderOutlinePass);

            //EnqueuePass(m_ForwardTransparentPass);

            //Must copy depth for deferred shading: TODO wait for API fix to bind depth texture as read-only resource.
            if (!hasDepthPrepass)
            {
                m_GBufferCopyDepthPass.Setup(m_CameraDepthAttachment, m_DepthTexture);
                EnqueuePass(m_GBufferCopyDepthPass);
            }

            // Note: DeferredRender.Setup is called by UniversalRenderPipeline.RenderSingleCamera (overrides ScriptableRenderer.Setup).
            // At this point, we do not know if m_DeferredLights.m_Tilers[x].m_Tiles actually contain any indices of lights intersecting tiles (If there are no lights intersecting tiles, we could skip several following passes) : this information is computed in DeferredRender.SetupLights, which is called later by UniversalRenderPipeline.RenderSingleCamera (via ScriptableRenderer.Execute).
            // However HasTileLights uses m_HasTileVisLights which is calculated by CheckHasTileLights from all visibleLights. visibleLights is the list of lights that have passed camera culling, so we know they are in front of the camera. So we can assume m_DeferredLights.m_Tilers[x].m_Tiles will not be empty in that case.
            // m_DeferredLights.m_Tilers[x].m_Tiles could be empty if we implemented an algorithm accessing scene depth information on the CPU side, but this (access depth from CPU) will probably not happen.
            if (m_DeferredLights.HasTileLights())
            {
                // Compute for each tile a 32bits bitmask in which a raised bit means "this 1/32th depth slice contains geometry that could intersect with lights".
                // Per-tile bitmasks are obtained by merging together the per-pixel bitmasks computed for each individual pixel of the tile.
                EnqueuePass(m_TileDepthRangePass);

                // On some platform, splitting the bitmasks computation into two passes:
                //   1/ Compute bitmasks for individual or small blocks of pixels
                //   2/ merge those individual bitmasks into per-tile bitmasks
                // provides better performance that doing it in a single above pass.
                if (m_DeferredLights.HasTileDepthRangeExtraPass())
                    EnqueuePass(m_TileDepthRangeExtraPass);
            }

            EnqueuePass(m_DeferredPass);
        }

        private struct RenderPassInputSummary
        {
            internal bool requiresDepthTexture;
            internal bool requiresDepthPrepass;
            internal bool requiresNormalsTexture;
            internal bool requiresColorTexture;
        }

        private RenderPassInputSummary GetRenderPassInputs(ref RenderingData renderingData)
        {
            RenderPassInputSummary inputSummary = new RenderPassInputSummary();
            for (int i = 0; i < activeRenderPassQueue.Count; ++i)
            {
                ScriptableRenderPass pass = activeRenderPassQueue[i];
                bool needsDepth = (pass.input & ScriptableRenderPassInput.Depth) != ScriptableRenderPassInput.None;
                bool needsNormals = (pass.input & ScriptableRenderPassInput.Normal) != ScriptableRenderPassInput.None;
                bool needsColor = (pass.input & ScriptableRenderPassInput.Color) != ScriptableRenderPassInput.None;
                bool eventBeforeOpaque = pass.renderPassEvent <= RenderPassEvent.BeforeRenderingOpaques;

                inputSummary.requiresDepthTexture |= needsDepth;
                inputSummary.requiresDepthPrepass |= needsNormals || needsDepth && eventBeforeOpaque;
                inputSummary.requiresNormalsTexture |= needsNormals;
                inputSummary.requiresColorTexture |= needsColor;
            }

            return inputSummary;
        }

        void CreateCameraRenderTarget(ScriptableRenderContext context, ref RenderTextureDescriptor descriptor, bool createColor, bool createDepth, bool taa)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, Profiling.createCameraRenderTarget))
            {
                if (createColor)
                {
                    bool useDepthRenderBuffer = m_ActiveCameraDepthAttachment == RenderTargetHandle.CameraTarget;
                    var colorDescriptor = descriptor;
                    colorDescriptor.useMipMap = false;
                    colorDescriptor.autoGenerateMips = false;
                    colorDescriptor.depthBufferBits = (useDepthRenderBuffer) ? k_DepthStencilBufferBits : 0;
                    if(taa && RenderingUtils.SupportsGraphicsFormat(GraphicsFormat.R16G16B16A16_SFloat, FormatUsage.Linear | FormatUsage.Render))
                    {
                        colorDescriptor.graphicsFormat = GraphicsFormat.R16G16B16A16_SFloat;
                    }
                    cmd.GetTemporaryRT(m_ActiveCameraColorAttachment.id, colorDescriptor, FilterMode.Bilinear);
                }

                if (createDepth)
                {
                    var depthDescriptor = descriptor;
                    depthDescriptor.useMipMap = false;
                    depthDescriptor.autoGenerateMips = false;
#if ENABLE_VR && ENABLE_XR_MODULE
                    // XRTODO: Enabled this line for non-XR pass? URP copy depth pass is already capable of handling MSAA.
                    depthDescriptor.bindMS = depthDescriptor.msaaSamples > 1 && !SystemInfo.supportsMultisampleAutoResolve && (SystemInfo.supportsMultisampledTextures != 0);
#endif
                    depthDescriptor.colorFormat = RenderTextureFormat.Depth;
                    depthDescriptor.depthBufferBits = k_DepthStencilBufferBits;
                    cmd.GetTemporaryRT(m_ActiveCameraDepthAttachment.id, depthDescriptor, FilterMode.Point);
                }
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        bool PlatformRequiresExplicitMsaaResolve()
        {
            // On Metal/iOS the MSAA resolve is done implicitly as part of the renderpass, so we do not need an extra intermediate pass for the explicit autoresolve.
            // TODO: should also be valid on Metal MacOS/Editor, but currently not working as expected. Remove the "mobile only" requirement once trunk has a fix.

            return !SystemInfo.supportsMultisampleAutoResolve &&
                   !(SystemInfo.graphicsDeviceType == GraphicsDeviceType.Metal && Application.isMobilePlatform);
        }

        /// <summary>
        /// Checks if the pipeline needs to create a intermediate render texture.
        /// </summary>
        /// <param name="cameraData">CameraData contains all relevant render target information for the camera.</param>
        /// <seealso cref="CameraData"/>
        /// <returns>Return true if pipeline needs to render to a intermediate render texture.</returns>
        bool RequiresIntermediateColorTexture(ref CameraData cameraData)
        {
            // When rendering a camera stack we always create an intermediate render texture to composite camera results.
            // We create it upon rendering the Base camera.
            if (cameraData.renderType == CameraRenderType.Base && !cameraData.resolveFinalTarget)
                return true;

            // Always force rendering into intermediate color texture if deferred rendering mode is selected.
            // Reason: without intermediate color texture, the target camera texture is y-flipped.
            // However, the target camera texture is bound during gbuffer pass and deferred pass.
            // Gbuffer pass will not be y-flipped because it is MRT (see ScriptableRenderContext implementation),
            // while deferred pass will be y-flipped, which breaks rendering.
            // This incurs an extra blit into at the end of rendering.
            if (this.actualRenderingMode == RenderingMode.Deferred)
                return true;

            bool isSceneViewCamera = cameraData.isSceneViewCamera;
            var cameraTargetDescriptor = cameraData.cameraTargetDescriptor;
            int msaaSamples = cameraTargetDescriptor.msaaSamples;
            bool isScaledRender = !Mathf.Approximately(cameraData.renderScale, 1.0f);
            bool isCompatibleBackbufferTextureDimension = cameraTargetDescriptor.dimension == TextureDimension.Tex2D;
            bool requiresExplicitMsaaResolve = msaaSamples > 1 && PlatformRequiresExplicitMsaaResolve();
            bool isOffscreenRender = cameraData.targetTexture != null && !isSceneViewCamera;
            bool isCapturing = cameraData.captureActions != null;

#if ENABLE_VR && ENABLE_XR_MODULE
            if (cameraData.xr.enabled)
                isCompatibleBackbufferTextureDimension = cameraData.xr.renderTargetDesc.dimension == cameraTargetDescriptor.dimension;
#endif

            bool requiresBlitForOffscreenCamera = cameraData.postProcessEnabled || cameraData.requiresOpaqueTexture || requiresExplicitMsaaResolve || !cameraData.isDefaultViewport;
            if (isOffscreenRender)
                return requiresBlitForOffscreenCamera;

            return requiresBlitForOffscreenCamera || isSceneViewCamera || isScaledRender || cameraData.isHdrEnabled ||
                   !isCompatibleBackbufferTextureDimension || isCapturing || cameraData.requireSrgbConversion;
        }

        bool CanCopyDepth(ref CameraData cameraData)
        {
            bool msaaEnabledForCamera = cameraData.cameraTargetDescriptor.msaaSamples > 1;
            bool supportsTextureCopy = SystemInfo.copyTextureSupport != CopyTextureSupport.None;
            bool supportsDepthTarget = RenderingUtils.SupportsRenderTextureFormat(RenderTextureFormat.Depth);
            bool supportsDepthCopy = !msaaEnabledForCamera && (supportsDepthTarget || supportsTextureCopy);

            // TODO:  We don't have support to highp Texture2DMS currently and this breaks depth precision.
            // currently disabling it until shader changes kick in.
            //bool msaaDepthResolve = msaaEnabledForCamera && SystemInfo.supportsMultisampledTextures != 0;
            bool msaaDepthResolve = false;
            return supportsDepthCopy || msaaDepthResolve;
        }
    }
}
