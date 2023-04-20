#ifndef SCENE_DEBUG_INPUT_INCLUDE
#define SCENE_DEBUG_INPUT_INCLUDE

#ifdef _SHADER_DEBUG

half _ShaderDebugModel;

#define SCENE_DEBUG_ALBEDO 1
#define SCENE_DEBUG_METALLIC 2
#define SCENE_DEBUG_ROUGHNESS 3
#define SCENE_DEBUG_SPECULAR 4
#define SCENE_DEBUG_NORMALWS 5
#define SCENE_DEBUG_NORMAL_GRAYSCALE 6
#define SCENE_DEBUG_IBL 7
#define SCENE_DEBUG_LIGHTMAP 8
#define SCENE_DEBUG_SHADOW_MASK 9

#ifdef _URP_DEBUG
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

half3 GetEnvironmentReflection(SurfaceData surface_data, InputData input_data)
{
    BRDFData brdfData;
    InitializeBRDFData(surface_data.albedo, surface_data.metallic, surface_data.specular, surface_data.smoothness, surface_data.alpha, brdfData);
    
    half3 reflectVector = reflect(-input_data.viewDirectionWS, input_data.normalWS);
    half3 indirectSpecular = GlossyEnvironmentReflection(reflectVector, brdfData.perceptualRoughness, surface_data.occlusion);
    #if defined(_IBL_SCALE_ENABLE)
    indirectSpecular *= _IBLScale;
    #endif
    #ifdef _DARK_METALLIC_IBL_SCALE_ENABLE
    indirectSpecular += (_DarkMetallicIBLOffset * brdfData.darkMetallicMask);
    #endif

    half NoV = saturate(dot(input_data.normalWS, input_data.viewDirectionWS));
    half fresnelTerm = Pow4(1.0 - NoV);
    half3 envBrdf = indirectSpecular * EnvironmentBRDFSpecular(brdfData, fresnelTerm);
    indirectSpecular *= envBrdf;
    return indirectSpecular;
}

half4 GetDebugColor(SurfaceData surface_data, InputData input_data)
{
    half4 debugolor = half4(0, 0, 0, 1);

    half debugMask = 1 - saturate(abs(_ShaderDebugModel - SCENE_DEBUG_ALBEDO));
    debugolor.rgb += surface_data.albedo * debugMask;

    debugMask = 1 - saturate(abs(_ShaderDebugModel - SCENE_DEBUG_METALLIC));
    debugolor.rgb += surface_data.metallic * debugMask;

    debugMask = 1 - saturate(abs(_ShaderDebugModel - SCENE_DEBUG_ROUGHNESS));
    debugolor.rgb += (1 - surface_data.smoothness) * debugMask;

    debugMask = 1 - saturate(abs(_ShaderDebugModel - SCENE_DEBUG_SPECULAR));
    debugolor.rgb += surface_data.specular * debugMask;

    debugMask = 1 - saturate(abs(_ShaderDebugModel - SCENE_DEBUG_NORMALWS));
    debugolor.rgb += input_data.normalWS * debugMask;

    debugMask = 1 - saturate(abs(_ShaderDebugModel - SCENE_DEBUG_NORMAL_GRAYSCALE));
    half NdotL = dot(_MainLightPosition.xyz, input_data.normalWS.xyz) * 0.5 + 0.5;
    debugolor.rgb += NdotL.xxx * debugMask;

    debugMask = 1 - saturate(abs(_ShaderDebugModel - SCENE_DEBUG_IBL));
    half3 envReflection = GetEnvironmentReflection(surface_data, input_data);
    debugolor.rgb += envReflection * debugMask;

    debugMask = 1 - saturate(abs(_ShaderDebugModel - SCENE_DEBUG_LIGHTMAP));
    debugolor.rgb += input_data.bakedGI * debugMask;

    debugMask = 1 - saturate(abs(_ShaderDebugModel - SCENE_DEBUG_SHADOW_MASK));
    debugolor += input_data.shadowMask * debugMask;

    return debugolor;
}

#endif


#ifdef _SRP_DEBUG
#include "../../StdLib.hlsl" 
#include "../NBR/Include/Head.hlsl"

half4 GetDebugColor(FFragData f_frag_data, FMaterialData f_material_data)
{
    half4 debugolor = half4(0,0,0,1);
    
    half debugMask = 1 - saturate(abs( _ShaderDebugModel - SCENE_DEBUG_ALBEDO));
    debugolor.rgb += f_material_data.BaseColor * debugMask;

    debugMask = 1 - saturate(abs( _ShaderDebugModel - SCENE_DEBUG_METALLIC));
    debugolor.rgb += f_material_data.Metallic * debugMask;

    debugMask = 1 - saturate(abs( _ShaderDebugModel - SCENE_DEBUG_ROUGHNESS));
    debugolor.rgb += f_material_data.Roughness * debugMask;

    debugMask = 1 - saturate(abs( _ShaderDebugModel - SCENE_DEBUG_SPECULAR));
    debugolor.rgb += f_material_data.SpecularColor * debugMask;

    debugMask = 1 - saturate(abs( _ShaderDebugModel - SCENE_DEBUG_NORMALWS));
    debugolor.rgb += f_material_data.WorldNormal * debugMask;

    debugMask = 1 - saturate(abs( _ShaderDebugModel - SCENE_DEBUG_NORMAL_GRAYSCALE));
    half NdotL = dot(_MainLightPosition, f_material_data.WorldNormal) * 0.5 + 0.5;
    debugolor.rgb += NdotL * debugMask;

    debugMask = 1 - saturate(abs( _ShaderDebugModel - SCENE_DEBUG_IBL));
    debugolor.rgb += f_frag_data.Ambient * debugMask;

    debugMask = 1 - saturate(abs( _ShaderDebugModel - SCENE_DEBUG_LIGHTMAP));
    debugolor.rgb += f_frag_data.Ambient * debugMask;

    debugMask = 1 - saturate(abs( _ShaderDebugModel - SCENE_DEBUG_SHADOW_MASK));
    debugolor.rgb += f_frag_data.ShadowCoord2 * debugMask;

    return debugolor;
}
#endif

#endif

#endif
