using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace AssetCheck
{

    public class OverDrawRenderPipeline : RenderPipeline
    {
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach (Camera camera in cameras)
            {
                Render(context, camera);
            }
        }
        const string BUFF_NAME = "Render_Camera";
        static CommandBuffer s_CommandBuffer = new CommandBuffer
        {
            name = BUFF_NAME
        };
        CullingResults cullingResults;

        static Material s_OverOpaque = new Material(Shader.Find("Hidden/Overdraw-Opaque"));
        static Material s_OverTransparent = new Material(Shader.Find("Hidden/Overdraw-Transparent"));

        static ShaderTagId[] s_ShaderTagIds =
        {
        new ShaderTagId("SRPDefaultUnlit"),
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
    };

        public void Render(ScriptableRenderContext context, Camera camera)
        {
            if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
            {
                cullingResults = context.Cull(ref p);
            }
            else
            {
                return;
            }
            context.SetupCameraProperties(camera);
            s_CommandBuffer.ClearRenderTarget(true, true, Color.clear);
            s_CommandBuffer.BeginSample(BUFF_NAME);
            context.ExecuteCommandBuffer(s_CommandBuffer);
            s_CommandBuffer.Clear();

            var sortingSettings = new SortingSettings(camera) { criteria = SortingCriteria.CommonOpaque };
            var drawingSettings = new DrawingSettings(s_ShaderTagIds[0], sortingSettings);
            var filteringSetting = new FilteringSettings(RenderQueueRange.opaque);
            for (int i = 1; i < s_ShaderTagIds.Length; i++)
            {
                drawingSettings.SetShaderPassName(i, s_ShaderTagIds[i]);
            }
            if (camera.cameraType != CameraType.SceneView)
            {
                drawingSettings.overrideMaterial = s_OverOpaque;
            }
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSetting);

            sortingSettings = new SortingSettings(camera) { criteria = SortingCriteria.CommonTransparent };
            drawingSettings = new DrawingSettings(s_ShaderTagIds[0], sortingSettings);
            filteringSetting = new FilteringSettings(RenderQueueRange.transparent);
            for (int i = 1; i < s_ShaderTagIds.Length; i++)
            {
                drawingSettings.SetShaderPassName(i, s_ShaderTagIds[i]);
            }
            if (camera.cameraType != CameraType.SceneView)
                drawingSettings.overrideMaterial = s_OverTransparent;
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSetting);
            s_CommandBuffer.EndSample(BUFF_NAME);
            context.ExecuteCommandBuffer(s_CommandBuffer);
            s_CommandBuffer.Clear();
            context.Submit();
            EndCameraRendering(context, camera);
        }
    }
}