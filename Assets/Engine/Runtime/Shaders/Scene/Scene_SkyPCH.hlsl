#include "../StdLib.hlsl"
#include "../Colors.hlsl"

#ifndef SCENE_SKYPCH_INCLUDE
#define SCENE_SKYPCH_INCLUDE

struct appdata_t 
{
	FLOAT4 vertex : POSITION;
};

struct v2f 
{
	FLOAT4 vertex : SV_POSITION;
	FLOAT3 texcoord : TEXCOORD0;
	FLOAT2 depth01  : TEXCOORD1;
	FLOAT4 WorldPosition :TEXCOORD2;
	
};
TEXCUBE_SAMPLER(_Tex);
FLOAT _Exposure;
FLOAT4 _Tint;

FLOAT3 _Tex_HDR;
FLOAT4 _SinCosDegree;
FLOAT _WeatherFact;
FLOAT3 _RainbowDir;
FLOAT4 _SceneColor;

FLOAT4 qMult(FLOAT4 q,FLOAT4 v)
{
	FLOAT4 r = FLOAT4
	(
		q.x*v.w + q.y*v.z - q.z*v.y + q.w*v.x,		//i
		- q.x*v.z + q.y*v.w + q.z*v.x + q.w*v.y,		//j
		q.x*v.y - q.y*v.x + q.z*v.w + q.w*v.z,		//k
		- q.x*v.x - q.y*v.y - q.z*v.z + q.w*v.w         //w
	);

	return r;
}

FLOAT3 RotateAroundYInDegrees (FLOAT3 vertex,FLOAT degree)
{
	FLOAT sina = _SinCosDegree.x, cosa = _SinCosDegree.y;
	sincos(degree * PI / 180.0,sina,cosa);
	FLOAT2x2 m = FLOAT2x2(cosa, -sina, sina, cosa);
	return FLOAT3(mul(m, vertex.xz), vertex.y).xzy;
}


#endif //SCENE_SKYPCH_INCLUDE