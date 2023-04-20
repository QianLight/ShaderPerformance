using System;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using UnityEngine.Profiling;

namespace UnityEngine.Rendering.Universal.Internal
{
    public class SceneDepthPrepass : ScriptableRenderPass
    {
        FilteringSettings m_FilteringSettings;
        RenderStateBlock m_RenderStateBlock;
        List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();
        ProfilingSampler m_ProfilingSampler;

        private bool isTransparent;
        
        internal SceneDepthPrepass(URPProfileId profileId, RenderPassEvent evt, ShaderTagId[] shaderTagIds, bool Transparent = false)
        {
            base.profilingSampler = new ProfilingSampler(nameof(SceneDepthPrepass));
            m_ProfilingSampler = ProfilingSampler.Get(profileId);
            foreach (ShaderTagId sid in shaderTagIds)
            {
                m_ShaderTagIdList.Add(sid);
            }

            isTransparent = Transparent;

            renderPassEvent = evt;

            LayerMask layerMask =  1 << LayerMask.NameToLayer("CULL_LOD0")
                                  | 1 << LayerMask.NameToLayer("CULL_LOD1")
                                  | 1 << LayerMask.NameToLayer("CULL_LOD2")
                                  | 1 << LayerMask.NameToLayer("Ignore Raycast")
                                  | 1 << LayerMask.NameToLayer("Default")
                                  | 1 << LayerMask.NameToLayer("Role");
            m_FilteringSettings = new FilteringSettings(Transparent ? RenderQueueRange.transparent : RenderQueueRange.opaque, layerMask);
            m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                Camera camera = renderingData.cameraData.camera;
                var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
                var drawSettings = CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, sortFlags);

                m_FilteringSettings.renderingLayerMask = renderingData.renderLayerMask;
                var filterSettings = m_FilteringSettings;
                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings,
                    ref m_RenderStateBlock);

                if (!isTransparent && GameQualitySetting.DrawGrass)
                {
                    // GPUInstancingPreDepth
                    CameraData cameraData = renderingData.cameraData;
                    if (cameraData.renderType == CameraRenderType.Base &&
                        cameraData.camera.gameObject.layer == LayerMaskName.Default &&
                        GPUInstancingManager.Instance.NeedRender())
                    {
                        GPUInstancingManager.Instance.Render(cmd, 0);
                    }
                }
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}