using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine.SRP
{
    public class DrawSkyboxPass : SparkRenderPass
    {
#if UNITY_EDITOR
        public static bool drawSkybox = true;
#endif

        private Mesh fakeSkybox;
        public DrawSkyboxPass(string profilerTag, RenderQueueRange renderQueueRange)
        {
#if UNITY_EDITOR
             if (!string.IsNullOrEmpty(profilerTag))
                 m_ProfilingSampler = new ProfilingSampler(profilerTag);
#endif
            // m_FilteringSettings = new FilteringSettings(renderQueueRange);
            // m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
        }

        public override void Execute(ref ScriptableRenderContext context, ref RenderingData renderingData)
        {
#if UNITY_EDITOR
            if (!drawSkybox)
            {
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilingSampler.name);/*RenderContext.singleton.afterOpaqueCmd;*//*CommandBufferPool.Get(m_ProfilingSampler.name);*/

           // using (new ProfilingScope(cmd, m_ProfilingSampler))
            // {
                 if ((RenderContext.gameOverdrawViewMode 
                     && renderingData.cameraData.camera.cameraType.Equals(CameraType.Game) 
                     ||RenderContext.sceneOverdrawViewMode 
                     && renderingData.cameraData.camera.cameraType.Equals(CameraType.SceneView)) && RenderSettings.skybox != null)
                 {
                     
                     Material old = RenderSettings.skybox;
                     //
                     fakeSkybox = new Mesh();
                     Camera currentCamera = EngineContext.instance.CameraRef;
                     Vector3[] point = new[]
                     {
                         new Vector3(0, 0, currentCamera.farClipPlane),
                         new Vector3(0, Screen.height, currentCamera.farClipPlane),
                         new Vector3(Screen.width,0,currentCamera.farClipPlane),
                         new Vector3(Screen.width,Screen.height,currentCamera.farClipPlane)
                     };
                     for (int i = 0; i < point.Length; i++)
                     {
                         Debug.DrawLine(currentCamera.gameObject.transform.position, point[i],Color.cyan);
                     }
                     fakeSkybox.vertices = new[]
                     {
                         currentCamera.ScreenToWorldPoint(point[0]),
                         currentCamera.ScreenToWorldPoint(point[1]),
                         currentCamera.ScreenToWorldPoint(point[2]),
                         currentCamera.ScreenToWorldPoint(point[3]),
                     };
                     
                     fakeSkybox.triangles = new[] {1,3,2,1,2,0};
                     cmd.DrawMesh(fakeSkybox, Matrix4x4.identity, old, 0, 1);
                     //
                     m_DrawingSettings.SetShaderPassName(0, new ShaderTagId("SRPDefaultUnlit"));
                     m_DrawingSettings.SetShaderPassName(1, new ShaderTagId("OverdrawSkybox"));
                     var sortingSettings = m_DrawingSettings.sortingSettings;
                     sortingSettings.criteria = SortingCriteria.CommonOpaque;
                     m_DrawingSettings.sortingSettings = sortingSettings;
                     //
                     context.ExecuteCommandBuffer(cmd);
                     cmd.Clear();
                     context.DrawRenderers(renderingData.cullResults, ref m_DrawingSettings, ref m_FilteringSettings, ref m_RenderStateBlock);
                     context.ExecuteCommandBuffer(cmd);
                     CommandBufferPool.Release(cmd);
                     
                     // context.DrawSkybox(renderingData.cameraData.camera);
                     return;
                 }
                 else
                 {
                     context.DrawSkybox(renderingData.cameraData.camera);
                 }
                 return;
                // }
#endif
            context.DrawSkybox(renderingData.cameraData.camera);
        }
        
    }
}