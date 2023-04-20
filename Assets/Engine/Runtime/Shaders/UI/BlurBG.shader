Shader "Custom/UI/BlurBG"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Param0("x:invRectX y:invRectY z:Reverse", Vector) = (1136,640,1,0)
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		Cull Off
        Lighting Off
        ZWrite Off
        // ZTest [unity_GUIZTestMode]

        Pass
        {
            Name "UIBlurBG"
			Tags{ "LightMode" = "UniversalForward" }
			HLSLPROGRAM

				#pragma target 2.0

				#include "UIVertex.hlsl"
				#include "UIPixel.hlsl"

				#pragma vertex uiCustomVert
				#pragma fragment uiFrag
				FLOAT4 _Param0;
				#define _InvRect _Param0.xy
				#define _Reverse _Param0.z

				void uiCustomVert(UIVertexInput Input,out UIMobileShadingVSToPS Output)
				{  
					INITIALIZE_OUTPUT(UIMobileShadingVSToPS, Output);

					Output.Interpolants.worldPosition = Input.vertex;
					Output.Position = TransformObjectToClipPos(Output.Interpolants.worldPosition.xyz);
					
					FLOAT2 uv = Input.vertex.xy*_InvRect + FLOAT2(0.5,0.5);// (v.vertex.xy + FLOAT2(_Rect_X, _Rect_Y) / 2) / FLOAT2(_Rect_X, _Rect_Y);

					
#if UNITY_UV_STARTS_AT_TOP
					FLOAT2 uv2 = uv;
#else
					FLOAT2 uv2 = uv * FLOAT2(1.0, -1.0) + FLOAT2(0.0, 1.0);
#endif

					Output.Interpolants.uv0 = uv - _Reverse * uv + _Reverse * uv2;
				} 
			ENDHLSL
        }		
		UsePass"Hidden/Preview_Overdraw/OverdrawF"
	}
}
