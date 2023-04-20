Shader "Custom/UI/Scene"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
		[Toggle] _TAAJitterRepair("TAA Jitter Repair", int) = 1
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
        // ZTest [unity_GUIZTestMode]
		
        Pass
        {
            Name "UniversalForward"
            Blend SrcAlpha OneMinusSrcAlpha
            Tags{ "LightMode" = "UniversalForward" }
			HLSLPROGRAM

				#pragma target 2.0

				#include "UIVertex.hlsl"
                #include "UICommon.hlsl" 
                TEX2D_SAMPLER(_SceneRT);
                TEX2D_SAMPLER(_CameraDepthRT);
				float4 _SceneRT_TexelSize;
				float2 _TAAoffsetUV;
				int _TAAJitterRepair;
                /*FLOAT4 uiFrag(UIInterpolantsVSToPS Interpolants, in FLOAT4 SvPosition : SV_Position) : SV_Target
                {
                    FLOAT2 uv = Interpolants.uv0;
                    FLOAT inRangeMask = InRange(uv, FLOAT4(0, 0, 1, 1));
                    FLOAT4 color = SAMPLE_TEX2D(_SceneRT, uv);
                    FLOAT alphaMask = DecodeAlpha(SAMPLE_TEX2D(_CameraDepthRT, uv).w,1);
                    color.a *= alphaMask* inRangeMask;
                    color *= Interpolants.color;

                // #ifdef UNITY_UI_CLIP_RECT
                //     color.a *= UIClip(Interpolants.worldPosition.xy, _ClipRect);
                // #endif
                // #ifdef _UI_GAMMA
                //     color.rgb *= color.a;// pow(color.a, 2.2);
                // #endif
                    return FLOAT4(color);
                }*/

				FLOAT4 uiFrag(UIInterpolantsVSToPS Interpolants, in FLOAT4 SvPosition : SV_Position) : SV_Target
				{
					float2 TAAJitter = -(lerp(0,_TAAoffsetUV.xy,_TAAJitterRepair));
					FLOAT4 color = (SAMPLE_TEX2D(_SceneRT, Interpolants.uv0+ TAAJitter * _SceneRT_TexelSize.xy) + _TextureSampleAdd);
				#ifdef _ADDITIVE
					color.rgb *= 2.0f;
				#endif

				#ifdef _ADDITIVE02
				color.rgb *= _ColorInt;
				float circle=1-saturate (length ( Interpolants.uv0.xy*2-1));
					  circle*=circle;
				color.a *= Interpolants.color.a*circle;

				#endif



					color.a *= Interpolants.color.a;

				#ifdef UNITY_UI_CLIP_RECT
					color.a *= UIClip(Interpolants.worldPosition.xy, _ClipRect);
				#endif
					ColorToGreySwitch(color, Interpolants.color);
				#ifdef _UI_GAMMA
					color.rgb *= color.a;// pow(color.a, 2.2);
				#endif
					//if (color.a < 0.5)
					//{
					//	color = FLOAT4(1, 0, 0, 1);
					//}
				#ifdef UNITY_UI_ALPHACLIP
				    clip (color.a - 0.001);
				#endif
					return color;
				}

				#pragma vertex uiVert
				#pragma fragment uiFrag

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
