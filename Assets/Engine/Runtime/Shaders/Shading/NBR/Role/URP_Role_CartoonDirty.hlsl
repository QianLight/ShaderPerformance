#ifndef URP_ROLE_CARTOON_DIRTY_INCLUDED
#define URP_ROLE_CARTOON_DIRTY_INCLUDED

#define _VERTEX_COLOR
#define _CUSTOM_EFFECT
#define _CUSTOM_VERTEX_PARAM
#define _ROLE_HEIGHT_GRADIENT
#define _INPUT_UV2_4

half4 _FaceLightingParam;
#define _FaceLightingUvMulX _FaceLightingParam.x
#define _FaceLightingUvAddX _FaceLightingParam.y
#define _FaceLightingCompare _FaceLightingParam.z

//----------------------
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

#define _MaskTexDirty _MainTex2
#define _DirtyInt _Param3.x
#define _NoiseInt _Param3.y
#define _NoiseColor _Color6.xyz
#define _linecolor _Color2.xyz

#include "Role_Head.hlsl"

DECLARE_PROPERTY_WITH_TYPE(FVertexInput, float2, uv2.zw, FacemapUV)
DECLARE_PROPERTY_WITH_TYPE(FInterpolantsVSToPS, float2, CustomData1.xy, FacemapUV)
DECLARE_PROPERTY_WITH_TYPE(FFragData, float2, CustomData1.xy, FacemapUV)
TEX2D_SAMPLER(_FaceLightmap);

#include "../Include/URP_LightingHead.hlsl"

inline void CustomVertex(in FVertexInput Input, inout FInterpolantsVSToPS Interpolants)
{
    TransformHeightGradient(Input, Interpolants);
    SetSmartShadowValue(Interpolants, GetRoleSmartShadow());

    // 脸部光照贴图
    Interpolants.CustomData1.xy = Input.uv2.zw;
}

half GetFaceShadow(half2 uv)
{
    uv.x = mad(uv.x, _FaceLightingUvMulX, _FaceLightingUvAddX);
    half mask = SAMPLE_TEX2D(_FaceLightmap, uv).r;
    return smoothstep(_FaceLightingCompare - 0.1, _FaceLightingCompare + 0.1, mask);
}

#include "../Include/URP_Vertex.hlsl"
//Lighting
#define _CUSTOM_LIGHT

inline void SM_RampDirty(in FMaterialData MaterialData, in FLightingContext LC,
    FLOAT ShadowMask, in FFragData FragData,
    inout REAL3 DirectDiffuse DEBUG_ARGS)
{

    REAL hahaNdotL = LC.NdotL * 0.5 + 0.5;
    REAL VdotLOffset = LC.VdotL * 0.5 + 0.5;

    #ifdef FACE_SHADOW_PASS
        hahaNdotL = 0;
    #elif _TOON_AO_FACE
        FLOAT2 facemapUV = FragData.CustomData1.xy;
        hahaNdotL = GetFaceShadow(facemapUV);
        hahaNdotL = min(hahaNdotL, lerp(1, 1 - MaterialData.CustomParam.b, _MaskInt));
    #else
        hahaNdotL = saturate(hahaNdotL + lerp(darkFaceRampOffset, lightFaceRampOffset, VdotLOffset));

        #ifdef _HEAD          
        half IsFace = MaterialData.BaseColor.a * _ShadowInt;
        REAL LV = LC.VdotL;
        IsFace = lerp(0, IsFace, LV);
        half FaceShadow = saturate(1.0f - (hahaNdotL + IsFace));
        half FaceShadowSmooth = smoothstep(0, 0.5f, FaceShadow);
        half IsShadowMask = step(ShadowMask, 0.1);
        hahaNdotL = lerp(hahaNdotL, hahaNdotL * 0.1, IsShadowMask);
        hahaNdotL = lerp(1, hahaNdotL, FaceShadowSmooth);
        #endif//_HEAD
        hahaNdotL = min(hahaNdotL, lerp(1, 1 - MaterialData.CustomParam.b, _MaskInt));
    #endif

    REAL3 ramp = min(SAMPLE_TEX2D(_Ramp, FLOAT2(hahaNdotL, hahaNdotL)).xyz, lerp(1, ShadowMask, _MaskInt) * hahaNdotL);

    DirectDiffuse = lerp(LC.DiffuseColor * _ShadowColorMultiply.xyz, LC.DiffuseColor, ramp) * LC.LightColor;

    DEBUG_CUSTOMDATA_PARAM(Ramp, ramp)

    DirectDiffuse = lerp(DirectDiffuse, LC.FixNdotL, _ContrastLight);

    DEBUG_CUSTOMDATA_PARAM(DirectDiffuse, DirectDiffuse)

}

void CustomLighting(FFragData FragData, FMaterialData MaterialData,
                    FShadowData ShadowData,FLOAT ShadowMask,
                    inout FLOAT3 DirectDiffuse, inout FLOAT3 DirectSpecular DEBUG_ARGS)
{
    FLOAT2 Shadow = ShadowData.Shadow.xy;

    //dirty
    REAL2 DirtyMapTex = SAMPLE_TEX2D(_MaskTexDirty, GET_FRAG_UV).rg;
    REAL DirtyMap = DirtyMapTex.r;
    REAL DirtyMapInvert = 1 - DirtyMap;
    REAL3 DirtyMapColor = DirtyMapInvert * _linecolor;
    REAL3 DirectDiffuseDirty = MaterialData.DiffuseColor + DirtyMapColor;
    REAL3 DirectDiffuseline = lerp(MaterialData.DiffuseColor * DirtyMap, DirectDiffuseDirty, _DirtyInt);
    MaterialData.DiffuseColor = lerp(MaterialData.DiffuseColor, DirectDiffuseline, _Color2.a);
    REAL DirtyMapsec = DirtyMapTex.g;
    REAL3 DirtyMapsecolor = DirtyMapsec * _NoiseColor.xyz;
    DirtyMapsecolor = lerp(MaterialData.DiffuseColor, DirtyMapsecolor * MaterialData.DiffuseColor, _NoiseInt);
    MaterialData.DiffuseColor = lerp(MaterialData.DiffuseColor, DirtyMapsecolor, DirtyMapsec);

    REAL3 shadowLightColor = lerp(_MainLightColor.xyz, _RoleShadowColor.rgb, (1 - Shadow.y));
    FLightingContext LC = InitLight(FragData, MaterialData,_SpecMult,
                                    _MainLightDir.xyz, shadowLightColor.xyz, 1, ShadowData,
                                    _ShadowColor.xyz);
    SM_RampDirty(MaterialData, LC, Shadow.y, FragData, DirectDiffuse DEBUG_PARAM);

    DEBUG_CUSTOMDATA_PARAM(DirectDiffuse, DirectDiffuse)

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
}

#include "../Include/URP_Pixel.hlsl"

#endif
