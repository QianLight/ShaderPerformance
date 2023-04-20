#include <HLSLSupport.cginc>
#ifndef FXAA_INCLUDE
#define FXAA_INCLUDE

#define ISMOBILE
//#define LUMINANCE_GREEN

#ifndef _MainTex_TexelSize
    #define _MainTex_TexelSize _SourceTex_TexelSize
#endif

#if defined(ISMOBILE)
#define EDGE_STEP_COUNT 3
#define EDGE_STEPS 0.75f, 0.75f, 2.0f
#define EDGE_GUESS 8.0f
#else
#define EDGE_STEP_COUNT 10
#define EDGE_STEPS 0.5f, 1.0f, 1.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 2.0f, 4.0f
#define EDGE_GUESS 8.0f

#endif
/*
9 pix filter
NW    N    NE
W     M    E
SW    S    SE
*/
static const float edgeSteps[EDGE_STEP_COUNT] = { EDGE_STEPS };
//#define _ContrastThreshold  0.0312f
//#define _RelativeThreshold  0.063f
//#define _ContrastThreshold  0.0512f
//#define _RelativeThreshold  0.093f
//#define FXAA_EDGE_THRESHOLD      (1.0/8.0)
//#define FXAA_EDGE_THRESHOLD_MIN  (1.0/24.0)

#define FXAA_EDGE_THRESHOLD      (1.0/24.0)
#define FXAA_EDGE_THRESHOLD_MIN  (1.0/72.0)

#define _PointScale  1.0f
float _Fxaa_Sharp;
float4 Sample(float2 uv)
{
    return SAMPLE_TEXTURE2D_X(_SourceTex, sampler_SourceTex_LinearClamp, uv);
}
//
//float GetLuminance(float4 col)
//{
//#if defined(LUMINANCE_GREEN)
//    //return col.y;
//    //return col.y * (0.587 / 0.299) + col.x;
//    return dot(col.rgb, float3(0.333, 0.6544, 0));
//#else
//    return col.a;
//#endif
//}

float GetLuminance(float4 col)
{
#if defined(_FXAA_LUMINANCE)
    return col.a;
#else
    //return col.y;
    //return col.y * (0.587 / 0.299) + col.x;
    return dot(col.rgb, float3(0.333, 0.6544, 0));
#endif
}

float SampleLuminance(float2 uv)
{
    return GetLuminance(Sample(uv));
}

float SampleLuminance(float2 uv, float uOffset, float vOffset)
{
    uv += _MainTex_TexelSize.xy * float2(uOffset, vOffset) * _PointScale;
    return SampleLuminance(uv);
}
float4 SampleLuminance(float2 uv, float uOffset, float vOffset, out float luma)
{
    uv += _MainTex_TexelSize.xy * float2(uOffset, vOffset) * _PointScale;
    float4 col = Sample(uv);
    luma = GetLuminance(col);
    return col;
}
struct LuminanceData
{
    float m, n, e, s, w, highest, lowest, contrast, ne, nw, se, sw;
};

LuminanceData SampleLuminanceNeighborhood(half4 color, float2 uv, out float4 blur)
{
    LuminanceData l;
    l.m = GetLuminance(color);

    blur = color;
    blur += SampleLuminance(uv, 0, 1, l.n);
    blur += SampleLuminance(uv, 1, 0, l.e);
    blur += SampleLuminance(uv, 0, -1, l.s);
    blur += SampleLuminance(uv, -1, 0, l.w);

    l.highest = max(max(max(max(l.n, l.e), l.s), l.w), l.m);
    l.lowest = min(min(min(min(l.n, l.e), l.s), l.w), l.m);
    l.contrast = l.highest - l.lowest;

    return l;
}

bool ShouldSkipPixel(LuminanceData l)
{
    //float threshold = max(_ContrastThreshold, _RelativeThreshold * l.highest);
    float threshold = max(FXAA_EDGE_THRESHOLD_MIN, FXAA_EDGE_THRESHOLD * l.highest);
    return l.contrast < threshold;
}

struct EdgeData
{
    bool isHorizontal;
    float pixelStep, oppositeLuminance, gradient;
};
EdgeData DetermineEdge(LuminanceData l)
{
    EdgeData e;
    float horizontal =
        abs(l.n + l.s - 2 * l.m) * 2 +
        abs(l.ne + l.se - 2 * l.e) +
        abs(l.nw + l.sw - 2 * l.w);
    float vertical =
        abs(l.e + l.w - 2 * l.m) * 2 +
        abs(l.ne + l.nw - 2 * l.n) +
        abs(l.se + l.sw - 2 * l.s);
    e.isHorizontal = horizontal >= vertical;
    e.pixelStep = e.isHorizontal ? _MainTex_TexelSize.y : _MainTex_TexelSize.x;
    float pLuminance = e.isHorizontal ? l.n : l.e;
    float nLuminance = e.isHorizontal ? l.s : l.w;
    float pGradient = abs(pLuminance - l.m);
    float nGradient = abs(nLuminance - l.m);
    if (pGradient < nGradient)
    {
        e.pixelStep = -e.pixelStep;
        e.oppositeLuminance = nLuminance;
        e.gradient = nGradient;
    }
    else
    {
        e.oppositeLuminance = pLuminance;
        e.gradient = pGradient;
    }
    return e;
}
float DetermineEdgeBlendFactor(LuminanceData l, EdgeData e, float2 uv)
{
    float2 uvEdge = uv;
    float2 edgeStep;
    if (e.isHorizontal)
    {
        uvEdge.y += e.pixelStep * 0.5f;
        edgeStep = float2(_MainTex_TexelSize.x, 0.0f);
    }
    else
    {
        uvEdge.x += e.pixelStep * 0.5f;
        edgeStep = float2(0.0h, _MainTex_TexelSize.y);
    }
    float edgeLuminance = (l.m + e.oppositeLuminance) * 0.5f;
    float gradientThreshold = e.gradient * 0.25f;

    float2 puv = uvEdge + edgeStep * edgeSteps[0];
    float pLuminanceDelta = SampleLuminance(puv) - edgeLuminance;
    bool pAtEnd = abs(pLuminanceDelta) >= gradientThreshold;

    UNITY_UNROLL
    for (int i = 0; i < EDGE_STEP_COUNT && !pAtEnd; i++)
    {
        puv += edgeStep * edgeSteps[i];
        pLuminanceDelta = SampleLuminance(puv) - edgeLuminance;
        pAtEnd = abs(pLuminanceDelta) >= gradientThreshold;
    }

    if (!pAtEnd)
    {
        puv += edgeStep * EDGE_GUESS;
    }

    float2 nuv = uvEdge - edgeStep;
    float nLuminanceDelta = SampleLuminance(nuv) - edgeLuminance;
    bool nAtEnd = abs(nLuminanceDelta) >= gradientThreshold;

    UNITY_UNROLL
    for (int i = 0; i < EDGE_STEP_COUNT && !nAtEnd; i++)
    {
        nuv -= edgeStep * edgeSteps[i];
        nLuminanceDelta = SampleLuminance(nuv) - edgeLuminance;
        nAtEnd = abs(nLuminanceDelta) >= gradientThreshold;
    }

    if (!nAtEnd)
    {
        nuv -= edgeStep * EDGE_GUESS;
    }
    float pDistance, nDistance;

    if (e.isHorizontal)
    {
        pDistance = puv.x - uv.x;
        nDistance = uv.x - nuv.x;
    }
    else
    {
        pDistance = puv.y - uv.y;
        nDistance = uv.y - nuv.y;
    }

    float shortestDistance;
    bool deltaSign;

    if (pDistance <= nDistance)
    {
        shortestDistance = pDistance;
        deltaSign = pLuminanceDelta >= 0;
    }
    else
    {
        shortestDistance = nDistance;
        deltaSign = nLuminanceDelta >= 0;
    }

    if (deltaSign == (l.m - edgeLuminance >= 0))
    {
        return 0;
    }
    else
    {
        return 0.5 - shortestDistance / (pDistance + nDistance);
    }

}
half4 FXAA(half4 color, float2 uv)
{
    float4 blur;
    LuminanceData l = SampleLuminanceNeighborhood(color, uv, blur);
    UNITY_BRANCH
    if (ShouldSkipPixel(l))
    {
        return color;
    }

    blur += SampleLuminance(uv, -1, 1, l.nw);
    blur += SampleLuminance(uv, 1, 1, l.ne);
    blur += SampleLuminance(uv, 1, -1, l.se);
    blur += SampleLuminance(uv, -1, -1, l.sw);
    blur = blur * .1111111f;

    EdgeData e = DetermineEdge(l);
    float edgeBlend = DetermineEdgeBlendFactor(l, e, uv);
    float finalBlend = saturate(edgeBlend);

    if (e.isHorizontal)
    {
        uv.y += e.pixelStep * finalBlend;
    }
    else
    {
        uv.x += e.pixelStep * finalBlend;
    }
    half4 colRAW = Sample(uv);
    //if (abs(colRAW.g - color.g) > 0.02f)
    //{
    //    _Fxaa_Sharp = 0.75f;
    //}
    return half4(lerp(blur.rgb, colRAW.rgb, _Fxaa_Sharp), colRAW.a);
}
//half3 FXAA(half3 color, float2 uv)
//{
//    LuminanceData l = SampleLuminanceNeighborhood(color, uv);
//    if (ShouldSkipPixel(l))
//    {
//        return l.col;
//    }
//    float pixelBlend = 0.0h;
//    EdgeData e = DetermineEdge(l);
//    float edgeBlend = DetermineEdgeBlendFactor(l, e, uv);
//    float finalBlend = max(pixelBlend, edgeBlend);
//    if (e.isHorizontal)
//    {
//        uv.y += e.pixelStep * finalBlend;
//    }
//    else
//    {
//        uv.x += e.pixelStep * finalBlend;
//    }
//    return lerp(l.blur, Sample(uv).rgb, _Fxaa_Sharp);
//}
#endif