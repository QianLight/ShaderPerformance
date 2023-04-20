// Copyright 2018- PWRD, Inc. All Rights Reserved.

/**
* MaterialTemplate.hlsl: Filled in by user defined function for each material being compiled.
*/

#include "../Noise.hlsl"
#ifndef PBS_MATERIALTEMPLATEHEAD_INCLUDE
#define PBS_MATERIALTEMPLATEHEAD_INCLUDE

#ifdef _DITHER_TRANSPARENT
static const FLOAT4x4 thresholdMatrix = {  1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0, 13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
		4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0, 16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0};
static const FLOAT4x4 RowAccess = { 1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1 };
#ifdef _SPHERE_DITHER_FADE
#define _Transparency _Color.x
#else
#define _Transparency _Color.w
#endif

#endif//_DITHER_TRANSPARENT
void MaterialAlphaTest(in FFragData FragData,FMaterialData MaterialData)
{
#ifdef _DITHER_TRANSPARENT
	FLOAT2 uv = fmod(FragData.SvPosition.xy, 4);
	FLOAT a = _Transparency* MaterialData.BaseColor.a;
	FLOAT clipValue = thresholdMatrix[uv.x] * RowAccess[uv.y];
//#ifdef _SPHERE_DITHER_FADE
//	FLOAT4 playerPos = mul(_matrixVP, FLOAT4(_PlayerPos.xyz, 1.0));
//	playerPos /= playerPos.w;
//	FLOAT lengthT = saturate(length(FragData.WorldPosition.xyz - _ObjectPos.xyz)/2;
//#else
//	
//#endif
	clip(a - clipValue);
#else//!_DITHER_TRANSPARENT
	#ifdef _SCENE_EFFECT
		clip(MaterialData.BaseColor.a - 0.5f);
	#else//!_SCENE_EFFECT
		clip(MaterialData.BaseColor.a - _Cutout);
	#endif//_SCENE_EFFECT
#endif//_DITHER_TRANSPARENT
}

inline void MaterialColorBlend(in FFragData FragData,inout FMaterialData MaterialData)
{
#ifdef _COLORBLEND
	MaterialData.DyeColor = _Color1.rgb*MaterialData.BaseColor.r*_Color1.a*FLOAT(10) +		
		_Color2.rgb*MaterialData.BaseColor.g*_Color2.a*FLOAT(10) +
		_Color3.rgb*MaterialData.BaseColor.b*_Color3.a*FLOAT(10);
		FLOAT BaseGray = MaterialData.BaseColor.r + MaterialData.BaseColor.g + MaterialData.BaseColor.b;
		FLOAT3 colors[3] = 
		{
			MaterialData.DyeColor,
			_Color4.rgb*_Color4.a*10*BaseGray,
			_Color5.rgb*_Color5.a*10*BaseGray
		};
	int id = (int)(FragData.TexCoords[0].y);
	MaterialData.DyeColor = saturate(colors[id]);
#else
	MaterialData.DyeColor = MaterialData.BaseColor.rgb;
#endif
}

inline void CalcDefaultBaseColor(in FFragData FragData,inout FMaterialData MaterialData)
{
#ifdef _BASE_FROM_COLOR
	MaterialData.BaseColor = _MainColor;
#else
	#ifdef _UV_MODIFY
		REAL2 uv = ModifyUV(GET_FRAG_UV);
	#else
		REAL2 uv = GET_FRAG_UV;
	#endif

		FLOAT4 color =  SAMPLE_TEX2D(_MainTex, uv);

	#ifdef _MAIN_COLOR
		MaterialData.BaseColor = color*_MainColor;
	#else//_MAIN_COLOR
		MaterialData.BaseColor = color;
	#endif//_MAIN_COLOR
#endif
	MaterialColorBlend(FragData,MaterialData);

#ifdef _CUSTOM_ALPHA
	MaterialData.BaseColor.a = CustomAlpha(FragData);
#endif
}

inline FLOAT3 CalcDefaultPBS(in FFragData FragData,inout FMaterialData MaterialData,out FLOAT2 rm)
{
	REAL4 pbs = REAL4(0.5,0.5,1,0);
#ifndef _DEFAULT_PBS_PARAM
	#ifndef _NO_NORMAL_MAP
			pbs = SAMPLE_TEX2D(_PBR_Tex, GET_FRAG_UV);
	#endif

	#if defined(_PBS_ADJUST)
		FLOAT r = lerp(_ParamRoughnessMin,_ParamRoughnessMax,pbs.z);
		FLOAT m = lerp(_ParamMetallicMin,_ParamMetallicMax,pbs.w);		
		pbs.z = r;//lerp(pbs.z,r,_RMAdjust);
		pbs.w = m;//lerp(pbs.w,m,_RMAdjust);
	#endif//_PBS_FROM_PARAM
	MaterialData.SrcAO = lerp(pbs.w, 1, _MetallicScale);
	#ifdef _NO_METALLIC
		pbs.w = 0;
	#else		
		pbs.w *= _MetallicScale;
	#endif
#endif//_DEFAULT_PBS_PARAM
	MaterialData.ScaleParam = REAL4(_SpecularScale,_IBLScale,_SpecK,_SpecB);
	rm = pbs.zw;
	return FLOAT3(pbs.xy,0);
}
//---------------------------------------------------------------------------------
inline FLOAT ModifyNormal(in FFragData FragData,inout FMaterialData MaterialData,
	FLOAT3 normal)
{
	FLOAT3 normalTS = normalize(normal);
	MaterialData.TangentSpaceNormal = normalTS;
	MaterialData.WorldNormal = normalize(mul(normalTS, FragData.TangentToWorld));
	return 1;
}

#if !REAL_IS_HALF //half Lighting中有定义
REAL OneMinusReflectivityMetallic(REAL metallic) 
{
	REAL oneMinusDielectricSpec = kDieletricSpec.a;
	return oneMinusDielectricSpec - metallic * oneMinusDielectricSpec;
}
#endif

#if !defined(REDEFINE_URP)
inline REAL PerceptualSmoothnessToPerceptualRoughness(REAL perceptualSmoothness)
{
    return (1.0 - perceptualSmoothness);
}

inline REAL PerceptualRoughnessToRoughness(REAL perceptualRoughness)
{
    return perceptualRoughness;
}
#endif
inline void CalcPBS(FFragData FragData, inout FMaterialData MaterialData)
{
	FLOAT2 rm = FLOAT2(1,0);
	MaterialData.SrcAO = 1;
#ifdef _CUSTOM_PBS
	FLOAT3 normal = CalcCustomPBS(FragData, MaterialData,rm);
#else//!_CUSTOM_PBS
	FLOAT3 normal = CalcDefaultPBS(FragData, MaterialData,rm);
#endif//_CUSTOM_PBS	
	//MaterialData.SrcPbs = pbs;
#define _Roughness rm.x			
#define _Metallic rm.y

#ifdef _CUSTOM_NORMAL
	CalcCustomNormal(FragData, MaterialData,normal);
#elif defined(_SIMPLE_NORMAL)
	ModifyNormal(FragData, MaterialData,normal);
#else//!_CUSTOM_NORMAL

	CalcWorldNormal(FragData.TangentToWorld,normal.xy,_NormalScale,
		MaterialData.WorldNormal,MaterialData.TangentSpaceNormal);

#ifdef _DIRTY_NORMAL
	FLOAT4 NormalDirty = SAMPLE_TEX2D(_DirtyNormalTexs, GET_FRAG_UV); 
	NormalDirty.xyz =NormalDirty.xyz*2.0-1.0;
	NormalDirty.xy *=_DirtyNormalInt;
	NormalDirty.xyz =FLOAT3(NormalDirty.xy,1);
    REAL3 DirtyNormalWS = normalize(mul(NormalDirty.xyz, FragData.TangentToWorld).xyz);	
    MaterialData.WorldNormal =lerp(MaterialData.WorldNormal , DirtyNormalWS , _DirtyNormalScale);	
#endif

#ifdef _DETAIL_NORMAL
	#define _DetailNormal _ProcedureTex1
	#define _DetailUV _Param6.xy
	#define _DetaillInt _Param6.z
    
	FLOAT4 DetailNormal = SAMPLE_TEX2D(_DetailNormal, GET_FRAG_UV*_DetailUV); 
	FLOAT Mask = 1-SAMPLE_TEX2D(_ProcedureTex0, GET_FRAG_UV).a; 
	DetailNormal.xyz =DetailNormal.xyz*2.0-1.0;
	DetailNormal.xy *=_DetaillInt;
	DetailNormal.xyz =FLOAT3(DetailNormal.xy,1);
    REAL3 DetailNormalWS = normalize(mul(DetailNormal.xyz, FragData.TangentToWorld).xyz);
	MaterialData.WorldNormal.xyz = normalize ( MaterialData.WorldNormal.xyz+ DetailNormalWS.xyz*Mask ) ;
	 
#endif

#endif//_CUSTOM_NORMAL

#ifdef _ANISOTROPY
	#ifdef _NO_TANGENTMAP
		REAL3 tsTangent = REAL3(1,0,0);
	#else
		REAL3 tsTangent = SAMPLE_TEX2D(_ProcedureTex3, GET_FRAG_UV * _UVST3.xy + _UVST3.zw).xyz * 2.0 - 1.0;
	#endif	
	REAL3 worldTangent = normalize(mul(tsTangent, FragData.TangentToWorld));
	REAL3 worldBinormal = cross(MaterialData.WorldNormal, worldTangent);
	MaterialData.WorldTangent = cross(worldBinormal, MaterialData.WorldNormal);
	MaterialData.WorldBinormal = worldBinormal;
#endif
	//FLOAT r  = TextureNormalFiltering(_Roughness,normalLength,0.1);
	//FLOAT r = saturate(_Roughness + min(0.4,saturate(1.0 - normalLength) * 0.5));

	#ifdef _NO_METALLIC
		MaterialData.Metallic = 0;
	#else
		MaterialData.Metallic = _Metallic;//max(_Metallic, FLOAT(0.02));
	#endif	
	MaterialData.NdotV = CalcNdotV(MaterialData.WorldNormal, FragData.CameraVector);	

	MaterialData.OneMinusReflectivity = OneMinusReflectivityMetallic(MaterialData.Metallic);
	
	MaterialData.DiffuseColor = MaterialData.DyeColor * MaterialData.OneMinusReflectivity;
	MaterialData.SpecularColor = lerp (kDieletricSpec.rgb, MaterialData.DyeColor.rgb, MaterialData.Metallic);

	MaterialData.PerceptualRoughness = _Roughness;//(1.0 - _Smoothness);
	MaterialData.Roughness =  max(MaterialData.PerceptualRoughness*MaterialData.PerceptualRoughness, HALF_MIN);
    MaterialData.Roughness2 = MaterialData.Roughness * MaterialData.Roughness;



	//MaterialData.Metallic = max(_Metallic, REAL(0.0001));
    // MaterialData.NormalizationTerm = MaterialData.Roughness * 4.0h + 2.0h;
    // MaterialData.Roughness2MinusOne = MaterialData.Roughness2 - 1.0h;

	// FLOAT3 DielectricSpecular = FLOAT3(0.04,0.04,0.04);
	
	// #ifdef _UNREAL_MODE
	// 	FLOAT3 MetalDielectricSpecular = 0.08 * _Spec;
	// 	FLOAT3 SpecularColor = (MetalDielectricSpecular - MetalDielectricSpecular * MaterialData.Metallic) + MaterialData.DyeColor.rgb * MaterialData.Metallic;	// 2 mad
	// 	MaterialData.SpecularColor = lerp(DielectricSpecular,SpecularColor,_ParamMetallic);
	// #else

	// 	outBRDFData.specular = lerp(kDieletricSpec.rgb, albedo, metallic);

	// 	MaterialData.SpecularColor = lerp (DielectricSpecular, MaterialData.DyeColor.rgb, MaterialData.Metallic);
	// #endif
}
//AO
// #define _AOBias _Param0.x

void CalcToonAO(inout FMaterialData MaterialData)
{
	#ifdef _NO_TOON_AO
		MaterialData.ToonAO = 0.5;
	#else
		MaterialData.ToonAO = MaterialData.CustomParam.a;
	#endif
}

void CalcAO(inout FMaterialData MaterialData)
{
	MaterialData.AO = 1;
#ifndef _NO_AO
	UNITY_BRANCH
	if(_AOBias>0.01)
	{
		// FLOAT gray = Luminance(MaterialData.DyeColor);
		// FLOAT a =  2.0404 * gray - 0.3324;
		// FLOAT b = -4.7951 * gray + 0.6417;
		// FLOAT c =  2.7552 * gray + 0.6903;
		// FLOAT AO = RangeMask(MaterialData.BaseColor.a,0.3,0.7);
		// FLOAT AO2 = AO*AO;
		// FLOAT AO3 = AO2*AO;
		// gray =  max(AO, (a*AO3 + b*AO2 + c*AO));
		MaterialData.AO = lerp(1, MaterialData.SrcAO, _AOBias);//gray + 1 - _AOBias;
		// MaterialData.AO = MaterialData.BaseColor.a*AO;
	}
#endif
}

//Emission
#define _Emi_Effect _Param2.w>0.5
#define _Emi_Tex _ProcedureTex3
#define _Emi_UseTex _Color1.w > 0.5

#define _Emi_Param _Param2
#define _Emi_Color _Color1

FLOAT3 Emissive0(FLOAT2 uv,FLOAT3 Emissive,FLOAT4 Param,FLOAT4 Color)
{
	#define _Emi_Intensity Param.x
	#define _Emi_FlowSpeed Param.y
	#define _Emi_Amplitude Param.z

	FLOAT a = abs(frac((uv.y+_Time.x*3)*3)-0.5); 	
	a = a * a;
	FLOAT3 geo = 0;

	UNITY_BRANCH
	if(_Emi_UseTex)
	{
		geo = SAMPLE_TEX2D(_ProcedureTex3, uv * 5 * _Emi_Amplitude + FLOAT2(0, _Time.y * _Emi_FlowSpeed)).rgb;
		return Emissive.xyz *_Emi_Color.xyz * (geo * _Emi_Intensity + 1);
	}else
	{
		FLOAT noise = abs(cnoise(uv*5*_Emi_Amplitude+FLOAT2(0,_Time.y*_Emi_FlowSpeed))*0.5+0.5);
		noise = noise*noise*noise*noise;
		geo = noise.xxx * 20 + 1;
		return Emissive.xyz *_Emi_Color.xyz*geo*_Emi_Intensity;
	}
	
	
}

FLOAT3 FlowUVW (FLOAT2 uv, FLOAT2 flowVector, FLOAT tiling, FLOAT time, bool flowB)
{
	FLOAT phaseOffset = flowB ? 0.5 : 0;
	FLOAT progress = frac(time + phaseOffset);
	FLOAT3 uvw;
	uvw.xy = uv - flowVector * progress;
	uvw.xy *= tiling;
	uvw.xy += phaseOffset;
	uvw.xy += (time - progress) * 0.25;
	uvw.z = 1 - abs(1 - 2 * progress);
	return uvw;
}
FLOAT3 Emissive1(FLOAT2 uv, FLOAT3 emissive, FLOAT4 param, FLOAT4 color)
{
	#ifdef _Emi_Intensity
	#undef _Emi_Intensity
	#endif
	#ifdef _Emi_FlowSpeed
	#undef _Emi_FlowSpeed
	#endif
	#ifdef _Emi_Amplitude
	#undef _Emi_Amplitude
	#endif
	
	#define _Emi_Intensity param.x
	#define _Emi_FlowSpeed param.y
	#define _Emi_Amplitude param.z
	
	half4 flowVector = SAMPLE_TEX2D(_ProcedureTex3, uv) * _Emi_Amplitude;
	// FLOAT2 flowVector = tex2D(_FlowMap, IN.uv_MainTex).rg * 2 - 1;
	FLOAT noise = flowVector.a;
	FLOAT time = _Time.y * _Emi_FlowSpeed + noise;
	FLOAT3 uvwA = FlowUVW(uv, flowVector.xy, _Emi_Amplitude, time, false);
	FLOAT3 uvwB = FlowUVW(uv, flowVector.xy, _Emi_Amplitude, time, true);
	
	FLOAT4 texA = SAMPLE_TEX2D(_MainTex, uvwA.xy) * uvwA.z;
	FLOAT4 texB = SAMPLE_TEX2D(_MainTex, uvwB.xy) * uvwB.z;
	emissive *= lerp((texA.rgb + texB.rgb) * 2, 1, _Emi_Color.w);
	return emissive * _Emi_Color.xyz * _Emi_Intensity;
}

#define CustomEmissive Emissive0
inline void CalcEmissive(in FFragData FragData,inout FMaterialData MaterialData)
{
#ifndef _NO_EMISSIVE
	UNITY_BRANCH
	if(_Emi_Effect)
	{
		FLOAT3 mask = saturate(MaterialData.BaseColor.a - 0.8)*5;
		FLOAT3 Emissive = MaterialData.BaseColor.xyz*mask;
		MaterialData.Emissive += CustomEmissive(GET_FRAG_UV,Emissive,_Emi_Param,_Emi_Color);
	}
#endif//_NO_EMISSIVE
}

// inline FLOAT4 GetEmissionAOColor(FLOAT2 uv,FLOAT4 color)
// {
// 	FLOAT4 emi = FLOAT4(0,0,0,0);
// 	#ifdef _SCENE_EFFECT
// 		FLOAT4 effectTex = color*color.a;	
// 		effectTex.a = 1;
// 	#else//!_SCENE_EFFECT
// 		FLOAT4 effectTex = SAMPLE_TEX2D(_EffectTex, uv);
// 		effectTex.xyz = effectTex.x*color.xyz;		
// 	#endif//_SCENE_EFFECT

// 	FLOAT a = abs(frac((uv.y+_Time.x*3)*3)-0.5); 	
// 	a = a * a;
// 	FLOAT noise = abs(cnoise(uv*5*_Emi_Amplitude+FLOAT2(0,_Time.y*_Emi_FlowSpeed))*0.5+0.5);
// 	noise = noise*noise*noise*noise;
// 	emi.xyz = effectTex.xyz *_Emi_Color.xyz*(1+noise*20)*_Emi_Intensity;
// 	emi.a = effectTex.a;
// 	return emi;
// }

#endif //PBS_MATERIALTEMPLATEHEAD_INCLUDE