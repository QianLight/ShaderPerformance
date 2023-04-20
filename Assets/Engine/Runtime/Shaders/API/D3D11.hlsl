#include "CommonAPI.hlsl" 
#define INSTANCING_CBUFFER_BEGIN(name)  CBUFFER_START(name)
#define INSTANCING_CBUFFER_END          CBUFFER_END
// #define UNITY_UV_STARTS_AT_TOP 1
// #define UNITY_REVERSED_Z 1
// #define UNITY_GATHER_SUPPORTED (SHADER_TARGET >= 50)

// SamplerState global_point_clamp_sampler;
// SamplerState global_linear_clamp_sampler;

// #define SharedPointClampState global_point_clamp_sampler
// #define SharedLinearClampState global_linear_clamp_sampler

// #define TEX2D_SAMPLER(textureName) sampler2D textureName
// #define TEXTURE2D_SAMPLER2D_ST(textureName) FLOAT4 textureName##_ST
// #define TRANSFORM_TEX(tex,name) (tex.xy * name##_ST.xy + name##_ST.zw)

// #define TEXTURE2D_SAMPLER2D_2(textureName) Texture2D textureName; SamplerState sampler##textureName
// #define TEXTURE2D_2(textureName) Texture2D textureName
// #define TEXCUBE_SAMPLER(textureName) samplerCUBE textureName

// #define TEX2DARRAY_SAMPLER(textureName) Texture2DArray textureName; SamplerState sampler##textureName

// #define TEX2D_ARGS(textureName) sampler2D textureName
// #define TEX2D_PARAM(textureName) textureName

// #define SAMPLE_TEX2D(textureName, coord2) tex2D(textureName, coord2)
// #define SAMPLE_TEXTURE2D_STATE(textureName, coord2) textureName.Sample(sampler##textureName, coord2)
// #define SAMPLE_TEXTURE2D_STATE2(textureName,samplerName, coord2) textureName.Sample(samplerName, coord2)
// #define SAMPLE_TEXTURE2D_STATE_LOD(textureName,samplerName, coord2, lod) textureName.SampleLevel(samplerName, coord2, lod)
// #define SAMPLE_TEXTURE2D_POINT(textureName, coord2) textureName.Sample(SharedPointClampState, coord2)
// #define SAMPLE_TEXTURE2D_LINEAR(textureName, coord2) textureName.Sample(SharedLinearClampState, coord2)

// #define SAMPLE_TEXTURE2DDDXY(textureName, coord2,dx,dy) tex2D(textureName, coord2,dx,dy)
// #define SAMPLE_TEX2D_LOD(textureName, coord2, lod) tex2Dlod(textureName, FLOAT4(coord2, 0.0, lod))
// // #define TEX2D(textureName, coord2) tex2D(textureName, coord2)
// #define SAMPLE_TEXPROJ(textureName,coord4)  tex2Dproj(textureName, coord4)

// #define SAMPLE_TEXCUBE(textureName,coord2)  texCUBE(textureName, coord2)
// #define SAMPLE_TEXCUBE_LOD(textureName,coord,lod) texCUBElod (textureName,FLOAT4(coord, lod))
// #define SAMPLE_TEX2DARRAY(textureName,coord)  textureName.Sample (sampler##textureName,coord)

// #define TEXTURE2D_GATHER(textureName) TEXTURE2D_SAMPLER2D_2(textureName)

// #define GATHER_TEXTURE2D(textureName, coord2) textureName.Gather(sampler##textureName, coord2)
// #define GATHER_TEXTURE2D_STATE(textureName,samplerName, coord2) textureName.Gather(samplerName, coord2)

// #define GATHER_RED(textureName,coord) textureName.GatherRed (sampler##textureName,coord)

// #define SAMPLE_DEPTH(textureName, coord2) SAMPLE_TEX2D(textureName, coord2).r
// #define SAMPLE_DEPTH_TEXTURE_LOD(textureName, coord2, lod) SAMPLE_TEX2D_LOD(textureName, coord2, lod).r
// #define SAMPLE_DEPTH(textureName, coord2)  textureName.Sample (sampler##textureName,coord2).r

// #define TEX2D_SHADOWMAP(tex) Texture2D tex; SamplerComparisonState sampler##tex
// #define SAMPLE_SHADOW(tex,coord) tex.Sample (SharedLinearClampState,(coord).xy)
// #define SAMPLE_SHADOW_CMP(tex,coord) tex.SampleCmpLevelZero (sampler##tex,(coord).xy,(coord).z)
// #define SAMPLE_SHADOW_POINT(tex,coord) tex.Sample (SharedPointClampState,(coord).xy)

// #define TEX2DARRAY_SHADOWMAP_SAMPLER(tex) Texture2DArray tex; SamplerComparisonState sampler##tex
// #define SAMPLE_SHADOWMAP_TEX2DARRAR(tex,coord) tex.Sample (SharedLinearClampState,(coord).xyz)
// #define SAMPLE_SHADOW_CMP(tex,coord) tex.SampleCmpLevelZero (sampler##tex,(coord).xyz,(coord).w)
// #define SAMPLE_SHADOW_POINT(tex,coord) tex.Sample (SharedPointClampState,(coord).xyz)

// #define UNITY_BRANCH    [branch]
// #define UNITY_FLATTEN   [flatten]
// #define UNITY_UNROLL    [unroll]
// #define UNITY_LOOP      [loop]
// #define UNITY_FASTOPT   [fastopt]

// #define CBUFFER_START(name) cbuffer name {
// #define CBUFFER_END };

// #if UNITY_GATHER_SUPPORTED
//     #define FXAA_HLSL_5 1
//     #define SMAA_HLSL_4_1 1
// #else
//     #define FXAA_HLSL_4 1
//     #define SMAA_HLSL_4 1
// #endif
