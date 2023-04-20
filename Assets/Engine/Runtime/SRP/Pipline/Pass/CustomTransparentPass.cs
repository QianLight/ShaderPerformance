using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine.SRP
{
    internal class CustomTransparentPass : SparkRenderPass
    {
        protected DrawingSettings m_DrawingSettingsTrans;
        private FilteringSettings m_FilterSettingsTrans;

        public CustomTransparentPass (string profilerTag,
            RenderQueueRange renderQueueRange) : base (profilerTag,renderQueueRange)
        {
            m_FilterSettingsTrans = new FilteringSettings(RenderQueueRange.all);
        }

        public override void Init (ref ScriptableRenderContext context, ref RenderingData renderingData, uint layerMask)
        {
            if (flag.HasFlag(Flag_IsInit))
            {
                m_DrawingSettings.sortingSettings = new SortingSettings(renderingData.cameraData.camera) { criteria = SortingCriteria.CommonOpaque };
                m_DrawingSettingsTrans.sortingSettings = new SortingSettings(renderingData.cameraData.camera) { criteria = SortingCriteria.CommonTransparent };
                if (renderingData.context != null)
                    m_FilteringSettings.renderingLayerMask = layerMask;
            }
            else
            {
                m_sortFlags = SortingCriteria.CommonTransparent;
                CreateDrawingSettings (
                    new ShaderTagId ("PreZ"),
                    SortingCriteria.CommonOpaque, ref renderingData, ref m_DrawingSettings);
                m_PassShaderTagId = new ShaderTagId("ForwardTransparent");
                CreateDrawingSettings(
                    m_PassShaderTagId,
                    m_sortFlags, ref renderingData, ref m_DrawingSettingsTrans);
                flag.SetFlag (Flag_IsInit, true);
            }
        }

        public virtual void ExecuteTrans(ref ScriptableRenderContext context, ref RenderingData rd)
        {
#if UNITY_EDITOR
            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilingSampler.name);
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                if (RenderContext.gameOverdrawViewMode
                    && rd.cameraData.camera.cameraType.Equals(CameraType.Game )
                    || RenderContext.sceneOverdrawViewMode
                    && rd.cameraData.camera.cameraType.Equals(CameraType.SceneView))
                {
                    m_DrawingSettingsTrans.SetShaderPassName(0, new ShaderTagId("SRPDefaultUnlit"));
                    m_DrawingSettingsTrans.SetShaderPassName(1, new ShaderTagId("OverdrawForwardTransparent"));
                }else
                {
                    CreateDrawingSettings(
                        m_PassShaderTagId,
                        m_sortFlags, ref rd, ref m_DrawingSettingsTrans );
                }
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
#endif
                context.DrawRenderers(rd.cullResults, ref m_DrawingSettingsTrans, ref m_FilterSettingsTrans, ref m_RenderStateBlock);

                // // Render objects that did not match any shader pass with error shader
                // RenderingUtils.RenderObjectsWithError (context, ref renderingData.cullResults, camera, m_FilteringSettings, SortingCriteria.None);
#if UNITY_EDITOR
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
#endif
        }
    }
}