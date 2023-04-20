Shader "Hidden/PostProcessing/RadialBlur" {
	Properties { }

	SubShader {

		Cull Off
		ZTest Always
		ZWrite Off
		
		HLSLINCLUDE

		#include "../Sampling.hlsl"

		// 打开则淡出效果使用alpha淡出，否则使用模糊强度淡出
		#define ALPHA_FADE_OUT
		
        TEX2D_SAMPLER(_MainTex);
        FLOAT4 _MainTex_TexelSize;
            
        FLOAT4 _Params0;
        #define _Aspect (_Params0.xy)
        #define _InvAspect (_Params0.zw)

        FLOAT4 _Params1;
        #define _RadiusMul (_Params1.xy)
        #define _RadiusAdd (_Params1.zw)
		#define _InnerRadius _Params1.x
		#define _OuterRadius _Params1.y
		#define _Size _Params1.z

        FLOAT4 _Params2;
        #define _Center (_Params2.xy)
        #define _OffsetScale (_Params2.z)

        struct appdata
        {
            FLOAT4 vertex : POSITION;
        };
            
        struct v2f
        {
            FLOAT2 uv : TEXCOORD0;
            FLOAT4 vertex : SV_POSITION;
        };

        v2f vert (appdata v)
        {
            v2f o;
			o.vertex = float4(v.vertex.xy, 0.0, 1.0);
			o.uv = TransformTriangleVertexToUV(v.vertex.xy);
			#if UNITY_UV_STARTS_AT_TOP
				o.uv = o.uv * float2(1.0, -1.0) + float2(0.0, 1.0);
			#endif
            return o;
        }

		ENDHLSL

		Pass {
			Name "DownSample"
			HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				FLOAT4 frag(v2f i) : SV_Target
				{
					return SAMPLE_TEX2D(_MainTex, i.uv);
				}
            
			ENDHLSL
		}

		Pass {
			Name "Mask and Blur"
			HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				
				FLOAT4 frag(v2f i) : SV_Target
				{
					FLOAT2 rawDelta = mad(i.uv, _Aspect, _Center);
					FLOAT2 delta = rawDelta * _InvAspect;
					FLOAT len = length(rawDelta);
					FLOAT2 dir = -delta / len;
					FLOAT2 fadeout2 = saturate(mad(len, _RadiusMul, _RadiusAdd));
					#define innerFadeOut fadeout2.x
					#define outterFadeOut fadeout2.y
					#if defined(ALPHA_FADE_OUT)
						FLOAT fadeout = innerFadeOut * outterFadeOut;
					#else
						FLOAT fadeout = innerFadeOut;
					#endif
					fadeout *= fadeout;
					FLOAT scale = fadeout * _OffsetScale * len;
					FLOAT2 offset = dir * scale;
                
					FLOAT4 uv01 = mad(offset.xyxy, FLOAT4(0,0,1,1), i.uv.xyxy);
					FLOAT4 uv23 = mad(offset.xyxy, FLOAT4(2,2,3,3), i.uv.xyxy);

					FLOAT4 col;
					col      = SAMPLE_TEX2D(_MainTex, uv01.xy);
				    col.rgb += SAMPLE_TEX2D(_MainTex, uv01.zw).rgb;
				    col.rgb += SAMPLE_TEX2D(_MainTex, uv23.xy).rgb;
				    col.rgb += SAMPLE_TEX2D(_MainTex, uv23.zw).rgb;
					col.rgb *= 0.25;

					#if defined(ALPHA_FADE_OUT)
						FLOAT2 alpha2;
						alpha2.x = dot(abs(scale), _MainTex_TexelSize.zw);
						alpha2.y = sqrt(outterFadeOut);
						alpha2 = saturate(alpha2 * FLOAT2(2, 1.25));
						col.a = min(alpha2.x, alpha2.y);
					#else
						col.a = saturate(dot(scale, _MainTex_TexelSize.zw) * 2);
					#endif

					return FLOAT4(col);
				}
            
			ENDHLSL
		}
		
		Pass {
			Name "Blur"
			HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				
				FLOAT4 frag(v2f i) : SV_Target
				{
					FLOAT2 rawDelta = mad(i.uv, _Aspect, _Center);
					FLOAT2 delta = rawDelta * _InvAspect;
					FLOAT len = length(rawDelta);
					FLOAT2 dir = -delta / len;
					FLOAT2 fadeout2 = saturate(mad(len, _RadiusMul, _RadiusAdd));
					#define innerFadeOut fadeout2.x
					#define outterFadeOut fadeout2.y
					#if defined(ALPHA_FADE_OUT)
						FLOAT fadeout = innerFadeOut * outterFadeOut;
					#else
						FLOAT fadeout = innerFadeOut;
					#endif
					fadeout *= fadeout;
					FLOAT scale = fadeout * _OffsetScale * len;
					FLOAT2 offset = dir * scale;
                
					FLOAT4 uv01 = mad(offset.xyxy, FLOAT4(0,0,1,1), i.uv.xyxy);
					FLOAT4 uv23 = mad(offset.xyxy, FLOAT4(2,2,3,3), i.uv.xyxy);

					FLOAT4 col;
					col      = SAMPLE_TEX2D(_MainTex, uv01.xy);
				    col.rgb += SAMPLE_TEX2D(_MainTex, uv01.zw).rgb;
				    col.rgb += SAMPLE_TEX2D(_MainTex, uv23.xy).rgb;
				    col.rgb += SAMPLE_TEX2D(_MainTex, uv23.zw).rgb;
					col.rgb *= 0.25;

					return FLOAT4(col);
				}
            
			ENDHLSL
		}

		// 2 : DrawDebugColor
		Pass {
			Name "DrawDebugColor"
			HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				
				TEX2D_SAMPLER(_RadialBlurTex);

				FLOAT4 frag(v2f i) : SV_Target
				{
					FLOAT2 rawDelta = mad(i.uv, _Aspect, _Center);
					FLOAT2 delta = rawDelta * _InvAspect;
					FLOAT len = length(rawDelta);
					FLOAT2 fadeout2 = saturate(mad(len, _RadiusMul, _RadiusAdd));
					#define innerFadeOut fadeout2.x
					#define outterFadeOut fadeout2.y
					float fullBlurArea = (innerFadeOut == 1) * (outterFadeOut == 1);
					float bluredArea = (innerFadeOut > 0.00) * (outterFadeOut > 0.00);
					float fadeArea = bluredArea - fullBlurArea;
					float alpha = bluredArea * 0.25;
					float4 debugColor = lerp(float4(1,1,0, alpha), float4(0,1,0, alpha), fullBlurArea);
					return debugColor;
				}
            
			ENDHLSL
		}
	}
}