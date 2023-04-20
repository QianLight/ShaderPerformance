using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine.SRP
{
    internal class DrawObjectsPass : SparkRenderPass
    {
        public DrawObjectsPass (string profilerTag,
            bool isOpaque,
            RenderQueueRange renderQueueRange) : base (profilerTag,renderQueueRange)
        {

            flag.SetFlag (Flag_IsOpaque, isOpaque);
        }

        public override void Init (ref ScriptableRenderContext context, ref RenderingData renderingData, uint layerMask)
        {
            m_sortFlags = (flag.HasFlag (Flag_IsOpaque)) ? renderingData.cameraData.defaultOpaqueSortFlags : SortingCriteria.CommonTransparent;
            if (flag.HasFlag (Flag_IsInit))
            {
                var camera = renderingData.cameraData.camera;
                m_DrawingSettings.sortingSettings = new SortingSettings (camera) { criteria = m_sortFlags };
                m_FilteringSettings.renderingLayerMask = layerMask;
            }
            else
            {
                m_PassShaderTagId = new ShaderTagId("ForwardBase");
                CreateDrawingSettings (m_PassShaderTagId,
                    m_sortFlags, ref renderingData, ref m_DrawingSettings);
                flag.SetFlag (Flag_IsInit, true);

            }
        }
    }
}