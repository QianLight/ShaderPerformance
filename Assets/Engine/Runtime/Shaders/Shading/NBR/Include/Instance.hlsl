#ifndef PBS_INSTANCE_INCLUDE
#define PBS_INSTANCE_INCLUDE 
#include "../Scene/Wind_Effect.hlsl"
#define SUPPORT_COMPUTERBUFFER (SHADER_TARGET>= 45)
 
#if defined(_INSTANCE)
	#define INSTANCE_INPUT ,uint instanceID : SV_InstanceID
	#define INSTANCE_ID ,uint instanceID
	#define INSTANCE_PARAM ,instanceID
	uint _InstanceOffset;
	#ifdef SUPPORT_COMPUTERBUFFER
		// #ifdef _USE_CONSTANT_BUFFER
		// #else//!_USE_CONSTANT_BUFFER
			#ifdef _SIMPLE_MATRIX
				REAL4 GetInstancePos(REAL4 localpos, uint id, REAL4 rot)
				{
					return mul(custom_ObjectToWorld, REAL4(localpos.xyz, 1.0));
				}
			#else
				IBUFFER_START(PRS)
				REAL4 posScale;
				REAL4 rot;
				IBUFFER_END(prsArray,64)

				REAL4 GetInstancePos(REAL4 localpos,uint id, out REAL4 rot)
				{
					uint offset = id + _InstanceOffset;
					REAL4 posScale = prsArray[offset].posScale;
					rot = prsArray[offset].rot;
					REAL4 pos = REAL4(Rot(rot,localpos.xyz*posScale.w) + posScale.xyz,1);
					return mul(custom_ObjectToWorld, REAL4(pos.xyz, 1.0));
				}
		// 	#else
		// 		struct InstanceInfo
		// 		{
		// 			REAL4 posScale;
		// 			REAL4 rot;
		// 		};
		// 		StructuredBuffer<InstanceInfo> _InstanceBuffer;	
  //
		// 		REAL4 GetInstancePos(REAL4 localpos,uint id,out REAL4 rot)
		// 		{
		// 			InstanceInfo ii = _InstanceBuffer[id+_InstanceOffset];
		// 			rot = ii.rot;
  //
		// 				localpos.xyz = Rot(ii.rot,localpos.xyz*ii.posScale.w);
		// 		     //#ifdef _GRASS_COLLISION
		// 			 if(_GRASS_COLLISION){					 
	 //                    localpos=GrassEffect(localpos, ii.posScale.xyz);	
		// 				}
  //                   // #endif
		// 			 
		// 			return REAL4(localpos + ii.posScale.xyz,1);
		// 		}	
			#endif
		// #endif//_USE_CONSTANT_BUFFER
		
	#else//!SUPPORT_COMPUTERBUFFER
		CBUFFER_START
			float2x4 _InstanceBuffer[250];
		CBUFFER_END
		REAL4 GetInstancePos(REAL4 localpos,uint id,out REAL4 rot)
		{
			float2x4 posRotScale = _InstanceBuffer[id+_InstanceOffset];
			REAL4 posScale = posRotScale[0];
			rot = posRotScale[1];
			return REAL4(Rot(rot,localpos) + posScale.xyz,1);
		}	
	#endif//SUPPORT_COMPUTERBUFFER	
	#define INSTANCE_WPOS(localpos,rot) GetInstancePos(localpos,instanceID,rot);

#else//!_INSTANCE

	// #define _ROT90_POS 0
	#define INSTANCE_INPUT
	#define INSTANCE_ID
	#define INSTANCE_PARAM
	#define INSTANCE_WPOS(localpos,rot ) mul(_objectToWorld, REAL4(localpos.xyz, 1.0));

#endif//_INSTANCE
#endif //PBS_INSTANCE_INCLUDE