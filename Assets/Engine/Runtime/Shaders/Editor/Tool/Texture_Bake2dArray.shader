Shader "Hidden/Custom/Editor/Texture_Bake2dArray"
{
	Properties
	{
		_2DArrayTex ("empty Tex", 2DArray) = "white" {}
		_Slice("Slice", Int) = 0
		
	}

		HLSLINCLUDE
			#include "../../StdLib.hlsl"

			struct Attributes
			{
				FLOAT3 vertex : POSITION;
				FLOAT2 texcoord : TEXCOORD0;
			};
			TEX2DARRAY_SAMPLER(_2DArrayTex);
			int _Slice;
			float4 Frag(VaryingsDefault i) : SV_Target
			{
				return SAMPLE_TEX2DARRAY(_2DArrayTex, i.texcoord,_Slice);
			}


		ENDHLSL

		SubShader
		{
			Cull Off ZWrite Off ZTest Always
			Pass
			{
				HLSLPROGRAM


				#pragma vertex VertDefault
				#pragma fragment Frag

				ENDHLSL
			}
		}
}
