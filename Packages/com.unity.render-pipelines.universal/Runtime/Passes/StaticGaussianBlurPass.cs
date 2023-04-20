/*
 * Author:  Takeshi
 * Date:    2022/3/2
 * Apply:   Blur effect for UI when Game is Pausing  
 */

using System.Diagnostics;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal.Internal
{
    /// <summary>
    /// Fix UI Gamma in case of Post_Processing Off; Change 3D render to SRGB
    /// </summary>
    public class StaticGaussianBlurPass : ScriptableRenderPass
    {
        private int _count = 7;
        private StaticGaussianBlurContext _blurContext;
        RenderTargetHandle _source;

        private ProfilingSampler m_ProfilingSampler;
        private string profilingTag ="Static Scene Blur Pass";
        private RenderTextureDescriptor _desc;
        
        public StaticGaussianBlurPass(RenderPassEvent evt)
        {
            base.profilingSampler = new ProfilingSampler(nameof(BlitFixGammaPass));
            renderPassEvent = evt;
            m_ProfilingSampler = new ProfilingSampler(profilingTag);
        }
        
        public void Setup(RenderTargetHandle source)
        {
            _blurContext = StaticGaussianBlurContext.Get();
            _source = source;
            _blurContext.ScreenRatio = (float)Screen.height / Screen.width;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            /* create a new RT */
            _desc = renderingData.cameraData.cameraTargetDescriptor;
            _desc.depthBufferBits = 0;
            _desc.graphicsFormat = GraphicsFormat.R8G8B8A8_UNorm;
            _desc.height = Screen.height / 8;
            _desc.width = Screen.width / 8;
            cmd.GetTemporaryRT(_blurContext.TempRT.id,_desc);
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (_blurContext.BlurMaterial == null)
            {
                Debug.LogErrorFormat("Missing {0}. {1} render pass will not execute. Check for missing reference in the renderer resources.", _blurContext.BlurMaterial, GetType().Name);
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                if((_blurContext.Process & 1) != 1)
                {
                    // Horizontal Gaussian Blur
                    cmd.SetGlobalTexture(_blurContext.StaticSceneBlurTexture,_blurContext.TempRT.Identifier());
                    cmd.SetGlobalVector(_blurContext.UVOffset, new Vector2(_blurContext.BlurIntensity, 0));
                    if (_blurContext.Process == 0) RenderingUtils.Blit(cmd, _source.Identifier(), _blurContext.TempRT.Identifier(), _blurContext.BlurMaterial);
                    else RenderingUtils.Blit(cmd, _blurContext.BlurRT, _blurContext.TempRT.Identifier(), _blurContext.BlurMaterial);
                    cmd.SetRenderTarget(_source.Identifier());
                }
                else
                {
                    // Vertical Gaussian Blur
                    cmd.SetGlobalTexture(_blurContext.StaticSceneBlurTexture,_blurContext.BlurRT);
                    cmd.SetGlobalVector(_blurContext.UVOffset,new Vector2(0,_blurContext.BlurIntensity/_blurContext.ScreenRatio));
                    RenderingUtils.Blit(cmd,_blurContext.TempRT.Identifier(),_blurContext.BlurRT,_blurContext.BlurMaterial);
                    cmd.SetRenderTarget(_source.Identifier());
                }

                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
            }
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if((_blurContext.Process & 1) == 1)
                cmd.ReleaseTemporaryRT(_blurContext.TempRT.id);
            
            if (_blurContext.Process < _count)
            {
                if (_blurContext.Process == 0)
                    _blurContext.AlreadyAlreadyStartToRenderBlur();
                
                _blurContext.Process ++;
            }
            else
            {
                _blurContext.Process = 0;
                _blurContext.IsRenderBlurRT = false;
            }
        }
    }

    public class StaticGaussianBlurContext
    {
        private static StaticGaussianBlurContext _context;
        public static StaticGaussianBlurContext Context => _context;
        private StaticGaussianBlurContext()
        {
            TempRT.Init("RenderTargetHandle");
            UVOffset = Shader.PropertyToID("_UvOffset");
            StaticSceneBlurTexture = Shader.PropertyToID("_StaticSceneBlurTexture");
        }

        public delegate void AlreadyStart();
        public AlreadyStart AlreadyAlreadyStartToRenderBlur;
        
        public RenderTargetHandle TempRT;
        public RenderTexture BlurRT;
        public int Process;
        public bool IsRenderBlurRT;
        public Material BlurMaterial;
        public float BlurIntensity;
        public int UVOffset;
        public int StaticSceneBlurTexture; 
        public float ScreenRatio;

        public static StaticGaussianBlurContext Get()
        {
            if (_context == null)
            {
                _context = new StaticGaussianBlurContext();
            }
            return _context;
        }

        public void Clear()
        {
            _context = null;
        }
    }
}