// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

#ifndef OP_SHADOW_INCLUDED
#define OP_SHADOW_INCLUDED


			uniform float4 shadowCoords[4];
			SamplerState global_point_clamp_sampler;
			SamplerState sampler_linear_clamp;
			SamplerState global_linear_clamp_sampler;
			sampler2D  _ShadowMapTerrainTexTest;
			SamplerState sampler_ShadowMapTerrainTexTest;

			float4 _MainLightDir0;
			#define _SceneLightDir _MainLightDir0
			float4 _ShadowParam0;
			#define _NormalBias0 _ShadowParam0.x
			#define _NormalBias1 _ShadowParam0.y
			#define _NormalBias2 _ShadowParam0.z
			#define _NormalBiasExtra _ShadowParam0.w
			#define  _ShadowLightDir (-_SceneLightDir.xyz)
			#define _ShadowLightInvSin (_SceneLightDir.w)
			float4x4 _ShadowMapParam0;//csm0 csm1
			float4x4 _ShadowMapParam1;//csm2 csmExtra
			float4 _ShadowMapFade;
			float4 _ShadowParam1;
			#define _ShadowFade _ShadowParam1.w
			float4 _ShadowColor;
			//float4 _MainLightDir0;
			#define _MainLightDir _MainLightDir0
			//Texture2DArray _ShadowMapTex;
			//UNITY_DECLARE_TEX2DARRAY (_ShadowMapTex);
			SamplerComparisonState sampler_ShadowMapTex;
			float4 _ShadowMapSize;
			sampler2D _ShadowMapTerrainTex;


void Get3TexelsWideTriangleFilter(float offset, out float4 computedArea)
{
	//Compute the exterior areas
	float offset01SquaredHalved = 0.5*offset*offset+0.5*offset + 0.125;//(offset + 0.5) * (offset + 0.5) * 0.5;
	computedArea.x = offset01SquaredHalved - offset;
	computedArea.w = offset01SquaredHalved;

	//Compute the middle areas
	//For Y : We find the area in Y of as if the left section of the isoceles triangle would
	//intersect the axis between Y and Z (ie where offset = 0).
	// computedAreaUncut.y = _UnityInternalGetAreaAboveFirstTexelUnderAIsocelesRectangleTriangle(1.5 - offset);
	//This area is superior to the one we are looking for if (offset < 0) thus we need to
	//subtract the area of the triangle defined by (0,1.5-offset), (0,1.5+offset), (-offset,1.5).
	float clampedOffsetLeft = min(offset,0);
	float areaOfSmallLeftTriangle = clampedOffsetLeft * clampedOffsetLeft;
	computedArea.y = 1 - offset - areaOfSmallLeftTriangle;

	//We do the same for the Z but with the right part of the isoceles triangle
	// computedAreaUncut.z = _UnityInternalGetAreaAboveFirstTexelUnderAIsocelesRectangleTriangle(1.5 + offset);
	float clampedOffsetRight = max(offset,0);
	float areaOfSmallRightTriangle = clampedOffsetRight * clampedOffsetRight;
	computedArea.z = 1  + offset - areaOfSmallRightTriangle;
	computedArea *= 0.44444;
}

float2 CalcDistDelta(in float4 worldPos,in float4 shadowMaxPlaneCenter)
{
	#define shadowHeight shadowMaxPlaneCenter.y
	#define shadowRange shadowMaxPlaneCenter.ww
	float deltaY = shadowHeight - worldPos.y;
	float3 newCenter = shadowMaxPlaneCenter.xyz + _ShadowLightDir.xyz*deltaY*_ShadowLightInvSin;
	float2 xz = worldPos.xz - newCenter.xz;
	return float2( shadowRange > abs(xz));
}


inline void CalcPCF(float3 ShadowCoord, float2 shadowMapSize,out float2 uvOrigin,out float2 fetchesWeightsU,out float2 fetchesWeightsV,out float2 fetchesOffsetsU,out float2 fetchesOffsetsV)
{
	#define _CSM_SHADOW_SIZE shadowMapSize.x
	#define _CSM_SHADOW_INVSIZE shadowMapSize.y

	float2 uvTexSpace = ShadowCoord.xy * _CSM_SHADOW_SIZE;
	float2 centerUVTexSpace = floor(uvTexSpace + float2(0.5,0.5));
	float2 offsetUV = uvTexSpace - centerUVTexSpace;

	// find the weight of each texel based
	float4 texelsWeightsU, texelsWeightsV;
	Get3TexelsWideTriangleFilter(offsetUV.x, texelsWeightsU);
	Get3TexelsWideTriangleFilter(offsetUV.y, texelsWeightsV);

	// each fetch will cover a group of 2x2 texels, the weight of each group is the sum of the weights of the texels
	fetchesWeightsU = texelsWeightsU.xz + texelsWeightsU.yw;
	fetchesWeightsV = texelsWeightsV.xz + texelsWeightsV.yw;

	// move the PCF bilinear fetches to respect texels weights
	fetchesOffsetsU = texelsWeightsU.yw / fetchesWeightsU.xy + float2(-1.5,0.5);
	fetchesOffsetsV = texelsWeightsV.yw / fetchesWeightsV.xy + float2(-1.5,0.5);
	fetchesOffsetsU *= _CSM_SHADOW_INVSIZE*0.5;
	fetchesOffsetsV *= _CSM_SHADOW_INVSIZE*0.5;

	uvOrigin = centerUVTexSpace * _CSM_SHADOW_INVSIZE;
}

inline float FetchDepth(float2 uvBase,float3 uvOffset, float slice)
{
	float4 uv = float4(uvBase + uvOffset.xy,slice, uvOffset.z);
	//return tex2D(_ShadowMapTerrainTexTest,uv.xy).r;
	float depth= tex2D(_ShadowMapTerrainTexTest,uv.xy).r;
	//return smoothstep(uv.w+0.021,uv.w,depth);
	return depth<uv.w?1:0;

}



	float SampleShadowMap(float3 ShadowCoord, float shadowMapIndex)
{
	float shadow = 1;

	float2 uvOrigin;
	float2 fetchesWeightsU;
	float2 fetchesWeightsV;
	float2 fetchesOffsetsU;
	float2 fetchesOffsetsV;
	CalcPCF(ShadowCoord,_ShadowMapSize.xy,uvOrigin,fetchesWeightsU,fetchesWeightsV,fetchesOffsetsU,fetchesOffsetsV);
	
	shadow =  fetchesWeightsU.x * fetchesWeightsV.x * FetchDepth(uvOrigin, float3(fetchesOffsetsU.x, fetchesOffsetsV.x,ShadowCoord.z), shadowMapIndex);
	shadow += fetchesWeightsU.y * fetchesWeightsV.x * FetchDepth(uvOrigin, float3(fetchesOffsetsU.y, fetchesOffsetsV.x,ShadowCoord.z), shadowMapIndex);
	shadow += fetchesWeightsU.x * fetchesWeightsV.y * FetchDepth(uvOrigin, float3(fetchesOffsetsU.x, fetchesOffsetsV.y,ShadowCoord.z), shadowMapIndex);
	shadow += fetchesWeightsU.y * fetchesWeightsV.y * FetchDepth(uvOrigin, float3(fetchesOffsetsU.y, fetchesOffsetsV.y,ShadowCoord.z), shadowMapIndex);

	//shadow = FetchDepth(ShadowCoord.xy,float3(0,0,ShadowCoord.z), shadowMapIndex);

	return shadow;
}

float2 ShadowCompareParallel(in float4 worldPos,in float3 shadowCoord,float mask)
{
	float shadow = 1;
	float2 delta0 = CalcDistDelta(worldPos,_ShadowMapParam0[0]); 
	float2 delta1 = CalcDistDelta(worldPos,_ShadowMapParam0[2]);
	float2 delta2 = CalcDistDelta(worldPos,_ShadowMapParam1[0]);
	float4 weights = float4(delta0.x*delta0.y,delta1.x*delta1.y,delta2.x*delta2.y,0);
	UNITY_BRANCH
	if(weights.z>0)
	{
		float mask0 = 1-weights.x;
		weights.y *= mask0;
		weights.z *= mask0-weights.y*mask0;
		shadow = SampleShadowMap(shadowCoord, 0);	
		 shadow *= mask;
		 shadow = _ShadowFade + shadow*(1-_ShadowFade);
	}
	//return shadow;
	return float2(_ShadowFade + shadow*(1-_ShadowFade),shadow);
}
float4 TransformShadowPos(float4 wpos,float3 shadowMaxPlaneCenter)
{
	float deltaY = shadowMaxPlaneCenter.y - wpos.y;
	float3 newCenter = shadowMaxPlaneCenter.xyz + _ShadowLightDir.xyz*deltaY*_ShadowLightInvSin;
	float2 xz = wpos.xz - newCenter.xz;
	return  float4(xz,-deltaY,1);
}

float3 GetShadowCoord(in float4 wpos,in float3 WorldNormal,float bias,in float4 shadowMaxPlaneCenter, in float4 shadowMapVP)
{
	wpos.xyz = WorldNormal *bias+ wpos.xyz;
	wpos = TransformShadowPos(wpos,shadowMaxPlaneCenter.xyz);
	float4 ShadowCoord = shadowMapVP*wpos;//mul(_ShadowMapVP0, float4(wpos.xyz,1)).xyz;
	ShadowCoord.z += shadowMapVP.w;
	ShadowCoord.xy = ShadowCoord.xy*0.5f+float2(0.5,0.5);
	ShadowCoord.z = clamp(ShadowCoord.z,0.001,1);
	return ShadowCoord.xyz;
}

float3 shadowCal (Input i )
{
	float mask = saturate(dot(i.worldNor, _MainLightDir.xyz)+0.5);
	float4 IntShadowCoord0,IntShadowCoord1,IntShadowCoord2;
	float3 FragShadowCoord[4] ;
	float3 myFragShadowCoord;

	myFragShadowCoord = GetShadowCoord(i.worldPos.xyzz,i.worldNor,_NormalBias2,_ShadowMapParam1[0],_ShadowMapParam1[1]);
	float2 shadow = ShadowCompareParallel(i.worldPos.xyzz,myFragShadowCoord,mask);
	float4 finalColor =shadow.x*_ShadowColor;
	finalColor.w=shadow.y;
	float3 DirectDiffuse =float3(1,1,1);
	float3 _FalloffColor=shadow.x*_ShadowColor.xyz;
	float _FalloffWeight=shadow.y;
	DirectDiffuse = lerp(_FalloffColor*DirectDiffuse,DirectDiffuse,_FalloffWeight);
	finalColor.xyz = DirectDiffuse;
	return finalColor;
	//return tex2D(_ShadowMapTerrainTexTest,myFragShadowCoord.xy).x*2;
	//return tex2D(_ShadowMapTerrainTexTest,myFragShadowCoord.xy).x > myFragShadowCoord.z  ? 0 :1;
	//return myFragShadowCoord;
}




#endif
