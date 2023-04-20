#ifndef URP_ROLE_HEAD_INCLUDED
#define URP_ROLE_HEAD_INCLUDED

#include "Role_Head.hlsl"

DECLARE_PROPERTY_WITH_TYPE(FVertexInput, float2, uv2.zw, FacemapUV)
DECLARE_PROPERTY_WITH_TYPE(FInterpolantsVSToPS, float2, CustomData1.xy, FacemapUV)
DECLARE_PROPERTY_WITH_TYPE(FFragData, float2, CustomData1.xy, FacemapUV)

#include "../Include/URP_LightingHead.hlsl"

TEX2D_SAMPLER(_FaceLightmap);
TEX2D_SAMPLER(_CameraDepthTexture);
#if !defined(ROLE_SRP_BATCH)
    FLOAT4 _ScreenSpaceRimParam;
#endif
#define _ScreenSpaceRimWidth _ScreenSpaceRimParam.x
#define _ScreenSpaceRimThreshold _ScreenSpaceRimParam.y
#define Remap(v,min,max,a,b) (linearstep(min,max,v) * (b - a) + a)

inline void CustomVertex(in FVertexInput Input, inout FInterpolantsVSToPS Interpolants)
{
    // 高度渐变
    TransformHeightGradient(Input, Interpolants);

    // 脸部光照贴图
    Interpolants.CustomData1.xy = Input.uv2.zw;

    SetSmartShadowValue(Interpolants, GetRoleSmartShadow());
}

#include "../Include/URP_Vertex.hlsl"

half GetFaceShadow(half2 uv)
{
    uv.x = mad(uv.x, _FaceLightingUvMulX, _FaceLightingUvAddX);
    half mask = SAMPLE_TEX2D(_FaceLightmap, uv).r;
    return smoothstep(_FaceLightingCompare - 0.1, _FaceLightingCompare + 0.1, mask);
}

float SampleSceneDepth(float2 uv)
{
    return SAMPLE_TEX2D(_CameraDepthTexture, uv).r;
}
            
inline float GetDepth(float2 targetUV)
{
    float rawDepth = SampleSceneDepth(targetUV);
    float depth = LinearEyeDepth(rawDepth, _ZBufferParams);
    return depth;
}

inline void SM_RampV2(in FMaterialData MaterialData, in FLightingContext LC,
                      FShadowData ShadowData, in FFragData FragData,
                      inout FLOAT3 DirectDiffuse DEBUG_ARGS)
{
    FLOAT srcShadow = GetSrcShadow(ShadowData);

    // TODO: 实现自投影
    // FLOAT selfShadow = GetSelfShadow(ShadowData);
    FLOAT selfShadow = 1;

    #ifdef FACE_SHADOW_PASS
        FLOAT hahaNdotL = 0;
    #elif _TOON_AO_FACE
        FLOAT2 facemapUV = FragData.CustomData1.xy;
        FLOAT hahaNdotL = GetFaceShadow(facemapUV);
        hahaNdotL = min(hahaNdotL, lerp(1, 1 - MaterialData.CustomParam.b, _MaskInt));
    #else
        FLOAT hahaNdotL = LC.NdotL * 0.5 + 0.5;
        FLOAT VdotLOffset = LC.VdotL * 0.5 + 0.5;
        hahaNdotL = saturate(hahaNdotL + lerp(darkFaceRampOffset, lightFaceRampOffset, VdotLOffset));
        hahaNdotL = min(hahaNdotL, lerp(1, 1 - MaterialData.CustomParam.b, _MaskInt));
        FLOAT IsFace = MaterialData.BaseColor.a * _ShadowInt;
        IsFace = lerp(0, IsFace, LC.VdotL);
        FLOAT FaceShadow = saturate(1.0f - (hahaNdotL + IsFace));
        FLOAT FaceShadowSmooth = smoothstep(0, 0.5f, FaceShadow);
        FLOAT IsShadowMask = step(srcShadow, 0.1);
        hahaNdotL = lerp(hahaNdotL, hahaNdotL * 0.1, IsShadowMask);
        hahaNdotL = lerp(1, hahaNdotL, FaceShadowSmooth);
        hahaNdotL = hahaNdotL * lerp(1, selfShadow, _MaskInt);
    #endif

    // <<< Head only
    FLOAT3 ramp = SAMPLE_TEX2D(_Ramp, FLOAT2(hahaNdotL, hahaNdotL)).xyz;
    DEBUG_CUSTOMDATA_PARAM(Ramp, ramp)

    // ndl & shadowmap
    FLOAT3 shadowLightColor = lerp(LC.LightColor, _RoleShadowColor.rgb, 1 - lerp(1, srcShadow, _MaskInt));
    FLOAT3 multipliedRamp = lerp(LC.DiffuseColor * _ShadowColorMultiply.rgb, LC.DiffuseColor, ramp);
    DirectDiffuse = multipliedRamp * shadowLightColor;

    #if defined(SCREEN_SPACE_RIM_PASS) && defined(_TOON_AO_FACE)
    
        FLOAT linearDepth = GetDepth(FragData.ScreenPosition.xy);
        // 不知道为什么用LinearEyeDepth有区别。 
        //LinearEyeDepth(FragData.SvPosition.z / FragData.SvPosition.w);
        FLOAT distanceScale = max(0, 0.01 / (sqrt(linearDepth) + 1)) * FLOAT2(_ScreenParams.y / _ScreenParams.x, 1.0);
        FLOAT2 offsetScale = _ScreenSpaceRimWidth * distanceScale;
    
        // 计算深度图采样UV
        FLOAT4 offsets;
        offsets.xy = offsetScale;
        offsets.zw = -offsetScale;
        FLOAT4 offsetPosSS01 = FragData.ScreenPosition.xyxy + offsets.xyxw;
        FLOAT4 offsetPosSS23 = FragData.ScreenPosition.xyxy + offsets.zyzw;

        // 计算深度 
        FLOAT4 depthOffset;
        depthOffset.x = GetDepth(offsetPosSS01.xy);
        depthOffset.y = GetDepth(offsetPosSS01.zw);
        depthOffset.z = GetDepth(offsetPosSS23.xy);
        depthOffset.w = GetDepth(offsetPosSS23.zw);
        FLOAT depthCompare = linearDepth + _ScreenSpaceRimThreshold;
        FLOAT4 compareResult = step(depthCompare, depthOffset);

        FLOAT2 rim2 = max(compareResult.xy, compareResult.zw);
        FLOAT rim = max(rim2.x, rim2.y);
                    
        FLOAT disFade = linearstep(15, 5, linearDepth);
        disFade *= disFade;
        FLOAT3 rimColor = _RoleRimColor * rim * _MainLightColor.xyz * lerp(GetSrcShadow(ShadowData), 1, _ShadowRimIntensity) * disFade;
        rimColor *= lerp(normalize(MaterialData.BaseColor.xyz + 0.01), MaterialData.BaseColor.xyz, _RoleRimMultiply);
        DirectDiffuse += rimColor;
    
    #endif

    DirectDiffuse = lerp(DirectDiffuse, LC.FixNdotL, _ContrastLight);

    DEBUG_CUSTOMDATA_PARAM(DirectDiffuse, DirectDiffuse)
}

void CustomLighting(FFragData FragData, inout FMaterialData MaterialData,
                    FShadowData ShadowData,FLOAT ShadowMask,
                    inout FLOAT3 DirectDiffuse, inout FLOAT3 DirectSpecular DEBUG_ARGS)
{
    FLightingContext LC = InitLight(FragData, MaterialData,_SpecMult,
                                    _MainLightDir1.xyz,_MainLightColor.xyz, 1, ShadowData, _ShadowColor.xyz);

    SM_RampV2(MaterialData, LC, ShadowData, FragData, DirectDiffuse DEBUG_PARAM);

    SM_StandardSpec(LC, DirectSpecular DEBUG_PARAM);
    DEBUG_CUSTOMDATA_PARAM(LC0, LC)

    // UNITY_BRANCH
    // if (_AddLightEnable)
    // {
    //     #define specScale1 _SpecMult*0.5f
    //     FLightingContext LC1 = InitLight(FragData, MaterialData,specScale1,
    //                                      _AddLightDir.xyz,_AddLightColor.xyz, 1, DefaultShadowData(), 1);
    //     SM_SupplementDiffuse(LC1, DirectDiffuse DEBUG_PARAM);
    //     SM_SupplementSpec(LC1, DirectSpecular DEBUG_PARAM);
    //     DEBUG_CUSTOMDATA_PARAM(LC1, LC1)
    // }
    MaterialData.BaseColor.a = 1;
}

#include "../../Include/ShaderDebug.hlsl"
#include "../Include/URP_Pixel.hlsl"

#endif
