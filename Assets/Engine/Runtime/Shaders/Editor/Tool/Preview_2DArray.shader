Shader "Hidden/Custom/Editor/Preview_2DArray"
{
	Properties
	{
		_MainTex ("empty Tex", 2D) = "white" {}
		_Slice("Slice", Int) = 0
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

			TEX2DARRAY_SAMPLER(_TextureArray);

			int _Slice;
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
				return SAMPLE_TEX2DARRAY(_TextureArray, i.texcoord,_Slice);
				
			}

		ENDHLSL

		SubShader
		{
			Cull Off ZWrite Off ZTest Always

			Pass
			{
				HLSLPROGRAM

				#pragma vertex Vert
				#pragma fragment Frag
				ENDHLSL
			}			
		}
}