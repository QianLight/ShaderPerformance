// Copyright 2018- PWRD, Inc. All Rights Reserved.

#ifndef PBS_ADDLIGHTING_INCLUDE
#define PBS_ADDLIGHTING_INCLUDE

//====================================base add lighting====================================
REAL DistanceAttenuation(FLOAT r2, FLOAT rangeAtten)
{
	float atten = max(0, 1 - r2 * rangeAtten);
	return atten * atten;
}

//inline void PointLighting(in FFragData FragData,in FMaterialData MaterialData,
//	in LightInfo li,FLOAT invRadius,FLOAT shadowMask,
//	out FLOAT3 diffuse)
//{
//	REAL3 lightVector =  li.lightPos.xyz - FragData.WorldPosition.xyz;		
//	REAL distanceSqr = dot( lightVector, lightVector );
//
//	FLOAT3 lightDir = FLOAT3(lightVector * rsqrt(distanceSqr));
//
//#ifdef _UNREAL_MODE
//	FLOAT falloff = Square(saturate(1 - Square(distanceSqr * (invRadius))));	
//	FLOAT distSqr = clamp(distanceSqr,0.1f,10000);
//	FLOAT atten = falloff *rcp(distSqr);
//#else
//	FLOAT atten = DistanceAttenuation(distanceSqr,invRadius);
//#endif//_UNREAL_MODE
//
//	#ifdef _CUSTOM_ADDNOL
//		FLOAT ndotl = CalcCustomAddNoL(FragData,MaterialData,lightDir);
//	#else//_CUSTOM_NOL		
//		FLOAT ndotl = dot(MaterialData.WorldNormal, lightDir);
//	#endif//_CUSTOM_NOL
//	FLOAT fixNdotL = saturate(ndotl);
//
//	#ifdef _CUSTOM_ADDDIFFUSE
//		diffuse = CalcCustomAddDiffuse(FragData,MaterialData,fixNdotL,shadowMask) * li.lightColor.xyz * atten;	
//	#else//_CUSTOM_NOL	
//		diffuse = MaterialData.DiffuseColor * fixNdotL * li.lightColor.xyz* atten*shadowMask;	
//	#endif//_CUSTOM_NOL	
//}

inline void PointLightingSpec(in FFragData FragData,in FMaterialData MaterialData,
	in LightInfo li,in FLOAT invSqrRange,in FLOAT shadowMask,
	out FLOAT3 diffuse, out FLOAT3 spec
	#ifdef _ENABLE_DEBUG
			, inout LightDebug ld
	#endif
	)
{
	FLOAT3 lightDir =  normalize(_DynamicLightPos - FragData.WorldPosition.xyz);

	FLOAT3 v = (_DynamicLightPos-FragData.WorldPosition.xyz);
	FLOAT atten = DistanceAttenuation(dot(v, v), invSqrRange);

	#ifdef _CUSTOM_ADDNOL
		FLOAT ndotl = CalcCustomAddNoL(FragData,MaterialData,lightDir);
	#else//_CUSTOM_NOL		
		FLOAT ndotl = dot(MaterialData.WorldNormal, lightDir);
	#endif//_CUSTOM_NOL
	FLOAT fixNdotL = saturate(ndotl);
	
	#ifdef _CUSTOM_ADDDIFFUSE
		diffuse = CalcCustomAddDiffuse(FragData,MaterialData,fixNdotL,shadowMask,li,atten
			#ifdef _ENABLE_DEBUG
				, ld
			#endif
			);
	#else//_CUSTOM_NOL
		diffuse = MaterialData.DiffuseColor * fixNdotL * li.lightColor.xyz * atten * shadowMask;
	#endif//_CUSTOM_NOL	

	//spec
	FLOAT3 H = normalize(FragData.CameraVector + lightDir);
	FLOAT NdotH = saturate(dot(MaterialData.WorldNormal, H));
	FLOAT LdotH = (dot(lightDir, H));

	half3 attenLightColor = atten * li.lightColor.xyz * fixNdotL * shadowMask;
	spec = SpecularUnityOrig(MaterialData.Roughness,
		MaterialData.Roughness2,
		NdotH,
		LdotH,
		MaterialData.SpecularColor);
	spec = attenLightColor * lerp(spec, 1, _DynamicLightCoverage);
}

//====================================voxel lighting====================================
inline FLOAT3 VoxelPointLightingWithSpec(
	FLOAT lightIndex,
	in FFragData FragData,
	in FMaterialData MaterialData,
	FLOAT shadowMask
#ifdef _ENABLE_DEBUG
	,inout LightDebug ld
#endif//_ENABLE_DEBUG
)
{
	FLOAT3 c = 0;
	UNITY_BRANCH
	if(lightIndex>0)
	{
		lightIndex -=1;
		LightInfo li = _StaticLightInfos[(uint)lightIndex];

		FLOAT3 diff = 0;
		FLOAT3 spec = 0;
		PointLightingSpec(FragData,MaterialData,li, _DynamicLightInvSqrRange, shadowMask, diff, spec
#ifdef _ENABLE_DEBUG
			,ld
#endif
		);
		c = diff + spec;
#ifdef _ENABLE_DEBUG
		ld.Diffuse += diff;
		ld.Spec += spec;
#endif//_ENABLE_DEBUG	
	}
	return c;
}


inline FLOAT3 VoxelLighting(
	in FFragData FragData,
	in FMaterialData MaterialData,
	FLOAT shadowMask,
	FLOAT4 lightParam,
	TEX2D_ARGS(lightIndexTex)
#ifdef _ENABLE_DEBUG
	,inout LightDebug ld
#endif//_ENABLE_DEBUG
 DEBUG_ARGS)
{
	#define lightGridXZ  lightParam.yz
	#define lightTexInvSize  lightParam.w
	FLOAT3 color = 0;
	FLOAT2 lightTexUV = (FragData.WorldPosition.xz - lightGridXZ)*lightTexInvSize;
	FLOAT4 lightIndex = SAMPLE_TEX2D(lightIndexTex, lightTexUV)*255;

#if CELL_DYNAMIC_LIGHT_COUNT >= 1
	color += VoxelPointLightingWithSpec(
		lightIndex.x,
		FragData,
		MaterialData,
		shadowMask
	#ifdef _ENABLE_DEBUG
			,ld
	#endif
	);
#endif
#if CELL_DYNAMIC_LIGHT_COUNT >= 2
	color += VoxelPointLightingWithSpec(
		lightIndex.y,
		FragData,
		MaterialData,
		shadowMask
	#ifdef _ENABLE_DEBUG
			,ld
	#endif
	);
#endif
#if CELL_DYNAMIC_LIGHT_COUNT >= 3
	color += VoxelPointLightingWithSpec(
		lightIndex.z,
		FragData,
		MaterialData,
		shadowMask
	#ifdef _ENABLE_DEBUG
			,ld
	#endif
	);
#endif
#if CELL_DYNAMIC_LIGHT_COUNT >= 4
	color += VoxelPointLightingWithSpec(
		lightIndex.w,
		FragData,
		MaterialData,
		shadowMask
	#ifdef _ENABLE_DEBUG
			,ld
	#endif
	);
#endif

	DEBUG_CUSTOMDATA_PARAM(AddIndex,lightIndex)
	return color;
}

//====================================simple lighting====================================
//inline FLOAT3 SimplePointLightingWithSpec(
//	in FFragData FragData,in FMaterialData MaterialData,FLOAT shadowMask,uint index
//#ifdef _ENABLE_DEBUG
//	,inout LightDebug ld
//#endif//_ENABLE_DEBUG
//)
//{
//		FLOAT3 diff = 0;
//		FLOAT3 spec = 0;
//		LightInfo li;
//		li.lightPos = _DyanmicPointLightPos[index];
//		li.lightColor = _DyanmicPointLightColor[index];
//		li.lightParam = _DyanmicPointLightParam[index];
//		PointLightingSpec(FragData, MaterialData, li, _DynamicLightInvSqrRange, shadowMask, diff, spec
//#ifdef _ENABLE_DEBUG
//			,ld
//#endif
//		);	
//
//	#ifdef _ENABLE_DEBUG
//		ld.dynamicDiffuse += diff;
//		ld.dynamicSpec += spec;
//	#endif//_ENABLE_DEBUG
//
//		return (diff + spec)*_PointLightIntensity;
//}

inline FLOAT3 SimplePointLighting(
	in FFragData FragData,in FMaterialData MaterialData,FLOAT shadowMask,uint index
#ifdef _ENABLE_DEBUG
	,inout LightDebug ld
#endif//_ENABLE_DEBUG
)
{
		FLOAT3 diff = 0;
		//FLOAT3 spec = 0;
		//LightInfo li;
		//li.lightPos = _DyanmicPointLightPos[index];
		//li.lightColor = _DyanmicPointLightColor[index];
		//PointLighting(FragData,MaterialData,li,_InvRadius,shadowMask,diff);	

	#ifdef _ENABLE_DEBUG
		ld.dynamicDiffuse += diff;
	#endif//_ENABLE_DEBUG

		return (diff)*_PointLightIntensity;
}

inline FLOAT3 CalcAddLighting(in FFragData FragData,in FMaterialData MaterialData,FLOAT shadowMask
#ifdef _ENABLE_DEBUG
	,inout LightDebug ld
#endif//_ENABLE_DEBUG
	DEBUG_ARGS)
{
	#ifdef _ENABLE_DEBUG
		ld.Diffuse = FLOAT3(0,0,0);
		ld.Spec = FLOAT3(0,0,0);
	#endif//_ENABLE_DEBUG
	FLOAT3 color = FLOAT3(0,0,0);

#if defined(_ADD_LIGHT)&&!defined(SHADER_API_MOBILE)
	////////////////////static light////////////////////
	UNITY_BRANCH
	if(_StaticLightCount>0)
	{
			color += VoxelLighting(
				FragData,
				MaterialData,
				shadowMask,
				_StaticLightParam,
				TEX2D_PARAM(_StaticLightTex)
			#ifdef _ENABLE_DEBUG
					,ld
			#endif
				DEBUG_PARAM)* _PointLightIntensity;
	}

	////////////////////simple light////////////////////
	//#if defined(_SIMPLE_ADD_LIGHT)
	//	UNITY_BRANCH
	//	if (_SimpleLightEnable)
	//	{
	//		color += SimplePointLightingWithSpec(FragData, MaterialData, shadowMask, _SimpleLightIndex0
	//#ifdef _ENABLE_DEBUG
	//			, ld
	//#endif
	//		);
	//		color += SimplePointLighting(FragData, MaterialData, shadowMask, _SimpleLightIndex1
	//#ifdef _ENABLE_DEBUG
	//			, ld
	//#endif
	//		);
	//	}
	//#endif//_SIMPLE_ADD_LIGHT
#endif//_ADD_LIGHT

	return color;
}


#endif //PBS_ADDLIGHTING_INCLUDE
