Shader "Custom/UI/UIBlur"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
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
            Name "UIDefaultNoTransparent"
            Tags{ "LightMode" = "UniversalForward" }
			HLSLPROGRAM

				#pragma target 2.0

				#include "UIVertex.hlsl"
                #include "UICommon.hlsl" 
                TEX2D_SAMPLER(_UIBlurRT);
                FLOAT4 uiFrag(UIInterpolantsVSToPS Interpolants, in FLOAT4 SvPosition : SV_Position) : SV_Target
                {
                    FLOAT4 color = (SAMPLE_TEX2D(_UIBlurRT, Interpolants.ndcPos*0.5 + 0.5));
                    color.a *= Interpolants.color.a;

                // #ifdef UNITY_UI_CLIP_RECT
                //     color.a *= UIClip(Interpolants.worldPosition.xy, _ClipRect);
                // #endif
                // #ifdef _UI_GAMMA
                //     color.rgb *= color.a;// pow(color.a, 2.2);
                // #endif
                    return FLOAT4(color);
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
