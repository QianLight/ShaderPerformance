#ifndef URP_LAYER_SHADOWCASTER_PASS_INCLUDED
#define URP_LAYER_SHADOWCASTER_PASS_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "../Include/OPPCore.hlsl"
#include "../Include/OPPInput.hlsl"

float3 _LightDirection;

//struct Attributes
//{
//    float4 positionOS   : POSITION;
//    float3 normalOS     : NORMAL;
//    float2 texcoord     : TEXCOORD0;
//    UNITY_VERTEX_INPUT_INSTANCE_ID
//};

//struct Varyings
//{
//    float2 uv           : TEXCOORD0;
//    float4 positionCS   : SV_POSITION;
//    UNITY_VERTEX_INPUT_INSTANCE_ID
//        UNITY_VERTEX_OUTPUT_STEREO
//};

float4 GetShadowPositionHClip(Attributes input)
{
    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

#if UNITY_REVERSED_Z
    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#else
    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#endif

    return positionCS;
}

Varyings ShadowPassVertex(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);

    output.uv = TRANSFORM_TEX(input.texcoord, _MainTex);
    output.positionCS = GetShadowPositionHClip(input);
    return output;
}

half4 ShadowPassFragment(Varyings input) : SV_TARGET
{
    half4 albedo = Sample2D(input.uv, TEXTURE2D_ARGS(_MainTex, sampler_MainTex));
    GetAlpha(albedo, _Cutoff);
    return 0;
}

Varyings DepthOnlyVertex(Attributes input)
{
    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    output.uv = TRANSFORM_TEX(input.texcoord, _MainTex);
    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
    return output;
}

half4 DepthOnlyFragment(Varyings input) : SV_TARGET
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    half4 albedo = Sample2D(input.uv, TEXTURE2D_ARGS(_MainTex, sampler_MainTex));
    GetAlpha(albedo, _Cutoff);
    return 0;
}
#endif
