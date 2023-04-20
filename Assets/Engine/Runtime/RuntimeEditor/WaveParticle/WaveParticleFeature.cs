using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WaveParticleFeature : ScriptableRendererFeature
{

    public enum RenderTarget
    {
        color,
        texture
    }

    public string m_textureName = "_WaveTex";
    public RenderTarget renderTarget;
    RenderTargetHandle m_RenderTextureHandle;
    WaveParticlePass pass;

    public override void Create()
    {
        m_RenderTextureHandle.Init(m_textureName);
        pass = new WaveParticlePass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var src = renderer.cameraColorTarget;
        var dest = m_RenderTextureHandle;
        pass.Setup(src, dest);
        renderer.EnqueuePass(pass);
    }
}
