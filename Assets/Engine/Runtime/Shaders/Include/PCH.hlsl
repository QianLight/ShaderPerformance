#include "../StdLib.hlsl"
#include "../Colors.hlsl"
#include "Common.hlsl"
#include "Head.hlsl"
#include "DefaultDebugHead.hlsl"
#include "DebugVertex.hlsl"
#include "LightingHead.hlsl"
#include "UniformPCH.hlsl"
#include "SH.hlsl"

#ifndef PBS_PCH_INCLUDE
#define PBS_PCH_INCLUDE

#if defined(_ALPHA_BLEND)&&!defined(_ALPHA_TEST)&&!defined(_CUSTOM_MRT)
#define _NO_MRT
#endif

#define _PBR_Tex _ProcedureTex0

#define _DynamicLightEnable _Param.x>0.5
#define _LightMapEnable _Param.y>0.5
// #define _RoleSceneShadowEnable _Param.y>0.5
#define _ExtraShadowEnable _Param.z>0.5
#define _SelfShadowEnable _Param.w>0.5

#define _SelfShadowValue _Param.w
#define _SelfShadow2Value _Param.w
// #define _EnableShadow _Param.w<0.5

//_Param0
#define _AOBias _Param0.x
#define _NormalScale _Param0.y//trick normal
//_Param0.z variant
#define _RMAdjust _Param0.z
#define _MetallicScale _Param0.w

//_Param1
#define _SpecularScale _Param1.x

//_Param1.y variant
#define _IBLScale _Param1.z
#define _Cutout _Param1.w
#define _IBLEnable _IBLScale>0.02

//_PBRParam
#define _ParamRoughnessMin _PBRParam.x
#define _ParamRoughnessMax _PBRParam.y
#define _ParamMetallicMin _PBRParam.z
#define _ParamMetallicMax _PBRParam.w
#define _SpecK (_PBRParam.z)
#define _SpecB (_PBRParam.w)

FLOAT4 _AmbientParam1;//xyz hdr decode w maxMip
//#define _MaxMip _AmbientParam1.w
#define _MaxMip 6
#define _AmbientLightScale _AmbientParam1.y
#define _AmbientDarkScale _AmbientParam1.z
#define _ContrastLight _AmbientParam1.w
// #define _EnvAvgOffset _EnvCubeParam.z

FLOAT4 _AmbientParam2;
#define _HDRParam _AmbientParam2.xyz
#define _ContrastInsity _AmbientParam2.w

#define _ViewZ FragData.WorldPosition.w


#ifdef _WORLD_UV_OFFSET
FLOAT4 _ChunkOffset;
#endif//_WORLDSPACE_UV

FLOAT4 _WorldSize;
#define _WorldSizeInv _WorldSize.zw

// Bloom Param

#define _BloomScale 1//(_BloomMaskParam.x)

FLOAT4 _SceneColor;
FLOAT4 _LocalOffset;
TEX2D_SAMPLER(_GlobalNoise);

#include "ShadowLib.hlsl"
#include "ShadingMode.hlsl"
#include "Instance.hlsl"
#endif //PBS_PCH_INCLUDE