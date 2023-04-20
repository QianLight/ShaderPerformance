Shader "Hidden/PostProcessing/Uber"
{
    HLSLINCLUDE

        #pragma target 3.0

        // #pragma multi_compile __ FINALPASS

        #if defined(_DEBUG_APP)&&!defined(SHADER_API_MOBILE)
        #define _ENABLE_DEBUG
        #endif

        #include "../StdLib.hlsl"
        #include "../Colors.hlsl"
        #include "../Sampling.hlsl"
        #include "Distortion.hlsl"
        #include "Dithering.hlsl"

        TEX2D_SAMPLER(_MainTex);
        FLOAT4 _MainTex_TexelSize;
        TEX2D_SAMPLER(_CameraDepthRT);
		FLOAT4 _Global_Setting;
		FLOAT4 _Global_Setting2;

        // FLOAT4 _Global_Setting2;
        // Bloom
		#define bloom_enable _Global_Setting.x>0.5
        TEX2D_SAMPLER(_BloomTex);
        FLOAT4 _BloomTex_TexelSize;
        FLOAT4 _BloomColor;
        #define _BloomTint _BloomColor.xyz
        #define _BloomIntensity _BloomColor.w

        //God Ray
		#define godray_enable _Global_Setting.y>0.5
        TEX2D_SAMPLER(_GodRayTex);
        FLOAT4 _SunColor;
        #define godray_LightFactor _SunColor.w

        // Color grading
		#define unityCustomTonemapping_enable  _Global_Setting.z>0.5
		TEX2D_SAMPLER(_Lut2D);
        FLOAT4 _Lut_Params;
        #define _LutParams               _Lut_Params.xyz
        #define _PostExposure            _Lut_Params.w
        #define _CustomLutEnable         (_Global_Setting2.w > 0.5)

        // Vignette
        FLOAT4 _Vignette_Settings; // x: intensity, y: smoothness, z: roundness, w: rounded
        #define vignette_enable _Vignette_Settings.x>0
        #define vignette_intensity _Vignette_Settings.x
        #define vignette_smoothness _Vignette_Settings.y
        #define vignette_roundness _Vignette_Settings.z
        #define vignette_rounded _Vignette_Settings.w
		FLOAT3 _Vignette_Color;
		FLOAT2 _Vignette_Center; // UV space

        // DOF
        #define dof_enable _Global_Setting2.x > 0.5
        TEX2D_SAMPLER(_DofTex);
        #include "DepthOfField.hlsl"

        // Distortion
        #define distortion_enable _Global_Setting2.y > 0.5
        TEX2D_SAMPLER(_DistortionTex);

        // RadialBlur
        #define radial_blur_enable _Global_Setting2.z > 0.5
		TEX2D_SAMPLER(_RadialBlurTex);
		TEX2D_SAMPLER(_RadialBlurDebugTex);

        // #define fxaa_enable _Global_Setting2.x>0.5
        #define FXAA_SPAN_MAX           (8.0)
        #define FXAA_REDUCE_MUL         (1.0 / 8.0)
        #define FXAA_REDUCE_MIN         (1.0 / 128.0)
        
		////background
		//#define background_enable _Global_Setting.w>0.5
  //      TEX2D_SAMPLER(_BackgroundTex);        
  //      FLOAT4 _BackgroundUV;
  //      #define _BackStartU _BackgroundUV.x
  //      #define _BackEndU _BackgroundUV.y
  //      #define _BlackLerp _BackgroundUV.z
  //      FLOAT4 _BackgroundColor;

        // Custom lut
        TEX2D_SAMPLER(_CustomLut2D);
        float4 _CustomLut2D_TexelSize;
        float4 _CustomLut_Params;
        #define _CustomLut_ScaleOffset (_CustomLut_Params.xyz)
        #define _CustomLut_Contribution (_CustomLut_Params.w)
        float4 _LutBake_Params;
        float4 _CustomToneCurve;
        float4 _ToeSegmentA;
        float4 _ToeSegmentB;
        float4 _MidSegmentA;
        float4 _MidSegmentB;
        float4 _ShoSegmentA;
        float4 _ShoSegmentB;
        float4 _CustomLutParam;

		#include "Debug.hlsl"

        //FXAA

        #define FXAA_Enable  _Global_Setting.w>0.5
        FLOAT4 _AntLevel;
	  	#define _SubpixelBlending _AntLevel.z;
     //   #define _HIGH_QUALITY _AntLevel.w>0.7;
       // #define _LOW_QUALITY _AntLevel.w<0.5;

        FLOAT3 Fetch(FLOAT2 coords, FLOAT2 offset)
        {
            FLOAT2 uv = coords + offset;
            return SAMPLE_TEX2D(_MainTex, uv).xyz;
        }

        FLOAT3 Load(int2 icoords, int idx, int idy)
        {
            return LOAD_TEX2D(_MainTex, clamp(icoords + int2(idx, idy), 0, _MainTex_TexelSize.zw - 1.0)).xyz;
        }

        FLOAT LinearRgbToLuminance(FLOAT3 linearRgb) {
	      return dot(linearRgb, FLOAT3(0.2126729,  0.7151522, 0.0721750));
        }

        FLOAT4  _Sample(FLOAT2 uv) 
        {       
			  return SAMPLE_TEX2D_LOD(_MainTex,uv,0);
		}

        FLOAT SampleLuminance (FLOAT2 uv) 
        {       
            return LinearRgbToLuminance(_Sample(uv).xyz);
			//#if defined(LUMINANCE_GREEN)
			//	return _Sample(uv).g;
			//#else
			//	return _Sample(uv).a;
			//#endif
		}
     
		FLOAT SampleLuminance (FLOAT2 uv, FLOAT uOffset, FLOAT vOffset) {
			uv += _MainTex_TexelSize.xy * FLOAT2(uOffset, vOffset);
			return SampleLuminance(uv);
		}
        struct LuminanceData
        {
			FLOAT m, n, e, s, w;
			FLOAT ne, nw, se, sw;
			FLOAT highest, lowest, contrast;
		};
        LuminanceData SampleLuminanceNeighborhood (FLOAT2  uv) 
        {
			LuminanceData l;
			l.m = SampleLuminance(uv);
			l.n = SampleLuminance(uv,  0,  1);
			l.e = SampleLuminance(uv,  1,  0);
			l.s = SampleLuminance(uv,  0, -1);
			l.w = SampleLuminance(uv, -1,  0);

			l.ne = SampleLuminance(uv,  1,  1);
			l.nw = SampleLuminance(uv, -1,  1);
			l.se = SampleLuminance(uv,  1, -1);
			l.sw = SampleLuminance(uv, -1, -1);

			l.highest = max(max(max(max(l.n, l.e), l.s), l.w), l.m);
			l.lowest = min(min(min(min(l.n, l.e), l.s), l.w), l.m);
			l.contrast = l.highest - l.lowest;
			return l;
		}

        bool ShouldSkipPixel (LuminanceData l) 
        {
			FLOAT threshold =max(_AntLevel.x, _AntLevel.y* l.highest);
			return l.contrast<threshold;
		}

        FLOAT DeterminePixelBlendFactor (LuminanceData l)
        {
			FLOAT filter = 2 * (l.n + l.e + l.s + l.w);
			filter += l.ne + l.nw + l.se + l.sw;
			filter *= 1.0 / 12;
			filter = abs(filter - l.m);
			filter = saturate(filter / l.contrast);

			FLOAT blendFactor = smoothstep(0, 1, filter);
			return blendFactor * blendFactor * _SubpixelBlending;
		}

        struct EdgeData 
        {
			bool isHorizontal;
			FLOAT pixelStep;
			FLOAT oppositeLuminance, gradient;
		};

        
		EdgeData DetermineEdge (LuminanceData l) {
			EdgeData e;
			FLOAT horizontal =
				abs(l.n + l.s - 2 * l.m) * 2 +
				abs(l.ne + l.se - 2 * l.e) +
				abs(l.nw + l.sw - 2 * l.w);
			FLOAT vertical =
				abs(l.e + l.w - 2 * l.m) * 2 +
				abs(l.ne + l.nw - 2 * l.n) +
				abs(l.se + l.sw - 2 * l.s);
			e.isHorizontal = horizontal >= vertical;

			FLOAT pLuminance = e.isHorizontal ? l.n : l.e;
			FLOAT nLuminance = e.isHorizontal ? l.s : l.w;
			FLOAT pGradient = abs(pLuminance - l.m);
			FLOAT nGradient = abs(nLuminance - l.m);

			e.pixelStep =
				e.isHorizontal ? _MainTex_TexelSize.y : _MainTex_TexelSize.x;
			
			if (pGradient < nGradient) {
				e.pixelStep = -e.pixelStep;
				e.oppositeLuminance = nLuminance;
				e.gradient = nGradient;
			}
			else {
				e.oppositeLuminance = pLuminance;
				e.gradient = pGradient;
			}
			return e;
		}
    

        FLOAT DetermineEdgeBlendFactor (LuminanceData l, EdgeData e, FLOAT2 uv) {
			FLOAT2 uvEdge = uv;
			FLOAT2 edgeStep;
			if (e.isHorizontal) {
				uvEdge.y += e.pixelStep * 0.5;
				edgeStep = FLOAT2(_MainTex_TexelSize.x, 0);
			}
			else {
				uvEdge.x += e.pixelStep * 0.5;
				edgeStep = FLOAT2(0, _MainTex_TexelSize.y);
			}

			FLOAT edgeLuminance = (l.m + e.oppositeLuminance) * 0.5;
			FLOAT gradientThreshold = e.gradient * 0.25;
              // qika

          FLOAT2 puv;
          FLOAT pLuminanceDelta;
          FLOAT2 nuv;
          FLOAT nLuminanceDelta;


            if(_AntLevel.w>0.7)
            {
                 FLOAT EDGE_STEP_COUNT=10;
                 FLOAT EDGE_GUESS=8;
                 FLOAT edgeSteps[10] ={1, 1.5, 2, 2, 2, 2, 2, 2, 2, 4};
                // FLOAT edgeSteps[12] ={0.2, 0.2, 0.2, 0.2, 0.2, 0.3, 0.3, 0.3, 0.5, 0.5,0.5,0.5};
			     puv = uvEdge + edgeStep * edgeSteps[0];
			     pLuminanceDelta = SampleLuminance(puv) - edgeLuminance;
			    bool pAtEnd = abs(pLuminanceDelta) >= gradientThreshold;
                // int i=0;
			    //UNITY_UNROLL
			    for (int i = 1; i < EDGE_STEP_COUNT && !pAtEnd; i++) {
				    puv += edgeStep * edgeSteps[i];
				    pLuminanceDelta = SampleLuminance(puv) - edgeLuminance;
				    pAtEnd = abs(pLuminanceDelta) >= gradientThreshold;
			    }
			    if (!pAtEnd) {
				    puv += edgeStep * EDGE_GUESS;
			    }

			     nuv = uvEdge - edgeStep * edgeSteps[0];
			     nLuminanceDelta = SampleLuminance(nuv) - edgeLuminance;
			    bool nAtEnd = abs(nLuminanceDelta) >= gradientThreshold;
             //   int i=0;
			   // UNITY_UNROLL
			    for (int i = 1; i < EDGE_STEP_COUNT && !nAtEnd; i++) {
				    nuv -= edgeStep * edgeSteps[i];
				    nLuminanceDelta = SampleLuminance(nuv) - edgeLuminance;
				    nAtEnd = abs(nLuminanceDelta) >= gradientThreshold;
			    }
			    if (!nAtEnd) {
				    nuv -= edgeStep * EDGE_GUESS;
			    }
            }
            else
            {
                 FLOAT EDGE_STEP_COUNT=4;
                 FLOAT EDGE_GUESS=12;
                 FLOAT edgeSteps[4] ={1, 1.5,2,4};                
		        puv = uvEdge + edgeStep * edgeSteps[0];
		        pLuminanceDelta = SampleLuminance(puv) - edgeLuminance;
			    bool pAtEnd = abs(pLuminanceDelta) >= gradientThreshold;
                // int i=0;
			   // UNITY_UNROLL
			    for (int i = 1; i < EDGE_STEP_COUNT && !pAtEnd; i++) {
				    puv += edgeStep * edgeSteps[i];
				    pLuminanceDelta = SampleLuminance(puv) - edgeLuminance;
				    pAtEnd = abs(pLuminanceDelta) >= gradientThreshold;
			    }
			    if (!pAtEnd) {
				    puv += edgeStep * EDGE_GUESS;
			    }

			     nuv = uvEdge - edgeStep * edgeSteps[0];
			     nLuminanceDelta = SampleLuminance(nuv) - edgeLuminance;
			    bool nAtEnd = abs(nLuminanceDelta) >= gradientThreshold;
             //   int i=0;
			   // UNITY_UNROLL
			    for (int i = 1; i < EDGE_STEP_COUNT && !nAtEnd; i++) {
				    nuv -= edgeStep * edgeSteps[i];
				    nLuminanceDelta = SampleLuminance(nuv) - edgeLuminance;
				    nAtEnd = abs(nLuminanceDelta) >= gradientThreshold;
			    }
			    if (!nAtEnd) {
				    nuv -= edgeStep * EDGE_GUESS;
			    }
            }
          //  #endif


			FLOAT pDistance, nDistance;
			if (e.isHorizontal) {
				pDistance = puv.x - uv.x;
				nDistance = uv.x - nuv.x;
			}
			else {
				pDistance = puv.y - uv.y;
				nDistance = uv.y - nuv.y;
			}

			FLOAT shortestDistance;
			bool deltaSign;
			if (pDistance <= nDistance) {
				shortestDistance = pDistance;
				deltaSign = pLuminanceDelta >= 0;
			}
			else {
				shortestDistance = nDistance;
				deltaSign = nLuminanceDelta >= 0;
			}

			if (deltaSign == (l.m - edgeLuminance >= 0)) {
				return 0;
			}
			return 0.5 - shortestDistance / (pDistance + nDistance);
		}

        FLOAT4 ApplyFXAA (FLOAT2 uv) {

			LuminanceData l = SampleLuminanceNeighborhood(uv);
			if (ShouldSkipPixel(l)) {
				return _Sample(uv);
			}
			FLOAT pixelBlend = DeterminePixelBlendFactor(l);
			EdgeData e = DetermineEdge(l);
			FLOAT edgeBlend = DetermineEdgeBlendFactor(l, e, uv);
			FLOAT finalBlend = max(pixelBlend, edgeBlend);

			if (e.isHorizontal) {
				uv.y += e.pixelStep * finalBlend;
			}
			else {
				uv.x += e.pixelStep * finalBlend;
			}
			return FLOAT4(_Sample(uv).rgb, l.m);
		}


        FLOAT4 FragUber(VaryingsDefault i): SV_Target
        {
			DEBUG_PP_CUSTOMDATA
            FLOAT2 uv = i.texcoord.xy;
            
            UNITY_BRANCH
			if(distortion_enable)
			{
				FLOAT3 distortion = SAMPLE_TEX2D(_DistortionTex, i.texcoord.xy).xyz;
				uv += (distortion.xy - 0.5) * distortion.z;
			}

            FLOAT2 uvDistorted = Distort(uv);
            FLOAT4 color;

            color = SAMPLE_TEX2D(_MainTex, uvDistorted);
            DEBUG_PP_CUSTOMDATA_PARAM(src, color)

            UNITY_BRANCH
            if (radial_blur_enable)
            {
                FLOAT4 radialBlur = SAMPLE_TEX2D(_RadialBlurTex, uv);
				color.rgb = lerp(color.rgb, radialBlur.rgb, radialBlur.a);
                
                #ifdef _ENABLE_DEBUG
                    FLOAT4 radialBlurDebugColor = SAMPLE_TEX2D(_RadialBlurDebugTex, uv);
				    FLOAT3 areaColor = radialBlurDebugColor.rgb * radialBlurDebugColor.a;
                    DEBUG_PP_CUSTOMDATA_PARAM(radialBlurArea, areaColor)
                #endif
            }

            UNITY_BRANCH
            if (dof_enable)
            {
                half coc = CalcCoc(uv);
				half4 dof = SAMPLE_TEX2D(_DofTex, uv);
                half dofStrength = smoothstep(0.2, 1, abs(coc));
                half t = dofStrength + dof.a - dofStrength * dof.a;
		        color.rgb = lerp(color.rgb, dof.rgb, t);
            }
            
			UNITY_BRANCH
			if (bloom_enable)
            {
                FLOAT3 bloom = SampleTexture2DBicubic(TEX2D_PARAM(_BloomTex), uvDistorted, _BloomTex_TexelSize.zwxy,1).xyz;
                color.xyz += bloom;
                DEBUG_PP_CUSTOMDATA_PARAM(bloom, bloom)
            }

            UNITY_BRANCH
			if (godray_enable)
            {
                FLOAT4 blur = SAMPLE_TEX2D(_GodRayTex, uvDistorted);
                //FLOAT3 godray = godray_LightFactor * blur.xyz;
                FLOAT3 godray = blur.xyz;
                color.xyz += godray.xyz;
                DEBUG_PP_CUSTOMDATA_PARAM(godray, godray.xyz)
            }

            UNITY_BRANCH
			if (unityCustomTonemapping_enable)
			{
                if (_CustomLutEnable)
                {
                    // TODO : 这个分支是可以被优化掉的，在BakeLutPass中烘焙出对应的HDR Lut图，可以省下Tonemapping的开销。
                    half3 input = CustomTonemap(
                        color.rgb, _CustomToneCurve.xyz,
                        _ToeSegmentA, _ToeSegmentB.xy,
                        _MidSegmentA, _MidSegmentB.xy,
                        _ShoSegmentA, _ShoSegmentB.xy
                    );

                    input.rgb = LinearToSRGB(input.rgb);
                    half3 outLut = ApplyLut2D(TEX2D_PARAM(_CustomLut2D), input, _CustomLut_ScaleOffset);
                    input = lerp(input, outLut, _CustomLut_Contribution);
                    input.rgb = SRGBToLinear(input.rgb);
                    color.rgb = input;
                }
                else
                {
                    FLOAT3 srcColor = color.rgb;
                    FLOAT lum = saturate(Max3(srcColor.r, srcColor.g, srcColor.b) - 2);
                    color.rgb = ApplyColorGrading(color.rgb, _PostExposure, TEX2D_PARAM(_Lut2D), _LutParams);
                    color.rgb = lerp(color.rgb, srcColor, lum);
                }
            }

			UNITY_BRANCH
			if (vignette_enable)
            {
				FLOAT2 d = abs(uvDistorted - _Vignette_Center) * vignette_intensity;
				d.x *= lerp(1.0, _ScreenParams.x / _ScreenParams.y, vignette_rounded);
				d = pow(saturate(d), vignette_roundness); // Roundness
				FLOAT vfactor = pow(saturate(1.0 - dot(d, d)), vignette_smoothness);
                color = saturate(color);
				color.rgb *= lerp(_Vignette_Color, (1.0).xxx, vfactor);
				color.a = lerp(1.0, color.a, vfactor);
            }
         
    //        UNITY_BRANCH
    //         if(FXAA_Enable)
    //         {
    //           // color.a = LinearRgbToLuminance(color.rgb);
    //         //   FXAAAPLHA= LinearRgbToLuminance(color.rgb);
				////#if defined(GAMMA_BLENDING)
				////color.rgb = pow(color.rgb,0.454545);
				////#endif           
    //         }
      

            // UNITY_BRANCH
            // if(background_enable)
            // {
            //     FLOAT depth = saturate(Linear01DepthPers(FetchFromDepthTex(TEX2D_PARAM(_CameraDepthRT), uv)));
            //     FLOAT mask = depth>0.7;
            //     FLOAT2 backUV = FLOAT2(lerp(_BackStartU,_BackEndU,uv.x),uv.y);
            //     FLOAT3 backTex = SAMPLE_TEX2D(_BackgroundTex, backUV).xyz;
            //     backTex = lerp(backTex,_BackgroundColor.xyz,_BackgroundColor.w);
            //     FLOAT3 backLum = Luminance(backTex);
            //     backTex = lerp(backTex,backLum,_BlackLerp);
            //     color.rgb = lerp(color.rgb,backTex,mask);
            // }

               FLOAT4 output = color;

            //output.rgb = Dither(output.rgb, uv);
			DEBUG_PP_COLOR(output, uv)
            return output;

        }

        FLOAT4 FXAALOW(VaryingsDefault i) 
        {
            
            FLOAT2 positionNDC = i.texcoord.xy;
            int2   positionSS  = i.texcoord.xy * _MainTex_TexelSize.zw;
               FLOAT3 color = Load(positionSS, 0, 0).xyz;
            // UNITY_BRANCH
            // if(fxaa_enable)
            // {
                // Edge detection
                FLOAT3 rgbNW = Load(positionSS, -1, -1);
                FLOAT3 rgbNE = Load(positionSS,  1, -1);
                FLOAT3 rgbSW = Load(positionSS, -1,  1);
                FLOAT3 rgbSE = Load(positionSS,  1,  1);

                rgbNW = saturate(rgbNW);
                rgbNE = saturate(rgbNE);
                rgbSW = saturate(rgbSW);
                rgbSE = saturate(rgbSE);
                color = saturate(color);

                FLOAT lumaNW = Luminance(rgbNW);
                FLOAT lumaNE = Luminance(rgbNE);
                FLOAT lumaSW = Luminance(rgbSW);
                FLOAT lumaSE = Luminance(rgbSE);
                FLOAT lumaM = Luminance(color);

                FLOAT2 dir;
                dir.x = -((lumaNW + lumaNE) - (lumaSW + lumaSE));
                dir.y = ((lumaNW + lumaSW) - (lumaNE + lumaSE));

                FLOAT lumaSum = lumaNW + lumaNE + lumaSW + lumaSE;
                FLOAT dirReduce = max(lumaSum * (0.25 * FXAA_REDUCE_MUL), FXAA_REDUCE_MIN);
                FLOAT rcpDirMin = rcp(min(abs(dir.x), abs(dir.y)) + dirReduce);

                dir = min((FXAA_SPAN_MAX).xx, max((-FXAA_SPAN_MAX).xx, dir * rcpDirMin)) * _MainTex_TexelSize.xy;

                // Blur
                FLOAT3 rgb03 = Fetch(positionNDC, dir * (0.0 / 3.0 - 0.5));
                FLOAT3 rgb13 = Fetch(positionNDC, dir * (1.0 / 3.0 - 0.5));
                FLOAT3 rgb23 = Fetch(positionNDC, dir * (2.0 / 3.0 - 0.5));
                FLOAT3 rgb33 = Fetch(positionNDC, dir * (3.0 / 3.0 - 0.5));

                rgb03 = saturate(rgb03);
                rgb13 = saturate(rgb13);
                rgb23 = saturate(rgb23);
                rgb33 = saturate(rgb33);

                FLOAT3 rgbA = 0.5 * (rgb13 + rgb23);
                FLOAT3 rgbB = rgbA * 0.5 + 0.25 * (rgb03 + rgb33);

                FLOAT lumaB = Luminance(rgbB);

                FLOAT lumaMin = Min3(lumaM, lumaNW, Min3(lumaNE, lumaSW, lumaSE));
                FLOAT lumaMax = Max3(lumaM, lumaNW, Max3(lumaNE, lumaSW, lumaSE));

                color.xyz = ((lumaB < lumaMin) || (lumaB > lumaMax)) ? rgbA : rgbB;
            // }
            return FLOAT4(color,1);
        }

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Off

        Pass
        {
            Name "Uber"
            HLSLPROGRAM

                #pragma vertex VertUVTransform
                #pragma fragment FragUber
                #pragma shader_feature_local _ _DEBUG_APP
            ENDHLSL
        }

        
        Pass
        {
            Name "FinalFXAA"

            HLSLPROGRAM
                #pragma vertex VertUVTransform
                #pragma fragment FragmentProgram
               // #pragma fragment FragFinal
               //  #pragma multi_compile _ LUMINANCE_GREEN
		       //  #pragma multi_compile _ LOW_QUALITY
			   //#pragma multi_compile _ GAMMA_BLENDING
              
                FLOAT4 FragmentProgram (VaryingsDefault i) : SV_Target {                 
                    FLOAT4 sample;
                     sample = ApplyFXAA(i.texcoord.xy);  

                     //if (_AntLevel.w<0.5){
                     //     sample= FXAALOW(i);  
                     //}else{
                     //     sample = ApplyFXAA(i.texcoord.xy);  
                     //}                                                                     
                     return sample;  
				}

            ENDHLSL
        }

    }
}
