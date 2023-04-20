//
// URP Debug Views for Unity
// (c) 2019 PH Graphics
//

using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

namespace URPDebugViews
{
    public class DebugViewPass : ScriptableRenderPass
    {
        private readonly List<ShaderTagId> _shaderTagIdList = new List<ShaderTagId>();

        private const string ProfilerTag = "Debug View Pass";

        private RenderStateBlock _renderStateBlock;
        private FilteringSettings _filteringSettings;

        public DebugViewPass()
        {
            _filteringSettings = new FilteringSettings(RenderQueueRange.all);

            _shaderTagIdList.Add(new ShaderTagId("UniversalForward"));
            _shaderTagIdList.Add(new ShaderTagId("LightweightForward"));
            _shaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit")); 
            
            _renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);

            // might be exposed later on, at the moment it's used only by wireframe
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var viewData = DebugViewsManager.Instance.CurrentViewData;

            if (!viewData)
                return;

            CommandBuffer cmd = CommandBufferPool.Get(ProfilerTag);
            using (new ProfilingSample(cmd, ProfilerTag))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
                DrawingSettings drawingSettings = CreateDrawingSettings(_shaderTagIdList, ref renderingData, sortingCriteria);
                drawingSettings.overrideMaterialPassIndex = 0;
                drawingSettings.overrideMaterial = viewData ? viewData.MatTransparent : null;
                
                // debug view drawing
                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref _filteringSettings,
                    ref _renderStateBlock);
                
            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
