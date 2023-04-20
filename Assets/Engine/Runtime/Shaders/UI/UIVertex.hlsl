#ifndef UI_VERTEX_INCLUDE
#define UI_VERTEX_INCLUDE 

#include "UIHead.hlsl"

struct UIMobileShadingVSToPS
{
	UIInterpolantsVSToPS Interpolants;
	FLOAT4 Position : SV_POSITION;
};

void uiVert(UIVertexInput Input,out UIMobileShadingVSToPS Output)
{  
	INITIALIZE_OUTPUT(UIMobileShadingVSToPS, Output);

	Output.Interpolants.worldPosition = Input.vertex;
	Output.Position = TransformObjectToClipPos(Output.Interpolants.worldPosition.xyz);
	Output.Interpolants.ndcPos = Output.Position.xy;
	Output.Interpolants.uv0 = Input.uv0;
	Output.Interpolants.color = Input.color * _Color;
#ifdef UV2
	Output.Interpolants.uv2 = Input.uv2;
#endif//UV2 > 0
} 

#endif //UI_VERTEX_INCLUDE