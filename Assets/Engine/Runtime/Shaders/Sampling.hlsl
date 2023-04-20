#ifndef UNITY_POSTFX_SAMPLING
#define UNITY_POSTFX_SAMPLING

#include "StdLib.hlsl"
#include "Colors.hlsl"

// Better, temporally stable box filtering
// [Jimenez14] http://goo.gl/eomGso
// . . . . . . .
// . A . B . C .
// . . D . E . .
// . F . G . H .
// . . I . J . .
// . K . L . M .
// . . . . . . .
half4 DownsampleBox13Tap(TEX2D_ARGS(tex), FLOAT2 uv, FLOAT2 texelSize)
{
    half4 A = SAMPLE_TEX2D(tex, (uv + texelSize * FLOAT2(-1.0, -1.0)));
    half4 B = SAMPLE_TEX2D(tex, (uv + texelSize * FLOAT2( 0.0, -1.0)));
    half4 C = SAMPLE_TEX2D(tex, (uv + texelSize * FLOAT2( 1.0, -1.0)));
    half4 D = SAMPLE_TEX2D(tex, (uv + texelSize * FLOAT2(-0.5, -0.5)));
    half4 E = SAMPLE_TEX2D(tex, (uv + texelSize * FLOAT2( 0.5, -0.5)));
    half4 F = SAMPLE_TEX2D(tex, (uv + texelSize * FLOAT2(-1.0,  0.0)));
    half4 G = SAMPLE_TEX2D(tex, (uv                                 ));
    half4 H = SAMPLE_TEX2D(tex, (uv + texelSize * FLOAT2( 1.0,  0.0)));
    half4 I = SAMPLE_TEX2D(tex, (uv + texelSize * FLOAT2(-0.5,  0.5)));
    half4 J = SAMPLE_TEX2D(tex, (uv + texelSize * FLOAT2( 0.5,  0.5)));
    half4 K = SAMPLE_TEX2D(tex, (uv + texelSize * FLOAT2(-1.0,  1.0)));
    half4 L = SAMPLE_TEX2D(tex, (uv + texelSize * FLOAT2( 0.0,  1.0)));
    half4 M = SAMPLE_TEX2D(tex, (uv + texelSize * FLOAT2( 1.0,  1.0)));

    half2 div = (1.0 / 4.0) * half2(0.5, 0.125);

    half4 o = (D + E + I + J) * div.x;
    o += (A + B + G + F) * div.y;
    o += (B + C + H + G) * div.y;
    o += (F + G + L + K) * div.y;
    o += (G + H + M + L) * div.y;

    return o;
}

// Standard box filtering
half4 DownsampleBox4Tap(TEX2D_ARGS(tex), FLOAT2 uv, FLOAT2 texelSize)
{
    FLOAT4 d = texelSize.xyxy * FLOAT4(-1.0, -1.0, 1.0, 1.0);

    half4 s;
    s =  (SAMPLE_TEX2D(tex, (uv + d.xy)));
    s += (SAMPLE_TEX2D(tex, (uv + d.zy)));
    s += (SAMPLE_TEX2D(tex, (uv + d.xw)));
    s += (SAMPLE_TEX2D(tex, (uv + d.zw)));

    return s * (1.0 / 4.0);
}
half4 DownsampleBox4TapAntiFlicker(TEX2D_ARGS(tex), FLOAT2 uv, FLOAT2 texelSize)
{
    FLOAT4 d = texelSize.xyxy * FLOAT4(-1.0, -1.0, 1.0, 1.0);
    
    FLOAT4 s1 = (SAMPLE_TEX2D(tex, (uv + d.xy)));
    FLOAT4 s2 = (SAMPLE_TEX2D(tex, (uv + d.zy)));
    FLOAT4 s3 = (SAMPLE_TEX2D(tex, (uv + d.xw)));
    FLOAT4 s4 = (SAMPLE_TEX2D(tex, (uv + d.zw)));
    FLOAT4 s  = (s1+s2+s3+s4)*0.25;

	FLOAT s1w = rcp(Brightness(s1.xyz) + 1);
	FLOAT s2w = rcp(Brightness(s2.xyz) + 1);
	FLOAT s3w = rcp(Brightness(s3.xyz) + 1);
	FLOAT s4w = rcp(Brightness(s4.xyz) + 1);
	FLOAT one_div_wsum = rcp(s1w + s2w + s3w + s4w);
	s = (s1 * s1w + s2 * s2w + s3 * s3w + s4 * s4w) * one_div_wsum;
    return s;
}
half4 DownsampleBoxMedian(TEX2D_ARGS(tex), FLOAT2 uv, FLOAT2 texelSize)
{
    FLOAT4 d = texelSize.xyxy * FLOAT4(-1.0, -1.0, 1.0, 1.0);

    half4 s0,s1,s2,s3,s4;
    s0 = (SAMPLE_TEX2D(tex, (uv)));
    s1 = (SAMPLE_TEX2D(tex, (uv + d.xy)));
    s2 = (SAMPLE_TEX2D(tex, (uv + d.zy)));
    s3 = (SAMPLE_TEX2D(tex, (uv + d.xw)));
    s4 = (SAMPLE_TEX2D(tex, (uv + d.zw)));

    s0 = s0 + s1 + s2 - Max3(s0,s1,s2) - Min3(s0,s1,s2);
    s0 = s0 + s3 + s4 - Max3(s0,s3,s4) - Min3(s0,s3,s4);

    return s0;
}

// 9-tap bilinear upsampler (tent filter)
half4 UpsampleTent(TEX2D_ARGS(tex), FLOAT2 uv, FLOAT2 texelSize, FLOAT sampleScale)
{
    FLOAT4 d = texelSize.xyxy * FLOAT4(1.0, 1.0, -1.0, 0.0) * sampleScale;

    half4 s;
    s =  SAMPLE_TEX2D(tex, (uv - d.xy));
    s += SAMPLE_TEX2D(tex, (uv - d.wy)) * 2.0;
    s += SAMPLE_TEX2D(tex, (uv - d.zy));

    s += SAMPLE_TEX2D(tex, (uv + d.zw)) * 2.0;
    s += SAMPLE_TEX2D(tex, (uv       )) * 4.0;
    s += SAMPLE_TEX2D(tex, (uv + d.xw)) * 2.0;

    s += SAMPLE_TEX2D(tex, (uv + d.zy));
    s += SAMPLE_TEX2D(tex, (uv + d.wy)) * 2.0;
    s += SAMPLE_TEX2D(tex, (uv + d.xy));

    return s * (1.0 / 16.0);
}

// Standard box filtering
half4 UpsampleBox(TEX2D_ARGS(tex), FLOAT2 uv, FLOAT2 texelSize, FLOAT sampleScale)
{
    FLOAT4 d = texelSize.xyxy * FLOAT4(-1.0, -1.0, 1.0, 1.0) * (sampleScale * 0.5);

    half4 s;
    s =  (SAMPLE_TEX2D(tex, (uv + d.xy)));
    s += (SAMPLE_TEX2D(tex, (uv + d.zy)));
    s += (SAMPLE_TEX2D(tex, (uv + d.xw)));
    s += (SAMPLE_TEX2D(tex, (uv + d.zw)));

    return s * (1.0 / 4.0);
}

half4 UpsampleBox4(TEX2D_ARGS(tex0), TEX2D_ARGS(tex1), TEX2D_ARGS(tex2), 
		FLOAT2 uv, FLOAT2 texelSize, FLOAT sampleScale)
{
	FLOAT4 d0 = texelSize.xyxy * FLOAT4(-1.0, -1.0, 1.0, 1.0) * (sampleScale * 0.5);
	FLOAT4 d1 = d0* 0.5;
	FLOAT4 d2 = d1* 0.5;
	FLOAT4 d3 = d2* 0.5;

	half4 s0 = (SAMPLE_TEX2D(tex0, (uv + d0.xy)));
	half4 s1 = (SAMPLE_TEX2D(tex1, (uv + d1.xy)));	
	half4 s2 = (SAMPLE_TEX2D(tex2, (uv + d2.xy)));

	half4 s2sum = s2 + (SAMPLE_TEX2D(tex2, (uv + d2.zy)));
	s2sum += (SAMPLE_TEX2D(tex2, (uv + d2.xw)));
	s2sum += (SAMPLE_TEX2D(tex2, (uv + d2.zw)));
	s2sum *= 0.5f;

	half4 s1sum = s1 + (SAMPLE_TEX2D(tex1, (uv + d1.zy)));
	s1sum += (SAMPLE_TEX2D(tex1, (uv + d1.xw)));
	s1sum += (SAMPLE_TEX2D(tex1, (uv + d1.zw)));
	s1sum *= 0.5f;
	s1sum += s2sum;

	half4 s0sum = s0 + (SAMPLE_TEX2D(tex0, (uv + d0.zy)));
	s0sum += (SAMPLE_TEX2D(tex0, (uv + d0.xw)));
	s0sum += (SAMPLE_TEX2D(tex0, (uv + d0.zw)));
	s0sum *= 0.25f;
	s0sum += s1sum;

	return s0sum;
}

//Next-Generation-Post-Processing-in-Call-of-Duty-Advanced-Warfare-v17
inline void Downsample13CoD(TEX2D_ARGS(tex), FLOAT2 uv, FLOAT2 texelSize,
    out FLOAT4 tap0,
    out FLOAT4 tap1,
    out FLOAT4 tap2,
    out FLOAT4 tap3,
    out FLOAT4 tap4)
{
    FLOAT4 d = texelSize.xyxy * FLOAT4(-1.0, -1.0, 1.0, 1.0);

    FLOAT4 s0 = (SAMPLE_TEX2D(tex, (uv)));
    //QUAD 0
    FLOAT4 s1 = SAMPLE_TEX2D(tex, (uv + d.xy));
    FLOAT4 s2 = SAMPLE_TEX2D(tex, (uv + d.zy));
    FLOAT4 s3 = SAMPLE_TEX2D(tex, (uv + d.xw));
    FLOAT4 s4 = SAMPLE_TEX2D(tex, (uv + d.zw));

    //QUAD 1 - Left up
    FLOAT4 s5 = SAMPLE_TEX2D(tex, (uv + d.xy*2));
    FLOAT4 s6 = SAMPLE_TEX2D(tex, (uv + FLOAT2(0,d.y*2)));
    FLOAT4 s7 = SAMPLE_TEX2D(tex, (uv + FLOAT2(d.x*2,0)));

    //QUAD 2 - Right up
    FLOAT4 s8 = SAMPLE_TEX2D(tex, (uv + FLOAT2(d.x*2,d.w*2)));
    FLOAT4 s9 = SAMPLE_TEX2D(tex, (uv + FLOAT2(d.x*2,0)));

    //QUAD 3 - Right down
    FLOAT4 s10 = SAMPLE_TEX2D(tex, (uv + d.zw*2));
    FLOAT4 s11 = SAMPLE_TEX2D(tex, (uv + FLOAT2(0,d.w*2)));

    //QUAD 4 - Left down
    FLOAT4 s12 = SAMPLE_TEX2D(tex, (uv + FLOAT2(d.x*2,d.w*2)));

    tap0 = s1*0.25 + s2*0.25 + s3*0.25 + s4*0.25;
    tap1 = s5*0.25 + s6*0.25 + s0*0.25 + s7*0.25;
    tap2 = s6*0.25 + s8*0.25 + s9*0.25 + s0*0.25;
    tap3 = s0*0.25 + s9*0.25 + s10*0.25 + s11*0.25;
    tap4 = s7*0.25 + s0*0.25 + s11*0.25 + s12*0.25;
}

FLOAT4 Downsample13CoD(TEX2D_ARGS(tex), FLOAT2 uv, FLOAT2 texelSize)
{
    FLOAT4 tap0;
    FLOAT4 tap1;
    FLOAT4 tap2;
    FLOAT4 tap3;
    FLOAT4 tap4;
    Downsample13CoD(TEX2D_PARAM(tex),uv,texelSize,tap0,tap1,tap2,tap3,tap4);   

    return tap0*0.5 + tap1*0.125 + tap2*0.125 + tap3*0.125 + tap4*0.125;
}

FLOAT4 Downsample13CoDFireFlies(TEX2D_ARGS(tex), FLOAT2 uv, FLOAT2 texelSize)
{
    FLOAT4 tap0;
    FLOAT4 tap1;
    FLOAT4 tap2;
    FLOAT4 tap3;
    FLOAT4 tap4;
    Downsample13CoD(TEX2D_PARAM(tex),uv,texelSize,tap0,tap1,tap2,tap3,tap4);   
    FLOAT l0 = Luminance(tap0);
    tap0 *= rcp(1+l0)*0.5;

    FLOAT l1 = Luminance(tap1);
    tap1 *= rcp(1+l1)*0.125;

    FLOAT l2 = Luminance(tap2);
    tap2 *= rcp(1+l2)*0.125;

    FLOAT l3 = Luminance(tap3);
    tap3 *= rcp(1+l3)*0.125;

    FLOAT l4 = Luminance(tap4);
    tap4 *= rcp(1+l4)*0.125;


    return tap0 + tap1 + tap2 + tap3 + tap4;
}

FLOAT2 Circle(FLOAT slices, FLOAT index, FLOAT start)
{
    FLOAT rad = (TWO_PI * (1.0 / slices)) * (index + start);
    FLOAT s;
    FLOAT c;
    sincos(rad, s, c);
    return FLOAT2(c, s);
}

FLOAT2 BSpline3MiddleLeft(FLOAT2 x)
{
    return 0.16666667 + x * (0.5 + x * (0.5 - x * 0.5));
}

FLOAT2 BSpline3MiddleRight(FLOAT2 x)
{
    return 0.66666667 + x * (-1.0 + 0.5 * x) * x;
}

FLOAT2 BSpline3Rightmost(FLOAT2 x)
{
    return 0.16666667 + x * (-0.5 + x * (0.5 - x * 0.16666667));
}

// Compute weights & offsets for 4x bilinear taps for the bicubic B-Spline filter.
// The fractional coordinate should be in the [0, 1] range (centered on 0.5).
// Inspired by: http://vec3.ca/bicubic-filtering-in-fewer-taps/
void BicubicFilter(FLOAT2 fracCoord, out FLOAT2 weights[2], out FLOAT2 offsets[2])
{
    FLOAT2 r  = BSpline3Rightmost(fracCoord);
    FLOAT2 mr = BSpline3MiddleRight(fracCoord);
    FLOAT2 ml = BSpline3MiddleLeft(fracCoord);
    FLOAT2 l  = 1.0 - mr - ml - r;

    weights[0] = r + mr;
    weights[1] = ml + l;
    offsets[0] = -1.0 + mr * rcp(weights[0]);
    offsets[1] =  1.0 + l * rcp(weights[1]);
}



// texSize = (width, height, 1/width, 1/height)
FLOAT4 SampleTexture2DBicubic(TEX2D_ARGS(tex), FLOAT2 coord, FLOAT4 texSize, FLOAT2 maxCoord)
{
    FLOAT2 xy = coord * texSize.xy + 0.5;
    FLOAT2 ic = floor(xy);
    FLOAT2 fc = frac(xy);

    FLOAT2 weights[2], offsets[2];
    BicubicFilter(fc, weights, offsets);

    return weights[0].y * (weights[0].x * SAMPLE_TEX2D_LOD(tex, min((ic + FLOAT2(offsets[0].x, offsets[0].y) - 0.5) * texSize.zw, maxCoord), 0.0)  +
                           weights[1].x * SAMPLE_TEX2D_LOD(tex, min((ic + FLOAT2(offsets[1].x, offsets[0].y) - 0.5) * texSize.zw, maxCoord), 0.0)) +
           weights[1].y * (weights[0].x * SAMPLE_TEX2D_LOD(tex, min((ic + FLOAT2(offsets[0].x, offsets[1].y) - 0.5) * texSize.zw, maxCoord), 0.0)  +
                           weights[1].x * SAMPLE_TEX2D_LOD(tex, min((ic + FLOAT2(offsets[1].x, offsets[1].y) - 0.5) * texSize.zw, maxCoord), 0.0));
}
#endif // UNITY_POSTFX_SAMPLING
