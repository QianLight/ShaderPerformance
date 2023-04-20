Shader "Hidden/Custom/Editor/LightIndexDraw"
{

	HLSLINCLUDE
		#pragma target 5.0
		#include "../../StdLib.hlsl"
		#include "../../Include/LightingHead.hlsl"

		struct Attributes
		{
			FLOAT4 vertex : POSITION;
			FLOAT4 texcoord : TEXCOORD0;
		};

		struct v2f
		{
			FLOAT4 vertex : SV_POSITION;
			FLOAT2 texcoord : TEXCOORD0;
			nointerpolation FLOAT3 posIndex : TEXCOORD1;
		};

		v2f Vert(Attributes v)
		{
			v2f o = (v2f)0;
			FLOAT4 WorldPosition = mul(unity_ObjectToWorld, v.vertex);
			o.vertex = mul(unity_MatrixVP, WorldPosition);
			o.posIndex = v.texcoord.xyz;
			return o;
		}
		inline float3 GetLightColor(uint index)
		{
			if(index>0)
			{
				index -=1;
				LightInfo li = _StaticLightInfos[index];
				return li.lightColor.xyz;
			}
			return 0;
		}

		float4 Frag(v2f i) : SV_Target
		{
			float4 lightIndex = SAMPLE_TEX2D(_StaticLightTex, (i.posIndex.xz*2 - _StaticLightParam.yz)*_StaticLightParam.w);
			float4 color = float4(0,0,0,0.1);
			color.xyz += GetLightColor((uint)(lightIndex.x*255));
			color.xyz += GetLightColor((uint)(lightIndex.y*255));
			color.xyz += GetLightColor((uint)(lightIndex.z*255));
			color.xyz += GetLightColor((uint)(lightIndex.w*255));
			return color;
		}

	ENDHLSL

	SubShader
	{
		Tags { "RenderType"="Transparent" "PerformanceChecks" = "False" "Queue"="Transparent" }
		LOD 100
		Cull Off ZWrite Off// ZTest Always

		Pass
		{
			Name "FORWARD"
			Tags{ "LightMode" = "ForwardBase" }
			Blend SrcAlpha OneMinusSrcAlpha

			HLSLPROGRAM

			#pragma vertex Vert
			#pragma fragment Frag

			ENDHLSL
		}			
	}
}
