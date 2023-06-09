﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GrassRenderPass : ScriptableRenderPass
{
    FilteringSettings m_FilteringSettings;
    RenderStateBlock m_RenderStateBlock;
    ProfilingSampler m_ProfilingSampler;
    private readonly ShaderTagId m_ShaderTagId;
    bool m_IsOpaque;

    static readonly int s_DrawObjectPassDataPropID = Shader.PropertyToID("_DrawObjectPassData");

    internal GrassRenderPass(URPProfileId profileId, bool opaque, RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask, StencilState stencilState, int stencilReference)
    {
        base.profilingSampler = new ProfilingSampler(nameof(GrassRenderPass));
        m_ProfilingSampler = ProfilingSampler.Get(profileId);
        m_ProfilingSampler = new ProfilingSampler("GrassRenderPass");
        m_ShaderTagId = new ShaderTagId("GrassForward");

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

            var sortFlags = (m_IsOpaque) ? renderingData.cameraData.defaultOpaqueSortFlags : SortingCriteria.CommonTransparent;

            var drawSettings = CreateDrawingSettings(m_ShaderTagId, ref renderingData, sortFlags);

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
            RenderingUtils.RenderObjectsWithError(context, ref renderingData.cullResults, camera, filterSettings, SortingCriteria.None);
        }

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}