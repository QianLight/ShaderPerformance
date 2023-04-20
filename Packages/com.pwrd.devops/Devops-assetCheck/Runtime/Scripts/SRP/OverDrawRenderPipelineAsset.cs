using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace AssetCheck
{
    [CreateAssetMenu(menuName = "Rendering/OverDraw Render Pipline")]
    public class OverDrawRenderPipelineAsset : RenderPipelineAsset
    {
        protected override RenderPipeline CreatePipeline()
        {
            return new OverDrawRenderPipeline();
        }
    }
}