using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Rendering.RenderPipelineManager;
using Object = UnityEngine.Object;

namespace CFEngine
{
    public abstract class BlurCore
    {
        public abstract void Excecute();
        public abstract void OnClear();
        protected void EnqueuePassCrabColor(Camera cameraGrabColor ,EndCrabColorPass endCrabColorPass, BlurContext blurContext)
        {
            if (!cameraGrabColor.TryGetComponent<UniversalAdditionalCameraData>(out var uacd))
            {
                return;
            } 
            if (blurContext.blurRT == null || blurContext.tmpRT == null)
            {
                float scale = 1f;
                if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset asset)
                {
                    scale = asset.renderScale;
                }
                 
                int width = (int)(cameraGrabColor.pixelWidth * scale);
                int height = (int)(cameraGrabColor.pixelHeight * scale);
                RenderTextureDescriptor descriptor = new RenderTextureDescriptor(width, height,GraphicsFormat.R8G8B8A8_UNorm,0);
                blurContext.blurRT = RenderTexture.GetTemporary(descriptor);
                blurContext.tmpRT = RenderTexture.GetTemporary(descriptor);
                blurContext.blurRT.name = "_StaticSceneBlurTexture";
                blurContext.tmpRT.name = "_StaticSceneTmpTexture";
            }
            GrabColorPass grabcolorPass = new GrabColorPass();
            grabcolorPass.Setup(blurContext.tmpRT,RenderPassEvent.BeforeRenderingPostProcessing,endCrabColorPass);
            uacd.scriptableRenderer.EnqueuePass(grabcolorPass);
        }
         
        protected void Blur(ScriptableRenderContext renderContext, Camera camera , BlurContext blurContext)
        {
            if (blurContext.process == blurContext.iteration)
            {
                return;
            }
            
            var cmd = CommandBufferPool.Get();
            cmd.Clear();
            if((blurContext.process & 1) != 1)
            {
                // Horizontal Gaussian Blur
                cmd.SetGlobalTexture(blurContext.staticSceneBlurTexture,blurContext.tmpRT);
                cmd.SetGlobalTexture(blurContext.source,blurContext.tmpRT);
                cmd.SetGlobalVector(blurContext.uvOffset, new Vector2(blurContext.BlurIntensity, 0));
                cmd.Blit(blurContext.tmpRT, blurContext.blurRT, blurContext.gaussianBlurMaterial);
            }
            else
            {
                // Vertical Gaussian Blur
                cmd.SetGlobalTexture(blurContext.staticSceneBlurTexture,blurContext.blurRT);
                cmd.SetGlobalTexture(blurContext.source,blurContext.blurRT);
                cmd.SetGlobalVector(blurContext.uvOffset,new Vector2(0,(float)blurContext.BlurIntensity * camera.aspect));
                cmd.Blit(blurContext.blurRT, blurContext.tmpRT, blurContext.gaussianBlurMaterial);
            }
            renderContext.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
            blurContext.process++;
        }
    }
    
    public class BlurContext
    {
        public GameObject rendererGameObject;
        public RenderTexture blurRT;
        public RenderTexture tmpRT;
        
        public Material gaussianBlurMaterial;
        public float BlurIntensity = 0.003f;
        
        public int uvOffset = Shader.PropertyToID("_UvOffset");
        public int staticSceneBlurTexture = Shader.PropertyToID("_StaticSceneBlurTexture");
        public int source = Shader.PropertyToID("_SourceTex");
        public int savedLayermask;
        public bool settingsIsDirty;

        public static readonly int UiColor = Shader.PropertyToID("_Color");
        public static readonly int UiOpacity = Shader.PropertyToID("_Opacity");
        
        public int process = 0;
        public int iteration = 0;
        public BlurContext(GameObject rendererGameObject ,Material materialForBlur, int iteration)
        {
            this.rendererGameObject = rendererGameObject;
            gaussianBlurMaterial = materialForBlur;
            this.iteration = iteration;
        }
        
        public void Clear()
        {
            process = 0;
            RenderTexture.ReleaseTemporary(blurRT);
            RenderTexture.ReleaseTemporary(tmpRT);
        }
    }
    
    public delegate void EndCrabColorPass(Camera currentCamera);
    public class GrabColorPass : ScriptableRenderPass
    {
        private RenderTexture _renderTexture;
        private RenderTargetIdentifier _identifier;
        private ProfilingSampler _profilingSampler = new ProfilingSampler("GrabColor");
        private EndCrabColorPass _endCrabColorPass = (camera) => { };
        private Camera _camera;
        public void Setup(RenderTexture renderTexture,RenderPassEvent passEventvent)
        {
            _renderTexture = renderTexture;
            renderPassEvent = passEventvent;
        }
        
        public void Setup(RenderTexture renderTexture,RenderPassEvent passEventvent, EndCrabColorPass endCrabColorPass) 
        {
            Setup(renderTexture,passEventvent);
            _endCrabColorPass = endCrabColorPass;
        }
    
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            _camera = renderingData.cameraData.camera;
            _identifier = renderingData.cameraData.renderer.cameraColorTarget;
            var cmd = CommandBufferPool.Get();
            // using (new ProfilingScope(cmd, _profilingSampler))
            // {
                cmd.Clear();
                cmd.Blit(_identifier,_renderTexture);
            // }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            _endCrabColorPass(_camera);
        }
    }
}