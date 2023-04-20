#include "../Include/Fog.hlsl"
#ifndef URP_SCENE_WATER_PIXEL_INCLUDE
#define URP_SCENE_WATER_PIXEL_INCLUDE

float _DitherTransparency;
uniform float _ImpostorAlpha;


float3 ScatterColour(float3 cameraVector,float3 worldPosition,in float depth )
{
	// base colour
	float v = abs(cameraVector.y);
	float grazTerm = pow(abs(1.0 - v), _GrazingPow);

	float3 col = lerp(_ScatteringColor.xyz, _DiffuseGrazing.xyz, grazTerm);
	float3 colHigh = lerp(_ScatteringColorHigh.xyz, _DiffuseGrazingHigh.xyz, grazTerm);
	col = lerp(col, colHigh, pow(saturate((worldPosition.y -_SDFStartHeight)/(_SDFEndHeight-_SDFStartHeight)),_SDFPower));
	float towardsSun = pow(max(0., dot(_WaterLightDir.xyz, -cameraVector)), _SubSurfaceSunFallOff);
	float3 subsurface = (_SubSurfaceBase + _SubSurfaceSun * towardsSun) * _SubSurfaceColor.rgb * _MainLightColor0.xyz;
	subsurface *= (1.0 - v * v) * 0.1f;
	col += subsurface;
	return col;
}


float WaveFoam(float3 worldPosition,float3 noNoiseWorldNormal)
{
	
	float3 cameraPos = _WorldSpaceCameraPos.xyz;
	float cameraY = cameraPos.y;
	float changeHeight = step(_ChangeRateHeight, cameraY); //相机高度超过_ChangeRateHeight，降低视角变化速率
	cameraY = lerp(max(_MinCameraY, cameraY), (cameraY - _ChangeRateHeight) / _CameraYRate + _ChangeRateHeight, changeHeight);
	cameraPos.y = cameraY;
	float3 viewDir = normalize(cameraPos - worldPosition.xyz);
	float foamFresnel = pow(1.0 - saturate(dot(noNoiseWorldNormal, viewDir)), _FoamFresnelPow);
	float dis = length(_WorldSpaceCameraPos.xyz - worldPosition.xyz);
	float att =  pow(saturate(_FoamAttenDistance / dis), _FoamDistancePow); // 距离相机越远泡沫越弱
	
	float4 foamMap = tex2D(_FoamTex, worldPosition.xz * _FoamTexScale);
    return smoothstep(_FoamMin, _FoamMin + _FoamSmooth, saturate(length(foamFresnel * foamMap.rgb))) * att * _FoamIntensity;
}

float3 OceanEmission(
	Waterv2f o,
	in const float3 normal, 
	in const float3 scatterCol)
{

	float3 col = scatterCol;
	UNITY_BRANCH
	if(_RefractEnable)
	{
		float vertexZ =  LinearEyeDepth(o.depth01,_ZBufferParams);
		float2 screenUV = o.screenPosition.xy;
		float sceneZ01 = tex2D(_GrabRT, screenUV).a;
		float sceneZ = LinearEyeDepth(sceneZ01,_ZBufferParams);

		const float2 uvBackground = o.screenPosition.zw;    
		const float2 refractOffset = _RefractionStrength* normal.xz * min(1.0, 0.5*(sceneZ - vertexZ)) / sceneZ;
		const float sceneZRefract = LinearEyeDepth(1-tex2D(_GrabRT, screenUV + refractOffset).a,_ZBufferParams);
		float depthFogDistance;
		float2 uvBackgroundRefract;
		UNITY_BRANCH
		if (sceneZRefract > vertexZ)
		{
			depthFogDistance = sceneZRefract - vertexZ;
			uvBackgroundRefract = uvBackground + refractOffset;
		}
		else
		{
			depthFogDistance = max(vertexZ - sceneZ, 0.0);
			uvBackgroundRefract = uvBackground;
		}
		
		float3 sceneCol = tex2D(_GrabRT, uvBackground.xy+refractOffset).xyz;
		float3 alpha = 1.0 - exp(-_DepthFogDensity.xyz * depthFogDistance);
		col = lerp(sceneCol, col, alpha);
	}
	return col;
}

inline float3 UnpackNormalMap(float4 packednormal)
{
	return packednormal.xyz * 2 - 1;
}

float3 SampleNormalMaps(float2 worldXZUndisplaced)
{
	//const float2 v0 = float2(0.94, 0.34), v1 = float2(-0.85, -0.53);
	const float2 v0 = float2(0.24, -0.34) + _FlowSpeedU;
	float2 v1 = float2(-0.25, -0.53) + _FlowSpeedV;
	const float lodDataGridSize = 0.25;//_GeomData.x;
	float nstretch = _NormalsScale * lodDataGridSize; // normals scaled with geometry
	const float spdmulL = 0.8;//_GeomData.y;
	float3 norm = UnpackNormalMap(tex2D(_WaterNormal, (v0*_Time.y*spdmulL + worldXZUndisplaced/nstretch) ))+
	UnpackNormalMap(tex2D(_WaterNormal, (v1*_Time.y*spdmulL + worldXZUndisplaced/nstretch*2.132) ));
	
	norm = normalize(norm);
	norm = norm.xzy;
	//norm.y *= -1;
	// approximate combine of normals. would be better if normals applied in local frame.
	return  normalize(lerp(norm,float3(0,1,0),_NormalsStrength));
}

float4 SampleFoam(float2 uv,float scale,float speed,float2 dir)
{
	return tex2D(_NoiseMap, (dir*_Time.y*speed + uv/scale) );
}

float3 OceanFoam(
	in float3 worldPosition,
	in float depth
)
{
	
	float foam = 0;
	UNITY_BRANCH
	if(_FoamEnable)
	{
		float2 worldXZUndisplaced = worldPosition.xz;
		float4 noise = SampleFoam(worldXZUndisplaced,_FoamNoiseUVScale,_FoamNoiseUVSpeed,_FoamNoiseUVFlowDir);
		float4 foamTex = SampleFoam(worldXZUndisplaced,_FoamTexUVScale,_FoamTexUVSpeed,_FoamTexUVFlowDir);

		depth = saturate(1-(depth*_FoamSize ));
		depth = pow(depth,_FoamPower);
		float mask =  depth;
		float fm = pow(saturate((worldPosition.y -_FoamModulateStartHeight)/(_FoamModulateEndHeight-_FoamModulateStartHeight)),_FoamModulatePower)*_FoamModulateMultiplier;
		float d = (sin( noise.r*_FoamNoiseScale + depth*PI*_FoamFrequency+_Time.y*_FoamSpeed+worldPosition.y* _FoamMoveDirScale+fm));
		d = lerp((d-(1-depth)),1, mask+0.1)*saturate(mask*2);
		foam = saturate(foamTex.a-(1-d));
		d = saturate(d);	
		d = 1 - d;	
		foam = saturate((foamTex.a -d));
		foam = smoothstep(0,_FoamEdge,foam);
	}
	
	return foam.xxx;	
}

float CalculateFresnelReflectionCoefficient(float cosTheta)
{
	float R_0 = (_RefractiveIndexOfAir - _RefractiveIndexOfWater) / (_RefractiveIndexOfAir + _RefractiveIndexOfWater); 
	R_0 *= R_0;
	const float R_theta = R_0 + (1.0 - R_0) * pow(saturate(1.0 - cosTheta), _FresnelPower);
	return R_theta;
}

float3 ApplyReflectionSky(
	in float3 CameraVector,
	in float3 normal, 
	inout float3 col)
{
	
	float3 refl = reflect(-CameraVector, normal);
	refl.y = max(refl.y, 0.0);
	float4 skyCube = texCUBElod(_Skybox, float4(refl,7));
	float3 skyCol = skyCube.rgb* _SkyBrightness; 
	skyCol += pow(max(0., dot(refl, _WaterLightDir.xyz)), _DirectionalLightFallOff) * 
			_DirectionalLightBoost * _MainLightColor.xyz;

	// Fresnel
	float R_theta = CalculateFresnelReflectionCoefficient(saturate(dot(normal, CameraVector)));

	float reflect = 0.5;
	float3 diffuse = col  + lerp(0,reflect,R_theta);//此处是bug
	col = skyCol;
	return min(lerp(diffuse, skyCol, R_theta * _Specular),float3(3,3,3));
}


float4 Frag(in Waterv2f IN) :SV_TARGET
{

	float3 gridPoint = IN.worldPosition;
	float3 tangent =float3(1, 0, 0);//IN.tangent.xyz;
	float3 binormal =  float3(0, 0, 1);//IN.bitangent.xyz;
	float3 p = gridPoint;
	float time = _Time.y;
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
	float3x3 worldToTangent = float3x3(tangent, normal, binormal);
	float3x3 tangentToWorld = transpose(worldToTangent);

	float3 CameraVector = SafeNormalize(float3(IN.normal.w, IN.tangent.w, IN.bitangent.w));
	float3 sdfUV = IN.worldPosition.xzy-_SDFBox.xzy;
	sdfUV.z *= 2;
	sdfUV += float3(0.5,0.5,0.5);
	sdfUV *= float3(_SDFSize,_SDFSize,_SDFLayerCount);
	float depth = tex3D(_SDF,sdfUV).r;
	float3 scatterCol = ScatterColour(CameraVector, IN.worldPosition, depth);
	float3 noNoiseWorldNormal = mul(tangentToWorld, float3(0, 1, 0)).xyz;

	//浪尖白沫
	float waveFoam = WaveFoam(IN.worldPosition, noNoiseWorldNormal);
    scatterCol *= 1 - saturate(waveFoam);
	scatterCol += waveFoam;
	
	float2 worldXZUndisplaced = IN.worldPosition.xz;
	float3 normalDetail = SampleNormalMaps(worldXZUndisplaced);
	float3 tangentworldNormal = mul(transpose(worldToTangent), normalDetail).xyz;
	float3 DirectDiffuse = float3(0, 0, 0);
	float3 DirectSpecular = float3(0, 0, 0);
	float3 Color = float3(0, 0, 0);

	DirectDiffuse =scatterCol;// OceanEmission(IN, worldNormal, scatterCol);

	
	float3 foam = OceanFoam(IN.worldPosition,depth);
	DirectDiffuse += foam;
	float3 spec = ApplyReflectionSky(CameraVector, tangentworldNormal,DirectSpecular);
	spec *= 1 - saturate(waveFoam);
	float4 shadowCoord = TransformWorldToShadowCoord(IN.worldPosition.xyz);
	float shadow = MainLightRealtimeShadow(shadowCoord);
	DirectSpecular = spec;
	DirectDiffuse *= saturate(shadow+0.4);
	float3 color = DirectSpecular + DirectDiffuse;
	return float4(color,1);
	//return float4(normal.xxx,1);
	
}	

#endif 

float _Cutout;
half4 fragForwardPreDepth(in Waterv2f IN) : SV_Target
{
	REAL2 uv = IN.Texcoord[0].xy;
	float4 color =  tex2D(_MainTex, uv);
	//clip(color.a - _Cutout);
	return 0;
}