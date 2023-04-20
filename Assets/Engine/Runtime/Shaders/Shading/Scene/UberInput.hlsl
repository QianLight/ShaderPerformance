#ifndef UBER_INPUT_INCLUDED
#define UBER_INPUT_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
half4 _BaseColor;
half4 _EmissionColor;
half4 _ReflectionLow;
half4 _ColorTerr;
half4 _ReflectionHigh;
half4 _GradientEnd;
half4 _GradientBegin;
half4 _WorldPositionOffset;
half4 _WorldPositionScale;

float _GradientSwitch;
float _BlendMode;
half _NeedImpostor;
half _DistinguishAlpha;
//float _DitherTransparency_On;
half _IgnoreAlpha;
half _Cutoff;
half _Emission;
half _RoughnessMin;
half _RoughnessMax;
half _Metallic;
half _MetallicMin;
half _MetallicMax;
half _Occlusion;
half _BaseMapScale;
half _BumpScale;
half _BumpZ;
half _BumpW;
half _SpecularScale;
half _OcclusionScale;
half _DitherTransparency;
half _IBLScale;
half _DarkMetallicIBLOffset;
/////////////////////////   For vertical gradient color    //////////////////////
half _GradientScale;
half _BlendIntensity;
half _DarkEnviromentRefOffset;
half _TerrainOn, _BlendOffset, _BlendFactor;
half _Parkout;
half _Ripples;
CBUFFER_END

#ifdef _GLOBAL_SPECULAR_ADJUST_ENABLE
    half4 _sceneSpecularColorParams;
#endif

half4 _SceneColorAdjustmentParams;
#define _SceneExposure _SceneColorAdjustmentParams.x

TEXTURE2D(_BaseMap);            SAMPLER(sampler_BaseMap);
TEXTURE2D(_BumpMap);            SAMPLER(sampler_BumpMap);

#ifdef _BRIGHT_AO_ENABLE
half4 _BrightAOParams;
#endif

#endif
