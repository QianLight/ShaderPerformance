#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine.SRP
{
    internal sealed class SceneViewRenderer : SparkRenderer
    {

        const string k_CreateCameraTextures = "Create Camera Texture";
        private SceneViewPreZPass m_ScenePreZ;
        private DrawObjectsPass m_OpaquePass;
        private OutlinePass m_OutlinePass;
        private DrawSkyboxPass m_SkyboxPass;
        private CustomTransparentPass m_CustomTransparentPass;
        private DrawObjectsPass m_TransparentPass;
        //private RenderTextureDescriptor depthDesc;
        private static SparkRenderer m_Render;
        private static RenderTargetHandle _SceneViewDepthCopy = new RenderTargetHandle("_SceneViewDepthCopy");
        public static SparkRenderer sceneViewRenderer
        {
            get
            {
                if (m_Render == null)
                {
                    m_Render = new SceneViewRenderer ();
                }
                return m_Render;
            }
        }
        public SceneViewRenderer ()
        {
            var passMgr = PassManager.singleton;
            m_OpaquePass = passMgr.opaquePass;
            m_OutlinePass = passMgr.outlinePass;
            m_SkyboxPass = passMgr.skyboxPass;
            m_TransparentPass = passMgr.transparentPass;
            m_CustomTransparentPass = passMgr.customTransparentPass;
            m_ScenePreZ = new SceneViewPreZPass (
                "SceneViewPreZ",
                RenderQueueRange.opaque);
            //depthDesc.dimension = TextureDimension.Tex2D;
            //depthDesc.colorFormat = RenderTextureFormat.ARGB32;
            //depthDesc.enableRandomWrite = false;
            //depthDesc.useMipMap = false;
            //depthDesc.sRGB = true;
            //depthDesc.depthBufferBits = 1;
            //depthDesc.msaaSamples = 1;

        }
        public override void Setup (ref ScriptableRenderContext context, ref ScriptableCullingParameters cullingParameters, ref RenderingData rd)
        {
            base.Setup (ref context, ref cullingParameters, ref rd);
            Camera camera = rd.cameraData.camera;
            ScriptableRenderContext.EmitWorldGeometryForSceneView (camera);
            uint layer = uint.MaxValue;
            // if (rd.context != null)
            //     layer = rd.context.layerMask;
            m_CustomTransparentPass.Init (ref context, ref rd, layer);
            m_OpaquePass.Init (ref context, ref rd, layer);
            m_OutlinePass.Init (ref context, ref rd, layer);
            m_SkyboxPass.Init(ref context, ref rd, layer);
            m_ScenePreZ.Init (ref context, ref rd, layer);
            m_TransparentPass.Init (ref context, ref rd, layer);
        }
        protected override void ExecuteCameraRT (ref ScriptableRenderContext context, ref RenderingData rd)
        {
            context.SetupCameraProperties (rd.cameraData.camera);
            Camera camera = rd.cameraData.camera;
            CommandBuffer cmd = CommandBufferPool.Get (k_CreateCameraTextures);
            Color clearColor = Color.clear;

            cmd.SetRenderTarget (SceneViewPreZPass._SceneViewDepthRT.rtID, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                camera.targetTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);

            RuntimeUtilities.ClearRenderTarget (cmd, ClearFlag.All, ref clearColor);

            cmd.SetRenderTarget (camera.targetTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
                camera.targetTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            context.ExecuteCommandBuffer (cmd);
            CommandBufferPool.Release (cmd);
        }
        protected override void ExecuteOpaque (ref ScriptableRenderContext context, ref RenderingData rd)
        {

            m_ScenePreZ.Execute(ref context, ref rd);

            //CommandBuffer cmd = CommandBufferPool.Get(k_CreateCameraTextures);
            //Camera camera = rd.cameraData.camera;
            //depthDesc.width = camera.pixelWidth;
            //depthDesc.height = camera.pixelHeight;
            //cmd.GetTemporaryRT(_SceneViewDepthCopy.id, depthDesc, FilterMode.Point);

            ////cmd.Blit(SceneViewPreZPass._SceneViewDepthRT.rtID, _SceneViewDepthCopy.rtID, 
            ////    AssetsConfig.instance.CopyMat);
            ////cmd.SetGlobalTexture(RenderContext.CameraDepthTex, _SceneViewDepthCopy.rtID);
            //context.ExecuteCommandBuffer(cmd);
            ////cmd.SetRenderTarget(camera.targetTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store,
            ////    camera.targetTexture, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            //CommandBufferPool.Release(cmd);

            m_OpaquePass.Execute (ref context, ref rd);
            m_CustomTransparentPass.Execute(ref context, ref rd);
            m_OutlinePass.Execute (ref context, ref rd);

            m_SkyboxPass.Execute(ref context, ref rd);

        }

        protected override void ExecuteTransparent (ref ScriptableRenderContext context, ref RenderingData rd)
        {
            var rc = rd.rc;
            if (rc.IsValid ())
            {
                context.ExecuteCommandBuffer (rc.debugCmd);
            }
            var engineContext = rd.context;
            if(engineContext!=null)
            {
                CommandBuffer cmd = CommandBufferPool.Get(k_CreateCameraTextures);
                DrawRender(ref context, ref engineContext.transparentDrawCall, cmd);
                CommandBufferPool.Release(cmd);
            }

            m_CustomTransparentPass.ExecuteTrans(ref context, ref rd);
            m_TransparentPass.Execute (ref context, ref rd);
        }

        protected override void FinishRendering (ref ScriptableRenderContext context, ref RenderingData rd)
        {
            m_ScenePreZ.FrameCleanup (ref context);
            //CommandBuffer cmd = CommandBufferPool.Get(k_CreateCameraTextures);
            //cmd.ReleaseTemporaryRT(_SceneViewDepthCopy.id);
            //context.ExecuteCommandBuffer(cmd);
            //CommandBufferPool.Release(cmd);
        }
    }
}
#endif