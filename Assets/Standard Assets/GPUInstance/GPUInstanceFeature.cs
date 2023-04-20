using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Experiemntal.Rendering.Universal
{
    public class GPUInstanceFeature : ScriptableRendererFeature
    {

        [System.Serializable]
        public class Settings
        {
            public RenderPassEvent RenderEvent = RenderPassEvent.AfterRenderingOpaques;
            public bool PreZ = false;
            public RenderPassEvent PreZRenderEvent = RenderPassEvent.BeforeRenderingOpaques;
        }

        public Settings settings = new Settings();
        public FilteringSettings Filter;
        GPUInstanceFeaturePass m_ScriptablePass;
        GPUInstancePreZPass m_ScriptablePreZPass;

        public override void Create()
        {
            m_ScriptablePass = new GPUInstanceFeaturePass(settings);
            m_ScriptablePreZPass = new GPUInstancePreZPass(settings);
        }


        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var src = renderer.cameraColorTarget;
            var dest = RenderTargetHandle.CameraTarget;

            //m_ScriptablePass.Setup(src,dest);
            renderer.EnqueuePass(m_ScriptablePass);
            renderer.EnqueuePass(m_ScriptablePreZPass);
        }

        public class GPUInstancePreZPass : ScriptableRenderPass
        {
            private Settings m_Settings = null;
            private string m_InstancePreZTag = "GPUInstanceFeaturePreZ";
            protected ProfilingSampler m_InstancePreSampler;

            public GPUInstancePreZPass(Settings settings)
            {
                m_InstancePreSampler = new ProfilingSampler(m_InstancePreZTag);
                m_Settings = settings;
                renderPassEvent = m_Settings.PreZRenderEvent;
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (renderingData.cameraData.renderType == CameraRenderType.Base && StaticInstanceMgr.NeedWork())
                {
                    if(!m_Settings.PreZ)
                    {
                        return;
                    }

                    CommandBuffer cmd = CommandBufferPool.Get(m_InstancePreZTag);
                    using (new ProfilingScope(cmd, m_InstancePreSampler))
                    {
                        StaticInstanceMgr.Update(cmd, 0);
                    }
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();
                    CommandBufferPool.Release(cmd);
                }
            }
        }

        public class GPUInstanceFeaturePass : ScriptableRenderPass
        {
            private Settings m_Settings = null;
            private string m_InstanceTag = "GPUInstanceFeature";
            protected ProfilingSampler m_InstanceSampler;

            public GPUInstanceFeaturePass(Settings settings)
            {
                m_InstanceSampler = new ProfilingSampler(m_InstanceTag);
                m_Settings = settings;
                renderPassEvent = m_Settings.RenderEvent;
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if(renderingData.cameraData.renderType == CameraRenderType.Base && StaticInstanceMgr.NeedWork())
                {
                    CommandBuffer cmd = CommandBufferPool.Get(m_InstanceTag);
                    using (new ProfilingScope(cmd, m_InstanceSampler))
                    {
                        StaticInstanceMgr.Update(cmd, 1);

                        //StaticInstanceMgr.InitRender();
                        //while (StaticInstanceMgr.FatchRender(cmd, 0))
                        //{
                        //    context.ExecuteCommandBuffer(cmd);
                        //    cmd.Clear();
                        //}
                    }
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();
                    CommandBufferPool.Release(cmd);
                }
            }
        }
    }
}


