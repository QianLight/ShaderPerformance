using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine.SRP
{
    internal class PreZPass : SparkRenderPass
    {
        public PreZPass (string profilerTag,
            RenderQueueRange renderQueueRange) : base (profilerTag,renderQueueRange)
        {
        }

        public override void Init (ref ScriptableRenderContext context, ref RenderingData renderingData, uint layerMask)
        {
            m_sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
            if (flag.HasFlag (Flag_IsInit))
            {
                var camera = renderingData.cameraData.camera;
                m_DrawingSettings.sortingSettings = new SortingSettings (camera) { criteria = m_sortFlags };
                m_FilteringSettings.renderingLayerMask = layerMask;
            }
            else
            {
                m_PassShaderTagId = new ShaderTagId("PreZ");
                CreateDrawingSettings (m_PassShaderTagId, m_sortFlags, ref renderingData, ref m_DrawingSettings);
                flag.SetFlag (Flag_IsInit, true);
            }
        }

    }
}