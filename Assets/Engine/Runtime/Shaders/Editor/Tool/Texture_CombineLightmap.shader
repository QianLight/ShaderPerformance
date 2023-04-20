Shader "Hidden/Custom/Editor/Texture_CombineLightmap"
{
	Properties
	{
		_LightmapColor ("Lightmap", 2D) = "white" {}
		_ShadowMask ("ShadowMask 02", 2D) = "white" {}
		
	}

		HLSLINCLUDE
			#include "../../StdLib.hlsl"

			struct Attributes
			{
				FLOAT3 vertex : POSITION;
				FLOAT2 texcoord : TEXCOORD0;
			};
			struct Varyings
			{
				FLOAT4 vertex : SV_POSITION;
				FLOAT2 texcoord : TEXCOORD0;
			};
			TEX2D_SAMPLER(_LightmapColor);
			TEX2D_SAMPLER(_ShadowMask);
			Varyings Vert2(Attributes v)
			{
				Varyings o;
				FLOAT4 WorldPosition = mul(unity_ObjectToWorld, FLOAT4(v.vertex,1));
				o.vertex = mul(unity_MatrixVP, WorldPosition);
				o.texcoord = v.texcoord;

				return o;
			}

			float4 Frag2(Varyings i) : SV_Target
			{
				float4 color = SAMPLE_TEX2D(_LightmapColor, i.texcoord);
				float4 shadowmask = SAMPLE_TEX2D(_ShadowMask, i.texcoord);
				return float4(color.xyz,shadowmask.x);
			}

			float4 Frag(VaryingsDefault i) : SV_Target
			{
				float4 color = SAMPLE_TEX2D(_LightmapColor, i.texcoord);
				float4 shadowmask = SAMPLE_TEX2D(_ShadowMask, i.texcoord);
				float mask = saturate(shadowmask.x+shadowmask.y+shadowmask.z+shadowmask.w);
				return float4(color.xyz,mask);
			}
		ENDHLSL

		SubShader
		{
			// Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
			// LOD 100

			// Blend One Zero
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
