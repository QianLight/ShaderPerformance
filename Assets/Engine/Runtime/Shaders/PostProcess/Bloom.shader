Shader "Hidden/PostProcessing/Bloom"
{
    Properties { }

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        HLSLINCLUDE

            #include "../Sampling.hlsl"

            #define FASTER_DOWNSAMPLE 1

            #if UNITY_UV_STARTS_AT_TOP
                #define SETUP(v, o) \
                    o.vertex = FLOAT4(v.vertex.xy, 0.0, 1.0); \
                    v.uv0.xy = TransformTriangleVertexToUV(v.vertex.xy); \
                    v.uv0.xy = v.uv0.xy * FLOAT2(1.0, -1.0) + FLOAT2(0.0, 1.0);
            #else
                #define SETUP(v, o) \
                    o.vertex = FLOAT4(v.vertex.xy, 0.0, 1.0); \
                    v.uv0.xy = TransformTriangleVertexToUV(v.vertex.xy);
            #endif
         
              struct v2f
            {
                FLOAT4 vertex : SV_POSITION;
                FLOAT2 uv0 : TEXCOORD0;
            };
              TEX2D_SAMPLER(_MainTex0);
              TEX2D_SAMPLER(_MainTex1);
              TEX2D_SAMPLER(_MainTex2);
              TEX2D_SAMPLER(_MainTex3);
              FLOAT4 _Blend;

            half4 fragH (v2f i) : SV_Target
            {                                 
                    return SAMPLE_TEX2D(_MainTex0, i.uv0.xy) * _Blend.x
                         + SAMPLE_TEX2D(_MainTex1, i.uv0.xy) * _Blend.y
                         + SAMPLE_TEX2D(_MainTex2, i.uv0.xy) * _Blend.z
                         + SAMPLE_TEX2D(_MainTex3, i.uv0.xy) * _Blend.w
                         ; 
                                                
            }
               half4 fragM (v2f i) : SV_Target
            {                           
                    return SAMPLE_TEX2D(_MainTex0, i.uv0.xy) * _Blend.x
                         + SAMPLE_TEX2D(_MainTex1, i.uv0.xy) * _Blend.y
                         + SAMPLE_TEX2D(_MainTex2, i.uv0.xy) * _Blend.z
                         ;                                                   
            }
                 half4 fragL (v2f i) : SV_Target
            {                            
                   return SAMPLE_TEX2D(_MainTex0, i.uv0.xy);                                                             
            }
        ENDHLSL

        // 0 : Downsampple x4
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                FLOAT4 vertex : POSITION;
                FLOAT2 uv0 : TEXCOORD0;
            };

            struct v2f0
            {
                FLOAT4 vertex : SV_POSITION;
                FLOAT4 uv01 : TEXCOORD0;
                FLOAT4 uv23 : TEXCOORD1;
            };

            TEX2D_SAMPLER(_MainTex);
            FLOAT4 _MainTex_TexelSize;

            v2f0 vert (appdata v)
            {
                v2f0 o;
                SETUP(v, o);
                o.uv01 = v.uv0.xyxy + _MainTex_TexelSize.xyxy * FLOAT4(+1.0, +1.0, +1.0, -1.0);
                o.uv23 = v.uv0.xyxy + _MainTex_TexelSize.xyxy * FLOAT4(-1.0, -1.0, -1.0, +1.0);
                return o;
            }

            half4 frag (v2f0 i) : SV_Target
            {
                FLOAT4 sum = SAMPLE_TEX2D(_MainTex, i.uv01.xy)
                           + SAMPLE_TEX2D(_MainTex, i.uv01.zw)
                           + SAMPLE_TEX2D(_MainTex, i.uv23.xy)
                           + SAMPLE_TEX2D(_MainTex, i.uv23.zw);
                return sum * 0.25;
            }
            ENDHLSL
        }

        // 1 : Down sample and filter
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                FLOAT4 vertex : POSITION;
                FLOAT2 uv0 : TEXCOORD0;
            };

            struct v2f1
            {
                FLOAT4 vertex : SV_POSITION;
                #if FASTER_DOWNSAMPLE
                    FLOAT2 uv0 : TEXCOORD0;
                #else
                    FLOAT2 uv0 : TEXCOORD0;
                    FLOAT2 uv1 : TEXCOORD1;
                    FLOAT2 uv2 : TEXCOORD2;
                    FLOAT2 uv3 : TEXCOORD3;
                #endif
            };

            FLOAT4 _GlobalSettings0;
            #define _IsRt1zForUIRT _GlobalSettings0.x
            #define _BloomDefaultIntensity 1

            TEX2D_SAMPLER(_CameraDepthRT);
            FLOAT4 _CameraDepthRT_TexelSize;
            TEX2D_SAMPLER(_MainTex);
            FLOAT4 _MainTex_TexelSize;
            FLOAT4 _BloomParam;
            FLOAT4 _BloomColor;
            #define _Threshhold _BloomParam.x
            #define _Scaler _BloomParam.y

            v2f1 vert (appdata v)
            {
                v2f1 o;
                SETUP(v, o);
                o.uv0 = v.uv0;
                return o;
            }

            half4 frag (v2f1 i) : SV_Target
            {
                // down sample
                FLOAT4 avg = SAMPLE_TEX2D(_MainTex, i.uv0.xy);
                FLOAT2 depthUv = i.uv0.xy * _CameraDepthRT_TexelSize.zw;
                // if is ui rt mode, disable mask feature, use contant 
                FLOAT bloomIntensity = lerp(DecodeAlpha(LOAD_TEX2D(_CameraDepthRT, depthUv).a,0), _BloomDefaultIntensity, _IsRt1zForUIRT);
                // filter
                FLOAT4 result;
                result.xyz = max(0, (avg.rgb * bloomIntensity - _Threshhold) * _Scaler.xxx * _BloomColor.rgb);
                result.a = bloomIntensity;
                return result;
            }
            ENDHLSL
        }
        
        // 2 : Blur0
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                FLOAT4 vertex : POSITION;
                FLOAT2 uv0 : TEXCOORD0;
            };

            struct v2f2
            {
                FLOAT4 vertex : SV_POSITION;
                FLOAT4 uv0 : TEXCOORD0;
                FLOAT4 uv1 : TEXCOORD1;
                FLOAT2 uv2 : TEXCOORD2;
            };

            TEX2D_SAMPLER(_MainTex);
            FLOAT4 _MainTex_TexelSize;
            FLOAT2 _Axis;

            v2f2 vert (appdata v)
            {
                v2f2 o;
                SETUP(v, o);
                o.uv0 = _Axis.xyxy * FLOAT4(-0.011912, -0.011912, -0.004764, -0.004764) + v.uv0.xyxy;
                o.uv1 = _Axis.xyxy * FLOAT4(+0.001547, +0.001547, +0.008234, +0.008234) + v.uv0.xyxy;
                o.uv2 = _Axis.xy   * FLOAT2(+0.015625, +0.015625) + v.uv0.xy;
                return o;
            }

            half4 frag (v2f2 i) : SV_Target
            {
                return SAMPLE_TEX2D(_MainTex, i.uv0.zw) * 0.308026
                     + SAMPLE_TEX2D(_MainTex, i.uv0.xy) * 0.008609
                     + SAMPLE_TEX2D(_MainTex, i.uv1.xy) * 0.607088
                     + SAMPLE_TEX2D(_MainTex, i.uv1.zw) * 0.075852
                     + SAMPLE_TEX2D(_MainTex, i.uv2.xy) * 0.000425
                ;
            }
            ENDHLSL
        }

        // 3 : Downsample X2
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                FLOAT4 vertex : POSITION;
                FLOAT2 uv0 : TEXCOORD0;
            };

            //struct v2f3
            //{
            //    FLOAT4 vertex : SV_POSITION;
            //    FLOAT2 uv0 : TEXCOORD0;
            //};

            TEX2D_SAMPLER(_MainTex);
            FLOAT4 _MainTex_TexelSize;

            v2f vert (appdata v)
            {
                v2f o;
                SETUP(v, o);
                o.uv0 = v.uv0;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                return SAMPLE_TEX2D(_MainTex, i.uv0.xy);
            }
            ENDHLSL
        }
        
        // 4 : Blur 1
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                FLOAT4 vertex : POSITION;
                FLOAT2 uv0 : TEXCOORD0;
                FLOAT2 uv1 : TEXCOORD0;
                FLOAT2 uv2 : TEXCOORD0;
                FLOAT2 uv3 : TEXCOORD0;
            };

            struct v2f4
            {
                FLOAT4 vertex : SV_POSITION;
                FLOAT4 uv0 : TEXCOORD0;
                FLOAT4 uv1 : TEXCOORD1;
                FLOAT4 uv2 : TEXCOORD2;
                FLOAT4 uv3 : TEXCOORD3;
            };

            TEX2D_SAMPLER(_MainTex);
            FLOAT4 _MainTex_TexelSize;
            FLOAT2 _Axis;

            v2f4 vert (appdata v)
            {
                v2f4 o;
                SETUP(v, o);
                o.uv0 = _Axis.xyxy * FLOAT4(-0.0475560017, -0.0475560017, -0.0325350016, -0.0325350016) + v.uv0.xyxy;
                o.uv1 = _Axis.xyxy * FLOAT4(-0.0178779997, -0.0178779997, -0.00355400005, -0.00355400005) + v.uv0.xyxy;
                o.uv2 = _Axis.xyxy * FLOAT4(0.0106859999, 0.0106859999, 0.0251579992, 0.0251579992) + v.uv0.xyxy;
                o.uv3 = _Axis.xyxy * FLOAT4(0.0400049984, 0.0400049984, 0.0546879992, 0.0546879992) + v.uv0.xyxy;
                return o;
            }

            half4 frag (v2f4 i) : SV_Target
            {
                return SAMPLE_TEX2D(_MainTex, i.uv0.zw) * 0.015949
                     + SAMPLE_TEX2D(_MainTex, i.uv0.xy) * 0.000394
                     + SAMPLE_TEX2D(_MainTex, i.uv1.xy) * 0.163609
                     + SAMPLE_TEX2D(_MainTex, i.uv1.zw) * 0.439939
                     + SAMPLE_TEX2D(_MainTex, i.uv2.xy) * 0.316580
                     + SAMPLE_TEX2D(_MainTex, i.uv2.zw) * 0.060512
                     + SAMPLE_TEX2D(_MainTex, i.uv3.xy) * 0.002982
                     + SAMPLE_TEX2D(_MainTex, i.uv3.zw) * 0.000034
                     ;
            }
            ENDHLSL
        }
        
        // 5 : Blur 2
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                FLOAT4 vertex : POSITION;
                FLOAT2 uv0 : TEXCOORD0;
            };

            struct v2f5
            {
                FLOAT4 vertex : SV_POSITION;
                FLOAT4 uv0 : TEXCOORD0;
                FLOAT4 uv1 : TEXCOORD1;
                FLOAT4 uv2 : TEXCOORD2;
                FLOAT4 uv3 : TEXCOORD3;
                FLOAT4 uv4 : TEXCOORD4;
                FLOAT2 uv5 : TEXCOORD5;
            };

            TEX2D_SAMPLER(_MainTex);
            FLOAT4 _MainTex_TexelSize;
            FLOAT2 _Axis;

            v2f5 vert (appdata v)
            {
                v2f5 o;
                SETUP(v, o);
                o.uv0 = _Axis.xyxy * FLOAT4(-0.143375993, -0.143375993, -0.112940997, -0.112940997) + v.uv0.xyxy;
                o.uv1 = _Axis.xyxy * FLOAT4(-0.0826620013, -0.0826620013, -0.0525240004, -0.0525240004) + v.uv0.xyxy;
                o.uv2 = _Axis.xyxy * FLOAT4(-0.0224900004, -0.0224900004, 0.00749500003, 0.00749500003) + v.uv0.xyxy;
                o.uv3 = _Axis.xyxy * FLOAT4(0.037498001, 0.037498001, 0.0675780028, 0.0675780028) + v.uv0.xyxy;
                o.uv4 = _Axis.xyxy * FLOAT4(0.0977829993, 0.0977829993, 0.128139004, 0.128139004) + v.uv0.xyxy;
                o.uv5 = _Axis.xy   * FLOAT2(0.15625, 0.15625) + v.uv0.xy;
                return o;
            }

            half4 frag (v2f5 i) : SV_Target
            {
                return SAMPLE_TEX2D(_MainTex, i.uv0.zw) * 0.003893
                     + SAMPLE_TEX2D(_MainTex, i.uv0.xy) * 0.000271
                     + SAMPLE_TEX2D(_MainTex, i.uv1.xy) * 0.029742
                     + SAMPLE_TEX2D(_MainTex, i.uv1.zw) * 0.121250
                     + SAMPLE_TEX2D(_MainTex, i.uv2.xy) * 0.264445
                     + SAMPLE_TEX2D(_MainTex, i.uv2.zw) * 0.309050
                     + SAMPLE_TEX2D(_MainTex, i.uv3.xy) * 0.193602
                     + SAMPLE_TEX2D(_MainTex, i.uv3.zw) * 0.064945
                     + SAMPLE_TEX2D(_MainTex, i.uv4.xy) * 0.011641
                     + SAMPLE_TEX2D(_MainTex, i.uv4.zw) * 0.001112
                     + SAMPLE_TEX2D(_MainTex, i.uv5.xy) * 0.000048
                     ;
            }
            ENDHLSL
        }

        // 6 : Blur 3
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                FLOAT4 vertex : POSITION;
                FLOAT2 uv0 : TEXCOORD0;
            };

            struct v2f6
            {
                FLOAT4 vertex : SV_POSITION;
                FLOAT4 uv0 : TEXCOORD0;
                FLOAT4 uv1 : TEXCOORD1;
                FLOAT4 uv2 : TEXCOORD2;
                FLOAT4 uv3 : TEXCOORD3;
                FLOAT4 uv4 : TEXCOORD4;
                FLOAT4 uv5 : TEXCOORD5;
                FLOAT4 uv6 : TEXCOORD6;
                FLOAT4 uv7 : TEXCOORD7;
                FLOAT4 uv8 : TEXCOORD8;
                FLOAT2 uv9 : TEXCOORD9;
            };

            TEX2D_SAMPLER(_MainTex);
            FLOAT4 _MainTex_TexelSize;
            FLOAT2 _Axis;

            v2f6 vert (appdata v)
            {
                v2f6 o;
                SETUP(v, o);
                o.uv0 = _Axis.xyxy * FLOAT4(-0.539946973, -0.539946973, -0.478147, -0.478147) + v.uv0.xyxy;
                o.uv1 = _Axis.xyxy * FLOAT4(-0.416377991, -0.416377991, -0.354636997, -0.354636997) + v.uv0.xyxy;
                o.uv2 = _Axis.xyxy * FLOAT4(-0.29292199, -0.29292199, -0.231227994, -0.231227994) + v.uv0.xyxy;
                o.uv3 = _Axis.xyxy * FLOAT4(-0.169552997, -0.169552997, -0.107891001, -0.107891001) + v.uv0.xyxy;
                o.uv4 = _Axis.xyxy * FLOAT4(-0.0462369993, -0.0462369993, 0.015412, 0.015412) + v.uv0.xyxy;
                o.uv5 = _Axis.xyxy * FLOAT4(0.0770630017, 0.0770630017, 0.138720006, 0.138720006) + v.uv0.xyxy;
                o.uv6 = _Axis.xyxy * FLOAT4(0.200387999, 0.200387999, 0.262071997, 0.262071997) + v.uv0.xyxy;
                o.uv7 = _Axis.xyxy * FLOAT4(0.323776007, 0.323776007, 0.385504007, 0.385504007) + v.uv0.xyxy;
                o.uv8 = _Axis.xyxy * FLOAT4(0.447257996, 0.447257996, 0.509042978, 0.509042978) + v.uv0.xyxy;
                o.uv9 = _Axis.xy   * FLOAT2(0.5625, 0.5625) + v.uv0.xy;
                return o;
            }

            half4 frag (v2f6 i) : SV_Target
            {
                return SAMPLE_TEX2D(_MainTex, i.uv0.xy) * 0.000049
                     + SAMPLE_TEX2D(_MainTex, i.uv0.zw) * 0.000291
                     + SAMPLE_TEX2D(_MainTex, i.uv1.xy) * 0.001383
                     + SAMPLE_TEX2D(_MainTex, i.uv1.zw) * 0.005300
                     + SAMPLE_TEX2D(_MainTex, i.uv2.xy) * 0.016378
                     + SAMPLE_TEX2D(_MainTex, i.uv2.zw) * 0.040822
                     + SAMPLE_TEX2D(_MainTex, i.uv3.xy) * 0.082068
                     + SAMPLE_TEX2D(_MainTex, i.uv3.zw) * 0.133082
                     + SAMPLE_TEX2D(_MainTex, i.uv4.xy) * 0.174080
                     + SAMPLE_TEX2D(_MainTex, i.uv4.zw) * 0.183685
                     + SAMPLE_TEX2D(_MainTex, i.uv5.xy) * 0.156350
                     + SAMPLE_TEX2D(_MainTex, i.uv5.zw) * 0.107352
                     + SAMPLE_TEX2D(_MainTex, i.uv6.xy) * 0.059457
                     + SAMPLE_TEX2D(_MainTex, i.uv6.zw) * 0.026562
                     + SAMPLE_TEX2D(_MainTex, i.uv7.xy) * 0.009571
                     + SAMPLE_TEX2D(_MainTex, i.uv7.zw) * 0.002781
                     + SAMPLE_TEX2D(_MainTex, i.uv8.xy) * 0.000652
                     + SAMPLE_TEX2D(_MainTex, i.uv8.zw) * 0.000123
                     + SAMPLE_TEX2D(_MainTex, i.uv0.xy) * 0.000014
                     ; 
            }
            ENDHLSL
        }
        
        // 7 : Upsample H
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment fragH

            struct appdata
            {
                FLOAT4 vertex : POSITION;
                FLOAT2 uv0 : TEXCOORD0;
            };


            v2f vert (appdata v)
            {
                v2f o;
                SETUP(v, o);
                o.uv0 = v.uv0;
                return o;
            }

            ENDHLSL
        }

           // 8 : Upsample M
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment fragM

            struct appdata
            {
                FLOAT4 vertex : POSITION;
                FLOAT2 uv0 : TEXCOORD0;
            };


            v2f vert (appdata v)
            {
                v2f o;
                SETUP(v, o);
                o.uv0 = v.uv0;
                return o;
            }

            ENDHLSL
        }
               // 9 : Upsample L
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment fragL

            struct appdata
            {
                FLOAT4 vertex : POSITION;
                FLOAT2 uv0 : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                SETUP(v, o);
                o.uv0 = v.uv0;
                return o;
            }

            ENDHLSL
        }
    }
}
