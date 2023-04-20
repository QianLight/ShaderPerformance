using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine.SRP
{
    internal sealed class OffscreenRenderer : SparkRenderer
    {
        const string k_CreateCameraTextures = "Create Camera Texture";
        private DrawObjectsPass m_OpaquePass;
        private DrawSkyboxPass m_SkyboxPass;
        private DrawObjectsPass m_TransparentPass;
        private static SparkRenderer m_Render;
        public static SparkRenderer offscreenRenderer
        {
            get
            {
                if (m_Render == null)
                {
                    m_Render = new OffscreenRenderer ();
                }
                return m_Render;
            }
        }
        public OffscreenRenderer ()
        {
            var passMgr = PassManager.singleton;
            m_OpaquePass = passMgr.opaquePass;
            m_SkyboxPass = passMgr.skyboxPass;
            m_TransparentPass = passMgr.transparentPass;
        }

        public override bool InitCamera(ref RenderingData rd, Camera camera)
        {
            bool b = base.InitCamera(ref rd, camera);
            if (b)
            {
                rd.cameraData.clearFlag = GetClearFlag(camera);
            }
            return b;
        }

        public override void Setup (ref ScriptableRenderContext context, ref ScriptableCullingParameters cullingParameters, ref RenderingData rd)
        {
            base.Setup (ref context, ref cullingParameters, ref rd);
            Camera camera = rd.cameraData.camera;
            m_OpaquePass.Init (ref context, ref rd, uint.MaxValue);
            m_TransparentPass.Init (ref context, ref rd, uint.MaxValue);
        }
        protected override void ExecuteCameraRT (ref ScriptableRenderContext context, ref RenderingData rd)
        {
            context.SetupCameraProperties (rd.cameraData.camera);
            Camera camera = rd.cameraData.camera;
            CommandBuffer cmd = CommandBufferPool.Get (k_CreateCameraTextures);
            Color clearColor = new Color (0.25f, 0.25f, 0.25f, 1);
            cmd.SetRenderTarget (camera.targetTexture);
            RuntimeUtilities.ClearRenderTarget (cmd, rd.cameraData.clearFlag, ref clearColor);
            context.ExecuteCommandBuffer (cmd);
            CommandBufferPool.Release (cmd);
        }
        protected override void ExecuteOpaque (ref ScriptableRenderContext context, ref RenderingData rd)
        {

            m_OpaquePass.Execute (ref context, ref rd);
            m_SkyboxPass.Execute (ref context, ref rd);
        }

        protected override void ExecuteTransparent (ref ScriptableRenderContext context, ref RenderingData rd)
        {
            m_TransparentPass.Execute (ref context, ref rd);
        }
    }
}