Shader "Hidden/PostProcessing/DepthOfField" {
	Properties { }

	SubShader {

		Cull Off
		ZTest Always
		ZWrite Off
		
		HLSLINCLUDE

		#include "../Sampling.hlsl"
        TEX2D_SAMPLER(_MainTex);
		FLOAT4 _MainTex_TexelSize;
		TEX2D_SAMPLER(_CameraDepthRT);
		TEX2D_SAMPLER(_DoFTex);

		struct Interpolators {
			FLOAT4 pos : SV_POSITION;
			FLOAT2 uv : TEXCOORD0;
		};
		
		#include "DepthOfField.hlsl"

		ENDHLSL

		// 0 : PreFilter
		Pass {
			HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				struct appdata
				{
					FLOAT4 vertex : POSITION;
				};
            
				struct v2f
				{
					FLOAT4 vertex : SV_POSITION;
					FLOAT4 uv01 : TEXCOORD0;
					FLOAT4 uv23 : TEXCOORD1;
				};

				v2f vert (appdata v)
				{
					v2f o;
					o.vertex = float4(v.vertex.xy, 0.0, 1.0);
					FLOAT2 uv = TransformTriangleVertexToUV(v.vertex.xy);
					#if UNITY_UV_STARTS_AT_TOP
						uv = uv * float2(1.0, -1.0) + float2(0.0, 1.0);
					#endif
					FLOAT4 offset = _MainTex_TexelSize.xyxy * FLOAT4(-1.0, -1.0, 1.0, 1.0);
					o.uv01 = uv.xyxy + offset.xyzw;
					o.uv23 = uv.xyxy + offset.xwzw;
					return o;
				}

				FLOAT Weigh (FLOAT3 c) {
					return max(max(c.r, c.g), c.b);
				}
				
				FLOAT4 frag (v2f i) : SV_Target {
					FLOAT4 offset = _MainTex_TexelSize.xyxy * FLOAT4(-1.0, -1.0, 1.0, 1.0);

					FLOAT3 s0 = SAMPLE_TEX2D(_MainTex, i.uv01.xy).rgb;
					FLOAT3 s1 = SAMPLE_TEX2D(_MainTex, i.uv01.zw).rgb;
					FLOAT3 s2 = SAMPLE_TEX2D(_MainTex, i.uv23.xy).rgb;
					FLOAT3 s3 = SAMPLE_TEX2D(_MainTex, i.uv23.zw).rgb;

					FLOAT w0 = Weigh(s0);
					FLOAT w1 = Weigh(s1);
					FLOAT w2 = Weigh(s2);
					FLOAT w3 = Weigh(s3);

					FLOAT3 color = s0 * w0 + s1 * w1 + s2 * w2 + s3 * w3;
					color /= max(w0 + w1 + w2 + s3, EPSILON);

					FLOAT coc0 = CalcCoc(i.uv01.xy);
					FLOAT coc1 = CalcCoc(i.uv01.zw);
					FLOAT coc2 = CalcCoc(i.uv23.xy);
					FLOAT coc3 = CalcCoc(i.uv23.zw);

					FLOAT cocMin = min(min(min(coc0, coc1), coc2), coc3);
					FLOAT cocMax = max(max(max(coc0, coc1), coc2), coc3);
					FLOAT coc = cocMax >= -cocMin ? cocMax : cocMin;

					return FLOAT4(color, coc);
				}
			ENDHLSL
		}

		// 1 : Bokeh
		Pass {
			HLSLPROGRAM
				#pragma vertex VertDefault
				#pragma fragment frag

				static const int kernelSampleCount = 16;
				static const FLOAT2 kernel[kernelSampleCount] = {
					FLOAT2(0, 0),
					FLOAT2(0.54545456, 0),
					FLOAT2(0.16855472, 0.5187581),
					FLOAT2(-0.44128203, 0.3206101),
					FLOAT2(-0.44128197, -0.3206102),
					FLOAT2(0.1685548, -0.5187581),
					FLOAT2(1, 0),
					FLOAT2(0.809017, 0.58778524),
					FLOAT2(0.30901697, 0.95105654),
					FLOAT2(-0.30901703, 0.9510565),
					FLOAT2(-0.80901706, 0.5877852),
					FLOAT2(-1, 0),
					FLOAT2(-0.80901694, -0.58778536),
					FLOAT2(-0.30901664, -0.9510566),
					FLOAT2(0.30901712, -0.9510565),
					FLOAT2(0.80901694, -0.5877853),
				};

				FLOAT Weigh (FLOAT coc, FLOAT radius) {
					return saturate((coc - radius + 2) * 0.5);
				}

				FLOAT4 frag (Interpolators i) : SV_Target {
					FLOAT coc = SAMPLE_TEX2D(_MainTex, i.uv).a;
					FLOAT3 bgColor = 0, fgColor = 0;
					FLOAT bgWeight = 0, fgWeight = 0;
					for (int k = 0; k < kernelSampleCount; k++) {
						FLOAT2 o = kernel[k].xy * _BokehRadius;
						FLOAT radius = length(o);
						o *= _MainTex_TexelSize.xy;
						FLOAT4 s = SAMPLE_TEX2D(_MainTex, i.uv + o);

						FLOAT bgw = Weigh(max(0, min(s.a, coc)), radius);
						bgColor += s.rgb * bgw;
						bgWeight += bgw;

						FLOAT fgw = Weigh(-s.a, radius);
						fgColor += s.rgb * fgw;
						fgWeight += fgw;
					}
					bgColor *= 1 / (bgWeight + (bgWeight == 0));
					fgColor *= 1 / (fgWeight + (fgWeight == 0));
					FLOAT bgfg = min(1, fgWeight * 3.14159265359 / kernelSampleCount);
					FLOAT3 color = lerp(bgColor, fgColor, bgfg);
					return FLOAT4(color, bgfg);
				}
			ENDHLSL
		}

		// 2 : PostFilter
		Pass {
			HLSLPROGRAM
				#pragma vertex VertDefault
				#pragma fragment frag

				FLOAT4 frag (Interpolators i) : SV_Target {
					FLOAT4 o = _MainTex_TexelSize.xyxy * FLOAT2(-0.5, 0.5).xxyy;
					FLOAT4 s =
						SAMPLE_TEX2D(_MainTex, i.uv + o.xy) +
						SAMPLE_TEX2D(_MainTex, i.uv + o.zy) +
						SAMPLE_TEX2D(_MainTex, i.uv + o.xw) +
						SAMPLE_TEX2D(_MainTex, i.uv + o.zw);
					return s * 0.25;
				}
			ENDHLSL
		}
	}
}