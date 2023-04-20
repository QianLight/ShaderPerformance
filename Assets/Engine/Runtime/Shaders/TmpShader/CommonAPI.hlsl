#ifndef COMMON_API_INCLUDDED
#define COMMON_API_INCLUDDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonCF.hlsl"

#if SHADER_API_MOBILE || SHADER_API_GLES || SHADER_API_GLES3
#pragma warning (disable : 3205) // conversion of larger type to smaller
#endif


#ifndef UNITY_UV_STARTS_AT_TOP
#define UNITY_UV_STARTS_AT_TOP 1
#endif

#ifndef UNITY_REVERSED_Z
#define UNITY_REVERSED_Z 1
#endif

#define UNITY_GATHER_SUPPORTED (SHADER_TARGET >= 40)

SamplerState global_point_clamp_sampler;
SamplerState global_linear_clamp_sampler;

SamplerState global_point_repeat_sampler;
SamplerState global_linear_repeat_sampler;

#define SharedPointClampState global_point_clamp_sampler
#define SharedLinearClampState global_linear_clamp_sampler

#define SharedPointRepeatState global_point_repeat_sampler
#define SharedLinearRepeatState global_linear_repeat_sampler

#define TEX_SAMPLER(name) SamplerState sampler##name
//2D
#define TEX2D(name) Texture2D name
#define TEX2D_SAMPLER(name) Texture2D name; TEX_SAMPLER(name)
#define TEX2D_ST(name) FLOAT4 name##_ST
#define TEX2D_TRANSFORM(tex,name) (tex.xy * name##_ST.xy + name##_ST.zw)
#define TEX2D_SIZE(name) FLOAT4 name##_TexelSize
#define GET_TEX2D_SIZE(name) (name##_TexelSize)

#define TEX2D_ARGS(name) Texture2D name, TEX_SAMPLER(name)
#define TEX2D_PARAM(name) name,sampler##name

#define SAMPLE_TEX2D(name, coord2) name.Sample(sampler##name, coord2)
#define SAMPLE_TEX2D_LOD(name, coord2, lod) name.SampleLevel(sampler##name, coord2, lod)

#define SAMPLE_TEX2D_LINEAR_CLAMP(name, coord2) name.Sample(SharedLinearClampState, coord2)
#define SAMPLE_TEX2D_POINT_CLAMP(name, coord2) name.Sample(SharedPointClampState, coord2)
#define SAMPLE_TEX2D_LINEAR_CLAMP_LOD(name, coord2,lod) name.SampleLevel(SharedLinearClampState, coord2, lod)
#define SAMPLE_TEX2D_POINT_CLAMP_LOD(name, coord2,lod) name.SampleLevel(SharedPointClampState, coord2, lod)

#define SAMPLE_TEX2D_LINEAR_REPEAT(name, coord2) name.Sample(SharedLinearRepeatState, coord2)
#define SAMPLE_TEX2D_POINT_REPEAT(name, coord2) name.Sample(SharedPointRepeatState, coord2)

#define LOAD_TEX2D(name, coord2) name.Load(int3(coord2, 0))

#define TEX2D_SAMPLER_EXT(name) sampler2D name
#define TEX2D_ARGS_EXT(name) sampler2D name
#define TEX2D_PARAM_EXT(name) name

#define SAMPLE_TEX2D_EXT(name, coord2) tex2D(name, coord2)
#define SAMPLE_TEX2D_LOD_EXT(name, coord2, lod) tex2Dlod(name, FLOAT4(coord2, 0.0, lod))
#define SAMPLE_TEX2D_PROJ_EXT(name,coord4)  tex2Dproj(name, coord4)

//cube
#define TEXCUBE_SAMPLER(name) TextureCube name;TEX_SAMPLER(name)
#define SAMPLE_TEXCUBE(name, coord3) name.Sample(sampler##name, coord3)
#define SAMPLE_TEXCUBE_LOD(name,coord3,lod)  name.SampleLevel(sampler##name, coord3, lod)

//2darray
#define TEX2DARRAY_SAMPLER(name) Texture2DArray name; TEX_SAMPLER(name)
#define SAMPLE_TEX2DARRAY(name,coord2,index)  name.Sample (sampler##name,FLOAT3(coord2,index))
#define SAMPLE_TEX2DARRAY_LOD(name,coord2,index,lod)  name.SampleLevel (sampler##name,FLOAT4(coord2,index,lod))

//3d
#define TEX3D_SAMPLER(name) Texture3D name; TEX_SAMPLER(name)
#define SAMPLE_TEX3D(name, coord3) name.Sample(sampler##name, coord3)

//gather
#ifdef UNITY_GATHER_SUPPORTED
#define GATHER_TEX2D(name, coord2) name.Gather(sampler##name, coord2)
#define GATHER_TEX2D_POINT_CLAMP(name, coord2) name.Gather(SharedPointClampState, coord2)
#define GATHER_TEX2D_RED(name,coord2) name.GatherRed (sampler##name,coord2)
#else
#define GATHER_TEX2D(name, coord2) 0
#define GATHER_TEX2D_POINT_CLAMP(name, coord2) 0
#define GATHER_TEX2D_RED(name,coord2) 0
#endif

//shadow
#define SAMPLE_DEPTH(name, coord2) SAMPLE_TEX2D_LINEAR_CLAMP(name, coord2).r
#define SAMPLE_DEPTH_LOD(name, coord2, lod) SAMPLE_TEX2D_LINEAR_CLAMP_LOD(textureName, coord2, lod).r
#define SAMPLE_DEPTH_POINT(name, coord2) SAMPLE_TEX2D_POINT_CLAMP(name, coord2).r
#define SAMPLE_DEPTH_POINT_LOD(name, coord2, lod) SAMPLE_TEX2D_POINT_CLAMP_LOD(textureName, coord2, lod).r

#define TEX2D_SHADOWMAP(name) Texture2D name; SamplerComparisonState sampler##name

#define SAMPLE_SHADOW(name,coord2) name.Sample (SharedLinearClampState,coord2)
#define SAMPLE_SHADOW_POINT(name,coord2) name.Sample (SharedPointClampState,coord2)
#define SAMPLE_SHADOW_CMP(name,coord3) name.SampleCmpLevelZero (sampler##name,(coord3).xy,(coord3).z)
#define SAMPLE_SHADOW_POINT(name,coord2) name.Sample (SharedPointClampState,coord2)

#define TEX2DARRAY_SHADOWMAP_SAMPLER(name) Texture2DArray name; SamplerComparisonState sampler##name
#define TEX2DARRAY_ARGS_SHADOW(name) Texture2DArray name, SamplerComparisonState sampler##name
#define TEX2D_ARGS_SHADOW(name) Texture2D name, SamplerComparisonState sampler##name
#define SAMPLE_SHADOWMAP_TEX2DARRAR(name,coord3) name.Sample (SharedLinearClampState,coord3)
#define SAMPLE_SHADOWMAP_TEX2DARRAR_CMP(name,coord4) name.SampleCmpLevelZero (sampler##name,(coord4).xyz,(coord4).w)
#define SAMPLE_SHADOWMAP_TEX2DARRAR_POINT(name,coord3) name.Sample (SharedPointClampState,coord3)

#ifndef UNITY_BRANCH
#define UNITY_BRANCH    [branch]
#endif

/*
#define UNITY_FLATTEN   [flatten]
#define UNITY_UNROLL    [unroll]
#define UNITY_LOOP      [loop]
#define UNITY_FASTOPT   [fastopt]
*/
#define CBUFFER_START(name) cbuffer name {
#define CBUFFER_END };


#define IBUFFER_START(name) CBUFFER_START(UnityInstancing_##name) struct{
#define IBUFFER_END(name,count) }name[count]; CBUFFER_END

#define INITIALIZE_OUTPUT(type,name) name = (type)0;
#define DECLARE_OUTPUT(type,name) type name;INITIALIZE_OUTPUT(type,name)


// #if defined(SHADER_STAGE_FRAGMENT)||defined(SHADER_STAGE_COMPUTE)
// #define FLOAT		float
// #define FLOAT2		float2
// #define FLOAT3		float3
// #define FLOAT4		float4
// #define FLOAT2x2	half2x2
// #define FLOAT3x3	half3x3
// #define FLOAT4x4	half4x4
// #else
// #define FLOAT		float
// #define FLOAT2		float2
// #define FLOAT3		float3
// #define FLOAT4		float4
// #define FLOAT2x2	float2x2
// #define FLOAT3x3	float3x3
// #define FLOAT4x4	float4x4
// #endif

#ifndef REAL

#if defined(SHADER_API_MOBILE) || defined(SHADER_API_SWITCH)
#define HAS_HALF 1
#else
#define HAS_HALF 0
#endif

#if HAS_HALF
#define REAL_IS_HALF 1
#else
#define REAL_IS_HALF 0
#endif // Do we have half?

#if REAL_IS_HALF
#define REAL		half
#define REAL2		half2
#define REAL3		half3
#define REAL4		half4
#define REAL2x2     half2x2
#define REAL3x3     half3x3
#define REAL4x4     half4x4

#define FLOAT		half
#define FLOAT2		half2
#define FLOAT3		half3
#define FLOAT4		half4
#define FLOAT2x2	half2x2
#define FLOAT3x3	half3x3
#define FLOAT4x4	half4x4

#else

#define REAL		float
#define REAL2		float2
#define REAL3		float3
#define REAL4		float4
#define REAL2x2     float2x2
#define REAL3x3     float3x3
#define REAL4x4     float4x4

#define FLOAT		float
#define FLOAT2		float2
#define FLOAT3		float3
#define FLOAT4		float4
#define FLOAT2x2	float2x2
#define FLOAT3x3	float3x3
#define FLOAT4x4	float4x4

#endif
#endif

#endif // #ifndef real