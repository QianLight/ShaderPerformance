// Copyright 2018- PWRD, Inc. All Rights Reserved.

#ifndef PBS_LIGHTINGHEAD_INCLUDE
#define PBS_LIGHTINGHEAD_INCLUDE

#define _DynamicLightColor (li.lightColor.xyz)
#define _DynamicLightInvSqrRange (li.lightColor.w)
#define _DynamicLightPos (li.lightPos.xyz)
#define _DynamicLightCoverage (li.lightPos.w)
#define _DynamicLightAmbient (li.lightParam.xyz)
#define _DynamicLightInvSoftness (li.lightParam.w)

#define CELL_DYNAMIC_LIGHT_COUNT 4

struct LightInfo
{
	FLOAT4 lightPos;//xyz:world pos w:coverage
	FLOAT4 lightColor;//xyz:color w: invSqrRange
	FLOAT4 lightParam;//x:hardness y:offset zw:null
    // FLOAT lightRangSqrInv;
};

#ifdef _ENABLE_DEBUG
struct LightDebug
{
	FLOAT3 Diffuse;
	FLOAT3 Spec;
	FLOAT3 dynamicDiffuse;
	FLOAT3 dynamicSpec;
	FLOAT softness;
	FLOAT ramp;
	FLOAT atten;
	FLOAT toonAO;
	FLOAT3 ambient;
	FLOAT3 lightColor;
	FLOAT3 result;
};
#endif
//===========================Static Light Param===========================
FLOAT4 _VoxelLightParam;
#define _GridSize (_VoxelLightParam.x)

FLOAT4 _StaticLightParam;
#define _StaticLightCount (uint)(_StaticLightParam.x)
#define _StaticLightGridXZ (_StaticLightParam.yz)

TEX2D_SAMPLER(_StaticLightTex);
StructuredBuffer<LightInfo> _StaticLightInfos;

//===========================Simple Light Param===========================
FLOAT4 _SimpleLightParam;
#define _SimpleLightEnable _SimpleLightParam.y>0.5
#define _SimpleLightIndex0 _SimpleLightParam.z
#define _SimpleLightIndex1 _SimpleLightParam.w


FLOAT4 _DyanmicPointLightPos[8];//xyz:world pos w:range or bias
FLOAT4 _DyanmicPointLightColor[8];//xyz:color w: oneOverLightRangeSqr 
FLOAT4 _DyanmicPointLightParam[8];//x:hardness y:offset zw:none

//===========================Global Light Param===========================
FLOAT4 _GlobalLightParam;
#define _PointLightIntensity _GlobalLightParam.x
#define _SpecAdd	 _GlobalLightParam.y

REAL4 _MainLightDir0;
#define _SceneLightDir _MainLightDir0

FLOAT4 _MainLightColor0;
#define _SceneLightColor _MainLightColor0

REAL4 _AddLightDir0;
FLOAT4 _AddLightColor0;
#define _AddLightDir _AddLightDir0
#define _AddLightColor _AddLightColor0


#ifdef _Role_Lighting

	REAL4 _MainLightDir1;
	#define _MainLightDir _MainLightDir1
	FLOAT4 _MainLightRoleColorV2;
	#define _MainLightColor _MainLightRoleColorV2
	#define _AddLightEnable _AddLightColor0.w>0.5
#else//!_Role_Lighting

	#define _MainLightDir _MainLightDir0
	#define _MainLightColor _MainLightColor0

	#define _AddLightEnable _AddLightDir.w>0.01
#endif//_Role_Lighting

FLOAT4 _WaterLightDir;
#endif //PBS_LIGHTINGHEAD_INCLUDE