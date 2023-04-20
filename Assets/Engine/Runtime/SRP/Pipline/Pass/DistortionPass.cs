using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine.SRP
{
    public class DistortionPass : SparkRenderPass
    {
        private static readonly int _DistortionTex = Shader.PropertyToID("_DistortionTex");
        private static readonly Color clearColor = new Color(0.5f, 0.5f, 0f, 0f);

        private static Matrix4x4 viewMatrix;
        private static Matrix4x4 projMatrix;

        public DistortionPass(string profilerTag, RenderQueueRange renderQueueRange) : base(profilerTag, renderQueueRange)
        {
            m_sortFlags = SortingCriteria.CommonTransparent;
        }

        public static void SetMatrixes(Matrix4x4 viewMatrix, Matrix4x4 projMatrix)
        {
            DistortionPass.viewMatrix = viewMatrix;
            DistortionPass.projMatrix = projMatrix;
        }

        public override void Init(ref ScriptableRenderContext context, ref RenderingData renderingData, uint layerMask)
        {
            if (flag.HasFlag(Flag_IsInit))
            {
                m_DrawingSettings.sortingSettings = new SortingSettings(renderingData.cameraData.camera) { criteria = m_sortFlags };
                if (renderingData.context != null)
                    m_FilteringSettings.renderingLayerMask = layerMask;
            }
            else
            {
                m_PassShaderTagId = new ShaderTagId("Distortion");
                CreateDrawingSettings(
                    m_PassShaderTagId,
                    m_sortFlags, ref renderingData, ref m_DrawingSettings);
                flag.SetFlag(Flag_IsInit, true);
            }
        }

        public override void Execute(ref ScriptableRenderContext context, ref RenderingData rd)
        {
            if (!EngineContext.instance.stateflag.HasFlag(EngineContext.SFlag_Distortion))
            {
                return;
            }

#if UNITY_EDITOR
            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilingSampler.name);
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();


                if (RenderContext.gameOverdrawViewMode 
                    && rd.cameraData.camera.cameraType.Equals(CameraType.Game) 
                    ||RenderContext.sceneOverdrawViewMode 
                    && rd.cameraData.camera.cameraType.Equals(CameraType.SceneView) )
                {
                    // bool isOp =flag.HasFlag(Flag_IsOpaque);
                        ShaderTagId id = new ShaderTagId("Overdraw" + m_PassShaderTagId.name);
                        // m_DrawingSettings.overrideMaterial = m_FilteringSettings.renderQueueRange.Equals(RenderQueueRange.opaque)? AssetsConfig.instance.OverdrawOpaque : AssetsConfig.instance.OverdrawTransparent;
                        m_DrawingSettings.SetShaderPassName(0, new ShaderTagId("SRPDefaultUnlit"));
                        m_DrawingSettings.SetShaderPassName(1, id);
                        var sortingSettings = m_DrawingSettings.sortingSettings;
                        sortingSettings.criteria = m_sortFlags;
                        m_DrawingSettings.sortingSettings = sortingSettings;
                }
                else
                {
                    // m_DrawingSettings.overrideMaterial = null;
                    m_DrawingSettings.SetShaderPassName(0, m_PassShaderTagId);
                        CreateDrawingSettings(
                            m_PassShaderTagId,
                            m_sortFlags, ref rd, ref m_DrawingSettings );
                }
#endif              
                CommandBuffer distortionCmd = rd.rc.distortionCmd;
                distortionCmd.GetTemporaryRT(RenderContext._DistortionRT.id, rd.rc.pixelWidth >> 1, rd.rc.pixelHeight >> 1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                distortionCmd.SetRenderTarget(RenderContext._DistortionRT.rtID, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
                distortionCmd.ClearRenderTarget(false, true, clearColor);
                distortionCmd.SetViewProjectionMatrices(viewMatrix, projMatrix);
                distortionCmd.SetGlobalTexture(_DistortionTex, RenderContext._DistortionRT.rtID);
                context.ExecuteCommandBuffer(distortionCmd);

                context.DrawRenderers(rd.cullResults, ref m_DrawingSettings, ref m_FilteringSettings, ref m_RenderStateBlock);

                rd.rc.stateFlag.SetFlag(RenderContext.SFlag_DistortionEnable, true);
#if UNITY_EDITOR
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
#endif
        }
    }
}