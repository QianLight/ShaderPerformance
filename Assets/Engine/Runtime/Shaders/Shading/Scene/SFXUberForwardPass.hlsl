#ifndef SFXUBER_FORWARD_PASS_INCLUDED
#define SFXUBER_FORWARD_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
//#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
#include "../NBR/Include/Fog.hlsl"
#include "../Include/OPPInput.hlsl"
#include "SFXUberInput.hlsl"

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 texcoord     : TEXCOORD0;
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
    outSurfaceData.darkValueInRain = 1;

    #ifdef _EMISSION_ON
        half4 emission = SAMPLE_TEXTURE2D_LOD(_BaseMap, sampler_BaseMap, uv, 0);
        outSurfaceData.emission = GetEmission(emission, _EmissionColor);
    #else
        outSurfaceData.emission = GetEmission(albedo, _EmissionColor);
    #endif
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
    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
}

Varyings SFXLitPassVertex(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs vertexInput = (VertexPositionInputs)0;//GetVertexPositionInputs(input.positionOS.xyz);
	vertexInput.positionWS = TransformObjectToWorld(input.positionOS.xyz);
	vertexInput.positionCS = TransformWorldToHClip(vertexInput.positionWS);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    half3 viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
    half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
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

    OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

    output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

    #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
        output.positionWS = vertexInput.positionWS;
    #endif

    #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
        output.shadowCoord = GetShadowCoord(vertexInput);
    #endif

    output.positionCS = vertexInput.positionCS;
    return output;
}

struct LitFragmentOutput
{
    half4 color0: SV_Target0;
};

LitFragmentOutput SFXLitPassFragment(Varyings input)
{
    LitFragmentOutput output = (LitFragmentOutput)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    SurfaceData surfaceData;
    InitializeStandardLitSurfaceData(input.uv, surfaceData);
    InputData inputData;
    InitializeInputData(input, surfaceData.normalTS, inputData);
    #if defined(_SHADER_LEVEL_LOW) || defined(_SHADER_LEVEL_VERY_LOW)
        half4 color = LowLevelLambert(inputData, surfaceData.albedo, surfaceData.emission, surfaceData.alpha, surfaceData.metallic);
    #else
        half4 color = UniversalFragmentPBR(inputData, surfaceData);//float4( surfaceData.albedo, 1);//
    #endif
	if(_IgnoreAlpha)
		color.a = 1;	
    APPLY_FOG(color.rgb, input.positionWS.xyz);
    SphereDitherTransparent(input.positionCS, _DitherTransparency);
    color.rgb *= _SceneExposure;   
    if(_NeedImpostor==1)
    {
        color.a = 1;
    }
    color.a *= _DistinguishAlpha;   
    output.color0 =  half4(color.rgb, color.a);
    return output;
}
#endif
