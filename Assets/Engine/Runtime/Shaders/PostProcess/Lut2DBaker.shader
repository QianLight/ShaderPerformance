Shader "Hidden/PostProcessing/Lut2DBaker"
{
    HLSLINCLUDE

        #pragma target 3.0
        #include "../StdLib.hlsl"
        #include "../Colors.hlsl"
        #include "../ACES.hlsl"

        TEX2D_SAMPLER(_MainTex);
		// FLOAT _ToneMode;
        FLOAT4 _Lut2D_Params;
        FLOAT4 _UserLut2D_Params;
        
        FLOAT3 _ColorBalance;
        FLOAT3 _ColorFilter;
        FLOAT3 _HueSatCon;
        FLOAT _Brightness; // LDR only

        FLOAT3 _ChannelMixerRed;
        FLOAT3 _ChannelMixerGreen;
        FLOAT3 _ChannelMixerBlue;

        FLOAT3 _Lift;
        FLOAT3 _InvGamma;
        FLOAT3 _Gain;

        TEX2D_SAMPLER(_Curves);
        
        FLOAT4 _CustomToneCurve;
        FLOAT4 _ToeSegmentA;
        FLOAT4 _ToeSegmentB;
        FLOAT4 _MidSegmentA;
        FLOAT4 _MidSegmentB;
        FLOAT4 _ShoSegmentA;
        FLOAT4 _ShoSegmentB;

        // uint OutputGamut;
        // FLOAT4 ColorShadow_Tint2;
        // #define LegacyTonemapping ColorShadow_Tint2.w <0.5
        FLOAT3 ApplyCommonGradingSteps(FLOAT3 colorLinear)
        {
            colorLinear = WhiteBalance1(colorLinear, _ColorBalance);
            colorLinear *= _ColorFilter;
            colorLinear = ChannelMixer(colorLinear, _ChannelMixerRed, _ChannelMixerGreen, _ChannelMixerBlue);
            colorLinear = LiftGammaGainHDR(colorLinear, _Lift, _InvGamma, _Gain);

            // Do NOT feed negative values to RgbToHsv or they'll wrap around
            colorLinear = max(0.0, colorLinear);

            FLOAT3 hsv = RgbToHsv(colorLinear);

            // Hue Vs Sat
            FLOAT satMult;
            satMult = saturate(SAMPLE_TEX2D_LOD(_Curves, FLOAT2(hsv.x, 0.25), 0).y) * 2.0;

            // Sat Vs Sat
            satMult *= saturate(SAMPLE_TEX2D_LOD(_Curves, FLOAT2(hsv.y, 0.25), 0).z) * 2.0;

            // Lum Vs Sat
            satMult *= saturate(SAMPLE_TEX2D_LOD(_Curves, FLOAT2(Luminance(colorLinear), 0.25), 0).w) * 2.0;

            // Hue Vs Hue
            FLOAT hue = hsv.x + _HueSatCon.x;
            FLOAT offset = saturate(SAMPLE_TEX2D_LOD(_Curves, FLOAT2(hue, 0.25), 0).x) - 0.5;
            hue += offset;
            hsv.x = RotateHue(hue, 0.0, 1.0);

            colorLinear = HsvToRgb(hsv);
            colorLinear = Saturation(colorLinear, _HueSatCon.y * satMult);

            return colorLinear;
        }

        //
        // LDR Grading process
        //
        FLOAT3 ColorGradeLDR(FLOAT3 colorLinear)
        {
           // Brightness is a simple linear multiplier. Works better in LDR than using e.v.
           colorLinear *= _Brightness;

           // Contrast is done in linear, switching to log for that in LDR is pointless and doesn't
           // feel as good to tweak
           const FLOAT kMidGrey = pow(0.5, 2.2);
           colorLinear = Contrast(colorLinear, kMidGrey, _HueSatCon.z);

           colorLinear = ApplyCommonGradingSteps(colorLinear);

           // YRGB only works in LDR for now as we don't do any curve range remapping
           colorLinear = YrgbCurve(saturate(colorLinear), TEX2D_PARAM(_Curves));

           return saturate(colorLinear);
        }

        FLOAT4 FragLDRFromScratch(VaryingsDefault i) : SV_Target
        {
           FLOAT3 colorLinear = GetLutStripValue(i.texcoord, _Lut2D_Params);
           FLOAT3 graded = ColorGradeLDR(colorLinear);
           return FLOAT4(graded, 1.0);
        }

        // FLOAT4 FragLDR(VaryingsDefault i) : SV_Target
        // {
        //    FLOAT3 colorLinear = SAMPLE_TEX2D(_MainTex, i.texcoord).rgb;
        //    FLOAT3 graded = ColorGradeLDR(colorLinear);
        //    return FLOAT4(graded, 1.0);
        // }
        FLOAT4 FragLDR(VaryingsDefault i) : SV_Target
        {
            // Note: user luts may not have the same size as the internal one
            FLOAT3 neutralColorLinear = GetLutStripValue(i.texcoord, _Lut2D_Params);
            FLOAT3 lookup = ApplyLut2D(TEX2D_PARAM(_MainTex), neutralColorLinear, _UserLut2D_Params.xyz);
            FLOAT3 colorLinear = lerp(neutralColorLinear, lookup, _UserLut2D_Params.w);
            FLOAT3 graded = ColorGradeLDR(colorLinear);
            return FLOAT4(graded, 1.0);
        }
        //
        // HDR Grading process
        //
        FLOAT3 LogGradeHDR(FLOAT3 colorLog)
        {
            // HDR contrast feels a lot more natural when done in log rather than doing it in linear
            colorLog = Contrast(colorLog, ACEScc_MIDGRAY, _HueSatCon.z);
            return colorLog;
        }

        FLOAT3 LinearGradeHDR(FLOAT3 colorLinear)
        {
            colorLinear = ApplyCommonGradingSteps(colorLinear);
            return colorLinear;
        }

        FLOAT3 ColorGradeHDR(FLOAT3 colorLutSpace)
        {
            //#if TONEMAPPING_ACES
			FLOAT3 colorLinear = FLOAT3(0,0,0);			

			// if(_ToneMode>1.5)
            // {
            //     colorLinear = LUT_SPACE_DECODE(colorLutSpace);
            //     FLOAT3 aces = unity_to_ACES(colorLinear);

            //     // ACEScc (log) space
            //     FLOAT3 acescc = ACES_to_ACEScc(aces);
            //     acescc = LogGradeHDR(acescc);
            //     aces = ACEScc_to_ACES(acescc);

            //     // ACEScg (linear) space
            //     FLOAT3 acescg = ACES_to_ACEScg(aces);
            //     acescg = LinearGradeHDR(acescg);

            //     // Tonemap ODT(RRT(aces))
            //     aces = ACEScg_to_ACES(acescg);
            //     colorLinear = AcesTonemap(aces);

            //     return colorLinear;
            // }


			// colorLutSpace is already in log space
			colorLutSpace = LogGradeHDR(colorLutSpace);

			// Switch back to linear
			colorLinear = LUT_SPACE_DECODE(colorLutSpace);
			colorLinear = LinearGradeHDR(colorLinear);
			colorLinear = max(0.0, colorLinear);
			
			// if (_ToneMode > 0.5)
			{
				colorLinear = CustomTonemap(
					colorLinear, _CustomToneCurve.xyz,
					_ToeSegmentA, _ToeSegmentB.xy,
					_MidSegmentA, _MidSegmentB.xy,
					_ShoSegmentA, _ShoSegmentB.xy
				);
			}

			return colorLinear;			
        }

        FLOAT4 FragHDR(VaryingsDefault i) : SV_Target
        {
            FLOAT3 colorLutSpace = GetLutStripValue(i.texcoord, _Lut2D_Params);
            FLOAT3 graded = ColorGradeHDR(colorLutSpace);
            return FLOAT4(graded, 1.0);
        }

        //ue4 ACES tonemapping
        static const FLOAT3x3 GamutMappingIdentityMatrix = { 1.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0 };

        //
        // Gamut conversion matrices
        //
        FLOAT3x3 OuputGamutMappingMatrix( uint OutputGamut )
        {
            // Gamut mapping matrices used later
            const FLOAT3x3 AP1_2_sRGB    = mul( XYZ_2_sRGB_MAT, mul( D60_2_D65_CAT, AP1_2_XYZ_MAT ) );
            // const FLOAT3x3 AP1_2_DCI_D65 = mul( XYZ_2_P3D65_MAT, mul( D60_2_D65_CAT, AP1_2_XYZ_MAT ) );
            // const FLOAT3x3 AP1_2_Rec2020 = mul( XYZ_2_Rec2020_MAT, mul( D60_2_D65_CAT, AP1_2_XYZ_MAT ) );

            // Set gamut mapping matrix 
            // 0 = sRGB - D65
            // 1 = P3 - D65
            // 2 = Rec.2020 - D65
            // 3 = ACES AP0 - D60
            // 4 = ACES AP1 - D60

            // if( OutputGamut == 1 )
            //     return AP1_2_DCI_D65;
            // else if( OutputGamut == 2 )
            //     return AP1_2_Rec2020;
            // else if( OutputGamut == 3 )
            //     return AP1_2_AP0_MAT;
            // else if( OutputGamut == 4 )
            //     return GamutMappingIdentityMatrix;
            // else
                return AP1_2_sRGB;
        }

        
        FLOAT3 ColorCorrect( FLOAT3 WorkingColor,
            FLOAT4 ColorSaturation,
            FLOAT4 ColorContrast,
            FLOAT4 ColorGamma,
            FLOAT4 ColorGain,
            FLOAT4 ColorOffset )
        {
            // TODO optimize
            FLOAT Luma = dot( WorkingColor, AP1_RGB2Y );
            WorkingColor = max( 0, lerp( Luma.xxx, WorkingColor, ColorSaturation.xyz*ColorSaturation.w ) );
            WorkingColor = pow( WorkingColor * (1.0 / 0.18), ColorContrast.xyz*ColorContrast.w ) * 0.18;
            WorkingColor = pow( WorkingColor, 1.0 / (ColorGamma.xyz*ColorGamma.w) );
            WorkingColor = WorkingColor * (ColorGain.xyz * ColorGain.w) + (ColorOffset.xyz + ColorOffset.w);
            return WorkingColor;
        }


        FLOAT4 ColorSaturation;
        FLOAT4 ColorContrast;
        FLOAT4 ColorGamma;
        FLOAT4 ColorGain;
        FLOAT4 ColorOffset;

        FLOAT4 ColorSaturationShadows;
        FLOAT4 ColorContrastShadows;
        FLOAT4 ColorGammaShadows;
        FLOAT4 ColorGainShadows;
        FLOAT4 ColorOffsetShadows;

        FLOAT4 ColorSaturationMidtones;
        FLOAT4 ColorContrastMidtones;
        FLOAT4 ColorGammaMidtones;
        FLOAT4 ColorGainMidtones;
        FLOAT4 ColorOffsetMidtones;

        FLOAT4 ColorSaturationHighlights;
        FLOAT4 ColorContrastHighlights;
        FLOAT4 ColorGammaHighlights;
        FLOAT4 ColorGainHighlights;
        FLOAT4 ColorOffsetHighlights;

        FLOAT4 ColorGradingParam0;
        #define ColorCorrectionShadowsMax ColorGradingParam0.x
        #define ColorCorrectionHighlightsMin ColorGradingParam0.y
        #define BlueCorrection ColorGradingParam0.z
        #define ExpandGamut ColorGradingParam0.w


        FLOAT4 ColorScale;
        FLOAT4 OverlayColor;
        // Nuke-style Color Correct
        FLOAT3 ColorCorrectAll( FLOAT3 WorkingColor )
        {
            FLOAT Luma = dot( WorkingColor, AP1_RGB2Y );

            // Shadow CC
            FLOAT3 CCColorShadows = ColorCorrect(WorkingColor, 
                ColorSaturationShadows*ColorSaturation, 
                ColorContrastShadows*ColorContrast, 
                ColorGammaShadows*ColorGamma, 
                ColorGainShadows*ColorGain, 
                ColorOffsetShadows+ColorOffset);
            FLOAT CCWeightShadows = 1- smoothstep(0, ColorCorrectionShadowsMax, Luma);
            
            // Highlight CC
            FLOAT3 CCColorHighlights = ColorCorrect(WorkingColor, 
                ColorSaturationHighlights*ColorSaturation, 
                ColorContrastHighlights*ColorContrast, 
                ColorGammaHighlights*ColorGamma, 
                ColorGainHighlights*ColorGain, 
                ColorOffsetHighlights+ColorOffset);
            FLOAT CCWeightHighlights = smoothstep(ColorCorrectionHighlightsMin, 1, Luma);

            // Midtone CC
            FLOAT3 CCColorMidtones = ColorCorrect(WorkingColor, 
                ColorSaturationMidtones*ColorSaturation, 
                ColorContrastMidtones*ColorContrast, 
                ColorGammaMidtones*ColorGamma, 
                ColorGainMidtones*ColorGain, 
                ColorOffsetMidtones+ColorOffset);
            FLOAT CCWeightMidtones = 1 - CCWeightShadows - CCWeightHighlights;

            // Blend Shadow, Midtone and Highlight CCs
            FLOAT3 WorkingColorSMH = CCColorShadows*CCWeightShadows + CCColorMidtones*CCWeightMidtones + CCColorHighlights*CCWeightHighlights;
            
            return WorkingColorSMH;
        }

        const static FLOAT4 MappingPolynomial = FLOAT4(0,1,0,0);			 // RGB = a, b, c where y = a * x*x + b * x + c
		// x is the input value, y the output value
		// RGB = a, b, c where y = a * x*x + b * x + c

		// float c = 0;
		// float b = 4 * 0.5 - 3 * 0 - 1;
		// float a = 1 - 0 - b;

        // @param InLDRColor needs to be LDR (0..1) and in linear space
        FLOAT3 ColorCorrection(FLOAT3 InLDRColor)
        {
            // final color correction to adjust for hardware differences, to make quick adjustements before a demo or simply a user setting
            return MappingPolynomial.x * (InLDRColor * InLDRColor) + MappingPolynomial.y * InLDRColor + MappingPolynomial.z;
        }

        FLOAT4 FragUE4LUTCombine(VaryingsDefault i) : SV_Target
        {
            FLOAT4 Neutral;
            { 
                FLOAT2 UV = i.texcoord;
                // 0.49999f instead of 0.5f to avoid getting into negative values
                UV -= FLOAT2(0.49999f / (LUTSize * LUTSize), 0.49999f / LUTSize);
                FLOAT Scale = LUTSize / (LUTSize - 1);
                FLOAT3 RGB;                
                RGB.r = frac(UV.x * LUTSize);
                RGB.b = UV.x - RGB.r / LUTSize;
                RGB.g = UV.y;
                Neutral = FLOAT4(RGB * Scale, 0);
            }

            FLOAT4 OutColor = 0;
            
            // AP1 to Working space matrices
            const FLOAT3x3 sRGB_2_AP1 = mul( XYZ_2_AP1_MAT, mul( D65_2_D60_CAT, sRGB_2_XYZ_MAT ) );
            const FLOAT3x3 AP1_2_sRGB = mul( XYZ_2_sRGB_MAT, mul( D60_2_D65_CAT, AP1_2_XYZ_MAT ) );

            const FLOAT3x3 AP0_2_AP1 = mul( XYZ_2_AP1_MAT, AP0_2_XYZ_MAT );
            const FLOAT3x3 AP1_2_AP0 = mul( XYZ_2_AP0_MAT, AP1_2_XYZ_MAT );

            const FLOAT3x3 AP1_2_Output  = OuputGamutMappingMatrix( 0 );

            FLOAT3 LUTEncodedColor = Neutral.rgb;
            FLOAT3 LinearColor;
            LinearColor = LogToLin( LUTEncodedColor ) - LogToLin( 0 );

            FLOAT3 BalancedColor = WhiteBalance2( LinearColor,WhiteTemp,WhiteTint );
            FLOAT3 ColorAP1 = mul( sRGB_2_AP1, BalancedColor );

            // Expand bright saturated colors outside the sRGB gamut to fake wide gamut rendering.
            // UNITY_BRANCH
            // if( !LegacyTonemapping )
            {
                FLOAT  LumaAP1 = dot( ColorAP1, AP1_RGB2Y );
                FLOAT3 ChromaAP1 = ColorAP1 / LumaAP1;

                FLOAT ChromaDistSqr = dot( ChromaAP1 - 1, ChromaAP1 - 1 );
                FLOAT ExpandAmount = ( 1 - exp2( -4 * ChromaDistSqr ) ) * ( 1 - exp2( -4 * ExpandGamut * LumaAP1*LumaAP1 ) );

                // Bizarre matrix but this expands sRGB to between P3 and AP1
                // CIE 1931 chromaticities:	x		y
                //				Red:		0.6965	0.3065
                //				Green:		0.245	0.718
                //				Blue:		0.1302	0.0456
                //				White:		0.3127	0.329
                const FLOAT3x3 Wide_2_XYZ_MAT = 
                {
                    0.5441691,  0.2395926,  0.1666943,
                    0.2394656,  0.7021530,  0.0583814,
                    -0.0023439,  0.0361834,  1.0552183,
                };

                const FLOAT3x3 Wide_2_AP1 = mul( XYZ_2_AP1_MAT, Wide_2_XYZ_MAT );
                const FLOAT3x3 ExpandMat = mul( Wide_2_AP1, AP1_2_sRGB );

                FLOAT3 ColorExpand = mul( ExpandMat, ColorAP1 );
                ColorAP1 = lerp( ColorAP1, ColorExpand, ExpandAmount );
            }

            ColorAP1 = ColorCorrectAll( ColorAP1 );
            
            // Store for Legacy tonemap later
            FLOAT3 GradedColor = mul( AP1_2_sRGB, ColorAP1 );
            
            const FLOAT3x3 BlueCorrect =
            {
                0.9404372683, -0.0183068787, 0.0778696104,
                0.0083786969,  0.8286599939, 0.1629613092,
                0.0005471261, -0.0008833746, 1.0003362486
            };
            const FLOAT3x3 BlueCorrectInv =
            {
                1.06318,     0.0233956, -0.0865726,
                -0.0106337,   1.20632,   -0.19569,
                -0.000590887, 0.00105248, 0.999538
            };
            const FLOAT3x3 BlueCorrectAP1    = mul( AP0_2_AP1, mul( BlueCorrect,    AP1_2_AP0 ) );
            const FLOAT3x3 BlueCorrectInvAP1 = mul( AP0_2_AP1, mul( BlueCorrectInv, AP1_2_AP0 ) );

            // Blue correction
            ColorAP1 = lerp( ColorAP1, mul( BlueCorrectAP1, ColorAP1 ), BlueCorrection );

            // Tonemapped color in the AP1 gamut
            ColorAP1 = FilmToneMap( ColorAP1 );

            // Uncorrect blue to maintain white point
            ColorAP1 = lerp( ColorAP1, mul( BlueCorrectInvAP1, ColorAP1 ), BlueCorrection );

            // Convert from AP1 to sRGB and clip out-of-gamut values
            FLOAT3 FilmColor = max(0, mul( AP1_2_sRGB, ColorAP1 ));

        // #if FEATURE_LEVEL > FEATURE_LEVEL_ES3_1 // Mobile path uses separate shader for legacy tone mapper
        //     BRANCH
        //     if( ColorShadow_Tint2.a == 0 )
        //     {
        //         // Legacy tone mapper
        //         FilmColor = FilmPostProcess( GradedColor );
        //     }
        // #endif // FEATURE_LEVEL > FEATURE_LEVEL_ES3_1

        // #if BLENDCOUNT > 1
        //     {
        //         // Legacy LDR LUT color grading

        //         // FIXME 
        //         // LUTs are in sRGB transfer function space
        //         // LUTs applied in sRGB gamut as well
        //         FLOAT3 GammaColor = LinearToSrgb( saturate( FilmColor ) );

        //         FLOAT3 UVW = GammaColor * (15.0 / 16.0) + (0.5f / 16.0);

        //         GammaColor = LUTWeights[0] * GammaColor;

        //         // BLENDCOUNT is the number of LUT that are blended together including the neutral one
        //     #if BLENDCOUNT >= 2
        //         GammaColor += LUTWeights[1] * UnwrappedTexture3DSample( Texture1, Texture1Sampler, UVW, 16 ).rgb;
        //     #endif

        //     #if BLENDCOUNT >= 3
        //         GammaColor += LUTWeights[2] * UnwrappedTexture3DSample( Texture2, Texture2Sampler, UVW, 16 ).rgb;
        //     #endif

        //     #if BLENDCOUNT >= 4
        //         GammaColor += LUTWeights[3] * UnwrappedTexture3DSample( Texture3, Texture3Sampler, UVW, 16 ).rgb;
        //     #endif

        //     #if BLENDCOUNT >= 5
        //         GammaColor += LUTWeights[4] * UnwrappedTexture3DSample( Texture4, Texture4Sampler, UVW, 16 ).rgb;
        //     #endif

        //         // Back to linear space, AP1 gamut
        //         FilmColor =  sRGBToLinear( GammaColor );
        //     }
        // #endif
            
            // apply math color correction on top to texture based solution
            FilmColor = ColorCorrection( FilmColor );

            // blend with custom LDR color, used for Fade track in Matinee
            FilmColor = lerp( FilmColor * ColorScale.xyz, OverlayColor.rgb, OverlayColor.a );

            // Apply "gamma" curve adjustment.
            FilmColor = pow( max(0, FilmColor), InverseGamma.y );

            FLOAT3 OutDeviceColor = 0;
    

            // sRGB, user specified gamut
            // if( GetOutputDevice() == 0 )
            {		
                // Convert from sRGB to specified output gamut	
                //FLOAT3 OutputGamutColor = mul( AP1_2_Output, mul( sRGB_2_AP1, FilmColor ) );

                // FIXME: Workaround for UE-29935, pushing all colors with a 0 component to black output
                // Default parameters seem to cancel out (sRGB->XYZ->AP1->XYZ->sRGB), so should be okay for a temp fix
                FLOAT3 OutputGamutColor = FilmColor;

                // Apply conversion to sRGB (this must be an exact sRGB conversion else darks are bad).
                OutDeviceColor = LinearToSrgb( OutputGamutColor );
            }

            // Better to saturate(lerp(a,b,t)) than lerp(saturate(a),saturate(b),t)
            OutColor.rgb = OutDeviceColor / 1.05;
            OutColor.a = 0;

            return OutColor;
        }
    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
           HLSLPROGRAM

               #pragma vertex VertDefault
               #pragma fragment FragLDRFromScratch

           ENDHLSL
        }

        Pass
        {
           HLSLPROGRAM

               #pragma vertex VertDefault
               #pragma fragment FragLDR

           ENDHLSL
        }

        Pass
        {
            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment FragHDR

            ENDHLSL
        }

        
        Pass
        {
            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment FragUE4LUTCombine

            ENDHLSL
        }
    }
}
