
// blur Shadowmap//
Shader "Hidden/Custom/Tools/BlurSM"
{
	HLSLINCLUDE
	#include "../StdLib.hlsl"

	TEX2D_SHADOWMAP(_MainTex);

	FLOAT4 _MainTex_TexelSize;
	FLOAT4 _SelfShadowParam;
	#define _SampleDistance _SelfShadowParam.y

	static const FLOAT weight[7] = { 0.0205, 0.0855, 0.232, 0.324, 0.232, 0.0855, 0.0205 };


	FLOAT SampleDepthH(FLOAT2 uv,int index)
	{
		uv = uv + _MainTex_TexelSize.xy*FLOAT2(index*_SampleDistance, 0);
		return SAMPLE_SHADOW(_MainTex, uv).r * weight[index+3];
	}

	FLOAT frag_horizontal (VaryingsDefault i) : SV_Depth
	{
		FLOAT depth = SampleDepthH(i.texcoord,-3);
		depth += SampleDepthH(i.texcoord,-2);
		depth += SampleDepthH(i.texcoord,-1);
		depth += SampleDepthH(i.texcoord,0);
		depth += SampleDepthH(i.texcoord,1);
		depth += SampleDepthH(i.texcoord,2);
		depth += SampleDepthH(i.texcoord,3);
		return depth;
	}

	FLOAT SampleDepthV(FLOAT2 uv,int index)
	{
		uv = uv + _MainTex_TexelSize.xy*FLOAT2(0, index*_SampleDistance);
		return SAMPLE_SHADOW(_MainTex, uv).r * weight[index+3];
	}

	FLOAT frag_vertical (VaryingsDefault i) : SV_Depth
	{
		FLOAT depth = SampleDepthV(i.texcoord,-3);
		depth += SampleDepthV(i.texcoord,-2);
		depth += SampleDepthV(i.texcoord,-1);
		depth += SampleDepthV(i.texcoord,0);
		depth += SampleDepthV(i.texcoord,1);
		depth += SampleDepthV(i.texcoord,2);
		depth += SampleDepthV(i.texcoord,3);
		return depth;
	}

	ENDHLSL

	SubShader
	{
		Cull Off
		ZTest Always
		ZWrite On

		// #0, horizontal //
		Pass
		{
			HLSLPROGRAM
			#pragma vertex VertDefault
			#pragma fragment frag_horizontal
			ENDHLSL
		}

		// #1, vertical //
		Pass
		{
			HLSLPROGRAM
			#pragma vertex VertDefault
			#pragma fragment frag_vertical
			ENDHLSL
		}
	}

	FallBack Off
}