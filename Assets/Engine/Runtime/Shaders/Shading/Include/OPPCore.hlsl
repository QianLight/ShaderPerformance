#ifndef URP_OPP_CORE
#define URP_OPP_CORE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

//#define SAMPLER2D(name) TEXTURE2D(name); SAMPLER(sampler##name); float4 name##_ST;
//#define TEX2D(name, uv) SAMPLE_TEXTURE2D(name, sampler##name, uv)
//#define TEX2DLOD(name, uv, lod) SAMPLE_TEXTURE2D_LOD(name, sampler##name, uv, lod)

half4 Sample2D(float2 uv, TEXTURE2D_PARAM(albedoAlphaMap, sampler_albedoAlphaMap))
{
    return SAMPLE_TEXTURE2D(albedoAlphaMap, sampler_albedoAlphaMap, uv);
}

half3 GetDefaultNormal()
{
    return half3(0.0h, 0.0h, 1.0h);
}

half GetAlpha(half4 color, half cutoff)
{
    half alpha = color.a;
#ifdef _ALPHATEST_ON
    clip(alpha - cutoff);
#endif
    return alpha;
}
#endif // UNIVERSAL_INPUT_SURFACE_PBR_INCLUDED
