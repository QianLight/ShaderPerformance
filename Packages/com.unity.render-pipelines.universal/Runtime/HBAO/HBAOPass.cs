using System;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using UnityEngine.Profiling;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal.Internal
{
    /// <summary>
    /// Draw  objects into the given color and depth target
    ///
    /// You can use this pass to render objects that have a material and/or shader
    /// with the pass names UniversalForward or SRPDefaultUnlit.
    /// </summary>
    public class HBAOPass : ScriptableRenderPass
    {
        private RenderTargetHandle m_Color { get; set; }
        private RenderTargetHandle m_Depth { get; set; }
        private RenderTargetIdentifier m_Source { get; set; }

        Material m_Material;
        Texture2D m_NoiseTexture;
        HBAOSetting m_Setting;
        RenderTextureDescriptor m_Descriptor;
        Camera m_Camera;
        Rect m_CameraRect;
        public HBAOPass(RenderPassEvent evt, PostProcessData data)
        {
            base.profilingSampler = new ProfilingSampler(nameof(HBAOPass));
            Material Load(Shader shader)
            {
                if (shader == null)
                {
                    Debug.LogError($"Missing shader. {GetType().DeclaringType.Name} render pass will not execute. Check for missing reference in the renderer resources.");
                    return null;
                }

                return CoreUtils.CreateEngineMaterial(shader);
            }

            m_Material = Load(data.shaders.hbaoPS);
            m_NoiseTexture = data.textures.hbaoNoise;
            renderPassEvent = evt;
            overrideCameraTarget = true;
        }

        /// <summary>
        /// Configure the pass with the source and destination to execute on.
        /// </summary>
        /// <param name="source">Source Render Target</param>
        /// <param name="destination">Destination Render Targt</param>
        public void Setup(RenderTargetHandle color, RenderTargetHandle depth, RenderTargetIdentifier source, HBAOSetting setting)
        {
            m_Color = color;
            m_Depth = depth;
            m_Source = source;
            m_Setting = setting;
        }
        
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {

        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (m_Material == null)
            {
                Debug.LogErrorFormat("Missing {0}. {1} render pass will not execute. Check for missing reference in the renderer resources.", m_Material, GetType().Name);
                return;
            }
            CameraData cameraData = renderingData.cameraData;
            m_Camera = cameraData.camera;
            m_Descriptor = cameraData.cameraTargetDescriptor;
            m_CameraRect = cameraData.pixelRect;

            int sWidth = (int)m_CameraRect.width;
            int sHeight = (int)m_CameraRect.height;
            int rtWidth = sWidth >> m_Setting.DownSample;
            int rtHeight = sHeight >> m_Setting.DownSample;

            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.HBAO)))
            {
                cmd.GetTemporaryRT(ShaderConstants._HbaoRT, GetCompatibleDescriptor(m_Descriptor, rtWidth, rtHeight, 0), FilterMode.Bilinear);
                cmd.GetTemporaryRT(ShaderConstants._HbaoBlurRT, GetCompatibleDescriptor(m_Descriptor, rtWidth >> 1, rtHeight >> 1, 0), FilterMode.Bilinear);

                // Globals
                float tanHalfFovY = Mathf.Tan(0.5f * m_Camera.fieldOfView * Mathf.Deg2Rad);
                float invFocalLenX = 1.0f / (1.0f / tanHalfFovY * (m_CameraRect.height / m_CameraRect.width));
                float invFocalLenY = 1.0f / (1.0f / tanHalfFovY);

                m_Material.SetTexture(ShaderConstants._HbaoNoiseTexture, m_NoiseTexture);
                m_Material.SetVector(ShaderConstants._HbaoUVToView, new Vector4(2.0f * invFocalLenX, -2.0f * invFocalLenY, -1.0f * invFocalLenX, 1.0f * invFocalLenY));
                m_Material.SetVector(ShaderConstants._HbaoTargetScalPara, new Vector4((m_CameraRect.width + 0.5f) / m_CameraRect.width, (m_CameraRect.height + 0.5f) / m_CameraRect.height, Mathf.Max(m_Setting.Strength, 0.001f), -1 / (m_Setting.RayMarchingRadius * m_Setting.RayMarchingRadius)));
                m_Material.SetVector(ShaderConstants._HbaoAORadius, new Vector4(m_Setting.RayMarchingRadius * 0.5f * (m_CameraRect.height / (tanHalfFovY * 2.0f)), m_Setting.MaxPixelRadius, m_Setting.RayMarchingStepCount, m_Setting.RayMarchingDirectionCount));
                m_Material.SetVector(ShaderConstants._HbaoDistanceFalloff, new Vector4(m_Setting.DistanceFalloff.x, m_Setting.DistanceFalloff.y, m_Setting.AngleBiasValue, 2.0f * (1.0f / (1.0f - m_Setting.AngleBiasValue))));
                m_Material.SetVector(ShaderConstants._HbaoBlurOffset, new Vector4(m_Setting.BlurRadius, 0, 0, m_Setting.BlurRadius * 0.5f));

                cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
                cmd.SetViewport(m_CameraRect);

                cmd.SetRenderTarget(new RenderTargetIdentifier(ShaderConstants._HbaoRT, 0, CubemapFace.Unknown, -1),
                    RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, 
                    RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
                cmd.ClearRenderTarget(true, true, Color.clear);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, m_Material, 0, 0);

                cmd.SetRenderTarget(new RenderTargetIdentifier(ShaderConstants._HbaoBlurRT, 0, CubemapFace.Unknown, -1),
                    RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, 
                    RenderBufferLoadAction.Load, RenderBufferStoreAction.DontCare);
                cmd.SetGlobalTexture(ShaderConstants._ColorTexture, ShaderConstants._HbaoRT);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, m_Material, 0, 1);

                cmd.SetRenderTarget(new RenderTargetIdentifier(ShaderConstants._HbaoRT, 0, CubemapFace.Unknown, -1),
                    RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                    RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
                cmd.SetGlobalTexture(ShaderConstants._ColorTexture, ShaderConstants._HbaoBlurRT);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, m_Material, 0, 2);

                if (m_Depth == RenderTargetHandle.CameraTarget || m_Descriptor.msaaSamples > 1)
                {
                    cmd.SetRenderTarget(m_Color.Identifier(), RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                    RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
                }
                else
                {
                    cmd.SetRenderTarget(m_Color.Identifier(), RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                    m_Depth.Identifier(), RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
                }

                cmd.SetGlobalTexture(ShaderConstants._ColorTexture, m_Source);
                cmd.SetGlobalTexture(ShaderConstants._HbaoRT, ShaderConstants._HbaoRT);
                cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, m_Material, 0, 3);
                cmd.SetViewProjectionMatrices(m_Camera.worldToCameraMatrix, m_Camera.projectionMatrix);

                cmd.ReleaseTemporaryRT(ShaderConstants._HbaoBlurRT);
                cmd.ReleaseTemporaryRT(ShaderConstants._HbaoRT);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        RenderTextureDescriptor GetCompatibleDescriptor(RenderTextureDescriptor m_Descriptor, int width, int height, int depthBufferBits = 0)
        {
            var desc = m_Descriptor;
            desc.depthBufferBits = depthBufferBits;
            desc.msaaSamples = 1;
            desc.width = width;
            desc.height = height;
            return desc;
        }
        /// <inheritdoc/>
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (cmd == null)
                throw new ArgumentNullException("cmd");
        }
        static class ShaderConstants
        {
            public static readonly int _ColorTexture = Shader.PropertyToID("_ColorTexture");
            public static readonly int _HbaoRT = Shader.PropertyToID("_HbaoRT");
            public static readonly int _HbaoBlurRT = Shader.PropertyToID("_HbaoBlurRT");
            public static readonly int _HbaoNoiseTexture = Shader.PropertyToID("_HbaoNoiseTexture");
            public static readonly int _HbaoUVToView = Shader.PropertyToID("_UVToView");
            public static readonly int _HbaoTargetScalPara = Shader.PropertyToID("_TargetScalePara");
            public static readonly int _HbaoAORadius = Shader.PropertyToID("_AORadius");
            public static readonly int _HbaoDistanceFalloff = Shader.PropertyToID("_DistanceFalloff");
            public static readonly int _HbaoBlurOffset = Shader.PropertyToID("_BlurOffset");
        }
    }
}
