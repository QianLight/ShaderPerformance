using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine.SRP
{
    internal sealed class ForwardRenderer : SparkRenderer
    {
        public static SparkRenderer mainRenderer;
        private DrawObjectsPass m_OpaquePass;
        private OutlinePass m_OutlinePass;
        private DrawSkyboxPass m_SkyboxPass;
        private CustomTransparentPass m_CustomTransparentPass;
        private DrawObjectsPass m_TransparentPass;
        private DistortionPass m_DistortionPass;

        public ForwardRenderer ()
        {
            var passMgr = PassManager.singleton;
            m_OpaquePass = passMgr.opaquePass;
            m_OutlinePass = passMgr.outlinePass;
            m_SkyboxPass = passMgr.skyboxPass;
            m_TransparentPass = passMgr.transparentPass;
            m_DistortionPass = passMgr.distortionPass;
            m_CustomTransparentPass = passMgr.customTransparentPass;
        }
        public override bool InitCamera (ref RenderingData rd, Camera camera)
        {
            var ec = rd.context;
            if (ec != null && ec.renderflag.HasFlag (EngineContext.RFlag_RenderEnable))
            {
                base.InitCamera (ref rd, camera);
                return true;
            }
            return false;
        }
        public override void ExecuteBeforeRendering (ref ScriptableRenderContext context, ref RenderingData rd)
        {
            var rc = rd.rc;
            if (rc.IsValid ())
            {
                context.SetupCameraProperties (rd.cameraData.camera);
                // rc.preWorkingCmd.DebugCmd();
                //RuntimeUtilities.CommitCmd (ref context, rc.preWorkingCmd);
            }
        }

        public override void Setup (ref ScriptableRenderContext context, ref ScriptableCullingParameters cullingParameters, ref RenderingData rd)
        {
            base.Setup (ref context, ref cullingParameters, ref rd);
            uint layer = uint.MaxValue;
            if (rd.context != null)
                layer = rd.context.layerMask;
            m_OpaquePass.Init (ref context, ref rd, layer);
            m_OutlinePass.Init (ref context, ref rd, layer);
            m_SkyboxPass.Init(ref context, ref rd, layer);
            m_CustomTransparentPass.Init (ref context, ref rd, layer);
            m_TransparentPass.Init (ref context, ref rd, layer);
            m_DistortionPass.Init (ref context, ref rd, layer);
        }

        private void DrawDummyCamera (ref ScriptableRenderContext context,
            EngineContext engineContext, ref OrderQueue drawCalls, CommandBuffer cmd, Camera camera,bool drawStencil = false)
        {
            if (drawCalls.count > 0)
            {
                while (drawCalls.Pop<DrawBatch> (out var db))
                {
                    if (db.mesh != null)
                    {
                        cmd.DrawMesh (db.mesh, db.matrix, db.mat, 0, 0);
                    }
                    else if (db.render != null)
                    {
                        ref var dc = ref engineContext.dummyCameras[db.id];
                        cmd.SetGlobalMatrix(ShaderManager.custom_MatrixVP, dc.viewProj);
                        cmd.SetGlobalMatrix(ShaderManager.custom_MatrixV, dc.view);
                        cmd.SetGlobalMatrix(ShaderManager.custom_MatrixP, dc.projectScale);
                        cmd.SetViewport(dc.viewPort);
                        cmd.DrawRenderer(db.render, db.mat, 0, 0);
                    }

                    SharedObjectPool<DrawBatch>.Release (db);
                }
                //Matrix4x4 viewMatrix = camera.worldToCameraMatrix;
                //Matrix4x4 projMatrix = camera.projectionMatrix;
                //cmd.SetViewProjectionMatrices(viewMatrix, projMatrix);
                RuntimeUtilities.CommitCmd (ref context, cmd);
            }
        }
       
        protected override void ExecuteCameraRT (ref ScriptableRenderContext context, ref RenderingData rd)
        {
            context.SetupCameraProperties (rd.cameraData.camera);
            var rc = rd.rc;
            if (rc.IsValid ())
            {
                Color clearColor = Color.black;
#if UNITY_EDITOR
                if (RenderContext.customClearColorGetter != null &&
                    RenderContext.customClearColorGetter(rd.cameraData.camera, out Color customClearColor))
                {
                    clearColor = customClearColor;
                }
#endif
                rc.CreateCameraRT(rc.preWorkingCmd);
                rc.SetCameraRT (rc.preWorkingCmd, rd.cameraData.camera, ref clearColor);
                RuntimeUtilities.CommitCmd (ref context, rc.preWorkingCmd);
            }
        }
        protected override void ExecuteOpaque (ref ScriptableRenderContext context, ref RenderingData rd)
        {
            var cmd = rd.rc.afterOpaqueCmd;
            var engineContext = rd.context;
            if (engineContext.renderflag.HasFlag (EngineContext.RFlag_DummyCameraRender))
            {
                DrawDummyCamera (ref context, engineContext, ref engineContext.opaqueDrawCall, cmd, rd.cameraData.camera,true);
                //m_SkyboxPass.Execute (ref context, ref rd);
            }
            else
            {
                m_OpaquePass.Execute (ref context, ref rd);
                m_CustomTransparentPass.Execute(ref context, ref rd);
                m_OutlinePass.Execute (ref context, ref rd);
                
                if (!engineContext.renderflag.HasFlag (EngineContext.RFlag_CamertRT) &&
                    !engineContext.renderflag.HasFlag (EngineContext.RFlag_DummyCameraRender)&&
                    !engineContext.renderflag.HasFlag(EngineContext.RFlag_NotRenderSky))
                    m_SkyboxPass.Execute (ref context, ref rd);
                Matrix4x4 viewMatrix = rd.cameraData.camera.worldToCameraMatrix;
                Matrix4x4 projMatrix = rd.cameraData.camera.projectionMatrix;
                cmd.SetViewProjectionMatrices(viewMatrix, projMatrix);
                DistortionPass.SetMatrixes(viewMatrix, projMatrix);
                RuntimeUtilities.CommitCmd (ref context, cmd);
            }

        }

        protected override void ExecuteTransparent (ref ScriptableRenderContext context, ref RenderingData rd)
        {
            var rc = rd.rc;
#if UNITY_EDITOR            
            if (rc.IsValid ())
            {
                context.ExecuteCommandBuffer (rc.debugCmd);
            }
#endif
            var engineContext = rd.context;
            var cmd = rc.afterOpaqueCmd;
            if (engineContext.renderflag.HasFlag (EngineContext.RFlag_DummyCameraRender))
            {                
                DrawDummyCamera (ref context, engineContext, ref engineContext.transparentDrawCall, cmd, rd.cameraData.camera);
            }
            else
            {
                DrawRender(ref context, ref engineContext.transparentDrawCall, cmd);
                m_CustomTransparentPass.ExecuteTrans(ref context, ref rd);
                m_TransparentPass.Execute (ref context, ref rd);
            }
        }

        protected override void ExecuteDistortion(ref ScriptableRenderContext context, ref RenderingData rd)
        {
            if (!rd.context.renderflag.HasFlag(EngineContext.RFlag_DummyCameraRender)
                && QualitySettingData.current.flag.HasFlag(QualitySet.Flag_EnableDistortion))
            {
                m_DistortionPass.Execute(ref context, ref rd);
            }
        }

        public override void ExecuteAfterRendering (ref ScriptableRenderContext context, ref RenderingData rd)
        {
            var rc = rd.rc;
            if (rc.IsValid () && rc.stateFlag.HasFlag (RenderContext.SFlag_PPEnable))
            {
                RuntimeUtilities.CommitCmd (ref context, rc.prePPCommand, false);

                ExecuteDistortion (ref context, ref rd);

                RuntimeUtilities.CommitCmd (ref context, rc.ppCommand, false);
                rc.ReleaseRtAndFlag (rc.postPPCommand);
                RuntimeUtilities.CommitCmd (ref context, rc.postPPCommand, true);

                rc.distortionCmd.Clear();
                if (rc.stateFlag.HasFlag(RenderContext.SFlag_DistortionEnable))
                {
                    rc.distortionCmd.ReleaseTemporaryRT(RenderContext._DistortionRT.id);
                }
                RuntimeUtilities.CommitCmd(ref context, rc.distortionCmd, true);
            }

        }
    }
}