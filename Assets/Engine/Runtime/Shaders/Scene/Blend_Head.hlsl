#ifndef TERRAIN_HEAD_INCLUDE
#define TERRAIN_HEAD_INCLUDE

#include "../Include/Common.hlsl"
REAL4 _Rot01;
REAL4 _Rot23;
REAL2 GetRotUV0(REAL2 uv)
{
	return REAL2(uv.x * _Rot01.x - uv.y * _Rot01.y, uv.x * _Rot01.y + uv.y * _Rot01.x);
}
REAL2 GetRotUV1(REAL2 uv)
{
	return REAL2(uv.x * _Rot01.z - uv.y * _Rot01.w, uv.x * _Rot01.w + uv.y * _Rot01.z);
}
REAL2 GetRotUV2(REAL2 uv)
{
	return REAL2(uv.x * _Rot23.x - uv.y * _Rot23.y, uv.x * _Rot23.y + uv.y * _Rot23.x);
}
REAL2 GetRotUV3(REAL2 uv)
{
	return REAL2(uv.x * _Rot23.z - uv.y * _Rot23.w, uv.x * _Rot23.w + uv.y * _Rot23.z);
}
// #define _TerrainNormalScale _Param1.w
FLOAT2 BlendFactor2(FLOAT2 height, FLOAT4 blend)
{
#ifdef _HEIGHT_BLEND
	FLOAT2 blendFactor;
	blendFactor.r = height.r * blend.r;
	blendFactor.g = height.g * blend.g;
	FLOAT maxValue = max(blendFactor.r, blendFactor.g);
	FLOAT transition = max(_BlendThreshold * maxValue, 0.0001);
	FLOAT threshold = maxValue - transition;
	FLOAT scale = rcp(transition);
	blendFactor = saturate((blendFactor - threshold) * scale);
	return blendFactor*rcp(blendFactor.r + blendFactor.g);
#else//!_HEIGHT_BLEND
	return blend.xy;
#endif//_HEIGHT_BLEND
}

FLOAT3 BlendFactor3(FLOAT3 height, FLOAT4 blend)
{
#ifdef _HEIGHT_BLEND
	FLOAT3 blendFactor;
	blendFactor.r = height.r * blend.r;
	blendFactor.g = height.g * blend.g;
	blendFactor.b = height.b * blend.b;
	FLOAT maxValue = max(blendFactor.r, max(blendFactor.g, blendFactor.b));
	FLOAT transition = max(_BlendThreshold * maxValue, 0.1);
          //transition = SG(transition, _BlendThreshold);

	FLOAT threshold = maxValue - transition;
	FLOAT scale = rcp(transition);
	blendFactor = saturate((blendFactor - threshold) * scale);
	return blendFactor*rcp(blendFactor.r + blendFactor.g + blendFactor.b);

#else//!_HEIGHT_BLEND
	return blend.xyz;
#endif//_HEIGHT_BLEND
}


FLOAT3 BlendFactor3New(FLOAT3 height, FLOAT4 blend)
{
#ifdef _HEIGHT_BLEND
	FLOAT3 blendFactor;
	blendFactor.r = height.r * blend.r;
	blendFactor.g = height.g * blend.g;
	blendFactor.b = height.b * blend.b;
	FLOAT maxValue = max(blendFactor.r, max(blendFactor.g, blendFactor.b));

    blendFactor =  max(blendFactor - maxValue.xxx + _BlendThreshold.xxx , 0) * blend.xyz;
 
    // old

	// FLOAT transition = max(_BlendThreshold * maxValue, 0.0001);
	// FLOAT threshold = maxValue - transition;
	// FLOAT scale = rcp(transition);
	// blendFactor = saturate((blendFactor - threshold) * scale);
	// return blendFactor*rcp(blendFactor.r + blendFactor.g + blendFactor.b);
	return blendFactor/(blendFactor.r + blendFactor.g + blendFactor.b);


#else//!_HEIGHT_BLEND
	return blend.xyz;
#endif//_HEIGHT_BLEND
}


FLOAT4 BlendFactor4(FLOAT4 height, FLOAT4 blend)
{
#ifdef _HEIGHT_BLEND
	FLOAT4 blendFactor;
	blendFactor.r = height.r * blend.r;
	blendFactor.g = height.g * blend.g;
	blendFactor.b = height.b * blend.b;
	blendFactor.a = height.a * blend.a;
	FLOAT maxValue = max(blendFactor.r, max(blendFactor.g, max(blendFactor.b, blendFactor.a)));
	blendFactor = max(blendFactor - maxValue + _BlendThreshold, 0) * blend;
	return blendFactor*rcp(blendFactor.r + blendFactor.g + blendFactor.b + blendFactor.a);

#else//!_HEIGHT_BLEND
	return blend;
#endif//_HEIGHT_BLEND
}

#define _Blendscale _Param5.y
FLOAT4 BlendColor2(FLOAT4 color0, FLOAT4 color1, 
	FLOAT4 blend, out FLOAT4 blendMask)
{
	FLOAT2 height = FLOAT2(color0.a, color1.a);
	FLOAT2 blendFactor = BlendFactor2(height, (pow(abs(blend),_Blendscale)));
	FLOAT4 color = color0 * blendFactor.r + color1 * blendFactor.g;
	
#ifdef _BLEND_NORMAL_MASK
	blendMask = FLOAT4(blendFactor,0,blend.w);
#else
	blendMask = FLOAT4(blendFactor,0,0);
#endif	
	return color;
}


FLOAT4 BlendColor3(FLOAT4 color0, FLOAT4 color1, FLOAT4 color2, 
	FLOAT4 blend, out FLOAT4 blendMask)
{
	FLOAT3 height = FLOAT3(color0.a, color1.a, color2.a);
	FLOAT3 blendFactor = BlendFactor3New(height, (pow(abs(blend),_Blendscale)));
	FLOAT4 color = color0 * blendFactor.r + color1 * blendFactor.g + 
	color2 * blendFactor.b;
#ifdef _BLEND_NORMAL_MASK
	blendMask = FLOAT4(blendFactor,blend.w);
#else
	blendMask = FLOAT4(blendFactor,0);
#endif	
	return color;
}

FLOAT4 BlendColor4(FLOAT4 color0, FLOAT4 color1, FLOAT4 color2, FLOAT4 color3, 
	FLOAT4 blend, out FLOAT4 blendMask)
{
	FLOAT4 height = FLOAT4(color0.a, color1.a, color2.a, color3.a);
	FLOAT4 blendFactor = BlendFactor4(height, blend);
	FLOAT4 color = color0 * blendFactor.r + color1 * blendFactor.g + 
	color2 * blendFactor.b + color3 * blendFactor.a;
	blendMask = blendFactor;
	return color;
}

// blend tangent space normal
FLOAT3 BlendPBS2(FLOAT4 pbs0, FLOAT4 pbs1,FLOAT normalScale, FLOAT4 blend, out FLOAT2 rm)
{
	FLOAT3 normalTS0 = UnpackNormal(pbs0.xy,normalScale);
	FLOAT3 normalTS1 = UnpackNormal(pbs1.xy,normalScale);
	rm = pbs0.zw*blend.x + pbs1.zw*blend.y;	
	FLOAT3 normalTS = normalTS0 * blend.x + normalTS1 * blend.y;
#ifdef _BLEND_NORMAL_MASK
	FLOAT3 normalRNM = Blend_RNM(normalTS0,normalTS1);
	normalTS = lerp(normalRNM,normalTS,blend.w);
#endif	
	return normalTS;
}

FLOAT3 BlendPBS3(FLOAT4 pbs0, FLOAT4 pbs1,FLOAT4 pbs2,FLOAT normalScale, FLOAT4 blend, out FLOAT2 rm)
{
	FLOAT3 normalTS0 = UnpackNormal(pbs0.xy,normalScale);
	FLOAT3 normalTS1 = UnpackNormal(pbs1.xy,normalScale);
	FLOAT3 normalTS2 = UnpackNormal(pbs2.xy,normalScale);	
	rm = pbs0.zw*blend.x + pbs1.zw*blend.y + pbs2.zw*blend.z;

	FLOAT3 normalTS12 = normalTS1 * (blend.y + 0.01) + normalTS2 * (blend.z + 0.01);
	FLOAT3 normalTS = normalTS0 * blend.x + normalTS12;
#ifdef _BLEND_NORMAL_MASK
	FLOAT3 normalRNM = Blend_RNM(normalTS0,normalTS12);
	normalTS = lerp(normalRNM,normalTS,blend.w);
#endif
	
	return normalTS;
}

FLOAT3 BlendPBS4(FLOAT4 pbs0, FLOAT4 pbs1,FLOAT4 pbs2,FLOAT4 pbs3,FLOAT normalScale, FLOAT4 blend, out FLOAT2 rm)
{
	FLOAT3 normalTS0 = UnpackNormal(pbs0.xy,normalScale);
	FLOAT3 normalTS1 = UnpackNormal(pbs1.xy,normalScale);
	FLOAT3 normalTS2 = UnpackNormal(pbs2.xy,normalScale);
	FLOAT3 normalTS3 = UnpackNormal(pbs3.xy,normalScale);
	FLOAT3 normalTS = normalTS0 * blend.x + normalTS1 * blend.y + 
					normalTS2 * blend.z + normalTS3 * blend.w;
	rm = pbs0.zw*blend.x + pbs1.zw*blend.y + pbs2.zw*blend.z+ pbs3.zw*blend.w;
	return normalTS;
}
// 获取水面高度，水面部分值为 [0,1] 区间，非水面部分值为 -1 //
// channelIndex 范围是 [0,3], 表示使用第1到第4张图 //
FLOAT GetWaterHeight(FLOAT4 height, FLOAT4 blend, int channelIndex)
{
	return 0;
	// 方式1 //
	// if(channelIndex < 0 || channelIndex > 3)
	// {
	// 	return -1;
	// }

	// blend = BlendFactor(height, blend);

	// FLOAT compareValue = -1;
	// compareValue = channelIndex == 0 ? blend.r : compareValue;
	// compareValue = channelIndex == 1 ? blend.g : compareValue;
	// compareValue = channelIndex == 2 ? blend.b : compareValue;
	// compareValue = channelIndex == 3 ? blend.a : compareValue;
	
	// FLOAT h = -1;
	// h = channelIndex == 0 ? height.r : h;
	// h = channelIndex == 1 ? height.g : h;
	// h = channelIndex == 2 ? height.b : h;
	// h = channelIndex == 3 ? height.a : h;

	// FLOAT maxValue = max(blend.r, max(blend.g, max(blend.b, blend.a)));
	// FLOAT waterHeight = maxValue == compareValue ? h : -1;
	// return waterHeight;
}

// 获取水面高度, 1: 不是水面, [0, 1) 区间: 是水面 //
void TerrainWaterHeight(FLOAT4 blend, inout FMaterialData MaterialData)
{
#if defined(_TERRAIN_WATER)
	MaterialData.WaterHeight = 1-blend.a;
#endif
}


// ==================== 以下是只有定义了 _TERRAIN_PBS 宏才能使用的方法 ==================== //
#if defined(_TERRAIN_PBS)
// 地形的pbs图对象声明 //
TEX2D_SAMPLER(_TerrainPBSTex0);
TEX2D_SAMPLER(_TerrainPBSTex1);
TEX2D_SAMPLER(_TerrainPBSTex2);
TEX2D_SAMPLER(_TerrainPBSTex3);
#endif

// ==================== 以下是只有定义了 _TERRAIN_WATER 宏才能使用的方法 ==================== //
#if defined(_TERRAIN_WATER)

// 为了 _Skybox 对象的声明 //
#define _WATER_LIGHT

#include "WaterLighting.hlsl"

// 地形上的水面相关参数 //
FLOAT _WaterHeight;
FLOAT _WetToGroundTrans;			// 潮湿地面到正常地面的过渡, [0,1]区间 //
FLOAT _WaterToWetTrans;				// 水面到潮湿地面的过渡, [0,1]区间 //
FLOAT4 _WaterColor;					// 水的颜色, RGBA //
FLOAT _FresnelPow;					// 菲涅尔强度 //
FLOAT _SkyboxIntensity;				// 天空盒反射强度 //
FLOAT _WaterRoughness;				// 水的粗糙度 //
// FLOAT _WaterNormalDisturbance;   	// 水表面法线的扰动 //
FLOAT _WetGroundDarkPercent;		// 潮湿地面的颜色变暗程度, [0,1]区间 //

// 处理 MaterialData //
void TerrainMaterialData(inout FMaterialData MaterialData)
{
	// FLOAT rnd1 = (rand(_Time.xz) - 0.5) * 2 * _WaterNormalDisturbance;
	// FLOAT rnd2 = (rand(_Time.yw) - 0.5) * 2 * _WaterNormalDisturbance;
	// FLOAT3 waterNormal = normalize(FLOAT3(rnd1, 1, rnd2));

	FLOAT3 waterNormal = FLOAT3(0,1,0);

	// 法线和粗糙度插值 //
	// 实际的水面区域 lerpValue = 0，过渡部分 lerpValue 在(0,1)之间， 非实际水面部分 lerpValue = 1 //
	FLOAT lerpValue = smoothstep(_WaterHeight - _WetToGroundTrans, _WaterHeight, MaterialData.WaterHeight);

	if(lerpValue == 0)		// 水区域 //
	{
		// 水面和过渡区域的界面分界明显是因为法线没有过渡，直接设置成了地面的法线 //
		// 法线过渡: 水面法线 到 地面法线 //
		FLOAT waterToWetLerp = smoothstep(_WaterHeight-_WetToGroundTrans*(1+_WaterToWetTrans), _WaterHeight-_WetToGroundTrans, MaterialData.WaterHeight);
		waterNormal = lerp(waterNormal, MaterialData.WorldNormal, waterToWetLerp);

		MaterialData.WorldNormal = waterNormal;
		MaterialData.Roughness = _WaterRoughness;
	}
	else if(lerpValue < 1)	// 水到地面过渡区域 //
	{
		// MaterialData.WorldNormal = MaterialData.WorldNormal;
		MaterialData.Roughness = lerp(_WaterRoughness, MaterialData.Roughness, lerpValue);
	}
	else {}				// 地面区域 //
}

// 处理地形水的光照 //
void TerrainWaterLighting(FFragData FragData, FMaterialData MaterialData, inout FLOAT3 DirectDiffuse, inout FLOAT3 DirectSpecular)
{
	// 实际的水面区域 lerpValue = 0，过渡区域 lerpValue 在(0,1)之间， 地面区域 lerpValue = 1 //
	FLOAT lerpValue = smoothstep(_WaterHeight - _WetToGroundTrans, _WaterHeight, MaterialData.WaterHeight);

	if(lerpValue == 0)			// 水区域 //
	{
		// 离水面距离越大alpha值越大，即越不透明 //
		FLOAT alpha = _WaterColor.a;
		alpha = alpha + 0.5*alpha*(max(0, _WaterHeight-MaterialData.WaterHeight)/_WaterHeight);
		alpha = saturate(alpha);
		FLOAT3 waterCol = alpha * _WaterColor + (1 - alpha) * DirectDiffuse;

		// reflect //
		FLOAT3 worldNormal = MaterialData.WorldNormal;
		worldNormal = normalize(worldNormal);

		FLOAT3 reflectDir = reflect(-normalize(FragData.CameraVector), worldNormal);
		FLOAT3 reflectCol = SAMPLE_TEXCUBE(_Skybox, reflectDir).rgb;
		
		// fresnel //
		FLOAT3 worldView = normalize(FragData.CameraVector);
		FLOAT fresnel = pow(1 - max(0, dot(worldNormal, worldView)), _FresnelPow);
		FLOAT3 waterDiffCol = (1-fresnel)*waterCol + fresnel*(reflectCol*_SkyboxIntensity);

		FLOAT waterToWetLerp = smoothstep(_WaterHeight-_WetToGroundTrans*(1+_WaterToWetTrans), _WaterHeight-_WetToGroundTrans, MaterialData.WaterHeight);
		waterDiffCol = lerp(waterDiffCol, DirectDiffuse*_WetGroundDarkPercent, waterToWetLerp);

		DirectDiffuse = waterDiffCol;

		// DirectSpecular = DirectSpecular * _SpecIntensity;
	}
	else if(lerpValue < 1)		// 水到地面过渡区域 //
	{
		DirectDiffuse = lerp(DirectDiffuse*_WetGroundDarkPercent, DirectDiffuse, lerpValue);
		// DirectSpecular = lerp(DirectSpecular * _SpecIntensity, 0, lerpValue);
	}
	else						// 地面区域 //
	{
		// DirectDiffuse 和 DirectSpecular  不做修改 //
	}
}

#endif				// _TERRAIN_WATER //
#endif				// TERRAIN_WATER_LIGHT_INCLUDE //