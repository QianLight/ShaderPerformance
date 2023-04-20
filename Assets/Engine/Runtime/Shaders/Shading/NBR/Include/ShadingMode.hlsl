// Copyright 2018- PWRD, Inc. All Rights Reserved.
#include "BRDF.hlsl" 
#include "URP_LightingHead.hlsl"
#include "DefaultDebugHead.hlsl"

#ifndef PBS_SHADINGMODE_INCLUDE
#define PBS_SHADINGMODE_INCLUDE

#define _FalloffColor(LC) (LC.Falloff.xyz)
#define _FalloffWeight(LC) (LC.Falloff.w)
#define _GIMode (_AmbientParam.w)

FLightingContext InitLight(FFragData FragData,FMaterialData MaterialData,FLOAT specScale,
	REAL3 LightDir,FLOAT3 LightColor,FShadowData ShadowData,FLOAT3 ShadowColor)
{
	DECLARE_OUTPUT(FLightingContext, LC);
	LC.DiffuseColor = MaterialData.DiffuseColor;
	LC.SpecularColor = MaterialData.SpecularColor*specScale;
	LC.LightDir = LightDir;
	LC.LightColor = LightColor;
	LC.NdotL = dot(MaterialData.WorldNormal, LightDir);
	LC.FixNdotL = saturate(LC.NdotL);
	LC.NdotV = MaterialData.NdotV;

	LC.R = MaterialData.Roughness;
	LC.R2 = MaterialData.Roughness2;
	_FalloffColor(LC) = ShadowColor*GetFadeShadow(ShadowData);
	_FalloffWeight(LC) = GetSrcShadow(ShadowData);
	LC.LightMask = saturate(LC.FixNdotL*_FalloffWeight(LC));
	return LC;

}

FLightingContext InitLight(FFragData FragData,FMaterialData MaterialData,FLOAT specScale,
	REAL3 LightDir,FLOAT3 LightColor,FLOAT Falloff, FShadowData ShadowData,FLOAT3 ShadowColor)
{
	DECLARE_OUTPUT(FLightingContext, LC);
	LC.DiffuseColor = MaterialData.DiffuseColor;
	LC.SpecularColor = MaterialData.SpecularColor*specScale;
    LC.LightDir = LightDir;
	LC.LightColor = LightColor;
	LC.NdotL = dot(MaterialData.WorldNormal, LightDir);
	LC.FixNdotL = saturate(LC.NdotL);
	LC.NdotV = MaterialData.NdotV;
	LC.VdotL = dot(FragData.CameraVector, LightDir);

	#ifdef _UNREAL_MODE		
		LC.H = saturate(rsqrt( 2 + 2 * LC.VdotL ));
		LC.NdotH = saturate( ( LC.FixNdotL + LC.NdotV ) * LC.H );
		LC.VdotH = saturate( LC.H + LC.H * LC.VdotL );
	#else
		LC.H = SafeNormalize(FragData.CameraVector + LightDir);
		LC.NdotH = saturate(dot(MaterialData.WorldNormal, LC.H));
		LC.LdotH = saturate(dot(LightDir, LC.H));
	#endif

	LC.R = MaterialData.Roughness;
	LC.R2 = MaterialData.Roughness2;
	_FalloffColor(LC) = ShadowColor*GetFadeShadow(ShadowData);
	_FalloffWeight(LC) = GetSrcShadow(ShadowData);
	LC.LightMask = saturate(LC.FixNdotL*_FalloffWeight(LC));
	return LC;
}

FLOAT3 SpecularGGXOrig( FLOAT Roughness2,FLOAT NdotV,FLOAT VdotH,FLOAT NdotH,FLOAT FixNdotL,FLOAT3 SpecularColor)
{
	FLOAT a2 = Roughness2*Roughness2;	
	// Generalized microfacet specular
	FLOAT D = D_GGX2(Roughness2, a2, NdotH );
	FLOAT Vis = Vis_SmithJointApprox(Roughness2, NdotV, FixNdotL );
	FLOAT3 F = F_Schlick( SpecularColor, VdotH );
	return min(abs((D * Vis)* F),10);
}

FLOAT3 SpecularGGX( FLightingContext LC)
{
	FLOAT a2 = LC.R2*LC.R2;	
	// Generalized microfacet specular
	FLOAT D = D_GGX2(LC.R2, a2, LC.NdotH );
	FLOAT Vis = Vis_SmithJointApprox( LC.R2, LC.NdotV, LC.FixNdotL );
	FLOAT3 F = F_Schlick( LC.SpecularColor, LC.VdotH );
	return min(abs((D * Vis)* F),10);
}
FLOAT3 SpecularGGX02( FLightingContext LC)
{
	FLOAT a2 = LC.R2;	
	// Generalized microfacet specular
	FLOAT D = D_GGX(a2, LC.NdotH );
	      D = lerp(1,D*1.5, _SpecAdd);
	FLOAT Vis = Vis_SmithJointApprox( LC.R2, LC.NdotV, LC.FixNdotL ); //暗部这里去掉了G项,效果不明显,用常量替代了.
	FLOAT3 F = F_Schlick( LC.SpecularColor, LC.VdotH );
	return min(abs((D *0.35)* F),5);
}
FLOAT3 SpecularAdd( FLightingContext LC)
{
	FLOAT a2 = LC.R2;	
	// Generalized microfacet specular
	FLOAT D = D_GGX( a2, LC.NdotH );
	FLOAT Vis = Vis_SmithJointApprox( LC.R2, LC.NdotV, LC.FixNdotL );
	FLOAT3 F = F_Schlick( LC.SpecularColor, LC.VdotH );
	return D * Vis* F*1.2;
}
void DualSpecularGGX( FLightingContext LC,FLightingContext LC1,
	out FLOAT3 spec0,out FLOAT3 spec1)
{
	FLOAT a2 = LC.R2*LC.R2;	
	FLOAT D0 = D_GGX2( LC.R2, a2, LC.NdotH );
	FLOAT Vis0 = Vis_SmithJointApprox( LC.R2, LC.NdotV, LC.FixNdotL );
	FLOAT3 F0 = F_Schlick( LC.SpecularColor, LC.VdotH );
	spec0 = (D0 * Vis0) * F0;

	FLOAT D1 = D_GGX2( LC.R2, a2, LC1.NdotH );
	FLOAT Vis1 = Vis_SmithJointApprox( LC1.R2, LC1.NdotV, LC1.FixNdotL );
	FLOAT3 F1 = F_Schlick( LC1.SpecularColor, LC1.VdotH );
	spec1 = (D1 * Vis1) * F1;
}

void DualSpecularGGX2( FLightingContext LC,FLightingContext LC1,
	out FLOAT3 spec0,out FLOAT3 spec1)
{
	FLOAT a2 = LC.R2*LC.R2;	
	FLOAT V0 = Vis_SmithJointApprox (LC.R, LC.NdotV, LC.FixNdotL);
	FLOAT D0 = D_GGX2 (LC.R2, a2, LC.NdotH);
	FLOAT specularTerm0 = V0*D0;
	specularTerm0 = max(0, specularTerm0 * LC.FixNdotL);
	spec0 = specularTerm0 * FresnelTerm (LC.SpecularColor, LC.LdotH);

	FLOAT V1 = Vis_SmithJointApprox (LC1.R, LC1.NdotV, LC1.FixNdotL);
	FLOAT D1 = D_GGX2 (LC.R2, a2, LC1.NdotH);
	FLOAT specularTerm1 = V1*D1;
	specularTerm1 = max(0, specularTerm1 * LC1.FixNdotL);
	spec1 = specularTerm1 * FresnelTerm (LC1.SpecularColor, LC1.LdotH);
}

FLOAT3 SpecularUnityOrig(REAL R,REAL R2,REAL NdotH,REAL LdotH,FLOAT3 SpecularColor)
{ 
	REAL roughness2MinusOne = R2 - 1;
    REAL d = NdotH * NdotH * roughness2MinusOne + 1.00001f;
    REAL LoH2 = LdotH * LdotH;
	REAL normalizationTerm = R * 4.0h + 2.0h;

    REAL specularTerm = R2 / ((d*d) * max(0.1h, LoH2) * normalizationTerm);
	specularTerm = specularTerm - HALF_MIN;
	specularTerm = clamp(specularTerm, 0.0, 100.0);
	//return d.xxx;
    return specularTerm * SpecularColor;
}

FLOAT3 SpecularUnity(FLightingContext LC)
{
	return SpecularUnityOrig(LC.R,LC.R2, LC.NdotH,LC.LdotH,LC.SpecularColor);
}

void DualSpecularUnity(FLightingContext LC,FLightingContext LC1,
	out FLOAT3 spec0,out FLOAT3 spec1)
{
	spec0 = SpecularUnity(LC);
	spec1 = SpecularUnity(LC1);
}

void SM_Unlit(in FLOAT3 Color,
	inout FLOAT3 DirectDiffuse,inout FLOAT3 DirectSpecular DEBUG_ARGS)
{
	DirectDiffuse = Color;
	DirectSpecular = FLOAT3(0,0,0);	
}
void CalcShadowColor(in FLightingContext LC,
	inout FLOAT3 DirectDiffuse DEBUG_ARGS)
{
	DirectDiffuse = lerp(_FalloffColor(LC)*DirectDiffuse,DirectDiffuse,_FalloffWeight(LC));
}

inline void SM_Lightmap(in FMaterialData MaterialData,FLOAT3 GI,
FLOAT dynamicShadow,FLOAT shadowMask,
	inout FLOAT3 DirectDiffuse DEBUG_ARGS)
{
	// FLOAT3 AddShadow= smoothstep(0,0.1,GI);
    FLOAT3 DiffuseGI = GI * MaterialData.DiffuseColor;
	DEBUG_CUSTOMDATA_PARAM(DiffuseGI, DiffuseGI)

	FLOAT lightmapShadow = lerp(1,dynamicShadow,shadowMask);
	DEBUG_CUSTOMDATA_PARAM(LightmapShadow, lightmapShadow)
	
	//DirectDiffuse = DirectDiffuse + DiffuseGI*lightmapShadow;
	DirectDiffuse = DirectDiffuse + DiffuseGI;
	DEBUG_CUSTOMDATA_PARAM(AddGI, DirectDiffuse)
}

void SM_Standard(in FLightingContext LC,
	inout FLOAT3 DirectDiffuse,inout FLOAT3 DirectSpecular DEBUG_ARGS)
{
	DirectDiffuse = LC.DiffuseColor;
	CalcShadowColor(LC,DirectDiffuse DEBUG_PARAM);
	DirectDiffuse *= LC.LightColor*LC.FixNdotL;
	
	#ifdef _UNREAL_MODE
		DirectSpecular = LC.FixNdotL * LC.LightColor * SpecularGGX(LC)*LC.LightMask;
	#else//!_UNREAL_MODE
		FLOAT3 Specular = SpecularUnity(LC);
		DEBUG_CUSTOMDATA_PARAM(Specular, Specular)
		DirectSpecular = LC.FixNdotL * LC.LightColor * Specular*LC.LightMask;
	#endif//_UNREAL_MODE
	DEBUG_CUSTOMDATA_PARAM(DirectDiffuse, DirectDiffuse)
	DEBUG_CUSTOMDATA_PARAM(DirectSpecular, DirectSpecular)
}



void SM_StandardSpec(in FLightingContext LC,
	inout FLOAT3 DirectSpecular DEBUG_ARGS)
{
	// DirectDiffuse = LC.DiffuseColor;
	// CalcShadowColor(LC,DirectDiffuse DEBUG_PARAM);
	// DirectDiffuse *= LC.LightColor*LC.FixNdotL;
	#ifdef _UNREAL_MODE		
		DirectSpecular = LC.FixNdotL * LC.LightColor * SpecularGGX(LC)*LC.LightMask;
	#else//!_UNREAL_MODE
		FLOAT3 Specular = SpecularUnity(LC);
		DEBUG_CUSTOMDATA_PARAM(Specular, Specular)
		DirectSpecular = LC.FixNdotL * LC.LightColor * Specular*LC.LightMask;
	#endif//_UNREAL_MODE
	DEBUG_CUSTOMDATA_PARAM(DirectSpecular, DirectSpecular)
}

void SM_Supplement(in FLightingContext LC,
	inout FLOAT3 DirectDiffuse, inout FLOAT3 DirectSpecular DEBUG_ARGS)
{
	//diffuse1
	FLOAT3 diffuse1 = LC.DiffuseColor * LC.FixNdotL * LC.LightColor;
	DEBUG_CUSTOMDATA_PARAM(DirectDiffuseSupplement, diffuse1)
	DirectDiffuse += diffuse1;
	#ifdef _UNREAL_MODE		
		FLOAT3 spec1 = LC.FixNdotL * LC.LightColor * SpecularGGX(LC)*LC.FixNdotL;
	#else//!SpecularGGX
		FLOAT3 spec1 = (1-LC.FixNdotL) * LC.LightColor *SpecularAdd(LC)*LC.FixNdotL;
		//FLOAT3 spec1 = LC.FixNdotL * LC.LightColor * SpecularGGX(LC)*LC.FixNdotL + (pow(1-LC.NdotV,10)) * float3(0,0.5,0)*2;
	#endif//_UNREAL_MODE
	DEBUG_CUSTOMDATA_PARAM(DirectSpecularSupplement, spec1)
	DirectSpecular +=  spec1;
}

void SM_SupplementDiffuse(in FLightingContext LC,
	inout FLOAT3 DirectDiffuse DEBUG_ARGS)
{
	//diffuse1
	FLOAT3 diffuse1 = LC.DiffuseColor * LC.FixNdotL * LC.LightColor;
	DEBUG_CUSTOMDATA_PARAM(DirectDiffuseSupplement, diffuse1)
	DirectDiffuse += diffuse1;
}

void SM_SupplementSpec(in FLightingContext LC,
	inout FLOAT3 DirectSpecular DEBUG_ARGS)
{
	#ifdef _UNREAL_MODE		
		FLOAT3 spec1 = LC.FixNdotL * LC.LightColor * SpecularGGX(LC)*LC.FixNdotL;
	#else//!_UNREAL_MODE
		FLOAT3 spec1 = LC.FixNdotL * LC.LightColor * SpecularUnity(LC)*LC.FixNdotL;
	#endif//_UNREAL_MODE
	DEBUG_CUSTOMDATA_PARAM(DirectSpecularSupplement, spec1)
	DirectSpecular += spec1;
}

// FLOAT CalcSupplementMask(FLOAT3 LightDir0,FLOAT3 LightDir1 DEBUG_ARGS)
// {
// 	FLOAT supplementMask = dot(LightDir0,-LightDir1);
// 	supplementMask = (0.5 + supplementMask*0.5);//*(ndotlMask>0.8);
// 	supplementMask = supplementMask*supplementMask;
// 	DEBUG_CUSTOMDATA_PARAM(SupplementMask, supplementMask)
// 	return supplementMask;
// }

// void SM_Supplement(in FLightingContext LC,FLightingContext LC1,
// 	inout FLOAT3 DirectDiffuse, inout FLOAT3 DirectSpecular DEBUG_ARGS)
// {
// 	// FLOAT ndotlMask = Pow8(LC1.FixNdotL);
// 	FLOAT supplementMask = 1;//CalcSupplementMask(LC.LightDir,LC1.LightDir DEBUG_PARAM);
	
// 	//diffuse0

// 	FLOAT3 diffuse0 = LC.DiffuseColor*LC.FixNdotL;
// 	CalcShadowColor(LC,diffuse0 DEBUG_PARAM);
// 	diffuse0 *= LC.LightColor;// * _FalloffColor(LC);	
// 	DEBUG_CUSTOMDATA_PARAM(DirectDiffuse, diffuse0)
// 	//diffuse1
// 	FLOAT3 diffuse1 = LC1.DiffuseColor * LC1.FixNdotL * LC1.LightColor*_FalloffWeight(LC1)*supplementMask;
// 	DEBUG_CUSTOMDATA_PARAM(DirectDiffuseSupplement, diffuse1)
// 	DirectDiffuse = diffuse0 + diffuse1;

// 	FLOAT3 spec0 = FLOAT3(0,0,0);
// 	FLOAT3 spec1 = FLOAT3(0,0,0);
// 	#ifdef _UNREAL_MODE
// 		//spec	
// 		DualSpecularGGX(LC,LC1,spec0,spec1);
// 	#else//_UNREAL_MODE
// 		//spec	
// 		DualSpecularUnity(LC,LC1,spec0,spec1);
// 	#endif//_UNREAL_MODE

// 	spec0 *= LC.FixNdotL * LC.LightColor * _FalloffWeight(LC);
// 	#ifndef _SCENE_EFFECT
// 		spec0 *= saturate(dot(_CameraLightDir.xyz,LC.LightDir));
// 	#endif
// 	spec1 *= Pow8(LC1.FixNdotL) * LC1.LightColor * _FalloffWeight(LC1) * supplementMask;
// 	DirectSpecular = spec0 + spec1;
// 	DEBUG_CUSTOMDATA_PARAM(DirectSpecular, spec0)
// 	DEBUG_CUSTOMDATA_PARAM(DirectSpecularSupplement, spec1)
// }

// void SM_SupplementSpec(FLightingContext LC,FLightingContext LC1,FLOAT supplementMask,
// 	inout FLOAT3 DirectSpecular DEBUG_ARGS)
// {
// 	FLOAT3 spec0 = FLOAT3(0,0,0);
// 	FLOAT3 spec1 = FLOAT3(0,0,0);
// 	#ifdef _UNREAL_MODE
// 		//spec	
// 		DualSpecularGGX(LC,LC1,spec0,spec1);
// 	#else//_UNREAL_MODE
// 		//spec	
// 		LC1.R = max(0.3f,LC1.R);
// 		LC1.R2 = LC1.R*LC1.R;
// 		DualSpecularUnity(LC,LC1,spec0,spec1);

// 	#endif//_UNREAL_MODE

// 	spec0 *= LC.FixNdotL * LC.LightColor * _FalloffWeight(LC);
// 	#ifndef _SCENE_EFFECT
// 		spec0 *= saturate(dot(_CameraLightDir.xyz,LC.LightDir));
// 	#endif
// 	spec1 *= Pow8(LC1.FixNdotL) * LC1.LightColor * _FalloffWeight(LC1) * supplementMask;	
// 	DirectSpecular = spec0 + spec1;
// 	DEBUG_CUSTOMDATA_PARAM(DirectSpecular, spec0)
// 	DEBUG_CUSTOMDATA_PARAM(DirectSpecularSupplement, spec1)
// }

FLOAT2 isectSphere(FLOAT3 p, FLOAT3 d, FLOAT3 sPos, FLOAT sRad) {
    FLOAT2 ret;
    FLOAT3 v = p - sPos;
    FLOAT r  = sRad;
    FLOAT dv = dot(d, v);
    FLOAT d2 = dot(d, d);
    FLOAT sq = dv*dv - d2 * (dot(v, v)-r*r);
    if(sq < 0.) 
	{
        return FLOAT2(-1.,-1);
    }
    else 
	{
		sq = sqrt(sq);
        FLOAT t1 = (-dv+sq)/d2;
		FLOAT t2 = (-dv-sq)/d2;
		return (t1<t2 ? FLOAT2(t1, t2) : FLOAT2(t2, t1));
    }
}

FLOAT3 FAST_SS(FFragData FragData,
	in FLOAT3 LightDir,in FLOAT3 LightColor,
	in FLOAT4 diffuseParam)
{
	#define _LightAmbient diffuseParam.x
	#define _LightPower diffuseParam.y
	#define _Thinkness diffuseParam.z
	
	FLOAT camdotl = dot(FragData.CameraVector,-LightDir);
	FLOAT halfLDot = pow(saturate((camdotl)),_LightPower)*2;
	return (halfLDot*_Thinkness + _LightAmbient)*LightColor.xyz;
}


#define diffuseParamWind _Param5
FLOAT3 FAST_SS_Wind(FFragData FragData,
	in FLOAT3 LightDir,in FLOAT3 LightColor,
	in FLOAT4 diffuseParamWind)
{
	#define _LightAmbientW diffuseParamWind.x
	#define _LightPowerW diffuseParamWind.y
	#define _ThinknessW diffuseParamWind.z
	
	FLOAT camdotl = dot(FragData.CameraVector,-LightDir);
	FLOAT halfLDot = pow(abs((camdotl)),_LightPowerW)*2;
	return (halfLDot*_ThinknessW + _LightAmbientW)*LightColor.xyz;
}



// #define _RampTex _ProcedureTex0
// void SM_TreeLeaf(FFragData FragData,FMaterialData MaterialData,
// 	in FLOAT3 LightDir,in FLOAT3 LightColor,in FLOAT Shadow,
// 	inout FLOAT3 DirectDiffuse,inout FLOAT3 DirectSpecular DEBUG_ARGS)
// {
// 	#define _NoLSteps _Param1.x
// 	#define _NoLFeather _Param1.y
// 	#define _LightLeafThreshOffset _Param1.z
// 	#define _LightLeafThreshFallOff _Param1.w
// 	#define _LightLeafColor _Color1.rgb

// 	FLOAT3 ss = FAST_SS(FragData,LightDir,LightColor,diffuseParam);

// 	FLOAT3 worldNormal  = MaterialData.WorldNormal;	
// 	FLOAT2 uv = GET_FRAG_UV;
// 	FLOAT3 color = MaterialData.DiffuseColor;

// 	FLOAT3 viewNormal = normalize(mul((FLOAT3x3)unity_MatrixV, worldNormal));
// 	UNITY_BRANCH
// 	if(_NoLFeather > 0.5)
// 		viewNormal.z = abs(viewNormal.z);
// 	FLOAT3 viewLightDir  = normalize(mul((FLOAT3x3)unity_MatrixV, LightDir));
// 	FLOAT NoL = (dot(viewNormal, viewLightDir)+1)*0.5;
// 	NoL = saturate(NoL + _NoLSteps);

// 	FLOAT NoV = saturate(1-max(0,MaterialData.NdotV)+0.5);
// 	FLOAT3 tint = lerp(color,_LightLeafColor, pow(saturate(NoL*Shadow*NoV+_LightLeafThreshOffset),_LightLeafThreshFallOff));
// 	NoL = SAMPLE_TEX2D(_RampTex, FLOAT2(NoL,0)).r;	

// 	DirectDiffuse = NoL*tint*Shadow*LightColor.xyz + color*ss*Shadow*NoV;
// 	DEBUG_CUSTOMDATA_PARAM(DirectDiffuse, DirectDiffuse)
	
// 	DirectSpecular = 0;

// 	DEBUG_CUSTOMDATA_PARAM(DirectSpecular, DirectSpecular)
// }

void SM_SS(FFragData FragData,FMaterialData MaterialData,
	in FLOAT3 LightDir,in FLOAT3 LightColor,in FShadowData ShadowData,
	inout FLOAT3 DirectDiffuse,inout FLOAT3 DirectSpecular, FLOAT4 diffuseParam, FLOAT4 darkColor,
	FLOAT _InvOcc,
	FLOAT _SpecularShininess,
	FLOAT _SpecularAmp,
	FLOAT _SpecularStart,
	FLOAT _SpecularRange,
	FLOAT _NormalBlend,
	FLOAT _SecondNormalUpright
	DEBUG_ARGS)
{
	FLOAT3 ss = FAST_SS(FragData,LightDir,LightColor,diffuseParam);
	FLOAT FaceLight = dot(LightDir,FragData.CameraVector);
	
	//uv mask
	FLOAT2 uv = GET_FRAG_UV;
	FLOAT colorMask = saturate((uv.y+darkColor.a));
	 _InvOcc = lerp(_InvOcc,_InvOcc*0.1,FaceLight);
	colorMask = pow(colorMask,_InvOcc); 
	FLOAT3 color = MaterialData.DiffuseColor;
	color = lerp(color*darkColor.xyz,MaterialData.DiffuseColor,colorMask)*ss;
	DirectDiffuse = lerp(_ShadowColor.xyz*GetFadeShadow(ShadowData)*color,color,GetSrcShadow(ShadowData));
	DEBUG_CUSTOMDATA_PARAM(DirectDiffuse, DirectDiffuse)

	FLOAT3 worldNormal = MaterialData.WorldNormal;
	// FLOAT facing = dot(FragData.CameraVector, MaterialData.WorldNormal);
	// UNITY_BRANCH
	// if (FaceLight < 0)
	// 	worldNormal *= -1;

	_SpecularStart =lerp(_SpecularStart,2,FaceLight);
	FLOAT specularFade = LinearStep(_SpecularStart, 2, uv.y);
	FLOAT3 H = normalize(FragData.CameraVector + LightDir);
	FLOAT3 secondNormal = normalize(FLOAT3(FragData.CameraVector.x, _SecondNormalUpright, FragData.CameraVector.z)); 
	FLOAT3 specuarlNormal = normalize(lerp( secondNormal, worldNormal, _NormalBlend));
	FLOAT NdotH = max(0, dot(specuarlNormal, H));
	FLOAT shininess =  pow(NdotH, _SpecularShininess) * saturate(1- length(FragData.WorldPosition_CamRelative)*_SpecularRange);
	FLOAT specular = (shininess * _SpecularAmp * specularFade);
	DirectSpecular = specular * LightColor.xyz * MaterialData.DiffuseColor*GetFadeShadow(ShadowData);
	DEBUG_CUSTOMDATA_PARAM(DirectSpecular, DirectSpecular)
}


void SM_Water(FFragData FragData,FMaterialData MaterialData,
	in FLOAT3 LightDir,in FLOAT3 LightColor,in FShadowData ShadowData,
	inout FLOAT3 DirectDiffuse,inout FLOAT3 DirectSpecular DEBUG_ARGS)
{	
	FLOAT3 color =MaterialData.DiffuseColor;	
	DirectDiffuse = lerp(GetFadeShadow(ShadowData)*color*2,color,GetSrcShadow(ShadowData));
			//URP|||||||||||||||||||||||||||Shadow
    float4 shadowCoord = TransformWorldToShadowCoord(FragData.WorldPosition.xyz);
	float shadow = MainLightRealtimeShadow(shadowCoord);
	DirectDiffuse *=saturate(shadow+0.4);
	DirectSpecular = MaterialData.SpecularColor;
	DEBUG_CUSTOMDATA_PARAM(DirectSpecular, DirectSpecular)
}

	/*#define _SSSLocation _Param3.x
	#define _SSSSmoothness _Param3.y
	#define _SSSSale _Param3.z
	#define _SSSLerp _Param3.w
	#define _SSSColor _Color2

void SM_SSS(FMaterialData MaterialData,
	FLOAT3 LightDir0,FLOAT3 LightColor0,FLOAT3 LightDir1,FLOAT3 LightColor1,
	inout FLOAT3 DirectDiffuse,inout FLOAT3 DirectSpecular DEBUG_ARGS)
{
	//sss					
	FLOAT vertexNdotL = dot(LightDir0.xyz,MaterialData.WorldNormal);
	FLOAT fixNdotL = vertexNdotL*0.5+0.5;
	DEBUG_CUSTOMDATA_PARAM(FixNdotL, fixNdotL)
	DEBUG_CUSTOMDATA_PARAM(NdotL, vertexNdotL)

	FLOAT3 SSS =  SAMPLE_TEX2D(_PreIntegrateSSS, FLOAT2(fixNdotL, _SSSLocation)).xyz*
		_SSSSmoothness*_SSSColor.rgb;
	SSS = saturate(SSS)*_SSSSale*(1-fixNdotL);
	DEBUG_CUSTOMDATA_PARAM(SSS, SSS)

	FLOAT3 mainLightDiffuse = MaterialData.DiffuseColor.rgb*LightColor0.xyz*fixNdotL;

	FLOAT cameraLightNdotL = saturate(dot(LightDir1,MaterialData.WorldNormal));
	FLOAT3 cameraLightDiffuse = MaterialData.DiffuseColor.rgb*LightColor1.xyz*(1-fixNdotL)*cameraLightNdotL*0.5;
	
	FLOAT3 directColor = mainLightDiffuse + lerp(mainLightDiffuse,SSS,_SSSLerp);
	DirectDiffuse = directColor*0.68 + cameraLightDiffuse;

	DEBUG_CUSTOMDATA_PARAM(DirectDiffuse, DirectDiffuse)
}*/
#if !defined(REDEFINE_URP)
//========================================Kajiya========================================
FLOAT3 ShiftTangent(FLOAT3 tangent, FLOAT3 normal, FLOAT shift)
{
	FLOAT3 shiftTangent = tangent + (shift * normal);
	return normalize(shiftTangent);
}
#endif

FLOAT StrandBlinn( FLOAT Roughness, FLOAT NoH)
{
	FLOAT a = Roughness * Roughness;
	FLOAT a2 = max(1e-4f, a * a);
	FLOAT n = max(2 / a2 - 2, 1e-4f);
	FLOAT specTerm = pow(saturate(NoH), n) * (0.5 * n + 1.0);
	return specTerm;
}

FLOAT StrandSpecular(FLOAT3 T, FLOAT3 H, FLOAT exponent,out FLOAT sinTH)
{
	FLOAT dotTH = dot( T, H );
	sinTH = sqrt( max(1e-4h, 1 - dotTH * dotTH) );
	FLOAT dirAtten = saturate(dotTH + 1);
	FLOAT roughness = max(0.01, 1.0 - exponent);
	return dirAtten * StrandBlinn( roughness, sinTH );
}


FLOAT StrandSpecular2(FLOAT3 T, FLOAT3 H, FLOAT exponent)
{
	FLOAT dotTH = clamp(dot(T, H), -1, 1);
	FLOAT sinTH = sqrt( 1 - dotTH * dotTH );
	FLOAT dirAtten = LinearStep(-1.0, 0.0, dotTH);
	
    FLOAT spec = pow( sinTH,  exp2(lerp(4, 12, exponent)));
	
	//FLOAT spec2 = StrandBlinn( 1, sinTH );
	//spec = smoothstep((spec2 - _SpecularCutSmoothness0), (spec2 + _SpecularCutSmoothness0), spec2);
	spec = LinearStep(0.1,0.6,spec);
	return dirAtten * spec;
}

FLOAT StrandSpecular2(FLOAT3 T, FLOAT3 H, FLOAT exponent,FLOAT backLightMask)
{
	return StrandSpecular2(T, H, exponent) * backLightMask;
}

FLOAT3 VisF_Zioma( FLOAT Roughness, FLOAT VoH, FLOAT3 SpecularColor )
{
	FLOAT k = VoH * VoH * (Roughness + 0.5);
	return 0.25 * SpecularColor / max(1e-4h, k);
}

#if !defined(UNIFORM_PCH_OFF)
FLOAT4 _KajiyaParam0;
FLOAT4 _KajiyaParam1;
#endif

#define _PrimarySpecualrColor _Color1
#define _SecondSpecualrColor _Color2

#define _ShiftIntensity _KajiyaParam1.x
#define _ShiftOffset _KajiyaParam1.y

#define _ShiftScaleX _KajiyaParam0.x
#define _ShiftScaleY _KajiyaParam0.y

#define _SpecualrIntensity1 _KajiyaParam1.z
#define _SpecualrIntensity2 _KajiyaParam1.w

#define _PrimaryShift _KajiyaParam0.z
#define _SecondShift _KajiyaParam0.w

#define _PrimaryExponent _Color1.a
#define _SecondExponent _Color2.a

inline void SM_KajiyaDiffuse(FMaterialData MaterialData,
	FLOAT3 LightDir,FLOAT3 LightColor,
	inout FLOAT3 DirectDiffuse)
{
	FLOAT ndotl = dot(MaterialData.WorldNormal, LightDir.xyz);
	FLOAT fixNdotL = ndotl*0.5+0.5;	
	DirectDiffuse = MaterialData.DiffuseColor.rgb*LightColor.xyz*fixNdotL;
}

#if defined(KAJIYA_ON) || !defined(UNIFORM_PCH_OFF)
inline void SM_KajiyaCommon2(FFragData FragData,FMaterialData MaterialData,
	FLOAT3 LightDir,FLOAT3 LightColor,FLOAT3 SpecLightDir,FLOAT3 T1,FLOAT3 T2,
	inout FLOAT3 DirectSpecular,inout FLOAT NoL)
{
	FLOAT3 lightDir = SpecLightDir.xyz;
	FLOAT3 viewDir = FragData.CameraVector;
#ifdef _CUSTOM_VERTEX_PARAM
	lightDir = normalize(FragData.CustomData1.xyz);
	viewDir = normalize(FragData.CustomData.xyz);  
	FLOAT3 H = normalize(viewDir + lightDir);
#else
	FLOAT3 H = normalize(FragData.CameraVector + SpecLightDir);
#endif
	NoL = dot(MaterialData.WorldNormal, SpecLightDir.xyz)*0.5 + 0.5;
	NoL = LinearStep(0.1,0.4,NoL);

	FLOAT3 S1 = StrandSpecular2(T1, H,_PrimaryExponent,NoL) * _PrimarySpecualrColor.rgb * _SpecualrIntensity1;
	FLOAT3 S2 = StrandSpecular2(T2, H,_SecondExponent,NoL) * _SecondSpecualrColor.rgb * _SpecualrIntensity2;
	FLOAT3 S = S1 + S2;
	DirectSpecular = S*LightColor.xyz;
}
#endif

#if defined(KAJIYA_ON) || !defined(UNIFORM_PCH_OFF)
inline void SM_Kajiya(FFragData FragData,FMaterialData MaterialData,FLOAT Shadow,
	FLOAT3 LightDir,FLOAT3 LightColor,FLOAT3 SpecLightDir,
	inout FLOAT3 DirectSpecular DEBUG_ARGS)
{
	#ifdef _SHIFT_TEX
		FLOAT shift = (SAMPLE_TEX2D(_AnisoShift, GET_FRAG_UV*FLOAT2(_ShiftScaleX, _ShiftScaleY)).r * 2 - 1)  * _ShiftIntensity + _ShiftOffset;
	#else//!_SHIFT_TEX
		FLOAT shift = 1;//_AnisoShiftValue *_AnisoIntensity;
	#endif//_SHIFT_TEX
	DEBUG_CUSTOMDATA_PARAM(KajiyaShift, shift)
    
    FLOAT3 tsNormal = FLOAT3(0, 0, 1);
	#ifndef _PBS_FROM_PARAM
		tsNormal = normalize(MaterialData.TangentSpaceNormal);
		FLOAT3 T = normalize(cross(FLOAT3(1, 0, 0), tsNormal));
	#else
		FLOAT3 T = normalize(cross(FLOAT3(1, 0, 0), tsNormal));
	#endif
	FLOAT3 T1 = ShiftTangent(T, tsNormal, _PrimaryShift + shift);
	FLOAT3 T2 = ShiftTangent(T, tsNormal, _SecondShift + shift);
	
	FLOAT3 spec = FLOAT3(0,0,0);
	FLOAT NoL = 1;
	SM_KajiyaCommon2(FragData,MaterialData,LightDir,LightColor,SpecLightDir,T1,T2,spec,NoL);
	FLOAT3 isShadow = step(Shadow,0.2);
	DirectSpecular = spec*_SpecMult*Shadow;
	DirectSpecular = lerp(DirectSpecular, spec*_SpecMult*0.5,isShadow);
	DEBUG_CUSTOMDATA_PARAM(DirectSpecular, DirectSpecular)
}
#endif

#ifdef _STYLE_HAIR
#define _Ramp _ProcedureTex2
inline void SM_StyleHair(FFragData FragData,FMaterialData MaterialData,FLOAT Shadow,
	FLOAT3 LightDir,FLOAT3 LightColor,FLOAT3 SpecLightDir,
	inout FLOAT3 DirectSpecular DEBUG_ARGS)
{
    FLOAT HNdL = dot(MaterialData.WorldNormal, LightDir)*0.5+0.5;
	FLOAT3 H = SafeNormalize(FragData.CameraVector + LightDir);
	FLOAT _ShadowStep = saturate(1.0 - (HNdL - (_StyleHairThreshold - _StyleHairFeather)) / _StyleHairFeather);
	FLOAT4 LightMapTex = SAMPLE_TEX2D(_StyleHairLightMap,GET_FRAG_UV);
	_ShadowStep = max(_ShadowStep, LightMapTex.b);
   
	/* Ver: Origin
	* Issue: This version has a wrong effect when Highlight in Shadow Side;
	*        NdotH be transformed with unity_MatrixV has a strange effect.
	*/
	//FLOAT3 NV = mul(unity_MatrixV, FLOAT4(MaterialData.WorldNormal,0)).xyz;
	//FLOAT3 HV = mul(unity_MatrixV, FLOAT4(H,0)).xyz;
		
	//FLOAT NdotH = dot(normalize(NV.xz), normalize(HV.xz));
	// NdotH = pow(abs(NdotH), 6) * _StyleHairLightWith;
	// NdotH = pow(abs(NdotH), 1 / _StyleHairLightLength);
	//FLOAT lightFeather = _StyleHairFeather * NdotH;
	//FLOAT lightStepMax = saturate(1 - NdotH + lightFeather);
	//FLOAT lightStepMin = saturate(1 - NdotH - lightFeather);
	// End Origin

	// /* Ver: TKS_21/8/27_15:40
	// * LastVer: Origin
	// * Change By: Takeshi
	// * Modify: Using NdotV for Highlight Mask in Shadow Area;
	// *         In this way, Highlight will follows ViewDir.
	// */
	// FLOAT NdotV = MaterialData.NdotV;
	// NdotV = pow(abs(NdotV), 5) * _StyleHairLightWith;
	// NdotV = pow(abs(NdotV), 1 / _StyleHairLightLength);
	// FLOAT lightFeather = _StyleHairFeather * NdotV;
	// FLOAT lightStepMax = saturate(1 - NdotV + lightFeather);
	// FLOAT lightStepMin = saturate(1 - NdotV - lightFeather);
	// FLOAT NdotH = dot(normalize(MaterialData.WorldNormal), normalize(H));
	// End Change

	/* Ver:      TKS_21/9/02_23:20
	* LastVer:   TKS_21/8/27_15:40
	* Change By: Takeshi
	* Modify:    Using Mixing NdotV and NdotH for Highlight Mask in Shadow Area;
	*            In this way, Highlight will be not appeared in the junction of light and shadow.
	* Notice:    This solution should change modeling and texture painting at the same time,
	*			 so leaving it to next game version.
	*/
	FLOAT3 V = MaterialData.WorldNormal;
	// // FLOAT NdotV = MaterialData.NdotV;
	FLOAT NdotH = pow(abs(dot(normalize(V), normalize(H))),2);
	// // FLOAT lerp_valve = saturate(smoothstep(0.6,0.8,HNdL));
	// // Lerp_HV = lerp(NdotV*0.6,NdotH,lerp_valve);
	// NdotH = pow(abs(NdotH), 3) * _StyleHairLightWith;
	NdotH = NdotH*NdotH*NdotH * _StyleHairLightWith;
	NdotH = pow(abs(NdotH), 1 / _StyleHairLightLength);
	//FLOAT lightFeather = _StyleHairFeather * Lerp_HV;
	// // FLOAT lightStepMax = saturate(1 - Lerp_HV + lightFeather);
	// // FLOAT lightStepMin = saturate(1 - Lerp_HV - lightFeather);
	// End Change

	/* Change By: Takeshi*/
	// FLOAT3 highlightRGB_H = smoothstep(lightStepMin, lightStepMax, clamp(LightMapTex.r,0, 0.99)) * _StyleHairSpecualrColor.rgb;
	// FLOAT3 highlightRGB_L = smoothstep(_StyleHairThreshold, 1, LightMapTex.r) * _StyleHairSpecualrColor.rgb*0.25;
	// FLOAT3 HighLightMapRGB = (highlightRGB_H + highlightRGB_L)*_StyleHairSpeculrInt;
	// DirectSpecular = HighLightMapRGB*Shadow;
	// FLOAT3 isShadow = step(Shadow,0.2);	
	// DirectSpecular = lerp(DirectSpecular,HighLightMapRGB*0.5,isShadow);
	
    FLOAT lightFeatherRange = LinearStep(0.1,0.15,NdotH);
	FLOAT3 highlightColor = LinearStep(_StyleHairFeather,_StyleHairFeather+0.2,LightMapTex.r) *lightFeatherRange * _StyleHairSpecualrColor.rgb;
	FLOAT3 highlightRGB_L = LinearStep(_StyleHairThreshold, 1, LightMapTex.r) * _StyleHairSpecualrColor.rgb*0.25;
	FLOAT3 HighLightMapRGB = (highlightColor + highlightRGB_L)*_StyleHairSpeculrInt;
	DirectSpecular = HighLightMapRGB*Shadow;

	FLOAT3 isShadow = step(Shadow,0.2);	
	DirectSpecular = lerp(DirectSpecular,HighLightMapRGB*0.5,isShadow);
	/* End Change */
	
	DEBUG_CUSTOMDATA_PARAM(DirectSpecular, DirectSpecular)
}
#endif

#if defined(URP_ROLE_HAIR) || !defined(UNIFORM_PCH_OFF) 
inline FLOAT3 CharacterHairMatcap(FLOAT3 matcapNormal, FLOAT3 viewWS,FLOAT diffuseIntensity)
{
	#define _MatcapOffset _Param3.x
	#define _MatcapTex _ProcedureTex3
	#define _MatcapDarkIntensity _Param3.y
	#define _MatcapTint _Color5

	FLOAT3 normalViewSpace = mul(unity_MatrixV, FLOAT4(matcapNormal,0)).xyz;
	FLOAT3 normalBlend_matcapUV_Detail = normalViewSpace * FLOAT3(-1,-1,1);
	FLOAT3 normalBlend_matcapUV_Base = mul(unity_MatrixV, FLOAT4(viewWS,0)).xyz * FLOAT3(-1, -1, 1) ;
	FLOAT3 noSknewViewNormal = normalBlend_matcapUV_Base * dot(normalBlend_matcapUV_Base, normalBlend_matcapUV_Detail ) / normalBlend_matcapUV_Base.z - normalBlend_matcapUV_Detail;
	FLOAT2 matcapUV = noSknewViewNormal.xy * 0.5 + 0.5;
	FLOAT matcapUVOffset = viewWS.y * _MatcapOffset;
	matcapUVOffset *= sqrt(1 - normalViewSpace.x * normalViewSpace.x);
	matcapUV += FLOAT2(0, matcapUVOffset);
    FLOAT4 sampledMatcap = SAMPLE_TEX2D(_MatcapTex, matcapUV);
    FLOAT3 matcapColor = sampledMatcap.rgb * _MatcapDarkIntensity;
	matcapColor *= _MatcapTint.rgb * _MatcapTint.a;
    return matcapColor;
}
#endif

#if defined(KAJIYA_ON) || !defined(UNIFORM_PCH_OFF)
inline void SM_MatcapHair(FFragData FragData,FMaterialData MaterialData,FLOAT Shadow,
	FLOAT3 LightDir,FLOAT3 LightColor,FLOAT3 SpecLightDir,
	inout FLOAT3 DirectSpecular DEBUG_ARGS)
{
	#define _MatcapTexNormal _ProcedureTex4
	#define _MatcapNormalScale _Param3.z
	#define _MatcapNormalTiling _Param3.w
	
	FLOAT3x3 TangentToWorld = FragData.TangentToWorld;
	FLOAT3 NormalWS = normalize(FLOAT3(MaterialData.WorldNormal.xy,MaterialData.WorldNormal.z));
	FLOAT4 sampledMatcapNormalMap = SAMPLE_TEX2D(_MatcapTexNormal,FLOAT2(GET_FRAG_UV.x*_MatcapNormalTiling,GET_FRAG_UV.y));
	FLOAT3 MatcapNormal = sampledMatcapNormalMap.xyz*2.0-1.0;// *_MatcapNormalScale ,1);
	MatcapNormal.xy *=_MatcapNormalScale;		
    FLOAT3 matcapNormalWS = normalize(mul(MatcapNormal , TangentToWorld).xyz);
	FLOAT NL = dot(MaterialData.WorldNormal, LightDir);
    FLOAT3 viewWS = FragData.CameraVector;
	FLOAT3 matcapColor = CharacterHairMatcap(matcapNormalWS, viewWS,NL);
	DirectSpecular = matcapColor;
	DEBUG_CUSTOMDATA_PARAM(DirectSpecular, DirectSpecular)
}
#endif

#ifdef _SM_DARK_RIM
inline FLOAT3 SM_DarkRim(in FLOAT NdotV, in FLOAT rawNdotL, 
	in FLOAT rimCutLocation, in FLOAT rimCutSmoothness
	DEBUG_ARGS)
{
	FLOAT darkRimCut = saturate(NdotV + rimCutLocation - 0.5);
	FLOAT darkRimMin = saturate(darkRimCut - rimCutSmoothness);
	FLOAT darkRimMax = saturate(darkRimCut + rimCutSmoothness);
	FLOAT darkRimDegree = saturate(0.5 - darkRimCut - rawNdotL);
	FLOAT darkRim = saturate(linearstep(darkRimMin, darkRimMax, darkRimDegree)) * _DarkRimLightColor.w;
	DEBUG_CUSTOMDATA_PARAM(DarkRim, darkRim)
	return lerp(1, _DarkRimLightColor.xyz, darkRim);
}
#endif

#if defined(_SM_RIM) 
inline FLOAT3 SM_Rim(FFragData FragData, FMaterialData MaterialData,
	FLOAT intensity, FLOAT mask, FLOAT3 color,
	FLOAT NdotV, FLOAT rawNdotL,FLOAT ShadowMask
	DEBUG_ARGS)
{
	FLOAT rimCut = saturate(NdotV + _RimCutLocation - 0.5);
	FLOAT rimMin = saturate(rimCut - _RimCutSmoothness);
	FLOAT rimMax = saturate(rimCut + _RimCutSmoothness);
	FLOAT rimDegree = saturate(1 - rimCut - (1 - rawNdotL) * _RimOrientLight);
	// 这里本来是smoothstep，为了优化性能改成了linearstep。
	FLOAT rim = linearstep(rimMin, rimMax, rimDegree) * mask;

	FLOAT3 rimColor = _RoleRimColor * _MainLightColor.xyz * lerp(ShadowMask, 1, _ShadowRimIntensity) * intensity * rim;

	// 这里【除以亮度】比【算单位长度颜色】快一些，但效果有细微差别。
	rimColor *= lerp(MaterialData.BaseColor.xyz / Luminance(MaterialData.BaseColor.xyz + 1e-4), MaterialData.BaseColor.xyz, _RoleRimMultiply);
	// rimColor *= lerp(normalize(MaterialData.BaseColor.xyz + 0.01), MaterialData.BaseColor.xyz, _RoleRimMultiply);

	DEBUG_CUSTOMDATA_PARAM(Rim, rimColor)

	return rimColor;
}
#endif



#define _AnisotropyScale _AnisotropyParam.x
#define _WarpIntensity _AnisotropyParam.y
#define _WeftIntensity _AnisotropyParam.z
#define _WeftBendFactor _AnisotropyParam.w
#define _WarpColor  _FabricColor1.rgb
#define _WeftColor  _FabricColor2.rgb
#define _WarpRoughness _FabricColor1.a
#define _WeftRoughness _FabricColor2.a

inline void SM_Anisotropy(in FLightingContext LC,
	in FFragData FragData,in FMaterialData MaterialData,
	inout FLOAT3 DirectDiffuse,inout FLOAT3 DirectSpecular DEBUG_ARGS)
{
#ifdef _ANISOTROPY
    FLOAT3 tangent = MaterialData.WorldTangent;
    FLOAT3 binormal = MaterialData.WorldBinormal;
    FLOAT th = dot(tangent, LC.H);
	FLOAT bh = dot(binormal, LC.H);
	FLOAT tv = dot(tangent, FragData.CameraVector);
	FLOAT bv = dot(binormal, FragData.CameraVector);
	FLOAT tl = dot(tangent, LC.LightDir);
	FLOAT bl = dot(binormal, LC.LightDir);

	FLOAT anisoAspect = sqrt(1.0 - 0.9 * _AnisotropyScale);
	FLOAT roughnessT = MaterialData.Roughness / anisoAspect;
	FLOAT roughnessB = MaterialData.Roughness * anisoAspect;
	FLOAT D = D_GGXaniso2(roughnessT, roughnessB, LC.NdotH, th, bh);
	FLOAT V = V_SmithJointGGXAniso(tv, bv, MaterialData.NdotV, tl, bl, LC.FixNdotL, roughnessT, roughnessB);
	FLOAT3 F = F_Schlick(LC.SpecularColor, LC.LdotH );
	DirectSpecular = D * V * F * PI;
	DirectSpecular *= LC.FixNdotL * LC.LightColor * _FalloffWeight(LC)*_SpecMult;
	
	DEBUG_CUSTOMDATA_PARAM(DirectSpecular, DirectSpecular)
#endif
}

#endif //PBS_SHADINGMODE_INCLUDE