#ifndef UBER_FORWARD_PASS_INCLUDED
#define UBER_FORWARD_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
#include "../NBR/Include/Fog.hlsl"
#include "../Include/OPPInput.hlsl"
#include "UberInput.hlsl"
#include "PS_TerrainBlending.hlsl"
/* Add by: Takeshi
 * Feature: Give Model a vertical gradient color */
#include "VerticalColor.hlsl" 

#include "Assets/Engine/Runtime/Shaders/Shading/Scene/ParkourDistort.hlsl"
/* End Add */

float _shadowMaskSharpness;
float _shadowMaskBias;
float4 _OceanColor;
float _TerrainEnable;//,_BlendDistance

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 texcoord     : TEXCOORD0;
    float2 lightmapUV   : TEXCOORD1;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv                       : TEXCOORD0;
    DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    float3 positionWS               : TEXCOORD2;
#endif

    float3 normalWS                 : TEXCOORD3;
#ifdef _NORMALMAP
    float4 tangentWS                : TEXCOORD4;    // xyz: tangent, w: sign
#endif
    float3 viewDirWS                : TEXCOORD5;

    half4 fogFactorAndVertexLight   : TEXCOORD6; // x: fogFactor, yzw: vertex light

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord              : TEXCOORD7;
#endif

    //////////////////////////  For vertical gradient color  /////////////////////////////////
    half3 positionWSColor           :COLOR;
    //////////////////////////  For vertical gradient color  /////////////////////////////////

    float4 positionCS               : SV_POSITION;
    float4 debug :TEXCOORD10;
    UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
};


inline void InitializeStandardLitSurfaceData(float2 uv, out SurfaceData outSurfaceData)//Add a input "PositionColor" for vertical Gradient color
{
    half4 albedo = Sample2D(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));

#ifdef _NORMALMAP
    half4 bump = Sample2D(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
#else
    half4 bump =  half4(0.5,0.5, _BumpZ, _BumpW);
#endif

    outSurfaceData.alpha = GetAlpha(albedo, _Cutoff);
    outSurfaceData.albedo = albedo.rgb * _BaseColor.rgb;
    outSurfaceData.metallic = GetMetallic(bump.w, _MetallicMin, _MetallicMax);
    outSurfaceData.specular = 0;
    outSurfaceData.smoothness = GetSmoothness(bump.z, _RoughnessMin, _RoughnessMax);
    outSurfaceData.normalTS = GetNormal(bump.xy, _BumpScale);
    outSurfaceData.occlusion = GetOcclusion(bump.w, _OcclusionScale);
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
				float3 up = lerp(normalTS, rippleNormalTS, saturate(step(0.8,inputData.normalWS.y)));
				float3 finalNormal = BlendNormals(normalTS, up);
				inputData.normalWS = TransformTangentToWorld(finalNormal, half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz));
			#endif
		#endif
	}
#endif

    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
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

    //inputData.shadowMask = GetSmartShadow(_MainLightPosition.xyz, inputData.normalWS, float4(inputData.positionWS, 1));
    //inputData.shadowMask = SAMPLE_TEXTURE2D_LIGHTMAP(unity_ShadowMask, samplerunity_ShadowMask, input.lightmapUV);
    //inputData.shadowMask = SAMPLE_TEXTURE2D_LIGHTMAP(unity_ShadowMask, samplerunity_ShadowMask, LIGHTMAP_EXTRA_ARGS_USE);

    // #if defined(LIGHTMAP_ON)
    //     // half4 transformCoords = half4(1, 1, 0, 0);
    //     // float2 uvv = input.lightmapUV * transformCoords.xy + transformCoords.zw;
    //     half4 bakedGITex = SAMPLE_TEXTURE2D(unity_Lightmap, samplerunity_Lightmap, input.lightmapUV);
    //     inputData.shadowMask = bakedGITex.a;
    //     inputData.bakedGI = bakedGITex.rgb * 4.5948;
    // #endif
    
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
	// _shadowMaskSharpness = max(1,_shadowMaskSharpness);
	// inputData.shadowMask = saturate(pow(inputData.shadowMask * (1 + _shadowMaskBias), _shadowMaskSharpness));
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

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);

    // already normalized from normal transform to WS.
    output.normalWS = normalInput.normalWS;
    output.viewDirWS = viewDirWS;
#if defined(_NORMALMAP)
    real sign = input.tangentOS.w * GetOddNegativeScale();
    half4 tangentWS = half4(normalInput.tangentWS.xyz, sign);
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

    //////////////////////////  For vertical gradient color  /////////////////////////////////
    output.positionWSColor = vertexInput.positionWS.rgb; 
    //////////////////////////  For vertical gradient color  /////////////////////////////////

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
// Used in Standard (Physically Based) shader
LitFragmentOutput LitPassFragment(Varyings input)
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
    InitializeStandardLitSurfaceData(input.uv, surfaceData);
    InputData inputData;
    InitializeInputData(input, surfaceData.normalTS, inputData);
    #if defined(TERRAIN_BLENDING_ALBEDO)
    output.color0 = half4(surfaceData.albedo,surfaceData.occlusion);
    output.color1 = half4(inputData.normalWS,surfaceData.smoothness);
    return output;
    #endif

    #if defined(_ENABLE_TERRAIN_BLENDING)
        UNITY_BRANCH
        if(_TerrainOn == 1)
        {
            float2 screenUV = input.positionCS.xy / _ScreenParams.xy;
            //UNITY_BRANCH
            // if(_BlendDistance<=input.positionCS.w)
            // {
                TerrainBlending(screenUV,input.positionCS.w, surfaceData.albedo,surfaceData.occlusion,surfaceData.smoothness, inputData.normalWS);
            // }
  
        }
    #endif
    //#endif

    //return half4(surfaceData.albedo,1);
    //return half4(inputData.normalWS,1);
    //return dot(inputData.normalWS,_MainLightDir0);
//        half realtimeShadow = MainLightRealtimeShadow(inputData.shadowCoord);
//#ifdef _ADDITIONAL_LIGHT_SHADOWS
//        half additionLigthAsShadow = 1;
//        uint pixelLightCount1 = GetAdditionalLightsCount();
//        if (_SpotLightShadowIntensity > 0)
//        {
//            for (uint lightIndex = 0u; lightIndex < pixelLightCount1; ++lightIndex)
//            {
//                Light light = GetAdditionalLight(lightIndex, inputData.positionWS, 1, false);
//                half isShadow = saturate((((light.distanceAttenuation) > 0) * (light.shadowAttenuation < .9)));
//                half atten = isShadow * light.shadowAttenuation + (1 - isShadow);
//                additionLigthAsShadow = min(additionLigthAsShadow, 1 - saturate((1 - atten) * light.distanceAttenuation * _SpotLightShadowIntensity));
//            }
//            realtimeShadow *= additionLigthAsShadow;
//        }
//#endif
//        output.color0 = half4(realtimeShadow, inputData.shadowCoord.xyz);
//        return output; 

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
        half4 color = UniversalFragmentPBR(inputData, surfaceData);//float4( surfaceData.albedo, 1);//
    #endif
#endif
    //half nov = pow(dot(inputData.normalWS,inputData.viewDirectionWS),0.1);
    //output.color0 =  nov;
    //return output;
	if(_IgnoreAlpha)
		color.a = 1;
		
    /* Add by: Takeshi
    * Feature: Give Model a vertical gradient color */
    #ifdef _GRADIENT_ON
        //half4 origion_color = UniversalFragmentPBR(inputData, surfaceData);
        //half4 pbrColor;
        VerticalColor(origion_color,input.positionWSColor,_GradientEnd,_GradientBegin,_WorldPositionOffset.xyz,
                    _WorldPositionScale.xyz,_GradientSwitch,_BlendMode,
                    _GradientScale,_BlendIntensity,color);    //pbrColor->preColor
    //#else
        //half4 pbrColor = UniversalFragmentPBR(inputData, surfaceData);
    #endif
    /* End Add */
    
	#ifdef _ReflectVertex
		clip(_WaterDisturbParam.x - input.positionWS.y);
		color.rgb *= _OceanColor.rgb;
	#endif

    #if defined(_GROUNDCOLORUNENABLE) && defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
    color.a = SAMPLE_SHADOWMASK(input.lightmapUV);
    #endif

    #if !defined(_LIGHTMAPUNENABLE)&& !defined(_GROUNDCOLORUNENABLE)


    // UNITY_BRANCH
    // if (_ScatterEnable)
    // {
    //     float depth01;
    //     float3 scatter = GetScatterRGB(inputData.viewDirectionWS, inputData.positionWS, _WorldSpaceCameraPos, _MainLightDir0.xyz, depth01, _ProjectionParams, _CameraBackward);
    //     // APPLY_FOG(scatter, inputData.positionWS);
    //     color.rgb = lerp(color.rgb, scatter, saturate(depth01 * 2));
    // }     
    APPLY_FOG(color.rgb, input.positionWS.xyz);

    SphereDitherTransparent(input.positionCS, _DitherTransparency);
   
    color.rgb *= _SceneExposure;   
   
    //#ifdef _NEEDIMPOSTOR
    //return 0;
    if(_NeedImpostor==1)
    {
        color.a = 1;
    }
   
    //#endif
    
    #endif
    color.a *= _DistinguishAlpha;   

    #if defined( _SHADER_DEBUG) && defined(_URP_DEBUG)
        half4 debugColor = GetDebugColor(surfaceData, inputData);
        output.color0 = debugColor;
        return output;
    #endif
    output.color0 =  half4(color.rgb, color.a);
    return output;
    //return float4(color.rgb, color.a);
}



#endif
