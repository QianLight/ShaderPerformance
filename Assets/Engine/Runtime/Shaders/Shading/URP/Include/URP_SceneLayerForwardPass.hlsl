#ifndef URP_SCENE_LAYER
#define URP_SCENE_LAYER

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

// For vertical gradient color    
#include "../../Scene/VerticalColor.hlsl"

CBUFFER_START(UnityPerMaterial)
float4 _MainTex_ST;
float4 _Layer0UVST;
float4 _Layer1UVST;
float4 _Layer2UVST;
float4 _Layer3UVST;

half4 _TextureIntensity;
half4 _Color0;
half4 _Color1;
half4 _Color2;
half4 _Color3;
half4 _EmissionColor;
half4 _Rot01;
half4 _Rot23;
half4 _GradientEnd;
half4 _GradientBegin;
half4 _WorldPositionOffset;
half4 _WorldPositionScale;

half _NormalScale;
half _MetallicScale;
half _SpecularScale;
half _IBLScale;
half _Emission;
half _Metallic;
half _Occlusion;
half _OcclusionScale;
half _PBS0Z;
half _PBS0W;
half _PBS1Z;
half _PBS1W;
half _PBS2Z;
half _PBS2W;
half _PBS3Z;
half _PBS3W;
half _RoughnessMin;
half _RoughnessMax;
half _MetallicMin;
half _MetallicMax;
half _BlendScale;
half _BlendThreshold;
half _AOBias;
half _DitherTransparency;
half _Surface;
half _GradientScale;
half _BlendIntensity;
half _Parkout;
half _Ripples;

half _NO_METALLIC;
half _BlendNormalize;
CBUFFER_END

#ifdef _GLOBAL_SPECULAR_ADJUST_ENABLE
half4 _sceneSpecularColorParams;
#endif

#ifdef _BRIGHT_AO_ENABLE
half4 _BrightAOParams;
#endif

half4 _SceneColorAdjustmentParams;
#define _SceneExposure _SceneColorAdjustmentParams.x

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ParallaxMapping.hlsl" 
#include "Assets/Engine/Runtime/Shaders/Shading/Scene/ParkourDistort.hlsl"

#define ATTRIBUTES_ADDITION float4 blend : COLOR;
#define VARYINGS_ADDITION float4 blend : COLOR;

#include "./URP_Input.hlsl"

TEXTURE2D(_MainTex);        SAMPLER(sampler_MainTex);
TEXTURE2D(_MainTex1);       //SAMPLER(sampler_MainTex1);
TEXTURE2D(_MainTex2);       //SAMPLER(sampler_MainTex2);
TEXTURE2D(_MainTex3);       //SAMPLER(sampler_MainTex3);

TEXTURE2D(_ProcedureTex0);       SAMPLER(sampler_ProcedureTex0);
TEXTURE2D(_ProcedureTex1);       //SAMPLER(sampler_ProcedureTex1);
TEXTURE2D(_ProcedureTex2);       //SAMPLER(sampler_ProcedureTex2);
TEXTURE2D(_ProcedureTex3);       //SAMPLER(sampler_ProcedureTex3);
TEXTURE2D(_BlendTex);       SAMPLER(sampler_BlendTex);

#include "../Scene/PBR_Blend_Head.hlsl"

inline half4 BlendBaseColor(float4 blend, float2 uv,out float4 blendMask)
{
    float2 uv0 = GetRotUV0(uv);
    float4 color0 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv0 * _Layer0UVST.xy + _Layer0UVST.zw);
    color0.rgb *= _Color0.rgb * _TextureIntensity.x;
    color0.rgb = pow(color0.rgb + 0.00001, _Color0.w);
#if defined(_SPLAT_2X)					
    //2 layer
    float2 uv1 = GetRotUV1(uv) * _Layer1UVST.xy + _Layer1UVST.zw;
    float4 color1 = SAMPLE_TEXTURE2D(_MainTex1, sampler_MainTex, uv1);
    color1.rgb *= _Color1.rgb * _TextureIntensity.y;
    color1.rgb = pow(color1.rgb + 0.00001, _Color1.w);
    float4 blendColor = BlendColor2(color0, color1, blend, blendMask);
#elif defined(_SPLAT_3X)
    //3 layer
    float2 uv1 = GetRotUV1(uv) * _Layer1UVST.xy + _Layer1UVST.zw;
    float2 uv2 = GetRotUV2(uv) * _Layer2UVST.xy + _Layer2UVST.zw;
    
    float4 color1 = SAMPLE_TEXTURE2D(_MainTex1, sampler_MainTex, uv1);
    color1.rgb *= _Color1.rgb * _TextureIntensity.y;
    color1.rgb = pow(color1.rgb + 0.00001, _Color1.w);

    float4 color2 = SAMPLE_TEXTURE2D(_MainTex2, sampler_MainTex, uv2);
    color2.rgb *= _Color2.rgb * _TextureIntensity.z;
    color2.rgb = pow(color2.rgb + 0.00001, _Color2.w);
    
    float4 blendColor = BlendColor3(color0, color1, color2, blend, blendMask);
#elif defined(_SPLAT_4X)
    //4 layer
    float2 uv1 = GetRotUV1(uv) * _Layer1UVST.xy + _Layer1UVST.zw;
    float2 uv2 = GetRotUV2(uv) * _Layer2UVST.xy + _Layer2UVST.zw;
    float2 uv3 = GetRotUV3(uv) * _Layer3UVST.xy + _Layer3UVST.zw;
    
    float4 color1 = SAMPLE_TEXTURE2D(_MainTex1, sampler_MainTex, uv1);
    color1.rgb *= _Color1.rgb * _TextureIntensity.y;
    color1.rgb = pow(color1.rgb + 0.00001, _Color1.w);

    float4 color2 = SAMPLE_TEXTURE2D(_MainTex2, sampler_MainTex, uv2);
    color2.rgb *= _Color2.rgb * _TextureIntensity.z;
    color2.rgb = pow(color2.rgb + 0.00001, _Color2.w);

    float4 color3 = SAMPLE_TEXTURE2D(_MainTex3, sampler_MainTex, uv3);
    color3.rgb *= _Color3.rgb * _TextureIntensity.w;
    color3.rgb = pow(color3.rgb + 0.00001, _Color3.w);
    
    float4 blendColor = BlendColor4(color0, color1, color2, color3, blend, blendMask);
    
#else
    //1 layer
    float4 blendColor = color0;
    blendMask = 0;
#endif
    return blendColor;
}

inline float3 BlendPBS(float2 uv, float4 blend, out float2 rm,out float4 scaleParam, out float ao)
{
    float2 uv0 = GetRotUV0(uv) * _Layer0UVST.xy + _Layer0UVST.zw;
#if defined(_NORMALMAP)
    float4 pbs0 = SAMPLE_TEXTURE2D(_ProcedureTex0, sampler_ProcedureTex0, uv0);
#else
    float4 pbs0 = float4(0.5, 0.5, _PBS0Z, _PBS0W);
#endif
    float3 normal = float3(0.5, 0.5, 1);

    rm = float2(1, 0);
#if defined(_SPLAT_2X)
    //2 layer
    float2 uv1 = GetRotUV1(uv) * _Layer1UVST.xy + _Layer1UVST.zw;
   
    #if defined(_NORMALMAP)
        float4 pbs1 = SAMPLE_TEXTURE2D(_ProcedureTex1, sampler_ProcedureTex0, uv1);
    #else
        float4 pbs1 = float4(0.5, 0.5, _PBS1Z, _PBS1W);
    #endif

    normal = BlendPBS2(pbs0, pbs1, _NormalScale, blend, rm);
    //normal = pbs1.xyz;
#elif defined(_SPLAT_3X)
    //3 layer
    float2 uv1 = GetRotUV1(uv) * _Layer1UVST.xy + _Layer1UVST.zw;
    float2 uv2 = GetRotUV2(uv) * _Layer2UVST.xy + _Layer2UVST.zw;
    #if defined(_NORMALMAP)
        float4 pbs1 = SAMPLE_TEXTURE2D(_ProcedureTex1, sampler_ProcedureTex0, uv1);
        float4 pbs2 = SAMPLE_TEXTURE2D(_ProcedureTex2, sampler_ProcedureTex0, uv2);
    #else
        float4 pbs1 = float4(0.5, 0.5, _PBS1Z, _PBS1W);
        float4 pbs2 = float4(0.5, 0.5, _PBS2Z, _PBS2W);
    #endif  
    normal = BlendPBS3(pbs0, pbs1, pbs2, _NormalScale, blend, rm);
#elif defined(_SPLAT_4X)
    //4 layer
    float2 uv1 = GetRotUV1(uv) * _Layer1UVST.xy + _Layer1UVST.zw;
    float2 uv2 = GetRotUV2(uv) * _Layer2UVST.xy + _Layer2UVST.zw;
    float2 uv3 = GetRotUV3(uv) * _Layer3UVST.xy + _Layer3UVST.zw;
    #if defined(_NORMALMAP)
    float4 pbs1 = SAMPLE_TEXTURE2D(_ProcedureTex1, sampler_ProcedureTex0, uv1);
    float4 pbs2 = SAMPLE_TEXTURE2D(_ProcedureTex2, sampler_ProcedureTex0, uv2);
    float4 pbs3 = SAMPLE_TEXTURE2D(_ProcedureTex3, sampler_ProcedureTex0, uv3);
    #else
    float4 pbs1 = float4(0.5, 0.5, _PBS1Z, _PBS1W);
    float4 pbs2 = float4(0.5, 0.5, _PBS2Z, _PBS2W);
    float4 pbs3 = float4(0.5, 0.5, _PBS3Z, _PBS3W);
    #endif  
    normal = BlendPBS4(pbs0, pbs1, pbs2, pbs3, _NormalScale, blend, rm);
#else
    //1 layer
    normal = UnpackNormal2(pbs0.xy, _NormalScale);
    rm = pbs0.zw;
#endif
    
    ao = 0;
// #if _NO_METALLIC_ON
//     ao = rm.y;
//     rm.y = 0;
// #else
//     rm.y *= _MetallicScale;
// #endif
    if (_NO_METALLIC == 1)
    {
        ao = rm.y;
        rm.y = 0;
    }
    else
    {
        rm.y *= _MetallicScale;
    }
    //MaterialData.ScaleParam = float4(_SpecularScale, _IBLScale, 1, 0);
    scaleParam = float4(_SpecularScale, _IBLScale, 1, 0);
    return normal;
}

inline void InitializeSurfaceData(half3 positionColor,float2 uv, out SurfaceData outSurfaceData, float4 blend)//Add a input "PositionColor" for vertical Gradient color
{
    float2 rm;
    float4 scaleParam;
    float ao;
    float3 normal;
    float4 blendMask;

#if !_VCMODE_ON
    blend = SAMPLE_TEXTURE2D(_BlendTex, sampler_BlendTex, uv);
#endif

// #if _BLENDNORMALIZE_ON
//     blend.xyz = normalize(float3(blend.xyz));
// #endif
    if (_BlendNormalize == 1)
    {
        blend.xyz = normalize(float3(blend.xyz));
    }



    half4 baseColor = BlendBaseColor(blend, uv, blendMask);
#ifdef _NORMALMAP
    normal = BlendPBS(uv, blendMask, rm, scaleParam, ao);
#else
    normal = 0;
    rm = float2(1, 0);
    ao = 1;
#endif

    float metallic = min(max(rm.y, _MetallicMin), _MetallicMax);
    float roughness = min(max(rm.x, _RoughnessMin), _RoughnessMax);
    //float3 specularColor = lerp(kDieletricSpec.rgb, baseColor.rgb, metallic) * _SpecularScale;
#ifdef _NORMALMAP
    ao = lerp(1, ao, _AOBias);
#endif

    ////Albedo without vertical gradient 
    // outSurfaceData.albedo = baseColor.rgb;

    //////////////////////////  For vertical gradient color  /////////////////////////////////
    #ifdef _GRADIENT_ON
        half4 originalAlbedo = baseColor;
        half4 gradientAlbedo;
        VerticalColor(
            originalAlbedo,positionColor,_GradientEnd,_GradientBegin,_WorldPositionOffset,
            _WorldPositionScale,_GradientSwitch,_BlendMode,_GradientScale,_BlendIntensity,
            gradientAlbedo);
        outSurfaceData.albedo = gradientAlbedo.rgb ;
    #else
        outSurfaceData.albedo = baseColor.rgb;
    #endif
    //////////////////////////  For vertical gradient color  //////////////////////////////////

    outSurfaceData.alpha = baseColor.a;
    outSurfaceData.metallic = metallic;
    //outSurfaceData.specular = specularColor;
    outSurfaceData.specular = 0;
    outSurfaceData.smoothness = 1 - roughness;
    outSurfaceData.normalTS = normal;
    outSurfaceData.occlusion = ao;
    outSurfaceData.emission = 0;
    outSurfaceData.clearCoatMask = 0.0h;
    outSurfaceData.clearCoatSmoothness = 0.0h;

#if defined(_RAIN)
        outSurfaceData.darkValueInRain = _darkValueInRain;
#else
        outSurfaceData.darkValueInRain = 1;
#endif

}

#define VS_ADDITION output.blend = input.blend;

#define URP_InitializeSurfaceData(uv, surfaceData) InitializeSurfaceData(input.positionWSColor, uv, surfaceData, input.blend);//Add a input "PositionColor" for vertical Gradient color

#include "../../Include/ShaderDebug.hlsl"
#include "./URP_ForwardPass.hlsl"
#endif