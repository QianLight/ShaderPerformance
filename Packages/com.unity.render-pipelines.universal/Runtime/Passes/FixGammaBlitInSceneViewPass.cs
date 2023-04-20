/*
 * Author:  Takeshi
 * Date:    2021/10/14
 * Apply:   Fix UI Gamma in SceneView: Change all render to Linear
 */
#if UNITY_EDITOR
namespace UnityEngine.Rendering.Universal.Internal
{
    /// <summary>
    /// Copy the given color target to the current camera target
    ///
    /// You can use this pass to copy the result of rendering to
    /// the camera target. The pass takes the screen viewport into
    /// consideration.
    /// </summary>
    public class FixGammaBlitInSceneViewPass : ScriptableRenderPass
    {
        RenderTargetHandle _source;
        Material m_BlitMaterial;
        RenderTargetHandle _fixGammaBlitRTHandle;
        private ProfilingSampler m_fixGammaBlitInSceneView = new ProfilingSampler("FixGammaBlit InSceneView");

        public FixGammaBlitInSceneViewPass(RenderPassEvent evt, Material blitMaterial)
        {
            base.profilingSampler = new ProfilingSampler(nameof(FixGammaBlitInSceneViewPass));

            m_BlitMaterial = blitMaterial;
            renderPassEvent = evt;
            _fixGammaBlitRTHandle.Init("_FixGammaBlit");
        }

        /// <summary>
        /// Configure the pass
        /// </summary>
        /// <param name="baseDescriptor"></param>
        /// <param name="colorHandle"></param>
        public void Setup(RenderTextureDescriptor baseDescriptor, RenderTargetHandle colorHandle)
        {
            _source = colorHandle;
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // _BaseBlitTex.name = "_BaseBlitTex";
            if (m_BlitMaterial == null)
            {
                Debug.LogErrorFormat("Missing {0}. {1} render pass will not execute. Check for missing reference in the renderer resources.", m_BlitMaterial, GetType().Name);
                return;
            }
            
            // Note: We need to get the cameraData.targetTexture as this will get the targetTexture of the camera stack.
            // Overlay cameras need to output to the target described in the base camera while doing camera stack.
            //ref CameraData cameraData = ref renderingData.cameraData;
            //RenderTargetIdentifier cameraTarget = (cameraData.targetTexture != null) ? new RenderTargetIdentifier(cameraData.targetTexture) : BuiltinRenderTextureType.CameraTarget;
            //RenderTargetIdentifier cameraTarget =  new RenderTargetIdentifier("_BaseBlitTex");
            //RenderTargetIdentifier cameraTarget = (cameraData.targetTexture != null) ? new RenderTargetIdentifier(_BaseBlitTex) : BuiltinRenderTextureType.CameraTarget;
            //RenderTargetIdentifier cameraTarget =  BuiltinRenderTextureType.RenderTexture;
            //bool isSceneViewCamera = cameraData.isSceneViewCamera;
            CommandBuffer cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, m_fixGammaBlitInSceneView))
            {
                /* create a new RT */
                RenderTextureDescriptor blitDesc = renderingData.cameraData.cameraTargetDescriptor;
                blitDesc.depthBufferBits = 0;
                cmd.GetTemporaryRT(_fixGammaBlitRTHandle.id,blitDesc);
                cmd.SetGlobalFloat(ShaderPropertyId.isInUICamera, 0);


                cmd.SetGlobalTexture(ShaderPropertyId.sourceTex, _source.Identifier());


                /* By:Takeshi Apply:矫正UI透明度Gamma的分支 */
                cmd.EnableShaderKeyword(ShaderKeywordStrings.SRGBToLinearConversion);


                cmd.Blit(_source.Identifier(), _fixGammaBlitRTHandle.Identifier(), m_BlitMaterial);
                /* By:Takeshi Apply:结束矫正UI透明度Gamma的分支 */
                cmd.DisableShaderKeyword(ShaderKeywordStrings.SRGBToLinearConversion);
                
                /* Blit 回到主RT上 */
                cmd.Blit(_fixGammaBlitRTHandle.Identifier(), _source.Identifier());
                cmd.ReleaseTemporaryRT(_fixGammaBlitRTHandle.id);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
#endif