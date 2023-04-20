#ifndef TYPE

#if defined(_SHADER_LEVEL_LOW) || defined(_FX_LEVEL_LOW)
#define TYPE_POSITION3		half3
#define TYPE_POSITION4		half4
#define TYPE		half
#define TYPE2		half2
#define TYPE3		half3
#define TYPE4		half4
#define TYPE2x2     half2x2
#define TYPE3x3     half3x3
#define TYPE4x4     half4x4

#else

#define TYPE_POSITION3		float3
#define TYPE_POSITION4		float4
#define TYPE		float
#define TYPE2		float2
#define TYPE3		float3
#define TYPE4		float4
#define TYPE2x2     float2x2
#define TYPE3x3     float3x3
#define TYPE4x4     float4x4




#endif
#endif