Shader "Hidden/Universal Render Pipeline/RadialBlur" {

    SubShader {

        Cull Off
        ZTest Always
        ZWrite Off

        HLSLINCLUDE
        
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"

        // 打开则淡出效果使用alpha淡出，否则使用模糊强度淡出
        #define ALPHA_FADE_OUT

        TEXTURE2D_X(_SourceTex);
        half4 _SourceTex_TexelSize;

        half4 _RadialBlurParam0;
        #define _Aspect (_RadialBlurParam0.xy)
        #define _InvAspect (_RadialBlurParam0.zw)

        half4 _RadialBlurParam1;
        #define _RadiusMul (_RadialBlurParam1.xy)
        #define _RadiusAdd (_RadialBlurParam1.zw)
        #define _InnerRadius _RadialBlurParam1.x
        #define _OuterRadius _RadialBlurParam1.y
        #define _Size _RadialBlurParam1.z

        half4 _RadialBlurParam2;
        #define _Center (_RadialBlurParam2.xy)
        #define _OffsetScale (_RadialBlurParam2.z)

        struct appdata
        {
            half4 vertex : POSITION;
        };

        struct v2f
        {
            half2 uv : TEXCOORD0;
            half4 vertex : SV_POSITION;
        };

        half4 FragDownsample(Varyings input) : SV_Target
        {
            return SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, input.uv);
        }
        ENDHLSL

        Pass {
            Name "DownSample"
            HLSLPROGRAM
            #pragma vertex FullscreenVert
            #pragma fragment FragDownsample
            ENDHLSL
        }

        Pass {
            Name "Mask and Blur"
            HLSLPROGRAM
            #pragma vertex FullscreenVert
            #pragma fragment frag

            half4 frag(Varyings input) : SV_Target
            {
                half2 rawDelta = mad(input.uv, _Aspect, _Center);
                half2 delta = rawDelta * _InvAspect;
                half len = length(rawDelta);
                half2 dir = -delta / len;
                half2 fadeout2 = saturate(mad(len, _RadiusMul, _RadiusAdd));
                #define innerFadeOut fadeout2.x
                #define outterFadeOut fadeout2.y
                #if defined(ALPHA_FADE_OUT)
                    half fadeout = innerFadeOut * outterFadeOut;
                #else
					half fadeout = innerFadeOut;
                #endif
                fadeout *= fadeout;
                half scale = fadeout * _OffsetScale * len;
                half2 offset = dir * scale;

                half4 uv01 = mad(offset.xyxy, half4(0, 0, 1, 1), input.uv.xyxy);
                half4 uv23 = mad(offset.xyxy, half4(2, 2, 3, 3), input.uv.xyxy);

                half4 col;
                col = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv01.xy);
                col.rgb += SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv01.zw).rgb;
                col.rgb += SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv23.xy).rgb;
                col.rgb += SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv23.zw).rgb;
                col.rgb *= 0.25;

                #if defined(ALPHA_FADE_OUT)
                    half2 alpha2;
                    alpha2.x = dot(abs(scale), _SourceTex_TexelSize.zw);
                    alpha2.y = sqrt(outterFadeOut);
                    alpha2 = saturate(alpha2 * half2(2, 1.25));
                    col.a = min(alpha2.x, alpha2.y);
                #else
					col.a = saturate(dot(scale, _SourceTex_TexelSize.zw) * 2);
                #endif

                return col;
            }
            ENDHLSL
        }

        Pass {
            Name "Blur"
            HLSLPROGRAM
            #pragma vertex FullscreenVert
            #pragma fragment FragBlur

            half4 FragBlur(Varyings input) : SV_Target
            {
                half2 rawDelta = mad(input.uv, _Aspect, _Center);
                half2 delta = rawDelta * _InvAspect;
                half len = length(rawDelta);
                half2 dir = -delta / len;
                half2 fadeout2 = saturate(mad(len, _RadiusMul, _RadiusAdd));
                #define innerFadeOut fadeout2.x
                #define outterFadeOut fadeout2.y
                #if defined(ALPHA_FADE_OUT)
                half fadeout = innerFadeOut * outterFadeOut;
                #else
						half fadeout = innerFadeOut;
                #endif
                fadeout *= fadeout;
                half scale = fadeout * _OffsetScale * len;
                half2 offset = dir * scale;

                half4 uv01 = mad(offset.xyxy, half4(0, 0, 1, 1), input.uv.xyxy);
                half4 uv23 = mad(offset.xyxy, half4(2, 2, 3, 3), input.uv.xyxy);

                half4 col;
                col = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv01.xy);
                col.rgb += SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv01.zw).rgb;
                col.rgb += SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv23.xy).rgb;
                col.rgb += SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv23.zw).rgb;
                col.rgb *= 0.25;

                return half4(col);
            }
            ENDHLSL
        }
    }
}