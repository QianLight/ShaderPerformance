using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Experiemntal.Rendering.Universal
{
    public class TemporaDebugFeature : ScriptableRendererFeature
    {

        [System.Serializable]
        public class Settings
        {
            public RenderPassEvent RenderEvent = RenderPassEvent.AfterRenderingPostProcessing;
            public RenderTexture RT = null;
            public Material OverrideMaterial = null;
        }

        public Settings settings = new Settings();
        TemporaDebugPass m_ScriptablePass;

        public override void Create()
        {
            m_ScriptablePass = new TemporaDebugPass(settings);
        }


        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var src = renderer.cameraColorTarget;
            var dest = RenderTargetHandle.CameraTarget;

            //m_ScriptablePass.Setup(src,dest);
            renderer.EnqueuePass(m_ScriptablePass);
        }


        public class TemporaDebugPass : ScriptableRenderPass
        {
            private Settings m_Settings = null;
            private List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();
            private string m_ProfilerTag = "TemporaDebug";
            protected ProfilingSampler m_ProfilingSampler;

            public TemporaDebugPass(Settings settings)
            {
                m_ProfilingSampler = new ProfilingSampler(m_ProfilerTag);
                m_Settings = settings;
                renderPassEvent = m_Settings.RenderEvent;
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
                using (new ProfilingScope(cmd, m_ProfilingSampler))
                {
                    if(m_Settings.RT == null)
                    {
                        m_Settings.RT = new RenderTexture(Screen.width, Screen.height, 0);
                        m_Settings.RT.name = "TemporaDebugRT";
                    }
                    if(m_Settings.OverrideMaterial == null)
                    {
                        m_Settings.OverrideMaterial = new Material(Shader.Find("Universal Render Pipeline/TemporaDebug"));
                        m_Settings.OverrideMaterial.name = "TemporaDebugMat";
                    }
                    cmd.SetRenderTarget(m_Settings.RT);
                    cmd.ClearRenderTarget(true, true, Color.black);
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();

                    //if (TemporaPass.Instance != null)
                    //{
                    //    cmd.Blit(TemporaPass.Instance.CurrentRT().Identifier(), m_Settings.RT, m_Settings.OverrideMaterial);
                    //}

                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();
                }
                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }
            internal void Cleanup()
            {
                if (m_Settings.RT != null)
                {
                    GameObject.DestroyImmediate(m_Settings.RT);
                }
                if (m_Settings.OverrideMaterial != null)
                {
                    GameObject.DestroyImmediate(m_Settings.OverrideMaterial);
                }

            }
        }
    }
}


