Shader "Hidden/Universal Render Pipeline/EasyDepthOfField"
{
    HLSLINCLUDE
    #pragma target 3.5
    //#pragma exclude_renderers gles
    // #pragma multi_compile _ _USE_DRAW_PROCEDURAL

    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/API/Validate.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
    // #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"


    // TEX2D_SAMPLER(_MainTex);
    float4 _SourceSize;
    TEXTURE2D(_MainTex);
    SAMPLER(sampler_MainTex);
    // float4 _MainTex_TexelSize;
    TEXTURE2D(_CameraDepthTexture);
    SAMPLER(sampler_CameraDepthTexture);
    TEXTURE2D(_DofTex);
    SAMPLER(sampler_DofTex);

    float4 _CoCParams;
    float4 _CoCParams2;

    #define _EasyMode _CoCParams.x
    #define _FocusDistance _CoCParams.y
    #define _FocusRangeNear _CoCParams.z
    #define _FocusRangeFar _CoCParams.w
    #define _BlurRadius _CoCParams2.x
    #define _BlurIter _CoCParams2.y
    #define _DOFIntensity _CoCParams2.z
    #define _BokehRadius 5.0
    #define _FarScale 20
    #define _NearScale 1

    half CalcCoc(float2 uv) : SV_Target
    {
        float2 depthUV = uv /** _SourceSize.zw*/;
        float depth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, depthUV);
        depth = LinearEyeDepth(depth, _ZBufferParams);
        float delta = depth - _FocusDistance;
        float coc;

        UNITY_BRANCH
        if (_EasyMode > 0.5)
        {
            // 慢的算法
            //coc = delta > 0
            //	? delta / (_FocusDistance * (_FocusDistance + 1))
            //	: depth / _FocusDistance - 1;

            // 快的算法
            float scaler = delta > 0 ? _FarScale : _NearScale;
            coc = (depth - _FocusDistance) / (_FocusDistance * scaler);
        }
        else
        {
            coc = (depth - _FocusDistance) / (delta > 0 ? _FocusRangeFar : _FocusRangeNear);
        }
        coc = clamp(coc, -1, 1) * _BokehRadius;
        return coc;
    }

    //    float2 TransformStereoScreenSpaceTex(float2 uv, float w)
    // {
    // 	float4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
    // 	// scaleOffset.xy *= _RenderViewportScaleFactor;
    // 	return uv.xy * scaleOffset.xy + scaleOffset.zw * w;
    // }

    struct AttributesDefault
    {
        float3 vertex : POSITION;
    };

    struct VaryingsDefault
    {
        float4 vertex : SV_POSITION;
        float2 texcoord : TEXCOORD0;
        // float2 texcoordStereo : TEXCOORD1;
    };

    VaryingsDefault VertDefault(AttributesDefault v)
    {
        VaryingsDefault o;
        o.vertex = float4(v.vertex.xy, 0.0, 1.0);
        o.texcoord = v.vertex.xy * 0.5 + float2(0.5, 0.5);

        #if UNITY_UV_STARTS_AT_TOP
        o.texcoord = o.texcoord * float2(1.0, -1.0) + float2(0.0, 1.0);
        #endif

        // o.texcoordStereo = TransformStereoScreenSpaceTex(o.texcoord, 1.0);

        return o;
    }
    ENDHLSL

    SubShader
    {
        Cull Off
        ZTest Always
        ZWrite Off

        HLSLINCLUDE
        struct Interpolators
        {
            float4 pos : SV_POSITION;
            float2 uv : TEXCOORD0;
        };

        // #include "DepthOfField.hlsl"
        ENDHLSL

        // 0 : PreFilter
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 uv01 : TEXCOORD0;
                float4 uv23 : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = float4(v.vertex.xy, 0.0, 1.0);
                float2 uv = v.vertex.xy * 0.5 + float2(0.5, 0.5);
                #if UNITY_UV_STARTS_AT_TOP
                uv = uv * float2(1.0, -1.0) + float2(0.0, 1.0);
                #endif
                float4 offset = _SourceSize.zwzw * float4(-1.0, -1.0, 1.0, 1.0);
                o.uv01 = uv.xyxy + offset.xyzw;
                o.uv23 = uv.xyxy + offset.xwzw;
                return o;
            }

            float Weigh(float3 c)
            {
                return max(max(c.r, c.g), c.b);
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 s0 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv01.xy).rgb;
                float3 s1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv01.zw).rgb;
                float3 s2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv23.xy).rgb;
                float3 s3 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv23.zw).rgb;

                float w0 = Weigh(s0);
                float w1 = Weigh(s1);
                float w2 = Weigh(s2);
                float w3 = Weigh(s3);

                float3 color = s0 * w0 + s1 * w1 + s2 * w2 + s3 * w3;
                color /= max(w0 + w1 + w2 + s3, 1.0e-4);
                float coc0 = CalcCoc(i.uv01.xy);
                float coc1 = CalcCoc(i.uv01.zw);
                float coc2 = CalcCoc(i.uv23.xy);
                float coc3 = CalcCoc(i.uv23.zw);

                float cocMin = min(min(min(coc0, coc1), coc2), coc3);
                float cocMax = max(max(max(coc0, coc1), coc2), coc3);
                float coc = cocMax >= -cocMin ? cocMax : cocMin;
                // return 1- abs(coc0-.5f)*2;
                // return coc0>-.1f && coc0 < .1f;
                return float4(color, coc);
            }
            ENDHLSL
        }

        // 1 : Bokeh
        Pass
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment frag

            static const int kernelSampleCount = 16;
            static const float2 kernel[kernelSampleCount] = {
                float2(0, 0),
                float2(0.54545456, 0),
                float2(0.16855472, 0.5187581),
                float2(-0.44128203, 0.3206101),
                float2(-0.44128197, -0.3206102),
                float2(0.1685548, -0.5187581),
                float2(1, 0),
                float2(0.809017, 0.58778524),
                float2(0.30901697, 0.95105654),
                float2(-0.30901703, 0.9510565),
                float2(-0.80901706, 0.5877852),
                float2(-1, 0),
                float2(-0.80901694, -0.58778536),
                float2(-0.30901664, -0.9510566),
                float2(0.30901712, -0.9510565),
                float2(0.80901694, -0.5877853),
            };

            float Weigh(float coc, float radius)
            {
                return saturate((coc - radius + 2) * 0.5);
            }


			float4 frag (Interpolators i) : SV_Target {
				float coc = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).a;
				float3 bgColor = 0, fgColor = 0;
				float bgWeight = 0, fgWeight = 0;
				for (int k = 0; k < kernelSampleCount; k++) {
					float2 o = kernel[k].xy * _BokehRadius;
					float radius = length(o);
					o *= _SourceSize.zw;
				    
					float4 s = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + o);

					float bgw = Weigh(max(0, min(s.a, coc)), radius);
					bgColor += s.rgb * bgw;
					bgWeight += bgw;

					float fgw = Weigh(-s.a, radius);
					fgColor += s.rgb * fgw;
					fgWeight += fgw;
				}
				bgColor *= 1 / (bgWeight + (bgWeight == 0));
				fgColor *= 1 / (fgWeight + (fgWeight == 0));
				float bgfg = min(1, fgWeight * 3.14159265359 / kernelSampleCount);
				float3 color = lerp(bgColor, fgColor, bgfg);
				return float4(color, bgfg);
			}
			ENDHLSL
		}

        // 2 : PostFilter
        Pass
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma fragment frag


            float4 frag(Interpolators i) : SV_Target
            {

                float4 o = _SourceSize.zwzw * float2(-.5, .5).xxyy * _BlurRadius *(1+ _BlurIter);
                				    
					// float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv + o);
				 //    depth = LinearEyeDepth(depth,_ZBufferParams);
				    // if(depth < _CoCParams.y)
                float4 s = 0 ;
                float depth = 0;
                float sub = 0;
                float bias = 0.5;
                float center = _CoCParams.y/* - bias*/;
                depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv + o.xy);
				depth = LinearEyeDepth(depth,_ZBufferParams) > center;
                s += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + o.xy) * depth;
                sub += depth;

                depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv + o.zy);
				depth = LinearEyeDepth(depth,_ZBufferParams) >  center;
                s += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + o.zy) * depth;
                sub += depth;

                depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv + o.xw);
				depth = LinearEyeDepth(depth,_ZBufferParams) > center;
                s += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + o.xw) * depth;
                sub += depth;

                depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv + o.zw);
				depth = LinearEyeDepth(depth,_ZBufferParams) > center;
                s += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + o.zw) * depth;
                sub += depth;

                if(sub == 0)
                {
                    s += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                    sub = 1;
                }
                    // SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + o.xy) +
                    // SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + o.zy) +
                    // SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + o.xw) +
                    // SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + o.zw);
                // return depth;
                return s / sub;
            }
            ENDHLSL
        }

        // 3 : End
        Pass
        {
            HLSLPROGRAM
            #pragma vertex VertDefault
            #pragma  fragment frag

            float4 frag(Interpolators i) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half coc = CalcCoc(i.uv);
                half4 dof = SAMPLE_TEXTURE2D(_DofTex, sampler_DofTex, i.uv);
                // return dof;
                half dofStrength = smoothstep(0.2, 1, abs(coc));
                half t = dofStrength + dof.a - dofStrength * dof.a;
                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.uv);
				depth = LinearEyeDepth(depth,_ZBufferParams) > _CoCParams.y;
                // return dof;
                color.rgb = lerp(color.rgb, dof.rgb, saturate(t*_DOFIntensity * depth));
                return color;
            }
            ENDHLSL
        }
    }
}