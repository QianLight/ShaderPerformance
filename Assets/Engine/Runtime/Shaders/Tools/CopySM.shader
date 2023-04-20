Shader "Hidden/Custom/Tools/CopySM"
{
	HLSLINCLUDE
	#include "../StdLib.hlsl"

	TEX2D_SHADOWMAP(_MainTex);

	FLOAT Frag (VaryingsDefault i) : SV_Depth
	{
		
		return SAMPLE_SHADOW_POINT(_MainTex, i.texcoord).r;
	}

	FLOAT Frag2 (VaryingsDefault i) : SV_Depth
	{
		FLOAT2 uv = abs(i.texcoord-0.5) - FLOAT2(0.499,0.499);
		FLOAT mask = (uv.x>0||uv.y>0)?1:-1;
		clip(mask);
		return SAMPLE_SHADOW_POINT(_MainTex, i.texcoord).r;
	}

	FLOAT Frag3 (VaryingsDefault i) : SV_Depth
	{
		FLOAT2 uv = abs(i.texcoord-0.5) - FLOAT2(0.4,0.4);
		FLOAT mask = (uv.x>0||uv.y>0)?1:-1;
		clip(mask);
		return 0;
	}

	ENDHLSL

	SubShader
	{
		Cull Off ZWrite On ZTest Always

		Pass
		{
			Name "Copy"
			HLSLPROGRAM
			#pragma vertex VertDefault
			#pragma fragment Frag
			ENDHLSL
		}

		Pass
		{
			Name "CopyBoard"
			HLSLPROGRAM
			#pragma vertex VertDefault
			#pragma fragment Frag2
			ENDHLSL
		}

		Pass
		{
			Name "Board"
			HLSLPROGRAM
			#pragma vertex VertDefault
			#pragma fragment Frag3
			ENDHLSL
		}
	}

	FallBack Off
}