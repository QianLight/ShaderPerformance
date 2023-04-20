// Copyright 2018- PWRD, Inc. All Rights Reserved.

/**
* MaterialTemplate.hlsl: Filled in by user defined function for each material being compiled.
*/
#include "MaterialTemplateHead.hlsl"

#ifndef PBS_MATERIALTEMPLATE_INCLUDE
#define PBS_MATERIALTEMPLATE_INCLUDE


FFragData GetFragData(FInterpolantsVSToPS Interpolants, FLOAT4 SvPosition)
{
	DECLARE_OUTPUT(FFragData, FragData);
	GET_VS_DEPTH(Interpolants);
	FragData.SvPosition = SvPosition;
#ifdef _SCREEN_POS
	FragData.ScreenPosition = FLOAT4(Interpolants.ScreenPosition.xy/Interpolants.ScreenPositionW.x,Interpolants.ScreenPosition.zw/Interpolants.ScreenPositionW.y);
	// FragData.Depth01 = Interpolants.Depth01;
#endif

#ifdef _SCREEN_POSS
	FragData.ScreenPosition = FLOAT4(Interpolants.ScreenPosition.xy/Interpolants.ScreenPositionW.x,Interpolants.ScreenPosition.zw/Interpolants.ScreenPositionW.y);
	//FragData.Depth01 = Interpolants.Depth01;
#endif

	FragData.WorldPosition = Interpolants.WorldPosition;

#ifdef _GAMEVIEW_CAMERA_POS
	FragData.WorldPosition_CamRelative = Interpolants.WorldPosition.xyz - _GameViewWorldSpaceCameraPos.xyz;
#else
	FragData.WorldPosition_CamRelative = Interpolants.WorldPosition.xyz - _CameraPos.xyz;
#endif
	
	//FragData.TangenSign = Interpolants.TangentToWorld2.w;
	//REAL3 TangentToWorld1 = cross(Interpolants.TangentToWorld2.xyz, Interpolants.TangentToWorld0.xyz)* Interpolants.TangentToWorld2.w;
	//FragData.TangentToWorld = REAL3x3(Interpolants.TangentToWorld0.xyz, TangentToWorld1, Interpolants.TangentToWorld2.xyz);
	FragData.TangentToWorld = REAL3x3(Interpolants.TangentWS.xyz, Interpolants.BitangentWS.xyz, Interpolants.NormalWS.xyz);
	FragData.CameraVector = SafeNormalize(FLOAT3(Interpolants.NormalWS.w,Interpolants.TangentWS.w,Interpolants.BitangentWS.w));
	FragData.LocalPosition = Interpolants.LocalPosition;

#ifdef _CLOUD2
	REAL3x3 Rotation = REAL3x3(Interpolants.TangentWS.xyz, Interpolants.BitangentWS.xyz, Interpolants.NormalWS.xyz);
	FLOAT3 ObjSpaceCameraPos = mul((FLOAT4x4)_worldToObject, FLOAT4(_CameraPos.xyz,1)).xyz;
    FragData.CameraVector = mul(Rotation, ObjSpaceCameraPos-Interpolants.ObjectPosition.xyz);
#endif


#if (_OUTPUT_UV_COUNT>0)
	FragData.TexCoords[0] = Interpolants.TexCoords[0];
	#if (_OUTPUT_UV_COUNT>1)
		FragData.TexCoords[1] = Interpolants.TexCoords[1];
	#endif//(_OUTPUT_UV_COUNT>1)
#endif//(_OUTPUT_UV_COUNT>0)

#ifdef _VERTEX_COLOR
	FragData.VertexColor = Interpolants.Color;
#endif//_VERTEX_COLOR

#ifdef _VERTEX_GI
	FragData.Ambient = Interpolants.DiffuseGI;
#endif//_VERTEX_GI

#if defined(_SHADOW_MAP)&&!defined(_NO_SHADOWMAP)
	#if defined(_SIMPLE_SHADOW)
		FragData.ShadowCoord2 = Interpolants.ShadowCoord0.xyz;
	#else
		DecodeShadowCoord(FragData.ShadowCoord,
			Interpolants.ShadowCoord0,
			Interpolants.ShadowCoord1,
			Interpolants.ShadowCoord2
		);
	#endif//_SIMPLE_SHADOW
#endif//_SHADOW_MAP



#ifdef _CUSTOM_VERTEX_PARAM
	FragData.CustomData = Interpolants.CustomData;
	FragData.CustomData1 = Interpolants.CustomData1;	
#endif//_CUSTOM_VERTEX_PARAM

#ifdef _INSTANCE
	FragData.InstanceID = Interpolants.InstanceID;
#endif//_INSTANCE

#ifdef _ENABLE_DEBUG
	FragData.VertexDebugData = Interpolants.VertexDebugData;
#endif
	return FragData;
}

inline FMaterialData GetMaterialData(FFragData FragData)
{
	DECLARE_OUTPUT(FMaterialData, MaterialData)

#ifdef _CUSTOM_MATERIAL
	CustomMaterial(FragData,MaterialData);
#else//!_CUSTOM_MATERIAL

	#ifdef _CUSTOM_BASECOLOR
		CalcCustomBaseColor(FragData,MaterialData);
	#else //!_CUSTOM_BASECOLOR
		CalcDefaultBaseColor(FragData,MaterialData);
	#endif//_CUSTOM_BASECOLOR

	#ifdef _CUSTON_TOON_AO
		CustomToonAO(MaterialData);
	#else
		CalcToonAO(MaterialData);
	#endif

	#ifdef _ALPHA_TEST
		MaterialAlphaTest(FragData,MaterialData);
	#endif

	// #ifdef _ALPHA_TEST_IMPOSTOR
	// MaterialAlphaTest(FragData,MaterialData);
	// #endif
	
	#ifndef _UN_LIGHT
		CalcPBS(FragData, MaterialData);
		CalcEmissive(FragData, MaterialData);

		#if (defined(_ALPHA_BLEND)&&!defined(_ALPHA_TEST))||(!defined(_ALPHA_BLEND))
			CalcAO(MaterialData);
		#else
			MaterialData.AO = 1;
		#endif
			
		#ifdef _CUSTOM_EFFECT
			CalcCustomEffect(FragData, MaterialData);
		#endif//_CUSTOM_EFFECT	
	#else//!_UN_LIGHT
		MaterialData.DiffuseColor = MaterialData.DyeColor;
	#endif//_UN_LIGHT
#endif//_CUSTOM_MATERIAL

#ifdef CUSTOM_BLOOM_INTENSITY
		MaterialData.BloomIntensity = CustomBloomIntensity(FragData, MaterialData);
#else
		MaterialData.BloomIntensity = 1;
#endif

	return MaterialData;
}
#endif //PBS_MATERIALTEMPLATE_INCLUDE