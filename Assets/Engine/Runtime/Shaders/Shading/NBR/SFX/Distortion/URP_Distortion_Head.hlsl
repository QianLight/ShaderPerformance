#include "../../Include/PCH.hlsl"
#include "../../StdLib.hlsl"

TEX2D_SAMPLER(_CameraDepthTexture);
// #define _DISTORTION_DEPTH_CULL  depth01 = Interpolants.TangentToWorld0.w;\					
// 					FLOAT2 uv = Interpolants.TexCoords[0].xy;\
// 					FLOAT depth = SAMPLE_DEPTH(_CameraDepthRT,uv);\
// 					FLOAT zCull = depth > depth01;