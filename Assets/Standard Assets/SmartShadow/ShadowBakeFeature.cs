using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Experiemntal.Rendering.Universal
{
    public class ShadowBakeFeature : ScriptableRendererFeature
    {

        [System.Serializable]
        public class Settings
        {
            public RenderPassEvent RenderEvent = RenderPassEvent.AfterRenderingTransparents;
            public string ShaderTagId = "ShadowBake";
            public RenderTexture RT = null;
            public Material OverrideMaterial = null;
            public int OverrideMaterialIndex = 0;
            public LayerMask Layer;
        }

        public Settings settings = new Settings();
        public FilteringSettings Filter;
        ShadowBakeFeaturePass m_ScriptablePass;

        public override void Create()
        {
            m_ScriptablePass = new ShadowBakeFeaturePass(settings);
        }


        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var src = renderer.cameraColorTarget;
            var dest = RenderTargetHandle.CameraTarget;

            //m_ScriptablePass.Setup(src,dest);
            renderer.EnqueuePass(m_ScriptablePass);
        }


        public class ShadowBakeFeaturePass : ScriptableRenderPass
        {
            private FilteringSettings m_FilteringSettings;
            private RenderStateBlock m_RenderStateBlock;
            private Settings m_Settings = null;
            private List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();
            private string m_ProfilerTag = "ShadowBakeFeature";
            protected ProfilingSampler m_ProfilingSampler;

            public ShadowBakeFeaturePass(Settings settings)
            {
                m_ProfilingSampler = new ProfilingSampler(m_ProfilerTag);
                m_Settings = settings;
                if(!string.IsNullOrEmpty(m_Settings.ShaderTagId))
                {
                    m_ShaderTagIdList.Add(new ShaderTagId(m_Settings.ShaderTagId));
                }
                m_FilteringSettings = new FilteringSettings(RenderQueueRange.opaque);
                m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
                renderPassEvent = m_Settings.RenderEvent;
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
                SortingCriteria sortingCriteria = SortingCriteria.CommonOpaque;
                m_FilteringSettings.layerMask = m_Settings.Layer;
                using (new ProfilingScope(cmd, m_ProfilingSampler))
                {
                    int layer = (int)Mathf.Pow(2, renderingData.cameraData.camera.gameObject.layer);
                    if (layer == LayerMask.GetMask("Ignore Raycast") && m_Settings.RT != null)
                    {
                        DrawingSettings drawingSettings = CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, sortingCriteria);
                        drawingSettings.overrideMaterial = m_Settings.OverrideMaterial;
                        drawingSettings.overrideMaterialPassIndex = m_Settings.OverrideMaterialIndex;

                        cmd.SetRenderTarget(m_Settings.RT);
                        cmd.ClearRenderTarget(true, true, Color.black);
                        context.ExecuteCommandBuffer(cmd);
                        cmd.Clear();

                        context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref m_FilteringSettings, ref m_RenderStateBlock);
                        context.ExecuteCommandBuffer(cmd);
                        cmd.Clear();
                    }
                }
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
        }
    }
}


