Shader "Custom/Common/Unlit"
{
	Properties
	{
		[NoScaleOffset]_MainTex ("Base Tex", 2D) = "white" {}
		[HDR]_Color0("Main Color", Color) = (1,1,1,1)
		_Color("transparent:x ", Vector) = (0,0,0,0.5)
		_Param1("SpecularScale:x Spec:y IBLScale:z Cutout:w", Vector) = (1,0.5,0,0.5)
		
		[HideInInspector] _SrcBlend("__src", Float) = 1.0
		[HideInInspector] _DstBlend("__dst", Float) = 0.0
		[HideInInspector] _ZWrite("__zw", Float) = 1.0
		[HideInInspector] _DebugMode("__debugMode", Float) = 0.0
		[HideInInspector] _CullMode("Cull Mode", Float) = 2
		[HideInInspector] _Stencil ("Stencil ID", Float) = 0
	}

	HLSLINCLUDE
		//feature
		#define _MAIN_COLOR
		//lighting
		#define _UN_LIGHT
		#define _PBS_NO_IBL	
		#define _NO_COLOR_EFFECT
	ENDHLSL

	SubShader
	{
		Tags { "RenderType"="Opaque" "PerformanceChecks" = "False" }
		ZWrite[_ZWrite]
		Cull [_CullMode]
		
		LOD 100

		Pass
		{
			Name "FORWARD"
			Tags{ "LightMode" = "ForwardBase" }
			Blend[_SrcBlend][_DstBlend]
			
			Stencil 
			{  
                Ref [_Stencil]
                Comp equal
            } 
			HLSLPROGRAM

				#pragma target 3.0

				#include "../Include/PCH.hlsl"
				#include "../Include/Vertex.hlsl"
				#include "../Include/Pixel.hlsl"
				#pragma shader_feature_local _ _DEBUG_APP

				#pragma shader_feature_local _ _ALPHA_BLEND _ALPHA_TEST
				#pragma shader_feature_local _ _DITHER_TRANSPARENT
				
				#pragma shader_feature_local _ _BASE_FROM_COLOR
				#pragma shader_feature_local _ _SHADOW_MAP
				#pragma shader_feature_local _ _EXTRA_SHADOW
				#pragma shader_feature_local _ _NO_CSM

				#pragma shader_feature_local _ _LOCAL_WORLD_OFFSET

				#pragma vertex vertForwardBase
				#pragma fragment fragForwardBase
			
			ENDHLSL
		}
		Pass
		{
			Name "OverdrawF"
			Tags{"LightMode" = "OverdrawForwardBase"}

			Blend One One
			CGPROGRAM

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

			ENDCG
		}
	}
	CustomEditor "CFEngine.Editor.PBSShaderGUI"
}
