#ifndef URP_SCENE_WATER_VERTEX03_INCLUDE
#define URP_SCENE_WATER_VERTEX03_INCLUDE

#define _WaveFrency   _Param5.x
#define _WaveSpeedx   _Param5.y
#define _WavePower    _Param5.z


inline float4 ComputeGrabScreenPos (float4 pos) 
{
#if UNITY_UV_STARTS_AT_TOP
    float scale = -1.0;
#else//!UNITY_UV_STARTS_AT_TOP
    float scale = 1.0;
#endif//UNITY_UV_STARTS_AT_TOP
    float4 o = pos * 0.5f;
    o.xy = float2(o.x, o.y*scale) + o.w;
    o.zw = pos.zw;
    return o;
}

Waterv2f03 vert(WaterInputData v)
{
	Waterv2f03 o;


	float3 WorldPosition = TransformObjectToWorld(v.vertex);
	float OffsetY = (WorldPosition.x+WorldPosition.y+WorldPosition.z) * _WaveFrency + _Time.y * _WaveSpeedx; 
	OffsetY = sin(OffsetY) * _WavePower; 
	float3 Offset = float3(0, OffsetY, 0);
	WorldPosition.xyz +=Offset;
	o.position = TransformWorldToHClip(WorldPosition.xyz);
	float4 screenpos = ComputeScreenPos(o.position);
	float3 grabPos = ComputeGrabScreenPos(o.position);
	o.screenPos = screenpos;
	o.posGrab.x = screenpos.z;
	o.posGrab.y = grabPos.z;

	VertexNormalInputs normalInput = GetVertexNormalInputs( v.normal, v.tangent );
	o.tSpace0 =  float4( normalInput.normalWS, WorldPosition.x);
	o.tSpace1 = float4( normalInput.tangentWS, WorldPosition.y);
	o.tSpace2 = float4( normalInput.bitangentWS, WorldPosition.z);
	o.uv = v.uv0;
	o.uv2 = v.uv1;
	o.Depth01 = o.position.zw;
	return o;
}


#endif //WATER_VERTEX_02_INCLUDE