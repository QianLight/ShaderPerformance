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
    public class DrawObjectsPass : ScriptableRenderPass
    {
        FilteringSettings m_FilteringSettings;
        RenderStateBlock m_RenderStateBlock;
        List<ShaderTagId> m_ShaderTagIdListHigh = new List<ShaderTagId>();
        List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();
        List<ShaderTagId> m_ShaderTagIdListLow = new List<ShaderTagId>();
        public string m_ProfilerTag;
        ProfilingSampler m_ProfilingSampler;
        bool m_IsOpaque;

        static readonly int s_DrawObjectPassDataPropID = Shader.PropertyToID("_DrawObjectPassData");

        public DrawObjectsPass(string profilerTag, bool opaque, RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask, StencilState stencilState, int stencilReference)
        {
            base.profilingSampler = new ProfilingSampler(nameof(DrawObjectsPass));

            m_ProfilerTag = profilerTag;
            m_ProfilingSampler = new ProfilingSampler(profilerTag);

            //ShaderTagId[] ids = new ShaderTagId[] { new ShaderTagId("SRPDefaultUnlit"), new ShaderTagId("UniversalForward"), new ShaderTagId("NoTessForward") };
            //ShaderTagId[] tessIds = new ShaderTagId[] { new ShaderTagId("SRPDefaultUnlit"), new ShaderTagId("UniversalForward"), new ShaderTagId("TessForward") };
            ShaderTagId[] ids = new ShaderTagId[] { new ShaderTagId("SRPDefaultUnlit"), new ShaderTagId("UniversalForward") };
            ShaderTagId[] tessIds = new ShaderTagId[] { new ShaderTagId("SRPDefaultUnlit"), new ShaderTagId("UniversalForward") };

            for (int i = 0; i < ids.Length; i++)
            {
                ShaderTagId tmp;
                if (Shader.IsKeywordEnabled("_TESSELLATION_ON"))
                    tmp = tessIds[i];
                else
                    tmp = ids[i];

                m_ShaderTagIdListHigh.Add(tmp);
                m_ShaderTagIdList.Add(tmp);
                m_ShaderTagIdListLow.Add(tmp);
            }
            m_ShaderTagIdListHigh.Add(new ShaderTagId("UniversalForwardHigh"));
            m_ShaderTagIdListLow.Add(new ShaderTagId("UniversalForwardLow"));

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

        public DrawObjectsPass(string profilerTag, bool opaque, RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask, StencilState stencilState, int stencilReference, ShaderTagId[] customTagIds = null)
            : this(profilerTag,
                opaque, evt, renderQueueRange, layerMask, stencilState, stencilReference)
        {}

        internal DrawObjectsPass(URPProfileId profileId, bool opaque, RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask, StencilState stencilState, int stencilReference, ShaderTagId[] customTagIds = null)
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

#if UNITY_EDITOR
                //ShaderTagId[] ids = new ShaderTagId[] { new ShaderTagId("SRPDefaultUnlit"), new ShaderTagId("UniversalForward"), new ShaderTagId("NoTessForward") };
                //ShaderTagId[] tessIds = new ShaderTagId[] { new ShaderTagId("SRPDefaultUnlit"), new ShaderTagId("UniversalForward"), new ShaderTagId("TessForward") };
                ShaderTagId[] ids = new ShaderTagId[] { new ShaderTagId("SRPDefaultUnlit"), new ShaderTagId("UniversalForward") };
                ShaderTagId[] tessIds = new ShaderTagId[] { new ShaderTagId("SRPDefaultUnlit"), new ShaderTagId("UniversalForward") };

                for (int i = 0; i < ids.Length; i++)
                {
                    ShaderTagId tmp;
                    if (Shader.IsKeywordEnabled("_TESSELLATION_ON"))
                    {
                        tmp = tessIds[i];
                    }
                    else
                    {
                        tmp = ids[i];
                    }
                    m_ShaderTagIdListHigh.Add(tmp);
                    m_ShaderTagIdList.Add(tmp);
                    m_ShaderTagIdListLow.Add(tmp);

                }
                m_ShaderTagIdListHigh.Add(new ShaderTagId("UniversalForwardHigh"));
                m_ShaderTagIdListLow.Add(new ShaderTagId("UniversalForwardLow"));
#endif

                var sortFlags = (m_IsOpaque) ? renderingData.cameraData.defaultOpaqueSortFlags : SortingCriteria.CommonTransparent;

                List<ShaderTagId> tags = null;
                if(GameQualitySetting.UniversalForwardHigh)
                {
                    tags = m_ShaderTagIdListHigh;
                }
                else if(GameQualitySetting.UniversalForwardLow)
                {
                    tags = m_ShaderTagIdList;
                }
                else
                {
                    tags = m_ShaderTagIdListLow;
                }

                var drawSettings = CreateDrawingSettings(tags, ref renderingData, sortFlags);
                
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

                // /* Add By: Takeshi; Don't Convert UI obj to Gamma from Linear when it not in a UI camera */
                // cmd.DisableShaderKeyword(ShaderKeywordStrings.IsNotInUICamera);

#if UNITY_EDITOR
                m_ShaderTagIdListHigh.Clear();
                m_ShaderTagIdList.Clear();
                m_ShaderTagIdListLow.Clear();
#endif
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
