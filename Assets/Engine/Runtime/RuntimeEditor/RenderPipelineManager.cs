using CFEngine;
using UnityEngine.Rendering;

public enum OPRenderPipeline
{
    Builtin,
    LegacySRP,
    URP,
}

public static class RenderPipelineManager
{
#if PIPELINE_URP
    public static OPRenderPipeline renderPipeline => OPRenderPipeline.URP;
#else
    public static OPRenderPipeline renderPipeline => EngineContext.UseSrp ? OPRenderPipeline.LegacySRP : OPRenderPipeline.Builtin;
#endif

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
    private static void OnAssimbliesLoaded()
    {
#if PIPELINE_URP
        EngineContext.UseUrp = true;
#else
        EngineContext.UseUrp = false;
#endif
    }
#endif
}
