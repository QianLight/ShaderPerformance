
#ifndef PBS_DEBUGHEAD_INCLUDE
#define PBS_DEBUGHEAD_INCLUDE
#include "CommonDebug.hlsl"
#include "LightingHead.hlsl"

#ifdef _ENABLE_DEBUG
struct FCustomData
{
	FLOAT3 GI;
	FLOAT3 DiffuseGI;
	FLOAT3 AddGI;
	FLOAT AddShadow;

	FLOAT ShadowMask;
	FLOAT3 DirectDiffuse;
	FLOAT3 DirectSpecular;
	FLOAT3 Specular;	
	FLOAT3 Shadow;
	FLOAT SceneShadow;
	FLOAT SimpleShadow;
	FLOAT ExtraShadow;
	FLOAT ExtraShadow1;
	FLOAT SelfShadow;
	FLOAT ShadowMapIndex;
	FLOAT3 ShadowColor;
	FLOAT LightmapShadow;

	FLOAT3 DirectDiffuseSupplement;
	FLOAT3 DirectSpecularSupplement;
	FLOAT SupplementMask;	
	FLOAT3 CombineColor;
	
	FLOAT3 AddDiffuse;
	FLOAT3 AddSpecular;
	FLOAT4 AddIndex;
	FLOAT3 AddDynamicDiffuse;
	FLOAT3 AddDynamicSpecular;
	FLOAT3 AddColor;

	FLOAT3 SH;
	FLOAT3 Ambient;
	FLOAT3 ReflectionVector;
	FLOAT CubeMipmap;
	FLOAT3 IndirectSpecular;
	FLOAT IBLFresnel;
	FLOAT3 IBLScale;
	FLOAT3 IBL;
	FLOAT3 LightOutput;

	FLightingContext LC0;
	FLightingContext LC1;
	FLOAT3 PostCustomLighting;
	FLOAT3 FogColor;
	LightDebug addLightDebug;

	FLOAT SDF;
	FLOAT3 Foam;
	FLOAT3 Subsurface;
	FLOAT SceneZRefract;
	FLOAT3 WaterAlpha;
	FLOAT3 SkyCol;
	FLOAT KajiyaShift;
	FLOAT3 Rim;
	FLOAT3 Ramp;
	FLOAT DarkRim;
};

FLOAT _GlobalDebugMode;
#define DEBUG_PBS_COLOR(OutColor, FragData, MaterialData) OutColor = DebugOutputColor(OutColor,FragData, MaterialData,CustomData,SvPosition.xy);
#else
#define DEBUG_PBS_COLOR(OutColor, FragData, MaterialData)
#endif//_ENABLE_DEBUG

#endif //PBS_DEBUG_INCLUDE