#ifndef DECAL_FORWARD2_INCLUDE
#define DECAL_FORWARD2_INCLUDE

#include "URP_Scene_DecalInput.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

#include "../../NBR/Include/Fog.hlsl"
#include "../../Include/OPPInput.hlsl"

#include "Assets/Engine/Runtime/Shaders/Shading/Scene/ParkourDistort.hlsl"
#include "../../Include/ShaderDebug.hlsl"

struct Attributes
{
    float4 positionOS : POSITION;
    float2 texcoord : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);
    float3 positionWS : TEXCOORD2;
    float3 normalWS : TEXCOORD3;
    float4 viewDirWS : TEXCOORD4;
    float4 screenPos : TEXCOORD5;
    half4 fogFactorAndVertexLight : TEXCOORD6; // x: fogFactor, yzw: vertex light
    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord              : TEXCOORD7;
    #endif

	#if defined(_DECAL_DEBUG)
	float3 positionOS : TEXCOORD8;
	#endif

    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};


inline void InitializeStandardLitSurfaceData(float2 uv, out SurfaceData outSurfaceData) //Add a input "PositionColor" for vertical Gradient color
{
    half4 albedo = Sample2D(uv, TEXTURE2D_ARGS(_BaseTex, sampler_BaseTex));


    #ifdef _NORMALMAP
    half4 bump = Sample2D(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
    #else
    half4 bump =  half4(0.5,0.5, _BumpZ, _BumpW);
    #endif


    outSurfaceData.alpha = GetAlpha(albedo, _Cutoff) * _AlphaAdjust;
    outSurfaceData.albedo = albedo.rgb * _BaseColor.rgb * _BaseColorInstensity;
    outSurfaceData.albedo = pow(saturate(outSurfaceData.albedo.rgb + 0.00001), _BaseColor.w);
    outSurfaceData.metallic = GetMetallic(bump.w, _MetallicMin, _MetallicMax);
    outSurfaceData.specular = 0;
    outSurfaceData.smoothness = GetSmoothness(bump.z, _RoughnessMin, _RoughnessMax);
    outSurfaceData.normalTS = GetNormal(bump.xy, _BumpScale);
    outSurfaceData.occlusion = 1; //GetOcclusion(bump.w, _OcclusionScale);
    outSurfaceData.clearCoatMask = 0.0h;
    outSurfaceData.clearCoatSmoothness = 0.0h;
    
	#if defined(_RAIN)
        outSurfaceData.darkValueInRain = _darkValueInRain;
    #else
        outSurfaceData.darkValueInRain = 1;
    #endif


    /* Add by: Takeshi
    * Issue:  Material which with Emission will blink in wrong place when it is far away from Camera
    * cause:  Emission will overflow to Non-Emission area when texture in a low LOD (Mip Map)
    * Cope:   Resample texture for Emission and give it a fixed LOD
    * Date:   21/9/8
    */
    #ifdef _EMISSION_ON
    half4 emission = SAMPLE_TEXTURE2D_LOD(_BaseMap, sampler_BaseMap, uv, 0);
    outSurfaceData.emission = GetEmission(emission, _EmissionColor);
    #else
    outSurfaceData.emission = GetEmission(albedo, _EmissionColor);
    #endif
    /* End Change */
}

void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
{
    inputData = (InputData)0;
    inputData.positionWS = input.positionWS;

    half3 viewDirWS = SafeNormalize(input.viewDirWS.xyz);

    // z/y axis swap for normal to decal space, Unity is column major
    float3x3 martix = (float3x3)GetWorldToObjectMatrix();
    
    float3 temp = martix[1];
    temp = normalize(temp);
    martix[1] = martix[2];
    martix[2] = temp;
    inputData.normalWS = mul(normalTS, martix);

    
    #if defined(_RAIN)
		#if defined(_SHADER_LEVEL_HIGH) || defined(_SHADER_LEVEL_VERY_HIGH)
			float4 rippleNormalWS = ComputeRippleWS(input.positionWS);
			float3 rippleNormalTS = mul(rippleNormalWS.xyz,martix);
			rippleNormalTS = lerp(float3(0,0,1), rippleNormalTS.xyz, rippleNormalWS.w);
			rippleNormalTS.xy *= _RippleIntensity;
		#else
			float3 rippleNormalTS = ComputeRippleTS(input.positionWS.xz, _Time.y);
			rippleNormalTS.xy *= _RippleIntensity;
		#endif	
			rippleNormalTS = normalize(rippleNormalTS);
		//	normalTS.xy *= _NormalTSScale;
			normalTS = normalize(normalTS);
			float3 up = lerp(normalTS, rippleNormalTS, saturate(step(0.8,inputData.normalWS.y)));
			float3 finalNormal = BlendNormals(normalTS, up);
			inputData.normalWS = mul(finalNormal, martix);// TransformTangentToWorld(finalNormal, half3x3(input.tangentWS.xyz, bitangent.xyz, input.norm
    #endif


    inputData.viewDirectionWS = viewDirWS;
    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);

    inputData.fogCoord = input.fogFactorAndVertexLight.x;
    inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
    
    float4 lightClipPos = mul(_BakeLightmapProjection, half4(input.positionWS, 1));
    lightClipPos.xyz = lightClipPos.xyz / lightClipPos.w;
    float2 buttomuv = lightClipPos.xy * 0.5f + 0.5f;
    half4 lightmapcolor = Sample2D(buttomuv, TEXTURE2D_ARGS(_CustomLightmap, sampler_CustomLightmap));
    half4 groundcolor = Sample2D(buttomuv, TEXTURE2D_ARGS(_CustomGroundColor, sampler_CustomGroundColor));
    inputData.shadowMask = groundcolor.a;
    inputData.bakedGI = lightmapcolor.rgb;

    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
}

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

// Used in Standard (Physically Based) shader
Varyings LitPassVertex(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs vertexInput = (VertexPositionInputs)0;//GetVertexPositionInputs(input.positionOS.xyz);
	vertexInput.positionWS = TransformObjectToWorld(input.positionOS.xyz);
	UNITY_BRANCH
	if(_IsParkour==1 && _Parkout == 1)
	{
		vertexInput.positionWS = ParkourDistortVertex(vertexInput.positionWS);
	}
	vertexInput.positionCS = TransformWorldToHClip(vertexInput.positionWS);

    half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
    half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
    
    output.viewDirWS.xyz = viewDirWS;
    output.viewDirWS.w = mul(GetWorldToViewMatrix(), float4(viewDirWS.xyz, 0)).z;

    #if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    half3 viewDirTS = GetViewDirectionTangentSpace(tangentWS, output.normalWS, viewDirWS);
    output.viewDirTS = viewDirTS;
    #endif

    OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

    output.fogFactorAndVertexLight = half4(fogFactor, half3(0, 0, 0));
    output.positionCS = vertexInput.positionCS;

	#if defined(_DECAL_DEBUG)
		output.positionOS = input.positionOS;
	#endif
	
    output.screenPos = ComputeScreenPos(output.positionCS);

    return output;
}


// Used in Standard (Physically Based) shader
half4 LitPassFragment(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    half2 screenUV = input.screenPos.xy / input.screenPos.w;
    float sceneDepth = SampleSceneDepth(screenUV).r;
    float linearSceneDepth = LinearEyeDepth(sceneDepth, _ZBufferParams);
    half radio = -linearSceneDepth / input.viewDirWS.w;
    float3 scenePosWS = GetCameraPositionWS().xyz + radio * input.viewDirWS.xyz;

    //Decal Space
    float3 scenePosDS = TransformWorldToObject(scenePosWS);
	
    clip(0.5 - abs(scenePosDS.xyz));

	#if defined(_DECAL_DEBUG)
		float3 positionOS = abs(input.positionOS);
		float3 debugColor = step(0.499, positionOS);
		return half4(debugColor, 1);
	#endif
	
    input.positionWS = scenePosWS;
    input.uv = scenePosDS.xz + 0.5;
    input.uv = input.uv * _BaseTex_ST.xy + _BaseTex_ST.zw;

    SurfaceData surfaceData;
    InitializeStandardLitSurfaceData(input.uv, surfaceData);


    InputData inputData;
    InitializeInputData(input, surfaceData.normalTS, inputData);

	float NdotV = dot(inputData.viewDirectionWS, inputData.normalWS);
	float t = step(NdotV, -0.01);
	inputData.viewDirectionWS.xyz *= lerp(1, -1, t);

    #if defined(_RAIN)
		surfaceData.smoothness = 1 - _RainRoughness;
		#if defined(_SHADER_LEVEL_LOW) || defined(_SHADER_LEVEL_VERY_LOW)
			 half4 color =  LowLevelBlinnPhong(inputData, surfaceData.albedo, surfaceData.emission, surfaceData.alpha);
		#else
		//	half4 color = LightweightFragmentBlinnPhong(inputData, surfaceData.albedo, float4(surfaceData.specular,1), surfaceData.smoothness, surfaceData.emission, surfaceData.alpha);
			half4 color = UniversalFragmentPBR(inputData, surfaceData);
		#endif
	#elif defined(_LIGHTMAPUNENABLE)
		#if defined(_SHADER_LEVEL_LOW) || defined(_SHADER_LEVEL_VERY_LOW)
			 half4 color = LowLevelLambert(inputData, surfaceData.albedo, surfaceData.emission, surfaceData.alpha, surfaceData.metallic);
		#else
			 half4 color = UniversalFragmentPBR(inputData, surfaceData);//float4( surfaceData.albedo, 1);//
		#endif
	#elif defined(_GROUNDCOLORUNENABLE)
		#if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
			 half shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
			 half4 color = half4(surfaceData.albedo, shadowMask);
		#else
			 half4 color = half4(surfaceData.albedo, 0);
		#endif
	#else
		#if defined(_SHADER_LEVEL_LOW) || defined(_SHADER_LEVEL_VERY_LOW)
			 half4 color = LowLevelLambert(inputData, surfaceData.albedo, surfaceData.emission, surfaceData.alpha, surfaceData.metallic);
		#else
			half4 color = UniversalFragmentPBR(inputData, surfaceData); //float4( surfaceData.albedo, 1);//
		#endif
    #endif


    #ifdef _ReflectVertex
		clip(_WaterDisturbParam.x - input.positionWS.y);
		color.rgb *= _OceanColor.rgb;
    #endif

    #if !defined(_LIGHTMAPUNENABLE)&& !defined(_GROUNDCOLORUNENABLE)

    APPLY_FOG(color.rgb, input.positionWS.xyz);

    SphereDitherTransparent(input.positionCS, _DitherTransparency);

    color.rgb *= _SceneExposure;

    #endif

	
    #if defined( _SHADER_DEBUG) && defined(_URP_DEBUG)
    half4 debugColor = GetDebugColor(surfaceData, inputData);
    return debugColor;
    #endif

    return color;
}

#endif
