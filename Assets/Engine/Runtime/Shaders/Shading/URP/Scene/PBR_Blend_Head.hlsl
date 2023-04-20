#ifndef TERRAIN_HEAD_INCLUDE
#define TERRAIN_HEAD_INCLUDE

inline real3 UnpackNormal2(float2 packednormal, float normalScale)
{
	real3 normal = real3(packednormal, 1) * 2 - 1;
	normal = normalize(normal);
	normal.xy = packednormal.xy * 2 - 1;
	normal.xy *= normalScale;
	normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));

	return normal;
}



float3 Blend_RNM(float3 n0, float3 n1)
{
	float3 t = n0.xyz + float3(0.0, 0.0, 1.0);
	float3 u = n1.xyz * float3(-1.0, -1.0, 1.0);
	float3 r = (t / t.z) * dot(t, u) - u;
	return r;
}

float2 GetRotUV0(float2 uv)
{
	return real2(uv.x * _Rot01.x - uv.y * _Rot01.y, uv.x * _Rot01.y + uv.y * _Rot01.x);
}
float2 GetRotUV1(float2 uv)
{
	return real2(uv.x * _Rot01.z - uv.y * _Rot01.w, uv.x * _Rot01.w + uv.y * _Rot01.z);
}
float2 GetRotUV2(float2 uv)
{
	return real2(uv.x * _Rot23.x - uv.y * _Rot23.y, uv.x * _Rot23.y + uv.y * _Rot23.x);
}
float2 GetRotUV3(float2 uv)
{
	return float2(uv.x * _Rot23.z - uv.y * _Rot23.w, uv.x * _Rot23.w + uv.y * _Rot23.z);
}

// #define _TerrainNormalScale _Param1.w
float2 BlendFactor2(float2 height, float4 blend)
{
#ifdef _HEIGHT_BLEND
	float2 blendFactor;
	blendFactor.r = height.r * blend.r;
	blendFactor.g = height.g * blend.g;
	float maxValue = max(blendFactor.r, blendFactor.g);
	float transition = max(_BlendThreshold * maxValue, 0.0001);
	float threshold = maxValue - transition;
	float scale = rcp(transition);
	blendFactor = saturate((blendFactor - threshold) * scale);
	return blendFactor*rcp(blendFactor.r + blendFactor.g);
#else//!_HEIGHT_BLEND
	return blend.xy;
#endif//_HEIGHT_BLEND
}

float3 BlendFactor3(float3 height, float4 blend)
{
#ifdef _HEIGHT_BLEND
	float3 blendFactor;
	blendFactor.r = height.r * blend.r;
	blendFactor.g = height.g * blend.g;
	blendFactor.b = height.b * blend.b;
	float maxValue = max(blendFactor.r, max(blendFactor.g, blendFactor.b));
	float transition = max(_BlendThreshold * maxValue, 0.1);
          //transition = SG(transition, _BlendThreshold);

	float threshold = maxValue - transition;
	float scale = rcp(transition);
	blendFactor = saturate((blendFactor - threshold) * scale);
	return blendFactor*rcp(blendFactor.r + blendFactor.g + blendFactor.b);

#else//!_HEIGHT_BLEND
	return blend.xyz;
#endif//_HEIGHT_BLEND
}


float3 BlendFactor3New(float3 height, float4 blend)
{
#ifdef _HEIGHT_BLEND
	float3 blendFactor;
	blendFactor.r = height.r * blend.r;
	blendFactor.g = height.g * blend.g;
	blendFactor.b = height.b * blend.b;
	float maxValue = max(blendFactor.r, max(blendFactor.g, blendFactor.b));

    blendFactor =  max(blendFactor - maxValue.xxx + _BlendThreshold.xxx , 0) * blend.xyz;
 
    // old

	// float transition = max(_BlendThreshold * maxValue, 0.0001);
	// float threshold = maxValue - transition;
	// float scale = rcp(transition);
	// blendFactor = saturate((blendFactor - threshold) * scale);
	// return blendFactor*rcp(blendFactor.r + blendFactor.g + blendFactor.b);
	return blendFactor/(blendFactor.r + blendFactor.g + blendFactor.b);


#else//!_HEIGHT_BLEND
	return blend.xyz;
#endif//_HEIGHT_BLEND
}


float4 BlendFactor4(float4 height, float4 blend)
{
#ifdef _HEIGHT_BLEND
	float4 blendFactor;
	blendFactor.r = height.r * blend.r;
	blendFactor.g = height.g * blend.g;
	blendFactor.b = height.b * blend.b;
	blendFactor.a = height.a * blend.a;
	float maxValue = max(blendFactor.r, max(blendFactor.g, max(blendFactor.b, blendFactor.a)));
	
	blendFactor = max(blendFactor - maxValue + _BlendThreshold, 0) * blend;
	blendFactor = blendFactor/(blendFactor.r + blendFactor.g + blendFactor.b + blendFactor.a);
	return blendFactor;


#else//!_HEIGHT_BLEND
	return blend;
#endif//_HEIGHT_BLEND
}

float4 BlendColor2(float4 color0, float4 color1, 
	float4 blend, out float4 blendMask)
{
	float2 height = float2(color0.a, color1.a);
	float2 blendFactor = BlendFactor2(height, (pow(abs(blend),_BlendScale)));
	float4 color = color0 * blendFactor.r + color1 * blendFactor.g;
	
#ifdef _BLEND_NORMAL_MASK
	blendMask = float4(blendFactor,0,blend.w);
#else
	blendMask = float4(blendFactor,0,0);
#endif	
	return color;
}


float4 BlendColor3(float4 color0, float4 color1, float4 color2, 
	float4 blend, out float4 blendMask)
{
	float3 height = float3(color0.a, color1.a, color2.a);
	float3 blendFactor = BlendFactor3New(height, (pow(abs(blend),_BlendScale)));
	float4 color = color0 * blendFactor.r + color1 * blendFactor.g + color2 * blendFactor.b;
#ifdef _BLEND_NORMAL_MASK
	blendMask = float4(blendFactor,blend.w);
#else
	blendMask = float4(blendFactor,0);
#endif
	return color;
}

float4 BlendColor4(float4 color0, float4 color1, float4 color2, float4 color3, 
	float4 blend, out float4 blendMask)
{
	float4 height = float4(color0.a, color1.a, color2.a, color3.a);
	float4 blendFactor = BlendFactor4(height, pow(abs(blend),_BlendScale));
	float4 color = color0 * blendFactor.r + color1 * blendFactor.g + color2 * blendFactor.b + color3 * blendFactor.a;
	blendMask = blendFactor;
	return color;
}

// blend tangent space normal
float3 BlendPBS2(float4 pbs0, float4 pbs1,float normalScale, float4 blend, out float2 rm)
{
	float3 normalTS0 = UnpackNormal2(pbs0.xy,normalScale);
	float3 normalTS1 = UnpackNormal2(pbs1.xy,normalScale);
	rm = pbs0.zw*blend.x + pbs1.zw*blend.y;	
	float3 normalTS = normalTS0 * blend.x + normalTS1 * blend.y;
#ifdef _BLEND_NORMAL_MASK
	float3 normalRNM = Blend_RNM(normalTS0,normalTS1);
	normalTS = lerp(normalRNM,normalTS,blend.w);
#endif	
	return normalTS;
}


float3 BlendPBS3(float4 pbs0, float4 pbs1,float4 pbs2,float normalScale, float4 blend, out float2 rm)
{
	float3 normalTS0 = UnpackNormal2(pbs0.xy,normalScale);
	float3 normalTS1 = UnpackNormal2(pbs1.xy,normalScale);
	float3 normalTS2 = UnpackNormal2(pbs2.xy,normalScale);
	rm = pbs0.zw*blend.x + pbs1.zw*blend.y + pbs2.zw*blend.z;

	float3 normalTS12 = normalTS1 * (blend.y + 0.01) + normalTS2 * (blend.z + 0.01);
	float3 normalTS = normalTS0 * blend.x + normalTS12;
#ifdef _BLEND_NORMAL_MASK
	float3 normalRNM = Blend_RNM(normalTS0,normalTS12);
	normalTS = lerp(normalRNM,normalTS,blend.w);
#endif
	
	return normalTS;
}

// Layer4不支持 Blend normal mask
float3 BlendPBS4(float4 pbs0, float4 pbs1,float4 pbs2,float4 pbs3,float normalScale, float4 blend, out float2 rm)
{
	float3 normalTS0 = UnpackNormal2(pbs0.xy,normalScale);
	float3 normalTS1 = UnpackNormal2(pbs1.xy,normalScale);
	float3 normalTS2 = UnpackNormal2(pbs2.xy,normalScale);
	float3 normalTS3 = UnpackNormal2(pbs3.xy,normalScale);
	float3 normalTS = normalTS0 * blend.x + normalTS1 * (blend.y + 0.01) + normalTS2 * (blend.z + 0.01) + normalTS3 * blend.w;
	rm = rm = pbs0.zw * blend.x + pbs1.zw * blend.y + pbs2.zw * blend.z + pbs3.zw * blend.w;
	return normalTS;
}
// 获取水面高度，水面部分值为 [0,1] 区间，非水面部分值为 -1 //
// channelIndex 范围是 [0,3], 表示使用第1到第4张图 //
float GetWaterHeight(float4 height, float4 blend, int channelIndex)
{
	return 0;
	// 方式1 //
	// if(channelIndex < 0 || channelIndex > 3)
	// {
	// 	return -1;
	// }

	// blend = BlendFactor(height, blend);

	// float compareValue = -1;
	// compareValue = channelIndex == 0 ? blend.r : compareValue;
	// compareValue = channelIndex == 1 ? blend.g : compareValue;
	// compareValue = channelIndex == 2 ? blend.b : compareValue;
	// compareValue = channelIndex == 3 ? blend.a : compareValue;
	
	// float h = -1;
	// h = channelIndex == 0 ? height.r : h;
	// h = channelIndex == 1 ? height.g : h;
	// h = channelIndex == 2 ? height.b : h;
	// h = channelIndex == 3 ? height.a : h;

	// float maxValue = max(blend.r, max(blend.g, max(blend.b, blend.a)));
	// float waterHeight = maxValue == compareValue ? h : -1;
	// return waterHeight;
}

#endif				// TERRAIN_WATER_LIGHT_INCLUDE //