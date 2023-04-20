#ifndef UNIVERSAL_SHADOWS_INCLUDED
#define UNIVERSAL_SHADOWS_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
// #define real float
// #define real2 float2
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl"
#include "Core.hlsl"
//#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RoleShadow.hlsl"
float4x4 _LightProjection;

#define SHADOWS_SCREEN 0
#define MAX_SHADOW_CASCADES 4
#define _CLOUDSHADOW_ON 

#if !defined(_RECEIVE_SHADOWS_OFF)
#if defined(_MAIN_LIGHT_SHADOWS)
#define MAIN_LIGHT_CALCULATE_SHADOWS
#if !defined(_MAIN_LIGHT_SHADOWS_CASCADE)
#define REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
#endif
#endif

#if defined(_ADDITIONAL_LIGHT_SHADOWS)
#define ADDITIONAL_LIGHT_CALCULATE_SHADOWS
#endif
#endif

#if defined(UNITY_DOTS_INSTANCING_ENABLED)
#define SHADOWMASK_NAME unity_ShadowMasks
#define SHADOWMASK_SAMPLER_NAME samplerunity_ShadowMasks
#define SHADOWMASK_SAMPLE_EXTRA_ARGS , unity_LightmapIndex.x
#else
#define SHADOWMASK_NAME unity_ShadowMask
#define SHADOWMASK_SAMPLER_NAME samplerunity_ShadowMask
#define SHADOWMASK_SAMPLE_EXTRA_ARGS
#endif

#if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
#define SAMPLE_SHADOWMASK(uv) SAMPLE_TEXTURE2D_LIGHTMAP(SHADOWMASK_NAME, SHADOWMASK_SAMPLER_NAME, uv SHADOWMASK_SAMPLE_EXTRA_ARGS);
#elif !defined (LIGHTMAP_ON)
#define SAMPLE_SHADOWMASK(uv) unity_ProbesOcclusion;
#else
#define SAMPLE_SHADOWMASK(uv) half4(1, 1, 1, 1);
#endif

#define REQUIRES_WORLD_SPACE_POS_INTERPOLATOR

#if defined(LIGHTMAP_ON) || defined(LIGHTMAP_SHADOW_MIXING) || defined(SHADOWS_SHADOWMASK)
#define CALCULATE_BAKED_SHADOWS
#endif

SCREENSPACE_TEXTURE(_ScreenSpaceShadowmapTexture);
SAMPLER(sampler_ScreenSpaceShadowmapTexture);

//#define _HUAWEI

#if defined(SHADER_API_MOBILE) || defined(SHADER_API_SWITCH) /*|| defined(_HUAWEI)*/
#define TEXTURE2D_SHADOW_PARAM_URP(textureName, samplerName)          TEXTURE2D_FLOAT(textureName),         SAMPLER(samplerName)
TEXTURE2D_FLOAT(_MainLightShadowmapTexture);
SAMPLER(sampler_MainLightShadowmapTexture);

TEXTURE2D_FLOAT(_AdditionalLightsShadowmapTexture);
SAMPLER(sampler_AdditionalLightsShadowmapTexture);
#else
#define TEXTURE2D_SHADOW_PARAM_URP(textureName, samplerName)          TEXTURE2D(textureName),         SAMPLER_CMP(samplerName)
TEXTURE2D_SHADOW(_MainLightShadowmapTexture);
SAMPLER_CMP(sampler_MainLightShadowmapTexture);

TEXTURE2D_SHADOW(_AdditionalLightsShadowmapTexture);
SAMPLER_CMP(sampler_AdditionalLightsShadowmapTexture);
#endif

//zh
#if defined(_CLOUDSHADOW_ON)
TEXTURE2D(_CloudShadowTex);
SAMPLER(sampler_CloudShadowTex);
#endif
//CBUFFER_START(UnityPerMaterial)
#if defined(_CLOUDSHADOW_ON)
TYPE4 _CloudShadowTex_ST;
half _CloudShadowIntensity;
TYPE _CloudPannerX, _CloudPannerY;
TYPE _CloudVerticalOffset;
TYPE _CloudVerticalOffsetIntensity;
TYPE _CloudVerticalAttenuation;
matrix _MainLightSpaceMatrix;
TYPE _MainTil;
#endif
//CBUFFER_END
// GLES3 causes a performance regression in some devices when using CBUFFER.
#ifndef SHADER_API_GLES3
CBUFFER_START(MainLightShadows)
#endif
// Last cascade is initialized with a no-op matrix. It always transforms
// shadow coord to half3(0, 0, NEAR_PLANE). We use this trick to avoid
// branching since ComputeCascadeIndex can return cascade index = MAX_SHADOW_CASCADES
float4x4 _MainLightWorldToShadow[MAX_SHADOW_CASCADES + 1];
float4 _CascadeShadowSplitSpheres0;
float4 _CascadeShadowSplitSpheres1;
float4 _CascadeShadowSplitSpheres2;
float4 _CascadeShadowSplitSpheres3;
float4 _CascadeShadowSplitSphereRadii;
half4 _MainLightShadowOffset0;
half4 _MainLightShadowOffset1;
half4 _MainLightShadowOffset2;
half4 _MainLightShadowOffset3;
half4 _MainLightShadowParams;
// (x: shadowStrength, y: 1.0 if soft shadows, 0.0 otherwise, z: oneOverFadeDist, w: minusStartFade)
float4 _MainLightShadowmapSize; // (xy: 1/width and 1/height, zw: width and height)
#ifndef SHADER_API_GLES3
CBUFFER_END
#endif

#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
StructuredBuffer<ShadowData> _AdditionalShadowsBuffer;
StructuredBuffer<int> _AdditionalShadowsIndices;
half4       _AdditionalShadowOffset0;
half4       _AdditionalShadowOffset1;
half4       _AdditionalShadowOffset2;
half4       _AdditionalShadowOffset3;
float4      _AdditionalShadowmapSize; // (xy: 1/width and 1/height, zw: width and height)
#else
// GLES3 causes a performance regression in some devices when using CBUFFER.
#ifndef SHADER_API_GLES3
CBUFFER_START(AdditionalLightShadows)
#endif
float4x4 _AdditionalLightsWorldToShadow[MAX_VISIBLE_LIGHTS];
half4 _AdditionalShadowParams[MAX_VISIBLE_LIGHTS];
half4 _AdditionalShadowOffset0;
half4 _AdditionalShadowOffset1;
half4 _AdditionalShadowOffset2;
half4 _AdditionalShadowOffset3;
float4 _AdditionalShadowmapSize; // (xy: 1/width and 1/height, zw: width and height)
#ifndef SHADER_API_GLES3
CBUFFER_END
#endif
#endif

float4 _ShadowBias; // x: depth bias, y: normal bias

#define BEYOND_SHADOW_FAR(shadowCoord) shadowCoord.z <= 0.0 || shadowCoord.z >= 1.0

struct ShadowSamplingData
{
    half4 shadowOffset0;
    half4 shadowOffset1;
    half4 shadowOffset2;
    half4 shadowOffset3;
    float4 shadowmapSize;
};

ShadowSamplingData GetMainLightShadowSamplingData()
{
    ShadowSamplingData shadowSamplingData;
    shadowSamplingData.shadowOffset0 = _MainLightShadowOffset0;
    shadowSamplingData.shadowOffset1 = _MainLightShadowOffset1;
    shadowSamplingData.shadowOffset2 = _MainLightShadowOffset2;
    shadowSamplingData.shadowOffset3 = _MainLightShadowOffset3;
    shadowSamplingData.shadowmapSize = _MainLightShadowmapSize;
    return shadowSamplingData;
}

ShadowSamplingData GetAdditionalLightShadowSamplingData()
{
    ShadowSamplingData shadowSamplingData;
    shadowSamplingData.shadowOffset0 = _AdditionalShadowOffset0;
    shadowSamplingData.shadowOffset1 = _AdditionalShadowOffset1;
    shadowSamplingData.shadowOffset2 = _AdditionalShadowOffset2;
    shadowSamplingData.shadowOffset3 = _AdditionalShadowOffset3;
    shadowSamplingData.shadowmapSize = _AdditionalShadowmapSize;
    return shadowSamplingData;
}

// ShadowParams
// x: ShadowStrength
// y: 1.0 if shadow is soft, 0.0 otherwise
half4 GetMainLightShadowParams()
{
    return _MainLightShadowParams;
}


// ShadowParams
// x: ShadowStrength
// y: 1.0 if shadow is soft, 0.0 otherwise
half4 GetAdditionalLightShadowParams(int lightIndex)
{
#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
    return _AdditionalShadowsBuffer[lightIndex].shadowParams;
#else
    return _AdditionalShadowParams[lightIndex];
#endif
}

half SampleScreenSpaceShadowmap(float4 shadowCoord)
{
    shadowCoord.xy /= shadowCoord.w;

    // The stereo transform has to happen after the manual perspective divide
    shadowCoord.xy = UnityStereoTransformScreenSpaceTex(shadowCoord.xy);

#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
    half attenuation = SAMPLE_TEXTURE2D_ARRAY(_ScreenSpaceShadowmapTexture, sampler_ScreenSpaceShadowmapTexture, shadowCoord.xy, unity_StereoEyeIndex).x;
#else
    half attenuation = SAMPLE_TEXTURE2D(_ScreenSpaceShadowmapTexture, sampler_ScreenSpaceShadowmapTexture,
        shadowCoord.xy).x;
#endif

    return attenuation;
}

// real SampleShadowmapFiltered(
//     TEXTURE2D_SHADOW_PARAM(ShadowMap, sampler_ShadowMap), float4 shadowCoord, ShadowSamplingData samplingData)
// {
//     real attenuation;
//     real4 attenuation4;
//     #if defined(_SHADER_LEVEL_HIGH)
//     real fetchesWeights[9];
//     real2 fetchesUV[9];
//     SampleShadow_ComputeSamples_Tent_5x5(samplingData.shadowmapSize, shadowCoord.xy, fetchesWeights, fetchesUV);
//
//     attenuation4.x = SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[1].xy, shadowCoord.z));
//     attenuation4.y = SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[3].xy, shadowCoord.z));
//     attenuation4.z = SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[5].xy, shadowCoord.z));
//     attenuation4.w = SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[7].xy, shadowCoord.z));
//
//     real center = SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[4].xy, shadowCoord.z));
//     attenuation = dot(attenuation4, 0.25f);
//
//     UNITY_BRANCH
//     //if (abs(attenuation - 0.5) > 0.499f)
//     if (center == attenuation)
//         return attenuation;
//
//     attenuation = fetchesWeights[0] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[0].xy, shadowCoord.z));
//     attenuation += fetchesWeights[1] * attenuation4.x;
//     attenuation += fetchesWeights[2] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[2].xy, shadowCoord.z));
//     attenuation += fetchesWeights[3] * attenuation4.y;
//     attenuation += fetchesWeights[4] * center;
//     attenuation += fetchesWeights[5] * attenuation4.z;
//     attenuation += fetchesWeights[6] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[6].xy, shadowCoord.z));
//     attenuation += fetchesWeights[7] * attenuation4.w;
//     attenuation += fetchesWeights[8] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, float3(fetchesUV[8].xy, shadowCoord.z));
//
//     #else
//     // 4-tap hardware comparison
//     attenuation4.x = SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap,
//                                              shadowCoord.xyz + samplingData.shadowOffset0.xyz);
//     attenuation4.y = SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap,
//                                              shadowCoord.xyz + samplingData.shadowOffset1.xyz);
//     attenuation4.z = SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap,
//                                              shadowCoord.xyz + samplingData.shadowOffset2.xyz);
//     attenuation4.w = SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap,
//                                              shadowCoord.xyz + samplingData.shadowOffset3.xyz);
//     attenuation = dot(attenuation4, 0.25);
//     #endif
//     return attenuation;
// }

real SampleShadowCompare(float shadow, float z)
{
#if SHADER_API_OPENGL || SHADER_API_GLES || SHADER_API_GLES3
    return step(z - shadow, 0);
#else
    return step(0, z - shadow);
#endif	
}

float4 SampleShadowCompare(float4 shadow, float z)
{
#if SHADER_API_OPENGL || SHADER_API_GLES || SHADER_API_GLES3
    return step(float4(z, z, z, z) - shadow, float4(0, 0, 0, 0));
#else
    return step(float4(0, 0, 0, 0), float4(z, z, z, z) - shadow);
#endif	
}

real SampleShadowDepth(TEXTURE2D_PARAM(ShadowMap, sampler_ShadowMap), float2 shadowCoord, float z)
{
    return SampleShadowCompare(SAMPLE_TEXTURE2D(ShadowMap, sampler_ShadowMap, shadowCoord).r, z);
}

void MakePcfUV(float2 uv, float4 rtSize, out float4 pcfFilter)
{
    //uv -= rtSize.xy * float2(0.5f, 0.5f);
    //pcfFilter = float4(floor(uv * rtSize.zw) / rtSize.zw, frac(uv * rtSize.zw));
    float2 uvBlock = uv * rtSize.zw;
    float2 blockIndex = frac(uvBlock);
    pcfFilter.xz = blockIndex.x > 0.5f ? float2(0.5f, blockIndex.x - 0.5f) : float2(-0.5f, 0.5f + blockIndex.x);
    pcfFilter.yw = blockIndex.y > 0.5f ? float2(0.5f, blockIndex.y - 0.5f) : float2(-0.5f, 0.5f + blockIndex.y);
    pcfFilter.xy = (floor(uvBlock) / rtSize.zw) + rtSize.xy * pcfFilter.xy;
}

real SampleShadowmapMobile(TEXTURE2D_SHADOW_PARAM_URP(ShadowMap, sampler_ShadowMap), float4 rtSize, float3 shadowCoord)
{
#if defined(SHADER_API_MOBILE) || defined(SHADER_API_SWITCH) /*|| defined(_HUAWEI)*/
    float4 pcfFilter;
    MakePcfUV(shadowCoord.xy, rtSize, pcfFilter);

    float4 f4;
    f4.x = SAMPLE_TEXTURE2D(ShadowMap, sampler_ShadowMap, pcfFilter.xy).r;
    f4.y = SAMPLE_TEXTURE2D(ShadowMap, sampler_ShadowMap, pcfFilter.xy + rtSize.xy * float2(0.0f, 1.0f)).r;
    f4.z = SAMPLE_TEXTURE2D(ShadowMap, sampler_ShadowMap, pcfFilter.xy + rtSize.xy * float2(1.0f, 0.0f)).r;
    f4.w = SAMPLE_TEXTURE2D(ShadowMap, sampler_ShadowMap, pcfFilter.xy + rtSize.xy * float2(1.0f, 1.0f)).r;
    real4 attenuation4 = SampleShadowCompare(f4, shadowCoord.z);
    return lerp(lerp(attenuation4.x, attenuation4.y, pcfFilter.w), lerp(attenuation4.z, attenuation4.w, pcfFilter.w), pcfFilter.z);
#else
    return SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, shadowCoord.xyz);
#endif
}

real SampleShadowmaPCF3x3(TEXTURE2D_PARAM(ShadowMap, sampler_ShadowMap), float3 shadowCoord, ShadowSamplingData samplingData)
{
    float4 rtSize = samplingData.shadowmapSize;
    real4 attenuation4;

    float3 shadowCoordA = shadowCoord.xyz + samplingData.shadowOffset0.xyz;

    float4 pcfFilterA;
    MakePcfUV(shadowCoordA.xy, rtSize, pcfFilterA);

    float depths[9];
    depths[0] = SAMPLE_TEXTURE2D(ShadowMap, sampler_ShadowMap, pcfFilterA.xy).r;
    depths[1] = SAMPLE_TEXTURE2D(ShadowMap, sampler_ShadowMap, pcfFilterA.xy + rtSize.xy * float2(1.0f, 0.0f)).r;
    depths[2] = SAMPLE_TEXTURE2D(ShadowMap, sampler_ShadowMap, pcfFilterA.xy + rtSize.xy * float2(2.0f, 0.0f)).r;
    depths[3] = SAMPLE_TEXTURE2D(ShadowMap, sampler_ShadowMap, pcfFilterA.xy + rtSize.xy * float2(0.0f, 1.0f)).r;
    depths[4] = SAMPLE_TEXTURE2D(ShadowMap, sampler_ShadowMap, pcfFilterA.xy + rtSize.xy * float2(1.0f, 1.0f)).r;
    depths[5] = SAMPLE_TEXTURE2D(ShadowMap, sampler_ShadowMap, pcfFilterA.xy + rtSize.xy * float2(2.0f, 1.0f)).r;
    depths[6] = SAMPLE_TEXTURE2D(ShadowMap, sampler_ShadowMap, pcfFilterA.xy + rtSize.xy * float2(0.0f, 2.0f)).r;
    depths[7] = SAMPLE_TEXTURE2D(ShadowMap, sampler_ShadowMap, pcfFilterA.xy + rtSize.xy * float2(1.0f, 2.0f)).r;
    depths[8] = SAMPLE_TEXTURE2D(ShadowMap, sampler_ShadowMap, pcfFilterA.xy + rtSize.xy * float2(2.0f, 2.0f)).r;

    //    float4 jump;
    //    jump.x = depths[0];
    //    jump.y = depths[2];
    //    jump.z = depths[6];
    //    jump.w = depths[8];
    //    real4 attenuationJump = SampleShadowCompare(jump, shadowCoord.z);
    //real jumpValue = dot(attenuationJump, 0.25);
    //real mid = SampleShadowCompare(depths[4], shadowCoord.z);
    //UNITY_BRANCH
    //    if (abs(jumpValue - 0.5f) == 0.5f && mid == jumpValue)
    //        clip(-1);

    float4 depth = float4(depths[0], depths[1], depths[3], depths[4]);
    half4 attenuationA = SampleShadowCompare(depth, shadowCoord.z);

    depth = float4(depths[1], depths[2], depths[4], depths[5]);
    half4 attenuationB = SampleShadowCompare(depth, shadowCoord.z);

    depth = float4(depths[3], depths[4], depths[6], depths[7]);
    half4 attenuationC = SampleShadowCompare(depth, shadowCoord.z);

    depth = float4(depths[4], depths[5], depths[7], depths[8]);
    half4 attenuationD = SampleShadowCompare(depth, shadowCoord.z);

    attenuation4 = lerp(
        lerp(half4(attenuationA.x, attenuationB.x, attenuationC.x, attenuationD.x), half4(attenuationA.y, attenuationB.y, attenuationC.y, attenuationD.y), pcfFilterA.z),
        lerp(half4(attenuationA.z, attenuationB.z, attenuationC.z, attenuationD.z), half4(attenuationA.w, attenuationB.w, attenuationC.w, attenuationD.w), pcfFilterA.z),
        pcfFilterA.w);

    return dot(attenuation4, 0.25);
}
//
//real SampleShadowmaPCF3x3Soft(TEXTURE2D_SHADOW_PARAM_URP(ShadowMap, sampler_ShadowMap), float3 shadowCoord, ShadowSamplingData samplingData)
//{
//    float4 rtSize = samplingData.shadowmapSize;
//    float fetchesWeights[4];
//    float2 fetchesUV[4];
//    SampleShadow_ComputeSamples_Tent_3x3(rtSize, shadowCoord.xy, fetchesWeights, fetchesUV);
//
//    //real4 attenuation4;
//    //float2 shadowCoordA = fetchesUV[0];
//    //float2 shadowCoordB = fetchesUV[1];
//    //float2 shadowCoordC = fetchesUV[2];
//    //float2 shadowCoordD = fetchesUV[3];
//
//    //float4 jump;
//
//    //float4 pcfFilterA;
//    //MakePcfUV(shadowCoordA.xy, rtSize, pcfFilterA);
//    //jump.x = SAMPLE_TEXTURE2D_LOD(ShadowMap, sampler_ShadowMap, pcfFilterA.xy, 0).r;
//
//    //float4 pcfFilterB;
//    //MakePcfUV(shadowCoordB.xy, rtSize, pcfFilterB);
//    //jump.y = SAMPLE_TEXTURE2D_LOD(ShadowMap, sampler_ShadowMap, pcfFilterB.xy + rtSize.xy * float2(1.0f, 0.0f), 0).r;
//
//    //float4 pcfFilterC;
//    //MakePcfUV(shadowCoordC.xy, rtSize, pcfFilterC);
//    //jump.z = SAMPLE_TEXTURE2D_LOD(ShadowMap, sampler_ShadowMap, pcfFilterC.xy + rtSize.xy * float2(0.0f, 1.0f), 0).r;
//
//    //float4 pcfFilterD;
//    //MakePcfUV(shadowCoordD.xy, rtSize, pcfFilterD);
//    //jump.w = SAMPLE_TEXTURE2D_LOD(ShadowMap, sampler_ShadowMap, pcfFilterD.xy + rtSize.xy * float2(1.0f, 1.0f), 0).r;
//
//    //real4 attenuationJump = SampleShadowCompare(jump, shadowCoord.z);
//    //real jumpValue = dot(attenuationJump, 0.25);
//    //UNITY_BRANCH
//    //    if (abs(jumpValue - 0.5f) > 0.49f)
//    //        return jumpValue;
//
//    return fetchesWeights[0] * SampleShadowmapMobile(TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap),
//        rtSize, float3(fetchesUV[0].xy, shadowCoord.z))
//        + fetchesWeights[1] * SampleShadowmapMobile(TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap),
//            rtSize, float3(fetchesUV[1].xy, shadowCoord.z))
//        + fetchesWeights[2] * SampleShadowmapMobile(TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap),
//            rtSize, float3(fetchesUV[2].xy, shadowCoord.z))
//        + fetchesWeights[3] * SampleShadowmapMobile(TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap),
//            rtSize, float3(fetchesUV[3].xy, shadowCoord.z));
//
//    /*   float4 depthA;
//       depthA.x = jump.x;
//       depthA.z = SAMPLE_TEXTURE2D(ShadowMap, sampler_ShadowMap, pcfFilterA.xy + rtSize.xy * float2(1.0f, 0.0f)).r;
//       depthA.y = SAMPLE_TEXTURE2D(ShadowMap, sampler_ShadowMap, pcfFilterA.xy + rtSize.xy * float2(0.0f, 1.0f)).r;
//       depthA.w = SAMPLE_TEXTURE2D(ShadowMap, sampler_ShadowMap, pcfFilterA.xy + rtSize.xy * float2(1.0f, 1.0f)).r;
//       real4 attenuationA = SampleShadowCompare(depthA, shadowCoord.z);
//       attenuation4.x = lerp(lerp(attenuationA.x, attenuationA.y, pcfFilterA.w), lerp(attenuationA.z, attenuationA.w, pcfFilterA.w), pcfFilterA.z);
//
//       float4 depthB;
//       depthB.x = SAMPLE_TEXTURE2D(ShadowMap, sampler_ShadowMap, pcfFilterB.xy).r;
//       depthB.z = jump.y;
//       depthB.y = SAMPLE_TEXTURE2D(ShadowMap, sampler_ShadowMap, pcfFilterB.xy + rtSize.xy * float2(0.0f, 1.0f)).r;
//       depthB.w = SAMPLE_TEXTURE2D(ShadowMap, sampler_ShadowMap, pcfFilterB.xy + rtSize.xy * float2(1.0f, 1.0f)).r;
//       real4 attenuationB = SampleShadowCompare(depthB, shadowCoord.z);
//       attenuation4.y = lerp(lerp(attenuationB.x, attenuationB.y, pcfFilterB.w), lerp(attenuationB.z, attenuationB.w, pcfFilterB.w), pcfFilterB.z);
//
//       float4 depthC;
//       depthC.x = SAMPLE_TEXTURE2D(ShadowMap, sampler_ShadowMap, pcfFilterC.xy).r;
//       depthC.z = SAMPLE_TEXTURE2D(ShadowMap, sampler_ShadowMap, pcfFilterC.xy + rtSize.xy * float2(1.0f, 0.0f)).r;
//       depthC.y = jump.z;
//       depthC.w = SAMPLE_TEXTURE2D(ShadowMap, sampler_ShadowMap, pcfFilterC.xy + rtSize.xy * float2(1.0f, 1.0f)).r;
//       real4 attenuationC = SampleShadowCompare(depthC, shadowCoord.z);
//       attenuation4.z = lerp(lerp(attenuationC.x, attenuationC.y, pcfFilterC.w), lerp(attenuationC.z, attenuationC.w, pcfFilterC.w), pcfFilterC.z);
//
//       float4 depthD;
//       depthD.x = SAMPLE_TEXTURE2D(ShadowMap, sampler_ShadowMap, pcfFilterD.xy).r;
//       depthD.z = SAMPLE_TEXTURE2D(ShadowMap, sampler_ShadowMap, pcfFilterD.xy + rtSize.xy * float2(1.0f, 0.0f)).r;
//       depthD.y = SAMPLE_TEXTURE2D(ShadowMap, sampler_ShadowMap, pcfFilterD.xy + rtSize.xy * float2(0.0f, 1.0f)).r;
//       depthD.w = jump.w;
//       float4 attenuationD = SampleShadowCompare(depthD, shadowCoord.z);
//       attenuation4.w = lerp(lerp(attenuationD.x, attenuationD.y, pcfFilterD.w), lerp(attenuationD.z, attenuationD.w, pcfFilterD.w), pcfFilterD.z);
//
//
//       return fetchesWeights[2] * attenuation4.x + fetchesWeights[0] * attenuation4.y + fetchesWeights[3] * attenuation4.z + fetchesWeights[1] * attenuation4.w;*/
//}

real SampleShadowmapFiltered(TEXTURE2D_SHADOW_PARAM_URP(ShadowMap, sampler_ShadowMap), float4 shadowCoord, ShadowSamplingData samplingData)
{
    real attenuation;
#if defined(SHADER_API_MOBILE) || defined(SHADER_API_SWITCH) /*|| defined(_HUAWEI)*/
    //real4 attenuation4;
    //attenuation4.x = SampleShadowmapMobile(TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap), samplingData.shadowmapSize, shadowCoord.xyz + samplingData.shadowOffset0.xyz);
    //attenuation4.y = SampleShadowmapMobile(TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap), samplingData.shadowmapSize, shadowCoord.xyz + samplingData.shadowOffset1.xyz);
    //attenuation4.z = SampleShadowmapMobile(TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap), samplingData.shadowmapSize, shadowCoord.xyz + samplingData.shadowOffset2.xyz);
    //attenuation4.w = SampleShadowmapMobile(TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap), samplingData.shadowmapSize, shadowCoord.xyz + samplingData.shadowOffset3.xyz);
    //attenuation = dot(attenuation4, 0.25);
    attenuation =  SampleShadowmaPCF3x3(TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap), shadowCoord.xyz, samplingData);
#else
    float fetchesWeights[9];
    float2 fetchesUV[9];
    SampleShadow_ComputeSamples_Tent_5x5(samplingData.shadowmapSize, shadowCoord.xy, fetchesWeights, fetchesUV);

    attenuation = fetchesWeights[0] * SampleShadowmapMobile(TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap),
        samplingData.shadowmapSize, float3(fetchesUV[0].xy, shadowCoord.z));
    attenuation += fetchesWeights[1] * SampleShadowmapMobile(TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap),
        samplingData.shadowmapSize, float3(fetchesUV[1].xy, shadowCoord.z));
    attenuation += fetchesWeights[2] * SampleShadowmapMobile(TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap),
        samplingData.shadowmapSize, float3(fetchesUV[2].xy, shadowCoord.z));
    attenuation += fetchesWeights[3] * SampleShadowmapMobile(TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap),
        samplingData.shadowmapSize, float3(fetchesUV[3].xy, shadowCoord.z));
    attenuation += fetchesWeights[4] * SampleShadowmapMobile(TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap),
        samplingData.shadowmapSize, float3(fetchesUV[4].xy, shadowCoord.z));
    attenuation += fetchesWeights[5] * SampleShadowmapMobile(TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap),
        samplingData.shadowmapSize, float3(fetchesUV[5].xy, shadowCoord.z));
    attenuation += fetchesWeights[6] * SampleShadowmapMobile(TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap),
        samplingData.shadowmapSize, float3(fetchesUV[6].xy, shadowCoord.z));
    attenuation += fetchesWeights[7] * SampleShadowmapMobile(TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap),
        samplingData.shadowmapSize, float3(fetchesUV[7].xy, shadowCoord.z));
    attenuation += fetchesWeights[8] * SampleShadowmapMobile(TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap),
        samplingData.shadowmapSize, float3(fetchesUV[8].xy, shadowCoord.z));
#endif
    return attenuation;
}
//
//real SampleShadowmapFilteredHL(
//    TEXTURE2D_SHADOW_PARAM(ShadowMap, sampler_ShadowMap), float4 shadowCoord, ShadowSamplingData samplingData)
//{
//    real attenuation;
//
//    #if defined(SHADER_API_MOBILE) || defined(SHADER_API_SWITCH)
//    // 4-tap hardware comparison
//    real4 attenuation4;
//    attenuation4.x = SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, shadowCoord.xyz + samplingData.shadowOffset0.xyz);
//    attenuation4.y = SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, shadowCoord.xyz + samplingData.shadowOffset1.xyz);
//    attenuation4.z = SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, shadowCoord.xyz + samplingData.shadowOffset2.xyz);
//    attenuation4.w = SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, shadowCoord.xyz + samplingData.shadowOffset3.xyz);
//    attenuation = dot(attenuation4, 0.25);
//    #else
//    float fetchesWeights[16];
//    float2 fetchesUV[16];
//    SampleShadow_ComputeSamples_Tent_7x7(samplingData.shadowmapSize, shadowCoord.xy, fetchesWeights, fetchesUV);
//
//    attenuation = fetchesWeights[0] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap,
//                                                              float3(fetchesUV[0].xy, shadowCoord.z));
//    attenuation += fetchesWeights[1] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap,
//                                                               float3(fetchesUV[1].xy, shadowCoord.z));
//    attenuation += fetchesWeights[2] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap,
//                                                               float3(fetchesUV[2].xy, shadowCoord.z));
//    attenuation += fetchesWeights[3] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap,
//                                                               float3(fetchesUV[3].xy, shadowCoord.z));
//    attenuation += fetchesWeights[4] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap,
//                                                               float3(fetchesUV[4].xy, shadowCoord.z));
//    attenuation += fetchesWeights[5] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap,
//                                                               float3(fetchesUV[5].xy, shadowCoord.z));
//    attenuation += fetchesWeights[6] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap,
//                                                               float3(fetchesUV[6].xy, shadowCoord.z));
//    attenuation += fetchesWeights[7] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap,
//                                                               float3(fetchesUV[7].xy, shadowCoord.z));
//    attenuation += fetchesWeights[8] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap,
//                                                               float3(fetchesUV[8].xy, shadowCoord.z));
//    attenuation = fetchesWeights[9] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap,
//                                                              float3(fetchesUV[9].xy, shadowCoord.z));
//    attenuation += fetchesWeights[10] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap,
//                                                                float3(fetchesUV[10].xy, shadowCoord.z));
//    attenuation += fetchesWeights[11] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap,
//                                                                float3(fetchesUV[11].xy, shadowCoord.z));
//    attenuation += fetchesWeights[12] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap,
//                                                                float3(fetchesUV[12].xy, shadowCoord.z));
//    attenuation += fetchesWeights[13] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap,
//                                                                float3(fetchesUV[13].xy, shadowCoord.z));
//    attenuation += fetchesWeights[14] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap,
//                                                                float3(fetchesUV[14].xy, shadowCoord.z));
//    attenuation += fetchesWeights[15] * SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap,
//                                                                float3(fetchesUV[15].xy, shadowCoord.z));
//    #endif
//
//    return attenuation;
//}


real SampleShadowmap(
    TEXTURE2D_SHADOW_PARAM_URP(ShadowMap, sampler_ShadowMap), float4 shadowCoord, ShadowSamplingData samplingData,
    half4 shadowParams, bool isPerspectiveProjection = true)
{
    // Compiler will optimize this branch away as long as isPerspectiveProjection is known at compile time
    if (isPerspectiveProjection)
        shadowCoord.xyz /= shadowCoord.w;

    real attenuation;
    real shadowStrength = shadowParams.x;

    // TODO: We could branch on if this light has soft shadows (shadowParams.y) to save perf on some platforms.
#ifdef _SHADOWS_SOFT
//#ifdef _HLSOFTSHADOWS
//   attenuation = SampleShadowmapFilteredHL(TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap), shadowCoord, samplingData);
//#else
    attenuation = SampleShadowmapFiltered(TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap), shadowCoord, samplingData);
    //#endif

#else
// 1-tap hardware comparison
    attenuation = SampleShadowmapMobile(TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap), samplingData.shadowmapSize, shadowCoord.xyz);
#endif

    attenuation = LerpWhiteTo(attenuation, shadowStrength);

    // Shadow coords that fall out of the light frustum volume must always return attenuation 1.0
    // TODO: We could use branch here to save some perf on some platforms.
    return BEYOND_SHADOW_FAR(shadowCoord) ? 1.0 : attenuation;
}



half ComputeCascadeIndex(float3 positionWS)
{
    float3 fromCenter0 = positionWS - _CascadeShadowSplitSpheres0.xyz;
    float3 fromCenter1 = positionWS - _CascadeShadowSplitSpheres1.xyz;
    float3 fromCenter2 = positionWS - _CascadeShadowSplitSpheres2.xyz;
    float3 fromCenter3 = positionWS - _CascadeShadowSplitSpheres3.xyz;
    float4 distances2 = float4(dot(fromCenter0, fromCenter0), dot(fromCenter1, fromCenter1),
        dot(fromCenter2, fromCenter2), dot(fromCenter3, fromCenter3));

    half4 weights = half4(distances2 < _CascadeShadowSplitSphereRadii);
    weights.yzw = saturate(weights.yzw - weights.xyz);

    return 4 - dot(weights, half4(4, 3, 2, 1));
}

float4 TransformWorldToShadowCoord(float3 positionWS)
{
#ifdef _MAIN_LIGHT_SHADOWS_CASCADE
    half cascadeIndex = ComputeCascadeIndex(positionWS);
#else
    half cascadeIndex = 0;
#endif

    float4 shadowCoord = mul(_MainLightWorldToShadow[cascadeIndex], float4(positionWS, 1.0));

    return float4(shadowCoord.xyz, cascadeIndex);
}
half SampleShadowNoPcf(float4 shadowCoord)
{
    #if defined(SHADER_API_MOBILE) || defined(SHADER_API_SWITCH) /*|| defined(_HUAWEI)*/
    float depth = SAMPLE_TEXTURE2D(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture, shadowCoord.xy).r;
    return SampleShadowCompare(depth, shadowCoord.z);
    #else
    return SAMPLE_TEXTURE2D_SHADOW(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture, shadowCoord.xyz);
    #endif

}
half MainLightRealtimeShadow(float4 shadowCoord)
{
#if !defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    return 1.0h;
#endif
    ShadowSamplingData shadowSamplingData = GetMainLightShadowSamplingData();
    half4 shadowParams = GetMainLightShadowParams();
    return SampleShadowmap(
        TEXTURE2D_ARGS(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture), shadowCoord, shadowSamplingData,
        shadowParams, false);
}

half AdditionalLightRealtimeShadow(int lightIndex, float3 positionWS)
{
#if !defined(ADDITIONAL_LIGHT_CALCULATE_SHADOWS)
    return 1.0h;
#endif

    ShadowSamplingData shadowSamplingData = GetAdditionalLightShadowSamplingData();

#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
    lightIndex = _AdditionalShadowsIndices[lightIndex];

    // We have to branch here as otherwise we would sample buffer with lightIndex == -1.
    // However this should be ok for platforms that store light in SSBO.
    UNITY_BRANCH
        if (lightIndex < 0)
            return 1.0;

    float4 shadowCoord = mul(_AdditionalShadowsBuffer[lightIndex].worldToShadowMatrix, float4(positionWS, 1.0));
#else
    float4 shadowCoord = mul(_AdditionalLightsWorldToShadow[lightIndex], float4(positionWS, 1.0));
#endif

    half4 shadowParams = GetAdditionalLightShadowParams(lightIndex);
    return SampleShadowmap(
        TEXTURE2D_ARGS(_AdditionalLightsShadowmapTexture, sampler_AdditionalLightsShadowmapTexture), shadowCoord,
        shadowSamplingData, shadowParams, true);
}

half GetShadowFade(float3 positionWS)
{
    float3 camToPixel = positionWS - _WorldSpaceCameraPos;
    float distanceCamToPixel2 = dot(camToPixel, camToPixel);

    half fade = saturate(distanceCamToPixel2 * _MainLightShadowParams.z + _MainLightShadowParams.w);
    return fade * fade;
}

half CloudShadowColor(float3 worldPos)
{
    float3 ligthPos = mul(_MainLightSpaceMatrix, float4(worldPos.xyz, 1)).xyz;
    float2 coord = ligthPos.xy * _MainTil * 10.f + float2(_CloudPannerX * _TimeParameters.x,
        _CloudPannerY * _TimeParameters.x);
    half projectionCol = SAMPLE_TEXTURE2D(_CloudShadowTex, sampler_CloudShadowTex, coord).r;
    return 1 - saturate(_CloudShadowIntensity * projectionCol);
}


half MixRealtimeAndBakedShadows(half realtimeShadow, half bakedShadow, half shadowFade, float3 positionWS,
    float3 NormalWs, bool needCloud = true)
{
    half Shadow = 1;
    half cloud = 1;
#if defined(LIGHTMAP_SHADOW_MIXING)
    Shadow = lerp(realtimeShadow, 1, shadowFade) * bakedShadow;
#else
    Shadow = lerp(realtimeShadow, bakedShadow, shadowFade);
#endif
#if defined(_CLOUDSHADOW_ON)&& !defined(_SHADER_LEVEL_LOW)
    cloud = (1 - CloudShadowColor(positionWS)) * needCloud;
    Shadow = (1 - cloud) * Shadow;
#endif

    return Shadow;
}

half BakedShadow(half4 shadowMask, half4 occlusionProbeChannels)
{
    // Here occlusionProbeChannels used as mask selector to select shadows in shadowMask
    // If occlusionProbeChannels all components are zero we use default baked shadow value 1.0
    // This code is optimized for mobile platforms:
    // half bakedShadow = any(occlusionProbeChannels) ? dot(shadowMask, occlusionProbeChannels) : 1.0h;
    half bakedShadow = 1.0h + dot(shadowMask - 1.0h, occlusionProbeChannels);
    return bakedShadow;
}

// 
half MainLightShadow(float4 shadowCoord, float3 positionWS, half4 shadowMask, half4 occlusionProbeChannels,
    float3 normalWs)
{
#ifdef MAIN_LIGHT_CALCULATE_SHADOWS
    half shadowFade = GetShadowFade(positionWS);
#else
    half shadowFade = 1.0h;
#endif

#if defined(_MAIN_LIGHT_SHADOWS_CASCADE) && defined(CALCULATE_BAKED_SHADOWS)
    // shadowCoord.w represents shadow cascade index
    // in case we are out of shadow cascade we need to set shadow fade to 1.0 for correct blending
    // it is needed when realtime shadows gets cut to early during fade and causes disconnect between baked shadow
    shadowFade = shadowCoord.w == 4 ? 1.0h : shadowFade;
#endif
#if defined(_FORCE_SHADOWMASK)
    half bakedShadow = shadowMask.r;
#else
#ifdef CALCULATE_BAKED_SHADOWS
    half bakedShadow = BakedShadow(shadowMask, occlusionProbeChannels);
#else
    half bakedShadow = 1.0h;
#endif
#endif

    //half realtimeShadow = MainLightRealtimeShadow(shadowCoord);

    half realtimeShadow = 1;
    UNITY_BRANCH
    if (shadowFade < 1)
        realtimeShadow = MainLightRealtimeShadow(shadowCoord);

    return MixRealtimeAndBakedShadows(realtimeShadow, bakedShadow, shadowFade, positionWS, normalWs);
}

half AdditionalLightShadow(int lightIndex, float3 positionWS, half4 shadowMask, half4 occlusionProbeChannels,
    float3 normalWs, bool needCloud = true)
{
    half realtimeShadow = AdditionalLightRealtimeShadow(lightIndex, positionWS);

#ifdef CALCULATE_BAKED_SHADOWS
    half bakedShadow = BakedShadow(shadowMask, occlusionProbeChannels);
#else
    half bakedShadow = 1.0h;
#endif

#ifdef ADDITIONAL_LIGHT_CALCULATE_SHADOWS
    half shadowFade = GetShadowFade(positionWS);
#else
    half shadowFade = 1.0h;
#endif

    return MixRealtimeAndBakedShadows(realtimeShadow, bakedShadow, shadowFade, positionWS, normalWs, false);
}

float4 GetShadowCoord(VertexPositionInputs vertexInput)
{
    return TransformWorldToShadowCoord(vertexInput.positionWS);
}

float3 ApplyShadowBias(float3 positionWS, float3 normalWS, float3 lightDirection)
{
    float invNdotL = 1.0 - saturate(dot(lightDirection, normalWS));
    float scale = invNdotL * _ShadowBias.y;

    // normal bias is negative since we want to apply an inset normal offset
    positionWS = lightDirection * _ShadowBias.xxx + positionWS;
    positionWS = normalWS * scale.xxx + positionWS;
    return positionWS;
}

///////////////////////////////////////////////////////////////////////////////
// Deprecated                                                                 /
///////////////////////////////////////////////////////////////////////////////

// Renamed -> _MainLightShadowParams
#define _MainLightShadowData _MainLightShadowParams

// Deprecated: Use GetShadowFade instead.
float ApplyShadowFade(float shadowAttenuation, float3 positionWS)
{
    float fade = GetShadowFade(positionWS);
    return shadowAttenuation + (1 - shadowAttenuation) * fade * fade;
}

// Deprecated: Use GetMainLightShadowParams instead.
half GetMainLightShadowStrength()
{
    return _MainLightShadowData.x;
}

// Deprecated: Use GetAdditionalLightShadowParams instead.
half GetAdditionalLightShadowStrenth(int lightIndex)
{
#if USE_STRUCTURED_BUFFER_FOR_LIGHT_DATA
    return _AdditionalShadowsBuffer[lightIndex].shadowParams.x;
#else
    return _AdditionalShadowParams[lightIndex].x;
#endif
}

// Deprecated: Use SampleShadowmap that takes shadowParams instead of strength.
real SampleShadowmap(float4 shadowCoord, TEXTURE2D_SHADOW_PARAM_URP(ShadowMap, sampler_ShadowMap),
    ShadowSamplingData samplingData, half shadowStrength, bool isPerspectiveProjection = true)
{
    half4 shadowParams = half4(shadowStrength, 1.0, 0.0, 0.0);
    return SampleShadowmap(
        TEXTURE2D_SHADOW_ARGS(ShadowMap, sampler_ShadowMap), shadowCoord, samplingData, shadowParams,
        isPerspectiveProjection);
}

#endif
