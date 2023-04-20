using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
public class ClearColorTextureFeature : ScriptableRendererFeature
{   
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent RenderEvent = RenderPassEvent.AfterRenderingTransparents;
        public Material clearMaterial = null;
    }

    public Settings settings = new Settings();
    ClearColorTexturePass pass;

    public override void Create()
    {
        pass = new ClearColorTexturePass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var src = renderer.cameraColorTarget;
        var dest = RenderTargetHandle.CameraTarget;

        pass.Setup(src,dest);        
        renderer.EnqueuePass(pass);
    }


    public class ClearColorTexturePass :ScriptableRenderPass
    {
        RenderTargetIdentifier source;
        RenderTargetHandle dest;        

        Material clearMaterial;

        public ClearColorTexturePass(Settings settings)
        {
            renderPassEvent = settings.RenderEvent;
            clearMaterial = settings.clearMaterial;
        }
        public void Setup(RenderTargetIdentifier src,RenderTargetHandle dest)
        {
            this.source  = src;
            this.dest = dest;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Clear");
            cmd.BeginSample("Clear");
            cmd.ClearRenderTarget(true,true,Color.black);
            cmd.EndSample("Clear");
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
