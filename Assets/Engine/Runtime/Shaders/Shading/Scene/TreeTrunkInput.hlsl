#ifndef TREETRUNK_INPUT_INCLUDED
#define TREETRUNK_INPUT_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
half4 _BaseColor;
half4 _EmissionColor;
half _Emission;
half _Cutoff;
half _RoughnessMin;
half _RoughnessMax;
half _Metallic;
half _MetallicMin;
half _MetallicMax;
half _BumpScale;
half _Occlusion;
half _OcclusionScale;
half _DitherTransparency;
CBUFFER_END

TEXTURE2D(_BaseMap);            SAMPLER(sampler_BaseMap);
TEXTURE2D(_BumpMap);            SAMPLER(sampler_BumpMap);

#endif
