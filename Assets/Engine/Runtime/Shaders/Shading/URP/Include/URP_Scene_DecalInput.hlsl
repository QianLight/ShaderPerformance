#ifndef DECAL_INPUT2_INCLUDE
#define DECAL_INPUT2_INCLUDE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

CBUFFER_START(UnityPerMaterial)
float4 _BaseTex_ST;
half4 _BaseColor;
half4 _EmissionColor;
half _Cutoff;
half _Emission; 
half _RoughnessMin;
half _RoughnessMax;
half _Metallic;
half _MetallicMin;
half _MetallicMax;
half _Occlusion;
half _BumpScale;
half _BumpZ;
half _BumpW;
half _DitherTransparency;
half _DistinguishAlpha;

half _BaseColorInstensity;
half _IBLScale;
half _SpecularScale;
half _AlphaAdjust;
CBUFFER_END

half _Parkout;

#ifdef _BRIGHT_AO_ENABLE
half4 _BrightAOParams;
#endif

#ifdef _GLOBAL_SPECULAR_ADJUST_ENABLE
    half4 _sceneSpecularColorParams;
#endif

half4 _SceneColorAdjustmentParams;
#define _SceneExposure _SceneColorAdjustmentParams.x

TEXTURE2D(_BaseTex); SAMPLER(sampler_BaseTex);
TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);

//------------Lightmap----------------
TEXTURE2D(_CustomLightmap); SAMPLER(sampler_CustomLightmap);
TEXTURE2D(_CustomGroundColor); SAMPLER(sampler_CustomGroundColor);
float4x4 _BakeLightmapProjection;
//----------------------------

#endif

