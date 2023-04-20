Shader "Hidden/PostProcessing/Water"
{
	HLSLINCLUDE
		//uv
		//feature
		#define _VERTEX_FOG
		#define _SCREEN_POS
		//lighting		
		#define _VERTEX_GI
		#define _BASE_FROM_COLOR
		#define _DEFAULT_PBS_PARAM
		#define _PBS_NO_IBL			
		#define _WATER_LIGHT
		#define _SCENE_EFFECT
		#define _TRANSPARENCY_ON
	ENDHLSL

	SubShader
	{
		Tags { "RenderType"="Opaque" "PerformanceChecks" = "False" }
		LOD 100		

		Pass
		{
			Name "FORWARD"
			Tags{ "LightMode" = "ForwardBase" }
			ZWrite Off
			HLSLPROGRAM

			#pragma target 3.5
			#include "../Scene/Scene_Head.hlsl"
			#include "../Include/Vertex.hlsl"
			#include "../Include/Pixel.hlsl"
			
			#pragma vertex vertForwardBase
			#pragma fragment fragForwardBase

			ENDHLSL
		}		
	}
}
