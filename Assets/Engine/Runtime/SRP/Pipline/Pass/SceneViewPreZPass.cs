#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
namespace CFEngine.SRP
{
    internal class SceneViewPreZPass : SparkRenderPass
    {
        private RenderTextureDescriptor depthDesc;
        public static RenderTargetHandle _SceneViewDepthRT = new RenderTargetHandle ("_SceneViewDepthRT");
        private DrawCameraMode cameraMode = DrawCameraMode.Textured;
        public SceneViewPreZPass (string profilerTag,
            RenderQueueRange renderQueueRange) : base (profilerTag,renderQueueRange)
        {

            depthDesc.dimension = TextureDimension.Tex2D;
            depthDesc.colorFormat = RenderTextureFormat.ARGB32;
            depthDesc.enableRandomWrite = false;
            depthDesc.useMipMap = false;
            depthDesc.sRGB = true;
            depthDesc.depthBufferBits = 24;
            depthDesc.msaaSamples = 1;
        }

        public override void Init (ref ScriptableRenderContext context, ref RenderingData rd, uint layerMask)
        {
            Camera camera = rd.cameraData.camera;
            var sv = SceneView.lastActiveSceneView;
            if (sv != null)
            {
                cameraMode = sv.cameraMode.drawMode;
            }
            else
            {
                cameraMode = DrawCameraMode.Textured;
            }
            if (cameraMode == DrawCameraMode.Wireframe)
            {
                var sortFlags = rd.cameraData.defaultOpaqueSortFlags;
                m_PassShaderTagId = new ShaderTagId("Always");
                CreateDrawingSettings (m_PassShaderTagId, sortFlags, ref rd, ref m_DrawingSettings);
                m_DrawingSettings.sortingSettings = new SortingSettings (camera) { criteria = sortFlags };
                m_FilteringSettings.renderingLayerMask = (uint) camera.cullingMask;               
            }
            m_DrawingSettings.overrideMaterial = AssetsConfig.instance.SceneViewPreZ;
            m_DrawingSettings.overrideMaterialPassIndex = 0;
            CommandBuffer cmd = CommandBufferPool.Get (m_ProfilingSampler.name);
            depthDesc.width = camera.pixelWidth;
            depthDesc.height = camera.pixelHeight;
            cmd.GetTemporaryRT (_SceneViewDepthRT.id, depthDesc, FilterMode.Point);
            context.ExecuteCommandBuffer (cmd);
            CommandBufferPool.Release (cmd);

        }

        public override void FrameCleanup (ref ScriptableRenderContext context)
        {
            CommandBuffer cmd = CommandBufferPool.Get (m_ProfilingSampler.name);
            if (cameraMode == DrawCameraMode.Wireframe)
            {
                cmd.SetRenderTarget (RenderTargetHandle.CameraTarget.rtID);
                cmd.SetGlobalTexture (ShaderIDs.MainTex, _SceneViewDepthRT.id);
                cmd.Blit (_SceneViewDepthRT.id, BuiltinRenderTextureType.CameraTarget, AssetsConfig.instance.CopyMat);
            }
            cmd.ReleaseTemporaryRT (_SceneViewDepthRT.id);
            context.ExecuteCommandBuffer (cmd);
            CommandBufferPool.Release (cmd);
        }
    }
}
#endif