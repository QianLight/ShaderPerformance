#ifndef URP_LAYER_META_PASS_INCLUDED
#define URP_LAYER_META_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "../Include/OPPCore.hlsl"
#include "../Include/OPPInput.hlsl"

CBUFFER_START(UnityMetaPass)
// x = use uv1 as raster position
// y = use uv2 as raster position
bool4 unity_MetaVertexControl;
// x = return albedo
// y = return normal
bool4 unity_MetaFragmentControl;
CBUFFER_END

//struct Attributes
//{
//    float4 positionOS   : POSITION;
//    float3 normalOS     : NORMAL;
//    float2 uv0          : TEXCOORD0;
//    float2 uv1          : TEXCOORD1;
//    float2 uv2          : TEXCOORD2;
//#ifdef _TANGENT_TO_WORLD
//    float4 tangentOS     : TANGENT;
//#endif
//};

//struct Varyings
//{
//    float4 positionCS   : SV_POSITION;
//    float2 uv           : TEXCOORD0;
//};

float unity_OneOverOutputBoost;
float unity_MaxOutputValue;
float unity_UseLinearSpace;

struct MetaInput
{
    half3 Albedo;
    half3 Emission;
    half3 SpecularColor;
};

float4 MetaVertexPosition(float4 positionOS, float2 uv1, float2 uv2, float4 uv1ST, float4 uv2ST)
{
    if (unity_MetaVertexControl.x)
    {
        positionOS.xy = uv1 * uv1ST.xy + uv1ST.zw;
        // OpenGL right now needs to actually use incoming vertex position,
        // so use it in a very dummy way
        positionOS.z = positionOS.z > 0 ? REAL_MIN : 0.0f;
    }
    if (unity_MetaVertexControl.y)
    {
        positionOS.xy = uv2 * uv2ST.xy + uv2ST.zw;
        // OpenGL right now needs to actually use incoming vertex position,
        // so use it in a very dummy way
        positionOS.z = positionOS.z > 0 ? REAL_MIN : 0.0f;
    }
    return TransformWorldToHClip(positionOS.xyz);
}

inline void InitializeStandardLitSurfaceData(float2 uv, out SurfaceData outSurfaceData)
{
    half4 albedo = Sample2D(uv, TEXTURE2D_ARGS(_MainTex, sampler_MainTex));
    half4 bump = Sample2D(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
    outSurfaceData.alpha = GetAlpha(albedo, _Cutoff);
    outSurfaceData.albedo = albedo.rgb * _Color0.rgb;

    outSurfaceData.metallic = GetMetallic(bump.w, _MetallicMin, _MetallicMax);
    outSurfaceData.specular = 0;
    outSurfaceData.emission = GetEmission(albedo, _EmissionColor);
    outSurfaceData.smoothness = GetSmoothness(bump.z, _RoughnessMin, _RoughnessMax);
   outSurfaceData.normalTS = GetNormal(bump.xy, _NormalScale);
    outSurfaceData.occlusion = GetOcclusion(bump.w, _OcclusionScale);
    outSurfaceData.clearCoatMask = 0.0h;
    outSurfaceData.clearCoatSmoothness = 0.0h;
    outSurfaceData.darkValueInRain = 1;
}

half4 MetaFragment(MetaInput input)
{
    half4 res = 0;
    if (unity_MetaFragmentControl.x)
    {
        res = half4(input.Albedo, 1.0);

        // Apply Albedo Boost from LightmapSettings.
        res.rgb = clamp(PositivePow(res.rgb, saturate(unity_OneOverOutputBoost)), 0, unity_MaxOutputValue);
    }
    if (unity_MetaFragmentControl.y)
    {
        half3 emission;
        if (unity_UseLinearSpace)
            emission = input.Emission;
        else
            emission = LinearToSRGB(input.Emission);

        res = half4(emission, 1.0);
    }
    return res;
}

Varyings UniversalVertexMeta(Attributes input)
{
    Varyings output;
    output.positionCS = MetaVertexPosition(input.positionOS, input.uv1, input.lightmapUV, unity_LightmapST, unity_DynamicLightmapST);
    output.uv = TRANSFORM_TEX(input.texcoord, _MainTex);
    return output;
}

half4 UniversalFragmentMeta(Varyings input) : SV_Target
{
    SurfaceData surfaceData;
    InitializeStandardLitSurfaceData(input.uv, surfaceData);

    BRDFData brdfData;
    InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.alpha, brdfData);

    MetaInput metaInput;
    metaInput.Albedo = brdfData.diffuse + brdfData.specular * brdfData.roughness * 0.5;
    metaInput.SpecularColor = surfaceData.specular;
    metaInput.Emission = surfaceData.emission;

    return MetaFragment(metaInput);
}

#endif
