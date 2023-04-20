#ifndef URP_GRASS_FORWARD_PASS_INCLUDED
#define URP_GRASS_FORWARD_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "../NBR/Include/Fog.hlsl"
#include "URP_GrassInput.hlsl"
#include "OPPCore.hlsl"
#include "../Scene/Wind_Effect.hlsl"
#include "Assets/Engine/Runtime/Shaders/Shading/Include/SmartShadow.hlsl"

struct Attributes
{
	float4 positionOS : POSITION;
	float2 texcoord : TEXCOORD0;
	float2 lightmapUV : TEXCOORD1;
	UNITY_VERTEX_INPUT_INSTANCE_ID
#if !defined(DEPTH_PRE_PASS)
		float3 normalOS : NORMAL;
	float4 tangentOS : TANGENT;
#endif
};

struct Varyings
{
	precise float4 positionCS : SV_POSITION;
	float2 uv : TEXCOORD0;

	UNITY_VERTEX_INPUT_INSTANCE_ID
		UNITY_VERTEX_OUTPUT_STEREO

#if !defined(DEPTH_PRE_PASS)
		DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);

	float3 posWS : TEXCOORD2; // xyz: posWS

#ifdef _NORMALMAP
	float4 normal                   : TEXCOORD3;    // xyz: normal, w: viewDir.x
	float4 tangent                  : TEXCOORD4;    // xyz: tangent, w: viewDir.y
	float4 bitangent                : TEXCOORD5;    // xyz: bitangent, w: viewDir.z
#else
	float3 normal : TEXCOORD3;
	float3 viewDir : TEXCOORD4;
#endif

	half4 fogFactorAndVertexLight : TEXCOORD6; // x: fogFactor, yzw: vertex light

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
	float4 shadowCoord              : TEXCOORD7;
#endif

	float4 bottomnum : COLOR0;
	float4 realnormal :COLOR1;
#endif
};

float hash12(float2 p)
{
	float3 p3 = frac(float3(p.xyx) * .1031);
	p3 += dot(p3, p3.yzx + 33.33);
	return frac((p3.x + p3.y) * p3.z);
}

half4 CalcCustomBaseColor(float2 uv)
{
	half4 color = Sample2D(uv, TEXTURE2D_ARGS(_MainTex, sampler_MainTex));
	return color;
}


///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

// Vertex  functions    

float3 CustomWindEffect(Attributes Input, float3 WorldPosition)
{
	float3 offset = 0;
	// 2022年1月28日 张欣颖：if不管是否带UNITY_BRANCH都会导致草闪烁，原因不明。
	// UNITY_BRANCH
	// if (_AmbientWindSpeed > 0.01)
	{
		float hweight = saturate(Input.lightmapUV.y) * 0.01;
		float2 WaveUV = (WorldPosition.xz + _TimeParameters.x * _AmbientWindSpeed * float2(
			_AmbientWindDirx, _AmbientWindDirz)) * 0.01;
		float WaveTex = SAMPLE_TEX2D_LOD(_AmbientWind, WaveUV, 0).r;
		float Wavebig = WaveTex * WaveTex * hweight * _GustingStrength;
		float Wavesmall = sin(_GustingFrequency * _TimeParameters.x) * hweight;
		offset.xz = Wavebig + Wavesmall;
	}
	UNITY_BRANCH
		if (_GRASS_COLLISION)
		{
#ifdef _ISFLOWER_OFF
			float2 UV = saturate((WorldPosition.xyz - _CollisionCenter.xyz).xz / _CollisionRadiusInv + float2(0.5, 0.5));
			float4 CollisionOffset = SAMPLE_TEX2D_LOD(_CollisionTex, UV, 0);
			float3 dirctions = (float3(CollisionOffset.x, 0, CollisionOffset.y) * 2 - float3(1, 0, 1)) * CollisionOffset.z *
				_PushValue;
			offset += dirctions;
			offset *= Input.texcoord.y;
#endif
		}
	return offset;
}

half4 GrassEffecttest(Attributes Input)
{
	half4 waveWind1 = half4(Input.positionOS.xyz, 1);
#ifdef _ISFLOWER_OFF
	half4 center = mul(unity_ObjectToWorld, half4(0, 0, 0, 1));
	half2 UV = saturate((center.xyz - _CollisionCenter.xyz).xz / _CollisionRadiusInv + half2(0.5, 0.5));
	half4 tmp_wind = SAMPLE_TEX2D_LOD(_CollisionTex, UV, 0);
	half tmp_height = saturate(Input.lightmapUV.y);
	half3 calwind = half3(tmp_wind.x, 0, tmp_wind.y) * 2 - half3(1, 0, 1);
	half3 windDirection = normalize(cross(calwind, half3(0, -1, 0)));
	half xddee = tmp_wind.z * tmp_wind.z * _PushAngle * tmp_height * any(windDirection);
	half3x3 tmp_rotation = RotationMatrix(windDirection, radians(xddee));
	half3 waveWind = mul(tmp_rotation, Input.positionOS.xyz);
	waveWind1 = half4(waveWind, 1);

#endif
	//}
	return waveWind1;
}


#if !defined(DEPTH_PRE_PASS)
void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
{
	inputData.positionWS = input.posWS;

#ifdef _NORMALMAP
	half3 viewDirWS = half3(input.normal.w, input.tangent.w, input.bitangent.w);
	inputData.normalWS = TransformTangentToWorld(normalTS,
		half3x3(input.tangent.xyz, input.bitangent.xyz, input.normal.xyz));
#else
	half3 viewDirWS = input.viewDir;
	inputData.normalWS = input.normal;
#endif

	inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
	viewDirWS = SafeNormalize(viewDirWS);

	inputData.viewDirectionWS = viewDirWS;

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
	inputData.shadowCoord = input.shadowCoord;
#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
	inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
#else
	inputData.shadowCoord = float4(0, 0, 0, 0);
#endif

	inputData.fogCoord = input.fogFactorAndVertexLight.x;
	inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
	inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);
	inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
#if defined(_SMARTSOFTSHADOW_ON)
	inputData.shadowMask = GetSmartShadow(_MainLightPosition.xyz, inputData.normalWS, float4(inputData.positionWS, 1), _SmartShadowIntensity);
#else
	//inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
	inputData.shadowMask = 0;
#endif
}
#endif

#if _INSTANCING_HIGH

//struct PS｛
//float3 position;
//float3 scale;
//｝;
//RWStructuredBuffer<PS> inss;
StructuredBuffer<float4x4> matrix4x4Buffer;
Varyings LitPassVertex(Attributes input, uint instanceID : SV_InstanceID)
#else
Varyings LitPassVertex(Attributes input)
#endif

{
	Varyings output = (Varyings)0;
	if (_GRASS_COLLISION)
	{
		input.positionOS = GrassEffecttest(input);
	}
	VertexPositionInputs vertexInput;
#if _INSTANCING_HIGH
	float4x4 materix = matrix4x4Buffer[instanceID];
	vertexInput.positionWS = mul(materix, input.positionOS);
#else
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_TRANSFER_INSTANCE_ID(input, output);
	vertexInput.positionWS = TransformObjectToWorld(input.positionOS.xyz);
#endif
	UNITY_BRANCH
		if (_GRASS_COLLISION)
		{
			input.positionOS = GrassEffecttest(input);
		}
	float3 xyzmove = CustomWindEffect(input, vertexInput.positionWS);
	vertexInput.positionWS.xyz += xyzmove;
	vertexInput.positionCS = TransformWorldToHClip(vertexInput.positionWS);

#if !defined(DEPTH_PRE_PASS)

	vertexInput.positionVS = TransformWorldToView(vertexInput.positionWS);

	float4 ndc = vertexInput.positionCS * 0.5f;
	vertexInput.positionNDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
	vertexInput.positionNDC.zw = vertexInput.positionCS.zw;

	VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
	half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
	half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
	half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

	output.posWS.xyz = vertexInput.positionWS;
	half4 treecenterpos = mul(unity_ObjectToWorld, half4(0, 0, 0, 1));
	//  output.realnormal.xyz=normalize(output.posWS.xyz - treecenterpos.xyz)+half3(xyzmove.x,0,xyzmove.z);
	output.realnormal.xyz = normalize(_GrassNormalDir.xyz + half3(xyzmove.x, 0, xyzmove.z));


	normalInput.normalWS = half3(0, 1, 0);
#ifdef _NORMALMAP
	output.normal = half4(normalInput.normalWS, viewDirWS.x);
	output.tangent = half4(normalInput.tangentWS, viewDirWS.y);
	output.bitangent = half4(normalInput.bitangentWS, viewDirWS.z);
#else
	output.normal = NormalizeNormalPerVertex(normalInput.normalWS);
	output.viewDir = viewDirWS;
#endif

	OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
	OUTPUT_SH(output.normal.xyz, output.vertexSH);

	output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
	output.shadowCoord = GetShadowCoord(vertexInput);
#endif

	output.bottomnum.z = saturate(input.lightmapUV.y);
	output.bottomnum.xy = output.posWS.xz * 0.01 * _AmbientWind_ST.xy + _AmbientWind_ST.zw;
#endif

	output.uv = TRANSFORM_TEX(input.texcoord, _MainTex);

	output.positionCS = vertexInput.positionCS;

	return output;
}

//Fragment functions 

half4 LitPassFragment(Varyings input) : SV_Target
{
	half4 diffuseAlpha = CalcCustomBaseColor(input.uv);
	half alpha = diffuseAlpha.a * _Color0.a;

#if defined(DEPTH_PRE_PASS)
	AlphaDiscard(alpha, _Cutoff);
	return 0;
#else
	UNITY_SETUP_INSTANCE_ID(input);
	half3 diffuse = diffuseAlpha.rgb * _Color0.rgb;
#ifdef _ALPHAPREMULTIPLY_ON
	diffuse *= alpha;
#endif

	half3 normalTS = GetDefaultNormal();
	half3 emission = input.bottomnum.z;

	InputData inputData;
	InitializeInputData(input, normalTS, inputData);

	float4 lightClipPos = mul(_BakeLightmapProjection, half4(input.posWS, 1));
	lightClipPos.xyz = lightClipPos.xyz / lightClipPos.w;
	float2 buttomuv = lightClipPos.xy * 0.5f + 0.5f;
	half4 lightmapcolor = Sample2D(buttomuv, TEXTURE2D_ARGS(_CustomLightmap, sampler_CustomLightmap));
	half4 groundcolor = Sample2D(buttomuv, TEXTURE2D_ARGS(_CustomGroundColor, sampler_CustomGroundColor));
	inputData.shadowMask = groundcolor.a;
	half ambientweight = Sample2D(input.bottomnum.xy, TEXTURE2D_ARGS(_AmbientWind, sampler_AmbientWind)).g;
	//   half  result=min(input.bottomnum.z*_BottomPersent,1);
	_BottomPersent = _BottomPersent - _BottomScale;
	half gweight = _BottomPersent + _BottomScale;
	half result = smoothstep(_BottomPersent, gweight, input.bottomnum.z);

	//  half bigvalue=smoothstep(0.34,1,_WeightRG);
	half testvalue = step(_WeightGB, _WeightRG); //bijizhi
												 //   half smalvalue=_WeightGB*testvalue+_WeightRG-abs(_WeightGB-_WeightGB)
	half bigvalue = _WeightRG * testvalue + _WeightGB * (1 - testvalue);
	half smalvalue = _WeightGB * testvalue + _WeightRG * (1 - testvalue);

	_Transvalue = _Transvalue * 0.2;

	half Rvalue = step(bigvalue, ambientweight);
	half Gvalue = step(smalvalue, ambientweight) * step(1, 1 - Rvalue);
	half Bvalue = step(1, 1 - Rvalue - Gvalue);

	//gudu
	half bigtranbool = Rvalue * step(ambientweight, bigvalue + _Transvalue); //需要不要处理
	half miduptranbool = Gvalue * step(bigvalue - _Transvalue, ambientweight);
	half middowtranbool = Gvalue * step(ambientweight, smalvalue + _Transvalue);
	half smaluptranbool = Bvalue * step(smalvalue - _Transvalue, ambientweight);

	half bigmidvs = abs(abs(ambientweight - bigvalue) - _Transvalue) / (_Transvalue * 2);
	half bigpercent = Rvalue * bigtranbool * (1 - bigmidvs) + Gvalue * miduptranbool * bigmidvs;
	half miduppercent = 1 - bigpercent;

	half midsmalvs = abs(abs(ambientweight - smalvalue) - _Transvalue) / (_Transvalue * 2);
	half middowpercent = Gvalue * middowtranbool * (1 - midsmalvs) + Bvalue * smaluptranbool * midsmalvs;
	half smalpercent = 1 - middowpercent;

	half4 trancolor = (bigtranbool + miduptranbool) * (bigpercent * _ColorR + miduppercent * _ColorG) + (middowtranbool
		+ smaluptranbool) * (middowpercent * _ColorG + smalpercent * _ColorB);
	half4 colorcombie = Rvalue * (1 - bigtranbool) * _ColorR + Gvalue * (1 - saturate(miduptranbool + middowtranbool)) *
		_ColorG + Bvalue * (1 - smaluptranbool) * _ColorB + trancolor;

	// diffuse = lerp(diffuse, colorcombie.xyz, 0.1);
	// diffuse = lerp(groundcolor.xyz, diffuse, result);
	diffuse = (0.9f * diffuse + 0.1 * colorcombie.xyz - groundcolor.xyz) * result + groundcolor.xyz;

	half4 color = GrassBlinnPhong(inputData, diffuse, emission, alpha, lightmapcolor.xyz, input.realnormal.xyz);
	APPLY_FOG(color.rgb, input.posWS.xyz);

	SphereDitherTransparent(input.positionCS, _DitherTransparency);
	return half4(color);
#endif
}


#endif
