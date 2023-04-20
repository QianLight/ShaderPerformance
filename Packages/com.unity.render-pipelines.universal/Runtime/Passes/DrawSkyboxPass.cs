namespace UnityEngine.Rendering.Universal
{
    /// <summary>
    /// Draw the skybox into the given color buffer using the given depth buffer for depth testing.
    ///
    /// This pass renders the standard Unity skybox.
    /// </summary>
    public class DrawSkyboxPass : ScriptableRenderPass
    {
        private Mesh fakeSkybox;
        protected DrawingSettings m_DrawingSettings;
        protected FilteringSettings m_FilteringSettings;
        protected RenderStateBlock m_RenderStateBlock;
        public DrawSkyboxPass(RenderPassEvent evt)
        {
            base.profilingSampler = new ProfilingSampler(nameof(DrawSkyboxPass));

            renderPassEvent = evt;
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
#if ENABLE_VR && ENABLE_XR_MODULE
            // XRTODO: Remove this code once Skybox pass is moved to SRP land.
            if (renderingData.cameraData.xr.enabled)
            {
                // Setup Legacy XR buffer states
                if (renderingData.cameraData.xr.singlePassEnabled)
                {
                    // Setup legacy skybox stereo buffer
                    renderingData.cameraData.camera.SetStereoProjectionMatrix(Camera.StereoscopicEye.Left, renderingData.cameraData.GetProjectionMatrix(0));
                    renderingData.cameraData.camera.SetStereoViewMatrix(Camera.StereoscopicEye.Left, renderingData.cameraData.GetViewMatrix(0));
                    renderingData.cameraData.camera.SetStereoProjectionMatrix(Camera.StereoscopicEye.Right, renderingData.cameraData.GetProjectionMatrix(1));
                    renderingData.cameraData.camera.SetStereoViewMatrix(Camera.StereoscopicEye.Right, renderingData.cameraData.GetViewMatrix(1));

                    CommandBuffer cmd = CommandBufferPool.Get();

                    // Use legacy stereo instancing mode to have legacy XR code path configured
                    cmd.SetSinglePassStereo(SystemInfo.supportsMultiview ? SinglePassStereoMode.Multiview : SinglePassStereoMode.Instancing);
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();

                    // Calling into built-in skybox pass
                    context.DrawSkybox(renderingData.cameraData.camera);

                    // Disable Legacy XR path
                    cmd.SetSinglePassStereo(SinglePassStereoMode.None);
                    context.ExecuteCommandBuffer(cmd);
                    // We do not need to submit here due to special handling of stereo matricies in core.
                    // context.Submit();
                    CommandBufferPool.Release(cmd);

                    renderingData.cameraData.camera.ResetStereoProjectionMatrices();
                    renderingData.cameraData.camera.ResetStereoViewMatrices();
                }
                else
                {
                    renderingData.cameraData.camera.projectionMatrix = renderingData.cameraData.GetProjectionMatrix(0);
                    renderingData.cameraData.camera.worldToCameraMatrix = renderingData.cameraData.GetViewMatrix(0);


                    context.DrawSkybox(renderingData.cameraData.camera);

                    // XRTODO: remove this call because it creates issues with nested profiling scopes
                    // See examples in UniversalRenderPipeline.RenderSingleCamera() and in ScriptableRenderer.Execute()
                    context.Submit(); // Submit and execute the skybox pass before resetting the matrices

                    renderingData.cameraData.camera.ResetProjectionMatrix();
                    renderingData.cameraData.camera.ResetWorldToCameraMatrix();
                }
            }
            else
#endif
            {
#if UNITY_EDITOR
                if ((OverdrawState.gameOverdrawViewMode 
                 && renderingData.cameraData.camera.cameraType.Equals(CameraType.Game) 
                 ||OverdrawState.sceneOverdrawViewMode 
                 && renderingData.cameraData.camera.cameraType.Equals(CameraType.SceneView)) && RenderSettings.skybox != null)
                {
                
                    Material old = RenderSettings.skybox;
                    CommandBuffer cmd = CommandBufferPool.Get();
                    //
                    fakeSkybox = new Mesh();
                    Camera currentCamera = renderingData.cameraData.camera;
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

#else
                context.DrawSkybox(renderingData.cameraData.camera);
#endif
            }
        }
    }
}
