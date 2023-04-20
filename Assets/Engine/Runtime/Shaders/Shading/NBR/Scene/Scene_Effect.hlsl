
/**
* Common.cginc: Scene Effect and paramaters.
*/
#ifndef SCENE_EFFECT_INCLUDE
#define SCENE_EFFECT_INCLUDE

#ifdef _CUSTOM_EFFECT

//OverLay
#define _OverLay_Effect _Param3.w>0.5
#define _OverLay_Tex _ProcedureTex1
#define _OverLay_Mask _Param3.x
#define _OverLay_Ratio _Param3.y
#define _OverLay_UVScale _Param3.zz
#define _OverLay_Color _Color2

inline void CalcOverlay(in FFragData FragData,inout FMaterialData MaterialData)
{
	FLOAT2 uv = GET_FRAG_UV * _OverLay_UVScale;
	FLOAT4 overlay = SAMPLE_TEX2D(_ProcedureTex1,uv);
	overlay *= _OverLay_Color;
	FLOAT OverLayAlpha = smoothstep(_OverLay_Ratio - _OverLay_Mask, _OverLay_Ratio + _OverLay_Mask, MaterialData.WorldNormal.y * 0.5);
	MaterialData.DiffuseColor = lerp(MaterialData.BaseColor.xyz, overlay.xyz, OverLayAlpha);
}

//detial
#define _DetailTex _ProcedureTex2
#define _Detail_Enable _Param4.w>0.5
#define _Detail_UVScale _Param4.xy
#define _Detail_NormalWeight _Param4.z

inline FLOAT3 DetailPBS(in FFragData FragData,inout FMaterialData MaterialData,out FLOAT2 rm)
{
	FLOAT3 normal = FLOAT3(0.5,0.5,1);
#ifndef _DEFAULT_PBS_PARAM
	FLOAT4 srcPbs = SAMPLE_TEX2D(_PBR_Tex, GET_FRAG_UV);
	normal.xy = srcPbs.xy;
	rm.xy = srcPbs.zw;
	#if defined(_PBS_FROM_PARAM)
		rm.x = lerp(_ParamRoughnessMin,_ParamRoughnessMax,rm.x);
		rm.y = lerp(_ParamMetallicMin,_ParamMetallicMax,rm.y);
	#endif//_PBS_FROM_PARAM

	MaterialData.SrcAO = lerp(srcPbs.w, 1, _MetallicScale);

	UNITY_BRANCH
	if(_Detail_Enable)
	{
		FLOAT2 detailUV = GET_FRAG_BACKUP_UV*_Detail_UVScale;	
		FLOAT4 detailPbs = SAMPLE_TEX2D(_DetailTex, detailUV);	
		normal.xy = (FLOAT3(clamp(-1,1,srcPbs.xy*2-1+(detailPbs.xy*2-1)*_Detail_NormalWeight),1)).xy*0.5+0.5;
	}
	rm.y *= _MetallicScale;
#endif//_DEFAULT_PBS_PARAM
	MaterialData.ScaleParam = FLOAT4(_SpecularScale,_IBLScale,1,0);
	return normal;
}

#define CalcCustomPBS DetailPBS
#define _CUSTOM_PBS

//BoxReflect
// #define _Box_Reflect _IBLScale>10//not share with ibl
#define _Reflect_Tex _EnvCube
#define _BoxCenter _Param2
#define _BoxSize _Param3

// #ifdef _NEED_BOX_PROJECT_REFLECT

// 	FLOAT3 ReflCube = DecodeHDR(SAMPLE_TEXCUBE_LOD(_EnvReflectTex, MaterialData.ReflectionVector, _BoxCenter.w), _EnvCubemapParam.xyz);
// 	DEBUG_CUSTOMDATA_PARAM(ReflCube, ReflCube)
// 	Color = lerp(Color,ReflCube,_BoxSize.w);
// #endif//_NEED_BOX_PROJECT_REFLECT

// #ifdef _PARALLAX_EFFECT
// 	Color *= FragData.Parallax.a;
// #endif //_PARALLAX_EFFECT

// inline void CalcBoxReflect (FLOAT3 worldRefl, FLOAT3 worldPos, FLOAT3 cubemapCenter, FLOAT3 boxMin, FLOAT3 boxMax)
// {
// 	MaterialData.ReflectionVector,FragData.WorldPosition,_BoxCenter.xyz,_BoxCenter.xyz-_BoxSize.xyz*0.5,_BoxCenter.xyz+_BoxSize.xyz*0.5

// 	FLOAT3 nrdir = normalize(MaterialData.ReflectionVector);
// 	FLOAT3 rbmax = (nrdir == 0.0f ? FLT_MAX : (boxMax - worldPos) / nrdir);
// 	FLOAT3 rbmin = (nrdir == 0.0f ? FLT_MAX : (boxMin - worldPos) / nrdir);

// 	FLOAT3 rbminmax = (nrdir > 0.0f) ? rbmax : rbmin;

// 	FLOAT fa = min(min(rbminmax.x, rbminmax.y), rbminmax.z);

// 	worldPos -= cubemapCenter;
// 	worldRefl = worldPos + nrdir * fa;
	
// 	return worldRefl;
// }

//Parallax
#define _POM_Effect _Param3.z>1
#define _POMParamX _Param3.x
#define _POMParamY _Param3.y
#define _POMCount ((int)_Param3.z)
#define _POMCount2 ((int)_Param3.w)


inline void CalcCustomEffect(in FFragData FragData,inout FMaterialData MaterialData)
{
	//OverLay
	UNITY_BRANCH
	if(_OverLay_Effect)
	{
		CalcOverlay(FragData,MaterialData);
	}
}

#endif//_CUSTOM_EFFECT

#endif //SCENE_EFFECT_INCLUDE