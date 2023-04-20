using CFEngine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DecalPass : ScriptableRenderPass
{
    private readonly ProfilingSampler _profilingSampler;
    private readonly ShaderTagId _shaderTagId;
    private FilteringSettings filteringSettings;

    internal DecalPass(RenderPassEvent renderPassEvent, LayerMask layerMask)
    {
        base.profilingSampler = new ProfilingSampler(nameof(DecalPass));
        _profilingSampler = ProfilingSampler.Get(URPProfileId.DecalPass);
        this.renderPassEvent = renderPassEvent;
        _shaderTagId = new ShaderTagId("DecalForward");
        filteringSettings = new FilteringSettings(RenderQueueRange.opaque, layerMask);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, _profilingSampler))
        {
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            DrawingSettings drawingSettings = CreateDrawingSettings(_shaderTagId, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
            filteringSettings.renderingLayerMask = renderingData.renderLayerMask;
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);

            if (EngineContext.IsRunning)
            {
                if (!DecalManager.Instance.GetIsInit())
                {
                    if (EngineContext.instance != null)
                    {
                        DecalManager.Instance.Init(EngineContext.instance.CameraRef);
                    }
                }
            }

            if (DecalManager.Instance.GetIsInit())
            {
                DecalManager.Instance.DrawDecal(cmd);
            }
        }

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}