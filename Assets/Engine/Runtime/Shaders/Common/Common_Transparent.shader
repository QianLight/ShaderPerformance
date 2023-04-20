Shader "Custom/Common/Transparent"
{
	Properties
	{
		[NoScaleOffset]_MainTex ("Base Tex", 2D) = "white" {}
		[HDR]_Color0("Main Color", Color) = (1,1,1,1)
		
		_Color("transparent:w ", Vector) = (0,0,0,0.5)
		[HideInInspector] _DebugMode("__debugMode", Float) = 0.0
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
		LOD 100
		
		Pass
		{
			Name "TransparentDepthPrepass"
			Tags
			{ 
				"LightMode" = "TransparentDepthPrepass" 
				"RenderType"="TransparentCutout" 
				"PerformanceChecks" = "False" 
				"Queue"="Transparent-10"
			}
			Cull Off
			AlphaToMask Off
			ColorMask 0
			ZWrite On
			
			HLSLPROGRAM

				#define DEPTH_PRE_PASS
				#pragma target 3.0

				#include "../Include/PCH.hlsl"
				#include "../Include/Vertex.hlsl"


				#define _CUSTOM_PS
				
				FLOAT4 CustomPS(in FInterpolantsVSToPS Interpolants, in FLOAT4 SvPosition,inout FLOAT4 rt1)
				{
					rt1.xyz = EncodeFloatRGB(Interpolants.Depth01.x/Interpolants.Depth01.y);
					rt1.w = EncodeAlpha(1, 0);
					return 0;
				}

				#include "../Include/Pixel.hlsl"
				#pragma shader_feature_local _ _DEBUG_APP

				#pragma shader_feature_local _ _BASE_FROM_COLOR

				#pragma vertex vertForwardBase
				#pragma fragment fragForwardBase
			
			ENDHLSL
		}
		
		Pass
		{
			Name "FORWARD"
			Tags
			{
				 "LightMode" = "UniversalForward"
				 "RenderType"="Transparent" 
				 "Queue"="Transparent-10"
			}
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			ZTest Equal
			HLSLPROGRAM

				#pragma target 3.0
				#define _ALPHA_BLEND
				#define _CUSTOM_ALPHA

				#define _NO_COLOR_EFFECT
				#define _NO_EMISSIVE
				#define _NO_AO
				#define _PBS_NO_IBL

				#include "../Include/PCH.hlsl"
				#include "../Include/Vertex.hlsl"

				FLOAT CustomAlpha(in FFragData FragData)
				{
					return _Color.a;
				}
				#include "../Include/Pixel.hlsl"
				#pragma shader_feature_local _ _DEBUG_APP
				
				#pragma shader_feature_local _ _BASE_FROM_COLOR

				#pragma vertex vertForwardBase
				#pragma fragment fragForwardBase
			
			ENDHLSL
		}
		Pass
		{
			Name "OverdrawZ"
			Tags{"LightMode" = "OverdrawPreZ" "RenderType"="Opaque" "PerformanceChecks" = "False"}
//				ColorMask 0
//				ColorMask RGBA 1

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
		Pass
		{
			Name "OverdrawCT"
			Tags{ "LightMode" = "OverdrawForwardTransparent" "Queue" = "Transparent-10" "RenderType" = "Transparent" }

			Blend One One
			ZWrite Off
			ZTest Equal

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
	CustomEditor "CFEngine.Editor.PBSShaderGUI"
}
