#ifndef PBS_FOG_INCLUDE
#define PBS_FOG_INCLUDE

#include "../API/CommonAPI.hlsl"

FLOAT4 _GlobalFog;
#define psFogEnable _GlobalFog.x>0.5

FLOAT4 _FogColor;
FLOAT4 _LightOnFogParam;
#define _HaloSize _LightOnFogParam.x
#define _Intensity _LightOnFogParam.y

FLOAT4 _Exp2FogHorizontalParam;
#define _HDensity _Exp2FogHorizontalParam.x
#define _StartDistance _Exp2FogHorizontalParam.y
#define _HorizontalMax _Exp2FogHorizontalParam.z

FLOAT4 _Exp2FogVerticalParam;
#define _StartHeight _Exp2FogVerticalParam.x
#define _EndHeight _Exp2FogVerticalParam.y
#define _VDensity _Exp2FogVerticalParam.z
#define _VerticalMax _Exp2FogVerticalParam.w

FLOAT4 _Exp2FogSkyboxParam;
#define _Skybox_StartHeight _Exp2FogSkyboxParam.x
#define _Skybox_EndHeight _Exp2FogSkyboxParam.y
#define _Skybox_V_Density _Exp2FogSkyboxParam.z
#define _Skybox_H_Density _Exp2FogSkyboxParam.w

#define PRECOMPUTE_SCATTER

#include "../../Include/PrecomputedScatter.hlsl"



// Scatter settings
FLOAT4 _FogMiscParams;
#define _ScatterEnable _FogMiscParams.x > 0
#define _ScatterScale _FogMiscParams.x
#define _FogIntensity _FogMiscParams.y 
#define _FogEnable _FogIntensity > 0
#define _FogNoiseDensity _FogMiscParams.z
#define _BaseFogEnable _FogMiscParams.w > 0

// Color settings
FLOAT4 _FogStartColor;
FLOAT4 _FogEndColor;
FLOAT4 _FogBottomColor;
FLOAT4 _FogTopColor;
TYPE _FogShaftOffset;

// Remap settings
// for each variable:
//		x channel is distance fog
//		y channel is height fog
//		z channel is distance noise fog
//		w channel is height noise fog
FLOAT4 _FogStartParams;
FLOAT4 _FogEndParams;
FLOAT4 _FogFalloffParams;
FLOAT4 _FogIntensityMin;
FLOAT4 _FogIntensityMax;
FLOAT4 _FogIntensityScale;

// Noise settings
// these settings were based on base settings.
TEX3D_SAMPLER(_Noise3DTex);
#define INV_128 = 0.007813;
FLOAT4 _FogNoiseScaleOffset;
#define _FogNoiseOffset _FogNoiseScaleOffset.xyz
#define _FogNoiseScale _FogNoiseScaleOffset.w
#define _FogNoiseEnable (_FogNoiseDensity > 0)

#define linearstep2(min, max, x) saturate((x - min) / (max - min))

#ifdef FOG_OFF
    #define APPLY_FOG(color, wpos)
#else
    #define APPLY_FOG(color, wpos) \
    FLOAT4 __fog = ApplyFog(wpos); \
    color.rgb = lerp(color.rgb, __fog.rgb, __fog.a);
#endif

uniform FLOAT _FogToggle;

FLOAT4 ApplyFog(FLOAT3 wpos)
{
    TYPE3 delta = wpos - _WorldSpaceCameraPos;
    FLOAT2 zy = FLOAT2(length(delta), wpos.y);
    FLOAT4 fogs = linearstep2(_FogStartParams, _FogEndParams, max(0, zy.xyxy));
    fogs.y *= fogs.y;

    
    
    // FallOff
    #ifdef FOG_OPTIMIZE_EXP // 这个性能好一些，但是效果没exp好。
    FLOAT4 fogsFalloff = 1 - rcp(fogs * _FogFalloffParams + 1);
    #else
    FLOAT4 fogsFalloff = 1 - exp2(-fogs * _FogFalloffParams * 1.5);
    #endif
    fogsFalloff = saturate(min(_FogIntensityMax, _FogIntensityMin + fogsFalloff));

    // Base Fog
    FLOAT distanceFog = fogsFalloff.x;
    FLOAT heightFog = fogsFalloff.y;
    FLOAT fogIntensity = saturate(distanceFog * heightFog);
    // Nosie
    // TODO: 顶点雾效
    #if /*!defined(_SHADER_LEVEL_LOW) && !defined(_SHADER_LEVEL_MEDIUM) &&*/ !defined(FOG_NOISE_OFF)
    UNITY_BRANCH
    if (_FogNoiseEnable)
    {
        FLOAT distanceNoise = fogsFalloff.z;
        FLOAT heightNoise = fogsFalloff.w;
        FLOAT noise = saturate(_FogNoiseDensity - SAMPLE_TEX3D(_Noise3DTex, wpos * _FogNoiseScale + _FogNoiseOffset).x);
        FLOAT noiseIntensity = distanceNoise * heightNoise * noise;
        fogIntensity = max(fogIntensity, noiseIntensity);
    }
    #endif

    // Color
    fogIntensity = saturate(fogIntensity * _FogIntensity);
    FLOAT4 fogColor = lerp(_FogStartColor, _FogEndColor, saturate(distanceFog));
    // fogColor = lerp(fogColor, _FogTopColor, heightFog);
    TYPE3 realLightDir = _MainLightDir0.xyz;
    #ifdef _FAKELIGHT
    realLightDir = _FakeLightDir.xyz;
    #endif
    TYPE vdl = saturate(dot(normalize(delta), realLightDir));
    vdl = saturate(1/_FogShaftOffset * vdl +(1-1/_FogShaftOffset));
    vdl = vdl * vdl;
    fogColor = lerp(fogColor, _FogTopColor, vdl);
    // FLOAT4 heightFogColor = lerp(_FogTopColor, _FogBottomColor, fogs.y);
    // FLOAT4 fogColor = lerp(distanceFogColor, heightFogColor, fogs.x/(fogs.x + fogs.y));
    fogColor.a = saturate(fogColor.a * fogIntensity);
    // Scatter
    // TODO: 顶点雾效
    #if defined(_SHADER_LEVEL_HIGH) && !defined(FOG_SCATTER_OFF)
    UNITY_BRANCH
    if (_ScatterEnable)
    {
        FLOAT depth01 = linearstep2(_FogStartParams.x, _FogEndParams.x, dot(_CameraBackward.xyz, delta));
        FLOAT3 viewDir = normalize(delta);
        FLOAT sunVdl = dot(viewDir, realLightDir.xyz);
        FLOAT3 sunVdlScatter = GetFogSunVdlScatter(sunVdl);
        FLOAT3 sunDepthScatter = GetFogSunDepthScatter(depth01);
        FLOAT3 sun = sunVdlScatter * sunDepthScatter;
        FLOAT3 scatterColor = sun * _ScatterScale;
        fogColor.rgb += scatterColor;
    }
    // fogColor.rgb = fogIntensity.xxx;
    #endif
    fogColor = max(0, fogColor);
    return fogColor * (1 - _FogToggle);
}

#endif //PBS_FOG_INCLUDE
