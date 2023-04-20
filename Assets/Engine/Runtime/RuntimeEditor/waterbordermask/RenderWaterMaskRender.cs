using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RenderWaterMaskRender : RenderPipeline
{protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (Camera camera in cameras)
        {
            Render(context, camera);
        }
    }
    const string BUFF_NAME = "Render Water Mask Camera";
    static CommandBuffer s_CommandBuffer = new CommandBuffer
    {
        name = BUFF_NAME
    };
    CullingResults cullingResults;
 
    static Material s_OverQueue = new Material(Shader.Find("Unlit/waterborder"));
    static Material s_OverTransparent =new Material(Shader.Find("Unlit/waterborder"));
 
    //支持的ShaderTagID
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
        if(camera.name != "WaterMask")
        {
            return;
        }
        //处理裁切
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
 
        //排序设置-SortingCriteria.CommonOpaque表示默认不透明物体排序
        var sortingSettings = new SortingSettings(camera) { criteria = SortingCriteria.CommonOpaque};
        var drawingSettings = new DrawingSettings(s_ShaderTagIds[0], sortingSettings);
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        for (int i = 1; i < s_ShaderTagIds.Length; i++)
        {
            drawingSettings.SetShaderPassName(i, s_ShaderTagIds[i]);
        }
        //开始绘制不透明物体，SceneView视图中还用物体自身的Shader
        if (camera.cameraType != CameraType.SceneView)
            drawingSettings.overrideMaterial = s_OverQueue;//换shader
 
        context.DrawRenderers(
            cullingResults, ref drawingSettings, ref filteringSettings
        );
 
        //排序设置-SortingCriteria.CommonTransparent表示默认透明物体排序
        sortingSettings = new SortingSettings(camera) { criteria = SortingCriteria.CommonTransparent};
        drawingSettings = new DrawingSettings(s_ShaderTagIds[0], sortingSettings);
        filteringSettings = new FilteringSettings(RenderQueueRange.transparent);
        for (int i = 1; i < s_ShaderTagIds.Length; i++)
        {
            drawingSettings.SetShaderPassName(i, s_ShaderTagIds[i]);
        }
 
        //开始绘制透明物体，SceneView视图中还用物体自身的Shader
        if (camera.cameraType != CameraType.SceneView)
            drawingSettings.overrideMaterial = s_OverTransparent;//换shader
 
        context.DrawRenderers(
            cullingResults, ref drawingSettings, ref filteringSettings
        );
 
        s_CommandBuffer.EndSample(BUFF_NAME);
        context.ExecuteCommandBuffer(s_CommandBuffer);
        s_CommandBuffer.Clear();
        context.Submit();
 
    }
    
} 