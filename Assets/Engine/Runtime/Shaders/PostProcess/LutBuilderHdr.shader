Shader "Hidden/PostProcessing/LutBuilderHdr"
{
    HLSLINCLUDE
        #define _TONEMAP_ACES 1
        #pragma target 3.0
        #include "../StdLib.hlsl"
        #include "../Colors.hlsl"
        #include "../ACES.hlsl"

        float4 _LutBake_Params;         // x: lut_height, y: 0.5 / lut_width, z: 0.5 / lut_height, w: lut_height / lut_height - 1
        float4 _ColorBalance;       // xyz: LMS coeffs, w: unused
        float4 _ColorFilter;        // xyz: color, w: unused
        float4 _ChannelMixerRed;    // xyz: rgb coeffs, w: unused
        float4 _ChannelMixerGreen;  // xyz: rgb coeffs, w: unused
        float4 _ChannelMixerBlue;   // xyz: rgb coeffs, w: unused
        float4 _HueSatCon;          // x: hue shift, y: saturation, z: contrast, w: unused
        float4 _Lift;               // xyz: color, w: unused
        float4 _Gamma;              // xyz: color, w: unused
        float4 _Gain;               // xyz: color, w: unused
        float4 _Shadows;            // xyz: color, w: unused
        float4 _Midtones;           // xyz: color, w: unused
        float4 _Highlights;         // xyz: color, w: unused
        float4 _ShaHiLimits;        // xy: shadows min/max, zw: highlight min/max
        float4 _SplitShadows;       // xyz: color, w: balance
        float4 _SplitHighlights;    // xyz: color, w: unused

        TEX2D_SAMPLER(_CurveMaster);
        TEX2D_SAMPLER(_CurveRed);
        TEX2D_SAMPLER(_CurveGreen);
        TEX2D_SAMPLER(_CurveBlue);

        TEX2D_SAMPLER(_CurveHueVsHue);
        TEX2D_SAMPLER(_CurveHueVsSat);
        TEX2D_SAMPLER(_CurveSatVsSat);
        TEX2D_SAMPLER(_CurveLumVsSat);

        float4 _CustomToneCurve;
        float4 _ToeSegmentA;
        float4 _ToeSegmentB;
        float4 _MidSegmentA;
        float4 _MidSegmentB;
        float4 _ShoSegmentA;
        float4 _ShoSegmentB;

        #define _ACES_Enable _HueSatCon.w>0.5

        float EvaluateCurve(TEX2D_ARGS(curve), float t)
        {
            float x = SAMPLE_TEX2D(curve, float2(t, 0.0)).x;
            return saturate(x);
        }

        float GetLuminance(float3 colorLinear)
        {
            return AcesLuminance(colorLinear);
        /*#if _TONEMAP_ACES
            return AcesLuminance(colorLinear);
        #else
            return Luminance(colorLinear);
        #endif*/
        }
        // Note: when the ACES tonemapper is selected the grading steps will be done using ACES spaces
        float3 ColorGrade(float3 colorLutSpace)
        {
            // Switch back to linear
            float3 colorLinear = LogCToLinear(colorLutSpace);

            // White balance in LMS space
            float3 colorLMS = LinearToLMS(colorLinear);
            colorLMS *= _ColorBalance.xyz;
            colorLinear = LMSToLinear(colorLMS);

            // Do contrast in log after white balance

            if(_ACES_Enable)
            {
                float3 colorLog = ACES_to_ACEScc(unity_to_ACES(colorLinear));
                colorLog = (colorLog - ACEScc_MIDGRAY) * _HueSatCon.z + ACEScc_MIDGRAY;
                colorLinear = ACES_to_ACEScg(ACEScc_to_ACES(colorLog));
            }
            else
            {
                float3 colorLog = LinearToLogC(colorLinear);
                colorLog = (colorLog - ACEScc_MIDGRAY) * _HueSatCon.z + ACEScc_MIDGRAY;
                colorLinear = LogCToLinear(colorLog);
            }
            /*#if _TONEMAP_ACES
            float3 colorLog = ACES_to_ACEScc(unity_to_ACES(colorLinear));
            #else
            float3 colorLog = LinearToLogC(colorLinear);
            #endif

            colorLog = (colorLog - ACEScc_MIDGRAY) * _HueSatCon.z + ACEScc_MIDGRAY;

            #if _TONEMAP_ACES
            colorLinear = ACES_to_ACEScg(ACEScc_to_ACES(colorLog));
            #else
            colorLinear = LogCToLinear(colorLog);
            #endif*/

            // Color filter is just an unclipped multiplier
            colorLinear *= _ColorFilter.xyz;

            
            // Do NOT feed negative values to the following color ops
            colorLinear = max(0.0, colorLinear);
    
            // Split toning
            // As counter-intuitive as it is, to make split-toning work the same way it does in Adobe
            // products we have to do all the maths in gamma-space...
            float balance = _SplitShadows.w;
            float3 colorGamma = PositivePow(colorLinear, 1.0 / 2.2);

            float luma = saturate(GetLuminance(saturate(colorGamma)) + balance);
            float3 halfV = float3(0.5,0.5,0.5);
            float3 splitShadows = lerp(halfV, _SplitShadows.xyz, 1.0 - luma);
            float3 splitHighlights = lerp(halfV, _SplitHighlights.xyz, luma);
            colorGamma = SoftLight(colorGamma, splitShadows);
            colorGamma = SoftLight(colorGamma, splitHighlights);

            colorLinear = PositivePow(colorGamma, 2.2);

            // Channel mixing (Adobe style)
            colorLinear = float3(
                dot(colorLinear, _ChannelMixerRed.xyz),
                dot(colorLinear, _ChannelMixerGreen.xyz),
                dot(colorLinear, _ChannelMixerBlue.xyz)
            );

            // Shadows, midtones, highlights
            luma = GetLuminance(colorLinear);
            float shadowsFactor = 1.0 - smoothstep(_ShaHiLimits.x, _ShaHiLimits.y, luma);
            float highlightsFactor = smoothstep(_ShaHiLimits.z, _ShaHiLimits.w, luma);
            float midtonesFactor = 1.0 - shadowsFactor - highlightsFactor;
            colorLinear = colorLinear * _Shadows.xyz * shadowsFactor
                        + colorLinear * _Midtones.xyz * midtonesFactor
                        + colorLinear * _Highlights.xyz * highlightsFactor;

            // Lift, gamma, gain
            colorLinear = colorLinear * _Gain.xyz + _Lift.xyz;
            colorLinear = sign(colorLinear) * pow(abs(colorLinear), _Gamma.xyz);

            // HSV operations
            float satMult = 0;
            float3 hsv = RgbToHsv(colorLinear);
            {
                // Hue Vs Sat
                satMult = EvaluateCurve(TEX2D_PARAM(_CurveHueVsSat), hsv.x) * 2.0;

                // Sat Vs Sat
                satMult *= EvaluateCurve(TEX2D_PARAM(_CurveSatVsSat), hsv.y) * 2.0;

                // Lum Vs Sat
                satMult *= EvaluateCurve(TEX2D_PARAM(_CurveLumVsSat), Luminance(colorLinear)) * 2.0;

                // Hue Shift & Hue Vs Hue
                float hue = hsv.x + _HueSatCon.x;
                float offset = EvaluateCurve(TEX2D_PARAM(_CurveHueVsHue), hue) - 0.5;
                hue += offset;
                hsv.x = RotateHue(hue, 0.0, 1.0);
            }
            colorLinear = HsvToRgb(hsv);

            // Global saturation
            luma = GetLuminance(colorLinear);
            colorLinear = luma.xxx + (_HueSatCon.yyy * satMult) * (colorLinear - luma.xxx);

            // YRGB curves
            // Conceptually these need to be in range [0;1] and from an artist-workflow perspective
            // it's easier to deal with
            colorLinear = FastTonemap(colorLinear);
            {
                const float kHalfPixel = (1.0 / 128.0) / 2.0;
                float3 c = colorLinear;

                //Y (master)
                c += kHalfPixel.xxx;
                float mr = EvaluateCurve(TEX2D_PARAM(_CurveMaster), c.r);
                float mg = EvaluateCurve(TEX2D_PARAM(_CurveMaster), c.g);
                float mb = EvaluateCurve(TEX2D_PARAM(_CurveMaster), c.b);
                c = float3(mr, mg, mb);

                //RGB
                c += kHalfPixel.xxx;
                float r = EvaluateCurve(TEX2D_PARAM(_CurveRed), c.r);
                float g = EvaluateCurve(TEX2D_PARAM(_CurveGreen), c.g);
                float b = EvaluateCurve(TEX2D_PARAM(_CurveBlue), c.b);
                colorLinear = float3(r, g, b);
            }
            colorLinear = FastTonemapInvert(colorLinear);

            colorLinear = max(0.0, colorLinear);
            return colorLinear;
        }

        float3 Tonemap(float3 colorLinear)
        {
            /*#if _TONEMAP_NEUTRAL
            {
                colorLinear = NeutralTonemap(colorLinear);
            }
            #elif _TONEMAP_ACES
            {
                // Note: input is actually ACEScg (AP1 w/ linear encoding)
                float3 aces = ACEScg_to_ACES(colorLinear);
                colorLinear = AcesTonemap(aces);
            }
            #endif*/
            if(_ACES_Enable)
            {
                float3 aces = ACEScg_to_ACES(colorLinear);
                colorLinear = AcesTonemap(aces);
            }
            else
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

        float4 Frag(VaryingsDefault i) : SV_Target
        {
            // Lut space
            // We use Alexa LogC (El 1000) to store the LUT as it provides a good enough range
            // (~58.85666) and is good enough to be stored in fp16 without losing precision in the
            // darks
            float3 colorLutSpace = GetLutStripValue(i.texcoord, _LutBake_Params);

            // Color grade & tonemap
            float3 gradedColor = ColorGrade(colorLutSpace);
            gradedColor = Tonemap(gradedColor);
            return float4(gradedColor, 1.0);
        }

    ENDHLSL

    SubShader
    {
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            Name "LutBuilderHdr"

            HLSLPROGRAM
                #pragma vertex VertDefault
                #pragma fragment Frag
            ENDHLSL
        }
    }
}
