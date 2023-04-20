using System.Runtime.CompilerServices;
using CFEngine;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal
{
    // TODO: xmldoc
    public interface IPostProcessComponent
    {
        bool IsActive();
        bool IsTileCompatible();
    }
    public enum TerrainFunc
    {
        Default,
        None,
        PointLight,
        TerrainBlend
    }
    public sealed class GameQualitySetting
    {
        /// <summary>
        /// 深度图开关
        /// </summary>
        public static bool Depth = false;
        /// <summary>
        /// Bloom
        /// </summary>
        public static bool Bloom = false;

        /// <summary>
        /// 景深
        /// </summary>
        public static bool DepthOfField = false;
        /// <summary>
        /// 径向模糊
        /// </summary>
        public static bool RadialBlur = false;

        /// <summary>
        /// 光晕
        /// </summary>
        public static bool LensFlares = false;
        /// <summary>
        /// ToneMapping(包含各种校色)
        /// </summary>
        public static bool ToneMapping = false;
        /// <summary>
        /// 绘制草体
        /// </summary>
        public static bool DrawGrass = true;
        /// <summary>
        /// GodRay
        /// </summary>
        public static bool GodRay = false;
        /// <summary>
        /// 高级效果ShaderTagId
        /// </summary>
        public static bool UniversalForwardHigh = true;

        /// <summary>
        /// 最差效果ShaderTagId
        /// </summary>
        public static bool UniversalForwardLow = false;

        /// <summary>
        /// 地形混合控制开关
        /// </summary>
        public static TerrainFunc TerrainFeature = TerrainFunc.None;

        public static RenderQualityLevel ResolutionLevel = RenderQualityLevel.High;
        public static RenderQualityLevel MatLevel = RenderQualityLevel.High;
        public static RenderQualityLevel ShadowLevel = RenderQualityLevel.High;
        public static RenderQualityLevel SFXLevel = RenderQualityLevel.High;
        public static RenderQualityLevel TextureLevel = RenderQualityLevel.High;
        private static bool init = true;
        public static void SetEnvironment()
        {
#if UNITY_EDITOR
            if (!EngineContext.IsRunning)
            {
                RenderLevelManager.ImposterRenderLevel = RenderQualityLevel.High;
                RenderLevelManager.PostProcessRenderLevel = RenderQualityLevel.Ultra;
                RenderLevelManager.SfxRenderLevel = RenderQualityLevel.High;
                RenderLevelManager.RoleRenderLevel = RenderQualityLevel.High;
                RenderLevelManager.ResolutionLevel = RenderQualityLevel.High;
                Debug.LogWarning("RenderLevelManager set high on eidtor mod.");

                QualitySettings.skinWeights = SkinWeights.FourBones;

                Depth = true;
                Bloom = true;
                DepthOfField = true;
                RadialBlur = true;
                LensFlares = true;
                ToneMapping = true;
                DrawGrass = true;
                GodRay = true;
                UniversalForwardHigh = true;
                UniversalForwardLow = false;
                TerrainFeature = TerrainFunc.None;
            }
#endif
            if(init)
            {
                init = false;

                SetMixFuncKeyword(TerrainFunc.None);


#if !UNITY_EDITOR
                RenderLevelManager.PostProcessRenderLevel = RenderQualityLevel.VeryLow;
#endif
            }
        }

        private static TerrainFunc lastFunc = TerrainFunc.Default;
        public static TerrainFunc SetMixFuncKeyword(TerrainFunc func)
        {
            Debug.LogWarning("SetMixFuncKeyword:" + func);
            if (lastFunc == func)
                return lastFunc;

            if (GameQualitySetting.MatLevel == RenderQualityLevel.Ultra)
            {
                lastFunc = func;
                switch (lastFunc)
                {
                    case TerrainFunc.None:
                        {
                            //Shader.DisableKeyword("_HUAWEI");
                            Shader.EnableKeyword("_NO_FUNC");
                            Shader.DisableKeyword("_ENABLE_TERRAIN_BLENDING");
                            Shader.DisableKeyword("_USE_ADDITIONAL");
                            break;
                        }
                    case TerrainFunc.TerrainBlend:
                        {
                            //Shader.DisableKeyword("_HUAWEI");
                            Shader.DisableKeyword("_NO_FUNC");
                            Shader.EnableKeyword("_ENABLE_TERRAIN_BLENDING");
                            Shader.DisableKeyword("_USE_ADDITIONAL");
                            break;
                        }
                    case TerrainFunc.PointLight:
                        {
                            //Shader.DisableKeyword("_HUAWEI");
                            Shader.DisableKeyword("_NO_FUNC");
                            Shader.DisableKeyword("_ENABLE_TERRAIN_BLENDING");
                            Shader.EnableKeyword("_USE_ADDITIONAL");
                            break;
                        }
                }

            }
            else
            {
                lastFunc = TerrainFunc.None;
                //Shader.DisableKeyword("_HUAWEI");
                Shader.EnableKeyword("_NO_FUNC");
                Shader.DisableKeyword("_ENABLE_TERRAIN_BLENDING");
                Shader.DisableKeyword("_USE_ADDITIONAL");
            }
            return lastFunc;
        }

        public static void SetToneMappingEnable()
        {
            Shader.SetGlobalFloat("tonemapping_enable", ToneMapping ? 1 : 0);
        }
        private static bool initSamplerComparisonState = true;
        private static bool isHuawei = false;
        private static string[] huaweiModels = new string[] { "mali-g52", "mali-g76"};
        /// <summary>
        /// 华为手机特殊处理
        /// </summary>
        public static bool IsHuawei
        {
            get
            {
                if (initSamplerComparisonState)
                {
                    initSamplerComparisonState = false;

                    string dm = SystemInfo.deviceModel;
                    string gdn = SystemInfo.graphicsDeviceName;
                    if (dm != null && gdn != null && dm.ToLower().Contains("huawei"))
                    {
                        foreach (string model in huaweiModels)
                        {
                            if (gdn.ToLower().Contains(model))
                            {
                                isHuawei = true;
                                break;
                            }
                        }
                    }
                }
                return isHuawei;
            }
        }

    }

}

namespace UnityEngine.Rendering.Universal.Internal
{
    // TODO: TAA
    // TODO: Motion blur
    /// <summary>
    /// Renders the post-processing effect stack.
    /// </summary>
    public class PostProcessPass : ScriptableRenderPass
    {
        private Tempora taaPass = null;
        RenderTextureDescriptor m_Descriptor;
        RenderTargetHandle m_Source;
        RenderTargetHandle m_Destination;
        RenderTargetHandle m_Depth;
        RenderTargetHandle m_InternalLut;

        const string k_RenderPostProcessingTag = "Render PostProcessing Effects";
        const string k_RenderFinalPostProcessingTag = "Render Final PostProcessing Pass";
        private static readonly ProfilingSampler m_ProfilingRenderPostProcessing = new ProfilingSampler(k_RenderPostProcessingTag);
        private static readonly ProfilingSampler m_ProfilingRenderFinalPostProcessing = new ProfilingSampler(k_RenderFinalPostProcessingTag);

        MaterialLibrary m_Materials;
        PostProcessData m_Data;

        // Builtin effects settings
        // DepthOfField m_DepthOfField;
        MotionBlur m_MotionBlur;
        PaniniProjection m_PaniniProjection;
        Bloom m_Bloom;
        GodRay m_GodRay;
        LensDistortion m_LensDistortion;
        ChromaticAberration m_ChromaticAberration;
        Vignette m_Vignette;
        ColorLookup m_ColorLookup;
        ColorAdjustments m_ColorAdjustments;
        Tonemapping m_Tonemapping;
        FilmGrain m_FilmGrain;

        // Misc
        const int k_MaxPyramidSize = 16;

        readonly GraphicsFormat m_DefaultHDRFormat;
        bool m_UseRGBM;
        readonly GraphicsFormat m_SMAAEdgeFormat;
        readonly GraphicsFormat m_GaussianCoCFormat;
        private readonly GraphicsFormat m_LdrFormat;

        Matrix4x4[] m_PrevViewProjM = new Matrix4x4[2];
        bool m_ResetHistory;
        int m_DitheringTextureIndex;
        RenderTargetIdentifier[] m_MRT2;
        Vector4[] m_BokehKernel;
        int m_BokehHash;

        // True when this is the very last pass in the pipeline
        bool m_IsFinalPass;

        // If there's a final post process pass after this pass.
        // If yes, Film Grain and Dithering are setup in the final pass, otherwise they are setup in this pass.
        bool m_HasFinalPass;

        // Some Android devices do not support sRGB backbuffer
        // We need to do the conversion manually on those
        bool m_EnableSRGBConversionIfNeeded;

        // Option to use procedural draw instead of cmd.blit
        bool m_UseDrawProcedural;

        readonly HableCurve m_HableCurve = new HableCurve();

        Material m_BlitMaterial;
        private RenderTargetIdentifier _screenEffectTmp;
        private bool _screenEffectTmpUsed = false;

        private PostProcessRTManager _ppRTManager;

        public PostProcessPass(RenderPassEvent evt, PostProcessData data, Material blitMaterial)
        {
            base.profilingSampler = new ProfilingSampler(nameof(PostProcessPass));
            renderPassEvent = evt;
            m_Data = data;
            m_Materials = new MaterialLibrary(data);
            m_BlitMaterial = blitMaterial;

            // Texture format pre-lookup
            if (SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, FormatUsage.Linear | FormatUsage.Render))
            {
                m_DefaultHDRFormat = GraphicsFormat.B10G11R11_UFloatPack32;
                m_UseRGBM = false;
            }
            else
            {
                m_DefaultHDRFormat = QualitySettings.activeColorSpace == ColorSpace.Linear
                    ? GraphicsFormat.R8G8B8A8_SRGB
                    : GraphicsFormat.R8G8B8A8_UNorm;
                m_UseRGBM = true;
            }

            // Only two components are needed for edge render texture, but on some vendors four components may be faster.
            if (SystemInfo.IsFormatSupported(GraphicsFormat.R8G8_UNorm, FormatUsage.Render) && SystemInfo.graphicsDeviceVendor.ToLowerInvariant().Contains("arm"))
                m_SMAAEdgeFormat = GraphicsFormat.R8G8_UNorm;
            else
                m_SMAAEdgeFormat = GraphicsFormat.R8G8B8A8_UNorm;

            if (SystemInfo.IsFormatSupported(GraphicsFormat.R16_UNorm, FormatUsage.Linear | FormatUsage.Render))
                m_GaussianCoCFormat = GraphicsFormat.R16_UNorm;
            else if (SystemInfo.IsFormatSupported(GraphicsFormat.R16_SFloat, FormatUsage.Linear | FormatUsage.Render))
                m_GaussianCoCFormat = GraphicsFormat.R16_SFloat;
            else // Expect CoC banding
                m_GaussianCoCFormat = GraphicsFormat.R8_UNorm;

            m_MRT2 = new RenderTargetIdentifier[2];
            m_ResetHistory = true;

            m_LdrFormat = GraphicsFormat.R8G8B8A8_SRGB;
            if (!RenderingUtils.SupportsGraphicsFormat(m_LdrFormat, FormatUsage.Linear | FormatUsage.Render))
            {
                m_LdrFormat = GraphicsFormatUtility.GetGraphicsFormat(RenderTextureFormat.ARGB32, true);
                if (!RenderingUtils.SupportsGraphicsFormat(m_LdrFormat, FormatUsage.Linear | FormatUsage.Render))
                {
                    m_LdrFormat = GraphicsFormat.R8G8B8A8_UNorm;
                }
            }
        }

        public void Cleanup() => m_Materials.Cleanup();

        public void Setup(Tempora taa, in RenderTextureDescriptor baseDescriptor, in RenderTargetHandle source, in RenderTargetHandle destination, in RenderTargetHandle depth, in RenderTargetHandle internalLut, bool hasFinalPass, bool enableSRGBConversion)
        {
            taaPass = taa;
            m_Descriptor = baseDescriptor;
            //m_Descriptor.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;//Add by: Takeshi
            m_Descriptor.useMipMap = false;
            m_Descriptor.autoGenerateMips = false;
            _ppRTManager = new PostProcessRTManager(6, m_Descriptor.width, m_Descriptor.height);
            m_Source = source;
            m_Destination = destination;
            m_Depth = depth;
            m_InternalLut = internalLut;
            m_IsFinalPass = false;
            m_HasFinalPass = hasFinalPass;
            m_EnableSRGBConversionIfNeeded = enableSRGBConversion;
        }

        //public void Setup(in RenderTextureDescriptor baseDescriptor, in RenderTargetHandle source, in RenderTargetHandle destination, in RenderTargetHandle depth, in RenderTargetHandle internalLut, bool hasFinalPass, bool enableSRGBConversion)
        //{
        //    m_Descriptor = baseDescriptor;
        //    //m_Descriptor.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;//Add by: Takeshi
        //    m_Descriptor.useMipMap = false;
        //    m_Descriptor.autoGenerateMips = false;
        //    m_Source = source;
        //    m_Destination = destination;
        //    m_Depth = depth;
        //    m_InternalLut = internalLut;
        //    m_IsFinalPass = false;
        //    m_HasFinalPass = hasFinalPass;
        //    m_EnableSRGBConversionIfNeeded = enableSRGBConversion;
        //}

        public void SetupFinalPass(Tempora taa, in RenderTextureDescriptor baseDescriptor, in RenderTargetHandle source)
        {
            taaPass = taa;
            m_Descriptor = baseDescriptor;
            m_Source = source;
            m_Destination = RenderTargetHandle.CameraTarget;
            m_IsFinalPass = true;
            m_HasFinalPass = false;
            m_EnableSRGBConversionIfNeeded = true;
        }

        /// <inheritdoc/>
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            if (m_Destination == RenderTargetHandle.CameraTarget)
                return;

            // If RenderTargetHandle already has a valid internal render target identifier, we shouldn't request a temp
            if (m_Destination.HasInternalRenderTargetId())
                return;

            var desc = GetCompatibleDescriptor();
            desc.depthBufferBits = 0;
            cmd.GetTemporaryRT(m_Destination.id, desc, FilterMode.Bilinear);
        }

        /// <inheritdoc/>
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (m_Destination == RenderTargetHandle.CameraTarget)
                return;

            // Logic here matches the if check in OnCameraSetup
            if (m_Destination.HasInternalRenderTargetId())
                return;
            _ppRTManager.Release(cmd);
            cmd.ReleaseTemporaryRT(m_Destination.id);
        }

        public void ResetHistory()
        {
            m_ResetHistory = true;
        }

        public bool CanRunOnTile()
        {
            // Check builtin & user effects here
            return false;
        }


        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // Start by pre-fetching all builtin effect settings we need
            // Some of the color-grading settings are only used in the color grading lut pass
            var stack = VolumeManager.instance.stack;
            // m_DepthOfField        = stack.GetComponent<DepthOfField>();
            m_MotionBlur = stack.GetComponent<MotionBlur>();
            m_PaniniProjection = stack.GetComponent<PaniniProjection>();
            m_Bloom = stack.GetComponent<Bloom>();
            m_GodRay = stack.GetComponent<GodRay>();
            m_LensDistortion = stack.GetComponent<LensDistortion>();
            m_ChromaticAberration = stack.GetComponent<ChromaticAberration>();
            m_Vignette = stack.GetComponent<Vignette>();
            m_ColorLookup = stack.GetComponent<ColorLookup>();
            m_ColorAdjustments = stack.GetComponent<ColorAdjustments>();
            m_Tonemapping = stack.GetComponent<Tonemapping>();
            m_FilmGrain = stack.GetComponent<FilmGrain>();
            m_UseDrawProcedural = renderingData.cameraData.xr.enabled;
            antialiasing = UniversalRenderPipeline.asset.Antialiasing;

            if (m_IsFinalPass)
            {
                var cmd = CommandBufferPool.Get();
                using (new ProfilingScope(cmd, m_ProfilingRenderFinalPostProcessing))
                {
                    RenderFinalPass(cmd, ref renderingData);
                }

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);

                ScreenEffect.enabled = false;
            }
            else if (CanRunOnTile())
            {
                // TODO: Add a fast render path if only on-tile compatible effects are used and we're actually running on a platform that supports it
                // Note: we can still work on-tile if FXAA is enabled, it'd be part of the final pass
            }
            else
            {
                // Regular render path (not on-tile) - we do everything in a single command buffer as it
                // makes it easier to manage temporary targets' lifetime
                var cmd = CommandBufferPool.Get();
                using (new ProfilingScope(cmd, m_ProfilingRenderPostProcessing))
                {
                    /* Add By:   Takeshi
                     * Apply:    This is The first process of Fixing UI Alpha Gamma (in case of Post Processing is working)
                     *           make 3D render image transform to sRGB Color, before the render of UI images,
                     *           alpha blend will be hapened in sRGB color space. */
                    cmd.EnableShaderKeyword(ShaderKeywordStrings.LinearToSRGBConversion);
                    /* End Add */

                    Render(cmd, ref renderingData);

                    /* Add By:Takeshi; Over the first process of Fix UI Alpha Gamma */
                    cmd.DisableShaderKeyword(ShaderKeywordStrings.LinearToSRGBConversion);
                }
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            m_ResetHistory = false;
        }

        RenderTextureDescriptor GetCompatibleDescriptor()
            => GetCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, m_Descriptor.graphicsFormat, m_Descriptor.depthBufferBits);

        RenderTextureDescriptor GetCompatibleDescriptor(int width, int height, GraphicsFormat format, int depthBufferBits = 0)
        {
            var desc = m_Descriptor;
            desc.depthBufferBits = depthBufferBits;
            desc.msaaSamples = m_Descriptor.msaaSamples;
            desc.width = width;
            desc.height = height;
            if (desc.graphicsFormat == GraphicsFormat.R8G8B8A8_SRGB)
            {
                luminanceAlpha = false;
            }
            else
            {
                luminanceAlpha = true;
            }
            desc.graphicsFormat = m_LdrFormat;

            return desc;
        }

        bool RequireSRGBConversionBlitToBackBuffer(CameraData cameraData)
        {
            return cameraData.requireSrgbConversion && m_EnableSRGBConversionIfNeeded;
        }

        private new void Blit(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, Material material, int passIndex = 0)
        {
            cmd.SetGlobalTexture(ShaderPropertyId.sourceTex, source);
            if (m_UseDrawProcedural)
            {
                Vector4 scaleBias = new Vector4(1, 1, 0, 0);
                cmd.SetGlobalVector(ShaderPropertyId.scaleBias, scaleBias);

                cmd.SetRenderTarget(new RenderTargetIdentifier(destination, 0, CubemapFace.Unknown, -1),
                    RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
                cmd.DrawProcedural(Matrix4x4.identity, material, passIndex, MeshTopology.Quads, 4, 1, null);
            }
            else
            {
                cmd.Blit(source, destination, material, passIndex);
            }
        }

        private void DrawFullscreenMesh(CommandBuffer cmd, Material material, int passIndex)
        {
            if (m_UseDrawProcedural)
            {
                Vector4 scaleBias = new Vector4(1, 1, 0, 0);
                cmd.SetGlobalVector(ShaderPropertyId.scaleBias, scaleBias);
                cmd.DrawProcedural(Matrix4x4.identity, material, passIndex, MeshTopology.Quads, 4, 1, null);
            }
            else
            {
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, material, 0, passIndex);
            }
        }

        public static void BlitFullscreenTriangle(
            CommandBuffer cmd,
            int source,
            int destination, Material mat, int pass)
        {
            cmd.SetGlobalTexture(ShaderConstants.MainTex, source);
            cmd.SetRenderTarget(destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, mat, 0, pass);
            // cmd.DrawMesh (fullscreenTriangle, Matrix4x4.identity, propertySheet.material, 0, pass, propertySheet.properties);
        }
        private AntialiasingMode antialiasing;
        private bool luminanceAlpha = true;
        void Render(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;

            // Don't use these directly unless you have a good reason to, use GetSource() and
            // GetDestination() instead
            bool tempTargetUsed = false;
            bool tempTarget2Used = false;
            int source = m_Source.id;
            int destination = -1;
            bool isSceneViewCamera = cameraData.isSceneViewCamera;

            // Utilities to simplify intermediate target management
            int GetSource() => source;

            int GetDestination()
            {
                if (destination == -1)
                {
                    cmd.GetTemporaryRT(ShaderConstants._TempTarget, GetCompatibleDescriptor(), FilterMode.Bilinear);
                    destination = ShaderConstants._TempTarget;
                    tempTargetUsed = true;
                }
                else if (destination == m_Source.id && m_Descriptor.msaaSamples > 1)
                {
                    // Avoid using m_Source.id as new destination, it may come with a depth buffer that we don't want, may have MSAA that we don't want etc
                    cmd.GetTemporaryRT(ShaderConstants._TempTarget2, GetCompatibleDescriptor(), FilterMode.Bilinear);
                    destination = ShaderConstants._TempTarget2;
                    tempTarget2Used = true;
                }

                return destination;
            }

            void Swap() => CoreUtils.Swap(ref source, ref destination);

            // Setup projection matrix for cmd.DrawMesh()
            cmd.SetGlobalMatrix(ShaderConstants._FullscreenProjMat, GL.GetGPUProjectionMatrix(Matrix4x4.identity, true));

            // Optional NaN killer before post-processing kicks in
            // stopNaN may be null on Adreno 3xx. It doesn't support full shader level 3.5, but SystemInfo.graphicsShaderLevel is 35.
            if (cameraData.isStopNaNEnabled && m_Materials.stopNaN != null)
            {
                using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.StopNaNs)))
                {
                    RenderingUtils.Blit(
                        cmd, GetSource(), GetDestination(), m_Materials.stopNaN, 0, m_UseDrawProcedural,
                        RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                        RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);

                    Swap();
                }
            }
            
            //ScreenEffect
            {
                if (!isSceneViewCamera)
                {
                    if (ScreenEffect.enabled)
                    {
                        RenderTargetIdentifier stencilDepth;
                        // if (m_Depth == RenderTargetHandle.CameraTarget || m_Descriptor.msaaSamples > 1)
                        // {
                        //     stencilDepth = ShaderConstants._EdgeTexture;
                        // }
                        // else
                        // {
                            stencilDepth = m_Depth.Identifier();
                        // }
                        DoScreenEffect(cameraData.camera, cmd, GetSource(), GetDestination(), stencilDepth, cameraData.pixelRect);
                        Swap();
                    }
                }
            }

            // Anti-aliasing
            if (antialiasing == AntialiasingMode.SMAA)
            {
                using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.SMAA)))
                {
                    DoSubpixelMorphologicalAntialiasing(ref cameraData, cmd, GetSource(), GetDestination());
                    Swap();
                }
            }
            

            //if (UnityEngine.Rendering.Universal.GameQualitySetting.Level != UnityEngine.Rendering.Universal.QualityLevel.Low)
            {
                // Depth of Field
                DOFParam dofParam;
                bool active = URPDepthOfField.instance.GetValue(out dofParam);
                if (!GameQualitySetting.DepthOfField)
                {
                    active = false;
                }
                if (/*m_DepthOfField.IsActive()*/active && !isSceneViewCamera)
                {
                    URPDepthOfField.instance.ResetParam();
                    if (dofParam.active)
                    {
                        URPProfileId markerName = URPProfileId.EasyDepthOfField;
                        // switch (m_DepthOfField.mode.value)
                        // {
                        //     case DepthOfFieldMode.Gaussian : markerName = URPProfileId.GaussianDepthOfField;
                        //         break;
                        //     case DepthOfFieldMode.Bokeh : markerName = URPProfileId.BokehDepthOfField;
                        //         break;
                        //     // case DepthOfFieldMode.Easy : markerName = URPProfileId.EasyDepthOfField;
                        //     //     break;
                        //     default:
                        //         markerName = URPProfileId.EasyDepthOfField;break;
                        // }
                        // var markerName = m_DepthOfField.mode.value == DepthOfFieldMode.Gaussian
                        //     ? URPProfileId.GaussianDepthOfField
                        //     : URPProfileId.BokehDepthOfField;

                        using (new ProfilingScope(cmd, ProfilingSampler.Get(markerName)))
                        {
                            DoDepthOfField(cameraData.camera, cmd, GetSource(), GetDestination(), cameraData.pixelRect, dofParam);
                            Swap();
                        }
                    }
                }
                
                // Motion blur
                if (m_MotionBlur.IsActive() && !isSceneViewCamera)
                {
                    using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.MotionBlur)))
                    {
                        DoMotionBlur(cameraData, cmd, GetSource(), GetDestination());
                        Swap();
                    }
                }
            }

            // Panini projection is done as a fullscreen pass after all depth-based effects are done
            // and before bloom kicks in
            if (m_PaniniProjection.IsActive() && !isSceneViewCamera)
            {
                using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.PaniniProjection)))
                {
                    DoPaniniProjection(cameraData.camera, cmd, GetSource(), GetDestination());
                    Swap();
                }
            }


            // Combined post-processing stack
            using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.UberPostProcess)))
            {
                // Reset uber keywords
                m_Materials.uber.shaderKeywords = null;

                if(luminanceAlpha && antialiasing == AntialiasingMode.FXAA)
                {
                    m_Materials.uber.EnableKeyword(ShaderKeywordStrings.Fxaa);
                }
                bool radialBlurActive = false;
                if (GameQualitySetting.RadialBlur)
                {
                    if (URPRadialBlur.instance.GetValue(out RadialBlurParam param))
                    {
                        radialBlurActive = SetupRadialBlur(param, cameraData.camera, cmd, GetSource(), m_Materials.uber);
                    }
                }

                //God Ray
                bool godrayActive = false;
                if (GameQualitySetting.GodRay)
                {
                    godrayActive = m_GodRay.IsActive();
                    if (godrayActive)
                    //if (godrayActive && PostProcessSetting.AfterEffect)
                    {
                        using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.GodRay)))
                        {
                            godrayActive = DoGodRay(cameraData.camera, cmd, GetSource(), m_Materials.uber);
                        }
                    }
                }

                // Bloom goes first
                bool bloomActive = false;
                if (GameQualitySetting.Bloom)
                {
                    bloomActive = m_Bloom.IsActive();
                    if (bloomActive)
                    //if (bloomActive && PostProcessSetting.AfterEffect)
                    {
                        using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.Bloom)))
                        {
                            SetupBloom(cmd, GetSource(), m_Materials.uber);
                        }
                    }
                }

                //if (PostProcessSetting.AfterEffect)
                {
                    // Setup other effects constants
                    SetupLensDistortion(m_Materials.uber, isSceneViewCamera);
                    SetupChromaticAberration(m_Materials.uber);
                    SetupVignette(m_Materials.uber);
                }
                SetupColorGrading(cmd, ref renderingData, m_Materials.uber);
                //if (PostProcessSetting.AfterEffect)
                {
                    // Only apply dithering & grain if there isn't a final pass.
                    SetupGrain(cameraData, m_Materials.uber);
                    SetupDithering(cameraData, m_Materials.uber);
                }
                if (RequireSRGBConversionBlitToBackBuffer(cameraData))
                    m_Materials.uber.EnableKeyword(ShaderKeywordStrings.LinearToSRGBConversion);

                // Done with Uber, blit it
                cmd.SetGlobalTexture(ShaderPropertyId.sourceTex, GetSource());

                var colorLoadAction = RenderBufferLoadAction.DontCare;
                if (m_Destination == RenderTargetHandle.CameraTarget && !cameraData.isDefaultViewport)
                    colorLoadAction = RenderBufferLoadAction.Load;

                // Note: We rendering to "camera target" we need to get the cameraData.targetTexture as this will get the targetTexture of the camera stack.
                // Overlay cameras need to output to the target described in the base camera while doing camera stack.
                RenderTargetHandle cameraTargetHandle = RenderTargetHandle.GetCameraTarget(cameraData.xr);
                RenderTargetIdentifier cameraTarget = (cameraData.targetTexture != null && !cameraData.xr.enabled) ? new RenderTargetIdentifier(cameraData.targetTexture) : cameraTargetHandle.Identifier();
                cameraTarget = (m_Destination == RenderTargetHandle.CameraTarget) ? cameraTarget : m_Destination.Identifier();

                // With camera stacking we not always resolve post to final screen as we might run post-processing in the middle of the stack.
                bool finishPostProcessOnScreen = cameraData.resolveFinalTarget || (m_Destination == cameraTargetHandle || m_HasFinalPass == true);

#if ENABLE_VR && ENABLE_XR_MODULE
                if (cameraData.xr.enabled)
                {
                    cmd.SetRenderTarget(new RenderTargetIdentifier(cameraTarget, 0, CubemapFace.Unknown, -1),
                         colorLoadAction, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);

                    bool isRenderToBackBufferTarget = cameraTarget == cameraData.xr.renderTarget && !cameraData.xr.renderTargetIsRenderTexture;
                    if (isRenderToBackBufferTarget)
                        cmd.SetViewport(cameraData.pixelRect);
                    // We y-flip if
                    // 1) we are bliting from render texture to back buffer and
                    // 2) renderTexture starts UV at top
                    bool yflip = isRenderToBackBufferTarget && SystemInfo.graphicsUVStartsAtTop;
                    Vector4 scaleBias = yflip ? new Vector4(1, -1, 0, 1) : new Vector4(1, 1, 0, 0);
                    cmd.SetGlobalVector(ShaderPropertyId.scaleBias, scaleBias);
                    cmd.DrawProcedural(Matrix4x4.identity, m_Materials.uber, 0, MeshTopology.Quads, 4, 1, null);

                    // call interface (blur)

                    // TODO: We need a proper camera texture swap chain in URP.
                    // For now, when render post-processing in the middle of the camera stack (not resolving to screen)
                    // we do an extra blit to ping pong results back to color texture. In future we should allow a Swap of the current active color texture
                    // in the pipeline to avoid this extra blit.
                    if (!finishPostProcessOnScreen)
                    {
                        cmd.SetGlobalTexture(ShaderPropertyId.sourceTex, cameraTarget);
                        cmd.SetRenderTarget(new RenderTargetIdentifier(m_Source.id, 0, CubemapFace.Unknown, -1),
                            colorLoadAction, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);

                        scaleBias = new Vector4(1, 1, 0, 0); ;
                        cmd.SetGlobalVector(ShaderPropertyId.scaleBias, scaleBias);
                        cmd.DrawProcedural(Matrix4x4.identity, m_BlitMaterial, 0, MeshTopology.Quads, 4, 1, null);
                    }
                }
                else
#endif
                {
                    cmd.SetRenderTarget(cameraTarget, colorLoadAction, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
                    cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);

                    if (m_Destination == RenderTargetHandle.CameraTarget)
                        cmd.SetViewport(cameraData.pixelRect);

                    cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, m_Materials.uber);

                    // TODO: We need a proper camera texture swap chain in URP.
                    // For now, when render post-processing in the middle of the camera stack (not resolving to screen)
                    // we do an extra blit to ping pong results back to color texture. In future we should allow a Swap of the current active color texture
                    // in the pipeline to avoid this extra blit.
                    if (!finishPostProcessOnScreen)
                    {
                        // Add by:Takeshi
                        // cmd.ReleaseTemporaryRT(m_Source.id);
                        RenderTextureDescriptor srgbDescriptor = m_Descriptor;
                        // srgbDescriptor.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
                        // srgbDescriptor.depthBufferBits = 0;
                        // cmd.GetTemporaryRT(m_Source.id, srgbDescriptor);
                        // End Add

                        cmd.SetGlobalTexture(ShaderPropertyId.sourceTex, cameraTarget);
                        //srgb.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
                        //cmd.SetRenderTarget(m_Source.id, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
                        //cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, m_BlitMaterial);
                        //---统一AA修改：程庆宝
                        SetupAA(m_BlitMaterial, cameraData, srgbDescriptor, cmd, RenderBufferLoadAction.DontCare, cameraTarget, m_Source.id);
                    }

                    cmd.SetViewProjectionMatrices(cameraData.camera.worldToCameraMatrix, cameraData.camera.projectionMatrix);
                }
                if (godrayActive)
                    cmd.ReleaseTemporaryRT(ShaderConstants._GodRayTex);

                if (bloomActive)
                    cmd.ReleaseTemporaryRT(ShaderConstants._BloomRT4x1);

                if (tempTargetUsed)
                    cmd.ReleaseTemporaryRT(ShaderConstants._TempTarget);

                if (tempTarget2Used)
                    cmd.ReleaseTemporaryRT(ShaderConstants._TempTarget2);

                // if (radialBlurActive)
                //     cmd.ReleaseTemporaryRT(ShaderConstants._RadialBlurTex);
            }

        }

        private BuiltinRenderTextureType BlitDstDiscardContent(CommandBuffer cmd, RenderTargetIdentifier rt)
        {
            // We set depth to DontCare because rt might be the source of PostProcessing used as a temporary target
            // Source typically comes with a depth buffer and right now we don't have a way to only bind the color attachment of a RenderTargetIdentifier
            cmd.SetRenderTarget(new RenderTargetIdentifier(rt, 0, CubemapFace.Unknown, -1),
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            return BuiltinRenderTextureType.CurrentActive;
        }


        #region Sub-pixel Morphological Anti-aliasing
        void DoSubpixelMorphologicalAntialiasing(ref CameraData cameraData, CommandBuffer cmd, int source, int destination)
        {
            var camera = cameraData.camera;
            var pixelRect = cameraData.pixelRect;
            var material = m_Materials.subpixelMorphologicalAntialiasing;
            const int kStencilBit = 64;

            // Globals
            material.SetVector(ShaderConstants._Metrics, new Vector4(1f / m_Descriptor.width, 1f / m_Descriptor.height, m_Descriptor.width, m_Descriptor.height));
            material.SetTexture(ShaderConstants._AreaTexture, m_Data.textures.smaaAreaTex);
            material.SetTexture(ShaderConstants._SearchTexture, m_Data.textures.smaaSearchTex);
            material.SetInt(ShaderConstants._StencilRef, kStencilBit);
            material.SetInt(ShaderConstants._StencilMask, kStencilBit);

            // Quality presets
            material.shaderKeywords = null;

            switch (UniversalRenderPipeline.asset.SMAAQuality)
            {
                case AntialiasingQuality.Low:
                    material.EnableKeyword(ShaderKeywordStrings.SmaaLow);
                    break;
                case AntialiasingQuality.Medium:
                    material.EnableKeyword(ShaderKeywordStrings.SmaaMedium);
                    break;
                case AntialiasingQuality.High:
                    material.EnableKeyword(ShaderKeywordStrings.SmaaHigh);
                    break;
            }

            // Intermediate targets
            RenderTargetIdentifier stencil; // We would only need stencil, no depth. But Unity doesn't support that.
            int tempDepthBits;
            if (m_Depth == RenderTargetHandle.CameraTarget || m_Descriptor.msaaSamples > 1)
            {
                // In case m_Depth is CameraTarget it may refer to the backbuffer and we can't use that as an attachment on all platforms
                stencil = ShaderConstants._EdgeTexture;
                tempDepthBits = 24;
            }
            else
            {
                stencil = m_Depth.Identifier();
                tempDepthBits = 0;
            }
            cmd.GetTemporaryRT(ShaderConstants._EdgeTexture, GetCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, m_SMAAEdgeFormat, tempDepthBits), FilterMode.Bilinear);
            cmd.GetTemporaryRT(ShaderConstants._BlendTexture, GetCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, GraphicsFormat.R8G8B8A8_UNorm), FilterMode.Point);

            // Prepare for manual blit
            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            cmd.SetViewport(pixelRect);

            // Pass 1: Edge detection
            cmd.SetRenderTarget(new RenderTargetIdentifier(ShaderConstants._EdgeTexture, 0, CubemapFace.Unknown, -1),
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, stencil,
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            cmd.ClearRenderTarget(true, true, Color.clear);
            cmd.SetGlobalTexture(ShaderConstants._ColorTexture, source);
            DrawFullscreenMesh(cmd, material, 0);

            // Pass 2: Blend weights
            cmd.SetRenderTarget(new RenderTargetIdentifier(ShaderConstants._BlendTexture, 0, CubemapFace.Unknown, -1),
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, stencil,
                RenderBufferLoadAction.Load, RenderBufferStoreAction.DontCare);
            cmd.ClearRenderTarget(false, true, Color.clear);
            cmd.SetGlobalTexture(ShaderConstants._ColorTexture, ShaderConstants._EdgeTexture);
            DrawFullscreenMesh(cmd, material, 1);

            // Pass 3: Neighborhood blending
            cmd.SetRenderTarget(new RenderTargetIdentifier(destination, 0, CubemapFace.Unknown, -1),
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            cmd.SetGlobalTexture(ShaderConstants._ColorTexture, source);
            cmd.SetGlobalTexture(ShaderConstants._BlendTexture, ShaderConstants._BlendTexture);
            DrawFullscreenMesh(cmd, material, 2);

            // Cleanup
            cmd.ReleaseTemporaryRT(ShaderConstants._EdgeTexture);
            cmd.ReleaseTemporaryRT(ShaderConstants._BlendTexture);
            cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
        }

        #endregion

        public override void FrameCleanup(CommandBuffer cmd)
        {
            base.FrameCleanup(cmd);
        }

        #region TimelineEffect
        void DoScreenEffect(Camera camera, CommandBuffer cmd, int source, int destination, RenderTargetIdentifier depth, Rect pixelRect)
        {
            ScreenEffect effect = ScreenEffect.Instance();
            if (effect.transitionState == 1)
            {
                DebugLog.AddLog2(m_Source.Identifier().ToString());
                cmd.Blit(source, ScreenEffect.GetTempTex());
                Shader.SetGlobalTexture(ScreenEffect.TransitionTex, ScreenEffect.GetTempTex());
                effect.transitionState = 2;
            }

            if (ScreenEffect.enabled)
            {
                BlitSp(camera, cmd, source, BlitDstDiscardContent(cmd, destination), depth, effect);
                ScreenEffect.enabled = false;
            }
        }


        void BlitSp(Camera camera, CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier dest,
            RenderTargetIdentifier depth, ScreenEffect singleton)
        {

            if (singleton.additivePass)cmd.SetGlobalTexture("_OldTex", source);
            cmd.SetGlobalTexture(ShaderPropertyId.sourceTex, source);
            // CoreUtils.Swap(ref source, ref dest);
            _screenEffectTmp = new RenderTargetIdentifier(dest, 0, CubemapFace.Unknown, -1);
            cmd.SetRenderTarget(_screenEffectTmp, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, 
                depth, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            cmd.ClearRenderTarget(false, true, Color.clear);
            // DrawFullscreenMesh(cmd, material, passIndex);
            cmd.SetViewProjectionMatrices(Matrix4x4.identity,Matrix4x4.identity);
            DrawScreenMesh(singleton, (int)singleton.type);
            if (singleton.additivePass)
            {
                CoreUtils.Swap(ref source, ref dest);
                DrawScreenMesh(singleton, 0);
            }
            cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
            CoreUtils.Swap(ref source, ref dest);

            void DrawScreenMesh(ScreenEffect screenEffect, int index)
            {
                if (screenEffect.overrideProperty && ScreenEffect.Instance().mpb != null)
                {
                    cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, screenEffect.GetMat(), 0, index,
                        ScreenEffect.Instance().mpb);
                }
                else
                {
                    cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, screenEffect.GetMat(), 0, index);
                }
            }
        }
        #endregion

        #region Depth Of Field

        void DoDepthOfField(Camera camera, CommandBuffer cmd, int source, int destination, Rect pixelRect, DOFParam currentParam)
        {
            // if (m_DepthOfField.mode.value == DepthOfFieldMode.Gaussian)
            //     DoGaussianDepthOfField(camera, cmd, source, destination, pixelRect);
            // else if (m_DepthOfField.mode.value == DepthOfFieldMode.Bokeh)
            //     DoBokehDepthOfField(cmd, source, destination, pixelRect);
            // else if (m_DepthOfField.mode.value == DepthOfFieldMode.Easy)
            DoEasyDepthOfField(camera, cmd, source, destination, pixelRect, currentParam);
        }

        private static class Pass
        {
            public const int preFilterPass = 0;
            public const int bokehPass = 1;
            public const int postFilterPass = 2;
        }

        void DoEasyDepthOfField(Camera camera, CommandBuffer cmd, int source, int destination, Rect pixelRect,
            DOFParam currentParam)
        {
            //============================== Shader Enviroment ==============================
            var material = m_Materials.easyDepthOfField;
            Vector4 param = new Vector4(currentParam.EasyMode ? 1 : 0, currentParam.FocusDistance,
                currentParam.BokehRangeFar,
                currentParam.FocusRangeFar);
            Vector4 param2 = new Vector4(currentParam.BlurRadius, 0, currentParam.Intensity, 1);
            cmd.SetGlobalVector(ShaderConstants._CoCParams, param);
            cmd.SetGlobalVector(ShaderConstants._CoCParams2, param2);


            //============================== Getting RenderTextures ==============================
            int width = m_Descriptor.width >> 1;
            int height = m_Descriptor.height >> 1;

            int dofD1 = _ppRTManager.GetTemporary(cmd, 1);
            int dofD1P = _ppRTManager.GetTemporary(cmd, 1, true);
            // cmd.GetTemporaryRT(ShaderConstants.downSampleX4,
            //     GetCompatibleDescriptor(width, height, GraphicsFormat.R16G16B16A16_SFloat), FilterMode.Bilinear);
            // //_DofRT
            // cmd.GetTemporaryRT(ShaderConstants._DofRT,
            //     GetCompatibleDescriptor(width, height, GraphicsFormat.R16G16B16A16_SFloat), FilterMode.Bilinear);

            PostProcessUtils.SetSourceSize(cmd, m_Descriptor);

            cmd.SetGlobalTexture(ShaderConstants.MainTex, source);
            cmd.SetRenderTarget(dofD1P, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, material, 0, Pass.preFilterPass);

            cmd.SetGlobalTexture(ShaderConstants.MainTex, dofD1P);
            cmd.SetRenderTarget(dofD1, RenderBufferLoadAction.DontCare,
                RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, material, 0, Pass.bokehPass);


            bool needSwitch = false;
#if UNITY_ANDROID
            for (int i = 0; i < 2; i++)
#else
            for (int i = 0; i < 4; i++)
#endif
            {
                var srcRT = needSwitch ? dofD1P : dofD1;
                var dstRT = needSwitch ? dofD1 : dofD1P;
                param2.y = i;
                cmd.SetGlobalVector(ShaderConstants._CoCParams2, param2);
                cmd.SetGlobalTexture(ShaderConstants.MainTex, srcRT);
                cmd.SetRenderTarget(dstRT, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                    RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, material, 0, Pass.postFilterPass);
                needSwitch = !needSwitch;
            }

            var src = needSwitch ? dofD1P : dofD1;

            cmd.SetGlobalTexture(ShaderConstants._DofTex, src);
            cmd.SetGlobalTexture(ShaderConstants.MainTex, source);
            cmd.SetRenderTarget(destination, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, material, 0, 3);

            // cmd.ReleaseTemporaryRT(ShaderConstants.downSampleX4);
            // cmd.ReleaseTemporaryRT(ShaderConstants._DofRT);
        }

        /// <summary>
        /// 统一AA控制
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="cameraData"></param>
        /// <param name="cmd"></param>
        /// <param name="colorLoadAction"></param>
        /// <param name="source"></param>
        /// <param name="cameraTarget"></param>
        void SetupAA(Material mat, CameraData cameraData, RenderTextureDescriptor baseDescriptor, CommandBuffer cmd, RenderBufferLoadAction colorLoadAction, RenderTargetIdentifier source, RenderTargetIdentifier cameraTarget)
        {
            if (!cameraData.isSceneViewCamera)
            {
                if (antialiasing == AntialiasingMode.FXAA)
                {
                    if(luminanceAlpha && !m_IsFinalPass)
                        cmd.EnableShaderKeyword(ShaderKeywordStrings.FxaaLuminance);
                    else
                         cmd.EnableShaderKeyword(ShaderKeywordStrings.Fxaa);

                    mat.SetFloat("_Fxaa_Sharp", UniversalRenderPipeline.asset.AntialiasingSharp);
                }
                else if (antialiasing == AntialiasingMode.TAA)
                {
                    if (taaPass != null && taaPass.CanDo)
                    {
                        cmd.EnableShaderKeyword(ShaderKeywordStrings.Taa);
                        RenderTextureDescriptor desc = baseDescriptor;
                        desc.graphicsFormat = m_LdrFormat;
                        taaPass.PostProcessDrawMesh(cmd, desc, m_Source.id, cameraTarget, mat);
                        cmd.DisableShaderKeyword(ShaderKeywordStrings.Fxaa);
                        cmd.DisableShaderKeyword(ShaderKeywordStrings.FxaaLuminance);
                        cmd.DisableShaderKeyword(ShaderKeywordStrings.Taa);
                        return;
                    }
                }
            }
            cmd.SetRenderTarget(cameraTarget, colorLoadAction, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, mat);
            cmd.DisableShaderKeyword(ShaderKeywordStrings.Fxaa);
            cmd.DisableShaderKeyword(ShaderKeywordStrings.Taa);
            cmd.DisableShaderKeyword(ShaderKeywordStrings.FxaaLuminance);
        }

        // void DoGaussianDepthOfField(Camera camera, CommandBuffer cmd, int source, int destination, Rect pixelRect)
        // {
        //     int downSample = 2;
        //     var material = m_Materials.gaussianDepthOfField;
        //     int wh = m_Descriptor.width / downSample;
        //     int hh = m_Descriptor.height / downSample;
        //     float farStart = m_DepthOfField.gaussianStart.value;
        //     float farEnd = Mathf.Max(farStart, m_DepthOfField.gaussianEnd.value);
        //
        //     // Assumes a radius of 1 is 1 at 1080p
        //     // Past a certain radius our gaussian kernel will look very bad so we'll clamp it for
        //     // very high resolutions (4K+).
        //     float maxRadius = m_DepthOfField.gaussianMaxRadius.value * (wh / 1080f);
        //     maxRadius = Mathf.Min(maxRadius, 2f);
        //
        //     CoreUtils.SetKeyword(material, ShaderKeywordStrings.HighQualitySampling, m_DepthOfField.highQualitySampling.value);
        //     material.SetVector(ShaderConstants._CoCParams, new Vector3(farStart, farEnd, maxRadius));
        //
        //     // Temporary textures
        //     cmd.GetTemporaryRT(ShaderConstants._FullCoCTexture, GetCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, m_GaussianCoCFormat), FilterMode.Bilinear);
        //     cmd.GetTemporaryRT(ShaderConstants._HalfCoCTexture, GetCompatibleDescriptor(wh, hh, m_GaussianCoCFormat), FilterMode.Bilinear);
        //     cmd.GetTemporaryRT(ShaderConstants._PingTexture, GetCompatibleDescriptor(wh, hh, m_DefaultHDRFormat), FilterMode.Bilinear);
        //     cmd.GetTemporaryRT(ShaderConstants._PongTexture, GetCompatibleDescriptor(wh, hh, m_DefaultHDRFormat), FilterMode.Bilinear);
        //     // Note: fresh temporary RTs don't require explicit RenderBufferLoadAction.DontCare, only when they are reused (such as PingTexture)
        //
        //     PostProcessUtils.SetSourceSize(cmd, m_Descriptor);
        //     cmd.SetGlobalVector(ShaderConstants._DownSampleScaleFactor, new Vector4(1.0f / downSample, 1.0f / downSample, downSample, downSample));
        //
        //     // Compute CoC
        //     Blit(cmd, source, ShaderConstants._FullCoCTexture, material, 0);
        //
        //     // Downscale & prefilter color + coc
        //     m_MRT2[0] = ShaderConstants._HalfCoCTexture;
        //     m_MRT2[1] = ShaderConstants._PingTexture;
        //
        //     cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
        //     cmd.SetViewport(pixelRect);
        //     cmd.SetGlobalTexture(ShaderConstants._ColorTexture, source);
        //     cmd.SetGlobalTexture(ShaderConstants._FullCoCTexture, ShaderConstants._FullCoCTexture);
        //     cmd.SetRenderTarget(m_MRT2, ShaderConstants._HalfCoCTexture, 0, CubemapFace.Unknown, -1);
        //     DrawFullscreenMesh(cmd, material, 1);
        //
        //     cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
        //
        //     // Blur
        //     cmd.SetGlobalTexture(ShaderConstants._HalfCoCTexture, ShaderConstants._HalfCoCTexture);
        //     Blit(cmd, ShaderConstants._PingTexture, ShaderConstants._PongTexture, material, 2);
        //     Blit(cmd, ShaderConstants._PongTexture, BlitDstDiscardContent(cmd, ShaderConstants._PingTexture), material, 3);
        //
        //     // Composite
        //     cmd.SetGlobalTexture(ShaderConstants._ColorTexture, ShaderConstants._PingTexture);
        //     cmd.SetGlobalTexture(ShaderConstants._FullCoCTexture, ShaderConstants._FullCoCTexture);
        //     Blit(cmd, source, BlitDstDiscardContent(cmd, destination), material, 4);
        //
        //     // Cleanup
        //     cmd.ReleaseTemporaryRT(ShaderConstants._FullCoCTexture);
        //     cmd.ReleaseTemporaryRT(ShaderConstants._HalfCoCTexture);
        //     cmd.ReleaseTemporaryRT(ShaderConstants._PingTexture);
        //     cmd.ReleaseTemporaryRT(ShaderConstants._PongTexture);
        // }
        //
        // void PrepareBokehKernel()
        // {
        //     const int kRings = 4;
        //     const int kPointsPerRing = 7;
        //
        //     // Check the existing array
        //     if (m_BokehKernel == null)
        //         m_BokehKernel = new Vector4[42];
        //
        //     // Fill in sample points (concentric circles transformed to rotated N-Gon)
        //     int idx = 0;
        //     float bladeCount = m_DepthOfField.bladeCount.value;
        //     float curvature = 1f - m_DepthOfField.bladeCurvature.value;
        //     float rotation = m_DepthOfField.bladeRotation.value * Mathf.Deg2Rad;
        //     const float PI = Mathf.PI;
        //     const float TWO_PI = Mathf.PI * 2f;
        //
        //     for (int ring = 1; ring < kRings; ring++)
        //     {
        //         float bias = 1f / kPointsPerRing;
        //         float radius = (ring + bias) / (kRings - 1f + bias);
        //         int points = ring * kPointsPerRing;
        //
        //         for (int point = 0; point < points; point++)
        //         {
        //             // Angle on ring
        //             float phi = 2f * PI * point / points;
        //
        //             // Transform to rotated N-Gon
        //             // Adapted from "CryEngine 3 Graphics Gems" [Sousa13]
        //             float nt = Mathf.Cos(PI / bladeCount);
        //             float dt = Mathf.Cos(phi - (TWO_PI / bladeCount) * Mathf.Floor((bladeCount * phi + Mathf.PI) / TWO_PI));
        //             float r = radius * Mathf.Pow(nt / dt, curvature);
        //             float u = r * Mathf.Cos(phi - rotation);
        //             float v = r * Mathf.Sin(phi - rotation);
        //
        //             m_BokehKernel[idx] = new Vector4(u, v);
        //             idx++;
        //         }
        //     }
        // }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float GetMaxBokehRadiusInPixels(float viewportHeight)
        {
            // Estimate the maximum radius of bokeh (empirically derived from the ring count)
            const float kRadiusInPixels = 14f;
            return Mathf.Min(0.05f, kRadiusInPixels / viewportHeight);
        }

        // void DoBokehDepthOfField(CommandBuffer cmd, int source, int destination, Rect pixelRect)
        // {
        //     int downSample = 2;
        //     var material = m_Materials.bokehDepthOfField;
        //     int wh = m_Descriptor.width / downSample;
        //     int hh = m_Descriptor.height / downSample;
        //
        //     // "A Lens and Aperture Camera Model for Synthetic Image Generation" [Potmesil81]
        //     float F = m_DepthOfField.focalLength.value / 1000f;
        //     float A = m_DepthOfField.focalLength.value / m_DepthOfField.aperture.value;
        //     float P = m_DepthOfField.focusDistance.value;
        //     float maxCoC = (A * F) / (P - F);
        //     float maxRadius = GetMaxBokehRadiusInPixels(m_Descriptor.height);
        //     float rcpAspect = 1f / (wh / (float)hh);
        //
        //     cmd.SetGlobalVector(ShaderConstants._CoCParams, new Vector4(P, maxCoC, maxRadius, rcpAspect));
        //
        //     // Prepare the bokeh kernel constant buffer
        //     int hash = m_DepthOfField.GetHashCode();
        //     if (hash != m_BokehHash)
        //     {
        //         m_BokehHash = hash;
        //         PrepareBokehKernel();
        //     }
        //
        //     cmd.SetGlobalVectorArray(ShaderConstants._BokehKernel, m_BokehKernel);
        //
        //     // Temporary textures
        //     cmd.GetTemporaryRT(ShaderConstants._FullCoCTexture, GetCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, GraphicsFormat.R8_UNorm), FilterMode.Bilinear);
        //     cmd.GetTemporaryRT(ShaderConstants._PingTexture, GetCompatibleDescriptor(wh, hh, GraphicsFormat.R16G16B16A16_SFloat), FilterMode.Bilinear);
        //     cmd.GetTemporaryRT(ShaderConstants._PongTexture, GetCompatibleDescriptor(wh, hh, GraphicsFormat.R16G16B16A16_SFloat), FilterMode.Bilinear);
        //
        //     PostProcessUtils.SetSourceSize(cmd, m_Descriptor);
        //     cmd.SetGlobalVector(ShaderConstants._DownSampleScaleFactor, new Vector4(1.0f / downSample, 1.0f / downSample, downSample, downSample));
        //
        //     // Compute CoC
        //     Blit(cmd, source, ShaderConstants._FullCoCTexture, material, 0);
        //     cmd.SetGlobalTexture(ShaderConstants._FullCoCTexture, ShaderConstants._FullCoCTexture);
        //
        //     // Downscale & prefilter color + coc
        //     Blit(cmd, source, ShaderConstants._PingTexture, material, 1);
        //
        //     // Bokeh blur
        //     Blit(cmd, ShaderConstants._PingTexture, ShaderConstants._PongTexture, material, 2);
        //
        //     // Post-filtering
        //     Blit(cmd, ShaderConstants._PongTexture, BlitDstDiscardContent(cmd, ShaderConstants._PingTexture), material, 3);
        //
        //     // Composite
        //     cmd.SetGlobalTexture(ShaderConstants._DofTexture, ShaderConstants._PingTexture);
        //     Blit(cmd, source, BlitDstDiscardContent(cmd, destination), material, 4);
        //
        //     // Cleanup
        //     cmd.ReleaseTemporaryRT(ShaderConstants._FullCoCTexture);
        //     cmd.ReleaseTemporaryRT(ShaderConstants._PingTexture);
        //     cmd.ReleaseTemporaryRT(ShaderConstants._PongTexture);
        // }

        #endregion

        #region Motion Blur
#if ENABLE_VR && ENABLE_XR_MODULE
        // Hold the stereo matrices to avoid allocating arrays every frame
        internal static readonly Matrix4x4[] viewProjMatrixStereo = new Matrix4x4[2];
#endif
        void DoMotionBlur(CameraData cameraData, CommandBuffer cmd, int source, int destination)
        {
            var material = m_Materials.cameraMotionBlur;

#if ENABLE_VR && ENABLE_XR_MODULE
            if (cameraData.xr.enabled && cameraData.xr.singlePassEnabled)
            {
                var viewProj0 = GL.GetGPUProjectionMatrix(cameraData.GetProjectionMatrix(0), true) * cameraData.GetViewMatrix(0);
                var viewProj1 = GL.GetGPUProjectionMatrix(cameraData.GetProjectionMatrix(1), true) * cameraData.GetViewMatrix(1);
                if (m_ResetHistory)
                {
                    viewProjMatrixStereo[0] = viewProj0;
                    viewProjMatrixStereo[1] = viewProj1;
                    material.SetMatrixArray("_PrevViewProjMStereo", viewProjMatrixStereo);
                }
                else
                    material.SetMatrixArray("_PrevViewProjMStereo", m_PrevViewProjM);

                m_PrevViewProjM[0] = viewProj0;
                m_PrevViewProjM[1] = viewProj1;
            }
            else
#endif
            {
                int prevViewProjMIdx = 0;
#if ENABLE_VR && ENABLE_XR_MODULE
                if (cameraData.xr.enabled)
                    prevViewProjMIdx = cameraData.xr.multipassId;
#endif
                // This is needed because Blit will reset viewproj matrices to identity and UniversalRP currently
                // relies on SetupCameraProperties instead of handling its own matrices.
                // TODO: We need get rid of SetupCameraProperties and setup camera matrices in Universal
                var proj = cameraData.GetProjectionMatrix();
                var view = cameraData.GetViewMatrix();
                var viewProj = proj * view;

                material.SetMatrix("_ViewProjM", viewProj);

                if (m_ResetHistory)
                    material.SetMatrix("_PrevViewProjM", viewProj);
                else
                    material.SetMatrix("_PrevViewProjM", m_PrevViewProjM[prevViewProjMIdx]);

                m_PrevViewProjM[prevViewProjMIdx] = viewProj;
            }

            material.SetFloat("_Intensity", m_MotionBlur.intensity.value);
            material.SetFloat("_Clamp", m_MotionBlur.clamp.value);

            PostProcessUtils.SetSourceSize(cmd, m_Descriptor);

            Blit(cmd, source, BlitDstDiscardContent(cmd, destination), material, (int)m_MotionBlur.quality.value);
        }

        #endregion

        #region Panini Projection

        // Back-ported & adapted from the work of the Stockholm demo team - thanks Lasse!
        void DoPaniniProjection(Camera camera, CommandBuffer cmd, int source, int destination)
        {
            float distance = m_PaniniProjection.distance.value;
            var viewExtents = CalcViewExtents(camera);
            var cropExtents = CalcCropExtents(camera, distance);

            float scaleX = cropExtents.x / viewExtents.x;
            float scaleY = cropExtents.y / viewExtents.y;
            float scaleF = Mathf.Min(scaleX, scaleY);

            float paniniD = distance;
            float paniniS = Mathf.Lerp(1f, Mathf.Clamp01(scaleF), m_PaniniProjection.cropToFit.value);

            var material = m_Materials.paniniProjection;
            material.SetVector(ShaderConstants._Params, new Vector4(viewExtents.x, viewExtents.y, paniniD, paniniS));
            material.EnableKeyword(
                1f - Mathf.Abs(paniniD) > float.Epsilon
                ? ShaderKeywordStrings.PaniniGeneric : ShaderKeywordStrings.PaniniUnitDistance
            );

            Blit(cmd, source, BlitDstDiscardContent(cmd, destination), material);
        }

        Vector2 CalcViewExtents(Camera camera)
        {
            float fovY = camera.fieldOfView * Mathf.Deg2Rad;
            float aspect = m_Descriptor.width / (float)m_Descriptor.height;

            float viewExtY = Mathf.Tan(0.5f * fovY);
            float viewExtX = aspect * viewExtY;

            return new Vector2(viewExtX, viewExtY);
        }

        Vector2 CalcCropExtents(Camera camera, float d)
        {
            // given
            //    S----------- E--X-------
            //    |    `  ~.  /,´
            //    |-- ---    Q
            //    |        ,/    `
            //  1 |      ,´/       `
            //    |    ,´ /         ´
            //    |  ,´  /           ´
            //    |,`   /             ,
            //    O    /
            //    |   /               ,
            //  d |  /
            //    | /                ,
            //    |/                .
            //    P
            //    |              ´
            //    |         , ´
            //    +-    ´
            //
            // have X
            // want to find E

            float viewDist = 1f + d;

            var projPos = CalcViewExtents(camera);
            var projHyp = Mathf.Sqrt(projPos.x * projPos.x + 1f);

            float cylDistMinusD = 1f / projHyp;
            float cylDist = cylDistMinusD + d;
            var cylPos = projPos * cylDistMinusD;

            return cylPos * (viewDist / cylDist);
        }

        #endregion

        #region Bloom
        void SetupBloom(CommandBuffer cmd, int source, Material uberMaterial)
        {
            RenderQualityLevel level = RenderLevelManager.PostProcessRenderLevel;
            Material bloomMaterial = m_Materials.bloom;

            // ============================== 预计算通用变量 ==============================
            Vector4 horizontal = new Vector4(Screen.height / (float)Screen.width, 0, 0, 0);
            Vector4 vertical = new Vector4(0, 1, 0, 0);
            Vector4 param = new Vector4(m_Bloom.directThreshold.value, m_Bloom.intensity.value, 0, 0);
            cmd.SetGlobalFloat(ShaderConstants._BloomBalance, m_Bloom.balance.value);
            cmd.SetGlobalVector(ShaderConstants._BloomParam, param);
            cmd.SetGlobalColor(ShaderConstants._BloomColor, m_Bloom.tint.value);
            CoreUtils.SetKeyword(bloomMaterial, ShaderKeywordStrings.UseRGBM, m_UseRGBM);

            // ============================== 降采样和颜色过滤 ==============================
            // TODO: 如果启用了dof，则直接取用dof图，可以减少一次降采样开销。
            RenderTextureDescriptor descX2 = GetDownSampleDescriptor(2);
            cmd.GetTemporaryRT(ShaderConstants._BloomRT4x0, descX2, FilterMode.Bilinear);
            cmd.GetTemporaryRT(ShaderConstants._BloomRT4x1, descX2, FilterMode.Bilinear);
            Blit(cmd, source, ShaderConstants._BloomRT4x0, bloomMaterial, Bloom.Pass.DownSampleAndFilter);
            
            // ============================== 第一次模糊 ==============================
            cmd.SetGlobalVector(ShaderConstants._Axis, horizontal);
            Blit(cmd, ShaderConstants._BloomRT4x0, ShaderConstants._BloomRT4x1, bloomMaterial, Bloom.Pass.Blur0);
            cmd.SetGlobalVector(ShaderConstants._Axis, vertical);
            Blit(cmd, ShaderConstants._BloomRT4x1, ShaderConstants._BloomRT4x0, bloomMaterial, Bloom.Pass.Blur0);

            // ============================== 高配额外3次模糊 ==============================
            if (level > RenderQualityLevel.High)
            {
                cmd.SetGlobalVector(ShaderConstants._Blend, Bloom.blend0);
                RenderDownSampleAndBlur(3, Bloom.Pass.Blur1, ShaderConstants._BloomRT4x0, ShaderConstants._BloomRT8x0, ShaderConstants._BloomRT8x1);
                RenderDownSampleAndBlur(4, Bloom.Pass.Blur2, ShaderConstants._BloomRT8x0, ShaderConstants._BloomRT16x0, ShaderConstants._BloomRT16x1);
                RenderDownSampleAndBlur(5, Bloom.Pass.Blur3, ShaderConstants._BloomRT16x0, ShaderConstants._BloomRT32x0, ShaderConstants._BloomRT32x1);
                cmd.SetGlobalTexture(ShaderConstants._MainTex0, ShaderConstants._BloomRT8x0);
                cmd.SetGlobalTexture(ShaderConstants._MainTex1, ShaderConstants._BloomRT16x0);
                cmd.SetGlobalTexture(ShaderConstants._MainTex2, ShaderConstants._BloomRT32x0);
                Blit(cmd, ShaderConstants._BloomRT4x0, ShaderConstants._BloomRT4x1, bloomMaterial, Bloom.Pass.UpSampleH);
            }
            // ============================== 中配额外1次模糊 ==============================
            // TODO：改成Kawase Bloom或者Dual Kawase Bloom
            else // Medium
            {
                cmd.SetGlobalVector(ShaderConstants._Blend, Bloom.blend1);
                RenderDownSampleAndBlur(3, Bloom.Pass.Blur1, ShaderConstants._BloomRT4x0, ShaderConstants._BloomRT8x0, ShaderConstants._BloomRT8x1);
                //RenderDownSampleAndBlur(4, Bloom.Pass.Blur2, ShaderConstants._BloomRT8x0, ShaderConstants._BloomRT16x0, ShaderConstants._BloomRT16x1);
                //RenderDownSampleAndBlur(5, Bloom.Pass.Blur2, ShaderConstants._BloomRT16x0, ShaderConstants._BloomRT32x0, ShaderConstants._BloomRT32x1);
                cmd.SetGlobalTexture(ShaderConstants._MainTex0, ShaderConstants._BloomRT8x0);
                //cmd.SetGlobalTexture(ShaderConstants._MainTex1, ShaderConstants._BloomRT16x0);
                //cmd.SetGlobalTexture(ShaderConstants._MainTex2, ShaderConstants._BloomRT32x0);
                Blit(cmd, ShaderConstants._BloomRT4x0, ShaderConstants._BloomRT4x1, bloomMaterial, Bloom.Pass.UpSampleM);
            }

            // ============================== 设置全局变量 ==============================
            cmd.SetGlobalTexture(ShaderConstants._Bloom_Texture, ShaderConstants._BloomRT4x1);
            uberMaterial.SetFloat(ShaderConstants._Bloom_RGBM, m_UseRGBM ? 1f : 0f);
            uberMaterial.EnableKeyword(ShaderKeywordStrings.Bloom);

            // ============================== 清理临时RT ==============================
            if (level > RenderQualityLevel.High)
            {
                cmd.ReleaseTemporaryRT(ShaderConstants._BloomRT4x0);
                cmd.ReleaseTemporaryRT(ShaderConstants._BloomRT8x0);
                cmd.ReleaseTemporaryRT(ShaderConstants._BloomRT8x1);
                cmd.ReleaseTemporaryRT(ShaderConstants._BloomRT16x0);
                cmd.ReleaseTemporaryRT(ShaderConstants._BloomRT16x1);
                cmd.ReleaseTemporaryRT(ShaderConstants._BloomRT32x0);
                cmd.ReleaseTemporaryRT(ShaderConstants._BloomRT32x1);
            }
            else // Medium
            {
                cmd.ReleaseTemporaryRT(ShaderConstants._BloomRT4x0);
                cmd.ReleaseTemporaryRT(ShaderConstants._BloomRT8x0);
                cmd.ReleaseTemporaryRT(ShaderConstants._BloomRT8x1);
            }

            RenderTextureDescriptor GetDownSampleDescriptor(int downSample)
            {
                var desc = m_Descriptor;
                desc.depthBufferBits = 0;
                desc.msaaSamples = 1;
                desc.width = Screen.width >> downSample;
                desc.height = Screen.height >> downSample;
                desc.graphicsFormat = m_DefaultHDRFormat;
                return desc;
            }

            void RenderDownSampleAndBlur(int downSample, int blurPass, int src, int des0, int des1)
            {
                RenderTextureDescriptor desc = GetDownSampleDescriptor(downSample);
                cmd.GetTemporaryRT(des0, desc, FilterMode.Bilinear);
                cmd.GetTemporaryRT(des1, desc, FilterMode.Bilinear);
                // Down sample
                Blit(cmd, src, des0, bloomMaterial, Bloom.Pass.FastDownSample);
                // Blur
                cmd.SetGlobalVector(ShaderConstants._Axis, horizontal);
                Blit(cmd, des0, des1, bloomMaterial, blurPass);
                cmd.SetGlobalVector(ShaderConstants._Axis, vertical);
                Blit(cmd, des1, des0, bloomMaterial, blurPass);
            }
        }
        #endregion

        #region GodRay

        bool DoGodRay(Camera camera, CommandBuffer cmd, int source, Material uberMaterial)
        {
#if UNITY_EDITOR
            if (m_GodRay.LightTransform != null)
            {
                Transform trans = m_GodRay.LightTransform.GetObjectByName();
                if (trans != null)
                {
                    m_GodRay.LightDir.value = trans.forward;
                    m_GodRay.LightPosition.value = trans.position;
                }
            }
#endif
            int wh = (int)(m_Descriptor.width / Mathf.Pow(2.0f, m_GodRay.DownSample.value));
            int hh = (int)(m_Descriptor.height / Mathf.Pow(2.0f, m_GodRay.DownSample.value));

            var material = m_Materials.godRay;

            Vector3 sunDir = m_GodRay.LightDir.value;
            sunDir.y += m_GodRay.Bias.value;
            sunDir.Normalize();

            var invLook = -camera.transform.forward;
            float lookLightAngle = Dot(invLook, sunDir);

            bool hasGodray = lookLightAngle > 0;
            if (hasGodray)
            {
                //Vector3 viewPortLightPos = camera.WorldToViewportPoint(m_GodRay.LightPosition.value) + new Vector3(0, m_GodRay.Bias.value, 0);
                Vector3 viewPortLightPos = camera.WorldToViewportPoint(sunDir * 1e8f);

                material.SetVector(ShaderConstants._GodRayViewPortLightPos, viewPortLightPos);
                material.SetVector(ShaderConstants._GodRayParam, new Vector4(m_GodRay.Threshold.value, m_GodRay.LinearDistance.value, m_GodRay.Power.value, m_GodRay.Radius.value));
                material.SetVector(ShaderConstants._GodRayParam1, new Vector4(m_GodRay.MaxPower.value, 0, 0, 0));
                material.SetColor(ShaderConstants._GodRayColor, m_GodRay.color.value);

                cmd.GetTemporaryRT(ShaderConstants._GodRayTex, GetCompatibleDescriptor(wh, hh, m_DefaultHDRFormat), FilterMode.Bilinear);
                cmd.GetTemporaryRT(ShaderConstants._PingTexture, GetCompatibleDescriptor(wh, hh, m_DefaultHDRFormat), FilterMode.Bilinear);

                if (m_GodRay.UseNoise.value)
                {
                    material.EnableKeyword(ShaderKeywordStrings.GodRayUseNoise);
                }
                else
                {
                    material.DisableKeyword(ShaderKeywordStrings.GodRayUseNoise);
                }
                Blit(cmd, source, ShaderConstants._GodRayTex, material, 0);
                int count = m_GodRay.BlurTimes.value;
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        float offset = m_GodRay.Offset.value * (i * 2 + 1);
                        material.SetVector(ShaderConstants._GodRayOffset, new Vector4(offset, offset, 0));
                        Blit(cmd, ShaderConstants._GodRayTex, ShaderConstants._PingTexture, material, 1);

                        offset = m_GodRay.Offset.value * (i * 2 + 2);
                        material.SetVector(ShaderConstants._GodRayOffset, new Vector4(offset, offset, 0, 0));
                        Blit(cmd, ShaderConstants._PingTexture, ShaderConstants._GodRayTex, material, 1);
                    }
                }
                cmd.ReleaseTemporaryRT(ShaderConstants._PingTexture);
                cmd.SetGlobalTexture(ShaderConstants._GodRayTex, ShaderConstants._GodRayTex);

                uberMaterial.SetFloat(ShaderConstants._GodRayIntensity, m_GodRay.Intensity.value);
                uberMaterial.EnableKeyword(ShaderKeywordStrings.GodRay);
                return true;
            }
            return false;
        }

        float Dot(Vector3 v0, Vector3 v1)
        {
            return v0.x * v1.x + v0.y * v1.y + v0.z * v1.z;
        }
        #endregion

        #region RadialBlur

        bool SetupRadialBlur(RadialBlurParam param, Camera camera, CommandBuffer cmd, int source, Material uberMaterial)
        {
            bool Culling(ref RadialBlurParam radialBlur, ref Vector3 screenPos, ref Vector3 cameraPos, RadialBlurQuality quality)
            {
                // 基本参数剔除
                if (radialBlur.intensity == 0 || radialBlur.innerRadius > radialBlur.outerRadius)
                {
                    return true;
                }

                // 距离剔除
                float sqrDistance = Vector3.SqrMagnitude(radialBlur.center - cameraPos);
                if (sqrDistance > quality.cullDistance * quality.cullDistance)
                {
                    return true;
                }

                // 视锥剔除 z
                if (screenPos.z < 0)
                {
                    return true;
                }

                // 视锥剔除 x
                float width = radialBlur.outerRadius * m_Descriptor.height / m_Descriptor.width * radialBlur.size / screenPos.z;
                float x01 = screenPos.x / m_Descriptor.width;
                if (x01 - width > 1f || x01 + width < 0f)
                {
                    return true;
                }

                // 视锥剔除 y
                float height = radialBlur.outerRadius * radialBlur.size / screenPos.z;
                float y01 = screenPos.y / m_Descriptor.height;
                if (y01 - height > 1f || y01 + height < 0f)
                {
                    return true;
                }

                return false;
            }

            if (!param.active)
                return false;

            RadialBlurQuality quality = RenderLevelManager.GetCurrentRadialBlurQuality();

            Vector3 cameraPos = camera.transform.position;
            Vector3 screenPos;
            float screenSize;
            if (param.useScreenPos)
            {
                Vector3 center = param.center;
                screenPos = new Vector3(m_Descriptor.width * center.x, m_Descriptor.height * center.y, center.z);
                screenSize = param.size;
            }
            else
            {
                screenPos = camera.WorldToScreenPoint(param.center);
                if (Culling(ref param, ref screenPos, ref cameraPos, quality))
                {
                    return false;
                }
                Vector3 screenOffsetPos = camera.WorldToScreenPoint(param.center + Vector3.up * param.size);
                screenSize = -0.5f * (screenOffsetPos - screenPos).magnitude / m_Descriptor.height;
            }

            #region Prepare parameters

            Vector4 params0 = default;
            params0.x = m_Descriptor.width / (float)m_Descriptor.height / screenSize;
            params0.y = 1 / screenSize;
            params0.z = m_Descriptor.height / (float)m_Descriptor.width;
            params0.w = 1;

            float fadeOutProgress = Mathf.Clamp01(Vector3.Distance(param.center, cameraPos) / quality.cullDistance);
            float distanceFadeout = quality.fadeOutCurve.Evaluate(fadeOutProgress);
            float innerInvFadeout = 1 / (param.innerFadeOut + 0.0001f);
            float outerInvFadeout = 1 / (param.outerFadeOut + 0.0001f);
            Vector4 params1 = new Vector4(
                innerInvFadeout,
                -outerInvFadeout,
                -param.innerRadius * innerInvFadeout,
                param.outerRadius * outerInvFadeout
            );

            Vector4 params2x0 = new Vector4(
                -screenPos.x / m_Descriptor.height / screenSize,
                -screenPos.y / m_Descriptor.height / screenSize,
                param.intensity / (screenPos.z == 0 ? 0.001f : screenPos.z),
                0
            );

            Vector4 params2x1 = params2x0;
            params2x1.z /= URPRadialBlur.BLUR_SAMPLE_COUNT;

            #endregion

            #region Rendering

            Material material = m_Materials.radialBlur;
            cmd.SetGlobalVector(ShaderConstants._RadialBlurParam0, params0);
            cmd.SetGlobalVector(ShaderConstants._RadialBlurParam1, params1);
            cmd.SetGlobalVector(ShaderConstants._RadialBlurParam2, params2x0);

            bool lowLod = screenSize < quality.lodSize;
            bool blurOnce = quality.times == PPRadialBlurTimes.One || lowLod;
            // int width = m_Descriptor.width >> quality.downSample;
            // int height = m_Descriptor.height >> quality.downSample;

            // cmd.GetTemporaryRT(ShaderConstants._RadialBlurTex, width, height, 0, FilterMode.Bilinear);
            // cmd.GetTemporaryRT(ShaderConstants._RadialBlurTempTex, width, height, 0, FilterMode.Bilinear);

            int radialBlurD = _ppRTManager.GetTemporary(cmd, quality.downSample);
            int radialBlurDP = _ppRTManager.GetTemporary(cmd, quality.downSample, true);
            // TODO: Avoid downdsample pass by use rt of dof. 
            if (blurOnce)
            {
                Blit(cmd, source, radialBlurDP, material, ShaderConstants.RadialBlurDownsamplePass);
                Blit(cmd, radialBlurDP, radialBlurD, material, ShaderConstants.RadialBlurMaskAndBlurPass);
            }
            else
            {
                Blit(cmd, source, radialBlurD, material, ShaderConstants.RadialBlurDownsamplePass);
                Blit(cmd, radialBlurD, radialBlurDP, material, ShaderConstants.RadialBlurMaskAndBlurPass);
                cmd.SetGlobalVector(ShaderConstants._RadialBlurParam2, params2x1);
                Blit(cmd, radialBlurDP, radialBlurD, material, ShaderConstants.RadialBlurBlurPass);
            }
            cmd.SetGlobalTexture(ShaderConstants._RadialBlurTex, radialBlurD);
            // cmd.ReleaseTemporaryRT(ShaderConstants._RadialBlurTempTex);

            uberMaterial.EnableKeyword(ShaderKeywordStrings.RadialBlur);
            #endregion

            return true;
        }

        #endregion

        #region Lens Distortion

        void SetupLensDistortion(Material material, bool isSceneView)
        {
            float amount = 1.6f * Mathf.Max(Mathf.Abs(m_LensDistortion.intensity.value * 100f), 1f);
            float theta = Mathf.Deg2Rad * Mathf.Min(160f, amount);
            float sigma = 2f * Mathf.Tan(theta * 0.5f);
            var center = m_LensDistortion.center.value * 2f - Vector2.one;
            var p1 = new Vector4(
                center.x,
                center.y,
                Mathf.Max(m_LensDistortion.xMultiplier.value, 1e-4f),
                Mathf.Max(m_LensDistortion.yMultiplier.value, 1e-4f)
            );
            var p2 = new Vector4(
                m_LensDistortion.intensity.value >= 0f ? theta : 1f / theta,
                sigma,
                1f / m_LensDistortion.scale.value,
                m_LensDistortion.intensity.value * 100f
            );

            material.SetVector(ShaderConstants._Distortion_Params1, p1);
            material.SetVector(ShaderConstants._Distortion_Params2, p2);

            if (m_LensDistortion.IsActive() && !isSceneView)
                material.EnableKeyword(ShaderKeywordStrings.Distortion);
        }

        #endregion

        #region Chromatic Aberration

        void SetupChromaticAberration(Material material)
        {
            material.SetFloat(ShaderConstants._Chroma_Params, m_ChromaticAberration.intensity.value * 0.05f);

            if (m_ChromaticAberration.IsActive())
                material.EnableKeyword(ShaderKeywordStrings.ChromaticAberration);
        }

        #endregion

        #region Vignette

        void SetupVignette(Material material)
        {
#if USE_URP_VIGNETTE
        URPVignette(material);
#else
            material.SetColor(ShaderConstants._Vignette_Color, m_Vignette.color.value);
            material.SetVector(ShaderConstants._Vignette_Center, m_Vignette.center.value);
            Vector4 settings = new Vector4(
                m_Vignette.intensity.value * 3f,
                m_Vignette.smoothness.value * 5f,
                (1f - m_Vignette.roundness.value) * 6f + m_Vignette.roundness.value,
                m_Vignette.rounded.value ? 1f : 0f);
            material.SetVector(ShaderConstants._Vignette_Settings, settings);
#endif
        }

        void URPVignette(Material material)
        {
            var color = m_Vignette.color.value;
            var center = m_Vignette.center.value;
            var aspectRatio = m_Descriptor.width / (float)m_Descriptor.height;

            var v1 = new Vector4(
                color.r, color.g, color.b,
                m_Vignette.rounded.value ? aspectRatio : 1f
            );
            var v2 = new Vector4(
                center.x, center.y,
                m_Vignette.intensity.value * 3f,
                m_Vignette.smoothness.value * 5f
            );

            material.SetVector(ShaderConstants._Vignette_Params1, v1);
            material.SetVector(ShaderConstants._Vignette_Params2, v2);
        }

        #endregion

        #region Color Grading

        void SetupColorGrading(CommandBuffer cmd, ref RenderingData renderingData, Material material)
        {
            ref var postProcessingData = ref renderingData.postProcessingData;
            bool hdr = postProcessingData.gradingMode == ColorGradingMode.HighDynamicRange;
            int lutHeight = postProcessingData.lutSize;
            int lutWidth = lutHeight * lutHeight;

            // Source material setup
            float postExposureLinear = Mathf.Pow(2f, m_ColorAdjustments.postExposure.value);
            //cmd.SetGlobalTexture(ShaderConstants._InternalLut, m_InternalLut.Identifier());
            material.SetVector(ShaderConstants._Lut_Params, new Vector4(1f / lutWidth, 1f / lutHeight, lutHeight - 1f, postExposureLinear));
            material.SetTexture(ShaderConstants._UserLut, m_ColorLookup.texture.value);
            material.SetVector(ShaderConstants._UserLut_Params, !m_ColorLookup.IsActive()
                ? Vector4.zero
                : new Vector4(1f / m_ColorLookup.texture.value.width,
                              1f / m_ColorLookup.texture.value.height,
                              m_ColorLookup.texture.value.height - 1f,
                              m_ColorLookup.contribution.value)
            );

            if (hdr)
            {
                material.EnableKeyword(ShaderKeywordStrings.HDRGrading);
            }
            else
            {
                switch (m_Tonemapping.mode.value)
                {
                    case TonemappingMode.Neutral:
                        material.EnableKeyword(ShaderKeywordStrings.TonemapNeutral);
                        break;
                    case TonemappingMode.ACES:
                        material.EnableKeyword(ShaderKeywordStrings.TonemapACES);
                        break;
                    case TonemappingMode.Custom:
                        material.EnableKeyword(ShaderKeywordStrings.TonemapCustom);
                        UnityCustomTonemapping(m_Tonemapping, material);
                        break;
                    default: break; // None
                }
            }
        }

        private void UnityCustomTonemapping(Tonemapping tonemapping, Material material)
        {
            m_HableCurve.Init(
                tonemapping.toneCurveToeStrength.value,
                tonemapping.toneCurveToeLength.value,
                tonemapping.toneCurveShoulderStrength.value,
                tonemapping.toneCurveShoulderLength.value,
                tonemapping.toneCurveShoulderAngle.value,
                tonemapping.toneCurveGamma.value
            );

            Shader.SetGlobalVector(ShaderConstants._CustomToneCurve, m_HableCurve.uniforms.curve);
            Shader.SetGlobalVector(ShaderConstants._ToeSegmentA, m_HableCurve.uniforms.toeSegmentA);
            Shader.SetGlobalVector(ShaderConstants._ToeSegmentB, m_HableCurve.uniforms.toeSegmentB);
            Shader.SetGlobalVector(ShaderConstants._MidSegmentA, m_HableCurve.uniforms.midSegmentA);
            Shader.SetGlobalVector(ShaderConstants._MidSegmentB, m_HableCurve.uniforms.midSegmentB);
            Shader.SetGlobalVector(ShaderConstants._ShoSegmentA, m_HableCurve.uniforms.shoSegmentA);
            Shader.SetGlobalVector(ShaderConstants._ShoSegmentB, m_HableCurve.uniforms.shoSegmentB);
        }

        #endregion

        #region Film Grain

        void SetupGrain(in CameraData cameraData, Material material)
        {
            if (!m_HasFinalPass && m_FilmGrain.IsActive())
            {
                material.EnableKeyword(ShaderKeywordStrings.FilmGrain);
                PostProcessUtils.ConfigureFilmGrain(
                    m_Data,
                    m_FilmGrain,
                    cameraData.pixelWidth, cameraData.pixelHeight,
                    material
                );
            }
        }

        #endregion

        #region 8-bit Dithering

        void SetupDithering(in CameraData cameraData, Material material)
        {
            if (!m_HasFinalPass && cameraData.isDitheringEnabled)
            {
                material.EnableKeyword(ShaderKeywordStrings.Dithering);
                m_DitheringTextureIndex = PostProcessUtils.ConfigureDithering(
                    m_Data,
                    m_DitheringTextureIndex,
                    cameraData.pixelWidth, cameraData.pixelHeight,
                    material
                );
            }
        }

        #endregion

        #region Final pass

        void RenderFinalPass(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;
            var material = m_Materials.finalPass;
            material.shaderKeywords = null;

            // FXAA setup
            //if (cameraData.antialiasing == AntialiasingMode.FastApproximateAntialiasing)
            //if (cameraData.antialiasing == AntialiasingMode.FastApproximateAntialiasing && PostProcessSetting.Fxaa)
            //material.EnableKeyword(ShaderKeywordStrings.Fxaa);

            PostProcessUtils.SetSourceSize(cmd, cameraData.cameraTargetDescriptor);

            SetupGrain(cameraData, material);
            SetupDithering(cameraData, material);

            /* Add By:  Takeshi
             * Apply:   This is the final Process of Fixing UI Alpha Gamma (in case of Final Post)
             *          make final image return to Linear Color, when the UI images are already blended alpha with the image of 3D render
             * Note:    Judge the camera if it is used in UI-Scene(小黑屋) Render,
             *          and it will transform image color to sRGB color in case of render a UI-Scene,
             *          because the render image of UI-Scene is a UI image. and it is not the final process,
             *          we will process it just like the first process of Fix UI Alpha Gamma.
             */

            if (cameraData.camera.gameObject.layer == LayerMaskName.UIScene)/* In case of render UI-Scene 渲染小黑屋的情况 */
            // if((cameraData.camera.gameObject.layer | LayerMaskName.UIScene)!=0)
            {
                if (GameQualitySetting.ToneMapping)
                {
                    material.EnableKeyword(ShaderKeywordStrings.LinearToSRGBConversion);
                }
            }
            else if (cameraData.camera.gameObject.layer == LayerMaskName.UI)/* In case of Final Process 渲染UI相机的情况 */
            {
                //现在走不到这里了。。。。。。。。。。。。。。。。。。
                //if (RenderLevelManager.PostProcessRenderLevel > RenderQualityLevel.VeryLow)
                //{
                //    material.EnableKeyword(ShaderKeywordStrings.SRGBToLinearConversion);
                //}
            }
            /* End Add */

            cmd.SetGlobalTexture(ShaderPropertyId.sourceTex, m_Source.Identifier());

            var colorLoadAction = cameraData.isDefaultViewport ? RenderBufferLoadAction.DontCare : RenderBufferLoadAction.Load;

            RenderTargetHandle cameraTargetHandle = RenderTargetHandle.GetCameraTarget(cameraData.xr);

#if ENABLE_VR && ENABLE_XR_MODULE
            if (cameraData.xr.enabled)
            {
                RenderTargetIdentifier cameraTarget = cameraTargetHandle.Identifier();

                //Blit(cmd, m_Source.Identifier(), BuiltinRenderTextureType.CurrentActive, material);
                bool isRenderToBackBufferTarget = cameraTarget == cameraData.xr.renderTarget && !cameraData.xr.renderTargetIsRenderTexture;
                // We y-flip if
                // 1) we are bliting from render texture to back buffer and
                // 2) renderTexture starts UV at top
                bool yflip = isRenderToBackBufferTarget && SystemInfo.graphicsUVStartsAtTop;

                Vector4 scaleBias = yflip ? new Vector4(1, -1, 0, 1) : new Vector4(1, 1, 0, 0);

                cmd.SetRenderTarget(new RenderTargetIdentifier(cameraTarget, 0, CubemapFace.Unknown, -1),
                    colorLoadAction, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
                cmd.SetViewport(cameraData.pixelRect);
                cmd.SetGlobalVector(ShaderPropertyId.scaleBias, scaleBias);
                cmd.DrawProcedural(Matrix4x4.identity, material, 0, MeshTopology.Quads, 4, 1, null);
            }
            else
#endif
            {
                // Note: We need to get the cameraData.targetTexture as this will get the targetTexture of the camera stack.
                // Overlay cameras need to output to the target described in the base camera while doing camera stack.
                RenderTexture target = cameraData.targetTexture;
                RenderTargetIdentifier cameraTarget = (target != null) ? new RenderTargetIdentifier(target) : cameraTargetHandle.Identifier();
                cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);

                //---统一AA修改：程庆宝
                //cmd.SetRenderTarget(cameraTarget, colorLoadAction, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
                //cmd.SetViewport(cameraData.pixelRect);
                //cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, material);

                RenderTextureDescriptor targetDescriptor = (target != null) ? target.descriptor : m_Descriptor;
                SetupAA(material, cameraData, targetDescriptor, cmd, colorLoadAction, m_Source.Identifier(), cameraTarget);

                cmd.SetViewProjectionMatrices(cameraData.camera.worldToCameraMatrix, cameraData.camera.projectionMatrix);
            }
        }

        #endregion

        #region Internal utilities

        class MaterialLibrary
        {
            public readonly Material stopNaN;
            public readonly Material radialBlur;
            public readonly Material subpixelMorphologicalAntialiasing;
            public readonly Material gaussianDepthOfField;
            public readonly Material bokehDepthOfField;
            public readonly Material easyDepthOfField;
            public readonly Material cameraMotionBlur;
            public readonly Material paniniProjection;
            public readonly Material bloom;
            public readonly Material godRay;
            public readonly Material uber;
            public readonly Material finalPass;

            public MaterialLibrary(PostProcessData data)
            {
                stopNaN = Load(data.shaders.stopNanPS);
                radialBlur = Load(data.shaders.radialBlur);
                subpixelMorphologicalAntialiasing = Load(data.shaders.subpixelMorphologicalAntialiasingPS);
                gaussianDepthOfField = Load(data.shaders.gaussianDepthOfFieldPS);
                bokehDepthOfField = Load(data.shaders.bokehDepthOfFieldPS);
                easyDepthOfField = Load(data.shaders.easyDepthOfFieldPS);
                cameraMotionBlur = Load(data.shaders.cameraMotionBlurPS);
                paniniProjection = Load(data.shaders.paniniProjectionPS);
                bloom = Load(data.shaders.bloomPS);
                godRay = Load(data.shaders.godRayPS);
                uber = Load(data.shaders.uberPostPS);
                finalPass = Load(data.shaders.finalPostPassPS);
            }

            Material Load(Shader shader)
            {
                if (shader == null)
                {
                    Debug.LogErrorFormat($"Missing shader. {GetType().DeclaringType.Name} render pass will not execute. Check for missing reference in the renderer resources.");
                    return null;
                }
                else if (!shader.isSupported)
                {
                    return null;
                }

                return CoreUtils.CreateEngineMaterial(shader);
            }

            internal void Cleanup()
            {
                CoreUtils.Destroy(stopNaN);
                CoreUtils.Destroy(subpixelMorphologicalAntialiasing);
                CoreUtils.Destroy(gaussianDepthOfField);
                CoreUtils.Destroy(bokehDepthOfField);
                CoreUtils.Destroy(easyDepthOfField);
                CoreUtils.Destroy(cameraMotionBlur);
                CoreUtils.Destroy(paniniProjection);
                CoreUtils.Destroy(bloom);
                CoreUtils.Destroy(uber);
                CoreUtils.Destroy(finalPass);
            }
        }

        // Precomputed shader ids to same some CPU cycles (mostly affects mobile)
        static class ShaderConstants
        {
            public static readonly int MainTex = Shader.PropertyToID("_MainTex");
            public static readonly int _TempTarget = Shader.PropertyToID("_TempTarget");
            public static readonly int _TempTarget2 = Shader.PropertyToID("_TempTarget2");

            public static readonly int _StencilRef = Shader.PropertyToID("_StencilRef");
            public static readonly int _StencilMask = Shader.PropertyToID("_StencilMask");

            public static readonly int _FullCoCTexture = Shader.PropertyToID("_FullCoCTexture");
            public static readonly int _HalfCoCTexture = Shader.PropertyToID("_HalfCoCTexture");
            public static readonly int _DofTexture = Shader.PropertyToID("_DofTexture");
            public static readonly int _CoCParams = Shader.PropertyToID("_CoCParams");
            public static readonly int _CoCParams2 = Shader.PropertyToID("_CoCParams2");
            
            
            public static readonly int _BokehKernel = Shader.PropertyToID("_BokehKernel");
            public static readonly int _PongTexture = Shader.PropertyToID("_PongTexture");
            public static readonly int _PingTexture = Shader.PropertyToID("_PingTexture");
            public static readonly int _DofRT = Shader.PropertyToID("_DofRT");
            public static readonly int _DofTex = Shader.PropertyToID("_DofTex");
            public static readonly int downSampleX4 = Shader.PropertyToID("_RT_DOF_TEMP");

            public static readonly int _Metrics = Shader.PropertyToID("_Metrics");
            public static readonly int _AreaTexture = Shader.PropertyToID("_AreaTexture");
            public static readonly int _SearchTexture = Shader.PropertyToID("_SearchTexture");
            public static readonly int _EdgeTexture = Shader.PropertyToID("_EdgeTexture");
            public static readonly int _BlendTexture = Shader.PropertyToID("_BlendTexture");

            public static readonly int _ColorTexture = Shader.PropertyToID("_ColorTexture");
            public static readonly int _Params = Shader.PropertyToID("_Params");
            public static readonly int _SourceTexLowMip = Shader.PropertyToID("_SourceTexLowMip");
            public static readonly int _Bloom_Params = Shader.PropertyToID("_Bloom_Params");
            public static readonly int _Bloom_RGBM = Shader.PropertyToID("_Bloom_RGBM");
            public static readonly int _Bloom_Texture = Shader.PropertyToID("_Bloom_Texture");
            public static readonly int _Bloom_DirectThreshold = Shader.PropertyToID("_Bloom_DirectThreshold");
            public static readonly int _LensDirt_Texture = Shader.PropertyToID("_LensDirt_Texture");
            public static readonly int _LensDirt_Params = Shader.PropertyToID("_LensDirt_Params");
            public static readonly int _LensDirt_Intensity = Shader.PropertyToID("_LensDirt_Intensity");
            public static readonly int _Distortion_Params1 = Shader.PropertyToID("_Distortion_Params1");
            public static readonly int _Distortion_Params2 = Shader.PropertyToID("_Distortion_Params2");
            public static readonly int _Chroma_Params = Shader.PropertyToID("_Chroma_Params");
            public static readonly int _Vignette_Params1 = Shader.PropertyToID("_Vignette_Params1");
            public static readonly int _Vignette_Params2 = Shader.PropertyToID("_Vignette_Params2");
            public static readonly int _Vignette_Color = Shader.PropertyToID("_Vignette_Color");
            public static readonly int _Vignette_Center = Shader.PropertyToID("_Vignette_Center");
            public static readonly int _Vignette_Settings = Shader.PropertyToID("_Vignette_Settings");
            public static readonly int _Lut_Params = Shader.PropertyToID("_Lut_Params");
            public static readonly int _UserLut_Params = Shader.PropertyToID("_UserLut_Params");
            public static readonly int _UserLut = Shader.PropertyToID("_UserLut");

            public static readonly int _DownSampleScaleFactor = Shader.PropertyToID("_DownSampleScaleFactor");
            public static readonly int _GodRayParam = Shader.PropertyToID("_GodRayParam");
            public static readonly int _GodRayParam1 = Shader.PropertyToID("_GodRayParam1");
            public static readonly int _GodRayViewPortLightPos = Shader.PropertyToID("_GodRayViewPortLightPos");
            public static readonly int _GodRayOffset = Shader.PropertyToID("_GodRayOffset");
            public static readonly int _GodRayColor = Shader.PropertyToID("_GodRayColor");
            public static readonly int _GodRayTex = Shader.PropertyToID("_GodRayTex");
            public static readonly int _GodRayIntensity = Shader.PropertyToID("_GodRayIntensity");

            public static readonly int _RadialBlurParam0 = Shader.PropertyToID("_RadialBlurParam0");
            public static readonly int _RadialBlurParam1 = Shader.PropertyToID("_RadialBlurParam1");
            public static readonly int _RadialBlurParam2 = Shader.PropertyToID("_RadialBlurParam2");
            public static readonly int _RadialBlurTex = Shader.PropertyToID("_RadialBlurTex");
            public static readonly int _RadialBlurTempTex = Shader.PropertyToID("_RadialBlurTempTex");

            public static readonly int _CustomToneCurve = Shader.PropertyToID("_CustomToneCurve");
            public static readonly int _ToeSegmentA = Shader.PropertyToID("_ToeSegmentA");
            public static readonly int _ToeSegmentB = Shader.PropertyToID("_ToeSegmentB");
            public static readonly int _MidSegmentA = Shader.PropertyToID("_MidSegmentA");
            public static readonly int _MidSegmentB = Shader.PropertyToID("_MidSegmentB");
            public static readonly int _ShoSegmentA = Shader.PropertyToID("_ShoSegmentA");
            public static readonly int _ShoSegmentB = Shader.PropertyToID("_ShoSegmentB");

            public static readonly int _FullscreenProjMat = Shader.PropertyToID("_FullscreenProjMat");

            public static readonly int _BloomBalance = Shader.PropertyToID("_BloomOffsetStrength");
            public static readonly int _BloomParam = Shader.PropertyToID("_BloomParam");
            public static readonly int _BloomRT4x0 = Shader.PropertyToID("_BloomRT4x0");
            public static readonly int _BloomRT4x1 = Shader.PropertyToID("_BloomRT4x1");
            public static readonly int _BloomRT8x0 = Shader.PropertyToID("_BloomRT8x0");
            public static readonly int _BloomRT8x1 = Shader.PropertyToID("_BloomRT8x1");
            public static readonly int _BloomRT16x0 = Shader.PropertyToID("_BloomRT16x0");
            public static readonly int _BloomRT16x1 = Shader.PropertyToID("_BloomRT16x1");
            public static readonly int _BloomRT32x0 = Shader.PropertyToID("_BloomRT32x0");
            public static readonly int _BloomRT32x1 = Shader.PropertyToID("_BloomRT32x1");
            public static readonly int _BloomRT64x0 = Shader.PropertyToID("_BloomRT64x0");
            public static readonly int _BloomRT64x1 = Shader.PropertyToID("_BloomRT64x1");
            public static readonly int _BloomColor = Shader.PropertyToID("_BloomColor");
            public static readonly int _Axis = Shader.PropertyToID("_Axis");
            public static readonly int _Blend = Shader.PropertyToID("_Blend");
            public static readonly int _MainTex0 = Shader.PropertyToID("_MainTex0");
            public static readonly int _MainTex1 = Shader.PropertyToID("_MainTex1");
            public static readonly int _MainTex2 = Shader.PropertyToID("_MainTex2");
            public static readonly int _MainTex3 = Shader.PropertyToID("_MainTex3");

            public static int[] _BloomMipUp;
            public static int[] _BloomMipDown;

            public const int RadialBlurDownsamplePass = 0;
            public const int RadialBlurMaskAndBlurPass = 1;
            public const int RadialBlurBlurPass = 2;
        }

        class PostProcessRTManager
        {
            private int originalWidth;
            private int originalHeight;
            // private RenderTexture[] rtPacks;
            // private RenderTexture[] rtPacksPong;

            private static RTPair[] rtPacksID;
            private static RTPair[] rtPacksPongID;
            
            private int maxLevel;

            struct RTPair
            {
                public bool hasInit;
                public int id;
            }
            public PostProcessRTManager(int l, int width, int height)
            {
                originalWidth = width;
                originalHeight = height;
                maxLevel = l;
                // rtPacks = new RenderTexture[maxLevel];
                // rtPacksPong = new RenderTexture[maxLevel];
                rtPacksID = new RTPair[maxLevel];
                rtPacksPongID = new RTPair[maxLevel];
                for (int i = 0; i < maxLevel; i++)
                {
                    rtPacksID[i].hasInit = false;
                    rtPacksPongID[i].hasInit = false;
                    rtPacksID[i].id = Shader.PropertyToID($"_PPPublicRT_D{i}");
                    
                    rtPacksPongID[i].id = Shader.PropertyToID($"_PPPublicRT_Pong_D{i}");
                }
            }

            public int GetTemporaryTransfer(CommandBuffer cmd, RenderTextureDescriptor textureDescriptor,
                bool pong = false)
            {
                int scale = originalHeight / textureDescriptor.height;
                int down = 0;
                while (scale / 2 >= 1)
                {
                    down++;
                    scale /= 2;
                }
                return GetTemporary(cmd, down, pong);
            }
            public int GetTemporary(CommandBuffer cmd, int downsample = 0, bool pong = false)
            {
                if (downsample > maxLevel)
                {
                    DebugLog.AddErrorLog("声明RT降采样级数超过限制");
                    return -1;
                }
                else
                {
                    if (pong)
                    {
                        if (!rtPacksPongID[downsample].hasInit)
                        {
                            cmd.GetTemporaryRT(rtPacksPongID[downsample].id,originalWidth>>downsample, originalHeight>>downsample, 0, FilterMode.Bilinear, GraphicsFormat.R8G8B8A8_SRGB);
                            rtPacksPongID[downsample].hasInit = true;
                        }
                        return rtPacksPongID[downsample].id;
                    }
                    else
                    {
                        if (!rtPacksID[downsample].hasInit)
                        {
                            cmd.GetTemporaryRT(rtPacksID[downsample].id,originalWidth>>downsample, originalHeight>>downsample, 0, FilterMode.Bilinear, GraphicsFormat.R8G8B8A8_SRGB);
                            rtPacksID[downsample].hasInit = true;
                        }
                        return rtPacksID[downsample].id;
                    }
                }
            }

            public void Release(CommandBuffer cmd)
            {
                for (int i = 0; i < maxLevel; i++)
                {
                   cmd.ReleaseTemporaryRT(rtPacksID[i].id);
                   rtPacksID[i].hasInit = false;
                   cmd.ReleaseTemporaryRT(rtPacksPongID[i].id);
                   rtPacksPongID[i].hasInit = false;
                }
            }
        }
        #endregion
    }
}
