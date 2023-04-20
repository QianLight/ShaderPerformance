Shader "Custom/UI/UI_MaskType"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
	     _Mask ("Mask", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

		 _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    	[Toggle(_IS_NOT_IN_UI_CAMERA)] _isNotInUICamera ("It isn't in UI-Camera", Float) = 0
    	[HideInInspector]_MeshUV("World Position Scale", Vector) = (1,1,1,0)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"
        }

		Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]

        ColorMask [_ColorMask]

        Pass
        {
            Name "UIDefault"
			Blend One OneMinusSrcAlpha
            Tags{ "LightMode" = "UniversalForward" }
			HLSLPROGRAM

				#pragma target 4.5
				#define _UI_GAMMA
				#include "UIVertex.hlsl"

				void Remap(float In, float2 InMinMax, float2 OutMinMax, out float Out)
				{
				    Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
				}
			
			    float4 _MeshUV;
				TEX2D_SAMPLER(_Mask);
				#define INITIALIZE_COLOR(Interpolants,color) InitializeColor(Interpolants,color)
				inline void InitializeColor(UIInterpolantsVSToPS Interpolants, out half4 color)
				{
					color = SAMPLE_TEX2D(_MainTex, Interpolants.uv0) + _TextureSampleAdd;

					float2 uv = float2(0,0);
					Remap(Interpolants.uv0.x,_MeshUV.xz,float2(0,1),uv.x);
					Remap(Interpolants.uv0.y,_MeshUV.yw,float2(0,1),uv.y);
					half mask = SAMPLE_TEX2D(_Mask, uv).x;
					color.w *= mask;
				}
			
				#include "UIPixel.hlsl"
				
				#pragma multi_compile __ UNITY_UI_CLIP_RECT
				#pragma multi_compile_local _ UNITY_UI_ALPHACLIP
			    //#pragma multi_compile _ _IS_NOT_IN_UI_CAMERA
				#pragma vertex uiVert
				#pragma fragment uiFrag

			ENDHLSL
        }
    	
//		Pass
//		{
//			Name "OverdrawF"
//			Tags{"LightMode" = "OverdrawForwardBase"}
//
//			Blend One One
//			CGPROGRAM
//
//			#pragma vertex Vert
//			#pragma fragment Frag
//
//			#include "UnityCG.cginc"
//
//			struct Attributes
//			{
//				float4 vertex : POSITION;
//			};
//			
//			struct Varyings
//			{
//				float4 vertex : SV_POSITION;
//			};
//			Varyings Vert(Attributes v)
//			{
//				Varyings o;
//				float4 WorldPosition = mul(unity_ObjectToWorld, v.vertex);
//				o.vertex = mul(unity_MatrixVP, WorldPosition);
//				return o;
//			}
//
//			half4 Frag(Varyings i) : SV_Target
//			{
//				return half4(0.1, 0.04, 0.02, 1);
//			}
//
//			ENDCG
//		}
    }
}
