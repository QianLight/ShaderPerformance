
#ifndef SHADOWLIB_INCLUDE
#define SHADOWLIB_INCLUDE

/**
* Common.cginc: Common fun and paramaters.
*/
#include "../StdLib.hlsl"
#include "../Colors.hlsl"
#include "DefaultDebugHead.hlsl"
#include "../../Include/SmartShadow.hlsl"

struct FShadowData {
	// x: shadow with _ShadowFade
	// y: composited shadow
	// z:dynamicShadow
	// w:mask
	FLOAT4 Shadow;

	// x: selfShadow
	// y:
	// z:
	// w: 
	FLOAT4 Shadow1;
};

inline FShadowData DefaultShadowData() {
	FShadowData ShadowData;
	ShadowData.Shadow = 1;
	ShadowData.Shadow1 = 1;
	return ShadowData;
}

DECLARE_PROPERTY(FShadowData, Shadow1.x, SelfShadow)
DECLARE_PROPERTY(FShadowData, Shadow.y, SrcShadow)
DECLARE_PROPERTY(FShadowData, Shadow.z, DynamicShadow)
DECLARE_PROPERTY(FShadowData, Shadow.x, FadeShadow)

#define _SHADOW_MAP_SOFT

//proj matrix
FLOAT4x4 _ShadowMapParam0;//csm0 csm1
FLOAT4x4 _ShadowMapParam1;//csm2 csmExtra

FLOAT4x4 _ShadowMapParam2;//simple 

FLOAT4 _ShadowDir;

#define _ShadowLightDir (-_ShadowDir.xyz)
#define _ShadowLightInvSin (_ShadowDir.w)

TEX2DARRAY_SHADOWMAP_SAMPLER(_ShadowMapTex);
TEX2D_SHADOWMAP(_ShadowMapExtraTex);
TEX2D_SHADOWMAP(_ShadowMapExtra1Tex);
TEX2D_SHADOWMAP(_CustomShadowMap);

TEX2D_SAMPLER(_CloudMap);
FLOAT4 _ShadowMapSize;

// Begin By MJ //
TEX2D_SHADOWMAP(_ShadowMapSelfTex);
FLOAT4 _ShadowMapSelfTex_TexelSize;

FLOAT4x4 _SelfShadowVP;
FLOAT4x4 _SelfShadow2VP;


#ifdef _SELF_SHADOW
#define _SFVP _SelfShadowVP
#else 
#define _SFVP _SelfShadow2VP
#endif

#ifdef _SELF_SHADOW2
FLOAT3 _ObjCenter;
#endif

FLOAT _PCFSampleDistance;

FLOAT4 _SelfShadowParam;
#define _Bias _SelfShadowParam.x
#define _ESM_Exponent _SelfShadowParam.w
// End By MJ //

//per csm
FLOAT4 _ShadowParam0;
#define _NormalBias0 _ShadowParam0.x
#define _NormalBias1 _ShadowParam0.y
#define _NormalBias2 _ShadowParam0.z
#define _NormalBiasExtra _ShadowParam0.w
// #define _ShadowBias _ShadowParam.z

FLOAT4 _ShadowParam1;
#define _ShadowBias _ShadowParam1.y
#define _Extra2Enable _ShadowParam1.z>0
#define _ShadowFade _ShadowParam1.w
FLOAT4 _ShadowMapExtra1Center;
FLOAT4 _ShadowMapExtra1VP;
FLOAT4 _ShadowColor;

FLOAT4 _ShadowMoveOffset;

#define _SoftShadow _ShadowParam1.x>0.5

#define _PlayerShadowOnly _ShadowParam1.x<0
#ifndef _CSM3
    #define _PlayerShadowOnly
#endif 



REAL4 _ShadowParam2;
#define _ChunkShadowBias _ShadowParam2.x
//#define _CloudMapEnable _ShadowParam2.x>0.5
//#define _CloudSpeed _ShadowParam2.yz
//#define _CloudScale _ShadowParam2.w

//FLOAT4 _ShadowParam3;
//#define _CloudHeightRatio _ShadowParam3.x
//#define _CloudBrightRatio _ShadowParam3.y
FLOAT4 _ShadowParam4;
#define _RoleShadowMultiply _ShadowParam4.y
#define _RoleRimFade _ShadowParam4.z
#define _ObjectBias _ShadowParam4.w

FLOAT _ShadowRimIntensity;

FLOAT4 _RoleShadowColor;

FLOAT4 _ShadowMapFade;

TEX2D_SAMPLER(_DepthShadowTex);
FLOAT4 _DepthShadow;
FLOAT4x4 _DepthsShadowMatrix;
FLOAT4 _ShadowDircontrol;
#define _DepthShadowEnable _DepthShadow.w>0.5
#define _DepthShadowLightDir _DepthShadow.xy



struct AttributesShadow
{
	FLOAT3 vertex : POSITION;
#if defined(_SELF_SHADOW)||defined(_SELF_SHADOW2)
	FLOAT3 normal	: NORMAL;
#endif//_SELF_SHADOW
#ifdef _ALPHA_TEST
	FLOAT2	uv : TEXCOORD0;
#endif//_ALPHA_TEST
};

struct VaryingsShadow
{
	FLOAT4 vertex : SV_POSITION;
#ifdef _ALPHA_TEST
	FLOAT2	uv : TEXCOORD0;
#endif//_ALPHA_TEST
};

FLOAT4 TransformShadowPos(FLOAT4 wpos,FLOAT3 shadowMaxPlaneCenter)
{
	FLOAT deltaY = shadowMaxPlaneCenter.y - wpos.y;
	FLOAT3 newCenter = shadowMaxPlaneCenter.xyz + _ShadowLightDir.xyz*deltaY*_ShadowLightInvSin;
	FLOAT2 xz = wpos.xz - newCenter.xz;
	return  FLOAT4(xz,-deltaY,1);
}

// #ifdef _DYNMAIC_SHADOWCASTER
// FLOAT4 _DynamicShadowLightCenter;
// #define _ShadowVolumCenter _DynamicShadowLightCenter
// #else
FLOAT4 _ShadowLightCenter;
#define _ShadowVolumCenter _ShadowLightCenter
// #endif

VaryingsShadow vertCustomCast(AttributesShadow v)
{
	VaryingsShadow o;
	o.vertex = mul(unity_ObjectToWorld, FLOAT4(v.vertex.xyz,1.0f));
	FLOAT4 pos = TransformShadowPos(o.vertex,_ShadowVolumCenter.xyz);
	
	pos = mul(unity_MatrixVP, pos);

	#if !defined(UNITY_REVERSED_Z)
		pos.z = 1 - pos.z;
	#endif

	o.vertex = pos;
	#ifdef _ALPHA_TEST
		o.uv = v.uv;
	#endif//_ALPHA_TEST
	return o;
}
FLOAT4 vertCustomFrag(VaryingsShadow i) : SV_Target
{			
	#ifdef _ALPHA_TEST
		FLOAT alpha = SAMPLE_TEX2D(_MainTex, i.uv).a;
		clip(alpha-0.4);
	#endif//_ALPHA_TEST
	return 0;
}


FLOAT3 GetShadowCoord(in FLOAT4 wpos,in FLOAT3 WorldNormal,FLOAT bias,in FLOAT4 shadowMaxPlaneCenter, in FLOAT4 shadowMapVP)
{

	//float3 wNormal = UnityObjectToWorldNormal(normal);
	//float3 wLight = normalize(UnityWorldSpaceLightDir(wPos.xyz));

	// apply normal offset bias (inset position along the normal)
	// bias needs to be scaled by sine between normal and light direction
	// (http://the-witness.net/news/2013/09/shadow-mapping-summary-part-1/)
	//
	// unity_LightShadowBias.z contains user-specified normal offset amount
	// scaled by world space texel size.

	//float shadowCos = dot(wNormal, wLight);
	//float shadowSine = sqrt(1 - shadowCos * shadowCos);
	//float normalBias = unity_LightShadowBias.z * shadowSine;

	FLOAT shadowCos = dot(WorldNormal, _ShadowLightDir);
	FLOAT shadowSine = sqrt(1 - shadowCos * shadowCos);
	float normalBias = bias * shadowSine;


	wpos.xyz += WorldNormal * normalBias;
	wpos = TransformShadowPos(wpos,shadowMaxPlaneCenter.xyz);
	FLOAT4 ShadowCoord = shadowMapVP*wpos;//mul(_ShadowMapVP0, FLOAT4(wpos.xyz,1)).xyz;
	ShadowCoord.z += shadowMapVP.w;
	ShadowCoord.xy = ShadowCoord.xy*0.5f+FLOAT2(0.5,0.5);
	ShadowCoord.z = saturate(ShadowCoord.z);
	return ShadowCoord.xyz;
}

void EncodeShadowCoord(in FLOAT4 worldPos,in FLOAT3 WorldNormal,
inout FLOAT4 ShadowCoord0,inout FLOAT4 ShadowCoord1,inout FLOAT4 ShadowCoord2)
{

	//worldPos.xyz += _ShadowMoveOffset.xyz;
	FLOAT3 coord0 = GetShadowCoord(worldPos + _ShadowMoveOffset,WorldNormal,_NormalBias0,_ShadowMapParam0[0] ,_ShadowMapParam0[1]);
	FLOAT3 coord1 = GetShadowCoord(worldPos + _ShadowMoveOffset,WorldNormal,_NormalBias1,_ShadowMapParam0[2],_ShadowMapParam0[3]);
	FLOAT3 coord3 = GetShadowCoord(worldPos,WorldNormal,_NormalBiasExtra,_ShadowMapParam1[2],_ShadowMapParam1[3]);
#ifdef _CSM3
	FLOAT3 coord2 = GetShadowCoord(worldPos + _ShadowMoveOffset,WorldNormal,_NormalBias2,_ShadowMapParam1[0],_ShadowMapParam1[1]);
	ShadowCoord0.xyz = coord0;
	ShadowCoord1.xyz = coord1;
	ShadowCoord2.xyz = coord2;
	ShadowCoord0.w = coord3.x;
	ShadowCoord1.w = coord3.y;
	ShadowCoord2.w = coord3.z;
#else
	ShadowCoord0.xyz = coord0;
	ShadowCoord1.xyz = coord1;
	ShadowCoord0.w = coord3.x;
	ShadowCoord1.w = coord3.y;
	ShadowCoord2.w = coord3.z;
#endif//_CSM3

}
void EncodeSimpleShadowCoord(in FLOAT4 worldPos, in FLOAT3 WorldNormal,
	inout FLOAT4 ShadowCoord0)
{
	ShadowCoord0.xyz = GetShadowCoord(worldPos + _ShadowMoveOffset, WorldNormal, _NormalBias2, _ShadowMapParam2[0], _ShadowMapParam2[1]);
}

void DecodeShadowCoord(
#ifdef _CSM3
	inout FLOAT3 ShadowCoord[4],
#else//!_CSM3
	inout FLOAT3 ShadowCoord[3],
#endif//_CSM3
	in FLOAT4 Coord0, 
	in FLOAT4 Coord1,
	in FLOAT4 Coord2
	)
{
	ShadowCoord[0] = Coord0.xyz;
	ShadowCoord[1] = Coord1.xyz;
#ifdef _CSM3
	ShadowCoord[2] = Coord2.xyz;
	ShadowCoord[3] = FLOAT3(Coord0.w,Coord1.w,Coord2.w);
#else
	ShadowCoord[2] = FLOAT3(Coord0.w,Coord1.w,Coord2.w);
#endif//_CSM3
}

/**
* Assuming a isoceles triangle of 1.5 texels height and 3 texels wide lying on 4 texels.
* This function return the area of the triangle above each of those texels.
*    |    <-- offset from -0.5 to 0.5, 0 meaning triangle is exactly in the center
*   / \   <-- 45 degree slop isosceles triangle (ie tent projected in 2D)
*  /   \
* _ _ _ _ <-- texels
* X Y Z W <-- result indices (in computedArea.xyzw and computedAreaUncut.xyzw)
*/
void Get3TexelsWideTriangleFilter(FLOAT offset, out FLOAT4 computedArea)
{
    //Compute the exterior areas
    FLOAT offset01SquaredHalved = 0.5*offset*offset+0.5*offset + 0.125;//(offset + 0.5) * (offset + 0.5) * 0.5;
    computedArea.x = offset01SquaredHalved - offset;
    computedArea.w = offset01SquaredHalved;

    //Compute the middle areas
    //For Y : We find the area in Y of as if the left section of the isoceles triangle would
    //intersect the axis between Y and Z (ie where offset = 0).
    // computedAreaUncut.y = _UnityInternalGetAreaAboveFirstTexelUnderAIsocelesRectangleTriangle(1.5 - offset);
    //This area is superior to the one we are looking for if (offset < 0) thus we need to
    //subtract the area of the triangle defined by (0,1.5-offset), (0,1.5+offset), (-offset,1.5).
    FLOAT clampedOffsetLeft = min(offset,0);
    FLOAT areaOfSmallLeftTriangle = clampedOffsetLeft * clampedOffsetLeft;
    computedArea.y = 1 - offset - areaOfSmallLeftTriangle;

    //We do the same for the Z but with the right part of the isoceles triangle
    // computedAreaUncut.z = _UnityInternalGetAreaAboveFirstTexelUnderAIsocelesRectangleTriangle(1.5 + offset);
    FLOAT clampedOffsetRight = max(offset,0);
    FLOAT areaOfSmallRightTriangle = clampedOffsetRight * clampedOffsetRight;
    computedArea.z = 1  + offset - areaOfSmallRightTriangle;
	computedArea *= 0.44444;
}

inline FLOAT FetchDepth(TEX2DARRAY_ARGS_SHADOW(tex),FLOAT2 uvBase,FLOAT3 uvOffset, FLOAT slice)
{
	FLOAT4 uv = FLOAT4(uvBase + uvOffset.xy,slice, uvOffset.z);
	#ifdef _SUPPORT_TEXARR_CMP
		return SAMPLE_SHADOWMAP_TEX2DARRAR_CMP(tex, uv);
	#else//!_SUPPORT_TEXARR_CMP
		FLOAT depth = SAMPLE_SHADOWMAP_TEX2DARRAR_POINT(tex,uv.xyz).r;
		return depth<uv.w?0:1;
	#endif//_SUPPORT_TEXARR_CMP
}

inline FLOAT FetchDepthNoArray(TEX2D_ARGS_SHADOW(tex),FLOAT2 uvBase,FLOAT3 uvOffset)
{
	FLOAT3 uv = FLOAT3(uvBase + uvOffset.xy, uvOffset.z);
	return SAMPLE_SHADOW_CMP(tex, uv);
}

inline void CalcPCF(FLOAT3 ShadowCoord, FLOAT2 shadowMapSize,out FLOAT2 uvOrigin,out FLOAT2 fetchesWeightsU,out FLOAT2 fetchesWeightsV,out FLOAT2 fetchesOffsetsU,out FLOAT2 fetchesOffsetsV)
{
	#define _CSM_SHADOW_SIZE shadowMapSize.x
	#define _CSM_SHADOW_INVSIZE shadowMapSize.y

	FLOAT2 uvTexSpace = ShadowCoord.xy * _CSM_SHADOW_SIZE;
	FLOAT2 centerUVTexSpace = floor(uvTexSpace + FLOAT2(0.5,0.5));
	FLOAT2 offsetUV = uvTexSpace - centerUVTexSpace;

	// find the weight of each texel based
	FLOAT4 texelsWeightsU, texelsWeightsV;
	Get3TexelsWideTriangleFilter(offsetUV.x, texelsWeightsU);
	Get3TexelsWideTriangleFilter(offsetUV.y, texelsWeightsV);

	// each fetch will cover a group of 2x2 texels, the weight of each group is the sum of the weights of the texels
	fetchesWeightsU = texelsWeightsU.xz + texelsWeightsU.yw;
	fetchesWeightsV = texelsWeightsV.xz + texelsWeightsV.yw;

	// move the PCF bilinear fetches to respect texels weights
	fetchesOffsetsU = texelsWeightsU.yw / fetchesWeightsU.xy + FLOAT2(-1.5,0.5);
	fetchesOffsetsV = texelsWeightsV.yw / fetchesWeightsV.xy + FLOAT2(-1.5,0.5);
	fetchesOffsetsU *= _CSM_SHADOW_INVSIZE;
	fetchesOffsetsV *= _CSM_SHADOW_INVSIZE;

	uvOrigin = centerUVTexSpace * _CSM_SHADOW_INVSIZE;
}

FLOAT SampleShadowMap(TEX2DARRAY_ARGS_SHADOW(tex), FLOAT3 ShadowCoord, FLOAT shadowMapIndex)
{
	FLOAT shadow = 1;
	UNITY_BRANCH
	if(_SoftShadow&&shadowMapIndex<2)
	{
		FLOAT2 uvOrigin;
		FLOAT2 fetchesWeightsU;
		FLOAT2 fetchesWeightsV;
		FLOAT2 fetchesOffsetsU;
		FLOAT2 fetchesOffsetsV;
		CalcPCF(ShadowCoord,_ShadowMapSize.xy,uvOrigin,fetchesWeightsU,fetchesWeightsV,fetchesOffsetsU,fetchesOffsetsV);
		
		shadow =  fetchesWeightsU.x * fetchesWeightsV.x * FetchDepth(TEX2D_PARAM(tex),uvOrigin, FLOAT3(fetchesOffsetsU.x, fetchesOffsetsV.x,ShadowCoord.z), shadowMapIndex);
		shadow += fetchesWeightsU.y * fetchesWeightsV.x * FetchDepth(TEX2D_PARAM(tex),uvOrigin, FLOAT3(fetchesOffsetsU.y, fetchesOffsetsV.x,ShadowCoord.z), shadowMapIndex);
		shadow += fetchesWeightsU.x * fetchesWeightsV.y * FetchDepth(TEX2D_PARAM(tex),uvOrigin, FLOAT3(fetchesOffsetsU.x, fetchesOffsetsV.y,ShadowCoord.z), shadowMapIndex);
		shadow += fetchesWeightsU.y * fetchesWeightsV.y * FetchDepth(TEX2D_PARAM(tex),uvOrigin, FLOAT3(fetchesOffsetsU.y, fetchesOffsetsV.y,ShadowCoord.z), shadowMapIndex);
	}
	else
	{
		shadow = FetchDepth(TEX2D_PARAM(tex),ShadowCoord.xy,FLOAT3(0,0,ShadowCoord.z), shadowMapIndex);
	}
	return shadow;
}

REAL SampleShadowMap2(TEX2D_ARGS_SHADOW(tex),REAL3 ShadowCoord)
{
	REAL shadow = 1;
	// UNITY_BRANCH
	// if(_SoftShadow)
	// {
	// 	FLOAT2 uvOrigin;
	// 	FLOAT2 fetchesWeightsU;
	// 	FLOAT2 fetchesWeightsV;
	// 	FLOAT2 fetchesOffsetsU;
	// 	FLOAT2 fetchesOffsetsV;
	// 	CalcPCF(ShadowCoord,_ShadowMapSize.zw,uvOrigin,fetchesWeightsU,fetchesWeightsV,fetchesOffsetsU,fetchesOffsetsV);
		
	// 	shadow =  fetchesWeightsU.x * fetchesWeightsV.x * FetchDepthNoArray(uvOrigin, FLOAT3(fetchesOffsetsU.x, fetchesOffsetsV.x,ShadowCoord.z));
	// 	shadow += fetchesWeightsU.y * fetchesWeightsV.x * FetchDepthNoArray(uvOrigin, FLOAT3(fetchesOffsetsU.y, fetchesOffsetsV.x,ShadowCoord.z));
	// 	shadow += fetchesWeightsU.x * fetchesWeightsV.y * FetchDepthNoArray(uvOrigin, FLOAT3(fetchesOffsetsU.x, fetchesOffsetsV.y,ShadowCoord.z));
	// 	shadow += fetchesWeightsU.y * fetchesWeightsV.y * FetchDepthNoArray(uvOrigin, FLOAT3(fetchesOffsetsU.y, fetchesOffsetsV.y,ShadowCoord.z));
	// }
  	// else
	{
		shadow = FetchDepthNoArray(TEX2D_PARAM(tex),ShadowCoord.xy, REAL3(0,0,ShadowCoord.z));
	}
	return shadow;	
 }

int IsInRange01(FLOAT value)
{
	return value >= 0 && value <= 1;
}

int IsInRange01(FLOAT2 value)
{
	return IsInRange01(value.x) * IsInRange01(value.y);
}

int IsInRange01(FLOAT3 value)
{
	return IsInRange01(value.x) * IsInRange01(value.y) * IsInRange01(value.z);
}

// 9次采样 //
FLOAT PCF_Regular(FLOAT2 uv, FLOAT depthInLS)
{
	FLOAT shadow = 0;
	UNITY_LOOP
	for(int i=-1; i<= 1; i++)
	{
		for(int j=-1; j<= 1; j++)
		{
			FLOAT2 tempUV = uv + _ShadowMapSelfTex_TexelSize.xy*FLOAT2(i, j)*_PCFSampleDistance;
			FLOAT3 coord = FLOAT3(tempUV, depthInLS);
			FLOAT tempShadow = SAMPLE_SHADOW_CMP(_ShadowMapSelfTex, coord);

			UNITY_BRANCH
			if(!IsInRange01(coord))
			{
				tempShadow = 1;
			}
			shadow += tempShadow;
		}
	}

	shadow /= 9.0;
	return shadow;
}

FLOAT Random(FLOAT4 seed4)
{
	FLOAT dot_product = dot(seed4, FLOAT4(12.9898,78.233,45.164,94.673));
	return frac(sin(dot_product) * 43758.5453);
}

// #define PI 3.1415926

static const FLOAT2 poissonDisk[16] =
{
	FLOAT2(-0.94201624, -0.39906216),
	FLOAT2(0.94558609, -0.76890725),
	FLOAT2(-0.094184101, -0.92938870),
	FLOAT2(0.34495938, 0.29387760),
	FLOAT2(-0.91588581, 0.45771432),
	FLOAT2(-0.81544232, -0.87912464),
	FLOAT2(-0.38277543, 0.27676845),
	FLOAT2(0.97484398, 0.75648379),
	FLOAT2(0.44323325, -0.97511554),
	FLOAT2(0.53742981, -0.47373420),
	FLOAT2(-0.26496911, -0.41893023),
	FLOAT2(0.79197514, 0.19090188),
	FLOAT2(-0.24188840, 0.99706507),
	FLOAT2(-0.81409955, 0.91437590),
	FLOAT2(0.19984126, 0.78641367),
	FLOAT2(0.14383161, -0.14100790)
};

// 8次采样 //
FLOAT PCF_RotatedPoisson(FLOAT3 worldPos, FLOAT2 uv, FLOAT depthInLS)
{
	int sampleCount = 8;
	FLOAT shadow = 0;
	for(int i=0; i<sampleCount; i++)
	{
		FLOAT randAngle = Random(FLOAT4(worldPos, 1));
		FLOAT s = sin(2*PI*randAngle);
		FLOAT c = cos(2*PI*randAngle);
		FLOAT2x2 rotateMatrix = FLOAT2x2(c, -s, s, c);

		FLOAT2 offset = _ShadowMapSelfTex_TexelSize.xy * mul(rotateMatrix, poissonDisk[i]) * _PCFSampleDistance;
		FLOAT3 coord = FLOAT3(uv + offset, depthInLS);
		FLOAT tempShadow = SAMPLE_SHADOW_CMP(_ShadowMapSelfTex, coord);
		shadow += tempShadow;
	}
	shadow /= sampleCount;
	return shadow; 
}

// exp(k*(z-d)) 参考: 13_exponential_shadow_maps.pdf //
REAL ESM(REAL depthInLS, REAL2 uv)
{
    FLOAT depthInSM = SAMPLE_SHADOW(_ShadowMapSelfTex, uv).r;
	FLOAT shadow = exp(-_ESM_Exponent * (depthInSM - depthInLS));
	return saturate(shadow);
}

FLOAT CalculateSelfShadow(FLOAT3 worldPos)
{
	REAL4 posLS = mul(_SFVP, FLOAT4(worldPos.xyz, 1));
	REAL2 uv = posLS.xy;

	uv = uv * 0.5 + 0.5;
	uv.y = 1 - uv.y;
	REAL depthInLS = posLS.z;
	REAL shadow = 1;
	// UNITY_BRANCH
	// if(_SoftShadow)
	// {
	// 	shadow = ESM(depthInLS, uv);
	// }
	// else
	{
		REAL3 coord = FLOAT3(uv, depthInLS);
		shadow = SAMPLE_SHADOW_CMP(_ShadowMapSelfTex, coord);
	}
	FLOAT mask0 = InRange(uv,FLOAT4(0,0,1,1));
	FLOAT mask1 = step(0, depthInLS)*step(depthInLS,1);
	shadow = lerp(1,shadow,mask0*mask1);
	return shadow;
}

// End By MJ //

FLOAT2 CalcDistDelta(in FLOAT4 worldPos,in FLOAT4 shadowMaxPlaneCenter)
{
	#define shadowHeight shadowMaxPlaneCenter.y
	#define shadowRange shadowMaxPlaneCenter.ww
	FLOAT deltaY = shadowHeight - worldPos.y;
	FLOAT3 newCenter = shadowMaxPlaneCenter.xyz + _ShadowLightDir.xyz*deltaY*_ShadowLightInvSin;
	FLOAT2 xz = worldPos.xz - newCenter.xz;
	return FLOAT2( shadowRange > abs(xz));
}

FLOAT CalcDistFall(in FLOAT4 worldPos,in FLOAT4 shadowMaxPlaneCenter,in FLOAT dist)
{
	#define shadowHeight shadowMaxPlaneCenter.y

	FLOAT border = shadowMaxPlaneCenter.w - dist;
	FLOAT deltaY = shadowHeight - worldPos.y;
	FLOAT3 newCenter = shadowMaxPlaneCenter.xyz + _ShadowLightDir.xyz*deltaY*_ShadowLightInvSin;
	FLOAT2 xz = abs(worldPos.xz - newCenter.xz);

	return 1-saturate(min((border-xz.x),(border-xz.y))*rcp(dist));
}

#ifdef _Role_Lighting
float GetRoleSmartShadow()
{
	#ifdef _ROLE_SOFT_SHADOW
		// 卡通渲染拿角色根位置渲染投影。因为战斗场景分层的原因，没办法做预烘焙。
		return GetSmartShadow9Point(GetRootPos(), 0.5, 1);
	#elif defined(_ROLE_SHADOW_OFF)
		return 1;
	#else
		// 这两个参数本来用于计算bias，但实际上算法里没用上，这里暂时传个空参数。
		float3 dummyParam = 0;
		return GetSmartShadow(dummyParam, dummyParam, GetRootPos(), 1);
	#endif
}
#endif

FShadowData ShadowCompareParallel(inout FFragData FragData, FMaterialData MaterialData,
FLOAT mask DEBUG_ARGS)
{
	FShadowData ShadowData = DefaultShadowData();

	#if defined(_OBJECT_ROOT_SHADOW)
		float shadow = FragData.CustomData1.w;
	#elif defined(BUILTIN_SM_OFF)
		float shadow = 1;
	#else // 大部分常规渲染使用 Unity内置CSM + SmartShadow(烘焙Shadowmap)
		// 采样CSM
		float4 shadowCoord = TransformWorldToShadowCoord(FragData.WorldPosition.xyz);
		float shadow = MainLightRealtimeShadow(shadowCoord);
		// 叠加SmartShadow
		UNITY_BRANCH
		if (shadow > 0.1f)
		{
			float4 worldPos = FragData.WorldPosition;
		#ifdef _SHADOW_RANDOM_OFFSET
			worldPos.x += sin(FragData.WorldPosition.y * 27.5) * sin(FragData.WorldPosition.z * 67.5)* 0.05;
			worldPos.y += sin(FragData.WorldPosition.z * 21.3) * sin(FragData.WorldPosition.x * 61.3)* 0.05;
			worldPos.z += sin(FragData.WorldPosition.x * 31.7) * sin(FragData.WorldPosition.y * 71.7)* 0.05;
		#endif		
			float smartShadow = GetSmartShadow(_MainLightDir.xyz, MaterialData.WorldNormal, worldPos, _SmartShadowIntensity);
			shadow = min(smartShadow, shadow);
		}
	#endif

	// 为了和旧Shader兼容需要拼回ShadowData。
	
	ShadowData.Shadow = saturate(FLOAT4(
		_ShadowFade + shadow * (1 - _ShadowFade), // 能被_ShadowFade缩放的实时投影.
		shadow, // 原extra shadow
		shadow, // 原dynamic shadow
		mask    // 原mask
	));

	// TODO: 渲染管线实现。
	SetSelfShadow(ShadowData, shadow);
	
	return ShadowData;
}

#endif //SHADOWLIB_INCLUDE