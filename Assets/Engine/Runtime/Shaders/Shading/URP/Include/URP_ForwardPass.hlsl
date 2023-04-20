#ifndef URP_FORWARD_PASS
#define URP_FORWARD_PASS

#include "../../NBR/Include/Fog.hlsl"

#ifndef VS_ADDITION
#define VS_ADDITION
#endif

float _shadowMaskSharpness;
float _shadowMaskBias;

#ifndef URP_InitializeSurfaceData

inline void InitializeBaseSurfaceData(float2 uv, out SurfaceData outSurfaceData)
{
    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    outSurfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff);

    half4 specGloss = SampleMetallicSpecGloss(uv, albedoAlpha.a);
    outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;

#if _SPECULAR_SETUP
    outSurfaceData.metallic = 1.0h;
    outSurfaceData.specular = specGloss.rgb;
#else
    outSurfaceData.metallic = specGloss.r;
    outSurfaceData.specular = half3(0.0h, 0.0h, 0.0h);
#endif

    outSurfaceData.smoothness = specGloss.a;
    outSurfaceData.normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
    outSurfaceData.occlusion = SampleOcclusion(uv);
    outSurfaceData.emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));

#if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
    half2 clearCoat = SampleClearCoat(uv);
    outSurfaceData.clearCoatMask = clearCoat.r;
    outSurfaceData.clearCoatSmoothness = clearCoat.g;
#else
    outSurfaceData.clearCoatMask = 0.0h;
    outSurfaceData.clearCoatSmoothness = 0.0h;
#endif

#if defined(_DETAIL)
    half detailMask = SAMPLE_TEXTURE2D(_DetailMask, sampler_DetailMask, uv).a;
    float2 detailUv = uv * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
    outSurfaceData.albedo = ApplyDetailAlbedo(detailUv, outSurfaceData.albedo, detailMask);
    outSurfaceData.normalTS = ApplyDetailNormal(detailUv, outSurfaceData.normalTS, detailMask);

#endif
}
#define URP_InitializeSurfaceData(uv, surfaceData) InitializeBaseSurfaceData(uv, surfaceData)
#endif // URP_InitializeSurfaceData


#ifndef URP_InitializeInputData
void InitializeInputDataBase(Varyings input, half3 normalTS, out InputData inputData)
{
    inputData = (InputData)0;

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    inputData.positionWS = input.positionWS;
#endif

    half3 viewDirWS = SafeNormalize(input.viewDirWS);
#if defined(_NORMALMAP) || defined(_DETAIL)
    float sgn = input.tangentWS.w;      // should be either +1 or -1
    float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
    float3x3 tbn = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);
    inputData.normalWS = TransformTangentToWorld(normalTS, tbn);
#else
    inputData.normalWS = input.normalWS;
#endif

#if defined(_RAIN)
    UNITY_BRANCH
    if(_Ripples > 0.5)
    {
		#if defined(_SHADER_LEVEL_HIGH) || defined(_SHADER_LEVEL_VERY_HIGH)
			float4 rippleNormalWS = ComputeRippleWS(input.positionWS);
			#if defined(_NORMALMAP)
				float3 rippleNormalTS = mul(rippleNormalWS.xyz,tbn);
				rippleNormalTS=lerp(float3(0,0,1), rippleNormalTS.xyz, rippleNormalWS.w);
				rippleNormalTS.xy *= _RippleIntensity;
				rippleNormalTS = normalize(rippleNormalTS);
			//	normalTS.xy *= _NormalTSScale;
			//	normalTS = normalize(normalTS);
				float3 up = normalize(lerp(normalTS, rippleNormalTS, saturate(step(0.8, inputData.normalWS.y))));
				float3 finalNormal = BlendNormals(normalTS, up);
				inputData.normalWS = TransformTangentToWorld(finalNormal, tbn);
			#else
				float3 up = lerp(inputData.normalWS.xyz, rippleNormalWS.xyz, saturate(step(0.8, inputData.normalWS.y)));
				inputData.normalWS = lerp( up,rippleNormalWS.xyz,rippleNormalWS.w);
			#endif
		#else
			#if defined(_NORMALMAP)
				float3 rippleNormalTS = ComputeRippleTS(input.positionWS.xz, _Time.y);
				rippleNormalTS.xy *= _RippleIntensity;
			//	normalTS.xy *= _NormalTSScale;
				normalTS = normalize(normalTS);
				float3 up = lerp(normalTS, rippleNormalTS, saturate(step(0.8, inputData.normalWS.y)));
				float3 finalNormal = BlendNormals(normalTS, up);
				inputData.normalWS = TransformTangentToWorld(finalNormal, half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz));
			#endif
		#endif
    }
#else
    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
#endif

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
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);

    //暂时注释掉合并光照贴图的采样方式，等找到合适的压缩方法在启用
    // #if defined(LIGHTMAP_ON) 
    // // half4 transformCoords = half4(1, 1, 0, 0);
    // // float2 uvv = input.lightmapUV * transformCoords.xy + transformCoords.zw;
    // half4 bakedGITex = SAMPLE_TEXTURE2D(unity_Lightmap, samplerunity_Lightmap, input.lightmapUV);
    // inputData.shadowMask = bakedGITex.a;
    // inputData.bakedGI = bakedGITex.rgb * 4.5948;
    // #endif
	// _shadowMaskSharpness = max(1,_shadowMaskSharpness);
	// inputData.shadowMask = saturate(pow(inputData.shadowMask * (1 + _shadowMaskBias), _shadowMaskSharpness));
}

#define URP_InitializeInputData(input,normalTS, inputData)  InitializeInputDataBase(input, normalTS, inputData);
#endif  //URP_InitializeInputData

//----------------------------------------------------------------------------
Varyings Vertex(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs vertexInput = (VertexPositionInputs)0;//GetVertexPositionInputs(input.positionOS.xyz);
	vertexInput.positionWS = TransformObjectToWorld(input.positionOS.xyz);
	UNITY_BRANCH
    if(_IsParkour == 1 && _Parkout == 1)
    {
        vertexInput.positionWS = ParkourDistortVertex(vertexInput.positionWS);
    }
	vertexInput.positionCS = TransformWorldToHClip(vertexInput.positionWS);
	
    // normalWS and tangentWS already normalize.
    // this is required to avoid skewing the direction during interpolation
    // also required for per-vertex lighting and SH evaluation
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
    half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

    output.uv = TRANSFORM_TEX(input.texcoord, _MainTex);

    // already normalized from normal transform to WS.
    output.normalWS = normalInput.normalWS;
    output.viewDirWS = viewDirWS;
#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR) || defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    real sign = input.tangentOS.w * GetOddNegativeScale();
    half4 tangentWS = half4(normalInput.tangentWS.xyz, sign);
#endif
#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
    output.tangentWS = tangentWS;
#endif

#if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    half3 viewDirTS = GetViewDirectionTangentSpace(tangentWS, output.normalWS, viewDirWS);
    output.viewDirTS = viewDirTS;
#endif

    OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
    OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

    output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    output.positionWS = vertexInput.positionWS;
#endif

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    output.shadowCoord = GetShadowCoord(vertexInput);
#endif

    output.positionCS = vertexInput.positionCS;

    VS_ADDITION
    
    //For vertical gradient color
    output.positionWSColor = vertexInput.positionWS.rgb; 

    return output;
}

struct LitFragmentOutput
{
    #if defined(TERRAIN_BLENDING_ALBEDO)
    half4 color0: SV_Target0;
    half4 color1: SV_Target1;
    #else
    half4 color0: SV_Target0;
    #endif
};

//----------------------------------------------------------------------------
LitFragmentOutput Fragment(Varyings input)
{
    LitFragmentOutput output = (LitFragmentOutput)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
#if defined(_PARALLAXMAP)
#if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    half3 viewDirTS = input.viewDirTS;
#else
    half3 viewDirTS = GetViewDirectionTangentSpace(input.tangentWS, input.normalWS, input.viewDirWS);
#endif
    ApplyPerPixelDisplacement(viewDirTS, input.uv);
#endif

    SurfaceData surfaceData;
    URP_InitializeSurfaceData(input.uv, surfaceData)
    InputData inputData;
    URP_InitializeInputData(input, surfaceData.normalTS, inputData)
    #if defined(TERRAIN_BLENDING_ALBEDO)
    output  = (LitFragmentOutput)0;
    output.color0 = half4(surfaceData.albedo,surfaceData.occlusion);
    output.color1 = half4(inputData.normalWS,surfaceData.smoothness);
    return output;
    #endif
    
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
        half4 color = LowLevelLambert(inputData, surfaceData.albedo, surfaceData.emission, surfaceData.alpha, 0);
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
        half4 color = UniversalFragmentPBR(inputData, surfaceData);//float4( surfaceData.albedo, 1);//
    #endif
#endif

    color.a = OutputAlpha(color.a, _Surface);

    #if defined(_GROUNDCOLORUNENABLE) && defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
        color.a = SAMPLE_SHADOWMASK(input.lightmapUV);
    #endif

    #if !defined(_LIGHTMAPUNENABLE) && !defined(_GROUNDCOLORUNENABLE)

    // UNITY_BRANCH
    // if (_ScatterEnable)
    // {
    //     float depth01;
    //     float3 scatter = GetScatterRGB(normalize(input.positionWS - _WorldSpaceCameraPos), input.positionWS, _WorldSpaceCameraPos, _MainLightDir0.xyz, depth01, _ProjectionParams, _CameraBackward);
    //     // APPLY_FOG(scatter, inputData.positionWS);
    //     color.rgb = lerp(color.rgb, scatter, saturate(depth01 * 2));
    // }     
    APPLY_FOG(color.rgb, input.positionWS);

    SphereDitherTransparent(input.positionCS, _DitherTransparency);

    color.rgb *=  _SceneExposure;
    
    #endif

    #if defined( _SHADER_DEBUG) && defined(_URP_DEBUG)
    half4 debugColor = GetDebugColor(surfaceData, inputData);
    output.color0 =  debugColor;
    return output;
    #endif

    output.color0 = color;
    return output;
}
#endif //URP_FORWARD_PASS