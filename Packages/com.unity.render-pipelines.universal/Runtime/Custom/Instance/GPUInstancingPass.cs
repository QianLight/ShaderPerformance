using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GPUInstancingPass : ScriptableRenderPass
{
    protected ProfilingSampler m_InstanceSampler;
    private int m_PassIndex;

    internal GPUInstancingPass(URPProfileId id, RenderPassEvent renderPassEvent, int passIndex)
    {
        this.renderPassEvent = renderPassEvent; 
        m_InstanceSampler = ProfilingSampler.Get(id);
        m_PassIndex = passIndex;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CameraData cameraData = renderingData.cameraData;
        if (cameraData.renderType == CameraRenderType.Base && cameraData.camera.gameObject.layer == LayerMaskName.Default && GPUInstancingManager.Instance.NeedRender())
        {
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, m_InstanceSampler))
            {
                GPUInstancingManager.Instance.Render(cmd, m_PassIndex);
            }
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }
    }
}