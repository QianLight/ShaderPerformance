// Because this framework is supposed to work with the legacy render pipelines AND scriptable render
// pipelines we can't use Unity's shader libraries (some scriptable pipelines come with their own
// shader lib). So here goes a minimal shader lib only used for post-processing to ensure good
// compatibility with all pipelines.

#ifndef UNITY_POSTFX_STDLIB
#define UNITY_POSTFX_STDLIB

// -----------------------------------------------------------------------------
// API macros

#if defined(SHADER_API_D3D11)
    #include "./API/D3D11.hlsl"    
// #elif defined(SHADER_API_D3D12)
//     #include "API/D3D12.hlsl"
#elif defined(SHADER_API_VULKAN) || defined(SHADER_API_SWITCH)
    #include "./API/Vulkan.hlsl"
#elif defined(SHADER_API_METAL)
    #include "./API/Metal.hlsl"
#else
    #include "./API/OpenGL.hlsl"
    #define ANDROID
#endif

// -----------------------------------------------------------------------------
// Constants

static const FLOAT4 ONES =  FLOAT4(1.0, 1.0, 1.0, 1.0);
static const FLOAT4 ZEROES = (FLOAT4)0.0;
#define HALF_MAX        65504.0
#define EPSILON         1.0e-4
#ifndef PI
    #define PI              3.14159265359
#endif
#ifndef TWO_PI
    #define TWO_PI          6.28318530718
#endif
#ifndef FOUR_PI
    #define FOUR_PI         12.56637061436
#endif
#ifndef INV_PI
    #define INV_PI          0.31830988618
#endif
#ifndef INV_TWO_PI
    #define INV_TWO_PI      0.15915494309
#endif
#ifndef INV_FOUR_PI
    #define INV_FOUR_PI     0.07957747155
#endif
#ifndef HALF_PI
    #define HALF_PI         1.57079632679
#endif
#ifndef INV_HALF_PI
    #define INV_HALF_PI     0.636619772367
#endif
#define HALF_MIN 6.103515625e-5

#define FLT_EPSILON     1.192092896e-07 // Smallest positive number, such that 1.0 + FLT_EPSILON != 1.0
#define FLT_MIN         1.175494351e-38 // Minimum representable positive FLOATing-point number
#define FLT_MAX         3.402823466e+38 // Maximum representable FLOATing-point number

// #define unity_ColorSpaceDielectricSpec FLOAT4(0.04, 0.04, 0.04, 1.0 - 0.04)
// #define oneMinusDielectricSpec unity_ColorSpaceDielectricSpec.a
#define int2float  (1.0/255)
#define float2int  (255)

#define s2(a, b)				temp = a; a = min(a, b); b = max(temp, b);
#define mn3(a, b, c)			s2(a, b); s2(a, c);
#define mx3(a, b, c)			s2(b, c); s2(a, c);

#define mnmx3(a, b, c)				mx3(a, b, c); s2(a, b);                                   // 3 exchanges
#define mnmx4(a, b, c, d)			s2(a, b); s2(c, d); s2(a, c); s2(b, d);                   // 4 exchanges
#define mnmx5(a, b, c, d, e)		s2(a, b); s2(c, d); mn3(a, c, e); mx3(b, d, e);           // 6 exchanges
#define mnmx6(a, b, c, d, e, f) 	s2(a, d); s2(b, e); s2(c, f); mn3(a, b, c); mx3(d, e, f); // 7 exchanges

#ifndef kDieletricSpec
    #define kDieletricSpec FLOAT4(0.04, 0.04, 0.04, 1.0 - 0.04)
#endif
// -----------------------------------------------------------------------------
// Std unity data

//per view
#ifndef _CGINC

CBUFFER_START(UnityPerFrame)
#if !defined(REDEFINE_URP)
    FLOAT4x4 glstate_matrix_projection;
    FLOAT4x4 unity_MatrixV;
    FLOAT4x4 unity_MatrixInvV;
    FLOAT4x4 unity_MatrixVP;
    FLOAT4x4 unity_MatrixInvP;
    FLOAT4 _Time;                     // x: t/20,           y: t,        z: t*2,       w: t*3
    FLOAT4 _SinTime;                  // x: sin(t/20),      y: sin(t),   z: sin(t*2),  w: sin(t*3)
    FLOAT4 _CosTime;                  // x: cos(t/20),      y: cos(t),   z: cos(t*2),  w: cos(t*3)
    FLOAT4 unity_DeltaTime;           // x: dt,             y: 1/dt,     z: smoothDt,  w: 1/smoothDt
    FLOAT4 _DeltaTime;
    FLOAT4 _TimeParameters;
#endif
CBUFFER_END

CBUFFER_START(UnityPerCamera)
#if !defined(REDEFINE_URP)
    FLOAT4 _WorldSpaceCameraPos;
    FLOAT4 _ProjectionParams;         // x: 1 (-1 flipped), y: near,     z: far,       w: 1/far
    FLOAT4 _ScreenParams;             // x: width,          y: height,   z: 1+1/width, w: 1+1/height
    FLOAT4 _ZBufferParams;            // x: 1-far/near,     y: far/near, z: x/far,     w: y/far
    FLOAT4x4 _InvCameraViewProj;

    // or in case of a reversed depth buffer (UNITY_REVERSED_Z is 1)
    // x = -1+far/near
    // y = 1
    // z = x/far
    // w = 1/far
    FLOAT4 unity_OrthoParams;         // x: width,          y: height,   z: unused,    w: ortho ? 1 : 0
    FLOAT4 _GameViewWorldSpaceCameraPos;
#endif
CBUFFER_END

CBUFFER_START(UnityPerCameraRare)
#if !defined(REDEFINE_URP)
    FLOAT4x4 unity_CameraProjection;
    FLOAT4x4 unity_CameraInvProjection;
    FLOAT4x4 unity_WorldToCamera;
    FLOAT4x4 unity_CameraToWorld;
    float4 _CameraBackward;
#endif
CBUFFER_END

CBUFFER_START(UnityPerDraw : register(b0))
#if !defined(REDEFINE_URP)
    REAL4x4 unity_ObjectToWorld;
    REAL4x4 unity_WorldToObject;
    REAL4 unity_LODFade;
    REAL4 unity_WorldTransformParams;

    REAL4 unity_LightData;
    REAL4 unity_LightIndices[2];

    REAL4 unity_ProbesOcclusion;

    // Reflection Probe 0 block feature
    // HDR environment map decode instructions
    REAL4 unity_SpecCube0_HDR;

    // Lightmap block feature
    REAL4 unity_LightmapST;
    REAL4 unity_DynamicLightmapST;

    // SH block feature
    REAL4 unity_SHAr;
    REAL4 unity_SHAg;
    REAL4 unity_SHAb;
    REAL4 unity_SHBr;
    REAL4 unity_SHBg;
    REAL4 unity_SHBb;
    REAL4 unity_SHC;
#endif
CBUFFER_END

// FLOAT4 unity_AmbientSky;
// FLOAT4 unity_AmbientEquator;
// FLOAT4 unity_AmbientGround;
// FLOAT4 unity_IndirectSpecColor;
// FLOAT4 unity_ColorSpaceLuminance;

#endif//_CGINC



//per draw

FLOAT4x4 custom_ObjectToWorld;
FLOAT4x4 custom_WorldToObject;
FLOAT4x4 custom_MatrixV;
FLOAT4x4 custom_MatrixP;
#ifdef _LOCAL_WORLD_OFFSET

    FLOAT4x4 custom_MatrixVP;
    FLOAT4x4 custom_MatrixITMV;
    #define _objectToWorld custom_ObjectToWorld
    #define _worldToObject custom_WorldToObject
    #define _matrixV custom_MatrixV
    #define _matrixVP custom_MatrixVP
    #define _matrixP custom_MatrixP   
    #define _CameraPos FLOAT3(0,0,0)
    #define _MatrixITMV custom_MatrixITMV     
#else//!_LOCAL_WORLD_OFFSET
    #define _objectToWorld unity_ObjectToWorld
    #define _worldToObject unity_WorldToObject
    #define _matrixV unity_MatrixV
    #define _matrixVP unity_MatrixVP
    #define _matrixP glstate_matrix_projection
    #define _CameraPos _WorldSpaceCameraPos.xyz
    static FLOAT4x4 _MatrixITMV = transpose(mul(unity_WorldToObject, unity_MatrixInvV));
#endif//_LOCAL_WORLD_OFFSET

// -----------------------------------------------------------------------------
// Compatibility functions

#if (SHADER_TARGET < 50 && !defined(SHADER_API_PSSL))
inline FLOAT rcp(FLOAT value)
{
    return 1.0 / value;
}

inline FLOAT2 rcp(FLOAT2 value)
{
    return 1.0 / value;
}

inline FLOAT3 rcp(FLOAT3 value)
{
    return 1.0 / value;
}

inline FLOAT4 rcp(FLOAT4 value)
{
    return 1.0 / value;
}

#endif

#define DECLARE_PROPERTY(structureName, path, propertyName) \
    inline void Set##propertyName(inout structureName root, in FLOAT value) { root.path = value; } \
    inline FLOAT Get##propertyName(in structureName root) { return root.path; }

#define DECLARE_PROPERTY_WITH_TYPE(structureName, type, path, propertyName) \
    inline void Set##propertyName(inout structureName root, in type value) { root.path = value; } \
    inline type Get##propertyName(in structureName root) { return root.path; }

#define DECLARE_INTERPOLANTS_PROPERTY(type, path, propertyName) \
    inline void Set##propertyName(inout FInterpolantsVSToPS Interpolants, in type value) { Interpolants.path = value; } \
    inline type Get##propertyName(in FFragData FragData) { return FragData.path; }

#if defined(SHADER_API_GLES)
    #define mad(a, b, c) ((a) * (b) + (c))
#endif

#define linearstep(min, max, x) saturate((x - min) / (max - min))

// See ReciprocalBasedCurve.cs for details.
#define reciprocalcurve(a, b, c, x) (rcp(mad(x, a, b)) + (c))

#if !defined(INTRINSIC_MINMAX3)
#if !defined(REDEFINE_URP)
inline FLOAT Min3(FLOAT a, FLOAT b, FLOAT c)
{
    return min(min(a, b), c);
}

inline FLOAT2 Min3(FLOAT2 a, FLOAT2 b, FLOAT2 c)
{
    return min(min(a, b), c);
}

inline FLOAT3 Min3(FLOAT3 a, FLOAT3 b, FLOAT3 c)
{
    return min(min(a, b), c);
}

inline FLOAT4 Min3(FLOAT4 a, FLOAT4 b, FLOAT4 c)
{
    return min(min(a, b), c);
}

inline FLOAT Min3(FLOAT3 x) 
{ 
	return min(x.x, min(x.y, x.z)); 
}

inline FLOAT Max3(FLOAT a, FLOAT b, FLOAT c)
{
    return max(max(a, b), c);
}

inline FLOAT2 Max3(FLOAT2 a, FLOAT2 b, FLOAT2 c)
{
    return max(max(a, b), c);
}

inline FLOAT3 Max3(FLOAT3 a, FLOAT3 b, FLOAT3 c)
{
    return max(max(a, b), c);
}

inline FLOAT4 Max3(FLOAT4 a, FLOAT4 b, FLOAT4 c)
{
    return max(max(a, b), c);
}


#endif
inline FLOAT Max3(FLOAT3 x)
{
    return max(x.x, max(x.y, x.z));
}
#endif // INTRINSIC_MINMAX3

// https://twitter.com/SebAaltonen/status/878250919879639040
// madd_sat + madd
inline FLOAT FastSign(FLOAT x)
{
    return saturate(x * FLT_MAX + 0.5) * 2.0 - 1.0;
}

inline FLOAT2 FastSign(FLOAT2 x)
{
    return saturate(x * FLT_MAX + 0.5) * 2.0 - 1.0;
}

inline FLOAT3 FastSign(FLOAT3 x)
{
    return saturate(x * FLT_MAX + 0.5) * 2.0 - 1.0;
}

inline FLOAT4 FastSign(FLOAT4 x)
{
    return saturate(x * FLT_MAX + 0.5) * 2.0 - 1.0;
}
#if !defined(REDEFINE_URP)
// Using pow often result to a warning like this
// "pow(f, e) will not work for negative f, use abs(f) or conditionally handle negative values if you expect them"
// PositivePow remove this warning when you know the value is positive and avoid inf/NAN.
inline FLOAT PositivePow(FLOAT base, FLOAT power)
{
    return pow(abs(base), power);
}

inline FLOAT2 PositivePow(FLOAT2 base, FLOAT2 power)
{
    return pow(max(abs(base), FLOAT2(FLT_EPSILON, FLT_EPSILON)), power);
}

inline FLOAT3 PositivePow(FLOAT3 base, FLOAT3 power)
{
    return pow(max(abs(base), FLOAT3(FLT_EPSILON, FLT_EPSILON, FLT_EPSILON)), power);
}

inline FLOAT4 PositivePow(FLOAT4 base, FLOAT4 power)
{
    return pow(max(abs(base), FLOAT4(FLT_EPSILON, FLT_EPSILON, FLT_EPSILON, FLT_EPSILON)), power);
}
#endif
// NaN checker
// /Gic isn't enabled on fxc so we can't rely on isnan() anymore
inline bool IsNan(FLOAT x)
{
    // For some reason the following tests outputs "internal compiler error" randomly on desktop
    // so we'll use a safer but slightly slower version instead :/
    //return (x <= 0.0 || 0.0 <= x) ? false : true;
    return (x < 0.0 || x > 0.0 || x == 0.0) ? false : true;
}

inline bool AnyIsNan(FLOAT2 x)
{
    return IsNan(x.x) || IsNan(x.y);
}

inline bool AnyIsNan(FLOAT3 x)
{
    return IsNan(x.x) || IsNan(x.y) || IsNan(x.z);
}

inline bool AnyIsNan(FLOAT4 x)
{
    return IsNan(x.x) || IsNan(x.y) || IsNan(x.z) || IsNan(x.w);
}


inline FLOAT Square(FLOAT x)
{
	return x*x;
}

inline FLOAT2 Square(FLOAT2 x)
{
	return x*x;
}

inline FLOAT3 Square(FLOAT3 x)
{
	return x*x;
}

inline FLOAT4 Square(FLOAT4 x)
{
	return x*x;
}
#if !defined(REDEFINE_URP)
inline FLOAT Pow4(FLOAT x)
{
    FLOAT x2 = x*x;
    
    return x2*x2;
}
#endif
inline FLOAT Pow5(FLOAT x)
{
	FLOAT xx = x*x;
	return xx * xx * x;
}

inline FLOAT2 Pow5(FLOAT2 x)
{
	FLOAT2 xx = x*x;
	return xx * xx * x;
}

inline FLOAT3 Pow5(FLOAT3 x)
{
	FLOAT3 xx = x*x;
	return xx * xx * x;
}

inline FLOAT4 Pow5(FLOAT4 x)
{
	FLOAT4 xx = x*x;
	return xx * xx * x;
}
inline FLOAT Pow8(FLOAT x)
{
    FLOAT x2 = x*x;
    FLOAT x4 = x2*x2;

    return x4*x4;
}

inline FLOAT Pow16(FLOAT x)
{
    FLOAT x2 = x*x;
    FLOAT x4 = x2*x2;
    FLOAT x8 = x4*x4;

    return x8*x8;
}
inline FLOAT Pow10(in FLOAT x)
{
	FLOAT x2 = x*x;
	FLOAT x4 = x2*x2;

	return x4*x4*x2;
}

inline FLOAT2 Pow10(in FLOAT2 x)
{
	FLOAT2 x2 = x*x;
	FLOAT2 x4 = x2*x2;

	return x4*x4*x2;
}

inline FLOAT3 Pow10(in FLOAT3 x)
{
	FLOAT3 x2 = x*x;
	FLOAT3 x4 = x2*x2;

	return x4*x4*x2;
}

inline FLOAT4 Pow10(in FLOAT4 x)
{
	FLOAT4 x2 = x*x;
	FLOAT4 x4 = x2*x2;

	return x4*x4*x2;
}

inline FLOAT LerpStep(FLOAT a,FLOAT b,FLOAT t)
{
    FLOAT m = b - a;
    m = (m <= 0.0 ? m : 1e-5);
    return saturate((t - a) * rcp(m));
}

inline FLOAT2 LerpStep(FLOAT2 a,FLOAT2 b,FLOAT2 t)
{
    FLOAT2 m = b - a;
    m = (m <= 0.0 ? m : 1e-5);
    return saturate((t - a) * rcp(m));
}

inline FLOAT3 LerpStep(FLOAT3 a,FLOAT3 b,FLOAT3 t)
{
    FLOAT3 m = b - a;
    m = (m <= 0.0 ? m : 1e-5);
    return saturate((t - a) * rcp(m));
}

inline FLOAT4 LerpStep(FLOAT4 a,FLOAT4 b,FLOAT4 t)
{
    FLOAT4 m = b - a;
    m = (m <= 0.0 ? m : 1e-5);
    return saturate((t - a) * rcp(m));
}

// Clamp the base, so it's never <= 0.0f (INF/NaN).
inline FLOAT ClampedPow(FLOAT X, FLOAT Y)
{
	return pow(max(abs(X), 0.000001f), Y);
}
inline FLOAT2 ClampedPow(FLOAT2 X, FLOAT2 Y)
{
	return pow(max(abs(X), FLOAT2(0.000001f, 0.000001f)), Y);
}
inline FLOAT3 ClampedPow(FLOAT3 X, FLOAT3 Y)
{
	return pow(max(abs(X), FLOAT3(0.000001f, 0.000001f, 0.000001f)), Y);
}
inline FLOAT4 ClampedPow(FLOAT4 X, FLOAT4 Y)
{
	return pow(max(abs(X), FLOAT4(0.000001f, 0.000001f, 0.000001f, 0.000001f)), Y);
}
/**
* Use this function to compute the pow() in the specular computation.
* This allows to change the implementation depending on platform or it easily can be replaced by some approxmation.
*/
inline FLOAT PhongShadingPow(FLOAT X, FLOAT Y)
{
	// The following clamping is done to prevent NaN being the result of the specular power computation.
	// Clamping has a minor performance cost.

	// In HLSL pow(a, b) is implemented as exp2(log2(a) * b).

	// For a=0 this becomes exp2(-inf * 0) = exp2(NaN) = NaN.

	// As seen in #TTP 160394 "QA Regression: PS3: Some maps have black pixelated artifacting."
	// this can cause severe image artifacts (problem was caused by specular power of 0, lightshafts propagated this to other pixels).
	// The problem appeared on PlayStation 3 but can also happen on similar PC NVidia hardware.

	// In order to avoid platform differences and rarely occuring image atrifacts we clamp the base.

	// Note: Clamping the exponent seemed to fix the issue mentioned TTP but we decided to fix the root and accept the
	// minor performance cost.

	return ClampedPow(X, Y);
}

inline FLOAT CosLike(FLOAT x)
{
    FLOAT z = abs(frac(x)-0.5)*2;
    z = 1 - z * z; 
    z = 1 - z * z; 
    z = (z - 0.5) * 2; 
    return z;
}

inline FLOAT2 CosLike(FLOAT2 x)
{
    FLOAT2 z = abs(frac(x)-0.5)*2;
    z = 1 - z * z; 
    z = 1 - z * z; 
    z = (z - 0.5) * 2; 
    return z;
}

inline FLOAT3 CosLike(FLOAT3 x)
{
    FLOAT3 z = abs(frac(x)-0.5)*2;
    z = 1 - z * z; 
    z = 1 - z * z; 
    z = (z - 0.5) * 2; 
    return z;
}

inline FLOAT4 CosLike(FLOAT4 x)
{
    FLOAT4 z = abs(frac(x)-0.5)*2;
    z = 1 - z * z; 
    z = 1 - z * z; 
    z = (z - 0.5) * 2; 
    return z;
}

inline FLOAT3 DecodeHDR(FLOAT4 encodedIrradiance, FLOAT3 decodeInstructions)
{
    // Take into account texture alpha if decodeInstructions.w is true(the alpha value affects the RGB channels)
    FLOAT alpha = max((encodedIrradiance.a - 1.0) + 1.0, 0.0);

    // If Linear mode is not supported we can skip exponent part
    return (decodeInstructions.x * PositivePow(alpha, decodeInstructions.y)) * encodedIrradiance.rgb;

    // FLOAT alpha = (data.a - 1.0) + 1.0;
    // return (decodeInstructions.x * pow(abs(alpha), decodeInstructions.y)) * data.rgb;
}

inline float4 EncodeFloatRGBA( float v )
{
    float4 kEncodeMul = float4(1.0, 255.0, 65025.0, 16581375.0);
    float kEncodeBit = 1.0/255.0;
    float4 enc = kEncodeMul * v;
    enc = frac (enc);
    enc -= enc.yzww * kEncodeBit;
    return enc;
}

inline float DecodeFloatRGBA( float4 enc )
{
    float4 kDecodeDot = float4(1.0, 1/255.0, 1/65025.0, 1/16581375.0);
    return dot( enc, kDecodeDot );
}

// Encoding/decoding [0..1) floats into 11 bit/channel RG. Note that 1.0 will not be encoded properly.
inline float2 EncodeFloatRG( float v )
{
    float2 kEncodeMul = float2(1.0, 255.0);
    float kEncodeBit = 1.0/ 255.0;
    float2 enc = kEncodeMul * v;
    enc = frac (enc);
    enc.x -= enc.y * kEncodeBit;
    return enc;
}
inline float DecodeFloatRG( float2 enc )
{
    float2 kDecodeDot = float2(1.0, 1/ 255.0);
    return dot( enc, kDecodeDot );
}

inline float3 EncodeFloatRGB(float v)
{
    float4 kEncodeMul = float4(1.0, 255.0, 65025.0, 16581375.0);
    float kEncodeBit = 1.0 / 255.0;
    float4 enc = kEncodeMul * v;
    enc = frac(enc);
    enc -= enc.yzww * kEncodeBit;
    return enc.xyz;
}

inline float DecodeFloatRGB(float3 enc)
{
    float3 kDecodeDot = float3(1.0, 1 / 255.0, 1 / 65025.0);
    return dot(enc, kDecodeDot);
}

inline float FetchFromDepthTex(TEX2D_ARGS(tex), float2 uv )
{
    return DecodeFloatRGB(SAMPLE_TEX2D(tex, uv).xyz);
}

inline float EncodeAlpha(float intensity, float mask)
{
    return lerp(intensity / 5, 0, mask);
}
inline float DecodeAlpha(float a, float mask)
{
    return  lerp(a * 5,1-a,mask);
}
inline FLOAT InRange(FLOAT2 uv, FLOAT4 ranges)
{
	FLOAT2 value = step(ranges.xy, uv) * step(uv, ranges.zw);
	return value.x * value.y;
}


struct MRTOutput 
{
    FLOAT4 rt0 : COLOR0;
    FLOAT4 rt1 : COLOR1;
};

#define SET_BLOOM(mrtOutput, value) mrtOutput.rt1.w = value;

// -----------------------------------------------------------------------------
// Std functions

// Z buffer depth to linear 0-1 depth
// Handles orthographic projection correctly
FLOAT Linear01Depth(FLOAT z)
{
    FLOAT isOrtho = unity_OrthoParams.w;
    FLOAT isPers = 1.0 - unity_OrthoParams.w;
    z *= _ZBufferParams.x;
    return (1.0 - isOrtho * z) / (isPers * z + _ZBufferParams.y);
}
FLOAT Linear01DepthPers(FLOAT z)
{
    z *= _ZBufferParams.x;// 1-far/near
    return rcp(z + _ZBufferParams.y);//far/near
}

FLOAT Linear01DepthPersCustom(FLOAT z,FLOAT farDividNear)
{
    #ifdef UNITY_REVERSED_Z
        z *= farDividNear;// -1+far/near
        return rcp(z + 1);//1
    #else
        z *= 1-farDividNear;// 1-far/near
        return rcp(z + farDividNear);//far/near
    #endif

}
inline FLOAT LinearEyeDepth(FLOAT z)
{
    return rcp(_ZBufferParams.z * z + _ZBufferParams.w);
}

#define DECLARE_POW(type) \
inline type PowApprouch(type x, type n) \
{ \
    n = n * 1.4427 + 1.4427; /* 1.4427f --> 1/ln(2)*/ \
    return exp2(x * n - n);  \
}

DECLARE_POW(half)
DECLARE_POW(half2)
DECLARE_POW(half3)
DECLARE_POW(half4)
DECLARE_POW(float)
DECLARE_POW(float2)
DECLARE_POW(float3)
DECLARE_POW(float4)

inline FLOAT LinearZFromDepth(FLOAT d)
{
    // d *= _ZBufferParams.x;
    // FLOAT d01 = rcp(d + _ZBufferParams.y);
    // return rcp(_ZBufferParams.z * d01 + _ZBufferParams.w);
    FLOAT dx = d * _ZBufferParams.x;
    FLOAT a = dx + _ZBufferParams.y;
    FLOAT b = _ZBufferParams.z + dx*_ZBufferParams.w + _ZBufferParams.y*_ZBufferParams.w;
    return a/b;
}

// Clamp HDR value within a safe range
inline FLOAT3 SafeHDR(FLOAT3 c)
{
    return min(c, HALF_MAX);
}

inline FLOAT4 SafeHDR(FLOAT4 c)
{
    return min(c, HALF_MAX);
}

#if !defined(REDEFINE_URP)
inline FLOAT GetOddNegativeScale()
{
    return unity_WorldTransformParams.w;
}
#endif

// Decode normals stored in _CameraDepthNormalsTexture
FLOAT3 DecodeViewNormalStereo(FLOAT4 enc4)
{
    FLOAT kScale = 1.7777;
    FLOAT3 nn = enc4.xyz * FLOAT3(2.0 * kScale, 2.0 * kScale, 0) + FLOAT3(-kScale, -kScale, 1);
    FLOAT g = 2.0 / dot(nn.xyz, nn.xyz);
    FLOAT3 n;
    n.xy = g * nn.xy;
    n.z = g - 1.0;
    return n;
}

// Interleaved gradient function from Jimenez 2014
// http://www.iryoku.com/next-generation-post-processing-in-call-of-duty-advanced-warfare
FLOAT GradientNoise(FLOAT2 uv)
{
    uv = floor(uv * _ScreenParams.xy);
    FLOAT f = dot(FLOAT2(0.06711056, 0.00583715), uv);
    return frac(52.9829189 * frac(f));
}

// Vertex manipulation
inline FLOAT2 TransformTriangleVertexToUV(FLOAT2 vertex)
{
    FLOAT2 uv = vertex* 0.5 + FLOAT2(0.5,0.5);
    return uv;
}

inline FLOAT2 TransformTriangleVertexToUV2(FLOAT2 vertex)
{
    FLOAT2 uv = vertex* 0.5 + FLOAT2(0.5,0.5);
#if UNITY_UV_STARTS_AT_TOP
	uv = uv * float2(1.0, -1.0) + float2(0.0, 1.0);
#endif
    return uv;
}

// Brightness function
inline FLOAT Brightness(FLOAT3 c)
{
	return Max3(c);
}

inline FLOAT4 ComputeGrabScreenPos (FLOAT4 pos) 
{
#if UNITY_UV_STARTS_AT_TOP
    FLOAT scale = -1.0;
#else//!UNITY_UV_STARTS_AT_TOP
    FLOAT scale = 1.0;
#endif//UNITY_UV_STARTS_AT_TOP
    FLOAT4 o = pos * 0.5f;
    o.xy = FLOAT2(o.x, o.y*scale) + o.w;
    o.zw = pos.zw;
    return o;
}
#if !defined(REDEFINE_URP)
inline FLOAT4 ComputeScreenPos(FLOAT4 pos)
{
    FLOAT4 o = pos * 0.5f;
    o.xy = FLOAT2(o.x, o.y*_ProjectionParams.x) + o.w;
    o.zw = pos.zw;
    return o;
}
#endif

// POW function
inline FLOAT SG(FLOAT x, FLOAT k) {
                FLOAT a = k * 1.44269504f + 1.089234755f;
                return exp2(a * x - a);
}
// -----------------------------------------------------------------------------
// Default vertex shaders

struct AttributesDefault
{
    FLOAT3 vertex : POSITION;
};

struct VaryingsDefault
{
	FLOAT4 vertex : SV_POSITION;
	FLOAT2 texcoord : TEXCOORD0;    
#ifdef _SCREEN_RAY
    FLOAT3 ray : TEXCOORD1;
    #endif//_SCREEN_RAY
};

FLOAT4x4 _FrustumCornersWS;
VaryingsDefault VertDefault(AttributesDefault v)
{
    VaryingsDefault o = (VaryingsDefault)0;
    o.vertex = FLOAT4(v.vertex.xy, 0.0, 1.0);
    o.texcoord = TransformTriangleVertexToUV(v.vertex.xy);

#if UNITY_UV_STARTS_AT_TOP
    o.texcoord = o.texcoord * FLOAT2(1.0, -1.0) + FLOAT2(0.0, 1.0);
#endif

#ifdef _SCREEN_RAY
    o.ray = mul(unity_CameraInvProjection, float4(v.vertex.x,-v.vertex.y, 0, -1)).xyz;
#endif//_SCREEN_RAY
    return o;
}

FLOAT4 _UVTransform; // xy: scale, wz: translate

VaryingsDefault VertUVTransform(AttributesDefault v)
{
    VaryingsDefault o = (VaryingsDefault)0;
    o.vertex = FLOAT4(v.vertex.xy, 0.0, 1.0);
    o.texcoord = TransformTriangleVertexToUV(v.vertex.xy) * _UVTransform.xy + _UVTransform.zw;
    //o.texcoord = TransformStereoScreenSpaceTex(o.texcoord, 1.0);
    return o;
}

struct v2f7
{
    FLOAT4 uv01 : TEXCOORD0;
    FLOAT4 uv23 : TEXCOORD1;
    FLOAT4 uv45 : TEXCOORD2;
    FLOAT4 uv67 : TEXCOORD3;
    FLOAT4 vertex : SV_POSITION;
};
#endif // UNITY_POSTFX_STDLIB
