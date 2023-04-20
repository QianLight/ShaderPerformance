#ifndef URP_ROLE_CARTOON_INCLUDED
#define URP_ROLE_CARTOON_INCLUDED

#include "Role_Head.hlsl"
#include "../Include/URP_LightingHead.hlsl"

inline void CustomVertex(in FVertexInput Input, inout FInterpolantsVSToPS Interpolants)
{
	TransformHeightGradient(Input, Interpolants);
	SetSmartShadowValue(Interpolants, GetRoleSmartShadow());
}

#include "../Include/URP_Vertex.hlsl"
//Lighting
#define _CUSTOM_LIGHT

inline void SM_RampV2(in FMaterialData MaterialData, in FLightingContext LC,
                      FLOAT Shadow, in FFragData FragData,
                      inout FLOAT3 DirectDiffuse DEBUG_ARGS)
{
	// calc ramp
	FLOAT hahaNdotL = LC.NdotL * 0.5 + 0.5;
	FLOAT VdotLOffset = LC.VdotL * 0.5 + 0.5;
	hahaNdotL = saturate(hahaNdotL + lerp(darkFaceRampOffset, lightFaceRampOffset, VdotLOffset));
	hahaNdotL = min(hahaNdotL, lerp(1, 1 - MaterialData.CustomParam.b, _MaskInt));

	#ifdef _DISABLE_RAMP_TEXTURE
	REAL3 ramp = linearstep(0.4, 0.6, hahaNdotL);
	#else
	REAL3 ramp = SAMPLE_TEX2D(_Ramp, REAL2(hahaNdotL, hahaNdotL)).xyz;
	#endif 

	// ndl & shadowmap

	
	FLOAT maskShadow = _MaskInt * -Shadow + _MaskInt; // FLOAT maskShadow = 1 - lerp(1, Shadow, _MaskInt); 
	REAL3 shadowLightColor = lerp(LC.LightColor, _RoleShadowColor.rgb, maskShadow);
	REAL3 multipliedRamp = lerp(LC.DiffuseColor * _ShadowColorMultiply.rgb, LC.DiffuseColor, ramp);
	DirectDiffuse = multipliedRamp * shadowLightColor;

	DirectDiffuse = lerp(DirectDiffuse, LC.FixNdotL, _ContrastLight);
}

void CustomLighting(FFragData FragData, inout FMaterialData MaterialData,
                    FShadowData ShadowData,FLOAT ShadowMask,
                    inout REAL3 DirectDiffuse, inout REAL3 DirectSpecular DEBUG_ARGS)
{
	FLightingContext LC = InitLight(FragData, MaterialData,_SpecMult,_MainLightDir.xyz,
		_MainLightColor.xyz, 1, ShadowData, _ShadowColor.xyz);
	SM_RampV2(MaterialData, LC, GetSrcShadow(ShadowData), FragData, DirectDiffuse DEBUG_PARAM);
	SM_StandardSpec(LC, DirectSpecular DEBUG_PARAM);

	MaterialData.BaseColor.a = 1;
}

#include "../../Include/ShaderDebug.hlsl"
#include "../Include/URP_Pixel.hlsl"

#endif