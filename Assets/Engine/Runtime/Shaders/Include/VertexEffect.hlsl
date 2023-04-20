#ifndef VERTEX_EFFECT_INCLUDE
#define VERTEX_EFFECT_INCLUDE

FLOAT4 VertexEffect( inout FVertexInput Input,FLOAT4 localPos,FLOAT4 WorldPosition
#ifdef _ENABLE_DEBUG
,inout FLOAT4 debugData
#endif
)
{
#ifdef _CUSTOM_VERTEX_OFFSET
		WorldPosition.xyz += CustomWindEffect(Input,WorldPosition
	#ifdef _ENABLE_DEBUG
			,debugData
	#endif//_ENABLE_DEBUG
);
#endif//_CUSTOM_VERTEX_OFFSET

#ifdef _CUSTOM_WORLD_POS
		WorldPosition.xyz = CustomWorldPos(Input,localPos,WorldPosition);
#endif//_CUSTOM_WORLD_POS

#ifdef _DIRTY_NORMAL
	#define _DirtyNormalTexs _ProcedureTex1
	#define _Expand _Param4.z
	REAL4 NormalMask = SAMPLE_TEX2D_LOD(_DirtyNormalTexs,Input.uv0, 0);
	NormalMask.z *= _Expand;
	//FurLength.r  +=_FurLengthMax ;
	FLOAT3 ExpandScale =  NormalMask.z * Input.TangentX.xyz + localPos.xyz;
	FLOAT3 Offset = mul(_objectToWorld,FLOAT4(ExpandScale,1)).xyz;
	WorldPosition=FLOAT4(Offset,1);
#endif

#ifdef _CUSTOM_CLOUD04
    //TEX2D_SAMPLER(_NoiseTex);
	#define _Height _Adjust.x
	#define _Speed _Adjust.y

	FLOAT2 uv1 = Input.uv0 + _Time.x * _Speed;
	FLOAT2 uv2 = Input.uv0 - _Time.x * _Speed*0.75;
	FLOAT NoisePosx = SAMPLE_TEX2D_LOD(_NoiseTex, uv1, 0).r;
	FLOAT NoisePosy = SAMPLE_TEX2D_LOD(_NoiseTex, uv2, 0).r;
	WorldPosition.y +=  NoisePosx * NoisePosy * _Height ;
#endif

#ifdef _WATER03
	#define _WaveFrency   _Param5.x
	#define _WaveSpeedx   _Param5.y
	#define _WavePower    _Param5.z
	// #define _Ratation     _Param6.x
	// #define _NoiseTexture _ProcedureTex1
    
	//usenoise
    // FLOAT2 WindUV = (WorldPosition.xz * _WaveFrency * 0.01) + (_Time.y * _WaveSpeedx*0.1);
	// WindUV.x = WindUV.x*cos(_Ratation)-WindUV.y *sin(_Ratation);
	// WindUV.y = WindUV.x*sin(_Ratation)+WindUV.y *cos(_Ratation);
	// FLOAT WindTex = SAMPLE_TEX2D_LOD(_NoiseTexture, WindUV, 0).b;
	// WorldPosition.y+=WindTex*_WavePower;

	 FLOAT OffsetY = (WorldPosition.x+WorldPosition.y+WorldPosition.z) * _WaveFrency + _Time.y * _WaveSpeedx; 
	 OffsetY = sin(OffsetY) * _WavePower; 
	 FLOAT3 Offset = FLOAT3(0, OffsetY, 0);
	 WorldPosition.xyz +=Offset;

#endif

	return WorldPosition;
}



#endif //VERTEX_EFFECT_INCLUDE