#ifndef URP_SCENE_WATER_VERTEX02_INCLUDE
#define URP_SCENE_WATER_VERTEX02_INCLUDE

Waterv2f02 vert(WaterInputData v)
{
	Waterv2f02 o;
	o.position = TransformObjectToHClip(v.vertex);
	o.screenPos = ComputeScreenPos(o.position);
	float3 WorldPosition = TransformObjectToWorld(v.vertex);
	VertexNormalInputs normalInput = GetVertexNormalInputs( v.normal, v.tangent );
	o.tSpace0 =  float4( normalInput.normalWS, WorldPosition.x);
	o.tSpace1 = float4( normalInput.tangentWS, WorldPosition.y);
	o.tSpace2 = float4( normalInput.bitangentWS, WorldPosition.z);
	o.uv = v.uv0;
	o.uv2 = v.uv1;
	return o;
}


#endif //WATER_VERTEX_02_INCLUDE