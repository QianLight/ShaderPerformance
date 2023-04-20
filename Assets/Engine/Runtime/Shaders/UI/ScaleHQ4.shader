﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UI/4xbr"
{
	Properties
	{
    	_Color("Color", Color) = (1,1,1,1) //You should set this field in Unity scripting, material.SetColor
	}
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		Pass
	{
		Tags{ "LightMode" = "UniversalForward" }
		CGPROGRAM
		#include "UnityCG.cginc"

		#pragma vertex main_vertex
		#pragma fragment main_fragment

		#define BLEND_NONE 0
		#define BLEND_NORMAL 1
		#define BLEND_DOMINANT 2
		#define LUMINANCE_WEIGHT 1.0
		#define EQUAL_COLOR_TOLERANCE 30.0/255.0
		#define STEEP_DIRECTION_THRESHOLD 2.2
		#define DOMINANT_DIRECTION_THRESHOLD 3.6
		static const float one_third = 1.0 /3.0;
		static const float two_third = 2.0 / 3.0;

		uniform half4 _Color;
		uniform sampler2D decal : TEXUNIT0;
		float2 texture_size; // set from outside the shader ( THIS NOTE WAS MADE BY SONOSHEE)
	
	struct out_vertex
	{
		float4 position : POSITION;
		float4 color : COLOR;
		float2 texCoord : TEXCOORD0;
		float4 t1       : TEXCOORD1;
		float4 t2       : TEXCOORD2;
		float4 t3       : TEXCOORD3;
		float4 t4       : TEXCOORD4;
		float4 t5       : TEXCOORD5;
		float4 t6       : TEXCOORD6;
		float4 t7       : TEXCOORD7;
	};

	out_vertex main_vertex(appdata_base v)
	{
		out_vertex OUT;

		OUT.position = UnityObjectToClipPos(v.vertex);
		OUT.color = _Color;

		float2 ps = float2(1.0 / texture_size.x, 1.0 / texture_size.y);
		float dx = ps.x;
		float dy = ps.y;

		// A1 B1 C1
		// A0 A B C C4
		// D0 D E F F4
		// G0 G H I I4
		// G5 H5 I5

		// This line fix a bug in ATI cards. ( THIS NOTE WAS MADE BY XBR AUTHOR)
		//float2 texCoord = texCoord1 + float2(0.0000001, 0.0000001);

		OUT.texCoord = v.texcoord;
		OUT.t1 = v.texcoord.xxxy + float4(-dx, 0, dx, -2.0*dy); // A1 B1 C1
		OUT.t2 = v.texcoord.xxxy + float4(-dx, 0, dx, -dy); // A B C
		OUT.t3 = v.texcoord.xxxy + float4(-dx, 0, dx, 0); // D E F
		OUT.t4 = v.texcoord.xxxy + float4(-dx, 0, dx, dy); // G H I
		OUT.t5 = v.texcoord.xxxy + float4(-dx, 0, dx, 2.0*dy); // G5 H5 I5
		OUT.t6 = v.texcoord.xyyy + float4(-2.0*dx, -dy, 0, dy); // A0 D0 G0
		OUT.t7 = v.texcoord.xyyy + float4(2.0*dx, -dy, 0, dy); // C4 F4 I4

		return OUT;
	}
	float reduce(const float3 color)
	{
		return dot(color, float3(65536.0, 256.0, 1.0));
	}

	float DistYCbCr(const float3 pixA, const float3 pixB)
	{
		const float3 w = float3(0.2627, 0.6780, 0.0593);
		const float scaleB = 0.5 / (1.0 - w.b);
		const float scaleR = 0.5 / (1.0 - w.r);
		float3 diff = pixA - pixB;
		float Y = dot(diff, w);
		float Cb = scaleB * (diff.b - Y);
		float Cr = scaleR * (diff.r - Y);

		return sqrt(((LUMINANCE_WEIGHT * Y) * (LUMINANCE_WEIGHT * Y)) + (Cb * Cb) + (Cr * Cr));
	}

	bool IsPixEqual(const float3 pixA, const float3 pixB)
	{
		return (DistYCbCr(pixA, pixB) < EQUAL_COLOR_TOLERANCE);
	}

	bool IsBlendingNeeded(const int4 blend)
	{
		return any(!(blend == int4(BLEND_NONE, BLEND_NONE, BLEND_NONE, BLEND_NONE)));
	}

	//---------------------------------------
	// Input Pixel Mapping:  --|21|22|23|--
	//                       19|06|07|08|09
	//                       18|05|00|01|10
	//                       17|04|03|02|11
	//                       --|15|14|13|--
	//
	// Output Pixel Mapping:  06|07|08|09
	//                        05|00|01|10
	//                        04|03|02|11
	//                        15|14|13|12


	//FRAGMENT SHADER
	half4 main_fragment(out_vertex VAR) : COLOR
	{

		float2 f = frac(VAR.texCoord*texture_size);

		//---------------------------------------
		// Input Pixel Mapping:  20|21|22|23|24
		//                       19|06|07|08|09
		//                       18|05|00|01|10
		//                       17|04|03|02|11
		//                       16|15|14|13|12

		float3 src[25];

	    src[21] = tex2D(decal, VAR.t1.xw).rgb;
		src[22] = tex2D(decal, VAR.t1.yw).rgb;
		src[23] = tex2D(decal, VAR.t1.zw).rgb;
		src[6] = tex2D(decal, VAR.t2.xw).rgb;
		src[7] = tex2D(decal, VAR.t2.yw).rgb;
		src[8] = tex2D(decal, VAR.t2.zw).rgb;
		src[5] = tex2D(decal, VAR.t3.xw).rgb;
		src[0] = tex2D(decal, VAR.t3.yw).rgb;
		src[1] = tex2D(decal, VAR.t3.zw).rgb;
		src[4] = tex2D(decal, VAR.t4.xw).rgb;
		src[3] = tex2D(decal, VAR.t4.yw).rgb;
		src[2] = tex2D(decal, VAR.t4.zw).rgb;
		src[15] = tex2D(decal, VAR.t5.xw).rgb;
		src[14] = tex2D(decal, VAR.t5.yw).rgb;
		src[13] = tex2D(decal, VAR.t5.zw).rgb;
		src[19] = tex2D(decal, VAR.t6.xy).rgb;
		src[18] = tex2D(decal, VAR.t6.xz).rgb;
		src[17] = tex2D(decal, VAR.t6.xw).rgb;
		src[9] = tex2D(decal, VAR.t7.xy).rgb;
		src[10] = tex2D(decal, VAR.t7.xz).rgb;
		src[11] = tex2D(decal, VAR.t7.xw).rgb;




		float v[9];
		v[0] = reduce(src[0]);
		v[1] = reduce(src[1]);
		v[2] = reduce(src[2]);
		v[3] = reduce(src[3]);
		v[4] = reduce(src[4]);
		v[5] = reduce(src[5]);
		v[6] = reduce(src[6]);
		v[7] = reduce(src[7]);
		v[8] = reduce(src[8]);

		int4 blendResult = int4(BLEND_NONE,BLEND_NONE,BLEND_NONE,BLEND_NONE);

		// Preprocess corners
		// Pixel Tap Mapping: --|--|--|--|--
		//                    --|--|07|08|--
		//                    --|05|00|01|10
		//                    --|04|03|02|11
		//                    --|--|14|13|--

		// Corner (1, 1)
		if (!((v[0] == v[1] && v[3] == v[2]) || (v[0] == v[3] && v[1] == v[2])))
		{
			float dist_03_01 = DistYCbCr(src[4], src[0]) + DistYCbCr(src[0], src[8]) + DistYCbCr(src[14], src[2]) + DistYCbCr(src[2], src[10]) + (4.0 * DistYCbCr(src[3], src[1]));
			float dist_00_02 = DistYCbCr(src[5], src[3]) + DistYCbCr(src[3], src[13]) + DistYCbCr(src[7], src[1]) + DistYCbCr(src[1], src[11]) + (4.0 * DistYCbCr(src[0], src[2]));
			bool dominantGradient = (DOMINANT_DIRECTION_THRESHOLD * dist_03_01) < dist_00_02;
			blendResult[2] = ((dist_03_01 < dist_00_02) && (v[0] != v[1]) && (v[0] != v[3])) ? ((dominantGradient) ? BLEND_DOMINANT : BLEND_NORMAL) : BLEND_NONE;
		}


		// Pixel Tap Mapping: --|--|--|--|--
		//                    --|06|07|--|--
		//                    18|05|00|01|--
		//                    17|04|03|02|--
		//                    --|15|14|--|--
		// Corner (0, 1)
		if (!((v[5] == v[0] && v[4] == v[3]) || (v[5] == v[4] && v[0] == v[3])))
		{
			float dist_04_00 = DistYCbCr(src[17], src[5]) + DistYCbCr(src[5], src[7]) + DistYCbCr(src[15], src[3]) + DistYCbCr(src[3], src[1]) + (4.0 * DistYCbCr(src[4], src[0]));
			float dist_05_03 = DistYCbCr(src[18], src[4]) + DistYCbCr(src[4], src[14]) + DistYCbCr(src[6], src[0]) + DistYCbCr(src[0], src[2]) + (4.0 * DistYCbCr(src[5], src[3]));
			bool dominantGradient = (DOMINANT_DIRECTION_THRESHOLD * dist_05_03) < dist_04_00;
			blendResult[3] = ((dist_04_00 > dist_05_03) && (v[0] != v[5]) && (v[0] != v[3])) ? ((dominantGradient) ? BLEND_DOMINANT : BLEND_NORMAL) : BLEND_NONE;
		}

		// Pixel Tap Mapping: --|--|22|23|--
		//                    --|06|07|08|09
		//                    --|05|00|01|10
		//                    --|--|03|02|--
		//                    --|--|--|--|--
		// Corner (1, 0)
		if (!((v[7] == v[8] && v[0] == v[1]) || (v[7] == v[0] && v[8] == v[1])))
		{
			float dist_00_08 = DistYCbCr(src[5], src[7]) + DistYCbCr(src[7], src[23]) + DistYCbCr(src[3], src[1]) + DistYCbCr(src[1], src[9]) + (4.0 * DistYCbCr(src[0], src[8]));
			float dist_07_01 = DistYCbCr(src[6], src[0]) + DistYCbCr(src[0], src[2]) + DistYCbCr(src[22], src[8]) + DistYCbCr(src[8], src[10]) + (4.0 * DistYCbCr(src[7], src[1]));
			bool dominantGradient = (DOMINANT_DIRECTION_THRESHOLD * dist_07_01) < dist_00_08;
			blendResult[1] = ((dist_00_08 > dist_07_01) && (v[0] != v[7]) && (v[0] != v[1])) ? ((dominantGradient) ? BLEND_DOMINANT : BLEND_NORMAL) : BLEND_NONE;
		}

		// Pixel Tap Mapping: --|21|22|--|--
		//                    19|06|07|08|--
		//                    18|05|00|01|--
		//                    --|04|03|--|--
		//                    --|--|--|--|--
		// Corner (0, 0)
		if (!((v[6] == v[7] && v[5] == v[0]) || (v[6] == v[5] && v[7] == v[0])))
		{
			float dist_05_07 = DistYCbCr(src[18], src[6]) + DistYCbCr(src[6], src[22]) + DistYCbCr(src[4], src[0]) + DistYCbCr(src[0], src[8]) + (4.0 * DistYCbCr(src[5], src[7]));
			float dist_06_00 = DistYCbCr(src[19], src[5]) + DistYCbCr(src[5], src[3]) + DistYCbCr(src[21], src[7]) + DistYCbCr(src[7], src[1]) + (4.0 * DistYCbCr(src[6], src[0]));
			bool dominantGradient = (DOMINANT_DIRECTION_THRESHOLD * dist_05_07) < dist_06_00;
			blendResult[0] = ((dist_05_07 < dist_06_00) && (v[0] != v[5]) && (v[0] != v[7])) ? ((dominantGradient) ? BLEND_DOMINANT : BLEND_NORMAL) : BLEND_NONE;
		}

		float3 dst[16];
		dst[0] = src[0];
		dst[1] = src[0];
		dst[2] = src[0];
		dst[3] = src[0];
		dst[4] = src[0];
		dst[5] = src[0];
		dst[6] = src[0];
		dst[7] = src[0];
		dst[8] = src[0];
		dst[9] = src[0];
		dst[10] = src[0];
		dst[11] = src[0];
		dst[12] = src[0];
		dst[13] = src[0];
		dst[14] = src[0];
		dst[15] = src[0];

		// Scale pixel
		if (IsBlendingNeeded(blendResult))
		{
			float dist_01_04 = DistYCbCr(src[1], src[4]);
			float dist_03_08 = DistYCbCr(src[3], src[8]);
			bool haveShallowLine = (STEEP_DIRECTION_THRESHOLD * dist_01_04 <= dist_03_08) && (v[0] != v[4]) && (v[5] != v[4]);
			bool haveSteepLine = (STEEP_DIRECTION_THRESHOLD * dist_03_08 <= dist_01_04) && (v[0] != v[8]) && (v[7] != v[8]);
			bool needBlend = (blendResult[2] != BLEND_NONE);
			bool doLineBlend = (blendResult[2] >= BLEND_DOMINANT ||
				!((blendResult[1] != BLEND_NONE && !IsPixEqual(src[0], src[4])) ||
				(blendResult[3] != BLEND_NONE && !IsPixEqual(src[0], src[8])) ||
					(IsPixEqual(src[4], src[3]) && IsPixEqual(src[3], src[2]) && IsPixEqual(src[2], src[1]) && IsPixEqual(src[1], src[8]) && !IsPixEqual(src[0], src[2]))));

			float3 blendPix = (DistYCbCr(src[0], src[1]) <= DistYCbCr(src[0], src[3])) ? src[1] : src[3];
			dst[2] = lerp(dst[2], blendPix, (needBlend && doLineBlend) ? ((haveShallowLine) ? ((haveSteepLine) ? 1.0 / 3.0 : 0.25) : ((haveSteepLine) ? 0.25 : 0.00)) : 0.00);
			dst[9] = lerp(dst[9], blendPix, (needBlend && doLineBlend && haveSteepLine) ? 0.25 : 0.00);
			dst[10] = lerp(dst[10], blendPix, (needBlend && doLineBlend && haveSteepLine) ? 0.75 : 0.00);
			dst[11] = lerp(dst[11], blendPix, (needBlend) ? ((doLineBlend) ? ((haveSteepLine) ? 1.00 : ((haveShallowLine) ? 0.75 : 0.50)) : 0.08677704501) : 0.00);
			dst[12] = lerp(dst[12], blendPix, (needBlend) ? ((doLineBlend) ? 1.00 : 0.6848532563) : 0.00);
			dst[13] = lerp(dst[13], blendPix, (needBlend) ? ((doLineBlend) ? ((haveShallowLine) ? 1.00 : ((haveSteepLine) ? 0.75 : 0.50)) : 0.08677704501) : 0.00);
			dst[14] = lerp(dst[14], blendPix, (needBlend && doLineBlend && haveShallowLine) ? 0.75 : 0.00);
			dst[15] = lerp(dst[15], blendPix, (needBlend && doLineBlend && haveShallowLine) ? 0.25 : 0.00);


			dist_01_04 = DistYCbCr(src[7], src[2]);
			dist_03_08 = DistYCbCr(src[1], src[6]);
			haveShallowLine = (STEEP_DIRECTION_THRESHOLD * dist_01_04 <= dist_03_08) && (v[0] != v[2]) && (v[3] != v[2]);
			haveSteepLine = (STEEP_DIRECTION_THRESHOLD * dist_03_08 <= dist_01_04) && (v[0] != v[6]) && (v[5] != v[6]);
			needBlend = (blendResult[1] != BLEND_NONE);
			doLineBlend = (blendResult[1] >= BLEND_DOMINANT ||
				!((blendResult[0] != BLEND_NONE && !IsPixEqual(src[0], src[2])) ||
				(blendResult[2] != BLEND_NONE && !IsPixEqual(src[0], src[6])) ||
					(IsPixEqual(src[2], src[1]) && IsPixEqual(src[1], src[8]) && IsPixEqual(src[8], src[7]) && IsPixEqual(src[7], src[6]) && !IsPixEqual(src[0], src[8]))));

			blendPix = (DistYCbCr(src[0], src[7]) <= DistYCbCr(src[0], src[1])) ? src[7] : src[1];
			dst[1] = lerp(dst[1], blendPix, (needBlend && doLineBlend) ? ((haveShallowLine) ? ((haveSteepLine) ? 1.0 / 3.0 : 0.25) : ((haveSteepLine) ? 0.25 : 0.00)) : 0.00);
			dst[6] = lerp(dst[6], blendPix, (needBlend && doLineBlend && haveSteepLine) ? 0.25 : 0.00);
			dst[7] = lerp(dst[7], blendPix, (needBlend && doLineBlend && haveSteepLine) ? 0.75 : 0.00);
			dst[8] = lerp(dst[8], blendPix, (needBlend) ? ((doLineBlend) ? ((haveSteepLine) ? 1.00 : ((haveShallowLine) ? 0.75 : 0.50)) : 0.08677704501) : 0.00);
			dst[9] = lerp(dst[9], blendPix, (needBlend) ? ((doLineBlend) ? 1.00 : 0.6848532563) : 0.00);
			dst[10] = lerp(dst[10], blendPix, (needBlend) ? ((doLineBlend) ? ((haveShallowLine) ? 1.00 : ((haveSteepLine) ? 0.75 : 0.50)) : 0.08677704501) : 0.00);
			dst[11] = lerp(dst[11], blendPix, (needBlend && doLineBlend && haveShallowLine) ? 0.75 : 0.00);
			dst[12] = lerp(dst[12], blendPix, (needBlend && doLineBlend && haveShallowLine) ? 0.25 : 0.00);


			dist_01_04 = DistYCbCr(src[5], src[8]);
			dist_03_08 = DistYCbCr(src[7], src[4]);
			haveShallowLine = (STEEP_DIRECTION_THRESHOLD * dist_01_04 <= dist_03_08) && (v[0] != v[8]) && (v[1] != v[8]);
			haveSteepLine = (STEEP_DIRECTION_THRESHOLD * dist_03_08 <= dist_01_04) && (v[0] != v[4]) && (v[3] != v[4]);
			needBlend = (blendResult[0] != BLEND_NONE);
			doLineBlend = (blendResult[0] >= BLEND_DOMINANT ||
				!((blendResult[3] != BLEND_NONE && !IsPixEqual(src[0], src[8])) ||
				(blendResult[1] != BLEND_NONE && !IsPixEqual(src[0], src[4])) ||
					(IsPixEqual(src[8], src[7]) && IsPixEqual(src[7], src[6]) && IsPixEqual(src[6], src[5]) && IsPixEqual(src[5], src[4]) && !IsPixEqual(src[0], src[6]))));

			blendPix = (DistYCbCr(src[0], src[5]) <= DistYCbCr(src[0], src[7])) ? src[5] : src[7];
			dst[0] = lerp(dst[0], blendPix, (needBlend && doLineBlend) ? ((haveShallowLine) ? ((haveSteepLine) ? 1.0 / 3.0 : 0.25) : ((haveSteepLine) ? 0.25 : 0.00)) : 0.00);
			dst[15] = lerp(dst[15], blendPix, (needBlend && doLineBlend && haveSteepLine) ? 0.25 : 0.00);
			dst[4] = lerp(dst[4], blendPix, (needBlend && doLineBlend && haveSteepLine) ? 0.75 : 0.00);
			dst[5] = lerp(dst[5], blendPix, (needBlend) ? ((doLineBlend) ? ((haveSteepLine) ? 1.00 : ((haveShallowLine) ? 0.75 : 0.50)) : 0.08677704501) : 0.00);
			dst[6] = lerp(dst[6], blendPix, (needBlend) ? ((doLineBlend) ? 1.00 : 0.6848532563) : 0.00);
			dst[7] = lerp(dst[7], blendPix, (needBlend) ? ((doLineBlend) ? ((haveShallowLine) ? 1.00 : ((haveSteepLine) ? 0.75 : 0.50)) : 0.08677704501) : 0.00);
			dst[8] = lerp(dst[8], blendPix, (needBlend && doLineBlend && haveShallowLine) ? 0.75 : 0.00);
			dst[9] = lerp(dst[9], blendPix, (needBlend && doLineBlend && haveShallowLine) ? 0.25 : 0.00);


			dist_01_04 = DistYCbCr(src[3], src[6]);
			dist_03_08 = DistYCbCr(src[5], src[2]);
			haveShallowLine = (STEEP_DIRECTION_THRESHOLD * dist_01_04 <= dist_03_08) && (v[0] != v[6]) && (v[7] != v[6]);
			haveSteepLine = (STEEP_DIRECTION_THRESHOLD * dist_03_08 <= dist_01_04) && (v[0] != v[2]) && (v[1] != v[2]);
			needBlend = (blendResult[3] != BLEND_NONE);
			doLineBlend = (blendResult[3] >= BLEND_DOMINANT ||
				!((blendResult[2] != BLEND_NONE && !IsPixEqual(src[0], src[6])) ||
				(blendResult[0] != BLEND_NONE && !IsPixEqual(src[0], src[2])) ||
					(IsPixEqual(src[6], src[5]) && IsPixEqual(src[5], src[4]) && IsPixEqual(src[4], src[3]) && IsPixEqual(src[3], src[2]) && !IsPixEqual(src[0], src[4]))));

			blendPix = (DistYCbCr(src[0], src[3]) <= DistYCbCr(src[0], src[5])) ? src[3] : src[5];
			dst[3] = lerp(dst[3], blendPix, (needBlend && doLineBlend) ? ((haveShallowLine) ? ((haveSteepLine) ? 1.0 / 3.0 : 0.25) : ((haveSteepLine) ? 0.25 : 0.00)) : 0.00);
			dst[12] = lerp(dst[12], blendPix, (needBlend && doLineBlend && haveSteepLine) ? 0.25 : 0.00);
			dst[13] = lerp(dst[13], blendPix, (needBlend && doLineBlend && haveSteepLine) ? 0.75 : 0.00);
			dst[14] = lerp(dst[14], blendPix, (needBlend) ? ((doLineBlend) ? ((haveSteepLine) ? 1.00 : ((haveShallowLine) ? 0.75 : 0.50)) : 0.08677704501) : 0.00);
			dst[15] = lerp(dst[15], blendPix, (needBlend) ? ((doLineBlend) ? 1.00 : 0.6848532563) : 0.00);
			dst[4] = lerp(dst[4], blendPix, (needBlend) ? ((doLineBlend) ? ((haveShallowLine) ? 1.00 : ((haveSteepLine) ? 0.75 : 0.50)) : 0.08677704501) : 0.00);
			dst[5] = lerp(dst[5], blendPix, (needBlend && doLineBlend && haveShallowLine) ? 0.75 : 0.00);
			dst[6] = lerp(dst[6], blendPix, (needBlend && doLineBlend && haveShallowLine) ? 0.25 : 0.00);
		}

		float3 dst67=lerp(dst[6], dst[7], step(0.25, f.x));
		float3 dst89=lerp(dst[8], dst[9], step(0.75, f.x));
		float3 dst6789=lerp(dst67, dst89, step(0.50, f.x));

		float3 dst50=lerp(dst[5], dst[0], step(0.25, f.x));
		float3 dst110=lerp(dst[1], dst[10], step(0.75, f.x));
		float3 dst50110=lerp(dst50, dst110, step(0.50, f.x));

		float3 dst43=lerp(dst[4], dst[3], step(0.25, f.x));
		float3 dst211=lerp(dst[2], dst[11], step(0.75, f.x));
		float3 dst43211=lerp(dst43, dst211, step(0.50, f.x));

		float3 dst1514=lerp(dst[15], dst[14], step(0.25, f.x));
		float3 dst1312= lerp(dst[13], dst[12], step(0.75, f.x));
		float3 dst15141312=lerp(dst1514, dst1312, step(0.50, f.x));



		float3 res = lerp(lerp(dst6789,dst50110, step(0.25, f.y)),lerp(dst43211,dst15141312, step(0.75, f.y)),step(0.50, f.y));


		return float4(res, 1.0) * _Color;
	}
		ENDCG
	}
		//Pass
		//{
		//	Name "OverdrawF"
		//	Tags{"LightMode" = "OverdrawForwardBase"}

		//	Blend One One
		//	CGPROGRAM


		//	#pragma vertex Vert
		//	#pragma fragment Frag

		//	#include "UnityCG.cginc"

		//	struct Attributes
		//	{
		//		float4 vertex : POSITION;
		//	};
			
		//	struct Varyings
		//	{
		//		float4 vertex : SV_POSITION;
		//	};
		//	Varyings Vert(Attributes v)
		//	{
		//		Varyings o;
		//		float4 WorldPosition = mul(unity_ObjectToWorld, v.vertex);
		//		o.vertex = mul(unity_MatrixVP, WorldPosition);
		//		return o;
		//	}

		//	half4 Frag(Varyings i) : SV_Target
		//	{
		//		return half4(0.1, 0.04, 0.02, 1);
		//	}

		//	ENDCG
		//}
	}
    Fallback "Hidden/Universal Render Pipeline/FallbackError"
}