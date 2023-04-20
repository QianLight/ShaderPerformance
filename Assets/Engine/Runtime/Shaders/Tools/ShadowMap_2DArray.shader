Shader "Hidden/Custom/Tool/ShadowMap_2DArray"
{
	Properties
	{
		_MainTex ("empty Tex", 2D) = "white" {}
		_Slice("Slice", Int) = 0
	}

		HLSLINCLUDE
			#pragma target 5.0
			#include "../StdLib.hlsl"
			#include "../Include/LightingHead.hlsl"

			struct Attributes
			{
				FLOAT4 vertex : POSITION;
				FLOAT2 texcoord : TEXCOORD0;
			};

			TEX2DARRAY_SHADOWMAP_SAMPLER(_ShadowMap);
			TEX2D_SHADOWMAP(_ShadowMap1);
			int _Slice;
			VaryingsDefault Vert(Attributes v)
			{
				VaryingsDefault o;
				FLOAT4 WorldPosition = mul(unity_ObjectToWorld, v.vertex);
				o.vertex = mul(unity_MatrixVP, WorldPosition);
				o.texcoord = v.texcoord;
				return o;
			}
			
			VaryingsDefault VertDefault2(AttributesDefault v)
			{
				VaryingsDefault o = (VaryingsDefault)0;
				o.vertex = FLOAT4(v.vertex.xy, 0.0, 1.0);
				o.texcoord = TransformTriangleVertexToUV(v.vertex.xy);
				FLOAT index = v.vertex.z;

			#if UNITY_UV_STARTS_AT_TOP
				o.texcoord = o.texcoord * FLOAT2(1.0, -1.0) + FLOAT2(0.0, 1.0);
				//index = 3 - index;
			#endif

				return o;
			}
			FLOAT4 Frag(VaryingsDefault i) : SV_Target
			{
				FLOAT4 c =  SAMPLE_SHADOWMAP_TEX2DARRAR(_ShadowMap, FLOAT3(i.texcoord,_Slice));
				c.a = 0.95;
				return c;
			}
			FLOAT4 Frag2(VaryingsDefault i) : SV_Target
			{
				FLOAT4 c =  SAMPLE_SHADOW(_ShadowMap1, i.texcoord);
				c.a = 0.95;
				return c;
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
			
			Pass
			{
				Blend SrcAlpha OneMinusSrcAlpha
				HLSLPROGRAM
				#pragma vertex VertDefault2
				#pragma fragment Frag

				ENDHLSL
			}	

			Pass
			{
				Blend SrcAlpha OneMinusSrcAlpha
				HLSLPROGRAM
				#pragma vertex VertDefault2
				#pragma fragment Frag2

				ENDHLSL
			}				
		}
}