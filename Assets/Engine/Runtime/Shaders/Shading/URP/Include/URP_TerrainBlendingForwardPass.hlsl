#ifndef URP_TERRAIN_BLENDING_FORWARD_PASS
#define URP_TERRAIN_BLENDING_FORWARD_PASS
// struct Attributes
// {
//     float4 positionOS   : POSITION;
//     float3 normalOS     : NORMAL;
//     float4 tangentOS    : TANGENT;
//     float2 texcoord     : TEXCOORD0;
//     float2 uv1          : TEXCOORD1;
// };
// struct Varyings
// {
//     float2 uv                       : TEXCOORD0;
//     #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
//     float3 positionWS               : TEXCOORD2;
//     #endif
//     float3 normalWS                 : TEXCOORD3;
//     #if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
//     float4 tangentWS                : TEXCOORD4;    // xyz: tangent, w: sign
//     #endif
//     float3 viewDirWS                : TEXCOORD5;
//     float4 positionCS               : SV_POSITION;
// };
// CBUFFER_START(UnityPerMaterial)
// half4 _MainTex_ST,_MainTex1_ST,_MainTex2_ST;
// half4 _Layer0UVST;
// float4 _Color0;
// float4 _Color1;
// float4 _Color2;
// float4 _TextureIntensity;
// CBUFFER_END
// TEXTURE2D(_MainTex);        SAMPLER(sampler_MainTex);
// TEXTURE2D(_MainTex1);       SAMPLER(sampler_MainTex1);
// TEXTURE2D(_MainTex2);       SAMPLER(sampler_MainTex2);
// TEXTURE2D(_BlendTex);       SAMPLER(sampler_BlendTex);
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "./URP_Input.hlsl"
#include "../Scene/PBR_Blend_Head.hlsl"
#define URP_InitializeSurfaceData(uv, surfaceData) InitializeSurfaceData(input.positionWSColor, uv, surfaceData, input.blend);


#ifndef URP_InitializeSurfaceData

inline void InitializeBaseSurfaceData(float2 uv, out SurfaceData outSurfaceData)
{
    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    outSurfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff);

    half4 specGloss = SampleMetallicSpecGloss(uv, albedoAlpha.a);
    outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;

    #if _SPECULAR_SETUP
    outSurfaceData.metallic = 1.0h;
    outSurfaceData.specular = specGloss.rgb;
    #else
    outSurfaceData.metallic = specGloss.r;
    outSurfaceData.specular = half3(0.0h, 0.0h, 0.0h);
    #endif

    outSurfaceData.smoothness = specGloss.a;
    outSurfaceData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
    outSurfaceData.occlusion = SampleOcclusion(uv);
    outSurfaceData.emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));

    #if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
    half2 clearCoat = SampleClearCoat(uv);
    outSurfaceData.clearCoatMask = clearCoat.r;
    outSurfaceData.clearCoatSmoothness = clearCoat.g;
    #else
    outSurfaceData.clearCoatMask = 0.0h;
    outSurfaceData.clearCoatSmoothness = 0.0h;
    #endif

    #if defined(_DETAIL)
    half detailMask = SAMPLE_TEXTURE2D(_DetailMask, sampler_DetailMask, uv).a;
    float2 detailUv = uv * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
    outSurfaceData.albedo = ApplyDetailAlbedo(detailUv, outSurfaceData.albedo, detailMask);
    outSurfaceData.normalTS = ApplyDetailNormal(detailUv, outSurfaceData.normalTS, detailMask);

    #endif
}
#define URP_InitializeSurfaceData(uv, surfaceData) InitializeBaseSurfaceData(uv, surfaceData)
#endif // URP_InitializeSurfaceData



Varyings TerrainBlendingVertex(Attributes input)
{
    Varyings output = (Varyings)0;
    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    output.uv = TRANSFORM_TEX(input.texcoord, _MainTex);
    output.positionCS = vertexInput.positionCS;
    return output;
}
#include "../Scene/PBR_Blend_Head.hlsl"
half4 TerrainBlendingFragment(Varyings input) : SV_Target
{
    SurfaceData surfaceData;
    URP_InitializeSurfaceData(input.uv, surfaceData)
    
    // float4 blend;
    // float4 blendMask;
    // #if !_VCMODE_ON
    // blend = SAMPLE_TEXTURE2D(_BlendTex, sampler_BlendTex, input.uv);
    // #endif
    // #if _BLENDNORMALIZE_ON
    // blend.xyz = normalize(float3(blend.xyz));
    // #endif
    // half4 baseColor = BlendBaseColor(blend, input.uv, blendMask);

    half3 baseColor = surfaceData.albedo;
    half4 color0 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv );
    
    return half4(baseColor,1);
}
#endif //URP_FORWARD_PASS