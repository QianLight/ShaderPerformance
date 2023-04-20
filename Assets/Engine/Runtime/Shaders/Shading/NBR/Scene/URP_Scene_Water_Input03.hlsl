#ifndef URP_SCENE_WATER_INPUT_INCLUDE
#define URP_SCENE_WATER_INPUT_INCLUDE


struct WaterInputData
{
	float4 vertex :POSITION;
	float3 normal :NORMAL;
	float4 tangent :TANGENT;
	float4	uv0 : TEXCOORD0;
	float4 uv1:TEXCOORD1;
};


struct Waterv2f03
{
	float4 position :SV_POSITION;
	float4 tSpace0 : TEXCOORD1;
	float4 tSpace1 : TEXCOORD2;
	float4 tSpace2 : TEXCOORD3;
	float4 uv :TEXCOORD4;
	float4 uv2 :TEXCOORD5;
	float4 screenPos :TEXCOORD6;
	float3 posGrab :TEXCOORD7;
	float2 Depth01 :TEXCOORD8;
};

#endif