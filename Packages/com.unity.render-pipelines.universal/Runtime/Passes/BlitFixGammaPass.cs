/*
 * Author:  Takeshi
 * Date:    2021/10/20
 * Apply:   Fix UI Gamma in case of Post-Processing Off: Change 3D render to SRGB
 */

using JetBrains.Annotations;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal.Internal
{
    /// <summary>
    /// Fix UI Gamma in case of Post_Processing Off; Change 3D render to SRGB
    /// </summary>
    public class BlitFixGammaPass : ScriptableRenderPass
    {
        RenderTargetHandle _source;
        RenderTargetHandle _finalOutput;
        Material m_BlitMaterial;
        RenderTargetHandle _blitRTHandle;
        private ProfilingSampler m_blitWhenPostProcessOffPass;
        [CanBeNull] private string profilingTag ="Blit ( When Post-Process Off ) Pass";
        private RenderTextureDescriptor blitDesc;
        private string _shaderKeyword;
        public BlitFixGammaPass(RenderPassEvent evt, Material blitMaterial)
        {
            base.profilingSampler = new ProfilingSampler(nameof(BlitFixGammaPass));

            m_BlitMaterial = blitMaterial;
            renderPassEvent = evt;
            _blitRTHandle.Init("_Blit_FixGamma");
            m_blitWhenPostProcessOffPass = new ProfilingSampler(profilingTag);
            _shaderKeyword = ShaderKeywordStrings.LinearToSRGBConversion;
        }
        
        public BlitFixGammaPass(RenderPassEvent evt, Material blitMaterial, string profilingSamplerName)
        {
            base.profilingSampler = new ProfilingSampler(nameof(BlitFixGammaPass));
            profilingTag = profilingSamplerName;
            m_BlitMaterial = blitMaterial;
            renderPassEvent = evt;
            _blitRTHandle.Init("_Blit_FixGamma");
            m_blitWhenPostProcessOffPass = new ProfilingSampler(profilingTag);
            _shaderKeyword = ShaderKeywordStrings.LinearToSRGBConversion;
        }
        public BlitFixGammaPass(RenderPassEvent evt, Material blitMaterial, string profilingSamplerName,string shaderKeyword)
        {
            base.profilingSampler = new ProfilingSampler(nameof(BlitFixGammaPass));
            profilingTag = profilingSamplerName;
            m_BlitMaterial = blitMaterial;
            renderPassEvent = evt;
            _blitRTHandle.Init("_Blit_FixGamma");
            m_blitWhenPostProcessOffPass = new ProfilingSampler(profilingTag);
            _shaderKeyword = shaderKeyword;
        }

        /// <summary>
        /// Configure the pass
        /// </summary>
        /// <param name="baseDescriptor"></param>
        /// <param name="colorHandle"></param>
        public void Setup(RenderTargetHandle colorHandle)
        {
            _source = colorHandle;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            /* create a new RT */
            blitDesc = renderingData.cameraData.cameraTargetDescriptor;
            blitDesc.depthBufferBits = 0;
            cmd.GetTemporaryRT(_blitRTHandle.id,blitDesc);
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

            ref CameraData cameraData = ref renderingData.cameraData;
            CommandBuffer cmd = CommandBufferPool.Get();
            _finalOutput = cameraData.camera.gameObject.layer == LayerMaskName.UIScene ? RenderTargetHandle.CameraTarget : _source;

            using (new ProfilingScope(cmd, m_blitWhenPostProcessOffPass))
            {
                cmd.SetGlobalTexture(ShaderPropertyId.sourceTex, _source.Identifier());



                cmd.Blit(_source.Identifier(), _blitRTHandle.Identifier(), m_BlitMaterial);
                
//                 /* Change Format of Render BUffer */
//                 cmd.ReleaseTemporaryRT(_source.id);
// #if UNITY_EDITOR
//                 if (!cameraData.isSceneViewCamera)
// #endif
//                 {
//                     blitDesc.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
//                     blitDesc.depthBufferBits = 0;
//                 }
//                 cmd.GetTemporaryRT(_source.id,blitDesc);
                /* By:Takeshi Apply:矫正UI透明度Gamma的分支 */
                cmd.EnableShaderKeyword(_shaderKeyword);
                
                /* Blit 回到主RT上 */
                cmd.SetGlobalTexture(ShaderPropertyId.sourceTex, _blitRTHandle.Identifier());
                
                cmd.Blit(_blitRTHandle.Identifier(), _finalOutput.Identifier(),m_BlitMaterial);
                
                /* By:Takeshi Apply:结束矫正UI透明度Gamma的分支 */
                cmd.DisableShaderKeyword(_shaderKeyword);
                cmd.ReleaseTemporaryRT(_blitRTHandle.id);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
        
    }
}