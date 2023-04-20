#include "../StdLib.hlsl"
#include "../Colors.hlsl"
#include "UICommon.hlsl"

#ifndef UI_HEAD_INCLUDE
#define UI_HEAD_INCLUDE

struct UIVertexInput
{  
	FLOAT4	vertex : POSITION;
	FLOAT4	color : COLOR;
	FLOAT2	uv0 : TEXCOORD0;
#ifdef UV2
	FLOAT2	uv2 : TEXCOORD1;
#endif//UV2 > 0
}; 

struct UIInterpolantsVSToPS
{
	FLOAT4 color : COLOR;
	FLOAT2 uv0 : TEXCOORD0;
#ifdef UV2
	FLOAT2	uv2 : TEXCOORD1;
#endif//UV2 > 0
	FLOAT4 worldPosition : TEXCOORD2;
	FLOAT2 ndcPos : TEXCOORD3;

};

#endif //UI_HEAD_INCLUDE