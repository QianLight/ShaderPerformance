#ifndef URP_GRASSINPUT_INCLUDED
#define URP_GRASSINPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Effects.hlsl"

CBUFFER_START(UnityPerMaterial)

float4 _MainTex_ST;
float4 _AmbientWind_ST;
float4 _Color0;
float4 _ColorR;
float4 _ColorG;
float4 _ColorB;
float4 _ColorSSS;
float4 _ColorSpecular;

float _BottomPersent;
float _BottomScale;
float _SSSSpecular;
float _Specular;
float _SpecularRamp;
float _Smoothness;
float _Cutoff;
float _AlphaTest;
float _AmbientWindDirx;
float _AmbientWindDirz;
float _AmbientWindSpeed;

float _GustingStrength;
float _GustingFrequency;
float _WeightRG;
float _WeightGB;
float _Transvalue;
float _PushAngle;
float _PushValue;
CBUFFER_END
float4x4 _BakeLightmapProjection;
float4 _GrassNormalDir;
float4 _SSSlightcontrol;
float _DitherTransparency;

half4 _SceneColorAdjustmentParams;
#define _SceneExposure _SceneColorAdjustmentParams.x

TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);
TEXTURE2D(_CustomLightmap);
SAMPLER(sampler_CustomLightmap);
TEXTURE2D(_CustomGroundColor);
SAMPLER(sampler_CustomGroundColor);
TEXTURE2D(_ProcedureTex3);
SAMPLER(sampler_ProcedureTex3);
TEXTURE2D(_ProcedureTex4);
SAMPLER(sampler_ProcedureTex4);
TEXTURE2D(_AmbientWind);
SAMPLER(sampler_AmbientWind);
TEXTURE2D(_CollisionTex);
SAMPLER(sampler_CollisionTex);

half3 GrassLightingSpecular(half3 lightColor, half3 lightDir, half3 posws, half3 viewDir, half uvy, half3 realnormal)
{
    half LVangle = saturate(dot(_SSSlightcontrol.xyz, viewDir));
    half3 HVec = SafeNormalize(normalize(-_SSSlightcontrol.xyz) + normalize(viewDir));
    half NdotH = max(dot(half3(0, 1, 0), HVec), 0);
    half modifier = pow(NdotH, _Smoothness);

    half lightvsviewfan = saturate(dot(lightDir, viewDir));
    half NdotHfan = max(dot(realnormal, HVec), 0);
    half specularfan = pow(NdotHfan, _Smoothness * 2) * 0.1;
    half3 specularReflection = half3(1, 1, 1) * modifier * pow(abs(uvy), _SpecularRamp) * _ColorSSS.xyz;
    //    half3 specularReflection = half3(1,1,1) * dis*uvy*uvy*uvy*_SSSSpecular;
    half3 specularReflectionfan = specularfan * uvy * _Specular * _ColorSpecular.xyz;

    return saturate(lightColor * (specularReflection + specularReflectionfan));
}


half4 GrassBlinnPhong(InputData inputData, half3 diffuse, half3 emission, half alpha, half3 lightmap, half3 realnormal)
{
    // To ensure backward compatibility we have to avoid using shadowMask input, as it is not present in older shaders
    #if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
    half4 shadowMask = inputData.shadowMask;
    #elif !defined (LIGHTMAP_ON)
    half4 shadowMask = unity_ProbesOcclusion;
    #else
    half4 shadowMask = half4(1, 1, 1, 1);
    #endif

    Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, shadowMask,inputData.normalWS.xyz);
    #if defined(_SHADOW_COLOR_ENABLE)
    half3 attenuatedLightColor = mainLight.color * (mainLight.distanceAttenuation * mainLight.shadowAttenuation) * saturate(dot(inputData.normalWS, mainLight.direction)) +
        mainLight.color * (1 - mainLight.shadowAttenuation) * urp_ShadowColor.rgb;

    #else
    half3 attenuatedLightColor = mainLight.color * (mainLight.distanceAttenuation * mainLight.shadowAttenuation);
    #endif
    half3 specularColor = GrassLightingSpecular(attenuatedLightColor, mainLight.direction, inputData.positionWS.xyz,
                                                inputData.viewDirectionWS, emission.x, realnormal);
    half3 diffuseColor = lightmap + attenuatedLightColor;
    
    half3 finalColor = diffuseColor * diffuse;
    finalColor += specularColor * diffuse * _SSSSpecular;
    CalcSceneColor(finalColor, mainLight.shadowAttenuation);
    finalColor *= _SceneExposure;
    return half4(finalColor, alpha);
}


#endif // UNIVERSAL_INPUT_SURFACE_PBR_INCLUDED
