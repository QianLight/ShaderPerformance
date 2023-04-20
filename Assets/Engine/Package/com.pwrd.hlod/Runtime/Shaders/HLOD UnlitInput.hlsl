#ifndef HLOD_UNLIT_INPUT_INCLUDED
#define HLOD_UNLIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _BaseMap_ST;
    half4 _BaseColor;
    half _Cutoff;
    float4 _BaseMap_TexelSize;
    float4 _TextureAtlasing_LocalMetric;
    half _SelfNormalBias;
    half _SelfDepthBias;
 CBUFFER_END

    //half _Glossiness;
    //half _Metallic;


#endif
