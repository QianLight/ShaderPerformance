Shader "Hidden/Custom/Editor/Texture_BakeChanel"
{
	Properties
	{
		_MainTex ("Main Tex", 2D) = "white" {}
		_ChanelMask("Chanel Mask",Vector) = (0,0,0,0)
		_TargetChanel("Target Mask",Vector) = (0,0,0,0)
		_LastRT ("_LastRT", 2D) = "black" {}
	}

		HLSLINCLUDE
			#include "../../StdLib.hlsl"

			struct Attributes
			{
				FLOAT3 vertex : POSITION;
				FLOAT2 texcoord : TEXCOORD0;
			};
			TEX2D_SAMPLER(_MainTex);
			FLOAT4 _ChanelMask;
			FLOAT4 _TargetChanel;
			TEX2D_SAMPLER(_LastRT);


			float4 Frag(VaryingsDefault i) : SV_Target
			{
				float4 color = SAMPLE_TEX2D(_MainTex, i.texcoord)*_ChanelMask;
				float c[4]  = {0,0,0,0};
				if(_TargetChanel.x>=0)
					c[(int)_TargetChanel.x] = color.x;
				if(_TargetChanel.y>=0)
					c[(int)_TargetChanel.y] = color.y;
				if(_TargetChanel.z>=0)
					c[(int)_TargetChanel.z] = color.z;
				if(_TargetChanel.w>=0)
					c[(int)_TargetChanel.w] = color.w;
				return float4(c[0],c[1],c[2],c[3]) + SAMPLE_TEX2D(_LastRT, i.texcoord);
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
