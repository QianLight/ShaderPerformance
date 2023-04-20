using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WaveParticlePass : ScriptableRenderPass
{

    RenderTargetIdentifier src;
    RenderTargetHandle dest;

    public WaveParticlePass()
    {

    }
    RenderTargetHandle m_TemporaryColorTexture01;
    public void Setup(RenderTargetIdentifier src, RenderTargetHandle dest)
    {
        this.src = src;
        this.dest = dest;
        m_TemporaryColorTexture01.Init("_TemporaryColorTexture1");

    }
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        int width = renderingData.cameraData.cameraTargetDescriptor.width;

        int height = renderingData.cameraData.cameraTargetDescriptor.height;
        RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
        CommandBuffer cmd = CommandBufferPool.Get("WaveParticle");
        cmd.GetTemporaryRT(m_TemporaryColorTexture01.id, opaqueDesc, FilterMode.Bilinear);
        cmd.BeginSample("WaveParticle");
        cmd.SetRenderTarget(m_TemporaryColorTexture01.Identifier());
        cmd.ClearRenderTarget(true, true,Color.black);
        cmd.SetGlobalTexture("_WaveTex", m_TemporaryColorTexture01.id);
        cmd.EndSample("WaveParticle");
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        base.FrameCleanup(cmd);
        cmd.ReleaseTemporaryRT(m_TemporaryColorTexture01.id);
    }
}
