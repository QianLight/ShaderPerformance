using CFEngine;
using UnityEngine;
using UnityEngine.Rendering;
namespace CFEngine.SRP
{
    internal sealed class ForwardUIRenderer : SparkRenderer
    {
        public static SparkRenderer uiRenderer;
        private DrawObjectsPass m_OpaquePass;
        private DrawObjectsPass m_TransparentPass;
        private uint uiLayerMask = (uint) (1 << DefaultGameObjectLayer.SRPLayer_UI);
        public ForwardUIRenderer ()
        {
            var passMgr = PassManager.singleton;
            m_OpaquePass = passMgr.opaquePass;
            m_TransparentPass = passMgr.transparentPass;
        }

        public override void Setup (ref ScriptableRenderContext context, ref ScriptableCullingParameters cullingParameters, ref RenderingData rd)
        {
            base.Setup (ref context, ref cullingParameters, ref rd);
            m_OpaquePass.Init (ref context, ref rd, uint.MaxValue);
            m_TransparentPass.Init (ref context, ref rd, uint.MaxValue);
        }
        protected override void ExecuteCameraRT (ref ScriptableRenderContext context, ref RenderingData rd)
        {
            context.SetupCameraProperties (rd.cameraData.camera);
            var rc = rd.rc;
            if (rc.IsValid ())
            {
                var color = Color.clear;
                RuntimeUtilities.ClearRenderTarget (rc.preWorkingCmd, ClearFlag.Depth, ref color);
                RuntimeUtilities.CommitCmd (ref context, rc.preWorkingCmd);
            }
        }

        protected override void ExecuteOpaque (ref ScriptableRenderContext context, ref RenderingData rd)
        {
            m_OpaquePass.Execute (ref context, ref rd);
        }

        protected override void ExecuteTransparent (ref ScriptableRenderContext context, ref RenderingData rd)
        {
            m_TransparentPass.Execute (ref context, ref rd);

        }
        public override void ExecuteAfterRendering (ref ScriptableRenderContext context, ref RenderingData rd)
        {
            var rc = rd.rc;
            if (rc.IsValid () && rc.flag.HasFlag (RenderContext.Flag_UICameraRT))
            {
                rc.ReleaseUIEndFrameRT (rc.postPPCommand);
                RuntimeUtilities.CommitCmd (ref context, rc.postPPCommand, true);
            }

        }
    }
}