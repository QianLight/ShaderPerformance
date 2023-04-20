Shader "Hidden/Custom/Tools/PreZ"
{
	Properties
	{
		[NoScaleOffset]_MainTex ("Base Tex", 2D) = "white" {}
	}
	HLSLINCLUDE
	#define _NO_NORMAL
	ENDHLSL

	SubShader
	{
		Tags { "Queue"="Geometry+100" "RenderType"="Opaque" "PerformanceChecks" = "False" "IgnoreProjector"="False"}
		Cull Off
		LOD 100
		
		Pass
		{
			Name "PreZ"
			Tags{ "LightMode" = "PreZ" }
			
			ColorMask 0 0
			ColorMask RGB 1
			HLSLPROGRAM
				#pragma target 3.0

				#include "../Include/PCH.hlsl"

				#define _CUSTOM_PS				
				FLOAT4 CustomPS(in FInterpolantsVSToPS Interpolants, in FLOAT4 SvPosition,inout FLOAT4 rt1)
				{
					FLOAT4 BaseColor = SAMPLE_TEX2D(_MainTex,Interpolants.TexCoords[0].xy);
					clip(BaseColor.a - 0.1f);
					rt1.xyz = EncodeFloatRGB(Interpolants.Depth01.x/Interpolants.Depth01.y);
					rt1.w = _IsRt1zForUIRT;
					return FLOAT4(0,0,0,1);
				}

				#include "../Include/Vertex.hlsl"
				#include "../Include/Pixel.hlsl"
			
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
	}
	// CustomEditor "CFEngine.Editor.PBSShaderGUI"
}
