// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Custom/UI/Additive02"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _ColorInt ("ColorInt", range(0,10)) = 1
        

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
		Blend One One
        ColorMask [_ColorMask]
    	
        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }
			HLSLPROGRAM
              
				#pragma target 2.0
				//#define _ADDITIVE02 
				#pragma multi_compile __ UNITY_UI_CLIP_RECT
				#pragma vertex uiVert
				#pragma fragment uiFrag
			
				#include "UIVertex.hlsl"
			
				/* Chang By: Takeshi
				 * Apply: 把原来写在公共头文件里的的Feature迁出来了
				 */
				FLOAT _ColorInt;
				/* 重写 INITIALIZE_COLOR */
				#define INITIALIZE_COLOR(Interpolants,color) Additive02Color(Interpolants,color)
				inline void Additive02Color(UIInterpolantsVSToPS Interpolants , out FLOAT4 color)
				{
					color = (SAMPLE_TEX2D(_MainTex, Interpolants.uv0)+ _TextureSampleAdd);
					color.rgb *= _ColorInt;
					float circle=1-saturate (length ( Interpolants.uv0.xy*2-1));
					circle*=circle;
					color.a *= Interpolants.color.a*circle;
				}
				/* End change */

				#include "UIPixel.hlsl"


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
