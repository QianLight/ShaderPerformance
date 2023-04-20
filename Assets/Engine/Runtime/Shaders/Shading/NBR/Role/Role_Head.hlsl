#ifndef _Role_Lighting
#define _Role_Lighting

#include "../API/CommonAPI.hlsl"

#define _TICK_NORMAL
#define _OBJECT_ROOT_SHADOW

#define _CUSTOM_VERTEX_PARAM

// 强制打开ShadowMap
#ifndef _SHADOW_MAP
    #define _SHADOW_MAP
#endif

// #ifndef _CUSTOM_EFFECT
//     #define _CUSTOM_EFFECT
// #endif

// #ifndef _CUSTOM_VERTEX_PARAM
//     #define _CUSTOM_VERTEX_PARAM
// #endif

// #ifndef _VERTEX_COLOR
//     #define _VERTEX_COLOR
// #endif

#if !defined(ROLE_SRP_BATCH)
FLOAT4 _RootPosWS;
#endif
FLOAT4 GetRootPos()
{
    // return FLOAT4(_RootPosWS.xyz, 1);
    FLOAT3 pos = unity_ObjectToWorld._14_24_34 /*+ _RootPosWS.xyz*/;
    return FLOAT4(pos, 1);
}
FLOAT GetGroundHeight()
{
    // return _RootPosWS.w;
    return 1;
}

#include "../Include/PCH.hlsl"



#define _HeightGradientPreviewStart _HeightGradientPreviewParam.x
#define _HeightGradientPreviewEnd _HeightGradientPreviewParam.y
#define _HeightGradientPreviewFade _HeightGradientPreviewParam.z
#define _HeightGradientPreviewEnable _HeightGradientPreviewParam.w > 0

#if defined(_ROLE_HEIGHT_GRADIENT)
DECLARE_PROPERTY(FVertexInput, Color.a, HeightGradientFactor)
DECLARE_PROPERTY(FInterpolantsVSToPS, CustomData.w, HeightGradientFactor)
DECLARE_PROPERTY(FFragData, CustomData.w, HeightGradientFactor)
#endif

DECLARE_INTERPOLANTS_PROPERTY(FLOAT, CustomData1.w, SmartShadowValue)

#ifdef _NO_COMMON_EFFECT
    #define _NO_COLOR_EFFECT
    #define _NO_EMISSIVE
    #define _NO_AO
    #define _PBS_NO_IBL
#endif//_NO_COMMON_EFFECT

inline void TransformHeightGradient(in FVertexInput Input,inout FInterpolantsVSToPS Interpolants)
{
    #ifdef _ROLE_HEIGHT_GRADIENT
    #ifdef _DEBUG_APP
    if (_HeightGradientPreviewEnable)
    {
        FLOAT height = Interpolants.WorldPosition.y - GetRootPos().y;
        FLOAT factor = pow(linearstep(_HeightGradientPreviewEnd, _HeightGradientPreviewStart, height), _HeightGradientPreviewFade); 
        SetHeightGradientFactor(Interpolants, factor);	
    }
    else
    {
        SetHeightGradientFactor(Interpolants, GetHeightGradientFactor(Input) * _HeightGradientPreivewColor.a);
    }
    #else
    SetHeightGradientFactor(Interpolants, GetHeightGradientFactor(Input) * _HeightGradientColor.a);
    #endif
    #endif
}

void HeightGradient(inout REAL3 color, FFragData FragData)
{
    #ifdef _ROLE_HEIGHT_GRADIENT
        #if _DEBUG_APP
        if (_HeightGradientPreviewEnable)
        {
            color = color * lerp(1, _HeightGradientPreivewColor.rgb, GetHeightGradientFactor(FragData));
        }
        else
        {
            color = color.rgb * lerp(1, _HeightGradientColor.rgb, GetHeightGradientFactor(FragData));
        }
        #else
        color = color.rgb * lerp(1, _HeightGradientColor.rgb, GetHeightGradientFactor(FragData));
        #endif
    #endif
}

#ifdef _CARTOON
    #define _MaskTex _MainTex1
    #define _MaskParam _Param2

    #define _MaskEnable _MaskParam.x>0.5
    #define _Emissive_Mask _Color5.w
    #define _Emissive_Color _Color5
    #define _ToonAoScale (_Param2.a)

    #ifdef _CUSTOM_EFFECT
        inline void CalcCustomEffect(in FFragData FragData,inout FMaterialData MaterialData)
        {
        #ifndef _NO_MASK
            UNITY_BRANCH
            if(_MaskEnable)
            {                                    
                MaterialData.CustomParam = SAMPLE_TEX2D(_MaskTex, GET_FRAG_UV);
            }          
            #ifdef _DIRTY
                FLOAT DirtyNoise = SAMPLE_TEX2D(_MaskTexDirty, GET_FRAG_UV+FLOAT2(_Time.x,0)*_NoiseDisturbSpeed).b;
                FLOAT2 Noise= FLOAT2(DirtyNoise,DirtyNoise);
                MaterialData.CustomParam.g = SAMPLE_TEX2D(_MaskTex, (GET_FRAG_UV+Noise*_NoiseDisturbInt)).g*_ColorInt;
            #endif 
            MaterialData.Emissive = _Emissive_Mask * _Emissive_Color.xyz * MaterialData.CustomParam.g;
            // MaterialData.Emissive =  MaterialData.CustomParam.g;
        #endif
        }
    #endif //_CUSTOM_EFFECT

    #define _CUSTOM_SHADOWMASK
    FLOAT CustomShadowMask(in FFragData FragData, in FMaterialData MaterialData, FLOAT2 Shadow)
    {
        FLOAT4 mask = MaterialData.CustomParam;
        FLOAT shadowMask = 1 - mask.b;
        return min(Shadow.y, shadowMask);	
    }

    FLOAT GetCustumShadowMapMask(in FFragData FragData,in FMaterialData MaterialData)
    {
        FLOAT RoleMask = saturate(dot(MaterialData.WorldNormal,  _MainLightDir.xyz));
        RoleMask = LinearStep(-0.75,0.2,RoleMask);
        return RoleMask;
    }
    #define _CUSTOM_SHADOW_MAP_MASK

    #define _MaskInt _Color6.w
    #define _ShadowInt _Param3.z  
    
    #ifndef _CUSTOM_RAMP_PARAM
	    #define _Ramp _ProcedureTex2
	    #define _RampParam _Param3
	    #define _ShadowColorMultiply _Color3
    #endif//_CUSTOM_RAMP_PARAM

    REAL4 _RoleRamp;
    #define lightFaceRampOffset _RoleRamp.x
    #define darkFaceRampOffset _RoleRamp.y

    inline void SM_Ramp(in FFragData FragData,in FMaterialData MaterialData,in FLightingContext LC,
	    FLOAT Shadow,
	    inout REAL3 DirectDiffuse DEBUG_ARGS)
    {
        // SAMPLE_TEX2D(lightMask,);
	    REAL hahaNdotL = LC.NdotL* 0.5 + 0.5;
	    REAL VdotLOffset = LC.VdotL * 0.5 + 0.5;
	    REAL hahaNdotV = max(SG(LC.NdotV,0.1), LC.NdotL);
	    hahaNdotL = saturate(hahaNdotL + lerp(darkFaceRampOffset, lightFaceRampOffset, VdotLOffset));
        #define _LightMap _ProcedureTex3
        FLOAT4 masks = SAMPLE_TEX2D(_LightMap, GET_FRAG_UV);
    #ifdef _CUSTOMSHADOW	
	    #define _CustomshadowInt _Param2.w
	    #define _CustomshadowColor _Color6.xyz
	    #define _CustomEdgeInt _Color6.w	
	    FLOAT CustomShadow = 1 - masks.g;
	    hahaNdotV = min(hahaNdotV, Shadow * 0.5 + 0.5);
    #else
	    hahaNdotL = min(hahaNdotL, Shadow);
    #endif	
        REAL NdotL_light_UV = hahaNdotL * masks.a;
	    
    #ifdef _SPHERE_LIGHTING    
        REAL3 ramp = SAMPLE_TEX2D(_Ramp, NdotL_light_UV).xyz;
    #else
        REAL3 ramp = SAMPLE_TEX2D(_Ramp, FLOAT2 (hahaNdotV, hahaNdotV)).xyz;
    #endif    


        // ndl & shadowmap
        REAL3 multipliedRamp = lerp(
            LC.DiffuseColor * _ShadowColorMultiply.rgb,
            LC.DiffuseColor,
            ramp * lerp(1, Shadow, _RoleShadowMultiply)
        );

        #ifdef _STYLE_HAIR
        multipliedRamp *= lerp(_HairLineColor.rgb, 1, masks.b);
        #endif
        
        DirectDiffuse = LC.LightColor * multipliedRamp;

    #ifdef _CUSTOMSHADOW
	    DirectDiffuse = lerp( DirectDiffuse , DirectDiffuse * _CustomshadowColor, CustomShadow * _CustomshadowInt);
    #endif

	    DEBUG_CUSTOMDATA_PARAM(DirectDiffuse, DirectDiffuse)
    }

    inline void SM_Ramp01(in FMaterialData MaterialData, in FLightingContext LC,
        FLOAT ShadowMask, in FFragData FragData,
        inout REAL3 DirectDiffuse DEBUG_ARGS)
    {

        REAL hahaNdotL = LC.NdotL * 0.5 + 0.5;
        REAL VdotLOffset = LC.VdotL * 0.5 + 0.5;
        hahaNdotL = saturate(hahaNdotL + lerp(darkFaceRampOffset, lightFaceRampOffset, VdotLOffset));

#ifdef _HEAD          
        REAL IsFace = MaterialData.BaseColor.a * _ShadowInt;
        REAL LV = LC.VdotL;
        IsFace = lerp(0, IsFace, LV);
        FLOAT FaceShadow = saturate(1.0f - (hahaNdotL + IsFace));
        FLOAT FaceShadowSmooth = smoothstep(0, 0.5f, FaceShadow);
        FLOAT IsShadowMask = step(ShadowMask, 0.1);
        hahaNdotL = lerp(hahaNdotL, hahaNdotL * 0.1, IsShadowMask);
        hahaNdotL = lerp(1, hahaNdotL, FaceShadowSmooth);
#endif//_HEAD

        REAL3 ramp = min(SAMPLE_TEX2D(_Ramp, REAL2(hahaNdotL, hahaNdotL)).xyz, lerp(1, ShadowMask, _MaskInt) * hahaNdotL);

        DirectDiffuse = lerp(LC.DiffuseColor * _ShadowColorMultiply.xyz, LC.DiffuseColor, ramp) * LC.LightColor;

        DEBUG_CUSTOMDATA_PARAM(Ramp, ramp)

        DirectDiffuse = lerp(DirectDiffuse, LC.FixNdotL, _ContrastLight);

        DEBUG_CUSTOMDATA_PARAM(DirectDiffuse, DirectDiffuse)

    }

    #define _POST_CUSTOM_LIGHTING
    REAL3 CalcPostCustomLighting(in FFragData FragData, in FMaterialData MaterialData,
        in FLOAT3 IBL,in REAL3 color,inout REAL opaque, in FLOAT ShadowMask DEBUG_ARGS)
    {
        #define VertexNormal FragData.TangentToWorld[2].xyz	
        #ifndef _SM_DARK_RIM
            _RimParam.zw = REAL2(0, 0);
        #endif

        REAL dis = length(FragData.WorldPosition_CamRelative);
        REAL disFade = linearstep(_RimDisFadeEnd,_RimDisFadeBegin,dis);
        disFade *= disFade;
        
        FLOAT mask = 1 - MaterialData.CustomParam.r;
        REAL3 WorldNormal = lerp(VertexNormal, MaterialData.WorldNormal, _RimNormalMapWeight);
        REAL NdotV = max(1e-4, dot(FragData.CameraVector, WorldNormal));
        REAL rawNdotL = dot(WorldNormal, _MainLightDir.xyz);

#ifdef _SM_DARK_RIM
        color *= SM_DarkRim(NdotV, rawNdotL,
            _DarkRimCutLocation, _DarkRimCutSmoothness
            DEBUG_PARAM);
#endif

#ifdef _SM_RIM
        color += SM_Rim(FragData, MaterialData,
            disFade, mask, color,
            NdotV, rawNdotL, ShadowMask
            DEBUG_PARAM);
#endif

        HeightGradient(color.rgb, FragData);
        
        DEBUG_CUSTOMDATA_PARAM(PostCustomLighting, color)

        return color;
    }
    
    //point light ramp
    #define _CUSTOM_ADDNOL
    REAL CalcCustomAddNoL(FFragData FragData,FMaterialData MaterialData,REAL3 lightDir)
    {
        return dot(FragData.TangentToWorld[2].xyz, lightDir);
    }

    DECLARE_PROPERTY(FMaterialData, CustomParam.w, ToonAO)

    #define _CUSTOM_ADDDIFFUSE
    REAL3 CalcCustomAddDiffuse(in FFragData FragData, in FMaterialData MaterialData,in REAL safeNol,in FLOAT Shadow, in LightInfo li, in FLOAT atten
	    #ifdef _ENABLE_DEBUG
		    , inout LightDebug ld
	    #endif
        )
    {
        REAL ramp = saturate((safeNol + (GetToonAO(MaterialData) * 2 - 1) * _ToonAoScale) * _DynamicLightInvSoftness);
        REAL3 result = lerp(_DynamicLightAmbient, _DynamicLightColor, ramp) * atten * lerp(MaterialData.DiffuseColor, 1, _DynamicLightCoverage);

        #ifdef _ENABLE_DEBUG
            ld.toonAO = GetToonAO(MaterialData);
            ld.softness = 1 / _DynamicLightInvSoftness;
            ld.ramp = ramp;
            ld.atten = atten;
            ld.lightColor = _DynamicLightColor;
            ld.ambient = _DynamicLightAmbient;
            ld.result = result;
        #endif

        return result;
    }

#endif//_CARTOON

#endif