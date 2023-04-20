using System;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using UnityEngine.Profiling;

namespace UnityEngine.Rendering.Universal.Internal
{
    /// <summary>
    /// Draw  objects into the given color and depth target
    ///
    /// You can use this pass to render objects that have a material and/or shader
    /// with the pass names UniversalForward or SRPDefaultUnlit.
    /// </summary>
    public class DrawRoleEffectsPass  : ScriptableRenderPass
    {
        FilteringSettings m_FilteringSettings;
        RenderStateBlock m_RenderStateBlock;
        List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();
        List<ShaderTagId> m_ShaderTagIdSingle = new List<ShaderTagId>();
        string m_ProfilerTag;
        ProfilingSampler m_ProfilingSampler;
        bool m_IsOpaque;

        static readonly int s_DrawObjectPassDataPropID = Shader.PropertyToID("_DrawRoleEffectsPass");

        public DrawRoleEffectsPass (string profilerTag, ShaderTagId[] shaderTagIds, bool opaque, RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask, StencilState stencilState, int stencilReference)
        {
            base.profilingSampler = new ProfilingSampler(nameof(DrawRoleEffectsPass ));

            m_ProfilerTag = profilerTag;
            m_ProfilingSampler = new ProfilingSampler(profilerTag);
            foreach (ShaderTagId sid in shaderTagIds)
                m_ShaderTagIdList.Add(sid);
            renderPassEvent = evt;
            m_FilteringSettings = new FilteringSettings(renderQueueRange, layerMask);
            m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
            m_IsOpaque = opaque;

            if (stencilState.enabled)
            {
                m_RenderStateBlock.stencilReference = stencilReference;
                m_RenderStateBlock.mask = RenderStateMask.Stencil;
                m_RenderStateBlock.stencilState = stencilState;
            }
        }

        public DrawRoleEffectsPass (string profilerTag, bool opaque, RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask, StencilState stencilState, int stencilReference, ShaderTagId[] customTagIds = null)
            : this(profilerTag,
                customTagIds ?? new ShaderTagId[] { new ShaderTagId("SRPDefaultUnlit"), new ShaderTagId("UniversalForward"), new ShaderTagId("UniversalForwardOnly"), new ShaderTagId("LightweightForward")},
                opaque, evt, renderQueueRange, layerMask, stencilState, stencilReference)
        {}

        internal DrawRoleEffectsPass (URPProfileId profileId, bool opaque, RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask, StencilState stencilState, int stencilReference, ShaderTagId[] customTagIds = null)
            : this(profileId.GetType().Name, opaque, evt, renderQueueRange, layerMask, stencilState, stencilReference, customTagIds)
        {
            m_ProfilingSampler = ProfilingSampler.Get(profileId);
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // NOTE: Do NOT mix ProfilingScope with named CommandBuffers i.e. CommandBufferPool.Get("name").
            // Currently there's an issue which results in mismatched markers.
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                Camera camera = renderingData.cameraData.camera;
                // Global render pass data containing various settings.
                // x,y,z are currently unused
                // w is used for knowing whether the object is opaque(1) or alpha blended(0)
                Vector4 drawObjectPassData = new Vector4(0.0f, 0.0f, 0.0f, (m_IsOpaque) ? 1.0f : 0.0f);
                cmd.SetGlobalVector(s_DrawObjectPassDataPropID, drawObjectPassData);

                // scaleBias.x = flipSign
                // scaleBias.y = scale
                // scaleBias.z = bias
                // scaleBias.w = unused
                float flipSign = (renderingData.cameraData.IsCameraProjectionMatrixFlipped()) ? -1.0f : 1.0f;
                Vector4 scaleBias = (flipSign < 0.0f)
                    ? new Vector4(flipSign, 1.0f, -1.0f, 1.0f)
                    : new Vector4(flipSign, 0.0f, 1.0f, 1.0f);
                cmd.SetGlobalVector(ShaderPropertyId.scaleBiasRt, scaleBias);

                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                if (renderPassEvent < RenderPassEvent.AfterRenderingOpaques)
                {
                    for (int i = 0; i < m_ShaderTagIdList.Count; i++)
                    {
                        DrawPass(m_ShaderTagIdList[i], context, renderingData, camera);
                    }
                }
                else
                {
                    DrawPass(new ShaderTagId("ScreenSpaceRim"), context, renderingData, camera);
                    DrawPass(new ShaderTagId("OutlineFade"), context, renderingData, camera);
                    
                    if (UniversalRenderPipeline.planarShadowEnabled)
                        DrawPass(new ShaderTagId("PlanarShadow"), context, renderingData, camera);
                }

                // /* Add By: Takeshi; Don't Convert UI obj to Gamma from Linear when it not in a UI camera */
                // cmd.DisableShaderKeyword(ShaderKeywordStrings.IsNotInUICamera);
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        void DrawPass(ShaderTagId tagId, ScriptableRenderContext context, RenderingData renderingData, Camera camera)
        {
            m_ShaderTagIdSingle.Clear();
            m_ShaderTagIdSingle.Add(tagId);
                    
            var sortFlags = (m_IsOpaque) ? renderingData.cameraData.defaultOpaqueSortFlags : SortingCriteria.CommonTransparent;
            var drawSettings = CreateDrawingSettings(m_ShaderTagIdSingle, ref renderingData, sortFlags);

            m_FilteringSettings.renderingLayerMask = renderingData.renderLayerMask;
            var filterSettings = m_FilteringSettings;

#if UNITY_EDITOR
            // When rendering the preview camera, we want the layer mask to be forced to Everything
            if (renderingData.cameraData.isPreviewCamera)
            {
                filterSettings.layerMask = -1;
            }
#endif

            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings, ref m_RenderStateBlock);

            // Render objects that did not match any shader pass with error shader
            RenderingUtils.RenderObjectsWithError(context, ref renderingData.cullResults, camera, filterSettings,
                SortingCriteria.None);
        }
    }
}
