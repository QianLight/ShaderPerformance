using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace AssetCheck
{
    public class OverDrawSRPHelper
    {
        static RenderPipelineAsset lastRenderPipelineAsset;

        public static bool IsUseUniversalRenderPipeline()
        {
            return GraphicsSettings.renderPipelineAsset != null;
        }

        public static void SetRenderPipeline(RenderPipelineAsset asset)
        {
            lastRenderPipelineAsset = GraphicsSettings.renderPipelineAsset;
            GraphicsSettings.renderPipelineAsset = asset;
        }

        public static void ResetRenderPipeline()
        {
            if (lastRenderPipelineAsset == null)
                return;
            GraphicsSettings.renderPipelineAsset = lastRenderPipelineAsset;
            lastRenderPipelineAsset = null;
        }
    }
}