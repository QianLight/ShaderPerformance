using UnityEngine.Rendering.Universal.Internal;
using System.Reflection;

namespace UnityEngine.Rendering.Universal
{
    public sealed class JitterPass : ScriptableRenderPass
    {
        public static readonly ProfilingSampler TAAProfilingSampler = new ProfilingSampler("TaaFrustumJitter");
        public bool CanWork = false;
        private TemporaAASetting taaSetting;
        private FrustumJitter frustum = null;
        public JitterPass()
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingPrepasses;
            frustum = new FrustumJitter();
        }
        public Matrix4x4 PrevV
        {
            get
            {
                return frustum.PrevV;
            }
        }
        public Matrix4x4 PM
        {
            get
            {
                return frustum.PM;
            }
        }

        public Vector4 ProjectionExtents
        {
            get
            {
                return frustum.ProjectionExtents;
            }
        }
        public Vector4 JitterSample
        {
            get
            {
                return frustum.JitterSample;
            }
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            frustum.PatternScale = taaSetting.JitterPoint;
           
            CameraData cameraData = renderingData.cameraData;
            //if (!cameraData.isPreviewCamera && cameraData.camera.CompareTag("MainCamera"))
            {
                CommandBuffer cmd = CommandBufferPool.Get();
                using (new ProfilingScope(cmd, TAAProfilingSampler))
                {
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();
                    frustum.Jitter(cameraData.camera, taaSetting.Pattern, true);
                    //Shader.SetGlobalVector("_TAAoffsetUV", frustum.JitterSample);
                    cmd.SetViewProjectionMatrices(frustum.CurrV, frustum.PM);
                }
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);

                CanWork = true;
            }
            //else
            {
                //CanWork = false;
            }
        }


     
        public void Setup(TemporaAASetting setting)
        {
            taaSetting = setting;
        }

        internal void Cleanup()
        {
            //CoreUtils.Destroy(stopNaN);

        }
    }
}
