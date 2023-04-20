Shader "Hidden/Custom/Tools/OutLine"
{
	Properties
	{
		_MainTex ("Base Tex", 2D) = "white" {}
		//_Color0("Main Color", Color) = (1,1,1,1)
		// _ParamOutline("", Vector) = (0.5,2.0,1.0,0)
		_Color("Effect Color", Color) = (1,1,1,1)
		//_ColorOutline("Outline Color", Color) = (0,0,0,0.03)
		[HideInInspector] _Stencil ("Stencil ID", Float) = 0
		// _ColorOutline2("Outline Color2", Color) = (0,0,0,0)
	}

	HLSLINCLUDE
	ENDHLSL

	SubShader
	{
		Tags { "RenderType"="Opaque" "PerformanceChecks" = "False" "IgnoreProjector"="False"}
		LOD 100
		Cull Front
		Offset 1,1
		Stencil 
		{  
            Ref [_Stencil]
            Comp equal
        }
		
		Pass
		{
			Name "OUTLINE"
			Tags{ "LightMode" = "Outline" }
			Cull Front
			Offset 1,1
			Stencil
			{
				Ref[_Stencil]
				Comp equal
			}
			HLSLPROGRAM
				#pragma target 3.0

#ifndef _LOCAL_WORLD_OFFSET
				#define _BASE_FROM_COLOR
#endif				
				#include "../Tools/Outline.hlsl"
				#pragma shader_feature_local _ _LOCAL_WORLD_OFFSET
				//#pragma multi_compile_local _ _SPHERE_DITHER_FADE
				#pragma vertex vertForwardBase
				#pragma fragment fragForwardBase
			ENDHLSL
		}
		Pass
		{
			Name "OverdrawO"
			Tags{"LightMode" = "OverdrawOutline"}
			LOD 100

			Blend One One
			HLSLPROGRAM

			#pragma vertex Vert
			#pragma fragment Frag

			#include "UnityCG.cginc"

			struct Attributes
			{
				float4 vertex : POSITION;
			};
			
			struct Varyings
			{
				float4 vertex : SV_POSITION;
			};
			Varyings Vert(Attributes v)
			{
				Varyings o;
				float4 WorldPosition = mul(unity_ObjectToWorld, v.vertex);
				o.vertex = mul(unity_MatrixVP, WorldPosition);
				return o;
			}

			half4 Frag(Varyings i) : SV_Target
			{
				return half4(0.1, 0.04, 0.02, 1);
			}
			
			ENDHLSL
		}
	}
	//CustomEditor "CFEngine.Editor.PBSShaderGUI"
}
