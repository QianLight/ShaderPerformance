#ifndef PBS_VERTEX_INCLUDE
#define PBS_VERTEX_INCLUDE 

#include "InterpolantsVSToPS.hlsl"
#include "GpuAnimation.hlsl"
#include "VertexEffect.hlsl"

struct FMobileShadingVSToPS
{
	FInterpolantsVSToPS Interpolants;
	FLOAT4 Position : SV_POSITION;
};
#ifdef _TERRAIN_LODCULL
FLOAT4 _TerrainLodCull;
#endif

void vertForwardBase(FVertexInput Input
	INSTANCE_INPUT,
out FMobileShadingVSToPS Output)
{  
	INITIALIZE_OUTPUT(FMobileShadingVSToPS, Output);

#ifdef _TERRAIN_LODCULL
	FLOAT inRangeMask = InRange(Input.uv0,_TerrainLodCull);
	if(inRangeMask>0.99)
	{
		Output.Position.z = -100;
		return;
	}
#endif//_TERRAIN_LODCULL

#ifdef _CUSTOM_INSTANCE_ID
	instanceID = GetCustomInstanceID(instanceID);
#endif	

	REAL4 localPos = Input.Position;

#ifdef _GPU_ANIMATION
	localPos = GPUAnimation(Input,localPos INSTANCE_PARAM);
#endif
	FLOAT4 rot = 0;
	//#ifdef _GRASS_COLLISION
	if(_GRASS_COLLISION)
	{
		localPos.w=Input.uv0.y;	
	   #ifndef _INSTANCE
	      localPos=GrassEffecttest(localPos);	
	   #endif
	 }
   // #endif

	FLOAT4 WorldPosition = INSTANCE_WPOS(localPos,rot )
#ifdef _ENABLE_DEBUG
	FLOAT4 debugData = 0;
#endif

	WorldPosition = VertexEffect(Input,localPos,WorldPosition
#ifdef _ENABLE_DEBUG
	,debugData
#endif
	);

	FLOAT4 projPos = 0;

#ifdef _CUSTOM_INTERPOLANTS
	Output.Interpolants = CustomInterpolantsVSToPS(Input, WorldPosition,projPos,rot);
#else	
	Output.Interpolants = GetInterpolantsVSToPS(Input, WorldPosition,rot,projPos);
#endif		

#ifdef _INSTANCE
	Output.Interpolants.InstanceID = instanceID;
#endif

#ifdef _ENABLE_DEBUG
	Output.Interpolants.VertexDebugData = debugData;
#endif

#ifdef _SCREEN_POS
	FLOAT3 pos = ComputeScreenPos(projPos).xyw;
	Output.Interpolants.ScreenPosition.xy = pos.xy;
	Output.Interpolants.ScreenPositionW.x = pos.z;
	FLOAT3 posGrab = ComputeGrabScreenPos(projPos).xyw;
	Output.Interpolants.ScreenPosition.zw = posGrab.xy;
	Output.Interpolants.ScreenPositionW.y = posGrab.z;
#endif//_SCREEN_POS
#ifdef _DEPTH_SHADOW	
	Output.Interpolants.CLIPPostion = mul(_DepthsShadowMatrix,FLOAT4(WorldPosition.xyz,1));	
	FLOAT3 pos1 = ComputeScreenPos(	Output.Interpolants.CLIPPostion).xyw;
	Output.Interpolants.CLIPScreenPosition.xy = pos1.xy;
	Output.Interpolants.CLIPScreenPositionW.x = pos1.z;
	FLOAT3 posGrab1 = ComputeGrabScreenPos(	Output.Interpolants.CLIPPostion).xyw;
	Output.Interpolants.CLIPScreenPosition.zw = posGrab1.xy;
	Output.Interpolants.CLIPScreenPositionW.y = posGrab1.z;
#endif
	Output.Position = projPos;
	//Output.Interpolants.WorldPosition.w = Output.Position.w;	
} 

#endif //PBS_VERTEX_INCLUDE