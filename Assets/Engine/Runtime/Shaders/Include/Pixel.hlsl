#include "DefaultDebug.hlsl"
#include "Fog.hlsl"
#include "MaterialTemplate.hlsl" 
#include "Lighting.hlsl" 

#ifndef PBS_PIXEL_INCLUDE
#define PBS_PIXEL_INCLUDE 

void Frag(in FInterpolantsVSToPS Interpolants, in FLOAT4 SvPosition, inout FLOAT4 rt0,inout FLOAT4 rt1, FLOAT facing)
{
#ifdef _CUSTOM_PS
		rt0 = CustomPS(Interpolants,SvPosition,rt1);
#else//_CUSTOM_PS
		DEBUG_CUSTOMDATA
		FFragData FragData = GetFragData(Interpolants, SvPosition);
		FragData.facing = facing;
		FMaterialData MaterialData = GetMaterialData(FragData);
	#ifdef _FRAMEBUFFER_FETCH
		MaterialData.FrameBuffer = outColor;
	#endif//_FRAMEBUFFER_FETCH
		FLOAT4 Color = CalcLighting(FragData, MaterialData DEBUG_PARAM);
		DEBUG_CUSTOMDATA_PARAM(LightOutput, Color.xyz)
	

		//Rim/Add
		Color.xyz = CalcColorEffect(MaterialData, Color.xyz) * lerp(_SceneColor.rgb, 1, _SceneColor.a);
		
		rt0 = Color;
#ifndef _NO_MRT
     
		rt1.xyz = EncodeFloatRGB(FragData.depth01);
		rt1.w = EncodeAlpha(MaterialData.BloomIntensity, _IsRt1zForUIRT);
#endif
	
		DEBUG_PBS_COLOR(rt0, FragData, MaterialData)

#endif//_CUSTOM_PS

	APPLY_FOG(rt0, Interpolants.WorldPosition.xyz);
}
#ifdef _FRAMEBUFFER_FETCH
	void fragForwardBase(in FInterpolantsVSToPS vs2ps, in FLOAT4 SvPosition : SV_Position,
		inout FLOAT4 rt0 : SV_Target0,in FLOAT3 rt1 : SV_Target1)
	{
		rt1.w = _IsRt1zForUIRT;
		Frag(vs2ps,SvPosition,rt0,rt1);
	}

#else//_FRAMEBUFFER_FETCH
	#ifdef _NO_MRT
		FLOAT4 fragForwardBase(in FInterpolantsVSToPS vs2ps, in FLOAT4 SvPosition : SV_Position, FLOAT facing : VFACE) : SV_Target
		{
			FLOAT4 rt0 = 0;
			FLOAT4 rt1 = FLOAT4(0, 0, 0, _IsRt1zForUIRT);
			Frag(vs2ps,SvPosition,rt0,rt1,facing);
			return rt0;
		}
	#else
		MRTOutput fragForwardBase(in FInterpolantsVSToPS vs2ps, in FLOAT4 SvPosition : SV_Position, FLOAT facing : VFACE)
		{		
			DECLARE_OUTPUT(MRTOutput, mrt);			
			FLOAT4 rt0 = 0;
			FLOAT4 rt1 = FLOAT4(0, 0, 0, _IsRt1zForUIRT);	
			Frag(vs2ps,SvPosition,rt0,rt1,facing);
			mrt.rt0 = rt0;
			mrt.rt1 = rt1;	
			return mrt;
		}
	#endif
#endif//_FRAMEBUFFER_FETCH
#endif //PBS_PIXEL_INCLUDE