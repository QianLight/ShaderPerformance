Shader "Hidden/Universal Render Pipeline/Bloom"
{
    Properties {}

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        HLSLINCLUDE

        #pragma multi_compile_local _ _USE_RGBM

        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"

        half4 EncodeHDR(half3 color)
        {
        #if _USE_RGBM
            return EncodeRGBM(color);
        #else
            return half4(color, 1.0);
        #endif
        }

        half3 DecodeHDR(half4 color)
        {
        #if _USE_RGBM
            return DecodeRGBM(color);
        #else
            return color.xyz;
        #endif
        }

        #define FASTER_DOWNSAMPLE 1

        TEXTURE2D_X (_SourceTex);
        half4 _SourceTex_TexelSize;
        TEXTURE2D_X (_MainTex0);
        TEXTURE2D_X (_MainTex1);
        TEXTURE2D_X (_MainTex2);
        
        half4 _Blend;

        half4 FragmentUpSampleHigh(Varyings i) : SV_Target
        {
            float3 color =
                   SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, i.uv.xy) * _Blend.x
                 + SAMPLE_TEXTURE2D_X(_MainTex0 , sampler_LinearClamp, i.uv.xy) * _Blend.y
                 + SAMPLE_TEXTURE2D_X(_MainTex1 , sampler_LinearClamp, i.uv.xy) * _Blend.z
                 + SAMPLE_TEXTURE2D_X(_MainTex2 , sampler_LinearClamp, i.uv.xy) * _Blend.w;
            return EncodeHDR(color);
        }

        half4 FragmentUpSampleMidiem(Varyings i) : SV_Target
        {
            float3 color =
                   SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, i.uv.xy) * _Blend.x
                 + SAMPLE_TEXTURE2D_X(_MainTex0, sampler_LinearClamp, i.uv.xy) * _Blend.y
                 ;//+ SAMPLE_TEXTURE2D_X(_MainTex1, sampler_LinearClamp, i.uv.xy) * _Blend.y;
            return EncodeHDR(color);
        }

        ENDHLSL

        // 0 : Downsampple x4
        Pass
        {
            Name "Down Sample x4"
            HLSLPROGRAM
            #pragma vertex FullscreenVert
            #pragma fragment frag

            struct v2f0
            {
                half4 vertex : SV_POSITION;
                half4 uv01 : TEXCOORD0;
                half4 uv23 : TEXCOORD1;
            };

            half4 frag(Varyings i) : SV_Target
            {
                float4 uv01 = i.uv.xyxy + _SourceTex_TexelSize.xyxy * half4(+1.0, +1.0, +1.0, -1.0);
                float4 uv23 = i.uv.xyxy + _SourceTex_TexelSize.xyxy * half4(-1.0, -1.0, -1.0, +1.0);
                
                half3 sum = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv01.xy)
                          + SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv01.zw)
                          + SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv23.xy)
                          + SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv23.zw);
                return EncodeHDR(sum * 0.25);
            }
            ENDHLSL
        }

        // 1 : Down sample and filter
        Pass
        {
            Name "Down Sample x4 + Filter"
            HLSLPROGRAM
            #pragma vertex FullscreenVert
            #pragma fragment frag

            struct v2f1
            {
                half4 vertex : SV_POSITION;
                #if FASTER_DOWNSAMPLE
                half2 uv0 : TEXCOORD0;
                #else
                    half2 uv0 : TEXCOORD0;
                    half2 uv1 : TEXCOORD1;
                    half2 uv2 : TEXCOORD2;
                    half2 uv3 : TEXCOORD3;
                #endif
            };

            half4 _GlobalSettings0;
            #define _IsRt1zForUIRT _GlobalSettings0.x
            #define _BloomDefaultIntensity 1

            TEXTURE2D_X (_CameraDepthRT);
            half4 _CameraDepthRT_TexelSize;
            half4 _BloomParam;
            half4 _BloomColor;
            #define _Threshhold _BloomParam.x
            #define _Scaler _BloomParam.y

            half4 frag(Varyings i) : SV_Target
            {
                // down sample
                float4 uv01 = i.uv.xyxy + _SourceTex_TexelSize.xyxy * half4(+1.0, +1.0, +1.0, -1.0);
                float4 uv23 = i.uv.xyxy + _SourceTex_TexelSize.xyxy * half4(-1.0, -1.0, -1.0, +1.0);
                
                half3 sum = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv01.xy).rgb
                          + SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv01.zw).rgb
                          + SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv23.xy).rgb
                          + SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv23.zw).rgb;
                half3 avg = sum * 0.25;
                half4 result;
                result.xyz = min(8, max(0, (avg.rgb - _Threshhold) * _Scaler.xxx * _BloomColor.rgb));
                result.w = 0;
                return EncodeHDR(result);
            }
            ENDHLSL
        }

        // 2 : Blur0
        Pass
        {
            Name "Blur"
            HLSLPROGRAM
            #pragma vertex FullscreenVert
            #pragma fragment frag

            half2 _Axis;

            half4 frag(Varyings i) : SV_Target
            {
                float4 uv0 = _Axis.xyxy * half4(-0.011912, -0.011912, -0.004764, -0.004764) + i.uv.xyxy;
                float4 uv1 = _Axis.xyxy * half4(+0.001547, +0.001547, +0.008234, +0.008234) + i.uv.xyxy;
                float2 uv2 = _Axis.xy * half2(+0.015625, +0.015625) + i.uv.xy;

                half3 color =
                       DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv0.zw)) * 0.308026
                     + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv0.xy)) * 0.008609
                     + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv1.xy)) * 0.607088
                     + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv1.zw)) * 0.075852
                     + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv2.xy)) * 0.000425;
                return EncodeHDR(color);
            }
            ENDHLSL
        }

        // 3 : Downsample X2
        Pass
        {
            Name "Downsample x2"

            HLSLPROGRAM
            #pragma vertex FullscreenVert
            #pragma fragment frag

            half4 frag(Varyings i) : SV_Target
            {
                return SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, i.uv.xy);
            }
            ENDHLSL
        }

        // 4 : Blur 1
        Pass
        {
            Name "Downsample 1"
            
            HLSLPROGRAM
            #pragma vertex FullscreenVert
            #pragma fragment frag

            half2 _Axis;

            half4 frag(Varyings i) : SV_Target
            {
                float4 uv0 = _Axis.xyxy * half4(-0.0475560017, -0.0475560017, -0.0325350016, -0.0325350016) + i.uv.xyxy;
                float4 uv1 = _Axis.xyxy * half4(-0.0178779997, -0.0178779997, -0.00355400005, -0.00355400005) + i.uv.xyxy;
                float4 uv2 = _Axis.xyxy * half4(0.0106859999, 0.0106859999, 0.0251579992, 0.0251579992) + i.uv.xyxy;
                float4 uv3 = _Axis.xyxy * half4(0.0400049984, 0.0400049984, 0.0546879992, 0.0546879992) + i.uv.xyxy;

                half3 color =  
                       DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv0.zw)) * 0.015949
                     + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv0.xy)) * 0.000394
                     + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv1.xy)) * 0.163609
                     + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv1.zw)) * 0.439939
                     + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv2.xy)) * 0.316580
                     + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv2.zw)) * 0.060512
                     + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv3.xy)) * 0.002982
                     + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv3.zw)) * 0.000034;
                return EncodeHDR(color);
            }
            ENDHLSL
        }

        // 5 : Blur 2
        Pass
        {
            Name "Blur 2"

            HLSLPROGRAM
            #pragma vertex FullscreenVert
            #pragma fragment frag

            half2 _Axis;

            half4 frag(Varyings i) : SV_Target
            {
                float4 uv0 = _Axis.xyxy * half4(-0.143375993, -0.143375993, -0.112940997, -0.112940997) + i.uv.xyxy;
                float4 uv1 = _Axis.xyxy * half4(-0.0826620013, -0.0826620013, -0.0525240004, -0.0525240004) + i.uv.xyxy;
                float4 uv2 = _Axis.xyxy * half4(-0.0224900004, -0.0224900004, 0.00749500003, 0.00749500003) + i.uv.xyxy;
                float4 uv3 = _Axis.xyxy * half4(0.037498001, 0.037498001, 0.0675780028, 0.0675780028) + i.uv.xyxy;
                float4 uv4 = _Axis.xyxy * half4(0.0977829993, 0.0977829993, 0.128139004, 0.128139004) + i.uv.xyxy;
                float2 uv5 = _Axis.xy * half2(0.15625, 0.15625) + i.uv.xy;

                half3 color =
                      DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv0.zw)) * 0.003893
                    + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv0.xy)) * 0.000271
                    + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv1.xy)) * 0.029742
                    + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv1.zw)) * 0.121250
                    + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv2.xy)) * 0.264445
                    + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv2.zw)) * 0.309050
                    + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv3.xy)) * 0.193602
                    + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv3.zw)) * 0.064945
                    + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv4.xy)) * 0.011641
                    + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv4.zw)) * 0.001112
                    + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv5.xy)) * 0.000048;
                return EncodeHDR(color);
            }
            ENDHLSL
        }

        // 6 : Blur 3
        Pass
        {
            Name "Blur 3"
            
            HLSLPROGRAM
            #pragma vertex FullscreenVert
            #pragma fragment frag

            half2 _Axis;

            half4 frag(Varyings i) : SV_Target
            {
                float4 uv0 = _Axis.xyxy * half4(-0.539946973, -0.539946973, -0.478147, -0.478147) + i.uv.xyxy;
                float4 uv1 = _Axis.xyxy * half4(-0.416377991, -0.416377991, -0.354636997, -0.354636997) + i.uv.xyxy;
                float4 uv2 = _Axis.xyxy * half4(-0.29292199, -0.29292199, -0.231227994, -0.231227994) + i.uv.xyxy;
                float4 uv3 = _Axis.xyxy * half4(-0.169552997, -0.169552997, -0.107891001, -0.107891001) + i.uv.xyxy;
                float4 uv4 = _Axis.xyxy * half4(-0.0462369993, -0.0462369993, 0.015412, 0.015412) + i.uv.xyxy;
                float4 uv5 = _Axis.xyxy * half4(0.0770630017, 0.0770630017, 0.138720006, 0.138720006) + i.uv.xyxy;
                float4 uv6 = _Axis.xyxy * half4(0.200387999, 0.200387999, 0.262071997, 0.262071997) + i.uv.xyxy;
                float4 uv7 = _Axis.xyxy * half4(0.323776007, 0.323776007, 0.385504007, 0.385504007) + i.uv.xyxy;
                float4 uv8 = _Axis.xyxy * half4(0.447257996, 0.447257996, 0.509042978, 0.509042978) + i.uv.xyxy;
                float2 uv9 = _Axis.xy * half2(0.5625, 0.5625) + i.uv.xy;
                
                half3 color =
                       DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv0.xy)) * 0.000049
                     + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv0.zw)) * 0.000291
                     + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv1.xy)) * 0.001383
                     + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv1.zw)) * 0.005300
                     + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv2.xy)) * 0.016378
                     + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv2.zw)) * 0.040822
                     + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv3.xy)) * 0.082068
                     + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv3.zw)) * 0.133082
                     + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv4.xy)) * 0.174080
                     + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv4.zw)) * 0.183685
                     + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv5.xy)) * 0.156350
                     + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv5.zw)) * 0.107352
                     + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv6.xy)) * 0.059457
                     + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv6.zw)) * 0.026562
                     + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv7.xy)) * 0.009571
                     + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv7.zw)) * 0.002781
                     + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv8.xy)) * 0.000652
                     + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv8.zw)) * 0.000123
                     + DecodeHDR(SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv9.xy)) * 0.000014;
                return EncodeHDR(color);
            }
            ENDHLSL
        }

        // 7 : Upsample H
        Pass
        {
            Name "Upsample High"
            
            HLSLPROGRAM
            #pragma vertex FullscreenVert
            #pragma fragment FragmentUpSampleHigh
            ENDHLSL
        }

        // 8 : Upsample M
        Pass
        {
            Name "Upsample Medium"
            HLSLPROGRAM
            #pragma vertex FullscreenVert
            #pragma fragment FragmentUpSampleMidiem
            ENDHLSL
        }
    }
}
