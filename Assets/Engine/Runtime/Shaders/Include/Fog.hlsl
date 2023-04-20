#ifndef PBS_FOG_INCLUDE
#define PBS_FOG_INCLUDE

float4 _GlobalFog;
#define psFogEnable _GlobalFog.x>0.5

float4 _FogColor;
float4 _LightOnFogParam;
#define _HaloSize _LightOnFogParam.x
#define _Intensity _LightOnFogParam.y

float4 _Exp2FogHorizontalParam;
#define _HDensity _Exp2FogHorizontalParam.x
#define _StartDistance _Exp2FogHorizontalParam.y
#define _HorizontalMax _Exp2FogHorizontalParam.z

float4 _Exp2FogVerticalParam;
#define _StartHeight _Exp2FogVerticalParam.x
#define _EndHeight _Exp2FogVerticalParam.y
#define _VDensity _Exp2FogVerticalParam.z
#define _VerticalMax _Exp2FogVerticalParam.w

float4 _Exp2FogSkyboxParam;
#define _Skybox_StartHeight _Exp2FogSkyboxParam.x
#define _Skybox_EndHeight _Exp2FogSkyboxParam.y
#define _Skybox_V_Density _Exp2FogSkyboxParam.z
#define _Skybox_H_Density _Exp2FogSkyboxParam.w

#include "../Scene/PrecomputedScatter.hlsl"

// Scatter settings 
float4 _ScatterParams;
#define _ScatterEnable _ScatterParams.x > 0
#define _ScatterScale _ScatterParams.x
#define _ScatterLerp _ScatterParams.z

// Color settings
float4 _FogStartColor;
float4 _FogEndColor;

// Remap settings
// for each variable:
//		x channel is distance fog
//		y channel is height fog
//		z channel is distance noise fog
//		w channel is height noise fog
float4 _FogStartParams;
float4 _FogEndParams;
float4 _FogFalloffParams;
float4 _FogIntensityMin;
float4 _FogIntensityMax;
float4 _FogIntensityScale;

// Noise settings
// these settings were based on base settings.
TEX3D_SAMPLER(_Noise3DTex);
#define INV_128 = 0.007813;
float4 _FogNoiseScaleOffset;
float4 _FogNoiseParams;
#define _FogNoiseOffset _FogNoiseScaleOffset.xyz
#define _FogNoiseScale _FogNoiseScaleOffset.w
#define _FogNoiseEnabled _FogNoiseParams.x
#define _FogNoiseWind _FogNoiseParams.yzw

#define APPLY_FOG(color, wpos) \
		float4 __fog = ApplyFog(wpos); \
		color.rgb = lerp(color.rgb, __fog.rgb, __fog.a);

#define distanceFog fogsFalloff.x
#define heightFog fogsFalloff.y
#define distanceNoise fogsFalloff.z
#define heightNoise fogsFalloff.w

#define linearstep2(min, max, x) saturate((x - min) / (max - min))

float4 ApplyFog(float3 wpos)
{
    float3 delta = wpos - _WorldSpaceCameraPos;
    float2 zy = float2(length(delta), wpos.y);

    // TODO: replace matrix with dot(cameraForward, delta)
    float depth01 = linearstep2(_ProjectionParams.y, _ProjectionParams.z, mul((float3x3)unity_WorldToCamera, delta).z);

    // FallOff
    // last version : float4 fogsFalloff = 1 - exp(-fogs * _FogFalloffParams);
    float4 fogs = linearstep2(_FogStartParams, _FogEndParams, max(0, zy.xyxy));
    float4 fogsFalloff = 1 - rcp(fogs * _FogFalloffParams + 1);
    fogsFalloff = min(_FogIntensityMax, _FogIntensityMin + fogsFalloff * _FogIntensityScale);

    float fogIntensity = distanceFog * heightFog;

    // Nosie
    UNITY_BRANCH
    if (_FogNoiseEnabled)
    {
        float noise = SAMPLE_TEX3D(_Noise3DTex, wpos * _FogNoiseScale + _FogNoiseOffset + _FogNoiseWind).x;
        float noiseIntensity = distanceNoise * heightNoise * noise;
        fogIntensity = max(fogIntensity, noiseIntensity);
    }

    // Color
    fogIntensity = min(1, fogIntensity);
    float4 fogColor = lerp(_FogStartColor, _FogEndColor, fogs.x);
    fogColor.a *= fogIntensity;

    // Scatter
    #ifndef FOG_SCATTER_OFF
    UNITY_BRANCH
    if (_ScatterEnable)
    {
        float3 viewDir = normalize(delta);
        float sunVdl = dot(viewDir, _MainLightDir0.xyz);
        float3 sunVdlScatter = GetFogSunVdlScatter(sunVdl);
        float3 sunDepthScatter = GetFogSunDepthScatter(depth01);
        float3 sun = sunVdlScatter * sunDepthScatter;
        float3 scatterColor = sun * _ScatterScale;
        // float3 scatterColor = sunVdlScatter * _ScatterScale;
        fogColor.rgb += scatterColor * fogColor.a;
    }
    #endif

    return fogColor;
}

#endif //PBS_FOG_INCLUDE
