// Copyright 2018- PWRD, Inc. All Rights Reserved.
#include "AddLighting.hlsl"
//#include "ShadowLib.hlsl"
#ifndef PBS_LIGHTING_INCLUDE
#define PBS_LIGHTING_INCLUDE

// #define _COMBINE_SHADOWMASK

//FLOAT4 _LightmapScale;
#define _SceneSHScale _AmbientParam.x
#define _RoleSHScale _AmbientParam.y
#define _GIScale _AmbientParam.z
#define _GIDefault _AmbientParam.w
// FLOAT4 _AmbientParamTest;
// #define _LightmapShadowInt (_AmbientParamTest.x)
// #define _LightmapShadowRange (_AmbientParamTest.y)
//#define _ShadowMaskScale _LightmapScale.y

#if defined(_CUSTOM_LIGHTMAP_ON)
TEX2D_SAMPLER(_CustomLightmap);
TEX2D_SAMPLER(unity_LightmapInd);
TEX2D_SAMPLER(_CustomShadowMask);
#ifdef _EDITOR
 //TEX2D_SAMPLER(unity_ShadowMask);
FLOAT shadowMaskLerp;
#endif
// FLOAT4 unity_Lightmap_HDR;

inline void SampleGI(out FLOAT4 color,out FLOAT4 dir,out FLOAT shadow,in FLOAT2 uv)
{
	color = SAMPLE_TEX2D(_CustomLightmap, uv);
	//dir = SAMPLE_TEX2D(unity_LightmapInd, uv);
	//dir.w = max(1e-4h, dir.w);
	dir = FLOAT4(0,1,0,1);
	shadow = 1;
// #ifdef _EDITOR
// 	shadow = lerp(shadow,1,shadowMaskLerp);
    FLOAT4 mask=FLOAT4(1,1,1,1);
    //#ifdef _PlayerShadowOnly 
	
	 	 //mask = SAMPLE_TEX2D(_CustomShadowMask, uv)*4;	
    //#endif
	 //TEST
	 //   if(_shadowMapIndex>0)
	 //{
	 //	 mask = SAMPLE_TEX2D(_CustomShadowMask, uv)*4;	
  //   }

 	//shadow *= saturate(mask.x+mask.y+mask.z+mask.w);
	shadow *= mask.x;
// #endif	
}

inline FLOAT3 DecodeLightmap(FLOAT4 color)
{
#if defined(UNITY_LIGHTMAP_DLDR_ENCODING)//mobile
	// #if defined(_GRASS_LIGHT)
	// 	return color.rgb*4.59f;
	// #else
	return color.rgb*_GIScale;//pow(2.0,2.2);		
	// #endif
#elif defined(UNITY_LIGHTMAP_RGBM_ENCODING)//pc
    return (34.49 * pow(color.a, 2.2)) * color.rgb*_GIScale;//x=pow(5.0,2.2);y=2.2
#else //defined(UNITY_LIGHTMAP_FULL_HDR)
    return color.rgb;
#endif
}
#endif


inline FShadowData CalcShadow(inout FFragData FragData,in FMaterialData MaterialData DEBUG_ARGS)
{
#if defined(_SHADOW_MAP)&&!defined(_NO_SHADOWMAP)
	#ifdef _CUSTOM_SHADOW_MAP_MASK
		FLOAT mask = GetCustumShadowMapMask(FragData,MaterialData);
	#else
		FLOAT mask = saturate(dot(MaterialData.WorldNormal, _MainLightDir.xyz)+0.05);
	#endif		
	 
	return ShadowCompareParallel(FragData,MaterialData,mask DEBUG_PARAM);
#else//!_SHADOW_MAP
   // _shadowMapIndex=2;
	return DefaultShadowData();
#endif//_SHADOW_MAP
}

// _SceneColor.a�����������Ǻͳ���+������ɫ
inline void CalcSceneColor(inout FLOAT3 color,FLOAT srcShadow)
{	
	#ifdef _SCENE_EFFECT		
		FLOAT3 targetColor = color.xyz * _SceneColor.rgb;
	#else
		FLOAT3 targetColor = color*lerp(_SceneColor.rgb, 1, _SceneColor.a);		
	#endif
		targetColor = lerp(targetColor, 0, _ContrastLight);
		FLOAT contrastRatio = lerp(0, srcShadow, _ContrastLight);
		color.xyz = lerp(targetColor, Luminance(color.xyz * _ContrastInsity) * _SceneColor.rgb, contrastRatio);

}

#define REFLECTION_CAPTURE_ROUGHEST_MIP 1
#define REFLECTION_CAPTURE_ROUGHNESS_MIP_SCALE 1.2

/** 
 * Compute absolute mip for a reflection capture cubemap given a roughness.
 */
FLOAT ComputeReflectionCaptureMipFromRoughness(FLOAT Roughness, FLOAT CubemapMaxMip)
{
	// Heuristic that maps roughness to mip level
	// This is done in a way such that a certain mip level will always have the same roughness, regardless of how many mips are in the texture
	// Using more mips in the cubemap just allows sharper reflections to be supported
	FLOAT LevelFrom1x1 = REFLECTION_CAPTURE_ROUGHEST_MIP - REFLECTION_CAPTURE_ROUGHNESS_MIP_SCALE * log2(Roughness);
	return CubemapMaxMip  - LevelFrom1x1;

	// FLOAT r = saturate(Roughness*2);
	// return  (r *1.7 - 0.7 * r * r) * CubemapMaxMip;
}

FLOAT3 CalcIBL(in FFragData FragData,in FMaterialData MaterialData DEBUG_ARGS)
{
	FLOAT3 IBL = FLOAT3(0,0,0);
#ifndef _PBS_NO_IBL
	UNITY_BRANCH
	if(_IBLEnable)
	{
		//FLOAT3 ReflectionVector = reflect(-FragData.CameraVector, MaterialData.WorldNormal);
		FLOAT3 ReflectionVector = -FragData.CameraVector + MaterialData.WorldNormal  * dot(MaterialData.WorldNormal, FragData.CameraVector) * 2.0;
		DEBUG_CUSTOMDATA_PARAM(ReflectionVector, ReflectionVector)
		#ifdef _UNREAL_MODE	
		
			FLOAT Mip = ComputeReflectionCaptureMipFromRoughness(MaterialData.Roughness, _MaxMip);
			DEBUG_CUSTOMDATA_PARAM(CubeMipmap, Mip)
			FLOAT4 ImageBasedReflections = FLOAT4(0, 0, 0, 1);

			FLOAT4 Sample = SAMPLE_TEXCUBE_LOD(_EnvCube, ReflectionVector, Mip);
			IBL = DecodeHDR(Sample,_HDRParam);
			DEBUG_CUSTOMDATA_PARAM(IndirectSpecular, IBL)

			/*FLOAT3 N = MaterialData.WorldNormal;
			FLOAT3 V = FragData.CameraVector;
			FLOAT3 RayDirection = 2 * dot( V, N ) * N - V;
			FLOAT NoV = saturate(MaterialData.NdotV);*/
			FLOAT3 SpecularColor = EnvBRDFApprox(MaterialData.SpecularColor, MaterialData.Roughness2, MaterialData.NdotV);

			IBL *= SpecularColor*_IBLMult;
		#else
			
			FLOAT perceptualRoughness = MaterialData.PerceptualRoughness * (1.7 - 0.7 * MaterialData.PerceptualRoughness);

			FLOAT Mip =  perceptualRoughness * _MaxMip;			
			DEBUG_CUSTOMDATA_PARAM(CubeMipmap, Mip)
			FLOAT4 cube = SAMPLE_TEXCUBE_LOD(_EnvCube, ReflectionVector, Mip);

			IBL = DecodeHDR(cube,_HDRParam);
			DEBUG_CUSTOMDATA_PARAM(IndirectSpecular, IBL)

			FLOAT fresnelTerm = Pow4(1.0 - MaterialData.NdotV);
			FLOAT surfaceReduction = 1.0 / (MaterialData.Roughness2 + 1.0);			
			DEBUG_CUSTOMDATA_PARAM(IBLFresnel, MaterialData.NdotV<0.2?1:0)

			REAL reflectivity = 1.0 - MaterialData.OneMinusReflectivity;
			REAL smoothness = 1 - MaterialData.PerceptualRoughness;
			FLOAT grazingTerm = saturate(smoothness + reflectivity);

			FLOAT3 iblScale = saturate(surfaceReduction*lerp(MaterialData.SpecularColor, grazingTerm.xxx, fresnelTerm));
			DEBUG_CUSTOMDATA_PARAM(IBLScale, grazingTerm)
			IBL *= iblScale*_IBLMult;
		#endif
	}
#endif
	return IBL;
}

FLOAT4 CalcLighting(FFragData FragData, FMaterialData MaterialData DEBUG_ARGS)
{
	FLOAT3 DirectDiffuse = FLOAT3(0, 0, 0);
	FLOAT3 DirectSpecular = FLOAT3(0, 0, 0);
	FLOAT3 Color = FLOAT3(0, 0, 0);
	FShadowData ShadowData = DefaultShadowData();
	FLOAT3 GI = FLOAT3(0,0,0);
	FLOAT AddShadow = 1;
	FLOAT ShadowMask = 1;
  //  FLOAT _shadowMapIndex=2;//TEST 
	// FLOAT ShadowNdotL = 1;
	#ifdef _UN_LIGHT
		SM_Unlit(MaterialData.DiffuseColor,DirectDiffuse,DirectSpecular DEBUG_PARAM);
		Color += DirectDiffuse + DirectSpecular;
		DEBUG_CUSTOMDATA_PARAM(CombineColor, Color)	
	#else
		#if defined(_CUSTOM_LIGHT)
			//Calc Shadow
			ShadowData = CalcShadow(FragData,MaterialData DEBUG_PARAM);


			DEBUG_CUSTOMDATA_PARAM(Shadow, ShadowData.Shadow.xyz)
			#if defined(_CUSTOM_SHADOWMASK)
				ShadowMask = CustomShadowMask(FragData,MaterialData, ShadowData.Shadow.xy);
			#endif
			CustomLighting(FragData,MaterialData, ShadowData,ShadowMask,DirectDiffuse,DirectSpecular DEBUG_PARAM);
		#else//_STANDARD_LIGHT

			//==========================1st dirLight Shadow & Lighting==========================
		
			ShadowData = CalcShadow(FragData,MaterialData DEBUG_PARAM);
			DEBUG_CUSTOMDATA_PARAM(Shadow, ShadowData.Shadow.xyz)

			FLightingContext LC = InitLight(FragData,MaterialData,_SpecMult,
				_MainLightDir.xyz,_MainLightColor.xyz,
				1, ShadowData,_ShadowColor.xyz);
			
			#ifdef _CUSTOM_LIGHT_MODIFY
				CustomLightContext(LC);
			#endif
			DEBUG_CUSTOMDATA_PARAM(LC0, LC)

			SM_Standard(LC,DirectDiffuse,DirectSpecular DEBUG_PARAM);
		#endif//_CUSTOM_LIGHT
		//==========================Lightmap GI==========================
		#ifdef _CUSTOM_LIGHTMAP_ON
			FLOAT4 bakedColorTex = _GIDefault;
			GI = DecodeLightmap(bakedColorTex);
			FLOAT bakeShadow = 1;
			FLOAT4 bakedDir = FLOAT4(0, 1, 0, 1);

			UNITY_BRANCH
			if(_LightMapEnable)
			{				
				SampleGI(bakedColorTex,bakedDir,bakeShadow,GET_FRAG_LIGTHMAP_UV);
				GI = DecodeLightmap(bakedColorTex);	
				//AddShadow*=bakeShadow;
			    AddShadow = lerp(1, Luminance(saturate(GI * 5)), 0.6);

				DirectDiffuse *= bakeShadow;
			}			
			
			DEBUG_CUSTOMDATA_PARAM(AddShadow, AddShadow)
			ShadowMask = bakeShadow;
		
			DEBUG_CUSTOMDATA_PARAM(GI, GI.xyz)

#ifdef _CUSTOM_LIGHTMAP
			Custom_Lightmap(FragData, MaterialData, GI, DirectDiffuse DEBUG_PARAM);
#else
			SM_Lightmap(MaterialData, GI, ShadowData.Shadow.z, ShadowData.Shadow.w, DirectDiffuse DEBUG_PARAM);
#endif
		#endif

		//==========================2end dirLight Lighting==========================
		#ifndef _NO_ADDLIGHT
		UNITY_BRANCH
		if(_AddLightEnable)
		{
			#define specScale1 _SpecMult*0.5f
			FLightingContext LC1 = InitLight(FragData,MaterialData,specScale1,_AddLightDir.xyz,_AddLightColor.xyz,1, DefaultShadowData(),1);
			SM_Supplement(LC1, DirectDiffuse,DirectSpecular DEBUG_PARAM);
			DEBUG_CUSTOMDATA_PARAM(LC1, LC1)
		}
		#endif//_NO_ADDLIGHT
		Color += DirectDiffuse + DirectSpecular;
		
		#ifdef _GAMMA_CORRECT
		Color = pow(abs(Color),2.2);
		#endif//_GAMMA_CORRECT
		DEBUG_CUSTOMDATA_PARAM(CombineColor, Color)	

		//==========================point Light Lighting==========================
		#ifdef _ENABLE_DEBUG
			DECLARE_OUTPUT(LightDebug, ld);
		#endif
		FLOAT3 AddColor = CalcAddLighting(FragData,MaterialData,ShadowMask
		#ifdef _ENABLE_DEBUG
			,ld
		#endif
			DEBUG_PARAM);
		#ifdef _ENABLE_DEBUG
			DEBUG_CUSTOMDATA_PARAM(addLightDebug, ld)
		#endif
		DEBUG_CUSTOMDATA_PARAM(AddColor, AddColor)
		Color += AddColor;

		//==========================Ambient Lighting==========================
		#ifdef _SCENE_EFFECT
			FLOAT3 Ambient = SHPerPixel(MaterialData.WorldNormal, FragData.Ambient.xyz, FragData.WorldPosition.xyz)*_SceneSHScale;
		#else//!_SCENE_EFFECT
			FLOAT3 Ambient = SHPerPixel(MaterialData.WorldNormal, FragData.Ambient.xyz, FragData.WorldPosition.xyz)*_RoleSHScale;
		#endif//_SCENE_EFFECT
		
		FLOAT3 ambientColor = Ambient*MaterialData.DiffuseColor;
		DEBUG_CUSTOMDATA_PARAM(SH, Ambient)
		DEBUG_CUSTOMDATA_PARAM(Ambient, ambientColor)
		Color += ambientColor;

		//==========================IBL Lighting==========================
		FLOAT3 IBL = CalcIBL(FragData,MaterialData DEBUG_PARAM);		
		DEBUG_CUSTOMDATA_PARAM(IBL, IBL)
		Color += IBL;
		Color *= MaterialData.AO*AddShadow;
	#endif

	// #ifdef _CUSTOM_POST_LIGHTING
	// 	CustomLightContext(LC,DirectDiffuse,DirectSpecular DEBUG_PARAM);
	// #endif	

	//==========================Emissive==========================
	Color += MaterialData.Emissive * lerp(_SceneColor.rgb, 1, _SceneColor.a);
	FLOAT opaque = MaterialData.BaseColor.a;

	#ifdef _POST_CUSTOM_LIGHTING
		Color.xyz = CalcPostCustomLighting(FragData,MaterialData,IBL,Color.xyz, opaque, ShadowMask DEBUG_PARAM);
	#endif //_POST_CUSTOM_LIGHTING

	//==========================Color Modify==========================
	CalcSceneColor(Color, GetSrcShadow(ShadowData));
	

	return FLOAT4(Color,opaque);
}

FLOAT3 CalcColorEffect(in FMaterialData MaterialData,in FLOAT3 color)
{
	FLOAT3 c = color;
#ifndef _NO_COLOR_EFFECT
	//add color
	UNITY_BRANCH
	if(_Color.a < 0.45)
	{
		//c = color * _Color.rgb;
		c = saturate(lerp(_Color.rgb*_Color.a,_Color.rgb,dot(c.rgb,0.333)));
	}
	//rim
	UNITY_BRANCH
	if(_Color.a > 0.49&&_Color.a < 0.91)
	{
		c = color + Square(saturate(1 - MaterialData.NdotV)) * (40 *_Color.a - 20) * _Color.rgb;
	}
#endif//_NO_COLOR_EFFECT




return c;
}

#endif //PBS_LIGHTING_INCLUDE
