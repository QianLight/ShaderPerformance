Shader "Hidden/Custom/Editor/ShadowMap_Extra"
{
	Properties
	{
		_MainTex ("empty Tex", 2D) = "white" {}
	}

		HLSLINCLUDE
			#pragma target 5.0
			#include "../../StdLib.hlsl"
			#include "../../Include/LightingHead.hlsl"

			struct Attributes
			{
				FLOAT4 vertex : POSITION;
				FLOAT2 texcoord : TEXCOORD0;
			};

			TEX2D_SHADOWMAP(_SimpleShadowMapTex);

			VaryingsDefault Vert(Attributes v)
			{
				VaryingsDefault o;
				FLOAT4 WorldPosition = mul(unity_ObjectToWorld, v.vertex);
				o.vertex = mul(unity_MatrixVP, WorldPosition);
				o.texcoord = v.texcoord;
				return o;
			}
			FLOAT4 Frag(VaryingsDefault i) : SV_Target
			{
				return SAMPLE_SHADOW(_SimpleShadowMapTex, i.texcoord);
			}

		ENDHLSL

		SubShader
		{
			Cull Off ZWrite Off ZTest Always

			Pass
			{
				Tags {
            "Queue" = "Geometry-100" 
            "RenderType" = "Opaque"
            "LightMode"="ForwardBase"
        }
				HLSLPROGRAM

				#pragma vertex Vert
				#pragma fragment Frag

				ENDHLSL
			}			
		}
}