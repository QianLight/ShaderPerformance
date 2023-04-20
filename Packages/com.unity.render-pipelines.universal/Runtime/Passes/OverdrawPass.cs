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
    public class OverdrawPass : ScriptableRenderPass
    {
        FilteringSettings m_FilteringSettings;
        RenderStateBlock m_RenderStateBlock;
        List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();
        string m_ProfilerTag;
        ProfilingSampler m_ProfilingSampler;
        bool m_IsOpaque;

        static readonly int s_OverdrawPassDataPropID = Shader.PropertyToID("_OverdrawPassData");

        public OverdrawPass(string profilerTag, ShaderTagId[] shaderTagIds, bool opaque, RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask, StencilState stencilState, int stencilReference)
        {
            base.profilingSampler = new ProfilingSampler(nameof(OverdrawPass));

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

        public OverdrawPass(string profilerTag, bool opaque, RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask, StencilState stencilState, int stencilReference, ShaderTagId[] customTagIds = null)
            : this(profilerTag,
                customTagIds ?? new ShaderTagId[] { new ShaderTagId("SRPDefaultUnlit"), new ShaderTagId("OverdrawForwardBase"), new ShaderTagId("OverdrawForwardBaseT")},
                opaque, evt, renderQueueRange, layerMask, stencilState, stencilReference)
        {}

        internal OverdrawPass(URPProfileId profileId, bool opaque, RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask, StencilState stencilState, int stencilReference, ShaderTagId[] customTagIds = null)
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
                // Global render pass data containing various settings.
                // x,y,z are currently unused
                // w is used for knowing whether the object is opaque(1) or alpha blended(0)
                Vector4 OverdrawPassData = new Vector4(0.0f, 0.0f, 0.0f, (m_IsOpaque) ? 1.0f : 0.0f);
                cmd.SetGlobalVector(s_OverdrawPassDataPropID, OverdrawPassData);

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

                Camera camera = renderingData.cameraData.camera;
                var sortFlags = (m_IsOpaque) ? renderingData.cameraData.defaultOpaqueSortFlags : SortingCriteria.CommonTransparent;
                var drawSettings = CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, sortFlags);
                
                m_FilteringSettings.renderingLayerMask = renderingData.renderLayerMask;
                var filterSettings = m_FilteringSettings;

                #if UNITY_EDITOR
                // When rendering the preview camera, we want the layer mask to be forced to Everything
                if (renderingData.cameraData.isPreviewCamera)
                {
                    filterSettings.layerMask = -1;
                }
                if (OverdrawState.gameOverdrawViewMode 
                    && renderingData.cameraData.camera.cameraType.Equals(CameraType.Game) 
                    ||OverdrawState.sceneOverdrawViewMode 
                    && renderingData.cameraData.camera.cameraType.Equals(CameraType.SceneView) )
                {
                    // bool isOp =flag.HasFlag(Flag_IsOpaque);
                    // if (m_ShaderTagIdList[1].name.Equals("ForwardTransparent"))
                    // {
                    //     drawSettings.SetShaderPassName(0, new ShaderTagId("SRPDefaultUnlit"));
                    //     drawSettings.SetShaderPassName(1, new ShaderTagId("OverdrawPreZ"));
                    //     SortingSettings ssT = new SortingSettings(renderingData.cameraData.camera)
                    //         {criteria = SortingCriteria.CommonTransparent};
                    //     ssT.criteria = SortingCriteria.CommonOpaque;
                    //     // m_FilteringSettings.renderQueueRange.Equals(RenderQueueRange.opaque)
                    //     //     ? SortingCriteria.CommonOpaque
                    //     //     : SortingCriteria.CommonTransparent;
                    //     drawSettings.sortingSettings = ssT;
                    // }
                    // else
                    // {
                        // ShaderTagId idT = new ShaderTagId("Overdraw" + m_ShaderTagIdList[1].name+"T");
                        // ShaderTagId idO = new ShaderTagId("Overdraw" + m_ShaderTagIdList[1].name);
                        ShaderTagId idT = new ShaderTagId("OverdrawForwardBaseT");
                        ShaderTagId idO = new ShaderTagId("OverdrawForwardBase");
                        // m_DrawingSettings.overrideMaterial = m_FilteringSettings.renderQueueRange.Equals(RenderQueueRange.opaque)? AssetsConfig.instance.OverdrawOpaque : AssetsConfig.instance.OverdrawTransparent;
                        drawSettings.SetShaderPassName(0, new ShaderTagId("SRPDefaultUnlit"));
                        if(OverdrawState.opaqueOverdraw)drawSettings.SetShaderPassName(1, idO);
                        if(OverdrawState.transparentOverdraw)drawSettings.SetShaderPassName(2, idT);
                        drawSettings.SetShaderPassName(3, new ShaderTagId("OverdrawSkybox"));
                        var sortingSettings = drawSettings.sortingSettings;
                        sortingSettings.criteria = sortFlags;
                        // m_FilteringSettings.renderQueueRange.Equals(RenderQueueRange.opaque)
                        //     ? SortingCriteria.CommonOpaque
                        //     : SortingCriteria.CommonTransparent;
                        drawSettings.sortingSettings = sortingSettings;
                    // }
                    
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
}
