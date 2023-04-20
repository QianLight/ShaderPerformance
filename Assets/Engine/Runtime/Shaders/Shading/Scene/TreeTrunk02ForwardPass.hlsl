#ifndef UBER_FORWARD_PASS_INCLUDED
#define UBER_FORWARD_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
#include "../NBR/Include/Fog.hlsl"
#include "../Include/SmartShadow.hlsl"
#include "Assets/Engine/Runtime/Shaders/Shading/Scene/ParkourDistort.hlsl"

CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
half4 _BaseColor;
half4 _EmissionColor;
half _Emission;
half _Cutoff;
half _RoughnessMin;
half _RoughnessMax;
half _Metallic;
half _MetallicMin;
half _MetallicMax;
half _BumpScale;
half _Occlusion;
half _OcclusionScale;
half _DitherTransparency;
half _NeedImpostor;;
CBUFFER_END

half4 _SceneColorAdjustmentParams;
#define _SceneExposure _SceneColorAdjustmentParams.x

TEXTURE2D(_BaseMap);            SAMPLER(sampler_BaseMap);
TEXTURE2D(_BumpMap);            SAMPLER(sampler_BumpMap);
//
//struct SurfaceData
//{
//    half3 albedo;
//    half3 specular;
//    half  metallic;
//    half  smoothness;
//    half3 normalTS;
//    half3 emission;
//    half  occlusion;
//    half  alpha;
//};
#include "../Include/OPPInput.hlsl"
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

    float4 positionCS               : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
};


inline void InitializeStandardLitSurfaceData(float2 uv, out SurfaceData outSurfaceData)
{
    half4 albedo = Sample2D(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    half4 bump = Sample2D(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap));
    if(_NeedImpostor==1)
    {
        albedo.a=1;
        bump.a=1;
    }
    
    outSurfaceData.alpha = GetAlpha(albedo, _Cutoff);
    
    outSurfaceData.albedo = albedo.rgb * _BaseColor.rgb;

    outSurfaceData.metallic = GetMetallic(bump.w, _MetallicMin, _MetallicMax);
    outSurfaceData.specular = 0;
    outSurfaceData.emission = GetEmission(albedo, _EmissionColor);
    outSurfaceData.smoothness = GetSmoothness(bump.z, _RoughnessMin, _RoughnessMax);
    outSurfaceData.normalTS = GetNormal(bump.xy, _BumpScale);
    outSurfaceData.occlusion = GetOcclusion(bump.w, _OcclusionScale);
    outSurfaceData.clearCoatMask = 0.0h;
    outSurfaceData.clearCoatSmoothness = 0.0h;
    outSurfaceData.darkValueInRain = 1;
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
    inputData.normalWS = TransformTangentToWorld(normalTS, half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz));
#else
    inputData.normalWS = input.normalWS;
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
    //inputData.bakedGI = GetLightmap(input.lightmapUV, inputData.normalWS);
    inputData.bakedGI = 0.1f;
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);

#if defined(_SMARTSOFTSHADOW_ON)
    inputData.shadowMask = GetSmartShadow(_MainLightPosition.xyz, inputData.normalWS, float4(inputData.positionWS, 1), _SmartShadowIntensity);
#else
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.lightmapUV);
#endif
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

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

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


    //vertexInput.positionCS;
    UNITY_BRANCH
    if(_IsParkour == 1)
    {
		vertexInput.positionWS = ParkourDistortVertex(vertexInput.positionWS);
    }

	output.positionCS = TransformWorldToHClip(vertexInput.positionWS);
	
    return output;
}

// Used in Standard (Physically Based) shader
half4 LitPassFragment(Varyings input) : SV_Target
{
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
    half4 color = UniversalFragmentPBR(inputData, surfaceData);

    //color.a = OutputAlpha(color.a, _Surface);

    APPLY_FOG(color.rgb, inputData.positionWS);

    SphereDitherTransparent(input.positionCS, _DitherTransparency);

    color.rgb *= _SceneExposure;

    #if defined(_SHADER_DEBUG) && defined(_URP_DEBUG)
    half4 debugColor = GetDebugColor(surfaceData, inputData);
    return debugColor;
    #endif

    return color;
}

#endif
