#ifndef URP_SCENE_WATER_VERTEX_INCLUDE
#define URP_SCENE_WATER_VERTEX_INCLUDE

float4 _UVST0;
sampler2D _GrabRT ;
sampler2D _MainTex ;
float4 _MainTex_ST;
sampler3D _SDF;
float4 _SDFBox;
float4 _SDFParam;
sampler2D _FoamTex;
sampler2D _ProcedureTex0;
samplerCUBE _Skybox;
float4 _Color;
float4 _Color0;
float4 _Color1;
float4 _Color2;
float4 _Color3;

float4 _Param0;
float4 _Param1;
float4 _Param2;
float4 _Param3;
float4 _Param4;
float4 _Param5;
float4 _Param6;
float4 _Param7;
float4 _Param8;
float4 _Param9;
float4 _Param10;
float4 _Param11;

float4 _ParamA;
float4 _ParamB;
float4 _ParamC;
float4 _Wave1;
float4 _Wave2;
float4 _Wave3;
float4 _Wave4;
float4 _Wave5;
float4 _Wave6;
float4 _Wave7;
float4 _Wave8;
float4 _Wave9;
float4 _SteepnessFadeout;
float4 _WaterLightDir;

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


#define _RefractEnable _Param0.w>0.5
#define _RefractionStrength _Param1.x
#define _DepthFogDensity _Param1.yzw
//reflect


//fresnel
#define _Specular _Param2.x
#define _FresnelPower _Param2.y
#define _RefractiveIndexOfAir _Param2.z
#define _RefractiveIndexOfWater _Param2.w

//sky
#define _DirectionalLightFallOff _Param3.x
#define _DirectionalLightBoost _Param3.y
#define _SkyBrightness _Param3.z

#define _SDFLayerCount _SDFParam.x
#define _SDFStartHeight _SDFParam.y
#define _SDFEndHeight _SDFParam.z
#define _SDFPower _SDFParam.w

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

#define _SDFSize _SDFBox.w
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
float3 GerstnerWave(
	float4 wave,
	float3 p, 
	inout float3 tangent, 
	inout float3 binormal, 
	float time) 
{			
		float steepness = wave.z;
		steepness *= saturate(1- pow(abs(length(p.xz-_SteepnessFadeout.xy)/_SteepnessFadeout.z),_SteepnessFadeout.w));
		float wavelength = wave.w;
		float k = TWO_PI / wavelength;
		float c = sqrt(9.8 / k);
		float2 d = normalize(wave.xy);
		float f = k * (dot(d, p.xz) - c * time);
		float a = steepness / k;

		float sin = 0;
		float cos = 0;
		sincos(f,sin,cos);
		tangent += float3(
			-d.x * d.x * (steepness * sin),
			d.x * (steepness * cos),
			-d.x * d.y * (steepness * sin)
		);
		binormal += float3(
			-d.x * d.y * (steepness * sin),
			d.y * (steepness * cos),
			-d.y * d.y * (steepness * sin)
		);
		return float3(
			d.x * (a * cos),
			a * sin,
			d.y * (a * cos)
		);
}

inline float4 ComputeGrabScreenPos (float4 pos) 
{
#if UNITY_UV_STARTS_AT_TOP
    float scale = -1.0;
#else//!UNITY_UV_STARTS_AT_TOP
    float scale = 1.0;
#endif//UNITY_UV_STARTS_AT_TOP
    float4 o = pos * 0.5f;
    o.xy = float2(o.x, o.y*scale) + o.w;
    o.zw = pos.zw;
    return o;
}

Waterv2f vert(WaterInputData v)
{
	Waterv2f o;
	float3 WorldPosition = TransformObjectToWorld(v.vertex);
	float3 gridPoint = WorldPosition.xyz;
	float3 tangent = float3(1, 0, 0);
	float3 binormal = float3(0, 0, 1);
	float3 p = gridPoint;
	float time = _Time.y;
	float3 worldPos =  WorldPosition.xyz;

	#ifdef _PPWave
		float worldWaveuvX = (worldPos.x  - _WaveCameraPos.x + _WaveCameraPos.w) / ((_WaveCameraPos.x + _WaveCameraPos.w) - (_WaveCameraPos.x - _WaveCameraPos.w));
		worldWaveuvX = saturate(worldWaveuvX);
		float worldWaveuvY = (worldPos.z  - _WaveCameraPos.z + _WaveCameraPos.w) / ((_WaveCameraPos.z + _WaveCameraPos.w) - (_WaveCameraPos.z - _WaveCameraPos.w));
		worldWaveuvY = saturate(worldWaveuvY);
		float2 worldWaveUV = float2(worldWaveuvX,worldWaveuvY);
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
	float3 normal = normalize(cross(binormal, tangent));

	float4 projPos = TransformWorldToHClip(p);
	o.worldPosition = float4(p,1);

	
	o.normal.xyz =tangent.y;//TransformObjectToWorldNormal(normal);
    o.tangent.xyz = TransformObjectToWorldDir(tangent.xyz);
    o.bitangent.xyz = cross(o.normal.xyz, o.tangent.xyz) ;

	float3 viewDirWS = _WorldSpaceCameraPos.xyz - o.worldPosition.xyz;
	o.normal.w = viewDirWS.x;
	o.tangent.w = viewDirWS.y;
	o.bitangent.w = viewDirWS.z;

	o.Texcoord[0] = WorldPosition;

	float3 vpos = mul(unity_MatrixV, WorldPosition).xyz;
	o.depth01 = projPos.z / projPos.w;


#ifdef _SCREEN_POS
	float3 pos = ComputeScreenPos(projPos).xyw;
	o.screenPosition.xy = pos.xy;
	o.screenPositionW.x = pos.z;
	float3 posGrab = ComputeGrabScreenPos(projPos).xyw;
	o.screenPosition.zw = posGrab.xy;
	o.screenPositionW.y = posGrab.z;
#endif//_SCREEN_POS
	
	o.position = projPos;
	return o;
}


#endif //WATER_VERTEX_INCLUDE