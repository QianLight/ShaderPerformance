using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Experiemntal.Rendering.Universal
{
    public class RenderWaterMaskFeature : ScriptableRendererFeature
    {

        [System.Serializable]
        public class Settings
        {
            public RenderPassEvent RenderEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            public string ShaderTagId = "WaterMask";
            public Material OverrideMaterial = null;
            public RenderTexture m_rt;
            public Material waterMaskSolveMaterial= null;
            public int OverrideMaterialIndex = 0;
            public LayerMask Layer;

        }

        public Settings settings = new Settings();
        public FilteringSettings Filter;
        RenderWaterMaskPass m_ScriptablePass;

        public override void Create()
        {
            m_ScriptablePass = new RenderWaterMaskPass(settings);
        }


        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var src = renderer.cameraColorTarget;
            var dest = RenderTargetHandle.CameraTarget;

            m_ScriptablePass.Setup(src,dest);
            renderer.EnqueuePass(m_ScriptablePass);
        }


        public class RenderWaterMaskPass : ScriptableRenderPass
        {
            private FilteringSettings m_FilteringSettings;
            private RenderStateBlock m_RenderStateBlock;
            private Settings m_Settings = null;

            private RenderTexture renderTexture;
            private List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();
            private string m_ProfilerTag = "WaterMask";
            protected ProfilingSampler m_ProfilingSampler;
            private readonly List<ShaderTagId> _shaderTagIdList = new List<ShaderTagId>();


            RenderTargetHandle m_TemporaryColorTexture01;
            RenderTargetHandle m_TemporaryColorTexture02;

            RenderTargetIdentifier source;
            RenderTargetHandle dest;
            public RenderWaterMaskPass(Settings settings)
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
                _shaderTagIdList.Add(new ShaderTagId("UniversalForward"));
			    _shaderTagIdList.Add(new ShaderTagId("LightweightForward"));
			    _shaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
                m_TemporaryColorTexture01.Init("_TemporaryColorTexture1");
                m_TemporaryColorTexture02.Init("_TemporaryColorTexture2");
                renderTexture = settings.m_rt;
            }

            public void Setup(RenderTargetIdentifier src,RenderTargetHandle dest)
            {
                this.source  = src;
                this.dest = dest;
            }
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                
                Camera camera = renderingData.cameraData.camera;
                CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
                cmd.BeginSample(m_ProfilerTag);
                cmd.GetTemporaryRT(m_TemporaryColorTexture01.id,Screen.width,Screen.height,0);
                cmd.GetTemporaryRT(m_TemporaryColorTexture02.id,Screen.width,Screen.height,0);
                var sortingSettings = new SortingSettings(camera) { criteria = SortingCriteria.CommonOpaque};
                var drawingSettings = new DrawingSettings();
                drawingSettings.sortingSettings = sortingSettings;
	            for (int i = 0; i < _shaderTagIdList.Count; i++)
                {
				    drawingSettings.SetShaderPassName(i, _shaderTagIdList[i]);
                }
                drawingSettings.overrideMaterial = m_Settings.OverrideMaterial;
                var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);


                cmd.Blit(source, m_TemporaryColorTexture01.Identifier(),m_Settings.waterMaskSolveMaterial, 0);
                cmd.Blit(m_TemporaryColorTexture01.Identifier(),renderTexture);
                cmd.Blit( m_TemporaryColorTexture01.Identifier(),source);

                cmd.EndSample(m_ProfilerTag);
                context.ExecuteCommandBuffer(cmd);
                // Shader.SetGlobalTexture("_WaterMask",renderTexture);
                cmd.ReleaseTemporaryRT(m_TemporaryColorTexture01.id);
                cmd.ReleaseTemporaryRT(m_TemporaryColorTexture02.id);
                CommandBufferPool.Release(cmd);

                
            }
        }
    }
}


