Shader "UI/AdditiveImageSequence"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Frame("Frame", Int) = 1
		_TimePerFrame("TimePerFrame", Float) = 0.5
		_RemainTime("RemainTime", Float) = 0.0
		_Color("Tint", Color) = (1,1,1,1)
		_ColorMask("Color Mask", Float) = 15
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}
		Cull Off
		Lighting Off
		ZWrite Off
		ZTest[unity_GUIZTestMode]
		ColorMask[_ColorMask]
		
		Pass
		{
			Name "UIAdditive"
			Tags{ "LightMode" = "UniversalForward" }
			Blend SrcAlpha One
			HLSLPROGRAM

				#pragma target 2.0
				#define _ADDITIVE
				#include "UIVertex.hlsl"
				#include "UIPixel.hlsl"

				#pragma multi_compile __ UNITY_UI_CLIP_RECT
				#pragma vertex vert
				#pragma fragment uiFrag

				int _Frame;
				float _TimePerFrame;
				float _RemainTime;

				struct v2f
				{
					float4 vert : SV_POSITION;
					float2 uv : TEXCOORD0;
				};

				void vert(UIVertexInput Input, out UIMobileShadingVSToPS Output)
				{
					INITIALIZE_OUTPUT(UIMobileShadingVSToPS, Output);

					Output.Interpolants.worldPosition = Input.vertex;
					Output.Position = TransformObjectToClipPos(Output.Interpolants.worldPosition.xyz);

					Output.Interpolants.color = Input.color * _Color;

					float width = 1.0f / _Frame;
					int index = floor(fmod(_Time.y, _TimePerFrame * _Frame + _RemainTime) / _TimePerFrame);
					int finalIndex = clamp(index, 0, _Frame - 1);
					Output.Interpolants.uv0 = float2((finalIndex + Input.uv0.x) * width, Input.uv0.y);
				}
			ENDHLSL
		}
		Pass
		{
			Name "OverdrawF"
			Tags{"LightMode" = "OverdrawForwardBase"}

			Blend One One
			CGPROGRAM

			#pragma vertex Vert
			#pragma fragment Frag

			#include "UnityCG.cginc"

			struct Attributes
			{
				float4 vertex : POSITION;
			};
			
			struct Varyings
			{
				float4 vertex : SV_POSITION;
			};
			Varyings Vert(Attributes v)
			{
				Varyings o;
				float4 WorldPosition = mul(unity_ObjectToWorld, v.vertex);
				o.vertex = mul(unity_MatrixVP, WorldPosition);
				return o;
			}

			half4 Frag(Varyings i) : SV_Target
			{
				return half4(0.1, 0.04, 0.02, 1);
			}

			ENDCG
		}
	}
}
