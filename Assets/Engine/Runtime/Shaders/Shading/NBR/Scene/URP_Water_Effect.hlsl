// Copyright 2018- PWRD, Inc. All Rights Reserved.

#ifndef WATER_EFFECT_INCLUDE
#define WATER_EFFECT_INCLUDE

#ifdef _WATER
// #define DEPTH_BASELINE 10.

// Geometry data
// x: A square is formed by 2 triangles in the mesh. Here x is square size
// yz: normalScrollSpeed0, normalScrollSpeed1
// FLOAT4 _OceanCenterPosWorld;

//normal
#define _WaterNormal _ProcedureTex0
#define _NoiseMap _MainTex
#define _NormalsStrength _UVST0.x
#define _NormalsScale _UVST0.y
#define _FlowSpeedU _UVST0.z
#define _FlowSpeedV _UVST0.w
//Scattering
#define _ScatteringColor _Color
#define _DiffuseGrazing _Color0

//high color
#define _ScatteringColorHigh _Color2
#define _DiffuseGrazingHigh _Color3

//Subsurface Scattering
#define _SubSurfaceColor _Color1
#define _SubSurfaceBase _Param0.x
#define _SubSurfaceSun _Param0.y
#define _SubSurfaceSunFallOff _Param0.z

//Shallow Scattering
// #define _SubSurfaceDepthMax _Param1.x
// #define _SubSurfaceDepthPower _Param1.y
// #define _SubSurfaceShallowCol _Color2

//Transparency
#define _RefractEnable _Param0.w>0.5
#define _RefractionStrength _Param1.x
#define _DepthFogDensity _Param1.yzw
//reflect
//light

//fresnel
#define _Specular _Param2.x
#define _FresnelPower _Param2.y
#define _RefractiveIndexOfAir _Param2.z
#define _RefractiveIndexOfWater _Param2.w

//sky
#define _DirectionalLightFallOff _Param3.x
#define _DirectionalLightBoost _Param3.y
#define _SkyBrightness _Param3.z


FLOAT4 _SDFParam;

#define _SDFLayerCount _SDFParam.x
#define _SDFStartHeight _SDFParam.y
#define _SDFEndHeight _SDFParam.z
#define _SDFPower _SDFParam.w

#ifndef _IgnoreSDF
TEX3D_SAMPLER(_SDF);

FLOAT4 _SDFBox;
#define _SDFSize _SDFBox.w
#endif

TEX2D_SAMPLER(_FoamTex);
FLOAT4 _Param9;
FLOAT4 _Param10;
FLOAT4 _Param11;

#define _GrazingPow _Param9.x
#define _FoamDistancePow _Param9.y
#define _FoamAttenDistance _Param9.z

#define _FoamIntensity _Param10.x
#define _FoamMin _Param10.y
#define _FoamSmooth _Param10.z
#define _FoamFresnelPow _Param10.w

#define _MinCameraY _Param11.x
#define _ChangeRateHeight _Param11.y
#define _CameraYRate _Param11.z
#define _FoamTexScale _Param11.w


FLOAT4 _ParamA;
FLOAT4 _ParamB;
FLOAT4 _ParamC;
FLOAT4 _Wave1;
FLOAT4 _Wave2;
FLOAT4 _Wave3;
FLOAT4 _Wave4;
FLOAT4 _Wave5;
FLOAT4 _Wave6;
FLOAT4 _Wave7;
FLOAT4 _Wave8;
FLOAT4 _Wave9;
FLOAT4 _SteepnessFadeout;
TEX2D_SAMPLER(_WaveTex); 
TEX2D_SAMPLER(_ReflectionMap);
SAMPLER(sampler_ReflectionMap_linear_clamp);
TEX2D_SAMPLER(_CameraOpaqueTexture);
SAMPLER(sampler_ScreenTextures_Trilinear_clamp);
float _WaveNormalDensity;
float4 _WaveCameraPos;
float _WaveVertexDensity;
//TDSSPR
float4 _MarchParam;

inline FLOAT3 UnpackNormalMap(FLOAT4 packednormal)
{
	return packednormal.xyz * 2 - 1;
}

FLOAT3 GerstnerWave(
	FLOAT4 wave,
	FLOAT3 p, 
	inout FLOAT3 tangent, 
	inout FLOAT3 binormal, 
	FLOAT time) 
{			
		FLOAT steepness = wave.z;
		steepness *= saturate(1- pow(abs(length(p.xz-_SteepnessFadeout.xy)/_SteepnessFadeout.z),_SteepnessFadeout.w));
		FLOAT wavelength = wave.w;
		FLOAT k = TWO_PI / wavelength;
		FLOAT c = sqrt(9.8 / k);
		FLOAT2 d = normalize(wave.xy);
		FLOAT f = k * (dot(d, p.xz) - c * time);
		FLOAT a = steepness / k;

		FLOAT sin = 0;
		FLOAT cos = 0;
		sincos(f,sin,cos);
		tangent += FLOAT3(
			-d.x * d.x * (steepness * sin),
			d.x * (steepness * cos),
			-d.x * d.y * (steepness * sin)
		);
		binormal += FLOAT3(
			-d.x * d.y * (steepness * sin),
			d.y * (steepness * cos),
			-d.y * d.y * (steepness * sin)
		);
		return FLOAT3(
			d.x * (a * cos),
			a * sin,
			d.y * (a * cos)
		);
}

FInterpolantsVSToPS CustomInterpolantsVSToPS(
	in FVertexInput Input, 
	in FLOAT4 WorldPosition,
	out FLOAT4 projPos,
	in REAL4 instanceRot)
{
	FLOAT3 gridPoint = WorldPosition.xyz;
	FLOAT3 tangent = FLOAT3(1, 0, 0);
	FLOAT3 binormal = FLOAT3(0, 0, 1);
	FLOAT3 p = gridPoint;
	FLOAT time = _Time.y;
	float3 worldPos =  WorldPosition.xyz;
	float worldWaveuvX = (worldPos.x  - _WaveCameraPos.x + _WaveCameraPos.w) / ((_WaveCameraPos.x + _WaveCameraPos.w) - (_WaveCameraPos.x - _WaveCameraPos.w));
	worldWaveuvX = saturate(worldWaveuvX);
	float worldWaveuvY = (worldPos.z  - _WaveCameraPos.z + _WaveCameraPos.w) / ((_WaveCameraPos.z + _WaveCameraPos.w) - (_WaveCameraPos.z - _WaveCameraPos.w));
	worldWaveuvY = saturate(worldWaveuvY);
	float2 worldWaveUV = float2(worldWaveuvX,worldWaveuvY);
	#ifdef _PPWave
		float border = (ceil(worldWaveuvX - 0.01)) *(ceil (-worldWaveuvX + 0.99)) * (ceil(worldWaveuvY - 0.01))* (ceil(-worldWaveuvY + 0.99));
		float3 WavePPTex = 	 SAMPLE_TEX2D_LOD( _WaveTex,  worldWaveUV,0)  - SAMPLE_TEX2D_LOD( _WaveTex, (worldWaveUV ) + float2( 0.01,0.01 ) ,0);
		WavePPTex *= border;
		p.y += WavePPTex.x * _WaveVertexDensity;
		p.y -= WavePPTex.y * _WaveVertexDensity;
	#endif
	p += GerstnerWave(_ParamA, gridPoint, tangent, binormal,time);
	p += GerstnerWave(_ParamB, gridPoint, tangent, binormal,time);
	p += GerstnerWave(_ParamC, gridPoint, tangent, binormal,time);
	p += GerstnerWave(_Wave1, gridPoint, tangent, binormal,time);
	p += GerstnerWave(_Wave2, gridPoint, tangent, binormal,time);
	p += GerstnerWave(_Wave3, gridPoint, tangent, binormal,time);
	p += GerstnerWave(_Wave4, gridPoint, tangent, binormal,time);
	p += GerstnerWave(_Wave5, gridPoint, tangent, binormal,time);
	p += GerstnerWave(_Wave6, gridPoint, tangent, binormal,time);
	p += GerstnerWave(_Wave7, gridPoint, tangent, binormal,time);
	p += GerstnerWave(_Wave8, gridPoint, tangent, binormal,time);
	p += GerstnerWave(_Wave9, gridPoint, tangent, binormal,time);			
	FLOAT3 normal = normalize(cross(binormal, tangent));
	// FLOAT3 viewPos = 0;
	projPos = TransformWorldToClipPos2(FLOAT4(p,1));
	DECLARE_OUTPUT(FInterpolantsVSToPS, Interpolants);
	Interpolants.WorldPosition = FLOAT4(p,1);

	// Interpolants.TangentToWorld0 = FLOAT4(tangent,1);
	// Interpolants.TangentToWorld2 = FLOAT4(normal,1);
	Interpolants.NormalWS.xyz = normal.xyz;
    Interpolants.TangentWS.xyz = tangent.xyz;
    Interpolants.BitangentWS.xyz = binormal.xyz;
	FLOAT3 viewDirWS = _WorldSpaceCameraPos.xyz - Interpolants.WorldPosition.xyz;
	Interpolants.NormalWS.w = viewDirWS.x;
	Interpolants.TangentWS.w = viewDirWS.y;
	Interpolants.BitangentWS.w = viewDirWS.z;

	Interpolants.TexCoords[0] = WorldPosition;

	float3 vpos = mul(unity_MatrixV, WorldPosition).xyz;
	SET_VS_DEPTH(Interpolants,projPos.zw);
	return Interpolants;
}

FLOAT WaveFoam(
    in FMaterialData MaterialData,
	in FFragData FragData)
{
	
	FLOAT3 cameraPos = _WorldSpaceCameraPos.xyz;
	FLOAT cameraY = cameraPos.y;
	FLOAT changeHeight = step(_ChangeRateHeight, cameraY); //相机高度超过_ChangeRateHeight，降低视角变化速率
	cameraY = lerp(max(_MinCameraY, cameraY), (cameraY - _ChangeRateHeight) / _CameraYRate + _ChangeRateHeight, changeHeight);
	cameraPos.y = cameraY;
	FLOAT3 viewDir = normalize(cameraPos - FragData.WorldPosition.xyz);
	FLOAT foamFresnel = pow(1.0 - saturate(dot(MaterialData.NoNoiseWorldNormal, viewDir)), _FoamFresnelPow);
	FLOAT dis = length(_WorldSpaceCameraPos.xyz - FragData.WorldPosition.xyz);
	FLOAT att =  pow(saturate(_FoamAttenDistance / dis), _FoamDistancePow); // 距离相机越远泡沫越弱
	
	FLOAT4 foamMap = SAMPLE_TEX2D(_FoamTex, FragData.WorldPosition.xz * _FoamTexScale);
    return smoothstep(_FoamMin, _FoamMin + _FoamSmooth, saturate(length(foamFresnel * foamMap.rgb))) * att * _FoamIntensity;
}

FLOAT3 ScatterColour(
	in FMaterialData MaterialData,
	in FFragData FragData
	DEBUG_ARGS)
{
	// base colour
	FLOAT v = abs(FragData.CameraVector.y);
	FLOAT grazTerm = pow(abs(1.0 - v), _GrazingPow);
	FLOAT3 col = lerp(_ScatteringColor.xyz, _DiffuseGrazing.xyz, grazTerm);
	FLOAT3 colHigh = lerp(_ScatteringColorHigh.xyz, _DiffuseGrazingHigh.xyz, grazTerm);
	col = lerp(col, colHigh, pow(saturate((FragData.WorldPosition.y -_SDFStartHeight)/(_SDFEndHeight-_SDFStartHeight)),_SDFPower));
    

	FLOAT towardsSun = pow(max(0., dot(_WaterLightPosition.xyz, -FragData.CameraVector)), _SubSurfaceSunFallOff);
	// DEBUG_CUSTOMDATA_PARAM(TowardsSun, towardsSun)
	FLOAT3 subsurface = (_SubSurfaceBase + _SubSurfaceSun * towardsSun) * _SubSurfaceColor.rgb * _MainLightColor.xyz;
	subsurface *= (1.0 - v * v) * 0.1f;
	col += subsurface;
	return col;
}

FLOAT3 SampleNormalMaps(FLOAT2 worldXZUndisplaced,float3 WavePPTex)
{
	//const FLOAT2 v0 = FLOAT2(0.94, 0.34), v1 = FLOAT2(-0.85, -0.53);
	const FLOAT2 v0 = FLOAT2(0.24, -0.34)+_FlowSpeedU, v1 = FLOAT2(-0.25, -0.53)+_FlowSpeedV;
	const FLOAT lodDataGridSize = 0.25;
	FLOAT nstretch = _NormalsScale * lodDataGridSize; // normals scaled with geometry
	const FLOAT spdmulL = 0.8;
    FLOAT3 norm = UnpackNormalMap(SAMPLE_TEX2D(_WaterNormal, (v0*_Time.y*spdmulL + worldXZUndisplaced/nstretch) ))+
	UnpackNormalMap(SAMPLE_TEX2D(_WaterNormal, (v1*_Time.y*spdmulL + worldXZUndisplaced/nstretch*2.132) ));
	
	WavePPTex = WavePPTex * 2 - 1;
	float3 blendNormal;
	blendNormal = float3((norm.x + WavePPTex.x), norm.z, (norm.y + WavePPTex.y));
	blendNormal = normalize(blendNormal);
	return  normalize(lerp(blendNormal,FLOAT3(0,1,0),_NormalsStrength));
}


FLOAT3 SampleNormalMaps(FLOAT2 worldXZUndisplaced)
{
	const FLOAT2 v0 = FLOAT2(0.24, -0.34) + _FlowSpeedU, v1 = FLOAT2(-0.25, -0.53)+_FlowSpeedV;
	const FLOAT lodDataGridSize = 0.25;
	FLOAT nstretch = _NormalsScale * lodDataGridSize; // normals scaled with geometry
	const FLOAT spdmulL = 0.8;
    FLOAT3 norm = UnpackNormalMap(SAMPLE_TEX2D(_WaterNormal, (v0*_Time.y*spdmulL + worldXZUndisplaced/nstretch) ))+
	UnpackNormalMap(SAMPLE_TEX2D(_WaterNormal, (v1*_Time.y*spdmulL + worldXZUndisplaced/nstretch*2.132) ));
	norm = normalize(norm.xzy);
	return  normalize(lerp(norm,FLOAT3(0,1,0),_NormalsStrength));
}


//foam
#define _FoamEnable _Param3.w>0.5
#define _FoamSize _Param4.x
#define _FoamPower _Param4.y
#define _FoamFrequency _Param4.z
#define _FoamSpeed _Param4.w

#define _FoamNoiseUVScale _Param6.x
#define _FoamNoiseUVSpeed _Param6.y
#define _FoamNoiseUVFlowDir _Param6.zw

#define _FoamTexUVScale _Param5.x
#define _FoamTexUVSpeed _Param5.y
#define _FoamTexUVFlowDir _Param5.zw

#define _FoamEdge _Param7.x
#define _FoamMoveDirScale _Param7.y
#define _FoamNoiseScale _Param7.z

// FLOAT4 _Param8;
#define _FoamModulateStartHeight _Param8.x
#define _FoamModulateEndHeight _Param8.y
#define _FoamModulatePower _Param8.z
#define _FoamModulateMultiplier _Param8.w

FLOAT4 SampleFoam(FLOAT2 uv,FLOAT scale,FLOAT speed,FLOAT2 dir)
{
	return SAMPLE_TEX2D(_NoiseMap, (dir*_Time.y*speed + uv/scale) );
}

FLOAT3 OceanFoam(
	in FFragData FragData,
	in FLOAT depth
)
{
	
	FLOAT foam = 0;
	UNITY_BRANCH
	if(_FoamEnable)
	{
		FLOAT2 worldXZUndisplaced = FragData.WorldPosition.xz;
		FLOAT4 noise = SampleFoam(worldXZUndisplaced,_FoamNoiseUVScale,_FoamNoiseUVSpeed,_FoamNoiseUVFlowDir);
		FLOAT4 foamTex = SampleFoam(worldXZUndisplaced,_FoamTexUVScale,_FoamTexUVSpeed,_FoamTexUVFlowDir);

		depth = saturate(1-(depth*_FoamSize ));
		depth = pow(depth,_FoamPower);
		FLOAT mask =  depth;
		FLOAT fm = pow(saturate((FragData.WorldPosition.y -_FoamModulateStartHeight)/(_FoamModulateEndHeight-_FoamModulateStartHeight)),_FoamModulatePower)*_FoamModulateMultiplier;
		FLOAT d = (sin( noise.r*_FoamNoiseScale + depth*PI*_FoamFrequency+_Time.y*_FoamSpeed+FragData.WorldPosition.y* _FoamMoveDirScale+fm));
		d = lerp((d-(1-depth)),1, mask+0.1)*saturate(mask*2);
		foam = saturate(foamTex.a-(1-d));
		d = saturate(d);	
		d = 1 - d;	
		foam = saturate((foamTex.a -d));
		foam = smoothstep(0,_FoamEdge,foam);
	}
	
	return foam.xxx;	
}

float _ColorTexDownSample;
float _PlannarReflectScale;
half3 SamplePlannarReflect(FFragData FragData, FMaterialData MaterialData)
{
	
	return SAMPLE_TEXTURE2D(_ReflectionMap,sampler_ReflectionMap_linear_clamp,FragData.ScreenPosition + _PlannarReflectScale *  (MaterialData.WorldNormal.z * MaterialData.WorldNormal.y));
}

half3 SampleTDSSPR(FFragData FragData, FMaterialData MaterialData)
{
	//half3 positionWS, half3 normalWS, half3 viewDirectionWS, float depth
	half3 positionWS = FragData.WorldPosition.xyz;
	half3 normalWS = MaterialData.WorldNormal;
	half3 viewDirectionWS =  _CameraPos.xyz - positionWS;
	half3 reflectionDirWS = normalize(reflect(-viewDirectionWS, normalWS));
	half3 worldReflectDirOffset = reflectionDirWS * (abs(viewDirectionWS).y * _MarchParam.x + _MarchParam.y) + positionWS;
	half4 reflectPositionCS = TransformWorldToHClip(worldReflectDirOffset);
	reflectPositionCS /= reflectPositionCS.w;

	float2 temp = reflectPositionCS.xy * reflectPositionCS.xy;
	reflectPositionCS.xy = reflectPositionCS.xy * 0.5 + 0.5;
	temp = max(1 - temp * temp, 0);
	FLOAT2 screenUV = FragData.ScreenPosition.xy;
	float scale = ddx(screenUV) + ddy(screenUV);
	scale *= scale;
	float downSample = max(1,_ColorTexDownSample / (100.0*scale));
	#if UNITY_UV_STARTS_AT_TOP
		reflectPositionCS.y = 1 - reflectPositionCS.y;
	#endif
	float screenLerpEdge = temp.x * temp.y;
	float4 ceiledUV;

	ceiledUV = floor(reflectPositionCS * downSample);
	ceiledUV = ceiledUV / downSample;
	half3 opaqueColor = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_ScreenTextures_Trilinear_clamp, ceiledUV.xy).xyz;
	half3 skyBoxCol = GlossyEnvironmentReflection(reflectionDirWS, 0, 1).xyz;
	return lerp(skyBoxCol, opaqueColor, screenLerpEdge);
}

FLOAT CalculateFresnelReflectionCoefficient(FLOAT cosTheta)
{
	// Fresnel calculated using Schlick's approximation
	// See: http://www.cs.virginia.edu/~jdl/bib/appearance/analytic%20models/schlick94b.pdf
	// reflectance at facing angle
	FLOAT R_0 = (_RefractiveIndexOfAir - _RefractiveIndexOfWater) / (_RefractiveIndexOfAir + _RefractiveIndexOfWater); 
	R_0 *= R_0;
	const FLOAT R_theta = R_0 + (1.0 - R_0) * pow(saturate(1.0 - cosTheta), _FresnelPower);
	return R_theta;
}

TEXCUBE_SAMPLER(_Skybox);
float4 _WaveSpecColor;
FLOAT3 ApplyReflectionSky(
	in FFragData FragData,
	in FMaterialData MaterialData, 
	inout FLOAT3 col
	DEBUG_ARGS)
{
	float3 normal = MaterialData.WorldNormal;
	FLOAT3 refl = reflect(-FragData.CameraVector, normal);
	refl.y = max(refl.y, 0.0);

///WavePP spec
	
	#ifdef _PPWave
		float3 Particlerefl = reflect(-FragData.CameraVector, MaterialData.waveParticleWorldNormal);
		float3 specPP = max(0,dot(_WaterLightPosition , MaterialData.waveParticleWorldNormal)) ;
		specPP= saturate(specPP);
	#else
	 	float3 specPP = 0;
	#endif
	FLOAT3 skyCol;

	FLOAT4 skyCube = SAMPLE_TEXCUBE_LOD(_Skybox, refl,7);
	skyCol = skyCube.rgb * _SkyBrightness; 

	skyCol += pow(max(0., dot(refl, _WaterLightPosition.xyz)), _DirectionalLightFallOff) * 
			_DirectionalLightBoost * _MainLightColor.xyz;
	DEBUG_CUSTOMDATA_PARAM(SkyCol, skyCol)
	// Fresnel
	FLOAT R_theta = CalculateFresnelReflectionCoefficient(saturate(dot(normal, FragData.CameraVector)));


	// DEBUG_CUSTOMDATA_PARAM(Rtheta, R_theta)
	half3 reflect;
#ifdef _PlannarReflect
	reflect = SamplePlannarReflect(FragData,MaterialData);	
#else
	#ifdef _Screen_Color
		reflect = SampleTDSSPR(FragData, MaterialData);
	#else
		reflect = 0.5;
	#endif
#endif

	FLOAT3 diffuse = col  + lerp(0,reflect,R_theta);	
	col = skyCol;
	return lerp(min(lerp(diffuse, skyCol, R_theta * _Specular), FLOAT3(3,3,3)), _WaveSpecColor,specPP);
	//return min(lerp(diffuse, skyCol, R_theta * _Specular), FLOAT3(3,3,3)) ;
}

void WaterMaterial(in FFragData FragData,inout FMaterialData MaterialData)
{
	//normal
	FLOAT2 worldXZUndisplaced = FragData.TexCoords[0].xz;
	float3 worldPos = FragData.WorldPosition.xyz;
	float worldWaveuvX = (worldPos.x  - _WaveCameraPos.x + _WaveCameraPos.w) / ((_WaveCameraPos.x + _WaveCameraPos.w) - (_WaveCameraPos.x - _WaveCameraPos.w));
	worldWaveuvX = saturate(worldWaveuvX);
	float worldWaveuvY = (worldPos.z  - _WaveCameraPos.z + _WaveCameraPos.w) / ((_WaveCameraPos.z + _WaveCameraPos.w) - (_WaveCameraPos.z - _WaveCameraPos.w));
	worldWaveuvY = saturate(worldWaveuvY);
	float2 worldWaveUV = float2(worldWaveuvX,worldWaveuvY);
	float border = (ceil(worldWaveuvX - 0.01)) * (ceil (-worldWaveuvX + 0.99)) * (ceil(worldWaveuvY - 0.01))* (ceil(-worldWaveuvY + 0.99));
	FLOAT3 normalDetail; 
	#ifdef _PPWave 
		float3 WavePPTex = 	SAMPLE_TEX2D( _WaveTex,  worldWaveUV) - SAMPLE_TEX2D( _WaveTex, (worldWaveUV ) + float2( 0.01,0.01 ) );
		WavePPTex *= border;
		WavePPTex.xy = WavePPTex.xy * _WaveNormalDensity;
		normalDetail = SampleNormalMaps(worldXZUndisplaced, WavePPTex);
	#else
		normalDetail = SampleNormalMaps(worldXZUndisplaced);
	#endif

	FLOAT3 gridPoint =  FragData.TexCoords[0].xyz;
	FLOAT3 tangent = FLOAT3(1, 0, 0);
	FLOAT3 binormal = FLOAT3(0, 0, 1);
	FLOAT3 p = gridPoint;
	FLOAT time = _Time.y;
	p += GerstnerWave(_ParamA, gridPoint, tangent, binormal,time);
	p += GerstnerWave(_ParamB, gridPoint, tangent, binormal,time);
	p += GerstnerWave(_ParamC, gridPoint, tangent, binormal,time);
	p += GerstnerWave(_Wave1, gridPoint, tangent, binormal,time);
	p += GerstnerWave(_Wave2, gridPoint, tangent, binormal,time);
	p += GerstnerWave(_Wave3, gridPoint, tangent, binormal,time);
	p += GerstnerWave(_Wave4, gridPoint, tangent, binormal,time);
	p += GerstnerWave(_Wave5, gridPoint, tangent, binormal,time);
	p += GerstnerWave(_Wave6, gridPoint, tangent, binormal,time);
	p += GerstnerWave(_Wave7, gridPoint, tangent, binormal,time);
	p += GerstnerWave(_Wave8, gridPoint, tangent, binormal,time);
	p += GerstnerWave(_Wave9, gridPoint, tangent, binormal,time);		

	FLOAT3 normal = normalize(cross(binormal, tangent));
	FLOAT3x3 worldToTangent = FLOAT3x3(tangent, normal, binormal);
	float3x3 tangentToWorld = transpose(worldToTangent);

	#ifdef _PPWave
		float3 mixedNormal = mul(tangentToWorld, float3(WavePPTex.xy,1)).xyz; 
		MaterialData.waveParticleWorldNormal = float3(lerp(0,mixedNormal.xy,WavePPTex.xy),1);
	#endif
	MaterialData.WorldNormal =  mul(tangentToWorld, normalDetail).xyz;
	MaterialData.NoNoiseWorldNormal =  mul(tangentToWorld, FLOAT3(0, 1, 0)).xyz;

	MaterialData.AO = 1;  
}

#define CustomMaterial WaterMaterial
#define _CUSTOM_MATERIAL

void CalcWaterLighting(FFragData FragData, FMaterialData MaterialData,
	FShadowData ShadowData, FLOAT ShadowMask, 
	inout FLOAT3 DirectDiffuse,inout FLOAT3 DirectSpecular DEBUG_ARGS)
{
	FLOAT3 scatterCol = ScatterColour(MaterialData, FragData DEBUG_PARAM);
	
	//浪尖白沫
	FLOAT waveFoam = WaveFoam(MaterialData, FragData);
    scatterCol *= 1 - saturate(waveFoam);
	scatterCol += waveFoam;
	
	DirectDiffuse = scatterCol;		//OceanEmission(FragData, MaterialData.WorldNormal, scatterCol, MaterialData.CustomParam.xyz DEBUG_PARAM);

#if _IgnoreSDF
	FLOAT3 foam = 0;
#else
	FLOAT3 sdfUV = FragData.WorldPosition.xzy-_SDFBox.xzy;
	sdfUV.z *= 2;
	sdfUV += FLOAT3(0.5,0.5,0.5);
	sdfUV *= FLOAT3(_SDFSize,_SDFSize,_SDFLayerCount);
	FLOAT depth = SAMPLE_TEX3D(_SDF,sdfUV).r;
	FLOAT3 foam = OceanFoam(FragData, depth);
	DirectDiffuse += foam;
#endif

	FLOAT3 spec = ApplyReflectionSky(FragData,MaterialData,DirectSpecular DEBUG_PARAM);
	spec *= 1 - saturate(waveFoam);

	//URP|||||||||||||||||||||||||||Shadow
    float4 shadowCoord = TransformWorldToShadowCoord(FragData.WorldPosition.xyz);
	float shadow = MainLightRealtimeShadow(shadowCoord);
	DirectDiffuse *= saturate(shadow+0.4);

	DirectSpecular = spec;
}

#define CustomLighting CalcWaterLighting
#define _CUSTOM_LIGHT



#endif//_WATER
#endif //WATER_EFFECT_INCLUDE