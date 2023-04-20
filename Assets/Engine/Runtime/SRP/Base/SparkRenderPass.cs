using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine;
namespace CFEngine.SRP
{
    public abstract class SparkRenderPass
    {
        protected DrawingSettings m_DrawingSettings;
        protected FilteringSettings m_FilteringSettings;
        protected RenderStateBlock m_RenderStateBlock;
        protected FlagMask flag;

        protected static uint Flag_IsInit = 0x00000001;
        protected static uint Flag_IsOpaque = 0x00000002;
#if UNITY_EDITOR
        protected ProfilingSampler m_ProfilingSampler;
#endif
        protected ShaderTagId m_PassShaderTagId;
        protected SortingCriteria m_sortFlags;
        public SparkRenderPass ()
        { }
        public SparkRenderPass (string profilerTag, RenderQueueRange renderQueueRange)
        {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty (profilerTag))
                m_ProfilingSampler = new ProfilingSampler (profilerTag);
#endif
            m_FilteringSettings = new FilteringSettings (renderQueueRange);
            m_RenderStateBlock = new RenderStateBlock (RenderStateMask.Nothing);

        }

        public virtual void Init (ref ScriptableRenderContext context, ref RenderingData renderingData, uint layerMask)
        {

        }

        public virtual void FrameCleanup (ref ScriptableRenderContext context) { }

        public virtual void Execute (ref ScriptableRenderContext context, ref RenderingData rd)
        {
#if UNITY_EDITOR
            CommandBuffer cmd = CommandBufferPool.Get (m_ProfilingSampler.name);
            using (new ProfilingScope (cmd, m_ProfilingSampler))
            {
                
                if (RenderContext.gameOverdrawViewMode 
                    && rd.cameraData.camera.cameraType.Equals(CameraType.Game) 
                    /*||RenderContext.sceneOverdrawViewMode 
                    && rd.cameraData.camera.cameraType.Equals(CameraType.SceneView)*/ )
                {
                    // bool isOp =flag.HasFlag(Flag_IsOpaque);
                    if (m_PassShaderTagId.name.Equals("ForwardTransparent"))
                    {
                        m_DrawingSettings.SetShaderPassName(0, new ShaderTagId("SRPDefaultUnlit"));
                        m_DrawingSettings.SetShaderPassName(1, new ShaderTagId("OverdrawPreZ"));
                        SortingSettings ssT = new SortingSettings(rd.cameraData.camera)
                            {criteria = SortingCriteria.CommonTransparent};
                        ssT.criteria = SortingCriteria.CommonOpaque;
                        // m_FilteringSettings.renderQueueRange.Equals(RenderQueueRange.opaque)
                        //     ? SortingCriteria.CommonOpaque
                        //     : SortingCriteria.CommonTransparent;
                        m_DrawingSettings.sortingSettings = ssT;
                    }
                    else
                    {
                        ShaderTagId idT = new ShaderTagId("Overdraw" + m_PassShaderTagId.name+"T");
                        ShaderTagId idO = new ShaderTagId("Overdraw" + m_PassShaderTagId.name);
                        // m_DrawingSettings.overrideMaterial = m_FilteringSettings.renderQueueRange.Equals(RenderQueueRange.opaque)? AssetsConfig.instance.OverdrawOpaque : AssetsConfig.instance.OverdrawTransparent;
                        m_DrawingSettings.SetShaderPassName(0, new ShaderTagId("SRPDefaultUnlit"));
                        if(RenderContext.opaqueOverdraw)m_DrawingSettings.SetShaderPassName(1, idO);
                        if(RenderContext.transparentOverdraw)m_DrawingSettings.SetShaderPassName(2, idT);
                        var sortingSettings = m_DrawingSettings.sortingSettings;
                        sortingSettings.criteria = m_sortFlags;
                        // m_FilteringSettings.renderQueueRange.Equals(RenderQueueRange.opaque)
                        //     ? SortingCriteria.CommonOpaque
                        //     : SortingCriteria.CommonTransparent;
                        m_DrawingSettings.sortingSettings = sortingSettings;
                        
                    }
                    
                }
                else
                {
                    m_DrawingSettings.overrideMaterial = null;
                    m_DrawingSettings.SetShaderPassName(0, m_PassShaderTagId);
                    if (m_PassShaderTagId.name.Equals("ForwardTransparent"))
                    {
                        CreateDrawingSettings (
                            new ShaderTagId ("PreZ"),
                            SortingCriteria.CommonOpaque, ref rd, ref m_DrawingSettings);
                    
                        SortingSettings ssT = new SortingSettings(rd.cameraData.camera)
                            {criteria = SortingCriteria.CommonTransparent};
                        DrawingSettings ssD = new DrawingSettings(m_PassShaderTagId, ssT);
                        CreateDrawingSettings(
                            m_PassShaderTagId,
                            m_sortFlags, ref rd, ref ssD );
                    }
                    else
                    {
                        CreateDrawingSettings(
                            m_PassShaderTagId,
                            m_sortFlags, ref rd, ref m_DrawingSettings );
                    }
                    // flag.SetFlag(Flag_IsInit, false);
                    // CreateDrawingSettings(m_PassShaderTagId, m_FilteringSettings.renderQueueRange.Equals(RenderQueueRange.opaque) ? SortingCriteria.CommonOpaque : SortingCriteria.CommonTransparent, ref rd, ref m_DrawingSettings);
                }
               

                context.ExecuteCommandBuffer (cmd);
                cmd.Clear ();
#endif
                context.DrawRenderers (rd.cullResults, ref m_DrawingSettings, ref m_FilteringSettings, ref m_RenderStateBlock);
                
#if UNITY_EDITOR
                if (CustomSceneView.GetDebugDrawMode())
                {
                    if(rd.cameraData.camera.cameraType == CameraType.SceneView) 
                    {
                        if (AssetsConfig.instance.ShadedWireframe != null )
                        {
                            // m_DrawingSettings.overrideMaterial = m_FilteringSettings.renderQueueRange.Equals(RenderQueueRange.opaque)? AssetsConfig.instance.OverdrawOpaque : AssetsConfig.instance.OverdrawTransparent;
                            m_DrawingSettings.overrideMaterial = AssetsConfig.instance.ShadedWireframe;
                            // m_DrawingSettings.SetShaderPassName(0, new ShaderTagId("SRPDefaultUnlit"));
                            // m_DrawingSettings.SetShaderPassName(1, new ShaderTagId("Wireframe"));
                       
                        }
                        else
                        {
                            m_DrawingSettings.overrideMaterial = null;
                        }
                        var sortingSettings = m_DrawingSettings.sortingSettings;
                        sortingSettings.criteria = m_sortFlags;
                        // m_FilteringSettings.renderQueueRange.Equals(RenderQueueRange.opaque)
                        //     ? SortingCriteria.CommonOpaque
                        //     : SortingCriteria.CommonTransparent;
                        m_DrawingSettings.sortingSettings = sortingSettings;
                    }
                    context.DrawRenderers (rd.cullResults, ref m_DrawingSettings, ref m_FilteringSettings, ref m_RenderStateBlock);
                }
#endif

                // // Render objects that did not match any shader pass with error shader
                // RenderingUtils.RenderObjectsWithError (context, ref renderingData.cullResults, camera, m_FilteringSettings, SortingCriteria.None);
#if UNITY_EDITOR
            }

            context.ExecuteCommandBuffer (cmd);
            CommandBufferPool.Release (cmd);
#endif
        }

        public void CreateDrawingSettings (
            ShaderTagId shaderTagId,
            SortingCriteria sortingCriteria,
            ref RenderingData renderingData,
            ref DrawingSettings settings)
        {
            SortingSettings sortingSettings = new SortingSettings () { criteria = sortingCriteria };
            settings = new DrawingSettings (shaderTagId, sortingSettings);
            settings.perObjectData = PerObjectData.None;
            settings.mainLightIndex = -1;
            settings.enableDynamicBatching = renderingData.supportsDynamicBatching;
            settings.enableInstancing = true;
        }

        public void CreateDrawingSettings (
            ShaderTagId shaderTagId,
            ShaderTagId shaderTagId2,
            SortingCriteria sortingCriteria,
            ref RenderingData renderingData,
            ref DrawingSettings settings)
        {
            SortingSettings sortingSettings = new SortingSettings () { criteria = sortingCriteria };
            settings = new DrawingSettings (shaderTagId, sortingSettings);
            settings.perObjectData = PerObjectData.None;
            settings.mainLightIndex = -1;
            settings.enableDynamicBatching = renderingData.supportsDynamicBatching;
            settings.enableInstancing = true;
            settings.SetShaderPassName (1, shaderTagId2);
        }

    }
}