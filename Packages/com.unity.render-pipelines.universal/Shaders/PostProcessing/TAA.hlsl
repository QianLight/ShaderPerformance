#define MINMAX_3X3 //MINMAX_3X3_ROUNDED //MINMAX_4TAP_VARYING
#define UNJITTER_COLORSAMPLES
#define USE_CLIPPING
//#define USE_MOTION_BLUR
#define USE_OPTIMIZATIONS
#define USE_ANTI_FLICKERING
#define USE_DILATION_5TAP

TEXTURE2D_X(_LastTex);

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

#if defined(_MIX_AA)
#include "FXAA.hlsl"
#endif

#if SHADER_API_MOBILE
static const float FLTEPS = 0.0001f;
#else
static const float FLTEPS = 0.00000001f;
#endif

uniform float4 _ProjectionExtents;
uniform float4x4 _PrevVP;

uniform float4 _Parameter;
uniform float4 _Parameter2;
uniform float4 _Parameter3;

//uniform float4 _SourceTex_TexelSize;
#define _FeedbackMin _Parameter.x
#define _FeedbackMax _Parameter.y
#define _FeedbackSpeed _Parameter.z
#define _Sharpness _Parameter.w

#define _UvAtTop _Parameter2.z
#define _AABBSize _Parameter2.w

#define _MotionFallOff _Parameter3.xy
#define _MotionScale _Parameter3.z

#define _MainTex_TexelSize _SourceTex_TexelSize
#define sampler2D Texture2D 

#define SampleDepth(uv) SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, uv)
#define SampleColor(tex, uv) SAMPLE_TEXTURE2D_X(tex, sampler_SourceTex_LinearClamp, uv)

float PDnrand(float2 n)
{
	return frac(sin(dot(n.xy, float2(12.9898f, 78.233f))) * 43758.5453f);
}
float4 PDnrand4(float2 n)
{
	return frac(sin(dot(n.xy, float2(12.9898f, 78.233f))) * float4(43758.5453f, 28001.8384f, 50849.4141f, 12996.89f));
}
//note: signed random, float=[-1;1[
float PDsrand(float2 n)
{
	return PDnrand(n) * 2 - 1;
}
float4 PDsrand4(float2 n) {
	return PDnrand4(n) * 2 - 1;
}
// https://software.intel.com/en-us/node/503873
float3 RGB_YCoCg(float3 c)
{
	// Y = R/4 + G/2 + B/4
	// Co = R/2 - B/2
	// Cg = -R/4 + G/2 - B/4
	return float3(
		c.x / 4.0 + c.y / 2.0 + c.z / 4.0,
		c.x / 2.0 - c.z / 2.0,
		-c.x / 4.0 + c.y / 2.0 - c.z / 4.0
		);
}

// https://software.intel.com/en-us/node/503873
float3 YCoCg_RGB(float3 c)
{
	// R = Y + Co - Cg
	// G = Y + Cg
	// B = Y - Co - Cg
	return saturate(float3(
		c.x + c.y - c.z,
		c.x + c.z,
		c.x - c.y - c.z
		));
}

float4 sample_color(sampler2D tex, float2 uv)
{
#if defined(USE_YCOCG)
	float4 c = SampleColor(tex, uv);
	return float4(RGB_YCoCg(c.rgb), c.a);
#else
	return SampleColor(tex, uv);
#endif
}
float4 resolve_color(float4 c)
{
#if defined(USE_YCOCG)
	return float4(YCoCg_RGB(c.rgb).rgb, c.a);
#else
	return c;
#endif
}

#if defined(UNITY_REVERSED_Z)
#define ZCMP_GT(a, b) (a < b)
#else
#define ZCMP_GT(a, b) (a > b)
#endif

float depth_resolve_linear(float z)
{
#if defined(CAMERA_ORTHOGRAPHIC)
#if defined(UNITY_REVERSED_Z)
	return (1.0 - z) * (_ZBufferParams.z - _ZBufferParams.y) + _ZBufferParams.y;
#else
	return z * (_ZBufferParams.z - _ZBufferParams.y) + _ZBufferParams.y;
#endif
#else
	return LinearEyeDepth(z, _ZBufferParams);
#endif
}

float depth_sample_linear(float2 uv)
{
	return depth_resolve_linear(SampleDepth(uv).x);
}

#if defined(USE_DILATION_5TAP) || defined(USE_DILATION_3X3)
float3 find_closest_fragment_3x3(float2 uv)
{
	float2 dd = abs(_MainTex_TexelSize.xy);
	float2 du = float2(dd.x, 0.0);
	float2 dv = float2(0.0, dd.y);

	float3 dtl = float3(-1, -1, SampleDepth(uv - dv - du).x);
	float3 dtc = float3(0, -1, SampleDepth(uv - dv).x);
	float3 dtr = float3(1, -1, SampleDepth(uv - dv + du).x);

	float3 dml = float3(-1, 0, SampleDepth(uv - du).x);
	float3 dmc = float3(0, 0, SampleDepth(uv).x);
	float3 dmr = float3(1, 0, SampleDepth(uv + du).x);

	float3 dbl = float3(-1, 1, SampleDepth(uv + dv - du).x);
	float3 dbc = float3(0, 1, SampleDepth(uv + dv).x);
	float3 dbr = float3(1, 1, SampleDepth(uv + dv + du).x);

	float3 dmin = dtl;
	if (ZCMP_GT(dmin.z, dtc.z)) dmin = dtc;
	if (ZCMP_GT(dmin.z, dtr.z)) dmin = dtr;

	if (ZCMP_GT(dmin.z, dml.z)) dmin = dml;
	if (ZCMP_GT(dmin.z, dmc.z)) dmin = dmc;
	if (ZCMP_GT(dmin.z, dmr.z)) dmin = dmr;

	if (ZCMP_GT(dmin.z, dbl.z)) dmin = dbl;
	if (ZCMP_GT(dmin.z, dbc.z)) dmin = dbc;
	if (ZCMP_GT(dmin.z, dbr.z)) dmin = dbr;

	return float3(uv + dd.xy * dmin.xy, dmin.z);
}

float3 find_closest_fragment_5tap(float2 uv)
{
	float2 dd = abs(_MainTex_TexelSize.xy);
	float2 du = float2(dd.x, 0.0);
	float2 dv = float2(0.0, dd.y);

	float2 tl = -dv - du;
	float2 tr = -dv + du;
	float2 bl = dv - du;
	float2 br = dv + du;

	float dtl = SampleDepth(uv + tl).x;
	float dtr = SampleDepth(uv + tr).x;
	float dmc = SampleDepth(uv).x;
	float dbl = SampleDepth(uv + bl).x;
	float dbr = SampleDepth(uv + br).x;

	float dmin = dmc;
	float2 dif = 0.0;

	if (ZCMP_GT(dmin, dtl)) { dmin = dtl; dif = tl; }
	if (ZCMP_GT(dmin, dtr)) { dmin = dtr; dif = tr; }
	if (ZCMP_GT(dmin, dbl)) { dmin = dbl; dif = bl; }
	if (ZCMP_GT(dmin, dbr)) { dmin = dbr; dif = br; }

	return float3(uv + dif, dmin);
}
#endif


float4 clip_aabb(float3 aabb_min, float3 aabb_max, float4 p, float4 q)
{
#if defined(USE_OPTIMIZATIONS)
	// note: only clips towards aabb center (but fast!)
	float3 p_clip = 0.5 * (aabb_max + aabb_min);
	float3 e_clip = 0.5 * (aabb_max - aabb_min) + FLTEPS;

	float4 v_clip = q - float4(p_clip, p.w);
	float3 v_unit = v_clip.xyz / e_clip;
	float3 a_unit = abs(v_unit);
	float ma_unit = max(a_unit.x, max(a_unit.y, a_unit.z));

	if (ma_unit > 1.0)
		return float4(p_clip, p.w) + v_clip / ma_unit;
	else
		return q;// point inside aabb
#else
	float4 r = q - p;
	float3 rmax = aabb_max - p.xyz;
	float3 rmin = aabb_min - p.xyz;

	const float eps = FLTEPS;

	if (r.x > rmax.x + eps)
		r *= (rmax.x / r.x);
	if (r.y > rmax.y + eps)
		r *= (rmax.y / r.y);
	if (r.z > rmax.z + eps)
		r *= (rmax.z / r.z);

	if (r.x < rmin.x - eps)
		r *= (rmin.x / r.x);
	if (r.y < rmin.y - eps)
		r *= (rmin.y / r.y);
	if (r.z < rmin.z - eps)
		r *= (rmin.z / r.z);

	return p + r;
#endif
}

float4 sample_color_motion(sampler2D tex, float2 uv, float2 ss_vel)
{
	const float2 v = 0.5 * ss_vel;
	const int taps = 3;// on either side!

	float srand = PDsrand(uv + _SinTime.xx);
	float2 vtap = v / taps;
	float2 pos0 = uv + vtap * (0.5 * srand);
	float4 accu = 0.0;
	float wsum = 0.0;

	[unroll]
	for (int i = 0; i > -taps; i--)
	{
		float w = 1.0;// box
		//float w = taps - abs(i) + 1;// triangle
		//float w = 1.0 / (1 + abs(i));// pointy triangle
		accu += w * sample_color(tex, pos0 + i * vtap);
		wsum += w;
	}
	float4 result = accu / wsum;
	return result;
}

void get_velocity(float2 uv, out float2 ss_vel, out float vs_dist)
{

#if defined(USE_DILATION_5TAP) || defined(USE_DILATION_3X3)
#if defined(USE_DILATION_5TAP)
	float3 c_frag = find_closest_fragment_5tap(uv);
#else
	float3 c_frag = find_closest_fragment_3x3(uv);
#endif
	vs_dist = depth_resolve_linear(c_frag.z);
	uv = c_frag.xy;
#else
	vs_dist = depth_sample_linear(uv);
#endif
	float2 uvRay = (2.0f * uv.xy - 1.0f) * _ProjectionExtents.xy + _ProjectionExtents.zw;
	float3 vs_pos = float3(uvRay, 1.0) * vs_dist;
	float4 ws_pos = mul(unity_CameraToWorld, float4(vs_pos, 1.0f));
	float4 rp_cs_pos = mul(_PrevVP, ws_pos);
	float3 rp_ss_ndc = rp_cs_pos.xyz / rp_cs_pos.w;
	float3 rp_ss_txc = rp_ss_ndc * 0.5f + 0.5f;
	//rp_ss_txc.z = step(0, rp_ss_txc.x) * step(0, rp_ss_txc.y) * step(rp_ss_txc.x, 1) * step(rp_ss_txc.y, 1);
	//ss_vel = (uv - rp_ss_txc.xy) * rp_ss_txc.z;
	ss_vel = uv - rp_ss_txc.xy;
}

#if defined(_MIX_AA)
struct Color3X3
{
	float4 ctl;
	float4 ctc;
	float4 ctr;
	float4 cml;

	float4 cmc;

	float4 cmr;
	float4 cbl;
	float4 cbc;
	float4 cbr;

	float4 blur;
};
Color3X3 fxaa_for_taa(float4 color, inout float2 uv)
{
	Color3X3 color3x3;

	LuminanceData l;
	color3x3.cmc = color;
	l.m = GetLuminance(color);

	color3x3.cbc = SampleLuminance(uv, 0, 1, l.n);
	color3x3.blur = color3x3.cbc;

	color3x3.cmr = SampleLuminance(uv, 1, 0, l.e);
	color3x3.blur += color3x3.cmr;

	color3x3.ctc = SampleLuminance(uv, 0, -1, l.s);
	color3x3.blur += color3x3.ctc;

	color3x3.cml = SampleLuminance(uv, -1, 0, l.w);
	color3x3.blur += color3x3.cml;

	color3x3.cbl = SampleLuminance(uv, -1, 1, l.nw);
	color3x3.blur += color3x3.cbl;

	color3x3.cbr = SampleLuminance(uv, 1, 1, l.ne);
	color3x3.blur += color3x3.cbr;

	color3x3.ctr = SampleLuminance(uv, 1, -1, l.se);
	color3x3.blur += color3x3.ctr;

	color3x3.ctl = SampleLuminance(uv, -1, -1, l.sw);
	color3x3.blur += color3x3.ctl;

	l.highest = max(max(max(max(l.n, l.e), l.s), l.w), l.m);
	l.lowest = min(min(min(min(l.n, l.e), l.s), l.w), l.m);
	l.contrast = l.highest - l.lowest;
	UNITY_BRANCH
		if (ShouldSkipPixel(l))
		{
			color3x3.blur += color3x3.cmc;
			color3x3.blur = color3x3.blur * .1111111f;
			return color3x3;
		}

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
	color3x3.cmc = Sample(uv);
	color3x3.blur += color3x3.cmc;
	color3x3.blur = color3x3.blur * .1111111f;

	return color3x3;
}
#endif

/*              ---�ϰ汾---              */
float4 temporal_reprojection(sampler2D _MainTex, sampler2D _PrevTex, float2 ss_txc, float2 ss_vel, float vs_dist, float4 _JitterUV)
{
#if defined(UNJITTER_COLORSAMPLES)
	float2 uv = ss_txc - _JitterUV.xy;
#else
	float2 uv = ss_txc;
#endif
	float4 texel0 = sample_color(_MainTex, uv);

	float2 preUV = ss_txc - ss_vel;
	//#if UNITY_UV_STARTS_AT_TOP
	preUV.y = _UvAtTop == 1 ? (1 - preUV.y) : preUV.y;
	//#endif
#if defined(UNJITTER_COLORSAMPLES)
	//preUV = preUV - _JitterUV.zw;
#endif
	float4 texel1 = sample_color(_PrevTex, preUV);
	float4 cavg;
	float2 lastUV = uv;

#if defined(_MIX_AA)
	Color3X3 color3x3 = fxaa_for_taa(texel0, uv);
	texel0 = color3x3.cmc;
	cavg = color3x3.blur;
	float4 cmin = min(color3x3.ctl, min(color3x3.ctc, min(color3x3.ctr, min(color3x3.cml, min(color3x3.cmc, min(color3x3.cmr, min(color3x3.cbl, min(color3x3.cbc, color3x3.cbr))))))));
	float4 cmax = max(color3x3.ctl, max(color3x3.ctc, max(color3x3.ctr, max(color3x3.cml, max(color3x3.cmc, max(color3x3.cmr, max(color3x3.cbl, max(color3x3.cbc, color3x3.cbr))))))));
#else
	//use MINMAX_3X3
	float2 du = float2(_MainTex_TexelSize.x * _AABBSize, 0.0);
	float2 dv = float2(0.0, _MainTex_TexelSize.y * _AABBSize);

	float4 ctl = sample_color(_MainTex, uv - dv - du);
	float4 ctc = sample_color(_MainTex, uv - dv);
	float4 ctr = sample_color(_MainTex, uv - dv + du);
	float4 cml = sample_color(_MainTex, uv - du);
	float4 cmc = texel0;
	float4 cmr = sample_color(_MainTex, uv + du);
	float4 cbl = sample_color(_MainTex, uv + dv - du);
	float4 cbc = sample_color(_MainTex, uv + dv);
	float4 cbr = sample_color(_MainTex, uv + dv + du);

	float4 cmin = min(ctl, min(ctc, min(ctr, min(cml, min(cmc, min(cmr, min(cbl, min(cbc, cbr))))))));
	float4 cmax = max(ctl, max(ctc, max(ctr, max(cml, max(cmc, max(cmr, max(cbl, max(cbc, cbr))))))));
	cavg = (ctl + ctc + ctr + cml + cmc + cmr + cbl + cbc + cbr) / 9.0;
#endif

#if defined(_MIX_AA)
	texel0 = lerp(color3x3.blur, texel0, _Sharpness);
#endif
	
	float motionLength = length(ss_vel);
	// shrink chroma min-max
#if defined(USE_YCOCG)
	float2 chroma_extent = 0.25 * 0.5 * (cmax.r - cmin.r);
	float2 chroma_center = texel0.gb;
	cmin.yz = chroma_center - chroma_extent;
	cmax.yz = chroma_center + chroma_extent;
	cavg.yz = chroma_center;
#endif

	// feedback weight from unbiased luminance diff (t.lottes)
#if defined(USE_YCOCG)
	float lum0 = texel0.r;
	float lum1 = texel1.r;
	float lum2 = cavg.r;
#else
				//float lum0 = Luminance(texel0.rgb);
				//float lum1 = Luminance(texel1.rgb);
	float3 unity_ColorSpaceLuminance = float3(0.0396819152, 0.458021790, 0.00609653955);
	float lum0 = dot(texel0.rgb, unity_ColorSpaceLuminance);
	float lum1 = dot(texel1.rgb, unity_ColorSpaceLuminance);
	float lum2 = dot(cavg.rgb, unity_ColorSpaceLuminance);
#endif

#if defined(USE_ANTI_FLICKERING)
	//unity defaultRP's trick against flickering
	float nudge = lerp(4.0, 0.25, saturate(motionLength * 100.0)) * abs(lum0 - lum2);
	cmin -= nudge;
	cmax += nudge;
#endif

	float4 lastColor = texel1;
#if defined(USE_CLIPPING)
	texel1 = clip_aabb(cmin.xyz, cmax.xyz, clamp(cavg, cmin, cmax), texel1);
#else
	texel1 = clamp(texel1, cmin, cmax);
#endif

	//float unbiased_diff = abs(lum0 - lum1) / max(lum0, max(lum1, 0.2));
	//float unbiased_weight = 1.0 - unbiased_diff;
	//float unbiased_weight_sqr = unbiased_weight * unbiased_weight;

	//#define MOTION_AMPLIFICATION 6000
	//unbiased_weight_sqr = lerp(_FeedbackMax, _FeedbackMin, clamp(motionLength * MOTION_AMPLIFICATION * unbiased_weight_sqr, 0, 1));

	//unbiased_weight_sqr Խ�󣬵�ǰ֡�Ļ�ϱ�����Խ�󣬶�̬���á�

	//if (texel0.a <= 0.05f)
	{
		//motionLength = 1;
		//texel1 = lerp(cavg, texel0, 0.5f);
		//_FeedbackMin = 0.2f;
		//texel1 = lastColor == texel1 ? texel1 : texel0;
	}
	//else
	{
		//if (texel0.a > lastColor.a)
		{
			//texel1 = lerp(cavg, texel0, 0.5f);
			//texel1.a = texel0.a;
			//return float4(0, 0, 0, texel0.a);
			//motionLength = 1;
#define FEEDBACK_MIN_FALLBACK 0.3

			_FeedbackMin = saturate((vs_dist - _MotionFallOff.x) / (_MotionFallOff.y - _MotionFallOff.x)) * (_FeedbackMin - FEEDBACK_MIN_FALLBACK) + FEEDBACK_MIN_FALLBACK;

			//�����������Զ��ģ��Ч��
			//if (motionLength > 0.001f)
				//_FeedbackMin = FEEDBACK_MIN_FALLBACK;
		}
	}
#define FEEDBACK_SPEED 100
#define MOTION_SPEED 100
	//return  saturate((vs_dist - _MotionFallOff.x) / (_MotionFallOff.y - _MotionFallOff.x));
	float unbiased_weight_sqr = saturate(max(abs(lum2 - lum1) * FEEDBACK_SPEED, motionLength * MOTION_SPEED) * _FeedbackSpeed);

	float k_feedback = lerp(_FeedbackMax, _FeedbackMin, unbiased_weight_sqr);
	k_feedback = lastColor != texel1 ? 0 : k_feedback;
	//texel1.a = texel0.a;
	return saturate(lerp(texel0, texel1, k_feedback));
}


//����ȥ��ȥ�� motion velocity
/*
float4 temporal_reprojection(sampler2D _MainTex, sampler2D _PrevTex, float2 ss_txc, float2 ss_vel, float vs_dist, float4 _JitterUV)
{
#if defined(UNJITTER_COLORSAMPLES)
	float2 uv = ss_txc - _JitterUV.xy;
#else
	float2 uv = ss_txc;
#endif
	float4 texel0 = sample_color(_MainTex, uv);

	float2 preUV = ss_txc - ss_vel;
	//#if UNITY_UV_STARTS_AT_TOP
	preUV.y = _UvAtTop == 1 ? (1 - preUV.y) : preUV.y;
	//#endif
#if defined(UNJITTER_COLORSAMPLES)
	//preUV = preUV - _JitterUV.zw;
#endif
	float4 texel1 = sample_color(_PrevTex, preUV);

#if defined(MINMAX_3X3) || defined(MINMAX_3X3_ROUNDED)

	float2 du = float2(_MainTex_TexelSize.x * _AABBSize, 0.0);
	float2 dv = float2(0.0, _MainTex_TexelSize.y * _AABBSize);
#if defined(_MIX_AA)
	texel0 = fxaa_for_taa(texel0, uv);
#endif
	float4 ctl = sample_color(_MainTex, uv - dv - du);
	float4 ctc = sample_color(_MainTex, uv - dv);
	float4 ctr = sample_color(_MainTex, uv - dv + du);
	float4 cml = sample_color(_MainTex, uv - du);
	float4 cmc = texel0;
	float4 cmr = sample_color(_MainTex, uv + du);
	float4 cbl = sample_color(_MainTex, uv + dv - du);
	float4 cbc = sample_color(_MainTex, uv + dv);
	float4 cbr = sample_color(_MainTex, uv + dv + du);

	float4 cmin = min(ctl, min(ctc, min(ctr, min(cml, min(cmc, min(cmr, min(cbl, min(cbc, cbr))))))));
	float4 cmax = max(ctl, max(ctc, max(ctr, max(cml, max(cmc, max(cmr, max(cbl, max(cbc, cbr))))))));

	float4 cavg = (ctl + ctc + ctr + cml + cmc + cmr + cbl + cbc + cbr) / 9.0;

#if defined(MINMAX_3X3_ROUNDED)
	float4 cmin5 = min(ctc, min(cml, min(cmc, min(cmr, cbc))));
	float4 cmax5 = max(ctc, max(cml, max(cmc, max(cmr, cbc))));
	float4 cavg5 = (ctc + cml + cmc + cmr + cbc) / 5.0;
	cmin = 0.5 * (cmin + cmin5);
	cmax = 0.5 * (cmax + cmax5);
	cavg = 0.5 * (cavg + cavg5);
#endif

#else
	// this is the method used in v2 (PDTemporalReprojection2)
	//use MINMAX_4TAP_VARYING
	float motionLength = length(ss_vel);
	const float _SubpixelThreshold = 0.5;
	const float _GatherBase = 0.5;
	const float _GatherSubpixelMotion = 0.1666;

	float2 texel_vel = ss_vel / _MainTex_TexelSize.xy;
	float texel_vel_mag = motionLength * vs_dist;
	float k_subpixel_motion = saturate(_SubpixelThreshold / (FLTEPS + texel_vel_mag));
	float k_min_max_support = _GatherBase + _GatherSubpixelMotion * k_subpixel_motion;

	float2 ss_offset01 = k_min_max_support * float2(-_MainTex_TexelSize.x, _MainTex_TexelSize.y);
	float2 ss_offset11 = k_min_max_support * float2(_MainTex_TexelSize.x, _MainTex_TexelSize.y);
	float4 c00 = sample_color(_MainTex, uv - ss_offset11);
	float4 c10 = sample_color(_MainTex, uv - ss_offset01);
	float4 c01 = sample_color(_MainTex, uv + ss_offset01);
	float4 c11 = sample_color(_MainTex, uv + ss_offset11);

	float4 cmin = min(c00, min(c10, min(c01, c11)));
	float4 cmax = max(c00, max(c10, max(c01, c11)));

	//#if defined(USE_YCOCG) || defined(USE_CLIPPING)
	float4 cavg = (c00 + c10 + c01 + c11 + texel0) / 5.0;
	//#endif

#endif
	texel0 = lerp(cavg, texel0, _Sharpness);
	// shrink chroma min-max
#if defined(USE_YCOCG)
	float2 chroma_extent = 0.25 * 0.5 * (cmax.r - cmin.r);
	float2 chroma_center = texel0.gb;
	cmin.yz = chroma_center - chroma_extent;
	cmax.yz = chroma_center + chroma_extent;
	cavg.yz = chroma_center;
#endif

	// feedback weight from unbiased luminance diff (t.lottes)
#if defined(USE_YCOCG)
	float lum0 = texel0.r;
	float lum1 = texel1.r;
	float lum2 = cavg.r;
#else
				//float lum0 = Luminance(texel0.rgb);
				//float lum1 = Luminance(texel1.rgb);
	float3 unity_ColorSpaceLuminance = float3(0.0396819152, 0.458021790, 0.00609653955);
	float lum0 = dot(texel0.rgb, unity_ColorSpaceLuminance);
	float lum1 = dot(texel1.rgb, unity_ColorSpaceLuminance);
	float lum2 = dot(cavg.rgb, unity_ColorSpaceLuminance);
#endif

#if defined(USE_ANTI_FLICKERING)
	//unity defaultRP's trick against flickering
	float nudge = 4.0 * abs(lum0 - lum2);
	cmin -= nudge;
	cmax += nudge;
#endif

	float4 lastColor = texel1;
#if defined(USE_CLIPPING)
	texel1 = clip_aabb(cmin.xyz, cmax.xyz, clamp(cavg, cmin, cmax), texel1);
#else
	texel1 = clamp(texel1, cmin, cmax);
#endif

	//unbiased_weight_sqr Խ�󣬵�ǰ֡�Ļ�ϱ�����Խ�󣬶�̬���á�
#define FEEDBACK_SPEED 100
	float unbiased_weight_sqr = saturate(abs(lum2 - lum1) * _FeedbackSpeed * FEEDBACK_SPEED);
	float k_feedback = lerp(_FeedbackMax, _FeedbackMin, unbiased_weight_sqr);
	//k_feedback = lastColor == texel1 ? k_feedback : 0;
	return saturate(lerp(texel0, texel1, k_feedback));
}
*/

void TAA(float2 uv, out float4 to_screen, out float4 to_buffer)
{

	//to_screen = sample_color(_SourceTex, uv) * 0.1f + sample_color(_LastTex, float2(uv.x, 1 - uv.y)) * 0.9f;
	//to_buffer = to_screen;
	//return;
	float4 _JitterUV = _Parameter2 * _MainTex_TexelSize.xyxy;

	float2 ss_vel = 0;
	float vs_dist = 0;

	float2 uvUnjittered = uv - _JitterUV.xy;

	//���ײ�������Motion Velocity
	get_velocity(uvUnjittered, ss_vel, vs_dist);

	half4 tex1;
	float4 color_temporal = temporal_reprojection(_SourceTex, _LastTex, uv, ss_vel, vs_dist, _JitterUV);

	to_buffer = resolve_color(color_temporal);
#if defined(USE_MOTION_BLUR)
	float vel_mag = saturate(length(ss_vel * _MainTex_TexelSize.zw));
	float trust = clamp(saturate(vs_dist - _MotionFallOff.x / _MotionFallOff.y - _MotionFallOff.x), 0, vel_mag) * _MotionScale;
#if defined(UNJITTER_COLORSAMPLES)
	float4 color_motion = sample_color_motion(_SourceTex, uv - _JitterUV, ss_vel);
#else
	float4 color_motion = sample_color_motion(_SourceTex, uv, ss_vel);
#endif

#define MONTION_MASK 3.0

	//float2 uvScale = float2(1, _MainTex_TexelSize.w / _MainTex_TexelSize.z);
	//trust *= saturate(MONTION_MASK * _MotionMaskScale * length((uv - _MotionMask) * uvScale));
	to_screen = resolve_color(lerp(color_temporal, color_motion, trust));
#else
	to_screen = resolve_color(color_temporal);
#endif
	//to_buffer = to_screen;
	//if(ss_vel.y > 0.001f)
	//to_buffer = half4(ss_vel.x, ss_vel.y, 0, 0);
	//	to_buffer = half4(1, 0, 0, 0);
	//else
	//	to_buffer = half4(0, 1, 0, 0);
	//to_screen = to_buffer;
	//float4 noise4 = PDsrand4(uv + _SinTime.x + 0.6959174) / 510.0;
	//return to_screen;
	//float4 noise4 = PDsrand4(IN.ss_txc + _SinTime.x + 0.6959174) / 510.0;

	//OUT.buffer = saturate(to_buffer);
	//OUT.screen = saturate(to_screen );
	//return OUT;
}
