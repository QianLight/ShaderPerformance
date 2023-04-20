#ifndef HLOD_L_SHADOW_CASTER_PASS_INCLUDED
#define HLOD_L_SHADOW_CASTER_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
// #include "Packages/com.unity.render-pipelines.universal/Athena/Runtime/ShaderLibrary/Atlasing.hlsl"

float3 _LightDirection;

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float2 texcoord     : TEXCOORD0;
#if _NeedVertexColor
    float4 color        : COLOR;
#endif

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv           : TEXCOORD0;
    float4 positionCS   : SV_POSITION;
    #if _NeedVertexColor
    float4 color        : COLOR;
    #endif
    #if _TEXTURE_ATLASING_ON
    // TEXTURE_ATLASING_COORDS(1)
    #endif


};
float3 ApplyShadowBias_Self(float3 positionWS, float3 normalWS, float3 lightDirection)
{
    float invNdotL = 1.0 - saturate(dot(lightDirection, normalWS));
    float scale = invNdotL * (_ShadowBias.y - _SelfNormalBias);

    // normal bias is negative since we want to apply an inset normal offset
    positionWS = lightDirection * (_ShadowBias.xxx - _SelfDepthBias) + positionWS;
    positionWS = normalWS * scale.xxx + positionWS;
    return positionWS;
}
float4 GetShadowPositionHClip(Attributes input)
{
    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

    float4 positionCS = TransformWorldToHClip(ApplyShadowBias_Self(positionWS, normalWS, _LightDirection));

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

    //* pwrd majiao:  Shadow-Cache阴影 //
    #if defined(SHADOW_CACHE_WRITE)
    {
        float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
        float3 worldNormal = TransformObjectToWorldNormal(input.normalOS);

        worldPos = ApplyNormalBias(worldPos, worldNormal, _ShadowLightForwardDir, _NormalBias);
        float4 clipPos = mul(_Matrix_VP, float4(worldPos, 1));

    #if UNITY_REVERSED_Z
        clipPos.z = min(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
    #else
        clipPos.z = max(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
    #endif

        clipPos = ApplyDepthBias(clipPos, _DepthBias);

        output.positionCS = clipPos;
    }
    #else
    {
        output.positionCS = GetShadowPositionHClip(input);
    }
    #endif
    //* pwrd majiao //

    //* pwrd lvjunyu
    #if _LayeredEnable
        output.uv = input.texcoord;
    #else
        output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    #endif
    //* pwrd lvjunyu

    #if _NeedVertexColor
        output.color = input.color;
    #endif

    #if _TEXTURE_ATLASING_ON
    // TEXTURE_ATLASING_TRANSFER_EXTRA(output, input);
    #endif


    return output;
}

half4 ShadowPassFragment(Varyings input) : SV_TARGET
{
    //* pwrd majiao:  Shadow-Cache阴影 //
    #if defined(SHADOW_CACHE_WRITE)
    {
    //* pwrd lvjunyu
    #if defined(_ALPHATEST_ON) && !_LayeredEnable
    //* pwrd lvjunyu
        #if _TEXTURE_ATLASING_ON
            float4 col = TEXTURE_ATLASING_TEX2D(input.color, _BaseMap, 0, input.uv,TEXTURE_ATLASING_PACKED(input));
        #else
            float4 col = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
        #endif
       clip(col.a - _Cutoff);
    #endif
        float depth = RemapDepthInLightSpace(input.positionCS.z);
        float4 finalColor = float4(depth, 0, 0, 1);
        return finalColor;
    }
    #else
    {
        //* pwrd lvjunyu
        #if !_LayeredEnable
            // #if _TEXTURE_ATLASING_ON
                // float4 packedData = TEXTURE_ATLASING_PACKED(input);
                // Alpha(TEXTURE_ATLASING_TEX2D(input.color, _BaseMap, 0, input.uv, packedData).a, _BaseColor, _Cutoff);
            // #else
                Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);
            // #endif
        #endif
        //* pwrd lvjunyu
        return 0;
    }
    #endif
    //* pwrd majiao //
}

#endif
