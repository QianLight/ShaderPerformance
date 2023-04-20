using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Rendering.Universal.Internal
{
    public class TerrainBlendingFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class TerrainBlendingSettings
        {
            public string passTag = "Terrain Blending";
            public RenderPassEvent Event = RenderPassEvent.BeforeRenderingOpaques;
            //public bool enable = true;
            public LayerMask TerrainLayer = 0;
            public int DowpSample = 0;
            [Range(0f, 100f)] public float blendDistance = 30;

        }
        private TerrainBlendingPass m_TerrainBlendingPass;
        public TerrainBlendingSettings _settings = new TerrainBlendingSettings();

        public override void Create()
        {
            m_TerrainBlendingPass = new TerrainBlendingPass(_settings);
            //Setup(_settings);
            Switch();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.camera != Camera.main && renderingData.cameraData.cameraType != CameraType.SceneView)
            {
                return;
            }
            //if (_settings.enable)
            {
                if (GameQualitySetting.TerrainFeature == TerrainFunc.TerrainBlend)
                {
                    renderer.EnqueuePass(m_TerrainBlendingPass);
                }
            }

        }
        // private void Setup(TerrainBlendingSettings settings)
        // {
        //    m_TerrainBlendingPass._settings = settings;
        // }
        private void Switch()
        {
        }
    }
    public class TerrainBlendingPass : ScriptableRenderPass
    {
        public TerrainBlendingFeature.TerrainBlendingSettings _settings;
        private const string m_ProfilerTag = "Terrain Blending Pass";
        private ProfilingSampler m_ProfilingSampler = new ProfilingSampler(m_ProfilerTag);
        private ShaderTagId m_ShaderTagId = new ShaderTagId("TerrainBlending");
        private FilteringSettings m_FilteringSettings;

        private int _TerrainAlbedo = Shader.PropertyToID("_TerrainAlbedo");
        private int _TerrainNormal = Shader.PropertyToID("_TerrainNormal");
        private int _TerrainDepth = Shader.PropertyToID("_TerrainDepth");
        private string _TerrainEnable = "_ENABLE_TERRAIN_BLENDING"; 
        public TerrainBlendingPass(TerrainBlendingFeature.TerrainBlendingSettings m_settings)
        {
            _settings = m_settings;
            m_FilteringSettings = new FilteringSettings(RenderQueueRange.opaque,m_settings.TerrainLayer);
            overrideCameraTarget = true;
            m_MRT = new RenderTargetIdentifier[2];
        }

        private RenderTargetIdentifier[] m_MRT;
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            cmd.GetTemporaryRT(_TerrainAlbedo, cameraTextureDescriptor.width >> _settings.DowpSample, cameraTextureDescriptor.height >> _settings.DowpSample, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
            cmd.GetTemporaryRT(_TerrainNormal, cameraTextureDescriptor.width >> _settings.DowpSample, cameraTextureDescriptor.height >> _settings.DowpSample, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
            cmd.GetTemporaryRT(_TerrainDepth, cameraTextureDescriptor.width >> _settings.DowpSample, cameraTextureDescriptor.height >> _settings.DowpSample, 24, FilterMode.Bilinear, RenderTextureFormat.Depth);

            renderPassEvent = _settings.Event;
            m_FilteringSettings.layerMask = _settings.TerrainLayer;
            
            m_MRT[0] = _TerrainAlbedo;
            m_MRT[1] = _TerrainNormal;
            
            //Shader.EnableKeyword(_TerrainEnable);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                cmd.SetRenderTarget(m_MRT, _TerrainDepth);
                cmd.ClearRenderTarget(true, true, Color.black);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                
                var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
                var drawSettings = CreateDrawingSettings(m_ShaderTagId, ref renderingData, sortFlags);
                drawSettings.perObjectData = PerObjectData.None;
                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref m_FilteringSettings);
            }
            cmd.SetGlobalTexture(_TerrainAlbedo, _TerrainAlbedo);
            cmd.SetGlobalTexture(_TerrainNormal, _TerrainNormal);
            cmd.SetGlobalTexture(_TerrainDepth, _TerrainDepth);
            cmd.SetGlobalFloat("_BlendDistance", _settings.blendDistance);
            
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (cmd == null)
            {
                throw new ArgumentNullException("cmd");
            }
            //Shader.DisableKeyword(_TerrainEnable);
            cmd.ReleaseTemporaryRT(_TerrainAlbedo);
            cmd.ReleaseTemporaryRT(_TerrainNormal);
            cmd.ReleaseTemporaryRT(_TerrainDepth);
        }
    }
}
