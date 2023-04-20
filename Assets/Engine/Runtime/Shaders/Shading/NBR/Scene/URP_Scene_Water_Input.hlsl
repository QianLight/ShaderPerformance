#ifndef URP_SCENE_WATER_INPUT_INCLUDE
#define URP_SCENE_WATER_INPUT_INCLUDE


struct WaterInputData
{
	float4 vertex :POSITION;
	float3 normal :NORMAL;
	float4 tangent :TANGENT;
	#ifdef _INPUT_UV0 
		float2	uv0 : TEXCOORD0;
	#endif

	#ifdef _INPUT_UV2
		FLOAT4	uv2 : TEXCOORD1;
	#endif
};


struct Waterv2f
{
	float4 position :SV_POSITION;
	float4 normal :TEXCOORD1;
	float4 tangent :TEXCOORD2;
	float4 bitangent :TEXCOORD3;
	#ifdef _VERTEX_GI
		float4 diffuseGI :TEXCOORD4;
	#endif

	float4 worldPosition :TEXCOORD5;

	#ifdef _SCREEN_POS
		float4 screenPosition :TEXCOORD6;
		float2 screenPositionW :TEXCOORD7;
	#endif
	
	float4 objectPos :TEXCOORD11;
	float3 Texcoord[1] :TEXCOORD12;
	float depth01 :TEXCOORD13;
	float4 debug :TEXCOORD14;
};

#endif