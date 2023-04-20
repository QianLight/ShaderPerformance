#ifndef ENVIROMENT_SH_COMMON_INCLUDED
#define ENVIROMENT_SH_COMMON_INCLUDED

#ifndef real

// The including shader should define whether half
// precision is suitable for its needs.  The shader
// API (for now) can indicate whether half is possible.
#ifdef SHADER_API_MOBILE
#define HAS_HALF 0
#else
#define HAS_HALF 0
#endif

#ifndef PREFER_HALF
#define PREFER_HALF 1
#endif

#if HAS_HALF && PREFER_HALF
#define REAL_IS_HALF 1
#else
#define REAL_IS_HALF 0
#endif // Do we have half?

#if REAL_IS_HALF
#define real half
#define real2 half2
#define real3 half3
#define real4 half4

#define real2x2 half2x2
#define real2x3 half2x3
#define real3x2 half3x2
#define real3x3 half3x3
#define real3x4 half3x4
#define real4x3 half4x3
#define real4x4 half4x4

#define half min16float
#define half2 min16float2
#define half3 min16float3
#define half4 min16float4

#define half2x2 min16float2x2
#define half2x3 min16float2x3
#define half3x2 min16float3x2
#define half3x3 min16float3x3
#define half3x4 min16float3x4
#define half4x3 min16float4x3
#define half4x4 min16float4x4

#define REAL_MIN HALF_MIN
#define REAL_MAX HALF_MAX
#define REAL_EPS HALF_EPS
#define TEMPLATE_1_REAL TEMPLATE_1_HALF
#define TEMPLATE_2_REAL TEMPLATE_2_HALF
#define TEMPLATE_3_REAL TEMPLATE_3_HALF

#else

#define real float
#define real2 float2
#define real3 float3
#define real4 float4

#define real2x2 float2x2
#define real2x3 float2x3
#define real3x2 float3x2
#define real3x3 float3x3
#define real3x4 float3x4
#define real4x3 float4x3
#define real4x4 float4x4

#define REAL_MIN FLT_MIN
#define REAL_MAX FLT_MAX
#define REAL_EPS FLT_EPS
#define TEMPLATE_1_REAL TEMPLATE_1_FLT
#define TEMPLATE_2_REAL TEMPLATE_2_FLT
#define TEMPLATE_3_REAL TEMPLATE_3_FLT

#endif // REAL_IS_HALF

#endif // #ifndef real

#ifdef  SHADER_STAGE_COMPUTE
#   ifndef SHADER_TARGET
#       if defined(SHADER_API_METAL)
#       define SHADER_TARGET 45
#       else
#       define SHADER_TARGET 50
#       endif
#   endif
#endif

#define sqrtPI (sqrt(PI))
#define PI          3.14159265358979323846
#define TWO_PI      6.28318530717958647693
#define FOUR_PI     12.5663706143591729538

#if (SHADER_TARGET >= 45)
uint FastLog2(uint x)
{
	return firstbithigh(x);
}
#endif

real3 SphericalToCartesian(real phi, real cosTheta)
{
	real sinPhi, cosPhi;
	sincos(phi, sinPhi, cosPhi);

	real sinTheta = sqrt(saturate(1.0 - cosTheta * cosTheta));

	return real3(sinTheta * cosPhi, sinTheta * sinPhi, cosTheta);
}

real3 SampleSphereUniform(real u1, real u2)
{
	real phi = TWO_PI * u2;
	real cosTheta = 1.0 - 2.0 * u1;

	return SphericalToCartesian(phi, cosTheta);
}



#endif