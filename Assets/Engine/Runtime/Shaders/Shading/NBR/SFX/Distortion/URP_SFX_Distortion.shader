Shader "URP/SFX/Distortion"
{
	Properties
	{
		_MainTex ("Base Tex", 2D) = "white" {}	
		_MainTex1 ("Mask Tex", 2D) = "white" {}	
		_UVST0("Offset", Vector) = (1,1,0,0)	
		_Param0("Param0", Vector) = (1,1,0,0)
	}

	HLSLINCLUDE
		//feature
		#define _OUTPUT_VERTEX_COLOR
		#define _VERTEX_COLOR
		#define _SCREEN_POS
		#define _NO_AO
		#define _NO_EMISSIVE
		#define _PBS_NO_IBL
		#define _PBS_FROM_PARAM
		#define _NO_MRT
	
	ENDHLSL

	SubShader
	{
		Tags { /*"RenderType"="UniversalForward" */"PerformanceChecks" = "False" "Queue"="Transparent" }
		LOD 100

		Pass
		{
			Name "Distortion"
			Tags{ "LightMode" = "Distortion" }
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Cull Off
			
			HLSLPROGRAM

				#pragma target 3.0
				#define URP_BASE
				#define REDEFINE_URP
			    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
				#include "URP_Distortion_Head.hlsl"

				#define _CUSTOM_PS
				#define _DisIntensity (_Param0.x)
				#define _HasMaskTex (_Param0.y>0.5)
				#define _Use_R_Channel (_Param0.z)
				#define _AlphaScale (_Param0.w)

				real4 CustomPS(in FInterpolantsVSToPS Interpolants, in FLOAT4 SvPosition,inout FLOAT4 rt1)
				{
					half2 uv = Interpolants.TexCoords[0].xy * _UVST0.xy + _UVST0.zw * _Time.x;
					half2 normal = SAMPLE_TEX2D(_MainTex, uv).xy;
					half4 result = half4(normal, _DisIntensity, Interpolants.Color.a * _AlphaScale);

					UNITY_BRANCH
					if(_HasMaskTex)
					{
						half2 uv1 = Interpolants.TexCoords[0].xy;
						half4 mask = SAMPLE_TEX2D(_MainTex1, uv1);
						result.a *= lerp(mask.a, mask.r, _Use_R_Channel);
					}

					// Soft Particle
					float2 screenPos = Interpolants.ScreenPosition.xy / Interpolants.ScreenPositionW.x;
					half4 depthTex = SAMPLE_TEX2D(_CameraDepthTexture, screenPos.xy);
					// FLOAT4 depthTex = SAMPLE_TEX2D(_CameraDepthRT, screenPos.xy);
					float sceneDepth = Linear01DepthPers(DecodeFloatRGB(depthTex.xyz));
					float depth = Linear01DepthPers(Interpolants.Depth01.x / Interpolants.Depth01.y);
					result.a *= step( depth, sceneDepth);

					return result;
				}
				
				#include "../../Include/URP_Vertex.hlsl"
				#include "../../Include/URP_Pixel.hlsl"
				
				#pragma vertex vertForwardBase
				#pragma fragment fragForwardBase
			
			ENDHLSL
		}
		Pass
		{
			Name "OverdrawDistortion"
			Tags{"LightMode" = "OverdrawDistortion"}

			Blend One One
			ZWrite off
			Cull Off
			HLSLPROGRAM

			#pragma vertex Vert
			#pragma fragment Frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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
