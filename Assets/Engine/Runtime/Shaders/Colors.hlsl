#ifndef UNITY_POSTFX_COLOR
#define UNITY_POSTFX_COLOR

#include "StdLib.hlsl"
#include "ACES.hlsl"

#define LUT_SPACE_ENCODE(x) LinearToLogC(x)
#define LUT_SPACE_DECODE(x) LogCToLinear(x)

#ifndef USE_PRECISE_LOGC
    // Set to 1 to use more precise but more expensive log/linear conversions. I haven't found a proper
    // use case for the high precision version yet so I'm leaving this to 0.
    #define USE_PRECISE_LOGC 0
#endif

#ifndef TONEMAPPING_USE_FULL_ACES
    // Set to 1 to use the full reference ACES tonemapper. This should only be used for research
    // purposes as it's quite heavy and generally overkill.
    #define TONEMAPPING_USE_FULL_ACES 0
#endif

#ifndef DEFAULT_MAX_PQ
    // PQ ST.2048 max value
    // 1.0 = 100nits, 100.0 = 10knits
    #define DEFAULT_MAX_PQ 100.0
#endif

#ifndef USE_VERY_FAST_SRGB
    #if defined(SHADER_API_MOBILE)
        #define USE_VERY_FAST_SRGB 1
    #else
        #define USE_VERY_FAST_SRGB 0
    #endif
#endif

#ifndef USE_FAST_SRGB
	#define USE_FAST_SRGB 0
#endif

// usually 1/2.2, the .y is used for inverse gamma when "gamma only" mode is not used
static const half3 InverseGamma = half3(1.0/2.2,1,1/2.2);
//
// Alexa LogC converters (El 1000)
// See http://www.vocas.nl/webfm_send/964
// Max range is ~58.85666
//
struct ParamsLogC
{
    float cut;
    float a, b, c, d, e, f;
};

static const ParamsLogC LogC =
{
    0.011361, // cut
    5.555556, // a
    0.047996, // b
    0.244161, // c
    0.386036, // d
    5.301883, // e
    0.092819  // f
};

float LinearToLogC_Precise(half x)
{
    float o;
    if (x > LogC.cut)
        o = LogC.c * log10(LogC.a * x + LogC.b) + LogC.d;
    else
        o = LogC.e * x + LogC.f;
    return o;
}

float3 LinearToLogC(float3 x)
{
#if USE_PRECISE_LOGC
    return float3(
        LinearToLogC_Precise(x.x),
        LinearToLogC_Precise(x.y),
        LinearToLogC_Precise(x.z)
    );
#else
    return LogC.c * log10(LogC.a * x + LogC.b) + LogC.d;
#endif
}

float LogCToLinear_Precise(float x)
{
    float o;
    if (x > LogC.e * LogC.cut + LogC.f)
        o = (pow(10.0, (x - LogC.d) / LogC.c) - LogC.b) / LogC.a;
    else
        o = (x - LogC.f) / LogC.e;
    return o;
}

float3 LogCToLinear(float3 x)
{
#if USE_PRECISE_LOGC
    return float3(
        LogCToLinear_Precise(x.x),
        LogCToLinear_Precise(x.y),
        LogCToLinear_Precise(x.z)
    );
#else
    return (pow(10.0, (x - LogC.d) / LogC.c) - LogC.b) / LogC.a;
#endif

}
float3 LinearToLMS(float3 x)
{
    const float3x3 LIN_2_LMS_MAT = {
        3.90405e-1, 5.49941e-1, 8.92632e-3,
        7.08416e-2, 9.63172e-1, 1.35775e-3,
        2.31082e-2, 1.28021e-1, 9.36245e-1
    };

    return mul(LIN_2_LMS_MAT, x);
}

float3 LMSToLinear(float3 x)
{
    const float3x3 LMS_2_LIN_MAT = {
        2.85847e+0, -1.62879e+0, -2.48910e-2,
        -2.10182e-1,  1.15820e+0,  3.24281e-4,
        -4.18120e-2, -1.18169e-1,  1.06867e+0
    };

    return mul(LMS_2_LIN_MAT, x);
}

float AcesLuminance(float3 linearRgb)
{
    return dot(linearRgb, AP1_RGB2Y);
}

// Soft-light blending mode use for split-toning. Works in HDR as long as `blend` is [0;1] which is
// fine for our use case.
float3 SoftLight(float3 base, float3 blend)
{
    float3 r1 = 2.0 * base * blend + base * base * (1.0 - 2.0 * blend);
    float3 r2 = sqrt(base) * (2.0 * blend - 1.0) + 2.0 * base * (1.0 - blend);
    float3 t = step(0.5, blend);
    return r2 * t + (1.0 - t) * r1;
}
//
// Generic log lin transforms
//
float3 LogToLin( float3 LogColor )
{
	const float LinearRange = 14;
	const float LinearGrey = 0.18;
	const float ExposureGrey = 444;

	// Using stripped down, 'pure log', formula. Parameterized by grey points and dynamic range covered.
	float3 LinearColor = exp2( ( LogColor - ExposureGrey / 1023.0 ) * LinearRange ) * LinearGrey;
	//float3 LinearColor = 2 * ( pow(10.0, ((LogColor - 0.616596 - 0.03) / 0.432699)) - 0.037584 );	// SLog
	//float3 LinearColor = ( pow( 10, ( 1023 * LogColor - 685 ) / 300) - .0108 ) / (1 - .0108);	// Cineon
	//LinearColor = max( 0, LinearColor );

	return LinearColor;
}

float3 LinToLog( float3 LinearColor )
{
	const float LinearRange = 14;
	const float LinearGrey = 0.18;
	const float ExposureGrey = 444;

	// Using stripped down, 'pure log', formula. Parameterized by grey points and dynamic range covered.
	float3 LogColor = log2(LinearColor) / LinearRange - log2(LinearGrey) / LinearRange + ExposureGrey / 1023.0;	// scalar: 3log2 3mad
	//float3 LogColor = (log2(LinearColor) - log2(LinearGrey)) / LinearRange + ExposureGrey / 1023.0;
	//float3 LogColor = log2( LinearColor / LinearGrey ) / LinearRange + ExposureGrey / 1023.0;
	//float3 LogColor = (0.432699 * log10(0.5 * LinearColor + 0.037584) + 0.616596) + 0.03;	// SLog
	//float3 LogColor = ( 300 * log10( LinearColor * (1 - .0108) + .0108 ) + 685 ) / 1023;	// Cineon
	LogColor = saturate( LogColor );

	return LogColor;
}
//
// SMPTE ST.2084 (PQ) transfer functions
// Used for HDR Lut storage, max range depends on the maxPQValue parameter
//
struct ParamsPQ
{
    float N, M;
    float C1, C2, C3;
};

static const ParamsPQ PQ =
{
    2610.0 / 4096.0 / 4.0,   // N
    2523.0 / 4096.0 * 128.0, // M
    3424.0 / 4096.0,         // C1
    2413.0 / 4096.0 * 32.0,  // C2
    2392.0 / 4096.0 * 32.0,  // C3
};

float3 LinearToPQ(float3 x, float maxPQValue)
{
    x = PositivePow(x / maxPQValue, PQ.N);
    float3 nd = (PQ.C1 + PQ.C2 * x) / (1.0 + PQ.C3 * x);
    return PositivePow(nd, PQ.M);
}

float3 LinearToPQ(float3 x)
{
    return LinearToPQ(x, DEFAULT_MAX_PQ);
}

float3 PQToLinear(float3 x, float maxPQValue)
{
    x = PositivePow(x, rcp(PQ.M));
    float3 nd = max(x - PQ.C1, 0.0) / (PQ.C2 - (PQ.C3 * x));
    return PositivePow(nd, rcp(PQ.N)) * maxPQValue;
}

float3 PQToLinear(float3 x)
{
    return PQToLinear(x, DEFAULT_MAX_PQ);
}

//
// sRGB transfer functions
// Fast path ref: http://chilliant.blogspot.com.au/2012/08/srgb-approximations-for-hlsl.html?m=1
//
half SRGBToLinear(half c)
{
#if USE_VERY_FAST_SRGB
    return c * c;
#elif USE_FAST_SRGB
    return c * (c * (c * 0.305306011 + 0.682171111) + 0.012522878);
#else
    half linearRGBLo = c / 12.92;
    half linearRGBHi = PositivePow((c + 0.055) / 1.055, 2.4);
    half linearRGB = (c <= 0.04045) ? linearRGBLo : linearRGBHi;
    return linearRGB;
#endif
}

half3 SRGBToLinear(half3 c)
{
#if USE_VERY_FAST_SRGB
    return c * c;
#elif USE_FAST_SRGB
    return c * (c * (c * 0.305306011 + 0.682171111) + 0.012522878);
#else
    half3 linearRGBLo = c / 12.92;
    half3 linearRGBHi = PositivePow((c + 0.055) / 1.055, half3(2.4, 2.4, 2.4));
    half3 linearRGB = (c <= 0.04045) ? linearRGBLo : linearRGBHi;
    return linearRGB;
#endif
}

half4 SRGBToLinear(half4 c)
{
    return half4(SRGBToLinear(c.rgb), c.a);
}

half LinearToSRGB(half c)
{
#if USE_VERY_FAST_SRGB
    return sqrt(c);
#elif USE_FAST_SRGB
    return max(1.055 * PositivePow(c, 0.416666667) - 0.055, 0.0);
#else
    half sRGBLo = c * 12.92;
    half sRGBHi = (PositivePow(c, 1.0 / 2.4) * 1.055) - 0.055;
    half sRGB = (c <= 0.0031308) ? sRGBLo : sRGBHi;
    return sRGB;
#endif
}

half3 LinearToSRGB(half3 c)
{
#if USE_VERY_FAST_SRGB
    return sqrt(c);
#elif USE_FAST_SRGB
    return max(1.055 * PositivePow(c, 0.416666667) - 0.055, 0.0);
#else
    half3 sRGBLo = c * 12.92;
    half3 sRGBHi = (PositivePow(c, half3(1.0 / 2.4, 1.0 / 2.4, 1.0 / 2.4)) * 1.055) - 0.055;
    half3 sRGB = (c <= 0.0031308) ? sRGBLo : sRGBHi;
    return sRGB;
#endif
}

half4 LinearToSRGB(half4 c)
{
    return half4(LinearToSRGB(c.rgb), c.a);
}

half3 LinearToSrgbBranchless(half3 lin) 
{
	lin = max(6.10352e-5, lin); // minimum positive non-denormal (fixes black problem on DX11 AMD and NV)
	return min(lin * 12.92, pow(max(lin, 0.00313067), 1.0/2.4) * 1.055 - 0.055);
	// Possible that mobile GPUs might have native pow() function?
	//return min(lin * 12.92, exp2(log2(max(lin, 0.00313067)) * (1.0/2.4) + log2(1.055)) - 0.055);
}

half LinearToSrgbBranchingChannel(half lin) 
{
	if(lin < 0.00313067) return lin * 12.92;
	return pow(lin, (1.0/2.4)) * 1.055 - 0.055;
}

half3 LinearToSrgbBranching(half3 lin) 
{
	return half3(
		LinearToSrgbBranchingChannel(lin.r),
		LinearToSrgbBranchingChannel(lin.g),
		LinearToSrgbBranchingChannel(lin.b));
}

half3 LinearToSrgb(half3 lin) 
{
// #if FEATURE_LEVEL > FEATURE_LEVEL_ES3_1
	// Branching is faster than branchless on AMD on PC.
	return LinearToSrgbBranching(lin);
// #else
// 	// Adreno devices(Nexus5) with Android 4.4.2 do not handle branching version well, so always use branchless on Mobile
// 	return LinearToSrgbBranchless(lin);
// #endif
}

//
// Convert rgb to luminance with rgb in linear space with sRGB primaries and D65 white point
//
half Luminance(half3 linearRgb)
{
    return dot(linearRgb, float3(0.2126729, 0.7151522, 0.0721750));
}

half Luminance(half4 linearRgba)
{
    return Luminance(linearRgba.rgb);
}

// Quadratic color thresholding
// curve = (threshold - knee, knee * 2, 0.25 / knee)
FLOAT3 QuadraticThreshold(FLOAT3 color, FLOAT threshold, FLOAT3 curve)
{
    // Pixel brightness
    FLOAT br = Max3(color.r, color.g, color.b);

    // Under-threshold part
    FLOAT rq = clamp(br - curve.x, 0.0, curve.y);
    rq = curve.z * rq * rq;

    FLOAT scale = max(rq, br - threshold) / max(br, 1e-4);
    // Combine and apply the brightness response curve
    color *= scale;

    return scale.xxx;
}
//
// Fast reversible tonemapper
// http://gpuopen.com/optimized-reversible-tonemapper-for-resolve/
//
float3 FastTonemap(float3 c)
{
    return c * rcp(Max3(c.r, c.g, c.b) + 1.0);
}

float4 FastTonemap(float4 c)
{
    return float4(FastTonemap(c.rgb), c.a);
}

float3 FastTonemap(float3 c, float w)
{
    return c * (w * rcp(Max3(c.r, c.g, c.b) + 1.0));
}

float4 FastTonemap(float4 c, float w)
{
    return float4(FastTonemap(c.rgb, w), c.a);
}

float3 FastTonemapInvert(float3 c)
{
    return c * rcp(1.0 - Max3(c.r, c.g, c.b));
}

float4 FastTonemapInvert(float4 c)
{
    return float4(FastTonemapInvert(c.rgb), c.a);
}

//
// Neutral tonemapping (Hable/Hejl/Frostbite)
// Input is linear RGB
//
float3 NeutralCurve(float3 x, float a, float b, float c, float d, float e, float f)
{
    return ((x * (a * x + c * b) + d * e) / (x * (a * x + b) + d * f)) - e / f;
}

float3 NeutralTonemap(float3 x)
{
    // Tonemap
    float a = 0.2;
    float b = 0.29;
    float c = 0.24;
    float d = 0.272;
    float e = 0.02;
    float f = 0.3;
    float whiteLevel = 5.3;
    float whiteClip = 1.0;

    float3 whiteScale = (1.0).xxx / NeutralCurve(whiteLevel, a, b, c, d, e, f);
    x = NeutralCurve(x * whiteScale, a, b, c, d, e, f);
    x *= whiteScale;

    // Post-curve white point adjustment
    x /= whiteClip.xxx;

    return x;
}

//
// Raw, unoptimized version of John Hable's artist-friendly tone curve
// Input is linear RGB
//
float EvalCustomSegment(float x, float4 segmentA, float2 segmentB)
{
    const float kOffsetX = segmentA.x;
    const float kOffsetY = segmentA.y;
    const float kScaleX  = segmentA.z;
    const float kScaleY  = segmentA.w;
    const float kLnA     = segmentB.x;
    const float kB       = segmentB.y;

    float x0 = (x - kOffsetX) * kScaleX;
    float y0 = (x0 > 0.0) ? exp(kLnA + kB * log(x0)) : 0.0;
    return y0 * kScaleY + kOffsetY;
}

float EvalCustomCurve(float x, float3 curve, float4 toeSegmentA, float2 toeSegmentB, float4 midSegmentA, float2 midSegmentB, float4 shoSegmentA, float2 shoSegmentB)
{
    float4 segmentA;
    float2 segmentB;

    if (x < curve.y)
    {
        segmentA = toeSegmentA;
        segmentB = toeSegmentB;
    }
    else if (x < curve.z)
    {
        segmentA = midSegmentA;
        segmentB = midSegmentB;
    }
    else
    {
        segmentA = shoSegmentA;
        segmentB = shoSegmentB;
    }

    return EvalCustomSegment(x, segmentA, segmentB);
}

// curve: x: inverseWhitePoint, y: x0, z: x1
float3 CustomTonemap(float3 x, float3 curve, float4 toeSegmentA, float2 toeSegmentB, float4 midSegmentA, float2 midSegmentB, float4 shoSegmentA, float2 shoSegmentB)
{
    float3 normX = x * curve.x;
    float3 ret;
    ret.x = EvalCustomCurve(normX.x, curve, toeSegmentA, toeSegmentB, midSegmentA, midSegmentB, shoSegmentA, shoSegmentB);
    ret.y = EvalCustomCurve(normX.y, curve, toeSegmentA, toeSegmentB, midSegmentA, midSegmentB, shoSegmentA, shoSegmentB);
    ret.z = EvalCustomCurve(normX.z, curve, toeSegmentA, toeSegmentB, midSegmentA, midSegmentB, shoSegmentA, shoSegmentB);
    return ret;
}

//
// Filmic tonemapping (ACES fitting, unless TONEMAPPING_USE_FULL_ACES is set to 1)
// Input is ACES2065-1 (AP0 w/ linear encoding)
//
float3 AcesTonemap(float3 aces)
{
#if TONEMAPPING_USE_FULL_ACES

    float3 oces = RRT(aces);
    float3 odt = ODT_RGBmonitor_100nits_dim(oces);
    return odt;

#else

    // --- Glow module --- //
    float saturation = rgb_2_saturation(aces);
    float ycIn = rgb_2_yc(aces);
    float s = sigmoid_shaper((saturation - 0.4) / 0.2);
    float addedGlow = 1.0 + glow_fwd(ycIn, RRT_GLOW_GAIN * s, RRT_GLOW_MID);
    aces *= addedGlow;

    // --- Red modifier --- //
    float hue = rgb_2_hue(aces);
    float centeredHue = center_hue(hue, RRT_RED_HUE);
    float hueWeight;
    {
        //hueWeight = cubic_basis_shaper(centeredHue, RRT_RED_WIDTH);
        hueWeight = smoothstep(0.0, 1.0, 1.0 - abs(2.0 * centeredHue / RRT_RED_WIDTH));
        hueWeight *= hueWeight;
    }

    aces.r += hueWeight * saturation * (RRT_RED_PIVOT - aces.r) * (1.0 - RRT_RED_SCALE);

    // --- ACES to RGB rendering space --- //
    float3 acescg = max(0.0, ACES_to_ACEScg(aces));

    // --- Global desaturation --- //
    //acescg = mul(RRT_SAT_MAT, acescg);
    acescg = lerp(dot(acescg, AP1_RGB2Y).xxx, acescg, RRT_SAT_FACTOR.xxx);

    // Luminance fitting of *RRT.a1.0.3 + ODT.Academy.RGBmonitor_100nits_dim.a1.0.3*.
    // https://github.com/colour-science/colour-unity/blob/master/Assets/Colour/Notebooks/CIECAM02_Unity.ipynb
    // RMSE: 0.0012846272106
    const float a = 278.5085;
    const float b = 10.7772;
    const float c = 293.6045;
    const float d = 88.7122;
    const float e = 80.6889;
    float3 x = acescg;
    float3 rgbPost = (x * (a * x + b)) / (x * (c * x + d) + e);

    // Scale luminance to linear code value
    // float3 linearCV = Y_2_linCV(rgbPost, CINEMA_WHITE, CINEMA_BLACK);

    // Apply gamma adjustment to compensate for dim surround
    float3 linearCV = darkSurround_to_dimSurround(rgbPost);

    // Apply desaturation to compensate for luminance difference
    //linearCV = mul(ODT_SAT_MAT, color);
    linearCV = lerp(dot(linearCV, AP1_RGB2Y).xxx, linearCV, ODT_SAT_FACTOR.xxx);

    // Convert to display primary encoding
    // Rendering space RGB to XYZ
    float3 XYZ = mul(AP1_2_XYZ_MAT, linearCV);

    // Apply CAT from ACES white point to assumed observer adapted white point
    XYZ = mul(D60_2_D65_CAT, XYZ);

    // CIE XYZ to display primaries
    linearCV = mul(XYZ_2_REC709_MAT, XYZ);

    return linearCV;

#endif
}
/*
============================================
// Uncharted settings
Slope = 0.63;
Toe = 0.55;
Shoulder = 0.47;
BlackClip= 0;
WhiteClip = 0.01;

// HP settings
Slope = 0.65;
Toe = 0.63;
Shoulder = 0.45;
BlackClip = 0;
WhiteClip = 0;

// Legacy settings
Slope = 0.98;
Toe = 0.3;
Shoulder = 0.22;
BlackClip = 0;
WhiteClip = 0.025;

// ACES settings
Slope = 0.91;
Toe = 0.53;
Shoulder = 0.23;
BlackClip = 0;
WhiteClip = 0.035;
===========================================
*/

FLOAT4 WhiteBalance;
#define WhiteTemp WhiteBalance.y
#define WhiteTint WhiteBalance.z


FLOAT4 Film;
#define FilmSlope Film.x
#define FilmToe Film.y
#define FilmShoulder Film.z
#define FilmBlackClip Film.w
// float FilmSlope = 0.91;
// float FilmToe = 0.53;
// float FilmShoulder = 0.23;
// float FilmBlackClip = 0;

#define FilmWhiteClip WhiteBalance.x
// float FilmWhiteClip = 0.035;

half3 FilmToneMap( half3 LinearColor ) 
{
	const float3x3 sRGB_2_AP0 = mul( XYZ_2_AP0_MAT, mul( D65_2_D60_CAT, sRGB_2_XYZ_MAT ) );
	const float3x3 sRGB_2_AP1 = mul( XYZ_2_AP1_MAT, mul( D65_2_D60_CAT, sRGB_2_XYZ_MAT ) );

	const float3x3 AP0_2_sRGB = mul( XYZ_2_sRGB_MAT, mul( D60_2_D65_CAT, AP0_2_XYZ_MAT ) );
	const float3x3 AP1_2_sRGB = mul( XYZ_2_sRGB_MAT, mul( D60_2_D65_CAT, AP1_2_XYZ_MAT ) );
	
	const float3x3 AP0_2_AP1 = mul( XYZ_2_AP1_MAT, AP0_2_XYZ_MAT );
	const float3x3 AP1_2_AP0 = mul( XYZ_2_AP0_MAT, AP1_2_XYZ_MAT );
	
	float3 ColorAP1 = LinearColor;
	//float3 ColorAP1 = mul( sRGB_2_AP1, float3(LinearColor) );

#if 0
	{
		float3 oces = Inverse_ODT_sRGB_D65( LinearColor );
		float3 aces = Inverse_RRT( oces );
		ColorAP1 = mul( AP0_2_AP1, aces );
	}
#endif
	
#if 0
	float3 ColorSRGB = mul( AP1_2_sRGB, ColorAP1 );
	ColorSRGB = max( 0, ColorSRGB );
	ColorAP1 = mul( sRGB_2_AP1, ColorSRGB );
#endif

	float3 ColorAP0 = mul( AP1_2_AP0, ColorAP1 );

#if 0
	{
		float3 aces = ColorAP0;
		float3 oces = RRT( aces );
		LinearColor = ODT_sRGB_D65( oces );
	}
	return mul( sRGB_2_AP1, LinearColor );
#endif

#if 1
	// "Glow" module constants
	const float RRT_GLOW_GAIN = 0.05;
	const float RRT_GLOW_MID = 0.08;

	float saturation = rgb_2_saturation( ColorAP0 );
	float ycIn = rgb_2_yc( ColorAP0 );
	float s = sigmoid_shaper( (saturation - 0.4) / 0.2);
	float addedGlow = 1 + glow_fwd( ycIn, RRT_GLOW_GAIN * s, RRT_GLOW_MID);
	ColorAP0 *= addedGlow;
#endif

#if 1
	// --- Red modifier --- //
	const float RRT_RED_SCALE = 0.82;
	const float RRT_RED_PIVOT = 0.03;
	const float RRT_RED_HUE = 0;
	const float RRT_RED_WIDTH = 135;
	float hue = rgb_2_hue( ColorAP0 );
	float centeredHue = center_hue( hue, RRT_RED_HUE );
	float hueWeight = Square( smoothstep( 0, 1, 1 - abs( 2 * centeredHue / RRT_RED_WIDTH ) ) );
		
	ColorAP0.r += hueWeight * saturation * (RRT_RED_PIVOT - ColorAP0.r) * (1. - RRT_RED_SCALE);
#endif
	
	// Use ACEScg primaries as working space
	float3 WorkingColor = mul( AP0_2_AP1_MAT, ColorAP0 );

	WorkingColor = max( 0, WorkingColor );

	// Pre desaturate
	WorkingColor = lerp( dot( WorkingColor, AP1_RGB2Y ), WorkingColor, 0.96 );
	
	const half ToeScale			= 1 + FilmBlackClip - FilmToe;
	const half ShoulderScale	= 1 + FilmWhiteClip - FilmShoulder;
	
	const float InMatch = 0.18;
	const float OutMatch = 0.18;

	float ToeMatch;
	if( FilmToe > 0.8 )
	{
		// 0.18 will be on straight segment
		ToeMatch = ( 1 - FilmToe  - OutMatch ) / FilmSlope + log10( InMatch );
	}
	else
	{
		// 0.18 will be on toe segment

		// Solve for ToeMatch such that input of InMatch gives output of OutMatch.
		const float bt = ( OutMatch + FilmBlackClip ) / ToeScale - 1;
		ToeMatch = log10( InMatch ) - 0.5 * log( (1+bt)/(1-bt) ) * (ToeScale / FilmSlope);
	}

	float StraightMatch = ( 1 - FilmToe ) / FilmSlope - ToeMatch;
	float ShoulderMatch = FilmShoulder / FilmSlope - StraightMatch;
	
	half3 LogColor = log10( WorkingColor );
	half3 StraightColor = FilmSlope * ( LogColor + StraightMatch );
	
	half3 ToeColor		= (    -FilmBlackClip ) + (2 *      ToeScale) / ( 1 + exp( (-2 * FilmSlope /      ToeScale) * ( LogColor -      ToeMatch ) ) );
	half3 ShoulderColor	= ( 1 + FilmWhiteClip ) - (2 * ShoulderScale) / ( 1 + exp( ( 2 * FilmSlope / ShoulderScale) * ( LogColor - ShoulderMatch ) ) );

	ToeColor		= LogColor <      ToeMatch ?      ToeColor : StraightColor;
	ShoulderColor	= LogColor > ShoulderMatch ? ShoulderColor : StraightColor;

	half3 t = saturate( ( LogColor - ToeMatch ) / ( ShoulderMatch - ToeMatch ) );
	t = ShoulderMatch < ToeMatch ? 1 - t : t;
	t = (3-2*t)*t*t;
	half3 ToneColor = lerp( ToeColor, ShoulderColor, t );

	// Post desaturate
	ToneColor = lerp( dot( float3(ToneColor), AP1_RGB2Y ), ToneColor, 0.93 );

	// Returning positive AP1 values
	return max( 0, ToneColor );
}
//
// 2D LUT grading
// scaleOffset = (1 / lut_width, 1 / lut_height, lut_height - 1)
//
half3 ApplyLut2D(TEX2D_ARGS(tex), half3 uvw, half3 scaleOffset)
{
    // Strip format where `height = sqrt(width)`
    uvw.z *= scaleOffset.z;
	half shift = floor(uvw.z);
    uvw.xy = uvw.xy * scaleOffset.z * scaleOffset.xy + scaleOffset.xy * 0.5;
    uvw.x += shift * scaleOffset.y;
    //uvw.xyz = lerp(
    //    SAMPLE_TEX2D(tex, uvw.xy).rgb,
    //    SAMPLE_TEX2D(tex, uvw.xy + half2(scaleOffset.y, 0.0)).rgb,
    //    uvw.z - shift
    //);
	uvw.xyz = lerp(
		SAMPLE_TEX2D(tex, uvw.xy).rgb,
		SAMPLE_TEX2D(tex, uvw.xy + half2(scaleOffset.y, 0.0)).rgb,
	    uvw.z - shift
	);
    return uvw;
}

half4 UnwrappedTexture3DSample( TEX2D_ARGS(tex), float3 UVW, float Size )
{
	// a volume texture 16x16x16 would be unwrapped to a 2d texture 256x16

	float IntW = floor( UVW.z * Size - 0.5 );
	half FracW = UVW.z * Size - 0.5 - IntW;

	float U = ( UVW.x + IntW ) / Size;
	float V = UVW.y;

	half4 RG0 = SAMPLE_TEX2D( tex, float2(U, V) );
	half4 RG1 = SAMPLE_TEX2D( tex, float2(U + 1.0f / Size, V) );

	return lerp(RG0, RG1, FracW);
}

static const FLOAT LUTSize = 32;
FLOAT3 ColorLookupTable(TEX2D_ARGS(tex), FLOAT3 LinearColor )
{
	FLOAT3 LUTEncodedColor = LinToLog( LinearColor + LogToLin( 0 ) );
	FLOAT3 UVW = LUTEncodedColor * ((LUTSize - 1) / LUTSize) + (0.5f / LUTSize);

    FLOAT3 OutDeviceColor = UnwrappedTexture3DSample( TEX2D_PARAM(tex), UVW, LUTSize ).rgb;
	
	return OutDeviceColor * 1.05;
}

//
// Returns the default value for a given position on a 2D strip-format color lookup table
// params = (lut_height, 0.5 / lut_width, 0.5 / lut_height, lut_height / lut_height - 1)
//
float3 GetLutStripValue(float2 uv, float4 params)
{
    uv -= params.yz;
    float3 color;
    color.r = frac(uv.x * params.x);
    color.b = uv.x - color.r / params.x;
    color.g = uv.y;
    return color * params.w;
}

//
// White balance
// Recommended workspace: ACEScg (linear)
//
static const float3x3 LIN_2_LMS_MAT = {
    3.90405e-1, 5.49941e-1, 8.92632e-3,
    7.08416e-2, 9.63172e-1, 1.35775e-3,
    2.31082e-2, 1.28021e-1, 9.36245e-1
};

static const float3x3 LMS_2_LIN_MAT = {
    2.85847e+0, -1.62879e+0, -2.48910e-2,
    -2.10182e-1,  1.15820e+0,  3.24281e-4,
    -4.18120e-2, -1.18169e-1,  1.06867e+0
};

float3 WhiteBalance1(float3 c, float3 balance)
{
    float3 lms = mul(LIN_2_LMS_MAT, c);
    lms *= balance;
    return mul(LMS_2_LIN_MAT, lms);
}

// Accurate for 1000K < Temp < 15000K
// [Krystek 1985, "An algorithm to calculate correlated colour temperature"]
float2 PlanckianLocusChromaticity( float Temp )
{
    float u = ( 0.860117757f + 1.54118254e-4f * Temp + 1.28641212e-7f * Temp*Temp ) / ( 1.0f + 8.42420235e-4f * Temp + 7.08145163e-7f * Temp*Temp );
    float v = ( 0.317398726f + 4.22806245e-5f * Temp + 4.20481691e-8f * Temp*Temp ) / ( 1.0f - 2.89741816e-5f * Temp + 1.61456053e-7f * Temp*Temp );

    float x = 3*u / ( 2*u - 8*v + 4 );
    float y = 2*v / ( 2*u - 8*v + 4 );

    return float2(x,y);
}

// Accurate for 4000K < Temp < 25000K
// in: correlated color temperature
// out: CIE 1931 chromaticity
float2 D_IlluminantChromaticity( float Temp )
{
    // Correct for revision of Plank's law
    // This makes 6500 == D65
    Temp *= 1.4388 / 1.438;

    float x =	Temp <= 7000 ?
                0.244063 + ( 0.09911e3 + ( 2.9678e6 - 4.6070e9 / Temp ) / Temp ) / Temp :
                0.237040 + ( 0.24748e3 + ( 1.9018e6 - 2.0064e9 / Temp ) / Temp ) / Temp;
    
    float y = -3 * x*x + 2.87 * x - 0.275;

    return float2(x,y);
}

float2 PlanckianIsothermal( float Temp, float Tint )
{
    float u = ( 0.860117757f + 1.54118254e-4f * Temp + 1.28641212e-7f * Temp*Temp ) / ( 1.0f + 8.42420235e-4f * Temp + 7.08145163e-7f * Temp*Temp );
    float v = ( 0.317398726f + 4.22806245e-5f * Temp + 4.20481691e-8f * Temp*Temp ) / ( 1.0f - 2.89741816e-5f * Temp + 1.61456053e-7f * Temp*Temp );

    float ud = ( -1.13758118e9f - 1.91615621e6f * Temp - 1.53177f * Temp*Temp ) / Square( 1.41213984e6f + 1189.62f * Temp + Temp*Temp );
    float vd = (  1.97471536e9f - 705674.0f * Temp - 308.607f * Temp*Temp ) / Square( 6.19363586e6f - 179.456f * Temp + Temp*Temp );

    float2 uvd = normalize( float2( u, v ) );

    // Correlated color temperature is meaningful within +/- 0.05
    u += -uvd.y * Tint * 0.05;
    v +=  uvd.x * Tint * 0.05;
    
    float x = 3*u / ( 2*u - 8*v + 4 );
    float y = 2*v / ( 2*u - 8*v + 4 );

    return float2(x,y);
}

float3 WhiteBalance2( float3 LinearColor,float whiteTemp,float whiteTint )
{
    float2 SrcWhiteDaylight = D_IlluminantChromaticity( whiteTemp );
    float2 SrcWhitePlankian = PlanckianLocusChromaticity( whiteTemp );

    float2 SrcWhite = whiteTemp < 4000 ? SrcWhitePlankian : SrcWhiteDaylight;
    float2 D65White = float2( 0.31270,  0.32900 );

    {
        // Offset along isotherm
        float2 Isothermal = PlanckianIsothermal( whiteTemp, whiteTint ) - SrcWhitePlankian;
        SrcWhite += Isothermal;
    }

    float3x3 WhiteBalanceMat = ChromaticAdaptation( SrcWhite, D65White );
    WhiteBalanceMat = mul( XYZ_2_sRGB_MAT, mul( WhiteBalanceMat, sRGB_2_XYZ_MAT ) );

    return mul( WhiteBalanceMat, LinearColor );
}
//
// RGB / Full-range YCbCr conversions (ITU-R BT.601)
//
float3 RgbToYCbCr(float3 c)
{
    float Y  =  0.299 * c.r + 0.587 * c.g + 0.114 * c.b;
    float Cb = -0.169 * c.r - 0.331 * c.g + 0.500 * c.b;
    float Cr =  0.500 * c.r - 0.419 * c.g - 0.081 * c.b;
    return float3(Y, Cb, Cr);
}

float3 YCbCrToRgb(float3 c)
{
    float R = c.x + 0.000 * c.y + 1.403 * c.z;
    float G = c.x - 0.344 * c.y - 0.714 * c.z;
    float B = c.x - 1.773 * c.y + 0.000 * c.z;
    return float3(R, G, B);
}

//
// Hue, Saturation, Value
// Ranges:
//  Hue [0.0, 1.0]
//  Sat [0.0, 1.0]
//  Lum [0.0, HALF_MAX]
//
float3 RgbToHsv(float3 c)
{
    float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
    float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
    float d = q.x - min(q.w, q.y);
    float e = EPSILON;
    return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

float3 HsvToRgb(float3 c)
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
}

float RotateHue(float value, float low, float hi)
{
    return (value < low)
            ? value + hi
            : (value > hi)
                ? value - hi
                : value;
}

//
// RGB Saturation (closer to a vibrance effect than actual saturation)
// Recommended workspace: ACEScg (linear)
// Optimal range: [0.0, 2.0]
//
float3 Saturation(float3 c, float sat)
{
    float luma = Luminance(c);
    return luma.xxx + sat.xxx * (c - luma.xxx);
}

//
// Contrast (reacts better when applied in log)
// Optimal range: [0.0, 2.0]
//
float3 Contrast(float3 c, float midpoint, float contrast)
{
    return (c - midpoint) * contrast + midpoint;
}

//
// Lift, Gamma (pre-inverted), Gain tuned for HDR use - best used with the ACES tonemapper as
// negative values will creep in the result
// Expected workspace: ACEScg (linear)
//
float3 LiftGammaGainHDR(float3 c, float3 lift, float3 invgamma, float3 gain)
{
    c = c * gain + lift;

    // ACEScg will output negative values, as clamping to 0 will lose precious information we'll
    // mirror the gamma function instead
    return FastSign(c) * pow(abs(c), invgamma);
}

//
// Lift, Gamma (pre-inverted), Gain tuned for LDR use
// Input is linear RGB
//
float3 LiftGammaGainLDR(float3 c, float3 lift, float3 invgamma, float3 gain)
{
    c = saturate(PositivePow(saturate(c), invgamma));
    return gain * c + lift * (1.0 - c);
}

//
// Remaps Y/R/G/B values
// curveTex has to be 128 pixels wide
//
float3 YrgbCurve(float3 c, TEX2D_ARGS(curveTex))
{
    const float kHalfPixel = (1.0 / 128.0) / 2.0;

    // Y (master)
    c += kHalfPixel.xxx;
    float mr = SAMPLE_TEX2D(curveTex, float2(c.r, 0.75)).a;
    float mg = SAMPLE_TEX2D(curveTex, float2(c.g, 0.75)).a;
    float mb = SAMPLE_TEX2D(curveTex, float2(c.b, 0.75)).a;
    c = saturate(float3(mr, mg, mb));

    // RGB
    c += kHalfPixel.xxx;
    float r = SAMPLE_TEX2D(curveTex, float2(c.r, 0.75)).r;
    float g = SAMPLE_TEX2D(curveTex, float2(c.g, 0.75)).g;
    float b = SAMPLE_TEX2D(curveTex, float2(c.b, 0.75)).b;
    return saturate(float3(r, g, b));
}

//
// Channel mixing (same as Photoshop's and DaVinci's Resolve)
// Recommended workspace: ACEScg (linear)
//      Input mixers should be in range [-2.0; 2.0]
//
float3 ChannelMixer(float3 c, float3 red, float3 green, float3 blue)
{
    return float3(
        dot(c, red),
        dot(c, green),
        dot(c, blue)
    );
}

float3 DropSaturationXColor(float3 x , float4 c)
{
    return lerp(Luminance(x),x,c.a)*c.rgb;
}

#define USE_COLOR_MATRIX	1
#define USE_SHADOW_TINT		1
#define USE_CONTRAST		1
// Film tonal and color control.
half4 ColorMatrixR_ColorCurveCd1;
half4 ColorMatrixG_ColorCurveCd3Cm3;
half4 ColorMatrixB_ColorCurveCm2;
half4 ColorCurve_Cm0Cd0_Cd2_Ch0Cm1_Ch3;
half4 ColorShadow_Luma;
half4 ColorShadow_Tint1;
half4 ColorShadow_Tint2;
#define ColorCurve_Ch1 ColorShadow_Luma.www
#define ColorCurve_Ch2 ColorShadow_Tint1.www

half3 FilmPostProcess(half3 LinearColor) 
{
	// Color and exposure control.
	half3 MatrixColor;
	#if USE_COLOR_MATRIX == 1
		// Apply color matrix (channel mixer, exposure, saturation).
		MatrixColor.r = dot(LinearColor, ColorMatrixR_ColorCurveCd1.rgb);
		MatrixColor.g = dot(LinearColor, ColorMatrixG_ColorCurveCd3Cm3.rgb);
		MatrixColor.b = dot(LinearColor, ColorMatrixB_ColorCurveCm2.rgb);
		#if USE_SHADOW_TINT == 1
			MatrixColor *= ColorShadow_Tint1.rgb + ColorShadow_Tint2.rgb * rcp(dot(LinearColor, ColorShadow_Luma.rgb) + 1.0);
		#endif
		// Required to insure saturation doesn't create negative colors!
		MatrixColor = max(half3(0.0, 0.0, 0.0), MatrixColor);
	#else
		// Less expensive route when not using saturation and channel mixer.
		#if USE_SHADOW_TINT == 1
			MatrixColor = LinearColor * (ColorShadow_Tint1.rgb + ColorShadow_Tint2.rgb * rcp(dot(LinearColor, ColorShadow_Luma.rgb) + 1.0));
		#else
			MatrixColor = LinearColor * ColorMatrixB_ColorCurveCm2.rgb;
		#endif
	#endif
	// Apply color curve (includes tonemapping).
	#if USE_CONTRAST == 1
		// Full path.
		half3 MatrixColorD = max(0, ColorCurve_Cm0Cd0_Cd2_Ch0Cm1_Ch3.xxx - MatrixColor);
		half3 MatrixColorH = max(MatrixColor, ColorCurve_Cm0Cd0_Cd2_Ch0Cm1_Ch3.zzz);
		half3 MatrixColorM = clamp(MatrixColor, ColorCurve_Cm0Cd0_Cd2_Ch0Cm1_Ch3.xxx, ColorCurve_Cm0Cd0_Cd2_Ch0Cm1_Ch3.zzz);
		half3 CurveColor = 
			// Highlights
			(MatrixColorH*ColorCurve_Ch1 + ColorCurve_Ch2) * rcp(MatrixColorH + ColorCurve_Cm0Cd0_Cd2_Ch0Cm1_Ch3.www) + 
				// Midtones
				((MatrixColorM*ColorMatrixB_ColorCurveCm2.aaa + 
					// Darks
					((MatrixColorD*ColorMatrixR_ColorCurveCd1.aaa) * rcp(MatrixColorD + ColorCurve_Cm0Cd0_Cd2_Ch0Cm1_Ch3.yyy) + ColorMatrixG_ColorCurveCd3Cm3.aaa)));
	#else
		// This is for mobile, it assumes color is not negative.
		// Fast path when contrast=1, can remove the dark part of the curve.
		half3 MatrixColorH = max(MatrixColor, ColorCurve_Cm0Cd0_Cd2_Ch0Cm1_Ch3.zzz);
		half3 MatrixColorM = min(MatrixColor, ColorCurve_Cm0Cd0_Cd2_Ch0Cm1_Ch3.zzz);
		half3 CurveColor = (MatrixColorH*ColorCurve_Ch1 + ColorCurve_Ch2) * rcp(MatrixColorH + ColorCurve_Cm0Cd0_Cd2_Ch0Cm1_Ch3.www) + MatrixColorM;
	#endif

	// Must hit black by log10(-2.474)
	CurveColor -= 0.002;

	return CurveColor;
}
half3 ApplyColorGrading(half3 input, float postExposure, 
    TEX2D_ARGS(lutTex), float3 lutParams)
{
    // Artist request to fine tune exposure in post without affecting bloom, dof etc
    input *= postExposure;

    float3 inputLutSpace = saturate(LinearToLogC(input)); // LUT space is in LogC
    input = ApplyLut2D(TEX2D_PARAM(lutTex), inputLutSpace, lutParams);

    /*UNITY_BRANCH
    if (userLutContrib > 0.0)
    {
        input = saturate(input);
        half3 outLut = ApplyLut2D(TEX2D_ARGS(userLutTex, userLutSampler), input, userLutParams);
        input = lerp(input, outLut, userLutContrib);
    }*/
    return input;
}
#endif // UNITY_POSTFX_COLOR
