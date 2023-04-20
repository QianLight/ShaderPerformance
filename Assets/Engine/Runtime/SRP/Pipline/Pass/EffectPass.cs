using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine.SRP
{
    internal class EffectPass : SparkRenderPass
    {
        public EffectPass (string profilerTag,
            RenderQueueRange renderQueueRange) : base (profilerTag,renderQueueRange)
        {

        }

        public override void Init (ref ScriptableRenderContext context, ref RenderingData renderingData, uint layerMask)
        {
            m_sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
            if (flag.HasFlag (Flag_IsInit))
            {
                m_DrawingSettings.sortingSettings = new SortingSettings (renderingData.cameraData.camera) { criteria = m_sortFlags };
                if (renderingData.context != null)
                    m_FilteringSettings.renderingLayerMask = layerMask;
            }
            else
            {
                m_PassShaderTagId = new ShaderTagId("XRay");
                CreateDrawingSettings (m_PassShaderTagId,
                    m_sortFlags, ref renderingData, ref m_DrawingSettings);
                flag.SetFlag (Flag_IsInit, true);
            }
        }
    }
}