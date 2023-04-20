Shader "Hidden/Custom/Editor/LightIndexPreview"
{

	Properties
	{
		_MainTex ("Main Tex", 2D) = "white" {} 
	}

	HLSLINCLUDE

		#include "../../StdLib.hlsl"

		struct Attributes
		{
			FLOAT4 vertex : POSITION;
			FLOAT2 texcoord : TEXCOORD0;
		};
		struct Varyings
		{
			FLOAT4 vertex : SV_POSITION;
			FLOAT2 texcoord : TEXCOORD0;
			FLOAT4 color : COLOR;
		};
		TEX2D_SAMPLER(_MainTex);
		Varyings Vert(Attributes v)
		{
			Varyings o;
			FLOAT4 WorldPosition = mul(unity_ObjectToWorld, v.vertex);
			o.vertex = mul(unity_MatrixVP, WorldPosition);
			o.texcoord = v.texcoord;
			o.color = FLOAT4(1,1,1,0.3);
			return o;
		}
		float _Alpha;

		half4 Frag(Varyings i) : SV_Target
		{
			half4 color = SAMPLE_TEX2D(_MainTex, i.texcoord);
			half c = (color.x + color.y+color.z)*255;
			return half4(c.xxx,1);
		}

	ENDHLSL

	SubShader
	{
		Cull Off ZWrite Off ZTest Always
		Pass
		{
			Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
			LOD 100

			Blend SrcAlpha OneMinusSrcAlpha
			HLSLPROGRAM


			#pragma vertex Vert
			#pragma fragment Frag

			ENDHLSL
		}
	}
}
