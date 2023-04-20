Shader "Hidden/Custom/Editor/Preview_3D"
{
	Properties
	{
		_MainTex ("empty Tex", 3D) = "" {}
		_Z("Slice", Float) = 0
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

			sampler3D  _MainTex;

			float _Z;
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
				return tex3D(_MainTex, FLOAT3(i.texcoord,_Z));
				
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